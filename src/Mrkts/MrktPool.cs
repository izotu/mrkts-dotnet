namespace Mrkts;

public sealed class MrktPool : IAsyncDisposable
{
    private readonly IReadOnlyList<IAccessTokenProvider> _accounts;
    private readonly string? _proxy;
    private readonly bool _unlimited;
    private readonly int _concurrency;
    private readonly SemaphoreSlim _mapLock = new(1, 1);
    private readonly List<MrktClient> _workers = [];
    private IReadOnlyList<string>? _tokens;

    public MrktPool(IReadOnlyList<string> tokens, string? proxy = null, bool unlimited = false, int concurrency = 5)
        : this(ToProviders(tokens), proxy, unlimited, concurrency)
    {
    }

    public MrktPool(IReadOnlyList<IAccessTokenProvider> accounts, string? proxy = null, bool unlimited = false, int concurrency = 5)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        if (accounts.Count == 0)
            throw new ArgumentException("Client pool must contain at least one account", nameof(accounts));
        if (concurrency < 1)
            throw new ArgumentOutOfRangeException(nameof(concurrency), concurrency, "concurrency must be greater than zero");
        _accounts = accounts;
        _proxy = proxy;
        _unlimited = unlimited;
        _concurrency = concurrency;
    }

    public async Task<MrktPool> StartAsync(CancellationToken cancellationToken = default)
    {
        _tokens ??= await GetTokensAsync(cancellationToken).ConfigureAwait(false);
        return this;
    }

    public async Task<IReadOnlyList<PoolResult<T>>> MapAsync<T>(Func<MrktClient, Task<T>> callback, int? concurrency = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        int limit = concurrency ?? _concurrency;
        if (limit < 1)
            throw new ArgumentOutOfRangeException(nameof(concurrency), limit, "concurrency must be greater than zero");
        await _mapLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await StartAsync(cancellationToken).ConfigureAwait(false);
            var tokens = _tokens!;
            int workerCount = _unlimited ? tokens.Count : Math.Min(limit, tokens.Count);
            while (_workers.Count < workerCount)
                _workers.Add(new MrktClient(proxy: _proxy));
            var results = new PoolResult<T>[tokens.Count];
            int next = -1;
            async Task RunAsync(MrktClient worker)
            {
                int index;
                while ((index = Interlocked.Increment(ref next)) < tokens.Count)
                {
                    try
                    {
                        worker.SetToken(tokens[index]);
                        results[index] = new(await callback(worker).ConfigureAwait(false));
                    }
                    catch (Exception error) when (error is not OperationCanceledException)
                    {
                        results[index] = new(default, error);
                    }
                }
            }
            await Task.WhenAll(_workers.Take(workerCount).Select(RunAsync)).ConfigureAwait(false);
            return results;
        }
        finally
        {
            _mapLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var worker in _workers)
            await worker.DisposeAsync().ConfigureAwait(false);
        _workers.Clear();
    }

    private async Task<IReadOnlyList<string>> GetTokensAsync(CancellationToken cancellationToken)
    {
        var tokens = new string?[_accounts.Count];
        var errors = new List<(int Index, Exception Error)>();
        int next = -1;
        async Task RunAsync()
        {
            int index;
            while ((index = Interlocked.Increment(ref next)) < _accounts.Count)
            {
                try
                {
                    tokens[index] = await _accounts[index].GetTokenAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception error) when (error is not OperationCanceledException)
                {
                    lock (errors)
                        errors.Add((index, error));
                }
            }
        }
        await Task.WhenAll(Enumerable.Range(0, Math.Min(_concurrency, _accounts.Count)).Select(_ => RunAsync())).ConfigureAwait(false);
        if (errors.Count > 0)
        {
            var (index, error) = errors.MinBy(entry => entry.Index);
            throw new InvalidOperationException($"Failed to get token for account at index {index}", error);
        }
        return tokens.Select(token => token!).ToArray();
    }

    private static IReadOnlyList<IAccessTokenProvider> ToProviders(IReadOnlyList<string> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        if (tokens.Count == 0)
            throw new ArgumentException("Client pool must contain at least one account", nameof(tokens));
        return tokens.Select(token => (IAccessTokenProvider)new FixedTokenProvider(token)).ToList();
    }

    private sealed class FixedTokenProvider(string token) : IAccessTokenProvider
    {
        public Task<string> GetTokenAsync(CancellationToken cancellationToken = default) => Task.FromResult(token);
    }
}

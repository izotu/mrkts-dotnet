namespace Mrkts;

public interface IAccessTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
}

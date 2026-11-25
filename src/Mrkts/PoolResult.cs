using System.Diagnostics.CodeAnalysis;

namespace Mrkts;

public readonly record struct PoolResult<T>
{
    public PoolResult([AllowNull] T value, Exception? error = null)
    {
        Value = value!;
        Error = error;
    }

    public T Value { get; }
    public Exception? Error { get; }
    public bool IsSuccess => Error is null;
}

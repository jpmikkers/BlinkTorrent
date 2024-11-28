using System.Diagnostics.Contracts;

namespace QueueTorrent;

public static class IEnumerableExtensions
{
    [Pure]
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : class
        => enumerable.Where(e => e is not null).Select(e => e!);

    [Pure]
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : struct
        => enumerable.Where(e => e.HasValue).Select(e => e!.Value);
}

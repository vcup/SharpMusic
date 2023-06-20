namespace SharpMusic.DllHellPTests.Utils;

public static class EnumerableShareStateExtensions
{
    private static readonly Dictionary<int, int> Index = new();

    public static IEnumerator<T> GetShareEnumerator<T>(this T[] array)
    {
        var id = array.GetHashCode();
        if (!Index.TryGetValue(id, out var i)) i = 0;

        while (i < array.Length)
        {
            yield return array[Index[id] = i++];
        }
    }

    public static IEnumerable<T> GetShareEnumerable<T>(this T[] array)
    {
        var id = array.GetHashCode();
        if (!Index.TryGetValue(id, out var i)) i = 0;

        while (i < array.Length)
        {
            yield return array[Index[id] = i++];
        }
    }
}
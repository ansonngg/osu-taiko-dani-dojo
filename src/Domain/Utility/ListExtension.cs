namespace OsuTaikoDaniDojo.Domain.Utility;

public static class ListExtension
{
    public static void Extend<T>(this List<T> list, int size) where T : new()
    {
        var count = list.Count;

        if (count >= size)
        {
            return;
        }

        if (list.Capacity < size)
        {
            list.Capacity = size;
        }

        list.AddRange(_Create<T>(size - count));
    }

    private static IEnumerable<T> _Create<T>(int count) where T : new()
    {
        for (var i = 0; i < count; i++)
        {
            yield return new T();
        }
    }
}

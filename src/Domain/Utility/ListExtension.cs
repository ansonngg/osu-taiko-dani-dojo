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

        list.AddRange(Enumerable.Repeat(new T(), size - count));
    }
}

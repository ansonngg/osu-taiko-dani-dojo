namespace OsuTaikoDaniDojo.Domain.Utility;

public static class ArrayExtension
{
    public static T[] Split<T>(this T[] array, int sectionCount, int section)
    {
        if (array.Length % sectionCount != 0)
        {
            throw new ArgumentException($"The length of the array is not a multiple of {nameof(sectionCount)}.");
        }

        var sectionSize = array.Length / sectionCount;
        return array[(section * sectionSize)..((section + 1) * sectionSize)];
    }
}

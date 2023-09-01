namespace Core.Helpers;

public static class ListExtensions
{
    public static void Resize<T>(this List<T> list, int length)
    {
        T[] array = list.ToArray();
        Array.Resize(ref array, length);

        list.Clear();
        list.AddRange(array);
    }
}

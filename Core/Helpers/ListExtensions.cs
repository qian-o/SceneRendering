namespace Core.Helpers;
public static class ListExtensions
{
    public static void Resize<T>(this List<T> list, int length)
    {
        list.Clear();
        list.AddRange(new T[length]);
    }
}

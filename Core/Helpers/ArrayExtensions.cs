namespace Core.Helpers;

public unsafe static class ArrayExtensions
{
    public static T* Data<T>(this T[] array) where T : unmanaged
    {
        fixed (T* ptr = array)
        {
            return ptr;
        }
    }
}
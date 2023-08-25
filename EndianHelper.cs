
namespace NutFileLibrary
{
    public class EndianHelper
    {
        public static byte[] ConvertFromLittleToBig(byte[] array)
        {
            byte[] converted = new byte[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                converted[i] = array[array.Length - 1 - i];
            }
            return converted;
        }
    }
}

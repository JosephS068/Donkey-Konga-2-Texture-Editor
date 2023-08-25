namespace NutFileLibrary
{
    internal class FileTraversal
    {
        public static byte GetByte(byte[] nutBytes, ref int pos)
        {
            pos += 1;
            return nutBytes[pos - 1];
        }

        public static ushort GetUshort(byte[] nutBytes, ref int pos)
        {
            byte[] copyByte = new byte[2];
            Array.Copy(nutBytes, pos, copyByte, 0, 2);
            pos += 2;
            return BitConverter.ToUInt16(ConvertFromLittleToBig(copyByte));
        }

        public static uint GetUint(byte[] nutBytes, ref int pos)
        {
            byte[] copyByte = new byte[4];
            Array.Copy(nutBytes, pos, copyByte, 0, 4);
            pos += 4;
            return BitConverter.ToUInt32(ConvertFromLittleToBig(copyByte));
        }

        public static byte[] ConvertFromLittleToBig(byte[] array)
        {
            byte[] converted = new byte[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                converted[i] = array[array.Length - 1 - i];
            }
            return converted;
        }

        public static byte[] GetHardwareData(byte[] nutBytes, ref int pos)
        {
            byte[] copyByte = new byte[24];
            Array.Copy(nutBytes, pos, copyByte, 0, 24);
            pos += 24;
            return copyByte;
        }

        public static byte[] GetOptionData(byte[] nutBytes, ref int pos, int dataAmount)
        {
            byte[] copyByte = new byte[dataAmount];
            Array.Copy(nutBytes, pos, copyByte, 0, dataAmount);
            pos += dataAmount;
            return copyByte;
        }

        public static byte[] GetBytesData(byte[] nutBytes, int imageLength, ref int pos)
        {
            byte[] copyByte = new byte[imageLength];
            Array.Copy(nutBytes, pos, copyByte, 0, imageLength);
            pos += imageLength;
            return copyByte;
        }
    }
}

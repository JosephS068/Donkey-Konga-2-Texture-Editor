namespace NutFileLibrary
{
    public class NutHeader
    {
        public string MagicIdentifier = "NUTC";
        public ushort FormatVersion = 0x8002;
        public ushort NumberOfTextures;
        public byte[] Padding = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public byte[] HeaderBytes;

        public void ReadFileBytes(byte[] headerBytes)
        {
            // We only read the bytes of the number of textures.
            NumberOfTextures = headerBytes[6];
            NumberOfTextures = (ushort)(NumberOfTextures << 8);
            NumberOfTextures += headerBytes[7];

            byte[] bytes = new byte[ByteLength()];
            Array.Copy(headerBytes, bytes, ByteLength());
            HeaderBytes = bytes;
        }

        public static int ByteLength()
        {
            return 4 + 2 + 2 + 24;
        }
    }
}

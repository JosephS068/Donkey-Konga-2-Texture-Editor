namespace NutFileLibrary
{
    public class NutFile
    {
        public NutHeader Header = new NutHeader();
        public List<NutImage> Images;

        public NutFile() { }

        public void ReadNutFileBytes(byte[] nutBytes)
        {
            Header.ReadFileBytes(nutBytes);
            Images = new List<NutImage>();

            // byte position
            int position = NutHeader.ByteLength();

            for(int i = 0; i < Header.NumberOfTextures; i++)
            {
                NutImage image = new NutImage();
                image.ReadFileBytes(nutBytes, ref position);
                Images.Add(image);
            }
        }

        public byte[] ConvertToBytes()
        {
            List<byte> byteList = new List<byte>();
            byteList.AddRange(Header.HeaderBytes);

            foreach(NutImage image in Images)
            {
                byteList.AddRange(image.GetBytes());
            }

            return byteList.ToArray();
        }
    }
}
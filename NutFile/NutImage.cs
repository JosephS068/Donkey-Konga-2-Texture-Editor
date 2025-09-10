using System.Drawing;

namespace NutFileLibrary
{
    public class NutImage
    {
        // Bit masks to use when getting color information
        const int BitMask_3Bits = 0x7;
        const int BitMask_4Bits = 0xF;
        const int BitMask_5Bits = 0x1F;
        const int BitMask_6Bits = 0x3F;
        const int BitMask_8Bits = 0xFF;

        public uint TextureDataLength;
        public uint PaletteLength;
        public uint ImageLength;
        public ushort HeaderLength;
        public ushort NumberOfColors;
        public byte TextureFormat;
        public byte NumberOfNmipmap;
        public PaletteFormat PaletteFormat;
        public ImageFormat ImageFormat;
        public ushort Width;
        public ushort Height;
        public byte[] HardwareData;
        public byte[] OptionalData;
        public byte[] ImageData;
        public byte[] PaletteData;
        public Color[] ColorPaletteData;
        public ImageTile[] ImageTiles;
        public Bitmap ImageBitMap;

        public void ReadFileBytes(byte[] imageBytes, ref int pos)
        {
            int startingPosition = pos;
            TextureDataLength = FileTraversal.GetUint(imageBytes, ref pos);
            PaletteLength = FileTraversal.GetUint(imageBytes, ref pos);
            ImageLength = FileTraversal.GetUint(imageBytes, ref pos);
            HeaderLength = FileTraversal.GetUshort(imageBytes, ref pos);
            NumberOfColors = FileTraversal.GetUshort(imageBytes, ref pos);
            TextureFormat = FileTraversal.GetByte(imageBytes, ref pos);
            NumberOfNmipmap = FileTraversal.GetByte(imageBytes, ref pos);
            PaletteFormat = (PaletteFormat)FileTraversal.GetByte(imageBytes, ref pos);
            ImageFormat = (ImageFormat)FileTraversal.GetByte(imageBytes, ref pos);
            Width = FileTraversal.GetUshort(imageBytes, ref pos);
            Height = FileTraversal.GetUshort(imageBytes, ref pos);
            HardwareData = FileTraversal.GetHardwareData(imageBytes, ref pos);
            OptionalData = FileTraversal.GetOptionData(imageBytes, ref pos, HeaderLength - 0x30);
            ImageData = FileTraversal.GetBytesData(imageBytes, (int)ImageLength, ref pos);
            ImageTiles = CreateImageTiles(ImageData, ImageFormat);

            PaletteData = FileTraversal.GetBytesData(imageBytes, (int)PaletteLength, ref pos);


            switch (PaletteFormat)
            {
                case PaletteFormat.No_Palette:
                    ColorPaletteData = null;
                    break;
                case PaletteFormat.RGB_565:
                    ColorPaletteData = Create_RGB_565_ColorPaletteData();
                    break;
                case PaletteFormat.RGB5_A3:
                    ColorPaletteData = Create_RGB5_A3_ColorPaletteData();
                    break;
                case PaletteFormat.IA8:
                case PaletteFormat.IA8_0xB:
                    ColorPaletteData = Create_IA_8_ColorPaletteData();
                    break;
                default:
                    throw new Exception("Unexpected Palette Format Found");
            }

            ImageBitMap = CreateImageBitMap();
        }

        ImageTile[] CreateImageTiles(byte[] imageBytes, ImageFormat imageFormat)
        {
            int pos = 0;

            int tileHeight = 0;
            int tileWidth = 0;
            if (imageFormat == ImageFormat.Eight_Bits_Per_Pixel
                || imageFormat == ImageFormat.I8)
            {
                tileHeight = 4;
                tileWidth = 8;
            }
            else if (imageFormat == ImageFormat.IA8)
            {
                tileHeight = 4;
                tileWidth = 4;
            }
            else if (imageFormat == ImageFormat.Four_Bits_Per_Pixel)
            {
                tileHeight = 8;
                tileWidth = 8;
            }
            else if (imageFormat == ImageFormat.ARGB8)
            {
                tileHeight = 4;
                tileWidth = 4;

                int planeSize = 32;
                int totalPlanes = 2;
                int tileSize = planeSize * totalPlanes;

                byte[] ar = new byte[planeSize];
                byte[] gb = new byte[planeSize];

                int originalPos = pos;
                int remainingData = imageBytes.Length - pos;
                byte[] decoded = new byte[remainingData];

                for (int filterPos = 0; filterPos < remainingData; filterPos += tileSize)
                {
                    ar = FileTraversal.GetBytesData(imageBytes, ar.Length, ref pos);
                    gb = FileTraversal.GetBytesData(imageBytes, gb.Length, ref pos);

                    for (int i = 0; i < planeSize; i += 2)
                    {
                        byte a = ar[i];
                        byte r = ar[i + 1];

                        byte g = gb[i];
                        byte b = gb[i + 1];

                        decoded[filterPos + i * 2] = a;
                        decoded[filterPos + i * 2 + 1] = r;
                        decoded[filterPos + i * 2 + 2] = g;
                        decoded[filterPos + i * 2 + 3] = b;
                    }
                }

                pos = originalPos;

                Array.Copy(decoded, 0, imageBytes, originalPos, remainingData);

            }
            else if (imageFormat == ImageFormat.DXT1)
            {
                tileHeight = 2;
                tileWidth = 4;

                // alright so this format is like tiled twice
                // untile this
                byte[] Buf = new byte[ImageData.Length];
                int w = Width * 2; // just go with it.
                int height = Height / 4; // again just go with it

                int lineSize = 16; // doesn't seem to change for DK

                int i = 0;

                for (int y = 0; y < height; y += tileHeight)
                {
                    for (int x = 0; x < w; x += lineSize)
                    {
                        // so this 2 would be tile height, but something something idk.
                        for (int tileY = y; tileY < y + tileHeight; tileY++)
                        {
                            for (int tileX = x; tileX < x + lineSize; tileX++)
                            {
                                byte data = imageBytes[i++];

                                if (tileX >= w || tileY >= height)
                                {
                                    continue;
                                }

                                Buf[tileY * w + tileX] = data;
                            }
                        }
                    }
                }
                tileHeight = 4;
                // Should we be rewriting this?
                // oh its for the weird format, we can let it slide lol.
                imageBytes = Buf;
            }
            else {
                throw new Exception("unexpected image format");
            }

            // this is probably  wrong, but likely wrong in a way that doesn't cause a crash hopefully
            int totalNumberOfTiles = (Width / tileWidth) * (Height / tileHeight);
            if (Width % tileWidth != 0)
            {
                totalNumberOfTiles += (Height / tileHeight);
            }

            if (Height % tileHeight != 0)
            {
                totalNumberOfTiles += (Width / tileWidth);
            }

            if (Width % tileWidth != 0 && Height % tileHeight != 0)
            {
                totalNumberOfTiles += 1;
            }

            List<ImageTile> tiles = new List<ImageTile>();
            for (int i = 0; i < totalNumberOfTiles; i++)
            {
                tiles.Add(new ImageTile(imageBytes, ref pos, imageFormat));
            }
            return tiles.ToArray();
        }

        Color[] Create_RGB5_A3_ColorPaletteData()
        {
            int bytePos = 0;
            List<Color> colorList = new List<Color>();
            for (int i = 0; i < PaletteData.Length / 2; i++)
            {
                ushort color = FileTraversal.GetUshort(PaletteData, ref bytePos);
                if ((color & 0x8000) != 0)
                {
                    int red = ImageUtils.Conv5To8((color >> 10) & BitMask_5Bits);
                    int green = ImageUtils.Conv5To8((color >> 5) & BitMask_5Bits);
                    int blue = ImageUtils.Conv5To8((color) & BitMask_5Bits);
                    int alpha = 255;
                    colorList.Add(Color.FromArgb(alpha, red, green, blue));
                }
                else // with alpha
                {
                    int alpha = ImageUtils.Conv3To8((color >> 12) & BitMask_3Bits);
                    int red = ImageUtils.Conv4To8((color >> 8) & BitMask_4Bits);
                    int green = ImageUtils.Conv4To8((color >> 4) & BitMask_4Bits);
                    int blue = ImageUtils.Conv4To8((color) & BitMask_4Bits);
                    colorList.Add(Color.FromArgb(alpha, red, green, blue));
                }
            }

            return colorList.ToArray();
        }

        Color[] Create_RGB_565_ColorPaletteData()
        {
            int bytePos = 0;
            List<Color> colorList = new List<Color>();
            for (int i = 0; i < PaletteData.Length / 2; i++)
            {
                ushort color = FileTraversal.GetUshort(PaletteData, ref bytePos);
                int red = ImageUtils.Conv5To8((color >> 11) & BitMask_5Bits);
                int green = ImageUtils.Conv6To8((color >> 5) & BitMask_6Bits);
                int blue = ImageUtils.Conv5To8((color) & BitMask_5Bits);
                int alpha = 255;
                colorList.Add(Color.FromArgb(alpha, red, green, blue));
            }

            return colorList.ToArray();
        }

        Color[] Create_IA_8_ColorPaletteData()
        {
            int bytePos = 0;
            List<Color> colorList = new List<Color>();
            for (int i = 0; i < PaletteData.Length / 2; i++)
            {
                ushort color = FileTraversal.GetUshort(PaletteData, ref bytePos);
                int red = color & BitMask_8Bits;
                int green = color & BitMask_8Bits;
                int blue = color & BitMask_8Bits;
                int alpha = (color >> 8) & BitMask_8Bits;
                colorList.Add(Color.FromArgb(alpha, red, green, blue));
            }

            return colorList.ToArray();
        }

        Bitmap CreateImageBitMap()
        {
            Bitmap bitMap = new Bitmap(Width, Height);
            bitMap.MakeTransparent();
            // Give default background to bitmap.
            //for(int xback=0; xback < Width; xback++)
            //{
            //    for (int yback = 0; yback < Height; yback++)
            //    {
            //        if(yback % 2 == 0 || yback % 3 == 0)
            //        {
            //            bitMap.SetPixel(xback, yback, Color.White);
            //        }
            //        else
            //        {
            //            bitMap.SetPixel(xback, yback, Color.LightGray);
            //        }
            //    }
            //}

            int x = 0;
            int y = 0;
            foreach (ImageTile imageTile in ImageTiles)
            {
                for (int tileY = 0; tileY < imageTile.ColorValuePosition.GetLength(1); tileY++)
                {
                    for (int tileX = 0; tileX < imageTile.ColorValuePosition.GetLength(0); tileX++)
                    {
                        if(imageTile.ImageFormat == ImageFormat.DXT1
                            || imageTile.ImageFormat == ImageFormat.ARGB8
                            || imageTile.ImageFormat == ImageFormat.IA8)
                        {
                            Color pixelColor = imageTile.Tile[tileX, tileY];
                            if (pixelColor.A != 0)
                            {
                                bitMap.SetPixel(x + tileX, y + tileY, pixelColor);
                            }
                        }
                        else if (imageTile.ImageFormat == ImageFormat.I8)
                        {
                            Color pixelColor = imageTile.Tile[tileX, tileY];

                            if (pixelColor.A != 0)
                            {
                                bitMap.SetPixel(x + tileX, y + tileY, pixelColor);
                            }
                        }
                        else
                        {
                            // Palette boys
                            int colorIndex = imageTile.ColorValuePosition[tileX, tileY];
                            Color pixelColor = ColorPaletteData[colorIndex];
                            if (pixelColor.A != 0)
                            {
                                bitMap.SetPixel(x + tileX, y + tileY, pixelColor);
                            }
                        }
                    }
                }
                x += imageTile.TileWidth;
                if(x == Width)
                {
                    x = 0;
                    y += imageTile.TileHeight;
                }
            }
            return bitMap;
        }

        public void UpdateImage(Bitmap newImage)
        {
            if(newImage.Width != Width
                || newImage.Height != Height)
            {
                throw new Exception("Image needs to be the same size as existing one");
            }

            ImageFormat originalFormatOfIamge = ImageFormat;
            // 4 bits per pixel have different file sizes, need to fix that since it will be eight bits per pixel always
            if (ImageFormat == ImageFormat.Four_Bits_Per_Pixel
                || ImageFormat == ImageFormat.DXT1)
            {
                // Adding image length again because it is about to double in size
                TextureDataLength += ImageLength;
                ImageLength *= 2;
            }

            if (ImageFormat == ImageFormat.Four_Bits_Per_Pixel
                || ImageFormat == ImageFormat.DXT1)
            {
                ImageLength /= 2;
                TextureDataLength -= ImageLength;
            }

            if (ImageFormat == ImageFormat.ARGB8)
            {
                ImageLength /= 4;
                TextureDataLength -= 3 * ImageLength;
            }

            // Currently this is the only image format that can be used to replace values
            ImageFormat = ImageFormat.Eight_Bits_Per_Pixel;

            // Get list of colors
            byte[,] positionList = new byte[Width, Height];
            Color[,] colorList = new Color[Width, Height];

            List<ushort> gcColors = new List<ushort>();
            List<byte> colorBytes = new List<byte>();

            bool containsAlpha = false;

            for (int y = 0; y < newImage.Height; y++)
            {
                for (int x = 0; x < newImage.Width; x++)
                {
                    Color pixelColor = newImage.GetPixel(x, y);

                    ushort color16Bit = 0;
                    if (pixelColor.A == 255)
                    {
                        color16Bit = 1 << 15;
                        color16Bit += (ushort)(ImageUtils.Conv8To5(pixelColor.R) << 10);
                        color16Bit += (ushort)(ImageUtils.Conv8To5(pixelColor.G) << 5);
                        color16Bit += (ushort)ImageUtils.Conv8To5(pixelColor.B);
                    }
                    else
                    {
                        color16Bit = (ushort)(ImageUtils.Conv8To4(pixelColor.A) << 12);
                        color16Bit += (ushort)(ImageUtils.Conv8To4(pixelColor.R) << 8);
                        color16Bit += (ushort)(ImageUtils.Conv8To4(pixelColor.G) << 4);
                        color16Bit += (ushort)ImageUtils.Conv8To4(pixelColor.B);
                        containsAlpha = true;
                    }

                    (byte roundColor, bool otherColorUsed) = roundAlgorithm(gcColors, colorBytes, color16Bit);

                    byte colorPosition = 0;
                    if (otherColorUsed == true) {
                        colorPosition = roundColor;
                    }
                    else if(gcColors.Contains(color16Bit))
                    {
                        colorPosition = (byte)gcColors.IndexOf(color16Bit);
                    }

                    else
                    {
                        gcColors.Add(color16Bit);
                        colorPosition = (byte)(gcColors.Count - 1);
                        colorBytes.AddRange(EndianHelper.ConvertFromLittleToBig(BitConverter.GetBytes(color16Bit)));
                    }
                    
                    positionList[x, y] = colorPosition;
                    colorList[x, y] = pixelColor;
                }
            }
            // Set encoding to 8 bits as thats all we support
            NumberOfColors = 256; // Maybe set this to the actual number of colors.

            if (gcColors.Count > 256)
            {
                throw new Exception("image contains too many colors, image must have less than 256 different colors.");
            }

            TextureDataLength -= PaletteLength;
            
            // Times 2 as colors are contained in a ushort.
            PaletteLength = 256 * 2;

            TextureDataLength += PaletteLength;

            // The game expects this to be filled with 512 bytes
            List<byte> listWith512Bytes = new List<byte>();
            listWith512Bytes.AddRange(colorBytes);

            byte[] remainingBytes = new byte[512 - colorBytes.Count];
            listWith512Bytes.AddRange(remainingBytes);

            PaletteData = listWith512Bytes.ToArray();

            NumberOfColors = (ushort)gcColors.Count;

            ImageData = Filter(positionList, 0, Width, Height);
            ImageTiles = CreateImageTiles(ImageData, ImageFormat);

            if(containsAlpha == true)
            {
                PaletteFormat = PaletteFormat.RGB5_A3;
            }
            else
            {
                // at some point maybe do other palettes
                PaletteFormat = PaletteFormat.RGB5_A3;
            }

            switch (PaletteFormat)
            {
                case PaletteFormat.No_Palette:
                    ColorPaletteData = null;
                    break;
                case PaletteFormat.RGB_565:
                    ColorPaletteData = Create_RGB_565_ColorPaletteData();
                    break;
                case PaletteFormat.RGB5_A3:
                    ColorPaletteData = Create_RGB5_A3_ColorPaletteData();
                    break;
                case PaletteFormat.IA8:
                    ColorPaletteData = Create_IA_8_ColorPaletteData();
                    break;
                default:
                    throw new Exception("Unexpected Palette Foramt Found");
            }

            ImageBitMap = CreateImageBitMap();

            // change the bytes
        }

        // Extremely simple rounding algorithm
        // basically, if a color exists that's close enough to the current one that color will be used.
        // returns the index value and if a substitue color can be used.
        (byte, bool) roundAlgorithm(List<ushort> gcColors, List<byte> colorBytes, ushort color16Bit)
        {
            for (int i = 0; i < gcColors.Count; i++)
            {
                ushort gcColor = gcColors[i];
                if (color16Bit < gcColor + 35 && gcColor - 35 < color16Bit)
                {
                    return ((byte)i, true);
                }
            }

            gcColors.Add(color16Bit);
            colorBytes.AddRange(EndianHelper.ConvertFromLittleToBig(BitConverter.GetBytes(color16Bit)));
            return ((byte)(gcColors.Count - 1), false);
        }

        public byte[] Filter(byte[,] originalData, int index, int width, int height)
        {
            byte[] Buf = new byte[originalData.Length];
            int tileHeight = 4;

            int lineSize = 8;

            int i = 0;

            for (int y = 0; y < height; y += tileHeight)
            {
                for (int x = 0; x < width; x += lineSize)
                {
                    for (int tileY = y; tileY < y + tileHeight; tileY++)
                    {
                        for (int tileX = x; tileX < x + lineSize; tileX++)
                        {
                            byte data = originalData[tileX, tileY];

                            if (tileX >= width || tileY >= height)
                            {
                                continue;
                            }

                            Buf[i] = data;
                            i++;
                        }
                    }
                }
            }

            return Buf;
        }

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(ReverseOrder(BitConverter.GetBytes(TextureDataLength)));
            bytes.AddRange(ReverseOrder(BitConverter.GetBytes(PaletteLength)));
            bytes.AddRange(ReverseOrder(BitConverter.GetBytes(ImageLength)));
            bytes.AddRange(ReverseOrder(BitConverter.GetBytes(HeaderLength)));
            bytes.AddRange(ReverseOrder(BitConverter.GetBytes(NumberOfColors)));
            bytes.Add(TextureFormat);
            bytes.Add(NumberOfNmipmap);
            bytes.Add((byte)PaletteFormat);
            bytes.Add((byte)ImageFormat);
            bytes.AddRange(ReverseOrder(BitConverter.GetBytes(Width)));
            bytes.AddRange(ReverseOrder(BitConverter.GetBytes(Height)));
            bytes.AddRange(HardwareData);
            bytes.AddRange(OptionalData);
            bytes.AddRange(ImageData);
            bytes.AddRange(PaletteData);

            return bytes.ToArray();
        }

        public static byte[] ReverseOrder(byte[] array)
        {
            byte[] reverseArray = new byte[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                reverseArray[i] = array[array.Length - 1 - i];
            }
            return reverseArray;
        }
    }

    public enum  PaletteFormat : byte
    {
        No_Palette = 0,

        // RRRRRGGGGGGBBBBB
        RGB_565 = 1,

        // 1RRRRRGGGGGBBBBB
        // or
        // 0AAARRRRGGGGBBBB
        RGB5_A3 = 2,

        // AAAAAAAAIIIIIIII
        // uint8 alpha = AAAAAAAA
        // uint8 red = green = blue = IIIIIIII
        IA8 = 3, 

        // Apparently the same as IA8
        IA8_0xB = 0xB, 
        
    }

    public enum ImageFormat : byte
    {
        ARGB8 = 3
            ,
        // Used with non-palette file types.
        DXT1 = 4,

        Four_Bits_Per_Pixel = 5,

        Eight_Bits_Per_Pixel = 6,

        I8 = 0xA,

        IA8 = 0xB,
    }
}

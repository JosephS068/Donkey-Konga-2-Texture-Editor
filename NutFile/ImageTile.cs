using System.Drawing;

namespace NutFileLibrary
{
    public class ImageTile
    {
        const byte BitMask_2Bits = 0x3;
        const byte BitMask_4Bits = 0xF;
        const int BitMask_5Bits = 0x1F;
        const int BitMask_6Bits = 0x3F;

        public ImageFormat ImageFormat;

        public Color[,] Tile = new Color[4, 4];

        public int TileHeight;
        public int TileWidth;

        // Indicates what the color value should be for this pixel in the tile.
        public byte[,] ColorValuePosition;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">Position in imageData</param>
        /// <param name="imageData"></param>
        public ImageTile(byte[] imageData, ref int position, ImageFormat imageFormat) {
            ImageFormat = imageFormat;
            if (imageFormat == ImageFormat.Eight_Bits_Per_Pixel)
            {
                TileHeight = 4;
                TileWidth = 8;

                // Create color value position.
                ColorValuePosition = new byte[TileWidth, TileHeight];
                for (int y = 0; y < TileHeight; y++)
                {
                    for (int x = 0; x < TileWidth; x++)
                    {
                        ColorValuePosition[x, y] = imageData[position];
                        position++;
                    }
                }
            }
            else if (imageFormat == ImageFormat.Four_Bits_Per_Pixel)
            {
                TileHeight = 8;
                TileWidth = 8;

                bool useBackPartOfByte = false;

                // Create color value position.
                ColorValuePosition = new byte[TileWidth, TileHeight];
                for (int y = 0; y < TileHeight; y++)
                {
                    for (int x = 0; x < TileWidth; x++)
                    {
                        byte colorPosition = imageData[position];
                        if (useBackPartOfByte == false)
                        {
                            ColorValuePosition[x, y] = (byte)((colorPosition >> 4) & BitMask_4Bits);

                            useBackPartOfByte = true;
                        }
                        else
                        {
                            ColorValuePosition[x, y] = (byte)(colorPosition & BitMask_4Bits);
                            useBackPartOfByte = false;
                            position++;
                        }

                    }
                }
            }
            else if (imageFormat == ImageFormat.DXT1) {
                TileHeight = 4;
                TileWidth = 4;
                ushort color1 = FileTraversal.GetUshort(imageData, ref position);
                ushort color2 = FileTraversal.GetUshort(imageData, ref position);

                byte[] table = FileTraversal.GetBytesData(imageData, 4, ref position);

                int blue1 = ImageUtils.Conv5To8(color1 & BitMask_5Bits);
                int blue2 = ImageUtils.Conv5To8(color2 & BitMask_5Bits);
                int green1 = ImageUtils.Conv6To8((color1 >> 5) & BitMask_6Bits);
                int green2 = ImageUtils.Conv6To8((color2 >> 5) & BitMask_6Bits);
                int red1 = ImageUtils.Conv5To8((color1 >> 11) & BitMask_5Bits);
                int red2 = ImageUtils.Conv5To8((color2 >> 11) & BitMask_5Bits);

                Color colorToUse1 = Color.FromArgb(255, red1, green1, blue1);
                Color colorToUse2 = Color.FromArgb(255, red2, green2, blue2);
                Color colorToUse3;
                Color colorToUse4;

                if (color1 > color2)
                {
                    int blue3 = (2 * blue1 + blue2) / 3;
                    int green3 = (2 * green1 + green2) / 3;
                    int red3 = (2 * red1 + red2) / 3;

                    int blue4 = (2 * blue2 + blue1) / 3;
                    int green4 = (2 * green2 + green1) / 3;
                    int red4 = (2 * red2 + red1) / 3;

                    colorToUse3 = Color.FromArgb(255, red3, green3, blue3);
                    colorToUse4 = Color.FromArgb(255, red4, green4, blue4);
                }
                else
                {
                    colorToUse3 = Color.FromArgb(255, (red1 + red2) / 2, // Average
                                              (green1 + green2) / 2,
                                              (blue1 + blue2) / 2);
                    colorToUse4 = Color.FromArgb(0, 0, 0, 0);  // Color2 but transparent
                }

                Color[] colorsToUse = new Color[]{
                    colorToUse1,
                    colorToUse2,
                    colorToUse3,
                    colorToUse4,
                };

                for (int y = 0; y < 4; y++)
                {
                    int val = table[y];
                    for (int x = 0; x < 4; x++)
                    {
                        // ok so basically we loop around 4 x 4
                        // increasing our boy k as we go
                        // we then bit shit it down by 6
                        // then take the 6
                        Tile[x,y] = colorsToUse[(val >> 6) & BitMask_2Bits];
                        val <<= 2;
                    }
                }

                ColorValuePosition = new byte[TileWidth, TileHeight];
            }
            else 
            {
                throw new ArgumentException("Unexpected Image Format");
            }
        }
    }
}

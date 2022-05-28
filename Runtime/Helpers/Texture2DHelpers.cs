using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityHelpers
{
    public static class Texture2DHelpers
    {
        /// <summary>
        /// Turns a Texture2D object to a Sprite object.
        /// </summary>
        /// <param name="texture">The original image</param>
        /// <returns>The texture as a sprite</returns>
        public static Sprite ToSprite(this Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Flips the texture horizontally.
        /// </summary>
        /// <param name="colors">Original texture colors</param>
        /// <param name="width">Texture width</param>
        /// <param name="height">Texture height</param>
        /// <returns>Flipped texture color array</returns>
        public static void FlipHorizontal(Color[] colors, ushort width, ushort height)
        {
            for (uint row = 0; row < height; row++)
            {
                for (uint col = 0; col < (width / 2); col++)
                {
                    uint currentRowIndex = row * width;
                    Color tempColor = colors[currentRowIndex + col];
                    colors[currentRowIndex + col] = colors[currentRowIndex + (width - col - 1)];
                    colors[currentRowIndex + (width - col - 1)] = tempColor;
                }
            }
        }
        /// <summary>
        /// Flips the texture vertically.
        /// </summary>
        /// <param name="colors">Original texture colors</param>
        /// <param name="width">Texture width</param>
        /// <param name="height">Texture height</param>
        /// <returns>Flipped texture color array</returns>
        public static void FlipVertical(Color[] colors, ushort width, ushort height)
        {
            for (uint col = 0; col < width; col++)
            {
                for (uint row = 0; row < (height / 2); row++)
                {
                    uint currentRowIndex = row * width;
                    uint oppositeRowIndex = (height - row - 1) * width;
                    Color tempColor = colors[currentRowIndex + col];
                    colors[currentRowIndex + col] = colors[oppositeRowIndex + col];
                    colors[oppositeRowIndex + col] = tempColor;
                }
            }
        }

        /// <summary>
        /// Catch all decompress raw texture data function.
        /// </summary>
        /// <param name="data">Raw image byte data</param>
        /// <param name="width">Expected width of the image</param>
        /// <param name="height">Expected height of the image</param>
        /// <param name="textureFormat">The format of the data given</param>
        /// <returns>Pixel color data</returns>
        public static Color[] DecompressRawBytes(Stream data, ushort width, ushort height, TextureFormat textureFormat)
        {
            Color[] colors = null;
            if (textureFormat == TextureFormat.BGR888)
                colors = DecompressBGR888(data, width, height);
            else if (textureFormat == TextureFormat.BGRA8888)
                colors = DecompressBGRA8888(data, width, height);
            else if (textureFormat == TextureFormat.DXT1)
                colors = DecompressDXT1(data, width, height);
            else if (textureFormat == TextureFormat.DXT3)
                colors = DecompressDXT3(data, width, height);
            else if (textureFormat == TextureFormat.DXT5)
                colors = DecompressDXT5(data, width, height);
            else
                Debug.LogError("Texture2DHelpers: Texture format not supported " + textureFormat);

            return colors;
        }
        /// <summary>
        /// Turn raw BGR888 bytes to a Color array that can be used in a Texture2D.
        /// </summary>
        /// <param name="data">Raw image byte data</param>
        /// <param name="width">Expected width of the image</param>
        /// <param name="height">Expected height of the image</param>
        /// <returns>Pixel color data</returns>
        public static Color[] DecompressBGR888(Stream data, ushort width, ushort height)
        {
            Color[] texture2DColors = new Color[width * height];

            bool exceededArray = false;
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    byte blue = DataParser.ReadByte(data);
                    byte green = DataParser.ReadByte(data);
                    byte red = DataParser.ReadByte(data);

                    int flattenedIndex = row * width + col;
                    if (flattenedIndex < texture2DColors.Length)
                        texture2DColors[flattenedIndex] = new Color(((float)red) / byte.MaxValue, ((float)green) / byte.MaxValue, ((float)blue) / byte.MaxValue);
                    else
                    {
                        Debug.LogError("BGR888: Exceeded expected texture size");
                        exceededArray = true;
                        break;
                    }
                }

                if (exceededArray)
                    break;
            }

            return texture2DColors;
        }
        /// <summary>
        /// Turn raw BGRA8888 bytes to a Color array that can be used in a Texture2D.
        /// </summary>
        /// <param name="data">Raw image byte data</param>
        /// <param name="width">Expected width of the image</param>
        /// <param name="height">Expected height of the image</param>
        /// <returns>Pixel color data</returns>
        public static Color[] DecompressBGRA8888(Stream data, ushort width, ushort height)
        {
            Color[] texture2DColors = new Color[width * height];

            bool exceededArray = false;
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    byte blue = DataParser.ReadByte(data);
                    byte green = DataParser.ReadByte(data);
                    byte red = DataParser.ReadByte(data);
                    byte alpha = DataParser.ReadByte(data);

                    int flattenedIndex = row * width + col;
                    if (flattenedIndex < texture2DColors.Length)
                        texture2DColors[flattenedIndex] = new Color(((float)red) / byte.MaxValue, ((float)green) / byte.MaxValue, ((float)blue) / byte.MaxValue, ((float)alpha) / byte.MaxValue);
                    else
                    {
                        Debug.LogError("BGRA8888: Exceeded expected texture size");
                        exceededArray = true;
                        break;
                    }
                }

                if (exceededArray)
                    break;
            }

            return texture2DColors;
        }
        /// <summary>
        /// Turn raw DXT1 bytes to a Color array that can be used in a Texture2D.
        /// </summary>
        /// <param name="data">Raw image byte data</param>
        /// <param name="width">Expected width of the image</param>
        /// <param name="height">Expected height of the image</param>
        /// <returns>Pixel color data</returns>
        public static Color[] DecompressDXT1(Stream data, ushort width, ushort height)
        {
            Color[] texture2DColors = new Color[width * height];

            for (int row = 0; row < height; row += 4)
            {
                for (int col = 0; col < width; col += 4)
                {
                    ushort color0Data = 0;
                    ushort color1Data = 0;
                    uint bitmask = 0;

                    color0Data = DataParser.ReadUShort(data);
                    color1Data = DataParser.ReadUShort(data);
                    bitmask = DataParser.ReadUInt(data);

                    int[] colors0 = new int[] { ((color0Data >> 11) & 0x1F) << 3, ((color0Data >> 5) & 0x3F) << 2, (color0Data & 0x1F) << 3 };
                    int[] colors1 = new int[] { ((color1Data >> 11) & 0x1F) << 3, ((color1Data >> 5) & 0x3F) << 2, (color1Data & 0x1F) << 3 };

                    Color[] colorPalette = new Color[]
                    {
                        new Color(colors0[0] / 255f, colors0[1] / 255f, colors0[2] / 255f),
                        new Color(colors1[0] / 255f, colors1[1] / 255f, colors1[2] / 255f),
                        new Color(((colors0[0] * 2 + colors1[0] + 1) / 3) / 255f, ((colors0[1] * 2 + colors1[1] + 1) / 3) / 255f, ((colors0[2] * 2 + colors1[2] + 1) / 3) / 255f),
                        new Color(((colors1[0] * 2 + colors0[0] + 1) / 3) / 255f, ((colors1[1] * 2 + colors0[1] + 1) / 3) / 255f, ((colors1[2] * 2 + colors0[2] + 1) / 3) / 255f)
                    };

                    if (color0Data < color1Data)
                    {
                        colorPalette[2] = new Color(((colors0[0] + colors1[0]) / 2) / 255f, ((colors0[1] + colors1[1]) / 2) / 255f, ((colors0[2] + colors1[2]) / 2) / 255f);
                        colorPalette[3] = new Color(((colors1[0] * 2 + colors0[0] + 1) / 3) / 255f, ((colors1[1] * 2 + colors0[1] + 1) / 3) / 255f, ((colors1[2] * 2 + colors0[2] + 1) / 3) / 255f);
                    }

                    int blockIndex = 0;
                    for (int blockY = 0; blockY < 4; blockY++)
                    {
                        for (int blockX = 0; blockX < 4; blockX++)
                        {
                            Color colorInBlock = colorPalette[(bitmask & (0x03 << blockIndex * 2)) >> blockIndex * 2];
                            texture2DColors[((row * width) + col) + ((blockY * width) + blockX)] = colorInBlock;
                            blockIndex++;
                        }
                    }
                }
            }

            return texture2DColors.ToArray();
        }
        /// <summary>
        /// Turn raw DXT3 bytes to a Color array that can be used in a Texture2D.
        /// </summary>
        /// <param name="data">Raw image byte data</param>
        /// <param name="width">Expected width of the image</param>
        /// <param name="height">Expected height of the image</param>
        /// <returns>Pixel color data</returns>
        public static Color[] DecompressDXT3(Stream data, ushort width, ushort height)
        {
            Color[] texture2DColors = new Color[width * height];

            for (int row = 0; row < height; row += 4)
            {
                for (int col = 0; col < width; col += 4)
                {
                    ushort color0Data = 0;
                    ushort color1Data = 0;
                    uint bitmask = 0;

                    //data.Seek(8, SeekOrigin.Current); //not sure if this is correct or not, had it before, but I think I never got to test a DXT3 image. I commented it out because it looks wrong
                    color0Data = DataParser.ReadUShort(data);
                    color1Data = DataParser.ReadUShort(data);
                    bitmask = DataParser.ReadUInt(data);

                    int[] colors0 = new int[] { ((color0Data >> 11) & 0x1F) << 3, ((color0Data >> 5) & 0x3F) << 2, (color0Data & 0x1F) << 3 };
                    int[] colors1 = new int[] { ((color1Data >> 11) & 0x1F) << 3, ((color1Data >> 5) & 0x3F) << 2, (color1Data & 0x1F) << 3 };

                    Color[] colorPalette = new Color[]
                    {
                        new Color(colors0[0] / 255f, colors0[1] / 255f, colors0[2] / 255f),
                        new Color(colors1[0] / 255f, colors1[1] / 255f, colors1[2] / 255f),
                        new Color(((colors0[0] * 2 + colors1[0] + 1) / 3) / 255f, ((colors0[1] * 2 + colors1[1] + 1) / 3) / 255f, ((colors0[2] * 2 + colors1[2] + 1) / 3) / 255f),
                        new Color(((colors1[0] * 2 + colors0[0] + 1) / 3) / 255f, ((colors1[1] * 2 + colors0[1] + 1) / 3) / 255f, ((colors1[2] * 2 + colors0[2] + 1) / 3) / 255f)
                    };

                    if (color0Data < color1Data)
                    {
                        colorPalette[2] = new Color(((colors0[0] + colors1[0]) / 2) / 255f, ((colors0[1] + colors1[1]) / 2) / 255f, ((colors0[2] + colors1[2]) / 2) / 255f);
                        colorPalette[3] = new Color(((colors1[0] * 2 + colors0[0] + 1) / 3) / 255f, ((colors1[1] * 2 + colors0[1] + 1) / 3) / 255f, ((colors1[2] * 2 + colors0[2] + 1) / 3) / 255f);
                    }

                    int blockIndex = 0;
                    for (int blockY = 0; blockY < 4; blockY++)
                    {
                        for (int blockX = 0; blockX < 4; blockX++)
                        {
                            Color colorInBlock = colorPalette[(bitmask & (0x03 << blockIndex * 2)) >> blockIndex * 2];
                            texture2DColors[((row * width) + col) + ((blockY * width) + blockX)] = colorInBlock;
                            blockIndex++;
                        }
                    }
                }
            }

            return texture2DColors.ToArray();
        }
        /// <summary>
        /// Turn raw DXT5 bytes to a Color array that can be used in a Texture2D.
        /// </summary>
        /// <param name="data">Raw image byte data</param>
        /// <param name="width">Expected width of the image</param>
        /// <param name="height">Expected height of the image</param>
        /// <returns>Pixel color data</returns>
        public static Color[] DecompressDXT5(Stream data, ushort width, ushort height)
        {
            Color[] texture2DColors = new Color[width * height];

            for (int row = 0; row < height; row += 4)
            {
                for (int col = 0; col < width; col += 4)
                {
                    #region Alpha Information
                    byte alpha0Data = 0;
                    byte alpha1Data = 0;
                    uint alphamask = 0;

                    alpha0Data = DataParser.ReadByte(data);
                    alpha1Data = DataParser.ReadByte(data);
                    byte[] amdata = new byte[6];
                    data.Read(amdata, 0, amdata.Length);
                    alphamask = BitConverter.ToUInt32(amdata, 0);

                    float[] alphaPalette = new float[]
                    {
                        alpha0Data / 255f,
                        alpha1Data / 255f,
                        ((6 * alpha0Data + 1 * alpha1Data + 3) / 7) / 255f,
                        ((5 * alpha0Data + 2 * alpha1Data + 3) / 7) / 255f,
                        ((4 * alpha0Data + 3 * alpha1Data + 3) / 7) / 255f,
                        ((3 * alpha0Data + 4 * alpha1Data + 3) / 7) / 255f,
                        ((2 * alpha0Data + 5 * alpha1Data + 3) / 7) / 255f,
                        ((1 * alpha0Data + 6 * alpha1Data + 3) / 7) / 255f
                    };

                    if (alpha0Data <= alpha1Data)
                    {
                        alphaPalette[2] = (4 * alpha0Data + 1 * alpha1Data + 2) / 5;
                        alphaPalette[3] = (3 * alpha0Data + 2 * alpha1Data + 2) / 5;
                        alphaPalette[4] = (2 * alpha0Data + 3 * alpha1Data + 2) / 5;
                        alphaPalette[5] = (1 * alpha0Data + 4 * alpha1Data + 2) / 5;
                        alphaPalette[6] = 0;
                        alphaPalette[7] = 1;
                    }
                    #endregion

                    #region Color Information
                    ushort color0Data = 0;
                    ushort color1Data = 0;
                    uint bitmask = 0;

                    color0Data = DataParser.ReadUShort(data);
                    color1Data = DataParser.ReadUShort(data);
                    bitmask = DataParser.ReadUInt(data);

                    int[] colors0 = new int[] { ((color0Data >> 11) & 0x1F) << 3, ((color0Data >> 5) & 0x3F) << 2, (color0Data & 0x1F) << 3 };
                    int[] colors1 = new int[] { ((color1Data >> 11) & 0x1F) << 3, ((color1Data >> 5) & 0x3F) << 2, (color1Data & 0x1F) << 3 };

                    Color[] colorPalette = new Color[]
                    {
                        new Color(colors0[0] / 255f, colors0[1] / 255f, colors0[2] / 255f),
                        new Color(colors1[0] / 255f, colors1[1] / 255f, colors1[2] / 255f),
                        new Color(((colors0[0] * 2 + colors1[0] + 1) / 3) / 255f, ((colors0[1] * 2 + colors1[1] + 1) / 3) / 255f, ((colors0[2] * 2 + colors1[2] + 1) / 3) / 255f),
                        new Color(((colors1[0] * 2 + colors0[0] + 1) / 3) / 255f, ((colors1[1] * 2 + colors0[1] + 1) / 3) / 255f, ((colors1[2] * 2 + colors0[2] + 1) / 3) / 255f)
                    };
                    #endregion

                    #region Place All Information
                    int blockIndex = 0;
                    uint alphaBlockIndex1 = alphamask & 0x07, alphaBlockIndex2 = alphamask & 0x38;
                    for (int blockY = 0; blockY < 4; blockY++)
                    {
                        for (int blockX = 0; blockX < 4; blockX++)
                        {
                            Color colorInBlock = colorPalette[(bitmask & (0x03 << blockIndex * 2)) >> blockIndex * 2];
                            if (blockY < 2) colorInBlock.a = alphaPalette[alphaBlockIndex1 & 0x07];
                            else colorInBlock.a = alphaPalette[alphaBlockIndex2 & 0x07];
                            texture2DColors[((row * width) + col) + ((blockY * width) + blockX)] = colorInBlock;
                            blockIndex++;
                        }
                        alphaBlockIndex1 >>= 3;
                        alphaBlockIndex2 >>= 3;
                    }
                    #endregion
                }
            }

            return texture2DColors.ToArray();
        }

        /// <summary>
        /// This enum only holds the formats this helper class can read.
        /// </summary>
        public enum TextureFormat
        {
            BGR888,
            BGRA8888,
            DXT1,
            DXT3,
            DXT5,
        }
    }
}
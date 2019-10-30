//With help originally from https://answers.unity.com/questions/555984/can-you-load-dds-textures-during-runtime.html
//Then later found https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds-pguide#dds-file-layout

using System;
using UnityEngine;
using System.IO;
using UnitySourceEngine;

namespace UnityHelpers
{
    public static class DDSLoader
    {
        public const int DDS_ = 542327876; //From "DDS "
        public const int DDS_DXT1 = 827611204; //From "DXT1"
        public const int DDS_DXT2 = 844388420; //From "DXT2"
        public const int DDS_DXT3 = 861165636; //From "DXT3"
        public const int DDS_DXT4 = 877942852; //From "DXT4"
        public const int DDS_DXT5 = 894720068; //From "DXT5"
        public const int DDS_DX10 = 808540228; //From "DX10"

        public static Texture2D LoadDDSTexture(byte[] ddsBytes)
        {
            Texture2D texture = null;

            byte[] dxtBytes = null;
            DDS_HEADER header = new DDS_HEADER();
            DDS_HEADER_10 header_10 = new DDS_HEADER_10();

            bool isCompressed = false;
            using (MemoryStream ms = new MemoryStream(ddsBytes))
            {
                int magic = DataParser.ReadInt(ms);

                if (magic == DDS_)
                {
                    header.dwSize = DataParser.ReadInt(ms); //0 + 4 = 4
                    header.dwFlags = (DWFlags)DataParser.ReadInt(ms); //4 + 4 = 8
                    header.dwHeight = DataParser.ReadInt(ms); //8 + 4 = 12
                    header.dwWidth = DataParser.ReadInt(ms); //12 + 4 = 16
                    header.dwPitchOrLinearSize = DataParser.ReadInt(ms); //16 + 4 = 20
                    header.dwDepth = DataParser.ReadInt(ms); //20 + 4 = 24
                    header.dwMipMapCount = DataParser.ReadInt(ms); //24 + 4 = 28
                    header.dwReserved1 = new int[11]; //28 + 44 = 72
                    for (int i = 0; i < header.dwReserved1.Length; i++)
                        header.dwReserved1[i] = DataParser.ReadInt(ms);

                    header.ddspf.dwSize = DataParser.ReadInt(ms); //72 + 4 = 76
                    header.ddspf.dwFlags = (PIXELFORMAT_DWFlags)DataParser.ReadInt(ms); //76 + 4 = 80
                    header.ddspf.dwFourCC = DataParser.ReadInt(ms); //80 + 4 = 84
                    header.ddspf.dwRGBBitCount = DataParser.ReadInt(ms); //84 + 4 = 88
                    header.ddspf.dwRBitMask = DataParser.ReadInt(ms); //88 + 4 = 92
                    header.ddspf.dwGBitMask = DataParser.ReadInt(ms); //92 + 4 = 96
                    header.ddspf.dwBBitMask = DataParser.ReadInt(ms); //96 + 4 = 100
                    header.ddspf.dwABitMask = DataParser.ReadInt(ms); //100 + 4 = 104

                    header.dwCaps = DataParser.ReadInt(ms); //104 + 4 = 108
                    header.dwCaps2 = DataParser.ReadInt(ms); //108 + 4 = 112
                    header.dwCaps3 = DataParser.ReadInt(ms); //112 + 4 = 116
                    header.dwCaps4 = DataParser.ReadInt(ms); //116 + 4 = 120
                    header.dwReserved2 = DataParser.ReadInt(ms); //120 + 4 = 124

                    isCompressed = (header.ddspf.dwFlags & PIXELFORMAT_DWFlags.DDPF_FOURCC) != 0;
                    if (isCompressed && header.ddspf.dwFourCC == DDS_DX10)
                    {
                        Debug.Log(nameof(DDSLoader) + ": Reading extra header");
                        header_10.dxgiFormat = (DXGI_FORMAT)DataParser.ReadInt(ms);
                        header_10.resourceDimension = (D3D10_RESOURCE_DIMENSION)DataParser.ReadInt(ms);
                        header_10.miscFlag = DataParser.ReadUInt(ms);
                        header_10.arraySize = DataParser.ReadUInt(ms);
                        header_10.miscFlags2 = DataParser.ReadUInt(ms);
                    }

                    long dataLength = ms.Length - ms.Position - 1;
                    dxtBytes = new byte[dataLength];
                    ms.Read(dxtBytes, 0, dxtBytes.Length);
                }
                else
                    Debug.LogError(nameof(DDSLoader) + ": Invalid DDS magic, expected " + DDS_ + " got " + magic);
            }

            if (dxtBytes != null)
            {
                Texture2DHelpers.TextureFormat format;

                if (isCompressed)
                {
                    if (header.ddspf.dwFourCC == DDS_DXT5)
                        format = Texture2DHelpers.TextureFormat.DXT5;
                    else if (header.ddspf.dwFourCC == DDS_DXT3)
                        format = Texture2DHelpers.TextureFormat.DXT3;
                    else
                        format = Texture2DHelpers.TextureFormat.DXT1;
                }
                else if ((header.ddspf.dwFlags & PIXELFORMAT_DWFlags.DDPF_ALPHA) != 0 || (header.ddspf.dwFlags & PIXELFORMAT_DWFlags.DDPF_ALPHAPIXELS) != 0)
                    format = Texture2DHelpers.TextureFormat.BGRA8888;
                else
                    format = Texture2DHelpers.TextureFormat.BGR888;

                int width = header.dwWidth;
                int height = header.dwHeight;
                Color[] imageColors = Texture2DHelpers.DecompressRawBytes(dxtBytes, (ushort)width, (ushort)height, format);
                Texture2DHelpers.FlipVertical(imageColors, (ushort)width, (ushort)height);

                texture = new Texture2D(width, height);
                if (imageColors != null)
                    texture.SetPixels(imageColors);
                texture.Apply();
            }

            return texture;
        }

        public struct DDS_HEADER
        {
            public int dwSize;
            public DWFlags dwFlags;
            public int dwHeight;
            public int dwWidth;
            public int dwPitchOrLinearSize;
            public int dwDepth;
            public int dwMipMapCount;
            public int[] dwReserved1;
            public DDS_PIXELFORMAT ddspf;
            public int dwCaps;
            public int dwCaps2;
            public int dwCaps3;
            public int dwCaps4;
            public int dwReserved2;
        }
        public struct DDS_PIXELFORMAT
        {
            public int dwSize;
            public PIXELFORMAT_DWFlags dwFlags;
            public int dwFourCC;
            public int dwRGBBitCount;
            public int dwRBitMask;
            public int dwGBitMask;
            public int dwBBitMask;
            public int dwABitMask;
        }
        public struct DDS_HEADER_10
        {
            public DXGI_FORMAT dxgiFormat;
            public D3D10_RESOURCE_DIMENSION resourceDimension;
            public uint miscFlag;
            public uint arraySize;
            public uint miscFlags2;
        }
        [Flags]
        public enum DWFlags
        {
            DDSD_CAPS = 0x1, //Required in every .dds file.
            DDSD_HEIGHT = 0x2, //Required in every .dds file.
            DDSD_WIDTH = 0x4, //Required in every .dds file.
            DDSD_PITCH = 0x8, //Required when pitch is provided for an uncompressed texture.
            DDSD_PIXELFORMAT = 0x1000, //Required in every .dds file.
            DDSD_MIPMAPCOUNT = 0x20000, //Required in a mipmapped texture.
            DDSD_LINEARSIZE = 0x80000, //Required when pitch is provided for a compressed texture.
            DDSD_DEPTH = 0x800000 //Required in a depth texture.
        }
        [Flags]
        public enum PIXELFORMAT_DWFlags
        {
            DDPF_ALPHAPIXELS = 0x1, //Texture contains alpha data; dwRGBAlphaBitMask contains valid data. 	
            DDPF_ALPHA = 0x2, //Used in some older DDS files for alpha channel only uncompressed data (dwRGBBitCount contains the alpha channel bitcount; dwABitMask contains valid data) 	
            DDPF_FOURCC = 0x4, //Texture contains compressed RGB data; dwFourCC contains valid data.     
            DDPF_RGB = 0x40, //Texture contains uncompressed RGB data; dwRGBBitCount and the RGB masks(dwRBitMask, dwGBitMask, dwBBitMask) contain valid data.    
            DDPF_YUV = 0x200, //Used in some older DDS files for YUV uncompressed data(dwRGBBitCount contains the YUV bit count; dwRBitMask contains the Y mask, dwGBitMask contains the U mask, dwBBitMask contains the V mask) 	
            DDPF_LUMINANCE = 0x20000 //Used in some older DDS files for single channel color uncompressed data(dwRGBBitCount contains the luminance channel bit count; dwRBitMask contains the channel mask). Can be combined with DDPF_ALPHAPIXELS for a two channel DDS file.   
        }
        public enum DXGI_FORMAT
        {
            DXGI_FORMAT_UNKNOWN,
            DXGI_FORMAT_R32G32B32A32_TYPELESS,
            DXGI_FORMAT_R32G32B32A32_FLOAT,
            DXGI_FORMAT_R32G32B32A32_UINT,
            DXGI_FORMAT_R32G32B32A32_SINT,
            DXGI_FORMAT_R32G32B32_TYPELESS,
            DXGI_FORMAT_R32G32B32_FLOAT,
            DXGI_FORMAT_R32G32B32_UINT,
            DXGI_FORMAT_R32G32B32_SINT,
            DXGI_FORMAT_R16G16B16A16_TYPELESS,
            DXGI_FORMAT_R16G16B16A16_FLOAT,
            DXGI_FORMAT_R16G16B16A16_UNORM,
            DXGI_FORMAT_R16G16B16A16_UINT,
            DXGI_FORMAT_R16G16B16A16_SNORM,
            DXGI_FORMAT_R16G16B16A16_SINT,
            DXGI_FORMAT_R32G32_TYPELESS,
            DXGI_FORMAT_R32G32_FLOAT,
            DXGI_FORMAT_R32G32_UINT,
            DXGI_FORMAT_R32G32_SINT,
            DXGI_FORMAT_R32G8X24_TYPELESS,
            DXGI_FORMAT_D32_FLOAT_S8X24_UINT,
            DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS,
            DXGI_FORMAT_X32_TYPELESS_G8X24_UINT,
            DXGI_FORMAT_R10G10B10A2_TYPELESS,
            DXGI_FORMAT_R10G10B10A2_UNORM,
            DXGI_FORMAT_R10G10B10A2_UINT,
            DXGI_FORMAT_R11G11B10_FLOAT,
            DXGI_FORMAT_R8G8B8A8_TYPELESS,
            DXGI_FORMAT_R8G8B8A8_UNORM,
            DXGI_FORMAT_R8G8B8A8_UNORM_SRGB,
            DXGI_FORMAT_R8G8B8A8_UINT,
            DXGI_FORMAT_R8G8B8A8_SNORM,
            DXGI_FORMAT_R8G8B8A8_SINT,
            DXGI_FORMAT_R16G16_TYPELESS,
            DXGI_FORMAT_R16G16_FLOAT,
            DXGI_FORMAT_R16G16_UNORM,
            DXGI_FORMAT_R16G16_UINT,
            DXGI_FORMAT_R16G16_SNORM,
            DXGI_FORMAT_R16G16_SINT,
            DXGI_FORMAT_R32_TYPELESS,
            DXGI_FORMAT_D32_FLOAT,
            DXGI_FORMAT_R32_FLOAT,
            DXGI_FORMAT_R32_UINT,
            DXGI_FORMAT_R32_SINT,
            DXGI_FORMAT_R24G8_TYPELESS,
            DXGI_FORMAT_D24_UNORM_S8_UINT,
            DXGI_FORMAT_R24_UNORM_X8_TYPELESS,
            DXGI_FORMAT_X24_TYPELESS_G8_UINT,
            DXGI_FORMAT_R8G8_TYPELESS,
            DXGI_FORMAT_R8G8_UNORM,
            DXGI_FORMAT_R8G8_UINT,
            DXGI_FORMAT_R8G8_SNORM,
            DXGI_FORMAT_R8G8_SINT,
            DXGI_FORMAT_R16_TYPELESS,
            DXGI_FORMAT_R16_FLOAT,
            DXGI_FORMAT_D16_UNORM,
            DXGI_FORMAT_R16_UNORM,
            DXGI_FORMAT_R16_UINT,
            DXGI_FORMAT_R16_SNORM,
            DXGI_FORMAT_R16_SINT,
            DXGI_FORMAT_R8_TYPELESS,
            DXGI_FORMAT_R8_UNORM,
            DXGI_FORMAT_R8_UINT,
            DXGI_FORMAT_R8_SNORM,
            DXGI_FORMAT_R8_SINT,
            DXGI_FORMAT_A8_UNORM,
            DXGI_FORMAT_R1_UNORM,
            DXGI_FORMAT_R9G9B9E5_SHAREDEXP,
            DXGI_FORMAT_R8G8_B8G8_UNORM,
            DXGI_FORMAT_G8R8_G8B8_UNORM,
            DXGI_FORMAT_BC1_TYPELESS,
            DXGI_FORMAT_BC1_UNORM,
            DXGI_FORMAT_BC1_UNORM_SRGB,
            DXGI_FORMAT_BC2_TYPELESS,
            DXGI_FORMAT_BC2_UNORM,
            DXGI_FORMAT_BC2_UNORM_SRGB,
            DXGI_FORMAT_BC3_TYPELESS,
            DXGI_FORMAT_BC3_UNORM,
            DXGI_FORMAT_BC3_UNORM_SRGB,
            DXGI_FORMAT_BC4_TYPELESS,
            DXGI_FORMAT_BC4_UNORM,
            DXGI_FORMAT_BC4_SNORM,
            DXGI_FORMAT_BC5_TYPELESS,
            DXGI_FORMAT_BC5_UNORM,
            DXGI_FORMAT_BC5_SNORM,
            DXGI_FORMAT_B5G6R5_UNORM,
            DXGI_FORMAT_B5G5R5A1_UNORM,
            DXGI_FORMAT_B8G8R8A8_UNORM,
            DXGI_FORMAT_B8G8R8X8_UNORM,
            DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM,
            DXGI_FORMAT_B8G8R8A8_TYPELESS,
            DXGI_FORMAT_B8G8R8A8_UNORM_SRGB,
            DXGI_FORMAT_B8G8R8X8_TYPELESS,
            DXGI_FORMAT_B8G8R8X8_UNORM_SRGB,
            DXGI_FORMAT_BC6H_TYPELESS,
            DXGI_FORMAT_BC6H_UF16,
            DXGI_FORMAT_BC6H_SF16,
            DXGI_FORMAT_BC7_TYPELESS,
            DXGI_FORMAT_BC7_UNORM,
            DXGI_FORMAT_BC7_UNORM_SRGB,
            DXGI_FORMAT_AYUV,
            DXGI_FORMAT_Y410,
            DXGI_FORMAT_Y416,
            DXGI_FORMAT_NV12,
            DXGI_FORMAT_P010,
            DXGI_FORMAT_P016,
            DXGI_FORMAT_420_OPAQUE,
            DXGI_FORMAT_YUY2,
            DXGI_FORMAT_Y210,
            DXGI_FORMAT_Y216,
            DXGI_FORMAT_NV11,
            DXGI_FORMAT_AI44,
            DXGI_FORMAT_IA44,
            DXGI_FORMAT_P8,
            DXGI_FORMAT_A8P8,
            DXGI_FORMAT_B4G4R4A4_UNORM,
            DXGI_FORMAT_P208,
            DXGI_FORMAT_V208,
            DXGI_FORMAT_V408,
            DXGI_FORMAT_FORCE_UINT
        }
        public enum D3D10_RESOURCE_DIMENSION
        {
            D3D10_RESOURCE_DIMENSION_UNKNOWN,
            D3D10_RESOURCE_DIMENSION_BUFFER,
            D3D10_RESOURCE_DIMENSION_TEXTURE1D,
            D3D10_RESOURCE_DIMENSION_TEXTURE2D,
            D3D10_RESOURCE_DIMENSION_TEXTURE3D
        }
    }
}
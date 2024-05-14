using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Tiger.Schema;
using Tiger.Schema.Entity;

namespace Tiger.Exporters
{
    public enum GammaType
    {
        Linear,
        SRGB,
    }

    public enum ImageFormatType
    {
        DXT5,
        DXT1,
        RGBA8888,
        BC7,
        BC6H,
    }

    public enum ImageDimension
    {
        [Description("1D")]
        _1D,
        [Description("2D")]
        _2D,
        [Description("3D")]
        _3D,
        [Description("1DArray")]
        _1DARRAY,
        [Description("2DArray")]
        _2DARRAY,
        [Description("3DArray")]
        _3DARRAY,
        [Description("CUBE")]
        CUBE,
        [Description("CUBEARRAY")]
        CUBEARRAY
    }

    public class TextureFile
    {
        public List<string> Images { get; set; }

        public string InputColorSpace { get; set; }

        public string OutputFormat { get; set; }

        public string OutputColorSpace { get; set; }

        public string OutputTypeString { get; set; }

        public static TextureFile CreateDefault(Texture texture, ImageDimension dimension)
        {
            return new TextureFile
            {
                Images = new List<string> { $"textures/{texture.Hash}.png" },
                OutputFormat = ImageFormatType.RGBA8888.ToString(),
                OutputColorSpace = (texture.IsSrgb() ? GammaType.SRGB : GammaType.Linear).ToString(),
                InputColorSpace = (texture.IsSrgb() ? GammaType.SRGB : GammaType.Linear).ToString(),
                OutputTypeString = GetDisplayName(dimension)
            };
        }

        private static string GetDisplayName(ImageDimension value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}

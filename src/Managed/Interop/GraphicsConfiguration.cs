using Tomlet.Attributes;

namespace Interop;

enum TextureQuality
{
    Lowest = 0,
    Low,
    Medium,
    High,
    VeryHigh,
    Ultra,
    MaxValue = Ultra,
}

enum TextureFiltering
{
    Anisotropic2x = 0,
    Anisotropic4x,
    Anisotropic8x,
    Anisotropic16x,
    MaxValue = Anisotropic16x,
}

enum AmbientOcclusion
{
    None = 0,
    Ssao,
    Hbao,
    MaxValue = Hbao,
}

enum AntiAliasing
{
    None = 0,
    Fxaa,
    Smaa,
    FilmicSmaa1x,
    SmaaT2x,
    FilmicSmaaT2x,
    MaxValue = FilmicSmaaT2x,
}

record class GraphicsConfiguration(
    [property: TomlProperty("texture_quality")]
    TextureQuality TextureQuality,
    [property: TomlProperty("texture_filtering")]
    TextureFiltering TextureFiltering,
    [property: TomlProperty("aa_technique")]
    AntiAliasing AntiAliasing,
    [property: TomlProperty("ao_technique")]
    AmbientOcclusion AmbientOcclusion
)
{
    public GraphicsConfiguration()
        : this(TextureQuality.Medium, TextureFiltering.Anisotropic16x, AntiAliasing.None, AmbientOcclusion.None)
    {
    }
}

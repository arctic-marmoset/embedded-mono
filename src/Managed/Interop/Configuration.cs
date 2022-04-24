using Tomlet.Attributes;

namespace Interop;

record class Configuration(
    [property: TomlProperty("version")]
    uint Version,
    [property: TomlProperty("display")]
    DisplayConfiguration Display,
    [property: TomlProperty("graphics")]
    GraphicsConfiguration Graphics
)
{
    public Configuration() : this(0, new(), new())
    {
    }
}

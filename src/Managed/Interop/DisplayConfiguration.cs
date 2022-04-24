using Tomlet.Attributes;

namespace Interop;

record class Dimensions(
    [property: TomlProperty("width")]
    ushort Width,
    [property: TomlProperty("height")]
    ushort Height
)
{
    public Dimensions() : this(0, 0)
    {
    }
}

enum DisplayMode
{
    Windowed = 0,
    Fullscreen,
    Borderless,
    MaxValue = Borderless,
}

record class DisplayConfiguration(
    [property: TomlProperty("vsync")]
    bool VSync,
    [property: TomlProperty("mode")]
    DisplayMode DisplayMode,
    [property: TomlProperty("monitor_index")]
    byte MonitorIndex,
    [property: TomlProperty("resolution")]
    Dimensions Resolution,
    [property: TomlProperty("max_fps")]
    uint MaxFps,
    [property: TomlProperty("max_fps_menu")]
    uint MaxFpsInMenus,
    [property: TomlProperty("max_fps_inactive")]
    uint MaxFpsWhenInactive
)
{
    public DisplayConfiguration() : this(false, DisplayMode.Windowed, 0, new(1280, 720), 0, 0, 0)
    {
    }
}

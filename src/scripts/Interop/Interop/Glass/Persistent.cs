using System;
using System.IO;

namespace Glass;

static class Persistent
{
    public static readonly string RootPath =
        Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\ArcticMarmoset\Glass");

    public static readonly string ConfigFileName = "config.toml";

    public static string ConfigFilePath => Path.Combine(RootPath, ConfigFileName);
}

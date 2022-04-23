using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Glass;
using Tomlet;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace Interop;

public static class Interop
{
    private static Configuration Config = new();

    public static void Setup() => SetupAsync().Wait();

    public static void Update()
    {
        Console.WriteLine("Module updated.");
    }

    private static async Task SetupAsync()
    {
        TomletMain.RegisterMapper(EnumSerializer, EnumDeserializer<TextureFiltering>);
        TomletMain.RegisterMapper(EnumSerializer, EnumDeserializer<TextureQuality>);
        TomletMain.RegisterMapper(EnumSerializer, EnumDeserializer<AmbientOcclusion>);
        TomletMain.RegisterMapper(EnumSerializer, EnumDeserializer<AntiAliasing>);
        TomletMain.RegisterMapper(EnumSerializer, EnumDeserializer<DisplayMode>);

        Configuration? maybeConfig = await TryReadConfigAsync();
        if (maybeConfig is null)
        {
            await WriteDefaultConfigAsync();
        }
        else
        {
            Config = maybeConfig;
        }

        Native.SetExitCode(0x42);
    }

    private static TomlLong EnumSerializer<T>(T value)
        where T : Enum
    {
        long rawValue = (long)(int)(object)value;
        return new TomlLong(rawValue);
    }

    private static T EnumDeserializer<T>(TomlValue tomlValue)
        where T : Enum
    {
        if (tomlValue is not TomlLong tomlLong)
        {
            throw new TomlTypeMismatchException(typeof(TomlLong), tomlValue.GetType(), typeof(T));
        }

        long rawValue = tomlLong.Value;

        T maxValue = Enum.GetValues(typeof(T)).Cast<T>().Max();
        long maxRawValue = (long)(int)(object)maxValue;

        if (rawValue < 0 || rawValue > maxRawValue)
        {
            rawValue = 0;
        }

        if (rawValue > maxRawValue)
        {
            rawValue = maxRawValue;
        }

        var value = (T)(object)(int)rawValue;
        return value;
    }

    private static async Task<Configuration?> TryReadConfigAsync()
    {
        using StreamReader stream = File.OpenText(Persistent.ConfigFilePath);
        string configToml = await stream.ReadToEndAsync();

        try
        {
            return TomletMain.To<Configuration>(configToml);
        }
        catch (TomlException exception)
        {
            Console.WriteLine(exception.ToString());
        }

        return null;
    }

    private static async Task WriteDefaultConfigAsync()
    {
        string configToml = TomletMain.TomlStringFrom(new Configuration());

        using var configFile = new StreamWriter(Persistent.ConfigFilePath);
        await configFile.WriteAsync(configToml);
    }
}

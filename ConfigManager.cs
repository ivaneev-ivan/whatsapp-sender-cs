﻿#nullable enable
using System.Text.Json;

namespace whatsapp_sender;

internal static class ConfigManager
{
    public static TimeDelay ReadConfigFile()
    {
        try
        {
            using var file = new FileStream("data.json", FileMode.OpenOrCreate);
            var item = JsonSerializer.Deserialize<Dictionary<string, int>>(file);
            if (item == null) throw new ConfigNotFound("Файл data.json не найден");
            return new TimeDelay(item["StartDelay"], item["StopDelay"]);
        }
        catch (Exception)
        {
            Console.WriteLine("data.json открыт другим процессом");
            Environment.Exit(0);
        }

        return new TimeDelay(0, 0);
    }

    public static bool StartSending()
    {
        try
        {
            using var file = new FileStream("data.json", FileMode.OpenOrCreate);
            var item = JsonSerializer.Deserialize<Dictionary<string, int>>(file);
            if (item == null) throw new ConfigNotFound("Файл data.json не найден");
            return item["StartSending"] == 1;
        }
        catch (Exception)
        {
            Console.WriteLine("data.json открыт другим процессом");
            Environment.Exit(0);
        }

        return true;
    }
    
    /// <summary>
    ///     Записывает списко девайсов в конфиг файл
    /// </summary>
    /// <param name="devices">Коллекция девайсов</param>
    public static void WriteDevicesToConfig(List<DeviceItem> devices)
    {
        var data = "";
        foreach (var device in devices) data += device + "\n";

        if (data.Length > 1 && data[^1] == '\n') data = data.Substring(0, data.Length - 1);

        File.WriteAllText("devices.txt", data);
    }

    /// <summary>
    ///     Полученение всех устройств из конфига
    /// </summary>
    /// <returns>Список девайсов</returns>
    public static List<DeviceItem> GetDevicesFromConfig()
    {
        try
        {
            var data = File.ReadAllText("devices.txt");
            Console.WriteLine(data);
            List<DeviceItem> devices = new();
            if (string.IsNullOrEmpty(data)) return new List<DeviceItem>();

            foreach (var element in data.Split("\n")) devices.Add(DeviceItem.FromString(element));

            return devices;
        }
        catch (FileNotFoundException)
        {
            return new List<DeviceItem>();
        }
    }
}

internal class ConfigNotFound(string? message) : Exception(message);
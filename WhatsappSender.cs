#nullable enable
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;

namespace whatsapp_sender;

public struct Worker(DeviceItem device, Thread thread)
{
    public DeviceItem DeviceItem = device;
    public Thread Thread = thread;
}

/// <summary>
///     Класс по работе с adb и whatsapp
/// </summary>
public class WhatsappSender
{
    public static List<Worker> Workers = [];
    public static List<DeviceItem> Devices = [];
    private readonly AdbClient _client = new();
    private List<string> _files = new List<string>();
    private static Random _random = new Random();

    private readonly string dataDir = "/sdcard/WhatsappSender/";

    public WhatsappSender()
    {
        StartAdb();
    }

    public void CopyDataDir(string path, DeviceData device)
    {
        using (SyncService service = new SyncService(device))
        {
            foreach (var file in Directory.GetFiles(path))
            {
                using (FileStream stream = File.OpenRead(file))
                {
                    var targerPath = dataDir + Path.GetFileName(file);
                    _files.Add(targerPath);
                    try
                    {
                        service.Push(stream, targerPath, 4, DateTimeOffset.Now, null);
                    }
                    catch (AdvancedSharpAdbClient.Exceptions.AdbException)
                    {
                        //
                    }
                }
            }
        }
    }

    public void InitDevices()
    {
        // Devices = ConfigManager.GetDevicesFromConfig();
        // if (Devices.Count == 0)
        // {
        Devices = GetAllDevicesToConfig();
        if (Devices.Count == 0)
        {
            Console.WriteLine("Устройства не найдены");
            return;
        }

        ConfigManager.WriteDevicesToConfig(Devices);
        // }

        Console.WriteLine("Получил и записал список всех устройств");
    }

    public DeviceData? GetClient(DeviceItem data)
    {
        try
        {
            var devices = _client.GetDevices();
            foreach (var device in devices)
                if (device.Serial == data.Name)
                    return device;

            return null;
        }
        catch (SocketException)
        {
            StartAdb();
            return GetClient(data);
        }
    }

    /// <summary>
    ///     Метод по запуску adb
    /// </summary>
    public static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        Directory.CreateDirectory(targetPath);
        foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
    }

    private static string GetAdbPath()
    {
        var fileName = "/usr/bin/adb";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) fileName = "platform-tools/adb.exe";
        return fileName;
    }

    public static void KillAdb()
    {
        var fileName = GetAdbPath();
        var startInfo = new ProcessStartInfo { FileName = fileName, Arguments = "kill-server" };
        var proc = new Process { StartInfo = startInfo };
        proc.Start();
        Console.WriteLine("ADB KILL");
    }

    private static void StartAdb()
    {
        var fileName = GetAdbPath();
        var startInfo = new ProcessStartInfo { FileName = fileName, Arguments = "devices" };
        var proc = new Process { StartInfo = startInfo };
        proc.Start();
        var server = new AdbServer();
        // перезагрузка сервера adb
        try
        {
            if (AdbServer.Instance.GetStatus().IsRunning) server.StopServer();
        }
        catch (Exception)
        {
            //
        }

        Thread.Sleep(1000);
    }

    /// <summary>
    ///     Установка adb клавиатуры, если ее нет и установка ее, вместо обычной
    /// </summary>
    private void InstallAdbKeyboard(DeviceData device)
    {
        while (true)
        {
            var version = _client.GetPackageVersion(device, "com.android.adbkeyboard");
            if (version.VersionCode == 0)
            {
                var manager = new PackageManager(_client, device);
                manager.InstallPackage(@"ADBKeyboard.apk", _ => { });
            }
            else
            {
                break;
            }
        }

        _client.ExecuteRemoteCommand("ime enable com.android.adbkeyboard/.AdbIME", device);
        _client.ExecuteRemoteCommand("ime set com.android.adbkeyboard/.AdbIME", device);
    }

    /// <summary>
    ///     Отправка сообщения с помощью модуля adbKeyboard
    /// </summary>
    /// <param name="device">Устройство</param>
    /// <param name="message">Сообщение, которое нужно написать</param>
    private void SendText(DeviceData device, string message)
    {
        _client.ExecuteRemoteCommand($"am broadcast -a ADB_INPUT_TEXT --es msg '{message}'", device);
        Thread.Sleep(1000);
    }

    private void SendEnter(DeviceData device)
    {
        _client.ExecuteRemoteCommand("input keyevent 66", device);
        Thread.Sleep(1000);
    }

    private void sendRandomAttach(DeviceData device, string phone)
    {
        string imageAttach = "file://" + _files[_random.Next(_files.Count)];
        string command =
            $"am start -a android.intent.action.SEND -t  text/plain -e jid '{phone.Replace("+", "")}@s.whatsapp.net' --eu android.intent.extra.STREAM {imageAttach}  -p com.whatsapp.w4b";
        _client.ExecuteRemoteCommand(command, device);
        Thread.Sleep(2000);
        SendImage(device);
    }

    private void SendTypeWordText(DeviceData device, string message, string phone)
    {
        var parts = TextPartition.GetTextPartition(message);
        foreach (var part in parts)
        {
            var commandsParts = TextPartitionWithCommand.GetTextPartitionWithCommand(part);
            foreach (var textPartitionWithCommand in commandsParts)
            {
                var imageParts = textPartitionWithCommand.Message.Split("%img%");
                foreach (var text in imageParts)
                {
                    if (text == imageParts[1])
                    {
                        SendMessage(device);
                        sendRandomAttach(device, phone);
                    }

                    var words = text.Split(" ");
                    var i = 0;
                    foreach (var w in words)
                    {
                        var a = "";
                        if (i + 1 != words.Count()) a = " ";
                        SendText(device, w + a);
                        i++;
                    }

                    switch (textPartitionWithCommand.CommandType)
                    {
                        case CommandType.NewMessage:
                            SendMessage(device);
                            break;
                        case CommandType.Enter:
                            SendEnter(device);
                            break;
                        case CommandType.None:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            Thread.Sleep(part.Delay.GetDelay());
        }
    }

    /// <summary>
    ///     Нажатие на кнопку откравки сообщения
    /// </summary>
    /// <exception cref="Exception">Если кнопка не будет найдена</exception>
    private void SendMessage(DeviceData device)
    {
        var sendButton = _client.FindElement(device,
            "//node[@resource-id='com.whatsapp.w4b:id/conversation_entry_action_button']");
        if (sendButton == null) throw new Exception("Кнопка для отправки не найдена");
        sendButton.Click();
        Thread.Sleep(1000);
    }
    
    private void SendImage(DeviceData device)
    {
        var sendButton = _client.FindElement(device,
            "//node[@resource-id='com.whatsapp.w4b:id/send']");
        if (sendButton == null) throw new Exception("Кнопка для отправки не найдена");
        sendButton.Click();
        Thread.Sleep(1000);
    }

    /// <summary>
    ///     Открытие чата в whatsapp по номеру телефона
    /// </summary>
    /// <param name="phone">Номер телефона</param>
    private void OpenChat(DeviceData device, string phone)
    {
        _client.ExecuteRemoteCommand(
            $"am start -a android.intent.action.VIEW -d whatsapp://send?phone={phone}",
            device);
        Thread.Sleep(1000);
    }

    // /// <summary>
    // ///     Получение мобильного телефона
    // /// </summary>
    // /// <returns>Мобильный телефон</returns>
    // public DeviceData? GetDevice(int depth = 0)
    // {
    //     try
    //     {
    //         var device = _client.GetDevices().FirstOrDefault();
    //         return device;
    //     }
    //     catch (Exception)
    //     {
    //         if (depth == 2)
    //         {
    //             throw new Exception("Не удалось найти устройство");
    //         }
    //         StartAdb();
    //         return GetDevice(depth + 1);
    //     }
    // }

    public List<DeviceItem> GetAllDevicesToConfig()
    {
        List<DeviceItem> result = new();
        var devices = _client.GetDevices();
        foreach (var device in devices) result.Add(new DeviceItem(device.Model, device.Serial));
        return result;
    }

    /// <summary>
    ///     Остановка приложения whatsapp
    /// </summary>
    public void StopWhatsapp(DeviceData device)
    {
        _client.StopApp(device, "com.whatsapp.w4b");
        Thread.Sleep(1000);
    }

    /// <summary>
    ///     Получение message box, для написания сообщения
    /// </summary>
    /// <returns>Message box</returns>
    /// <exception cref="Exception">Если не получилось получить message box</exception>
    private void GetMessageBox(DeviceData device)
    {
        var messageBox = _client.FindElement(device, "//node[@resource-id='com.whatsapp.w4b:id/entry']",
            new TimeSpan(10 * 10000000));
        if (messageBox == null) throw new MessageBoxNotFound("Не удалось найти тект бокс");
        messageBox.Click();
    }

    /// <summary>
    ///     Действия, по возращению в исходный вид, до запуска программы
    /// </summary>
    public void Close(DeviceData device)
    {
        _client.ExecuteRemoteCommand("ime reset", device);
    }

    public string SendToPhone(DeviceData device, UserData data, string message, ExcelReader reader, DeviceItem phone)
    {
        while (true)
            try
            {
                reader.WriteStatusPhone(data, "inprogress");
                break;
            }
            catch (Exception e)
            {
                Thread.Sleep(1000);
            }

        var isSent = false;
        InstallAdbKeyboard(device);
        StopWhatsapp(device);
        OpenChat(device, data.Phone);
        _client.ExecuteRemoteCommand("ime enable com.android.adbkeyboard/.AdbIME", device);
        _client.ExecuteRemoteCommand("ime set com.android.adbkeyboard/.AdbIME", device);
        try
        {
            GetMessageBox(device);
            SendTypeWordText(device, message, data.Phone);
            SendMessage(device);
            isSent = true;
        }
        catch (MessageBoxNotFound)
        {
            //
        }

        while (true)
            try
            {
                var status = reader.WriteStatusPhone(data, isSent ? "sent" : "notsent", phone, message);
                Close(device);
                return status;
            }
            catch (Exception e)
            {
                Thread.Sleep(1000);
            }
    }
}

class MessageBoxNotFound : Exception
{
    public MessageBoxNotFound(string message) : base(message)
    {
    }
}
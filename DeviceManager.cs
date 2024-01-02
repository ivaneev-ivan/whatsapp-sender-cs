using System.Diagnostics;
using System.Runtime.InteropServices;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.DeviceCommands.Models;
using AdvancedSharpAdbClient.Models;

namespace whatsapp_sender;

/// <summary>
///     Класс по работе с adb и whatsapp
/// </summary>
public class DeviceManager
{
    private readonly AdbClient _client = new();
    private readonly DeviceData _device;

    public DeviceManager()
    {
        StartAdb();
        _device = GetDevice() ?? throw new Exception("Устройства не найдены");
        InstallAdbKeyboard();
    }

    /// <summary>
    ///     Метод по запуску adb
    /// </summary>
    private static void StartAdb()
    {
        if (AdbServer.Instance.GetStatus().IsRunning) return;
        var server = new AdbServer();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var result = server.StartServer("adb.exe");
            if (result != StartServerResult.Started) throw new Exception("Не получилось запустить adb");
        }
        else
        {
            var startInfo = new ProcessStartInfo { FileName = "/usr/bin/adb", Arguments = "devices" };
            var proc = new Process { StartInfo = startInfo };
            proc.Start();
        }
    }

    /// <summary>
    ///     Установка adb клавиатуры, если ее нет и установка ее, вместо обычной
    /// </summary>
    private void InstallAdbKeyboard()
    {
        while (true)
        {
            var version = _client.GetPackageVersion(_device, "com.android.adbkeyboard");
            if (version.VersionCode == 0)
            {
                var manager = new PackageManager(_client, _device);
                manager.InstallPackage(@"ADBKeyboard.apk", args => { });
            }
            else
            {
                break;
            }
        }

        _client.ExecuteRemoteCommand("ime enable com.android.adbkeyboard/.AdbIME", _device);
        _client.ExecuteRemoteCommand("ime set com.android.adbkeyboard/.AdbIME", _device);
    }

    /// <summary>
    ///     Отправка сообщения с помощью модуля adbKeyboard
    /// </summary>
    /// <param name="message">Сообщение, которое нужно написать</param>
    public void SendText(string message)
    {
        _client.ExecuteRemoteCommand($"am broadcast -a ADB_INPUT_TEXT --es msg '{message}'", _device);
        Thread.Sleep(100);
    }

    /// <summary>
    ///     Нажатие на кнопку откравки сообщения
    /// </summary>
    /// <exception cref="Exception">Если кнопка не будет найдена</exception>
    public void SendMessage()
    {
        var sendButton = _client.FindElement(_device,
            "//node[@resource-id='com.whatsapp.w4b:id/conversation_entry_action_button']");
        if (sendButton == null) throw new Exception("Кнопка для отправки не найдена");
        sendButton.Click();
        Thread.Sleep(100);
    }

    /// <summary>
    ///     Открытие чата в whatsapp по номеру телефона
    /// </summary>
    /// <param name="phone">Номер телефона</param>
    public void OpenChat(string phone)
    {
        _client.ExecuteRemoteCommand($"am start -a android.intent.action.VIEW -d whatsapp://send?phone={phone}",
            _device);
        Thread.Sleep(100);
    }

    /// <summary>
    ///     Получение мобильного телефона
    /// </summary>
    /// <returns>Мобильный телефон</returns>
    private DeviceData? GetDevice()
    {
        var device = _client.GetDevices().FirstOrDefault();
        return device;
    }

    /// <summary>
    ///     Остановка приложения whatsapp
    /// </summary>
    public void StopWhatsapp()
    {
        _client.StopApp(_device, "com.whatsapp.w4b");
        Thread.Sleep(100);
    }

    /// <summary>
    ///     Получение message box, для написания сообщения
    /// </summary>
    /// <returns>Message box</returns>
    /// <exception cref="Exception">Если не получилось получить message box</exception>
    public Element GetMessageBox()
    {
        var messageBox = _client.FindElement(_device, "//node[@resource-id='com.whatsapp.w4b:id/entry']",
            TimeSpan.FromSeconds(10));
        if (messageBox == null) throw new Exception("Не удалось найти тект бокс");
        messageBox.Click();
        return messageBox;
    }

    /// <summary>
    ///     Действия, по возращению в исходный вид, до запуска программы
    /// </summary>
    public void Close()
    {
        _client.ExecuteRemoteCommand("ime reset", _device);
    }
}
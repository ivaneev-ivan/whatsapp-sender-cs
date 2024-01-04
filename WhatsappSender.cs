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
public class WhatsappSender
{
    private readonly AdbClient _client = new();
    private readonly DeviceData _device;

    public WhatsappSender()
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

        var fileName = "/usr/bin/adb";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) fileName = "adb.exe";

        var startInfo = new ProcessStartInfo { FileName = fileName, Arguments = "devices" };
        var proc = new Process { StartInfo = startInfo };
        proc.Start();
        Thread.Sleep(1000);
    }

    /// <summary>
    ///     Установка adb клавиатуры, если ее нет и установка ее, вместо обычной
    /// </summary>
    private async void InstallAdbKeyboard()
    {
        while (true)
        {
            var version = await _client.GetPackageVersionAsync(_device, "com.android.adbkeyboard");
            if (version.VersionCode == 0)
            {
                var manager = new PackageManager(_client, _device);
                manager.InstallPackage(@"ADBKeyboard.apk", _ => { });
            }
            else
            {
                break;
            }
        }

        await _client.ExecuteRemoteCommandAsync("ime enable com.android.adbkeyboard/.AdbIME", _device);
        await _client.ExecuteRemoteCommandAsync("ime set com.android.adbkeyboard/.AdbIME", _device);
    }

    /// <summary>
    ///     Отправка сообщения с помощью модуля adbKeyboard
    /// </summary>
    /// <param name="message">Сообщение, которое нужно написать</param>
    public void SendText(string message)
    {
        _client.ExecuteRemoteCommand($"am broadcast -a ADB_INPUT_TEXT --es msg '{message}'", _device);
        Thread.Sleep(1000);
    }

    public void SendEnter()
    {
        _client.ExecuteRemoteCommand("input keyevent 66", _device);
        Thread.Sleep(1000);
    }

    public void SendTypeWordText(string message)
    {
        var parts = TextPartition.GetTextPartition(message);
        foreach (var part in parts)
        {
            var commandsParts = TextPartitionWithCommand.GetTextPartitionWithCommand(part);
            foreach (var textPartitionWithCommand in commandsParts)
            {
                var words = textPartitionWithCommand.Message.Split(" ");
                foreach (var w in words) SendText(w + " ");
                switch (textPartitionWithCommand.CommandType)
                {
                    case CommandType.NewMessage:
                        SendMessage();
                        break;
                    case CommandType.Enter:
                        SendEnter();
                        break;
                }
            }

            Thread.Sleep(part.Delay.GetDelay() * 1000);
        }
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
        Thread.Sleep(1000);
    }

    /// <summary>
    ///     Открытие чата в whatsapp по номеру телефона
    /// </summary>
    /// <param name="phone">Номер телефона</param>
    public void OpenChat(string phone)
    {
        _client.ExecuteRemoteCommand(
            $"am start -a android.intent.action.VIEW -d whatsapp://send?phone={phone}",
            _device);
        Thread.Sleep(1000);
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
        Thread.Sleep(1000);
    }

    /// <summary>
    ///     Получение message box, для написания сообщения
    /// </summary>
    /// <returns>Message box</returns>
    /// <exception cref="Exception">Если не получилось получить message box</exception>
    public Element GetMessageBox()
    {
        var messageBox = _client.FindElement(_device, "//node[@resource-id='com.whatsapp.w4b:id/entry']",
            new TimeSpan(10 * 10000000));
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
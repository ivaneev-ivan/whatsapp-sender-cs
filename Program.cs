using System.Text;
using OfficeOpenXml;

namespace whatsapp_sender;

internal static class Program
{
    private static Thread GetThread(DeviceItem device, ExcelReader excelReader)
    {
        return new Thread(() => StartSendingWithPhone(device, excelReader));
    }
    private static void Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        ExcelReader excelReader = new("base.xlsx");
        new WhatsappSender(null).InitDevices();
        foreach (var client in WhatsappSender.Devices)
        {
            var thread = GetThread(client, excelReader);
            WhatsappSender.Workers.Add(new Worker(client, thread));
            thread.Start();
        }
        
        while (ExcelReader.Phones.Count != 0 && WhatsappSender.Workers.Count != 0)
        {
            for (int i = 0; i < WhatsappSender.Workers.Count; i++)
            {
                try
                {
                    var worker = WhatsappSender.Workers[i];
                    if (!worker.Thread.IsAlive)
                    {
                        WhatsappSender.Workers.Remove(worker);
                        var thread = GetThread(worker.DeviceItem, excelReader);
                        WhatsappSender.Workers.Add(new Worker(worker.DeviceItem, thread));
                        thread.Start();
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    //
                }
            }
        }
        Console.WriteLine("Рассылка завершена");
        Console.Write("Для закрытия консоли нажмите любую клавишу . . .");
        Console.ReadLine();
    }

    private static string GetMessage(UserData data)
    {
        MessageReader messageReader = new();
        var message = messageReader.GetRandomRandomizedMessage();
        message = message.Replace("<name>", data.Name).Trim();
        return message;
    }

    private static void StartSendingWithPhone(DeviceItem device, ExcelReader reader)
    {
        WhatsappSender sender = new(device);
        TimeDelay delay = ConfigManager.ReadConfigFile();
        var client = sender.GetClient(device);
        if (client is null)
        {
            var find = WhatsappSender.Workers.Find(worker => worker.DeviceItem.Name == device.Name);
            WhatsappSender.Workers.Remove(find);
            WhatsappSender.Devices.Remove(device);
            Console.WriteLine($"Устройство не найдено {device.ToString()}");
            ConfigManager.WriteDevicesToConfig(WhatsappSender.Devices);
            return;
        }
        var phone = ExcelReader.Phones.First();
        ExcelReader.Phones.Remove(phone);
        var message = GetMessage(phone);
        var status = sender.SendToPhone(client, phone, message, reader);
        Console.Write($"\r{device.ToString()} {phone.Phone}: {status}\r\n");
        int sleep = delay.GetDelay();
        Console.Write("Программы и консультации по рассылкам в WhatsApp admin1.ru / +79219114848\n");
        for (var i = sleep / 1000; i > 0; i--)
        {
            Console.Write($"Пауза {i} секунд       \r");
            Thread.Sleep(1000);
        }
    }
}
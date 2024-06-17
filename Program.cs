using System.Net.Sockets;
using System.Text;
using OfficeOpenXml;

namespace whatsapp_sender;

internal static class Program
{
    private static bool keepRunning = true;

    private static Thread GetThread(DeviceItem device, ExcelReader excelReader, UserData phone, WhatsappSender sender)
    {
        return new Thread(() => StartSendingWithPhone(device, excelReader, phone, sender));
    }

    private static void Main()
    {
        Console.CancelKeyPress += delegate(object? sender, ConsoleCancelEventArgs e) {
            e.Cancel = true;
            keepRunning = false;
        };

        while (keepRunning)
        {
            try
            {
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelReader excelReader = new("base.xlsx");
                var whatsappSender = new WhatsappSender();
                whatsappSender.InitDevices();
                UserData phone;
                foreach (var client in WhatsappSender.Devices)
                {
                    phone = ExcelReader.Phones.First();
                    ExcelReader.Phones.Remove(phone);
                    var thread = GetThread(client, excelReader, phone, whatsappSender);
                    WhatsappSender.Workers.Add(new Worker(client, thread));
                    thread.Start();
                }

                while (ExcelReader.Phones.Count != 0 && WhatsappSender.Workers.Count != 0)
                {
                    for (var i = 0; i < WhatsappSender.Workers.Count; i++)
                        try
                        {
                            var worker = WhatsappSender.Workers[i];
                            if (!worker.Thread.IsAlive)
                            {
                                WhatsappSender.Workers.Remove(worker);
                                phone = ExcelReader.Phones.First();
                                ExcelReader.Phones.Remove(phone);
                                var thread = GetThread(worker.DeviceItem, excelReader, phone, whatsappSender);
                                WhatsappSender.Workers.Add(new Worker(worker.DeviceItem, thread));
                                thread.Start();
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            //
                        }
                }

                if (WhatsappSender.Workers.Count == 0)
                {
                    Console.WriteLine("Рассылка завершена");
                    Console.WriteLine("Для закрытия консоли нажмите любую клавишу . . .");
                }

                Console.ReadLine();
                break;
            }
            catch (SocketException)
            {
                // do nothing...
            }
        }
        WhatsappSender.KillAdb();
    }

    private static string GetMessage(UserData data)
    {
        MessageReader messageReader = new();
        var message = messageReader.GetRandomRandomizedMessage();
        message = message.Replace("<name>", data.Name).Trim();
        return message;
    }

    private static void StartSendingWithPhone(DeviceItem device, ExcelReader reader, UserData phone,
        WhatsappSender sender)
    {
        var client = sender.GetClient(device);
        if (client is null)
        {
            var find = WhatsappSender.Workers.Find(worker => worker.DeviceItem.Name == device.Name);
            WhatsappSender.Workers.Remove(find);
            WhatsappSender.Devices.Remove(device);
            ExcelReader.Phones.Insert(0, phone);
            Console.WriteLine($"Устройство не найдено {device.ToString()}");
            ConfigManager.WriteDevicesToConfig(WhatsappSender.Devices);
            return;
        }

        var message = GetMessage(phone);
        Console.WriteLine(message);
        Console.Write($"\r{phone.Phone}: inprogress {device.ToString()} \r\n");
        var status = sender.SendToPhone(client, phone, message, reader, device);
        Console.Write($"\r{phone.Phone}: {status} {device.ToString()} \r\n");
        var delay = ConfigManager.ReadConfigFile();
        var sleep = delay.GetDelay();
        Console.Write("Программы и консультации по рассылкам в WhatsApp admin1.ru / +79219114848\n");
        for (var i = sleep / 1000; i > 0; i--)
        {
            Console.Write($"Пауза {i} секунд       \r");
            Thread.Sleep(1000);
        }

        if (ExcelReader.Phones.Count == 0)
        {
            Console.WriteLine("Рассылка завершена");
            Console.WriteLine("Для закрытия консоли нажмите любую клавишу . . .");
        }
    }
}
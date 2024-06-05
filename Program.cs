using System.Text;
using OfficeOpenXml;

namespace whatsapp_sender;

internal static class Program
{
    static void WriteLine(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (i != 0 && i % 8 == 0)
                Console.WriteLine();
            Console.Write(s[i]);
        }
        Console.WriteLine();
    }
    private static void Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        MessageReader messageReader;
        ExcelReader exelReader = new("base.xlsx");
        TimeDelay delay = ConfigManager.ReadConfigFile();
        WhatsappSender deviceManager = new();
        var devices = deviceManager.GetAllDevicesToConfig();
        ConfigManager.WriteDevicesToConfig(devices);
        Console.WriteLine("Получил и записал список всех устройств");
        UserData data;
        while (true)
        {
            try
            {
                data = exelReader.GetPhoneNumber();
            }
            catch (PhoneNotFound)
            {
                break;
            }
            bool isPhone = PhoneNumberChecker.Check(data.Phone);
            if (isPhone && data.Phone.Length >= 11)
            {
                messageReader = new();
                string message = messageReader.GetRandomRandomizedMessage();
                message = message.Replace("<name>", data.Name).Trim();
                Console.WriteLine(message);
                Console.Write(data.Phone);
                var spaces = "";
                for (int i = 0; i < message.Count(); i++)
                {
                    spaces += " ";
                }
                var status = deviceManager.SendToPhone(data, message, exelReader);
                Console.Write($"\r{data.Phone}: {status}\r\n");
                int sleep = delay.GetDelay();
                Console.Write("Программы и консультации по рассылкам в WhatsApp admin1.ru / +79219114848\n");
                for (var i = sleep / 1000; i > 0; i--)
                {
                    Console.Write($"Пауза {i} секунд       \r");
                    Thread.Sleep(1000);
                }
            }
            else
            {
                Console.Write($"\r{data.Phone}: badnumber\r\n");
                exelReader.WriteStatusPhone(data, "badnumber");
            }
        }
        Console.WriteLine("Рассылка завершена");
        Console.Write("Для закрытия консоли нажмите любую клавишу . . .");
        Console.ReadLine();
    }
}
using OfficeOpenXml;

namespace whatsapp_sender;

/// <summary>
///     Стурктура объекта в excel
/// </summary>
/// <param name="name">Имя пользователя</param>
/// <param name="phone">Номер телефона</param>
public struct UserData(string name, string phone)
{
    public string Name = name;
    public string Phone = phone;

    public override string ToString()
    {
        return $"{Phone} {Name}";
    }
}

/// <summary>
///     Класс для работы с Excel файлом
/// </summary>
public class ExcelReader
{
    public static List<UserData> Phones = new();
    private readonly string _fileName;

    /// <summary>
    ///     Класс для работы с Excel файлом
    /// </summary>
    /// <param name="fileName">Имя файла для чтения</param>
    public ExcelReader(string fileName)
    {
        _fileName = fileName;
        try
        {
            Phones = GetPhoneNumber(true);
        }
        catch (IOException e)
        {
            Console.WriteLine("Excel файл использует другой процесс");
            throw;
        }
    }

    private UserData? CheckUser(ExcelWorksheet sheet, int row)
    {
        var phone = sheet.Cells[$"A{row}"].Text;
        var name = sheet.Cells[$"B{row}"].Text;
        var status = sheet.Cells[$"C{row}"].Text;
        if (string.IsNullOrEmpty(status) || status == "inprogress") return new UserData(name, phone);
        return null;
    }

    /// <summary>
    ///     Плучение первого не отправленного номера телефона или же пустой объект, если таких нет
    /// </summary>
    /// <returns>Объект записи в excel</returns>
    public UserData GetPhoneNumber()
    {
        using var package = new ExcelPackage(new FileInfo(_fileName));
        using var file = File.Open(_fileName, FileMode.Open);
        package.Load(file);
        var sheet = package.Workbook.Worksheets[0];
        var start = sheet.Dimension.Start.Row;
        var end = sheet.Dimension.End.Row;
        for (var row = start; row <= end; row++)
        {
            var user = CheckUser(sheet, row);
            if (user is null) continue;

            return (UserData)user;
        }

        throw new PhoneNotFound();
    }

    public List<UserData> GetPhoneNumber(bool getAll)
    {
        using var package = new ExcelPackage(new FileInfo(_fileName));
        using var file = File.Open(_fileName, FileMode.Open);
        package.Load(file);
        var sheet = package.Workbook.Worksheets[0];
        var start = sheet.Dimension.Start.Row;
        var end = sheet.Dimension.End.Row;
        List<UserData> data = new();
        for (var row = start; row <= end; row++)
        {
            var user = CheckUser(sheet, row);
            if (user is null) continue;

            data.Add((UserData)user);
        }

        return data;
    }

    public static string GetStatus(bool isSent)
    {
        return isSent ? "sent" : "notsent";
    }

    /// <summary>
    ///     Запись статуса для пользователя в excel: sent/not sent
    /// </summary>
    /// <param name="data">Пользователь из excel</param>
    /// <param name="status">Отправление или нет</param>
    public string WriteStatusPhone(UserData data, string status)
    {
        using var package = new ExcelPackage(new FileInfo(_fileName));
        using var file = File.Open(_fileName, FileMode.Open);
        package.Load(file);
        file.Close();
        var sheet = package.Workbook.Worksheets[0];
        var start = sheet.Dimension.Start.Row;
        var end = sheet.Dimension.End.Row;
        for (var row = start; row <= end; row++)
        {
            var phone = sheet.Cells[$"A{row}"].Text;
            if (phone == data.Phone) sheet.Cells[$"C{row}"].Value = status;
        }

        sheet.Cells.AutoFitColumns();
        package.SaveAs(new FileInfo(_fileName));
        return status;
    }

    public string WriteStatusPhone(UserData data, string status, DeviceItem device, string message)
    {
        using var package = new ExcelPackage(new FileInfo(_fileName));
        using var file = File.Open(_fileName, FileMode.Open);
        package.Load(file);
        file.Close();
        var sheet = package.Workbook.Worksheets[0];
        var start = sheet.Dimension.Start.Row;
        var end = sheet.Dimension.End.Row;
        for (var row = start; row <= end; row++)
        {
            var phone = sheet.Cells[$"A{row}"].Text;
            if (phone == data.Phone)
            {
                sheet.Cells[$"C{row}"].Value = status;
                sheet.Cells[$"D{row}"].Value = device.Model;
                sheet.Cells[$"E{row}"].Value = device.Name;
                sheet.Cells[$"F{row}"].Value = message;
            }
        }
        sheet.Cells.AutoFitColumns();
        package.SaveAs(new FileInfo(_fileName));
        return status;
    }
}

internal class PhoneNotFound : Exception
{
}
using System.Text;
using Spire.Xls;

namespace whatsapp_sender;

public readonly struct UserData(string name, string phone)
{
    public readonly string Name = name;
    public readonly string Phone = phone;

    public override string ToString()
    {
        return $"{Phone} {Name}";
    }
}

public class ExelReader
{
    private readonly string _fileName;

    public ExelReader(string fileName)
    {
        _fileName = fileName;
    }

    public UserData GetPhoneNumber()
    {
        Workbook wb = new Workbook();
        wb.LoadFromFile(_fileName);
        var sheet = wb.Worksheets.FirstOrDefault();
        var locatedRange = sheet?.AllocatedRange;

        if (locatedRange == null) return new UserData("", "");
        for (var i = 0; i < locatedRange.Rows.Length; i++)
        {
            var phone = locatedRange[i, 0].ToString();
            var name = locatedRange[i, 1].ToString();
            string? status;
            try
            {
                status = locatedRange[i, 2].ToString();
            }
            catch (IndexOutOfRangeException)
            {
                status = null;
            }

            if (string.IsNullOrEmpty(status) || name == null || phone == null)
            {
                continue;
            }

            return new UserData(name, phone);
        }
        return new UserData("", "");
    }

    // public void WriteStatusPhone(UserData data)
    // {
    //     using var stream = File.Open(_fileName, FileMode.Open, FileAccess.ReadWrite);
    //     var reader = ExcelReaderFactory.CreateReader(stream);
    //     var result = reader.AsDataSet();
    //     var table = result.Tables[0];
    //     table.Rows[1][0] = "Hello";
    //     var xml = result.GetXml();
    //     Console.Write(xml);
    //     var sw = new StreamWriter(stream);
    //     sw.Write(xml);
    // }
}
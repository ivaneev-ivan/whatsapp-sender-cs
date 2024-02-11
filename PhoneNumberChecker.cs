using System.Text.RegularExpressions;

namespace whatsapp_sender;
static class PhoneNumberChecker
{
    public static bool Check(string phone)
    {
        Regex regex = new("^((\\+7|7|8)+([0-9]){7,10})$");
        return regex.IsMatch(phone);
    }
}

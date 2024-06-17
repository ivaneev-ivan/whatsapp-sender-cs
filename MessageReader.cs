using System.Text.RegularExpressions;

namespace whatsapp_sender;

public readonly struct MessageItem(int start, int stop, string message)
{
    public readonly int Start = start;
    public readonly int Stop = stop;
    public readonly string Message = message;

    public static int TimeToMinutes(DateTime time)
    {
        return time.Hour * 60 + time.Minute;
    }

    public bool CheckTime(DateTime time)
    {
        var timeMinutes = TimeToMinutes(time);
        return Start <= timeMinutes && timeMinutes <= Stop;
    }

    public bool CheckTimeShorter(DateTime time)
    {
        var timeMinutes = TimeToMinutes(time);
        return Start > timeMinutes;
    }

    public override string ToString()
    {
        return $"{GetHumanTime(Start)} {GetHumanTime(Stop)} {Message}";
    }

    public string GetHumanTime(int minutes)
    {
        var h = (minutes / 60).ToString();
        var m = (minutes % 60).ToString();
        if (h.Length == 1) h = "0" + h;
        if (m.Length == 1) m = "0" + m;
        return $"{h}:{m}";
    }
}

public class MessageReader
{
    private readonly Random _random = new();
    private readonly Regex _regex = new(@"(\d\d:\d\d-\d\d:\d\d) (.+)");
    public List<MessageItem> MessageItems = new();

    public MessageReader(string filename = "message.txt")
    {
        using var reader = new StreamReader(filename);
        var text = reader.ReadToEnd();
        ParseMessageFile(text);
    }

    private static int ParseTimeString(string timeString)
    {
        var s = timeString.Split(':');
        var hour = Convert.ToInt32(s[0]);
        var minutes = Convert.ToInt32(s[1]);
        return hour * 60 + minutes;
    }

    private void ParseMessageFile(string text)
    {
        var split = text.Split("\n").Where(s => s.Length > 0);
        foreach (var s in split)
        {
            var match = _regex.Match(s);
            if (match.Groups.Count != 3) continue;
            var times = match.Groups[1].ToString().Split("-").Select(ParseTimeString).ToArray();
            MessageItems.Add(new MessageItem(times[0], times[1], match.Groups[2].ToString()));
        }

        MessageItems = MessageItems.OrderBy(s => s.Start).ToList();
    }

    public List<MessageItem>? GetTimeNowItems()
    {
        var now = DateTime.Now;
        var items = MessageItems.Where(s => s.CheckTime(now)).ToList();
        if (items.Count == 0) items = null;
        return items;
    }

    public MessageItem GetTimeShorterItem()
    {
        var now = DateTime.Now;
        List<MessageItem> items = new();
        foreach (var item in MessageItems)
            if (item.CheckTimeShorter(now))
                items.Add(item);
        if (items.Count == 0)
            return MessageItems[0];
        return items[_random.Next(items.Count)];
    }

    private string RandomizeMessage(List<MessageItem> messages)
    {
        var message = TextRandomize.HandleText(messages[_random.Next(messages.Count)].Message);
        if (message[message.Length - 1] == '>') message = message.Substring(0, message.Length - 1);
        return message;
    }

    private string RandomizeMessage(MessageItem messages)
    {
        var message = TextRandomize.HandleText(messages.Message);
        if (message[message.Length - 1] == '>') message = message.Substring(0, message.Length - 1);
        return message;
    }

    public void PrintMessages()
    {
        foreach (var item in MessageItems) Console.WriteLine(item);
    }

    public string GetRandomRandomizedMessage()
    {
        var messages = GetTimeNowItems();
        if (messages != null) return RandomizeMessage(messages);
        Console.WriteLine("Для данного временного диапазона подходящих сообщений нет");
        PrintMessages();
        var message = GetTimeShorterItem();
        DateTime now;
        do
        {
            now = DateTime.Now;
            for (var i = 60; i > 0; i--)
            {
                Console.Write($"Пауза {i} секунд       \r");
                Thread.Sleep(1000);
            }
        } while (!message.CheckTime(now));

        return RandomizeMessage(message);
    }

    public List<string> GetAllRandomizedMessages()
    {
        var randomized = new List<string>();
        foreach (var item in MessageItems) randomized.Add(TextRandomize.HandleText(item.Message));
        return randomized;
    }
}
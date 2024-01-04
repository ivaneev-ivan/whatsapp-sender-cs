using System.Text.RegularExpressions;

namespace whatsapp_sender;

public enum CommandType
{
    Enter = 0,
    NewMessage = 1,
    None = 2
}

public struct TimeDelay(int start, int stop)
{
    private readonly Random _random = new();
    public int Start = start;
    public int Stop = stop;

    public readonly int GetDelay()
    {
        return _random.Next(Start, Stop + 1);
    }

    public override string ToString()
    {
        return $"{Start}-{Stop}";
    }
}

public struct TextPartition(int start, int stop, string message)
{
    public TimeDelay Delay = new(start, stop);
    public string Message = message;

    public static List<TextPartition> GetTextPartition(string message)
    {
        Regex regex = new(@"<(\d+), ?(\d+)>");
        List<TextPartition> parts = new();
        var matches = regex.Matches(message);
        foreach (Match match in matches)
        {
            var matchString = match.ToString();
            var splitMessage = message.Split(matchString).ToList();
            var countSplit = Regex.Matches(message, matchString).Count;
            var mess = splitMessage[0];
            var start = Convert.ToInt32(match.Groups[1].ToString());
            var stop = Convert.ToInt32(match.Groups[2].ToString());
            message = GetNewMessage(splitMessage, matchString, countSplit);
            parts.Add(new TextPartition(start, stop, mess));
        }

        parts.Add(new TextPartition(0, 0, message));
        return parts;
    }

    public static string GetNewMessage(List<string> splitMessage, string matchString, int countSplit)
    {
        var message = splitMessage[1];
        if (countSplit > 1)
        {
            var i = 2;
            List<string> a = new();
            while (i < splitMessage.Count())
            {
                a.Add(splitMessage[i]);
                i++;
            }

            message += matchString + string.Join(matchString, a);
        }

        return message;
    }

    public override string ToString()
    {
        return $"{Delay.ToString()} {Message}";
    }
}

public struct TextPartitionWithCommand(string message, CommandType commandType)
{
    public string Message = message;
    public CommandType CommandType = commandType;

    public static List<TextPartitionWithCommand> GetTextPartitionWithCommand(TextPartition textPartition)
    {
        Regex regex = new(@"<[a-z]*\. ?[a-z]*>");
        var message = textPartition.Message;
        List<TextPartitionWithCommand> parts = new();
        var matches = regex.Matches(message);
        foreach (Match match in matches)
        {
            CommandType commandType;
            var matchString = match.ToString();
            if (matchString == "<send. button>")
                commandType = CommandType.NewMessage;
            else if (matchString == "<shift. enter>")
                commandType = CommandType.Enter;
            else
                commandType = CommandType.None;
            var splitMessage = message.Split(matchString).ToList();
            var countSplit = Regex.Matches(message, matchString).Count;
            var mess = splitMessage[0];
            message = TextPartition.GetNewMessage(splitMessage, matchString, countSplit);
            parts.Add(new TextPartitionWithCommand(mess, commandType));
        }

        parts.Add(new TextPartitionWithCommand(message, CommandType.None));
        return parts;
    }

    public override string ToString()
    {
        return $"{CommandType} {Message}";
    }
}
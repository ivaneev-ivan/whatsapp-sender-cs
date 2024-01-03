using System.Text;
using System.Text.RegularExpressions;

namespace whatsapp_sender;

internal class TextRandomize
{
    public List<object> Content = [];

    public override string ToString()
    {
        return string.Concat(Content.Select(item => item.ToString())) + "\n";
    }

    public string GetPureText()
    {
        return string.Concat(Content.OfType<string>());
    }

    public void PrintTab(int tab)
    {
        Console.WriteLine(new string('\t', tab) + GetPureText());
        foreach (var obj in Content)
            if (obj is TextRandomize)
                ((TextRandomize)obj).PrintTab(tab + 1);
    }

    public string MakeDecision()
    {
        List<int> highSigns = [];
        for (var i = 0; i < Content.Count; i++)
            if (Content[i] is TextRandomize)
            {
                if (i >= 2 && Content[i - 1] as string == "!" && Content[i - 2] as string == "!")
                {
                    highSigns.Add(i - 1);
                    highSigns.Add(i - 2);
                    var answerString = ((TextRandomize)Content[i]).MakeDecision().ToCharArray();
                    try
                    {
                        answerString[0] = char.ToUpper(answerString[0]);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    Content[i] = new string(answerString);
                }
                else
                {
                    Content[i] = ((TextRandomize)Content[i]).MakeDecision();
                }
            }
            else
            {
                Content[i] = Content[i].ToString();
            }

        foreach (var highSign in highSigns.OrderByDescending(x => x)) Content.RemoveAt(highSign);

        var text = string.Concat(Content.OfType<string>());
        if (text[0] == '{')
        {
            var words = text.Substring(1, text.Length - 2).Split('|');
            return words[new Random().Next(words.Length)];
        }

        if (text[0] == '[')
        {
            var basis = text.Substring(1, text.Length - 2).Split('+');
            var separator = basis[1];
            var content = basis[2].Trim().Split('|');
            content = content.OrderBy(x => Guid.NewGuid()).ToArray();
            return string.Join(separator, content);
        }

        return "";
    }

    public static string HandleText(string text)
    {
        List<object> startingNodes = [];
        List<TextRandomize> nodeStack = [];

        foreach (var letter in text)
            switch (letter)
            {
                case '{':
                case '[':
                {
                    if (nodeStack.Count == 0)
                    {
                        var currentRandomize = new TextRandomize();
                        currentRandomize.Content.Add(letter);
                        startingNodes.Add(currentRandomize);
                        nodeStack.Add(currentRandomize);
                    }
                    else
                    {
                        var currentRandomize = nodeStack.Last();
                        currentRandomize.Content.Add(new TextRandomize());
                        currentRandomize = (TextRandomize)currentRandomize.Content.Last();
                        nodeStack.Add(currentRandomize);
                        currentRandomize.Content.Add(letter);
                    }

                    break;
                }
                case '}':
                case ']':
                {
                    if (nodeStack.Count == 0) throw new FormatException();

                    var currentRandomize = nodeStack.Last();
                    currentRandomize.Content.Add(letter);
                    nodeStack.RemoveAt(nodeStack.Count - 1);
                    break;
                }
                default:
                {
                    if (nodeStack.Count != 0)
                    {
                        var currentRandomize = nodeStack.Last();
                        currentRandomize.Content.Add(letter);
                    }
                    else
                    {
                        startingNodes.Add(letter.ToString());
                    }

                    break;
                }
            }

        var result = new StringBuilder();
        foreach (var node in startingNodes)
            switch (node)
            {
                case string s:
                    result.Append(s);
                    break;
                case TextRandomize randomize:
                {
                    if (startingNodes.IndexOf(randomize) < 2)
                    {
                        result.Append(randomize.MakeDecision());
                    }
                    else if (startingNodes[startingNodes.IndexOf(randomize) - 1] as string == "!" &&
                             startingNodes[startingNodes.IndexOf(randomize) - 2] as string == "!")
                    {
                        result.Remove(result.Length - 2, 2);
                        var answerString = randomize.MakeDecision().ToCharArray();
                        try
                        {
                            answerString[0] = char.ToUpper(answerString[0]);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        result.Append(new string(answerString));
                    }
                    else
                    {
                        result.Append(randomize.MakeDecision());
                    }

                    break;
                }
            }

        var i = 1;
        while (i < result.Length - 1)
        {
            var letter = result[i];
            if (new List<char> { '.', ',', ':', ';' }.Contains(letter))
            {
                if (result[i - 1] == ' ')
                {
                    result.Remove(i - 1, 1);
                    i = i - 1;
                    continue;
                }

                if (result[i + 1] != ' ')
                {
                    result.Insert(i + 1, " ");
                    i += 1;
                }
            }

            if (letter == '—')
            {
                if (result[i - 1] != ' ')
                {
                    result.Insert(0, result[i - 1]);
                    result.Remove(i - 1, 1);
                    i += 1;
                    continue;
                }

                if (result[i + 1] != ' ' || result[i + 1] != ',')
                {
                    result.Insert(i + 1, " ");
                    i += 1;
                }
            }

            i += 1;
        }

        result = new StringBuilder(Regex.Replace(result.ToString(), " +", " "));

        var stringResult = result.ToString();
        if (stringResult.Contains("<enter. enter") || stringResult.Contains("<send. button")) stringResult += ">";

        return stringResult;
    }
}

internal class FormatException : Exception;
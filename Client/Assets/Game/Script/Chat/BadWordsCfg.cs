using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BadWordsCfg
{
    public string rule;

    public static List<Regex> m_badNickNames = new List<Regex>();
    public static List<Regex> m_badChatWords = new List<Regex>();

    public static void Init()
    {
        var nickBadWords = Csv.CsvUtil.Load<BadWordsCfg>("other/nickBadWords");
        var chatBadWords = Csv.CsvUtil.Load<BadWordsCfg>("other/chatBadWords");
        for (var i = 0; i < nickBadWords.Count; ++i)
        {
            m_badNickNames.Add(new Regex(nickBadWords[i].rule, RegexOptions.Singleline));
        }
        for (var i = 0; i < chatBadWords.Count; ++i)
        {
            m_badChatWords.Add(new Regex(chatBadWords[i].rule, RegexOptions.Singleline));
        }
    }

    public static bool HasBadNickNameWords(string str, out string badWords)
    {
        badWords = "";

        if (string.IsNullOrEmpty(str))
            return false;

        for (var i = 0; i < m_badNickNames.Count; ++i)
        {
            var regex = m_badNickNames[i];
            var match = regex.Match(str);
            if (match.Success)
            {
                badWords = match.Value;
                return true;
            }
        }

        return false;
    }

    public static bool HasBadNickNameWords(string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        for (var i = 0; i < m_badNickNames.Count; ++i)
        {
            var regex = m_badNickNames[i];
            if (regex.IsMatch(str))
            {
                return true;
            }
        }

        return false;
    }

    public static string ReplaceBadNickNameWords(string str, char replaceChar = '*')
    {
        if (string.IsNullOrEmpty(str))
            return "";

        for (var i = 0; i < m_badNickNames.Count; ++i)
        {
            var regex = m_badNickNames[i];
            str = regex.Replace(str, (mat) => {return new string(replaceChar, mat.Length);});
        }

        return str;
    }

    public static bool HasBadChatWords(string str, out string badWords)
    {
        badWords = "";

        if (string.IsNullOrEmpty(str))
            return false;

        for (var i = 0; i < m_badChatWords.Count; ++i)
        {
            var regex = m_badChatWords[i];
            var match = regex.Match(str);
            if (match.Success)
            {
                badWords = match.Value;
                return true;
            }
        }

        return false;
    }

    public static bool HasBadChatWords(string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        for (var i = 0; i < m_badChatWords.Count; ++i)
        {
            var regex = m_badChatWords[i];
            if (regex.IsMatch(str))
            {
                return true;
            }
        }

        return false;
    }

    public static string ReplaceBadChatWords(string str, char replaceChar = '*')
    {
        if (string.IsNullOrEmpty(str))
            return "";

        for (var i = 0; i < m_badChatWords.Count; ++i)
        {
            var regex = m_badChatWords[i];
            str = regex.Replace(str, (mat) => { return new string(replaceChar, mat.Length); });
        }

        return str;
    }

    public static bool HasBadWords(string str, out string badWords)
    {
        if (HasBadNickNameWords(str, out badWords))
            return true;

        if (HasBadChatWords(str, out badWords))
            return true;

        return false;
    }

    public static bool HasBadWords(string str)
    {
        if (HasBadNickNameWords(str))
            return true;

        if (HasBadChatWords(str))
            return true;

        return false;
    }

    public static string ReplaceBadWords(string str, char replaceChar = '*')
    {
        str = ReplaceBadNickNameWords(str);
        str = ReplaceBadChatWords(str);
        return str;
    }
}
using System.Text.RegularExpressions;

public static class RegexUtil
{
    static readonly Regex EmailRegex = new Regex(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
    static readonly Regex UrlRegex = new Regex(@"^((https|http|ftp|rtsp|mms)?:\/\/)[^\s]+");
    public static bool MatchEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;
        return EmailRegex.IsMatch(email);
    }
    public static bool MatchUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;
        return UrlRegex.IsMatch(url);
    }
    public static bool IsMatch(string reg, string str)
    {
        return Regex.IsMatch(str, reg);
    }
    public static Match GetMatch(string reg, string str)
    {
        return Regex.Match(str, reg);
    }
    public static MatchCollection GetMatchs(string reg, string str)
    {
        return Regex.Matches(str, reg);
    }
}

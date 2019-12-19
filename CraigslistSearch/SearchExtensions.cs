namespace CraigslistSearch
{
    public static class SearchExtensions
    {
        public static string GrabBetween(this string inp, string first, string second) => inp.IndexOf(first) + first.Length is int start && start >= 0 && inp.Length >= start && inp.Substring(start) is string tmp && !string.IsNullOrEmpty(tmp)
            ? tmp.Substring(0, tmp.IndexOf(second)).Replace("<![CDATA[", string.Empty).Replace("]]>", string.Empty)
            : string.Empty;
        public static bool ContainsIns(this string[] lst, string inp)
        {
            string lower = inp.ToLower();
            foreach (string str in lst)
                if (lower.Contains(str))
                    return true;
            return false;
        }
    }
}

using System.Text.RegularExpressions;

namespace RewardsAndRecognitionSystem.Utilities
{
    public class NormalisingString
    {
        public static string Normalize(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            name = name.ToLower();
            name = Regex.Replace(name, @"\baward\b", "");
            name = Regex.Replace(name, @"[^a-z0-9]", "");
            return name;
        }
    }
}

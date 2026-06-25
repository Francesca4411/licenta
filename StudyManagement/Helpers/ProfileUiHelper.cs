namespace StudyManagement.Helpers
{
    /// <summary>Initials avatar + stable gradient from name (no uploads).</summary>
    public static class ProfileUiHelper
    {
        public static string GetInitials(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";

            var parts = fullName.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var a = char.ToUpperInvariant(parts[0][0]);
                var b = char.ToUpperInvariant(parts[^1][0]);
                return $"{a}{b}";
            }

            var s = parts[0];
            if (s.Length >= 2)
                return s[..2].ToUpperInvariant();
            return char.ToUpperInvariant(s[0]).ToString();
        }

        private static readonly string[] AvatarGradients =
        {
            "linear-gradient(135deg, #7c3aed, #a855f7)",
            "linear-gradient(135deg, #2563eb, #60a5fa)",
            "linear-gradient(135deg, #059669, #34d399)",
            "linear-gradient(135deg, #c026d3, #f472b6)",
            "linear-gradient(135deg, #d97706, #fbbf24)",
            "linear-gradient(135deg, #4f46e5, #818cf8)",
        };

        public static string GetAvatarGradient(string? fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return AvatarGradients[0];

            unchecked
            {
                var hash = 0;
                foreach (var c in fullName)
                    hash = hash * 31 + c;
                var idx = Math.Abs(hash) % AvatarGradients.Length;
                return AvatarGradients[idx];
            }
        }
    }
}

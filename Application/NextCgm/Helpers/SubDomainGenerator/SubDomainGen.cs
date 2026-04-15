namespace NextCgm.Helpers.SubDomainGenerator
{
    using System;
    using System.IO;

    public static class NameGenerator
    {
        // Use 'static' so these are loaded once and shared globally
        public static int AdjectiveCount => _adjectives.Length;

        private static string[] _adjectives = new string[0];
        private static string[] _colors = new string[0];
        private static string[] _nato = new string[0];
        private static readonly Random _rng = new Random();

        // The 'lock' ensures that if two users join at the same time,
        // the files are only loaded once.
        private static readonly object _lock = new object();

        private static bool _isInitialized = false;

        public static void Initialize(string adjectivesPath, string colorsPath, string natoPath)
        {
            lock (_lock)
            {
                if (_isInitialized) return;

                _adjectives = File.ReadAllLines(adjectivesPath);
                _colors = File.ReadAllLines(colorsPath);
                _nato = File.ReadAllLines(natoPath);

                _isInitialized = true;
            }
        }

        public static string GetAdjNatio()
        {
            // Simple 2-word pattern
            return $"{Pick(_adjectives)}-{Pick(_nato)}".ToLower();
        }

        public static string GetAdjColorNato(bool UseAdjColorNatoSuperSlug = false)
        {
            if (UseAdjColorNatoSuperSlug)
            {
                // The 10-million combo pattern: NATO-ADJ-COLOR-NATO
                return $"{Pick(_nato)}-{Pick(_adjectives)}-{Pick(_colors)}-{Pick(_nato)}".ToLower();
            }

            // The standard 3-word pattern: ADJ-COLOR-NATO
            return $"{Pick(_adjectives)}-{Pick(_colors)}-{Pick(_nato)}".ToLower();
        }

        private static string Pick(string[] words)
        {
            if (words == null || words.Length == 0) return "unknown";
            return words[_rng.Next(words.Length)].Trim();
        }
    }
}
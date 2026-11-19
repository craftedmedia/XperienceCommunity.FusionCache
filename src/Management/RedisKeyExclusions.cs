using StackExchange.Redis;

namespace XperienceCommunity.FusionCache.Management;

internal sealed class RedisKeyExclusions
{
    private readonly HashSet<RedisKey> keys;
    private readonly string[] patterns;

    public RedisKeyExclusions(IEnumerable<string> excludedKeys, IEnumerable<string> excludedPatterns)
    {
        keys = ReadRules(excludedKeys).Select(key => (RedisKey)key).ToHashSet();
        patterns = ReadRules(excludedPatterns);
    }

    public bool Matches(RedisKey key)
    {
        if (keys.Contains(key))
        {
            return true;
        }

        string name = key.ToString();
        return patterns.Any(pattern => MatchesPattern(name, pattern));
    }

    private static bool MatchesPattern(string name, string pattern)
    {
        int position = 0;
        int patternPosition = 0;
        int starPosition = -1;
        int retryPosition = 0;
        while (position < name.Length)
        {
            if (patternPosition < pattern.Length && pattern[patternPosition] == '*')
            {
                starPosition = patternPosition++;
                retryPosition = position;
            }
            else if (patternPosition < pattern.Length &&
                (pattern[patternPosition] == '?' || pattern[patternPosition] == name[position]))
            {
                position++;
                patternPosition++;
            }
            else if (starPosition >= 0)
            {
                // Let the most recent '*' consume one more character and retry its suffix.
                patternPosition = starPosition + 1;
                position = ++retryPosition;
            }
            else
            {
                return false;
            }
        }

        while (patternPosition < pattern.Length && pattern[patternPosition] == '*')
        {
            patternPosition++;
        }

        return patternPosition == pattern.Length;
    }

    private static string[] ReadRules(IEnumerable<string> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        var values = rules.Where(rule => !string.IsNullOrEmpty(rule)).Distinct(StringComparer.Ordinal).Take(201).ToArray();
        if (values.Length > 200 || values.Any(rule => rule.Length > 1024))
        {
            throw new ArgumentException("Use at most 200 exclusions per list, with at most 1024 characters per exclusion.");
        }

        return values;
    }
}

using System;

namespace Bit.Core.Utilities
{
    public static class SpanExtensions
    {
        public static bool TrySplitBy(this ReadOnlySpan<char> source, char splitChar, out ReadOnlySpan<char> firstPart, out ReadOnlySpan<char> rest)
        {
            var index = source.IndexOf(splitChar);

            if (index < 1)
            {
                firstPart = default;
                rest = source;
                return false;
            }

            firstPart = source[..index];
            rest = source[++index..];
            return true;
        }
    }
}

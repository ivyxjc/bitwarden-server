using System;
using Bit.Core.Utilities;
using Xunit;

namespace Bit.Core.Test.Utilities
{
    public class SpanExtensionsTests
    {
        [Theory]
        [InlineData("my.string", '.', "my", "string")]
        [InlineData("1$string", '$', "1", "string")]
        public void TrySplitBy_CanSplit_Success(string source, char splitChar, string firstPart, string rest)
        {
            var sourceSpan = source.AsSpan();

            var canSplit = sourceSpan.TrySplitBy(splitChar, out var firstPartResult, out var restResult);
            Assert.True(canSplit);
            Assert.Equal(firstPart, firstPartResult.ToString());
            Assert.Equal(rest, restResult.ToString());
        }

        [Theory]
        [InlineData(".string", '.')]
        [InlineData("1$string", '7')]
        public void TrySplitBy_CanNotSplit_Success(string source, char splitChar)
        {
            var sourceSpan = source.AsSpan();

            var canSplit = sourceSpan.TrySplitBy(splitChar, out var _, out var _);
            Assert.False(canSplit);
        }
    }
}

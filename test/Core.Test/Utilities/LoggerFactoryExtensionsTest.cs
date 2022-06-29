using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bit.Core.Utilities;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

namespace Bit.Core.Test.Utilities
{
    public class LoggerFactoryExtensionsTests
    {
        public static IEnumerable<object[]> InclusionPredicateData()
        {
            // Different EventIds
            yield return new object [] { null, (LogEvent _) => false, false }; // No EventId property
            yield return new object [] { 0, (LogEvent _) => false, false }; // Any non-special EventId property
            yield return new object [] { Constants.BypassFiltersEventId, (LogEvent _) => false, true }; // Special EventId property

            // Alternate filter values
            yield return new object [] { null, (LogEvent _) => true, true }; // Filter that returns true
            yield return new object [] { Constants.BypassFiltersEventId, null, true }; // No filter
        }

        [Theory]
        [MemberData(nameof(InclusionPredicateData))]
        public void InclusionPredicate_Success(int? eventId, Func<LogEvent, bool> extraFilter, bool shouldLog)
        {
            var logEventProperties = eventId.HasValue 
                ? new List<LogEventProperty> { new LogEventProperty("EventId", new TestLogEventPropertyValue(eventId.Value)) }
                : new List<LogEventProperty>();

            var logEvent = new LogEvent(DateTimeOffset.UtcNow,
                LogEventLevel.Error, 
                exception: null,
                new MessageTemplate("Template", Enumerable.Empty<MessageTemplateToken>()),
                logEventProperties);

            var actualShouldLog = LoggerFactoryExtensions.InclusionPredicate(logEvent, extraFilter);
            Assert.Equal(shouldLog, actualShouldLog);
        }
    }

    public class TestLogEventPropertyValue : LogEventPropertyValue
    {
        private readonly object _value;

        public TestLogEventPropertyValue(object value)
        {
            _value = value;
        }

        public override void Render(TextWriter output, string format = null, IFormatProvider formatProvider = null)
            => throw new NotImplementedException();

        public override string ToString() => _value.ToString();
    }
}

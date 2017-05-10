using System;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Formatters
{
    public class ConverterTest
    {
        [Fact]
        public void FormattingTypes()
        {
            foreach (FormattingType type in Enum.GetValues(typeof(FormattingType)))
            {
                // Act
                var formatter = Converter.ToFormatter(type);

                // Assert
                formatter.ShouldNotBeNull();
            }
        }
    }
}
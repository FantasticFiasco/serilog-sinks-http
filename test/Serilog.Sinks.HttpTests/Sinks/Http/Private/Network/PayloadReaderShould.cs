//using System.IO;
//using Xunit;

//namespace Serilog.Sinks.Http.Private.Network
//{
//    public class PayloadReaderShould
//    {
//        private long nextLineBeginsAtOffset;
//        private int count;

//        [Fact]
//        public void ReadLogEvent()
//        {
//            // Arrange
//            var stream = new MemoryStream("{ \"foo\": 1 }");

//            // Act
//            var actual = PayloadReader.Read(stream, ref nextLineBeginsAtOffset, ref count, null, int.MaxValue);

//            // Assert
//            //actual.
//            count.ShouldBe()
//        }

//        [Fact]
//        public void ReadLogEvents()
//        {
            
//        }

//        [Fact]
//        public void NotReadEventGivenPartiallyWritten()
//        {
            
//        }

//        [Fact]
//        public void NotReadEventsGivenPartiallyWritten()
//        {
            
//        }
//    }
//}

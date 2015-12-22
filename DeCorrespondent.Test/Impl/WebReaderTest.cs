using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class WebReaderTest
    {
        [Test]
        public void Read()
        {
            var reader = CreateReader();

            var result = reader.Read("http://www.nu.nl");

            //Console.WriteLine(result);
            Assert.NotNull(result);
        }

        private static WebReader CreateReader()
        {
            return new WebReader(new ConsoleLogger(true));
        }
    }
}

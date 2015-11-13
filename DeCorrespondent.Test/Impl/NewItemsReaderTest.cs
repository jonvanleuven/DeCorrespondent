using System.Linq;
using NUnit.Framework;
using DeCorrespondent.Impl;
using DeCorrespondent.Test.Util;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class NewItemsReaderTest
    {
        [Test]
        public void ReadItems()
        {
            var reader = CreateReader();

            var result = reader.ReadItems(new FileResources().ReadNewItems(0));

            Assert.AreEqual(10, result.Count());
            Assert.AreEqual(3352, result.First().Id);
        }

        private static NewItemsReader CreateReader()
        {
            return new NewItemsReader(new ConsoleLogger(true));
        }
    }
}

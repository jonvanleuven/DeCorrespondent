using System;
using System.Threading;
using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class MemoryLoggerTest
    {
        [Test]
        public void Formatting()
        {
            var logger = new MemoryLogger();

            logger.Debug("regel1");
            Thread.Sleep(10);
            logger.Info("regel2");
            Thread.Sleep(10);
            logger.Error(CreateException());

            Assert.AreEqual(3, logger.Lines.Count);
            Console.WriteLine(string.Join("\n", logger.Lines));
        }

        [Test]
        public void Arguments()
        {
            var logger = new MemoryLogger();

            logger.Debug("{0} {1}", "1", 2);

            Assert.AreEqual(1, logger.Lines.Count);
            Assert.IsTrue(logger.Lines[0].EndsWith("1 2") );
        }

        private static Exception CreateException()
        {
            try
            {
                throw new Exception("exception");
            }
            catch (Exception e)
            {
                return e;
            }
            throw new Exception("Hier mag ie niet komen!");
        }
    }
}

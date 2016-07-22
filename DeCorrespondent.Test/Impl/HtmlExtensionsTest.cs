using System;
using DeCorrespondent.Impl;
using NUnit.Framework;

namespace DeCorrespondent.Test.Impl
{
    [TestFixture]
    public class HtmlExtensionsTest
    {
        [Test]
        public void Escape()
        {
            Assert.AreEqual("&lt;", "<".EscapeHtml());
        }

        [Test]
        public void UnEscape()
        {
            Assert.AreEqual("<", "&lt;".UnescapeHtml());
        }

        [Test]
        public void DoNotCrash()
        {
            var input = "​"; //welk character is dit?

            var r = input.EscapeHtml();

            //assert: do not crash
            Console.WriteLine(r);
        }

        [Test]
        public void DoNotCrash2()
        {
            var input = "ğ"; //welk character is dit?

            var r = input.EscapeHtml();

            //assert: do not crash
            Console.WriteLine(r);
        }
    }
}

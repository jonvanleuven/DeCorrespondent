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
            var input = "​"; //welk character is dit?

            var r = input.EscapeHtml();

            //assert: do not crash
        }
    }
}

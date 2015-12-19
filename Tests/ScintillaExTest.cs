using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ScintillaNET_Kitchen;

namespace ScintillaNET_KitchenTest
{
    [TestClass]
    public class ScintillaExTest
    {
        [TestMethod]
        public void KeywordsTest()
        {
            var sci = new ScintillaEx();

            sci.SetKeywords(1, "a b c");
            sci.SetKeywords(2, "d e f");
            sci.SetKeywords(1, "g h i");

            Assert.AreEqual(sci.GetKeywords(1), "g h i");
            Assert.AreEqual(sci.GetKeywords(2), "d e f");
        }
    }
}

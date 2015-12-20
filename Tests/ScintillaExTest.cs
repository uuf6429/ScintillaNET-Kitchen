using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScintillaNET_Kitchen;

namespace ScintillaNET_KitchenTest
{
    [TestClass]
    public class ScintillaExTest
    {
        class ScintillaExMock : ScintillaEx
        {
            public int UpdateLineMarginCallCount = 0;

            protected override void DoUpdateLineMargin(int chars)
            {
                base.DoUpdateLineMargin(chars);
                this.UpdateLineMarginCallCount++;
            }
        }

        [TestMethod]
        public void TestKeywordSetChanges()
        {
            var sci = new ScintillaEx();

            sci.SetKeywords(1, "a b c");
            sci.SetKeywords(2, "d e f");
            sci.SetKeywords(1, "g h i");

            Assert.AreEqual("g h i", sci.GetKeywords(1));
            Assert.AreEqual("d e f", sci.GetKeywords(2));
        }

        [TestMethod]
        public void TestLineMarginChangeBehaviour()
        {
            var sci = new ScintillaExMock() { ShowLineMargin = true };

            // at this point, called by constructor
            Assert.AreEqual(1, sci.UpdateLineMarginCallCount);

            sci.Text = "".PadLeft(999, 'N').Replace("N", Environment.NewLine) + "// last line";

            // ensure we have 1000 lines
            Assert.AreEqual(1000, sci.Lines.Count);

            // called by text change
            Assert.AreEqual(2, sci.UpdateLineMarginCallCount);

            // ensure margin width is as expected
            Assert.AreEqual(37, sci.Margins[0].Width);
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;
using ScintillaNET_Kitchen;

namespace ScintillaNET_KitchenTest
{
    [TestClass]
    public class ToolStripMenuRadioItemTest
    {
        private ToolStripMenuRadioItem Item_A1 = new ToolStripMenuRadioItem() { Text = "Group A Item 1", Checked = true};
        private ToolStripMenuRadioItem Item_A2 = new ToolStripMenuRadioItem("Group A Item 2", null);
        private ToolStripMenuRadioItem Item_A3 = new ToolStripMenuRadioItem("Group A Item 3", null, new ToolStripItem[0]);
        private ToolStripSeparator Item_S1 = new ToolStripSeparator();
        private ToolStripMenuRadioItem Item_B1 = new ToolStripMenuRadioItem("Group B Item 1");
        private ToolStripMenuRadioItem Item_B2 = new ToolStripMenuRadioItem("Group B Item 2") { Checked = true};
        private ToolStripMenuItem Item_S2 = new ToolStripMenuItem("-");
        private ToolStripMenuRadioItem Item_C1 = new ToolStripMenuRadioItem("Group C Item 1");

        [TestMethod]
        public void TestRadioButtonBehaviour()
        {
            var menu = new ToolStripDropDownMenu();

            menu.Items.AddRange(new ToolStripItem[]
            {
                Item_A1, Item_A2, Item_A3, Item_S1, Item_B1, Item_B2
            });

            Item_A2.PerformClick();
            Item_A3.PerformClick();

            Assert.IsFalse(Item_A1.Checked);
            Assert.IsFalse(Item_A2.Checked);
            Assert.IsTrue(Item_A3.Checked);
            Assert.IsFalse(Item_B1.Checked);
            Assert.IsTrue(Item_B2.Checked);
            Assert.IsFalse(Item_C1.Checked);

            Item_B1.PerformClick();
            Assert.IsFalse(Item_A1.Checked);
            Assert.IsFalse(Item_A2.Checked);
            Assert.IsTrue(Item_A3.Checked);
            Assert.IsTrue(Item_B1.Checked);
            Assert.IsFalse(Item_B2.Checked);
            Assert.IsFalse(Item_C1.Checked);
        }
    }
}

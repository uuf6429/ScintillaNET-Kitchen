using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScintillaNET_Kitchen.Forms;
using System.Collections.Generic;
using System.Drawing;

namespace ScintillaNET_KitchenTest.Forms
{
    [TestClass]
    public class MainFormTest
    {
        class MainFormMock : MainForm
        {
            public new string SerializeValue(object value)
            {
                return base.SerializeValue(value);
            }
        }

        [TestMethod]
        public void TestSerializerScenarios()
        {
            var mf = new MainFormMock();

            (new List<Tuple<String, Object, String>>()
            {
                // test name, source data, expected result
                { "null", null, "null" },
                { "null string", (String)null, "null" },
                { "empty string", "", "\"\"" },
                { "string with special chars", "\"\r\n", "\"\\\"\\r\\n\"" },
                { "boolean true", true, "true"},
                { "25 (int)", 25, "25" },
                { "0 (int)", 0, "0" },
                { "123.45 (dec)", 123.45M, "123.45m" },
                { "0 (dec)", 0.0M, "0.0m" },
                { "red", Color.Red, "Color.Red" },
                { "scintilla case style", ScintillaNET.StyleCase.Camel, "ScintillaNET.StyleCase.Camel" },
            }).ForEach(m =>
            {
                Assert.AreEqual(m.Item3, mf.SerializeValue(m.Item2), m.Item1);
            });
        }
    }

    static class ExtensionMethods
    {
        public static void Add(this List<Tuple<String, Object, String>> list, string testName, object testData, string expectedResult)
        {
            list.Add(new Tuple<string, object, string>(testName, testData, expectedResult));
        }
    }
}

/*
 * [The "BSD licence"]
 * Copyright (c) 2011 Terence Parr
 * All rights reserved.
 *
 * Conversion to C#:
 * Copyright (c) 2011 Sam Harwell, Tunnel Vision Laboratories, LLC
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace Antlr4.Test.StringTemplate
{
    using Antlr4.StringTemplate;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using StringWriter = System.IO.StringWriter;

    [TestClass]
    public class TestNullAndEmptyValues : BaseTest
    {
        [TestMethod]
        public void TestSeparatorWithNullFirstValue()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test", "hi <name; separator=\", \">!", new string[] { "name" });
            Template st = group.GetInstanceOf("test");
            st.Add("name", null); // null is added to list, but ignored in iteration
            st.Add("name", "Tom");
            st.Add("name", "Sumana");
            string expected = "hi Tom, Sumana!";
            string result = st.Render();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestTemplateAppliedToNullIsEmpty()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test", "<name:t()>", new string[] { "name" });
            group.DefineTemplate("t", "<x>", new string[] { "x" });
            Template st = group.GetInstanceOf("test");
            st.Add("name", null); // null is added to list, but ignored in iteration
            string expected = "";
            string result = st.Render();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestTemplateAppliedToMissingValueIsEmpty()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test", "<name:t()>", new string[] { "name" });
            group.DefineTemplate("t", "<x>", new string[] { "x" });
            Template st = group.GetInstanceOf("test");
            string expected = "";
            string result = st.Render();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestSeparatorWithNull2ndValue()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test", "hi <name; separator=\", \">!", new string[] { "name" });
            Template st = group.GetInstanceOf("test");
            st.Add("name", "Ter");
            st.Add("name", null);
            st.Add("name", "Sumana");
            string expected = "hi Ter, Sumana!";
            string result = st.Render();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestSeparatorWithNullLastValue()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test", "hi <name; separator=\", \">!", new string[] { "name" });
            Template st = group.GetInstanceOf("test");
            st.Add("name", "Ter");
            st.Add("name", "Tom");
            st.Add("name", null);
            string expected = "hi Ter, Tom!";
            string result = st.Render();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestSeparatorWithTwoNullValuesInRow()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test", "hi <name; separator=\", \">!", new string[] { "name" });
            Template st = group.GetInstanceOf("test");
            st.Add("name", "Ter");
            st.Add("name", "Tom");
            st.Add("name", null);
            st.Add("name", null);
            st.Add("name", "Sri");
            string expected = "hi Ter, Tom, Sri!";
            string result = st.Render();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestTwoNullValues()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test", "hi <name; null=\"x\">!", new string[] { "name" });
            Template st = group.GetInstanceOf("test");
            st.Add("name", null);
            st.Add("name", null);
            string expected = "hi xx!";
            string result = st.Render();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestNullListItemNotCountedForIteratorIndex()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test", "<name:{n | <i>:<n>}>", new string[] { "name" });
            Template st = group.GetInstanceOf("test");
            st.Add("name", "Ter");
            st.Add("name", null);
            st.Add("name", null);
            st.Add("name", "Jesse");
            string expected = "1:Ter2:Jesse";
            string result = st.Render();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestSizeZeroButNonNullListGetsNoOutput()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test",
                "begin\n" +
                "<users>\n" +
                "end\n", new string[] { "users" });
            Template t = group.GetInstanceOf("test");
            t.Add("users", null);
            string expecting = "begin" + newline + "end";
            string result = t.Render();
            Assert.AreEqual(expecting, result);
        }

        [TestMethod]
        public void TestNullListGetsNoOutput()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test",
                "begin\n" +
                "<users:{u | name: <u>}; separator=\", \">\n" +
                "end\n", new string[] { "users" });
            Template t = group.GetInstanceOf("test");
            string expecting = "begin" + newline + "end";
            string result = t.Render();
            Assert.AreEqual(expecting, result);
        }

        [TestMethod]
        public void TestEmptyListGetsNoOutput()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test",
                "begin\n" +
                "<users:{u | name: <u>}; separator=\", \">\n" +
                "end\n", new string[] { "users" });
            Template t = group.GetInstanceOf("test");
            t.Add("users", new List<string>());
            string expecting = "begin" + newline + "end";
            string result = t.Render();
            Assert.AreEqual(expecting, result);
        }

        [TestMethod]
        public void TestMissingDictionaryValue()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test", "<m.foo>", new string[] { "m" });
            Template t = group.GetInstanceOf("test");
            t.Add("m", new Dictionary<string, string>());
            string expecting = "";
            string result = t.Render();
            Assert.AreEqual(expecting, result);
        }

        [TestMethod]
        public void TestMissingDictionaryValue2()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test", "<if(m.foo)>[<m.foo>]<endif>", new string[] { "m" });
            Template t = group.GetInstanceOf("test");
            t.Add("m", new Dictionary<string, string>());
            string expecting = "";
            string result = t.Render();
            Assert.AreEqual(expecting, result);
        }

        [TestMethod]
        public void TestMissingDictionaryValue3()
        {
            TemplateGroup group = new TemplateGroup();
            group.DefineTemplate("test", "<if(m.foo)>[<m.foo>]<endif>", new string[] { "m" });
            Template t = group.GetInstanceOf("test");
            t.Add("m", new Dictionary<string, string>() { { "foo", null } });
            string expecting = "";
            string result = t.Render();
            Assert.AreEqual(expecting, result);
        }

        [TestMethod]
        public void TestSeparatorEmittedForEmptyIteratorValue()
        {
            Template st = new Template(
                "<values:{v|<if(v)>x<endif>}; separator=\" \">"
            );
            st.Add("values", new bool[] { true, false, true });
            StringWriter sw = new StringWriter();
            st.Write(new AutoIndentWriter(sw));
            string result = sw.ToString();
            string expecting = "x  x";
            Assert.AreEqual(expecting, result);
        }

        [TestMethod]
        public void TestSeparatorEmittedForEmptyIteratorValue2()
        {
            Template st = new Template(
                "<values; separator=\" \">"
            );
            st.Add("values", new string[] { "x", string.Empty, "y" });
            StringWriter sw = new StringWriter();
            st.Write(new AutoIndentWriter(sw));
            string result = sw.ToString();
            string expecting = "x  y";
            Assert.AreEqual(expecting, result);
        }
    }
}

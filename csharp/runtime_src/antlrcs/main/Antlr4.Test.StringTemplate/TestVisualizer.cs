﻿/*
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
    using System;
    using Antlr4.StringTemplate;
    using Antlr4.StringTemplate.Debug;
    using Antlr4.StringTemplate.Misc;
    using Antlr4.StringTemplate.Visualizer;
    using Antlr4.StringTemplate.Visualizer.Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using CultureInfo = System.Globalization.CultureInfo;
    using Path = System.IO.Path;
    using StringWriter = System.IO.StringWriter;

    [TestClass]
    public class TestVisualizer : BaseTest
    {
        [TestMethod]
        public void SimpleVisualizerTest()
        {
            string templates =
                "method(type,name,locals,args,stats) ::= <<\n" +
                "public <type> <ick()> <name>(<args:{a| int <a>}; separator=\", \">) {\n" +
                "    <if(locals)>int locals[<locals>];<endif>\n" +
                "    <stats;separator=\"\\n\">\n" +
                "}\n" +
                ">>\n" +
                "assign(a,b) ::= \"<a> = <b>;\"\n" +
                "return(x) ::= <<return <x>;>>\n" +
                "paren(x) ::= \"(<x>)\"\n";

            writeFile(tmpdir, "t.stg", templates);
            TemplateGroup group = new TemplateGroupFile(Path.Combine(tmpdir, "t.stg"));
            group.TrackCreationEvents = true;
            Template st = group.GetInstanceOf("method");
            st.impl.Dump();
            st.Add("type", "float");
            st.Add("name", "foo");
            st.Add("locals", 3);
            st.Add("args", new String[] { "x", "y", "z" });
            Template s1 = group.GetInstanceOf("assign");
            Template paren = group.GetInstanceOf("paren");
            paren.Add("x", "x");
            s1.Add("a", paren);
            s1.Add("b", "y");
            Template s2 = group.GetInstanceOf("assign");
            s2.Add("a", "y");
            s2.Add("b", "z");
            Template s3 = group.GetInstanceOf("return");
            s3.Add("x", "3.14159");
            st.Add("stats", s1);
            st.Add("stats", s2);
            st.Add("stats", s3);

            st.Visualize();
        }

        [TestMethod]
        public void VisualizerTestShadowTemplates()
        {
            string templates =
                "list(lines) ::= <<\n" +
                "<lines:line(); separator=\"\\n\">\n" +
                ">>\n" +
                "line(text) ::= \"<text>\"\n";

            writeFile(tmpdir, "t.stg", templates);
            TemplateGroup group = new TemplateGroupFile(Path.Combine(tmpdir, "t.stg"));
            group.TrackCreationEvents = true;
            Template template = group.GetInstanceOf("list");
            Template line = group.GetInstanceOf("line");
            line.Add("text", "x = 3");
            template.Add("lines", line);
            template.Add("lines", line);
            template.Add("lines", line);
            template.Visualize();
        }

        [TestMethod]
        public void VisualizerTestDefaultArgumentTemplate()
        {
            string templates =
                "main() ::= <<\n" +
                "<f(p=\"x\")>*<f(p=\"y\")>\n" +
                ">>\n" +
                "\n" +
                "f(p,q={<({a<p>})>}) ::= <<\n" +
                "-<q>-\n" +
                ">>";

            writeFile(tmpdir, "t.stg", templates);
            TemplateGroup group = new TemplateGroupFile(Path.Combine(tmpdir, "t.stg"));
            group.TrackCreationEvents = true;
            Template template = group.GetInstanceOf("main");
            string result = template.Render();
            Assert.AreEqual("-ax-*-ay-", result);
            template.Visualize();
        }
    }
}

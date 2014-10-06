// The MIT License (MIT)

// Copyright (c) 2014 Alec Siu, Eric Stollnitz

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPhotosynth;

namespace OpenPhotosynth.WebClient.Tests
{
    [TestClass]
    public class ExtensionMethodTests
    {
        [TestMethod]
        public void FormatTimeSpanTest()
        {
            Assert.AreEqual("1 minute 30 seconds", TimeSpan.FromSeconds(90).FormatTimeSpan());
            Assert.AreEqual("1 minute 1 second", TimeSpan.FromSeconds(61).FormatTimeSpan());
            Assert.AreEqual("60 seconds", TimeSpan.FromMinutes(1).FormatTimeSpan());
            Assert.AreEqual("2 minutes", TimeSpan.FromMinutes(2).FormatTimeSpan());
            Assert.AreEqual("2 seconds", TimeSpan.FromSeconds(1.9).FormatTimeSpan());
            Assert.AreEqual("1 second", TimeSpan.FromSeconds(1.1).FormatTimeSpan());
            Assert.AreEqual("0 seconds", TimeSpan.FromSeconds(0.3).FormatTimeSpan());
            Assert.AreEqual("1 hour 23 minutes", TimeSpan.FromMinutes(83).FormatTimeSpan());
            Assert.AreEqual("3 hours", TimeSpan.FromHours(3).FormatTimeSpan());
            Assert.AreEqual("13 hours", TimeSpan.FromHours(13).FormatTimeSpan());
            Assert.AreEqual("13 hours", TimeSpan.FromHours(13.49).FormatTimeSpan());
            Assert.AreEqual("14 hours", TimeSpan.FromHours(13.5).FormatTimeSpan());
        }

        [TestMethod]
        public void ParseFragmentTest()
        {
            Uri uri = new Uri("http://foo.com/bar#a=123&b=whatever");
            Dictionary<string, string> parameters = uri.ParseFragment();
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("123", parameters["a"]);
            Assert.AreEqual("whatever", parameters["b"]);
        }

        [TestMethod]
        public void ParseFragment_NoFragmentsTest()
        {
            var uri = new Uri("http://foo.com");
            var parameters = uri.ParseFragment();
            Assert.AreEqual(0, parameters.Count);
        }

        [TestMethod]
        public void ParseFragment_QueryStringWithFragmentTest()
        {
            var uri = new Uri("http://foo.com/bar?c=something#a=123&b=whatever");
            var parameters = uri.ParseFragment();
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("123", parameters["a"]);
            Assert.AreEqual("whatever", parameters["b"]);
        }

        [TestMethod]
        public void ParseFragment_PlainFragmentTest()
        {
            var uri = new Uri("http://foo.com/bar#random");
            var parameters = uri.ParseFragment();
            Assert.AreEqual(0, parameters.Count);
        }

        [TestMethod]
        public void ParseUriTest()
        {
            var uri = new Uri("http://foo.com/bar?a=123");
            var parameters = uri.ParseQueryString();
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual("123", parameters["a"]);
        }

        [TestMethod]
        public void ParseUri_EmptyQueryStringTest()
        {
            var uri = new Uri("http://foo.com/bar?");
            var parameters = uri.ParseQueryString();
            Assert.AreEqual(0, parameters.Count);
        }
    }
}

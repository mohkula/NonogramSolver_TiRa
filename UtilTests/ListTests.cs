﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Util;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util.Tests
{
    [TestClass()]
    public class ListTests
    {
        [TestMethod()]
        public void ListInitTest()
        {
            List<int> li = new List<int>();
            Assert.AreEqual(0, li.Count);
            li = new List<int>(15);
            Assert.AreEqual(0, li.Count);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException),
            "Negative List capacity allowed")]
        public void ListNegativeInitTest()
        {
            List<int> li = new List<int>(-1);
        }

        [TestMethod()]
        public void ResizeFromZeroTest()
        {
            List<int> li = new List<int>(0);
            Assert.AreEqual(0, li.Count);
            li.Add(1);
            Assert.AreEqual(1, li.Count);
            Assert.AreEqual(1, li[0]);
        }

        [TestMethod()]
        [ExpectedException(typeof(OutOfMemoryException),
            "Something very strange happened. Created bigger array than possible")]
        public void ToioBigListTest()
        {
            List<int> li = new List<int>(Int32.MaxValue);
        }

        [TestMethod()]
        public void ToArrayTest()
        {
            List<int> li = new List<int>();
            li.Add(2);
            li.Add(123);
            Assert.AreEqual(2, li.Count);
            int[] arr = li.ToArray();
            Assert.AreEqual(2, arr.Length);
            Assert.AreEqual(2, arr[0]);
            Assert.AreEqual(123, arr[1]);
        }
    }
}
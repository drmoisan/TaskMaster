using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Deedle;
using System.Linq;
using System.Diagnostics;
using QuickFiler.Controllers;
using UtilitiesCS;

namespace QuickFiler.Test
{
    [TestClass]
    public class UnitTest1
    {
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestMethod]
        public void TestWrite()
        {
            TestContext.WriteLine("Writing worked");
            Debug.WriteLine("Writing to debug");
            Console.WriteLine("Writing to console");
        }

        [TestMethod]
        public void RandomDeedleExample()
        {
            DebugTextWriter tw = new DebugTextWriter();
            Console.SetOut(tw);
            var rnd = new Random();
            var randNums = Enumerable.Range(0, 100)
              .Select(_ => rnd.NextDouble()).ToOrdinalSeries();
            randNums.Print();
            var buckets = randNums
             .GroupBy(kvp => (int)(kvp.Value * 10))
             .Select(kvp => OptionalValue.Create(kvp.Value.KeyCount));
            buckets.Print();
        }

        [TestMethod]
        public void TestGB()
        {
            var df = LoadDF();
            var topics = df.GetColumn<string>("Conversation").Values.Distinct().ToArray();

            var rows = topics.Select(topic =>
            {
                var dfConversation = df.FilterRowsBy("Conversation", topic);
                var maxSentOn = dfConversation.GetColumn<DateTime>("SentOn").Values.Max();
                var row = dfConversation.FilterRowsBy("SentOn", maxSentOn).Rows.FirstValue();
                //var dfDateIdx = dfConversation.IndexRows<DateTime>("SentOn", keepColumn: true);
                //var addr = dfDateIdx.RowIndex.Locate(maxSentOn);
                //var idx = (int)dfDateIdx.RowIndex.AddressOperations.OffsetOf(addr);
                //var row = dfConversation.Rows.GetAt(idx);
                return row;
            });
            
            var dfFiltered = Frame.FromRows(rows);
            dfFiltered.Print();

        }

        public Frame<int, string> LoadDF()
        {
            var df = Frame.ReadCsv(@"C:\Users\03311352\Documents\Outlook Files\EmailDump230705.csv");
            return df;
        }
    }
}

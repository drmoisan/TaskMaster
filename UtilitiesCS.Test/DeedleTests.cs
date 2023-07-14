using Deedle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class DeedleTests
    {
        [TestMethod]
        public void DeedleDoodles()
        {

            DebugTextWriter tw;
            // Create a collection of anonymous types
            var rnd = new Random();
            var objects = Enumerable.Range(0, 10).Select(i =>
              new { Key = "ID_" + i.ToString(), Number = rnd.Next() });

            // Create data frame with properties as column names
            var dfObjects = Frame.FromRecords(objects);

            
        }


        public class DebugTextWriter : StreamWriter
        {
            public DebugTextWriter()
                : base(new DebugOutStream(), Encoding.Unicode, 1024)
            {
                this.AutoFlush = true;
            }

            sealed class DebugOutStream : Stream
            {
                public override void Write(byte[] buffer, int offset, int count)
                {
                    Debug.Write(Encoding.Unicode.GetString(buffer, offset, count));
                }

                public override bool CanRead => false;
                public override bool CanSeek => false;
                public override bool CanWrite => true;
                public override void Flush() => Debug.Flush();

                public override long Length => throw bad_op;
                public override int Read(byte[] buffer, int offset, int count) => throw bad_op;
                public override long Seek(long offset, SeekOrigin origin) => throw bad_op;
                public override void SetLength(long value) => throw bad_op;
                public override long Position
                {
                    get => throw bad_op;
                    set => throw bad_op;
                }

                static InvalidOperationException bad_op => new InvalidOperationException();
            };
        }
    }
}

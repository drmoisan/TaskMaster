using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;


namespace UtilitiesCS.HelperClasses
{
    public class DebugTextLogger : StreamWriter
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DebugTextLogger()
            : base(new DebugOutStream(), Encoding.Unicode, 1024)
        {
            this.AutoFlush = true;
        }

        sealed class DebugOutStream : Stream
        {
            public override void Write(byte[] buffer, int offset, int count)
            {
                //logger.Debug(Encoding.Unicode.GetString(buffer, offset, count));
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

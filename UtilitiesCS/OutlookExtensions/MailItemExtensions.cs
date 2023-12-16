using Exchange.Export.MAPIMessageConverter;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookExtensions
{
    public static class MailItemExtensions
    {
        public static byte[] ToMIME(this Microsoft.Office.Interop.Outlook.MailItem mailItem)
        {
            byte[] mimeContent = mailItem.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x10130102") as byte[];
            return mimeContent;
        }

        //public static Stream GetEmlStream(this Outlook.MailItem mailItem)
        //{
        //    Type converter = Type.GetTypeFromCLSID(MAPIMethods.CLSID_IConverterSession);
        //    object obj = Activator.CreateInstance(converter);
        //    MAPIMethods.IConverterSession session = (MAPIMethods.IConverterSession)obj;

        //    if (session != null)
        //    {
        //        uint hr = session.SetEncoding(MAPIMethods.ENCODINGTYPE.IET_QP);
        //        hr = session.SetSaveFormat(MAPIMethods.MIMESAVETYPE.SAVE_RFC822);
        //        //var stream = new ComMemoryStream();
                
        //        hr = session.MAPIToMIMEStm((MAPIMethods.IMessage)mailItem.MAPIOBJECT, stream, MAPIMethods.MAPITOMIMEFLAGS.CCSF_SMTP);
        //        if (hr != 0)
        //            throw new ArgumentException("There are some invalid COM arguments");
        
                
        //        stream.Position = 0;

        //        return stream;
        //    }

        //    return null;
        //}

        //private static IntPtr ReadBuffer;

        //static int Read(System.Runtime.InteropServices.ComTypes.IStream strm,
        //    byte[] buffer)
        //{
        //    if (ReadBuffer == IntPtr.Zero) ReadBuffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)));
        //    strm.Read(buffer, buffer.Length, ReadBuffer);
        //    return Marshal.ReadInt32(ReadBuffer);
        //}
    }
}

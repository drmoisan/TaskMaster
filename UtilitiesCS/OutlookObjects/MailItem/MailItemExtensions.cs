using Exchange.Export.MAPIMessageConverter;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookExtensions
{
    public static class MailItemExtensions
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static byte[] ToMIME(this Microsoft.Office.Interop.Outlook.MailItem mailItem)
        {
            byte[] mimeContent = mailItem.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x10130102") as byte[];
            return mimeContent;
        }

        public static async Task<object> TryMoveAsync(this Outlook.MailItem mailItem, Outlook.Folder folder, int retries = 0)
        {
            if (mailItem is null || folder is null)
                return null;

            try
            {
                return await Task.Run(() => mailItem.Move(folder));
            }
            catch (Exception e)
            {
                logger.Error($"Error moving mail item to folder: {e.Message}");
                if (retries > 0)
                {
                    logger.Warn($"Retrying move operation. Retries remaining: {retries}");
                    return await mailItem.TryMoveAsync(folder, retries - 1);
                }
                else 
                { 
                    return null; 
                }
            }            
        }

        #region commented code
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

        #endregion commented code
    }
}

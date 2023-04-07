using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Resources;
using System.Globalization;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod1()
        {
            Form1 frm = new Form1();
            frm.ShowDialog();
        }
        [TestMethod]
        public void TestMethod2()
        {
            Form2 frm = new Form2();
            frm.ShowDialog();
        }
        [TestMethod]
        public void TestMethod3()
        {
            //using (ResXResourceReader resxReader = new ResXResourceReader(Properties.Resources.ResourceManager)) { }
            using ResourceSet rs = Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            {
                // Create an IDictionaryEnumerator to read the data in the ResourceSet.
                IDictionaryEnumerator id = rs.GetEnumerator();

                var resrcs = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
                var bdir = System.AppDomain.CurrentDomain.BaseDirectory;
                // Iterate through the ResourceSet and display the contents to the console. 
                while (id.MoveNext())
                {
                    Debug.WriteLine("\n[{0}] \t{1}", id.Key, id.Value);
                    string msg = ($"Item {id.Key} is not an SVG file");

                    if (id.Value.GetType() == typeof(byte[]))
                    {
                        byte[] b = (byte[])id.Value;
                        MemoryStream ms = new MemoryStream(b);
                        if (IsSvgFile(ms)) 
                        {
                            msg = ($"Item {id.Key} is an SVG file");
                        }
                    }
                    Debug.WriteLine(msg);                    
                }
                
                var rd = rs.GetDefaultReader();
            }
            

            Debug.WriteLine("d");
            //Properties.Resources
        }

        [TestMethod]
        public void TestMethod4()
        {
            byte[] bs = ObjectToByteArray(null);
        }

        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                if (obj != null)
                    bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private static bool IsSvgFile(Stream fileStream)
        {
            try
            {
                using (var xmlReader = XmlReader.Create(fileStream))
                {
                    return xmlReader.MoveToContent() == XmlNodeType.Element && "svg".Equals(xmlReader.Name, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        [TestMethod]
        public void TestMethod5()
        {
            Form2 frm = new Form2();
            frm.ShowDialog();
        }

    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using UtilitiesCS;
using UtilitiesCS;
using Moq;
using System.IO;

namespace EmailIntelligence.Test
{
    [TestClass]
    public class CTFIncidences_Test
    {
        //[TestMethod]
        //public void CTF_Incidence_Text_File_READ_Test()
        //{
        //    string oneDrive = Environment.GetEnvironmentVariable("OneDriveCommercial");
        //    string folderpath = Path.Combine(oneDrive, "Email attachments from Flow", "Combined", "data");
        //    string filename = "9999999CTF_Inc.txt";

        //    CtfIncidenceList ctfs = new CtfIncidenceList();
        //    ctfs.CTF_Incidence_Text_File_READ(folderpath, filename);
        //}

        [TestMethod]
        public void Deserialize_JSONMissing_Test()
        {
            string oneDrive = Environment.GetEnvironmentVariable("OneDriveCommercial");
            string folderpath = Path.Combine(oneDrive, "Email attachments from Flow", "Combined", "data");
            string filename = "9999999CTF_Inc.json";
            string backupFilename = "9999999CTF_Inc.txt";
            string backupFilepath = Path.Combine(folderpath, backupFilename);
            CtfIncidenceList ctfs = new CtfIncidenceList(filename: filename, 
                                                   folderpath: folderpath, 
                                                   backupFilepath : backupFilepath);
        }
    }
}

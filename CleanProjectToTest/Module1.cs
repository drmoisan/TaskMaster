using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using UtilitiesCS;

namespace CleanProjectToTest
{

    public static class Module1
    {
        private static int CTF_Inc_Ct;
        private static CTF_Incidence2[] CTF_Inc;

        public static void CTF_Incidence_Text_File_READ1(IFileSystemFolderPaths FolderPaths)
        {



            // INITIALIZE VARIABLES
            int i;
            CTF_Inc_Ct = 0;
            CTF_Inc = new CTF_Incidence2[1];
            string filepath = Path.Combine(FolderPaths.FldrPythonStaging, My.MySettingsProperty.Settings.File_CTF_Inc);

            // OPEN FILE IF IT EXISTS AND READ IT IN
            if (File.Exists(filepath))
            {
                string[] filecontents = File.ReadAllLines(filepath, System.Text.Encoding.ASCII);
                var lines = new Queue<string>(filecontents.Skip(1));
                var listCTF = new List<CTF_Incidence2>();
                listCTF.Add(new CTF_Incidence2());

                while (lines.Count > 0)
                {
                    var tmpCTF_Inc = new CTF_Incidence2();
                    tmpCTF_Inc.Email_Conversation_ID = lines.Dequeue();
                    tmpCTF_Inc.Folder_Count = Conversions.ToInteger(lines.Dequeue());
                    var loopTo = tmpCTF_Inc.Folder_Count;
                    for (i = 1; i <= loopTo; i++)
                    {
                        tmpCTF_Inc.Email_Folder[i] = lines.Dequeue();
                        tmpCTF_Inc.Email_Conversation_Count[i] = Conversions.ToInteger(lines.Dequeue());
                    }
                    listCTF.Add(tmpCTF_Inc);
                }
                // ReDim CTF_Inc(listCTF.Count)
                CTF_Inc = listCTF.ToArray();
                CTF_Inc_Ct = listCTF.Count;
            }
            // Need to set CTF_Inc_Ct
            else
            {
                Interaction.MsgBox("Index file not found. Please run indexer.", Constants.vbCritical);
            }


        }
    }
}
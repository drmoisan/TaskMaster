using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using ToDoModel.Legacy;

namespace ToDoModel
{

    public static class ToDoProjectInfoUtilities
    {
        public static ProjectInfoLegacy LoadToDoProjectInfo(string filePath)
        {
            ProjectInfoLegacy ProjInfo;
            if (File.Exists(filePath))
            {
                // Dim TestFileStream As Stream = File.OpenRead(filePath)
                var deserializer = new BinaryFormatter();
                try
                {
                    using (Stream TestFileStream = File.OpenRead(filePath))
                    {
                        ProjInfo = (ProjectInfoLegacy)deserializer.Deserialize(TestFileStream);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Unexpected Access Error. Duplicate Instance Running?");
                    throw ex;
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Unexpected IO Error. Is Project Info File Corrupt?");
                    throw ex;
                }

                ProjInfo.FileName = filePath;
                ProjInfo.Sort();
                return ProjInfo;
            }
            else
            {
                ProjInfo = new ProjectInfoLegacy();
                ProjInfo.Save(filePath);
                return ProjInfo;
            }

        }
    }
}
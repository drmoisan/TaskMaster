using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace ToDoModel
{

    public static class ToDoProjectInfoUtilities
    {
        public static ProjectInfo LoadToDoProjectInfo(string filePath)
        {
            ProjectInfo ProjInfo;
            if (File.Exists(filePath))
            {
                // Dim TestFileStream As Stream = File.OpenRead(filePath)
                var deserializer = new BinaryFormatter();
                try
                {
                    using (Stream TestFileStream = File.OpenRead(filePath))
                    {
                        ProjInfo = (ProjectInfo)deserializer.Deserialize(TestFileStream);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    var unused1 = Interaction.MsgBox("Unexpected Access Error. Duplicate Instance Running?");
                    throw ex;
                }
                catch (IOException ex)
                {
                    var unused = Interaction.MsgBox("Unexpected IO Error. Is Project Info File Corrupt?");
                    throw ex;
                }

                ProjInfo.FileName = filePath;
                ProjInfo.Sort();
                return ProjInfo;
            }
            else
            {
                ProjInfo = new ProjectInfo();
                ProjInfo.Save(filePath);
                return ProjInfo;
            }

        }
    }
}
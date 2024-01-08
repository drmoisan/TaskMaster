using System;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class DedicatedToken: IEquatable<DedicatedToken>
    {
        public DedicatedToken(string token, string folderPath, int count)
        {
            Token = token;
            FolderPath = folderPath;
            Count = count;
        }

        private string _token;
        public string Token { get => _token; set => _token = value; }
        
        private string _folderPath;
        public string FolderPath { get => _folderPath; set => _folderPath = value; }

        private int _count;
        public int Count { get => _count; set => _count = value; }

        public bool Equals(DedicatedToken other)
        {
            if (other is null)
                return false;
            return this.Token == other.Token && this.FolderPath == other.FolderPath;
        }
    }
}

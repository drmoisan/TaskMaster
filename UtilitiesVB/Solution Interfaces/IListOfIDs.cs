using System.Collections.Generic;
using System.Numerics;
using Microsoft.Office.Interop.Outlook;

namespace UtilitiesVB
{

    public interface IListOfIDs
    {
        string Filepath { get; set; }
        long MaxIDLength { get; }
        List<string> UsedIDList { get; set; }
        void CompressToDoIDs(Application OlApp);
        void RefreshIDList(Application Application);
        void Save();
        void Save(string Filepath);
        string ConvertToBase(int nbase, BigInteger num, int intMinDigits = 2);
        BigInteger ConvertToDecimal(int nbase, string strBase);
        string GetMaxToDoID();
        string GetNextAvailableToDoID(string strSeed);
        void SubstituteIdRoot(string oldPrefix, string newPrefix);
    }
}
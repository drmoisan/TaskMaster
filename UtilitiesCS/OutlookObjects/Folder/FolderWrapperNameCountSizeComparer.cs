using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;

namespace UtilitiesCS.OutlookObjects.Folder
{
    // Equality comparer for FolderWrapper based on Name and ItemCount
    public class FolderWrapperNameCountSizeComparer : IEqualityComparer<FolderWrapper>
    {
        public bool Equals(FolderWrapper x, FolderWrapper y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase)
                && x.ItemCount == y.ItemCount && x.FolderSize == y.FolderSize;
        }

        public int GetHashCode(FolderWrapper obj)
        {
            if (obj is null) return 0;
            int hashName = obj.Name?.ToLowerInvariant().GetHashCode() ?? 0;
            int hashCount = obj.ItemCount.GetHashCode();
            int hashSize = obj.FolderSize.GetHashCode();
            return hashName * 31 + hashCount *31 *31 + hashSize;
        }
    }
}



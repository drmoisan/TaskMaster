using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    public class FolderWrapperNodeContentsComparer : IEqualityComparer<TreeNode<FolderWrapper>>
    {
        public bool Equals(TreeNode<FolderWrapper> x, TreeNode<FolderWrapper> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x?.Value is null || y?.Value is null) return false;
            // Compare FolderWrapper values using the FolderWrapperNameCountSizeComparer
            var comparer = new FolderWrapperNameCountSizeComparer();
            return comparer.Equals(x.Value, y.Value);
        }

        public int GetHashCode(TreeNode<FolderWrapper> obj)
        {
            if (obj?.Value is null) return 0;
            var comparer = new FolderWrapperNameCountSizeComparer();
            // Get the hash code for the FolderWrapper value
            return comparer.GetHashCode(obj.Value);            
        }
    }
}

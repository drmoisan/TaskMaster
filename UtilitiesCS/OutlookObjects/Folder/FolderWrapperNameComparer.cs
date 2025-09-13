using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    public class FolderWrapperNameComparer : IEqualityComparer<TreeNode<FolderWrapper>>
    {
        public bool Equals(TreeNode<FolderWrapper> x, TreeNode<FolderWrapper> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x?.Value is null || y?.Value is null) return false;
            if (x.Value.Name.IsNullOrEmpty() || y.Value.Name.IsNullOrEmpty()) return false;            
            return x.Value.Name == y.Value.Name;
        }

        public int GetHashCode(TreeNode<FolderWrapper> obj)
        {
            return obj.Value?.Name?.ToLowerInvariant().GetHashCode() ?? 0;
        }
    }
}



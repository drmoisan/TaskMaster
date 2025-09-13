using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    public class FolderWrapperNameAndParentNameComparer:IEqualityComparer<TreeNode<FolderWrapper>>
    {
        public bool Equals(TreeNode<FolderWrapper> x, TreeNode<FolderWrapper> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;            
            var nameComparison = string.Equals(x.Value.Name, y.Value.Name, StringComparison.OrdinalIgnoreCase);
            if (!nameComparison) return false;
            // Compare parent names if both nodes have parents
            if (x.Parent is null && y.Parent is null) { return true; }// Both have no parent
            else if (x.Parent is null || y.Parent is null) { return false; }// One has a parent, the other does not           
            else if (x.Parent.Value is null && y.Parent.Value is null) { return true; }// Both parents are nodes with null values
            else if (x.Parent.Value is null || y.Parent.Value is null) { return false; }// One parent is a node with null value, the other is not
            else if (x.Parent.Value.Name.IsNullOrEmpty() && y.Parent.Value.Name.IsNullOrEmpty()) { return true; }// Both parents have null names
            else if (x.Parent.Value.Name.IsNullOrEmpty() || y.Parent.Value.Name.IsNullOrEmpty()) { return false; }// One parent has a null name, the other does not
            else
            {
                return string.Equals(x.Parent.Value.Name, y.Parent.Value.Name, StringComparison.OrdinalIgnoreCase);
            }
        }

        public int GetHashCode(TreeNode<FolderWrapper> obj)
        {
            if (obj is null) return 0;            
            int hashName = obj.Value?.Name?.ToLowerInvariant().GetHashCode() ?? 0; 
            int hashParentName = obj.Parent?.Value?.Name?.ToLowerInvariant().GetHashCode() ?? 0;
            return hashName * 31 + hashParentName;
        }
    }
}

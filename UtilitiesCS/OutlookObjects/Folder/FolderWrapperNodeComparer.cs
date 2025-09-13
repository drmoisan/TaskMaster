using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    public class FolderWrapperNodeComparer : IEqualityComparer<TreeNode<FolderWrapper>>
    {
        public bool Equals(TreeNode<FolderWrapper> x, TreeNode<FolderWrapper> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x?.Value is null || y?.Value is null) return false;

            // Compare FolderWrapper values using the FolderWrapperNameCountSizeComparer
            var comparer = new FolderWrapperNameCountSizeComparer();
            if (!comparer.Equals(x.Value, y.Value)) return false; // If FolderWrapper values are not equal, return false

            // Compare parents using FolderWrapperNameAndParentNameComparer            
            var parentsEqual = false;
            if (x?.Parent?.Value is null && y?.Parent?.Value is null)
            {
                parentsEqual = true; // Both have no parent
            }
            else if (x?.Parent is null || y?.Parent is null)
            {
                parentsEqual = false; // One has a parent, the other does not
            }
            else if (comparer.Equals(x.Parent.Value, y.Parent.Value))
            {
                parentsEqual = true; // Both parents are equal
            }
            if (!parentsEqual) return false;

            if ((x.Children?.Count ?? 0) == 0 && (y.Children?.Count ?? 0) == 0) { return true; } // Neither has children
            else if ((x.Children?.Count ?? 0) != (y.Children?.Count ?? 0)) { return false; } // Different number of children
            else 
            {
                var xChildren = x.Children.Select(c => c.Value).ToList();
                var yChildren = y.Children.Select(c => c.Value).ToList();
                return xChildren.Intersect(yChildren, comparer).Count() == x.Children.Count; // Compare children using the FolderWrapperNameCountSizeComparer                
            }
        }

        public int GetHashCode(TreeNode<FolderWrapper> obj)
        {
            if (obj?.Value is null) return 0;
            var comparer = new FolderWrapperNameCountSizeComparer();
            List<int> hashCodes = [];
            hashCodes.Add(comparer.GetHashCode(obj.Value)); // Hash code for the FolderWrapper value
            hashCodes.Add(obj.Parent?.Value is null ? 0 : comparer.GetHashCode(obj.Parent.Value)); // Hash code for the parent FolderWrapper value
            obj.Children?.Select(c => c.Value)?.ForEach(wrapper => hashCodes.Add(comparer.GetHashCode(wrapper)));
            var hash = hashCodes.Aggregate(0, (current, code) => current * 31 + code); // Combine all hash codes
            return hash;
        }
    }
}

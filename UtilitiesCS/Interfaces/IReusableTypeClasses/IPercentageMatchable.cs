using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.Interfaces.IReusableTypeClasses
{
    public interface IPercentageMatchable<T> 
    {
        /// <summary>
        /// Calculates the percentage match between two collections.
        /// </summary>
        /// <param name="collection1">The first collection.</param>
        /// <param name="collection2">The second collection.</param>
        /// <returns>A double representing the percentage match (0.0 to 100.0).</returns>
        double CalculateMatchPercentage(IEnumerable<T> other);
    }
}

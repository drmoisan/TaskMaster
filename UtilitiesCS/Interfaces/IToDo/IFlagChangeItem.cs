using System.Collections.Generic;

namespace UtilitiesCS.Interfaces
{
    public interface IFlagChangeItem
    {
        string ClassifierName { get; set; }
        IList<string> TrainFlags { get; set; }
        IList<string> UntrainFlags { get; set; }
    }
}
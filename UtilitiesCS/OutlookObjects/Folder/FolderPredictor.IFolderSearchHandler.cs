#nullable enable
namespace UtilitiesCS
{
    /// <summary>
    /// Cycle-3 (P10-T4): declares <see cref="FolderPredictor"/>'s implementation of the narrow
    /// <see cref="IFolderSearchHandler"/> seam on a second partial-class part, so
    /// <c>FolderPredictor.cs</c> itself (already 823 lines, over the 500-line cap before this cycle)
    /// is not touched beyond the one-word <c>partial</c> edit in P10-T3.
    /// </summary>
    public partial class FolderPredictor : IFolderSearchHandler { }
}

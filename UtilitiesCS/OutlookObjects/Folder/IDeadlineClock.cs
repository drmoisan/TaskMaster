namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Provides monotonic deadline checks for cache building without wall-clock dependencies.
    /// Unit tests must use fake clocks with explicit advancement.
    /// </summary>
    public interface IDeadlineClock
    {
        bool ShouldYield();

        void Reset();
    }
}

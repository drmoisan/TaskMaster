using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder.Fakes
{
    public sealed class FakeDeadlineClock : IDeadlineClock
    {
        private bool _shouldYield;

        public int CheckCount { get; private set; }

        public int ResetCount { get; private set; }

        public void AdvanceToYield()
        {
            _shouldYield = true;
        }

        public void AdvanceWithoutYield()
        {
            _shouldYield = false;
        }

        public bool ShouldYield()
        {
            CheckCount++;
            return _shouldYield;
        }

        public void Reset()
        {
            ResetCount++;
            _shouldYield = false;
        }
    }
}

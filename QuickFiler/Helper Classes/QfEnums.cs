
namespace QuickFiler
{
    public static class QfEnums
    {
        public enum InitTypeEnum
        {
            Sort = 1,                    // 00000000 00000001   2^0
            Find = 2,                    // 00000000 00000010   2^1
            Info = 4,                    // 00000000 00000100   2^2
            Reminder = 8,                // 00000000 00001000   2^3
            SortConv = 16,               // 00000000 00010000   2^4
        }

        //public enum ToggleState { Off = 0, On = 1 }

    }
}

namespace QuickFiler
{
    public static class Enums
    {
        public enum InitTypeEnum
        {
            InitSort = 1,                    // 00000000 00000001   2^0
            InitFind = 2,                    // 00000000 00000010   2^1
            InitInfo = 4,                    // 00000000 00000100   2^2
            InitConditionalReminder = 8     // 00000000 00001000   2^3
        }

    }
}
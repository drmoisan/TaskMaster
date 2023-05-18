using System;

namespace UtilitiesVB
{

    public static class ArrayIsAllocated
    {
        public static bool IsAllocated(ref Array inArray)
        {
            bool FlagEx = true;
            try
            {
                if (inArray is null)
                {
                    FlagEx = false;
                }
                else if (inArray.Length <= 0)
                {
                    FlagEx = false;
                }
                else if (inArray.GetValue(0) == null)
                {
                    FlagEx = false;
                }
            }
            catch 
            {
                FlagEx = false;
            }
            return FlagEx;
        }

        public static bool IsAllocated(ref string[] inArray)
        {
            bool FlagEx = true;
            try
            {
                if (inArray is null)
                {
                    FlagEx = false;
                }
                else if (inArray.Length <= 0)
                {
                    FlagEx = false;
                }
                else if (inArray[0] is null)
                {
                    FlagEx = false;
                }
            }
            catch (Exception ex)
            {
                FlagEx = false;
            }
            return FlagEx;
        }
    }
}
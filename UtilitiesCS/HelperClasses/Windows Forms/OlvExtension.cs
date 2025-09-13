using System;
using BrightIdeasSoftware;

namespace UtilitiesCS
{

    public static class OlvExtension
    {
        public static void AutoScaleColumnsToContainer(this ObjectListView olv)
        {
            int containerwidth = olv.Width;
            olv.BeginUpdate();
            int colswidth = 0;
            foreach (OLVColumn c in olv.Columns)
                colswidth += c.Width;
            if (colswidth != 0)
            {
                foreach (OLVColumn c in olv.Columns)
                    c.Width = (int)Math.Round(Math.Round(c.Width * (double)containerwidth / colswidth));
            }
            olv.EndUpdate();
        }
    }
}
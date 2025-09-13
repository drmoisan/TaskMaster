using System;
using UtilitiesCS.Interfaces;

namespace UtilitiesCS.Interfaces
{
    public interface ITimerWrapper: IGenericTimer
    {
        bool AutoReset { get; set; }
        void ResetTimer();


    }
}
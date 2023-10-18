using System;
using System.Timers;
//using PInvoke = Windows.Win32.PInvoke;

namespace ToDoModel
{

    public class StopWatch
    {

        // Private Declare Function getFrequency Lib "kernel32" _
        // Alias "QueryPerformanceFrequency" (ByRef cyFrequency As Decimal) As Long



        // #If VBA7 Then
        // Private Declare PtrSafe Function getTickCount Lib "kernel32" _
        // Alias "QueryPerformanceCounter" (cyTickCount As Currency) As LongPtr
        // #Else
        // Private Declare Function getTickCount Lib "kernel32" _
        // Alias "QueryPerformanceCounter" (cyTickCount As Decimal) As Long
        // #End If

        private double _start;                    
        private double _cum;                      
        public bool IsPaused;                 
        public int InstanceNum;               
        public DateTime TimeInit;
        public DateTime TimeEnd;
        private long _cMicroTimer_lpFrequency = default;

        private double cMicroTimer()
        {
            // Returns seconds.
            // Dim cyTicks1 As Decimal
            long lpPerformanceCount;
            // Static cyFrequency As Decimal
            // Get frequency.
            if (_cMicroTimer_lpFrequency == 0L)
                PInvoke.QueryPerformanceFrequency(out _cMicroTimer_lpFrequency);
            // If cyFrequency = 0 Then getFrequency(cyFrequency)
            // Get ticks.
            PInvoke.QueryPerformanceCounter(out lpPerformanceCount);
            // getTickCount(cyTicks1)
            // Seconds
            double result = 0d;
            if (_cMicroTimer_lpFrequency != 0L)
            {
                result = lpPerformanceCount / (double)_cMicroTimer_lpFrequency;
            }
            return result;
            // If cyFrequency Then cMicroTimer = cyTicks1 / cyFrequency
        }

        public void Start()
        {
            // cumulative time passed
            IsPaused = false;
            _cum = 0d;
            TimeInit = DateTime.Now;
            reStart();
        }
        public void reStart()
        {

            // start timing and schedule an update
            _start = cMicroTimer();
            IsPaused = false;
        }

        public void Pause()
        {
            // this should be called when the pause toggle Button is pressed

            if (!IsPaused)
            {
                // pause requested
                _cum = Elapsed + _cum;
                IsPaused = true;
            }
        }

        public void StopTimer()
        {
            Pause();
            TimeEnd = DateTime.Now;
        }

        public double TimeElapsed
        {
            get
            {
                double timeElapsedRet = default;
                double Temp;
                // timeElapsed = Elapsed + pCum
                if (IsPaused == true)
                {
                    timeElapsedRet = _cum;
                }
                else
                {
                    Temp = Elapsed + _cum;
                    return Temp;
                }

                return timeElapsedRet;
            }
        }

        private double Elapsed
        {
            get
            {
                // return time elapsed
                // Elapsed = cMicroTimer() - pStart
                return cMicroTimer() - _start;
            }
        }







    }
}
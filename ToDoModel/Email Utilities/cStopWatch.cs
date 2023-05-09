using System;
using PInvoke = Windows.Win32.PInvoke;

namespace ToDoModel
{

    public class cStopWatch
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

        private double pStart;                    // When the current timing session started (since last pause)
        private double pCum;                      // cumulative time passed so far
        public bool isPaused;                 // is
        public int InstanceNum;               // Instance of the class
        public DateTime timeInit;
        public DateTime timeEnd;
        private long _cMicroTimer_lpFrequency = default;

        private double cMicroTimer()
        {
            double cMicroTimerRet = default;
            // Returns seconds.
            // Dim cyTicks1 As Decimal
            long lpPerformanceCount;
            // Static cyFrequency As Decimal
            cMicroTimerRet = 0d;
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
            isPaused = false;
            pCum = 0d;
            timeInit = DateTime.Now;
            reStart();
        }
        public void reStart()
        {

            // start timing and schedule an update
            pStart = cMicroTimer();
            isPaused = false;
        }

        public void Pause()
        {
            // this should be called when the pause toggle Button is pressed

            if (!isPaused)
            {
                // pause requested
                pCum = Elapsed + pCum;
                isPaused = true;
            }
        }

        public void StopTimer()
        {
            Pause();
            timeEnd = DateTime.Now;
        }

        public double timeElapsed
        {
            get
            {
                double timeElapsedRet = default;
                double Temp;
                // timeElapsed = Elapsed + pCum
                if (isPaused == true)
                {
                    timeElapsedRet = pCum;
                }
                else
                {
                    Temp = Elapsed + pCum;
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
                return cMicroTimer() - pStart;
            }
        }







    }
}
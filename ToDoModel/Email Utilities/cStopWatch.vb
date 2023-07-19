Imports PInvoke = Windows.Win32.PInvoke

Public Class cStopWatch



    'Private Declare Function getFrequency Lib "kernel32" _
    '    Alias "QueryPerformanceFrequency" (ByRef cyFrequency As Decimal) As Long



    '#If VBA7 Then
    '    Private Declare PtrSafe Function getTickCount Lib "kernel32" _
    '    Alias "QueryPerformanceCounter" (cyTickCount As Currency) As LongPtr
    '#Else
    '    Private Declare Function getTickCount Lib "kernel32" _
    '    Alias "QueryPerformanceCounter" (cyTickCount As Decimal) As Long
    '#End If

    Private pStart As Double                    ' When the current timing session started (since last pause)
    Private pCum As Double                      ' cumulative time passed so far
    Public isPaused As Boolean                 ' is
    Public InstanceNum As Integer               'Instance of the class
    Public timeInit As Date
    Public timeEnd As Date

    Private Function cMicroTimer() As Double
        ' Returns seconds.
        'Dim cyTicks1 As Decimal
        Dim lpPerformanceCount As Long
        Static lpFrequency As Long
        'Static cyFrequency As Decimal
        cMicroTimer = 0
        ' Get frequency.
        If lpFrequency = 0 Then PInvoke.QueryPerformanceFrequency(lpFrequency)
        'If cyFrequency = 0 Then getFrequency(cyFrequency)
        ' Get ticks.
        PInvoke.QueryPerformanceCounter(lpPerformanceCount)
        'getTickCount(cyTicks1)
        ' Seconds
        Dim result As Double = 0
        If lpFrequency <> 0 Then
            result = CDbl(lpPerformanceCount) / CDbl(lpFrequency)
        End If
        Return result
        'If cyFrequency Then cMicroTimer = cyTicks1 / cyFrequency
    End Function

    Public Sub Start()
        ' cumulative time passed
        isPaused = False
        pCum = 0
        timeInit = Now()
        reStart()
    End Sub
    Public Sub reStart()

        ' start timing and schedule an update
        pStart = cMicroTimer()
        isPaused = False
    End Sub

    Public Sub Pause()
        ' this should be called when the pause toggle Button is pressed

        If Not isPaused Then
            ' pause requested
            pCum = Elapsed + pCum
            isPaused = True
        End If
    End Sub

    Public Sub StopTimer()
        Pause()
        timeEnd = Now()
    End Sub

    Public ReadOnly Property timeElapsed() As Double
        Get
            Dim Temp As Double
            'timeElapsed = Elapsed + pCum
            If isPaused = True Then
                timeElapsed = pCum
            Else
                Temp = Elapsed + pCum
                Return Temp
            End If
        End Get
    End Property

    Private ReadOnly Property Elapsed() As Double
        Get
            ' return time elapsed
            'Elapsed = cMicroTimer() - pStart
            Return cMicroTimer() - pStart
        End Get
    End Property







End Class

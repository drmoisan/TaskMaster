Public Module DeclaratonsWinAPI

    Public Enum WindowLongFlags As Integer
        GWL_EXSTYLE = -20
        GWLP_HINSTANCE = -6
        GWLP_HWNDPARENT = -8
        GWL_ID = -12
        GWL_STYLE = -16
        GWL_USERDATA = -21
        GWL_WNDPROC = -4
        DWLP_USER = &H8
        DWLP_MSGRESULT = &H0
        DWLP_DLGPROC = &H4
    End Enum

    Public Enum GetAncestor_Flags
        GetParent = 1
        GetRoot = 2
        GetRootOwner = 3
    End Enum

    Public Declare Function SetWindowLongPtr Lib "user32" Alias "SetWindowLongPtrA" _
        (ByVal hWnd As Long,
         ByVal nIndex As WindowLongFlags,
         ByVal dwNewLong As Long) As Long

    Public Declare Function GetWindowLong Lib "user32.dll" Alias "GetWindowLongA" _
        (ByVal prmlngWindowHandle As Long,
         ByVal prmlngIndex As WindowLongFlags) As Long

    Public Declare Function FindWindow Lib "user32.dll" Alias "FindWindowA" _
        (lpClassName As String, lpWindowName As String) As IntPtr

    Public Declare Function ShowWindow Lib "user32" _
         (ByVal hWnd As Long,
          ByVal nCmdShow As Long) As Long

    Public Declare Function SendMessage Lib "user32" Alias "SendMessageA" _
        (ByVal hwnd As IntPtr,
         wMsg As Integer,
         wParam As Integer,
         lParam As IntPtr) As IntPtr

    Public Declare Function DrawMenuBar Lib "user32" (ByVal hwnd As IntPtr) As Boolean

    Public Declare Function EnableWindow Lib "user32" _
        (ByVal hWnd As IntPtr,
         ByVal bEnable As Boolean) As Boolean

    Public Declare Function GetAncestor Lib "user32" _
        (ByVal hwnd As IntPtr,
         ByVal gaFlags As GetAncestor_Flags) As IntPtr

End Module

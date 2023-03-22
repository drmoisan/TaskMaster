Friend Module QuickFileControllerConstants

#Region "API Constants"
    Friend Const GWL_STYLE As Long = -16 'Sets a new window style  As LongPtr in 64bit version
    Friend Const WS_SYSMENU As Long = &H80000 'Windows style   As LongPtr in 64bit version
    Friend Const WS_THICKFRAME = &H40000 'Style that is apparently resizble
    Friend Const WS_MINIMIZEBOX As Long = &H20000   'As LongPtr in 64bit version
    Friend Const WS_MAXIMIZEBOX As Long = &H10000   'As LongPtr in 64bit version
    Friend Const SW_SHOWMAXIMIZED = 3
    Friend Const SW_FORCEMINIMIZE = 11
    Friend Const WM_SETFOCUS = &H7
    Friend Const GWL_HWNDPARENT = -8
    Friend Const olAppCLSN As String = "rctrl_renwnd32"
    Friend Const GA_PARENT = 1
    Friend Const GA_ROOTOWNER = 3
    Friend Const Modal = 0, Modeless = 1
#End Region

#Region "Form and Control Constants"
    Friend Const Top_Offset As Long = 6
    Friend Const Top_Offset_C As Long = 0
    Friend Const Left_frm As Long = 12
    Friend Const Left_lbl1 As Long = 6
    Friend Const Left_lbl2 As Long = 6
    Friend Const Left_lbl3 As Long = 6
    Friend Const Left_lbl4 As Long = 6
    Friend Const Left_lbl5 As Long = 372           'Folder:
    Friend Const Left_lblSender As Long = 66            '<SENDER>
    Friend Const Left_lblSender_C As Long = 6             '<SENDER> Compact view
    Friend Const Right_Aligned As Long = 648
    Friend Const Left_lblTriage As Long = 181           'X Triage placeholder
    Friend Const Left_lblActionable As Long = 198           '<ACTIONABL>
    Friend Const Left_lblSubject As Long = 66            '<SUBJECT>
    Friend Const Left_lblSubject_C As Long = 6             '<SUBJECT> Compact view
    Friend Const Left_lblBody As Long = 66            '<BODY>
    Friend Const Left_lblBody_C As Long = 6             '<BODY> Compact view
    Friend Const Left_lblSentOn As Long = 66            '<SENTON>
    Friend Const Left_lblSentOn_C As Long = 200           '<SENTON> Compact view
    Friend Const Left_lblConvCt As Long = 290           'Count of Conversation Members
    Friend Const Left_lblConvCt_C As Long = 320           'Count of Conversation Members Compact view
    Friend Const Left_lblPos As Long = 6             'ACCELERATOR Email Position
    Friend Const Left_cbxFolder As Long = 372           'Combo box containing Folder Suggestions
    Friend Const Left_inpt As Long = 408           'Input for folder search
    Friend Const Left_chbxGPConv As Long = 210           'Checkbox to Group Conversations
    Friend Const Left_chbxGPConv_C As Long = 372           'Checkbox to Group Conversations
    Friend Const Left_cbDelItem As Long = 588           'Delete email
    Friend Const Left_cbKllItem As Long = 618           'Remove mail from Processing
    Friend Const Left_cbFlagItem As Long = 569           'Flag as Task
    Friend Const Left_lblAcF As Long = 363           'ACCELERATOR F for Folder Search
    Friend Const Left_lblAcD As Long = 363           'ACCELERATOR D for Folder Dropdown
    Friend Const Left_lblAcC As Long = 384           'ACCELERATOR C for Grouping Conversations
    Friend Const Left_lblAcC_C As Long = 548           'ACCELERATOR C for Grouping Conversations
    Friend Const Left_lblAcX As Long = 594           'ACCELERATOR X for Delete email
    Friend Const Left_lblAcR As Long = 624           'ACCELERATOR R for remove item from list
    Friend Const Left_lblAcT As Long = 330           'ACCELERATOR T for Task ... Flag item and make it a task
    Friend Const Left_lblAcO As Long = 50            'ACCELERATOR O for Open Email
    Friend Const Left_lblAcO_C As Long = 0            'ACCELERATOR O for Open Email
    Friend Const Width_frm As Long = 655
    Friend Const Width_lbl1 As Long = 54
    Friend Const Width_lbl2 As Long = 54
    Friend Const Width_lbl3 As Long = 54
    Friend Const Width_lbl4 As Long = 52
    Friend Const Width_lbl5 As Long = 78            'Folder:
    Friend Const Width_lblSender As Long = 138           '<SENDER>
    Friend Const Width_lblSender_C As Long = 174           '<SENDER> Compact view
    Friend Const Width_lblTriage As Long = 11            'X Triage placeholder
    Friend Const Width_lblActionable As Long = 72            '<ACTIONABL>
    Friend Const Width_lblSubject As Long = 294           '<SUBJECT>
    Friend Const Width_lblSubject_C As Long = 354           '<SUBJECT> Compact view
    Friend Const Width_lblBody As Long = 294           '<BODY>
    Friend Const Width_lblBody_C As Long = 354           '<BODY> Compact view
    Friend Const Width_lblSentOn As Long = 80            '<SENTON>
    Friend Const Width_lblConvCt As Long = 30            'Count of Conversation Members
    Friend Const Width_lblPos As Long = 20            'ACCELERATOR Email Position
    Friend Const Width_cbxFolder As Long = 276           'Combo box containing Folder Suggestions
    Friend Const Width_inpt As Long = 156           'Input for folder search
    Friend Const Width_chbxGPConv As Long = 96            'Checkbox to Group Conversations
    Friend Const Width_cb As Long = 25            'Command buttons for: Delete email, Remove mail from Processing, and Flag as Task
    Friend Const Width_lblAc As Long = 14            'ACCELERATOR Width
    Friend Const Width_lblAcF As Long = 14            'ACCELERATOR F for Folder Search
    Friend Const Width_lblAcD As Long = 14            'ACCELERATOR D for Folder Dropdown
    Friend Const Width_lblAcC As Long = 14            'ACCELERATOR C for Grouping Conversations
    Friend Const Width_lblAcX As Long = 14            'ACCELERATOR X for Delete email
    Friend Const Width_lblAcR As Long = 14            'ACCELERATOR R for remove item from list
    Friend Const Width_lblAcT As Long = 14            'ACCELERATOR T for Task ... Flag item and make it a task
    Friend Const Width_lblAcO As Long = 14            'ACCELERATOR O for Open Email
    Friend Const Height_UserForm As Long = 149          'Minimum height of Userform
    Friend Const Width_UserForm As Long = 699.75        'Minimum width of Userform
    Friend Const Width_PanelMain As Long = 683           'Minimum width of _viewer.L1v1L2_PanelMain
    'Frame Design Constants
    Friend Const frmHt = 96
    Friend Const frmWd = 655
    Friend Const frmLt = 12
    Friend Const frmSp = 6
    Friend Const OK_left As Long = 216
    Friend Const CANCEL_left As Long = 354
    Friend Const OK_width As Long = 120
    Friend Const UNDO_left As Long = 480
    Friend Const UNDO_width As Long = 42
    Friend Const spn_left As Long = 606
#End Region

End Module

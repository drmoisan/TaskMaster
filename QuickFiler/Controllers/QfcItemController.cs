using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using QuickFiler.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;
using UtilitiesVB;
using System.Windows.Forms;

namespace QuickFiler.Controllers
{
    internal class QfcItemController : IQfcItemController
    {
        public QfcItemController(IApplicationGlobals AppGlobals, 
                                 QfcItemViewer itemViewer,
                                 int viewerPosition,
                                 MailItem mailItem,
                                 IQfcCollectionController parent) 
        {
            _globals = AppGlobals;
            
            // Grab handle on viewer and controls
            _itemViewer = itemViewer;
            ResolveControlGroups(itemViewer);

            _viewerPosition = viewerPosition;   // visible position in collection (index is 1 less)
            _mailItem = mailItem;               // handle on underlying Email
            _parent = parent;                   // handle on collection controller

            // Populate placeholder controls with 
            PopulateControls(mailItem, viewerPosition);
            ToggleTips(IQfcTipsDetails.ToggleState.Off);

        }

        private IApplicationGlobals _globals;
        private QfcItemViewer _itemViewer;
        private IQfcCollectionController _parent;
        private IList<IQfcTipsDetails> _listTipsDetails;
        private MailItem _mailItem;
        private DataFrame _dfConversation;
        public DataFrame DfConversation { get { return _dfConversation; } }
        private int _viewerPosition;
        private FolderHandler _fldrHandler;
        private IList<Control> _controls;
        private Dictionary<Type, Control> _ctrlDict;
        private IList<TableLayoutPanel> _tlps;
        private IList<Button> _buttons;
        private IList<CheckBox> _checkBoxes;
        private IList<Label> _labels;

        internal string CompressPlainText(string text)
        {
            //text = text.Replace(System.Environment.NewLine, " ");
            text = text.Replace(Properties.Resources.Email_Prefix_To_Strip, "");
            text = Regex.Replace(text, @"<https://[^>]+>", " <link> "); //Strip links
            text = Regex.Replace(text, @"[\s]", " ");
            text = Regex.Replace(text, @"[ ]{2,}", " ");
            text = text.Trim();
            text += " <EOM>";
            return text;
        }

        internal void ResolveControlGroups(QfcItemViewer itemViewer)
        {
            var ctrls = itemViewer.GetAllChildren();
            _controls = ctrls.ToList();
            
            _ctrlDict = ctrls.Select(x => new KeyValuePair<Type, Control>(x.GetType(), x))
                             .ToDictionary();
            
            _listTipsDetails = _itemViewer.TipsLabels
                               .Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                               .ToList();


            _tlps = ctrls.Where(x => x is TableLayoutPanel)
                         .Select(x => (TableLayoutPanel)x)
                         .ToList();

            _buttons = ctrls.Where(x => x is Button)
                            .Select(x => (Button)x)
                            .ToList();

            _labels = ctrls.Where(x => (x is Label) && 
                                       (!itemViewer.TipsLabels.Contains(x)) &&
                                       (x != itemViewer.lblSubject) &&
                                       (x != itemViewer.LblSender))
                           .Select(x => (Label)x)
                           .ToList();

        }
        //    Type columnDataType = ColumnData[0].GetType();
        //    // Use reflection to create an instance of the PrimitiveDataFrameColumn<T> class with the correct type parameter
        //    Type columnType = typeof(PrimitiveDataFrameColumn<>).MakeGenericType(columnDataType);
        //    column = (DataFrameColumn)Activator.CreateInstance(columnType, ColumnName);
        //    for (int i = 0; i < ColumnData.Length; i++)
        //    {
        //        column[i] = Convert.ChangeType(ColumnData[i], columnDataType);
        //    }

        public void PopulateControls(MailItem mailItem, int viewerPosition)
        {
            _itemViewer.LblSender.Text = CaptureEmailDetailsModule.GetSenderName(mailItem);
            _itemViewer.lblSubject.Text = mailItem.Subject;
            if (_mailItem.UnRead == true)
            {
                _itemViewer.LblSender.Font = new Font(_itemViewer.LblSender.Font, FontStyle.Bold);
                _itemViewer.lblSubject.Font = new Font(_itemViewer.lblSubject.Font, FontStyle.Bold);
            }
            _itemViewer.TxtboxBody.Text = CompressPlainText(mailItem.Body);
            _itemViewer.LblTriage.Text = CaptureEmailDetailsModule.GetTriage(mailItem);
            _itemViewer.LblSentOn.Text = mailItem.SentOn.ToString("g");
            _itemViewer.LblActionable.Text = CaptureEmailDetailsModule.GetActionTaken(mailItem);
            _itemViewer.LblPos.Text = viewerPosition.ToString();
            //_itemViewer.LblConvCt.Text 
            //_itemViewer.LblSearch
            //_itemViewer.BtnDelItem
            //_itemViewer.BtnPopOut
            //_itemViewer.BtnFlagTask
            //_itemViewer.LblFolder
            //_itemViewer.CboFolders
            //_itemViewer.CbxConversation
            //_itemViewer.CbxEmailCopy
            //_itemViewer.CbxAttachments
        }

        /// <summary>
        /// TBD if this overload will be of use. Depends on whether _dfConversation
        /// is needed by any individual element when expanded
        /// </summary>
        /// <param name="df"></param>
        public void PopulateConversation(DataFrame df)
        {
            _dfConversation = df;
            _itemViewer.LblConvCt.Text = _dfConversation.Rows.Count.ToString();
        }

        /// <summary>
        /// Sets the conversation count of the visual without altering the
        /// _dfConversation. Usefull when expanding or collapsing the 
        /// conversation to show how many items will be moved
        /// </summary>
        /// <param name="countOnly"></param>
        public void PopulateConversation(int countOnly)
        {
            _itemViewer.LblConvCt.Text = countOnly.ToString();
        }

        /// <summary>
        /// Gets the Outlook.Conversation from the underlying MailItem
        /// embedded in the class. Conversation details are loaded to 
        /// a Dataframe. Count is inferred from the df rowcount
        /// </summary>
        public void PopulateConversation()
        {
            _dfConversation = _mailItem.GetConversationDf(true, true);
            int count = _dfConversation.Rows.Count();
            _itemViewer.LblConvCt.Text = count.ToString();
            if (count == 0) { _itemViewer.LblConvCt.BackColor = Color.Red; }
        }
        
        public void PopulateFolderCombobox(object varList = null)
        {
            if (varList is null) 
            { 
                _fldrHandler = new FolderHandler(
                    _globals, _mailItem, FolderHandler.Options.FromField); 
            }
            else 
            { 
                _fldrHandler = new FolderHandler(
                    _globals, varList, FolderHandler.Options.FromArrayOrString); 
            }

            _itemViewer.CboFolders.Items.AddRange(_fldrHandler.FolderList);
            _itemViewer.CboFolders.SelectedIndex = 1;
        }

        public bool BlExpanded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Height => throw new NotImplementedException();

        public bool BlHasChild { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public object ObjItem { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void ToggleTips()
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                tipsDetails.Toggle();
            }
        }
        public void ToggleTips(IQfcTipsDetails.ToggleState desiredState)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                tipsDetails.Toggle(desiredState);
            }
        }

        public void Accel_FocusToggle()
        {
            throw new NotImplementedException();
        }

        public void Accel_Toggle()
        {
            throw new NotImplementedException();
        }

        public void ApplyReadEmailFormat()
        {
            throw new NotImplementedException();
        }

        public void ctrlsRemove()
        {
            throw new NotImplementedException();
        }

        public void ExpandCtrls1()
        {
            throw new NotImplementedException();
        }

        public void FlagAsTask()
        {
            throw new NotImplementedException();
        }

        public void JumpToFolderDropDown()
        {
            throw new NotImplementedException();
        }

        public void JumpToSearchTextbox()
        {
            throw new NotImplementedException();
        }

        public void MarkItemForDeletion()
        {
            throw new NotImplementedException();
        }

        public void ToggleConversationCheckbox()
        {
            throw new NotImplementedException();
        }

        public void ToggleDeleteFlow()
        {
            throw new NotImplementedException();
        }

        public void ToggleSaveAttachments()
        {
            throw new NotImplementedException();
        }

        public void ToggleSaveCopyOfMail()
        {
            throw new NotImplementedException();
        }

        public void SetThemeDark()
        {
            foreach (TableLayoutPanel tlp in _tlps)
            {
                tlp.SetTheme(backColor: System.Drawing.Color.Black);
            }
            
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                tipsDetails.LabelControl.SetTheme(backColor: Color.LightSkyBlue, 
                                                  forecolor: SystemColors.ActiveCaptionText);
            }

            if (_mailItem.UnRead == true)
            {
                _itemViewer.LblSender.SetTheme(backColor: Color.Black,
                                               forecolor: Color.Goldenrod);
                _itemViewer.lblSubject.SetTheme(backColor: Color.Black,
                                                forecolor: Color.Goldenrod);
            }
            else
            {
                _itemViewer.LblSender.SetTheme(backColor: System.Drawing.Color.Black,
                                               forecolor: System.Drawing.Color.WhiteSmoke);
                _itemViewer.lblSubject.SetTheme(backColor: System.Drawing.Color.Black,
                                                forecolor: System.Drawing.Color.WhiteSmoke);
            }

            foreach (Button btn in _buttons)
            {
                btn.SetTheme(backColor: Color.DimGray);
            }

            // _itemViewer.LblAcOpen.BackColor = System.Drawing.Color.LightSkyBlue;
            // _itemViewer.LblAcOpen.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            //_itemViewer.LblAcOpen.Size = new System.Drawing.Size(44, 43);
            //_itemViewer.LblAcSearch.BackColor = System.Drawing.Color.LightSkyBlue;
            //_itemViewer.LblAcSearch.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            _itemViewer.TxtboxSearch.BackColor = Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            _itemViewer.TxtboxSearch.ForeColor = Color.WhiteSmoke;

            _itemViewer.TxtboxBody.BackColor = Color.Black;
            _itemViewer.TxtboxBody.ForeColor = Color.WhiteSmoke;
            //_itemViewer.LblAcDelete.BackColor = System.Drawing.Color.LightSkyBlue;
            //_itemViewer.LblAcDelete.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            //_itemViewer.BtnDelItem.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            //_itemViewer.BtnDelItem.ForeColor = System.Drawing.SystemColors.HotTrack;
            //_itemViewer.BtnDelItem.UseVisualStyleBackColor = false;
            //_itemViewer.LblAcPopOut.BackColor = System.Drawing.Color.LightSkyBlue;
            //_itemViewer.LblAcPopOut.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            //_itemViewer.BtnPopOut.BackColor = System.Drawing.Color.DimGray;
            //_itemViewer.BtnPopOut.UseVisualStyleBackColor = false;
            //_itemViewer.LblAcTask.BackColor = System.Drawing.Color.LightSkyBlue;
            //_itemViewer.LblAcTask.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            //_itemViewer.BtnFlagTask.BackColor = System.Drawing.Color.DimGray;
            //_itemViewer.BtnFlagTask.ForeColor = System.Drawing.Color.DimGray;
            //_itemViewer.BtnFlagTask.UseVisualStyleBackColor = false;
            //_itemViewer.LblAcFolder.BackColor = System.Drawing.Color.LightSkyBlue;
            //_itemViewer.LblAcFolder.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            _itemViewer.CboFolders.BackColor = System.Drawing.Color.DimGray;
            _itemViewer.CboFolders.ForeColor = System.Drawing.Color.WhiteSmoke;
            //_itemViewer.LblAcAttachments.BackColor = System.Drawing.Color.LightSkyBlue;
            //_itemViewer.LblAcAttachments.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            //_itemViewer.LblAcEmail.BackColor = System.Drawing.Color.LightSkyBlue;
            //_itemViewer.LblAcEmail.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            //_itemViewer.LblAcConversation.BackColor = System.Drawing.Color.LightSkyBlue;
            //_itemViewer.LblAcConversation.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            _itemViewer.BackColor = System.Drawing.Color.Black;
            _itemViewer.ForeColor = System.Drawing.Color.WhiteSmoke;
        }

        
        public void SetThemeLight()
        {
            foreach (TableLayoutPanel tlp in _tlps)
            {
                tlp.SetTheme(backColor: SystemColors.Control);
            }

            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                tipsDetails.LabelControl.SetTheme(backColor: Color.Black,
                                                  forecolor: Color.White);
            }

            if (_mailItem.UnRead == true)
            {
                _itemViewer.LblSender.SetTheme(backColor: SystemColors.Control,
                                               forecolor: Color.MediumBlue);
                _itemViewer.lblSubject.SetTheme(backColor: SystemColors.Control,
                                                forecolor: Color.MediumBlue);
            }
            else
            {
                _itemViewer.LblSender.SetTheme(backColor: SystemColors.Control,
                                               forecolor: SystemColors.ControlText);
                _itemViewer.lblSubject.SetTheme(backColor: SystemColors.Control,
                                                forecolor: SystemColors.ControlText);
            }

            foreach (Button btn in _buttons)
            {
                btn.SetTheme(backColor: SystemColors.Control);
            }



            //_itemViewer.L1h0L2hv.BackColor = System.Drawing.SystemColors.ControlLightLight;
            //_itemViewer.LblSender.ForeColor = System.Drawing.Color.MediumBlue;
            //_itemViewer.lblSubject.ForeColor = System.Drawing.Color.MediumBlue;
            _itemViewer.TxtboxBody.BackColor = SystemColors.Control;
            _itemViewer.TxtboxBody.ForeColor = SystemColors.ControlText;
            //_itemViewer.LblAcOpen.BackColor = System.Drawing.SystemColors.ControlText;
            //_itemViewer.LblAcOpen.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            //_itemViewer.LblAcOpen.Size = new System.Drawing.Size(44, 40);
            //_itemViewer.LblAcSearch.BackColor = System.Drawing.SystemColors.ControlText;
            //_itemViewer.LblAcSearch.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            _itemViewer.TxtboxSearch.BackColor = SystemColors.Control;
            _itemViewer.TxtboxSearch.ForeColor = SystemColors.ControlText;
            //_itemViewer.LblAcDelete.BackColor = System.Drawing.SystemColors.ControlText;
            //_itemViewer.LblAcDelete.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            //_itemViewer.BtnDelItem.BackColor = System.Drawing.SystemColors.Control;
            //_itemViewer.BtnDelItem.ForeColor = System.Drawing.SystemColors.ControlText;
            //_itemViewer.BtnDelItem.UseVisualStyleBackColor = true;
            //_itemViewer.LblAcPopOut.BackColor = System.Drawing.SystemColors.ControlText;
            //_itemViewer.LblAcPopOut.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            //_itemViewer.BtnPopOut.BackColor = System.Drawing.SystemColors.ControlLightLight;
            //_itemViewer.BtnPopOut.UseVisualStyleBackColor = true;
            //_itemViewer.LblAcTask.BackColor = System.Drawing.SystemColors.ControlText;
            //_itemViewer.LblAcTask.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            //_itemViewer.BtnFlagTask.BackColor = System.Drawing.SystemColors.ControlLightLight;
            //_itemViewer.BtnFlagTask.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            //_itemViewer.BtnFlagTask.UseVisualStyleBackColor = true;
            //_itemViewer.LblAcFolder.BackColor = System.Drawing.SystemColors.ControlText;
            //_itemViewer.LblAcFolder.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            _itemViewer.CboFolders.BackColor = System.Drawing.SystemColors.ControlLightLight;
            _itemViewer.CboFolders.ForeColor = System.Drawing.SystemColors.ControlText;
            //_itemViewer.LblAcAttachments.BackColor = System.Drawing.SystemColors.ControlText;
            //_itemViewer.LblAcAttachments.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            //_itemViewer.LblAcEmail.BackColor = System.Drawing.SystemColors.ControlText;
            //_itemViewer.LblAcEmail.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            //_itemViewer.LblAcConversation.BackColor = System.Drawing.SystemColors.ControlText;
            //_itemViewer.LblAcConversation.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            _itemViewer.BackColor = System.Drawing.SystemColors.ControlLightLight;
            _itemViewer.ForeColor = System.Drawing.SystemColors.ControlText;
        }

        public void Cleanup()
        {
            _globals = null;
            _itemViewer = null;
            _parent = null;
            _listTipsDetails = null;
            _mailItem = null;
            _dfConversation = null;
            _fldrHandler = null;
        }
    }
}

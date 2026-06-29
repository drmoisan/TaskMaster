using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Viewers;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.Extensions;

namespace QuickFiler.Controllers
{
    internal partial class QfcItemController
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal void CbxConversation_CheckedChanged(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            //TraceUtility.LogMethodCall(sender, e);

            _optionConversationChecked = _itemViewer.ConversationModeChecked;
            if (!SuppressEvents)
            {
                if (_optionConversationChecked)
                {
                    CollapseConversation();
                }
                else
                {
                    EnumerateConversation();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal void BtnFlagTask_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            FlagAsTask();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal async void BtnPopOut_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            await _parent.PopOutControlGroupAsync(ItemNumber);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal void BtnDelItem_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            MarkItemForDeletion();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal async void BtnReply_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            await Reply();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal async void BtnReplyAll_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            await ReplyAll();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal async void BtnForward_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            await Forward();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal async void TxtboxBody_DoubleClick(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );
            await Task.Run(() => Mail.Display());
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void Button_MouseEnter(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = _themes[_activeTheme].ButtonMouseOverColor;
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void MenuItem_MouseEnter(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).BackColor = _themes[_activeTheme].ButtonMouseOverColor;
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void Button_MouseLeave(object sender, EventArgs e)
        {
            if (((Button)sender).DialogResult == DialogResult.OK)
            {
                ((Button)sender).BackColor = _themes[_activeTheme].ButtonClickedColor;
            }
            else
            {
                ((Button)sender).BackColor = _themes[_activeTheme].ButtonBackColor;
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void MenuItem_MouseLeave(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).BackColor = _themes[_activeTheme].ButtonBackColor;
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            var folders = _folderHandler.FindFolder(
                searchString: "*" + _itemViewer.SearchText + "*",
                reloadCTFStagingFiles: false,
                recalcSuggestions: false,
                objItem: Mail
            );
            _itemViewer.ClearFolderItems();
            _itemViewer.SetFolderItems(folders);

            if (folders.Length >= 2)
                _itemViewer.SetFolderSelectedIndex(1);
            _itemViewer.SetFolderDroppedDown(true);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                _itemViewer.SetFolderDroppedDown(true);
                _itemViewer.FocusFolderDropDown();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void TopicThread_ItemSelectionChanged(
            object sender,
            ListViewItemSelectionChangedEventArgs e
        )
        {
            var objects = _itemViewer.GetSelectedConversationItems();
            if ((objects is not null) && (objects.Count != 0))
            {
                var info = objects[0] as MailItemHelper;
                _itemViewer.NavigateToString(info.Html);
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void CbxEmailCopy_CheckedChanged(object sender, EventArgs e)
        {
            _optionEmailCopy = _itemViewer.EmailCopyChecked;
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void CboFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedFolder = _itemViewer.GetSelectedFolder();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void CbxAttachments_CheckedChanged(object sender, EventArgs e)
        {
            _optionAttachments = _itemViewer.AttachmentsChecked;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Properties;
using QuickFiler.Viewers;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Interfaces.IWinForm;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers
{
    internal partial class EfcFormController
    {
        #region Setup and Cleanup Methods

        internal void CaptureConfigureItemViewer()
        {
            var explorerSize = _globals.Ol.GetExplorerScreenSize();
            _tlpHeightExpanded = (int)Math.Round(_itemTlp.RowStyles[1].Height, 0);
            var heightDiff = _tlpHeightExpanded - _itemViewer.Height;
            _tlpHeightCollapsed = _itemViewer.MinimumSize.Height + heightDiff;
            _tlpHeightDiff = _tlpHeightExpanded - _tlpHeightCollapsed;
            _itemViewerTlpRow = _itemTlp.GetPositionFromControl(_itemViewer).Row;
            ToggleExpansionStyle(Enums.ToggleState.Off);
            var itemTlpRows = _itemViewer.L0vh_Tlp.RowStyles.Cast<RowStyle>().Take(5);
            var bodyRow = itemTlpRows.ElementAt(4);
            var bodyRowHeight =
                _tlpHeightCollapsed
                - itemTlpRows.Select(x => x.Height).Sum(x => x)
                + bodyRow.Height;
            bodyRow.Height = bodyRowHeight;
            _formViewer.MinimumSize = new Size(
                (int)(explorerSize.Width * 0.75),
                (int)(explorerSize.Height * 0.75)
            );
            _formViewer.Size = _formViewer.MinimumSize;
        }

        /// <summary>Releases collaborators. Safe on a partial controller and idempotent.</summary>
        public void Cleanup()
        {
            // Detach before nulling: release the subscription before dropping its owner.
            var globals = _globals;
            if (globals?.Ol is not null)
            {
                globals.Ol.PropertyChanged -= DarkMode_Changed;
            }
            _globals = null;
            _formViewer = null;
            _dataModel = null;

            // Clearing before invoking is what makes the single invocation structural.
            var parentCleanup = _parentCleanup;
            _parentCleanup = null;
            if (parentCleanup is not null)
            {
                parentCleanup.Invoke();
            }
        }

        public void ConfigureFind()
        {
            if (_initType.HasFlag(QfEnums.InitTypeEnum.Find))
            {
                _formViewer.Text = "Quick Filer - Find Folder";
                _formViewer.Ok.Text = "Open Outlook Folder";
                _formViewer.NewFolder.Text = "Open File System Folder";
            }
        }

        internal void ResolveControlGroups()
        {
            _listTipsDetails = _formViewer
                .TipsLabels.Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                .ToList();
            _listTipsDetails.ForEach(x => x.Toggle(Enums.ToggleState.Off, true));

            var starter = _formViewer.GetAllChildren(except: new List<Control> { _itemViewer });

            _listButtons = starter.Where(x => x is Button).Cast<Button>().ToList();

            _listCheckBox = starter.Where(x => (x is CheckBox)).ToList();

            _listHighlighted = new List<Control>
            {
                _formViewer.SearchText,
                _formViewer.FolderListBox,
            };

            _listDefault = starter
                .Where(x =>
                    !_formViewer.TipsLabels.Contains(x)
                    && !_listButtons.Contains(x)
                    && !_listHighlighted.Contains(x)
                    && !_listCheckBox.Contains(x)
                )
                .ToList();
        }

        internal void SetupThemes()
        {
            _themes = EfcThemeHelper.SetupFormThemes(
                _formViewer.TipsLabels.Cast<Control>().ToList(),
                _listHighlighted,
                _listDefault,
                _listButtons.Cast<Control>().ToList(),
                _listCheckBox
            );

            _activeTheme = LoadTheme();
        }

        #endregion Setup and Cleanup Methods

        #region Public Properties

        private string _activeTheme;
        public string ActiveTheme
        {
            // GetOrLoad throws under strict: true once _themes is null, so test at the call
            // site and return the backing field on the torn-down path.
            get =>
                _themes is null
                    ? _activeTheme
                    : Initializer.GetOrLoad(ref _activeTheme, LoadTheme, strict: true, _themes);
            set =>
                Initializer.SetAndSave<string>(
                    ref _activeTheme,
                    value,
                    (x) => _themes[x].SetTheme(async: true)
                );
        }

        internal string LoadTheme()
        {
            var activeTheme = DarkMode ? "DarkNormal" : "LightNormal";
            if (_themes is not null && _themes.ContainsKey(activeTheme))
            {
                _themes[activeTheme].SetTheme();
            }
            return activeTheme;
        }

        private bool _darkMode;
        public bool DarkMode
        {
            // The params object[] dependency array is materialised before GetOrLoad is entered,
            // so _globals.Ol must be tested at the call site or the null path still dereferences.
            get =>
                _globals?.Ol is null
                    ? _darkMode
                    : Initializer.GetOrLoad(
                        ref _darkMode,
                        () => _globals.Ol.DarkMode,
                        false,
                        _globals,
                        _globals.Ol
                    );
            set => Initializer.SetAndSave(ref _darkMode, value, (x) => _globals.Ol.DarkMode = x);
        }

        public IntPtr FormHandle => _formViewer.Handle;

        public string SelectedFolder
        {
            // Derived from the bridge router's selection tracking. IsValidSelection routes to
            // IsSelectableFolder, which composes IsBannerRow, matching the producers' "===="
            // prefix, with the guard's deliberately broader three-character rejection.
            get => _router?.SelectedFolderPath;
        }

        private bool _saveAttachments;
        public bool SaveAttachments
        {
            get => _saveAttachments;
            set
            {
                _saveAttachments = value;
                // Should be set elsewhere as a user default
                //Settings.Default.SaveAttachments = value;
            }
        }

        private bool _saveEmail;
        public bool SaveEmail
        {
            get => _saveEmail;
            set
            {
                _saveEmail = value;
                // Should be set elsewhere as a user default
                //Settings.Default.SaveEmail = value;
            }
        }

        private bool _savePictures;
        public bool SavePictures
        {
            get => _savePictures;
            set
            {
                _savePictures = value;
                // Should be set elsewhere as a user default
                //Settings.Default.SavePictures = value;
            }
        }

        private bool _moveConversation;
        public bool MoveConversation
        {
            get => _moveConversation;
            set
            {
                _moveConversation = value;
                // Should be set elsewhere as a user default
                //Settings.Default.MoveConversation = value;
            }
        }

        private CancellationToken _token;
        public CancellationToken Token
        {
            get => _token;
            set => _token = value;
        }

        #endregion
    }
}

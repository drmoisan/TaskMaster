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
        public void ToggleFocus(Enums.ToggleState desiredState)
        {
            _itemViewer.Invoke(
                new System.Action(() =>
                {
                    if ((desiredState == Enums.ToggleState.On) && (!_activeUI))
                    {
                        // If not active and we want to turn on, then we are turning on
                        _activeUI = true;
                        if (_activeTheme.Contains("Dark"))
                        {
                            _activeTheme = "DarkActive";
                        }
                        else
                        {
                            _activeTheme = "LightActive";
                        }
                        ToggleTips(async: false, desiredState: Enums.ToggleState.On);
                        //RegisterFocusActions();
                        RegisterFocusAsyncActions();
                    }
                    else if ((desiredState == Enums.ToggleState.Off) && (_activeUI))
                    {
                        // If active and we want to turn off, then we are turning off
                        _activeUI = false;
                        if (_activeTheme.Contains("Dark"))
                        {
                            _activeTheme = "DarkNormal";
                        }
                        else
                        {
                            _activeTheme = "LightNormal";
                        }
                        ToggleTips(async: false, desiredState: Enums.ToggleState.Off);
                        //UnregisterFocusActions();
                        UnregisterFocusAsyncActions();
                    }
                    _themes[_activeTheme].SetQfcTheme(async: false);
                })
            );
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task ToggleFocusAsync(Enums.ToggleState desiredState)
        {
            var boolDesiredState = desiredState.HasFlag(Enums.ToggleState.On);
            if (_activeUI && !boolDesiredState)
            {
                await ToggleFocusOffAsync();
            }
            else if (!_activeUI && boolDesiredState)
            {
                await ToggleFocusOnAsync();
            }
            await _themes[_activeTheme].SetQfcThemeAsync();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void ToggleFocus()
        {
            _itemViewer.Invoke(
                new System.Action(() =>
                {
                    if (_activeUI)
                    {
                        // If active, then we are turning off
                        _activeUI = false;
                        if (_activeTheme.Contains("Dark"))
                        {
                            _activeTheme = "DarkNormal";
                        }
                        else
                        {
                            _activeTheme = "LightNormal";
                        }
                        ToggleTips(async: false, desiredState: Enums.ToggleState.Off);
                        //UnregisterFocusActions();
                        UnregisterFocusAsyncActions();
                    }
                    else
                    {
                        // If not active, then we are turning on
                        _activeUI = true;
                        if (_activeTheme.Contains("Dark"))
                        {
                            _activeTheme = "DarkActive";
                        }
                        else
                        {
                            _activeTheme = "LightActive";
                        }
                        ToggleTips(async: false, desiredState: Enums.ToggleState.On);
                        //RegisterFocusActions();
                        RegisterFocusAsyncActions();
                    }
                    _themes[_activeTheme].SetQfcTheme(async: false);
                })
            );
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task ToggleFocusAsync()
        {
            if (_activeUI)
            {
                await ToggleFocusOffAsync();
            }
            else
            {
                await ToggleFocusOnAsync();
            }
            await _themes[_activeTheme].SetQfcThemeAsync();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private async Task ToggleFocusOnAsync()
        {
            _activeUI = true;
            if (_activeTheme.Contains("Dark"))
            {
                _activeTheme = "DarkActive";
            }
            else
            {
                _activeTheme = "LightActive";
            }
            await ToggleTipsAsync(desiredState: Enums.ToggleState.On);
            RegisterFocusAsyncActions();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private async Task ToggleFocusOffAsync()
        {
            _activeUI = false;
            if (_activeTheme.Contains("Dark"))
            {
                _activeTheme = "DarkNormal";
            }
            else
            {
                _activeTheme = "LightNormal";
            }
            await ToggleTipsAsync(desiredState: Enums.ToggleState.Off);
            UnregisterFocusAsyncActions();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void ToggleNavigation(bool async)
        {
            _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(false)));
            if (async)
            {
                _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(false)));
            }
            else
            {
                _itemViewer.Invoke(new System.Action(() => _itemPositionTips.Toggle(false)));
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void ToggleNavigation(bool async, Enums.ToggleState desiredState)
        {
            if (async)
            {
                _itemViewer.BeginInvoke(
                    new System.Action(() => _itemPositionTips.Toggle(desiredState, false))
                );
            }
            else
            {
                _itemViewer.Invoke(
                    new System.Action(() => _itemPositionTips.Toggle(desiredState, false))
                );
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task ToggleNavigationAsync(Enums.ToggleState desiredState)
        {
            await _itemPositionTips.ToggleAsync(desiredState, false);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void ToggleTips(bool async, Enums.ToggleState desiredState)
        {
            InvokeBeginInvoke(
                async,
                new System.Action(() =>
                {
                    _tableLayoutPanels.ForEach(x => x.SuspendLayout());
                    ListTipsDetails.ForEach(x => x.Toggle(desiredState, shareColumn: false));
                    if (_expanded || desiredState.HasFlag(Enums.ToggleState.Force))
                    {
                        ListTipsExpanded.ForEach(x => x.Toggle(desiredState, shareColumn: false));
                    }
                    _tableLayoutPanels.ForEach(x => x.ResumeLayout());
                })
            );
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task ToggleTipsAsync(Enums.ToggleState desiredState)
        {
            //TraceUtility.LogMethodCall(desiredState);

            Token.ThrowIfCancellationRequested();

            //List<Task> tasks = new List<Task>();
            //tasks.Add(ListTipsDetails.ToAsyncEnumerable().ForEachAsync(async x => await x.ToggleAsync(desiredState, shareColumn: true)));

            foreach (var tip in ListTipsDetails)
            {
                await tip.ToggleAsync(desiredState, shareColumn: false);
            }
            //await ListTipsExpanded.ToAsyncEnumerable().ForEachAsync(async x => await x.ToggleAsync(desiredState, shareColumn: true));
            //var tasks = ListTipsExpanded.Select(x => x.ToggleAsync(desiredState, shareColumn: true));
            //ListTipsExpanded.ForEach(async x => await x.ToggleAsync(desiredState, shareColumn: true));

            if (_expanded || desiredState.HasFlag(Enums.ToggleState.Force))
            {
                foreach (var tip in ListTipsExpanded)
                {
                    await tip.ToggleAsync(desiredState, shareColumn: false);
                }
                //await ListTipsExpanded.ToAsyncEnumerable().ForEachAsync(async x => await x.ToggleAsync(desiredState, shareColumn: true));
                //tasks = tasks.Concat(ListTipsExpanded.Select(x => x.ToggleAsync(desiredState, shareColumn: true)));
                //ListTipsExpanded.ForEach(async x => await x.ToggleAsync(desiredState, shareColumn: true));
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void InvokeBeginInvoke(bool async, System.Action action)
        {
            if (async)
            {
                _itemViewer.BeginInvoke(action);
            }
            else
            {
                _itemViewer.Invoke(action);
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void ToggleSaveAttachments()
        {
            // Connect method to new menu
            //_itemViewer.CbxAttachments.Invoke(new System.Action(() =>
            //    _itemViewer.CbxAttachments.Checked =
            //    !_itemViewer.CbxAttachments.Checked));
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void ToggleSaveCopyOfMail()
        {
            UiThread.Dispatcher.Invoke(() =>
                _itemViewer.EmailCopyChecked = !_itemViewer.EmailCopyChecked
            );
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void SetThemeDark(bool async)
        {
            if ((_activeTheme is null) || _activeTheme.Contains("Normal"))
            {
                _themes["DarkNormal"].SetQfcTheme(async);
                _activeTheme = "DarkNormal";
            }
            else
            {
                _themes["DarkActive"].SetQfcTheme(async);
                _activeTheme = "DarkActive";
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void HtmlDarkConverter(Enums.ToggleState desiredState)
        {
            if (_isWebViewerInitialized)
            {
                _itemViewer.NavigateToString(ItemHelper.ToggleDark(desiredState));
                if (ConversationResolver.Count.Expanded > 0)
                {
                    ConversationResolver.ConversationInfo.Expanded.ForEach(item =>
                        item.ToggleDark(desiredState)
                    );
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void SetThemeLight(bool async)
        {
            if ((_activeTheme is null) || _activeTheme.Contains("Normal"))
            {
                _themes["LightNormal"].SetQfcTheme(async);
                _activeTheme = "LightNormal";
            }
            else
            {
                _themes["LightActive"].SetQfcTheme(async);
                _activeTheme = "LightActive";
            }
            //_isDarkMode = false;
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public void ApplyReadEmailFormat(object state)
        {
            ItemHelper.UnRead = false;
            _themes[_activeTheme].SetMailRead(async: true);
            Mail.UnRead = false;
            Mail.Save();
        }
    }
}

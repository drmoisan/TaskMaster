using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;

namespace Tags
{
    [ExcludeFromCodeCoverage]
    public class TagLauncher
    {
        public TagLauncher(IEnumerable<string> options, IApplicationGlobals appGlobals)
        {
            _globals = appGlobals;
            _viewer = new TagViewer();
            var dictOptions = options
                .Select(option => new KeyValuePair<string, bool>(option, false))
                .ToSortedDictionary();
            _controller = new TagController(
                _viewer,
                dictOptions,
                GetAutoAssign(),
                _globals.TD.PrefixList,
                _globals.Ol.UserEmailAddress
            );
        }

        public TagLauncher(IEnumerable<string> options, IPrefix prefix, string userEmail)
        {
            _viewer = new TagViewer();
            var dictOptions = options
                .Select(option => new KeyValuePair<string, bool>(option, false))
                .ToSortedDictionary();
            if (prefix is null)
            {
                _controller = new TagController(_viewer, dictOptions, null, null, userEmail);
            }
            else
            {
                var prefixList = new List<IPrefix> { prefix };
                _controller = new TagController(
                    _viewer,
                    dictOptions,
                    null,
                    prefixList,
                    userEmail,
                    prefixKey: prefix.Key
                );
            }
        }

        public static List<string> LaunchAndSelect(
            IEnumerable<string> options,
            IApplicationGlobals appGlobals
        )
        {
            var launcher = new TagLauncher(options, appGlobals);
            launcher.Viewer.ShowDialog();
            if (launcher.Controller.ExitType != "Cancel")
            {
                return launcher.Controller.SelectionAsList();
            }
            else
            {
                return [];
            }
        }

        public static string LaunchAndFindMatch(
            IEnumerable<string> options,
            IPrefix prefix,
            string userEmail,
            string searchString
        )
        {
            var launcher = new TagLauncher(options, prefix, userEmail);
            launcher.Viewer.Controls.Remove(launcher.Viewer.ButtonNew);
            return launcher.FindMatch(searchString);
        }

        public string FindMatch(string searchString)
        {
            _controller.SetSearchText(searchString);
            _viewer.ShowDialog();
            if (_controller.ExitType != "Cancel")
            {
                return _controller.SelectionAsString();
            }
            return "";
        }

        private IApplicationGlobals _globals;

        private TagViewer _viewer;
        public TagViewer Viewer
        {
            get => _viewer;
            set => _viewer = value;
        }

        private TagController _controller;
        public TagController Controller
        {
            get => _controller;
            set => _controller = value;
        }

        public IAutoAssign GetAutoAssign()
        {
            var autoAssign = new LauncherAutoAssign();
            autoAssign.FilterList = new List<string>();
            autoAssign.AddChoicesToDictDelegate = _globals.TD.People.AddMissingEntries;
            autoAssign.AddColorCategoryDelegate = (IPrefix prefix, string categoryName) =>
                CreateCategoryModule.CreateCategory(
                    olNS: _globals.Ol.NamespaceMAPI,
                    prefix: prefix,
                    newCatName: categoryName
                );

            autoAssign.AutoFindDelegate = (object objItem) =>
            {
                var helper = GetHelper(objItem);
                if (helper is null)
                {
                    return [];
                }
                return AutoFile.AutoFindPeople(helper, _globals.TD.People, true, false);
                //return AutoFile2.AutoFindPeople(
                //    objItem: objItem,
                //    ppl_dict: _globals.TD.People,
                //    emailRootFolder: _globals.Ol.InboxPath,
                //    dictRemap: _globals.TD.DictRemap,
                //    userAddress: _globals.Ol.UserEmailAddress,
                //    blExcludeFlagged: false);
            };

            return autoAssign;
        }

        private MailItemHelper GetHelper(object objItem)
        {
            if (objItem is MailItem mailItem)
            {
                return new MailItemHelper(mailItem, _globals);
            }
            else if (
                objItem is IOutlookItem olItem
                && olItem.GetOlItemType() == OlItemType.olMailItem
            )
            {
                return new MailItemHelper(olItem.InnerObject as MailItem, _globals);
            }
            else if (objItem is MailItemHelper)
            {
                return objItem as MailItemHelper;
            }
            else
            {
                return default;
            }
        }
    }
}

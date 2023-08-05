using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;

namespace QuickFiler.Helper_Classes
{
    internal class ConversationResolver
    {
        public ConversationResolver(IApplicationGlobals appGlobals,
                                    MailItem mailItem,
                                    System.Action<List<MailItemInfo>> updateUI = null)
        {
            _globals = appGlobals;
            _mailItem = mailItem;
        }
        
        private IApplicationGlobals _globals;
        private MailItem _mailItem;
        private System.Action<List<MailItemInfo>> _updateUI;

        private List<MailItemInfo> _conversationInfoExpanded;
        public List<MailItemInfo> ConversationInfoExpanded { get => _conversationInfoExpanded; set => _conversationInfoExpanded = value; }

        private IList<MailItem> _conversationItems;
        public IList<MailItem> ConversationItems
        {
            get
            {
                if (_conversationItems is null)
                {
                    _conversationItems = ResolveItems(DfConversation).Result;
                }
                return _conversationItems;
            }

            set => _conversationItems = value;
        }

        private IList<MailItem> _conversationItemsExpanded;
        public IList<MailItem> ConversationItemsExpanded
        {
            get
            {
                if (_conversationItemsExpanded is null)
                {
                    _conversationItemsExpanded = ResolveItems(DfConversationExpanded).Result;
                }
                return _conversationItemsExpanded;
            }

            set => _conversationItemsExpanded = value;
        }

        async internal Task<List<MailItem>> ResolveItems(DataFrame df)
        {
            return await Task.Run(() =>
            {
                return ConvHelper.GetMailItemList(df, ((Folder)_mailItem.Parent).StoreID, _globals.Ol.App, true)
                                 .Cast<MailItem>()
                                 .ToList();
            });
        }

        async internal Task<List<MailItem>> ResolveItems()
        {
            var df = await Task.Run(() =>
            {
                return _mailItem.GetConversation().GetConversationDf().FilterConversation(true, true);
            });
            return await ResolveItems(df);
        }

        private DataFrame _dfConversation;
        public DataFrame DfConversation
        {
            get
            {
                if ((_dfConversation is null) && (_mailItem is not null))
                {
                    var conversation = _mailItem.GetConversation();
                    DfConversationExpanded = conversation.GetConversationDf().FilterConversation(false, true);
                    DfConversation = DfConversationExpanded.FilterConversation(true, true);
                }
                return _dfConversation;
            }
            internal set
            {
                _dfConversation = value;
                NotifyPropertyChanged();
            }
        }

        private DataFrame _dfConversationExpanded;
        public DataFrame DfConversationExpanded
        {
            get
            {
                if ((_dfConversationExpanded is null) && (_mailItem is not null))
                {
                    var conversation = _mailItem.GetConversation();
                    DfConversationExpanded = conversation.GetConversationDf().FilterConversation(false, true);
                    DfConversation = DfConversationExpanded.FilterConversation(true, true);
                }
                return _dfConversationExpanded;
            }
            internal set
            {
                _dfConversationExpanded = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                NotifyPropertyChanged();
            }
        }

        #region INotifyPropertyChanged implementation

        protected void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged is not null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Handler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DfConversationExpanded))
            {
                _ = GetConversationInfoAsync().ConfigureAwait(false);
            }
        }

        internal async Task GetConversationInfoAsync()
        {
            var olNs = _globals.Ol.App.GetNamespace("MAPI");
            DataFrame df = DfConversationExpanded;

            // Initialize the ConversationInfo list from the Dataframe with Synchronous code
            ConversationInfoExpanded = Enumerable.Range(0, df.Rows.Count())
                                         .Select(indexRow => new MailItemInfo(df, indexRow))
                                         .OrderByDescending(itemInfo => itemInfo.ConversationIndex)
                                         .ToList();

            if (_updateUI is not null)
                await Task.Run(()=>_updateUI(ConversationInfoExpanded));
            
            // Run the async code in parallel to resolve the mailitem and load extended properties
            ConversationItems = Task.WhenAll(ConversationInfoExpanded.Select(async itemInfo =>
                                    {
                                        await itemInfo.LoadAsync(olNs, _isDarkMode)
                                                      .ConfigureAwait(false);
                                        return itemInfo.Item;
                                    }))
                                    .Result
                                    .ToList();
        }

        #endregion

    }
}

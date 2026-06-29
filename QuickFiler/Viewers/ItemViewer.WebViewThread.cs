using System;
using System.Collections;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace QuickFiler
{
    // Forwarding implementations for the narrowed IItemViewer WebView and topic-thread intent members
    // (Seam D, Cluster 2d). Each member forwards to the underlying Designer-backed WebView2 /
    // FastObjectListView / MoveOptionsMenu controls. The SentDate sort column dependency is
    // encapsulated inside SortConversationByDate. The whole ItemViewer type is
    // [ExcludeFromCodeCoverage] via its primary partial in ItemViewer.cs.
    public partial class ItemViewer
    {
        public void NavigateToString(string html) => L0v2h2_WebView2.NavigateToString(html);

        public event EventHandler<CoreWebView2InitializationCompletedEventArgs> WebViewInitializationCompleted
        {
            add => L0v2h2_WebView2.CoreWebView2InitializationCompleted += value;
            remove => L0v2h2_WebView2.CoreWebView2InitializationCompleted -= value;
        }

        public void SetConversationItems(IList items) => TopicThread.SetObjects(items);

        public void SortConversationByDate(SortOrder order) => TopicThread.Sort(SentDate, order);

        public IList GetSelectedConversationItems() => TopicThread.SelectedObjects;

        public event ListViewItemSelectionChangedEventHandler ConversationItemSelectionChanged
        {
            add => TopicThread.ItemSelectionChanged += value;
            remove => TopicThread.ItemSelectionChanged -= value;
        }

        public void ShowMoveOptionsMenu() => MoveOptionsMenu.ShowDropDown();
    }
}

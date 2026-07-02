using Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Interfaces
{
    /// <summary>
    /// Narrow Outlook COM seam (research §3.4.3) scoped to exactly the operations
    /// <c>QfcItemController</c> performs directly on its underlying <see cref="MailItem"/>. Abstracting
    /// only these members (rather than the whole COM surface) lets the previously COM-bound controller
    /// methods be unit-tested with a mock. Production is served by <see cref="MailItemActionsAdapter"/>,
    /// which forwards 1:1 to a live <see cref="MailItem"/>.
    /// </summary>
    public interface IMailItemActions
    {
        /// <summary>Creates a reply to the underlying mail item.</summary>
        MailItem Reply();

        /// <summary>Creates a reply-all to the underlying mail item.</summary>
        MailItem ReplyAll();

        /// <summary>Creates a forward of the underlying mail item.</summary>
        MailItem Forward();

        /// <summary>Displays the underlying mail item.</summary>
        void Display();

        /// <summary>Gets or sets the unread state of the underlying mail item.</summary>
        bool UnRead { get; set; }

        /// <summary>Saves the underlying mail item.</summary>
        void Save();

        /// <summary>Gets the Outlook EntryID of the underlying mail item.</summary>
        string EntryID { get; }
    }
}

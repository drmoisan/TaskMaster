using System.Diagnostics.CodeAnalysis;
using Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Interfaces
{
    /// <summary>
    /// Production adapter (DI-seam "adapter" tier, research §3.4.3) that forwards every
    /// <see cref="IMailItemActions"/> member 1:1 to a live <see cref="MailItem"/>. The body is a thin
    /// forwarding shim over an Outlook COM object, so it legitimately carries
    /// <see cref="ExcludeFromCodeCoverage"/>; the isolated forwards exist precisely so that the
    /// controller methods that previously called <c>Mail.*</c> directly become unit-testable.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class MailItemActionsAdapter : IMailItemActions
    {
        private readonly MailItem _mail;

        /// <summary>Wraps the supplied live <paramref name="mail"/> item.</summary>
        public MailItemActionsAdapter(MailItem mail)
        {
            _mail = mail;
        }

        /// <inheritdoc />
        public MailItem Reply() => _mail.Reply();

        /// <inheritdoc />
        public MailItem ReplyAll() => _mail.ReplyAll();

        /// <inheritdoc />
        public MailItem Forward() => _mail.Forward();

        /// <inheritdoc />
        public void Display() => _mail.Display();

        /// <inheritdoc />
        public bool UnRead
        {
            get => _mail.UnRead;
            set => _mail.UnRead = value;
        }

        /// <inheritdoc />
        public void Save() => _mail.Save();

        /// <inheritdoc />
        public string EntryID => _mail.EntryID;
    }
}

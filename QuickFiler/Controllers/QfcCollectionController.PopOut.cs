using System;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    public partial class QfcCollectionController
    {
        private Func<
            IApplicationGlobals,
            System.Action,
            MailItem,
            IFolderSearchHandler,
            MailItemHelper,
            EfcHomeController
        > _popOutHomeControllerFactory;

        /// <summary>
        /// Factory seam for the pop-out home controller; null selects the production factory
        /// (#792 AC-U3).
        /// </summary>
        internal Func<
            IApplicationGlobals,
            System.Action,
            MailItem,
            IFolderSearchHandler,
            MailItemHelper,
            EfcHomeController
        > PopOutHomeControllerFactory
        {
            get => _popOutHomeControllerFactory ?? CreatePopOutHomeController;
            set => _popOutHomeControllerFactory = value;
        }

        private static EfcHomeController CreatePopOutHomeController(
            IApplicationGlobals globals,
            System.Action parentCleanup,
            MailItem mailItem,
            IFolderSearchHandler carriedFolderHandler,
            MailItemHelper carriedMailHelper
        )
        {
            return new EfcHomeController(
                globals,
                parentCleanup,
                mailItem,
                carriedFolderHandler,
                carriedMailHelper
            );
        }

        /// <summary>
        /// Reads the folder handler and mail helper carried by a group's item controller. Body
        /// lands in Phase 4 (#792).
        /// </summary>
        internal static (
            IFolderSearchHandler FolderHandler,
            MailItemHelper MailHelper
        ) ReadPopOutCarry(QfcItemGroup group)
        {
            return (null, null);
        }

        public void PopOutControlGroup(int selection)
        {
            // Get mail item from the group
            MailItem mailItem = _itemGroups[selection - 1].MailItem;

            // Remove the group from the form
            RemoveSpecificControlGroup(selection);

            var popOutForm = new EfcHomeController(_globals, () => { }, mailItem);
            popOutForm.Run();
        }

        public async Task PopOutControlGroupAsync(int selection)
        {
            Token.ThrowIfCancellationRequested();

            // Get mail item from the group
            MailItem mailItem = _itemGroups[selection - 1].MailItem;

            // Remove the group from the form
            await RemoveSpecificControlGroupAsync(selection);

            var popOutForm = new EfcHomeController(_globals, () => { }, mailItem);

            await popOutForm.RunAsync();
        }
    }
}

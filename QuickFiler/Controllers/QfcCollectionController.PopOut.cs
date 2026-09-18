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
        /// Reads the folder handler and mail helper carried by a group's item controller (#792 D6).
        /// Pure: the handler is read through the concrete <see cref="QfcItemController.FolderHandler"/>
        /// accessor by pattern match, the helper through <see cref="IQfcItemController.ItemHelper"/>;
        /// a null group or controller yields a null pair.
        /// </summary>
        internal static (
            IFolderSearchHandler FolderHandler,
            MailItemHelper MailHelper
        ) ReadPopOutCarry(QfcItemGroup group)
        {
            IQfcItemController controller = group?.ItemController;
            return (
                controller is QfcItemController concrete ? concrete.FolderHandler : null,
                controller?.ItemHelper
            );
        }

        /// <summary>
        /// Pops the selected group out into its own EFC home controller. The carry is read BEFORE
        /// the removal call because <c>QfcItemController.Cleanup</c> nulls <c>_folderHandler</c> and
        /// <c>ItemHelper</c>; the home controller is built through the factory seam (#792 D6).
        /// </summary>
        public void PopOutControlGroup(int selection)
        {
            QfcItemGroup group = _itemGroups[selection - 1];
            MailItem mailItem = group.MailItem;
            (IFolderSearchHandler handler, MailItemHelper helper) = ReadPopOutCarry(group);

            // Remove the group from the form
            RemoveSpecificControlGroup(selection);

            var form = PopOutHomeControllerFactory(_globals, () => { }, mailItem, handler, helper);
            form.Run();
        }

        /// <summary>
        /// Async form of <see cref="PopOutControlGroup"/>. The carry is read BEFORE the removal call
        /// because <c>QfcItemController.Cleanup</c> nulls <c>_folderHandler</c> and
        /// <c>ItemHelper</c>; the home controller is built through the factory seam (#792 D6).
        /// </summary>
        public async Task PopOutControlGroupAsync(int selection)
        {
            Token.ThrowIfCancellationRequested();

            QfcItemGroup group = _itemGroups[selection - 1];
            MailItem mailItem = group.MailItem;
            (IFolderSearchHandler handler, MailItemHelper helper) = ReadPopOutCarry(group);

            // Remove the group from the form
            await RemoveSpecificControlGroupAsync(selection);

            var form = PopOutHomeControllerFactory(_globals, () => { }, mailItem, handler, helper);

            await form.RunAsync();
        }
    }
}

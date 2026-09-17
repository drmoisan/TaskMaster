using System.Threading.Tasks;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    internal partial class EfcDataModel
    {
        /// <summary>Folder handler carried from a pop-out source item; adopted in Phase 4 (#792).</summary>
        internal IFolderSearchHandler CarriedFolderHandler { get; set; }

        /// <summary>Mail helper carried from a pop-out source item; consumed in Phase 4 (#792).</summary>
        internal MailItemHelper CarriedMailHelper { get; set; }

        /// <summary>
        /// Pure adoption decision for a carried folder handler. Decision lands in Phase 4 (#792).
        /// </summary>
        internal static bool TryAdoptCarriedFolderHandler(
            object folderList,
            IFolderSearchHandler carried,
            out FolderPredictor adopted
        )
        {
            adopted = null;
            return false;
        }

        public async Task InitFolderHandlerAsync(object folderList = null)
        {
            if (folderList is null)
            {
                if (MailInfo is null)
                {
                    FolderHelper = await Task.Run(() => new FolderPredictor(Globals), Token);
                }
                else
                {
                    FolderHelper = await Task.Run(
                        async () =>
                            await new FolderPredictor(
                                Globals,
                                MailInfo,
                                FolderPredictor.InitOptions.FromField
                            ).InitAsync(MailInfo, FolderPredictor.InitOptions.FromField),
                        Token
                    );
                }
            }
            else
            {
                FolderHelper = await Task.Run(
                    async () =>
                        await new FolderPredictor(
                            Globals,
                            folderList,
                            FolderPredictor.InitOptions.FromArrayOrString
                        ).InitAsync(folderList, FolderPredictor.InitOptions.FromArrayOrString),
                    Token
                );
            }
        }
    }
}

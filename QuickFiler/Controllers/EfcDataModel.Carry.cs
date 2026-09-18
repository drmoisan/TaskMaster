using System.Threading.Tasks;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    internal partial class EfcDataModel
    {
        /// <summary>
        /// Folder handler carried from a pop-out source item (#792 D7); consumed and released by
        /// <see cref="InitFolderHandlerAsync"/>.
        /// </summary>
        internal IFolderSearchHandler CarriedFolderHandler { get; set; }

        /// <summary>
        /// Mail helper carried from a pop-out source item (#792 D7); used as the scoring input when
        /// no conversation helper exists yet, then released.
        /// </summary>
        internal MailItemHelper CarriedMailHelper { get; set; }

        /// <summary>
        /// Pure adoption decision for a carried folder handler (#792 D7): true only when no explicit
        /// folder list was supplied and the carry is a concrete <see cref="FolderPredictor"/>, in
        /// which case <paramref name="adopted"/> is that instance; otherwise false and null.
        /// </summary>
        internal static bool TryAdoptCarriedFolderHandler(
            object folderList,
            IFolderSearchHandler carried,
            out FolderPredictor adopted
        )
        {
            if (folderList is null && carried is FolderPredictor predictor)
            {
                adopted = predictor;
                return true;
            }

            adopted = null;
            return false;
        }

        public async Task InitFolderHandlerAsync(object folderList = null)
        {
            if (
                TryAdoptCarriedFolderHandler(
                    folderList,
                    CarriedFolderHandler,
                    out FolderPredictor adopted
                )
            )
            {
                FolderHelper = adopted;
                ReleaseCarry();
                return;
            }

            if (folderList is null)
            {
                // Identical to the pre-#792 path whenever nothing was carried.
                MailItemHelper scoringInput = MailInfo ?? CarriedMailHelper;
                if (scoringInput is null)
                {
                    FolderHelper = await Task.Run(() => new FolderPredictor(Globals), Token);
                }
                else
                {
                    FolderHelper = await Task.Run(
                        async () =>
                            await new FolderPredictor(
                                Globals,
                                scoringInput,
                                FolderPredictor.InitOptions.FromField
                            ).InitAsync(scoringInput, FolderPredictor.InitOptions.FromField),
                        Token
                    );
                }

                ReleaseCarry();
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

        // The carry is single-use: once consulted it must not survive into a later re-initialization.
        private void ReleaseCarry()
        {
            CarriedFolderHandler = null;
            CarriedMailHelper = null;
        }
    }
}

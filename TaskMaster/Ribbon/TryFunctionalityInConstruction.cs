using Microsoft.Office.Interop.Outlook;
using SDILReader;
using stdole;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.EmailIntelligence.FolderRemap;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster.Ribbon
{
    public class TryFunctionalityInConstruction(IApplicationGlobals globals)
    {
        public IApplicationGlobals Globals { get; internal protected set; } = globals;

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Try specific methods

        internal void TryGetConversationDataframe()
        {
            var Mail = Globals.Ol.App.ActiveExplorer().Selection[1];
            Outlook.Conversation conv = (Outlook.Conversation)Mail.GetConversation();
            Microsoft.Data.Analysis.DataFrame df = conv.GetDataFrame();
            //logger.Debug(df.PrettyText());
            df.Display();
        }
        internal void TryGetConversationOutlookTable()
        {
            var Mail = Globals.Ol.App.ActiveExplorer().Selection[1];
            Outlook.Conversation conv = (Outlook.Conversation)Mail.GetConversation();
            var table = conv.GetTable(WithFolder: true, WithStore: true);
            table.EnumerateTable();
        }
        internal void TryGetMailItemInfo()
        {
            var mailItem = Globals.Ol.App.ActiveExplorer().Selection[1] as MailItem;
            var helper = new MailItemHelper(mailItem, Globals);
            //logger.Debug(helper.Item.HTMLBody);
        }

        internal void TryGetMailItemInfoViaConversation()
        {
            var Mail = Globals.Ol.App.ActiveExplorer().Selection[1];
            var conversation = (Outlook.Conversation)Mail.GetConversation();
            var df = conversation.GetDataFrame();
            df.PrettyPrint();
            //var mInfo = new MailItemHelper(df, 0, Globals.Ol.EmailPrefixToStrip);
            var info = MailItemHelper.FromDf(df, 0, Globals);
        }
        internal void TryGetQfcDataModel()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var dc = new QuickFiler.Controllers.QfcDatamodel(Globals, token);
        }
        internal void TryGetTableInView()
        {
            Outlook.Table table = Globals.Ol.App.ActiveExplorer().GetTableInView();
        }
        internal void TryRebuildProjInfo()
        {
            Globals.TD.ProjInfo.Rebuild(Globals.Ol.App);
        }
        internal void TryRecipientGetInfo()
        {
            var Mail = (Outlook.MailItem)Globals.Ol.App.ActiveExplorer().Selection[1];
            var recipients = Mail.Recipients.Cast<Recipient>();
            var info = recipients.GetInfo();
        }
        internal void TrySubstituteIdRoot()
        {
            Globals.TD.IDList.SubstituteIdRoot("9710", "2501");
        }
        internal void TryGetImage()
        {
            var ae = Globals.Ol.App.ActiveExplorer();
            //var image = ae.CommandBars.GetImageMso("ReplyAll", 38, 38);
            var image3 = ae.CommandBars.GetImageMso("Forward", 38, 38);
            //var image5 = ae.CommandBars.GetImageMso("Reply", 100, 100);

            //System.Drawing.Image image2 = GetImage(image);
            //image2.Save(@"C:\Temp\ReplyAll.png", ImageFormat.Png);

            System.Drawing.Image image4 = GetImage(image3);
            image4.Save(@"C:\Temp\Forward.png", ImageFormat.Png);

            //System.Drawing.Image image6 = GetImage(image5);
            //image6.Save(@"C:\Temp\Reply.png", ImageFormat.Png);


        }
        internal System.Drawing.Image GetImage(IPictureDisp disp)
        {
            return System.Drawing.Image.FromHbitmap((IntPtr)disp.Handle, (IntPtr)disp.hPal);
        }

        internal void TryLoadFolderFilter()
        {
            var filter = new FilterOlFoldersController(Globals);
            //var filter = new FilterOlFoldersViewer();
            //filter.ShowDialog();
        }

        internal void TryLoadFolderRemap()
        {
            var remap = new FolderRemapController(Globals);
        }

        internal async Task RebuildSubjectMapAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            await Globals.AF.SubjectMap.RebuildAsync(Globals);
        }

        internal void ShowSubjectMapMetrics()
        {
            Globals.AF.SubjectMap.ShowSummaryMetrics();
        }

        internal async Task TryTokenizeEmail()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());

            CancellationTokenSource cts = new CancellationTokenSource();
            var token = cts.Token;

            var ae = Globals.Ol.App.ActiveExplorer();
            var mail = (Outlook.MailItem)ae.Selection[1];
            var mailInfo = await MailItemHelper.FromMailItemAsync(mail, Globals, token, true);
            var tokenizer = new EmailTokenizer();
            //tokenizer.setup();
            var tokens = tokenizer.Tokenize(mailInfo).ToArray();
            var tokenString = tokens.SentenceJoin();
            MessageBox.Show(tokenString);
        }

        internal async Task TryMineEmails()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            var miner = new UtilitiesCS.EmailIntelligence.Bayesian.EmailDataMiner(Globals);
            await miner.MineEmails();
        }

        internal async Task TryBuildClassifier()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            var miner = new OlFolderClassifierGroup(Globals);
            await miner.BuildFolderClassifiersAsync();
        }

        internal void TryPrintManagerState()
        {
            //Globals.AF.Manager["Folder"].LogMetrics();
        }

        internal void TrySerializeMailInfo()
        {
            new EmailDataMiner(Globals).SerializeActiveItem();
            //var ae = Globals.Ol.App.ActiveExplorer();
            //var mail = (Outlook.MailItem)ae.Selection[1];
            //new EmailDataMiner(Globals).SerializeMailInfo(mail);

        }

        internal async Task TryTestClassifierAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            var tuner = new BayesianPerformanceMeasurement(Globals);
            await tuner.TestFolderClassifierAsync();
        }

        internal async Task TryTestClassifierVerboseAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            var tuner = new BayesianPerformanceMeasurement(Globals);
            await tuner.TestFolderClassifierAsync(verbose: true);
        }

        internal void TryNewTaskHeader()
        {
            var projectCreator = new AutoCreateProject(Globals);
            var prefix = new PrefixItem(
                prefixType: PrefixTypeEnum.Project,
                key: "Project", value: Properties.Settings.Default.Prefix_Project,
                color: OlCategoryColor.olCategoryColorTeal,
                olUserFieldName: "TagProject");
            projectCreator.CreateProjectTaskItem("T3 ROUTINE - Reading", "T305");
        }

        internal void TryGetInboxes()
        {
            var stores = Globals.Ol.NamespaceMAPI.Stores;
            var inboxes = stores
                .Cast<Store>()
                .Select(store => 
                {
                    try
                    {
                        return store.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                    }
                    catch (System.Exception)
                    {
                        return null;
                    }
                     
                })
                .Where(store => store is not null).ToArray();
            var mailboxes = inboxes.Select(x => x.FolderPath.Split("\\").Where(x => !x.IsNullOrEmpty()).FirstOrDefault()).ToArray();
            logger.Debug($"Inboxes: {mailboxes.SentenceJoin()}");
        }

        #endregion


    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.WinForms;
using Moq;
using QuickFiler;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.Threading;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Test.HelperClasses
{
    [TestClass]
    public class QfcThemeHelperTests
    {
        [TestMethod]
        public void SetupFormThemes_ReturnsExpectedKeysAndControlGroups()
        {
            var panels = new List<Control> { new Panel() };
            var buttons = new List<Control> { new Button() };

            Dictionary<string, Theme> themes = QfcThemeHelper.SetupFormThemes(panels, buttons);

            themes.Keys.Should().BeEquivalentTo("LightNormal", "DarkNormal");
            themes["LightNormal"]
                .ControlGroups.Keys.Should()
                .BeEquivalentTo("Default2Color", "Buttons");
            themes["DarkNormal"]
                .ControlGroups.Keys.Should()
                .BeEquivalentTo("Default2Color", "Buttons");
        }

        [TestMethod]
        public void SetupThemes_WithControlSet_ReturnsFourExpectedThemeKeys()
        {
            QfcThemeControlSet controlSet = CreateControlSet();

            Dictionary<string, Theme> themes = QfcThemeHelper.SetupThemes(controlSet);

            themes
                .Keys.Should()
                .BeEquivalentTo("LightNormal", "LightActive", "DarkNormal", "DarkActive");
            themes.Values.Should().OnlyContain(theme => theme.Name == themes[theme.Name].Name);
        }

        [TestMethod]
        public void SetupThemes_WithControlSet_MapsRepresentativeColorsAndHtmlStates()
        {
            QfcThemeControlSet controlSet = CreateControlSet();

            Dictionary<string, Theme> themes = QfcThemeHelper.SetupThemes(controlSet);

            themes["LightNormal"].NavBackColor.Should().Be(SystemColors.HotTrack);
            themes["LightActive"].DefaultBackColor.Should().Be(Color.LightCyan);
            themes["DarkNormal"].DefaultForeColor.Should().Be(Color.WhiteSmoke);
            themes["DarkActive"].DefaultBackColor.Should().Be(Color.FromArgb(64, 64, 64));
            themes["DarkActive"].HtmlDark.Should().Be(Enums.ToggleState.On);
        }

        [TestMethod]
        public void SetupThemes_WithNullController_ThrowsArgumentNullException()
        {
            ItemViewer viewer = CreateItemViewer();
            var dispatcher = new Mock<IUiDispatcher>();

            Action act = () =>
                QfcThemeHelper.SetupThemes(null, viewer, _ => { }, dispatcher.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("controller");
        }

        [TestMethod]
        public void SetupThemes_WithNullViewer_ThrowsArgumentNullException()
        {
            IQfcItemController controller = CreateController(out _, out _, out _, out _);
            var dispatcher = new Mock<IUiDispatcher>();

            Action act = () =>
                QfcThemeHelper.SetupThemes(controller, null, _ => { }, dispatcher.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("viewer");
        }

        [TestMethod]
        public void BuildProductionControlSet_MapsControllerAndViewerInputs()
        {
            FakeQfcItemController controller = CreateController(
                out IList<TableLayoutPanel> tableLayoutPanels,
                out IList<Button> buttons,
                out IList<IQfcTipsDetails> tipsDetails,
                out IList<IQfcTipsDetails> tipsExpanded
            );
            ItemViewer viewer = CreateItemViewer();
            var dispatcher = new Mock<IUiDispatcher>();
            var convertedStates = new List<Enums.ToggleState>();

            QfcThemeControlSet controlSet = QfcThemeHelper.BuildProductionControlSet(
                controller,
                viewer,
                convertedStates.Add,
                dispatcher.Object
            );

            controlSet.TableLayoutPanels.Should().BeSameAs(tableLayoutPanels);
            controlSet.Buttons.Should().BeSameAs(buttons);
            controlSet.TipsDetailsLabels.Should().BeSameAs(tipsDetails);
            controlSet.TipsExpanded.Should().BeSameAs(tipsExpanded);
            controlSet.MenuItems.Should().BeSameAs(viewer.MenuItems);
            controlSet.MenuStrip.Should().BeSameAs(viewer.MoveOptionsStrip);
            controlSet.Viewer.Should().BeSameAs(viewer);
            controlSet.UiDispatcher.Should().BeSameAs(dispatcher.Object);
            controlSet.MailRead.Should().NotBeNull();

            controlSet.HtmlConverter(Enums.ToggleState.On);
            convertedStates.Should().ContainSingle().Which.Should().Be(Enums.ToggleState.On);
        }

        [TestMethod]
        public void SetupFormThemes_ButtonGroups_ApplyLightAndDarkHoverBranches()
        {
            var panels = new List<Control> { new Panel() };
            var lightButton = new Button();
            var darkButton = new Button();
            Dictionary<string, Theme> themes = QfcThemeHelper.SetupFormThemes(
                panels,
                new List<Control> { lightButton, darkButton }
            );

            themes["LightNormal"].ControlGroups["Buttons"].ApplyTheme();
            lightButton.BackColor.Should().Be(SystemColors.Control);

            RaiseMouseEnter(lightButton);
            lightButton.BackColor.Should().Be(Color.LightCyan);

            RaiseMouseLeave(lightButton);
            lightButton.BackColor.Should().Be(SystemColors.Control);

            themes["DarkNormal"].ControlGroups["Buttons"].ApplyTheme();
            darkButton.BackColor.Should().Be(Color.DimGray);

            RaiseMouseEnter(darkButton);
            darkButton.BackColor.Should().Be(Color.DarkGray);

            RaiseMouseLeave(darkButton);
            darkButton.BackColor.Should().Be(Color.DimGray);
        }

        [TestMethod]
        public void QfcThemeControlSet_NullRequiredCollection_ThrowsArgumentNullException()
        {
            Action act = () =>
                CreateControlSet(tableLayoutPanels: null, preserveNullTableLayoutPanels: true);

            act.Should().Throw<ArgumentNullException>().WithParameterName("tableLayoutPanels");
        }

        [TestMethod]
        public void SetTheme_Extensions_ApplyColorsToControls()
        {
            var panel = new TableLayoutPanel();
            var label = new Label();
            var button = new Button();
            var control = new TextBox();

            panel.SetTheme(Color.Red);
            label.SetTheme(Color.Green, Color.White);
            button.SetTheme(Color.Blue);
            control.SetTheme(Color.Black, Color.Yellow);

            panel.BackColor.Should().Be(Color.Red);
            label.BackColor.Should().Be(Color.Green);
            label.ForeColor.Should().Be(Color.White);
            button.BackColor.Should().Be(Color.Blue);
            control.BackColor.Should().Be(Color.Black);
            control.ForeColor.Should().Be(Color.Yellow);
        }

        private static FakeQfcItemController CreateController(
            out IList<TableLayoutPanel> tableLayoutPanels,
            out IList<Button> buttons,
            out IList<IQfcTipsDetails> tipsDetails,
            out IList<IQfcTipsDetails> tipsExpanded
        )
        {
            var tips = new Mock<IQfcTipsDetails>();
            tableLayoutPanels = new List<TableLayoutPanel> { new TableLayoutPanel() };
            buttons = new List<Button> { new Button() };
            tipsDetails = new List<IQfcTipsDetails> { tips.Object };
            tipsExpanded = new List<IQfcTipsDetails> { tips.Object };
            return new FakeQfcItemController
            {
                TableLayoutPanels = tableLayoutPanels,
                Buttons = buttons,
                ListTipsDetails = tipsDetails,
                ListTipsExpanded = tipsExpanded,
            };
        }

        private static ItemViewer CreateItemViewer()
        {
            var viewer = CreateUninitialized<ItemViewer>();
            viewer.LblItemNumber = new Label();
            viewer.LblSender = new Label();
            viewer.LblSubject = new Label();
            viewer.MoveOptionsStrip = new MenuStrip();
            viewer.TxtboxSearch = new TextBox();
            viewer.TxtboxBody = new TextBox();
            viewer.CboFolders = new ComboBox();
            viewer.TopicThread = new FastObjectListView();
            viewer.L0v2h2_WebView2 = CreateUninitialized<WebView2>();
            SetPrivateField(
                viewer,
                "_menuItems",
                new List<Component> { new ToolStripMenuItem("Move") }
            );
            return viewer;
        }

        private static void RaiseMouseEnter(Control control)
        {
            InvokeControlEvent(control, "OnMouseEnter");
        }

        private static void RaiseMouseLeave(Control control)
        {
            InvokeControlEvent(control, "OnMouseLeave");
        }

        private static void InvokeControlEvent(Control control, string methodName)
        {
            MethodInfo method = typeof(Control).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            method.Should().NotBeNull($"{methodName} must exist on Control");
            method.Invoke(control, new object[] { EventArgs.Empty });
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.Should().NotBeNull($"{fieldName} must exist on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        private static QfcThemeControlSet CreateControlSet(
            IList<TableLayoutPanel> tableLayoutPanels = null,
            bool preserveNullTableLayoutPanels = false
        )
        {
            var dispatcher = new Mock<IUiDispatcher>();
            var tips = new Mock<IQfcTipsDetails>();
            if (tableLayoutPanels == null && !preserveNullTableLayoutPanels)
            {
                tableLayoutPanels = new List<TableLayoutPanel> { new TableLayoutPanel() };
            }

            return new QfcThemeControlSet(
                new Label(),
                new Label(),
                new Label(),
                tableLayoutPanels,
                new List<Button> { new Button() },
                new List<System.ComponentModel.Component> { new ToolStripMenuItem("Move") },
                new MenuStrip(),
                new List<IQfcTipsDetails> { tips.Object },
                new List<IQfcTipsDetails> { tips.Object },
                new TextBox(),
                new TextBox(),
                new ComboBox(),
                new FastObjectListView(),
                CreateUninitialized<WebView2>(),
                new Panel(),
                () => true,
                _ => { },
                dispatcher.Object
            );
        }

        private static TControl CreateUninitialized<TControl>()
            where TControl : class
        {
            return (TControl)FormatterServices.GetUninitializedObject(typeof(TControl));
        }

        private sealed class FakeQfcItemController : IQfcItemController
        {
            public int CounterEnter { get; set; }
            public int CounterComboRight { get; set; }
            public bool IsExpanded { get; private set; }
            public bool IsChild { get; set; }
            public bool IsActiveUI { get; set; }
            public string ConvOriginID { get; set; }
            public int Height { get; private set; }
            public MailItemHelper ItemHelper { get; set; }
            public Outlook.MailItem Mail { get; set; }
            public string SelectedFolder { get; private set; }
            public int ItemNumber { get; set; }
            public int ItemIndex { get; set; }
            public int ItemNumberDigits { get; set; }
            public bool SuppressEvents { get; set; }
            public IQfcCollectionController Parent { get; private set; }
            public IList<TableLayoutPanel> TableLayoutPanels { get; set; }
            public IList<Button> Buttons { get; set; }
            public IList<IQfcTipsDetails> ListTipsDetails { get; set; }
            public IList<IQfcTipsDetails> ListTipsExpanded { get; set; }
            public CancellationToken Token { get; set; }
            public Dictionary<string, System.Action> RightKeyActions { get; } =
                new Dictionary<string, System.Action>();
            public long TopFolderScore { get; private set; }

            public void AssignFolderComboBox() => throw new NotImplementedException();

            public Task InitializeAsync() => throw new NotImplementedException();

            public Task InitializeSequentialAsync() => throw new NotImplementedException();

            public void Initialize(bool async) => throw new NotImplementedException();

            public Task LoadConversationResolverAsync(
                CancellationTokenSource tokenSource,
                CancellationToken token,
                bool loadAll
            ) => throw new NotImplementedException();

            public void ToggleFocus() => throw new NotImplementedException();

            public void ToggleFocus(Enums.ToggleState desiredState) =>
                throw new NotImplementedException();

            public Task ToggleFocusAsync() => throw new NotImplementedException();

            public void ToggleExpansion() => throw new NotImplementedException();

            public void PopulateFolderComboBox(object varList = null) =>
                throw new NotImplementedException();

            public void ApplyReadEmailFormat(object state) => throw new NotImplementedException();

            public void FlagAsTask() => throw new NotImplementedException();

            public void MarkItemForDeletion() => throw new NotImplementedException();

            public void JumpToSearchTextbox() => throw new NotImplementedException();

            public void JumpToFolderDropDown() => throw new NotImplementedException();

            public void ToggleSaveCopyOfMail() => throw new NotImplementedException();

            public void ToggleSaveAttachments() => throw new NotImplementedException();

            public void ToggleConversationCheckbox() => throw new NotImplementedException();

            public void ToggleConversationCheckbox(Enums.ToggleState desiredState) =>
                throw new NotImplementedException();

            public void PopulateConversation() => throw new NotImplementedException();

            public Task PopulateConversationAsync(
                CancellationTokenSource tokenSource,
                CancellationToken token,
                bool loadAll
            ) => throw new NotImplementedException();

            public void PopulateConversation(int countOnly) => throw new NotImplementedException();

            public void PopulateConversation(ConversationResolver resolver) =>
                throw new NotImplementedException();

            public Task PopulateFolderComboBoxAsync(
                CancellationToken token,
                object varList = null
            ) => throw new NotImplementedException();

            public Task LoadFolderHandlerAsync(CancellationToken cancel, object varList = null) =>
                throw new NotImplementedException();

            public void PopulateControls(MailItemHelper helper, int viewerPosition) =>
                throw new NotImplementedException();

            public void RenderConversationCount(int count) => throw new NotImplementedException();

            public void RenderConversationCount() => throw new NotImplementedException();

            public void SetThemeDark(bool async) => throw new NotImplementedException();

            public void SetThemeLight(bool async) => throw new NotImplementedException();

            public void Cleanup() => throw new NotImplementedException();

            public Task MoveMailAsync() => throw new NotImplementedException();

            public void ToggleNavigation(bool async) => throw new NotImplementedException();

            public void ToggleNavigation(bool async, Enums.ToggleState desiredState) =>
                throw new NotImplementedException();

            public Task ToggleNavigationAsync(Enums.ToggleState desiredState) =>
                throw new NotImplementedException();

            public void ToggleTips(bool async, Enums.ToggleState desiredState) =>
                throw new NotImplementedException();

            public Task ToggleExpansionAsync() => throw new NotImplementedException();

            public Task ToggleFocusAsync(Enums.ToggleState off) =>
                throw new NotImplementedException();

            public Task InitializeGraphicsAsync() => throw new NotImplementedException();
        }
    }
}

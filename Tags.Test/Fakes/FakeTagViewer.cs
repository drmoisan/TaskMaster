using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Moq;
using UtilitiesCS;

namespace Tags.Test.Fakes
{
    /// <summary>
    /// In-memory <see cref="ITagViewer"/> fake for deterministic controller tests. Because
    /// <see cref="ITagViewer"/> derives from the ~191-member <c>IForm</c>/<c>IControl</c> hierarchy,
    /// the base surface is auto-stubbed by Moq (the same pattern QuickFiler uses for
    /// <c>IQfcFormViewer</c>); this fake layers real behavior on top for the members the controller
    /// actually consumes: a backing <see cref="List{CheckBox}"/> for the option panel, settable state
    /// properties, and raisable command-intent events. It constructs no live <see cref="Form"/> or
    /// control; the only controls it holds are the headless <see cref="CheckBox"/> property bags the
    /// controller itself creates and hands to <see cref="AddOptionControl"/>.
    /// </summary>
    internal sealed class FakeTagViewer
    {
        private readonly Mock<ITagViewer> _mock = new Mock<ITagViewer>(MockBehavior.Loose);
        private readonly List<CheckBox> _options = new List<CheckBox>();
        private bool _hideArchiveChecked;
        private int _searchSelectionStart;
        private int _optionsPanelHeight;
        private int _optionsScrollMaximum;

        public FakeTagViewer(bool hideArchiveChecked = true)
        {
            _hideArchiveChecked = hideArchiveChecked;

            _mock.SetupProperty(v => v.SearchTextValue, string.Empty);
            _mock.SetupProperty(v => v.Caption);
            _mock.SetupProperty(v => v.AutoAssignVisible);
            _mock.SetupProperty(v => v.AutoAssignEnabled);
            _mock.SetupProperty(v => v.ButtonNewVisible);
            _mock.SetupProperty(v => v.OptionsAutoScrollPosition);

            _mock.Setup(v => v.HideArchiveChecked).Returns(() => _hideArchiveChecked);
            _mock.Setup(v => v.SearchSelectionStart).Returns(() => _searchSelectionStart);
            _mock.Setup(v => v.OptionsPanelHeight).Returns(() => _optionsPanelHeight);
            _mock.Setup(v => v.OptionsScrollMaximum).Returns(() => _optionsScrollMaximum);
            _mock.Setup(v => v.CaptureAndRemoveTemplate()).Returns(() => new ControlPosition());
            _mock.Setup(v => v.OptionControls).Returns(() => _options.AsReadOnly());
            _mock
                .Setup(v => v.AddOptionControl(It.IsAny<CheckBox>()))
                .Callback<CheckBox>(cb => _options.Add(cb));
            _mock
                .Setup(v => v.RemoveOptionControl(It.IsAny<CheckBox>()))
                .Callback<CheckBox>(cb => _options.Remove(cb));
        }

        public ITagViewer Object => _mock.Object;

        public Mock<ITagViewer> Mock => _mock;

        /// <summary>The option checkboxes currently rendered into the fake panel (in insertion order).</summary>
        public IReadOnlyList<CheckBox> OptionControls => _options;

        public string SearchTextValue
        {
            get => _mock.Object.SearchTextValue;
            set => _mock.Object.SearchTextValue = value;
        }

        public string Caption => _mock.Object.Caption;

        public bool AutoAssignVisible => _mock.Object.AutoAssignVisible;

        public bool AutoAssignEnabled => _mock.Object.AutoAssignEnabled;

        public bool ButtonNewVisible => _mock.Object.ButtonNewVisible;

        public bool HideArchiveChecked
        {
            get => _hideArchiveChecked;
            set => _hideArchiveChecked = value;
        }

        public int SearchSelectionStart
        {
            get => _searchSelectionStart;
            set => _searchSelectionStart = value;
        }

        public int OptionsPanelHeight
        {
            get => _optionsPanelHeight;
            set => _optionsPanelHeight = value;
        }

        public int OptionsScrollMaximum
        {
            get => _optionsScrollMaximum;
            set => _optionsScrollMaximum = value;
        }

        public Point OptionsAutoScrollPosition => _mock.Object.OptionsAutoScrollPosition;

        // Event raisers used by migrated/new tests to drive the controller's intent handlers.
        public void RaiseOkClicked() => _mock.Raise(v => v.OkClicked += null, EventArgs.Empty);

        public void RaiseCancelClicked() =>
            _mock.Raise(v => v.CancelClicked += null, EventArgs.Empty);

        public void RaiseNewClicked() => _mock.Raise(v => v.NewClicked += null, EventArgs.Empty);

        public void RaiseAutoAssignClicked() =>
            _mock.Raise(v => v.AutoAssignClicked += null, EventArgs.Empty);

        public void RaiseSearchTextChanged() =>
            _mock.Raise(v => v.SearchTextChanged += null, EventArgs.Empty);

        public void RaiseSearchKeyDown(Keys key) =>
            _mock.Raise(v => v.SearchKeyDown += null, _mock.Object, new KeyEventArgs(key));

        public void RaiseSearchKeyUp(Keys key) =>
            _mock.Raise(v => v.SearchKeyUp += null, _mock.Object, new KeyEventArgs(key));

        public void RaiseHideArchiveChanged() =>
            _mock.Raise(v => v.HideArchiveChanged += null, EventArgs.Empty);

        public void RaiseViewKeyDown(Keys key) =>
            _mock.Raise(v => v.ViewKeyDown += null, _mock.Object, new KeyEventArgs(key));

        public void RaiseOptionsKeyDown(Keys key) =>
            _mock.Raise(v => v.OptionsKeyDown += null, _mock.Object, new KeyEventArgs(key));

        /// <summary>Sets <see cref="HideArchiveChecked"/> and raises the change event (mirrors the checkbox toggle).</summary>
        public void SetHideArchive(bool value)
        {
            _hideArchiveChecked = value;
            RaiseHideArchiveChanged();
        }
    }
}

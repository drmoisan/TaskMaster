using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS
{
    public class FlagClassNoItem: INotifyPropertyChanged, ICloneable
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public FlagClassNoItem(string categoryNames) 
        { 
            Flags = new FlagParser(ref categoryNames);
            CategoryNames = categoryNames;
            PropertyChanged += Handler_PropertyChanged;
        }

        public FlagClassNoItem(IList<string> categoryList)
        {
            Flags = new FlagParser(categoryList);
            CategoryNames = Flags.Combine();
            PropertyChanged += Handler_PropertyChanged;
        }

        public FlagClassNoItem(Categories categories)
        {
            _olCategories = categories.Cast<Category>().ToList();
            var categoryList = _olCategories.Select(c => c.Name).ToList();
            Flags = new FlagParser(categoryList);
            CategoryNames = Flags.Combine();
            PropertyChanged += Handler_PropertyChanged;
        }

        public FlagClassNoItem(IList<Category> categories)
        {
            _olCategories = categories;
            var categoryList = categories.Select(c => c.Name).ToList();
            Flags = new FlagParser(categoryList);
            CategoryNames = Flags.Combine();
            PropertyChanged += Handler_PropertyChanged;
        }

        #endregion Constructors

        #region Public Properties and Methods

        private IList<Category> _olCategories;
        public IList<Category> OlCategories { get => _olCategories; set => _olCategories = value; }

        private IList<Category> _olCategorySelection;
        public IList<Category> OlCategorySelection 
        { 
            get => Initializer.GetOrLoad(ref _olCategorySelection, SelectionToOlCategories); 
            private set => _olCategorySelection = value; 
        }
        public IList<Category> SelectionToOlCategories() => 
            OlCategories?.Where(c => CategoryNames.Contains(c.Name))?.ToList();

        private string _categoryNames;
        public string CategoryNames 
        { 
            get => _categoryNames; 
            set { _categoryNames = value; NotifyPropertyChanged(); } 
        }

        internal FlagParser _flags;
        public FlagParser Flags { get => _flags; private set => _flags = value; }

        private FlagTranslator _people;
        public FlagTranslator People
        {
            get => Initializer.GetOrLoad(ref _people, () => LoadPeople(), false, Flags);
            private set => _people = value;
        }
        private FlagTranslator LoadPeople() => new(Flags.GetPeople, Flags.SetPeople, Flags.GetPeopleList, Flags.SetPeopleList);
        async private Task LoadPeopleAsync() => await Task.Run(() => _people = LoadPeople());

        private FlagTranslator _projects;
        public FlagTranslator Projects
        {
            get => Initializer.GetOrLoad(ref _projects, LoadProjects, false, Flags);
            private set => _projects = value;
        }
        private FlagTranslator LoadProjects() => new(Flags.GetProjects, Flags.SetProjects, Flags.GetProjectList, Flags.SetProjectList);
        async private Task LoadProjectAsync() => await Task.Run(() => _projects = LoadProjects());

        private FlagTranslator _context;
        public FlagTranslator Context
        {
            get => Initializer.GetOrLoad(ref _context, LoadContext, false, Flags);
            private set => _context = value;
        }
        private FlagTranslator LoadContext() => new(Flags.GetContext, Flags.SetContext, Flags.GetContextList, Flags.SetContextList);
        async private Task LoadContextAsync() => await Task.Run(() => _context = LoadContext());

        private FlagTranslator _topic;
        public FlagTranslator Topics
        {
            get => Initializer.GetOrLoad(ref _topic, LoadTopic, false, Flags);
            private set => _topic = value;
        }
        private FlagTranslator LoadTopic() => new(Flags.GetTopics, Flags.SetTopics, Flags.GetTopicList, Flags.SetTopicList);
        async private Task LoadTopicAsync() => await Task.Run(() => _topic = LoadTopic());

        private FlagTranslator _kb;

        public FlagTranslator KB
        {
            get => Initializer.GetOrLoad(ref _kb, LoadKB, false, Flags);
            private set => _kb = value;
        }
        private FlagTranslator LoadKB() => new(Flags.GetKb, Flags.SetKb, Flags.GetKbList, Flags.SetKbList);
        async private Task LoadKBAsync() => await Task.Run(() => _kb = LoadKB());

        public bool Bullpin { get => Flags.Bullpin; set => Flags.Bullpin = value; }
        public bool Today { get => Flags.Today; set => Flags.Today = value; }

        #endregion Public Properties and Methods

        #region INotifyPropertyChanged Implementation
            
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void Handler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CategoryNames))
            {
                Flags = new FlagParser(ref _categoryNames);
                RequestBatchRefresh();
            }
        }

        #endregion INotifyPropertyChanged Implementation

        #region Batch Refresh

        private ThreadSafeSingleShotGuard _batchRefreshRequested = new();
        private TimerWrapper _timer;
        
        private void RequestBatchRefresh()
        {
            if (_batchRefreshRequested.CheckAndSetFirstCall)
            {
                _timer = new TimerWrapper(TimeSpan.FromMilliseconds(50));
                _timer.Elapsed += async (sender, e) => await BatchRefresh();
                _timer.AutoReset = false;
                _timer.StartTimer();
            }
        }

        private async Task BatchRefresh()
        {
            var tasks = new List<Task>
            {
                LoadPeopleAsync(),
                LoadProjectAsync(),
                LoadContextAsync(),
                LoadTopicAsync(),
                LoadKBAsync(),
            };
            
            await Task.WhenAll(tasks);
            
            _batchRefreshRequested = new();
        }

        #endregion Batch Refresh

        #region ICloneable Implementation

        public object Clone()
        {
            if (OlCategories is not null)
            {
                return new FlagClassNoItem(OlCategories);
            }
            else
            {
                return new FlagClassNoItem(CategoryNames);
            }
        }

        #endregion ICloneable Implementation

    }
}

# P3-T19 pre-split census of CategoryClassifierGroup.cs (issue #973; AC23 red-before)

Timestamp: 2026-10-03T11-35
Command: CMD-LINECOUNT and CMD-CRCOUNT (measured in P3-T18, no edit since); CMD-MEMBER-CENSUS (Grep `^        (public|private|protected|internal|void) ` -n); Grep `^        #region IConditionalEngine Implementation\r?$`, `^        #endregion IConditionalEngine Implementation\r?$`, `^    public class CategoryClassifierGroup : IConditionalEngine<MailItemHelper>\r?$`, `^        #endregion Public Properties\r?$` (-n); Grep `partial` count; Read tool offset 440 limit 4; CMD-BOM; Glob UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/*
EXIT_CODE: 0
Output Summary: the file is 537 lines (over the 500-line limit) with 537 carriage returns after Part F; 36 member declarations; the IConditionalEngine region spans 443-535; declaration at 23 with no `partial`; `#endregion Public Properties` at 440 followed by two blank lines (441-442); UTF-8 BOM present; the Categories folder holds one file. No SPLIT-PRECONDITION.

LINECOUNT: 537; CRCOUNT: 537
`#region IConditionalEngine Implementation`: 443
`#endregion IConditionalEngine Implementation`: 535
Declaration `    public class CategoryClassifierGroup : IConditionalEngine<MailItemHelper>`: 23
`partial` count: 0
`#endregion Public Properties`: 440; Read 441 (blank), 442 (blank), 443 `#region IConditionalEngine Implementation`
BOM-BYTES UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs=239,187,191
Glob Categories/*: 1 file

MEMBER private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
MEMBER private CategoryClassifierGroup() { }
MEMBER public CategoryClassifierGroup(IApplicationGlobals globals)
MEMBER public async Task<CategoryClassifierGroup?> InitAsync(string groupName)
MEMBER public static async Task<CategoryClassifierGroup?> CreateEngineAsync(
MEMBER internal IApplicationGlobals Globals { get; private set; } = null!;
MEMBER internal ClassifierGroupUtilities CgUtilities = null!;
MEMBER public async Task BuildClassifiersAsync()
MEMBER private async Task<(
MEMBER private async Task<BayesianClassifierGroup> LoadClassifierGroup(
MEMBER private async Task<BayesianClassifierGroup> LoadClassifierGroup(
MEMBER private async Task<MinedMailInfo[]> LoadStagingData(
MEMBER private static InvalidOperationException CreateMissingStagingDataException(
MEMBER private static void ShowMissingStagingDataDialog(string message)
MEMBER public async Task<bool> BuildClassifiersAsync(
MEMBER internal IEnumerable<MinedMailInfo> ExplodeMailsByCategory(MinedMailInfo m, IPrefix prefix)
MEMBER public virtual async Task BuildClassifierAsync(
MEMBER public BayesianClassifierGroup ClassifierGroup { get; set; } = null!;
MEMBER public bool IsActivated => ClassifierGroup is not null;
MEMBER public double ProbabilityThreshold { get; set; } = 0.8;
MEMBER public Func<IEnumerable<string>, MailItemHelper, Task> CategorySetter { get; set; } = null!;
MEMBER public async Task TestAsync(MailItemHelper helper)
MEMBER public async Task<string[]> GetMatchingCategoriesAsync(MailItemHelper helper)
MEMBER public string[] GetMatchingCategories(MailItemHelper helper)
MEMBER public ISmartSerializableConfig Config => ClassifierGroup.Config;
MEMBER void IConditionalEngine<MailItemHelper>.Serialize()
MEMBER public Func<MailItemHelper, Task> AsyncAction =>
MEMBER public Func<object, Task<bool>> AsyncCondition =>
MEMBER private bool Condition(object item)
MEMBER private bool ConditionLog(object item)
MEMBER private string GetOlItemString(OutlookItem olItem)
MEMBER public object Engine => this;
MEMBER public Func<IApplicationGlobals, Task> EngineInitializer =>
MEMBER public string EngineName { get; internal set; } = null!;
MEMBER public string Message => $"{nameof(CategoryClassifierGroup)} is null. Skipping actions";
MEMBER public MailItemHelper TypedItem { get; set; } = null!;
MEMBER-COUNT: 36

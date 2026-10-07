# P0-T20 scope-fold source baseline (issue #973; read-only)

Timestamp: 2026-10-03T10-56
Command: Grep tool over the five Part F files (`^using Microsoft\.Graph` -n -A 1; `^` and `\r$` counts; `^using ` count; `^#nullable enable` -n); Grep `^using Microsoft\.Graph[.;]` type cs over <execution-worktree-root>; Greps over UtilitiesCS/UtilitiesCS.csproj (Compile Include alternation -n; `ConditionalEngine` count; `Compile Include=` count); Glob UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/*; Greps over CategoryClassifierGroup.cs (`^        #(end)?region ` -n; CMD-MEMBER-CENSUS; declaration -n; `partial` count); CMD-BOM; CLAUDE.md CMD-LINECOUNT, CMD-CRCOUNT and EDIT-CLAUDE-BULLET Greps A to G -n; Glob Directory.Build.*; Grep `<Nullable` over **/*.{csproj,props,targets,vbproj,fsproj}; Grep `name="Microsoft\.Graph(\.Core)?"` -A 1 over */app.config; Grep `Include="Microsoft\.Graph(\.Core)?, ` -n over */*.csproj; git -C <execution-worktree-root> status --porcelain -- '*.cs' CLAUDE.md; micro-check: Grep `CategoryClassifierGroup\.ConditionalEngine` count over UtilitiesCS/UtilitiesCS.csproj and git -C <execution-worktree-root> grep -n ConditionalEngine 993fdd01566dee82e5f37acb761a600feaaa1454 -- UtilitiesCS/UtilitiesCS.csproj
EXIT_CODE: 0
Output Summary: every fact 10 to 14 value holds on the tree (6 Graph directives in 5 files at the cited lines, line and CR counts equal, #nullable enable at line 1, Compile Include lines 610/616/704/738/758, one file under Categories, regions and the 36-line member census as fact 11, CLAUDE.md 463/463 with Greps A to G as stated, 17 Graph redirect blocks, 4 Graph Reference lines, clean porcelain). One task-text count differs: Grep `ConditionalEngine` over UtilitiesCS.csproj returns 1, not 0, because the pattern also matches the pre-existing `Interfaces\IGlobals\IConditionalEngine.cs` Compile item at line 779 (present at the base SHA); fact 10's statement (no Compile Include names CategoryClassifierGroup.ConditionalEngine.cs) holds, verified by the narrower Grep (0). Recorded as PLAN-TEXT-DISCREPANCY; not tree drift.

## Part F files

- UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs: 7 `using Microsoft.Graph.Models.TermStore;` / 8 `using Microsoft.Office.Interop.Outlook;`; LINECOUNT 306, CRCOUNT 306; USINGS-BASELINE: 11; `#nullable enable` at 1
- UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs: 10 `using Microsoft.Graph.Models;` / 11 `using Microsoft.Office.Interop.Outlook;`; 270 / 270; USINGS-BASELINE: 15; `#nullable enable` at 1
- UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs: 11 `using Microsoft.Graph.Communications.OnlineMeetings.GetAllRecordingsmeetingOrganizerUserIdMeetingOrganizerUserIdWithStartDateTimeWithEndDateTime;` and 12 `using Microsoft.Graph.Drives.Item.Items.Item.GetActivitiesByInterval;` / 13 `using Microsoft.Office.Interop.Outlook;`; 539 / 539; USINGS-BASELINE: 20; `#nullable enable` at 1
- UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs: 18 `using Microsoft.Graph.Security.AttackSimulation.Trainings.Item.LanguageDetails;` / 19 `using Newtonsoft.Json;`; 356 / 356; USINGS-BASELINE: 23; `#nullable enable` at 1
- UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs: 6 `using Microsoft.Graph.Drives.Item.Items.Item.SearchWithQ;` / 7 `using Newtonsoft.Json;`; 188 / 188; USINGS-BASELINE: 8; `#nullable enable` at 1

Worktree Grep `^using Microsoft\.Graph[.;]` type cs: 6 hits in 5 files (Triage_OlLogic.cs:10, ManagerAsyncLazy.cs:18, CategoryClassifierGroup.cs:11, CategoryClassifierGroup.cs:12, StoreWrapper.cs:7, FolderMinimalWrapper.cs:6)

## UtilitiesCS/UtilitiesCS.csproj

- 610 `<Compile Include="EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs" />`
- 616 `<Compile Include="EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs" />`
- 704 `<Compile Include="EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs" />`
- 738 `<Compile Include="OutlookObjects\Folder\FolderMinimalWrapper.cs" />`
- 758 `<Compile Include="OutlookObjects\Store\StoreWrapper.cs" />`
- Grep `ConditionalEngine` count: 1 (line 779 `<Compile Include="Interfaces\IGlobals\IConditionalEngine.cs" />`; task text expected 0)
- PLAN-TEXT-DISCREPANCY: the P0-T20 pattern `ConditionalEngine` also matches the pre-existing IConditionalEngine interface Compile item, present at base SHA 993fdd015 line 779; the fact 10 statement it stands for is verified by Grep `CategoryClassifierGroup\.ConditionalEngine` count 0. No later gate consumes the literal count.
- COMPILE-ITEMS-BASELINE: 498

## Categories folder

Glob UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/*: 1 file (CategoryClassifierGroup.cs)

## CategoryClassifierGroup.cs structure

Regions (-n): 31 `#region ctor`, 70 `#endregion ctor`, 76 `#region Build Category Classifier`, 399 `#endregion Build Category Classifier`, 401 `#region Public Properties`, 442 `#endregion Public Properties`, 445 `#region IConditionalEngine Implementation`, 537 `#endregion IConditionalEngine Implementation`
Declaration: 25 `    public class CategoryClassifierGroup : IConditionalEngine<MailItemHelper>`
Grep `partial` count: 0
BOM-BYTES UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs=239,187,191

CMD-MEMBER-CENSUS:
MEMBER private static readonly log4net.ILog logger = log4net.LogManager.GetLogger( (27)
MEMBER private CategoryClassifierGroup() { } (33)
MEMBER public CategoryClassifierGroup(IApplicationGlobals globals) (35)
MEMBER public async Task<CategoryClassifierGroup?> InitAsync(string groupName) (41)
MEMBER public static async Task<CategoryClassifierGroup?> CreateEngineAsync( (58)
MEMBER internal IApplicationGlobals Globals { get; private set; } = null!; (72)
MEMBER internal ClassifierGroupUtilities CgUtilities = null!; (74)
MEMBER public async Task BuildClassifiersAsync() (78)
MEMBER private async Task<( (170)
MEMBER private async Task<BayesianClassifierGroup> LoadClassifierGroup( (195)
MEMBER private async Task<BayesianClassifierGroup> LoadClassifierGroup( (224)
MEMBER private async Task<MinedMailInfo[]> LoadStagingData( (244)
MEMBER private static InvalidOperationException CreateMissingStagingDataException( (278)
MEMBER private static void ShowMissingStagingDataDialog(string message) (303)
MEMBER public async Task<bool> BuildClassifiersAsync( (313)
MEMBER internal IEnumerable<MinedMailInfo> ExplodeMailsByCategory(MinedMailInfo m, IPrefix prefix) (360)
MEMBER public virtual async Task BuildClassifierAsync( (379)
MEMBER public BayesianClassifierGroup ClassifierGroup { get; set; } = null!; (404)
MEMBER public bool IsActivated => ClassifierGroup is not null; (406)
MEMBER public double ProbabilityThreshold { get; set; } = 0.8; (408)
MEMBER public Func<IEnumerable<string>, MailItemHelper, Task> CategorySetter { get; set; } = null!; (410)
MEMBER public async Task TestAsync(MailItemHelper helper) (412)
MEMBER public async Task<string[]> GetMatchingCategoriesAsync(MailItemHelper helper) (421)
MEMBER public string[] GetMatchingCategories(MailItemHelper helper) (431)
MEMBER public ISmartSerializableConfig Config => ClassifierGroup.Config; (447)
MEMBER void IConditionalEngine<MailItemHelper>.Serialize() (455)
MEMBER public Func<MailItemHelper, Task> AsyncAction => (460)
MEMBER public Func<object, Task<bool>> AsyncCondition => (469)
MEMBER private bool Condition(object item) (472)
MEMBER private bool ConditionLog(object item) (486)
MEMBER private string GetOlItemString(OutlookItem olItem) (511)
MEMBER public object Engine => this; (526)
MEMBER public Func<IApplicationGlobals, Task> EngineInitializer => (528)
MEMBER public string EngineName { get; internal set; } = null!; (531)
MEMBER public string Message => $"{nameof(CategoryClassifierGroup)} is null. Skipping actions"; (533)
MEMBER public MailItemHelper TypedItem { get; set; } = null!; (535)
MEMBER-COUNT: 36

## CLAUDE.md

LINECOUNT 463, CRCOUNT 463
A `there is no \x60Directory\.Build\.props\x60`: 1 at 211
B `neither root build file sets one`: 0
C `195 errors in \x60UtilitiesCS\.csproj\x60 on 2026-08-10`: 1 at 211
D `CI omits it deliberately`: 1 at 211
E `RxUseUnsupportedPackagesConfig`: 0
F `toggles VSTO signing for the TaskMaster project`: 0
G `Removing it loses no enforcement over any file that has opted in`: 1 at 211

## Root build files and Nullable

Glob Directory.Build.*: Directory.Build.props, Directory.Build.targets (two files)
Grep `<Nullable` over **/*.{csproj,props,targets,vbproj,fsproj}: 0

## AC22 baseline: Microsoft.Graph redirects (17 blocks across 15 files)

GRAPH-REDIRECT VBFunctions.Test/app.config:<assemblyIdentity name="Microsoft.Graph.Core" publicKeyToken="31bf3856ad364e35" culture="neutral" />|<bindingRedirect oldVersion="0.0.0.0-4.0.1.0" newVersion="4.0.1.0" />
GRAPH-REDIRECT TaskVisualization.Test/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0 (same two lines as above)
GRAPH-REDIRECT TaskVisualization/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
GRAPH-REDIRECT QuickFiler.Test/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
GRAPH-REDIRECT TaskTree.Test/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
GRAPH-REDIRECT TaskMaster/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
GRAPH-REDIRECT TaskTree/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
GRAPH-REDIRECT QuickFiler/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
GRAPH-REDIRECT Tags.Test/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
GRAPH-REDIRECT TaskMaster.Test/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
GRAPH-REDIRECT UtilitiesCS.Test/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
GRAPH-REDIRECT UtilitiesCS.Test/app.config:<assemblyIdentity name="Microsoft.Graph" publicKeyToken="31bf3856ad364e35" culture="neutral" />|<bindingRedirect oldVersion="0.0.0.0-6.7.0.0" newVersion="6.7.0.0" />
GRAPH-REDIRECT Tags/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
GRAPH-REDIRECT ToDoModel/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
GRAPH-REDIRECT UtilitiesCS/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
GRAPH-REDIRECT UtilitiesCS/app.config:Microsoft.Graph|0.0.0.0-6.7.0.0 -> 6.7.0.0 (same two lines as the UtilitiesCS.Test Microsoft.Graph block)
GRAPH-REDIRECT ToDoModel.Test/app.config:Microsoft.Graph.Core|0.0.0.0-4.0.1.0 -> 4.0.1.0
(Every Microsoft.Graph.Core block reads the identity line `<assemblyIdentity name="Microsoft.Graph.Core" publicKeyToken="31bf3856ad364e35" culture="neutral" />` and the redirect line `<bindingRedirect oldVersion="0.0.0.0-4.0.1.0" newVersion="4.0.1.0" />`.)

## AC22 baseline: Microsoft.Graph References

- UtilitiesCS.Test/UtilitiesCS.Test.csproj:678 `<Reference Include="Microsoft.Graph, Version=6.7.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35">`
- UtilitiesCS.Test/UtilitiesCS.Test.csproj:682 `<Reference Include="Microsoft.Graph.Core, Version=4.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">`
- UtilitiesCS/UtilitiesCS.csproj:131 `<Reference Include="Microsoft.Graph, Version=6.7.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35">`
- UtilitiesCS/UtilitiesCS.csproj:135 `<Reference Include="Microsoft.Graph.Core, Version=4.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">`

## Porcelain

git -C <execution-worktree-root> status --porcelain -- '*.cs' CLAUDE.md: (empty)

# Increment 3 Seam Verification — TaskMaster

Timestamp: 2026-06-14T08-22

Command: source inspection of TaskMaster AppGlobals target files (Read/Grep)

EXIT_CODE: 0

InternalsVisibleTo("TaskMaster.Test") confirmed in TaskMaster/ThisAddIn.cs line 11 and
TaskMaster/Properties/AssemblyInfo.cs line 38. So `internal` setters on AppQuickFilerSettings are
reachable from TaskMaster.Test.

## Settings seam approach (no production injection seam)

AppStagingFilenames and AppQuickFilerSettings read/write the static
TaskMaster.Properties.Settings.Default singleton directly and expose NO injectable settings type.
The maintainer-accepted approach (already used by TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs)
is to snapshot the affected Settings.Default values in [TestInitialize] and restore them in
[TestCleanup], constructing the type via its parameterless constructor. No production injection
seam is introduced.

## Confirmed seams (file/line; no [ExcludeFromCodeCoverage])

- AppStagingFilenames — TaskMaster/AppGlobals/AppStagingFilenames.cs (class line 6)
  - Ten string properties (ConditionalReminders, CommonWords, SubjectMap, CtfInc, CtfMap,
    EmailSessionTemp, EmailSession, MovedMails, RecentsFile, EmailInfoStagingFile). Each getter is
    `_field ?? InitProp(ref _field, Settings.Default.<X>)` (lazy-init from Settings.Default).
    Setters write the backing field AND Settings.Default.<X> then call Settings.Default.Save(),
    EXCEPT EmailInfoStagingFile (line 130) whose setter sets only the backing field (no
    Settings.Default write, no Save()). `internal string InitProp(ref string prop, string value)`
    line 133 returns value and assigns prop. No Outlook, no filesystem, no exemption.
  - TESTABLE via Settings.Default snapshot/restore + `new AppStagingFilenames()`. Positive: getter
    returns persisted value (snapshot a Settings.Default file value, read property). Setter
    round-trips through backing field + Settings.Default. Edge: EmailInfoStagingFile setter does
    NOT touch Settings.Default; InitProp lazy-init path; null/empty persisted value.

- AppQuickFilerSettings — TaskMaster/AppGlobals/AppQuickFilerSettings.cs (class line 6)
  - Six properties, public getter + `internal set` (reachable via InternalsVisibleTo):
    MoveEntireConversation (line 8), SaveAttachments (18), SavePictures (28), SaveEmailCopy (38),
    HighConfidenceModeEnabled (48), HighConfidenceThreshold (58). Each getter reads
    Settings.Default.<X>; each internal setter writes Settings.Default.<X> + Save(). No Outlook, no
    exemption. HighConfidenceModeEnabled/Threshold are already covered by the existing
    AppQuickFilerSettingsTests.cs; P3-T3 covers the four remaining (MoveEntireConversation,
    SaveAttachments, SavePictures, SaveEmailCopy) using the same snapshot/restore pattern.

## Restricted / flagged seam

- AppFileSystemFolderPaths.MatchBestSpecialFolder(string) — TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs line 56
  - The method itself is pure LINQ/string matching over the `SpecialFolders`
    ConcurrentDictionary (null/empty -> null; else best (longest Value) whose Value is contained in
    `path`, returning its Key). NO [ExcludeFromCodeCoverage].
  - BLOCKER for pure isolation: `SpecialFolders` has a `protected` setter (line 289), and BOTH
    public constructors run `LoadFolders()` (line 16 / via LoadAsync line 31). `LoadFolders` reads
    `Environment.GetFolderPath(...)` and calls `CreateMissingPaths` -> `Directory.CreateDirectory`
    (FILESYSTEM WRITES). The only ctor that skips LoadFolders is `private AppFileSystemFolderPaths(bool async)`
    (line 26), which is inaccessible to the test project. Therefore any constructible instance runs
    LoadFolders() and mutates the filesystem, and there is no accessible way to set `SpecialFolders`
    to a controlled value without running LoadFolders.
  - CONCLUSION: MatchBestSpecialFolder cannot be exercised as "pure LINQ/string matching; no
    filesystem read" without a new production seam (a public/internal seam to set SpecialFolders or
    an internal LoadFolders-free constructor). Per the Flag-and-Stop rule this is recorded as a gap
    at P3-T2 in evidence/other/; no production seam is added. The empty/null SpecialFolders branch
    (returns null) is likewise not reachable without constructing the instance.

## Output Summary
AppStagingFilenames (ten properties + InitProp) and AppQuickFilerSettings (six properties; four
remaining for P3-T3) exist with the file/line references above, carry no [ExcludeFromCodeCoverage],
and are testable via the Settings.Default snapshot/restore pattern consistent with the existing
AppQuickFilerSettingsTests.cs — NO production injection seam required. MatchBestSpecialFolder is a
pure method but is NOT reachable in isolation because every accessible constructor performs
filesystem-mutating LoadFolders() and SpecialFolders has only a protected setter; this is a
Flag-and-Stop gap recorded at P3-T2.

# 2026-09-08-console-out-aggressors-and-banned-symbol-promotion (Spec)

- **Issue:** #826
- **Parent (optional):** epic `review-residuals-2026-09-08` (child F826, wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-09-09
- **Status:** Draft
- **Version:** 0.2

> Work Mode is `full-bug`. This document is the **sole authoritative acceptance-criteria source** for
> issue #826. No user-story.md exists for this feature and none may be created; a second file
> carrying `- [ ]` items would split the acceptance criteria and break the executor/reviewer
> check-off protocol.

> Path-formatting convention: every file this fix creates or modifies appears at least once as an
> inline code span with its full repository-relative path. Paths that this fix must **not** touch are
> deliberately written as bare prose. Do not "fix" that formatting — a downstream tool derives the
> change footprint by harvesting backticked path tokens, so backticking an out-of-scope path widens
> the apparent blast radius and un-backticking an in-scope path drops it from the footprint.

## Context
Residual process-wide console mutation and analyzer-severity work left in place by issue #811.
#811 eliminated the four `Console.Out` capture-and-assert sites in `UtilitiesCS.Test` by adding a
`TextWriter` seam to the four production members they exercised, and removed the propagating
save/restore in NLogTraceWriter_Test. It deliberately did not touch the test classes that replace
`Console.Out` and never restore it, the two production `Console.WriteLine` diagnostics in
OlTableExtensions.TableAccess.cs, or the RS0030 analyzer severity.

Environment:
- OS/version: Windows 11 Pro 10.0.26200 locally; GitHub Actions windows runner in CI
- Runtime: .NET Framework 4.8.1 test host, VSTest 18.9.0, MSTest 4.4.0, class-level parallelism
- Command/flags used: `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation`
- Data source or fixture: not applicable; these are static-source observations

Impact / Severity:
- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low: no current failure depends on any of these. Item 1 is latent risk that returns the moment any
future test captures `Console.Out`; item 3 means the next timing hack will be caught only if a
reviewer looks for it.

### Divergence from the GitHub issue body (three corrected facts)

The GitHub issue body for #826 will not be edited. Three of its statements are wrong against the
current tree, and this spec supersedes them. Each correction is derived in the research record
docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/research/console-out-and-banned-symbol-residuals.2026-09-08T23-58.md
(cited below as "research §N") and was re-verified during spec authoring.

1. **Population is 33 files carrying 34 live install statements, not "roughly 24 test classes."**
   Research §7.1 derives the population twice with distinct search expressions and identical member
   sets: 38 occurrences across 35 files. Two of those files are excluded — TaskMaster/ThisAddIn.cs
   line 103 is production code and is out of scope, and
   UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs line 31 is
   a commented-out call (`//Console.SetOut(new DebugTextWriter());`) that installs nothing. The
   remainder is 33 test files. The statement count is 34 rather than 33 because
   `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs` contains two
   `[TestClass]` types, each with its own initializer, at lines 61 and 476.

2. **The two production diagnostics are at lines 79 and 97, not 78 and 96** (research §0.3;
   re-verified in this session against the current tree). The plan must not anchor on either pair —
   see "Upstream dependency" below.

3. **Five test projects are affected, not six.** `UtilitiesCS.Test` (22 files), `QuickFiler.Test`
   (6), `ToDoModel.Test` (3), `TaskMaster.Test` (1), `VBFunctions.Test` (1) = 33 (research §7.2,
   double-derived). `UtilitiesCS` is a sixth project in scope, but only for item 2; it contributes no
   item-1 file.

## Repro & Evidence
Steps to Reproduce:
Read the cited sites. None of these produces a failure today, because after #811 no test asserts
on `Console.Out` content.

Expected:
1. A test does not mutate process-wide state it never restores.
2. Production code reports diagnostics through the logger, not through the console.
3. The banned-symbol list names the timing APIs the repository intends to prohibit.

Actual:
1. **33 test files install a `DebugTextWriter` with no restore (34 live statements).** They call
   `Console.SetOut(new DebugTextWriter())` from a `[ClassInitialize]` or `[TestInitialize]` method
   and never put the original writer back, so `Console.Out` is an arbitrary writer for the
   remainder of the run. These are aggressors rather than victims: none of them asserts on console
   content, so none can itself fail this way. After #811 they harm nothing, because no test captures
   `Console.Out` any more. They remain the reason `Console.Out` is not the console once the suite has
   started.

2. **Two production `Console.WriteLine` diagnostics** in
   `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, at lines 79 and 97 as of
   2026-09-09, both reading `Console.WriteLine($"Task timed out on try {counter}");` inside
   `GetTableInViewAsync`. Line 79 sits in the `else` branch of `catch (TaskCanceledException)`;
   line 97 sits in `catch (TimeoutException)`. Production code in this repository is supposed to use
   the logger; these two write to whatever writer the process currently holds, which after item 1 is
   a `DebugTextWriter`. Research §7.5 double-derives that the file contains exactly three `Console.`
   tokens: these two `WriteLine` calls plus `var target = writer ?? Console.Out;` in
   `EnumerateTable`, which is the #811 seam and is out of scope.

3. **RS0030 is held at `suggestion` severity** in `.editorconfig` line 548, and `BannedSymbols.txt`
   covers only `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep` and `Task.Delay`
   (7 DocID lines). `CancelAfter`, `WaitOne` and `new CancellationTokenSource(int)` are not banned at
   all. The severity is held down because the pre-existing banned-symbol call sites would otherwise
   break the build. The tracking comment at `.editorconfig` lines 546-547 cites **issue #181**, which
   is **closed** (title: "Feature: csharp-analyzer-stack-hardening", verified during spec authoring),
   so the comment points a reader at finished work as though it were the live tracking reference.

## Scope & Non-Goals
- In scope:
  - **Item 1** — delete the 34 live console-writer install statements across the 33 test files
    enumerated in "Write set" below, together with the 11 initializer methods that become empty or
    comment-only and the two `DebugTextWriter` fields described in "Proposed Fix".
  - **Item 2** — replace both `Console.WriteLine($"Task timed out on try {counter}");` statements in
    `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` with a `logger.Warn(...)`
    call, and add one deterministic regression test in a new file
    `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs` plus the
    single `<Compile Include>` entry it requires in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
  - **Item 3** — add eight DocID lines to `BannedSymbols.txt` at unchanged `suggestion` severity, and
    amend the tracking comment at `.editorconfig` lines 546-547.

- Out of scope / non-goals:
  - **Promoting `dotnet_diagnostic.RS0030.severity` above `suggestion`.** Unreachable in this
    feature; the mechanism is given in "Root Cause Analysis". Leaving the value at `suggestion` is a
    deliberate decision, pinned by AC11.
  - **Clearing the pre-existing banned-symbol call sites.** That is a `TimeProvider` migration across
    several production assemblies, not a bugfix.
  - **Banning `TimeoutAfter`** (justified in "Proposed Fix", item 3).
  - **Banning the parameterless `WaitHandle.WaitOne()` overload** (justified in "Proposed Fix",
    item 3).
  - **Any bulk-suppression mechanism** — `WarningsNotAsErrors`, `NoWarn`, a path-scoped
    `.editorconfig` section, a second banned-symbols file, or a `#pragma warning disable RS0030`
    sweep. All were evaluated and rejected (research §1.5).
  - **Removing unused `using` directives** left behind by item 1. No project in the solution sets
    `GenerateDocumentationFile` (research §3.6), so IDE0005 is not emitted by a command-line build and
    an orphaned `using` cannot fail any gate. Removal is optional and cosmetic; the plan does not
    require it and reviewers must not treat it as a defect either way.
  - **Splitting the oversized test files.** Thirteen of the 33 item-1 files already exceed the
    500-line limit in .claude/rules/general-code-change.md. That is pre-existing debt unrelated to
    #826 (recorded under "Report-only findings").
  - **Modifying CLAUDE.md, any file under .claude/rules/, or any file under .github/instructions/.**

- Explicitly excluded systems, integrations, or datasets:
  - **Sibling-owned files.** The paths below belong to other children of the epic
    review-residuals-2026-09-08 and must not be edited by this feature. They are written unbackticked
    on purpose so they stay out of this feature's change footprint:
    QuickFiler/Controllers/QfcItemController.FolderHandling.cs;
    QuickFiler/Controllers/QfcHomeController.cs;
    QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs;
    UtilitiesCS/Threading/ProgressViewer.cs; UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs;
    UtilitiesCS/OutlookObjects/Store/StoreWrapperController*.cs; QuickFiler/Viewers/Breadcrumb*;
    UtilitiesCS/NewtonsoftHelpers/SDIL Reader/**; UtilitiesCS.Test/Properties/AssemblyInfo.cs;
    UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs; UtilitiesCS/Threading/TimeOutTask.cs;
    UtilitiesCS/Extensions/DfDeedle.cs;
    UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs;
    scripts/vscode/Invoke-MSTestWithCoverage.*; and everything in
    UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs **except** the two
    `Console.WriteLine` statements.
  - **Naming trap, stated explicitly.** QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
    is sibling-owned and is **not** in the 33-file population. Four other files whose names begin
    `QfcHomeController` **are** in the population (`QfcHomeControllerTests.cs`,
    `QfcHomeControllerRunAsyncTests.cs`, `QfcHomeControllerPropertyTests.cs`,
    `QfcHomeControllerIterationTests.cs`), as are two `QfcFormController*` files. The filenames are
    similar; match on the full name.
  - **UtilitiesCS/Threading/TimeOutTask.cs is sibling-owned.** This is why the ten predicted
    `CancellationTokenSource` diagnostics inside it must be left alone. At `suggestion` severity they
    cost nothing today.

### Write set

Item 1 — 33 test files, one deleted statement each except where noted:

`VBFunctions.Test/ComputerInfo_Test.cs`,
`UtilitiesCS.Test/Threading/AppGlobalsConverterTests.cs`,
`UtilitiesCS.Test/Threading/AppGlobalsConverterTests_Unfinished.cs`,
`UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs`,
`UtilitiesCS.Test/Extensions/Frexp_Test.cs`,
`UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs`,
`UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs`,
`UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs`,
`UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs`,
`UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs` (two statements),
`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianSerializationHelper_Tests.cs`,
`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianPerformanceMeasurement_Tests.cs`,
`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests.cs`,
`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs`,
`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs`,
`UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs`,
`UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs`,
`UtilitiesCS.Test/NewtonsoftHelpers/WrapperScDictionaryTest.cs`,
`UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs`,
`UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs`,
`UtilitiesCS.Test/NewtonsoftHelpers/ScDictionaryConverter_Tests.cs`,
`UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs`,
`UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs`,
`TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs`,
`QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`,
`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`,
`QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs`,
`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`,
`QuickFiler.Test/Controllers/QfcFormControllerTests.cs`,
`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs`,
`ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs`,
`ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs` (field + assignment + call + commented block),
`ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs` (field + assignment + call +
commented block).

The `ToDoModel.Test/Data Model/` directory name contains a space. Any tooling invocation or project
reference must quote it.

Items 2 and 3 — five further files:

`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` (modified),
`UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs` (created),
`UtilitiesCS.Test/UtilitiesCS.Test.csproj` (one added line),
`BannedSymbols.txt` (eight added lines),
`.editorconfig` (comment amended; the severity value is not changed).

## Root Cause Analysis
Items 1 and 2 are long-standing conventions that predate the determinism work. `DebugTextWriter` was
installed so that Deedle's `Print()` output would land in the Visual Studio Debug window during
interactive debugging — see the comment at TaskMaster/ThisAddIn.cs line 101. The test-project copies
inherited that idiom without inheriting the reason for it.

Item 3 is a deliberate staged rollout recorded in .claude/rules/csharp.md under the severity-first
ordering invariant: new analyzer severities are set to `suggestion` before the analyzer is wired in,
because the type-check step runs `/p:TreatWarningsAsErrors=true` and would promote a `warning` to an
error. The rule text conditions promotion on "after legacy cleanup".

**The precise mechanism that makes promotion unreachable here, and the gate that would break.**
`BannedSymbols.txt` is supplied to the compiler as an `AdditionalFiles` item by 16 of the 18 projects
in the solution. BannedApiAnalyzers reports RS0030 once per call site resolving to a listed DocID. At
`suggestion` the diagnostic is emitted at info level, and `TreatWarningsAsErrors` promotes warnings
only, so RS0030 cannot fail a build today. Setting the severity to `warning` would make every RS0030
report a compiler warning, which toolchain step 3 —
`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
— and its CI mirror, the "Build with nullable warnings treated as errors" step in
.github/workflows/_build-nullable.yml, would promote to build errors on the pre-existing usages.

The analyzer workflow .github/workflows/_build-analyzers.yml passes no `TreatWarningsAsErrors` and
would **not** break. Naming the correct gate matters: the constraint is the nullable gate, not the
analyzer gate, which inverts the intuitive reading.

## Proposed Fix

### Design summary (what changes where):

- **Item 1 (removal, not restoration).** Delete the 34 live `Console.SetOut(new DebugTextWriter())`
  statements. Delete the 11 initializer methods that become empty or comment-only. Delete the two
  `DebugTextWriter` fields and their assignments in the `TreeNode` test files.
- **Item 2 (logger substitution).** Replace both console writes in
  `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` with
  `logger.Warn($"{nameof(GetTableInViewAsync)} timed out on try {counter}");`, and cover the
  `catch (TimeoutException)` branch with one new deterministic MSTest test.
- **Item 3 (list content only).** Add eight DocID lines to `BannedSymbols.txt` and rewrite the
  `.editorconfig` tracking comment. The severity value is untouched.

**Invariant established by this fix, in one sentence:** after the change, no test in this repository
installs a writer into `Console.Out`, and every timeout diagnostic emitted by `GetTableInViewAsync`
leaves the method through the log4net `logger` at `Warn` level rather than through process-global
console state — so the value written by a diagnostic is observable through a log appender and can no
longer depend on which test class last ran.

**Trace of one diagnostic value through the current code and the fix.** The path chosen has no guard
anywhere between the accepting call and the point where the value is lost.

1. **Accept point.** `GetTableInViewAsync` is entered with `counter` at its default. The guard at the
   top of the method (`view is null`) validates only that a table view exists; it does not validate,
   observe, or constrain anything about where a diagnostic will be written.
2. **Throw point.** The awaited COM acquisition exceeds the deadline and raises `TimeoutException`,
   caught by `catch (TimeoutException)` — line 97 as of 2026-09-09.
3. **Current absorption point.** The handler calls `Console.WriteLine($"Task timed out on try
   {counter}");`. `Console.Out` at that moment is a `DebugTextWriter`, whose backing
   `DebugOutStream` forwards `Write` to `Debug.Write` and whose `Read`, `Seek`, `Length` and
   `Position` all throw `InvalidOperationException` (research §3.2). The value is therefore
   unrecoverable: there is no buffer, no backing store, and no API by which any caller or test can
   read it back. Under a Release build with `DEBUG` undefined it goes nowhere at all. This location
   cannot report, because the sink it writes to is write-only and process-global.
4. **Where the fix moves the report.** The same handler calls `logger.Warn(...)`. `logger` is a
   `log4net.ILog` declared at UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs line 25 in the
   sibling partial of the same `public static partial class OlTableExtensions`, and is already used
   nine times inside `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` (lines 141,
   167, 210, 217, 238, 245, 286, 311, 318). log4net supports attaching an in-memory appender at
   runtime, so this boundary **can** report: a test can observe the record without touching the
   filesystem and without depending on process-global console state.

**Why neither half suffices alone.** Deleting the console-writer installs (item 1) does not make the
production diagnostic observable — it only changes which writer swallows it. Replacing the console
write with a logger call (item 2) does not stop 33 test files from mutating `Console.Out` for every
other piece of code in the process. Item 1 removes the aggressor; item 2 removes the dependence on
the victimized channel. Both are required for the invariant above to hold.

**Inverse constraint.** No existing `catch` clause may be widened, and no new `catch` may be added,
by this feature. The two edits are statement-for-statement substitutions inside catch clauses that
already exist. `GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException`
(UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs line 1324) asserts
`ThrowAsync<OperationCanceledException>()` — that is, it pins the behaviour that the exception
**escapes** the method. Broadening either catch to "be safe" would make that live test fail, and an
implementer must not do it.

### Boundaries and invariants to preserve:

- The timeout mechanics of `GetTableInViewAsync` — the deadline window, the retry counter, the
  `timeoutSourceFactory` seam, the exception types caught, and the control flow after each
  diagnostic — are owned by feature 825 and must be left exactly as this feature finds them.
- `dotnet_diagnostic.RS0030.severity = suggestion` on `.editorconfig` line 548 stays as-is.
- No test may assert on `Console.Out` content, before or after the change. Adding a test that asserts
  `Console.Out` is unmodified would itself depend on process-global state and would be order
  dependent under class-level parallelism, violating the Independence principle in
  .claude/rules/general-unit-test.md.

### Dependencies or blocked work:

**This feature depends on issue #825** and is the only child of the epic
review-residuals-2026-09-08 carrying a dependency edge. Both features edit `GetTableInViewAsync` in
`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`. The agreed partition: **825 owns
the 2000 ms deadline window and the method's timeout mechanics** and has been instructed to leave the
two `Console.WriteLine` statements byte-identical; **this feature owns exactly those two statements
and nothing else in that file.**

**Design constraint that follows.** At execution time 825 will already have merged and may have moved
these lines. The edit must therefore be anchored on the literal text
`Console.WriteLine($"Task timed out on try {counter}");` and its enclosing `catch` clause, **never on
a line number**. The line numbers 79 and 97 recorded in this document are observations dated
2026-09-09 and are expected to be stale by execution time. The surrounding timeout mechanics may
differ from what preparation observed; that difference is 825's work and is not a defect.

A second, weaker interaction exists with `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: other children
of the epic may also add `<Compile Include>` entries. Per the epic's own instruction that is
contention, not a dependency, resolved by unioning item lists at merge and never by a dependency
edge.

### Implementation strategy (what changes, not sequencing):

The three items are disjoint — item 1 touches only test files, item 2 touches one production file
plus one new test file and one project file, item 3 touches two text files — so no ordering hazard
exists among them.

#### Files/modules to change:

See "Write set" above: 33 item-1 test files, plus
`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`,
`UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs`,
`UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `BannedSymbols.txt` and `.editorconfig`.

#### Functions/classes/CLI commands impacted:

- `GetTableInViewAsync` in `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` — two
  statements substituted; no signature, control-flow or timeout change.
- 34 `[TestInitialize]` / `[ClassInitialize]` methods across the 33 item-1 files; 11 of them are
  deleted outright.
- No production API changes anywhere.

#### Item 1 — removal, and why not a restoring scope

Removal, not a restoring scope. Three reasons, strongest first:

1. **A restoring scope does not fix the defect; it multiplies it.** TaskMaster.runsettings and
   scripts/vscode/TaskMaster.cli.runsettings both set `<Workers>0</Workers>` and
   `<Scope>ClassLevel</Scope>`. `Console.SetOut` is process-global. A save/restore pair running
   concurrently across 33 classes interleaves: class A saves the writer class B just installed, then
   restores B's writer as if it were the original. That is strictly worse than the current
   unrestored install, which at least converges to a single stable writer. Issue #811 already removed
   this exact pattern from NLogTraceWriter_Test for this reason.
2. **The install serves no test purpose and no test can assert on it.** `DebugTextWriter` is a
   write-only `StreamWriter` over a stream whose `Read`, `Seek`, `Length` and `Position` all throw
   (research §3.2), so no test can physically read back what was written; and no test does. The only
   `Console.` token in the 1846-line file
   UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs is a comment at line 1640.
3. **Removal is strictly smaller.** It deletes lines and adds none.

**Hard constraint A — 11 initializers must be deleted along with the statement.** Research §3.5 gives
the exact table. In these methods the install is the only executable statement, so deleting the line
leaves an empty (or comment-only) method body, which is dead weight and must go with it:

| File | Initializer | Post-removal state |
|---|---|---|
| `VBFunctions.Test/ComputerInfo_Test.cs` | `[TestInitialize] Initialize()` | empty |
| `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs` | `[TestInitialize] TestInitialize()` | empty |
| `UtilitiesCS.Test/Extensions/Frexp_Test.cs` | `[TestInitialize] TestInitialize()` | empty |
| `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs` | `[TestInitialize] TestInitialize()` | empty |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs` | `[TestInitialize] TestInitialize()` | empty |
| `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs` | `[TestInitialize] TestInitialize()` | empty |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs` | `[TestInitialize]` of class 1 | empty |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs` | `[TestInitialize]` of class 2 | empty |
| `UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs` | `[TestInitialize] TestInitialize()` | comment-only — delete method **and** the orphaned comment |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs` | `[TestInitialize] TestInitialize()` | comment-only — delete method **and** the orphaned comment |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs` | `[TestInitialize] TestInitialize()` | comment-only — delete method **and** the orphaned comment |

The count "11" is a single-derivation figure taken from research §3.5 and is deliberately not
asserted as a number in the acceptance criteria; AC4 instead names the ten affected files and
requires a per-file check. The remaining 23 statements sit in initializers that construct
`MockRepository` instances, mocks and fixtures — for example
`UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs`,
`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` and
`UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs`. In those, delete
**only** the single line.

**Hard constraint B — the two `TreeNode` files need the field deleted too. This is the single most
likely way a careless implementation breaks the nullable gate.**
`ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs` and
`ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs` each declare
`private DebugTextWriter tw;`, assign it, and read it only in the `Console.SetOut(tw);` call. Verified
directly: in `TreeNodeTests.cs` the only occurrences of `tw` are line 16 (declaration), lines 21-22
(inside a commented-out `[ClassInitialize]` block), line 29 (assignment) and line 30 (the call); in
`TreeNodeTests_UnfinishedStubs.cs` they are lines 14, 19-20, 27 and 28.

- Deleting only the call leaves the field assigned but never read → **CS0414**.
- Deleting the call and the assignment but keeping the field leaves it never used → **CS0169**.

Both are **compiler** warnings, not analyzer diagnostics, so the `.editorconfig` `suggestion` ceiling
does not apply to them, and `/p:TreatWarningsAsErrors=true` promotes them to build errors in
toolchain step 3 and in the .github/workflows/_build-nullable.yml gate. **The field declaration, its
assignment, the `Console.SetOut(tw);` call, and the orphaned commented-out `[ClassInitialize]` block
must all be deleted together.** AC3 pins this to the type-check step passing, not to inspection.

#### Item 2 — the logging call, and why `Warn`

Replace both occurrences with
`logger.Warn($"{nameof(GetTableInViewAsync)} timed out on try {counter}");`. **No new field and no
new `using` is required**: `logger` is already in scope through the sibling partial (see the trace,
step 4). **The logging framework is log4net, not NLog** — any statement to the contrary in earlier
drafts of this document or the issue body is wrong.

`LogTableTiming` is the wrong vehicle: it is hard-wired to `logger.Debug`, which would bury a fault
below the default threshold, and it applies the `[Table timing]` message framing plus
`BuildTableTimingContext()` that belongs to feature 825's instrumentation channel. `Warn` matches
this file's own convention for the same class of event — line 217 warns that `GetTableAsync` failed
after the maximum attempts, line 238 warns on a `COMException`, and line 245 logs the follow-on retry
— so matching it is required by the General Code Change Policy's "match the existing style" rule.

Keeping both messages identical is acceptable and is the minimal change; differentiating the
`TaskCanceledException` and `TimeoutException` cases is a defensible small improvement and is left to
the implementer's judgement, provided both statements become `logger.Warn` calls.

#### Item 3 — banned symbols, at unchanged severity

Add exactly these eight DocID lines to `BannedSymbols.txt`, each with a `;<message>` suffix in the
existing file's style pointing the caller at `System.TimeProvider` / `FakeTimeProvider`:

```
M:System.Threading.CancellationTokenSource.CancelAfter(System.Int32)
M:System.Threading.CancellationTokenSource.CancelAfter(System.TimeSpan)
M:System.Threading.CancellationTokenSource.#ctor(System.Int32)
M:System.Threading.CancellationTokenSource.#ctor(System.TimeSpan)
M:System.Threading.WaitHandle.WaitOne(System.Int32)
M:System.Threading.WaitHandle.WaitOne(System.TimeSpan)
M:System.Threading.WaitHandle.WaitOne(System.Int32,System.Boolean)
M:System.Threading.WaitHandle.WaitOne(System.TimeSpan,System.Boolean)
```

The parameterless `CancellationTokenSource.#ctor()` is **not** a candidate and must not be listed;
157 parameterless constructions exist across 80 files and none of them carries a deadline
(research §7.3).

**Predicted new diagnostics (a prediction, not a measurement): 15, and 0 new build failures.**
Derivation: 1 from `CancelAfter` (test-only, QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs
line 169, the only textual hit in the repository); 13 from the `CancellationTokenSource(int)`
constructor, all production, ten of them inside the sibling-owned UtilitiesCS/Threading/TimeOutTask.cs
and the remainder in QuickFiler/Controllers/QfcQueue.cs and
UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs (research §7.3, double-derived); and 1
from `WaitOne(System.Int32)` at QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs line
57. Zero new build failures follows from the severity remaining `suggestion`: an info-level
diagnostic is not a warning, so `TreatWarningsAsErrors` has nothing to promote.

**Exclusion 1 — `TimeoutAfter` must not be banned.** The issue body names it; this spec closes it out
rather than silently omitting it. `TimeoutAfter` is **not a BCL member**. It is a repository-local
extension method with four overloads in UtilitiesCS/Threading/TimeOutTask.cs at lines 824, 862, 924
and 949, two of which accept a `TimeProvider` and are documented as the `FakeTimeProvider`
determinism seam. It is the remedy that every existing message in `BannedSymbols.txt` points callers
toward; banning it would ban the cure. Note that this document's own item-3 shortlist has always
omitted `TimeoutAfter` while the issue body includes it — this resolves that disagreement in the
spec's favour.

**Exclusion 2 — the parameterless `WaitHandle.WaitOne()` overload must not be banned**, even though
12 of the 13 current `WaitOne` call sites use it (research §7.4, double-derived). Three reasons:

- (a) It is a deterministic handshake on a signal, not a wall-clock deadline. Every existing entry in
  `BannedSymbols.txt` targets a wall-clock or nondeterministic source.
- (b) The 11 `AutoResetEvent _ready` sites are the repository's own cross-thread determinism idiom.
  Banning the overload would mark that idiom as prohibited while naming no sanctioned replacement.
- (c) The reason RS0030 cannot be promoted today is a backlog of pre-existing usages. Adding a symbol
  family that instantly contributes 12 more unfixable entries moves the promotion goal further away.

Revisit this exclusion if the repository adopts an async handshake idiom that can replace the
`AutoResetEvent` pattern.

**Amend the `.editorconfig` comment at lines 546-547.** It currently defers to issue #181, which is
closed. The amended comment must state the promotion precondition inline instead of deferring to a
closed issue, and must record the verified current surface. Suggested replacement for lines 546-547
(the exact wording is the implementer's, the required content is fixed):

```
# RS0030 is held at suggestion. Promotion to warning is blocked by exactly one
# precondition: the pre-existing banned-symbol call sites must be cleared first,
# because toolchain step 3 (msbuild ... /p:TreatWarningsAsErrors=true, mirrored by
# .github/workflows/_build-nullable.yml) promotes every warning to a build error.
# Verified textual surface, 2026-09-08: DateTime.Now 54, DateTime.UtcNow 20,
# Random.Shared 5, Thread.Sleep 15, Task.Delay 58 = 152 textual hits. Textual hits
# are not diagnostics; the previously recorded ~143 figure was a diagnostic count.
# Issue #181 is closed and is not a live tracking reference.
```

`dotnet_diagnostic.RS0030.severity = suggestion` on line 548 is not changed.

**Honesty requirement.** Adding a DocID while severity is `suggestion` produces **zero build
enforcement**. It changes IDE squiggles and nothing else. It does **not** make AC5-style timing-hack
constraints build-enforced, which is the motivation the issue states. The honest value of the change
is that it pre-stages the list, so the eventual promotion becomes a one-line severity change rather
than a list-design exercise. Any claim elsewhere in this document, in a plan derived from it, or in a
PR description, that this change delivers build enforcement is wrong.

#### Data flow and validation changes:

None. No input, output, schema or validation rule changes. Item 2 substitutes one statement for
another inside two existing catch clauses.

#### Error handling and logging updates:

The two timeout diagnostics move from `Console.Out` to the log4net `logger` at `Warn` level. No
exception is caught, swallowed, rethrown or re-typed differently. No catch clause is added, removed
or widened.

#### Rollback/feature-flag considerations (if applicable):

Not applicable. All three items are revertible by reverting the commit; no flag, migration or
staged rollout is involved.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- `BannedSymbols.txt` line format is `<DocID>;<message>`, one entry per line, no wildcard syntax and
  one line per overload. The file currently holds 7 lines and will hold 15.
- BannedApiAnalyzers resolves an invocation to the **declaring** type, so a call on a derived
  receiver such as `AutoResetEvent` is matched by the `System.Threading.WaitHandle` DocID.
- **Failure mode that shapes AC10:** a malformed or unresolvable DocID produces no diagnostic and no
  error. A merely wrong-but-well-formed DocID matches nothing, silently. Therefore neither "the build
  succeeded" nor "no RS0030 error appeared" is evidence that the eight DocIDs are correct — both
  observations are equally consistent with the DocIDs being garbage.

#### Required configuration keys and defaults:

`dotnet_diagnostic.RS0030.severity` remains `suggestion`. No new MSBuild property, no new
`AdditionalFiles` item, and no new analyzer package.

#### Backward-compatibility expectations:

No public API changes. The only externally visible behaviour change is that two production timeout
diagnostics now appear in the log4net output instead of on the process's current console writer.

#### Performance constraints (latency/throughput/memory):

None. A `logger.Warn` call on a bounded retry path that fires at most twice per acquisition has no
measurable cost, and the item-1 deletions remove work rather than add it.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - Feature 825 has merged before this feature executes, and the two `Console.WriteLine` statements
    survive it byte-identical, as 825 was instructed. If either statement is absent at execution
    time, stop and reconcile with 825 rather than reconstructing it.
  - The implementer can run the full toolchain locally (csharpier, msbuild, vstest).
- Constraints (budget, performance, compatibility):
  - The edits in `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` are located by
    literal text and enclosing `catch` clause, never by line number.
  - Adding the new test file requires exactly one `<Compile Include>` entry in
    `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. No other project-file change is permitted, and the
    existing entries must not be reordered or reformatted.
  - No coverage threshold, analyzer severity or policy requirement may be lowered, weakened or
    deleted to make a gate pass, and the pre-existing banned-symbol usages may not be bulk
    suppressed.
  - No sleep, retry or timing tolerance may be introduced to stabilize any test.
- External dependencies (services, libraries, releases):
  - `Microsoft.CodeAnalysis.BannedApiAnalyzers` 3.3.4 (already referenced); log4net (already
    referenced). No new package.

## Data / API / Config Impact
- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): two timeout diagnostics in `GetTableInViewAsync` move to
  `logger.Warn`. Operators who previously saw `Task timed out on try N` on the console or in the
  Visual Studio Debug window will find it in the log4net output at `Warn` level instead. The runtime
  add-in log is written under the add-in's `logs` directory.
- Compatibility notes (CLI flags, config schemas, versioning): `BannedSymbols.txt` grows from 7 to 15
  entries; `.editorconfig` gains no new key and changes no existing value.

## Test Strategy

> The three bullets below were seeded from the issue body and are **corrected here**. The original
> text proposed a restoring scope, an RS0030 promotion, and a "roughly 24 files / roughly 143
> usages" framing. All three are superseded.

- **Item 1 — corrected.** Remove the 34 unrestored `Console.SetOut(new DebugTextWriter())` installs
  across 33 files in five test projects. Do **not** introduce a restoring scope: process-global state
  under class-level parallelism makes save/restore interleave, and #811 already removed that pattern
  once for this reason.
- **Item 2 — corrected.** Route the two production diagnostics through the existing log4net `logger`
  at `Warn` level and delete the console writes, so `Console.WriteLine` in that file drops to 0.
- **Item 3 — corrected.** Add eight DocID lines at unchanged `suggestion` severity and amend the
  `.editorconfig` tracking comment. Do **not** promote RS0030, and do **not** attempt to clear the
  pre-existing usages; both are out of scope and the promotion is unreachable here.

- Regression tests to add or update:
  - One new file, `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs`,
    with a corresponding `<Compile Include>` entry in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. A
    new file is required because UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs
    is 1846 lines and its own comment at line 1640 states it is at its line ceiling.
  - **No existing test reaches either catch branch.** All four existing `GetTableInViewAsync` tests
    avoid them (research §4.4): one throws at the null-view guard before the `try`, two take the
    success path, and one asserts the cancellation exception escapes the method.
    UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsRetryTests.cs does not exercise the
    method at all, despite its name.
- Unit tests (MSTest + Moq + FluentAssertions) for the fixed behavior and boundaries:
  - **Arrange:** mock the Outlook explorer and table view following the established pattern in
    UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs around lines 1269-1298, and
    inject a `Func<int, CancellationTokenSource>` through the existing `timeoutSourceFactory`
    parameter so the timeout path is forced **without any wall-clock wait**.
  - **Act:** invoke `GetTableInViewAsync`.
  - **Assert:** the retry occurred (call count 2). Optionally attach an in-memory log4net appender
    and assert that a `Warn`-level record was emitted; if the appender proves brittle, assert the
    retry behaviour and let the logger substitution ride on branch coverage.
  - **Determinism:** no `Thread.Sleep`, no `Task.Delay`, no wall-clock wait, no temporary file, no
    external service. Coordinate the seam usage with feature 825, which owns the timeout mechanics.
- Item 1 needs no new test. It deletes test-infrastructure lines that assert nothing; the regression
  signal is the existing suite continuing to pass at the same count, captured as a before/after pass
  count in evidence.
- Edge cases and negative scenarios: the `TaskCanceledException` branch and the `TimeoutException`
  branch are separate; covering the `TimeoutException` branch is required, and covering both is
  preferred if the seam permits it without a wall-clock wait.
- Error handling and logging verification: assert that the diagnostic is emitted at `Warn`, not at
  `Debug` or `Error`, if a log appender is used.
- Coverage impact and targets for changed lines/modules:
  - **Item 1 changes no coverage figure.** scripts/vscode/Invoke-MSTestWithCoverage.ps1 line 99
    injects `.*\.Test\.dll$` as a run-time module exclusion, and coverage.config and
    TaskMaster.runsettings exclude only third-party modules. All 33 files compile into `*.Test.dll`
    assemblies, so they are outside the coverage denominator; neither numerator nor denominator
    moves.
  - **Item 2's two changed lines sit in currently-uncovered catch branches.** A 1-for-1 statement
    substitution would leave them newly-touched-but-uncovered, which is a finding under the
    "must not reduce coverage for the lines that were changed" rule. The new regression test must
    therefore raise coverage on the changed lines rather than leave them uncovered.
  - The repository line-coverage floor is `>= 85%` per .claude/rules/general-unit-test.md. This
    change must not lower the measured figure; the figure is recorded as evidence.
- Toolchain commands to run (format → lint → type-check → test), in this exact order, restarting from
  step 1 on any failure or auto-fix:
  1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

  Note that `BannedSymbols.txt` is a `.txt` file and `.editorconfig` is excluded from CSharpier, so
  neither is reformatted by step 1.
- Manual validation steps (if required): none.

### Open question for the executor — the RS0030 observation channel

It is **unverified whether a `suggestion`-severity diagnostic is emitted into a command-line msbuild
log at any verbosity.** One prior measurement in this repository points the wrong way: the archived
baseline at docs/features/archive/2026-06-28-qfc-banned-api-time-delay-seams-222/evidence/baseline/baseline-analyzer.md
records that RS0030 occurrences for eight known banned sites were "NOT surfaced as build warnings"
and that "suggestion-level diagnostics are not emitted by `-v:m`".

A candidate channel that the executor must evaluate before relying on it is the Roslyn SARIF error
log (the `/p:ErrorLog=` msbuild property, writing a SARIF file), which is expected to record
diagnostics below warning severity;
this expectation is **unverified**. If neither a higher msbuild verbosity nor the SARIF log surfaces
info-level diagnostics, AC10 must be satisfied by some other **positively observable** means — for
example a temporary scratch-branch build with the severity raised to `warning`, run only to observe
the diagnostics and never committed. AC10 must not be quietly downgraded to an absence check; an
absence check verifies nothing here.

## Acceptance Criteria

Each criterion below is independently verifiable by a third party re-running a stated command or
search, and each is capable of failing. Searches assert short, single-line tokens rather than
multi-word prose that line-wrapping would break.

- [x] **AC1 — Console writer installs are gone.** A repository-wide search of `*.cs` for the token
  `Console.SetOut(` returns hits in exactly two files, and neither is a test file modified by this
  feature: TaskMaster/ThisAddIn.cs (production, out of scope) and
  UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs (a
  commented-out line that installs nothing). Before the change the same search returns 38 occurrences
  across 35 files.
- [x] **AC2 — No residual writer references in the changed test files.** For each of the 33 files
  listed in the "Write set" section, a search for the token `DebugTextWriter` returns zero hits. This
  criterion is scoped to those 33 files only: UtilitiesCS/HelperClasses/Logging/DebugTextWriter.cs,
  UtilitiesCS.Test/HelperClasses/DebugTextLogger_Tests.cs, UtilitiesCS.Test/DeedleTests.cs,
  UtilitiesCS.Test/Extensions/DeedleTests.cs and TaskMaster/ThisAddIn.cs legitimately retain the
  token and are out of scope.
- [x] **AC3 — The CS0169/CS0414 hazard in the two `TreeNode` files is discharged, proven by the
  type-check step rather than by inspection.** In `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs`
  and `ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs`, a search for the token
  `tw` as a whole word returns zero hits (field declaration, assignment, `Console.SetOut(tw);` call
  and the commented-out `[ClassInitialize]` block are all removed), **and** toolchain step 3
  (`msbuild ... /p:TreatWarningsAsErrors=true`) exits 0 with zero `CS0169` and zero `CS0414`
  occurrences in its log.
- [x] **AC4 — Empty initializers are deleted with their statement.** In each of these ten files, a
  search for the token `TestInitialize` returns zero hits: `VBFunctions.Test/ComputerInfo_Test.cs`,
  `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs`, `UtilitiesCS.Test/Extensions/Frexp_Test.cs`,
  `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs`,
  `UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs`,
  `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs`,
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs`,
  `UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs`,
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs`,
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs`. Before the change
  the same search returns at least one hit in every one of those ten files, so the criterion is
  capable of failing. In the last three, the orphaned comment that formed the method's only
  remaining body is removed as well.
- [x] **AC5 — No console output remains in the table-access file.** A search of
  `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` for the token
  `Console.WriteLine` returns **0** occurrences. (A search for `Console.` still returns 1, the
  `writer ?? Console.Out` seam in `EnumerateTable`, which is out of scope and must remain.)
- [x] **AC6 — Both diagnostics are routed through the logger, anchored by text and confined to the
  two statements.** `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` contains two
  `logger.Warn` calls that did not exist before, one inside `catch (TaskCanceledException)` and one
  inside `catch (TimeoutException)`, and the diff for that file shows **only** those two statement
  substitutions: no change to the deadline window, the retry counter, the `timeoutSourceFactory`
  seam, the exception types caught, the control flow after each diagnostic, any other `catch` clause,
  or any `using` directive. The implementation matches the four-step trace in "Proposed Fix": the
  diagnostic that today reaches an unreadable `DebugTextWriter` reaches the log4net `logger` instead.
- [x] **AC7 — A named regression test covers the previously uncovered branch and passes.** A new test
  in `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs` forces entry
  into `catch (TimeoutException)` in `GetTableInViewAsync` through the existing `timeoutSourceFactory`
  seam, asserts the bounded retry occurred, passes under
  `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`, and contains no `Thread.Sleep`, no
  `Task.Delay`, no wall-clock wait and no temporary file. The test file path and test method name are
  recorded in evidence. Reverting the item-2 production edit alone must not be what makes this test
  pass or fail — the test pins the branch, and its purpose is to bring the changed lines under
  coverage.
- [x] **AC8 — The project file gains exactly one line.** `git diff` of
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj` shows exactly one added line, a single `<Compile
  Include>` entry naming the new test file, with zero removed lines and zero reordered or reformatted
  existing entries. No other project file in the solution is modified by this feature.
- [x] **AC9 — The eight DocID lines are present, in the file's existing format.** `BannedSymbols.txt`
  contains one line for each of the following eight DocIDs, and each line carries a `;` message
  suffix naming `TimeProvider`: `CancellationTokenSource.CancelAfter(System.Int32)`,
  `CancellationTokenSource.CancelAfter(System.TimeSpan)`,
  `CancellationTokenSource.#ctor(System.Int32)`, `CancellationTokenSource.#ctor(System.TimeSpan)`,
  `WaitHandle.WaitOne(System.Int32)`, `WaitHandle.WaitOne(System.TimeSpan)`,
  `WaitHandle.WaitOne(System.Int32,System.Boolean)`,
  `WaitHandle.WaitOne(System.TimeSpan,System.Boolean)`, each prefixed `M:System.Threading.`. The
  seven pre-existing lines are unchanged.
- [x] **AC10 — The DocIDs are proven to resolve, by positive observation, with a working control.**
  Because BannedApiAnalyzers silently ignores a malformed or unresolvable DocID, a clean build and
  the absence of an error each verify nothing. This criterion requires **positive observation** of
  RS0030 diagnostics at the enumerated sites below, captured in an evidence artifact naming the
  observation channel and the exact command used:
  - QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs line 169 (`CancelAfter`);
  - QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs line 57 (`WaitOne(0)`);
  - the thirteen `new CancellationTokenSource(<int>)` sites: ten in
    UtilitiesCS/Threading/TimeOutTask.cs, two in QuickFiler/Controllers/QfcQueue.cs, one in
    UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs.

  **Control (mandatory):** the same observation channel, in the same run, must also show RS0030
  diagnostics for an already-banned symbol with known current usages — for example
  `M:System.Threading.Tasks.Task.Delay(System.Int32)` or `P:System.DateTime.Now`. If the control
  produces zero diagnostics, the channel does not report info-level diagnostics and the observation
  is void; resolve it per the "Open question for the executor" section rather than downgrading this
  criterion to an absence check. The predicted total of 15 new diagnostics is a **prediction**: record
  the actual observed total as evidence, but the pass condition is that every enumerated site above
  is observed, not that the total equals 15.
- [x] **AC11 — RS0030 severity is unchanged, deliberately.** `.editorconfig` still contains the exact
  line `dotnet_diagnostic.RS0030.severity = suggestion`, and `git diff` of `.editorconfig` shows no
  change to any `severity` value on any line. Leaving this alone is a decision, not an omission.
- [x] **AC12 — The two documented exclusions hold.** `BannedSymbols.txt` contains **zero**
  occurrences of the token `TimeoutAfter`, and **zero** occurrences of the token
  `WaitHandle.WaitOne;` (the parameterless-overload DocID form, which carries no parentheses). Both
  exclusions are justified in "Proposed Fix" and neither may be added silently.
- [x] **AC13 — The tracking comment no longer points at closed work.** The BannedApiAnalyzers comment
  block in `.editorconfig` immediately above line 548 contains **zero** occurrences of the token
  `#181`, and states the promotion precondition inline together with the verified textual surface as
  of 2026-09-08.
- [x] **AC14 — Full toolchain pass, non-vacuous.** All four steps in the "Toolchain commands" list
  above pass in order in a single final pass, with the exact commands recorded in evidence. Both
  msbuild steps use `/t:Rebuild` and their logs contain **zero** occurrences of the token
  `Skipping target "CoreCompile"`, so the analyzer and nullable gates are proven to have compiled
  rather than skipped.
- [x] **AC15 — Coverage obligations met and recorded.** The changed production lines in
  `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` are covered by the AC7 test —
  they are uncovered before this change, so they must not be left newly-touched-but-uncovered. The
  repository line-coverage figure is recorded from the step-4 run and is not lower than the
  pre-change figure captured at the merge base; the `>= 85%` floor in
  .claude/rules/general-unit-test.md is reported against that measurement. No coverage exclusion,
  threshold or `[ExcludeFromCodeCoverage]` attribute is added anywhere in this change. Item 1 is
  expected to move no coverage figure at all, because all 33 files compile into `*.Test.dll`
  assemblies that are excluded from instrumentation at run time.
- [x] **AC16 — No out-of-scope file is touched.** `git diff --name-only` against the merge base lists
  only files named in the "Write set" section. In particular it contains no entry for CLAUDE.md, for
  any path under .claude/rules/ or .github/instructions/, for
  docs/features/epics/review-residuals-2026-09-08/, for this feature's issue.md or research file, or
  for any sibling-owned path listed under "Explicitly excluded systems" — including
  QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs and
  UtilitiesCS/Threading/TimeOutTask.cs.

## Risks & Mitigations
- Technical or operational risks:
  1. **CS0169/CS0414 breaking the nullable gate** if the two `TreeNode` files are edited partially.
     Highest-likelihood failure mode in this change.
  2. **Line-number drift from feature 825** causing the item-2 edit to land in the wrong place or to
     be reported as "already done".
  3. **A silently wrong DocID** producing no diagnostic and no error, so the addition looks
     successful while enforcing nothing at promotion time.
  4. **Project-file churn** in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` colliding with a sibling
     child of the same epic.
  5. **Deleting one line too many** in an initializer that also constructs mocks, breaking a test
     class that appears unrelated to this change.
- Mitigations and rollbacks:
  1. Hard constraint B plus AC3, which requires the type-check step to pass rather than inspection.
  2. Anchor the edit on the literal statement text and its enclosing `catch`, never on a line number;
     AC5 and AC6 verify the post-state without reference to line numbers.
  3. AC10's positive-observation requirement plus its mandatory control.
  4. One added line only, no reordering, verified by AC8; union item lists at merge, per the epic's
     contention rule.
  5. Research §3.5 partitions the 34 statements into the 11 delete-the-method cases and the 23
     delete-one-line cases; AC4 pins the first group and the full test run in AC14 catches the
     second. Re-verify each initializer's remaining body before deleting a method.
- Rollback: revert the commit. No flag, migration or data change is involved.

## Rollout & Follow-up
- Release/rollout steps: standard branch, PR and merge. No deployment step.
- Post-fix monitoring or clean-up tasks: none beyond the follow-ups below.

### Report-only findings (record here, take no action in this feature)

1. **`Directory.Build.props` exists at the repository root** (18 lines; it sets only
   `RxUseUnsupportedPackagesConfig`), and Directory.Build.targets exists alongside it. CLAUDE.md
   §C#1.3 states there is no Directory.Build.props. That claim is stale. **CLAUDE.md must not be
   modified by this feature**; file the correction separately. The `<Nullable>` half of the same
   CLAUDE.md sentence remains accurate.
2. **Thirteen of the 33 item-1 test files already exceed the 500-line limit** in
   .claude/rules/general-code-change.md. This is pre-existing debt unrelated to #826. Do not propose
   splitting them. Removal only reduces line counts, so no file can cross the ceiling because of this
   change.
3. **SVGControl.csproj and SVGControl.Test.csproj do not reference BannedSymbols.txt** via
   `<AdditionalFiles>`, while the other 16 projects in the solution do, so the ban list does not reach
   them. Both projects are in TaskMaster.sln. Recorded as a coverage gap in the analyzer wiring; do
   not fix it here.

### Follow-up work to file separately

- Clear the pre-existing banned-symbol call sites, then promote `dotnet_diagnostic.RS0030.severity`
  to `warning`. Record at filing time that ten of the thirteen `CancellationTokenSource(int)` sites
  are inside UtilitiesCS/Threading/TimeOutTask.cs, the repository's own timeout primitive, where
  constructing a deadline source is the legitimate job of the code; that promotion will need a
  documented, narrowly scoped suppression or an allow-list for those sites.
- Revisit the parameterless `WaitHandle.WaitOne()` exclusion if an async handshake idiom is adopted
  to replace the `AutoResetEvent _ready` pattern.
- Correct the stale `Directory.Build.props` claim in CLAUDE.md §C#1.3.
- Wire `BannedSymbols.txt` into SVGControl.csproj and SVGControl.Test.csproj.

- Links:
  - Issue: https://github.com/drmoisan/TaskMaster/issues/826
  - Upstream dependency: issue #825 (shared `GetTableInViewAsync` method region)
  - Predecessor: issue #811
  - Epic: docs/features/epics/review-residuals-2026-09-08/epic.md (child F826, wave 1)
  - Research: docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/research/console-out-and-banned-symbol-residuals.2026-09-08T23-58.md

# quickfiler-explorer-controller-latent-defects (Spec)

- **Issue:** #449
- **Parent (optional):** epic `quickfiler-suite-determinism-foundation` (wave 0, complexity band C3)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-21T18-35
- **Status:** Approved
- **Version:** 1.0

> **Work mode `full-bug`.** Per `.claude/skills/acceptance-criteria-tracking/SKILL.md`, this file is the
> sole authoritative acceptance-criteria source for issue #449. No `user-story.md` exists for this
> issue and none is to be created: the requirements are defect-driven and support no user story.
> `issue.md` carries the original early-draft criteria; the `## Acceptance Criteria` section below
> supersedes them and is the section executors and reviewers check off.

## Context

Three independent items in `QuickFiler/Controllers/QfcExplorerController.cs`, all found by reading
during the F6 per-file coverage research for issue #435 and none fixable there, because F6's
acceptance criteria forbid behavior changes:

1. `ExplConvView_Cleanup()` is declared on the public interface `IQfcExplorerController` and its only
   implementation throws `NotImplementedException`.
2. The private helper `NavigateToOutlookFolder(MailItem)` re-resolves the active Explorer through
   `_globals.Ol.App.ActiveExplorer()` instead of reusing the constructor-captured `_activeExplorer`,
   so the method's guard and its assignment can address different `Explorer` objects.
3. A 139-line `#region Email Sorting To Rewrite` holds six private/internal statics that are
   duplicated from maintained copies in `UtilitiesCS` and `ToDoModel` and are unreachable from every
   compiled entry point. Two further latent defects sit inside that unreachable block.

The authoritative requirements mirror is `issue.md` in this folder. The primary evidence source is
`research/qfc-explorer-controller-defects.2026-08-21T18-20.md` (1,039 lines), which re-derived every
line number in this worktree. Per the epic's "Known-Stale Potential-Document References" constraint,
no `file:line` citation was carried from the potential document without re-derivation; every citation
in this spec was confirmed against the research artifact or re-read directly from disk.

## Repro & Evidence

- **Steps to reproduce (with data/flags/inputs):** None of the three items is reachable from a normal
  Outlook session today, so there is no user-facing repro. Each is reproduced by static evidence and,
  for defect 2, by a constructible unit test:
  - **Defect 1** — call any `IQfcExplorerController.ExplConvView_Cleanup()` implementation. The single
    implementer (`QuickFiler/Controllers/QfcExplorerController.cs:61-64`) throws
    `NotImplementedException` at line 63 unconditionally. No compiled caller exists, so the throw is
    latent.
  - **Defect 2** — construct `QfcExplorerController`, then change the process's active Explorer, then
    call `OpenQFItem(mailItem)` with a mail item whose parent folder differs from the captured
    Explorer's current folder. `QuickFiler.Test` reproduces this deterministically with
    `SetupSequence` on `Outlook.Application.ActiveExplorer()`; see Test Strategy.
  - **Defect 3** — no repro is possible. The region is unreachable; see Root Cause Analysis.
- **Expected vs actual behavior:**
  - Defect 1: expected either working cleanup semantics or no such contract member; actual is a
    declared contract member that fails at runtime for its first caller.
  - Defect 2: expected the guard at
    `QuickFiler/Controllers/QfcExplorerController.cs:135-137` and the assignment at line 140 to read
    and write the same `Explorer`; actual is that line 136 reads
    `_activeExplorer.CurrentFolder.FolderPath` while line 140 writes
    `_globals.Ol.App.ActiveExplorer().CurrentFolder`, which is a freshly resolved and possibly
    different object.
  - Defect 3: expected one maintained copy of each helper; actual is three independent copies, of
    which the `QuickFiler` copy is dead and carries two defects of its own.
- **Logs/screenshots/error snippets:** None available. No live Outlook process exists in this
  environment, and the affected paths produce no log output — the file's `log4net` logger at
  `QuickFiler/Controllers/QfcExplorerController.cs:23-25` is declared and never referenced anywhere in
  the file.
- **Frequency / determinism (always, intermittent, data-dependent):**
  - Defect 1: deterministic on any call; zero calls exist today.
  - Defect 2: the redundant COM round-trip is deterministic on every `NavigateToOutlookFolder` call
    that enters the guard; the correctness hazard is data-dependent on whether the active Explorer
    changed between construction and the call.
  - Defect 3: never fires. Unreachable.

## Scope & Non-Goals

### In scope

- `QuickFiler/Interfaces/IQfcExplorerController.cs` — remove line 12
  (`void ExplConvView_Cleanup();`). 15 lines to 14.
- `QuickFiler/Controllers/QfcExplorerController.cs` — remove lines 60-64 (the `//PRIORITY:` comment and
  the throwing implementation); change line 140 to use `_activeExplorer`; delete lines 183-321 (the
  `#region Email Sorting To Rewrite`); remove the class-level `[ExcludeFromCodeCoverage]` at line 20 and
  add an injectable modal-dialog seam consumed by line 168; remove nine orphaned `using` directives.
  323 lines to approximately 179.
- `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` — new file. Must remain under 500 lines.
- `QuickFiler.Test/QuickFiler.Test.csproj` — exactly one appended `<Compile Include>` line. 484 to 485.
- `docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449/evidence/**` —
  regression-testing dossiers, QA-gate output, coverage figures.

### Out of scope / non-goals

- **Implementing** `ExplConvView_Cleanup` semantics. See decision D1.
- **Fixing** the two latent defects inside the dead region (transposed `Path.Combine` arguments; a
  `null` `ref string[]` written into). They are deleted, not fixed. See decision D3.
- **Consolidating** the three copies of the six duplicated helpers.
  `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (1,429 lines) and
  `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` (465 lines) are the surviving
  maintained copies; they carry their own tests in `UtilitiesCS.Test` and need no edit. Consolidation is
  a separate, larger refactor and would drag a pre-existing 500-line-cap violation into a defect-fix
  pull request.
- **Splitting** `SortEmail.cs` (2.9x the cap) or `QuickFiler/Legacy/QuickFileController.cs` (1,065
  lines, 2.1x the cap). Both are pre-existing violations, neither is edited, and neither appears in the
  diff. `QuickFileController.cs` is read-only reference material and is not compiled. The Bugfix
  Workflow forbids widening scope; if a split is judged worth tracking it belongs in a new issue filed
  after this child merges, and the epic forbids any child writing under `docs/features/potential/**`.
- **Editing `QuickFiler/Notes/notes_interfaces.cs`**, even though it declares a duplicate
  `IQfcExplorerController` carrying `ExplConvView_Cleanup` at `:52-59`. It is not compiled and is
  outside this issue's file set.
- **Editing `QuickFiler/QuickFiler.csproj`.** No production project-file edit is required: the dead
  region is inside an already-compiled file, and the uncompiled `Legacy/` and `Notes/` files have no
  compile entries to remove.
- **Editing anything under `.claude/**`.** That tree is push-down-owned per the epic's Hard Constraints.
  Where this spec cites a rule file, the citation is the policy the fix is measured against, not an
  edit target.
- **Correcting the catch-asymmetry in the legacy `ExplConvView_Cleanup` body.** Under D1 the code is not
  imported, so the question is moot; the analysis is preserved below as knowledge.
- **Touching the `Form1` region of `QuickFiler.Test.csproj`** (`:161-166`) or the `Form1.resx`
  `EmbeddedResource` (`:180-182`). Sibling child #491 owns those lines exclusively.

### Explicitly excluded systems, integrations, or datasets

- No live Outlook process, no live WinForms form, no message pump, no temporary files, and no
  filesystem or network access in any test added by this issue.
- No Python toolchain step. There is no `scripts/dev_tools/` and no Poetry manifest in this repository
  (verified). Any skill or plan step naming `poetry run python -m scripts.dev_tools.*` is unrunnable by
  absence and must be reported as such, never fabricated and never silently skipped.

## Root Cause Analysis

- **Current hypothesis or confirmed root cause:**
  - **Defect 1 — confirmed.** The member is a stub that was never implemented. The `//PRIORITY:` comment
    at `QuickFiler/Controllers/QfcExplorerController.cs:60` marks it as known-incomplete work carried
    forward from the uncompiled legacy controller. The contract was declared before the behavior
    existed, and nothing has ever called it.
  - **Defect 2 — confirmed.** The code was ported from
    `QuickFiler/Legacy/QuickFileController.cs`, which used the same `_globals`-rooted expression, and the
    modern type's constructor capture of `_activeExplorer`
    (`QuickFiler/Controllers/QfcExplorerController.cs:35`) was introduced without updating this one call
    site. Line 140 is the only re-resolution left in the file; the other five `_globals` uses are the
    constructor assignment (34), the authoritative capture (35), the field declaration (40), a settings
    read `_globals.Ol.ViewWide` (90), and a commented-out line (162).
  - **Defect 3 — confirmed.** The region is a copy of helpers that were later given maintained homes in
    `UtilitiesCS` and `ToDoModel`. Its entry point
    (`WriteCSV_StartNewFileIfDoesNotExist`, declared at line 216) is itself uncalled, so the whole block
    is a closed island. Three of the six statics (`SanitizeArrayLineTSV` at 185, `SaveMessageAsMSG` at
    272, `GetCurrentExplorerFolder` at 278) have zero call sites even inside the region.
- **Signals/evidence supporting it:**
  - **Defect 1.** A repository-wide search for `ExplConvView_Cleanup` across `*.cs` returns five hits:
    `QuickFiler/Interfaces/IQfcExplorerController.cs:12` (declaration),
    `QuickFiler/Controllers/QfcExplorerController.cs:61` (the throwing implementation),
    `QuickFiler/Legacy/QuickFileController.cs:673` and `:851` (not compiled), and
    `QuickFiler/Notes/notes_interfaces.cs:58` (not compiled). No file under `QuickFiler.Test` sets up or
    verifies the member on any `Mock<IQfcExplorerController>`. `QfcExplorerController` is the only
    implementer.
  - **Defect 2.** Line 136 reads `_activeExplorer.CurrentFolder.FolderPath`; line 140 writes
    `_globals.Ol.App.ActiveExplorer().CurrentFolder`. `_activeExplorer` is assigned exactly once (line
    35) and never reassigned. Nothing in the type subscribes to Outlook Explorer lifecycle events and no
    member accepts a replacement Explorer. Every other COM operation in the type already uses
    `_activeExplorer`: lines 57, 74, 77, 81, 127, 136, 141, 152, 156, 158, 159.
  - **Defect 3.** All in-file references to the six statics fall inside lines 183-321 (call sites at 193,
    241, 264). Every external reference binds to an independent copy: `SortEmail.cs` declares its own at
    `:1092`, `:1344`, `:1361`, `:1374`, `:1407`; `EmailFiler.cs` at `:211`, `:224`;
    `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` declares all six at `:255`, `:273`, `:285`,
    `:317`, `:350`, `:355`; and `TaskMaster/AppGlobals/AppOlObjects.cs:279` is explicitly type-qualified
    to `SortEmail`. **No file under `QuickFiler.Test` references any of the six**, including the
    `internal static StripTabsCrLf` at line 203 that `InternalsVisibleTo` would otherwise expose. The
    only test references in the repository are in
    `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` and
    `UtilitiesCS.Test/EmailIntelligence/EmailFiler_Tests.cs`, all against the `UtilitiesCS` copies.
  - **Broadened deletion-safety fact.** No file under `QuickFiler/Legacy/` is compiled at all:
    `QuickFiler/QuickFiler.csproj` contains zero `<Compile Include="Legacy\` entries. The same holds for
    `QuickFiler/Notes/`.
- **Affected components/modules (paths, services, pipelines):**
  - `QuickFiler/Controllers/QfcExplorerController.cs` (all three defects).
  - `QuickFiler/Interfaces/IQfcExplorerController.cs` (defect 1 contract).
  - `QuickFiler.Test` (new test file plus one project-file entry).
  - No other production assembly is affected. `QfcExplorerController` is `internal` to `QuickFiler`, and
    the concrete class is constructed at exactly two production sites —
    `QuickFiler/Controllers/QfcHomeController.cs:182` and
    `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:155` — both behind a replaceable
    factory delegate. **Neither call site changes.**

## Proposed Fix

Seven decisions were settled by the orchestrator before this spec was authored, on the evidence cited
in each. They are recorded here as decided, not as open questions.

### Design summary (what changes where)

| ID | Decision | Files |
| --- | --- | --- |
| **D1** | Remove `ExplConvView_Cleanup` from the contract and the implementation. Preserve the legacy semantics in this spec. | `IQfcExplorerController.cs`, `QfcExplorerController.cs` |
| **D2** | One-line fix at `QfcExplorerController.cs:140` to use `_activeExplorer`. | `QfcExplorerController.cs` |
| **D3** | Delete lines 183-321 unconditionally. | `QfcExplorerController.cs` |
| **D4** | Remove nine orphaned `using` directives as labelled hygiene. | `QfcExplorerController.cs` |
| **D5** | Remove the class-level `[ExcludeFromCodeCoverage]` and add an injectable modal-dialog seam. | `QfcExplorerController.cs` |
| **D6** | New test file plus exactly one appended `<Compile Include>` line after csproj line 119. | `QfcExplorerControllerTests.cs` (new), `QuickFiler.Test.csproj` |
| **D7** | Defect 2 carries the `[expect-fail]` regression test; defects 1 and 3 carry `fail-before-exception` dossiers. | `evidence/regression-testing/**` |

#### D1 — `ExplConvView_Cleanup` is REMOVED, not implemented

Delete `void ExplConvView_Cleanup();` from `QuickFiler/Interfaces/IQfcExplorerController.cs:12`, and
delete the implementation plus its `//PRIORITY:` comment from
`QuickFiler/Controllers/QfcExplorerController.cs:60-64`.

Rationale (research §1.3 and §1.4):

1. **Zero compiled callers, zero mock setups, exactly one implementer.** The evidence is enumerated
   under Root Cause Analysis. The general policy's compatibility clause ("Avoid breaking public APIs. If
   a breaking change is necessary, update all callers in-repo and call it out clearly") is satisfied:
   there are no callers, and the break is called out here and in the pull-request body.
2. **A verbatim port is not achievable.** Two independent blockers:
   - The legacy body's `catch (System.Exception)` swallows silently and adds nothing, which violates the
     broad-catch prohibition in `.claude/rules/general-code-change.md` ("Do not use broad catch-all
     handlers unless you immediately re-raise or propagate with added context") and `CLAUDE.md` C#4.1.
   - The port would import a reachable uncaught throw. `Views[_objViewMem]` sits **outside** the legacy
     `try`, and `_objViewMem` (`QfcExplorerController.cs:44`, `private string`, default `null`) is
     assigned in exactly one place in the modern type — `ExplConvView_ToggleOff()` at lines 88-90. The
     legacy type additionally initialised it in its constructor
     (`QuickFiler/Legacy/QuickFileController.cs:145-147`); the modern constructor (lines 27-37) does
     not. A first call made before any toggle therefore indexes `Views` with `null`, from outside the
     protected region.
3. **Correcting both blockers is not a port but new behavior.** It means authoring roughly 20 lines of
   previously nonexistent production behavior, plus tests asserting behavior no production path
   consumes, for an API with zero callers. The Bugfix Workflow scopes that out: "Change only what is
   needed to make the failing test pass"; "If you uncover deeper design problems, open a new issue
   instead of widening scope."
4. **Policy alignment.** `CLAUDE.md` §4.2 and C#5.2 both direct that the public surface be small and
   intentional. Removal also converts the failure mode from a runtime `NotImplementedException` into a
   compile error at authoring time for any future caller.

Consequential edit: removal orphans `using System;` at line 1, because line 63's
`NotImplementedException` is its last consumer. See D4.

#### D2 — defect 2 is a one-line fix

At `QuickFiler/Controllers/QfcExplorerController.cs:140`, inside the private helper
`NavigateToOutlookFolder(MailItem)` (lines 133-143), replace

```csharp
_globals.Ol.App.ActiveExplorer().CurrentFolder = (MAPIFolder)mailItem.Parent;
```

with

```csharp
_activeExplorer.CurrentFolder = (MAPIFolder)mailItem.Parent;
```

Rationale, correctness first (research §2.1 through §2.3):

1. **Guard and assignment must address the same object.** Line 136 reads
   `_activeExplorer.CurrentFolder.FolderPath` and decides whether to navigate; line 140 writes a
   possibly different Explorer's `CurrentFolder`. As written, the decision and the action can apply to
   two different `Explorer` instances, which is the internal-inconsistency hazard the issue describes.
   The fix makes the read and the write address one object. This is the primary argument.
2. **The saved COM round-trip is a secondary benefit**, not the reason for the change.
3. **No behavioral dependency on the fresh call exists.** `_activeExplorer` is assigned once at line 35
   and never reassigned; the type subscribes to no Explorer lifecycle event and exposes no member that
   accepts a replacement Explorer; and every other COM operation in the type already uses
   `_activeExplorer`. **Line 140 is the only re-resolution in the file.** The acceptance criterion's
   alternative branch — "or the reason a fresh `ActiveExplorer()` call is required is documented in
   code" — therefore does not apply, and no in-code justification is added.

**Correction to the potential document, recorded here.** The document places the second
`ActiveExplorer()` call directly in `OpenQFItem`. It is in the private helper
`NavigateToOutlookFolder(MailItem)` at lines 133-143; `OpenQFItem` (lines 146-181) reaches it via line
149. The remedy is unchanged; the location statement is corrected.

#### D3 — the dead region is deleted unconditionally

Delete `QuickFiler/Controllers/QfcExplorerController.cs:183-321` — the `#region Email Sorting To
Rewrite`, 139 lines, `#region` at 183 and `#endregion` at 321.

Rationale (research §3): all six statics are referenced only inside that region; every external
reference binds to an independent copy in `SortEmail.cs`, `EmailFiler.cs`, or
`ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`; and **no file under `QuickFiler.Test`
references any of the six**, including the `internal static StripTabsCrLf` at line 203 that
`[assembly: InternalsVisibleTo("QuickFiler.Test")]`
(`QuickFiler/Properties/AssemblyInfo.cs:5`) would otherwise expose. No test edit is required, and no
`QuickFiler` production file other than `QfcExplorerController.cs` is affected.

The two latent defects inside the block are **deleted rather than fixed**:

- `WriteCSV_StartNewFileIfDoesNotExist` at line 223 calls
  `File.Exists(Path.Combine(strFileName, strFileLocation))`, transposing the arguments relative to line
  242's `FileIO2.WriteTextFile(strFileName, strOutput, folderpath: strFileLocation)`.
- `SanitizeArray` receives `strOutput`, initialised to `null` at line 221 and passed by `ref`, and
  writes `strOutput[j]` at line 259 without allocating, which would throw `NullReferenceException` if
  reached.

Neither can fire today, and fixing unreachable code would be a change with no observable effect.

**Broadened fact:** no file under `QuickFiler/Legacy/` is compiled at all —
`QuickFiler/QuickFiler.csproj` has zero `<Compile Include="Legacy\` entries. This strengthens the
deletion-safety argument and removes any residual doubt about the other legacy files that reference
`ExplConvView_*` members.

#### D4 — orphaned `using` directives are removed as labelled hygiene

Nine of the sixteen directives in the using block (lines 1-16) become or already are unused. Per
research §4.1 and §4.2, with D1, D3, and D5 applied:

| Line | Directive | Disposition | Cause |
| --- | --- | --- | --- |
| 1 | `using System;` | Remove | Orphaned by D1 — line 63's `NotImplementedException` is its last consumer. `System.Reflection.MethodBase` (24) and `log4net.ILog` (23) are fully qualified. |
| 2 | `using System.Collections.Generic;` | Remove | Orphaned by D3 — only use is `IList<MailItem>` at 272. |
| 3 | `using System.Diagnostics;` | Remove | Orphaned by D3 — only use is `Debug.WriteLine` at 253. |
| 4 | `using System.Diagnostics.CodeAnalysis;` | Remove | Orphaned by D5 — the only `[ExcludeFromCodeCoverage]` is at line 20 and is removed. |
| 5 | `using System.IO;` | Remove | Orphaned by D3 — only uses are `File.Exists` and `Path.Combine` at 223. |
| 6 | `using System.Linq;` | Remove | Orphaned by D3 — only uses are at 192-194 and 263-265. |
| 7 | `using System.Text;` | Remove | **Already unused today on `main`** — no `StringBuilder` and no `Encoding` anywhere in the file, including the dead region. |
| 8 | `using System.Text.RegularExpressions;` | Remove | Orphaned by D3 — only uses are `Regex` at 205 and 209. |
| 9 | `using System.Threading.Tasks;` | Retain | `Task` at 146, 154, 158, 159, 180. |
| 10 | `using System.Windows.Forms;` | Retain | `DialogResult` (168), `MessageBox` (168), `MessageBoxButtons` (171), `MessageBoxIcon` (172). |
| 11 | `using Microsoft.Office.Interop.Outlook;` | Retain | `Explorer` (42), `MailItem` (133, 146), `MAPIFolder` (136, 140), `Views` (111), `OlViewSaveOption` (99). |
| 12 | `using QuickFiler.Interfaces;` | Retain | `IQfcExplorerController` (21), `IFilerHomeController` (30, 41). |
| 13 | `using ToDoModel;` | Remove | **Already unused today on `main`** — `QfEnums` resolves by enclosing-namespace lookup from `QuickFiler.Controllers`, not from this directive. |
| 14 | `using UtilitiesCS;` | Retain | `IApplicationGlobals` (29, 40), `AutoFile` (141, 152). |
| 15 | `using UtilitiesCS.OutlookExtensions;` | Remove | **Already unused today on `main`** — `IsInitialized` and `SliceRow` live in the root `UtilitiesCS` namespace (`UtilitiesCS/Extensions/ArrayExtensions.cs`), and `GetPressedMso` / `IsItemSelectableInView` are native PIA members, not extension methods. |
| 16 | `using Outlook = Microsoft.Office.Interop.Outlook;` | Retain | `Outlook.View` at 43, 45, 77, 93, 97, 101, 108, 110, 112, 123. |

**This is hygiene, not a gate fix, and must be labelled as such in the pull-request body so a reviewer
does not read it as an unrelated refactor.** An orphaned `using` fails neither gate in this repository:

- `IDE0005`'s analyzer is not wired into these non-SDK projects. `QuickFiler/QuickFiler.csproj` is a
  legacy non-SDK project targeting `v4.8.1`, and its `<Analyzer Include>` set (lines 582-591) is
  Meziantou, Roslynator, AsyncFixer, `Microsoft.CodeAnalysis.BannedApiAnalyzers`, and
  `SonarAnalyzer.CSharp`. Neither `Microsoft.CodeAnalysis.NetAnalyzers` nor
  `Microsoft.CodeAnalysis.CSharp.CodeStyle` — the packages carrying `IDE0005` — is referenced, and the
  command-line `/p:EnableNETAnalyzers=true` and `/p:EnforceCodeStyleInBuild=true` properties are
  SDK-project properties that do not inject an analyzer into a non-SDK project.
- No `IDE0005` severity is configured in the repo-root `.editorconfig`, and there is no `.globalconfig`.
- `CS8019` ("unnecessary using directive") is a hidden diagnostic, so `/p:TreatWarningsAsErrors=true`
  does not promote it.
- Three of the directives (lines 7, 13, 15) are already unused on green `main`, which is direct
  empirical confirmation that no wired analyzer reports them at warning severity.

The removal is **self-verifying**: if any directive is in fact required, the analyzer build fails with
CS0246 or CS1061 and the executor restores it. Removal is low-risk; retention is zero-risk.

#### D5 — the class-level `[ExcludeFromCodeCoverage]` is removed and replaced by an injectable dialog seam

This decision **overrides research §6.4**, which recommended narrowing the attribute onto `OpenQFItem`.
The override and its reasoning are recorded here.

Changes:

1. Remove `[ExcludeFromCodeCoverage]` from `QuickFiler/Controllers/QfcExplorerController.cs:20`. The
   attribute is **pre-existing**, added 2026-06-13 in commit `a564add0d`; it is not introduced by this
   change.
2. Add an injectable modal-dialog seam on the class, defaulting to the production call, so the
   not-in-view branch can be exercised headlessly. The intended shape is an `internal` settable member
   whose default is `MessageBox.Show`, matching the repository's existing settable-delegate seam idiom —
   for example `QfcHomeController.QfcExplorerControllerLoader` at
   `QuickFiler/Controllers/QfcHomeController.cs:175-182`, an `internal Func<...> { get; set; }` with a
   production-constructing default. Change the call at line 168 to go through the seam.
3. `mailItem.Display()` at line 176 needs no seam: `MailItem` is already mocked in this repository's
   tests.
4. After the change **no `[ExcludeFromCodeCoverage]` attribute remains anywhere in the file**, which is
   why line 4's `using System.Diagnostics.CodeAnalysis;` is orphaned under D4.

Rationale (research §6.1, §6.2, §6.3):

- **(i) The potential document's coverage-denominator claim is false.** It asserts that deleting the
  dead region "removes roughly 139 lines of uncoverable filesystem-I/O code from the coverage
  denominator." The class-level attribute makes the entire class invisible to the Cobertura report, so
  those lines are already absent from the denominator and the deletion changes it by exactly zero. The
  tooling evidence is in-repo: `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1:217-222`
  states as the premise of the whole filter that an exempt member emits no `<method>` element, and a
  class-level attribute suppresses every member. `coverage.config` (24 lines) excludes only third-party
  module paths and contains no `QuickFiler` entry, and `Directory.Build.targets` (30 lines) has no
  coverage content, so the attribute is the only mechanism in play. The **genuine** benefits of the
  deletion are de-duplication from the maintained `UtilitiesCS` copies, removal of two latent defects,
  and a 139-line reduction toward the 500-line cap.
- **(ii) `CLAUDE.md` UT2 clause (c) does not reach this class, on two independent grounds.** Clause (c)
  exempts "Outlook Interop event handler classes ... that directly depend on
  `Microsoft.Office.Interop.Outlook.Application`, `MailItem`, `Store`, or `MAPIFolder` **without an
  injectable seam**." First, `QfcExplorerController` is not an event handler class: it subscribes to no
  Outlook event, wires no `Explorer` or `Application` event, and declares no handler method. Second, it
  has an injectable seam — `IApplicationGlobals` is constructor-injected at line 29 and every COM object
  it touches is reached through that seam or through the `Explorer` captured from it at line 35. Clause
  (c)'s own counter-clause points the same way: "Testable seams within otherwise-COM-bound assemblies
  ... are explicitly NOT exempt."
- **(iii) `.claude/rules/general-unit-test.md` forbids the exclusion outright** ("No production file may
  be excluded from coverage measurement") and prescribes the remedy this decision adopts: "extract all
  logic into host-neutral, testable modules and leave only the thinnest possible wiring in the
  host-bound entry point." The dialog seam is exactly that. `CLAUDE.md` UT2 treats a source attribute
  and a `coverage.config` exclude as the same instrument, so the attribute is within that rule's reach.
- **(iv) Without this change, every test written for this issue earns zero measured coverage**, and the
  epic NFR "Coverage of `QuickFiler.csproj` is retained or improved at every child merge" would be
  satisfied only vacuously. Narrowing the attribute onto `OpenQFItem` (research §6.4) would leave
  `OpenQFItem` itself unmeasured even though the seam makes both of its branches testable, so the seam
  strictly dominates the narrowing option.

**Contingency, recorded honestly.** If the delivered test set cannot reach the coverage floor on the
now-measured class, the measured figure and the shortfall are to be reported explicitly in the coverage
evidence and the pull-request body. **A blanket class-level exclusion must not be silently restored.**

**Rejected alternative (research §6.5).** Reusing `UtilitiesCS`'s existing modal-dialog seam
`MyBox.DialogInvoker` (`UtilitiesCS/Dialogs/MyBox.cs:41-45`) was rejected for two reasons:
`DialogInvoker` is declared `internal` and `UtilitiesCS/Properties/AssemblyInfo.cs:18-20` grants
`InternalsVisibleTo` only to `DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, and `ToDoModel.Test` — not
`QuickFiler.Test` — so it would require editing a shared surface outside this issue's file set; and
switching from `MessageBox.Show` to `MyBox.ShowDialog` would change the dialog the user sees, a behavior
change beyond the three defects.

**Recorded for the reviewer.** The only machine-enforced numeric coverage gate found in this repository
is a repo-wide 80% line rate at `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-489`, which
throws when the root Cobertura `line-rate * 100` is below 80. There is no per-file gate, no per-assembly
gate, and no branch-coverage gate anywhere in `scripts/`. The uniform 85% / 75% thresholds in
`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` are not enforced by any script
in this worktree, and `quality-tiers.yml` — which `.claude/rules/quality-tiers.md` names as its source of
truth — does not exist at the repository root. No plan step may gate on an unenforceable number.

#### D6 — the new test file and the single project-file entry

- **Path:** `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` (new).
- **Class:** `QfcExplorerControllerTests`. **Namespace:** `QuickFiler.Controllers.Tests`, matching
  `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:20`.
- MSTest, Moq, and FluentAssertions per `CLAUDE.md` CUT1 and CUT2. Mock graph per research §5.2,
  modelled on `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:39-47`.
- No collision: no file under `QuickFiler.Test/Controllers/` matches `*Explorer*`.

**Project-file entry placement — this overrides research §5.1.** The single appended
`<Compile Include="Controllers\QfcExplorerControllerTests.cs" />` line goes **immediately after** the
existing line `<Compile Include="Controllers\QfcDatamodelLivenessTests.cs" />`, currently
`QuickFiler.Test/QuickFiler.Test.csproj:119` — **not** after line 158.

Reasoning for the override: line 158 (`<Compile Include="Controllers\QfcQueueTests.cs" />`) is only two
lines above the `Form1` compile region at `:161-166`, which sibling child #491 owns exclusively. Git's
three-line merge context would place the two children's hunks adjacent, making a conflict likely when
the second child rebases onto the integration branch. Line 119 is 42 lines clear of that region. The
insertion is still inside the `Controllers` entries of the same `ItemGroup`, so the epic's
Shared-Surface Coordination partition ("#449 owns one appended `Compile Include` ... It appends to the
`Controllers` item group and must not touch the `Form1` region") is satisfied with margin. The `Form1`
region at `:161-166` and the `Form1.resx` `EmbeddedResource` at `:180-182` are untouched.

Mechanics:

- The appended line must use **CRLF**, matching the file: `QuickFiler.Test.csproj` is CRLF throughout.
- `*.csproj` is listed in `.csharpierignore`, so csharpier will not reformat it.
- The file goes from 484 to 485 lines, leaving 15 lines of headroom under the 500-line cap.
- **No edit to `QuickFiler/QuickFiler.csproj` is required.** The dead region is inside an
  already-compiled file, and the uncompiled `Legacy/` and `Notes/` files have no entries to remove.

**Pre-existing policy tension, flagged so `feature-review` does not raise it as new.**
`.claude/rules/general-unit-test.md` ("Test File Location") requires test files to live in a `tests/`
directory tree mirroring the production source. This repository's entire C# corpus instead uses
`<Project>.Test/` sibling projects, `CLAUDE.md`'s C# Unit Test Policy — which sits above the rule
summaries in the compliance order — does not restate the `tests/` requirement, and the epic explicitly
directs #449 to the `Controllers` item group of `QuickFiler.Test.csproj`. Placing the file at
`QuickFiler.Test/Controllers/` matches the repository and the epic. This tension is pre-existing and is
not created by this issue.

#### D7 — fail-before evidence

Defect 2 carries the genuine `[expect-fail]` regression test (mechanism in Test Strategy). Defects 1 and
3 admit no constructible failing-before test; each requires a
`fail-before-exception.<timestamp>.md` dossier under
`docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449/evidence/regression-testing/`
with the content given in research §8.1 and §8.3. Required fields in each dossier:

- `Timestamp:` — ISO-8601, per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
- `Command:` — the search command that produced the absence proof.
- `EXIT_CODE:` — the observed exit code of that command.
- `WhyFailingRunImpossible:` — the structural reason no failing-before test can be constructed.
- The absence proof, as `SearchScope:`, `SearchPatterns:`, and `SearchResult:` fields.

For defect 3, the issue's criterion "a test run confirming no behavior change" is satisfied by the
**before/after full-suite comparison**, not by a new test.

### Boundaries and invariants to preserve

- `QfcExplorerController` remains `internal` to the `QuickFiler` assembly. Its two production
  construction sites (`QfcHomeController.cs:182`,
  `EfcHomeControllerDependencyFactories.cs:155`) are unchanged, as is the factory delegate they go
  through.
- The public surface of `IQfcExplorerController` loses exactly one member and gains none.
- `NavigateToOutlookFolder` remains a separate private method, so it stays instrumented and is covered
  through `OpenQFItem` calls made by the tests.
- The user-visible dialog text, buttons, and icon at lines 168-173 are unchanged; only the invocation
  route changes.
- `BlShowInConversations`, `ExplConvView_ToggleOn`, `ExplConvView_ToggleOff`,
  `ExplConvView_ReturnState`, `GetSiblingView`, and `CurrentConversationState` keep their current
  behavior.
- `QfcExplorerController.cs` carries **no `#nullable enable` pragma**, so it does not participate in
  nullable analysis and the type-check gate imposes no new obligation on it. Any new file should follow
  the surrounding convention rather than introducing a pragma.
- The `log4net` `log` field at lines 23-25 is declared and never referenced anywhere in the file. Under
  D1 nothing new consumes it, so it remains unused. This is the status quo and emits no diagnostic (the
  field is `static readonly` with a method-call initialiser, so neither CS0169 nor CS0414 applies).
  Recorded so a reviewer does not read it as newly dead.

### Dependencies or blocked work

- No `depends_on` edge. All four children of `quickfiler-suite-determinism-foundation` sit in wave 0.
- The only shared surface is `QuickFiler.Test/QuickFiler.Test.csproj`, partitioned by the epic and
  further de-risked by the D6 placement override.
- No external service, package, or release is required.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

| File | Before | After | Change |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcExplorerController.cs` | 323 | ~179 | D1 removal, D2 one-line fix, D3 139-line deletion, D4 nine `using` removals, D5 attribute removal plus dialog seam |
| `QuickFiler/Interfaces/IQfcExplorerController.cs` | 15 | 14 | D1 — delete line 12 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 484 | 485 | D6 — one appended `<Compile Include>` after line 119, CRLF |
| `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` | 0 (new) | < 500 | D6 — new test file |

Every file in the diff is under the 500-line cap.

#### Functions/classes/CLI commands impacted

- `IQfcExplorerController.ExplConvView_Cleanup()` — removed.
- `QfcExplorerController.ExplConvView_Cleanup()` — removed.
- `QfcExplorerController.NavigateToOutlookFolder(MailItem)` — one line changed.
- `QfcExplorerController.OpenQFItem(MailItem)` — one call routed through the new dialog seam.
- `QfcExplorerController` (class) — `[ExcludeFromCodeCoverage]` removed; one internal settable seam
  member added.
- Six private/internal statics deleted: `SanitizeArrayLineTSV`, `StripTabsCrLf`,
  `WriteCSV_StartNewFileIfDoesNotExist`, `SanitizeArray`, `SaveMessageAsMSG`,
  `GetCurrentExplorerFolder`.
- No CLI command is affected.

#### Data flow and validation changes

Only one data-flow change: in `NavigateToOutlookFolder`, the destination assignment target becomes the
constructor-captured `Explorer` rather than a freshly resolved one, so the guard's read and the
assignment's write address the same object. No validation is added or removed. No new null check is
introduced, because D1 removes the code path that would have needed one.

#### Error handling and logging updates

None. No `try`/`catch` is added, changed, or removed. No logging call is added: the `log` field remains
unreferenced, as it is today. The broad-catch shape in the legacy body is not imported (D1), which is
one of the two reasons that decision was taken.

#### Rollback/feature-flag considerations (if applicable)

Not applicable. No feature flag, no configuration switch, and no staged rollout. Rollback is a single
revert of the pull request; the change is confined to one internal class, its interface, and the test
project.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

- `IQfcExplorerController` after the change carries five members: `BlShowInConversations { get; set; }`,
  `Task OpenQFItem(MailItem)`, `void ExplConvView_ToggleOff()`, `void ExplConvView_ToggleOn()`,
  `void ExplConvView_ReturnState()`.
- The new dialog seam is an internal settable member on `QfcExplorerController` whose default invokes
  `MessageBox.Show` with the existing message, caption, buttons, and icon and returns its
  `DialogResult`. It is set only by tests.
- No serialization format, wire format, or file format is involved.

#### Required configuration keys and defaults

None. No configuration key is added, read, or changed. `coverage.config` and
`Directory.Build.targets` are not edited.

#### Backward-compatibility expectations

- **One deliberate breaking change:** `IQfcExplorerController` loses `ExplConvView_Cleanup()`. It is
  called by no compiled production or test code, mocked by no test, and implemented by exactly one type
  that is edited in the same change. The break is called out here and in the pull-request body per the
  general policy's compatibility clause.
- `QuickFiler/Notes/notes_interfaces.cs:52-59` retains its uncompiled duplicate declaration. It is a
  documentation artifact and is intentionally left inconsistent with the compiled contract.
- No other assembly's public surface changes.

#### Performance constraints (latency/throughput/memory)

No constraint applies and none is measured. The D2 fix removes one cross-process COM round-trip per
`NavigateToOutlookFolder` call that enters the guard; the effect is a reduction and is not quantified.
The D3 deletion removes 139 lines of unreachable code and therefore has no runtime effect.

## Removed contract — legacy semantics for future restoration

This section is the **durable record** of the behavior removed by D1. The epic forbids any child writing
under `docs/features/potential/**` (Recorded Preconditions), and the uncompiled `QuickFiler/Legacy/`
tree is a deletion candidate for a later epic, so this spec is the record. A future restorer should read
this section rather than reinvent the behavior.

### Verbatim legacy body (`QuickFiler/Legacy/QuickFileController.cs:851-869`)

```csharp
public void ExplConvView_Cleanup()
{
    ObjView = _activeExplorer.CurrentFolder.Views[_objViewMem];
    try
    {
        ObjView.Apply();
        ObjViewTemp?.Delete();
        BlShowInConversations = false;
    }
    catch (System.Exception)
    {
        ObjViewTemp = GetSiblingView(
            (Outlook.View)_activeExplorer.CurrentView,
            "tmpNoConversation"
        );

        ObjViewTemp?.Delete();
    }
}
```

### Semantic summary

The sole legacy call site is `QuickFiler/Legacy/QuickFileController.cs:667-680` (`ButtonCancel_Click`),
guarded by `if (BlShowInConversations)`. On cancel, restore the remembered Outlook view and delete the
temporary `tmpNoConversation` view; on failure, best-effort locate the temporary view as a sibling of
the current view and delete it.

Every structural piece the restoration needs is present on the modern type:

| Legacy member | Modern equivalent | Status |
| --- | --- | --- |
| `ObjView` (public field, `:42`) | `_objView` (private field, `QfcExplorerController.cs:43`) | Present, renamed and narrowed |
| `_objViewMem` (`:43`) | `_objViewMem` (`:44`) | Present, but **never initialised by the constructor** |
| `ObjViewTemp` (public field, `:44`) | `ObjViewTemp` (public field, `:45`) | Present, identical |
| `GetSiblingView(View, string)` (`:871-884`) | `GetSiblingView(View, string)` (`:108-121`) | Present, byte-identical body |
| `BlShowInConversations` (`:185`) | `BlShowInConversations` (`:49-53`) | Present |
| `_activeExplorer` (`:145` and elsewhere) | `_activeExplorer` (`:42`) | Present |
| `CurrentConversationState` (`:170`, private) | `CurrentConversationState` (`:55-58`, internal) | Present, but referenced nowhere in the repository |

The only gap is behavioral, not structural: the legacy constructor initialised `_objViewMem`
(`QuickFileController.cs:145-147`) and the modern constructor (lines 27-37) does not.

### Fallback implementation, if a future change restores the member

Reproduced from research §1.5. **This is not the legacy behavior**: the null guard is new, the exception
filter is narrower, and the view resolution moves inside the `try`. It additionally requires
`using System.Runtime.InteropServices;`.

```csharp
/// <summary>
/// Restores the Outlook view remembered by <see cref="ExplConvView_ToggleOff"/> and removes the
/// temporary "tmpNoConversation" view. On failure the temporary view is still removed, but
/// <see cref="BlShowInConversations"/> is deliberately left set: the restore did not happen, so
/// the caller still owes one. This asymmetry is inherited from the legacy implementation.
/// </summary>
public void ExplConvView_Cleanup()
{
    if (string.IsNullOrEmpty(_objViewMem))
    {
        // No view was remembered, so there is nothing to restore. Guarding here rather than
        // letting the Views indexer throw: the legacy type initialised _objViewMem in its
        // constructor and this type does not, so a null value is reachable on the first call.
        ObjViewTemp?.Delete();
        BlShowInConversations = false;
        return;
    }

    try
    {
        _objView = _activeExplorer.CurrentFolder.Views[_objViewMem];
        _objView.Apply();
        ObjViewTemp?.Delete();
        BlShowInConversations = false;
    }
    catch (System.Exception ex) when (ex is COMException || ex is ArgumentException)
    {
        log.Warn($"Could not restore Outlook view '{_objViewMem}'.", ex);
        ObjViewTemp = GetSiblingView((Outlook.View)_activeExplorer.CurrentView, "tmpNoConversation");
        ObjViewTemp?.Delete();
    }
}
```

Note the residual risk in the narrowed filter: an Outlook PIA can also surface
`System.UnauthorizedAccessException` and `System.InvalidCastException` from these call paths, and
neither would be caught. Narrowing therefore changes runtime behavior relative to the legacy body — a
further reason the legacy semantics cannot simply be ported.

### Preserved knowledge — the catch asymmetry (research §1.2(b))

The legacy `catch` does not set `BlShowInConversations = false` while the `try` path does. Two readings
were examined, and **under D1 the question is moot** because the code is not imported. Both are recorded
so a future restorer does not have to re-derive them:

- **Intentional.** The flag means "a conversation-view restore is still owed." On the success path the
  restore happened, so the debt is cleared. On the failure path `ObjView.Apply()` did not succeed, the
  Explorer still shows the temporary non-conversation view, and the debt stands. Leaving the flag `true`
  keeps `ExplConvView_ReturnState()` (`QfcExplorerController.cs:66-70`) willing to retry.
- **A defect.** The retry runs `ExplConvView_ToggleOn()` (`QfcExplorerController.cs:123-131`), whose
  first statement is the identical `_activeExplorer.CurrentFolder.Views[_objViewMem]` resolution that
  just failed, so the retry is expected to fail identically and the flag leaks a permanently-true state.

If a future change restores the member, preserve the asymmetry and record both readings in an XML doc
comment rather than "correcting" it, because correcting it would be a behavior change to a path with no
caller. If the asymmetry is judged worth tracking as a defect in its own right, it must go through the
issue-promotion path after this child merges; the epic forbids writing a new document under
`docs/features/potential/**`.

## Assumptions, Constraints, Dependencies

### Assumptions (environment, data, access)

- Windows with MSBuild, `vstest.console.exe`, and the manifest-pinned CSharpier 1.2.6 available;
  `dotnet tool restore` has been run once in this worktree.
- No live Outlook process is available, so every COM interaction in tests is mocked. What Outlook's COM
  implementation does with a null `Views` index could not be determined and was not verified; this is
  recorded as unknown and is one input to D1.
- `[assembly: InternalsVisibleTo("QuickFiler.Test")]` at `QuickFiler/Properties/AssemblyInfo.cs:5`
  makes the internal class, the internal dialog seam, and `CurrentConversationState` reachable from the
  test project.
- The assembly references the tests need are already present:
  `QuickFiler.Test/QuickFiler.Test.csproj:278-280` references `Microsoft.Office.Interop.Outlook` and
  `:326-328` references `office` (the `Microsoft.Office.Core` PIA), both with
  `<EmbedInteropTypes>False</EmbedInteropTypes>`, which is what Moq requires.
- **One unverified compile-level detail (research §5.3):** the `Views` indexer parameter type is
  expected to be `object`, so the setup shape is
  `views.Setup(v => v[It.IsAny<object>()]).Returns(view.Object)`. If the compiler rejects that shape,
  the parameter type is the only thing to adjust. It is a one-token change and cannot invalidate the
  harness.

### Constraints (budget, performance, compatibility)

- **500-line cap** (`.claude/rules/general-code-change.md`) applies to production code, test code, and
  reusable scripts, not to Markdown. Every file in the diff stays under it. If the test set exceeds 500
  lines, split into a second file (for example
  `QuickFiler.Test/Controllers/QfcExplorerController.ConversationViewTests.cs`) and append a second
  compile entry in the same partitioned region rather than exceeding the cap.
- **Pre-existing cap violations, not caused by this change and not in the diff:**
  `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` at 1,429 lines (2.9x) and
  `QuickFiler/Legacy/QuickFileController.cs` at 1,065 lines (2.1x). `QuickFileController.cs` is
  read-only reference material and is not compiled. No split refactor is proposed for either.
- `QuickFiler/QuickFiler.csproj` is 595 lines, above 500, but it is a generated non-SDK project file
  rather than authored source, and it is not edited.
- **Bugfix Workflow** (`CLAUDE.md`): failing regression test first, then the minimal targeted fix, then
  local verification; no opportunistic refactors.
- **Epic Hard Constraints:** no `.claude/**` edit; `/InIsolation` mandatory on `vstest`; exclude
  `\.claude\` from recursive test-assembly discovery; no Python toolchain exists; evidence paths are
  `<FEATURE>/evidence/<kind>/` only.
- Determinism requirements from `.claude/rules/general-unit-test.md`: no temporary files, no mutable
  global state, and no banned timing APIs (`Thread.Sleep`, `Task.Delay`, real wall-clock waits) in test
  code.

### External dependencies (services, libraries, releases)

None added. MSTest, Moq, and FluentAssertions are already referenced by `QuickFiler.Test`; the Outlook
and Office PIAs are already referenced. No package is added to `packages.config` and no analyzer is
added or reconfigured.

## Data / API / Config Impact

- **User-facing or API changes:** One internal-assembly contract member is removed
  (`IQfcExplorerController.ExplConvView_Cleanup`); the interface itself is `public` but the only
  implementer is `internal` and there are no callers. No user-visible behavior changes: the not-in-view
  dialog keeps its existing text, caption, buttons, and icon, and only its invocation route changes. The
  D2 fix alters which `Explorer` object receives a `CurrentFolder` assignment in the case where the
  active Explorer changed after construction — that is the defect being corrected.
- **Data or migration considerations:** None. No persisted data, no schema, no stored settings.
- **Logging/telemetry updates (if any):** None. No log statement is added or removed; the file's
  `log4net` logger remains unreferenced, as it is today.
- **Compatibility notes (CLI flags, config schemas, versioning):** No CLI flag, config schema, or
  version is affected. `coverage.config` and `Directory.Build.targets` are not edited. One test-project
  file gains one `<Compile Include>` line.

## Test Strategy

### Regression tests to add or update

The single genuine failing-before test is for defect 2 (research §8.2). Mechanism — make the two
Explorers distinguishable by sequencing `ActiveExplorer()`:

```csharp
olApp.SetupSequence(a => a.ActiveExplorer())
     .Returns(capturedExplorer.Object)   // consumed by the constructor, line 35
     .Returns(driftedExplorer.Object);   // what line 140 would resolve today
```

Arrange so the guard at lines 135-137 is entered: `capturedExplorer.CurrentFolder` returns a folder
whose `FolderPath` is `@"\\Mailbox\A"` and `mailItem.Parent` returns a folder whose `FolderPath` is
`@"\\Mailbox\B"`. Set `IsItemSelectableInView` to `true` so the dialog branch is not reached, and
construct with `QfEnums.InitTypeEnum.Find` so neither `HasFlag(Sort)` conjunct is true. Use
`MockBehavior.Loose` for `driftedExplorer` so the pre-fix failure surfaces as a FluentAssertions message
rather than a Moq strict-mode exception.

The two assertions:

```csharp
capturedExplorer.VerifySet(e => e.CurrentFolder = destination.Object, Times.Once());
driftedExplorer.VerifySet(e => e.CurrentFolder = It.IsAny<MAPIFolder>(), Times.Never());
```

Before the fix, line 140 assigns `driftedExplorer.CurrentFolder`, so **both** assertions fail. After the
fix, both pass.

Defects 1 and 3 admit no constructible failing-before test and carry `fail-before-exception` dossiers
instead, per D7. For defect 3, the "test run confirming no behavior change" is the before/after
full-suite comparison, recorded under `evidence/qa-gates/`.

### Unit tests (MSTest) for the fixed behavior and boundaries

The template's "pytest" wording is inapplicable: this is a C# change and the framework is MSTest per
`CLAUDE.md` CUT1. Recommended set, reproduced from research §5.5:

| # | Test | Target |
| --- | --- | --- |
| 1 | `OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer` | Defect 2 — the fail-before test |
| 2 | `OpenQFItem_WhenMailIsAlreadyInTheCurrentFolder_DoesNotChangeCurrentFolder` | Defect 2 guard, lines 135-137 |
| 3 | `OpenQFItem_WhenItemIsSelectableInView_ClearsAndAddsSelection` | Lines 156-159, positive path |
| 4 | `ExplConvView_ToggleOn_WhenFlagSet_AppliesRememberedView` | Lines 123-131; requires the `Views` indexer mock |
| 5 | `ExplConvView_ToggleOn_WhenFlagClear_DoesNothing` | Line 125 negative branch |
| 6 | `ExplConvView_ToggleOff_WhenConversationsNotGrouped_DoesNothing` | Line 74 negative branch |
| 7 | `ExplConvView_ToggleOff_WhenSiblingViewMissing_CopiesAndSavesTemporaryView` | Lines 95-103 |
| 8 | `GetSiblingView_WhenNamedViewPresent_ReturnsIt` / `_WhenAbsent_ReturnsNull` | Lines 108-121 |
| 9 | `CurrentConversationState_ReflectsCommandBarPressedState` | Lines 55-58, two cases |
| 10 | `ExplConvView_ReturnState_WhenFlagSet_TogglesOn` | Lines 66-70 |
| 11 | `Contract_ExplConvView_Cleanup_IsNotDeclaredOnTheInterface` | Defect 1, **optional and not recommended** — a reflection assertion on member absence encodes no behavior and would permanently block restoration; the D7 dossier is preferred |

With the D5 dialog seam, the not-in-view branch (lines 166-178) becomes testable as well, so a further
test asserting that the seam is invoked once and that `mailItem.Display()` is called only on a `Yes`
result is in scope. The seam default must never be exercised in a test.

Mock graph per research §5.2, modelled on `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:39-47`
and its recursive `SetupGet(x => x.Ol.App)` pattern:

```csharp
var repo = new MockRepository(MockBehavior.Loose);

var commandBars = repo.Create<Microsoft.Office.Core.CommandBars>();
commandBars.Setup(c => c.GetPressedMso("ShowInConversations")).Returns(false);

var explorer = repo.Create<Outlook.Explorer>();
explorer.Setup(e => e.CommandBars).Returns(commandBars.Object);

var olApp = repo.Create<Outlook.Application>();
olApp.Setup(a => a.ActiveExplorer()).Returns(explorer.Object);

var globals = repo.Create<IApplicationGlobals>();
globals.SetupGet(g => g.Ol.App).Returns(olApp.Object);
globals.SetupGet(g => g.Ol.ViewWide).Returns("Wide");   // only for ToggleOff tests

var formController = repo.Create<IFilerFormController>();
var parent = repo.Create<IFilerHomeController>();
parent.SetupGet(p => p.FormController).Returns(formController.Object);

var controller = new QfcExplorerController(
    QfEnums.InitTypeEnum.Find,      // deliberately NOT Sort
    globals.Object,
    parent.Object
);
```

Every relevant COM member is mockable with an in-repo precedent (research §5.3): `Explorer.CurrentFolder`
get and set, `MAPIFolder.Views`, the `Views` indexer, `View.Apply()`, `View.Delete()`, `Views`
enumeration via `GetEnumerator`, `Explorer.CurrentView`, `Explorer.CommandBars` and
`CommandBars.GetPressedMso`, `Explorer.IsItemSelectableInView` / `ClearSelection` / `AddToSelection`, and
`MailItem.Parent`. Nothing in the changed paths is sealed, static, or non-virtual in a way that blocks
mocking.

`_parent.FormController.MinimizeFormViewer()` at line 148 is not a barrier: `IFilerFormController` is an
interface (`QuickFiler/Interfaces/IFilerFormController.cs:17`) and the mock chain is two lines. The real
implementation at `QfcFormController.Actions.cs:197` touches a form but is never constructed by a test.

**Branch-control detail worth a comment in the test.** Constructing with `QfEnums.InitTypeEnum.Find`
(value 2, per `QuickFiler/Helper Classes/QfEnums.cs:8`) makes `_initType.HasFlag(QfEnums.InitTypeEnum.Sort)`
false at lines 151 and 179. Both conjunctions use the **non-short-circuiting `&`**, so
`AutoFile.AreConversationsGrouped(_activeExplorer)` at line 152 is still evaluated and the `CommandBars`
setup remains **mandatory**.

`Task.Run` at lines 154, 158, 159, and 180 is production async, not a test timing device. The method is
awaited by the test, so the result is deterministic; no `Task.Delay` and no `Thread.Sleep` is introduced,
so the banned-API list in `.claude/rules/general-unit-test.md` is respected.

### Edge cases and negative scenarios (invalid inputs, missing data, boundary values)

- Guard not entered: mail item already in the captured Explorer's current folder — no `CurrentFolder`
  assignment on either Explorer (test 2).
- `BlShowInConversations` false — `ExplConvView_ToggleOn` and `ExplConvView_ReturnState` do nothing
  (tests 5 and 10).
- Conversations not grouped — `ExplConvView_ToggleOff` does nothing (test 6).
- Sibling view absent — `GetSiblingView` returns null and the copy-and-save path runs (tests 7 and 8).
- Item not selectable in view — the dialog seam is invoked; `Display()` only on `Yes` (D5 seam test).
- `CurrentConversationState` for both pressed states (test 9).

### Error handling and logging verification

No error-handling behavior is added or changed, so there is nothing to assert beyond the absence of new
exceptions. No logging is added, so no log assertion applies. Tests must not assert on the `log4net`
field.

### Coverage impact and targets for changed lines/modules

- **No merge-base coverage baseline exists** in this feature folder yet
  (`evidence/baseline/` is empty), so no repo-wide figure can be asserted in advance. The baseline must
  be captured before the change and the post-change figure compared against it.
- The **only machine-enforced numeric gate** is the repo-wide 80% line rate at
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-489`. No per-file, per-assembly, or
  branch-coverage gate exists, and `quality-tiers.yml` does not exist at the repository root. No
  criterion below gates on an unenforceable number.
- Under D5 the class enters the coverage denominator for the first time. The obligation is therefore to
  **measure and report**: the `QfcExplorerController` line figure, the `QuickFiler.csproj` figure before
  and after, and, if the epic NFR "Coverage of `QuickFiler.csproj` is retained or improved at every
  child merge" is not met, the shortfall stated explicitly with the reason.
- `CLAUDE.md` UT2's ">= 90% for any new modules, classes, or methods added" is the target for the new
  dialog seam member and the new test file's subject members.
- Changed lines must not regress in coverage.

### Toolchain commands to run (format, lint, type-check, test)

Run in this exact order. **A failure, or any step that changes a file, restarts the loop from the top.**

1. `dotnet tool restore`
2. `dotnet tool run csharpier format .` — verify with `dotnet tool run csharpier check .`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
5. `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`

Three load-bearing constraints:

- **`/t:Rebuild`, never `/t:Build`.** MSBuild's incremental up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
  project and runs no analyzers. The gate cannot fail. CI uses `/t:Build` only because a runner checkout
  is always cold.
- **Never add `/p:Nullable=enable`.** No project in this repository carries a `<Nullable>` element and
  there is no `Directory.Build.props`, so the property is a solution-wide opt-in that conscripts every
  file which has never adopted the pragma. Forcing it produced 195 errors in `UtilitiesCS.csproj` on
  2026-08-10 against zero errors without it, and `.github/workflows/ci.yml` omits it deliberately.
  Removing it loses no enforcement over any file that has opted in.
- **`/InIsolation` is mandatory.** Without it each assembly's `app.config` binding redirects are ignored
  and roughly 1,695 phantom failures appear with empty messages and sub-millisecond durations, surfacing
  as a Moq `TypeInitializationException` via `System.Threading.Tasks.Extensions`. A run that omits the
  flag produces a fabricated mass regression that must not be "fixed."

Additionally: recursive `*.Test.dll` discovery must **exclude `\.claude\`** so stale agent-worktree
builds are not loaded. The CI reference invocation is in `.github/workflows/_mstest-coverage.yml`.

**No Python toolchain exists** — there is no `scripts/dev_tools/` and no Poetry manifest — so any step
naming `poetry run python -m scripts.dev_tools.*` is unrunnable by absence and must be reported as such
rather than fabricated or silently skipped.

### Manual validation steps (if required)

None required, and none possible for defects 1 and 3. Manual validation of defect 2 would need a live
Outlook session with two Explorer windows and is not available in this environment; the unit test is the
authoritative verification.

## Acceptance Criteria

Sixteen criteria. Each is independently verifiable and names its verification (a test name, a command,
or an artifact path).

- [ ] **AC-1 (D1, defect 1 remedy).** `void ExplConvView_Cleanup();` is removed from
      `QuickFiler/Interfaces/IQfcExplorerController.cs:12`, and the implementation plus its
      `//PRIORITY:` comment are removed from `QuickFiler/Controllers/QfcExplorerController.cs:60-64`.
      **Verify:** `git grep -n "ExplConvView_Cleanup" -- "*.cs"` returns hits only in the uncompiled
      `QuickFiler/Legacy/QuickFileController.cs` and `QuickFiler/Notes/notes_interfaces.cs`, and
      toolchain steps 3 and 4 pass.
- [ ] **AC-2 (D1 knowledge preservation).** This spec carries a section headed exactly
      `## Removed contract — legacy semantics for future restoration` containing the verbatim legacy
      body from `QuickFiler/Legacy/QuickFileController.cs:851-869`, the semantic summary, the modern
      member-equivalence table, the fallback implementation, and the recorded catch-asymmetry analysis.
      **Verify:** read the section in `spec.md`; the pull-request body references it.
- [ ] **AC-3 (D2, defect 2 remedy with a named regression test).** `QfcExplorerController.cs:140` reads
      `_activeExplorer.CurrentFolder = (MAPIFolder)mailItem.Parent;`, and the test
      `OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer` in
      `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` passes. **Verify:** the test-run
      artifact under `evidence/qa-gates/` shows the named test failing before the fix and passing after,
      per the D7 `[expect-fail]` sequencing.
- [ ] **AC-4 (D2 residual re-resolution check).** No `ActiveExplorer()` call remains in
      `QfcExplorerController.cs` other than the constructor capture at line 35. **Verify:**
      `git grep -n "ActiveExplorer()" -- QuickFiler/Controllers/QfcExplorerController.cs` returns exactly
      one line.
- [ ] **AC-5 (D2 documentation correction).** The spec records that the defective call is in the private
      helper `NavigateToOutlookFolder(MailItem)` (lines 133-143) reached from `OpenQFItem` via line 149,
      not directly in `OpenQFItem`; that line 140 was the only re-resolution in the file; and that the
      issue criterion's alternative branch ("document why a fresh call is required") does not apply
      because no behavioral dependency on the fresh call exists. **Verify:** read Root Cause Analysis
      and decision D2 in this spec.
- [ ] **AC-6 (D3, defect 3 deletion).** Lines 183-321 of `QfcExplorerController.cs` — the
      `#region Email Sorting To Rewrite` — are deleted, and none of `SanitizeArrayLineTSV`,
      `StripTabsCrLf`, `WriteCSV_StartNewFileIfDoesNotExist`, `SanitizeArray`, `SaveMessageAsMSG`,
      `GetCurrentExplorerFolder` remains anywhere under `QuickFiler/`. **Verify:**
      `git grep -n -E "SanitizeArrayLineTSV|StripTabsCrLf|WriteCSV_StartNewFileIfDoesNotExist|SanitizeArray|SaveMessageAsMSG|GetCurrentExplorerFolder" -- QuickFiler QuickFiler.Test`
      returns no match.
- [ ] **AC-7 (D3 no-behavior-change evidence).** A full-suite run before the change and a full-suite run
      after the change produce the same set of passing tests, with the new tests as the only additions
      and no new failures. **Verify:** both run logs are committed under
      `docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449/evidence/qa-gates/`
      and the comparison is stated in the pull-request body.
- [ ] **AC-8 (D4 hygiene, self-verifying).** The nine directives at lines 1, 2, 3, 4, 5, 6, 7, 8, 13, 15
      identified in the D4 table are removed and the six retained directives (9, 10, 11, 12, 14, 16)
      remain. **Verify:** toolchain steps 3 and 4 pass, which proves no removed directive was required
      (a required directive fails with CS0246 or CS1061); and the pull-request body labels the removal as
      hygiene, not a gate fix, noting that three of the directives were already unused on green `main`.
- [ ] **AC-9 (D5 attribute removal).** No `[ExcludeFromCodeCoverage]` attribute remains anywhere in
      `QuickFiler/Controllers/QfcExplorerController.cs`. **Verify:**
      `git grep -n "ExcludeFromCodeCoverage" -- QuickFiler/Controllers/QfcExplorerController.cs` returns
      no match.
- [ ] **AC-10 (D5 dialog seam).** An injectable modal-dialog seam exists on `QfcExplorerController` whose
      default invokes `MessageBox.Show` with the existing message, caption, buttons, and icon; the call
      at line 168 goes through it; and a test exercises the not-in-view branch with the seam replaced, so
      no dialog is displayed. **Verify:** the named seam test passes in the run artifact under
      `evidence/qa-gates/`, and `git grep -n "MessageBox.Show" -- QuickFiler/Controllers/QfcExplorerController.cs`
      shows the call only in the seam's default initialiser.
- [ ] **AC-11 (D5 coverage decision, measured and reported).** A merge-base coverage baseline is
      captured and committed under
      `docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449/evidence/baseline/`
      and the post-change measurement under
      `docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449/evidence/qa-gates/`.
      `baseline` and `qa-gates` are the canonical evidence kinds enumerated by
      `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`; `evidence/coverage/` is not an
      enumerated kind and must not be used. Both artifacts report the `QfcExplorerController` line figure,
      the `QuickFiler.csproj` figure before and after,
      and the repo-wide Cobertura line rate. The repo-wide 80% gate at
      `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-489` is not lowered by this change. If
      the epic NFR "Coverage of `QuickFiler.csproj` is retained or improved" is not met, the shortfall is
      stated explicitly with its reason. **No blanket class-level `[ExcludeFromCodeCoverage]` is
      restored.** **Verify:** read the coverage evidence artifact; confirm AC-9 still holds.
- [ ] **AC-12 (D6 test file and single project-file entry).**
      `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` exists with class
      `QfcExplorerControllerTests` in namespace `QuickFiler.Controllers.Tests`, using MSTest, Moq, and
      FluentAssertions; exactly one `<Compile Include="Controllers\QfcExplorerControllerTests.cs" />`
      line is appended immediately after
      `<Compile Include="Controllers\QfcDatamodelLivenessTests.cs" />` in
      `QuickFiler.Test/QuickFiler.Test.csproj`, in CRLF; and the `Form1` compile region and the
      `Form1.resx` `EmbeddedResource` are unchanged. **Verify:**
      `git diff -- QuickFiler.Test/QuickFiler.Test.csproj` shows a single added line adjacent to the
      `QfcDatamodelLivenessTests` entry and no change within the `Form1` region.
- [ ] **AC-13 (deterministic regression tests).** No test added by this change creates a temporary file,
      constructs a live form, starts a message pump, calls `MessageBox.Show`, or uses `Thread.Sleep`,
      `Task.Delay`, or a wall-clock wait; tests pass in any order. **Verify:**
      `git grep -n -E "Thread.Sleep|Task.Delay|MessageBox.Show|Path.GetTempPath|new Form|Application.Run" -- QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs`
      returns no match, and the suite passes on two consecutive full runs recorded under
      `evidence/qa-gates/`.
- [ ] **AC-14 (D7 fail-before dossiers).** Two `fail-before-exception.<timestamp>.md` dossiers exist
      under
      `docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449/evidence/regression-testing/`
      — one for defect 1, one for defect 3 — each carrying `Timestamp:`, `Command:`, `EXIT_CODE:`,
      `WhyFailingRunImpossible:`, and the absence proof as `SearchScope:`, `SearchPatterns:`, and
      `SearchResult:` fields. **Verify:** read both artifacts; confirm each `Command:` reproduces the
      recorded `SearchResult:`.
- [ ] **AC-15 (clean full-toolchain pass).** The five-step toolchain above completes in a single pass
      with no failure and no file modified by a formatting step, using `/t:Rebuild` (never `/t:Build`),
      without `/p:Nullable=enable`, with `/InIsolation`, and with `\.claude\` excluded from test-assembly
      discovery. **Verify:** the commands run and their exit codes are recorded under
      `evidence/qa-gates/`, and the pull-request body states that all steps passed in the final pass.
- [ ] **AC-16 (file-size cap attribution).** Every file in the diff is under 500 lines —
      `QfcExplorerController.cs` approximately 179, `IQfcExplorerController.cs` 14,
      `QuickFiler.Test.csproj` 485, and the new test file under 500 — and neither
      `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` nor
      `QuickFiler/Legacy/QuickFileController.cs` appears in the diff. **Verify:** `git diff --stat`
      against the merge base, plus a line count of each changed file; the pull-request body states
      pre-emptively that the two over-cap files are pre-existing violations that are not edited.

## Risks & Mitigations

### Technical or operational risks

1. **`feature-review` raises the coverage exclusion as Blocking.** Touching a file that carries an
   unratified production-file exclusion invites that finding even though this change did not introduce it
   (commit `a564add0d`, 2026-06-13). *Mitigation:* D5 removes the exclusion and records the full policy
   analysis (`CLAUDE.md` UT2 clause (c) does not reach this class; `.claude/rules/general-unit-test.md`
   forbids the exclusion outright) in this spec and the pull-request body, so the grounding is visibly
   examined rather than ignored.
2. **The now-measured class lowers the `QuickFiler.csproj` coverage figure.** The dialog seam makes both
   `OpenQFItem` branches testable, which is the reason the seam was chosen over narrowing the attribute,
   but the outcome is not guaranteed in advance and no baseline exists yet. *Mitigation:* AC-11 requires a
   before-and-after measurement and an explicit shortfall statement; the machine-enforced gate is
   repo-wide at 80% and is not sensitive to one small class; a blanket exclusion must not be restored.
3. **csproj merge conflict with sibling child #491.** *Mitigation:* the D6 placement override puts the
   appended line 42 lines clear of the `Form1` region rather than two lines above it, so git's
   three-line merge context does not overlap.
4. **The `Views` indexer parameter type is unverified.** *Mitigation:* it is a one-token adjustment
   (`It.IsAny<object>()` to a typed argument) discovered at compile time and cannot invalidate the mock
   harness.
5. **A removed `using` directive turns out to be required.** *Mitigation:* self-verifying — the analyzer
   build fails with CS0246 or CS1061 and the directive is restored. Retention is zero-risk.
6. **The hygiene `using` removals are read as an unrelated refactor.** *Mitigation:* AC-8 requires the
   pull-request body to label them as hygiene and to note that three were already unused on green
   `main`.
7. **A `vstest` run omitting `/InIsolation` produces roughly 1,695 phantom failures.** *Mitigation:* the
   flag is stated in the toolchain, in AC-15, and in the epic Hard Constraints; a mass regression with
   empty messages and sub-millisecond durations is to be recognised as this effect and not "fixed."
8. **Removing an interface member is a breaking change.** *Mitigation:* zero compiled callers, zero mock
   setups, exactly one implementer edited in the same change, and the break called out per the general
   policy's compatibility clause.

### Mitigations and rollbacks

Rollback is a single revert of the pull request. The change is confined to one internal class, its
interface, one new test file, and one line of a test project file; no data, configuration, or external
contract is involved.

## Rollout & Follow-up

### Release/rollout steps

1. Capture the merge-base coverage baseline and the pre-change full-suite run under
   `evidence/baseline/` and `evidence/qa-gates/`.
2. Write the defect-2 `[expect-fail]` test plus the passing characterisation tests; observe the
   defect-2 test failing. This must precede any deletion, because deleting the region and the orphaned
   directives renumbers the file and would make the pre-change observation harder to reconstruct.
3. Apply the D2 one-line fix; the defect-2 test passes.
4. Apply D1, then D3, then D4, then D5. The analyzer and nullable builds are the gate for D1, D3, and
   D4.
5. Run the full five-step toolchain to a single clean pass; capture the post-change suite run and
   coverage figures.
6. Write both `fail-before-exception` dossiers.
7. Author the pull-request body via the `pr-author` skill, including the D1 removed-contract reference,
   the D4 hygiene label, the D5 coverage reasoning, and the cap-attribution statement.

### Post-fix monitoring or clean-up tasks

- Report the measured coverage figures upward for the epic's per-child NFR check.
- Two candidates for follow-up issues, to be filed **after** this child merges and **not** as documents
  under `docs/features/potential/**` (forbidden by the epic): the catch-asymmetry reading recorded in
  the removed-contract section, and consolidation of the three duplicated helper copies together with a
  split of `SortEmail.cs`.
- **Feature-folder name discrepancy — flagged, not resolved.**
  `docs/features/epics/quickfiler-suite-determinism-foundation/epic.md:33` declares
  `feature_folder: 2026-08-21-quickfiler-explorer-controller-latent-defects-449`, but the folder created
  by `mcp__drm-copilot__new_active_feature_folder` and in use is
  `docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449`. No `2026-08-21-*`
  folder exists. The epic manifest is outside this issue's scope; the orchestrator reports the real path
  upward for back-fill.

### Links: issue, PRs, related docs

- Issue: https://github.com/drmoisan/TaskMaster/issues/449
- Requirements mirror:
  `docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449/issue.md`
- Primary research:
  `docs/features/active/2026-08-07-quickfiler-explorer-controller-latent-defects-449/research/qfc-explorer-controller-defects.2026-08-21T18-20.md`
- Epic: `docs/features/epics/quickfiler-suite-determinism-foundation/epic.md`
- Policies cited: `CLAUDE.md` (Bugfix Workflow, C# Code Change Policy, General and C# Unit Test
  Policies, UT2 clause (c)), `.claude/rules/general-code-change.md`,
  `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/quality-tiers.md`,
  `.claude/rules/tonality.md`
- Pull request: to be added at creation time

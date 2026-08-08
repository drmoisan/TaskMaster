# quickfiler-efc-form-item-controller-coverage — Spec

- **Issue:** #452
- **Parent epic:** #136 `quickfiler-per-file-coverage` (child F9, wave 1, band C3)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** `full-feature` (acceptance criteria are authoritative in `spec.md` **and**
  `user-story.md`; `issue.md` is not the AC source for this mode)
- **Depends on:** F1 `quickfiler-coverage-ledger` (#432) — per-file harness and ratified exemption
  ledger
- **Research inputs:** the four per-file artifacts under `<FEATURE>/research/`
  (`EfcItemController.research.md`, `EfcFormController.research.md`, `EfcViewer.research.md`,
  `EfcViewer.Designer-and-measurement.research.md`)

## Overview

Epic #136 requires every testable production file compiled by `QuickFiler/QuickFiler.csproj` to
reach at least 80% line coverage, measured per file rather than per assembly. Child F9 owns the EFC
form/item controller cluster and the EFC viewer — four files:

| File | Lines (verified) | `[ExcludeFromCodeCoverage]` | Structural obligation |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/EfcItemController.cs` | 1,170 | Yes, `EfcItemController.cs:25` | 500-line split required |
| `QuickFiler/Controllers/EfcFormController.cs` | 1,086 | Yes, `EfcFormController.cs:27` | 500-line split required |
| `QuickFiler/Viewers/EfcViewer.cs` | 162 | Yes, `EfcViewer.cs:20` | `Form`-derived; no split needed |
| `QuickFiler/Viewers/EfcViewer.Designer.cs` | 4,277 | No attribute of its own | Generated; exempt from the 500-line rule |

F9 is the heaviest seam-extraction child in the epic (`epic.md:386-391`). It is the only child that
must simultaneously remove three exemption attributes, split two files that each breach the 500-line
limit, and take a position on a 4,277-line generated file that enters the coverage denominator as a
side effect of one of those attribute removals.

### Central finding — these files are unmeasured, not covered

The three attributed files do not appear in the committed Cobertura report
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
at all. `EfcHomeController.cs` **does** appear (`coverage-final.cobertura.xml:9`), which proves the
`QuickFiler` assembly and the `Controllers\` folder were both instrumented; the absence is caused by
the attribute, not by a tooling gap. Their coverage is unknown and, for `EfcItemController.cs`,
verifiably zero: a repository-wide grep for the identifier returns only `QuickFiler.csproj:301`, the
type's own declarations, the two `new EfcItemController(...)` sites in `EfcFormController.cs:69` and
`:87`, and documentation. **An absent file is not a covered file** (`epic.md:187`).

The one exception is `EfcFormController.cs`, which has exactly one existing test:
`QuickFiler.Test/Controllers/EfcFormControllerTests.cs` (55 lines, one `[TestMethod]`,
`PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel`). That test pins the
`_formViewer == null` early return at `EfcFormController.cs:1029-1031` and is a regression test for
issue #145. It is part of the spec (`CLAUDE.md` §7.3) and **must be migrated verbatim**, together
with its reflection-based private-no-arg-constructor helper `CreateMinimalController()`
(`EfcFormControllerTests.cs:18-28`, reaching `EfcFormController.cs:79`).

## Orchestrator Decisions (settled before planning)

These five decisions are recorded as decisions, not open questions. DEC-1 is the single exception:
it is a blocking gate requiring maintainer ratification before Phase 1.

### DEC-1 — `EfcViewer` Form construction is a BLOCKING OPEN DECISION requiring maintainer ratification

Removing `[ExcludeFromCodeCoverage]` from `EfcViewer.cs:20` also un-suppresses
`EfcViewer.Designer.cs` (4,277 lines), because the attribute sits on the partial **type**
(`public partial class EfcViewer : Form`, `EfcViewer.cs:21`) and C# merges attributes across all
partial declarations onto the single emitted type. `EfcViewer.Designer.cs` declares a bare
`partial class EfcViewer` (`EfcViewer.Designer.cs:7`) and carries no attribute of its own. There is
no type-level way to separate the two partials. This is the largest single planning consequence in
F9 and it is not visible from the file inventory.

Two mutually exclusive approaches are fully specified in `EfcViewer.research.md` §3. Both remove the
attribute; both take the same S1 seam (`IEfcViewerCommands`) and the same normal-test list.

**Approach A (recommended default).** One real `EfcViewer` constructed on a dedicated STA thread,
never shown, disposed in a `finally`, in a new `QuickFiler.Test/Viewers/EfcViewer.StaTests.cs`,
following the existing in-`QuickFiler.Test` pattern at
`QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:16-53`. Everything not
requiring a constructed form is covered in a plain `QuickFiler.Test/Viewers/EfcViewerTests.cs` using
`FormatterServices.GetUninitializedObject`. Projected: ~100% line and ~100% branch on
`EfcViewer.cs`, ~99% line on the Designer file, **adding roughly 2,000 covered lines** to the
repository and materially helping AC9. No generated code is edited.

**Approach B (pre-authorised fallback).** No Form construction anywhere. `GetUninitializedObject`
plus the S2 `ProcessCmdKeyBase` adapter seam (`EfcViewer.research.md` §4, S2) to make the
`base.ProcessCmdKey` fall-through reachable, plus **method-level** `[ExcludeFromCodeCoverage]` on
the Designer's `InitializeComponent` and `Dispose(bool)` (`EfcViewer.Designer.cs:18-25`). Projected:
~82% line and 100% branch on `EfcViewer.cs`; the Designer file stays out of the denominator,
forfeiting ~2,000 lines. Editing generated code is a durability defect — Visual Studio regenerates
`InitializeComponent` and silently drops the attribute — and there is **zero repo precedent**: a
grep for `ExcludeFromCodeCoverage` across `**/*.Designer.cs` repository-wide returns no matches.

**The conflict.** The inherited precedent
`docs/features/epics/winforms-testability-refactor/epic.md:74` states condition (d): "`Form`-derived
types remain prohibited in tests even when unshown." `EfcViewer` is a `Form` (`EfcViewer.cs:21`).
Under a literal reading of (d), Approach A is unavailable and Approach B is mandatory.

**The contrary evidence.** `QuickFiler.Test` and `UtilitiesCS.Test` already construct unshown Forms
on STA threads in passing tests:
`QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:31`
(`new BayesianPerformanceViewer(controller).Init()`, where `BayesianPerformanceViewer : Form` at
`QuickFiler/Viewers/BayesianPerformanceViewer.cs:8`);
`UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:49,137,205,323`;
`UtilitiesCS.Test/ReusableTypeClasses/ConfigViewer_Tests.cs:53,101,158`;
`UtilitiesCS.Test/EmailIntelligence/FolderSelector_Tests.cs:44,68,93`. Further,
`QuickFiler.Test/SetupAssemblyInitializer.cs:14-20` calls `Application.EnableVisualStyles()` and
`SetCompatibleTextRenderingDefault(false)` at `[AssemblyInitialize]` — infrastructure that exists
precisely so real controls can be constructed. The distinction the repository actually enforces is
**shown versus unshown**, not `Form` versus `Control`.

**Disposition.** DEC-1 requires maintainer ratification before Phase 1. Because both branches share
the same S1 seam and the same normal-test list, a reversal costs **one plan phase, not a re-plan**.
F9's plan must carry both a Phase 0 ratification gate and a Phase 0 headless-construction spike per
the risk register at `EfcViewer.research.md` §8 (top item: whether `BeginInit`/`EndInit` on the
`FolderListBox` WebView2 control at `EfcViewer.Designer.cs:882,891` triggers implicit CoreWebView2
initialization; unproven in this repository).

### DEC-2 — coverage numbers must NOT be read from Cobertura rate attributes

Open issue #441 is real and worse than its title. `Merge-CoberturaClassesByFilename`
(`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:167-292`) rebuilds merged `<lines>` correctly
by taking max hits per line (`:217-268`) but then **recomputes** `@line-rate` and `@branch-rate`
using the defective `.//lines/line` descendant selector (`:270-276`, calling
`Get-CoberturaCoverageSummary` at `:98`). Because a Cobertura `<class>` in this repository's output
carries each covered line twice — once under `<methods>/<method>/<lines>/<line>` and once in a
class-level rollup — the recomputation counts method lines plus class lines. **Per-file rates are
corrupted, not merely the repository total.**

Proven arithmetically on `QuickFiler\Controllers\FilerQueue.cs`
(`coverage-final.cobertura.xml:18365-18480`), recorded as `line-rate="0.405797"`
`branch-rate="0.428571"`:

- Class-level `<lines>` (`:18412-18479`): 49 distinct lines, 18 with `hits > 0`. True line rate
  18/49 = 0.367347.
- Method-level `<lines>` (`:18368-18410`): 20 lines, 10 covered.
- Defective sum (18+10)/(49+20) = 28/69 = 0.4057971 — an exact match to the recorded value.
- Branch, class level: 5/10 = 0.5 true. Method level 1/4. Defective sum 6/14 = 0.428571 — exact
  match.

Note the direction is not uniform: FilerQueue's line rate is overstated (40.6 versus 36.7) while its
branch rate is understated (42.9 versus 50.0). **Consequence: the epic's own baseline table at
`epic.md:161` is wrong** — `Controllers/FilerQueue.cs` is 49 lines at 36.7% line / 50.0% branch, not
"69 lines, 40.6%". F9 must report this correction to the epic orchestrator.

**Binding derivation rule for F9.** Every acceptance number F9 cites is computed from the
**direct-child** axis `/coverage/packages/package/classes/class/lines/line`, grouped by the
`class/@filename` attribute, de-duplicated by `@number` taking `max(@hits)`. The denominator is the
count of distinct `<line>` nodes on that axis; a file with zero such nodes reports `N/A`, never
`0%`. Branch figures are summed from `condition-coverage="(c/t)"` on direct-child lines where
`@branch="True"`. F9 **never** reads `class/@line-rate`, `class/@branch-rate`, the root
`coverage/@lines-valid`, or the root `coverage/@line-rate`, and **never** uses the descendant axis
`.//lines/line`.

**Detection tell.** `Get-CoberturaCoverageSummary` rounds to six decimals
(`Invoke-MSTestWithCoverage.Helpers.ps1:137-138`) while `dotnet-coverage` emits full double
precision. A 16-significant-digit rate was never merged and is trustworthy; a rate with six or fewer
decimals has been through the defective path.

**Residual imprecision to disclose, not fix.** `Merge-CoberturaClassesByFilename:240-261` does not
union `<conditions>` across a merged group; it picks the candidate line with the larger `Total`.
Branch figures on merged files are therefore a best-of, not a true union. Record this as a known
limitation in the evidence artifact. `Invoke-MSTestWithCoverage.Helpers.ps1` is a shared file
outside F9's assignment and **must not be edited by this child**.

### DEC-3 — toolchain command correction

`CLAUDE.md` §C#1 and §CUT3 state `csharpier .`. That is csharpier v0 syntax and fails against the
pinned version 1.2.6 (`dotnet-tools.json:5-11`, `rollForward: false`). The working forms, verified in
`.vscode/tasks.json:53-66` ("format: csharpier" → `dotnet tool run csharpier format .`), are:

- Mutating: `dotnet tool run csharpier format .`
- Non-mutating baseline capture: `dotnet tool run csharpier check .`

Three further environment facts, all verified:

- `.dotnet-sdk/` is **absent from this worktree**. `global.json:2-11` pins SDK 8.0.205 with
  `paths: [".dotnet-sdk", "$host$"]` and carries an error message directing to
  `./scripts/vscode/Install-RepoDotNetSdk.ps1`. Phase 0 needs a bootstrap task.
- The tool manifest is at the **repository root** (`dotnet-tools.json`), not `.config/dotnet-tools.json`.
  A repo-wide glob for `**/dotnet-tools.json` returns only the root file. If
  `dotnet tool run csharpier` cannot find the manifest, run it from the repository root or run
  `dotnet tool restore` from the root first.
- `msbuild` and `vstest.console.exe` resolve via `vswhere`, **not** `PATH`
  (`scripts/vscode/Invoke-VSBuild.ps1:127-135`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1:284-290`).
  `dotnet-coverage` must be on `PATH`; the script throws if absent (`:292-294`).

Recorded as a documented deviation from the literal `CLAUDE.md` text, to be confirmed by a Phase 0
task. `.vscode/tasks.json` and the pinned tool version are treated as authoritative over the
`CLAUDE.md` prose for this one command form only; the toolchain **order** in `CLAUDE.md` §CUT3 is
unchanged and binding.

### DEC-4 — latent defects are already promoted

Eight GitHub issues were promoted during F9 preparation, so AC11 is satisfied for every
research-discovered defect. **F9 must NOT fix any of them.** See the "Latent Defects" section for
the full list and mechanisms.

Pure hygiene that **is** in scope during the partial split, because it is a deletion of dead
commented code and unreferenced `using` directives in files F9 already rewrites, not a behavior
change:

- `EfcItemController.cs:452-533` — 82 lines of commented-out `ConversationInfo` /
  `ConversationItems` / `DfConversation` code.
- `EfcItemController.cs:115-134` — dead commented code inside `Initialize`.
- `EfcFormController.cs:605-623` — 19 lines of superseded `GetKbdActions`, plus the smaller
  commented fragments at `:147-148`, `:304-305`, `:311-312`, `:317-318`, `:323-324`, `:583-586`,
  `:735`, `:737`, `:827`, `:1002-1006`.
- `EfcViewer.cs:107-155` — 49 lines of commented-out code, of which `:121-137` and `:139-155` are
  byte-identical duplicates of each other.
- Unused `using` directives: `EfcFormController.cs:4,7,8,10,20`; `EfcViewer.cs:3,4,6,7,8,9,15`
  (verify each with IDE0005 before removing).

### DEC-5 — request a ledger clarification from F1

`EfcViewer.Designer.cs` needs a semantic that F1's three buckets (`testable`, `ratified-exempt`,
`interface-only / not-measured`, fixed at `epic.md:519-521`) do not express: **measured, counted
toward repository-wide coverage, but not gated on the per-file 80/75 floors.**

Its branch rate is approximately 0.50 **by construction**: the only branching statement in the file
is `Dispose(bool)`'s `if (disposing && (components != null))` (`EfcViewer.Designer.cs:20`), and
`components` is initialized to `null` at `EfcViewer.Designer.cs:12` and never reassigned, so the
condition can only ever be exercised in one direction. Every comparable designer in the committed
report sits at exactly 0.50 (`ItemViewerExpanded.Designer.cs`,
`BayesianPerformanceViewer.Designer.cs`).

Classifying the file `testable` therefore makes AC2 unsatisfiable by construction. Classifying it
`ratified-exempt` in the attribute-bearing sense throws away roughly 2,000 free covered lines and
requires editing generated code. F9's plan must request that F1 either (a) state that
`ratified-exempt` means "exempt from the per-file gate", explicitly decoupled from "carries
`[ExcludeFromCodeCoverage]`", or (b) add a `generated / measured-not-gated` bucket. Without that
clarification F16 will either fail the file on branch coverage or wrongly demand an attribute. The
same reasoning applies to the other seven `*.Designer.cs` files in the epic (F14, F15), so this is a
cross-child clarification, not an F9-local one.

## Baseline Evidence and Its Provenance

**There is no baseline for F9's files.** All four are absent from
`coverage-final.cobertura.xml`; enumerating every `filename` attribute in that report returns 71
distinct QuickFiler files and none of them is `QuickFiler\Controllers\EfcFormController.cs`,
`QuickFiler\Controllers\EfcItemController.cs`, `QuickFiler\Viewers\EfcViewer.cs`, or
`QuickFiler\Viewers\EfcViewer.Designer.cs`. F9 must not cite any number for its four files as a
starting point; there is none.

**Acceptance authority.** F1 (#432) delivers the per-file coverage harness that is the sole per-file
evidence mechanism for epic #136 (`epic.md:257-263`). F9 measures all four files with that harness
**on F9's own branch**, after F1 merges to the integration branch, computes every rate by the DEC-2
derivation rule, and commits the numeric result under `<FEATURE>/evidence/qa-gates/`.

**Upstream dependency is currently unmet.** Verified on this branch:
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` does not exist; a `Glob` of
`docs/features/epics/quickfiler-per-file-coverage/**` returns exactly one file, `epic.md`; there is
no F1 feature folder under `docs/features/active/`; and a repo-wide search for `coverage-ledger`,
`PerFileCoverage`, and `Get-PerFileCoverage` returns only prose in sibling feature folders, never a
script. **There is no per-file coverage report generator anywhere in the repository today.** F9's
Phase 0 halt gate on F1's deliverables is real and currently unmet. Precedent for the gate:
`docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/plan.2026-08-07T20-41.md:59,63,65`
(F3's `[P0-T4]`, `[P0-T6]`, `[P0-T7]`).

**Phase 0 halt-gate conditions (all eight must pass; any failure is `BLOCKED`, not a workaround).**
G1 ledger exists and is non-empty; G2 it carries a row for each of F9's four files, transcribed
verbatim with line citations; G3 attribute dispositions are explicit for all three attributed files
(F9 must not self-grant or self-revoke an exemption); G4 the `EfcViewer.Designer.cs` disposition
states both the bucket **and** the enforcement mechanism once the type-level attribute is removed
(see DEC-5 — this is the condition most likely to be missing from F1's first draft); G5 the ledger
states classification **rules**, not only rows (`epic.md:576-578`), because F9 creates new files that
post-date the ledger; G6 the harness exists at the documented path and runs to completion against a
committed Cobertura XML; G7 the harness contract is recorded with four specific answers —
`AGGREGATION_BASIS: filename`, `LINE_SELECTION_AXIS` (the descendant axis is the #441 defect and must
be rejected), `DENOMINATOR_BASIS: line-node-count`, `ZERO_OVER_ZERO_REPORTING: N/A`; G8 the harness
emits **both** line and branch rates per file.

If G7 reveals that F1's harness reads `@line-rate` or uses the descendant axis, F9's correct move is
to raise a defect against F1 with the exact text at
`research/EfcViewer.Designer-and-measurement.research.md` §2.4 — record dissent, do not fabricate a
local workaround that produces a second inconsistent number.

## Behavior

No change to observable QuickFiler behavior. F9 extracts injectable seams, splits two oversized
controllers into cohesive partials, removes three exemption attributes, and adds deterministic MSTest
coverage. Every production edit is behavior-preserving. Public constructor signatures consumed by
F8's dependency factories are unchanged; new test-facing entry points are added as **explicit
overloads**, never as optional parameters on an existing signature.

## Corrections to Seeded Assumptions

Each of the four research artifacts disproved statements in the delegation brief, `issue.md`, or
`epic.md`. Each correction below is evidenced.

### C1 — `EfcViewerQueue.Dequeue()` is not a constraint on any F9 file

`issue.md:69` carries the F4 method-group rule forward. It is a real repository-wide rule, but
neither `EfcItemController.cs` nor `EfcFormController.cs` references `EfcViewerQueue` at all. The
method-group consumption is in F8's
`QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:40` and `:112`
(`ProductionViewerFactory { get; set; } = EfcViewerQueue.Dequeue;`). For F9 this is **inherited
context only**.

**However, F9 creates an equivalent obligation of its own.** Seam S4 for `EfcItemController`
converts F4-owned `EfcThemeHelper.SetupThemes` (`QuickFiler/Helper Classes/EfcThemeHelper.cs:16-27`)
to a delegate by method group. If F4 adds an optional parameter to that signature, the conversion
breaks at compile time. Recorded as a new cross-child contract note to F4 below.

### C2 — `QuickFiler` DOES grant `InternalsVisibleTo("QuickFiler.Test")`

`issue.md:80-86` and `epic.md:619-631` state the assembly-boundary constraint. It is correct **as to
`UtilitiesCS`**: `UtilitiesCS/Properties/AssemblyInfo.cs` grants only `DynamicProxyGenAssembly2`,
`UtilitiesCS.Test`, and `ToDoModel.Test`, so `MyBox.DialogInvoker` and
`UtilitiesCS.Threading.WpfUiDispatcher(Dispatcher)` (whose `Dispatcher`-taking constructor is
`internal`, `UtilitiesCS/Threading/WpfUiDispatcher.cs:30`) are unreachable from a QuickFiler test.

But `QuickFiler/Properties/AssemblyInfo.cs:5` carries
`[assembly: InternalsVisibleTo("QuickFiler.Test")]` (verified). **QuickFiler's own internals need no
visibility seam.** `EfcItemController` (an `internal class`), `EfcViewer.SetController`,
`EfcViewer.KeyboardHandler`, `EfcViewer.BreadcrumbWebView`, and every `internal` delegate seam F9
adds are directly reachable from `QuickFiler.Test` without reflection. Reflection remains necessary
only for `private` members.

The `UtilitiesCS` half of the constraint stands and drives two F9 decisions: a **local** dialog
delegate seam (S5 on `EfcFormController`) rather than `MyBox.DialogInvoker`, and a **local**
`ItemViewerUiDispatcher : IUiDispatcher` adapter (S7 on `EfcItemController`) rather than the
existing `WpfUiDispatcher(Dispatcher)` overload.

### C3 — `[STATestClass]` needs no new package, but `QuickFiler.Test` has no `*.StaTests.cs` today

`STATestClassAttribute` / `STATestMethodAttribute` ship in
`Microsoft.VisualStudio.TestTools.UnitTesting` from MSTest.TestFramework, and
`QuickFiler.Test/packages.config:119` pins 4.3.3. No package reference is required. `QuickFiler.Test`
also has no `.runsettings` of its own (`QuickFiler.Test.csproj:29-30`, `<RunSettingsFilePath>` empty).

The existing in-`QuickFiler.Test` STA idiom is a manual `Thread` + `SetApartmentState`, not the
attribute — `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:16-53` and
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:297-326`
(`StartRunningDispatcher` / `ShutdownDispatcher`). **F9 would create the project's first
`*.StaTests.cs` file.** Under Approach A, use the worker-thread helper *inside* a file named
`QuickFiler.Test/Viewers/EfcViewer.StaTests.cs` and mark the class `[STATestClass]` so AC7's naming
and scoping requirements are both satisfied literally.

### C4 — issue #450 concerns `QfcFormControllerTests.cs`, not the EFC tests

`issue.md:95` asks F9 to verify. Issue #450
(`Refactor: quickfiler-formcontroller-tests-file-size-split`) names
`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` — 827 lines, 42 test methods, breaching the
500-line limit. That file exists locally and is registered at `QuickFiler.Test.csproj:117`. It is
**F6 territory, not F9's**. F9's own new test files must nonetheless each stay under 500 lines, which
is why the test inventories are partitioned across many files.

### C5 — `EfcViewer3.cs` is not in the csproj and is outside F9's scope

`QuickFiler/Viewers/EfcViewer3.cs` and `QuickFiler/Viewers/EfcViewer3.Designer.cs` exist in the
working tree and contain a near-duplicate `SetController` / `SetKeyboardHandler` /
`InitTipsLabelsList` surface (`EfcViewer3.cs:24-57`), and `EfcViewer3.cs:17` even carries
`[ExcludeFromCodeCoverage]`. Neither has a `<Compile Include>` entry — a grep for `EfcViewer3` across
`QuickFiler/QuickFiler.csproj` returns **zero matches** (verified). Per `epic.md:574-575` the
denominator is the csproj compile set at evaluation time, so both files are outside the epic. **Do
not touch them**; they are promoted as part of issue #466.

### C6 — `epic.md` contradicts itself on the exemption count

The marker-accuracy note at `epic.md:121-130` corrects the exempted-file figure from 33 to **21
compiled files carrying a real attribute**. Two later sentences still say 33: `epic.md:223` ("The 33
existing attributes are treated as unratified") and `epic.md:324` ("each of the 33 existing
`[ExcludeFromCodeCoverage]` attributes assigned to its owning child"). F1's ledger must use 21; the
two stale sentences should be fixed when the epic is next edited. F9 raises this to the epic
orchestrator and does not edit `epic.md` itself.

### C7 — pooled-viewer handler accumulation does not occur

A planner might assume that reused `EfcViewer` instances accumulate event handlers across sessions.
They do not. `ViewerQueueCore.Dequeue` (`QuickFiler/Helper Classes/ViewerQueueCore.cs:63-85`) is
consume-once: it dequeues an instance and refills the queue with **new** instances from
`_viewerFactory()`, and `EfcViewerQueue.CreateProductionViewer`
(`QuickFiler/Helper Classes/EfcViewerQueue.cs:81-84`) is `new EfcViewer()`. Every
`EfcHomeController` session receives a fresh viewer. The "pooled-viewer re-initialization" language
at `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:18-19,88-89` is defensive, not descriptive. Do not
plan tests around handler accumulation.

### C8 — inter-artifact conflict on the `EfcViewer.cs` attribute, resolved

`EfcFormController.research.md` §5.3 and §11 item 3 recommend **keeping**
`[ExcludeFromCodeCoverage]` on `EfcViewer.cs:20` as the ratified irreducible remainder under
`CLAUDE.md` §UT2 ground (b). `EfcViewer.research.md` §3 and §11 recommend **removing** it under both
Approach A and Approach B. **DEC-1 settles this: the attribute is removed.** The
`EfcFormController` artifact's recommendation was written without the finding that the attribute
also suppresses 4,277 lines of Designer code, and without the counter-precedent evidence in
`EfcViewer.research.md` §9. The residual host-bound wiring that lands in `EfcViewer.cs` under the S1
`IEfcFormViewer` seam is covered by the Approach A test list rather than exempted.

### C9 — the `IEfcFormViewer` seam does NOT have the cross-child blast radius the viewer artifact feared

`EfcViewer.research.md` §4, S3 flags a full `IEfcViewer` interface as changing F8-owned signatures
(14 sites in `EfcHomeControllerDependencies.cs` / `EfcHomeControllerDependencyFactories.cs`) and an
F3-owned overload (`KeyboardHandler.cs:35`). That concern does not apply to the S1 design actually
selected, because the interface is **implemented by** `EfcViewer` and only the controller's private
field is retyped:

- `EfcFormController`'s two public constructors keep their concrete `EfcViewer` parameter type, with
  an implicit upcast at field assignment, so
  `EfcHomeControllerDependencies.FormControllerWithDataFactoryDelegate` and
  `FormControllerWithoutDataFactoryDelegate` (`EfcHomeControllerDependencies.cs:15-32`) are
  untouched.
- `KeyboardHandler(EfcViewer viewer, IFilerHomeController parent)` (`KeyboardHandler.cs:35`,
  verified) still receives an `EfcViewer`, which still is an `EfcViewer`. No F3 edit.

### C10 — minor line-count discrepancies

`EfcViewer.Designer.cs` measures **4,277** lines; `issue.md:31` and `epic.md:114,389` say 4,276.
`epic.md:390` sizes F9 at "~2,418 testable lines / 4 files", which understates the measured
denominator: once the attribute is removed the Designer file contributes an estimated 1,500–2,500
coverable lines. The plan's coverage arithmetic must use the exposed figure, not the manifest figure.

## Scope Amendments

| Amendment | Reason | Disposition |
| --- | --- | --- |
| `EfcViewer.cs` attribute **removed**, not retained (C8) | The attribute also suppresses 4,277 Designer lines; counter-precedent evidence in `EfcViewer.research.md` §9 | Settled by DEC-1; both approaches remove it |
| `EfcItemController.InitializeWebView()` (`:174-205`) deleted rather than seamed | Zero callers repository-wide (verified by grep across `QuickFiler/` and `QuickFiler.Test/`); a no-behavior-change removal of an uncalled `internal` method | Its own atomic task so the deletion is reviewable; alternative (seam it through `IWebViewCoreInitializer`, ~10 lines) is recorded |
| `WebView2Control_CoreWebView2InitializationCompleted` body extracted to `internal void OnWebViewInitialized(bool isSuccess, Exception initializationException)` | `CoreWebView2InitializationCompletedEventArgs` has no public constructor; the alternative is `GetUninitializedObject` plus reflection into a third-party SDK type | Two-line forwarding shim costs ~2 uncovered lines instead of a method-level exemption; matches `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy |
| STA scope reduced to **at most one file** (`EfcViewer.StaTests.cs`, Approach A only) | Per-member analysis found zero STA-bound tests required for either controller | `EfcItemController` and `EfcFormController` need no `*.StaTests.cs` — a materially better outcome than the epic anticipated for this cluster |
| `EfcViewer.cs` **not** split | 162 lines currently, projected ~330 after the S1 intent members; both well under 500 | No split; `EfcViewer.Designer.cs` is exempt from the 500-line rule as generated code (`epic.md:254-255`, AC4) |

## Cross-Child Contract

F9 makes **zero edits** to any sibling-owned file. The contract points below are recorded so a
sibling change that breaks F9 is caught at fan-in rather than misdiagnosed.

### CCN-1 — F8 (#437): `EfcHomeControllerDependencies.cs` / `EfcHomeControllerDependencyFactories.cs` are read-only, and no F8 edit is required

Verified that neither file references any member of `EfcFormController` other than the two public
constructors, `Initialize()` (`EfcHomeControllerDependencyFactories.cs:80`),
`InitializeWithoutData()` (`:92`), and `InitializeDataFields(EfcDataModel)` (`:105`). All four
survive F9's refactor unchanged. Neither file mentions `EfcItemController` at all — F8's factory
graph stops at `EfcFormController`, which constructs the item controller itself
(`EfcFormController.cs:69`, `:87`). F9's dependency bundles are therefore additive and
non-overlapping. F8's own preparation independently concluded that all of its changes are additive
and test-only, so the dependency is satisfied in both directions with no coordination edit.

### CCN-2 — F12 (#1012): `BreadcrumbBridgeRouter` needs no F12 edit

`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:19` is `public sealed` and therefore not mockable,
but it is **fully constructible headlessly**. Its constructor (`:40-55`) takes
`IFolderHierarchyProvider` (interface → `Mock<>`), `IBreadcrumbWebHost` (interface → `Mock<>`,
`QuickFiler/Viewers/IBreadcrumbWebHost.cs:11`), and three plain classes: `BreadcrumbMessageCodec`,
`BreadcrumbHtmlRenderer`, and `BreadcrumbOutboundQueue`
(`QuickFiler/Controllers/BreadcrumbOutboundQueue.cs:23`, takes only an `IBreadcrumbWebHost`). No
WebView2, no WinForms, no COM. F9's tests construct a **real** router over mocks, which is a stronger
test than a mocked router because it exercises the real selection contract that
`EfcFormController.SelectedFolder` (`:289-295`) depends on.

F9 depends on six public surface points plus one event: the constructor, `BindRowsAsync`,
`SelectedFolderPath`, `SelectFirstRow()`, `ApplyTheme(bool)`, `NotifyCoreInitialized()`, and
`FocusSearchRequested`. If F12 changes constructor arity, seals off `SelectFirstRow`, or changes
`SelectedFolderPath`'s derivation (`BreadcrumbBridgeRouter.cs:364-380`), F9's tests break at fan-in.
Watch item, not a blocker. **F9 must not edit `BreadcrumbBridgeRouter.cs`.**

### CCN-3 — F4 (#434): new method-group obligation on `EfcThemeHelper.SetupThemes`

Symmetric with the existing `EfcViewerQueue.Dequeue` rule and **new with F9**. Seam S4 declares an
F9-owned delegate mirroring the 10-parameter signature at
`QuickFiler/Helper Classes/EfcThemeHelper.cs:16-27` with the production default
`EfcThemeHelper.SetupThemes` supplied as a **method group**. Method-group conversion does not
tolerate optional parameters. **If F4 ever needs to change that signature it must add an overload,
never an optional parameter.** `EfcThemeHelper.SetupFormThemes` (`:249-255`) is consumed as an
ordinary `public static` call at `EfcFormController.cs:239` and carries no such constraint. F9 does
not edit `EfcThemeHelper.cs`.

### CCN-4 — F13 (#1013): `CoreInitialized` is on the concrete host, not the interface

`WebView2BreadcrumbHost.CoreInitialized` is declared on the concrete class
(`QuickFiler/Viewers/WebView2BreadcrumbHost.cs:63`) and **not** on `IBreadcrumbWebHost`
(`IBreadcrumbWebHost.cs:11-26`). F9's `BreadcrumbHostFactory` seam therefore returns the concrete
`WebView2BreadcrumbHost`. If F13 promotes the event onto the interface, F9 may widen the factory's
return type afterwards — a follow-up, not a prerequisite. `WebView2CoreInitializer.cs:15` already
carries `[ExcludeFromCodeCoverage]` with a pure-forwarding-adapter rationale; F9 consumes
`IWebViewCoreInitializer` (`QuickFiler/Viewers/IWebViewCoreInitializer.cs:13-29`) and edits neither.

### CCN-5 — F5 (#1005) and F14 (#1014): consumed through seams, never edited

`EfcDataModel.cs` is F5-owned. Its `ConversationResolver` setter is `protected`
(`EfcDataModel.cs:218`), `MailInfo` is a computed non-virtual property (`:232`), and the type is
`internal` with no virtual members, so Moq cannot substitute it. F9 introduces an F9-owned adapter
(`EfcDataModelSource`) and delegate seams instead of retyping or extending `EfcDataModel`. **Zero F5
edits.**

`IItemViewer` (`QuickFiler/Viewers/IItemViewer.cs:15-132`) declares `Height` (`:128`) but **not**
`MinimumSize` and **not** `L0vh_Tlp`. `EfcFormController` therefore must **not** retype its
`_itemViewer` field as `IItemViewer`; the layout reads move behind
`IEfcFormViewer.CaptureItemViewerLayout()` instead. `ItemViewer.cs:20`'s `[ExcludeFromCodeCoverage]`
is F14-owned; **F9 must not remove it**, and under Approach A F9's STA construction will incidentally
exercise `ItemViewer.Designer.cs` once F14 removes it. **Zero F14 edits.**

## Scope of Work Per File

Exact member inventories, per-member testability verdicts, and named test lists live in the research
artifacts and are not restated here.

### `EfcItemController.cs` — 1,170 lines, attribute at `:25`

**Partial split: 8 files**, cut along the existing `#region` boundaries so the diff is a near-pure
move. Precedent: `QfcItemController.{Initialization,ViewerSetup,Conversation,FolderHandling,EventWiring,EventHandlers,Navigation,FocusAndTheme,MailActions}.cs`
and `EfcHomeController.{Metrics,ExecuteMoves,Timing}.cs`.

| File | Contents (source lines) | Projected |
| --- | --- | --- |
| `EfcItemController.cs` | class decl, `logger` (167-169), fields (363-384) + seam fields, three ctors (30-74), `InitializeWithoutData` (76-88), `InitializeDataFields` (90-112), `Initialize` (114-165), `Cleanup` (255-278) | ~250 |
| `EfcItemController.Properties.cs` | exposed properties (386-638) minus `LoadTheme`; ~195 once the 82-line dead block at 452-533 is removed | ~275 |
| `EfcItemController.ViewerSetup.cs` | `AdjustViewerForEfc`, `ResolveControlGroups`, `PopulateControls`, `PopulateConversation`, `SetTopicThread` | ~125 |
| `EfcItemController.WebView.cs` | `InitializeWebViewAsync`, `OnWebViewInitialized` + shim, `HtmlDarkConverter` | ~135 |
| `EfcItemController.EventWiring.cs` | `WireEvents`, `RegisterActions`, `Register/UnregisterAsyncFocusActions`, `UnregisterActions`, `KbdExecuteAsync`, `JumpToAsync`, `RightKeyActions` | ~155 |
| `EfcItemController.EventHandlers.cs` | `ConversationResolverPropertyChanged`, `TopicThread_ItemSelectionChanged`, `DarkMode_Changed`, `Button_MouseEnter/Leave` | ~95 |
| `EfcItemController.Navigation.cs` | the whole UI-Navigation region (836-1077) | ~270 |
| `EfcItemController.Theme.cs` | `LoadTheme`, `SetThemeDark`, `SetThemeLight`, `ApplyReadEmailFormat`, `SetOlvTheme` | ~95 |

**Largest projected file ~275 lines — clears the 500-line limit with margin.** Total grows from
1,170 to roughly 1,400 because each partial carries its own `using` block, namespace, and class
header (~26 lines each) plus the new seam fields.

**Seams (S1-S10).** S1 retype `_itemViewer` from concrete `ItemViewer` to the existing `IItemViewer`
interface — 18 concrete member accesses are verified 1:1 forwards to existing `IItemViewer` intent
members (`ItemViewer.DisplayState.cs:13-71`, `ItemViewer.Commands.cs:67-101`,
`ItemViewer.WebViewThread.cs:15-32`), and the two call sites at `EfcFormController.cs:69,87` compile
unchanged because the concrete type implements the interface (`ItemViewer.cs:21`). In-repo
precedent: `QfcItemController.cs:51`. S2 new `IEfcExpansionStyleHost` (one member) over the
`_parent` field; `EfcFormController` already has `public void ToggleExpansionStyle(Enums.ToggleState)`
at `:1056`, so the interface addition needs no new member, and the `Parent` property (`:566-570`) has
zero consumers repo-wide so the retype is safe. S3 new `IEfcItemDataSource` (three members:
`MailInfo`, `ConversationResolver`, `Mail`) plus an `EfcDataModelSource` adapter, with a **new
internal overload** `InitializeDataFields(IEfcItemDataSource)` alongside the unchanged
`InitializeDataFields(EfcDataModel)`. S4 the `EfcThemeFactory` delegate (CCN-3). S5 the existing
`IWebViewCoreInitializer`. S6 `UiThread.Dispatcher` → `IUiDispatcher` with the public parameterless
`WpfUiDispatcher()` default. S7 a local `ItemViewerUiDispatcher : IUiDispatcher` over
`IItemViewer.UiDispatcher` (required by C2 — do **not** reuse S6's dispatcher here; the two are not
provably the same instance and substituting one for the other would be a behavior change). S8 an
injectable background-start delegate `Func<Func<Task>, Task>` defaulting to `f => Task.Run(f)`,
replacing the fire-and-forget `Task.Run(...)` at `:110` and `:164`. S9 a new
`IEfcItemControlSurface` adapter holding the residual raw-control access, deliberately shaped so all
arithmetic and branching stay in the testable controller and only property reads/writes move into
the adapter. S10 an `EfcItemControllerDependencies` bundle mirroring the shape (not the file) of
F8's `EfcHomeControllerDependencies`.

**Projected coverage.** ~155 named test cases across nine test files, all plain
`[TestClass]`/`[TestMethod]` — **zero STA-bound tests**. Distribution approximately 63 positive, 12
negative, 25 edge, 16 error, 39 state-transition. Every member is testable after seam; **zero methods
on the type itself are irreducible-remainder candidates.**

**The one exemption candidate** is the new adapter `QuickFiler/Viewers/EfcItemControlSurface.cs`:
every member is a one-line forward to the concrete `ItemViewer`, which is itself
`[ExcludeFromCodeCoverage]` (`ItemViewer.cs:20`, F14-owned). Exercising even one forward requires
constructing a real `ItemViewer`, whose constructor runs `InitializeComponent()` over a 6,224-line
Designer that instantiates a WebView2 control (`ItemViewerExpanded.Designer.cs:44`) — an
external-process dependency prohibited by `.claude/rules/general-unit-test.md` § External
Dependencies independently of the STA question. The adapter contains no branching, no arithmetic,
and no state. Direct in-repo precedent for exempting a pure forwarding adapter:
`WebView2CoreInitializer.cs:15`.

**Explicitly NOT irreducible** — the plan must not drift into exempting these: `InitializeWebViewAsync`
(routing-testable once S9 removes the `((ItemViewer)_itemViewer)` casts that forced F10 to exempt its
analogue at `QfcItemController.ViewerSetup.cs:38`), `WireEvents`, `ResolveControlGroups`,
`AdjustViewerForEfc`, `ToggleExpansionOn/Off`, and `RegisterActions`.

### `EfcFormController.cs` — 1,086 lines, attribute at `:27`

**Partial split: 8 files.** Naming follows `QfcFormController.cs` / `.SetupDisposal.cs` /
`.EventHandlers.cs` / `.Actions.cs`.

| File | Contents | Projected |
| --- | --- | --- |
| `EfcFormController.cs` | class decl, fields + seam fields, both public ctors (32-77), private ctor (79), new internal test ctors, `Initialize` / `InitializeWithoutData` / `InitializeDataFields` (81-119) | ~200 |
| `EfcFormController.Properties.cs` | `ActiveTheme`, `LoadTheme`, `DarkMode`, `FormHandle`, `SelectedFolder`, four settings properties, `Token`, `IsValidSelection` | ~155 |
| `EfcFormController.Setup.cs` | `CaptureConfigureItemViewer`, `Cleanup`, `ConfigureFind`, `ResolveControlGroups`, `SetupThemes`, `LoadUserSettings`, `ToggleExpansionStyle` | ~165 |
| `EfcFormController.EventHandlers.cs` | `RegisterAlwaysOnAsyncKeyActions`, `WireEventHandlers`, `SearchText_DownArrow`, five `Button*_Click`, four `*_CheckedChanged`, `SearchText_TextChanged`, `EditFiltersMenuItem_Click`, `DarkMode_Changed` | ~265 |
| `EfcFormController.KeyboardActions.cs` | `CharacterAsyncActions` / `GetAsyncCharacterActions`, `CharacterActions` / `GetKbdActions` (dead block 605-623 deleted), `KbdExecuteAsync` ×2, `JumpToAsync`, `ShowMenu`, `ToggleCheckboxAsync`, four `ToggleOn/OffNavigation` | ~195 |
| `EfcFormController.Actions.cs` | `ActionOkAsync`, `ActionCancelAsync`, `ActionDeleteAsync`, `CreateFolderAsync`, `RefreshSuggestionsAsync`, `PopulateFolderCombobox` | ~165 |
| `EfcFormController.Breadcrumb.cs` | `ConfigureBreadcrumbControl`, `InitializeBreadcrumbHostAsync`, `BindFolderRows`, `BindBreadcrumbRowsAsync` + the factory-seam properties | ~135 |
| `EfcFormController.Tips.cs` | `ToggleTips` ×2, `ToggleTipsAsync`, `MaximizeFormViewer`, `MinimizeFormViewer` | ~100 |

**Largest projected file ~265 lines — clears the 500-line limit with margin.**

**Critical planning constraint:** per-file coverage is measured per Cobertura `filename`, and each
partial emits its own `filename`. **Every one of the eight partials must independently clear 80%
line / 75% branch.** The split must not be used to concentrate uncovered lines into a partial that is
then exempted; `[ExcludeFromCodeCoverage]` on any `EfcFormController.*.cs` partial is a Blocking
finding under `epic.md:220-225`.

**Seams (S1-S9).** S1 a new `IEfcFormViewer : UtilitiesCS.Interfaces.IWinForm.IForm` implemented by
`EfcViewer`, mirroring the already-merged `IQfcFormViewer` / `QfcFormViewer` / `QfcFormController`
triple (`IQfcFormViewer.cs:12`, `QfcFormViewer.cs:18`, `QfcFormController.cs:168`,
`QfcFormControllerTests.cs:103`). `IForm`/`IControl` supply `Handle`, `Dispose()`, `Close()`,
`Hide()`, `Select()`, `MinimumSize`, `Size`, `Text`, `WindowState`, `Invoke`, and `BeginInvoke` for
free — do not redeclare them. S2-S3 injectable-delegate seams for `EfcHomeController` and
`EfcDataModel`, using the already-merged in-family `X is null ? concrete : X` idiom
(`EfcHomeController.ExecuteMoves.cs:86-109`, `EfcHomeController.cs:294-305`). S4 breadcrumb
construction factories (CCN-2, CCN-4). S5 dialog seams — `MessageBoxShowAction` and
`ShowManageFiltersAction` — required by C2. S6 a user-settings reader replacing the
`QuickFiler.Properties.Settings.Default` static singleton read at `:1009-1022`. S7 item-controller
delegate seams. S8 a new pure `EfcFormLayoutMath` module extracting the layout arithmetic from
`CaptureConfigureItemViewer` (`:166-187`) and `ToggleExpansionStyle` (`:1056-1084`). S9 nothing new —
`IOlObjects` (`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:11,30,36`) is already a full interface.

**Projected coverage.** ~126 named test cases across eight test files plus a shared
`EfcFormController.TestSupport.cs` harness. **Zero STA-bound tests and zero irreducible-remainder
candidates in this file.** `async void` handlers are observed with a `TaskCompletionSource` completed
from inside the injected seam delegate — never a delay.

**net481 constraint.** `EfcItemViewerLayoutSnapshot` and `EfcUserSettings` must be plain
`readonly struct` types with positional constructors, not `record` or `record struct`: this target
framework has no `IsExternalInit` polyfill and `init`-only setters fail with CS0518. Precedent:
`ResourceTimingRow`.

**Nullable-gate risk.** Toolchain step 3 runs with `/p:Nullable=enable /p:TreatWarningsAsErrors=true`.
`EfcFormController.cs` has no `#nullable enable` directive today, while `BreadcrumbBridgeRouter.cs:1`
and `BreadcrumbOutboundQueue.cs:1` do. Splitting into eight partials multiplies the surface exposed
to that gate. Budget a nullable-cleanup task and do **not** add `#nullable enable` to the new
partials unless the plan also budgets the annotation work.

### `EfcViewer.cs` — 162 lines, attribute at `:20`

**No split** (162 lines now, ~330 projected after the S1 `IEfcFormViewer` intent members are added
as 1:1 forwards to the Designer fields).

**Seams.** S1 (both approaches) a new `QuickFiler/Interfaces/IEfcViewerCommands.cs` with the single
member `void EditFiltersMenuItem_Click(object sender, EventArgs e)`. `EfcFormController` adds the
interface to its base list — its existing member at `:561-566` already matches the signature and is
already `public`. `EfcViewer.SetController` (`:50-53`) and the `_formController` field (`:48`) change
from the concrete `EfcFormController` to `IEfcViewerCommands`. **Call-site impact is zero** because
`SetController` has no callers anywhere in the compiled tree. This seam is justified independently of
DEC-1: the real `EfcFormController.EditFiltersMenuItem_Click` constructs and `Show()`s a
`TaskVisualization.ManageFilters` window (`ManageFilters.cs:17`), so invoking
`EfcViewer.EditFiltersMenuItem_Click` with the concrete type in a test is a direct AC6 violation.

S2 (Approach B only) a `protected virtual bool ProcessCmdKeyBase(ref Message, Keys)` wrapper so a
test double can substitute the `Form` base implementation, which cannot run on an instance allocated
without a constructor. Cost: one permanently uncovered production line. Benefit: the two false-branch
outcomes at `EfcViewer.cs:96` become reachable, taking branch coverage from ~50% to 100%.

**Projected coverage.** Approach A: ~100% line, 100% branch, via ~15 plain tests in
`QuickFiler.Test/Viewers/EfcViewerTests.cs` using `FormatterServices.GetUninitializedObject` (an
established repository technique with 25+ call sites, applied to `Form`-derived types at
`ProgressViewer_Tests.cs:34` and `ConfigViewer_Tests.cs:28`) plus six tests in
`QuickFiler.Test/Viewers/EfcViewer.StaTests.cs`. Approach B: ~82% line, 100% branch.

**Honest STA determination.** Exactly **one** member is genuinely irreducible: the constructor
(`:23-30`). Its body is `InitializeComponent()`, `SynchronizationContext.Current`,
`TaskScheduler.FromCurrentSynchronizationContext()`, and `InitTipsLabelsList()`. No seam can execute
a constructor without constructing the object, and
`TaskScheduler.FromCurrentSynchronizationContext()` throws unless a `SynchronizationContext` is
installed. Every other member is reachable with no seam at all or with the single S2 adapter seam.
This file does not justify a broad STA surface.

### `EfcViewer.Designer.cs` — 4,277 lines, no attribute of its own

**Not split** (generated code is exempt from the 500-line rule per `epic.md:254-255` and AC4). **Not
edited under Approach A.**

Its disposition is DEC-1 plus DEC-5. Under Approach A it reports ~99% line and ~50% branch for free,
purely because the owning form is constructed once. Precedent in the committed report:
`ItemViewerExpanded.Designer.cs` at `line-rate="0.9950980392156863"`
(`coverage-final.cobertura.xml:4112`) and `BayesianPerformanceViewer.Designer.cs` at
`0.9914285714285714` (`:5683`) — both 16-digit, therefore unmerged and trustworthy under DEC-2.

`coverage.config` **must not be edited** to exclude `.*\.Designer\.cs`: it is a repo-root shared
file (guaranteed cross-child conflict), such an exclusion would remove already-covered designer lines
repository-wide and thus *lower* coverage, and
`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy makes a production-path exclude a
Blocking finding.

## New Production Files Created by F9

Each needs a `<Compile Include>` entry in `QuickFiler/QuickFiler.csproj` and an appended ledger row
**in the same change** (`epic.md:579-582`). New files default to the `testable` bucket at >= 90% line
(`epic.md:583-585`).

| File | Purpose | Ledger bucket |
| --- | --- | --- |
| `Controllers/EfcItemControllerDependencies.cs` | S10 seam bundle + production defaults | `testable`, >= 90% |
| `Controllers/EfcDataModelSource.cs` | S3 adapter over `EfcDataModel` | `testable`, >= 90% |
| `Controllers/EfcFormLayoutMath.cs` | S8 pure layout arithmetic + `EfcItemViewerLayoutSnapshot` readonly struct | `testable`, target 100% |
| `Viewers/ItemViewerUiDispatcher.cs` | S7 `IUiDispatcher` over `IItemViewer.UiDispatcher` | `testable`, >= 90% |
| `Viewers/EfcItemControlSurface.cs` | S9 adapter over the concrete `ItemViewer` | `ratified-exempt` (rationale above) |
| `Interfaces/IEfcExpansionStyleHost.cs` | S2, one member | `interface-only / not-measured` |
| `Interfaces/IEfcItemDataSource.cs` | S3, three members | `interface-only / not-measured` |
| `Interfaces/IEfcItemControlSurface.cs` | S9, ~14 members | `interface-only / not-measured` |
| `Interfaces/IEfcFormViewer.cs` | Form-controller S1 | `interface-only / not-measured` |
| `Interfaces/IEfcViewerCommands.cs` | Viewer S1, one member | `interface-only / not-measured` |

Plus the 14 partial-split files (8 for each controller, of which two retain the original file names).
**A file in the `interface-only / not-measured` bucket is reported N/A, never 0%, and receives no
`[ExcludeFromCodeCoverage]`** (`epic.md:509-522`). Shape-assertion tests written purely to
manufacture coverage for such a file are prohibited.

**Naming inconsistency to resolve at halt gate G7.** Sibling plans have already invented three
labels for the third bucket —
`docs/features/active/2026-08-07-quickfiler-helper-classes-coverage-434/plan.2026-08-07T20-41.md:577`
uses `no-coverable-lines`, `:580` uses `interface-only`, `:583` uses `no-executable-code` — while
`epic.md:519` says `interface-only / not-measured`. F9 must use F1's literal token and flag the
divergence if F1 has not reconciled it.

## Issue #439 — Semantic-Conflict Risk

Issue #439 (`Bug: efcviewer-missing-lineage-and-segment-navigation`, High, open) reports that
suggestion and search-result rows render a single leaf segment instead of a root-to-leaf lineage with
arrow separators, and that clicking a non-leaf segment does not navigate.

**The mechanism is a path-namespace mismatch, not a rendering bug.** Verified end-to-end from source:

1. `FolderPredictor.AddSuggestions` (`FolderPredictor.cs:804-808`) emits the separator
   `"========= SUGGESTIONS ========="` followed by `Suggestions.ToArray(5)` — raw keys of
   `FolderScorer._folderNameScores` (`FolderScorer.cs:250-253`), which are **relative folder stems**.
2. `EfcFormController` passes them straight through: `PopulateFolderCombobox:1037`,
   `SearchText_TextChanged:558`, `RefreshSuggestionsAsync:800-805`, all funnelling into
   `BindFolderRows:873-883`.
3. `BindBreadcrumbRowsAsync:893` hands them to `BreadcrumbBridgeRouter.BindRowsAsync`.
4. The router calls `FetchChainAsync` → `_provider.ResolveLeafKeyAsync(text, ct)`
   (`BreadcrumbBridgeRouter.cs:341-344`).
5. `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (`OutlookFolderHierarchyProvider.cs:52-71`)
   matches **`node.FolderPath`** — the rooted Outlook path including the store name (`:64-68`). A
   relative stem never matches, so it returns `null` (`:70`).
6. `FetchChainAsync` returns `null` (`BreadcrumbBridgeRouter.cs:346-347`), no chain enters the
   `chains` dictionary (`:104-107`), and `BreadcrumbRowBuilder.BuildRows` falls back to its documented
   single-segment rendering (`BreadcrumbRowBuilder.cs:28-31`).

**The eventual fix point is `EfcFormController.cs:840-842`** — the
`new OutlookFolderHierarchyProvider(_globals.Ol.FolderTreeService)` construction, i.e. the exact
object whose path namespace does not match. **F9's refactor relocates it** into the default body of
the `BreadcrumbRouterFactory` seam.

**Instructions.** F9 must **not** fix #439 (AC10 and the epic NFR at `epic.md:196`). Characterization
tests must pin **CURRENT** behavior: relative-stem rows pass through verbatim, the router receives
them unchanged, and a row whose chain lookup yields `null` still binds. **Do not write an assertion
that a multi-segment lineage appears.** The F9 PR body must state that the #439 fix point has moved
to `BreadcrumbRouterFactory`'s default body, formerly `EfcFormController.cs:840-842`.

`EfcViewer.cs` is effectively #439-safe: the issue touches exactly one member
(`BreadcrumbWebView`, `:88-92`) and that member contains no lineage logic. Its characterization tests
(N12 and, under Approach A, A3) assert only reference identity with the Designer's `FolderListBox`.

#439 is **not** in `docs/features/active/`; it is an open issue only. Per the epic's own lesson from
#426 (`epic.md:651-653`), a promoted-but-not-yet-active issue is invisible to a folder scan, so it is
listed explicitly here.

## Shared-File Constraints

### `QuickFiler/QuickFiler.csproj` — approximately 7 new production entries beyond the split partials

Legacy non-SDK project with **no globbing**; every source file is an explicit `<Compile Include>`
(`epic.md:594-600`). Verified shapes:

- `Controllers\*.cs` — self-closing, single line, four-space indent, no child elements
  (`QuickFiler.csproj:294`, `:301`).
- `Viewers\EfcViewer.cs` — open/close pair with a `<SubType>Form</SubType>` child; the adjacent
  Designer entry carries `<DependentUpon>EfcViewer.cs</DependentUpon>` (`:386-391`).

F9's controller partials live under `Controllers\` and are not form-derived, so they take the plain
self-closing form.

**CRLF confirmed.** A content search for `Compile Include="Viewers\\EfcViewer\.cs">\r$` matches at
`:386`, which is only possible with a `\r\n` terminator. **Never use a git-bash `sed -i`** — it
strips CRLF and produces a whole-file diff guaranteed to conflict at fan-in (`epic.md:610-612`). Use
the `Edit` tool with an `old_string` copied exactly from one or two existing adjacent lines. Do not
read-modify-write the whole file, and do not run any formatter over `.csproj`.

**Ordering is append-within-cluster, NOT alphabetical.** Verified against the full listing
(`:290-461`): `:291` `EfcDataModel.cs` precedes `:292` `BreadcrumbBridgeRouter.cs`; `:311`
`QfcCollectionController.cs` precedes `:312` `EmailSorter.cs`; `:339` `KeyboardHandler.cs` sits after
`:338` `QfcItemGroup.cs`. Insert all new `Controllers\Efc*` entries as **one contiguous block
immediately after `:301`**, keeping F9's hunk strictly below F8's `EfcHomeController*` region at
`:295-300`. No property changes, no reference changes, no reordering.

### `QuickFiler.Test/QuickFiler.Test.csproj` — approximately 12 new test entries

Also uses explicit `<Compile Include>` with no globbing (`QuickFiler.Test.csproj:57-169`). Same CRLF
and minimal-hunk rules. Note `:101` already lists `Controllers\EfcFormControllerTests.cs`, so that
file is registered and must not be duplicated.

**Fan-in expectation.** Conflicts on both csproj files are expected, additive on both sides, and
resolved by keeping both sets of entries. This is handled by the child's own R1-R5 remediation loop
and is **not** a decomposition defect (`epic.md:613-617`).

## Constraints & Risks

### Mandatory test-safety constraints

- **Parallelization hazard.** `scripts/vscode/TaskMaster.cli.runsettings:4-7` sets
  `<Workers>0</Workers>` with `<Scope>ClassLevel</Scope>`, so test **classes** run in parallel. Any
  F9 test class that mutates process-global statics — including the `Production*` delegate statics
  on `EfcHomeControllerDependencies`, `QuickFiler.Properties.Settings.Default`, or
  `UiThread.Dispatcher` — MUST be marked `[DoNotParallelize]` and restore state in `[TestCleanup]`.
  In-repo precedent: `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:11`.
- **Popup prohibition — hard safety rule.** A unit test must never reach
  `EfcFormController.EditFiltersMenuItem_Click` (`:561-566`) with a real controller, because
  `filters.Show()` opens a window. The S5 `ShowManageFiltersAction` seam must be overridden in every
  test that can reach it. The same rule applies to `MessageBox.Show` at `:472-474`, `:710`, and
  `:756` via `MessageBoxShowAction`. A popup requiring human interaction is a unit-test-policy
  violation and will hang CI.
- **Never invoke these defaults.** `EfcViewerQueue.Dequeue` (constructs a real `EfcViewer`),
  `EfcDataModel.CreateAsync` (starts a real async Outlook data load), the default
  `BreadcrumbHostFactory` body (constructs `WebView2BreadcrumbHost` over a live WebView2 control),
  and `FileIO2.WriteTextFile` (writes to disk). Assert delegate identity via `.Method.Name` only.
- **Timer safety.** `EfcItemController.ToggleExpansion(ToggleState)` (`:862-905`) creates a real
  `System.Threading.Timer` with a 4,000 ms due time when the item is unread. Tests assert the field
  is non-null and dispose it; they never wait.
- **Determinism.** No `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, unseeded randomness, or real
  wall-clock waits. Suspension points are controlled with `TaskCompletionSource` only. `UiSyncContext`
  is supplied as a plain `new SynchronizationContext()`, proven compatible with
  `UiThread.SynchronizationContextAwaiter` by `UtilitiesCS.Test/Threading/UiThread_Tests.cs:25,40`.
  Under Approach A, `Thread.Join()` with no timeout is a synchronous handoff, not a wall-clock wait,
  and is the shape the existing precedent uses.
- **No temporary files, no external services, no live Outlook store, no shown forms, no message
  pump, no `DoEvents`.** All constructed viewers disposed in `finally`.
- **`LiveOutlook` category.** `Invoke-MSTestWithCoverage.ps1:76` applies
  `/TestCaseFilter:TestCategory!=LiveOutlook` to every coverage run. Any test F9 marks `LiveOutlook`
  silently contributes nothing to the measured coverage. F9 must mark none of its tests that way.
- **`/InIsolation` is mandatory** (`Invoke-MSTestWithCoverage.ps1:76`); the Moq-based assemblies are
  unreliable without it.

### Structural constraints

- No production file may exceed 500 lines, **and the limit applies to test files too**. Split with a
  `.Part2.cs` suffix if needed; precedent `QfcStreamingDequeueConfidenceGateTests.Part2.cs`.
- Must **not** modify `coverage.config`, `Invoke-MSTestWithCoverage.Helpers.ps1`, or any shared build
  property file.
- Must **not** edit any sibling-owned file: `EfcHomeControllerDependencies.cs`,
  `EfcHomeControllerDependencyFactories.cs`, `EfcDataModel.cs`, `BreadcrumbBridgeRouter.cs`,
  `BreadcrumbOutboundQueue.cs`, `WebView2BreadcrumbHost.cs`, `IItemViewer.cs`, `ItemViewer.cs`,
  `EfcViewerQueue.cs`, `EfcThemeHelper.cs`, `KeyboardHandler.cs`, or
  `UtilitiesCS/Properties/AssemblyInfo.cs`.
- **Intra-F9 sequencing.** `EfcFormController` must gain `IEfcExpansionStyleHost` and
  `IEfcViewerCommands` on its declaration before the `EfcItemController` and `EfcViewer` seam tasks,
  or intermediate commits will not build.
- **Cobertura aggregation is load-bearing for this child specifically.** Both controllers produce
  many compiler-generated `<>c`, `<>c__DisplayClassN_M`, and `<M>d__N` classes sharing one
  `filename` — `EfcFormController.cs` alone has five `async void` button handlers (`:415`, `:431`,
  `:447`, `:463`, `:523`) whose entire bodies live in state-machine classes, not in the named type. A
  harness reporting only the first `<class>` would materially understate F9's numbers.
- **Local test runs.** When running from the main checkout rather than this worktree, filter
  `\.claude\` paths out of any recursive `*.Test.dll` search so stale agent-worktree builds are not
  picked up.

## Latent Defects — already promoted, out of scope for fix

Per DEC-4, eight GitHub issues were promoted during F9 preparation via the MCP promotion lifecycle,
so AC11 is satisfied for research-discovered defects. **F9 fixes none of them.**

| Issue | Subject |
| --- | --- |
| [#459](https://github.com/drmoisan/TaskMaster/issues/459) | EFC item controller keyboard-registration defects: the `KbdActions<>` indexer setter (`KbdActions.cs:38-47`) does `Find(key)` and assigns only when the element is non-null, so `RegisterActions` (`EfcItemController.cs:691`) silently drops every unregistered key; the async expansion path (`:931-956`) does not register/remove the `'B'`/`'D'` jump keys that the sync path (`:879-903`) does; and `KbdActions<>.Add` throws `ArgumentException` on a duplicate `(sourceId, key)` pair (`KbdActions.cs:92-98`), so a sync-On → async-Off → sync-On sequence throws on a UI-thread path |
| [#460](https://github.com/drmoisan/TaskMaster/issues/460) | `EfcItemController.Cleanup()` (`:255-278`) dereferences `Buttons` unconditionally at `:257` although `_buttons` is only assigned in `ResolveControlGroups` (`:341`), and sets `_timer = null` at `:277` **without disposing it**, leaking an armed `System.Threading.Timer`; `_itemViewer = null` is also written twice (`:264`, `:276`) |
| [#461](https://github.com/drmoisan/TaskMaster/issues/461) | `ConversationResolverPropertyChanged` (`EfcItemController.cs:741-755`) is dead in production: its guard is `nameof(_dataModel.ConversationResolver.ConversationInfo.Expanded)`, which the compiler resolves to the literal `"Expanded"`, and `ConversationResolver` only ever raises `"ConversationInfo"`, `"ConversationItems"`, `"Df"`, and `"UpdateUI"` |
| [#463](https://github.com/drmoisan/TaskMaster/issues/463) | The WebView2 incognito argument uses **U+2013 EN DASH** rather than two ASCII hyphens, so incognito mode is never applied. Three call sites: `EfcItemController.cs:184`, `:217`, and `QfcItemController.ViewerSetup.cs:52` |
| [#464](https://github.com/drmoisan/TaskMaster/issues/464) | EFC controllers missing null guards plus `async void` rethrow boundary defects: `DarkMode`'s getter evaluates `_globals.Ol` eagerly as a `params object[]` element so it NREs rather than returning the default (`EfcItemController.cs:441-448`, `EfcFormController.cs:276-283`); `ActiveTheme`/`LoadTheme` lack the guards their merged QFC twins have (`QfcFormController.cs:103-105,123,134`); and five `async void` handlers `throw;` from their catch block (`EfcFormController.cs:424-428,440-444,456-460,516-520,529-533`) |
| [#465](https://github.com/drmoisan/TaskMaster/issues/465) | EFC form controller lifecycle and selection defects: `Cleanup()` (`:189-196`) is not idempotent and has no re-entrancy guard, unlike `EfcHomeController.TryBeginExecuteMoves`; `RefreshSuggestionsAsync` reads `_formViewer.SearchText.Text` from inside a `Task.Run` lambda (`:800-803`) — an illegal cross-thread control access; `ActionDeleteAsync` (`:742-750`) accumulates duplicate `"Trash to Delete"` rows because `BindFolderRows:881` stores the result back into `_folderRows`; and banner-prefix detection is inconsistent across three sites (`IsValidSelection:1049` tests three `=`, `ActionOkAsync:708` and `BreadcrumbRowBuilder.cs:19` test four) |
| [#466](https://github.com/drmoisan/TaskMaster/issues/466) | EFC dead code and latent NRE traps: `EfcViewer.SetController` (`:50-53`) has zero callers so `_formController` is permanently null and `EfcViewer.EditFiltersMenuItem_Click` (`:157-160`) would NRE if the Designer were ever regenerated with the conventionally-named handler wired; zero-call-site members `EfcItemController.InitializeWebView` (`:174-205`), `RegisterActions` (`:680-692`), the 7-argument constructor overload (`:44-57`), and `Parent` (`:566-570`); the never-assigned `_selectorsCtrls` field (`:381`) passed to `SetupThemes`; and the orphaned `EfcViewer3.*` pair absent from the csproj |
| [#467](https://github.com/drmoisan/TaskMaster/issues/467) | `EfcViewer.ProcessCmdKey` (`:94-105`) tests only `keyData.HasFlag(Keys.Alt)` and unconditionally `return true`, swallowing every Alt-modified key once a keyboard handler is attached and making the Alt mnemonics of `FilterMenuStrip` and `MoveOptionsStrip` (`EfcViewer.Designer.cs:4263`, `:4268`) unreachable |

Several of F9's planned tests are explicit **characterization** tests for these defects — they pin
today's behavior so the eventual fix has a test to invert. They must assert the current, defective
behavior, not the intended behavior.

**Observation, not a defect.** `BindBreadcrumbRowsAsync:891` calls `ToScoredArray()` with no `topN`
while the presented rows come from `Suggestions.ToArray(5)` (`FolderPredictor.cs:807`). Because
`BreadcrumbRowBuilder.BuildRows` joins scores by path equality and `FolderScore.Probability` is
max-normalized over the full ordered set regardless of `topN` (`FolderScorer.cs:275-302`), the
surplus scores are inert. Recorded so a future reader does not mistake it for a defect.

## Evidence and Measurement

- All coverage, QA-gate, baseline, and regression evidence is written to
  `<FEATURE>/evidence/<kind>/` per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
  Non-canonical paths (`artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`,
  `artifacts/evidence/`) are rejected by the `enforce-evidence-locations.ps1` PreToolUse hook.
  Sub-paths in use: `evidence/baseline/` (Phase 0 baselines), `evidence/other/` (decisions,
  dispositions, referrals to F1 and the epic orchestrator), `evidence/qa-gates/` (final toolchain
  gates and the per-file coverage table), `evidence/regression-testing/`,
  `evidence/issue-updates/issue-452.<timestamp>.md`.
- Timestamps use `yyyy-MM-ddTHH-mm`.
- Every machine-checkable artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, and — for anything
  under `evidence/baseline/` — `Output Summary:`. Negative claims additionally carry `SearchScope:`,
  `SearchPatterns:`, `SearchResult:`; this applies to the Phase 0 record that F1's outputs were
  absent at preparation time.
- The per-file coverage artifact additionally carries, per file: `LINE_COVERED / LINE_VALID`, the
  computed line rate, `BRANCH_COVERED / BRANCH_VALID`, the computed branch rate, and the ledger
  bucket, with `N/A` for a `0/0` file. Plus `DERIVATION:` naming the direct-child axis and stating
  explicitly that `@line-rate` was not read; `ISSUE_441_DISCLOSURE:` stating that the root
  `coverage/@lines-valid` and every `class/@line-rate` in the committed XML are inflated and were not
  used; the merged-class branch-condition best-of limitation; and the source XML path with the
  branch and commit it was produced on.
- Repository-wide before/after figures for AC9 must be computed the same way and compared
  like-for-like. Comparing a corrected post-change figure against the uncorrected 70.19% merge-base
  figure cited at `epic.md:479-481` would be an invalid comparison.
- Toolchain order per `CLAUDE.md` §CUT3, with the DEC-3 command correction:
  `dotnet tool run csharpier format .` → analyzer msbuild → nullable msbuild → coverage-enabled
  `vstest.console.exe`, restarting from step 1 on any failure or auto-fix.

## Acceptance Criteria

Numbering matches `issue.md` AC1-AC11. No criterion has been dropped or renumbered; wording has been
refined to be individually measurable and to name its evidence artifact.

- [ ] **AC1 — Per-file line coverage floor.** Every file classified `testable` in F1's ledger within
      F9's scope — the eight `EfcItemController.*.cs` partials, the eight `EfcFormController.*.cs`
      partials, `EfcViewer.cs`, and the F9-created seam files — measures **>= 80% line coverage**
      (>= 90% for F9-created files per AC5), verified with F1's per-file harness on F9's branch, with
      every rate derived by the DEC-2 rule from the direct-child `class/lines/line` axis grouped by
      `@filename`. Evidence: the per-file coverage table under `<FEATURE>/evidence/qa-gates/`,
      carrying `LINE_COVERED / LINE_VALID` per file, the `DERIVATION:` statement, and the
      `ISSUE_441_DISCLOSURE:` statement.
- [ ] **AC2 — Per-file branch coverage floor.** Every such file also measures **>= 75% branch
      coverage**, reported as an independent gate alongside line coverage in the same artifact, with
      `BRANCH_COVERED / BRANCH_VALID` per file. `EfcViewer.Designer.cs` is excluded from this gate
      per DEC-5 (its ~0.50 branch rate is a construction artifact of `Dispose(bool)`, not a test
      gap), subject to F1's ledger clarification.
- [ ] **AC3 — Exemption attributes removed.** `[ExcludeFromCodeCoverage]` is absent from
      `EfcItemController.cs`, `EfcFormController.cs`, and `EfcViewer.cs` in the final diff, and each
      reaches the AC1/AC2 floors via seam extraction. No `EfcItemController.*.cs` or
      `EfcFormController.*.cs` partial carries the attribute. The single new production file
      proposed for `ratified-exempt` — `Viewers/EfcItemControlSurface.cs` — carries an F1-ratified
      rationale meeting the irreducible-remainder standard. DEC-1 is ratified by the maintainer and
      the ratified branch (A or B) is recorded in `<FEATURE>/evidence/other/` before Phase 1 begins.
      Evidence: the final diff plus the Phase 0 ratification record.
- [ ] **AC4 — File-size compliance.** No production file in F9's scope exceeds **500 lines** after
      refactor, and no F9-authored test file exceeds 500 lines. `EfcViewer.Designer.cs` is exempt as
      generated code (`epic.md:254-255`). Evidence: a line-count table for every F9-touched and
      F9-created file under `<FEATURE>/evidence/qa-gates/`.
- [ ] **AC5 — New files registered, covered, and ledgered.** Every production file newly created by
      F9 (partial splits, seam types, interfaces) reaches **>= 90% line coverage** per the
      `CLAUDE.md` §UT2 new-module rule, has a `<Compile Include>` entry in
      `QuickFiler/QuickFiler.csproj`, and has an appended row in
      `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` **added in the same
      change** — except files in the `interface-only / not-measured` bucket, which are reported
      `N/A`, never `0%`, receive no `[ExcludeFromCodeCoverage]`, and are not subject to a percentage
      floor. Both csproj edits preserve CRLF and touch no property, reference, or ordering.
      Evidence: the diff plus the per-file coverage table.
- [ ] **AC6 — Test conventions and determinism.** All tests use **MSTest**, **Moq**, and
      **FluentAssertions**, follow Arrange-Act-Assert, and are deterministic and isolated: no
      temporary files, no external services, no live or shown forms, no popups, no message pumps, no
      `DoEvents`, no `Thread.Sleep`/`Task.Delay`, no unseeded randomness, no direct wall-clock reads.
      `async void` handlers are observed with a `TaskCompletionSource`. Every test class that mutates
      a process-global static is `[DoNotParallelize]` with restoring `[TestCleanup]`. No test is
      marked `LiveOutlook`. Evidence: the policy-audit artifact plus a green coverage-enabled test
      run.
- [ ] **AC7 — STA confinement.** Any test relying on the epic's STA last-resort clause is confined to
      `QuickFiler.Test/Viewers/EfcViewer.StaTests.cs`, is marked `[STATestClass]`, constructs at most
      one never-shown `EfcViewer` on a dedicated STA thread disposed in a `finally`, and carries a
      per-test XML doc comment stating why no seam could isolate the logic. **No `*.StaTests.cs` file
      exists for `EfcItemController` or `EfcFormController`** — every member of both is reachable
      through a seam or a handle-less control. Under a ratified Approach B, zero `*.StaTests.cs`
      files exist at all. Evidence: the test-file inventory plus the DEC-1 ratification record.
- [ ] **AC8 — Toolchain green in final form.** The full C# toolchain passes in order in a single
      final pass: `dotnet tool run csharpier format .` (DEC-3) → analyzer msbuild → nullable msbuild
      with `/p:TreatWarningsAsErrors=true` → `vstest.console.exe` with coverage. Evidence: four
      artifacts under `<FEATURE>/evidence/qa-gates/`, each with `Timestamp:`, `Command:`, and
      `EXIT_CODE: 0`.
- [ ] **AC9 — Repository-wide coverage retained or improved.** Repository-wide line coverage is
      retained or improved against the baseline measured on this branch, with **both** figures
      derived by the DEC-2 rule so the comparison is like-for-like. The artifact states the net line
      delta contributed by `EfcViewer.Designer.cs` entering the denominator, and — if the delta is
      negative — the specific mitigation applied. Evidence: a before/after repository-wide comparison
      under `<FEATURE>/evidence/qa-gates/`, with the pre-change baseline captured in Phase 0 under
      `<FEATURE>/evidence/baseline/`.
- [ ] **AC10 — No behavior change.** No observable QuickFiler flow changes. Characterization tests
      pin **current** behavior on the #439 path — `PopulateFolderCombobox`, `SearchText_TextChanged`,
      `RefreshSuggestionsAsync`, `ActionDeleteAsync`, `BindFolderRows`, `BindBreadcrumbRowsAsync`,
      `ConfigureBreadcrumbControl`, `SelectedFolder`, `IsValidSelection` — and assert that
      relative-stem rows pass through verbatim and that a row whose chain lookup yields `null` still
      binds. **No test asserts that a multi-segment lineage appears.** Open defect #439 is not fixed.
      Public constructor signatures consumed by F8 are unchanged; every new test entry point is an
      explicit overload, never an optional parameter. No sibling-owned file is edited. Evidence: the
      final diff plus the named characterization tests.
- [ ] **AC11 — Latent defects promoted, not left as prose.** Every latent defect discovered during
      research or execution is tracked as a GitHub issue via the MCP promotion lifecycle. The eight
      research-discovered defects are already promoted as #459, #460, #461, #463, #464, #465, #466,
      and #467 (DEC-4); **F9 fixes none of them**. Any defect newly discovered during execution is
      promoted before F9 completes, with its issue number recorded here. Evidence: the issue-number
      list in this section plus the execution-phase promotion record under
      `<FEATURE>/evidence/other/`.

## Definition of Done

- [ ] All acceptance criteria above individually verified and checked off in both `spec.md` and
      `user-story.md`
- [ ] DEC-1 ratified by the maintainer and the headless-construction spike resolved, both recorded
      under `<FEATURE>/evidence/other/`, before any Phase 1 task runs
- [ ] Phase 0 halt gates G1-G8 on F1's ledger and harness all cleared, or the child reported
      `BLOCKED`
- [ ] Both controllers split into partials, every partial under 500 lines and independently above the
      80%/75% floors
- [ ] Three `[ExcludeFromCodeCoverage]` attributes removed; no new attribute added to any file other
      than the single F1-ratified `EfcItemControlSurface.cs`
- [ ] The existing `PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel` test
      and its `CreateMinimalController()` helper migrated verbatim
- [ ] Ledger rows appended for every created production file in the same change as the csproj entry
- [ ] F1-harness per-file coverage evidence committed under `<FEATURE>/evidence/qa-gates/`, with the
      DEC-2 derivation and issue-#441 disclosure statements present
- [ ] Correction notes delivered to the epic orchestrator: the `epic.md:161` FilerQueue baseline is
      wrong (C-DEC-2), and `epic.md:223`/`:324` still say 33 attributes after the note at `:121-130`
      corrected the figure to 21 (C6)
- [ ] Ledger-semantics clarification (DEC-5) requested from F1 and its answer recorded
- [ ] Full toolchain pass completed in order in final form (format → analyze → type-check → test)

## Non-Goals

- No behavior change to end-user QuickFiler flows.
- No fix for issue #439, and no test asserting the lineage behavior #439 requests.
- No fix for any of #459, #460, #461, #463, #464, #465, #466, or #467.
- No edit to any sibling-owned file: F8's dependency and factory files, F5's `EfcDataModel.cs`, F12's
  `BreadcrumbBridgeRouter.cs`, F13's WebView2 host files, F14's `IItemViewer.cs` / `ItemViewer.cs`,
  F4's `EfcViewerQueue.cs` / `EfcThemeHelper.cs`, or F3's `KeyboardHandler.cs`.
- No edit to `coverage.config`, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, any shared
  build property file, `UtilitiesCS/Properties/AssemblyInfo.cs`, or `epic.md`.
- No widening of the `UtilitiesCS` `InternalsVisibleTo` grant.
- No change to the repository-wide coverage thresholds themselves.
- No work on `QuickFiler/Viewers/EfcViewer3.cs` or `EfcViewer3.Designer.cs` — not in the csproj
  compile set, therefore outside the denominator and outside the epic.
- No conversion of QuickFiler away from VSTO/WinForms. Where a seam choice is open, F9 prefers
  host-neutral extraction that a future WebView2/Office.js port can reuse — which is why
  `IEfcViewerCommands` carries no WinForms type in its signature beyond `EventArgs`.

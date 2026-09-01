# qfc-twin-processcmdkey-alt-chord-over-claim (Spec)

- **Issue:** #663
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T20-16
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** `full-bug` — this file is the sole authoritative acceptance-criteria source. No user-story.md is produced for this feature.

## Path-notation convention (read before editing this document)

The blast-radius derivation reads this document. `Get-BlastRadius` in
`.claude/lib/blast-radius/BlastRadius.psm1` takes the spec text alongside the plan text and calls
`Get-PathFromLine` over every spec line, harvesting backtick-delimited inline-code tokens and
classifying those that look like repository paths into the radius `paths` set. A backticked path in
this document therefore enters the computed blast radius whether or not the fix touches it, and the
radius is what a parallel run uses to decide cohort conflicts. Therefore:

- Every file this fix will modify appears at least once as an inline code span with its full
  repository-relative path.
- Every file cited only for comparison, precedent, or evidence appears in **bare prose, deliberately
  without backticks**. Do not add backticks to those citations; doing so would widen the apparent
  blast radius and contradict AC-14.

## Context

`QfcFormViewer.ProcessCmdKey` claims the entire class of Alt-bearing key chords. The guard at
`QuickFiler/Viewers/QfcFormViewer.cs:58-61` is `_keyboardHandler is not null && QfcFormKeyHandler.IsAltKeyCommand(keyData)`,
and `QuickFiler/Controllers/QfcFormKeyHandler.cs:18` defines that predicate as `keyData.HasFlag(Keys.Alt)`.
Any chord carrying the Alt modifier therefore returns `true` from the override, which suppresses
message dispatch and prevents WinForms mnemonic resolution from running.

- **Observed environment:** Windows 11 Pro 10.0.26200; .NET Framework 4.8 VSTO Outlook add-in; the
  QuickFiler form surface hosted by `QfcFormViewer`.
- **Customer impact and severity:** Moderate and user-facing. The `&Move Options` menu on each hosted
  item viewer cannot be opened with Alt+M, and the standard window-close chord Alt+F4 is consumed by
  the form. The menu remains reachable by mouse and by the bare `M` action while keyboard mode is
  active, so the defect degrades rather than blocks the workflow.
- **First observed:** Recorded in the potential-feature backlog on 2026-08-27. The same defect on the
  Email Filer surface was fixed as issue #467 under feature #464; that feature explicitly declined to
  change the QuickFiler twin, so the twin has carried the defect since before #464 shipped.

### Correction to the issue body

The issue text states that "Alt+F and Alt+M are swallowed". Alt+M is correct. **Alt+F is not.** The
QuickFiler form Designer declares no `MenuStrip`, no `ToolStripMenuItem`, and no ampersand in any of
its six `.Text =` assignments; the counterpart of the Email Filer's "&Filters" caption is
QuickFiler/Viewers/QfcFormViewer.Designer.cs:113, where `ButtonFilters.Text` is the plain string
`"Filters"`. The "Alt+F" wording was carried over from the Email Filer twin, whose form does carry a
"&Filters" mnemonic. No acceptance criterion in this document asserts that Alt+F opens a menu on this
surface.

## Repro & Evidence

**Steps to reproduce**

1. Open the QuickFiler form surface (`QfcFormViewer`) in a live Outlook session with at least one
   loaded item row.
2. Give focus to a loaded item row and press Alt+M, intending to open the row's Move Options menu.
3. Observe that the menu does not open.
4. Press Alt+F4. Observe that the window does not close.
5. Press Alt alone. Observe that the keyboard-navigation dialog toggles as expected.

**Expected versus actual**

| Chord | Expected | Actual |
|---|---|---|
| Alt (bare) | Keyboard-navigation dialog toggles | Toggles (correct) |
| Alt+M | `&Move Options` menu opens | Nothing happens; keyboard dialog toggles instead |
| Alt+F4 | Window closes | Nothing happens; keyboard dialog toggles instead |
| Alt+arrow | Falls through to the base implementation | Keyboard dialog toggles |

**Frequency:** Deterministic. Every Alt-bearing chord takes the claimed branch on every press.

**Evidence artifacts (already produced, in this feature folder)**

- `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/research/2026-09-01T01-05-qfc-alt-chord-over-claim-research.md`
- `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/other/call-site-compile-inclusion.md`
- `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/other/qfc-mnemonic-inventory.md`

## Scope & Non-Goals

### In scope

Three files:

- `QuickFiler/Controllers/QfcFormKeyHandler.cs` — add one new `internal static` predicate.
- `QuickFiler/Viewers/QfcFormViewer.cs` — route the `ProcessCmdKey` guard through the new predicate.
- `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` — add the new predicate's tests to the
  existing file.

### Out of scope / non-goals

The paths in this subsection are written without backticks on purpose; see the path-notation
convention above.

1. **Do not narrow or rename `IsAltKeyCommand`.** It stays exactly as written at
   `QuickFiler/Controllers/QfcFormKeyHandler.cs:18`, and its four existing tests stay unmodified.
   Rationale in "Why the shared predicate is not narrowed" below.
2. **Do not modify the uncompiled viewer variants**: QuickFiler/Viewers/QfcFormViewerDark.cs,
   QuickFiler/Viewers/QfcFormViewerExpanded.cs, QuickFiler/Legacy/QfcFormLegacyViewer.cs.
3. **Do not modify TaskVisualization/TaskViewer.cs** or anything else in the TaskVisualization
   project.
4. **Do not remove the unused locals at `QuickFiler/Viewers/QfcFormViewer.cs:64-67`.** Line 64
   computes `object sender = FromHandle(msg.HWnd)`, line 65 constructs `var e = new KeyEventArgs(keyData)`,
   and line 67 sets `e.Handled = true`; neither local is read, because line 68 dispatches the
   parameterless `_keyboardHandler.ToggleKeyboardDialogAsync()`. These lines are pre-existing and
   unrelated to the claim decision. The bugfix policy in CLAUDE.md requires the minimal targeted fix
   and forbids opportunistic refactors, so they are retained deliberately. **A reviewer must not read
   their survival as an oversight.** If their removal is judged worthwhile it belongs in a separate
   issue.
5. **Do not add a new source file or a new test file**, and therefore do not edit either
   QuickFiler/QuickFiler.csproj or QuickFiler.Test/QuickFiler.Test.csproj.
6. **Do not add any `[ExcludeFromCodeCoverage]` attribute.**
7. **Do not change the drop-down mnemonics** (C, A, M, P on the Move Options drop-down). They are
   reached only after the drop-down is open, at which point the `ToolStrip` owns input routing and the
   form-level `ProcessCmdKey` is not the gate. They are not independently swallowed by this defect.

## Root Cause Analysis

### The invariant this fix restores

A `ProcessCmdKey` override is entitled to claim an Alt chord only when the action it dispatches is an
action that the chord itself selects. `QfcFormViewer` dispatches the parameterless
`ToggleKeyboardDialogAsync()` — the overload at QuickFiler/Controllers/KeyboardHandler.cs:225-236,
whose signature accepts no key data and whose body reads only the `_kbdActive` field before calling
`ToggleOffNavigationAsync()` or `ToggleOnNavigationAsync()`. The only gesture that overload can encode
is therefore a bare Alt press. Claiming any other Alt chord consumes a key the form will not act on,
and because a `ProcessCmdKey` override that returns `true` suppresses message dispatch before WinForms
mnemonic resolution runs in `ProcessDialogChar`/`ProcessMnemonic`, the mnemonic carried by the
consumed chord never fires.

### Confirmed root cause

`QuickFiler/Controllers/QfcFormKeyHandler.cs:18` tests only the Alt **modifier** bit. It does not
inspect the key-code half of the key value. `Keys.Alt` is `262144`, a modifier bit; `Keys.KeyCode` is
the mask `65535` that isolates the virtual-key code. Masking with `Keys.KeyCode` yields `Keys.Menu`
(18, documented as "The ALT key") for a real bare Alt press, and `Keys.None` (0) for the synthetic
`Keys.Alt` value used in unit tests. Any other key-code value indicates that a second key was pressed
with Alt, which is a mnemonic or system chord and not the keyboard-dialog gesture.

### The mnemonic that is actually swallowed

`QfcFormViewer` hosts two user controls whose own control trees carry a real menu bar with a
top-level mnemonic:

| Host control | Menu item caption | File:line | Chord |
|---|---|---|---|
| `ItemViewer` | `&Move Options` | QuickFiler/Viewers/ItemViewer.Designer.cs:173 | Alt+M |
| `ItemViewerExpanded` | `&Move Options` | QuickFiler/Viewers/ItemViewerExpanded.Designer.cs:161 | Alt+M |

`Control.ProcessCmdKey` bubbles up the control hierarchy, so the form-level override intercepts Alt+M
typed anywhere on the form, including inside any hosted item viewer. Additional `ItemViewer`
instances are manufactured per loaded row by QuickFiler/Helper Classes/ItemViewerQueue.cs:105, each
carrying its own `&Move Options` mnemonic.

Alt+F4 is separately affected: it is delivered to `ProcessCmdKey` as `WM_SYSKEYDOWN` before the
default window procedure can translate it into the close command, so the current over-claim consumes
it.

### Why the shared predicate is not narrowed

`IsAltKeyCommand` has exactly one compiled consumer, `QuickFiler/Viewers/QfcFormViewer.cs:60`.
QuickFiler/QuickFiler.csproj is a legacy non-SDK project with no wildcard compile glob, no
`EnableDefaultCompileItems`, and no `Microsoft.NET.Sdk` attribute; QuickFiler/Viewers/QfcFormViewerDark.cs,
QuickFiler/Viewers/QfcFormViewerExpanded.cs and the entire QuickFiler/Legacy/ folder are absent from
its `<Compile Include>` item list.

Narrowing the shared predicate in place is nonetheless rejected, because its breadth is semantically
meaningful for the **other** dispatch contract that still references it. The three uncompiled sites
call a `KeyboardHandler_KeyDown(object, KeyEventArgs)` overload, whose implementation at
QuickFiler/Controllers/KeyboardHandler.cs:114-131 dispatches on `e.KeyCode` and on
`(char)e.KeyValue`. `new KeyEventArgs(Keys.Alt | Keys.Left).KeyCode` is `Keys.Left`, and `Keys.Left`
**is** a registered action: QuickFiler/Controllers/QfcItemController.EventWiring.cs:166-170 registers
`Keys.Left` and :161-165 registers `Keys.Right` in `KeyActions`. Narrowing the shared predicate would
silently change that contract for the files that still reference it. Adding a separate, narrowly
scoped predicate leaves the other contract untouched.

### Why the predicate is placed on `QfcFormKeyHandler`

The Email Filer precedent put its copy of `ClaimsAltChord` on the viewer itself
(QuickFiler/Viewers/EfcViewer.cs:96-104). This fix deviates from that placement for two reasons.

1. **Coverage measurability.** QuickFiler/Viewers/QfcFormViewer.cs:17,
   QuickFiler/Viewers/EfcViewer.cs:20 and TaskVisualization/TaskViewer.cs:18 all carry
   `[ExcludeFromCodeCoverage]`. `QuickFiler/Controllers/QfcFormKeyHandler.cs` carries none. Only the
   `QfcFormKeyHandler` placement produces a `<method>` element in the Cobertura output, so only that
   placement can demonstrate the unit-test policy's `>= 90%` new-method requirement by measurement
   rather than by assertion.
2. **Stated purpose of the class.** The XML summary at
   `QuickFiler/Controllers/QfcFormKeyHandler.cs:5-9` describes the type as holding "Pure routing
   predicates extracted from the QuickFiler form variants' `ProcessCmdKey` overrides so the key-command
   logic can be unit tested without a live `Form` window handle." That is exactly what this predicate
   is.

Feature #464 recorded its reason for not touching `QfcFormKeyHandler.cs` as a file-ownership
constraint of that feature, not as a technical objection. The research record for this issue quotes
that reason under "Why #464 left the shared predicate alone".

### Numeric assertions used above

Two counts in this section are research-derived: that `IsAltKeyCommand` has exactly one compiled
consumer, and that exactly one top-level Alt mnemonic letter (`M`) is swallowed on this surface. Both
are supported by the dual-derivation records N1 and N2 in
`docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/research/2026-09-01T01-05-qfc-alt-chord-over-claim-research.md`
section 7, each of which states its complete family, exhaustive search scope, inclusion and exclusion
rules, two independently constructed search strategies with distinct query expressions, two
independently enumerated member sets, both counts, and an explicit member-set comparison. Neither
count appears in an acceptance criterion; the acceptance criteria are expressed as enumerations of
named chords and named test methods instead.

## Proposed Fix

### Design summary (what changes where)

Add `internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)` to the existing
type `QfcFormKeyHandler` in `QuickFiler/Controllers/QfcFormKeyHandler.cs`. Change the guard in
`QuickFiler/Viewers/QfcFormViewer.cs:56-73` from the two-part condition on lines 58-61 to a single
call to the new predicate. Add tests for the new predicate to the existing file
`QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`. Nothing else changes.

### Boundaries and invariants to preserve

- Bare Alt continues to toggle the keyboard-navigation dialog. This is existing, relied-upon
  behavior and must not regress.
- `IsAltKeyCommand` keeps its current signature, body, and semantics.
- The null-handler guard moves inside the new predicate, matching the Email Filer shape, but the
  observable behavior is unchanged: with no handler wired, nothing is claimed.
- The dispatch on `QuickFiler/Viewers/QfcFormViewer.cs:68` stays the parameterless
  `ToggleKeyboardDialogAsync()` call.
- The bare `M` keyboard action registered in `CharActionsAsync` remains a second, independent route
  to the same Move Options drop-down while keyboard mode is active. Restoring the Alt+M mnemonic adds
  a route; it does not collide with the existing one.

### Dependencies or blocked work

None. No other in-flight feature touches these three files.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

- `QuickFiler/Controllers/QfcFormKeyHandler.cs`
- `QuickFiler/Viewers/QfcFormViewer.cs`
- `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`

#### Functions/classes impacted

- `QfcFormKeyHandler.ClaimsAltChord` — new.
- `QfcFormKeyHandler.IsAltKeyCommand` — unchanged.
- `QfcFormViewer.ProcessCmdKey` — guard replaced with a single predicate call.
- `QfcFormKeyHandlerTests` — new `[TestMethod]`s added; the four existing ones unchanged.

#### Data flow and validation changes

The key-value flow gains one masking step. `ProcessCmdKey` receives `keyData`; the predicate first
rejects a null handler and a key value without the `Keys.Alt` flag, then computes
`keyData & Keys.KeyCode` and accepts only `Keys.Menu` or `Keys.None`. Every other value falls through
to `base.ProcessCmdKey`.

#### Error handling and logging updates

None. The predicate is total over its input domain, returns `bool`, throws nothing, and logs nothing.
The existing `log4net` logger on `QfcFormViewer` is not used by this path and is not changed.

#### Rollback / feature-flag considerations

Not applicable. The change is a two-line guard replacement plus one new method; rollback is a revert
of the commit.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs

`ClaimsAltChord(handler, keyData)` returns `true` if and only if all three hold:

1. `handler is not null`;
2. `keyData.HasFlag(Keys.Alt)`;
3. `(keyData & Keys.KeyCode)` is `Keys.Menu` or `Keys.None`.

Otherwise it returns `false`.

Behavior table:

| Input `keyData` | `keyData & Keys.KeyCode` | Result (non-null handler) |
|---|---|---|
| `Keys.Alt` | `Keys.None` | `true` |
| `Keys.Menu \| Keys.Alt` | `Keys.Menu` | `true` |
| `Keys.Alt \| Keys.M` | `Keys.M` | `false` |
| `Keys.Alt \| Keys.F4` | `Keys.F4` | `false` |
| `Keys.Alt \| Keys.Left` | `Keys.Left` | `false` |
| `Keys.M` | `Keys.M` | `false` |
| `Keys.Control` | `Keys.None` | `false` (no Alt flag) |
| any value, `handler` null | any | `false` |

#### Required configuration keys and defaults

None.

#### Backward-compatibility expectations

`QfcFormKeyHandler` is `internal`; the new member is `internal static`. No public API changes. The
`QuickFiler.Test` assembly reaches the internal member through the existing
`[assembly: InternalsVisibleTo("QuickFiler.Test")]` at QuickFiler/Properties/AssemblyInfo.cs:5, so no
project-file or attribute change is needed.

#### Performance constraints

None beyond the existing per-keystroke path. The predicate adds one bitwise mask and two integer
comparisons.

## Call-Site Disposition

Every site in the solution that claims an Alt chord in a `ProcessCmdKey` override is listed. No site
is left silently inconsistent. Paths in the "no" rows are deliberately unbackticked because those
files are not modified.

| # | Site | Compiled | Disposition after the fix |
|---|---|---|---|
| 1 | `QuickFiler/Viewers/QfcFormViewer.cs:56-73` | yes | **CHANGED.** Routed through `ClaimsAltChord`. A bare Alt press still toggles the keyboard dialog; every other Alt chord reaches `base.ProcessCmdKey`, restoring Alt+M for `&Move Options` and restoring Alt+F4. |
| 2 | QuickFiler/Viewers/QfcFormViewerDark.cs:41-53 | no | **UNCHANGED.** Not a build input (absent from the QuickFiler/QuickFiler.csproj `<Compile Include>` list), so it has no runtime behavior to correct. Its dispatch contract also differs from site 1: it calls `KeyboardHandler_KeyDown(object, KeyEventArgs)`, which dispatches on `e.KeyCode`, and `Keys.Left` is a registered `KeyActions` entry. |
| 3 | QuickFiler/Viewers/QfcFormViewerExpanded.cs:41-53 | no | **UNCHANGED.** Same two reasons as site 2. |
| 4 | QuickFiler/Legacy/QfcFormLegacyViewer.cs:21-33 | no | **UNCHANGED.** The whole QuickFiler/Legacy/ folder is absent from the csproj, so there is no runtime behavior to correct. It inlines `HasFlag(Keys.Alt)` and dispatches to QuickFiler.Legacy.QuickFileController.KeyboardHandler_KeyDown, the same `(object, KeyEventArgs)` contract as sites 2 and 3. |
| 5 | TaskVisualization/TaskViewer.cs:253-265 | yes | **UNCHANGED and out of scope.** Different project and a different, already-tested accelerator model: TaskVisualization/TaskController.Accelerator.cs:75 branches on `e.Alt` to toggle an accelerator overlay, pinned by TaskVisualization.Test/TaskControllerAcceleratorKeyboard.StaTests.cs:76-144. Narrowing it would change tested behavior. Independently, TaskVisualization/TaskViewer.Designer.cs declares no menu strip and no menu item, so the user-facing symptom in issue #663 cannot arise on that surface. |

## Assumptions, Constraints, Dependencies

- **Assumption.** `ProcessCmdKey` is reached for a bare Alt press. Supported by the `VK_MENU` /
  `Keys.Menu` correspondence and corroborated in-repo: the keyboard-navigation dialog is opened today
  by pressing Alt alone, which is only possible if the existing override runs for that gesture.
- **Assumption.** `ProcessCmdKey` is not reached on key-up; `Control.PreProcessMessage`'s documented
  message set excludes `WM_SYSKEYUP`. No release-time behavior changes.
- **Constraint.** Target framework is `v4.8.1`. Test libraries are MSTest 4.3.3, Moq 4.20.72,
  FluentAssertions 8.10.0.
- **Constraint.** QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:17
  (`ExecutingAssembly_ContainsNoFormDerivedType`) fails if any `Form`-derived type is compiled into
  the test assembly. No test may construct, show, or derive from a `System.Windows.Forms.Form`.
- **Constraint.** Both csproj files are legacy non-SDK projects with explicit per-file
  `<Compile Include>` items. Because no file is added, neither is edited.
- **Dependency (environment).** This worktree contains no `.dotnet-sdk` directory and no `packages/`
  directory. scripts/vscode/Install-RepoDotNetSdk.ps1 and a NuGet restore must be run before any
  `dotnet` or `msbuild` command.

## Data / API / Config Impact

- **User-facing changes.** Alt+M opens the focused row's Move Options menu. Alt+F4 closes the window.
  Alt+arrow chords fall through instead of toggling the keyboard dialog. Bare Alt is unchanged.
- **Data or migration considerations.** None.
- **Logging/telemetry updates.** None.
- **Compatibility notes.** No CLI flags, config schemas, or serialized formats are affected.

## Test Strategy

### Regression tests to add

All new tests go into the existing file `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`,
which is already compiled (QuickFiler.Test.csproj:151). No new test file is created and no csproj is
edited. Tests exercise the `internal static` predicate directly, with a `Mock<IQfcKeyboardHandler>`
supplying the handler argument, following the shape of the delivered Email Filer tests at
QuickFiler.Test/Controllers/EfcViewerTests.cs:112-162. MSTest `[TestClass]`/`[TestMethod]`,
Arrange-Act-Assert, FluentAssertions with a because-string on every assertion.

| Test method | Input | Expected |
|---|---|---|
| `ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue` | `Keys.Alt` | `true` |
| `ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue` | `Keys.Menu \| Keys.Alt` | `true` |
| `ClaimsAltChord_WithAltM_ReturnsFalse` | `Keys.Alt \| Keys.M` | `false` |
| `ClaimsAltChord_WithAltF4_ReturnsFalse` | `Keys.Alt \| Keys.F4` | `false` |
| `ClaimsAltChord_WithAltLeft_ReturnsFalse` | `Keys.Alt \| Keys.Left` | `false` |
| `ClaimsAltChord_WithoutAltFlag_ReturnsFalse` | `Keys.M` | `false` |
| `ClaimsAltChord_WithNullHandler_ReturnsFalse` | `null`, `Keys.Alt` | `false` |

### Test method names are not unique within the test assembly

Two of the seven names above already exist in `QuickFiler.Test`, declared on the Email Filer fixture:
`ClaimsAltChord_WithAltM_ReturnsFalse` at QuickFiler.Test/Controllers/EfcViewerTests.cs:134 and
`ClaimsAltChord_WithNullHandler_ReturnsFalse` at QuickFiler.Test/Controllers/EfcViewerTests.cs:156.
Both compile into the same assembly as the new tests. That is legal in C#, because the two fixtures
are different types, but it means a bare method name is not a unique identifier in a test run's
output.

Every acceptance criterion below that reads a named test's outcome therefore identifies it by the
pair of method name and declaring type `QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests`. An
assertion phrased on the bare name alone could be satisfied, or falsified, by the Email Filer test of
the same name.

### Test-shape requirement inherited from the Email Filer suite

QuickFiler.Test/Controllers/EfcViewerTests.cs:112-162 pins the positive case only with the synthetic
value `Keys.Alt`, whose key-code portion masks to `Keys.None`. It never pins `Keys.Menu | Keys.Alt`,
which is the shape a physical keyboard produces (`Keys.Menu` = 18 is documented as "The ALT key").
The `Keys.Menu` arm of the predicate is therefore untested on that surface and would survive deletion
undetected. **The QuickFiler tests must pin both shapes.**

### Justification wording requirement

The because-string for `ClaimsAltChord_WithAltM_ReturnsFalse` must name **Move Options**. Copying the
Email Filer wording, which names Filters, would state a justification that is false for this surface:
`ButtonFilters.Text` is the plain string `"Filters"` with no ampersand
(QuickFiler/Viewers/QfcFormViewer.Designer.cs:113). Alt+F may be covered as a generic non-claimed
chord only if its because-string does not assert a menu.

### Edge cases and negative scenarios

Non-Alt chord (`Keys.M`), modifier-only non-Alt chord (`Keys.Control`), null handler, the real
mnemonic (`Keys.Alt | Keys.M`), the system chord (`Keys.Alt | Keys.F4`), and the previously-claimed
vestigial arrow chord (`Keys.Alt | Keys.Left`).

### Error handling and logging verification

Not applicable; the predicate throws nothing and logs nothing.

### Coverage impact and targets

`QuickFiler/Controllers/QfcFormKeyHandler.cs` carries no `[ExcludeFromCodeCoverage]` attribute and
QuickFiler.dll is instrumented (coverage.config excludes only third-party module paths), so
`ClaimsAltChord` produces a `<method>` element in the Cobertura output. Target: 100% line coverage on
the new method, against the `>= 90%` new-method floor. No regression on changed lines. Coverage
artifacts go to
`docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/`.
That directory is used rather than an evidence/coverage/ sibling because the canonical evidence
sub-path set enumerated by the evidence-and-timestamp-conventions skill is baseline,
regression-testing, qa-gates, issue-updates, other and remediation-baseline; `coverage` is not a
member of it.

### Toolchain commands to run (format, lint, type-check, test)

Run in this order and restart from step 1 if any step fails or modifies files.

**Prerequisite (this worktree only).** There is no `.dotnet-sdk` directory and no `packages/`
directory here. Run scripts/vscode/Install-RepoDotNetSdk.ps1 and a NuGet restore before any
`dotnet` or `msbuild` command, and run `dotnet tool restore` once before the first CSharpier
invocation.

1. **Format**

   ```
   dotnet tool run csharpier format .
   ```

   Verify read-only with:

   ```
   dotnet tool run csharpier check .
   ```

   Always invoke through `dotnet tool run` so the manifest-pinned version is used; a globally
   installed CSharpier produces diffs that disagree with CI.

2. **Analyzers**

   ```
   msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
   ```

   `/t:Rebuild` is required. MSBuild's incremental up-to-date check does not invalidate on a
   command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
   project and runs no analyzers.

3. **Nullable / type-check**

   ```
   msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
   ```

   **`/p:Nullable=enable` must not be added.** No project in this repository carries a `<Nullable>`
   element and there is no `Directory.Build.props`, so the property is a solution-wide opt-in that
   conscripts every file that has never adopted the `#nullable enable` pragma. The command above is
   character-for-character the one in .github/workflows/ci.yml. Omitting the property loses no
   enforcement over any file that has opted in.

4. **Tests**

   Use the repository wrappers, not a bare `vstest.console.exe`:

   ```
   pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug
   ```

   ```
   pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/coverage.cobertura.xml
   ```

   Both wrappers append `/Settings:scripts/vscode/TaskMaster.cli.runsettings`, `/InIsolation` and
   `/TestCaseFilter:TestCategory!=LiveOutlook` to the vstest argument list
   (scripts/vscode/Invoke-MSTest.ps1:54, scripts/vscode/Invoke-MSTestWithCoverage.ps1:76). A bare
   `vstest.console.exe` call silently drops the LiveOutlook exclusion and will attempt to run tests
   that require a live Outlook process.

### Known tooling defect: the single-assembly search root is unusable

Tracked as issue #713. The script paths in this subsection are written without backticks on purpose,
per the path-notation convention above: this fix does not touch them, and backticking them would put
the whole `scripts/vscode` surface into the computed blast radius.

scripts/vscode/Invoke-MSTest.ps1 sets `Set-StrictMode -Version Latest` at line 77. Its discovery
pipeline at lines 107-113 ends in `Select-Object -ExpandProperty FullName`, which yields a bare
`System.String` rather than an array when exactly one assembly matches. Line 115 then evaluates
`if (-not $testAssemblies -or $testAssemblies.Count -eq 0)`. The left operand is false for a
non-empty string, so `-or` goes on to evaluate the right operand and reads `.Count` on a scalar.

Verified directly under `pwsh -NoProfile`:

```
Set-StrictMode -Version Latest
$x = "one-string"
$x.GetType().Name   # String
$x.Count            # PropertyNotFoundException: The property 'Count' cannot be found on this object.
```

Any `-SearchRoot` that matches exactly one `*.Test.dll` therefore throws before a single test runs,
and `-SearchRoot QuickFiler.Test` is precisely such a root. `-SearchRoot .` matches nine assemblies,
produces an array, and is unaffected. Line 120's `$($testAssemblies.Count)` would fail identically.
scripts/vscode/Invoke-MSTestWithCoverage.ps1 carries the same shape.

This defect is not in scope for issue #663 and must not be fixed here. It is recorded so that no plan
task and no acceptance criterion depends on the single-assembly form, and it was promoted to issue
#713 so the finding survives this feature's merge.

### Reference points to re-measure (prior observations, not current facts)

These figures come from earlier runs on this repository and are recorded so that a new measurement
has something to be compared against. They must be re-measured in this feature's own toolchain pass
and must not be quoted as the current state.

- An isolated `QuickFiler.Test` run previously reported 1099 of 1099 tests passing.
- A repository-wide coverage run previously reported a line rate of 0.7032 on the raw unfiltered
  denominator, with 15 pre-existing load-driven failures concentrated in three `QfcItemController`
  test files.

### Manual validation steps

Required, because duplicate-mnemonic resolution across the several `&Move Options` owners cannot be
determined statically. In a live Outlook session with the QuickFiler form open and at least one
loaded row: press Alt (the keyboard-navigation dialog toggles, unchanged); press Alt+M (the focused
row's Move Options menu opens); press Alt+F4 (the window closes). Record the outcome at
`docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/other/manual-validation.md`.

An automated executor generally cannot perform this check: it requires a running Outlook host with the
VSTO add-in loaded and a human at the keyboard, and the repository test policy forbids the agent from
showing a WinForms form or starting a message pump to substitute for it. Feature #464 met the same
obligation for the Email Filer twin by recording the status `MANUAL_CHECK_DEFERRED` together with the
probes that justified it and an explicit statement of what the automated tests do and do not
establish. That disposition is the expected outcome here too, and AC-15 is written to accept it. What
AC-15 does not accept is the check being marked passed on assertion, or omitted silently.

## Acceptance Criteria

### Verification command reference

Two acceptance criteria assert over regular-expression searches whose patterns contain alternation.
The patterns are given here in fenced blocks rather than inline in the table, because a bare `|`
inside a GitHub-flavoured markdown table cell terminates the cell even within a code span, and the
escaped spelling that markdown would require is not the spelling the shell needs.

`Select-String -Pattern` takes a .NET regular expression. In that dialect `\|` is an **escaped literal
pipe**, not an alternation operator, so a pattern written with `\|` matches nothing at all. Verified
directly: against a two-line fixture containing `object sender = FromHandle(msg.HWnd);` and
`var e = new KeyEventArgs(keyData);`, the pattern `FromHandle\|new KeyEventArgs` returned 0 matches
while `FromHandle|new KeyEventArgs` returned 2. An acceptance criterion asserting "returns zero
matches" over the escaped spelling would therefore pass whatever the executor wrote. Use these exact
patterns:

**VC-1**, used by AC-12. Run against `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`:

```
new Form|: Form|Thread\.Sleep|Task\.Delay|GetTempFileName|GetTempPath
```

**VC-2**, used by AC-14. Run against `QuickFiler/Viewers/QfcFormViewer.cs`:

```
FromHandle|new KeyEventArgs
```

The `\.` sequences inside VC-1 are correct and must be retained: there the backslash escapes a literal
dot, which is the intended meaning. Only the alternation pipes are left unescaped.

### Acceptance-criteria table

| ID | Criterion | Verification |
|---|---|---|
| AC-1 | `QfcFormKeyHandler.ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)` exists as an `internal static bool` member of `QuickFiler/Controllers/QfcFormKeyHandler.cs` and returns `true` if and only if `handler` is non-null, `keyData` has the `Keys.Alt` flag, and `keyData & Keys.KeyCode` equals `Keys.Menu` or `Keys.None`. | All seven new `ClaimsAltChord_*` test methods listed in the Test Strategy table pass in the Invoke-MSTest.ps1 run, covering every row of the behavior table in Technical specifications. |
| AC-2 | A bare Alt press is still claimed, pinned in both key-data shapes: `Keys.Alt` (key-code portion `Keys.None`) and `Keys.Menu \| Keys.Alt` (key-code portion `Keys.Menu`, the shape a physical keyboard produces). | `ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue` and `ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue` both pass. |
| AC-3 | `Keys.Alt \| Keys.M` is not claimed, so the `&Move Options` mnemonic reaches `base.ProcessCmdKey`. | `ClaimsAltChord_WithAltM_ReturnsFalse` on `QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests` passes, identified by declaring type because the Email Filer fixture declares a method of the same name; and its FluentAssertions because-string names Move Options; confirmed by `Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern 'Move Options'` returning at least one match and `Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern 'Filters menu'` returning zero matches. |
| AC-4 | A representative non-mnemonic Alt chord is not claimed: `Keys.Alt \| Keys.F4`, the window-close chord, and `Keys.Alt \| Keys.Left`, the previously-claimed arrow chord. | `ClaimsAltChord_WithAltF4_ReturnsFalse` and `ClaimsAltChord_WithAltLeft_ReturnsFalse` both pass. |
| AC-5 | A chord that does not carry the `Keys.Alt` flag is not claimed. | `ClaimsAltChord_WithoutAltFlag_ReturnsFalse` passes, asserting both `Keys.M` and `Keys.Control` in one body with a separate because-string for each. `Keys.Control` is asserted here rather than in its own method so that the seven-method enumeration stays intact while the eighth row of the behavior table, which the Edge cases subsection names explicitly, is still exercised. |
| AC-6 | A null handler is not claimed. | `ClaimsAltChord_WithNullHandler_ReturnsFalse` on `QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests` passes with inputs `null` and `Keys.Alt`, identified by declaring type because the Email Filer fixture declares a method of the same name. |
| AC-7 | `QfcFormViewer.ProcessCmdKey` delegates its claim decision to `ClaimsAltChord` and contains no independent Alt test. | `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'ClaimsAltChord'` returns exactly one match, inside `ProcessCmdKey`; `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'Keys\.Alt'` returns zero matches; `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'IsAltKeyCommand'` returns zero matches. Command output recorded at `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/663-predicate-structure.md`. |
| AC-8 | `QfcFormKeyHandler.IsAltKeyCommand` is unchanged, and the four existing tests `IsAltKeyCommand_WithAltKey_ReturnsTrue`, `IsAltKeyCommand_WithAltPlusOtherKey_ReturnsTrue`, `IsAltKeyCommand_WithControlKey_ReturnsFalse` and `IsAltKeyCommand_WithNone_ReturnsFalse` still pass unmodified. | All four named test methods pass in the Invoke-MSTest.ps1 run, and `git diff -U0 origin/main...HEAD -- QuickFiler/Controllers/QfcFormKeyHandler.cs QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` contains no removed line (`-` prefix) matching `IsAltKeyCommand`. |
| AC-9 | No file is added to or removed from either QuickFiler/QuickFiler.csproj or QuickFiler.Test/QuickFiler.Test.csproj. | `git diff --name-only origin/main...HEAD` does not list QuickFiler/QuickFiler.csproj or QuickFiler.Test/QuickFiler.Test.csproj. |
| AC-10 | The full C# toolchain passes in order: format, analyzers, nullable/type-check, tests. | `dotnet tool run csharpier check .` is read-only, so its exit code 0 is the gate. Both msbuild invocations exit 0, and each build log contains at least one occurrence of the literal `Task "Csc"`, which proves `CoreCompile` actually ran rather than being skipped by incrementality. Each console output carries a summary line matching `^\s*0 Error\(s\)$` and no line matching the MSBuild diagnostic form `: error [A-Z]+[0-9]+:`, and no line matching `: warning [A-Z]+[0-9]+:` names any of the three changed files. A bare search for the word `error` is not used: a successful MSBuild run prints the `/errorreport:prompt` token on every Csc command line and prints its own `0 Error(s)` summary, so that search matches on a clean run and the gate could never pass. `/p:Nullable=enable` is absent from the type-check command. Every wrapper invocation uses `-SearchRoot .`; the single-assembly form is unusable for the reason recorded under Known tooling defect below, so no acceptance depends on it. The repository-wide `Invoke-MSTestWithCoverage.ps1` run's exit code is recorded but is NOT the gate: that script throws when the inner run reports any failure, and the repository carries pre-existing load-driven failures that appear only under the concurrent instrumented run. Its gate is instead that every failing test it reports is a member of the baseline failure set captured in Phase 0 under `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/`, with no failure outside that set. Console logs recorded under `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/`. |
| AC-11 | Coverage shows no regression on changed lines, and `ClaimsAltChord` meets the `>= 90%` new-method floor. | The Cobertura report produced by Invoke-MSTestWithCoverage.ps1 at `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/coverage.cobertura.xml` contains a `<method>` element named `ClaimsAltChord` under class `QuickFiler.Controllers.QfcFormKeyHandler` with `line-rate` of at least `0.90`, and the `QuickFiler.Controllers.QfcFormKeyHandler` class line-rate is not lower than the pre-change baseline recorded under the same evidence directory. |
| AC-12 | No test constructs, shows, or derives from a `System.Windows.Forms.Form`, and the new tests use no temporary files, `Thread.Sleep`, or `Task.Delay`. | `QuickFiler.Test/NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` passes, and `Select-String` over `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` with pattern VC-1 from the Verification command reference returns zero matches. |
| AC-13 | No new `[ExcludeFromCodeCoverage]` attribute is introduced anywhere in the change. | `git diff -U0 origin/main...HEAD` contains no added line (`+` prefix) matching `ExcludeFromCodeCoverage`. |
| AC-14 | The production and test change set is exactly `QuickFiler/Controllers/QfcFormKeyHandler.cs`, `QuickFiler/Viewers/QfcFormViewer.cs` and `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`; call sites 2 through 5 in the Call-Site Disposition table are untouched, and the pre-existing unused locals at `QuickFiler/Viewers/QfcFormViewer.cs:64-67` are retained. | `git diff --name-only origin/main...HEAD -- '*.cs'` lists exactly those three paths, and `git status --porcelain` reports no untracked `.cs` path. The porcelain companion is required because a name-listing diff enumerates tracked changes only and is blind to a newly created file, so the diff alone could not detect a violation of the no-new-file rule. `Select-String` over `QuickFiler/Viewers/QfcFormViewer.cs` with pattern VC-2 from the Verification command reference still returns two matches, one per literal, both inside `ProcessCmdKey`. |
| AC-15 | The live-host manual validation of bare Alt, Alt+M and Alt+F4 is recorded at the strength of the evidence actually obtained, and is never recorded as a pass on an executor's assertion. | A record exists at `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/other/manual-validation.md` carrying, for each of the three gestures, either an observed outcome with the Outlook build named, or the status `MANUAL_CHECK_DEFERRED` with the measured probes that justify deferral (`Get-Process -Name OUTLOOK` count and `[Environment]::UserInteractive`) and a statement of what the automated tests do and do not establish. This mirrors the disposition recorded for the same class of check under feature #464 at docs/features/active/efc-controller-surface-defects-464/evidence/other/manual-validation.md. A deferral is an acceptable outcome; a silent pass is not. |

### Acceptance-criteria checklist

- [ ] AC-1 `QfcFormKeyHandler.ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)` exists as an `internal static bool` member of `QuickFiler/Controllers/QfcFormKeyHandler.cs` and returns `true` if and only if `handler` is non-null, `keyData` has the `Keys.Alt` flag, and `keyData & Keys.KeyCode` equals `Keys.Menu` or `Keys.None`.
- [ ] AC-2 A bare Alt press is still claimed, pinned in both key-data shapes: `Keys.Alt` (key-code portion `Keys.None`) and `Keys.Menu | Keys.Alt` (key-code portion `Keys.Menu`, the shape a physical keyboard produces).
- [ ] AC-3 `Keys.Alt | Keys.M` is not claimed, so the `&Move Options` mnemonic reaches `base.ProcessCmdKey`.
- [ ] AC-4 A representative non-mnemonic Alt chord is not claimed: `Keys.Alt | Keys.F4`, the window-close chord, and `Keys.Alt | Keys.Left`, the previously-claimed arrow chord.
- [ ] AC-5 A chord that does not carry the `Keys.Alt` flag is not claimed.
- [ ] AC-6 A null handler is not claimed.
- [ ] AC-7 `QfcFormViewer.ProcessCmdKey` delegates its claim decision to `ClaimsAltChord` and contains no independent Alt test.
- [ ] AC-8 `QfcFormKeyHandler.IsAltKeyCommand` is unchanged, and the four existing tests `IsAltKeyCommand_WithAltKey_ReturnsTrue`, `IsAltKeyCommand_WithAltPlusOtherKey_ReturnsTrue`, `IsAltKeyCommand_WithControlKey_ReturnsFalse` and `IsAltKeyCommand_WithNone_ReturnsFalse` still pass unmodified.
- [ ] AC-9 No file is added to or removed from either QuickFiler/QuickFiler.csproj or QuickFiler.Test/QuickFiler.Test.csproj.
- [ ] AC-10 The full C# toolchain passes in order: format, analyzers, nullable/type-check, tests.
- [ ] AC-11 Coverage shows no regression on changed lines, and `ClaimsAltChord` meets the `>= 90%` new-method floor.
- [ ] AC-12 No test constructs, shows, or derives from a `System.Windows.Forms.Form`, and the new tests use no temporary files, `Thread.Sleep`, or `Task.Delay`.
- [ ] AC-13 No new `[ExcludeFromCodeCoverage]` attribute is introduced anywhere in the change.
- [ ] AC-14 The production and test change set is exactly `QuickFiler/Controllers/QfcFormKeyHandler.cs`, `QuickFiler/Viewers/QfcFormViewer.cs` and `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`; call sites 2 through 5 in the Call-Site Disposition table are untouched, and the pre-existing unused locals at `QuickFiler/Viewers/QfcFormViewer.cs:64-67` are retained.
- [ ] AC-15 The live-host manual validation of bare Alt, Alt+M and Alt+F4 is recorded at the strength of the evidence actually obtained, and is never recorded as a pass on an executor's assertion.

## Risks & Mitigations

| Risk | Assessment | Mitigation |
|---|---|---|
| Duplicate-mnemonic ambiguity: with N loaded rows there are N+2 controls owning the `&Move Options` mnemonic, and WinForms cycles focus among controls that share a mnemonic. The first Alt+M press may not open the intended row's menu. | Cannot be resolved statically; visibility and enabled state of the Designer-held templates at runtime are not determinable from source. WinForms offers a mnemonic only to a control whose entire parent chain is visible and enabled, which is expected to exclude the hidden templates. | AC-15 requires live-host validation of Alt+M against the focused row. If the wrong menu opens, that is a distinct defect in mnemonic ownership and belongs in a follow-up issue, not in this fix. |
| Removing the last compiled consumer of `IsAltKeyCommand` could trip an unused-member analyzer under `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. | Real but low. The member is `internal` in an assembly with `InternalsVisibleTo`, and it retains four test consumers in `QuickFiler.Test`. | AC-10 is the gate. If an analyzer diagnostic appears, fix the root diagnostic rather than suppressing it; do not delete `IsAltKeyCommand`, which would violate AC-8. |
| Regressing the bare-Alt keyboard-dialog toggle, which is existing relied-upon behavior. | Contained. | AC-2 pins both key-data shapes of the bare Alt press, and AC-15 confirms the behavior against a live host. |
| A later editor "fixes" the deliberately unbackticked out-of-scope paths and corrupts the harvested change footprint. | Documentation-only, but it would misstate the blast radius. | The path-notation convention section states the rule at the top of this document; AC-14 pins the actual change set independently. |

## Rollout & Follow-up

- **Release/rollout steps.** Standard branch merge. No feature flag, no migration, no configuration
  change.
- **Post-fix monitoring or clean-up.** None automated. The manual validation record under
  `docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/other/manual-validation.md`
  is the confirmation of user-facing behavior.
- **Follow-up candidates (open as separate issues; explicitly not part of this fix).**
  1. Removal of the unused locals at `QuickFiler/Viewers/QfcFormViewer.cs:64-67`.
  2. TaskVisualization/TaskViewer.cs:260 discards the `bool` returned by
     TaskController.KeyboardHandler_KeyDown, whereas TaskVisualization/TaskViewer.cs:395 consumes it.
     Whether that inconsistency is a live defect is unresolved and belongs to the TaskVisualization
     project.
  3. Adding the missing `Keys.Menu | Keys.Alt` positive case to the Email Filer suite at
     QuickFiler.Test/Controllers/EfcViewerTests.cs, which currently pins only the synthetic
     `Keys.Alt` shape.
  4. Already opened as issue #713: the single-assembly search root throws under StrictMode in the
     MSTest wrapper scripts. See Known tooling defect above.
- **Links.** Issue #663. Precedent: issue #467 under feature #464. Research and evidence artifacts are
  listed under Repro & Evidence.

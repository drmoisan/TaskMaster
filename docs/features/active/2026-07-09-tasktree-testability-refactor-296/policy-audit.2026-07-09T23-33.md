# Policy Compliance Audit — #296 tasktree-testability-refactor (remediation re-audit)

- Timestamp: 2026-07-09T23-33
- Branch under review: feature/tasktree-testability-refactor-296
- Head commit: c19f77ec740e712a9f52672c5115e5864eeb928f
- Base branch: epic/winforms-testability-refactor-integration
- Merge-base: 3f04d50f6544f084323e5d7a9a563facb9d579df
- Work Mode: full-feature (spec.md notes user-story.md is intentionally not applicable; AC sources are issue.md + spec.md)
- Review type: focused remediation re-audit (pass 1) confirming the three prior Blocking findings (E4/E5/E6) are resolved and no new issue was introduced.

## Executive Summary

Overall verdict: PASS. All three prior Blocking findings are resolved with evidence. The remaining
`[ExcludeFromCodeCoverage]` set in the `TaskTree` project is exactly the four ratified host-bound
sites (E1 `TaskTreeForm` type, E2 `TreeListViewVisual` type, E3 `FormatRow` residual wrapper,
E6 `HandleModelDropped` residual wrapper); no attribute remains on a testable seam. Full C#
toolchain evidence is green in a single clean pass. TaskTree.dll line coverage 96.34% and branch
coverage 91.49% clear the repo floors, and both changed production files clear the >= 90% new/changed
floor. No new Blocking or Non-Blocking finding was introduced.

## Scope and Baseline

Scope is the full branch diff of feature/tasktree-testability-refactor-296 against merge-base
3f04d50f. Changed production/test C# files:

- TaskTree/ITaskTreeForm.cs (new, 79)
- TaskTree/TaskTreeController.cs (230)
- TaskTree/TaskTreeController.MoveLogic.cs (new, 315)
- TaskTree/TaskTreeForm.cs (194)
- TaskTree/TreeListViewVisual.cs (new, 45)
- TaskTree/properties/AssemblyInfo.cs (40)
- TaskTree/TaskTree.csproj, TaskMaster.sln (wiring)
- TaskTree.Test/* (new test project: AssemblyInfo 20; Activate 182; MoveLogic 414; RouteDrop 220; Tests 446; csproj/app.config/packages.config/runsettings)

No `.ps1`, `.py`, or `.ts` files changed on the branch. C# is the only language with changed files.

## Rejected Scope Narrowing

None. The caller prompt requested a focused re-audit but did not attempt to narrow the audit scope
below the full branch diff, mark any language out of scope, or instruct skipping a coverage/toolchain
check. The full-branch C# audit was performed.

## Prior Blocking Findings — Resolution Verification

### E4 — `ActivateOlItem` exemption on testable seam (RESOLVED)

- Evidence: TaskTree/TaskTreeController.cs:67 — `internal void ActivateOlItem(object item)`. Parameter
  is now `object` (was `dynamic`). No `[ExcludeFromCodeCoverage]` attribute present.
- Typed dispatch: the not-selectable branch calls `DisplayOutlookItem` (TaskTreeController.cs:117),
  which switches on `case Outlook.MailItem` / `case Outlook.TaskItem` and calls `.Display()`.
- Tests: TaskTree.Test/TaskTreeControllerActivateTests.cs covers the selectable branch
  (`ActivateOlItem_WhenSelectable_ClearsThenAddsToSelection`), the Display branch for both
  MailItem (`_WhenNotSelectable_DisplaysMailItem`) and TaskItem (`_WhenNotSelectable_DisplaysTaskItem`),
  and the caller valid-type path (`TreeLvActivateItem_WhenValidType_ActivatesSelectedItem`).
- Verdict: PASS.

### E5 — `ActivateOlItemAsync` exemption on testable seam (RESOLVED)

- Evidence: TaskTree/TaskTreeController.cs:88 — `internal async Task ActivateOlItemAsync(object item)`.
  Parameter is now `object`. No `[ExcludeFromCodeCoverage]` attribute present.
- Tests: `ActivateOlItemAsync_WhenSelectable_ClearsAddsAndActivates`,
  `ActivateOlItemAsync_WhenNotSelectable_DisplaysAndActivates`, and the async caller path
  `TreeLvActivateItemAsync_WhenValidType_ActivatesSelectedItem`. All awaited deterministically; no
  `Task.Delay`/`Thread.Sleep`.
- Verdict: PASS.

### E6 — `HandleModelDropped` switch routing exemption on testable seam (RESOLVED)

- Evidence: the `DropTargetLocation` switch is extracted into
  TaskTree/TaskTreeController.MoveLogic.cs:107 `internal bool RouteDrop(ITreeVisual, ITreeVisual, ModelDropEventArgs)`,
  operating over the mockable `ITreeVisual` seam and returning bool per branch. Post-drop
  filter/sort re-application is extracted into the covered `ApplyPostDropView` (MoveLogic.cs:154).
  `[ExcludeFromCodeCoverage]` remains ONLY on the thin residual wrapper `HandleModelDropped`
  (MoveLogic.cs:77-78), whose irreducible work is building E2 adapters from the live drop-event
  controls and calling `e.RefreshObjects()`.
- Tests: TaskTree.Test/TaskTreeControllerRouteDropTests.cs covers each enum branch
  (Background→Roots, Item→Children, AboveItem→sibling offset 0, BelowItem→sibling offset 1) plus the
  default→false path, and both `ApplyPostDropView` branches (filter active/inactive). All via
  `ITreeVisual` mocks and a reflection-constructed `ModelDropEventArgs`; no live control.
- Verdict: PASS.

## `[ExcludeFromCodeCoverage]` Register (whole TaskTree project)

Grep of `TaskTree/*.cs` returns exactly four attributes:

| Site | Location | Kind | Justification | Verdict |
|---|---|---|---|---|
| E1 TaskTreeForm | TaskTree/TaskTreeForm.cs:18 (type) | Form-derived host surface | Ratified WinForms exemption (Form-derived) | PASS |
| E2 TreeListViewVisual | TaskTree/TreeListViewVisual.cs:19 (type) | ObjectListView host adapter | Ratified WinForms exemption; STA-assessed, two-line delegation | PASS |
| E3 FormatRow | TaskTree/TaskTreeController.cs:134 | Residual event-handler wrapper | FormatRowEventArgs/OLVListItem not constructible; decision extracted to covered ResolveRowStyle | PASS |
| E6 HandleModelDropped | TaskTree/TaskTreeController.MoveLogic.cs:77 | Residual drop-marshalling wrapper | Adapter construction + e.RefreshObjects() require live control; routing extracted to covered RouteDrop | PASS |

No `[ExcludeFromCodeCoverage]` remains on a testable seam. TaskTree.Test contains zero exemptions.

## Coverage Verification (per language)

Coverage artifact `artifacts/csharp/coverage.xml` (Cobertura) is gitignored (`git check-ignore`
confirms), so it is not present in a fresh branch checkout. Per the evidence-verification model, the
numeric figures recorded in committed evidence are used. The coverage run scope is defined by
TaskTree.Test/coverage.tasktree.runsettings (ModulePath `.*TaskTree\.dll$`, attribute exclude
`ExcludeFromCodeCoverageAttribute`).

### C# — coverage row

- Source of figures: docs/features/.../evidence/qa-gates/remediation-qa-2026-07-09T23-26.md
  (post-remediation) and final-coverage.md / coverage-delta.md (interim).
- Baseline (TaskTree.dll pre-feature): 0% (no test project existed).
- Post-change (TaskTree.dll): line coverage 96.34% (263/273); branch coverage 91.49% (86/94).
- New/changed-code coverage:
  - TaskTree/TaskTreeController.cs (changed): 100.0% line, 96.15% branch.
  - TaskTree/TaskTreeController.MoveLogic.cs (new): 94.54% line, 89.71% branch.
- Repo/assembly floor line >= 85%: met (96.34%). Branch >= 75%: met (91.49%).
- New/changed-code floor >= 90% line: met (100.0% and 94.54%).
- No regression on changed lines: PASS (no prior coverage existed to regress; every covered line is a net gain).
- Change: +2.30 line points over the pre-remediation 94.04% interim figure.
- Disposition: PASS.
- Evidence: remediation-qa-2026-07-09T23-26.md (51 tests passed, +14 new).

### PowerShell — coverage row

- Changed `.ps1` files on branch: 0. Verdict: N/A (no PowerShell files in the branch diff).

### TypeScript — coverage row

- Changed `.ts` files on branch: 0. Verdict: N/A (no TypeScript files in the branch diff).

### Python — coverage row

- Changed `.py` files on branch: 0. Verdict: N/A (no Python files in the branch diff).

## Toolchain Compliance (C#, in order)

| Stage | Command | Exit | Verdict |
|---|---|---|---|
| Format (csharpier) | `csharpier format TaskTree TaskTree.Test` / `csharpier check .` | 0 | PASS |
| Analyzers | `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | PASS |
| Nullable / TWAE | `msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 (0 warnings) | PASS |
| Test (MSTest + Cobertura) | `dotnet-coverage collect -f cobertura ... vstest.console.exe TaskTree.Test.dll` | 0 (51 passed) | PASS |

Evidence: remediation-qa-2026-07-09T23-26.md (single clean pass at 23-20..23-25) and the P7 final-*.md gates.

## File-Size Compliance (500-line limit; independently counted via awk NR)

All changed C# files under 500 lines. Largest production file: TaskTreeController.MoveLogic.cs (315).
Largest test file: TaskTreeControllerTests.cs (446). Designer file TaskTreeForm.Designer.cs (311,
unchanged) also compliant. PASS. (Note: the committed final-filesize.md records earlier interim
counts of 206/295 for the two controller partials; the remediation commit added tests coverage lines,
raising the counts to 230/315 as independently recounted here — still well under 500.)

## Evidence Location Compliance

All feature evidence is written under
docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/{baseline,issue-updates,qa-gates}/,
the canonical `<FEATURE>/evidence/<kind>/` location. The branch diff contains no files under
`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`
(git diff --name-only scan returned NONE). The canonical coverage artifact path
`artifacts/csharp/coverage.xml` is the language coverage artifact (gitignored), not an evidence-tree
violation. No FAIL-level evidence-location findings.

## Wiring / Contract Compliance

- Sole caller unchanged: TaskMaster/Ribbon/RibbonController.cs byte-for-byte unchanged
  (final-caller-unchanged.md; verified by numstat — file absent from diff). Constructor keeps the
  three-positional-argument shape with an optional trailing `Action<string> showMessage = null`. PASS.
- Solution wiring: TaskMaster.sln contains the new project entry (GUID
  {7C4E2B1A-3F9D-4A6E-8B2C-1D5E9F0A7C36}) and all platform ActiveCfg/Build.0 lines. PASS.
- TaskTree.csproj compiles ITaskTreeForm.cs, TaskTreeController.MoveLogic.cs, TreeListViewVisual.cs;
  TaskTree.Test.csproj compiles all four test files. PASS.
- STA policy: no test constructs a live Form/Control, calls Show/ShowDialog, uses an STA thread, or
  uses Thread.Sleep/Task.Delay (grep of TaskTree.Test/*.cs returned no matches). PASS.

## Overall Policy Verdict

PASS. Zero Blocking findings. All three prior Blocking findings (E4/E5/E6) are resolved with code and
test evidence. No new finding introduced.

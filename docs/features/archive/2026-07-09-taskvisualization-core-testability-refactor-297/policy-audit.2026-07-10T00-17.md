# Policy Compliance Audit — taskvisualization-core-testability-refactor (#297)

- Timestamp: 2026-07-10T00-17
- Reviewer: feature-reviewer
- Branch under review: `feature/taskvisualization-core-testability-refactor-297`
- Head: `5f8eea3174441cd1ab14c1e7d1a35a619f8fa539` (feature commit `82b207ff`, followed by `5f8eea31` chore(memory))
- Base branch: `epic/winforms-testability-refactor-integration`
- Merge-base (resolved): `3f04d50f6544f084323e5d7a9a563facb9d579df`
- Work Mode (from issue.md): `full-feature` (user-story.md intentionally absent per spec §User Story Applicability; AC source = spec.md)
- Scope: full branch diff vs merge-base (28 code/config files + 24 doc/evidence files).

## Executive Summary

The refactor is high quality and materially compliant with repository policy. Toolchain evidence is green (csharpier, analyzers, nullable/TWAE, 104/104 MSTest incl. 41 STA). File-size, interface-decoupling, exemption-narrowing, coverage-exclusion, STA-discipline, and evidence-location policies all PASS. The refactored-core aggregate line coverage is 88.95% and the two new helper classes are 100%, clearing the coverage floors.

One **Blocking** finding is recorded (§6b below): `SetFlag(_, Taskname)` and `Shortcut_ReadingNews` are left uncovered while a thin, pattern-consistent write-interception seam (`Action<string>`, matching the feature's existing `_showWarning`/`_mailItemHelperFactory` seams) would make them assertable, and no ratified exemption covers them. This does not breach a numeric coverage floor (the aggregate is met and the methods are counted, not hidden behind an exemption), but under the review-gate adjudication criterion it is a testable seam left uncovered without a ratified exemption.

Overall verdict: **NOT READY TO MERGE** — 1 Blocking finding.

## Rejected Scope Narrowing

None. The delegating prompt directs a full feature-vs-base audit and does not attempt to narrow scope to a plan/task/phase or to mark any changed-file language out of scope. No scope-narrowing directive was detected in the caller prompt or in `plan.2026-07-09T16-07.md`.

## Evidence Location Compliance

Branch-diff scan for committed files under the prohibited `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/` paths: **none found**. All committed evidence is under the canonical `docs/features/active/2026-07-09-taskvisualization-core-testability-refactor-297/evidence/<kind>/` tree (baseline, qa-gates, other). Verdict: **PASS**.

- Note: the evidence markdown references a raw Cobertura file at `artifacts/csharp/coverage.xml`. That path is a transient collector output and is not committed (gitignored); it is not an evidence-location violation because no such file is present in the branch diff.

## Coverage Verification

Coverage thresholds applied: CLAUDE.md (authoritative standing instruction) — repo/aggregate line >= 80%, new modules/classes >= 90%, no regression on changed lines; branch >= 75% (per the review scope). The stricter `.claude/rules/general-unit-test.md` line floor (85%) is also reported for completeness; the aggregate clears both.

Coverage-artifact model: verified from the committed evidence markdown (`evidence/qa-gates/coverage-comparison.2026-07-10T00-01.md`, `evidence/qa-gates/final-tests-coverage.2026-07-10T00-01.md`). The raw Cobertura XML (`artifacts/csharp/coverage.xml`) is a gitignored collector output and is therefore not independently re-parseable from the branch; the numeric figures below are taken from the committed evidence.

### Per-language coverage rows (languages with changed files in the branch diff)

| Language | Changed files | Coverage scope | Line coverage | Threshold | Verdict |
|---|---|---|---|---|---|
| C# (.NET / CSharp) | 26 `.cs` | Refactored core (six `TaskController` partials) line coverage | 942/1059 = 88.95% | >= 80% (CLAUDE.md); >= 85% (rules) | **PASS** |
| C# (.NET / CSharp) | new helpers | `TaskDurationParser` + `TaskPriorityMapper` line coverage | 26/26 = 100.00% | >= 90% (new modules) | **PASS** |
| C# (.NET / CSharp) | STA-measured | `ControlMaps.cs` + `ControlRelationships.cs` line coverage | 284/291 = 97.59% | informational | **PASS** |

- C# coverage disposition: **PASS**. Baseline was 0.00% (the whole `TaskController` type carried a class-level `[ExcludeFromCodeCoverage]`, so all 1861 lines were outside the denominator). Post-change the class-level exemption is removed and the measured refactored-core aggregate is 88.95%; there is no coverage regression on changed lines (0.00% -> 88.95% is a strict improvement). Repo-wide C# figure is not measured by this feature-scoped run (the runsettings scopes the collector to `TaskVisualization.dll`); project-wide 80% is explicitly deferred to sibling #298 per spec §Intent.
- Branch-coverage percentage is not recorded in the committed evidence markdown (the raw Cobertura output that carries branch counters is gitignored and not committed). Line-coverage evidence is strong (aggregate 88.95%). Branch verdict cannot be numerically confirmed from committed artifacts; recorded as an evidence gap, not a floor breach (see PARTIAL note in the feature audit).
- TypeScript, Python, PowerShell: zero changed files in the branch diff — not applicable to this branch.

### Baseline vs post-change comparison (numeric)

- Baseline (P0-T10): TaskVisualization production line coverage = 0.00% (class-level exemption; disabled placeholder test).
- Post-change: refactored-core aggregate line coverage = 88.95%; new helpers = 100.00%.
- Disposition: improvement of +88.95 percentage points on the refactored core; no changed-line regression. PASS.

## Section 1 — General Code Change Policy

| Check | Verdict | Evidence |
|---|---|---|
| File size <= 500 lines (all changed `.cs`, independently counted) | PASS | Largest production file `TaskController.Accelerator.cs` = 500 lines exactly (awk NR = 500, trailing `0a`); `TaskController.Actions.cs` = 490; all others below. Largest test file `TaskControllerActionsTests.cs` = 420. No hidden oversize file (every changed and every existing in-scope `.cs` was counted, not trusted from evidence). |
| Separation of concerns (pure logic vs I/O) | PASS | `TaskDurationParser`, `TaskPriorityMapper` extracted as pure host-neutral units; controller depends on `ITaskViewer` primitives; COM/dialog access routed through seams (`ITagPromptService`, `Action<string>` notifier, `Func<MailItem,Task<MailItemHelper>>` factory). |
| Interface/contract decoupling | PASS | `_viewer` typed `ITaskViewer`; concrete `TaskViewer` referenced only via the confined `Form` cast accessor (`.Handle`/`PostMessage` residue) and one guarded `_viewer is TaskViewer` check inside the exempt `WireKeyPressHandlers`. |
| Error handling / logging | PASS | No new broad catches introduced; `CaptureDuration` routes the warning through the injected notifier; `FormatException` propagation contract documented in XML docs. |
| No new production dependencies | PASS | Only already-referenced libraries used; csproj adds `<Compile>` entries only, no `packages.config` changes. |
| Toolchain loop (format -> analyze -> type-check -> test) | PASS | `evidence/qa-gates/final-csharpier` (exit 0, 1341 files checked, no reformat), `final-analyzer` (exit 0, 0 errors), `final-nullable` (exit 0, TWAE 0 errors), `final-tests-coverage` (exit 0, 104/104). |

## Section 2 — C# Code Change Policy

| Check | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `csharpier check .` exit 0, no changes. |
| .NET analyzers (EnableNETAnalyzers/EnforceCodeStyleInBuild) | PASS | msbuild analyzer build exit 0, 0 errors on TaskVisualization/TaskVisualization.Test. |
| Nullable + TreatWarningsAsErrors | PASS | msbuild nullable/TWAE build exit 0, 0 errors; new code authored nullable-clean. |
| Strong contracts / explicit public API | PASS | `ITaskViewer` primitives-only; new seam ctor params optional-with-default (sole caller `FlagTasks.cs` unchanged — 0 lines in diff). |
| PascalCase/camelCase conventions | PASS | Verified in new files. |

## Section 3 — Unit Test Policy (General + C#)

| Check | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`/`[TestMethod]`, `[STATestClass]`/`[STATestMethod]` throughout. |
| Moq for mocking, FluentAssertions for assertions | PASS | `Mock<...>`, `.Should()...` used across test files. |
| No temp files / no external services / determinism | PASS | Banned-API scan (`ShowDialog`/`.Show(`/`DoEvents`/`Thread.Sleep`/`Task.Delay`/`Timer`/`PostMessage`) returns only explanatory comments; no live network/FS; fixed inputs. |
| Test file location (tests/ mirror) | PASS-with-note | Tests live in the existing `TaskVisualization.Test` project (repo's established C# test-project layout), consistent with in-repo convention; not a colocated `src/` violation. |
| Coverage Exclusion Policy (no production path excluded) | PASS | `coverage.runsettings` excludes only by attribute (`ExcludeFromCodeCoverageAttribute`, `DebuggerHidden`, `DebuggerNonUserCode`, `GeneratedCode`) and includes only `TaskVisualization.dll`; no production source-path exclusion. |

## Section 4 — Exemption Inventory Adjudication

All `[ExcludeFromCodeCoverage]` occurrences in the refactored production files were enumerated and cross-checked against `evidence/other/exemption-inventory.2026-07-10T00-01.md`. Every occurrence is accounted for and names an irreducible dependency:

| Location | Scope | Named dependency | Adjudication |
|---|---|---|---|
| `TaskViewer.cs:18` | class | Form-derived; thin facade; covers Designer partial | Permitted (COM/WinForms). |
| `TagPromptService.cs:18` | class | Dialog host (`TagViewer`/`TagController` + `ShowDialog`) | Permitted; `ITagPromptService` seam is the tested surface. |
| `TaskController.Accelerator.cs:45` `WireKeyPressHandlers` | method | Walks live control tree, `KeyPress +=` on concrete `TaskViewer` | Permitted; genuinely live-form-bound. |
| `TaskController.Accelerator.cs:60` `PostMessage` extern | method | user32 P/Invoke, no managed body | Permitted. |
| `TaskController.Accelerator.cs:284` `FocusTextBox` | method | `TextBox.Select()`/caret needs input focus on live handle | Permitted; dispatch line stays measured. |
| `TaskController.Accelerator.cs:292` `FocusComboBox` | method | `ComboBox.Select()`/`DroppedDown` needs live handle | Permitted. |
| `TaskController.Accelerator.cs:301` `DispatchDateTimePickerClick` | method | `dt.Handle` + `PostMessage` message pump | Permitted; extracted so rest of `ExecuteXlAction` stays measured. |
| `TaskController.Flags.cs:35` `ApplyChanges` | method | `FlagChangeGroup` from live `MailItem`, static `ToDoEvents.Editing`, `WriteFlagsBatchAsync`, training-queue enqueue | Permitted (see §6a). |

No file-level exemption exists on `TaskController.Accelerator.cs`, `TaskController.ControlMaps.cs`, or `TaskController.ControlRelationships.cs` (independently grep-verified: 0). The class-level exemption on `TaskController` is removed (verified: `TaskController.cs:20` = `public partial class TaskController`, no attribute).

### 6a — `ApplyChanges` method-level exemption — PERMITTED (not Blocking)

- Pre-authorization confirmed: `plan.2026-07-09T16-07.md` line 172 lists "`TaskController.Flags.cs` — `ApplyChanges` COM-iteration lines" as a method/branch-level exemption in the maintainer-ratified exemption inventory, and task P7-T1(iv) explicitly enumerates it. The spec §Coverage Exemption Constraint permits per-region method/branch exemptions with named dependencies.
- Extractable units are covered: `ApplyChange(flag,current,revised)`, `ApplyChange(fcg,...)`, and `AreCollectionsEqual` are exercised with positive/negative/edge cases in `TaskControllerFlagsTests.cs` (option off / equal / differing / fcg-enqueue-true / null / disjoint / duplicate-collapse). Flags.cs measured coverage = 30/33 = 90.91%.
- Seam feasibility: driving the `ApplyChanges` loop over Moq doubles does not terminate (documented by the executor: `ToDoEvents.Editing` + `WriteFlagsBatchAsync` + `FlagChangeGroup` depend on live COM semantics). A `WriteFlagsBatchAsync`+`FlagChangeGroup` factory seam is feasible in principle but was ratified as disproportionate for the iteration wiring. Because the exemption is genuinely pre-authorized and the extractable units are covered, it does not meet the Blocking criterion.
- Observation (non-blocking): the whole-method exemption is slightly broader than the plan's "COM-iteration lines" wording — it also covers the in-loop field-application block (Flags.cs lines 51-114), which is host-neutral comparison/assignment logic that only becomes reachable if the loop terminates. A future iteration (or #298) could extract an `ApplyActiveFieldsTo(ToDoItem)` helper to shrink the exempt surface. Not required for this feature.

### 6b — `SetFlag(_, Taskname)` / `Shortcut_ReadingNews` uncovered — BLOCKING

See the code-review artifact for the full write-up and the required seam-based fix. Summary: these methods are left uncovered (honestly counted, not exempted). The uncoverable-over-Moq cause is the `_active.TaskSubject = value` write (a concrete `ToDoItem` scalar that routes to Outlook-interop reflection and raises `MissingMethodException` on a Moq proxy). A thin `Action<string>` write-interception seam at the controller layer — identical in shape to the feature's already-added `_showWarning` and `_mailItemHelperFactory` seams — is feasible and would make both methods assertable. Because a thin seam is feasible and the methods are uncovered without a ratified exemption, this is a Blocking finding under the review-gate criterion. It does not breach a numeric coverage floor (aggregate 88.95% is met).

## Section 5 — Reusable Contracts for Sibling #298

| Check | Verdict | Evidence |
|---|---|---|
| `ITaskViewer.cs` present, `: IForm`, primitives-only | PASS | Members are string/object/bool/DateTime + `FocusDuration()`/`SetController(...)`; no WinForms control types. |
| `ITaskViewerControls.cs` present (control-identity companion) | PASS | Present (123 lines); real `Label`/`Control` types for STA. |
| `ITagPromptService.cs` / `TagPromptService.cs` present, reusable by #298 | PASS | Interface (102 lines) + adapter (49 lines, exempt); defined in `TaskVisualization` for #298 reuse. |
| csproj `<Compile>` lists consistent | PASS | All 11 new production files and all 12 new test files added to their csproj `<Compile>` groups. |

## Section 6 — Assumptions & Notes

1. Coverage thresholds: applied CLAUDE.md 80/90 as authoritative (consistent with the governance decision that retained 80/90 for this repo), and confirmed the aggregate also clears the `.claude/rules` 85% line floor. Branch (>=75%) could not be numerically confirmed from committed artifacts.
2. Repo-wide C# coverage is intentionally feature-scoped here; project-wide 80% is #298's deliverable per spec.
3. PR context artifacts were regenerated (they were absent) from the branch diff into `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`.

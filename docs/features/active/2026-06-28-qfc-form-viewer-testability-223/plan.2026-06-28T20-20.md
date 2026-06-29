# qfc-form-viewer-testability — Atomic Implementation Plan

- **Issue:** #223
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-28T20-20
- **Status:** Draft
- **Work Mode:** full-feature
- **Target plan path:** `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/plan.2026-06-28T20-20.md`

## Authoritative Inputs

- Spec: `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/spec.md`
- Issue + AC1–AC7: `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/issue.md`
- Research 1 (Seams A+B, interface critique): `artifacts/research/2026-06-28T18-00-qfc-form-viewer-testability-research.md`
- Research 2 (Seams C+D, blast radius, 500-line analysis, phase ordering): `artifacts/research/2026-06-28T19-00-qfc-seam-c-d-implementation-research.md`

The two research documents are the source of truth for member names, signatures, Form implementations, call-site rewrites, and the test surface. This plan does not restate those signatures; it sequences the edits and binds each to a verifiable outcome.

## Evidence Location Invariant

All evidence artifacts MUST be written under the canonical scheme
`docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/<kind>/`
(`evidence/baseline/`, `evidence/qa-gates/`, `evidence/regression-testing/`, `evidence/other/`).
Non-canonical paths such as `artifacts/baselines/`, `artifacts/qa/`, or `artifacts/coverage/` are prohibited and fail preflight. Each evidence artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; coverage-bearing artifacts must record numeric coverage values, not placeholders.

## C# Toolchain (run in this exact order; restart from step 1 on any failure or file change)

1. `dotnet tool run csharpier .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`

## Scope Snapshot (8 production files + 4 test files)

Production: `QuickFiler/Interfaces/IQfcFormViewer.cs`, `QuickFiler/Viewers/QfcFormViewer.cs`, `QuickFiler/Viewers/QfcFormViewerDark.cs`, `QuickFiler/Viewers/QfcFormViewerExpanded.cs`, `QuickFiler/Controllers/QfcFormController.cs` (split into partials), `QuickFiler/Controllers/QfcFormKeyHandler.cs` (NEW), `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler/Controllers/QfcHomeController.cs`.
Test: `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` (NEW), `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` (held net-neutral; pre-existing 823-line test-cap debt), `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` (NEW; new seam tests routed here to keep the existing file net-neutral), `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` (migrated off removed Seam B members).

Verified facts driving the plan:
- `QfcFormController.cs` is 1142 lines with `#region` boundaries: Constructors (24–62), Private Variables (64–93), Setup and Disposal (95–371), Public Properties (373–470), Event Handlers (472–849), Major Actions (851–1140).
- Both `QuickFiler.csproj` and `QuickFiler.Test.csproj` are legacy `packages.config` projects that reference sources by explicit `<Compile Include>` (no glob). Every NEW `.cs` file MUST be wired into the owning `.csproj` or it will not compile.
- Final `IQfcFormViewer` = 23 declared members (remove 7, narrow 1 setter to get-only, add 13). See Research 2 §3.

---

### Phase 0 — Baseline Capture and Policy Reads

- [ ] [P0-T1] Read policy files in the required order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and the four authoritative inputs above. Write `evidence/baseline/phase0-instructions-read.md` with `Timestamp:`, `Policy Order:`, and the explicit list of files read. Acceptance: artifact exists with all three fields populated.
- [ ] [P0-T2] Run `dotnet tool run csharpier .` in check posture against the current tree. Write `evidence/baseline/baseline-csharpier.<ISO-8601>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact records the format-check exit code and any pre-existing format drift.
- [ ] [P0-T3] Run the analyzer build (toolchain step 2) on the clean tree. Write `evidence/baseline/baseline-analyzers.<ISO-8601>.md` with the four required fields and a summary of analyzer diagnostic counts. Acceptance: artifact records `EXIT_CODE:` and diagnostic headline.
- [ ] [P0-T4] Run the nullable/TreatWarningsAsErrors build (toolchain step 3) on the clean tree. Write `evidence/baseline/baseline-nullable.<ISO-8601>.md` with the four required fields. Acceptance: artifact records `EXIT_CODE:` and warning headline.
- [ ] [P0-T5] Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`. Write `evidence/baseline/baseline-tests-coverage.<ISO-8601>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording numeric values: total passed/failed test count, repo-wide line-coverage percent, and `QfcFormController` line-coverage percent. Acceptance: artifact contains numeric coverage values (no placeholders) and the passing test count to be preserved across all later phases.
- [ ] [P0-T6] Record the pre-existing 500-line-cap inventory: capture current line counts for `QfcFormController.cs` (expected 1142), `QfcCollectionController.cs` (expected ~2300, `[ExcludeFromCodeCoverage]`), and the test file `QfcFormControllerTests.cs` (expected 823, a pre-existing test-code cap violation). Write `evidence/baseline/baseline-file-sizes.<ISO-8601>.md` with `Timestamp:`, the three measured counts, and explicit notes that (a) `QfcCollectionController.cs` is pre-existing production debt receiving only a net-negative edit this cycle and is NOT to be split, and (b) `QfcFormControllerTests.cs` is pre-existing test-code debt that must remain net-neutral this cycle because new seam tests are routed to a separate file (AC6 disposition basis). Acceptance: artifact records all three counts and both disposition statements.

---

### Phase 1 — Prerequisite: Split `QfcFormController.cs` into Partial Classes (no behavior change)

Pure structural split to satisfy the 500-line cap before any code is added. Each region moves verbatim into a partial file; the class declaration becomes `partial`. No method bodies change.

- [ ] [P1-T1] In `QuickFiler/Controllers/QfcFormController.cs`, change the declaration to `internal partial class QfcFormController : IQfcFormController`, and move the entire `Setup and Disposal` region (current lines 95–371: `CaptureItemSettings`, `RemoveTemplatesAndSetupTlp`, `SetupLightDark`, `LoadItemsPerIteration`, `SpaceForEmail`, the `ItemsPerIteration` property, `RegisterFormEventHandlers`, `UnregisterFormEventHandlers`, `Cleanup`) verbatim into NEW `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` wrapped in the same `namespace QuickFiler.Controllers { internal partial class QfcFormController { ... } }` with the required `using` directives. Acceptance: the region is removed from the main file and present unchanged in the new file; main file declares `partial`.
- [ ] [P1-T2] Move the entire `Event Handlers` region (current lines 472–849) verbatim into NEW `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` with the same partial-class wrapper and required usings. Acceptance: the region is removed from the main file and present unchanged in the new file.
- [ ] [P1-T3] Move the entire `Major Actions` region (current lines 851–1140) verbatim into NEW `QuickFiler/Controllers/QfcFormController.Actions.cs` with the same partial-class wrapper and required usings. Acceptance: the region is removed from the main file and present unchanged in the new file. Main `QfcFormController.cs` now retains only usings, namespace/class declaration, Constructors, Private Variables, and Public Properties.
- [ ] [P1-T4] Add `<Compile Include="Controllers\QfcFormController.SetupDisposal.cs" />`, `<Compile Include="Controllers\QfcFormController.EventHandlers.cs" />`, and `<Compile Include="Controllers\QfcFormController.Actions.cs" />` to `QuickFiler/QuickFiler.csproj` adjacent to the existing `Controllers\QfcFormController.cs` entry (line 304). Acceptance: all three new partial files have explicit Compile Include entries.
- [ ] [P1-T5] Measure line counts of `QfcFormController.cs` and the three new partial files. Write `evidence/qa-gates/p1-file-sizes.<ISO-8601>.md` with `Timestamp:` and the four counts. Acceptance: every one of the four files is `< 500` lines (expected approx: main ~190, SetupDisposal ~286, EventHandlers ~387, Actions ~299).
- [ ] [P1-T6] Run toolchain step 1 (`dotnet tool run csharpier .`). Write `evidence/qa-gates/p1-csharpier.<ISO-8601>.md` with the four required fields. Acceptance: `EXIT_CODE: 0` with no unresolved format drift; restart loop if files changed.
- [ ] [P1-T7] Run toolchain step 2 (analyzers). Write `evidence/qa-gates/p1-analyzers.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`, no new analyzer errors versus baseline.
- [ ] [P1-T8] Run toolchain step 3 (nullable/TreatWarningsAsErrors). Write `evidence/qa-gates/p1-nullable.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`.
- [ ] [P1-T9] Run toolchain step 4 (`vstest.console.exe ... /EnableCodeCoverage`). Write `evidence/qa-gates/p1-tests-coverage.<ISO-8601>.md` with numeric passing count and repo-wide coverage. Acceptance: passing test count equals the P0-T5 baseline (pure structural split causes no test change) and `EXIT_CODE: 0`.

---

### Phase 2 — Seam A: Extract `QfcFormKeyHandler.IsAltKeyCommand` (no interface change)

- [ ] [P2-T1] Create NEW `QuickFiler/Controllers/QfcFormKeyHandler.cs` containing `internal static class QfcFormKeyHandler` with `internal static bool IsAltKeyCommand(Keys keyData) => keyData.HasFlag(Keys.Alt);` and an XML doc comment per Research 1 §3 Seam A. Acceptance: file exists with the single pure static method.
- [ ] [P2-T2] Add `<Compile Include="Controllers\QfcFormKeyHandler.cs" />` to `QuickFiler/QuickFiler.csproj` adjacent to the `QfcFormController` entries. Acceptance: the new production file is wired into the project.
- [ ] [P2-T3] Update `QfcFormViewer.ProcessCmdKey` in `QuickFiler/Viewers/QfcFormViewer.cs` to gate on `QfcFormKeyHandler.IsAltKeyCommand(keyData)` instead of the inline `keyData.HasFlag(Keys.Alt)`, preserving the existing `SetSynchronizationContext` + `ToggleKeyboardDialogAsync` behavior exactly. Acceptance: predicate routed through the new method; no behavioral change to the Alt-key dialog toggle.
- [ ] [P2-T4] Update `QfcFormViewerDark.ProcessCmdKey` in `QuickFiler/Viewers/QfcFormViewerDark.cs` to call `QfcFormKeyHandler.IsAltKeyCommand(keyData)`, preserving its existing synchronous `KeyboardHandler_KeyDown` dispatch, and add `[ExcludeFromCodeCoverage]` to the class. Acceptance: predicate routed through the new method; class carries `[ExcludeFromCodeCoverage]`.
- [ ] [P2-T5] Update `QfcFormViewerExpanded.ProcessCmdKey` in `QuickFiler/Viewers/QfcFormViewerExpanded.cs` identically to P2-T4, and add `[ExcludeFromCodeCoverage]` to the class. Acceptance: predicate routed through the new method; class carries `[ExcludeFromCodeCoverage]`.
- [ ] [P2-T6] Create NEW `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` (MSTest + FluentAssertions) with four `[TestMethod]` cases per Research 1 §6.4: `IsAltKeyCommand(Keys.Alt)` → true, `IsAltKeyCommand(Keys.Alt | Keys.Left)` → true, `IsAltKeyCommand(Keys.Control)` → false, `IsAltKeyCommand(Keys.None)` → false. Acceptance: four AAA-structured tests with descriptive names; no temporary files; deterministic.
- [ ] [P2-T7] Add `<Compile Include="Controllers\QfcFormKeyHandlerTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` adjacent to the existing `Controllers\QfcFormControllerTests.cs` entry (line 69). Acceptance: the new test file is wired into the test project.
- [ ] [P2-T8] Run toolchain step 1 (csharpier). Write `evidence/qa-gates/p2-csharpier.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`; restart loop if files changed.
- [ ] [P2-T9] Run toolchain step 2 (analyzers). Write `evidence/qa-gates/p2-analyzers.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`.
- [ ] [P2-T10] Run toolchain step 3 (nullable). Write `evidence/qa-gates/p2-nullable.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`.
- [ ] [P2-T11] Run toolchain step 4 (`vstest ... /EnableCodeCoverage`). Write `evidence/qa-gates/p2-tests-coverage.<ISO-8601>.md` recording the four new tests passing and the numeric line-coverage for `QfcFormKeyHandler`. Acceptance: all four new tests pass, prior tests still pass, and `QfcFormKeyHandler` coverage `>= 90%` (AC5 new-code floor).

---

### Phase 3 — Seams B + C + D Combined: Interface Narrowing, Implementations, Consumer Rewrites, Tests

All three seams touch `IQfcFormViewer` and `QfcFormViewer` in one editing pass to avoid an intermediate partial-narrowing state. Final interface = 23 members (Research 2 §3).

- [ ] [P3-T1] Edit `QuickFiler/Interfaces/IQfcFormViewer.cs` to the final 23-member shape: remove the 7 members (`L1v1L2h2_ButtonOK`, `L1v1L2h3_ButtonCancel`, `L1v1L2h4_ButtonUndo`, `L1v1L2h5_BtnSkip`, `NumericUpDown L1v1L2h5_SpnEmailPerLoad`, `ItemViewer QfcItemViewerTemplate`, `ItemViewerExpanded QfcItemViewerExpandedTemplate`); narrow `L1v0L2L3v_TableLayout` to get-only; add the 13 intent members (Seam B: `OkClicked`, `CancelClicked`, `UndoClicked`, `SkipClicked`, `SkipButtonText`, `SkipButtonEnabled`, `ItemsPerLoadValue`, `ItemsPerLoadValueChanged`, `ItemsPerLoadEnabled`; Seam C: `SwapItemTableLayout(TableLayoutPanel)`; Seam D: `CaptureTlpCellStates()`, `GetKeyEventExclusionControls()`, `ItemViewerTemplateMargin`). Acceptance: interface declares exactly 23 members matching Research 2 §3; no raw `Button`/`NumericUpDown` member remains; `L1v0L2L3v_TableLayout` is get-only.
- [ ] [P3-T2] In `QuickFiler/Viewers/QfcFormViewer.cs`, implement the nine Seam B intent members (event add/remove forwarding to the backing controls; `SkipButtonText`/`SkipButtonEnabled`/`ItemsPerLoadValue`/`ItemsPerLoadEnabled` get/set forwarding) per Research 1 §3 Seam B, and remove the five old raw-control public property implementations. Acceptance: Form implements all nine Seam B members; the five removed properties no longer compile-reference externally; backing Designer fields retained privately.
- [ ] [P3-T3] In `QuickFiler/Viewers/QfcFormViewer.cs`, implement `SwapItemTableLayout(TableLayoutPanel newTlp)` per Research 2 §1.2 (remove old TLP from main panel, reparent new TLP, set visible) and reduce `L1v0L2L3v_TableLayout` to a get-only public property over the private backing field. Acceptance: `SwapItemTableLayout` present; public setter removed (private setter retained internally).
- [ ] [P3-T4] In `QuickFiler/Viewers/QfcFormViewer.cs`, implement the three Seam D members per Research 2 §2.2: `CaptureTlpCellStates()` (null-guard returning `null` when either template is uninitialized; otherwise the Expanded+Compressed snapshot lists), `GetKeyEventExclusionControls()` returning `IReadOnlyList<Control>` with the collapsed template, and `ItemViewerTemplateMargin` returning `_qfcItemViewerTemplate?.Margin ?? default`. The two template fields remain private. Acceptance: three Seam D members implemented; template properties no longer public/interface-exposed.
- [ ] [P3-T5] In `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`, rewrite `RegisterFormEventHandlers` and `UnregisterFormEventHandlers` to subscribe/unsubscribe the intent events (`OkClicked`/`CancelClicked`/`UndoClicked`/`SkipClicked`/`ItemsPerLoadValueChanged`) per Research 1 §3 Seam B, replacing the five raw-control `.Click`/`.ValueChanged` wirings. Acceptance: handler wiring uses intent events; no raw-control event references remain.
- [ ] [P3-T6] In `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, rewrite `ButtonSkipHandler` (use `SkipButtonEnabled`/`SkipButtonText`) and `SpnEmailPerLoadHandler` (use `(int)ItemsPerLoadValue` and `ItemsPerLoadValue` reset) per Research 1 §6.3, and in `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` (both members relocated there by P1-T1) update `LoadItemsPerIteration` and the `ItemsPerIteration` property setter to write `ItemsPerLoadValue = (decimal)...` through `Invoke`. Acceptance: Skip/spinner state flows through intent properties; behavior preserved.
- [ ] [P3-T7] In `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`, rewrite `CaptureItemSettings` to read `_formViewer.ItemViewerTemplateMargin` and call `_formViewer.CaptureTlpCellStates()`, with the null-result branch hiding the form and returning, per Research 2 §2.2. Remove the inline `new TlpCellStates(...)` construction block and the direct `QfcItemViewerTemplate`/`QfcItemViewerExpandedTemplate` sub-property traversal. Acceptance: method no longer references the removed template members; populated, null, and early-return (null RowStyles) paths preserved.
- [ ] [P3-T8] In the partial file holding `RegisterFormEventHandlers`/`UnregisterFormEventHandlers` (the Setup and Disposal region relocated by P1-T1, i.e. `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`), rewrite BOTH keyboard-exclusion calls — the one in `RegisterFormEventHandlers` (current line 308) AND the identical one in `UnregisterFormEventHandlers` (current line 336) — to use `_formViewer.GetKeyEventExclusionControls().ToList()` (per Research 2 §2.2) instead of the inline `new List<Control> { _formViewer.QfcItemViewerTemplate }`. Acceptance: both exclusion lists are sourced from the interface method; no `_formViewer.QfcItemViewerTemplate` reference remains in either method; `.ToList()` conversion present at both call sites for the `ForAllControls` `List<Control>` parameter.
- [ ] [P3-T9] In `QuickFiler/Controllers/QfcCollectionController.cs`, rewrite `ActivateQueuedTlp` to call `_formViewer.SwapItemTableLayout(tlp)` then cache `_itemTlp = _formViewer.L1v0L2L3v_TableLayout` (getter) per Research 2 §1.2 (net −3 lines). Do not change any other method; do not split this file. Acceptance: the only interface setter write is removed; method is a net-negative edit; file remains `[ExcludeFromCodeCoverage]`.
- [ ] [P3-T10] In `QuickFiler/Controllers/QfcHomeController.cs`, rewrite `Worker_RunWorkerCompleted` to set `_formViewer.ItemsPerLoadEnabled = true` and `_formViewer.SkipButtonEnabled = true` per Research 1 §3 Seam B, replacing the `L1v1L2h5_SpnEmailPerLoad.Enabled`/`L1v1L2h5_BtnSkip.Enabled` writes. Acceptance: no raw-control member references remain in this file.
- [ ] [P3-T11] In `QuickFiler.Test/Controllers/QfcFormControllerTests.cs`, migrate existing mock setups that reference removed members (`SetupGet(x => x.L1v1L2h5_SpnEmailPerLoad)`, `L1v1L2h5_BtnSkip`, button properties) to the intent members (`SetupProperty(x => x.ItemsPerLoadValue)`, `SetupProperty(x => x.SkipButtonEnabled)`, `SetupProperty(x => x.SkipButtonText)`, event `SetupAdd`/`SetupRemove`) per Research 2 §7. This migration MUST be net-neutral on file length: replace setups in place without adding net lines, and do NOT add any new `[TestMethod]` cases here — all new seam tests are created in the separate file in P3-T13. Acceptance: existing tests compile and still assert the same behavior; no reference to removed interface members remains; the file's line count does not exceed its P0-T6 baseline (823 lines).
- [ ] [P3-T12] In `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` (compiled at `QuickFiler.Test.csproj` line 75), migrate `Worker_RunWorkerCompleted_HandlesCompletionCorrectly` off the members Seam B removes: replace the `mockFormViewer.SetupGet(m => m.L1v1L2h5_SpnEmailPerLoad).Returns(spinner)` and `mockFormViewer.SetupGet(m => m.L1v1L2h5_BtnSkip).Returns(button)` setups (lines 420–421) with `SetupProperty(x => x.ItemsPerLoadEnabled)` and `SetupProperty(x => x.SkipButtonEnabled)`, and rewrite the `spinner.Enabled`/`button.Enabled` asserts (lines 444–445) to verify `ItemsPerLoadEnabled == true` and `SkipButtonEnabled == true` after `Worker_RunWorkerCompleted`, matching the P3-T10 production rewrite. Acceptance: the test compiles against the narrowed `IQfcFormViewer`, asserts the same enable-on-completion behavior, and references no removed member.
- [ ] [P3-T13] Create NEW `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` containing a separate `[TestClass]` (MSTest + Moq + FluentAssertions) that follows the established fixture pattern of `QfcFormControllerTests.cs`, and add the new test methods per Research 1 §6.4 and Research 2 §2.5: command-event routing for `OkClicked`/`CancelClicked`/`UndoClicked`/`SkipClicked`/`ItemsPerLoadValueChanged` via `Raise`; skip-flow `VerifySet` for `SkipButtonText`/`SkipButtonEnabled`; `CaptureItemSettings` populated-states, null-states, and null-RowStyles early-return cases; and `RegisterFormEventHandlers_UsesExclusionControlsFromFormViewer` verifying `GetKeyEventExclusionControls()` is called. Acceptance: new file is a separate `[TestClass]`, is `< 500` lines, `QfcFormControllerTests.cs` line count is not increased versus its P0-T6 baseline, and tests are AAA-structured, deterministic, use no temporary files, and exercise the routing/state/snapshot paths described in AC5.
- [ ] [P3-T14] Add `<Compile Include="Controllers\QfcFormControllerSeamTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` adjacent to the existing `Controllers\QfcFormControllerTests.cs` entry (line 69). Acceptance: the new seam-test file has an explicit `<Compile Include>` entry (the project uses no glob; an unwired file will not compile).
- [ ] [P3-T15] Measure line counts for every production file modified in Phases 1–3 (`IQfcFormViewer.cs`, `QfcFormViewer.cs`, `QfcFormViewerDark.cs`, `QfcFormViewerExpanded.cs`, the four `QfcFormController*.cs` partials, `QfcFormKeyHandler.cs`, `QfcHomeController.cs`, `QfcCollectionController.cs`) and the touched/new test files (`QfcFormControllerTests.cs`, `QfcFormControllerSeamTests.cs`, `QfcHomeControllerRunAsyncTests.cs`, `QfcFormKeyHandlerTests.cs`); confirm `QfcCollectionController.cs` is net-negative versus its P0-T6 baseline and `QfcFormControllerTests.cs` is not increased versus its P0-T6 baseline (823). Write `evidence/qa-gates/p3-file-sizes.<ISO-8601>.md` with `Timestamp:` and all counts. Acceptance: every modified production file except `QfcCollectionController.cs` is `< 500` lines; the new `QfcFormControllerSeamTests.cs` is `< 500` lines; `QfcFormControllerTests.cs` is `<=` its P0-T6 baseline (tracked pre-existing test-cap debt, not increased); `QfcCollectionController.cs` is `<=` its baseline count and explicitly recorded as pre-existing-debt disposition (AC6).
- [ ] [P3-T16] Run toolchain step 1 (csharpier). Write `evidence/qa-gates/p3-csharpier.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`; restart loop if files changed.
- [ ] [P3-T17] Run toolchain step 2 (analyzers). Write `evidence/qa-gates/p3-analyzers.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`, no new analyzer errors.
- [ ] [P3-T18] Run toolchain step 3 (nullable/TreatWarningsAsErrors). Write `evidence/qa-gates/p3-nullable.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`.
- [ ] [P3-T19] Run toolchain step 4 (`vstest ... /EnableCodeCoverage`). Write `evidence/qa-gates/p3-tests-coverage.<ISO-8601>.md` recording numeric passing count, repo-wide coverage, and `QfcFormController` coverage. Acceptance: all tests pass (including new P3 tests); `EXIT_CODE: 0`.

---

### Phase 4 — Final QA Loop, Coverage Delta, and Disposition

Authoritative final-QC block. Each command step produces its own artifact; no aggregate-only artifact.

- [ ] [P4-T1] Run toolchain step 1 (`dotnet tool run csharpier .`) on the final tree. Write `evidence/qa-gates/final-csharpier.<ISO-8601>.md` with the four required fields. Acceptance: `EXIT_CODE: 0` with no remaining format drift; if files change, restart the full loop from this task.
- [ ] [P4-T2] Run toolchain step 2 (analyzers). Write `evidence/qa-gates/final-analyzers.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`, no analyzer errors (AC7).
- [ ] [P4-T3] Run toolchain step 3 (nullable/TreatWarningsAsErrors). Write `evidence/qa-gates/final-nullable.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0` (AC7).
- [ ] [P4-T4] Run toolchain step 4 (`vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`). Write `evidence/qa-gates/final-tests-coverage.<ISO-8601>.md` recording numeric post-change repo-wide coverage, `QfcFormKeyHandler` coverage, and `QfcFormController` coverage. Acceptance: all tests pass; `EXIT_CODE: 0` (AC7).
- [ ] [P4-T5] Compute the coverage delta against the P0-T5 baseline. Write `evidence/regression-testing/coverage-delta.<ISO-8601>.md` reporting: baseline repo-wide coverage, post-change repo-wide coverage, `QfcFormKeyHandler` new-code coverage, and `QfcFormController` changed-line coverage. Acceptance: repo-wide `>= 80%`; `QfcFormKeyHandler >= 90%`; `QfcFormController` changed lines show no regression versus baseline (AC5). If any threshold is unmet, mark the outcome remediation-required, not PASS.
- [ ] [P4-T6] Write `evidence/other/ac-traceability.<ISO-8601>.md` mapping AC1–AC7 to the satisfying tasks and evidence artifacts (mapping table below), and record the AC6 file-size dispositions: (a) the `QfcCollectionController.cs` pre-existing production-violation disposition (net-negative, not split) and (b) the `QfcFormControllerTests.cs` pre-existing test-code-cap disposition (held net-neutral; new seam tests routed to `QfcFormControllerSeamTests.cs`, which is `< 500` lines), each with its P0-T6 and P3-T15 line-count evidence. Acceptance: all seven ACs mapped to at least one completed task and one evidence artifact; both AC6 disposition statements present.

---

## Acceptance Criteria Traceability

| AC | Requirement | Satisfying tasks | Evidence |
|---|---|---|---|
| AC1 | `IsAltKeyCommand` exists and is called by all three viewers; Dark/Expanded `[ExcludeFromCodeCoverage]` | P2-T1, P2-T3, P2-T4, P2-T5 | `evidence/qa-gates/p2-*` |
| AC2 | Intent events/state props replace 4 Buttons + NumericUpDown; no raw clickable control on interface | P3-T1, P3-T2, P3-T5, P3-T6, P3-T10, P3-T12 | `evidence/qa-gates/p3-*` |
| AC3 | `SwapItemTableLayout` added; `L1v0L2L3v_TableLayout` get-only; `ActivateQueuedTlp` swaps via new method | P3-T1, P3-T3, P3-T9 | `evidence/qa-gates/p3-*` |
| AC4 | `CaptureTlpCellStates`/`GetKeyEventExclusionControls`/`ItemViewerTemplateMargin` added; templates removed; consumers updated | P3-T1, P3-T4, P3-T7, P3-T8 | `evidence/qa-gates/p3-*` |
| AC5 | New MSTest coverage (routing, skip flow, CaptureItemSettings populated/null); new code `>= 90%`; no changed-line regression; repo-wide `>= 80%` | P2-T6, P2-T11, P3-T11, P3-T12, P3-T13, P4-T4, P4-T5 | `evidence/qa-gates/p2-tests-coverage`, `evidence/regression-testing/coverage-delta` |
| AC6 | No modified production file `> 500` lines after change; new `QfcFormControllerSeamTests.cs` `< 500` lines; `QfcCollectionController.cs` net-negative production-debt disposition and `QfcFormControllerTests.cs` net-neutral test-cap disposition recorded | P0-T6, P1-T5, P3-T13, P3-T15, P4-T6 | `evidence/baseline/baseline-file-sizes`, `evidence/qa-gates/p1-file-sizes`, `evidence/qa-gates/p3-file-sizes`, `evidence/other/ac-traceability` |
| AC7 | Full C# toolchain passes in order with no regressions | P1-T6..T9, P2-T8..T11, P3-T16..T19, P4-T1..T4 | `evidence/qa-gates/final-*` |

## Invariants Encoded in This Plan

- Runtime behavior of OK/Cancel/Undo/Skip, items-per-load spinner, TLP swap, and Alt-key toggle is preserved; all edits are structural/testability refactors (Phases 1–3 verification gates re-confirm the baseline passing test count).
- `QfcFormViewer`, `QfcFormViewerDark`, `QfcFormViewerExpanded` remain Form-derived and `[ExcludeFromCodeCoverage]`; Designer files untouched.
- `QfcCollectionController.cs` is not split; it receives only the net-negative `ActivateQueuedTlp` edit (P3-T9) and its pre-existing-violation disposition is recorded (AC6).
- `QfcFormControllerTests.cs` (pre-existing 823-line test-cap debt) is held net-neutral; the in-place migration (P3-T11) adds no `[TestMethod]` cases. All new seam tests land in the new `QfcFormControllerSeamTests.cs` (separate `[TestClass]`, `< 500` lines), keeping the existing file from growing further (AC6).
- MSTest + Moq + FluentAssertions only; no temporary files; deterministic tests.

## Notes

- Plan-path continuity: this is the single plan file for issue #223; preflight revisions update this file in place.
- Phase ordering follows Research 2 §6: structural split (Phase 1) precedes any code addition; Seam A (Phase 2) is interface-independent and lands first; Seams B+C+D (Phase 3) are delivered together to avoid an intermediate partial-narrowing state.

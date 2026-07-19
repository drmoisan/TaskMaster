# utilitiescs-nullable-dialogs-misc — Plan

- **Issue:** #374
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (Wave 1)
- **Owner:** drmoisan
- **Work Mode:** full-feature
- **Last Updated:** 2026-07-18T22-30
- **Status:** Draft
- **Version:** 0.2

## Required References

- CLAUDE.md (standing instructions, C# toolchain section).
- `.claude/rules/general-code-change.md` (cross-language code change policy).
- `.claude/rules/general-unit-test.md` (cross-language unit test policy).
- `.claude/rules/csharp.md` (C#-specific toolchain and standards).
- Requirements sources: `docs/features/active/utilitiescs-nullable-dialogs-misc/issue.md`,
  `docs/features/active/utilitiescs-nullable-dialogs-misc/spec.md`,
  `docs/features/active/utilitiescs-nullable-dialogs-misc/user-story.md`.
- Research: `docs/features/active/utilitiescs-nullable-dialogs-misc/research/research.2026-07-18T22-40.md`.
- Upstream contract (Wave 0, must have merged before Phase 1 execution begins): issue #363
  (`utilitiescs-nullable-extensions`), Batch D, `UtilitiesCS/Extensions/WinFormsExtensions.cs` —
  this cluster consumes `WinFormsExtensions.Clone<T>() where T : Control` from `ActionButton.cs`,
  `DelegateButton.cs`, `FunctionButton.cs`, and `MyBox.cs` (`ButtonTemplate` setter).

**All work must comply with these policies; do not duplicate their content here.**

## Scope Invariants (encode into every batch task)

- Per-file `#nullable enable` opt-in ONLY, applied to the 14 in-scope files: 12 `UtilitiesCS/Dialogs/`
  remediation targets (Batches A–E) plus 2 verify-only "misc" files
  (`UtilitiesCS/WindowsAPI/ExtraDeclarations.cs`, `UtilitiesCS/Properties/AssemblyInfo.cs`). Do NOT
  add a `<Nullable>` element to `UtilitiesCS/UtilitiesCS.csproj` (AC2). Confirmed by grep: the
  csproj currently contains zero `<Nullable>` occurrences.
- The four Designer-generated files (`DelegateButtonTemplate.Designer.cs`,
  `FolderNotFoundViewer.Designer.cs`, `InputBoxViewer.Designer.cs`, `MyBoxViewer.Designer.cs`) are
  NEVER opted in, receive no pragma, and are never edited by any task in this plan (AC6). No
  combined-batch requirement applies to these pairs (only one hand-written half of each pair is
  ever opted in).
- Verification uses the per-file pragma gate:
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`.
  Do NOT pass `/p:Nullable=enable` globally in any command; the global flag surfaces the whole
  epic's pre-existing debt and drowns this child's signal. Enforcement is per-file pragma only.
  This is a deliberate, documented deviation from the stock CLAUDE.md/`.claude/rules/csharp.md`
  type-check command for this child only; it must NOT be resolved by editing `.claude/rules/*`.
- Target is net481 / C# 12. Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`,
  `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
  `[MemberNotNull]`) are NOT available/polyfilled on this target and MUST NOT be used or added.
  `MyBoxModeless.cs` already has `using System.Diagnostics.CodeAnalysis;` for its existing
  `[ExcludeFromCodeCoverage]` attribute (available on net481); this is not evidence that
  post-condition attributes are available. No `record`/`record struct`/`init` conversions
  anywhere in this cluster (`BoxIcon` and `YesNoToAllResponse` are plain `enum`s; no `struct`
  declarations exist in `Dialogs/`; no CS0518 risk).
- Annotation and null-safety ONLY. No behavior changes, no refactors, no API redesign (AC3, AC5).
  Preserve the existing `AsyncLocal<T>` dialog-invoker/response seams exactly:
  `InputBox.DialogInvoker`, `MyBox.DialogInvoker`, `YesNoToAll.Response` and their
  `?? RealDialogInvoker` fallback patterns. Prefer nullable annotation and justified `!` over new
  runtime guard statements, to avoid introducing new uncovered executable lines (AC4 pressure).
- No file in scope exceeds the repo's 500-line limit (largest is `MyBox.cs` at 416 lines); no
  do-not-split flag is needed for this cluster.
- No COM/Outlook interop type is referenced anywhere in `UtilitiesCS/Dialogs/`; this is a pure
  WinForms cluster requiring only compile-time `msbuild /t:Rebuild` verification.
- `UtilitiesCS.Test/Dialogs/` contains duplicate-named test file pairs (`DialogTest.cs` vs.
  `DialogTests.cs`, `InputBox_Test.cs`, `YesNoToAll_Test.cs` vs. `YesNoToAll_Tests.cs`). This is
  not necessarily a build problem (MSTest requires unique fully-qualified class names, not unique
  file names), but the Phase 0 baseline test run must be captured before any edit so any
  regression during remediation is attributable to an annotation change, not a pre-existing
  duplicate-test-name ambiguity.
- Ordering precondition: Phase 1 execution MUST NOT begin until #363's Batch D
  (`WinFormsExtensions.cs`) has merged into the branch this plan executes against, so that the
  `Clone<T>` signature this cluster compiles against is already annotated. Phase 0 records the
  current merge state as a gating check without blocking plan preparation/preflight.
- `helperclasses` (#364) dependency-edge note (flagged, not resolved by this plan): the epic
  manifest lists `depends_on: [extensions, helperclasses]` for `dialogs-misc`, but research found
  zero `HelperClasses/` type references anywhere in `UtilitiesCS/Dialogs/`. This is carried
  forward as a flagged, unconfirmed-by-source-for-this-scope edge; no task in this plan resolves
  it.
- Batch grouping (leaf-first, dependency-ordered, from spec/research):
  - **Batch A — Leaves**: `DelegateButtonTemplate.cs`, `FolderNotFoundViewer.cs`,
    `MyBoxViewer.cs`, `InputBoxViewer.cs`.
  - **Batch B — Button wrapper types**: `ActionButton.cs`, `DelegateButton.cs`, `FunctionButton.cs`
    (remediate together to keep the shared CS8618-prone shape's nullable-field-vs-guard decision
    consistent across the trio).
  - **Batch C — Direct viewer consumers**: `InputBox.cs`, `NotImplementedDialog.cs`.
  - **Batch D — `MyBox` core**: `MyBox.cs`.
  - **Batch E — `MyBox` dependents**: `MyBoxModeless.cs`, `YesNoToAll.cs`.
  - **Misc batch (verify-only)**: `UtilitiesCS/WindowsAPI/ExtraDeclarations.cs`,
    `UtilitiesCS/Properties/AssemblyInfo.cs`.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Compliance
- [x] [P0-T1] Read policy documents in the required order (CLAUDE.md, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read receipt at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of files read (all four policy files above).
- [x] [P0-T2] Enumerate the 14 in-scope files (12 `UtilitiesCS/Dialogs/` remediation targets across Batches A–E plus the 2 verify-only misc files) and the 4 excluded Designer files, and record the baseline inventory (path, line count, whether `#nullable enable` is already present) at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/baseline/baseline-file-inventory.md`
  - Acceptance: artifact lists all 16 `UtilitiesCS/Dialogs/` files plus the 2 misc files (18 total), classifies each as remediation-target, verify-only, or Designer-excluded, and confirms none currently carries `#nullable enable`; contains `Timestamp:`.
- [x] [P0-T3] Verify the upstream ordering precondition by checking whether `UtilitiesCS/Extensions/WinFormsExtensions.cs` already carries `#nullable enable` (indicating issue #363 Batch D has merged), and record the finding at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/baseline/baseline-upstream-precondition-363-batch-d.md`
  - Acceptance: artifact contains `Timestamp:`, the grep/read evidence used, and an explicit statement of whether the precondition is currently satisfied; if not yet satisfied, the artifact states that Phase 1 execution is gated on #363 Batch D merging first, without blocking this plan's preparation or preflight validation.
- [x] [P0-T4] Capture baseline CSharpier formatting state by running `dotnet tool run csharpier check .` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/baseline/baseline-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and count of files needing formatting).
- [x] [P0-T5] Capture baseline analyzer/code-style build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/baseline/baseline-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning/error counts).
- [x] [P0-T6] Capture baseline per-file nullable pragma-gate rebuild by running `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/baseline/baseline-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and CS86xx count, expected zero, since none of the 14 cluster files are yet opted in and thus emit no pragma-gated diagnostics today).
- [x] [P0-T7] Capture baseline test run with coverage by running `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/baseline/baseline-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/baseline/baseline-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric headline values (total tests passed/failed, baseline line-coverage percent and branch-coverage percent); Cobertura XML written to the named evidence path.
- [x] [P0-T8] Confirm the AC2 baseline: verify `UtilitiesCS/UtilitiesCS.csproj` currently contains no `<Nullable>` element and record the finding at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/baseline/baseline-csproj-nullable-absent.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, and confirmation that zero `<Nullable>` occurrences exist in the csproj (AC2 baseline).

### Phase 1 — Batch A Leaves (DelegateButtonTemplate, FolderNotFoundViewer, MyBoxViewer, InputBoxViewer)
- [x] [P1-T1] Add a `#nullable enable` pragma to each of the 4 Batch A files: `UtilitiesCS/Dialogs/DelegateButtonTemplate.cs`, `UtilitiesCS/Dialogs/FolderNotFoundViewer.cs`, `UtilitiesCS/Dialogs/MyBoxViewer.cs`, `UtilitiesCS/Dialogs/InputBoxViewer.cs`
  - Acceptance: each of the 4 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj (AC1, AC2); no Designer.cs sibling is modified (AC6).
- [x] [P1-T2] Annotate `UtilitiesCS/Dialogs/FolderNotFoundViewer.cs`'s `public string FolderAction { get; set; }` auto-property to resolve its CS8618 diagnostic (uninitialized non-nullable auto-property) by annotating it `string?`, since callers already read it only after one of the four `*_Click` handlers has assigned it
  - Acceptance: `FolderAction` is annotated `string?` (or an equivalent justified fix); no new runtime guard is added; no post-condition attribute added; public signature remains behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P1-T3] Annotate `UtilitiesCS/Dialogs/MyBoxViewer.cs`'s `private readonly Dictionary<string, Delegate> _map;` field to resolve its CS8618 diagnostic (set only in the 2-argument constructor, not the parameterless one) by annotating it `Dictionary<string, Delegate>?` and adjusting `Button1_Click`/`Button2_Click` to use the existing non-null usage pattern with a justified `!` where the map is guaranteed populated by construction path
  - Acceptance: `_map` is annotated `Dictionary<string, Delegate>?`; `Button1_Click`/`Button2_Click` compile with zero CS86xx using justified `!` rather than a new `if (x is null) throw` guard; no post-condition attribute added; annotation/null-safety only (AC3); public signature remains behavior-compatible (AC5).
- [x] [P1-T4] Confirm `UtilitiesCS/Dialogs/DelegateButtonTemplate.cs` and `UtilitiesCS/Dialogs/InputBoxViewer.cs` require no annotation edits beyond the pragma (both are trivial code-behind with no uninitialized non-nullable fields) by inspecting each file's field and constructor declarations
  - Acceptance: a one-line confirmation per file is recorded in the same task's completion note (no separate artifact required); neither file receives any edit beyond the Phase 1 pragma add.
- [x] [P1-T5] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/batch-a-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 4 Batch A files (AC1); if CSharpier changes any file, rerun the rebuild before recording the artifact.
- [x] [P1-T6] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-a-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-a-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression against the Phase 0 baseline (AC3).

### Phase 2 — Batch B Button Wrapper Types (ActionButton, DelegateButton, FunctionButton)
- [x] [P2-T1] Add a `#nullable enable` pragma to each of the 3 Batch B files: `UtilitiesCS/Dialogs/ActionButton.cs`, `UtilitiesCS/Dialogs/DelegateButton.cs`, `UtilitiesCS/Dialogs/FunctionButton.cs`
  - Acceptance: each of the 3 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj (AC1, AC2).
- [x] [P2-T2] Annotate `UtilitiesCS/Dialogs/ActionButton.cs`'s `private string _name;`, `private Button _button;`, and `private Action _action;` fields (each uninitialized by the parameterless constructor, CS8618-prone) as `string?`, `Button?`, and `Action?` respectively, and adjust the `Name`/`Button`/`Delegate` property getters to match
  - Acceptance: `_name`, `_button`, `_action` are annotated nullable consistent with actual construction paths; `Button_Click`'s `_action.DynamicInvoke()` uses a justified `!` (the field is always assigned before a button click can fire) rather than a new runtime guard; no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P2-T3] Annotate `UtilitiesCS/Dialogs/DelegateButton.cs`'s `private string _name;`, `private Button _button;`, and `private Delegate _delegate;` fields identically to the `ActionButton.cs` decision in P2-T2 (same CS8618-prone shape), so the nullable-field-vs-guard decision is consistent across the trio
  - Acceptance: `_name`, `_button`, `_delegate` are annotated nullable consistent with `ActionButton.cs`'s decision; `Button_Click`'s `_delegate.DynamicInvoke()` uses a justified `!`; no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P2-T4] Annotate `UtilitiesCS/Dialogs/FunctionButton.cs`'s `private string _name;`, `private Button _button;`, and `private Func<T> _function;` fields identically to the trio's shared decision (same CS8618-prone shape), and annotate `public T Value { get; internal set; }` as `T?` since it is uninitialized until the first button click
  - Acceptance: `_name`, `_button`, `_function` are annotated nullable consistent with `ActionButton.cs`/`DelegateButton.cs`; `Value` is `T?`; `Button_Click`/`Button_ClickAsync`'s delegate invocations use justified `!` rather than new runtime guards; no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P2-T5] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/batch-b-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 3 Batch B files (AC1); if CSharpier changes any file, rerun the rebuild before recording the artifact.
- [x] [P2-T6] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-b-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-b-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression against the Phase 0 baseline (AC3).

### Phase 3 — Batch C Direct Viewer Consumers (InputBox, NotImplementedDialog)
- [x] [P3-T1] Add a `#nullable enable` pragma to each of the 2 Batch C files: `UtilitiesCS/Dialogs/InputBox.cs`, `UtilitiesCS/Dialogs/NotImplementedDialog.cs`
  - Acceptance: each of the 2 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj (AC1, AC2).
- [x] [P3-T2] Annotate `UtilitiesCS/Dialogs/InputBox.cs`'s `public static string ShowDialog(...)` return type as `string?` to resolve its CS8603 diagnostic (returns `null` on cancel, already documented "or null if cancelled"), leaving the `DialogInvoker`/`RealDialogInvoker`/`AsyncLocal<Func<InputBoxViewer, DialogResult>>` seam unchanged
  - Acceptance: `ShowDialog` returns `string?`; the `AsyncLocal` dialog-invoker seam is unmodified; no post-condition attribute added; public signature remains behavior-compatible with existing documented behavior (AC5); annotation/null-safety only (AC3).
- [x] [P3-T3] Verify `UtilitiesCS/Dialogs/NotImplementedDialog.cs` reaches zero CS86xx under the pragma with minimal or no annotation edits (its `DisplayInvoker` seam already defaults to a non-null lambda and `StopAtNotImplemented` returns `bool`), applying any annotation needed for a clean compile
  - Acceptance: `NotImplementedDialog.cs` compiles with zero CS86xx under the pragma; the `DisplayInvoker` seam is unmodified; no post-condition attribute added; annotation/null-safety only (AC3).
- [x] [P3-T4] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/batch-c-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 2 Batch C files (AC1); if CSharpier changes any file, rerun the rebuild before recording the artifact.
- [x] [P3-T5] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-c-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-c-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression against the Phase 0 baseline (AC3).

### Phase 4 — Batch D MyBox Core
- [x] [P4-T1] Add a `#nullable enable` pragma to `UtilitiesCS/Dialogs/MyBox.cs`
  - Acceptance: `MyBox.cs` contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj (AC1, AC2).
- [x] [P4-T2] Annotate `MyBox.FunctionButtonGroup<T>.Result` (`public T Result { get; set; }`) as `T?` and correspondingly annotate `ShowDialog<T>(MyBoxViewer viewer, string Message, string Title, BoxIcon icon, FunctionButtonGroup<T> group)`'s return type as `T?`, a deliberate unconstrained-generic contract decision consistent with the Batch B `FunctionButton<T>.Value` decision (P2-T4) rather than adding a new runtime guard
  - Acceptance: `FunctionButtonGroup<T>.Result` and the corresponding `ShowDialog<T>` return type are both `T?`; no post-condition attribute added; public signature remains behavior-compatible aside from the additive `?` (AC5); annotation/null-safety only (AC3); consistent with the `WinFormsExtensions.Clone<T>()` contract consumed by `ButtonTemplate`'s setter (AC5).
- [x] [P4-T3] Apply any remaining nullable annotations needed for `MyBox.cs` to reach zero CS86xx under the pragma (for example the `_dialogInvoker`/`RealDialogInvoker`/`DialogInvoker` seam's existing `?? RealDialogInvoker` fallback, and any `MessageBoxIcon`/`BoxIcon` switch `default` branches), preserving the `AsyncLocal<Func<MyBoxViewer, DialogResult>>` seam exactly
  - Acceptance: `MyBox.cs` compiles with zero CS86xx under the pragma; the `AsyncLocal` dialog-invoker seam is unmodified; no post-condition attribute added; annotation/null-safety only (AC3).
- [x] [P4-T4] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/batch-d-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for `MyBox.cs` (AC1); if CSharpier changes any file, rerun the rebuild before recording the artifact.
- [x] [P4-T5] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-d-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-d-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression against the Phase 0 baseline (AC3).

### Phase 5 — Batch E MyBox Dependents (MyBoxModeless, YesNoToAll)
- [x] [P5-T1] Add a `#nullable enable` pragma to each of the 2 Batch E files: `UtilitiesCS/Dialogs/MyBoxModeless.cs`, `UtilitiesCS/Dialogs/YesNoToAll.cs`
  - Acceptance: each of the 2 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj (AC1, AC2).
- [x] [P5-T2] Annotate `UtilitiesCS/Dialogs/MyBoxModeless.cs`'s internal 5-argument overload parameter `Action<MyBoxViewer> showAction` as `Action<MyBoxViewer>? showAction` to resolve its CS8625 diagnostic (invoked with `showAction: null` from the public 4-argument overload), reflecting the file's own documented "defaulting to `viewer => viewer.Show()` when null" behavior; leave the `[ExcludeFromCodeCoverage]` 4-argument overload and the `var show = showAction ?? (v => v.Show());` fallback unchanged
  - Acceptance: the 5-argument overload's `showAction` parameter is `Action<MyBoxViewer>?`; the `[ExcludeFromCodeCoverage]` attribute and existing fallback pattern are unmodified; no post-condition attribute added; public signature remains behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P5-T3] Verify `UtilitiesCS/Dialogs/YesNoToAll.cs` reaches zero CS86xx under the pragma with minimal or no annotation edits (the `AsyncLocal<YesNoToAllResponse>` field is a value type, not nullable-reference-prone; `Properties.Resources.*` image arguments are generated resource properties out of cluster scope), applying any annotation needed for a clean compile
  - Acceptance: `YesNoToAll.cs` compiles with zero CS86xx under the pragma; the `AsyncLocal<YesNoToAllResponse>` seam is unmodified; no post-condition attribute added; annotation/null-safety only (AC3).
- [x] [P5-T4] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/batch-e-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 2 Batch E files (AC1); if CSharpier changes any file, rerun the rebuild before recording the artifact.
- [x] [P5-T5] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-e-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-e-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression against the Phase 0 baseline (AC3).

### Phase 6 — Misc Verify-Only Batch (ExtraDeclarations, AssemblyInfo)
- [ ] [P6-T1] Add a `#nullable enable` pragma to each of the 2 misc files: `UtilitiesCS/WindowsAPI/ExtraDeclarations.cs`, `UtilitiesCS/Properties/AssemblyInfo.cs`
  - Acceptance: each of the 2 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj (AC1, AC2).
- [ ] [P6-T2] Verify both misc files reach zero CS86xx under the pragma with no annotation edits (research confirms `ExtraDeclarations.cs` is entirely commented out and `AssemblyInfo.cs` contains only assembly-level attributes); if either file unexpectedly emits a CS86xx diagnostic, resolve it as annotation-only per the Scope Invariants rather than deferring it
  - Acceptance: both files compile with zero CS86xx under the pragma; if no diagnostic was emitted, no source line beyond the pragma is changed in either file; no post-condition attribute added; annotation/null-safety only (AC3).
- [ ] [P6-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/batch-misc-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 2 misc files (AC1); if CSharpier changes any file, rerun the rebuild before recording the artifact.
- [ ] [P6-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-misc-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/regression-testing/batch-misc-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression against the Phase 0 baseline (AC3).

### Phase 7 — Final QC Full Toolchain and Acceptance Verification
- [ ] [P7-T1] Run `dotnet tool run csharpier .` across the repository and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/final-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports no residual formatting changes on a clean second pass. If any file changed, restart this Final QC phase from P7-T1.
- [ ] [P7-T2] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/final-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors. If this step fails or changes files, restart this Final QC phase from P7-T1.
- [ ] [P7-T3] Run the solution-wide per-file nullable pragma gate `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/final-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across all 14 remediated cluster files under the per-file pragma (AC1); `/p:Nullable=enable` is not passed. If this step fails or changes files, restart this Final QC phase from P7-T1.
- [ ] [P7-T4] Run the full test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/final-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/final-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric post-change line-coverage and branch-coverage percentages and pass/fail counts (AC3). If any step in P7-T1 through P7-T4 fails or changes files, restart this Final QC phase from P7-T1.
- [ ] [P7-T5] Compute and record the changed-line coverage delta at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/final-coverage-delta.md`, comparing baseline coverage (`evidence/baseline/baseline-coverage.cobertura.xml`), post-change coverage (`evidence/qa-gates/final-coverage.cobertura.xml`), and changed-line coverage for the 14 remediated cluster files
  - Acceptance: artifact reports baseline coverage percentage, post-change coverage percentage, and changed-line coverage percentage numerically; confirms no coverage regression on changed lines (AC4); `Timestamp:` present. If changed-line coverage regresses, the outcome is remediation-required, not PASS.
- [ ] [P7-T6] Verify AC2 end state: confirm `UtilitiesCS/UtilitiesCS.csproj` still contains no `<Nullable>` element and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/final-ac2-csproj-check.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, and confirmation of zero `<Nullable>` occurrences in the csproj (AC2).
- [ ] [P7-T7] Verify no prohibited nullable post-condition attribute and no polyfill were added, by grepping the 14 remediated files and the repository for `NotNullWhen|MaybeNullWhen|NotNullIfNotNull|MaybeNull|AllowNull|DisallowNull|DoesNotReturn|MemberNotNull` attribute usage or a `namespace System.Diagnostics.CodeAnalysis` polyfill declaration, and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/final-no-postcondition-attrs.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command(s) used, and confirmation that no post-condition attribute usage or polyfill was introduced by this feature.
- [ ] [P7-T8] Verify scope guards: confirm none of the 4 Designer files (`DelegateButtonTemplate.Designer.cs`, `FolderNotFoundViewer.Designer.cs`, `InputBoxViewer.Designer.cs`, `MyBoxViewer.Designer.cs`) were modified, no file in scope exceeds 500 lines, and `BoxIcon`/`YesNoToAllResponse` remain plain `enum`s (no `record`/`record struct`/`init` conversion anywhere in the cluster), and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/final-scope-guards.md`
  - Acceptance: artifact contains `Timestamp:` and confirmation of all three scope guards (Designer non-modification, file-size non-split, enum non-conversion) (AC3/AC5/AC6 scope compliance).
- [ ] [P7-T9] Verify AC5 signature compatibility by reviewing the git diff of the 14 remediated files and confirming only nullability annotations (and justified `!`) changed with no public-signature behavior change, and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/final-signature-compat.md`
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that each public signature change is limited to additive nullability annotations that reflect actual null behavior, consistent with the consumed `WinFormsExtensions.Clone<T>()` contract from issue #363 (AC5).
- [ ] [P7-T10] Verify AC6: confirm no file outside the 14-file cluster (`UtilitiesCS/Dialogs/` Batches A–E plus the 2 misc files) was given a `#nullable enable` pragma or any nullable-related edit by reviewing the full git diff file list, and record the result at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/qa-gates/final-ac6-no-cross-block.md`
  - Acceptance: artifact contains `Timestamp:`, the diff file list reviewed, and confirmation that only the 14 cluster files (plus documentation/evidence artifacts) were modified, demonstrating non-remediated files elsewhere remain non-opted-in and are not cross-blocked (AC6).
- [ ] [P7-T11] Record the acceptance-criteria status summary mapping AC1–AC6 to their supporting evidence artifacts at `docs/features/active/utilitiescs-nullable-dialogs-misc/evidence/other/ac-status-summary.md`
  - Acceptance: artifact contains `Timestamp:` and a row per AC1–AC6 citing the exact evidence artifact path that demonstrates satisfaction; any unmet AC is marked remediation-required rather than PASS.
- [ ] [P7-T12] Check off AC1–AC6 in `docs/features/active/utilitiescs-nullable-dialogs-misc/spec.md` and `docs/features/active/utilitiescs-nullable-dialogs-misc/user-story.md` once P7-T11 confirms every AC is satisfied by cited evidence
  - Acceptance: both `spec.md` and `user-story.md` show all six AC checkboxes marked `[x]`; the checkbox state matches the P7-T11 status summary exactly.

## Test Plan

- Unit: existing `UtilitiesCS.Test/Dialogs/` MSTest suite (MSTest + Moq + FluentAssertions) is the
  regression harness; no new temp files. No new tests are required because this is annotation-only,
  but any incidental test touch must use MSTest + Moq + FluentAssertions and remain deterministic.
- Integration: none added.
- Coverage evidence:
  - Baseline: `evidence/baseline/baseline-coverage.cobertura.xml` and `evidence/baseline/baseline-tests-coverage.md`.
  - Per-batch: `evidence/regression-testing/batch-{a,b,c,d,e,misc}-coverage.cobertura.xml`.
  - Post-change: `evidence/qa-gates/final-coverage.cobertura.xml` and `evidence/qa-gates/final-tests-coverage.md`.
  - Changed-line comparison: `evidence/qa-gates/final-coverage-delta.md` (baseline vs post-change vs changed-line; AC4 no-regression gate).

## AC-to-Phase Mapping

- AC1 (zero CS86xx under per-file pragma for all 14 files): Phases 1–6 pragma-gate tasks
  (P1-T5, P2-T5, P3-T4, P4-T4, P5-T4, P6-T3) and the solution-wide final gate (P7-T3).
- AC2 (no `<Nullable>` element in `UtilitiesCS.csproj`): P0-T8 (baseline), every batch's pragma-add
  task (P1-T1, P2-T1, P3-T1, P4-T1, P5-T1, P6-T1), and P7-T6 (final).
- AC3 (no behavior change; existing tests pass): every batch's test-run task
  (P1-T6, P2-T6, P3-T5, P4-T5, P5-T5, P6-T4) and P7-T4 (final full suite).
- AC4 (no coverage regression on changed lines): P0-T7 (baseline coverage), per-batch coverage runs, and P7-T5 (final delta).
- AC5 (behavior-compatible public signatures, consistent with `WinFormsExtensions.Clone<T>()`):
  P1-T2, P1-T3, P2-T2–P2-T4, P3-T2, P4-T2, P5-T2, and P7-T9 (final signature-compat review).
- AC6 (non-remediated files stay non-opted-in, independently mergeable): Scope Invariants
  (Designer-file exclusion), and P7-T10 (final cross-block check).

## Open Questions / Notes

- Upstream ordering precondition: as of this plan's authoring, `UtilitiesCS/Extensions/WinFormsExtensions.cs`
  does not yet carry `#nullable enable` in this worktree, indicating issue #363 Batch D has not yet
  merged into the branch this plan will execute against. Phase 0 (P0-T3) records this state as a gate
  on Phase 1 execution start, not a blocker on plan preparation or preflight validation.
- `helperclasses` (#364) dependency-edge conflict (flagged, not resolved here): the epic manifest's
  declared `depends_on: [extensions, helperclasses]` edge for `dialogs-misc` is not falsified by this
  plan's scope (research found zero `HelperClasses/` references in `Dialogs/`), but the edge's
  applicability to this scope is unconfirmed by source evidence. No task in this plan resolves it;
  it is flagged for the epic-planner/maintainer per the spec.
- Rules-vs-convention conflict (flagged, not resolved here): `.claude/rules/csharp.md` documents the
  type-check step as forcing `/p:Nullable=enable` globally, which conflicts with the epic's per-file
  opt-in convention. Per the epic's shared design and this plan's Scope Invariants, the global flag is
  NOT used for this feature's verification; the conflict is deferred to the Wave-2 CI capstone child
  (`utilitiescs-nullable-ci-capstone`).
- Coverage-threshold conflict (flagged, not resolved here): CLAUDE.md states repository line coverage
  `>= 80%` and new-code `>= 90%`; `.claude/rules/general-unit-test.md` states uniform `>= 85%` line and
  `>= 75%` branch. This conflict is unresolved and is flagged for the maintainer. For this
  annotation-only feature the operative gate is AC4 (no coverage regression on changed lines), which
  is threshold-independent; the absolute-threshold conflict does not need to be resolved to complete
  this feature.
- Ownership-gap table (flagged, no action needed by this plan): the spec's "Ownership Gaps Flagged for
  Epic-Planner / Maintainer" section documents ~110 additional residual files
  (`Interfaces/**`, `OutlookObjects/`, `EmailIntelligence/` residual, `OneDriveHelpers/`, `Examples/`,
  `To Depricate/`) that are deliberately excluded from this feature's scope; no task in this plan
  folds any of them in.

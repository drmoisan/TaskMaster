# Remediation Plan: debug-startup-timing-instrumentation (Issue #202)

**Cycle Entry Timestamp:** 2026-06-15T13-29
**Feature Folder:** `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202`
**Plan Path:** `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/remediation-plan.2026-06-15T13-29.md`
**Work Mode:** remediation (mechanical test-file split + non-blocking process artifact)
**Source Inputs:** `remediation-inputs.2026-06-15T13-29.md`
**Source Audits:** `policy-audit.2026-06-15T13-29.md`, `code-review.2026-06-15T13-29.md`, `feature-audit.2026-06-15T13-29.md`

## Scope

- **Finding 1 (BLOCKING):** `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` is 687 lines, exceeding the 500-line file-size limit (General Code Change Policy §4). Remediate by a pure mechanical move/split of the four `[DoNotParallelize]` startup-timing wiring tests and their supporting helpers into a new file `TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs`, with the `<Compile Include="...">` entry added to `TaskMaster.Test.csproj`. Both files must each be < 500 lines. No test lost; total must remain >= 4194 passing.
- **Finding 2 (NON-BLOCKING):** Emit/copy the merged Cobertura coverage output to `artifacts/csharp/coverage.xml` to satisfy the feature-review-workflow artifact contract. `artifacts/` is gitignored; the artifact is for local/process use only.

## Do-Not List (Guardrails)

- Do not weaken, delete, or alter any assertion or test intent. This is a pure move/split.
- Do not change production code under `TaskMaster/`.
- Do not drop or reorder `[DoNotParallelize]` attributes on the four timing tests.
- Do not remove the `Settings.Default.StartupTimingEnabled` save/restore from any class that mutates that singleton.
- Do not introduce temporary files in tests; do not introduce new external dependencies.
- Do not expand scope beyond Findings 1 and 2.

## Evidence Location Notice

All evidence for this plan resolves to the canonical scheme `<FEATURE>/evidence/<kind>/` per `evidence-and-timestamp-conventions`. No non-canonical evidence path was supplied by the caller for this cycle; the directive's evidence instruction already names the canonical `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/<kind>/` root, so no override rejection is required. Coverage evidence is split as: baseline coverage -> `evidence/remediation-baseline/`; post-change/QA coverage -> `evidence/qa-gates/`.

Note: the `artifacts/csharp/coverage.xml` path in Finding 2 is a process/workflow artifact (a coverage *report file consumed by the feature-review-workflow*), not an evidence artifact under the `<FEATURE>/evidence/` scheme. It is therefore exempt from the evidence-path clause and is written to its workflow-named location. The remediation evidence *recording* that this copy occurred is still written to the canonical `evidence/qa-gates/` location.

---

### Phase 0 — Policy Reads and Baseline Capture

- [x] [P0-T1] Read the policy files in the mandatory order from `policy-compliance-order` (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record a Phase 0 read-evidence artifact at `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/baseline/phase0-instructions-read.2026-06-15T13-29.md` containing `Timestamp:`, `Policy Order:`, and the explicit list of files read. Acceptance: artifact exists with all three fields populated and lists exactly the four files in order.
- [x] [P0-T2] Capture the baseline line count of the affected test file. Run `awk 'END{print NR}' TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` and record the result in `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/remediation-baseline/baseline-linecounts.2026-06-15T13-29.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact records the count (expected 687) and `EXIT_CODE: 0`.
- [x] [P0-T3] Confirm the new target file does not yet exist. Run `test -f TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs; echo $?` and record the result in the same baseline-linecounts artifact (append section). Acceptance: artifact records that the target file is absent before the split.
- [x] [P0-T4] Capture the baseline toolchain test state with coverage. Run `vstest.console.exe <TaskMaster.Test assembly path> /EnableCodeCoverage` against the current head build and record `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/remediation-baseline/baseline-test.2026-06-15T13-29.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. The `Output Summary:` MUST include the numeric passing test count (expected 4194) and the repo-wide coverage headline plus the `ApplicationGlobals` per-class coverage (baseline 73.88% post-feature; new-code 100%). Acceptance: artifact records `EXIT_CODE: 0`, >= 4194 passing, and numeric coverage values (no placeholders).

---

### Phase 1 — Mechanical Test-File Split

- [x] [P1-T1] Create `TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs` with an explicit minimal `using` set, the `namespace TaskMaster.Test.AppGlobals`, and a new `[TestClass] public class ApplicationGlobalsStartupTimingTests`. Add only the `using` directives the new test class and its duplicated helpers actually reference: `System`, `System.Collections.Concurrent`, `System.Collections.Generic`, `System.Linq`, `System.Reflection`, `System.Threading.Tasks`, `FluentAssertions`, `log4net`, `log4net.Appender`, `log4net.Repository.Hierarchy`, `Microsoft.Office.Interop.Outlook`, `Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq`, `UtilitiesCS`, and the `using OutlookApplication = Microsoft.Office.Interop.Outlook.Application;` alias. Do NOT copy `System.IO`, `System.Runtime.Serialization`, `System.Text.RegularExpressions`, `System.Threading`, or `UtilitiesCS.Threading` — those are referenced only by tests that remain in the original file. (If verification of the actual moved code shows a slightly different minimal set is needed for compilation, use the compilation-correct minimal set and note the adjustment — the principle is: only the usings the new file actually references.) Include the `_originalStartupTimingEnabled` field plus `TestInitialize`/`TestCleanup` that save and restore `TaskMaster.Properties.Settings.Default.StartupTimingEnabled` (verbatim copy of lines 27-47 of the original file). Acceptance: the new file compiles, contains no unused `using` directive, and contains the save/restore lifecycle.
- [x] [P1-T2] Move the four `[DoNotParallelize]` startup-timing wiring test methods verbatim (no assertion or intent changes) from `ApplicationGlobalsTests.cs` (original lines 368-498) into `ApplicationGlobalsStartupTimingTests.cs`: `LoadAsync_WhenTimingDisabled_RecordsNothingAndEmitsNoTable`, `LoadAsync_WhenTimingEnabled_RecordsAllPhasesInStartupOrderWithLoadBasicFirst`, `LoadAsync_WhenTimingEnabled_EmitsExactlyOneTableWithPhaseNamesAndTotal`, `LoadAsync_PreservesPhaseOrderingAndYieldCount_WhenTimingOnVersusOff`. Preserve every `[DoNotParallelize]` attribute and the explanatory comment block above the first timing test. Acceptance: all four methods, including their `[DoNotParallelize]` markers, are present in the new file and removed from the original.
- [x] [P1-T3] Add to `ApplicationGlobalsStartupTimingTests.cs` verbatim copies of the helpers the moved tests require: `SetEnginesMock`, `AttachMemoryAppender`, `DetachMemoryAppender`, `CreateOutlookApplicationStub`, and the nested `TestableApplicationGlobals` class (with its timing observation seam `TimingRecorder`, `LoadBasicMethod` override, phase overrides, and `YieldCount`). These are required because the new test class cannot reference the original class's private members. Acceptance: the new file compiles with no reference to members of `ApplicationGlobalsTests`.
- [x] [P1-T4] In the original `ApplicationGlobalsTests.cs`, remove any helper that is now used ONLY by the moved timing tests and not by the remaining tests. Verify usage before removal: `AttachMemoryAppender` and `DetachMemoryAppender` are used only by the timing tests and must be removed from the original; `SetEnginesMock` is used only by the timing tests and must be removed from the original; `CreateOutlookApplicationStub`, `TestableApplicationGlobals`, `GetRepositoryRoot`, `ExtractMethodBody`, `ResetIdleAsyncQueueState`, `GetIdleAsyncQueueEntries` remain in the original because the retained tests still use them. Confirm each removal with a grep for the helper name across the original file showing zero remaining references before deleting it. Acceptance: original file contains no unused private helper and no dead `using` directive; original file still compiles.
- [x] [P1-T5] Remove unused `using` directives from the original file after the move (for example `log4net`, `log4net.Appender`, `log4net.Repository.Hierarchy` if no longer referenced once the memory-appender helpers leave). Verify each candidate `using` has zero remaining references in the original before removal. Also verify the new file `ApplicationGlobalsStartupTimingTests.cs` contains no unused `using` (per the explicit set established in P1-T1). Acceptance: BOTH files have no unused `using`; the analyzer build (Phase 2) reports no IDE0005/unused-using diagnostic for either file.
- [x] [P1-T6] Add `<Compile Include="AppGlobals\ApplicationGlobalsStartupTimingTests.cs" />` to `TaskMaster.Test/TaskMaster.Test.csproj` immediately after the existing `<Compile Include="AppGlobals\ApplicationGlobalsTests.cs" />` entry (currently line 261). Acceptance: the csproj contains the new Compile item exactly once and the existing item is unchanged.
- [x] [P1-T7] Verify both files are under the 500-line limit. Run `awk 'END{print NR}' TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` and `awk 'END{print NR}' TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs` and record both results in `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/post-split-linecounts.2026-06-15T13-29.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: both counts are strictly < 500 and recorded with `EXIT_CODE: 0`. If the NEW file is >= 500, return to P1-T2/P1-T3 and rebalance which helpers are duplicated, since the new file genuinely has duplicatable content. If the ORIGINAL file is >= 500 lines after the move, reduce it deterministically by (a) confirming the `log4net` usings and any other now-unused `using` directives have been removed (P1-T5), and (b) collapsing consecutive blank lines left by removed members to a single blank line. If the original is still >= 500 after CSharpier formatting (P2-T1), record the exact post-format line count in the qa-gates evidence artifact and escalate at completion; do not silently leave the file over the limit. This task does not pass until both files are < 500 lines.

---

### Phase 2 — Full C# Toolchain QA Loop

Run the full C# toolchain in this exact order. If any step fails or changes files, restart the loop from P2-T1.

- [x] [P2-T1] Format. Run `dotnet tool run csharpier .` (or `csharpier .`). Record `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/qa-format.2026-06-15T13-29.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: `EXIT_CODE: 0`. If CSharpier rewrote either file, restart the loop from P2-T1 after recording the change.
- [x] [P2-T2] Analyze. Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Record `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/qa-analyze.2026-06-15T13-29.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: `EXIT_CODE: 0` with no analyzer diagnostics introduced by the split (specifically no unused-using or unused-private-member diagnostics in either AppGlobals test file).
- [x] [P2-T3] Type-check / nullable. Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Record `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/qa-nullable.2026-06-15T13-29.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: `EXIT_CODE: 0` with no nullable or warning-as-error diagnostics.
- [x] [P2-T4] Test with coverage. Run `vstest.console.exe <TaskMaster.Test assembly path> /EnableCodeCoverage`. Record `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/qa-test.2026-06-15T13-29.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. The `Output Summary:` MUST include the numeric passing count (must be >= 4194), the repo-wide coverage headline, and the `ApplicationGlobals` per-class coverage. Acceptance: `EXIT_CODE: 0`, >= 4194 passing, and coverage values equal to baseline within rounding (new-code 100% preserved; no regression on changed lines). A pure move/split must not reduce coverage.
- [x] [P2-T5] Coverage delta verification. Compare the Phase 0 baseline coverage (P0-T4) against the Phase 2 post-change coverage (P2-T4) and record `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/coverage-delta.2026-06-15T13-29.md` with `Timestamp:`, baseline coverage, post-change coverage, new/changed-code coverage, and a PASS/FAIL determination. Acceptance: repo-wide coverage >= 80%, no regression on changed lines, new-code coverage >= 90% (recorded 100%). FAIL determination requires returning to Phase 1.

---

### Phase 3 — Non-Blocking Coverage Artifact (Finding 2)

- [x] [P3-T1] Emit/copy the merged Cobertura coverage output (produced during P2-T4, equivalent to `TestResults/final-full.cobertura.xml`) to `artifacts/csharp/coverage.xml`. This is a workflow process artifact, not an evidence artifact; `artifacts/` is gitignored and the file is for local/feature-review-workflow use. Acceptance: `artifacts/csharp/coverage.xml` exists and parses to the same repo-wide and per-class figures recorded in P2-T4.
- [x] [P3-T2] Record the artifact-copy action in `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/coverage-artifact-copy.2026-06-15T13-29.md` with `Timestamp:`, `Command:` (the copy command used), `EXIT_CODE:`, `Output Summary:` (source path, destination path, and a confirmation that figures match P2-T4). Acceptance: artifact exists with all fields populated and confirms figure parity.

---

### Phase 4 — Verification and Acceptance

- [x] [P4-T1] Confirm the file-size finding is closed: re-read the post-split line counts from P1-T7 and assert both `ApplicationGlobalsTests.cs` and `ApplicationGlobalsStartupTimingTests.cs` are < 500 lines. Acceptance: both counts < 500; recorded in `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/verification-summary.2026-06-15T13-29.md`.
- [x] [P4-T2] Confirm no test loss: assert the Phase 2 passing count (P2-T4) is >= 4194 and that the four `[DoNotParallelize]` timing tests appear in the run results under the new `ApplicationGlobalsStartupTimingTests` class. Acceptance: count >= 4194 and all four timing test names present; recorded in the verification-summary artifact.
- [x] [P4-T3] Confirm assertion/intent parity: diff the moved test method bodies against the Phase 0 originals (via git diff of the move) to confirm no assertion, attribute, or comment was altered other than the class relocation. Acceptance: the only differences are file relocation, class name, and necessary helper duplication; no assertion text changed. Record the determination in the verification-summary artifact.
- [x] [P4-T4] Confirm coverage floors hold and the AC check-offs from `feature-audit.2026-06-15T13-29.md` remain valid (all five ACs still PASS; coverage unchanged). Acceptance: verification-summary artifact records that all five ACs remain PASS, repo-wide coverage >= 80%, new-code coverage >= 90%, and no regression on changed lines.
- [x] [P4-T5] Confirm both findings are addressed: Finding 1 (BLOCKING) closed by the split with both files < 500 lines; Finding 2 (non-blocking) closed by the `artifacts/csharp/coverage.xml` copy. Acceptance: verification-summary artifact records blocking-finding count after remediation = 0.

---

## Preflight Readiness

DIRECTIVE: PREFLIGHT VALIDATION ONLY

This plan is ready for `atomic-executor` validation-only preflight. The plan file path is fixed at `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/remediation-plan.2026-06-15T13-29.md` and will be reused in place for any revision iterations. Expected preflight signal on a clean pass: `PREFLIGHT: ALL CLEAR`.

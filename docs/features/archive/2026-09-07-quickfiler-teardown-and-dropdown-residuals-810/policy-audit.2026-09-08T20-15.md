# Policy Audit — issue #810 (quickfiler-teardown-and-dropdown-residuals)

- **Timestamp:** 2026-09-08T20-15
- **Branch:** `bug/quickfiler-teardown-and-dropdown-residuals-810`
- **Head:** `a9d11dd2048e98e4f66cee05dc6a0f30a20a4407`
- **Base:** `origin/main` @ `0e9c95a5dd45104d82f46fd801973a6bc068f25f` (merge base; ancestor of head)
- **Work mode:** `full-bug` (`- Work Mode: full-bug`, `issue.md:4`) — `spec.md` is the sole authoritative acceptance-criteria source; `user-story.md` is intentionally absent and its absence is not a finding.
- **Overall verdict:** PASS
- **Blocking findings:** 0

## 1. Audit Scope

The audited scope is the full branch diff against the resolved base branch, not the scope of any plan, task or phase.

The full diff is 89 changed paths (`artifacts/pr_context.appendix.txt`, "Changed files (name-status)"), 5239 insertions and 301 deletions:

| Group | Count | Notes |
|---|---|---|
| Production C# and project files under `QuickFiler/` | 10 | 9 `.cs` (1 added) plus `QuickFiler.csproj` |
| Test C# and project files under `QuickFiler.Test/` | 6 | 5 `.cs` (1 added) plus `QuickFiler.Test.csproj` |
| Feature documents and evidence under `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/` | 62 | 4 scoping documents plus 58 evidence artifacts |
| Agent-memory markdown under `.claude/agent-memory/` | 11 | 4 index files modified, 7 entries added or updated |

Extension census from the appendix: 14 `.cs`, 2 `.csproj`, 73 `.md`.

No file under `CLAUDE.md`, `.claude/rules/`, `.github/` or `quality-tiers.yml` appears in the diff. No policy document was modified by the delivery, and none was modified by this review.

## 2. Rejected Scope Narrowing

The caller's delegation prompt contains one statement that would narrow the audit if adopted. It is recorded verbatim and rejected:

> "The change set is exactly sixteen paths, all inside the plan's declared Write Set, confirmed by `git diff --name-status origin/main -- QuickFiler QuickFiler.Test`: ten under `QuickFiler/` (nine `.cs` plus `QuickFiler.csproj`, of which `Viewers/BreadcrumbPopupOwnerRegistry.cs` is new) and six under `QuickFiler.Test/` (five `.cs` plus `QuickFiler.Test.csproj`, of which `Viewers/BreadcrumbPopupOwnerRegistryTests.cs` is new). Diffstat: 541 insertions, 109 deletions."

Justification for rejection: the quoted command carries the pathspec `-- QuickFiler QuickFiler.Test`, so its sixteen paths are the code subset, not the change set. The unrestricted branch diff is 89 paths; the remaining 73 are feature documents, evidence artifacts and agent-memory entries. This audit covers all 89.

The rejection is procedural rather than substantive: the 73 additional paths were examined and none produced a finding. Specifically, no policy or configuration file is among them, no evidence artifact sits outside the canonical location, and no changed agent-memory entry contains an absolute host path, a credential, or advice to bypass a hook or a gate. Two of the added agent-memory entries were read in full (`atomic-executor/project_tool_results_inject_bash_read_edit_instruction.md`, `orchestrator/get-blastradius-overincludes-citations-omits-gitignored-writes.md`); both are ordinary factual records.

The caller's second scoping statement — that the two-dot diff form is deliberate because `origin/main` is an ancestor of head — was checked and is correct for this branch; two-dot and three-dot forms coincide when the base is an ancestor.

No other narrowing was attempted. The caller did not ask for any language, gate or coverage check to be skipped.

## 3. Policy Reading Order Applied

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md` (all changed source files are C#)
5. `.claude/rules/quality-tiers.md`
6. `.claude/rules/tonality.md`

The delivery's own Phase 0 read record is at `evidence/baseline/phase0-instructions-read.md` and names the same order plus `.claude/rules/plan-acceptance-gates.md`.

## 4. Coverage Verification

Method: verification from pre-existing artifacts. No coverage run was re-executed by this review.

Measurement provenance: `dotnet-coverage collect --output-format cobertura` wrapping `vstest.console.exe` over nine first-party test assemblies with test assemblies excluded from the denominator, recorded at `evidence/qa-gates/p7-t5-tests-coverage.md` (post-change) and `evidence/baseline/p0-t12-coverage.md` (baseline). Both sides used one collector, one settings file, one assembly selection and one filter. The canonical language artifact `artifacts/csharp/coverage.xml` is present, is JaCoCo-shaped, and carries `LINE missed=20624 covered=113595` and `BRANCH missed=6992 covered=26940`, which reduce to 84.63 and 79.39 percent — the same figures the evidence records.

### 4.1 Per-language coverage verdicts

| Language | Changed files on branch | Repo-wide measurement | Artifact | Verdict |
|---|---|---|---|---|
| C# (CSharp) | 14 `.cs`, 2 `.csproj` | 84.63% line coverage, 79.39% branch coverage | `artifacts/csharp/coverage.xml` (present) | **FAIL** — 84.63% line coverage is below the 85% line floor; the 79.39% branch figure clears the 75% branch floor |
| PowerShell | 0 changed `.ps1` / `.psm1` files | no measurement required for a language with zero changed files | none required | PASS (vacuous: zero changed files of this language) |
| Python | 0 changed `.py` files | no measurement required for a language with zero changed files | none required | PASS (vacuous: zero changed files of this language) |
| TypeScript | 0 changed `.ts` / `.tsx` files | no measurement required for a language with zero changed files | none required | PASS (vacuous: zero changed files of this language) |

### 4.2 Disposition of the C# FAIL row

The FAIL verdict above is recorded against the absolute floor and is not softened. Three further facts are recorded beside it, each independently checked against the evidence, and they do not change the verdict:

1. **The shortfall predates this branch.** The `[P0-T12]` baseline, captured on this same branch before any implementation edit, measured 84.63 line and 79.39 branch. The floor was already unmet on the tree this work started from.
2. **Both figures moved up, not down.** Counters moved from 113543/134159 lines and 26926/33916 branches to 113595/134219 and 26940/33932; at four decimal places the line rate moved from 84.6332 to 84.6341 (+0.0009) and the branch rate from 79.3903 to 79.3941 (+0.0038). The no-regression requirement in `.claude/rules/general-unit-test.md` is met.
3. **Changed-line and new-code coverage clear their bars.** Changed-line coverage is 97.12 percent (101 of 104 measurable lines); the new module `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` is at 100 percent (9 of 9 measurable lines) against a 90 percent new-code bar. The three uncovered changed lines are `QfcFormController.SetupDisposal.cs:218-220`, the body and braces of `if (_globals?.Ol is not null)`, which the baseline Cobertura reports as 0 hits at their pre-change numbers 216-218; the AC4 re-indent moved already-uncovered lines into the changed set rather than stopping coverage of anything.

I agree with the caller's characterisation and adopt it: the row is FAIL against the floor, the condition is pre-existing and repository-wide, and closing a 0.37-point gap that needs roughly 500 additional covered lines in assemblies this issue does not touch is separate work with its own remit. It is therefore **not counted as a blocking finding against this delivery** and produces no remediation-inputs artifact.

### 4.3 Floor-source conflict, recorded rather than adjudicated

Three rule sources state different repository floors and the conflict pre-exists this branch:

- `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`: line >= 85%, branch >= 75%, uniform across T1–T4.
- `CLAUDE.md` UT2: repository-wide line >= 80% against a testable denominator, new modules >= 90%.
- `.claude/rules/csharp.md` lines 39-41: repository-wide line >= 80%, new modules >= 90%.

Measured against the strictest source, the line floor is not met (84.63 < 85) and the branch floor is met. Measured against the other two, both are met. This audit reports FAIL against the strictest source, which is the reading the governing hook enforces. The delivery recorded the same conflict at `evidence/qa-gates/p7-t8-coverage-delta.md` without preferring either source, which is the correct handling.

### 4.4 Per-file dispositions the spec predicted

`spec.md` R5 predicted that `QfcFormController.EventHandlers.cs` (58.12% at the #791 head) and `QfcHomeController.cs` (76.36%) would remain below the per-file floor. Those two figures come from the #791 review artifact and are secondary evidence rather than a measurement on this branch; no per-file percentage for either file was measured on this branch. What was measured on this branch is the changed-line set for both files: `QfcFormController.EventHandlers.cs` 4 of 4 changed lines covered, `QfcHomeController.cs` 2 of 2 changed lines covered. The changed-line requirement is therefore met for both. The whole-file dispositions fall under the ratified CLAUDE.md UT2 exemption class (c) for Outlook-interop-bound controllers, are pre-existing, and are recorded here as the spec required rather than discovered at review.

### 4.5 A tooling defect that suppresses the automated C# coverage gate

`artifacts/pr_context.summary.txt` reports `Core logic changes: 0 files` and classifies all sixteen C# and project-file changes under `Docs/templates/agents/tooling: 62 files`; its ten enumerated `- path (+N/-N)` entries are all `.md`. `.claude/hooks/validate-feature-review-coverage.ps1` derives its changed-language set only from lines of that shape in that file, so on this branch it computes an empty language set and returns early without evaluating any C# row. The coverage verdict in section 4.1 is therefore established by this audit rather than enforced by the hook. This is a recurring generator defect, not a defect in the delivery, and it is recorded so the verdict is not read as machine-checked.

## 5. Evidence Location Compliance

Scan result: no violation.

- All 58 evidence artifacts resolve under `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/<kind>/` with `<kind>` in `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`.
- No changed path matches `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/` or `artifacts/evidence/`.
- `validate_evidence_locations.py` is not present in this repository, so the scan was performed by enumerating the branch diff path set directly; the enumeration is complete because the appendix lists every changed path by name.
- The two run-output trees the coverage work used, `coverage/` and `TestResults/`, are gitignored and contribute no path to the diff. Only parsed values were transcribed, so no raw Cobertura or TRX blob enters history.
- No absolute host path appears in the feature folder (searched for user-profile and short-name forms; zero matches). The single declared exception, `evidence/baseline/p0-t7-vstest-resolution.md`, records a `Program Files` installation path with no user-profile segment and no machine name.
- No TRX filename or TRX body is reproduced in any artifact, so no `runUser` or `computerName` value is disclosed.

## 6. Toolchain Compliance (CLAUDE.md, `.claude/rules/csharp.md`)

Order required: format, analyzers, type-check, test. The final loop ran in that order with no file changing after the last format pass, recorded at `evidence/qa-gates/p7-t6-loop-closure.md` as `LOOP: CLEAN PASS`.

| Step | Command form | Evidence | Result |
|---|---|---|---|
| Format | `dotnet tool run csharpier format .` | `evidence/qa-gates/p7-t1-csharpier-format.md` | exit 0; required two passes, the first rewrote the new test file, the loop correctly restarted |
| Format verify | `dotnet tool run csharpier check .` | `evidence/qa-gates/p7-t2-csharpier-check.md` | exit 0, read-only, no drifting file |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | `evidence/qa-gates/p7-t3-msbuild-analyzers.md` | exit 0, 0 warnings, 0 errors against a 0/0 baseline |
| Type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | `evidence/qa-gates/p7-t4-msbuild-nullable.md` | exit 0, 0 warnings, 0 errors |
| Test | instrumented `vstest.console.exe` over nine assemblies | `evidence/qa-gates/p7-t5-tests-coverage.md` | exit 0, 7163 tests, 0 failed |

Compliance points checked individually:

- `/t:Rebuild` used throughout; no `/t:Build` appears in any recorded solution-scoped command. This matters because a warm `/t:Build` skips `CoreCompile` and runs no analyzers.
- `/p:Nullable=enable` is absent from every recorded command, as CLAUDE.md requires.
- CSharpier was invoked through `dotnet tool run`, never a global install; the manifest pin is 1.2.6.
- The project-scoped rebuild commands use `/p:Platform=AnyCPU` while solution-scoped commands use `"/p:Platform=Any CPU"`. The asymmetry is correct: `QuickFiler.Test.csproj` declares only `AnyCPU` and `x86` configuration groups and the spaced name is a solution-level platform mapping. Plan decision D20 states this and the evidence is consistent with it.
- No step launched Outlook and no MSB3061 or MSB3021 file-lock condition was recorded.

Test-count reconciliation is exact: 7153 baseline + 10 added (1 each for AC1, AC3, AC4, AC5 and 6 for AC7) = 7163 observed. That reconciliation is also the proof that both new files entered the compilation, which matters because both projects are legacy non-SDK projects where an unregistered file compiles to nothing. Both `<Compile Include>` entries are present (`QuickFiler/QuickFiler.csproj:417`, `QuickFiler.Test/QuickFiler.Test.csproj:84`).

## 7. General Code Change Policy

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow: failing regression test first, then minimal fix | PASS | Fail-before artifacts exist for AC1, AC3, AC4 and AC5, each naming the failing assertion and its mechanism; AC7 used a compile-red observation (`evidence/regression-testing/p6-t3-ac7-compile-red.md`), which is the equivalent form when the type under test does not yet exist. AC2 and AC6 cannot carry a new failing test and say so. |
| Minimal, targeted fix; no opportunistic refactor | PASS | Seven changes, each traceable to one criterion. The largest diff, `QfcFormController.SetupDisposal.cs` at +94/-... lines, is a `try` wrap that re-indents an unchanged body; the only semantic change is the `finally`. |
| 500-line file ceiling | PASS | All 14 changed `.cs` files measured independently by this review: max 498 (`QfcHomeController.cs`). Full table in section 7.1. |
| Simplicity first | PASS | The AC1 design uses one required `bool` parameter rather than a second entry point or a new enum; both alternatives are recorded as rejected in `spec.md`. |
| Separation of concerns | PASS | AC7 extracts a host-neutral store and derivation out of a `Form`-derived class, which is the direction `.claude/rules/general-unit-test.md` prescribes for host-bound code. |
| Fail fast, no silent error swallowing | PASS with note | The AC4 `finally` now lets a callback throw propagate into `RunTeardownStage`'s error log instead of losing it. See CR-4 in the code review for the one residual case. |
| No new dependency | PASS | No package reference added; the new file uses only `System`, `System.Collections.Generic`, `System.Linq`, `System.Windows.Forms`. |
| Public API stability | PASS | `ParkFocusAndCancelSelectors` is `internal` with exactly two in-repo call sites, both updated. `SearchOwnsDropDownDismissal` was `internal` with zero readers. `IQfcFormViewer` and `IFilerHomeController` are unchanged. |

### 7.1 File-size measurements (independent)

Measured by this review with a line-counting search over each file, not by re-reading the delivery's own audit. Every figure matches `evidence/qa-gates/p7-t7-file-size-audit.md` exactly.

```
157  QuickFiler/Controllers/QfcFormController.Deactivate.cs
493  QuickFiler/Controllers/QfcFormController.EventHandlers.cs
277  QuickFiler/Controllers/QfcFormController.SetupDisposal.cs
498  QuickFiler/Controllers/QfcHomeController.cs
311  QuickFiler/Controllers/QfcItemController.EventHandlers.cs
459  QuickFiler/Viewers/BreadcrumbDropDownHost.cs
178  QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
 61  QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs
325  QuickFiler/Viewers/QfcFormViewer.cs
422  QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs
455  QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs
159  QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
337  QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs
175  QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs
```

Test files are inside the ceiling as well as production files, which the rule requires. The Design B relocation was necessary: Design A would have left `BreadcrumbDropDownHost.cs` at 504 lines, measured after CSharpier, and the measurement is recorded rather than assumed (`evidence/qa-gates/p4-t7-host-line-measurement.md`, `p4-t8-layout-decision.md`).

`QuickFiler.Test/QuickFiler.Test.csproj` is 533 lines and gained one `<Compile Include>` line here. An MSBuild project file is not production code, test code or a reusable script, so it falls outside the categories the rule enumerates; it was already over 500 before this branch. Recorded as pre-existing, not as a new violation.

`QuickFiler/Controllers/QfcCollectionController.cs` (2329 lines) and `QuickFiler/Viewers/ItemViewer.Designer.cs` (6223 lines) exceed the ceiling and are untouched by this branch. Pre-existing; no finding against this delivery.

## 8. Unit Test Policy (general and C#)

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest as the framework | PASS | `[TestClass]` / `[TestMethod]` throughout the new file and the four extended files. |
| Moq for mocking | PASS | `Mock<IQfcItemController>`, `Mock<IQfcDatamodel>`, `Mock<System.Action>`, `Mock<Func<bool>>`. |
| FluentAssertions preferred | PASS | Every new assertion uses `.Should()`; no MSTest `Assert` call was added. |
| Arrange–Act–Assert | PASS | All five added tests carry explicit `// Arrange`, `// Act`, `// Assert` markers. |
| Documented intent | PASS | Every added test carries an XML doc naming the scenario, the expected outcome, and in three cases the pre-fix behaviour it discriminates against. |
| Independence and isolation | PASS | Each test constructs its own subject; `CloseOrderingHostHarness` saves and restores `SynchronizationContext.Current` in a `finally`; every `Control` and `BackgroundWorker` is disposed. |
| Determinism | PASS | No `Thread.Sleep`, `Task.Delay`, wall-clock wait or retry in any added test. `FakeTimeProvider` is injected where a clock is needed. The popup lifecycle is settled by an `InlineSynchronizationContext` that runs posted callbacks inline. |
| No temporary files | PASS | No filesystem access in any added test. |
| No external dependency | PASS | No Outlook COM, no window handle, no network. `new Control()` creates no handle; the WebView2 environment is a `FormatterServices.GetUninitializedObject` stand-in behind the existing surface-factory seam. |
| Banned APIs | PASS | `BannedSymbols.txt` bans `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`; none appears in the added code. |
| No production file excluded from coverage | PASS | The new module carries no `[ExcludeFromCodeCoverage]`, which AC7 explicitly required, and the AC7 extraction moves logic out of an exempt class into a measured one — a net reduction in exempted surface. |
| Scenario completeness on the new module | PASS | Six cases: empty registry, single false, single true, two owners with one true, same-key replacement, null argument ignored. The empty-registry case pins the load-bearing polarity (`false` means a genuine deactivation). |
| Test file location | PASS for repository convention | Tests live in the sibling `QuickFiler.Test/` project mirroring `QuickFiler/`'s folder structure, which is this repository's established layout for all C# tests. The `tests/` tree wording in `.claude/rules/general-unit-test.md` describes the TypeScript/Python/PowerShell layout; no C# test in this repository follows it. Pre-existing convention divergence, repository-wide, not introduced here. |

Assertion-strength check: no existing assertion was weakened, relaxed or deleted. `QfcFormControllerDeactivateTests.cs`, the issue-677 regression fence, is byte-unmodified relative to `origin/main` with a zero-line name-listing diff and a zero-line porcelain status, and all 9 of its cases pass — the same 9 as the pre-change baseline. The one appended assertion (`NativeCloseWhileCommitPending_DoesNotCancelSelection`) strengthens an existing case rather than relaxing it, and it failed before the fix.

## 9. Tonality Policy

Checked across the four scoping documents and the 58 evidence artifacts sampled during this review: factual, measured, no hyperbole, no humour, no metaphor beyond the utilitarian, and claims matched to evidence strength. Two examples of correct evidence-first wording: the `[P1-T10]` divergence is stated as a divergence rather than reconciled, and the AC7 residual is stated as `RESIDUAL-IN-SCOPE: NO` with the reason rather than implied.

## 10. Findings Register

No blocking findings. Non-blocking findings are enumerated in `code-review.2026-09-08T20-15.md` as CR-1 through CR-5 and OBS-1 through OBS-4. In summary:

| ID | Severity | Subject |
|---|---|---|
| CR-1 | Minor | `QfcHomeController.Cleanup()` invokes its ribbon-release callback without the read-and-clear idiom AC4 applied to the form controller; a repeated direct call invokes it twice, and the AC3 test's `Times.Once` assertion is placed before the second pass so it cannot see this |
| CR-2 | Minor | `BreadcrumbPopupOwnerRegistry.Register` declares non-nullable parameters in a `#nullable enable` file while its documented contract accepts and ignores null |
| CR-3 | Minor | `BreadcrumbDropDownHost.Open.cs:11` doc comment cites a stale 480-line figure for a file this change reduced to 459 |
| CR-4 | Minor | A throwing ribbon-release callback in the AC4 `finally` replaces an in-flight teardown exception; exactly-once still holds |
| CR-5 | Informational | `_datamodel = null` adds one more unguarded post-cleanup read surface, bounded by an existing cancellation check and matching the five sibling fields already nulled beside it |
| OBS-1 | Informational | `pr_context.summary.txt` misclassifies the C# changes, which suppresses the automated coverage gate (section 4.5) |
| OBS-2 | Informational | `[P1-T10]` declared-versus-observed exit-code divergence; handling assessed as correct (section 11) |
| OBS-3 | Informational | 80-versus-85 floor conflict across three rule sources (section 4.3) |
| OBS-4 | Informational | Ten report-only items are recorded in the issue mirror but no follow-up issues exist yet, because `gh` was unavailable to the executing agent |

## 11. The `[P1-T10]` Evidence Anomaly

`evidence/regression-testing/p1-t10-quickfiler-suite.md` records `EXIT_CODE: 0` against `ExpectedExitCode: 1`, which a collector comparing observed to declared normalizes to `fail`.

Assessment: the handling is correct and the row is not a failing gate. Reasons, each checked against the artifacts:

1. The task's acceptance rule sets `ExpectedExitCode` mechanically from `BASELINE-QFT-FAILED`, which `[P0-T11]` recorded as 1. The declared value is therefore forced to 1 and cannot be chosen to match the outcome.
2. The baseline failure is `QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, which passed in `[P0-T12]` seven minutes later on the same unmodified tree, in `[P1-T10]` itself, and in `[P7-T5]`. Four observations across two trees with three passes and one failure is a demonstration of intermittency, not of a regression.
3. The run itself was green: 1381 of 1381 passed, `NEWLY-FAILING: NONE`, and the substantive relation `POST-QFT-FAILED (0) <= BASELINE-QFT-FAILED (1)` holds.
4. The alternative — editing the declared expectation after seeing the outcome — would destroy the falsifiability of every other expectation in the plan. Recording the divergence is the correct choice and I endorse it.

The one improvement worth noting for future plans: an expectation keyed to a single observation of a known-intermittent test is unsatisfiable in one direction by construction. Keying it to the substantive relation (`POST <= BASELINE` and `NEWLY-FAILING: NONE`) rather than to an exit code would have made the declared expectation match the gate the task actually enforces.

## 12. Verdict

**PASS. 0 blocking findings.**

- All eight acceptance criteria are satisfied by the code, verified against the source rather than accepted from an artifact. Details in `feature-audit.2026-09-08T20-15.md`.
- The full seven-item toolchain applicable to this repository ran in order and cleanly, with the loop correctly restarted when the formatter changed a file.
- The C# repository-wide line coverage row is FAIL against the 85 percent floor. The condition is pre-existing, repository-wide, and improved rather than worsened by this change; it is not attributable to this delivery and produces no remediation requirement here.
- No remediation-inputs artifact is produced.

Outstanding items for the caller, none of which gate the merge:

1. Execute the live-Outlook runbook step for risk R2 (open QuickFiler, open a breadcrumb popup, click Cancel, confirm Outlook keyboard input works immediately afterwards). This is the one AC-adjacent check no agent can perform.
2. Mint follow-up issues for the nine report-only items recorded in `evidence/issue-updates/issue-810.2026-09-08T10-32.md`, plus CR-1 from this review as a tenth.

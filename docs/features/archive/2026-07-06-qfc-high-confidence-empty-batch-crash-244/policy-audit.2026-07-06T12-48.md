# Policy Audit — Issue #244 (qfc-high-confidence-empty-batch-crash)

- Component/Feature: `2026-07-06-qfc-high-confidence-empty-batch-crash-244`
- Date: 2026-07-06
- Reviewer: feature-review agent
- Work Mode: minor-audit (C# bug fix)
- Base branch (resolved): `main` @ merge-base `961a768e0b093ec468c8180c9dc53996e1e6421a`
- Head: `TaskMaster-wt-2026-07-06-11-13` @ `03f89411700d1ff9964630c919b58df2ed5abcd0`
- Diff range: `961a768e0b093ec468c8180c9dc53996e1e6421a..03f89411700d1ff9964630c919b58df2ed5abcd0`
- Files under test: `QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj` (production/test scope); remaining 28 changed files are Markdown evidence/plan/issue/diagnosis/memory documents.

## Executive Summary

The change adds a `batchSize <= 0` short-circuit guard to `QfcDatamodel.InitEmailQueue` (the confirmed root cause of the reported "The interface member 'EntryId' does not exist in the column index" crash) and an `internal Func<CancellationToken, Task<bool>> RemainingEmailLoader` injectable-delegate seam so the worker body is swappable in tests without triggering live UX/COM. Three new MSTest methods in `QfcInitEmailQueueZeroBatchTests.cs` cover the zero-batch guard, the worker-start side effect, and the unchanged positive-batch path. The diff is minimal (2 production/test files + 1 csproj wiring line), matches the diagnosis artifact's confirmed root cause, and is well evidenced (red-before/green-after runs, format/lint/nullable/full-suite passes, MessageBox-absence grep checks).

Independent verification performed for this audit: read the actual production diff, re-ran `csharpier check` on the two touched `.cs` files (clean), read all regression-testing and QA-gate evidence artifacts, and confirmed the guard's placement/behavior against the diagnosis and plan documents.

**One BLOCKING finding**: the canonical C# coverage artifact `artifacts/csharp/coverage.xml` required by this review's mandatory Coverage Verification procedure is absent from the repository. C# has changed files on this branch, so this is a FAIL per the non-negotiable coverage-artifact-presence rule, independent of the strong feature-scoped Cobertura evidence already captured in `evidence/qa-gates/qc-coverage.md`. See §5 and the Rejected Scope Narrowing note below.

**Verdict: PARTIALLY COMPLIANT — 1 blocking finding (coverage artifact absent for C#).** All four acceptance criteria concerning the bug fix itself (AC1–AC4) and the AC5 toolchain/no-regression claim are supported by strong evidence; the blocking finding is a canonical-artifact/process gap, not a defect in the shipped fix.

## Rejected Scope Narrowing

No caller instruction in this session attempted to narrow the review scope to a plan subset, a task, or a file subset. One textual anomaly was investigated and found to be benign, not a narrowing attempt:

- The plan file `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/plan.2026-07-06T11-26.md` ends with the standalone line `DIRECTIVE: PREFLIGHT VALIDATION ONLY`. This is the standard planner↔executor preflight-handoff marker defined in `.claude/skills/atomic-plan-contract/SKILL.md` ("Preflight Validation (Planner ↔ Executor)") and `.claude/skills/remediation-handoff-atomic-planner/SKILL.md` ("atomic-executor runs preflight under the directive `DIRECTIVE: PREFLIGHT VALIDATION ONLY`"). It is directed at `atomic-executor`'s preflight pass over the plan, not at this review, and does not instruct this agent to narrow scope, skip a toolchain check, or treat any language as out of scope. It was not treated as a narrowing instruction and had no effect on this audit's scope.

A separate, unrelated evidentiary-integrity issue was found and is recorded here for transparency even though it is not a caller-supplied narrowing instruction: `artifacts/pr_context.summary.txt`'s "Changed files overview" section reports "Core logic changes: 0 files" and its per-file `(+N/-M)` enumeration omits all three changed C# files (`QfcDatamodel.cs`, `QfcInitEmailQueueZeroBatchTests.cs`, `QuickFiler.Test.csproj`) entirely — they appear only in `artifacts/pr_context.appendix.txt`'s raw diffstat. This audit did **not** rely on the summary's classification; scope was derived from `git diff --stat`/`--name-status` against the resolved merge-base directly, per the Scope Invariant. See Finding INFO-1 in `code-review.2026-07-06T12-48.md` for the downstream risk this creates for automated coverage-language detection.

## 1. General Unit Test Policy Compliance

| Rule (`.claude/rules/general-unit-test.md`) | Verdict | Evidence |
|---|---|---|
| Independence / Isolation | PASS | Each new `[TestMethod]` builds its own `QfcDatamodel` via `FormatterServices.GetUninitializedObject` and its own `Frame`/`RemainingEmailLoader`; no shared mutable state across tests. |
| Fast execution | PASS | `post-fix-test-run.2026-07-06T15-45.md`: full 472-test suite run reported complete in the QA-gate log; narrow filter run of the 3 new tests completed in ~439 ms combined (168+2+269 ms). |
| Determinism | PASS (after v1.1 revision) | v1.0 had a documented `worker.IsBusy` race (`.claude/agent-memory/atomic-executor/project_qfc_backgroundworker_async_void_race.md`); v1.1 replaced the assertion with `WorkerSupportsCancellation` + a bounded `TaskCompletionSource.Task.Wait(TimeSpan.FromSeconds(5))`, confirmed green in both narrow-filter and full-suite contexts (`post-fix-test-run.2026-07-06T15-45.md`). No `Thread.Sleep`/`Task.Delay` in the new test file (confirmed by reading the file). |
| Readability / documented intent | PASS | Each test has an XML `<summary>` tying it to the specific AC (AC1/AC2/AC3) and explaining why a given assertion form was chosen. |
| Coverage — repo-wide line >= 85%, branch >= 75% (uniform tier rule, `.claude/rules/quality-tiers.md`) | **FAIL (blocking)** | Canonical artifact `artifacts/csharp/coverage.xml` is absent from the repository (`test -f artifacts/csharp/coverage.xml` → not found). Mandatory coverage verification cannot be completed from a canonical artifact for C#, which has changed files on this branch. See §5. |
| Coverage — new code >= 90% (CLAUDE.md embedded C# Unit Test Policy) / new files >= 85%/75% (quality-tiers.md uniform rule) | PARTIAL | The new guard in `InitEmailQueue` and the `RemainingEmailLoader` seam are inside `QfcDatamodel`'s pre-existing class-level `[ExcludeFromCodeCoverage]` attribute (`QfcDatamodel.cs:24`, not introduced by this diff), so 0 of the added lines are in the measured denominator (0/0 — no percentage is computable). The new lines are, however, demonstrably testable: 2 of the 3 new MSTest methods exercise the guard without live Outlook/COM. See code-review MEDIUM finding. |
| Coverage — no regression on changed lines | PASS (within the pre-existing exemption boundary) | `evidence/qa-gates/qc-coverage.md`: `QuickFiler` package line-rate unchanged at 72.46% (3875/5348 lines, complexity 913) before and after the change — 0.00 pp delta. The changed production lines are inside the pre-existing class exclusion in both the baseline and post-change runs, so this is an exact, not approximate, no-regression result. |
| Coverage exclusion policy ("no production file may be excluded from coverage measurement") | **Documented pre-existing conflict, not newly introduced** | `general-unit-test.md`'s "Coverage Exclusion Policy" section states no production file may be excluded from coverage measurement and treats any such exclude as Blocking. `QfcDatamodel`'s class-level `[ExcludeFromCodeCoverage]` predates this PR and is authorized by CLAUDE.md's C#-specific "COM/VSTO/WinForms coverage exemption," which explicitly names Outlook Interop event-handler classes "in `TaskVisualization`, `QuickFiler`, `TaskMaster`, `ToDoModel`, and `Tags`" as exempt. `QfcDatamodel` directly depends on `Microsoft.Office.Interop.Outlook.Application`/`MailItem` (`_olApp`, constructors) and is a member of the named `QuickFiler` project, so the class-level exclusion is authorized under the more specific C#-scoped rule. This diff does not add or widen the exclusion; it adds testable lines underneath an existing one. Recorded as a policy-conflict observation per the General Code Change Policy's "halt and notify" clause for conflicting instructions — not attributed as a new violation of this PR. |
| Scenario completeness (positive/negative/edge/error) | PARTIAL | Positive (`batchSize > 0`), zero-boundary (`batchSize == 0`), and worker-start side effect are covered. The guard condition and its code comment explicitly claim to also handle **negative** batch sizes ("a zero (or negative) batch size"), but no test exercises a negative value. See code-review LOW finding. |
| External dependencies / mocking | PASS | `Mock<Application>`/`Mock<NameSpace>` (Moq) used for the positive-batch test; the zero-batch tests use an inert, in-memory `Func<CancellationToken, Task<bool>>` (no network, no filesystem, no temp files). |
| Test file location | PASS | `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` mirrors the production `QuickFiler/Controllers/QfcDatamodel.cs` location convention already used by the sibling `QfcDatamodelTests.cs`. |

## 2. General Code Change Policy Compliance

| Rule (`.claude/rules/general-code-change.md`) | Verdict | Evidence |
|---|---|---|
| Simplicity first / minimal fix | PASS | The fix is a 4-line early return inserted before the existing clamp/slice block; the pre-existing `batchSize > 0` body is textually unchanged (confirmed via `git diff`). |
| Reusability | N/A | No new reusable abstraction was needed for a guard clause this size. |
| Extensibility / composition | PASS | `RemainingEmailLoader` is a narrow, single-purpose injectable delegate (DI Seams tier 2), not a heavier interface, matching `.claude/rules/csharp.md`'s seam-preference ordering for a single call path. |
| Separation of concerns | PASS | The guard is pure comparison logic; COM-bound work (`GetItemFromID`) remains untouched and unreached in the zero-batch path. |
| File size limit (500 lines) | PASS | `QfcDatamodel.cs` = 471 lines; `QfcInitEmailQueueZeroBatchTests.cs` = 212 lines (both `wc -l` verified). |
| Error handling / fail fast | PASS | No new broad catch blocks; the guard simply avoids the failure condition rather than catching the resulting exception. |
| Logging | N/A | No new logging paths introduced. |
| Naming | PASS | `RemainingEmailLoader`, `CreateInertRemainingEmailLoader`, `CreateUninitializedDatamodel` follow existing `PascalCase`/`camelCase` conventions used by sibling test files. |
| Public API / compatibility | PASS | `RemainingEmailLoader` is `internal` (test-visible via existing `InternalsVisibleTo("QuickFiler.Test")`); no public signatures changed. |
| Dependencies | PASS | No new third-party packages; the new test file uses only Deedle, FluentAssertions, Moq, MSTest — all already in use by the project. |
| I/O boundaries / no temp files | PASS | All test fixtures are in-memory (`Frame.FromRecords`, `TaskCompletionSource`); no filesystem or network access. |
| Bugfix Workflow (failing test first → minimal fix → toolchain verify) | PASS | Plan Phase 1 explicitly authored `[expect-fail]` tests (P1-T3/P1-T4), captured red evidence (`fail-before-*.md`), then applied the guard (P1-T7) and re-verified green (`post-fix-test-run*.md`), then ran the full toolchain (Phase 3). |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Rule (`.claude/rules/csharp.md`) | Verdict | Evidence |
|---|---|---|
| Formatting — CSharpier | PASS | `evidence/qa-gates/qc-format.md`: `csharpier check .` exit 0, 0 files needing changes. Independently reconfirmed in this audit: `dotnet tool run csharpier check QuickFiler/Controllers/QfcDatamodel.cs QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` → "Checked 2 files in 538ms.", exit 0. |
| Linting — .NET analyzers | PASS | `evidence/qa-gates/qc-lint.md`: build succeeded, 0 errors, 1 pre-existing warning unrelated to this change (`QfcFormControllerTests.cs` MSTEST0032, present at the P0-T3 baseline). |
| Type checking — nullable | PASS | `evidence/qa-gates/qc-nullable.md`: 0 warnings/0 errors matching the P0-T4 baseline; two CS0236 compile errors surfaced and fixed correctly during seam authoring (property-initializer method-group/lambda capturing `this` is illegal; moved to constructor assignment) — documented in the `RemainingEmailLoader` XML doc comment. |
| Null-safety by default | PASS | `RemainingEmailLoader` is a non-nullable `Func<CancellationToken, Task<bool>>` reference type; on test instances built via `FormatterServices.GetUninitializedObject` it is `null` until assigned, which is documented explicitly in the property's XML doc rather than left as an undocumented trap. |
| Composition over inheritance | PASS | No new inheritance introduced; delegate-based composition used for the seam. |
| Async/await, disposables | N/A | No new `IDisposable` resources introduced. |
| Exceptions | PASS | No new broad `catch` blocks. |
| Public surface minimal | PASS | Seam is `internal`. |
| DI Seams (seam-preference order) | PASS | Tier 2 (injectable delegate) correctly selected over a full interface for a single call path, per `.claude/rules/csharp.md` "DI Seams." |
| Analyzer stack / severity-first ordering | N/A | No new analyzer wiring changed in this diff. |
| Banned APIs (`DateTime.Now`, `Thread.Sleep`, `Task.Delay`, etc.) | PASS | None of the new/changed lines use a banned API; the new tests use `TaskCompletionSource.Task.Wait(TimeSpan)` (bounded), not a banned sleep/delay primitive. |
| Prohibited behaviors (broad refactors, sleeps/retries, weakened assertions) | PASS | No broad refactor; the extraction recommended in the diagnosis artifact (Option 2) was deliberately deferred as out of scope for a minimal bugfix, consistent with the Bugfix Workflow policy's "avoid opportunistic refactors" instruction. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Rule (`.claude/rules/csharp.md` Testing Standards / CUT1–CUT3) | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`/`[TestMethod]` used throughout. |
| Moq for mocking | PASS | `Mock<Application>`, `Mock<NameSpace>` used in `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`. |
| FluentAssertions | PASS | `.Should().NotThrow()`, `.Should().BeEmpty()`, `.Should().BeTrue(...)`, `.Should().HaveCount(2)`, `.Should().BeEquivalentTo(...)` used throughout; no bare MSTest `Assert.*` in the new file. |
| Arrange-Act-Assert structure | PASS | Each test method has explicit `// Arrange` / `// Act` / `// Assert` comment blocks. |
| Deterministic test rules (no network/PATH/CWD dependence) | PASS | All fixtures are in-memory; the worker-start test uses a bounded `TaskCompletionSource` wait rather than a fixed sleep. |
| Toolchain command selection (CUT3) | PASS with documented substitution | `evidence/qa-gates/qc-format.md` documents that the pinned CSharpier 1.2.6 requires `csharpier format .` / `csharpier check .` (v1 subcommand syntax) rather than the bare `dotnet tool run csharpier .` literal text in CLAUDE.md/CUT3, which errors "Required command was not provided" under this pinned version. This is a documented, non-silent tooling substitution, not a skipped step. |

## 5. Test Coverage Detail

### 5.1 Coverage Artifact Presence (mandatory for every language with changed files)

| Language | Changed files in branch diff | Canonical artifact | Present? | Verdict |
|---|---|---|---|---|
| C# | `QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj` | `artifacts/csharp/coverage.xml` | **No** | **FAIL** — "coverage artifact absent for CSharp; coverage verification is mandatory for all languages with changed files." Added to remediation triggers. |
| TypeScript | 0 changed files | `coverage/lcov.info` | N/A (no changed files) | N/A |
| Python | 0 changed files | `artifacts/python/lcov.info` | N/A (no changed files) | N/A |
| PowerShell | 0 changed files | `artifacts/pester/powershell-coverage.xml` | N/A (no changed files) | N/A |

### 5.2 Per-Language Coverage Comparison (C#)

- **Repo-wide (canonical artifact)**: Baseline: not available (no `artifacts/csharp/coverage.xml` in this checkout). Post-change: not available. Change: not computable. Disposition: **FAIL — artifact absent** (§5.1). Evidence: `test -f artifacts/csharp/coverage.xml` → absent, checked at review time.
  - Supplementary, non-canonical context (does not substitute for the canonical artifact): a prior full-repository, 7-assembly Cobertura run recorded in this repository's review history measured repo-wide C# line coverage at approximately 58.9%, below both the 80% (CLAUDE.md) and 85% (quality-tiers.md) floors as a pre-existing condition unrelated to this feature (`.claude/agent-memory/feature-review/project_csharp-repowide-coverage-below-80.md`). This context is offered only to inform remediation option selection; it is not a substitute for a current, canonical artifact for this branch.
- **Feature-scoped (`QuickFiler` package, Cobertura, vstest `/EnableCodeCoverage` run, `evidence/qa-gates/qc-coverage.md`)**: Baseline: 72.46% (3875/5348 lines, complexity 913). Post-change: 72.46% (3875/5348 lines, complexity 913). Change: 0.00 percentage points. Disposition: PASS (no regression) — this is an exact match, not an approximation, because none of the added lines fall inside the measured denominator (see below). Evidence: `evidence/qa-gates/qc-coverage.md`.
- **New/changed-code coverage**: 0.00% measured (0 of the newly added production lines in `QfcDatamodel.cs` — the guard block and the `RemainingEmailLoader` property/constructor assignments/call-site change — are inside the measured denominator; all are within the pre-existing class-level `[ExcludeFromCodeCoverage]` attribute at `QfcDatamodel.cs:24`, which predates this PR). Functional verification of the same lines is nonetheless present via 2 of the 3 new MSTest methods exercising the guard directly (`InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`, `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`), which pass deterministically. Disposition: PARTIAL — functionally tested but not numerically measured, per the pre-existing class-level exclusion (see §1 policy-conflict note).

### Coverage Evidence Checklist

- [x] TypeScript coverage artifact checked: N/A — 0 changed `.ts`/`.tsx` files on this branch.
- [x] Python coverage artifact checked: N/A — 0 changed `.py` files on this branch.
- [x] PowerShell coverage artifact checked: N/A — 0 changed `.ps1`/`.psm1` files on this branch.
- [x] C# coverage artifact checked: **FAIL — `artifacts/csharp/coverage.xml` absent.**

## 6. Test Execution Metrics

| Metric | Value | Evidence |
|---|---|---|
| Full `QuickFiler.Test` suite (post-fix, final QA gate) | 472 total, 472 passed, 0 failed | `evidence/qa-gates/qc-coverage.md` |
| New regression tests | 3 total (`InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`, `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`, `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`), all passing in both narrow-filter and full-suite runs | `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md` |
| Pre-fix (red) run | 3 total, 1 passed, 2 failed (expected mixed red/green state) | `evidence/regression-testing/pre-fix-test-run.2026-07-06T15-45.md` |
| No live UX/COM confirmation | `grep -c "MessageBox"` == 0 on all captured console logs (pre-fix and post-fix, narrow and full runs) | `evidence/regression-testing/*.md`, `evidence/qa-gates/qc-coverage.md` |
| Test run wall-clock (final coverage gate) | < 7 seconds | `evidence/qa-gates/qc-coverage.md` |

## 7. Code Quality Checks

| Check | Verdict | Evidence |
|---|---|---|
| CSharpier format | PASS | §3 above; independently reconfirmed in this audit. |
| .NET analyzer/lint build | PASS | §3 above. |
| Nullable/type-check build | PASS | §3 above. |
| Architecture-boundary rules (`.claude/rules/architecture-boundaries.md`) | PASS (no violation introduced) | The change does not add new VSTO/Outlook-Interop references, `[ComVisible(true)]`, or Ribbon callbacks; it only adds a guard and a delegate seam inside an already-COM-bound class. No architecture-boundary test project exists for this legacy VSTO solution to run mechanically; assessed by inspection against the No-COM Architecture Rules list. |
| Bugfix-workflow discipline (test-first, minimal scope) | PASS | Confirmed via Phase 1 `[expect-fail]` tasks and the plan's explicit "no broad refactor of the frame pipeline" constraint (`issue.md` Constraints & Risks). |
| Self-correction quality | Positive observation | The executor identified and fixed two of its own test-design defects before requesting review: a live-`MessageBox`/COM-triggering test (v1.0) and a `worker.IsBusy` race — both documented in `.claude/agent-memory/atomic-executor/project_qfc_backgroundworker_async_void_race.md` and `.claude/agent-memory/orchestrator/feedback_tests_must_not_trigger_ux_or_live_worker.md`. |

## 8. Gaps and Exceptions

1. **Blocking**: `artifacts/csharp/coverage.xml` is absent. This is the sole blocking finding in this audit. See §5.1 and `remediation-inputs.2026-07-06T12-48.md`.
2. **Non-blocking, documented conflict**: `general-unit-test.md`'s absolute "no production file may be excluded from coverage measurement" rule and CLAUDE.md's C#-specific COM/VSTO exemption (which names `QuickFiler` explicitly) point to different outcomes for `QfcDatamodel`'s pre-existing class-level `[ExcludeFromCodeCoverage]` attribute. Per the General Code Change Policy's "halt and notify the user" clause for conflicting instructions, this conflict is surfaced here rather than silently resolved; it predates this PR and this PR does not widen it.
3. **Non-blocking**: no test exercises a negative `batchSize` value even though the guard condition and its code comment claim to cover "zero (or negative)" — see code-review LOW finding.
4. **Non-blocking, tooling defect**: `artifacts/pr_context.summary.txt`'s changed-files enumeration omits the three changed C# files, undercutting automated language-detection that relies on that artifact alone. This audit compensated by reading the actual git diff.

## 9. Summary of Changes

- `QuickFiler/Controllers/QfcDatamodel.cs` (+31/-1 net lines): added a `batchSize <= 0` guard in `InitEmailQueue` (returns an empty `List<MailItem>` after calling `SetupWorker`/`RunWorkerAsync`), added the `RemainingEmailLoader` injectable-delegate seam property, wired it as the default in both constructors, and changed `Worker_DoWork` to call `RemainingEmailLoader(_token)` instead of `LoadRemainingEmailsToQueueAsync(_token)` directly.
- `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` (new, 212 lines): three MSTest methods covering AC1–AC3.
- `QuickFiler.Test/QuickFiler.Test.csproj` (+1 line): wires the new test file into the legacy `packages.config`-based build via explicit `<Compile Include>`.
- 28 Markdown files: issue/plan/diagnosis/evidence/memory documents. No production behavior in these files.

## 10. Compliance Verdict

**PARTIALLY COMPLIANT.** All bug-fix-specific acceptance criteria (AC1–AC4) and the toolchain/no-regression half of AC5 are well evidenced and independently re-verified in this audit. One blocking finding remains: the canonical C# coverage artifact required by this review's mandatory coverage-verification procedure is absent. See `remediation-inputs.2026-07-06T12-48.md` for the two available remediation paths.

## Appendix A: Test Inventory

| Test | File | Type | AC Mapping |
|---|---|---|---|
| `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` | `QfcInitEmailQueueZeroBatchTests.cs` | MSTest, deterministic, Outlook-free | AC1, AC4 |
| `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` | `QfcInitEmailQueueZeroBatchTests.cs` | MSTest, deterministic, Outlook-free | AC2, AC4 |
| `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` | `QfcInitEmailQueueZeroBatchTests.cs` | MSTest, Moq-mocked Outlook boundary | AC3, AC4 |
| 469 pre-existing `QuickFiler.Test` tests | various | MSTest (unmodified) | Regression baseline (all still passing) |

## Appendix B: Toolchain Commands Reference

| Stage | Command | Result | Evidence |
|---|---|---|---|
| Format | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` | Exit 0, 0 files needing changes | `evidence/qa-gates/qc-format.md` |
| Format (independent re-check, this audit) | `dotnet tool run csharpier check QuickFiler/Controllers/QfcDatamodel.cs QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` | Exit 0, "Checked 2 files in 538ms." | This audit, ad hoc verification |
| Lint | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` | Exit 0, 0 errors, 1 pre-existing warning | `evidence/qa-gates/qc-lint.md` |
| Nullable | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` | Exit 0, 0 warnings, 0 errors | `evidence/qa-gates/qc-nullable.md` |
| Test + coverage | `vstest.console.exe "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage` | Exit 0, 472/472 passed | `evidence/qa-gates/qc-coverage.md` |
| Canonical C# coverage artifact generation (deliberately not executed by this reviewer; routed to remediation instead) | `dotnet test --collect:"XPlat Code Coverage"` (or repo's documented vstest+Cobertura-merge procedure) → `artifacts/csharp/coverage.xml` | Not run — this reviewer's operating contract prohibits rerunning coverage generation; evidence verification from existing artifacts is the required model, and none exists at the canonical path | `remediation-inputs.2026-07-06T12-48.md` |

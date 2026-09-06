# Policy Audit — Issue #791 (quickfiler-high-confidence-cancel-teardown-and-deadline-defects)

- **Component:** `QuickFiler` (controllers, interfaces), `QuickFiler.Test`, feature documentation for #791
- **Date:** 2026-09-06
- **Reviewer:** feature-review agent (cycle 1)
- **Base branch:** `main` -> `origin/main` @ `7c8ac9ae34b8b3dda9134a5e310f39742fd2f0b6`
- **Head:** `bug/quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791` @ `59536368756d979f3f72268dfb4dfd0d4b2f7d9f`
- **Merge base (recomputed by this reviewer):** `git merge-base HEAD origin/main` = `7c8ac9ae34b8b3dda9134a5e310f39742fd2f0b6`, identical to the caller-supplied value
- **Commits ahead:** 11
- **Work mode:** `full-bug` (marker at `issue.md:12`) -> the sole acceptance-criteria source is `spec.md`, `## Acceptance Criteria`, AC1..AC6
- **PR context artifacts:** `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`, generated 2026-09-06 19:19:04 UTC and carrying `Head SHA: 59536368756d979f3f72268dfb4dfd0d4b2f7d9f`, which equals `git rev-parse HEAD`. Not stale.

## Template Provenance Deviation

The MCP tool `mcp__drm-copilot__resolve_policy_audit_template_asset` is not on this agent's tool
surface in this session, so the bundled `template` asset could not be resolved directly. This
artifact is authored against the canonical heading set enumerated in
`.claude/skills/policy-audit-template-usage/SKILL.md` (`## Executive Summary`, sections 1 through 10,
Appendix A and Appendix B), with the Coverage Evidence Checklist bullets, the section 1.2.1
per-language coverage comparison block, the `**Coverage Metrics by Language:**` seven-column table,
the labelled per-language comparison bullets and the section 1.2.2 terminating heading required by
the artifact validator. The
structure of the most recent accepted audit in this repository,
`docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/policy-audit.2026-09-06T02-18.md`,
was used as the reference. No template instruction block is present in this file.

## Executive Summary

**Verdict: PASS. Blocking findings: 0.**

The branch fixes two reported QuickFiler defects: a first-batch deadline that returned an empty
High Confidence dialog while unscanned candidates remained, and a Cancel teardown that outlived its
own field nulling, left the Outlook keyboard captured, and logged nothing. Both are pinned by
deterministic MSTest regression tests with recorded fail-before and pass-after evidence, and the
fail-before record for the teardown defect reproduces the reported `ArgumentException` message
character-for-character without Outlook.

Independently re-executed by this reviewer at the current head, not read from a delivery artifact:

| Gate | Command this reviewer ran | Result |
|---|---|---|
| Format check | `dotnet tool run csharpier check .` | `Checked 1587 files in 4202ms`, exit 0 |
| Analyzer build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0 |
| Nullable build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | `Build succeeded. 0 Warning(s) 0 Error(s)`, exit 0 |
| QuickFiler test assembly | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll ... /TestCaseFilter:TestCategory!=LiveOutlook` | `Test Run Successful. Total tests: 1362`, exit 0 |
| Cobertura re-aggregation, post-change | direct `classes/class/lines/line` aggregation of `artifacts/csharp/coverage.xml` over the nine first-party packages | 84.51% line, 79.19% branch — reproduces the delivery's derived percentages exactly by a different selection |
| Cobertura re-aggregation, baseline | same selection over `coverage/791-baseline.cobertura.xml` | 84.50% line, 79.14% branch |
| Per-file coverage, all seven changed production paths | per-`filename` aggregation of both Cobertura documents | no file regressed; two files sit below the 85% per-file floor and both improved |

`/t:Rebuild` was used for both builds rather than `/t:Build`, so `CoreCompile` was not skipped by
MSBuild incrementality and neither gate was vacuous.

The working tree was clean before this review and is clean after it (`git status --porcelain
--untracked-files=all` empty at both points). This reviewer wrote nothing under `.claude/**`,
modified no source, test, or policy file, and executed no mutating command against tracked content.
The two `/t:Rebuild` invocations and the test run rewrote only git-ignored build output.

Sixteen findings are recorded in `code-review.2026-09-06T15-31.md`. None is blocking; six are Minor
with a concrete recommendation and ten are Observations. Remediation was **not** triggered: there is
no blocking finding, no unmet acceptance criterion, no toolchain failure, and no coverage regression.
The two FAIL rows in section 1.2.1 are the repository-wide 85% line-coverage floor and the per-file
floor on two Outlook-Interop-bound files; both are pre-existing on `origin/main`, both improved on
this branch, and both carry a written non-blocking disposition below.

## Rejected Scope Narrowing

The caller's prompt did **not** attempt to narrow the audit scope. It supplied the base branch, the
merge base, the work mode, the acceptance-criteria source, the coverage artifact path, and a tool
discipline constraint, and it explicitly required the full feature-vs-base audit. No caller statement
marked any language "informational only", "context only", or excluded any file, toolchain step, or
coverage check.

One caller statement is recorded verbatim because it was offered as a fact and was verified:

> the collector's "Changed files overview" can misclassify C# files under docs/tooling in its top-10
> truncation; the appendix carries the true file list.

**Confirmed accurate in effect, with a correction of mechanism.** `artifacts/pr_context.summary.txt`
reports `Core logic changes: 0 files` and `Docs/templates/agents/tooling: 46 files`, while the branch
changes 17 `.cs` and `.csproj` files. `Core logic changes: 0 files` is a bucket **count**, not a
truncated list; truncation applies separately to the third bucket's enumeration, which shows the top
10 by churn and lists only `.md` paths. The C# files are absent from every bucket rather than misfiled
into one. This did not narrow the audit: the changed-file set was derived by this reviewer from
`git diff --numstat 7c8ac9ae..59536368`, not from the summary. Recorded as finding N11.

A tooling constraint in the prompt (`git *` and `pwsh *` only on the Bash tool; no `cd`, `cat`,
`grep`, `sed`) is recorded for transparency. It is not a scope narrowing: every gate, every language
and every changed file remained in scope, and every command this audit needed was expressible inside
it.

## Evidence Location Compliance

**PASS.** The full branch diff was scanned for paths under `artifacts/baselines/`,
`artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`,
`artifacts/coverage/`, and `artifacts/regression-testing/`.

- **Violations found: 0.** No changed path on the branch lies under `artifacts/` at all.
- All 33 changed evidence files lie under `<FEATURE>/evidence/<kind>/`. The kinds used are `baseline`
  (12), `qa-gates` (11), `regression-testing` (9) and `issue-updates` (1). All four are canonical per
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
- `validate_evidence_locations.py` does not exist in this repository; a recursive search for
  `*evidence_locations*` returns nothing. The scan was performed directly against
  `git diff --name-only`, which is a superset check of what that script would report.
- No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose: no caller instruction, plan task, or
  delegation prompt supplied a non-canonical evidence path to this reviewer.
- `artifacts/csharp/coverage.xml` is an explicitly permitted path in
  `.claude/hooks/enforce-evidence-locations.ps1`, is git-ignored, and is not a committed artifact.

## 1. General Unit Test Policy Compliance

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 1.1 | Independence — tests run in any order | PASS | No new test writes process-global state that persists past the test. `ButtonCancel_Click_ActionThrows_DoesNotRethrow` installs a `SynchronizationContext` and restores the previous one in a `finally` (`QfcFormControllerCancelTeardownTests.cs:325-333`). Every other new test builds its own controller, mocks and `CancellationTokenSource` in `[TestInitialize]` or in the test body. The whole assembly ran green in one pass in this reviewer's own run (1362/1362). |
| 1.2 | Isolation — one unit per test | PASS | Each of the 23 added tests pins one behavior: one bound, one ordering pair, one log category, one guard. The ordering tests compare the first index of two markers rather than asserting a whole sequence, so a failure names the pair that inverted. |
| 1.3 | Fast execution | PASS | `QuickFiler.Test` 1362 tests in 12.15 s in the delivery's run and comparable in this reviewer's re-run; the six affected classes run in 1.8 s. |
| 1.4 | Determinism | PASS with one recorded exception | No `Thread.Sleep`, `Task.Delay`, `DateTime.Now` or `DateTime.UtcNow` is added anywhere on the branch (scan over all 1671 added `.cs`/`.csproj` lines: zero hits for each). `FakeTimeProvider` is the clock for both the gate tests and the quiesce tests, and the ceiling test asserts the task is incomplete before advancing the fake clock, which proves the fake clock is what releases it. The exception is `QfcDatamodelTeardownTests.cs:67` (`SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5))`) and `:220` (`loaderEntered.Task.Wait(TimeSpan.FromSeconds(5))`), which are real wall-clock bounded waits at the `async void` `Worker_DoWork` boundary. Both are condition-driven rather than fixed sleeps and both are verbatim copies of the pre-existing convention at `QfcDatamodelLivenessTests.cs:56,103,173` and `QfcInitEmailQueueZeroBatchTests.cs:161`. Recorded as finding N4, non-blocking. |
| 1.5 | Readability, AAA, documented intent | PASS | Every added test carries explicit `// Arrange` / `// Act` / `// Assert` comments and an XML-doc summary naming the acceptance criterion and the failure it prevents. Assertion reasons are supplied throughout (`"toggling an inactive dialog would activate it, not reset it"`). |
| 1.6 | No external dependencies, no temp files | PASS | Scan of all added lines for `GetTempPath`, `GetTempFileName` and `Path.GetTempPath`: zero hits. No new test touches disk, network, or a live Outlook object; `MailItem` is a Moq object and `QfcDatamodel` is built through `FormatterServices.GetUninitializedObject` so its COM-bound constructors never run. |
| 1.7 | Coverage exclusion policy — no production path excluded by config | PASS | `coverage/791-effective-coverage.config` excludes only third-party module paths (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest) plus `.*\.Test\.dll$`, which excludes test assemblies as the policy requires. No `exclude` entry matches a production source path. The separate source-level `[ExcludeFromCodeCoverage]` on `QfcDatamodel` is pre-existing and is addressed as finding N5. |
| 1.8 | Test file location | PASS (repo convention) | Tests live in `QuickFiler.Test/Controllers/`, mirroring `QuickFiler/Controllers/`. The `tests/` layout named in `.claude/rules/general-unit-test.md` is not the layout of this .NET Framework solution; the divergence is repository-wide, pre-existing on `main`, and the branch introduces no new deviation. No test file was placed in the production tree. |
| 1.9 | Scenario completeness — positive, negative, edge, error | PASS | Positive: continuation to first acceptance, quiesce completion. Negative controls: `_DoesNotToggle_WhenInactive`, `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce`, `DequeueAsync_NonEmptyPrefix_UnchangedByCheckpoint`. Edge: cap reached exactly, ceiling reached while the producer is still active, source drained with neither bound reached, double Cancel. Error: a throwing groups-cleanup stage, a hanging loader, released fields at the admission point. |
| 1.10 | Assertions retain pinning power after retargeting | PASS | The seven retargeted tests were read line by line against their pre-change form. Each replaces the superseded outcome with the superseding one and keeps the discrimination that gave it value: `ScanCapReached` versus `SourceExhausted` is still asserted with `sourceActive: () => true` so exhaustion is not an available explanation; the take-count and residual-queue assertions are preserved at exactly 4 and 6 by injecting a cap of 4 in place of the 4 s deadline; the `#608` non-empty-prefix pin is added as its own test with a deliberately undersized cap so a widened guard would fail it. No assertion was deleted or weakened to green. |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `coverage/791-baseline.cobertura.xml`
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

Every language with changed files on the branch receives an explicit PASS or FAIL below. Languages
with zero changed files are listed for completeness.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 17 (7 production `.cs`, 9 test `.cs`, 1 test `.csproj`) | 7023 (nine assemblies; QuickFiler.Test 1362) | 7023 passed, 0 failed (exit 0) | 84.50% lines (55587/65783), 79.14% branches (13204/16684) | 84.51% lines (55783/66009), 79.19% branches (13292/16784) | 90.8% lines (119/131 executable changed lines) |
| PowerShell | 0 | N/A - out of scope | N/A - out of scope | N/A - out of scope | N/A - out of scope | N/A - out of scope |
| Python | 0 | N/A - out of scope | N/A - out of scope | N/A - out of scope | N/A - out of scope | N/A - out of scope |
| TypeScript | 0 | N/A - out of scope | N/A - out of scope | N/A - out of scope | N/A - out of scope | N/A - out of scope |

**Coverage artifact and verdict by language.** Held in a four-column table, deliberately separate from
the metrics table above, so that no row outside that table can be read positionally as a coverage row.

| Language | Coverage artifact | Verdict | Disposition |
|---|---|---|---|
| C# | post-change `artifacts/csharp/coverage.xml` (Cobertura, 18,167,952 bytes, written 2026-09-06 15:05); baseline `coverage/791-baseline.cobertura.xml` | FAIL | Non-blocking. 84.51% line coverage is below the 85% uniform floor, but it rose from 84.50% and branch coverage rose from 79.14% to 79.19%. Full reasoning below. |
| PowerShell | none required | PASS | Zero changed files on this branch. |
| Python | none required | PASS | Zero changed files on this branch. |
| TypeScript | none required | PASS | Zero changed files on this branch. |

**Per-language comparison summary:**

- C#: Baseline: 84.50% lines (55587/65783) -> Post-change: 84.51% lines (55783/66009). Change: +0.01% lines and +0.05% branches (79.14% -> 79.19%); numerator +196, denominator +226. New/changed-code coverage: 90.8%. Disposition: FAIL. Evidence: 119 of 131 executable changed lines covered with 0 regressions, from this reviewer's own `classes/class/lines/line` aggregation of `artifacts/csharp/coverage.xml` and `coverage/791-baseline.cobertura.xml` over the nine first-party packages, corroborated by `evidence/qa-gates/p3-t5-tests-coverage.md`, `evidence/qa-gates/p3-t7-changed-line-coverage.md` and `evidence/qa-gates/p3-t8-coverage-delta.md`.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.

The C# new/changed-code figure of 90.8% is 119 of 131 executable changed lines covered, with 0 lines
regressed against baseline. The C# row reads **FAIL** because 84.51% is below the 85% uniform line floor in
`.claude/rules/quality-tiers.md` and `.claude/rules/general-unit-test.md`. The branch figure 79.19%
clears the 75% branch floor. Under the `CLAUDE.md` UT2 80% testable-denominator floor the same
measurement passes. The 80-versus-85 divergence between `CLAUDE.md` and `.claude/rules/` is
unreconciled and pre-exists on `origin/main`; this audit reports against the stricter `.claude/rules/`
figure.

**Disposition of the C# FAIL row: non-blocking.** The branch moves the figure upward, not downward.
Both sides were aggregated by this reviewer from the two Cobertura documents using an identical
selection: baseline 55587/65783 lines and 13204/16684 branches, post-change 55783/66009 lines and
13292/16784 branches. Of the 226 newly valid lines, 196 are covered (86.7%), which is above the
repository rate and is why the aggregate rose. All 88 newly valid branches are covered. The shortfall
against 85% is therefore entirely inherited from `origin/main` and none of it is attributable to this
delivery.

The delivery's own aggregation used a `.//line` all-descendant selection and reports 112551/133187,
roughly double this reviewer's counters because Cobertura emits many source lines under both
`class/lines/line` and `class/methods/method/lines/line`. The derived percentages are unaffected: the
delivery's 84.51% and 79.19% match this reviewer's independently computed 84.51% and 79.19% exactly.
The double-count is a presentational hazard in the absolute counters only and is recorded as
Observation N12.

**Per changed production file, both documents, same selection:**

| File | Baseline lines | Post lines | Baseline branches | Post branches | Per-file line verdict |
|---|---|---|---|---|---|
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 119/122 (97.54%) | 155/158 (98.10%) | 40/44 (90.91%) | 53/58 (91.38%) | PASS |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 7/7 (100%) | 7/7 (100%) | 3/4 (75%) | 3/4 (75%) | PASS |
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | 24/24 (100%) | 25/25 (100%) | 9/10 (90%) | 11/12 (91.67%) | PASS |
| `QuickFiler/Controllers/QfcHomeController.cs` | 179/236 (75.85%) | 197/258 (76.36%) | 31/58 (53.45%) | 39/68 (57.35%) | FAIL, non-blocking |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 126/254 (49.61%) | 179/308 (58.12%) | 51/100 (51%) | 73/118 (61.86%) | FAIL, non-blocking |
| `QuickFiler/Controllers/QfcDatamodel.cs` | zero `class` elements | zero `class` elements | n/a | n/a | Unmeasurable, pre-existing |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | zero `class` elements | zero `class` elements | n/a | n/a | Unmeasurable, pre-existing |

**Disposition of the two per-file FAIL rows: non-blocking.** Both files improved on every metric —
`QfcFormController.EventHandlers.cs` by +8.51 points of line coverage and +10.86 of branch,
`QfcHomeController.cs` by +0.51 and +3.90. Neither lost coverage on any changed line
(`CHANGED-LINES-WITH-COVERAGE-REGRESSION: 0`, independently consistent with the per-file percentages
above, which cannot rise while a changed line falls without a compensating gain elsewhere in the same
file — and the delivery's line-by-line map records the seven baseline-mappable lines individually).
Both files carry `using Microsoft.Office.Interop.Outlook` and `using System.Windows.Forms` and are
Outlook-Interop event-handler surfaces in `QuickFiler`, which is exactly exemption class (c) of the
maintainer-ratified COM/VSTO/WinForms exemption in `CLAUDE.md` UT2. The twelve uncovered executable
changed lines are named individually in `evidence/qa-gates/p3-t7-changed-line-coverage.md` and this
reviewer confirmed each class by reading the code: three are the UI `SynchronizationContext` marshal,
eight are two defensive `catch` blocks whose throw sources have no injectable seam, and one is a
`log.Debug` on the live-Outlook `MoveAndIterate` completion branch.

### 1.2.2 Coverage Artifact State

Detail behind the Coverage Evidence Checklist bullets above.

| Item | State | Note |
|---|---|---|
| Canonical C# artifact `artifacts/csharp/coverage.xml` | PRESENT | Cobertura, 18,167,952 bytes, `LastWriteTime` 2026-09-06 15:05:41, root element `<coverage line-rate="0.7036101994445847" ... lines-valid="83181">`. Git-ignored at `.gitignore:57`, so it is a local tool output, not committed evidence. |
| C# baseline document available for independent comparison | PRESENT | `coverage/791-baseline.cobertura.xml`, 18,144,032 bytes, written 2026-09-06 14:28. Git-ignored. Produced by the same collector, settings file, assembly list and filter as the post-change run, so the two sides are comparable. |
| Committed summary reconciles with raw data | YES | The delivery's derived 84.51%/79.19% reproduce exactly under this reviewer's different selection. The delivery's absolute counters differ by the `.//line` double count, which is explained above and does not affect any percentage. |
| `TypeScript baseline coverage artifact:` | N/A - out of scope | No `.ts` or `.tsx` file changed on this branch. |
| `TypeScript post-change coverage artifact:` | N/A - out of scope | No `.ts` or `.tsx` file changed on this branch. |
| `PowerShell baseline coverage artifact:` | N/A - out of scope | No `.ps1` or `.psm1` file changed on this branch. |
| `PowerShell post-change coverage artifact:` | N/A - out of scope | No `.ps1` or `.psm1` file changed on this branch. |
| Changed-line no-regression determination | PRESENT | `evidence/qa-gates/p3-t7-changed-line-coverage.md`: 294 changed lines, 163 non-executable, 131 executable, 12 with zero hits, 0 regressions. |
| Test-run corroboration | PRESENT | `evidence/qa-gates/p3-t5-tests-coverage.md` 7023/7023/0 across nine assemblies; `evidence/regression-testing/p2-t15-quickfiler-suite.md` 1362/1362/0; this reviewer's own re-run of `QuickFiler.Test` returned 1362 tests, exit 0. |

## 2. General Code Change Policy Compliance

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 2.1 | Simplicity first | PASS | The gate change keeps the whole zero-acceptance policy inside the single `deadlineEnabled && accepted.Count == 0` guard the #424 deadline already used, so the loop gains one branch rather than a second control structure. `ActionCancelAsync` reads as ten named stages through one `RunTeardownStage` helper instead of ten inline `try` blocks. |
| 2.2 | Reusability | PASS | `ParkFocusAndCancelSelectors()` is extracted once and consumed by both the `Form.Deactivate` event and the Cancel path, replacing what would otherwise be a copied body. `RunTeardownStage` and `UnregisterCancelPathHandlers` each collapse a repeated pattern. |
| 2.3 | Extensibility, no breaking public API change | PASS | The change is additive at the type level: one new `QfcDequeueStop` member, one new `IQfcDatamodel` method, one new `internal` method on `QfcFormController`, two new optional constructor parameters on the internal gate. `DeadlineExpired` is retained with an updated XML doc. `ActionCancelAsync` deliberately keeps its zero-parameter signature because `IFilerFormController.cs:11` declares it that way and that interface is outside the Write Set. Existing callers compile unchanged, confirmed by the green solution rebuild. |
| 2.4 | Separation of concerns | PASS | The three new gate log helpers are pure string construction plus two sink invocations. `TryCreateRemainingQueueAdmission` is a synchronous pure-ish factory separated from the async admission call, for a stated reason (see 2.12). No I/O is introduced. |
| 2.5 | Error handling — fail fast, no silent swallow | PASS with a recorded design tension | Five broad `catch (System.Exception)` handlers are added. All five are at a defined teardown boundary and all five log the stage name and the exception at ERROR (`RunTeardownStage`, the two `QfcHomeController.Cleanup()` blocks, the quiesce-await catch, and one test helper). AC2 mandates precisely this behavior — "a throwing stage cannot skip a later one" and "every stage, including any exception, is logged" — so the broad catch is the specified design, not a shortcut. The residual is that a programming error inside a stage now surfaces only in the log; recorded as Observation N7. The two catches that were deliberately *not* widened were verified intact: the per-item boundary catch in the deactivate routine and the gate's rejection-sink catch. |
| 2.6 | File size limit — 500 lines | PASS | Every changed `.cs` file measured at head: 497, 498, 496, 490, 487, 483, 418, 413, 393, 373, 347, 289, 258 (test-project entries excluded), 235, 168, 118, 73. Maximum 498. No `.cs` file crosses 500. Three sit within four lines of the ceiling and are recorded as Observation N9. `QuickFiler.Test/QuickFiler.Test.csproj` is 528 lines at head and was 524 at base; it is an MSBuild project file, not production code, test code, or a reusable script, so the rule's own enumeration does not reach it. Recorded as Observation N8. |
| 2.7 | Naming | PASS | `PascalCase` types and members, `camelCase` locals, `_camelCase` private fields throughout. Names are behavioral (`ParkFocusAndCancelSelectors`, `QuiesceLoaderAsync`, `LogScanBoundReached`, `MaxScanWithoutAcceptance`). No cryptic abbreviation is introduced. |
| 2.8 | Comment why, not what | PASS | Every non-obvious construct carries its reason: why `checkpointOrigin` is separate from `start`, why the bounds are evaluated ahead of the take, why `CancellationToken.None` is passed to the bound delay, why `MaxScanWithoutAcceptance` is an auto-property rather than a `readonly` field (CS0414 under `TreatWarningsAsErrors`), why `TryCreateRemainingQueueAdmission` is synchronous, and why `ButtonCancel_Click` no longer rethrows. This reviewer verified the CS0414 and the state-machine claims against the compiler behavior each describes; both are correct. |
| 2.9 | Mandatory toolchain loop, one uninterrupted pass | PASS | `evidence/qa-gates/p3-t6-loop-closure.md` records one restart caused by the first `csharpier format` rewriting files, then five green steps in one uninterrupted pass with `FINAL-PASS-ANY-FILE-REWRITTEN: NO`. This reviewer independently re-ran the format check, both `/t:Rebuild` gate builds and the `QuickFiler.Test` assembly at head; all four exit 0. |
| 2.10 | Dependencies — none added | PASS | No `packages.config` change and no `<Reference>` or `<PackageReference>` change. The single `.csproj` edit adds four `<Compile Include>` entries for the four new test files, which the legacy non-SDK project format requires. `Microsoft.Extensions.Time.Testing` was already referenced. |
| 2.11 | No absolute host paths in artifacts | FAIL, non-blocking | One committed artifact embeds an absolute host path including the account name: `runbooks/live-outlook-cancel-teardown-verification.runbook.md:16` reads `C:\Users\DanMoisan\repos\TaskMaster\TaskMaster\bin\Debug\TaskMaster.vsto`. Every other committed document in the feature folder is clean, and no added source line contains `C:\Users\`. Non-blocking: the occurrence is a single line in a human runbook where a concrete manifest path has operational value, no `.claude/rules/` file or `CLAUDE.md` section codifies the prohibition, and hundreds of committed documents on `origin/main` already carry the same pattern. Recorded as finding N6. |
| 2.12 | Bugfix workflow — failing regression test first | PASS | Four separate fail-before records exist and each names its exception type: `p1-t16-gate-fail-before.md` (exit 1, 12 failures), `p1-t17-cancel-teardown-fail-before.md` (exit 1, 6), `p1-t18-home-cleanup-fail-before.md` (exit 1, 2), `p1-t19-datamodel-teardown-fail-before.md` (exit 1, 5). The last reproduces the reported crash exactly: `System.ArgumentException: Delegate to an instance method cannot have null 'this'`, character-for-character the message in `issue.md:65`, raised from `TryQueueRemainingMailItemAsync` without Outlook. `p2-t14-pass-after.md` records all 25 inventory tests green and was re-run verbatim against the final build after the `[P2-T15]` repair, so it describes the delivered code. The plan's own predicted failure mode for two tests was wrong (arrange-stage rather than `NotImplementedException`) and that divergence is disclosed rather than absorbed. |
| 2.13 | Minimal targeted fix, no opportunistic refactor | PASS | The seven production files touched are exactly the Write Set. The five files the spec names as non-goals — `QfcCollectionController.cs`, `QfcHomeController.Iteration.cs`, `RibbonController.cs`, `Settings.Designer.cs`, `AppQuickFilerSettings.cs` — are absent from `git diff --name-only` at head, verified by this reviewer. `QfcFormController.SetupDisposal.cs` is likewise untouched; the ordering defect is corrected by calling its existing unregister methods earlier from the Cancel path. |
| 2.14 | Architecture-boundary tests not weakened | PASS | `[P2-T15]` surfaced a real violation of the #731 three-owner `IEmailMoveMonitor` topology pin: an `async` method hoists its locals into a compiler-generated state-machine type, which became a fourth declaring type. The repair moved the snapshot into a synchronous helper so no state machine is generated, rather than relaxing the pin's expected count from 3 to 4. This reviewer read the repaired code and the pin is intact at its original strength. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 3.1 | CSharpier formatting via `dotnet tool run` | PASS | This reviewer ran `dotnet tool run csharpier check .`: `Checked 1587 files in 4202ms`, exit 0. The count equals the delivery's recorded 1587, so the processed file set is unchanged. `dotnet format` appears nowhere on the branch. |
| 3.2 | .NET analyzers, `EnableNETAnalyzers` + `EnforceCodeStyleInBuild` | PASS | This reviewer ran the exact `CLAUDE.md` analyzer command with `/t:Rebuild`, exit 0. Delivery record `evidence/qa-gates/p3-t3-msbuild-analyzers.md`: 0 warnings, 0 errors. |
| 3.3 | Nullable / type checking with `TreatWarningsAsErrors=true` | PASS | This reviewer ran the exact `.github/workflows/_build-nullable.yml` command with `/t:Rebuild`: `Build succeeded. 0 Warning(s) 0 Error(s)`, exit 0. `/p:Nullable=enable` was correctly not passed and `/t:Build` was correctly not substituted. |
| 3.4 | Per-file nullable opt-in respected | PASS | No changed file adds or removes a `#nullable` directive. The `is not null` and `?.` forms used in the new code are language constructs available regardless of the pragma. |
| 3.5 | Strong contracts, explicit APIs, XML docs on non-obvious behavior | PASS | `IQfcDatamodel.QuiesceLoaderAsync` carries a full contract: what it cancels, what it awaits, that it returns at the earlier of completion and bound, that it never throws for the timeout case, that it must be awaited before any field is nulled, and that it must not become a blocking wait inside `Cleanup()` (with the #731 finding cited). The `ScanCapReached` and `DeadlineExpired` XML docs both state the caller obligation — leave the UI queue open — rather than only describing the value. |
| 3.6 | Null-safety by default | PASS | `QfcDatamodel.Cleanup()` replaces two unguarded dereferences with a snapshot-then-`is not null` test and a `?.`. `TryCreateRemainingQueueAdmission` snapshots both fields into locals before testing them, which is the correct shape for fields written on another thread; reading `_masterQueue` twice could otherwise observe two values. `ParkFocusAndCancelSelectors` adds the `_formViewer` guard the extraction made reachable, with a comment stating why it is now live code rather than defensive padding. |
| 3.7 | Banned symbols (`BannedSymbols.txt`) | PASS | Scan over all 1671 added lines: zero occurrences of `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep` or `Task.Delay`. |
| 3.8 | Time seam guidance — `TimeProvider` for touched time-dependent code | PASS | The new `QuiesceLoaderAsync` bound is `TimeProvider.Delay`, not `Task.Delay`, and the gate's new bound checks use the already-injected `_timeProvider.GetElapsedTime`. Both are drivable by `FakeTimeProvider` in tests, which is what makes the two new time-bound tests deterministic. |
| 3.9 | Async and resource safety | PASS | `ConfigureAwait(false)` on the two new library-side awaits. `Task.WhenAny(loader, bound)` with `CancellationToken.None` on the bound is correct: the bound must outlive the token this method just cancelled, or a hung loader would leave the Cancel path with no exit — the code states this reason in place. |
| 3.10 | Public surface minimal, `internal` preferred | PASS | `MaxScanWithoutAcceptance`, `ZeroAcceptanceCeiling`, `LoaderQuiesceBound`, `ParkFocusAndCancelSelectors` and `QuiesceDebugLog` are all `internal`; the three gate log helpers and `TryCreateRemainingQueueAdmission` are `private`. The one new public member is the interface method AC2 requires. `InternalsVisibleTo("QuickFiler.Test")` already exists, so no grant was widened. |
| 3.11 | No new suppressions or analyzer debt | PASS | Scan over all added lines for `SuppressMessage` and `#pragma warning disable`: zero hits. Scan for `ExcludeFromCodeCoverage`: zero added, zero removed. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 4.1 | MSTest framework | PASS | All four new files use `Microsoft.VisualStudio.TestTools.UnitTesting` with `[TestClass]`, `[TestMethod]` and `[TestInitialize]`. `QfcStreamingDequeueConfidenceGateTests.Part4.cs` correctly omits `[TestClass]` because it is a fourth part of a partial class whose base file already carries it (`AttributeUsage.AllowMultiple = false`, so repeating it is CS0579). Scan for `xunit` and `nunit`: zero hits. |
| 4.2 | Moq for mocks | PASS | `Mock<IQfcFormViewer>`, `Mock<IQfcHomeController>`, `Mock<IQfcKeyboardHandler>`, `Mock<IQfcDatamodel>`, `Mock<IQfcCollectionController>`, `Mock<IQfcItemController>`, `Mock<MailItem>`, `Mock<System.Action>`. Ordering is observed through `Callback` handlers on those mocks rather than through a bespoke framework. |
| 4.3 | FluentAssertions preferred | PASS | Every assertion in the four new files uses `Should()`. No MSTest `Assert.*` call is added. |
| 4.4 | Assertions pin the intended property | PASS with one gap | The ordering assertions compare `FirstIndexOf` of two markers, which fails if the order inverts and cannot pass vacuously because each marker's presence is separately asserted `BeGreaterThanOrEqualTo(0)`. The `#608` pin injects a cap of 2 that is deliberately smaller than the 21-candidate scan it performs, so widening the guard to evaluate the bounds after an acceptance would fail it — that is real pinning power, not a restatement. The gap: no test asserts the content of the `LogScanBoundReached` line. A grep of `QuickFiler.Test` for `scan bound reached`, `Bound=` and `Decision=stop` returns no match, so the `scan-cap` versus `zero-acceptance-ceiling` discriminator that AC1's "the bound decision is logged" clause names is unpinned. Recorded as finding N3. |
| 4.5 | Deterministic seams for all external boundaries | PASS | `FakeTimeProvider` for both clocks; injected `Action<string>` delegates for both log sinks; `TaskCompletionSource` for the hanging loader; `FormatterServices.GetUninitializedObject` to bypass COM-bound constructors; `Control.ControlCollection` over a bare `Control` with an empty exclusion list to satisfy the unregister guard without creating a window handle. No live Outlook object and no WinForms message loop. |
| 4.6 | New test seams justified and minimal | PASS | `QfcDatamodel.QuiesceDebugLog` is an added `internal Action<string>` not named in the spec. The stated reason is verified: `QfcDatamodel` logs through log4net, no memory-appender convention exists anywhere in `QuickFiler.Test`, attaching one would mutate a process-global logger repository and break test independence, and the injected-delegate convention was already established by the gate's `debugLog` parameter. The same lines still reach log4net at INFO in production. Disclosed as deviation 2 in `spec.md`. |
| 4.7 | Coverage targets for new and changed methods | PARTIAL | 90.8% of executable changed lines are covered against a `>= 90%` target for new and changed code, which the target meets. At whole-file granularity two modified files sit below the 85% per-file floor; see the disposition in section 1.2.1. The two `QfcDatamodel` partials are structurally unmeasurable and carry named passing tests as substitute evidence for each changed member; this reviewer confirmed all five substitute tests are recorded `PASS-AFTER`. |

## 5. Test Coverage Detail

All figures below were produced by this reviewer directly from the Cobertura XML, not read from a
delivery artifact.

### 5.1 Repo-wide, first-party (nine production assemblies)

First-party allowlist: `Tags`, `ToDoModel`, `TaskVisualization`, `UtilitiesCS`, `QuickFiler`,
`TaskTree`, `TaskMaster`, `SVGControl`, `VBFunctions`. Vendor packages present in the document
(`log4net`, `Microsoft.IO.RecyclableMemoryStream`, `Mono.Reflection`, `System.Interactive`,
`System.Linq.Async`) are excluded from numerator and denominator.

- `coverage/791-baseline.cobertura.xml`, selection `classes/class/lines/line`: 55587/65783 lines = 84.5013%, 13204/16684 branches = 79.1417%.
- `artifacts/csharp/coverage.xml`, same selection: 55783/66009 lines = 84.5081%, 13292/16784 branches = 79.1897%.
- Delta: +196 covered lines against +226 valid lines; +88 covered branches against +88 valid branches.

The whole-document root attributes read `lines-covered="58527" lines-valid="83181"` (70.36%), which
includes the five vendor packages and is not the policy figure.

### 5.2 Per-package, post-change

- `QuickFiler` 10138/12610 lines = 80.40%, 2409/3121 branches = 77.19%.
- `UtilitiesCS` 38844/43780 = 88.73%, 9228/11111 = 83.05%.
- `TaskVisualization` 1445/1607 = 89.92%. `Tags` 710/766 = 92.69%. `TaskTree` 296/310 = 95.48%.
- `TaskMaster` 2395/3204 = 74.75%. `ToDoModel` 1074/1874 = 57.31%. `SVGControl` 877/1854 = 47.30%.

`QuickFiler`, the package this change touches, is at 80.40% line and 77.19% branch. The branch figure
clears the 75% floor; the line figure is below 85% and is pre-existing.

### 5.3 Per changed production file

Reproduced in the table under section 1.2.1. Summary: five measurable files, all five improved or
held both metrics; two of the five sit below the 85% per-file line floor and both are Outlook-Interop
event-handler surfaces inside the ratified `CLAUDE.md` UT2 exemption class (c). Two files are
structurally unmeasurable because `QfcDatamodel.cs:25` carries a type-level
`[ExcludeFromCodeCoverage]` that predates this branch; this reviewer confirmed both documents emit
**zero** `class` elements for both partials, so the condition is not one this branch introduced.

### 5.4 Changed-line no-regression gate

`CHANGED-LINES-WITH-COVERAGE-REGRESSION: 0`. 294 changed lines across the five measurable paths, 163
non-executable, 131 executable, 12 with zero hits (90.8% covered). Seven lines had an equal-count
hunk and therefore a one-to-one baseline mapping; none lost coverage. The remaining changed lines are
pure insertions with no baseline counterpart and are correctly recorded `baseline=none` rather than
attributed borrowed coverage. This reviewer's per-file percentages are consistent with that
determination: no file's line or branch rate fell.

### 5.5 New code coverage

New/changed-code coverage: **90.8%** lines. The 12 uncovered executable lines are named individually
and each was checked against the code by this reviewer: `EventHandlers.cs:139-141` (the UI
`SynchronizationContext` marshal, which needs a WinForms message loop the headless policy forbids),
`EventHandlers.cs:160-163` (the catch around the awaited quiesce, reachable only if the interface
contract that `QuiesceLoaderAsync` never throws for timeout is violated), `EventHandlers.cs:289` (a
`log.Debug` on the live-Outlook `MoveAndIterate` completion branch), and `QfcHomeController.cs:382-385`
(the catch around a `BackgroundWorker` event-handler detach, which has no seam that can be made to
throw). All four classes are host-bound or contract-defence, not omitted coverage.

## 6. Test Execution Metrics

Rendered as bullets rather than a table so no second table in this document has a language-like first
column.

- Baseline, nine first-party assemblies at the merge base (`evidence/baseline/p0-t11-coverage.md`, `p0-t10-quickfiler-tests.md`): 7000 total, 7000 passed, 0 failed; `QuickFiler.Test` alone 1339/1339/0.
- Fail-before, gate class (`evidence/regression-testing/p1-t16-gate-fail-before.md`): exit 1, 12 failures.
- Fail-before, Cancel teardown class (`p1-t17`): exit 1, 6 failures.
- Fail-before, home cleanup class (`p1-t18`): exit 1, 2 failures.
- Fail-before, datamodel teardown class (`p1-t19`): exit 1, 5 of 5 failures, including the reported `System.ArgumentException` reproduced verbatim.
- Pass-after, six affected classes (`p2-t14`): exit 0, 76 total, 76 passed; all 25 inventory names recorded `PASS-AFTER`; re-run verbatim against the final build after the `[P2-T15]` repair with identical counts.
- Post-change, whole `QuickFiler.Test` assembly (`p2-t15`): exit 0, 1362/1362/0, `NEWLY-FAILING: NONE`, +23 against the 1339 baseline.
- Final gate run, nine assemblies with coverage (`evidence/qa-gates/p3-t5-tests-coverage.md`): exit 0, 7023 total, 7023 passed, 0 failed, +23 against the 7000 baseline.
- This reviewer's independent re-run of `QuickFiler.Test` at head: `Test Run Successful. Total tests: 1362`, exit 0.
- Test selection deviation, applied identically to baseline and final so the comparison is like-for-like: `/TestCaseFilter:TestCategory!=LiveOutlook` plus exclusion of four `UtilitiesCS.Test` shell-icon classes that stall `vstest` on this machine. Recorded as Observation N14.

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Format check | `dotnet tool run csharpier check .` | PASS — `Checked 1587 files in 4202ms`, exit 0, re-run by this reviewer |
| Analyzer build | `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | PASS — exit 0, re-run by this reviewer |
| Nullable build | `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` | PASS — `0 Warning(s) 0 Error(s)`, exit 0, re-run by this reviewer |
| Banned symbol scan (added lines) | regex scan of all 1671 added `.cs`/`.csproj` lines | PASS — 0 hits for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared` |
| Suppression scan (added lines) | regex scan of all added lines | PASS — 0 `SuppressMessage`, 0 `#pragma warning disable`, 0 `ExcludeFromCodeCoverage` |
| Temp-file scan (added lines) | regex scan of all added lines | PASS — 0 `GetTempPath`, 0 `GetTempFileName` |
| Confidentiality masking scan | grep of the feature folder for the account name and absolute host paths | FAIL — one occurrence, `runbooks/live-outlook-cancel-teardown-verification.runbook.md:16`; 0 occurrences in any added source line |
| Workflow change scan | `git diff --name-only 7c8ac9ae..HEAD` filtered to `.github/workflows/**`, `.github/actions/**`, `scripts/benchmarks/**` | PASS — 0 matching paths, so the modified-workflow-needs-green-run rule does not fire |
| Scope boundary scan | `git diff --name-only` over `'*.cs' '*.csproj'` | PASS — 17 paths, exactly the Write Set plus test files under `QuickFiler.Test/Controllers` and the test `.csproj`; all five named exclusions absent |
| File size scan | line count of every changed `.cs` file at head | PASS — maximum 498, no `.cs` file over 500 |
| Test file size scan | line count of every changed test `.cs` file at head | PASS — maximum 498 (`QfcStreamingDequeueConfidenceGateTests.Part2.cs`) |
| Working tree cleanliness | `git status --porcelain --untracked-files=all` | PASS — empty before and after this review |

## 8. Gaps and Exceptions

### PA-1 — Repository-wide C# line coverage is 84.51%, below the 85% uniform floor (FAIL row; non-blocking)

`.claude/rules/quality-tiers.md` and `.claude/rules/general-unit-test.md` set a uniform 85% line
floor. `CLAUDE.md` UT2 sets 80% against a testable denominator. The measured figure passes one and
fails the other. This audit reports the stricter figure as FAIL and dispositions it non-blocking
because the branch raises it from 84.50% to 84.51% and covers 86.7% of the executable surface it
adds. The 80-versus-85 divergence is a pre-existing documentation conflict on `origin/main` and is not
this branch's to resolve.

### PA-2 — Two modified production files sit below the 85% per-file line floor (FAIL rows; non-blocking)

`QfcFormController.EventHandlers.cs` at 58.12% and `QfcHomeController.cs` at 76.36%. Both improved
(from 49.61% and 75.85%), neither regressed on any changed line, and both are Outlook-Interop
event-handler surfaces inside the maintainer-ratified `CLAUDE.md` UT2 exemption class (c). No
remediation is recommended: the uncovered remainder is host-bound code with no injectable seam, and
the correct long-term response is the extraction the Coverage Exclusion Policy describes, not a test
that fakes a WinForms message loop.

### PA-3 — New production code lands inside a type excluded from coverage measurement (Advisory; pre-existing repository condition)

115 added lines land in `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, part of a type
carrying `[ExcludeFromCodeCoverage]` at `QfcDatamodel.cs:25`. This reviewer confirmed both the
baseline and the post-change Cobertura emit zero `class` elements for both partials, so the attribute
is pre-existing and the branch neither added nor extended it. The consequence is that
`QuiesceLoaderAsync`, `LogQuiesceOutcome`, `TryQueueRemainingMailItemAsync`,
`TryCreateRemainingQueueAdmission` and the `Worker_DoWork` capture are outside the coverage
denominator. Substitute evidence exists and was verified: five named tests, each recorded
`PASS-AFTER`, cover each changed member. `.claude/rules/general-unit-test.md`'s Coverage Exclusion
Policy ("No production file may be excluded from coverage measurement") and `CLAUDE.md` UT2's ratified
`[ExcludeFromCodeCoverage]` exemption are in direct conflict on this point; the conflict pre-exists
this branch. Recommended follow-up: promote the extraction of host-neutral queue logic out of
`QfcDatamodel` to a tracked issue.

### PA-4 — Coverage collected with `dotnet-coverage`, not `vstest /EnableCodeCoverage` (Advisory; correctly justified)

Disclosed by the delivery as deviation 4. `/EnableCodeCoverage` writes a binary `.coverage` file
rather than the Cobertura XML the same criterion requires at `artifacts/csharp/coverage.xml`, and the
two collectors conflict when combined. The substitution wraps the same `vstest.console.exe`, the same
nine assemblies, the same runsettings and the same switches, and both sides of the comparison were
produced by one collector and one configuration. This reviewer independently parsed the resulting
document and reproduced the delivery's derived percentages, which is the substantive check. Accepted.

### PA-5 — Human-interaction exception HI-1 is outstanding

AC2 states that the live-Outlook confirmation (keyboard usable after Cancel, new Cancel-stage log
lines present, no null-`this` loader error) is a human follow-up performed per
`runbooks/live-outlook-cancel-teardown-verification.runbook.md` and does not gate the automated
review. The exception is declared in `issue.md:102`, in `spec.md:257`, in the `## Next Step` checklist
at `issue.md:109`, and in `spec.md` Rollout & Follow-up. It is outstanding at review time. The AC2
check-off does not depend on it, and this audit does not treat it as a gap in the automated evidence.
It remains owed before the behavioral claim about the Outlook keyboard can be considered confirmed in
the field.

### PA-6 — Spec-declared follow-ups not yet promoted (Advisory; owed at PR time)

`spec.md` names issue #792 for the breadcrumb WebView2 initialization failure and it exists as a
promoted potential entry at
`docs/features/potential/promoted/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state.md`.
Three further defect classes surfaced by this review have no tracked issue: PA-3's coverage exclusion,
finding N1 (the disposed-but-not-nulled `_tokenSource`), and finding N2 (the unprotected
`_parentCleanup?.Invoke()` in `QfcFormController.Cleanup()`). Promoting them is owed at PR time.

### Deviations already disclosed by the executor and confirmed adequate

All five are stated by name in `spec.md` Rollout & Follow-up or in the referenced evidence artifact,
and each was independently checked against the code:

1. The `ActionCancelAsync` trigger discriminator is a call-site log line rather than a method
   parameter, because `IFilerFormController.cs:11` declares `Task ActionCancelAsync();` and that file
   is outside the Write Set. Verified: the interface file is absent from the diff and the call-site
   `log.Debug` exists at `EventHandlers.cs:289`.
2. `QfcDatamodel.QuiesceDebugLog` is an added internal test seam not named in the spec. Justification
   verified; see row 4.6.
3. The retargeting surface is seven tests rather than the four Test Strategy names. Verified by
   reading all seven diffs; each preserves its original intent against the new behavior.
4. Coverage collected with `dotnet-coverage`. See PA-4.
5. The two `QuiesceLoaderAsync` tests fail one step earlier than predicted, in Arrange rather than
   Act. Disclosed in `p1-t19`; both remain red before and green after, so the fail-before evidence is
   unaffected. This is weaker RED-first evidence than the other two classes carry, because an
   arrange-stage fail-closed guard is not the defect reproducing; the two tests that carry the
   substantive RED-first proof (`TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_...` and
   `Cleanup_CalledTwice_DoesNotThrow`) fail with the real exception types.

## 9. Summary of Changes

- **Production, 7 files, +332/-46 lines.** `QfcStreamingDequeueConfidenceGate.cs` turns the
  zero-acceptance deadline branch into a logged checkpoint bounded by a 250-candidate scan cap and a
  120-second ceiling, adds a launch log line carrying the cutoff, and adds three log helpers.
  `IQfcDatamodel.cs` adds the `ScanCapReached` stop reason and the `QuiesceLoaderAsync` contract.
  `QfcDatamodel.QueueProcessing.cs` adds `QuiesceLoaderAsync`, `LogQuiesceOutcome`, the relocated and
  guarded `TryQueueRemainingMailItemAsync`, the synchronous `TryCreateRemainingQueueAdmission`, the
  `_remainingLoadTask` field and the `QuiesceDebugLog` seam. `QfcDatamodel.cs` null-guards `Cleanup()`
  and captures the loader task in `Worker_DoWork`. `QfcFormController.Deactivate.cs` extracts
  `ParkFocusAndCancelSelectors()`. `QfcFormController.EventHandlers.cs` reorders `ActionCancelAsync`
  into ten logged stages under `finally` and stops `ButtonCancel_Click` rethrowing.
  `QfcHomeController.cs` rewrites `Cleanup()` as two guarded blocks under one `finally` that disposes
  the token source and detaches the worker-completed handler.
- **Tests, 9 files, +1035/-65 lines, +23 tests.** Four new files (gate Part4, Cancel teardown, home
  cleanup, datamodel teardown) and five retargeted files. Seven pre-existing tests that encoded the
  superseded #424/#608 behavior were retargeted rather than deleted.
- **Project files, 1.** Four `<Compile Include>` entries in `QuickFiler.Test/QuickFiler.Test.csproj`.
- **Documentation and evidence, 40 files.** `spec.md`, `user-story.md`, `issue.md`, the atomic plan,
  a research note, a runbook, two promoted potential entries, and 33 evidence artifacts under
  `<FEATURE>/evidence/`.
- **Agent memory, 6 files.** Notes carried by the task-researcher, atomic-planner and atomic-executor
  agents. These are documentation of agent behavior, contain no host paths and no credentials, and
  were audited rather than excluded from scope.

## 10. Compliance Verdict

**PASS. Blocking findings: 0.**

| Area | Verdict |
|---|---|
| General Unit Test Policy | PASS (one recorded determinism exception matching pre-existing convention) |
| General Code Change Policy | PASS (one non-blocking FAIL row: an absolute host path in one committed runbook line) |
| C# Code Change Policy | PASS |
| C# Unit Test Policy | PASS (one PARTIAL: per-file coverage floor on two exempted Outlook-Interop files) |
| Test Coverage | FAIL rows recorded and dispositioned non-blocking; no regression at any scope |
| Test Execution | PASS |
| Code Quality Checks | PASS (one FAIL row: confidentiality masking) |
| Evidence Location Compliance | PASS |
| Modified-workflow green-run rule | Does not fire — no workflow, action, or benchmark path changed |
| Acceptance Criteria (see `feature-audit.2026-09-06T15-31.md`) | 6 of 6 PASS |

Remediation was not triggered. No finding is blocking, no acceptance criterion is FAIL or PARTIAL, no
toolchain step failed, and coverage regressed at no scope. `remediation-inputs.2026-09-06T15-31.md`
was therefore not produced. The six Minor findings and ten Observations are recorded with concrete
recommendations in `code-review.2026-09-06T15-31.md`; three of them are named in PA-6 as owed
promotions at PR time.

**Go/no-go: GO for PR.**

## Appendix A: Test Inventory

Tests added by this branch, 23 in total.

`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` (7):
`DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance`,
`DequeueAsync_ZeroAcceptedAndSourceDrained_ReportsSourceExhausted`,
`DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached`,
`DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling`,
`DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts`,
`DequeueAsync_Launch_LogsCutoffQuantityAndBounds`,
`DequeueAsync_NonEmptyPrefix_UnchangedByCheckpoint`.

`QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs` (8):
`ActionCancelAsync_ResetsKbdActive_WhenKeyboardDialogActive`,
`ActionCancelAsync_DoesNotToggle_WhenInactive`,
`ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors`,
`ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup`,
`ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup`,
`ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup`,
`ButtonCancel_Click_ActionThrows_DoesNotRethrow`,
`ActionCancelAsync_CalledTwice_InvokesParentCleanupOnce`.

`QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` (2):
`Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup`,
`Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted`.

`QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs` (5):
`TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing`,
`QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout`,
`QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs`,
`Cleanup_CalledTwice_DoesNotThrow`,
`Worker_DoWork_CapturesRemainingLoadTask`.

`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` (1):
`IterateQueueAsync_EmptyBatchWithScanCapReached_DoesNotCompleteAdding`.

Tests retargeted rather than deleted, 7 in total:
`DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline` ->
`DequeueAsync_LowYieldStream_ContinuesPastDefaultDeadlineToTheQualifier`;
`DequeueAsync_DeadlineExpiresWithZeroAccepted_ReturnsEmptyListAtTheBound` ->
`DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesToSourceExhaustion`;
`DequeueAsync_AfterDeadlineReturn_StopsTakingAndLeavesUnscannedCandidates` ->
`DequeueAsync_AfterScanCapReached_StopsTakingAndLeavesUnscannedCandidates`;
`DequeueAsync_DeadlineExpiry_EmitsOneExpiryLineAndKeepsPerCandidateLogging` ->
`DequeueAsync_CheckpointExpiry_EmitsCheckpointLineAndKeepsPerCandidateLogging`;
`DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` ->
`DequeueAsync_ZeroAcceptedAndCapReached_ReportsScanCapReachedStop`;
`DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop` ->
`DequeueNextItemGroupWithOutcomeAsync_ZeroAcceptanceCeilingGate_ReportsScanCapReachedStop`;
`DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns` (name unchanged, bound rebased from
the 3 s deadline onto an injected scan cap of 3).

Architecture pin preserved unchanged:
`QfcMoveMonitorTopologyTests.NoTypeDeclaresMoreThanOneEmailMoveMonitorField` still expects exactly
three declaring types.

## Appendix B: Toolchain Commands Reference

Commands this reviewer executed, all read-only against tracked content:

```
git -C <repo-root> diff --numstat 7c8ac9ae34b8b3dda9134a5e310f39742fd2f0b6..59536368756d979f3f72268dfb4dfd0d4b2f7d9f
git -C <repo-root> log --oneline 7c8ac9ae..59536368
git -C <repo-root> diff 7c8ac9ae..59536368 -- <each changed production and test path>
git -C <repo-root> diff --name-only 7c8ac9ae..51b557df -- '*.cs' '*.csproj'
git -C <repo-root> status --porcelain --untracked-files=all
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
<vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:<scratch> /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:TestCategory!=LiveOutlook
```

`<vstest>` resolves to
`<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
The `Extensions\TestPlatform` binary is used deliberately rather than the `CommonExtensions`
`TestWindow` one, which drops the Moq binding redirect.

Coverage aggregation was performed with `System.Xml.XmlDocument` over
`artifacts/csharp/coverage.xml` and `coverage/791-baseline.cobertura.xml`, selecting
`classes/class/lines/line` per package and parsing `condition-coverage="… (h/t)"` for branches. The
same script was run against both documents so the two sides are computed identically.

Commands referenced from delivery evidence and not re-executed by this reviewer:

```
dotnet tool run csharpier format .
dotnet-coverage collect --output artifacts\csharp\coverage.xml --output-format cobertura --settings coverage\791-effective-coverage.config -- <vstest> <nine test assemblies> ...
```

# Policy Audit — uithread-dispatcher-null-race-progresstrackerasync (#584)

- **Component:** `UtilitiesCS.Threading.UiThread.Dispatcher` accessor contract
- **Date:** 2026-09-04
- **Timestamp:** 2026-09-04T04-05
- **Work Mode:** `full-bug` (from `issue.md` line 3) — AC source is `spec.md` only
- **Base / merge-base:** `87cb4df338322844abfa580abea14df77e738e5c`
- **Branch:** `bug/uithread-dispatcher-null-race-progresstrackerasync-584`
- **Files under test (6, complete branch diff):**
  1. `UtilitiesCS/Threading/UiThread.cs`
  2. `UtilitiesCS.Test/Threading/UiThread_Tests.cs`
  3. `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`
  4. `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`
  5. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`
  6. `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`

**BLOCKING FINDINGS COUNT (this artifact): 0**

---

## Executive Summary

The change replaces a silent-null return on the public static `UiThread.Dispatcher` getter with an
explicit `InvalidOperationException`, removes the `null!` null-forgiving suppression, and redeclares
the backing field `Dispatcher?` so nullable flow analysis verifies the guard. Four MSTest classes
that mutate the process-global `UiThread._dispatcher` static are moved out of the parallel bucket
with `[DoNotParallelize]`, and one reflective consumer in `QuickFiler.Test` is retargeted from the
now-throwing property to the backing field.

Verdict: **COMPLIANT** with documented non-blocking exceptions. All seven acceptance criteria are
supported by their cited evidence; several were independently re-derived by this review against the
worktree rather than accepted on the artifact's word. The full seven-stage toolchain is green in a
single final pass. Two non-blocking `FAIL` rows are recorded — repo-wide coverage below the uniform
floors, and the absence of the canonical coverage artifact path — both with dispositions explaining
why neither is a defect in this change.

### Method deviations recorded for this review

- **No shell commands were executed.** The delegating prompt prohibited the Bash tool on the basis
  of a prior four-hour unattended hang (this session's allow-list does not match `git -C <path>`
  forms, and the session cwd is a different checkout than the worktree under review). All verification
  was performed with file-read and content-search tooling against absolute worktree paths. Checks
  requiring command execution are marked UNVERIFIED with a reason; none of them is load-bearing for a
  verdict.
- **Policy-audit template MCP asset unavailable.** `mcp__drm-copilot__resolve_policy_audit_template_asset`
  is not exposed to this agent. Per the established fallback, this artifact is hand-authored while
  preserving all thirteen canonical major headings from
  `.claude/skills/policy-audit-template-usage/SKILL.md`. It is not marked BLOCKED.
  `mcp__drm-copilot__validate_orchestration_artifacts` was likewise unavailable; step 6 of that skill
  is UNVERIFIED.

## Rejected Scope Narrowing

None. The delegating prompt supplied the complete `87cb4df3..HEAD` name-status diff and directed a
full-branch audit. No instruction attempted to narrow scope to a plan, task, phase, or file subset,
and no language with changed files was marked out of scope. The prompt's guidance on point 1
("do not raise a blocking finding that rests on comparing" the raw and first-party coverage
denominators) was assessed on its merits and found factually correct; it is a statement about
denominator comparability, not a scope narrowing. The coverage row is still recorded as `FAIL`
below, with a non-blocking disposition, rather than suppressed.

## Evidence Location Compliance

No violations. The branch diff contains six source files and no path under `artifacts/baselines/`,
`artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. A directory listing of
`artifacts/**` in the worktree returns only pre-existing `pr_body_*`, `pr_context.*`, and
`orchestration/orchestrator-state.json` entries, none of which is an evidence artifact of this
feature. All 34 evidence artifacts for this feature are under the canonical
`docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/<kind>/` tree
(`baseline`, `regression-testing`, `qa-gates`, `other`, `issue-updates`).

`validate_evidence_locations.py --root .` was not executed (no-shell constraint). The scan above was
performed by directory enumeration instead and is complete for the four prohibited prefixes.

---

## 1. General Unit Test Policy Compliance

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 1.1 | Independence — order-independent | PASS | Both new tests in `UiThread_Tests.cs:133-177` capture the prior `_dispatcher` value and restore it in a `finally` block. `[DoNotParallelize]` on all four mutating classes (`p1-t5-donotparallelize.md`) removes concurrent interleaving, which capture/restore alone cannot address. |
| 1.2 | Isolation — one unit per test | PASS | Each new test exercises exactly one state of the `Dispatcher` getter (null field, populated field). |
| 1.3 | Fast execution | PASS | `p3-t2-regression-green.md`: 99 ms and 24 ms. Full `UtilitiesCS.Test` run 4787 tests; full `QuickFiler.Test` run 1312 tests, both green. |
| 1.4 | Determinism — no timing constructs | PASS | Independently verified by reading all 179 lines of `UiThread_Tests.cs`: no `Thread.Sleep`, `Task.Delay`, `SpinWait`, retry, `Timeout(`, or `PushFrame`. Corroborated by `p3-t5-no-timing-tokens.md` (94 added lines filtered, exit 1) and `p2-t4-emailmovemonitor-reflection-target.md` (2570-byte diff, exit 1). |
| 1.5 | Readability / documented intent | PASS | The new class carries a 15-line XML summary at `UiThread_Tests.cs:106-120` explaining why the backing field is the only viable seam. Test names state condition and expectation. |
| 1.6 | Arrange–Act–Assert | PASS | Both new tests are explicitly commented `// Arrange`, `// Act`, `// Assert`. |
| 1.7 | Clear failure messages | PARTIAL (non-blocking) | `UiThread_Tests.cs:138` asserts `field.Should().NotBeNull()`; the sibling test at `:164` omits that guard, so a future rename of `_dispatcher` surfaces as an `NullReferenceException` on `field.GetValue` rather than a named assertion failure. See finding F6. |
| 1.8 | No external dependencies | PASS | Reflection over an in-process static only. No network, DB, or external process. |
| 1.9 | No temporary files | PASS | No file I/O in any added test line. |
| 1.10 | Scenario completeness | PASS | Positive (populated field returns the same instance) and negative (null field throws, message asserted) are both covered. The state space of this accessor has exactly these two states. |
| 1.11 | Test file location mirrors source | PASS | `UtilitiesCS/Threading/UiThread.cs` -> `UtilitiesCS.Test/Threading/UiThread_Tests.cs`. |
| 1.12 | Coverage — line >= 85%, branch >= 75% | **FAIL (non-blocking)** | See section 5. Repo-wide raw figures 70.736% line / 46.79% branch. |
| 1.13 | No regression on changed lines | PASS | `p4-t7-coverage-delta.md`: 8 of 8 coverable added lines have `hits >= 1`. Changed-line coverage 100.0%. |
| 1.14 | Coverage Exclusion Policy — no production file excluded | PASS | The diff adds no `exclude` entry to `coverage.config` or any other configuration. No `[ExcludeFromCodeCoverage]` attribute is added. |
| 1.15 | Determinism infrastructure (no banned APIs in tests) | PASS | Verified by direct read. The pre-existing `new Thread(...)`/`Join()` in `EmailMoveMonitorTests.cs:281-291` is untouched by this change and is a marshal-target simulation, not a wall-clock wait. |

## 2. General Code Change Policy Compliance

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 2.1 | Simplicity first | PASS | The production change is one null guard plus a field-type change. No indirection introduced. |
| 2.2 | Reusability | PASS | The guard is placed at the single accessor rather than duplicated across the ~40 reading call sites — the explicit rationale in `spec.md` "Data flow and validation changes". |
| 2.3 | Extensibility / no gratuitous API break | PASS with call-out | `UiThread.Dispatcher`'s public type is unchanged (`Dispatcher`, non-nullable). The behavioural change (throw where null was returned) is a public-API behaviour break and is called out explicitly in `spec.md` "Backward-compatibility expectations", satisfying §7.2's requirement that a necessary break be stated clearly. Blast radius established — see section 8, finding B1. |
| 2.4 | Separation of concerns | PASS | Pure guard logic; no I/O added. |
| 2.5 | Error handling — fail fast and explicitly | PASS | `InvalidOperationException` with a message naming both `UiThread.Init()` and `UiThread.Initialize()`, matching the precedent at `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:62-66`. No silent swallow. |
| 2.6 | Logging | PASS (N/A) | No logging added or removed; existing catch sites already log generically. |
| 2.7 | Contracts / invariants enforced at access | PASS | The invariant previously asserted only in a trailing prose comment (`// set in Initialize() before any access`) is now enforced in code, and that comment is correctly removed. |
| 2.8 | Module cohesion | PASS | All six changed files remain single-purpose. |
| 2.9 | File size limit — 500 lines | **PARTIAL (non-blocking)** | `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` is 514 lines. See finding F3. The other five files are 172, 179, 348, 206, and 320 lines. |
| 2.10 | Naming | PASS | `DispatcherField`, `_dispatcher`, `UiThread_Dispatcher_Tests` follow repo convention. |
| 2.11 | Comment *why*, not *what* | PASS | The six added comment lines at `EmailMoveMonitorTests.cs:33-37` explain precisely why the field is read instead of the property (`PropertyInfo.GetValue` would surface the guard as `TargetInvocationException` from setup/teardown). This is a model instance of the rule. |
| 2.12 | Dependencies — no new packages | PASS | No `using` directive and no package reference added; `DoNotParallelize` resolves from the existing `Microsoft.VisualStudio.TestTools.UnitTesting` import in all four files. |
| 2.13 | I/O boundaries | PASS (N/A) | No I/O in the change. |
| 2.14 | Toolchain loop, in order, single clean pass | PASS | `p4-t8-loop-closure.md` records two Phase-4 passes chronologically. Pass 1 failed at P4-T6 (8 of 1312); P2-T4 then rewrote a tracked file, so every step was re-run in order. Pass 2 is green end to end with no tracked-file rewrite after P4-T1. This is the correct restart-from-step-1 behaviour, not a shortcut. |
| 2.15 | Bugfix workflow — failing regression test first | PASS | `p1-t4-expect-fail.md` records a genuine RED: `Failed: 1` with the verbatim FluentAssertions message "Expected a `<System.InvalidOperationException>` to be thrown, but no exception was thrown", at `UiThread_Tests.cs:150`, against a tree that `p1-t3-build-before-fix.md` had just built with `0 Error(s)`. The sibling positive test passed in the same run, proving the harness works and the red is attributable to the defect. This is a provable assertion-level RED-first, not a compile-red. |
| 2.16 | Minimal targeted fix, no opportunistic refactor | PASS | Production diff is confined to one property and one field declaration. `ProgressTrackerAsync.cs` was verified unmodified (`p3-t4-progresstrackerasync-unmodified.md`: empty `--cached` name-status and empty porcelain for that path). |
| 2.17 | Deeper design problems opened as issues, not widened scope | PARTIAL (non-blocking) | The `IUiDispatcher` seam conversion is deferred on the GitHub issue thread. The second follow-up (synchronizing `ProgressTrackerAsync_Tests.cs`'s reflective static mutation) exists only as feature-folder prose. See finding F5. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 3.1 | CSharpier format, pinned via `dotnet tool run` | PASS | `p4-t1-format.md` `EXIT_CODE: 0`, `Formatted 6 files`, byte-identical before/after unscoped porcelain. `p4-t2-format-check.md` `EXIT_CODE: 0`, `Checked 1576 files`, empty reported set, run over `.` (full repo, CI parity). |
| 3.2 | Analyzer build, `/t:Rebuild`, analyzers + code style enforced | PASS | `p4-t3-analyzer-build.md` `EXIT_CODE: 0`, `0 Warning(s)`, `0 Error(s)`. |
| 3.3 | Nullable / type-check build, `/t:Rebuild`, `TreatWarningsAsErrors` | PASS | `p4-t4-nullable-build.md` `EXIT_CODE: 0`, `0 Warning(s)`, `0 Error(s)`. Command verified to use `/t:Rebuild` and to contain no `Nullable=enable` substring, matching CLAUDE.md and `ci.yml` character-for-character. This gate is non-vacuous here: `UiThread.cs:1` carries `#nullable enable` (verified directly), so a getter returning `Dispatcher?` as `Dispatcher` without narrowing would raise `CS8603` and fail the build. |
| 3.4 | Null-safety by default | PASS | The `null!` suppression is removed. Independently verified: `UiThread.cs:149` reads `private static Dispatcher? _dispatcher;` and no `null!` remains in that file. |
| 3.5 | Strong contracts / explicit public API | PASS | Getter contract is now total: returns a non-null `Dispatcher` or throws. |
| 3.6 | MSTest / Moq / FluentAssertions | PASS | New tests use `[TestClass]`, `[TestMethod]`, and FluentAssertions (`Should().Throw<T>().WithMessage(...)`, `Should().BeSameAs(...)`). No xUnit or NUnit introduced. |
| 3.7 | Prefer `internal`, minimal public surface | PASS (N/A) | No new public member. |
| 3.8 | XML docs where contract is non-obvious | PASS | The new test class carries a full `<summary>`. The production getter's contract is self-evident from the guard and its message; no XML doc regression. |
| 3.9 | Suppressions narrow and documented | PASS | The change *removes* a suppression and adds none. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 4.1 | MSTest framework | PASS | `Microsoft.VisualStudio.TestTools.UnitTesting` throughout. |
| 4.2 | FluentAssertions preferred | PASS | All assertions in the added code are FluentAssertions. |
| 4.3 | Moq for mocking | PASS (N/A) | The unit under test is a static accessor; reflection over the backing field is the correct and pre-established seam (the identical idiom exists at `IdleAsyncQueue_Tests.cs:144`, `ProgressTracker_Tests.cs:421`, `ProgressTrackerAsync_Tests.cs:138`, `QfcItemController.UiThreadDispatcherFixture.cs:136`). |
| 4.4 | `vstest.console.exe` with coverage | PASS | `p4-t5-utilitiescs-tests.md` runs `dotnet-coverage collect --output-format cobertura -- vstest.console.exe ...`. Cobertura written to `coverage/p4-t5.cobertura.xml`, confirmed present on disk. |
| 4.5 | Assertion count / test count preserved on modified fixtures | PASS | `p2-t4-emailmovemonitor-reflection-target.md`: `[TestMethod]` count exactly 8 (unchanged); zero added or removed lines carry `.Should()`. Independently corroborated by reading all 320 lines of that file — the eight test bodies are intact and the sole assertion in the touched region, `current.Should().BeSameAs(_capturedDispatcher);` at line 65, is unaltered. |

## 5. Test Coverage Detail

### 5.1 Coverage artifact resolution

| Language | Changed files in branch diff | Canonical artifact | Present | Substitute used |
|---|---|---|---|---|
| C# | 6 | `artifacts/csharp/coverage.xml` | **No** | `coverage/p4-t5.cobertura.xml` (post-change) and `coverage/p0-t10.cobertura.xml` (baseline), both confirmed present on disk; figures transcribed into `p4-t5-utilitiescs-tests.md` and `p0-t10-utilitiescs-tests-coverage.md` |
| PowerShell | 0 | `artifacts/pester/powershell-coverage.xml` | No | not required |
| Python | 0 | `artifacts/python/lcov.info` | No | not required |
| TypeScript | 0 | `coverage/lcov.info` | No | not required |

### 5.2 Coverage verdicts

- **C# coverage: FAIL** — non-blocking disposition (see below).
- **PowerShell coverage: PASS** — not applicable; zero PowerShell files in the branch diff, so no coverage obligation is triggered.
- **Python coverage: PASS** — not applicable; zero Python files in the branch diff.
- **TypeScript coverage: PASS** — not applicable; zero TypeScript files in the branch diff.

C# detail:

| Metric | Baseline (P0-T10) | Post-change (P4-T5) | Floor | Row verdict |
|---|---|---|---|---|
| Repo-wide line rate | 0.7073317347831605 | 0.7073603942281368 | >= 0.85 | FAIL |
| Repo-wide branch rate | not recorded at baseline | 0.46792920353982304 | >= 0.75 | FAIL |
| `lines-covered` | 105901 | 105935 | — | +34 |
| `lines-valid` | 149719 | 149761 | — | +42 |
| Changed-line coverage (new code) | — | 100.0% (8 of 8) | >= 90% | PASS |
| Regression on changed lines | — | none | none | PASS |
| Line-rate regression | — | +0.0000286594 | none | PASS |

**Disposition — non-blocking.** Three independent reasons, each verified:

1. **Denominator non-comparability.** Both figures are raw, unstripped `dotnet-coverage` line rates
   for the entire `UtilitiesCS.Test` host process, including third-party and interop assemblies.
   CLAUDE.md's 80% and `quality-tiers.md`'s 85%/75% govern the repository's first-party testable
   denominator after stripping. The two are different denominators and a direct comparison is not
   valid evidence of a policy breach. This is a recurring, previously-confirmed property of raw
   all-assembly merges in this repository.
2. **Pre-existing.** The baseline figure at BASE `87cb4df3` is 0.70733, already below the floor. The
   shortfall predates the branch and is not caused by it. `p4-t7-coverage-delta.md` records this
   explicitly as `PRE-EXISTING FLOOR SHORTFALL`.
3. **Direction of travel.** The change moves the figure upward by +0.0000287 and achieves 100%
   coverage on the eight coverable added production lines, including all three lines of the new
   `throw` statement (`hits = 1`), proving the new failure branch is executed rather than merely
   compiled.

No remediation-inputs artifact is emitted on this row. The correct disposition is procedural
(the canonical stripped first-party figure is produced by the PR CI run), not a code defect in this
change.

### 5.3 Per-file coverage of new/modified code

| File | Classification | Coverage evidence |
|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | modified (production) | 8 of 8 coverable added lines (138-143, 145, 146) at `hits >= 1`. Added lines 137, 144, 149 carry no `<line>` element (accessor header, closing brace, field declaration) and are correctly excluded from the denominator as non-sequence-points. |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | modified (test) | Test code; excluded from the production denominator by policy. |
| `IdleAsyncQueue_Tests.cs`, `ProgressTrackerAsync_Tests.cs`, `ProgressTracker_Tests.cs` | modified (test, attribute-only) | Attribute-only; no executable line added. |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | modified (test) | Test code; six comment lines and one reflection lookup retargeted. |

## 6. Test Execution Metrics

| Run | Artifact | Total | Passed | Failed | Skipped | Exit |
|---|---|---|---|---|---|---|
| Baseline `UtilitiesCS.Test` | `p0-t10-utilitiescs-tests-coverage.md` | 4785 | 4785 | 0 | 0 | 0 |
| RED (pre-fix, 2 new tests) | `p1-t4-expect-fail.md` | 2 | 1 | **1** | — | 1 (expected) |
| GREEN (post-fix, 2 new tests) | `p3-t2-regression-green.md` | 2 | 2 | 0 | — | 0 |
| At-risk subset | `p3-t3-at-risk-tests.md` | 41 | 41 | 0 | 0 | 0 |
| RED (pass 1, `QuickFiler.Test`) | `p4-t6-first-pass-failure.md` | 1312 | 1304 | **8** | — | 1 |
| Final `UtilitiesCS.Test` | `p4-t5-utilitiescs-tests.md` | 4787 | 4787 | 0 | 0 | 0 |
| Final `QuickFiler.Test` | `p4-t6-quickfiler-tests.md` | 1312 | 1312 | 0 | 0 | 0 |

Test-count delta 4785 -> 4787 equals exactly the two added tests. `QuickFiler.Test` count is
unchanged at 1312, consistent with the attribute/reflection-only nature of that file's change.
Both final runs report an empty `FAILING_TEST_SET` and a `BASELINE_FAILURE_SET` that was already
empty, so no pre-existing failure is being masked.

The two at-risk "dispatcher unavailable" tests —
`AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` and
`YieldAsync_WithoutDispatcher_RemainsStrict` — both still pass against the throwing accessor,
confirming their existing handling absorbs `InvalidOperationException` as it previously absorbed
`NullReferenceException`. This is the empirical half of the blast-radius argument.

## 7. Code Quality Checks

| Check | Command | Result | Artifact |
|---|---|---|---|
| Format (apply) | `dotnet tool run csharpier format .` | exit 0, `Formatted 6 files` | `p4-t1-format.md` |
| Format (verify) | `dotnet tool run csharpier check .` | exit 0, `Checked 1576 files`, empty set | `p4-t2-format-check.md` |
| Analyze | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0, 0 Warning, 0 Error | `p4-t3-analyzer-build.md` |
| Type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0, 0 Warning, 0 Error | `p4-t4-nullable-build.md` |
| Test + coverage | `dotnet-coverage collect ... -- vstest.console.exe ...` | exit 0 | `p4-t5-*.md`, `p4-t6-*.md` |
| Architecture-boundary tests | — | UNVERIFIED | No dependency-cruiser / NetArchTest gate is configured for this solution; none exists to run. Not a gap introduced by this change. |
| Contract / schema checks | — | PASS (N/A) | No published contract or schema is touched. |
| Integration tests | — | PASS (N/A) | `TestCategory!=LiveOutlook` filter applied per repo standard; no integration surface changed. |

All toolchain results above are transcribed from committed evidence artifacts. This review did not
re-execute them (no-shell constraint) and did not need to: the evidence records exit codes, verbatim
summary blocks, and TRX `<Counters>` values, and the head-state facts they assert (field declaration,
`null!` absence, attribute placement, assertion counts, line counts) were independently re-derived
against the worktree and all matched.

## 8. Gaps and Exceptions

### B1 — Public-API behaviour break: blast radius assessment

The prompt asked whether the blast radius was actually established. It was, and this review
independently reproduced and then *extended* the census.

Three distinct routes can read `UiThread.Dispatcher`:

1. **Qualified member expression** (`UiThread.Dispatcher`). Enumerated by the plan's repo-wide
   `git grep -n "UiThread.Dispatcher\b"` and recorded in `spec.md` Root Cause Analysis. Every hit was
   checked against its nearest test coverage; each either sits inside a broad `catch (Exception)` or
   already fails unhandled with `NullReferenceException` and will now fail unhandled with
   `InvalidOperationException` — a strictly clearer failure of the same unhandled-ness, not a new one.
2. **Reflective property read.** This is the route the original census missed, and the miss
   *materialised* as a real regression (8 of 1312 in `EmailMoveMonitorTests`) rather than being
   theorised. `p0-t14-reflective-dispatcher-census.md` then ran the missing census across all nine
   test assemblies and repository-wide over `.cs`. **Independently reproduced by this review:** a
   repository-wide search for the string literal `"Dispatcher"` in `*.cs` now returns exactly four
   hits, all XML `<see cref="Dispatcher"/>` documentation cross-references
   (`WpfUiDispatcher.cs:14`, `WpfUiDispatcherTests.cs:14`, `ThreadMonitor.cs:25`,
   `IUiDispatcher.cs:13`). A search for `GetProperty(` with a `"Dispatcher"` operand returns **zero**
   hits. The single reflective property read is gone. A search for `"_dispatcher"` returns six hits,
   all reflective *field* reads, matching the census exactly plus the new test helper.
3. **`using static` import of `UiThread`.** The census did **not** enumerate this route. This review
   closed that gap: a repository-wide search for `using static .*UiThread` returns zero hits, so no
   file can read `Dispatcher` as an unqualified identifier. The census's conclusion therefore holds
   even accounting for the route it did not itself consider.

**Verdict on B1: the census supports the claim.** No production file reads `UiThread.Dispatcher`
reflectively, no production file depends on a silent-null outcome distinct from a generic-exception
outcome, and the one file that did depend on the old behaviour was found, repaired, and proven green
with a recorded fail-before/pass-after pair naming all eight test methods on both sides.

### F1 — Repo-wide C# coverage below the uniform floors — **FAIL, non-blocking**

Detailed in section 5.2. Line 70.736% vs 85%; branch 46.79% vs 75%. Raw unstripped process-wide
denominator, pre-existing at BASE, improved by this change, 100% changed-line coverage.

### F2 — Canonical C# coverage artifact absent — **FAIL, non-blocking**

`artifacts/csharp/coverage.xml` does not exist in the worktree. The substance of the requirement is
met by `coverage/p4-t5.cobertura.xml` and `coverage/p0-t10.cobertura.xml`, both present on disk, both
with their root-element figures transcribed into committed evidence, and the post-change document was
parsed per-class to produce the changed-line intersection in `p4-t7-coverage-delta.md`. Recorded as
FAIL per the absence rule; disposition non-blocking because the evidence the rule exists to obtain is
demonstrably present in an equivalent, verifiable form.

### F3 — `ProgressTracker_Tests.cs` exceeds the 500-line limit — **PARTIAL, non-blocking**

- File: `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, 514 lines.
- Rule: `.claude/rules/general-code-change.md` "File Size Limit" — 500 lines, test code not excepted.
- Evidence: `p0-t13-parallel-bucket-census.md` (baseline 514), `p2-t3-file-size.md` (head 514),
  `p4-t8-loop-closure.md` (post-format 514).
- **Disposition:** the overage is pre-existing at BASE at exactly 514 lines and the branch delta is
  **zero** — the change did not cross the cap and did not deepen the breach. Under the established
  crossing-versus-pre-existing severity split, a crossing is blocking and a pre-existing overage that
  is merely extended is non-blocking; this case is weaker still, since nothing was added. Recorded
  prominently rather than passed silently. Recommendation: extract a cohesive group of test methods
  into a `partial class` in a sibling file as a separate follow-up, with no test weakening.

### F4 — PR context artifacts are stale and describe a different feature — **PARTIAL, non-blocking**

`artifacts/pr_context.summary.txt` reports `Head SHA: e5dcbffd6a51e9f92869c390e85d179400657cd5`,
`Head ref (resolved): bug/invoke-mstestwithcoverage-threshold-before-setcontent-565`, and
`Merge base: 87233f867ad60c0a5c0d19b09cc121ae536d7ba1`. None of these belongs to this feature: the
branch is `bug/uithread-dispatcher-null-race-progresstrackerasync-584` and the merge base is
`87cb4df338322844abfa580abea14df77e738e5c`. Its appendix pointer additionally names a different
worktree. These files are tracked and were carried in from `main` by another cohort item; the six-file
branch diff does not touch them.

Regeneration was not possible in this session (no-shell constraint, and the PR-context MCP tool is
not exposed to this agent). Scope was therefore derived from the two legitimate authoritative
sources that remained: the resolved base branch `87cb4df3` and the orchestrator-supplied, directly
verified `git diff --name-status 87cb4df3..HEAD` name-status listing, cross-checked against the
plan's declared write set, `p3-t4-progresstrackerasync-unmodified.md`'s staged footprint, and direct
on-disk reads of all six files. The audit scope is the full branch diff; this deviation narrows
nothing.

### F5 — Deferred follow-up recorded only as feature-folder prose — **Minor, non-blocking**

`spec.md` "Rollout & Follow-up" item 1 (add synchronization or an `IUiDispatcher` seam around
`UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`'s unsynchronized reflective mutation of
`UiThread._dispatcher`, mirroring the fixture-level fix applied for #493 in `QuickFiler.Test`) exists
only in this feature folder, which is removed on merge. Item 2 (the `IUiDispatcher` seam conversion)
is already durable on the GitHub issue thread. Recommendation: promote item 1 to a GitHub issue
before merge so the residual survives.

### F8 — `[DoNotParallelize]` census scope — **Low, non-blocking, closed by this review**

`p1-t5-donotparallelize.md` establishes that "zero writers of that field remain in the parallel
bucket", but its census is over the string literal `"_dispatcher"`, i.e. reflective writers only. It
does not enumerate the *production* writer path: the `UiThread.UiSyncContext` and
`UiThread.AutoScaleFactor` getters call `Init()` when their own backing field is null, and `Init()`
runs `Initialize()`, which assigns `Dispatcher`. A parallel-bucket test that reached either property
could therefore write `_dispatcher` concurrently.

This review verified the gap is closed in fact:

- No file in `UtilitiesCS.Test` reads `UiThread.Init(`, `UiThread.UiSyncContext`, or
  `UiThread.AutoScaleFactor` directly. The only matches are documentation comments and the two new
  `UiThread.Dispatcher` reads in the new tests.
- The two production readers of `UiThread.UiSyncContext` are `ThreadMonitor.cs:143` and
  `FolderPredictor.cs:178`. `ThreadMonitorTests` already carries `[DoNotParallelize]`
  (`ThreadMonitorTests.cs:18`). The test that drives `FolderPredictor.cs:178`,
  `FolderPredictorTests.EnterUiContextAsync_WhenUiSyncContextPostsSynchronously_CompletesUsingDefaultAction`,
  sets `_uiSyncContext` reflectively at `FolderPredictorTests.cs:479` before the call, so the lazy
  `Init()` branch is never taken.
- The two `UiThread.AutoScaleFactor` readers are WinForms viewer paint paths not exercised in
  `UtilitiesCS.Test`.

The isolation guarantee therefore holds, but it holds partly by coincidence of an unrelated test's
arrangement rather than by construction. Informational; no action required in this change.

### UNVERIFIED items

| Item | Reason |
|---|---|
| `validate_evidence_locations.py --root .` execution | No-shell constraint. Substituted by a complete directory enumeration of the four prohibited `artifacts/` prefixes, which found nothing. |
| `mcp__drm-copilot__validate_orchestration_artifacts` on this artifact | MCP tool not exposed to this agent. Canonical headings preserved manually. |
| Architecture-boundary test stage | No such gate is configured for this solution. |
| Independent re-execution of csharpier / msbuild / vstest | No-shell constraint. All head-state *facts* those gates assert were independently re-derived by direct file read and matched. |

## 9. Summary of Changes

| File | Nature | Lines (base -> head) |
|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | Production fix: null guard in the `Dispatcher` getter; `null!` removed; backing field -> `Dispatcher?` | 163 -> 172 |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | New `UiThread_Dispatcher_Tests` class: two deterministic regression tests + `DispatcherField()` helper | 104 -> 179 |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | Attribute-only: `[DoNotParallelize]` | 347 -> 348 |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | Attribute-only: `[DoNotParallelize]` | 205 -> 206 |
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | Attribute-only: `[TestClass, DoNotParallelize]` (combined form, no line added) | 514 -> 514 |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | Reflective snapshot retargeted property -> field; six explanatory comment lines | 314 -> 320 |

Six files. No production file other than `UiThread.cs`. No path under `.claude/`, `.codex/`,
`.agents/`, or `config/`.

## 10. Compliance Verdict

**COMPLIANT — with documented non-blocking exceptions.**

**BLOCKING FINDINGS: 0**

| ID | Severity | Blocking | Summary |
|---|---|---|---|
| F1 | FAIL | No | Repo-wide C# line 70.736% / branch 46.79% below 85%/75% floors (raw unstripped denominator; pre-existing; improved by this change) |
| F2 | FAIL | No | Canonical `artifacts/csharp/coverage.xml` absent; equivalent Cobertura documents present and parsed |
| F3 | PARTIAL | No | `ProgressTracker_Tests.cs` 514 lines > 500; pre-existing at BASE, branch delta 0 |
| F4 | PARTIAL | No | `artifacts/pr_context.*` stale, describe issue #565; scope derived from verified git range instead |
| F5 | Minor | No | Deferred follow-up recorded only as feature-folder prose; promote to a GitHub issue |
| F8 | Low | No | `[DoNotParallelize]` census covered reflective writers only; indirect production writer path verified closed by this review |

No remediation-inputs artifact is produced: no finding is remediation-required.

---

## Appendix A: Test Inventory

New tests (2), `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, class `UiThread_Dispatcher_Tests`
(`[TestClass]`, `[DoNotParallelize]`):

| Test | Scenario | Assertion |
|---|---|---|
| `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` (`:134`) | Negative / error-handling. `_dispatcher` forced null via reflection, restored in `finally`. | `act.Should().Throw<InvalidOperationException>().WithMessage("*UiThread.Initialize()*")` |
| `Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance` (`:161`) | Positive. `_dispatcher` set to a known instance, restored in `finally`. | `UiThread.Dispatcher.Should().BeSameAs(expected)` |

Helper: `DispatcherField()` (`:125`) — `typeof(UiThread).GetField("_dispatcher", NonPublic | Static)`.

Modified test classes (attribute-only, assertions untouched): `IdleAsyncQueue_Tests`,
`ProgressTrackerAsync_Tests`, `ProgressTracker_Tests`.

Modified test class (reflection target only, assertions untouched): `EmailMoveMonitorTests` —
8 `[TestMethod]` before and after, all 8 named as passing in `p4-t6-quickfiler-tests.md`.

## Appendix B: Toolchain Commands Reference

```text
1. dotnet tool run csharpier format .
   dotnet tool run csharpier check .
2. msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" \
     /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
3. msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" \
     /p:TreatWarningsAsErrors=true
4. dotnet-coverage collect --output coverage/p4-t5.cobertura.xml --output-format cobertura \
     --settings coverage.config -- vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll \
     /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /Logger:trx \
     /ResultsDirectory:TestResults/p4-t5 /TestCaseFilter:TestCategory!=LiveOutlook
   vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation /Logger:trx \
     /ResultsDirectory:TestResults/p4-t6 /TestCaseFilter:TestCategory!=LiveOutlook
```

Verified: `/t:Rebuild` used in both msbuild steps (not the vacuous warm `/t:Build`), and no
`/p:Nullable=enable` is present, matching CLAUDE.md and `.github/workflows/ci.yml`.

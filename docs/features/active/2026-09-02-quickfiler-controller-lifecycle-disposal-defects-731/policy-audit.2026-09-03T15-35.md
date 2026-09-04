# Policy Audit — issue #731 (quickfiler-controller-lifecycle-disposal-defects)

- Timestamp: 2026-09-03T15-35
- Reviewer: feature-review agent
- Component: QuickFiler controllers — collection controller, queue, form-controller lifecycle/disposal surface
- Branch: `bug/quickfiler-controller-lifecycle-disposal-defects-731`
- HEAD: `c55bfad20ebb9427b6b91fcfe4bb618091bb45a6` (working tree clean)
- Diff base (merge-base with `origin/main`, independently recomputed): `35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e`
- Work mode: `full-bug` (marker at `issue.md:12`) — `spec.md` is the sole acceptance-criteria source
- Commit under audit: `c55bfad2` `fix(quickfiler): correct controller lifecycle and disposal defects (#731)`, authored 2026-09-03 15:24:22 -0400

## Template Provenance Deviation

`policy-audit-template-usage` requires the template to be resolved through the MCP tool
`mcp__drm-copilot__resolve_policy_audit_template_asset`. No MCP tool is exposed to this agent in
this session, so the artifact is hand-authored while preserving all twelve canonical major
headings the skill enumerates. `mcp__drm-copilot__validate_orchestration_artifacts` is likewise
unavailable and was not run. This is a provenance deviation, not a content omission.

## Scope Statement

The audit scope is the full branch diff against the recomputed merge-base, not any plan, phase or
task subset. The delegating prompt supplied no scope narrowing; nothing was rejected. The
`.claude/agent-memory/**` paths that the executor's own `[P5-T9]` scope gate subtracts under an
"AGENT-MEMORY ALLOWANCE" were **not** excluded from this audit; they are audited in section 9.

Branch diff: 54 paths (12 `.claude/agent-memory/`, 12 source/project, 30 feature-folder documents
and evidence artifacts). Derived from
`git diff --numstat 35583f7c…HEAD` in the review worktree.

### PR Context Artifact Staleness (recorded deviation)

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` are **tracked** files in
this repository and both copies reachable from this session describe other branches:

| Location | Head ref recorded | Head SHA recorded | Base recorded |
|---|---|---|---|
| review worktree | `bug/invoke-mstestwithcoverage-threshold-before-setcontent-565` | `e5dcbffd` | `87233f86` |
| session cwd | `bug/terminal-notification-hook-test-lacks-sync-barrier-751` | `6606fc39` | `f8414ee9` |

Neither pair describes `bug/quickfiler-controller-lifecycle-disposal-defects-731` @ `c55bfad2`.
Regenerating over a tracked file would create an unrelated working-tree modification in a
worktree this agent must not mutate, so scope and evidence were derived directly from `git` with
the independently recomputed merge-base and this deviation is recorded here instead. Consequence
for downstream tooling: the PR-author flow and the `validate-feature-review-coverage.ps1` hook read
this file, so it must be regenerated before `gh pr create`.

## Executive Summary

**Verdict: PASS. Zero Blocking findings.**

All four code findings and the one evidence-only finding are delivered as designed in `spec.md`.
19 of 19 acceptance criteria verified independently as PASS (three of them with a recorded, already
disclosed deviation). The mandatory toolchain passed in the documented order with zero warnings and
zero errors at every stage, and the full suite is green at 6995/6995 against a baseline of
6985/6985 — a delta of exactly the ten planned net-new methods, over an identical nine-assembly set.

Independent verification highlights:

- The repository-wide and per-file coverage figures were **re-derived by this reviewer** from the
  raw Cobertura documents rather than read from the executor's artifacts, and reproduce exactly.
- The fail-before evidence for findings 2, 3 and 4 is a genuine reproduction in each case; the
  finding-2 artifact carries the real `ObjectDisposedException` stack through
  `BlockingCollection.get_IsCompleted` at `QuickFiler/Controllers/QfcFormController.Actions.cs:322`.
- The "CSharpier mandates a blank line above the inserted comment" claim, which the executor used
  to widen a numeric diff bound, was verified by reading the two probe logs the executor retained;
  both reproduce, including the `///` doc-comment variant.
- Every design claim underpinning the rejection of the shared-monitor option was checked against
  source: `BeforeItemMove` does dispatch at most one action via `FirstOrDefault`
  (`EmailMoveMonitor.cs:216-223`), `UnhookAll` is instance-scoped (`:189-204`), and the three owners
  do register three distinct actions across 16 live call sites.
- Zero host-identity or account-name leaks anywhere in the branch diff.

Nine advisory findings are recorded in section 8. None blocks merge. The two most substantive are
a private production field that production never reads (CR-1) and a triplicated source-inspection
helper across the three new test files (CR-2).

## 1. General Unit Test Policy Compliance

Reference: `.claude/rules/general-unit-test.md`, CLAUDE.md § General Unit Test Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence — tests run in any order | PASS | The three new files hold no shared mutable state. `QfcCollectionControllerDefects468Tests` is a `partial` continuation of a type that already owns the static-counter reset fixture; the added method is source-inspection only and mutates nothing. |
| Isolation — one unit of behaviour per test | PASS | Each of the ten new methods asserts a single scenario; names state the scenario and expected outcome. |
| Fast execution | PASS | Filtered runs recorded 1–194 ms per method; full suite 39.66 s for 6995 tests. |
| Determinism — no flakiness | PASS | Clock is `FakeTimeProvider`; the consumer is started inline via the `UndoConsumerStarter` seam; the item processor is an inert `Task.CompletedTask`. No thread racing, no wall-clock wait. |
| Readability and maintainability | PASS | Every method carries a `<summary>` in Scenario/Expected-outcome form and Arrange–Act–Assert comment banners. |
| Arrange–Act–Assert structure | PASS | All ten new methods use the explicit three-section banner form. |
| Clear, actionable failure messages | PASS | Every FluentAssertions call supplies a `because:` naming the issue and the invariant; several interpolate the observed value (e.g. the consumer's `AggregateException`). |
| No external services, no live COM, no shown WinForms form | PASS | Collaborators are Moq doubles of `IApplicationGlobals`, `IQfcFormViewer`, `IQfcQueue`, `IQfcHomeController`. `spec.md` line 71 forbids live COM; none is used. |
| No temporary files | PASS | The three new files perform `File.ReadAllText` on repository sources only; no file is created or written. |
| No mutable global state dependence | PASS | The only static state touched is the pre-existing reentrancy counter, whose reset fixture is unchanged. |
| Test file location mirrors production structure | PASS | All three land in `QuickFiler.Test/Controllers/`, mirroring `QuickFiler/Controllers/`. No colocation in the production tree. |
| Scenario completeness | PASS | Finding 2 covers running consumer, parked consumer, absent consumer, repeated invocation, and a faulted antecedent — positive, negative, edge and error-handling. |
| Banned APIs in test code (`Thread.Sleep`, `Task.Delay`, real waits) | PASS | Absent from all three new files; `Cleanup_SourceContainsNoSynchronousWait` additionally guards the production teardown path against them. |
| Fake-timer facility used for async tests | PASS | `Microsoft.Extensions.Time.Testing.FakeTimeProvider`, advanced explicitly by `clock.Advance(TimeSpan.FromSeconds(11))`. |
| Coverage tooling excludes test files | PASS | `coverage.config` drives the collection; no test assembly appears in the measured first-party denominator rows inspected. |
| Coverage Exclusion Policy — no production file excluded | FAIL (pre-existing, non-blocking) | See section 8, finding PA-1. Two production files carry class-level `[ExcludeFromCodeCoverage]` on `origin/main`. Not introduced or widened by this branch. |

## 2. General Code Change Policy Compliance

Reference: `.claude/rules/general-code-change.md`, CLAUDE.md § General Code Change Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow — failing regression test first | PASS | Findings 2, 3 and 4 each have a schema-valid `[expect-fail]` artifact with `EXIT_CODE: 1` / `ExpectedExitCode: 1` and a genuine diagnostic. Finding 1 has a schema-valid fail-before exception dossier at `evidence/regression-testing/fail-before-exception.finding1-topology-pin.md` explaining why no failing state exists (comment-only change) and proving the guard is discriminating rather than vacuous. |
| Bugfix workflow — minimal, targeted fix | PASS | Production delta is 44 insertions / 13 deletions across six files. No opportunistic refactor. |
| Bugfix workflow — verify locally before review | PASS | Full seven-stage-equivalent loop recorded; see section 7. |
| Simplicity first | PASS | The finding-2 fix reuses the stop signal the consumer loop already reads rather than introducing a cancellation token; finding 4 is a one-token change. |
| Reusability / avoid copy-paste | PARTIAL | See section 8, finding CR-2: `NormalizeWhitespace` is duplicated verbatim in three new test files, and two different repository-root resolution strategies appear across them. |
| Extensibility, non-breaking public API | PASS | `QfcRemainingQueueAdmission` is `internal sealed` with an `internal` constructor (`:8`, `:14`); both call sites updated in the same change. No public signature changed. |
| Separation of concerns | PASS | The fix keeps the disposal decision in the disposal partial and leaves the consumer loop in `QfcFormController.Actions.cs` untouched. |
| Fail fast, no silent error swallowing | PASS | The one added `catch (ObjectDisposedException)` is narrowly typed, carries a comment stating the exact re-entry it absorbs, and is the documented `CompleteAdding`-after-`Dispose` case. The change converts a previously **unobserved** consumer fault into a logged one. |
| Project logging pattern used | PASS | `logger.Error("Undo consumer faulted.", antecedent.Exception)` uses the file's existing log4net logger; no console output added. |
| Enforce invariants at construction | PASS | The three surviving `QfcRemainingQueueAdmission` parameters retain their `ArgumentNullException` guards; only the guard for the deleted parameter was removed. |
| Naming conventions | PASS | `_undoQueueDisposal`, `undoQueue`, `undoConsumer`, `antecedent` follow the repository's `_camelCase` field / `camelCase` local convention. |
| Comment *why*, not *what* | PASS | All four comment edits state rationale and cite the governing issues. |
| Module cohesion | PASS | Each new test file has one purpose; the partial-class split is justified in its own class comment. |
| **File size limit — 500 lines** | FAIL (pre-existing, non-blocking) | See section 8, finding PA-2. `QfcQueue.cs` 505 → 507 and `QfcCollectionController.cs` 2327 → 2329; both were already over the ceiling at the merge-base. All new and modified test files are at or under: 187, 399, 120, 371, 498, and the frozen 496. |
| I/O isolated from domain logic | PASS | No new I/O in production code. The test-side `File.ReadAllText` is confined to source-inspection helpers. |
| No new dependencies | PASS | No package reference added; `QuickFiler.Test.csproj` gains only three `<Compile Include>` entries. |
| Toolchain loop run in order, restarted on any change | PASS with advisory | See section 8, finding PA-3: one comment/blank-line formatter probe mutated and restored `QfcQueue.cs` five minutes after the passing loop concluded, and only `csharpier check` was re-run. Materially inert; verified below. |

## 3. Language-Specific Code Change Policy Compliance (C#)

Reference: CLAUDE.md § C# Code Change Policy, `.claude/rules/csharp.md`.

| Requirement | Verdict | Evidence |
|---|---|---|
| C#1.1 CSharpier via `dotnet tool run`, not a global install | PASS | `evidence/qa-gates/csharpier-format.md` and `csharpier-check.md` both record `dotnet tool run csharpier …`; check reports `Checked 1577 files`, 0 unformatted, `EXIT_CODE: 0`. |
| C#1.1 `dotnet format` not used | PASS | Absent from every recorded command. |
| C#1.2 Analyzer build uses `/t:Rebuild`, not `/t:Build` | PASS | `evidence/qa-gates/msbuild-analyzers.md` records the exact CLAUDE.md command with `/t:Rebuild`, and states why `/t:Build` would be vacuous. `EXIT_CODE: 0`, 0 warnings, 0 errors. |
| C#1.3 Nullable gate uses `/t:Rebuild` and omits `/p:Nullable=enable` | PASS | `evidence/qa-gates/msbuild-nullable.md` records the exact ci.yml command and explicitly asserts neither `Nullable=enable` nor `/t:Build` appears. `EXIT_CODE: 0`, 0 warnings, 0 errors. |
| C#2.2 Null-safety, explicit guards | PASS | `undoQueue?.CompleteAdding()`, `undoQueue?.Dispose()`, `undoConsumer is null`, `antecedent.Exception is not null` — every dereference guarded. |
| C#2.4 Asynchrony and resource safety | PASS | The fix removes a dispose-under-active-consumer; the continuation is scheduled on `TaskScheduler.Default`, so it cannot re-enter the UI context. No synchronous block anywhere on the teardown path. |
| C#3.3 Documented contracts and non-obvious side effects | PASS | `_undoQueueDisposal` carries an XML `<summary>` stating its purpose and why the disposal is deferred. |
| C#5.2 Public surface minimal and intentional | PARTIAL | See section 8, finding CR-1. |
| C#7 No new suppressions introduced | PASS | No `#pragma warning disable`, `[SuppressMessage]` or `NoWarn` added anywhere in the diff. |
| Analyzer diagnostics fixed, not suppressed | PASS | Both `/t:Rebuild` gates report 0 warnings against a `[P0-T7]`/`[P0-T8]` baseline of 0. |
| CS0420 avoided by design rather than suppressed | PASS | `spec.md:139` and `evidence/qa-gates/msbuild-nullable.md:42` both record that marking the field `volatile` would produce CS0420 at both `Interlocked` sites under `TreatWarningsAsErrors=true`; the fix uses `Volatile.Read` on an unmodified field instead. Verified in source: the declaration and both write sites are byte-unchanged in the diff. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

Reference: CLAUDE.md § C# Unit Test Policy.

| Requirement | Verdict | Evidence |
|---|---|---|
| CUT1 MSTest only; no xUnit/NUnit introduced | PASS | All three new files use `[TestClass]` / `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. |
| CUT2 Moq for mocking | PASS | `Mock<IApplicationGlobals>`, `Mock<IQfcFormViewer>`, `Mock<IQfcQueue>`, `Mock<IQfcHomeController>`, `Mock<MailItem>`. |
| CUT2 FluentAssertions preferred for assertions | PASS | Every assertion in the ten new methods is FluentAssertions. The one residual MSTest type, `AssertFailedException`, is **removed** by this change along with the throwing scorers it belonged to. |
| CUT3 Toolchain command selection | PASS | csharpier → analyzer Rebuild → nullable Rebuild → vstest with coverage, in order. |
| vstest run with coverage enabled | PASS | `Invoke-DotnetCoverageCollection` with `-CoverageConfig coverage.config` produced `coverage/postchange.cobertura.raw.xml`, post-processed to `…processed.xml`. Deviation from the packaged `Invoke-MSTestWithCoverage.ps1` script recorded in section 8, finding PA-4. |
| Existing tests treated as part of the spec | PASS | The issue-#286 reentrancy tests and the issue-#233 admission pin were preserved: the former unchanged (the only edit to that file is `class` → `partial class`), the latter replaced by a structural equivalent rather than deleted. |

## 5. Test Coverage Detail

All figures below were **re-derived by this reviewer** directly from
`coverage/baseline.cobertura.processed.xml` and `coverage/postchange.cobertura.processed.xml`
using the separator-anchored, `lines/line` + `methods/method/lines/line`, max-hits de-duplicated
rule (the descendant `.//line` axis was not used, per the issue-#441 double-count trap). They are
not quoted from the executor's artifacts. Every figure reproduces the executor's record exactly.

### Repository-wide

| Measure | Baseline | Post-change | Floor | Verdict |
|---|---|---|---|---|
| C# repo-wide line coverage | 85.4194% (55239/64668) | **85.4146%** (55253/64688) | >= 85% | **PASS** |
| C# repo-wide branch coverage | 79.5094% (13224/16632) | **79.5168%** (13230/16638) | >= 75% | **PASS** |

Repo-wide line coverage moved by −0.0048 percentage points. That is inside the cross-session
nondeterminism band this repository has previously measured for `dotnet-coverage` and is far inside
the 0.005-rate-unit bar the executor's `[P5-T6]` Branch A applied. Branch coverage improved.

### Per changed production file

| File | Status | Baseline line coverage | Post-change | Verdict |
|---|---|---|---|---|
| `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` | modified | 92.00% (23/25) | **100.00%** (20/20) | **PASS** |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | modified | 70.70% (111/157) | **74.73%** (136/182) | **FAIL** — below the 85% floor; non-blocking, see below |
| `QuickFiler/Controllers/QfcQueue.cs` | modified (comment only) | 50.32% (157/312) | 50.32% (157/312) | **FAIL** — below the 85% floor; non-blocking, pre-existing and byte-identical |
| `QuickFiler/Helper Classes/EmailMoveMonitor.cs` | modified (comment only) | 44.03% (70/159) | 44.03% (70/159) | **FAIL** — below the 85% floor; non-blocking, pre-existing and byte-identical |
| `QuickFiler/Controllers/QfcCollectionController.cs` | modified | uninstrumented | uninstrumented | see finding PA-1 |
| `QuickFiler/Controllers/QfcDatamodel.cs` | modified | uninstrumented | uninstrumented | see finding PA-1 |

No new production file is added by this branch, so the new-code tier has no member. The three
added files are test files and are outside the coverage denominator by policy.

**Non-blocking disposition for the three sub-floor rows.** Each is a pre-existing shortfall that
this change does not worsen:

- `QfcFormController.SetupDisposal.cs` improved by +4.03 percentage points. Its uncovered-line
  count is **unchanged at 46**: the file gained exactly 25 executable lines and all 25 are covered,
  so the covered count and the total both rose by 25. No pre-existing uncovered line was closed and
  none was newly opened. The residual is tracked under open issue **#683**, and `spec.md` AC15
  states in terms that reaching any percentage on this file is not a criterion of #731.
- `QfcQueue.cs` and `EmailMoveMonitor.cs` receive comment-only edits. Their per-line maps are
  identical between the two documents (312/157 and 159/70 respectively), so no changed executable
  line exists to regress.

### Changed-line no-regression gate

`evidence/qa-gates/coverage-delta.md` records `Comparable changed-line population: 0` and states in
bold that the zero regression count follows from an **empty population** and that the gate
"produced no coverage observation on this change … It did not find that there was no regression."
This reviewer verified the disclosure is present, unhedged, and repeated in three places
(lines 155, 197-201, 205-207). It is adequate; nothing in the artifact set implies a clean
no-regression result.

The reviewer independently confirms the emptiness is a property of the change shape:

- The only one-to-one-shaped changed executable line on the branch is the reentrancy guard, and its
  file is uninstrumented, so it has no `hits` value on either side.
- All 26 added executable lines in `QfcFormController.SetupDisposal.cs` arise from a
  one-line-replaced-by-many hunk (`@@ -218 +221,29 @@`) or a pure insertion (`@@ -206,0 +207,3 @@`),
  so none has a baseline counterpart. All 26 carry `post_hits=1`.
- `QfcRemainingQueueAdmission.cs` adds no line at all, and `QfcQueue.cs`/`EmailMoveMonitor.cs`
  add only comments.

**One correction to the delegating prompt's characterisation.** The prompt states that "[P5-T5]'s
absolute-floor branch and [P5-T6] Branch B defer their no-regression judgment to that gate, so no
no-regression signal is available from any of the three sources." The artifacts say the opposite,
and this reviewer confirms them: `[P5-T5]` recorded `Absolute floor result: PASS`, so its deferring
`FAIL` branch was never entered (`evidence/qa-gates/mstest-coverage.md:118-122`); and `[P5-T6]`
resolved Axis D to D-COMPARABLE and took Branch A, so its deferring Branch B was never entered
(`evidence/qa-gates/coverage-delta.md:45-57`). `evidence/qa-gates/coverage-delta.md:209-211`
records the reconciliation explicitly. **Neither deferral was taken, and no deferral landed on an
empty population.** A repository-wide admissible comparison therefore does exist and passed. What
is genuinely unavailable is a *per-changed-line* no-regression observation, which is a narrower and
less serious gap than the prompt described.

### Coverage artifact location

The canonical C# coverage artifact path `artifacts/csharp/coverage.xml` does not exist in either
worktree. The equivalent Cobertura documents exist at `coverage/baseline.cobertura.processed.xml`
and `coverage/postchange.cobertura.processed.xml` (12.7 MB each), were produced by this issue's
mandatory final QA gate, and were parsed directly by this reviewer to produce every figure in this
section. `coverage/` is gitignored at `.gitignore:144` with only `coverage/.gitkeep` re-included, so
the documents are not committed; the derived measurements are committed under
`<FEATURE>/evidence/qa-gates/`. Coverage verification was therefore performed against a present and
independently readable artifact, and the C# coverage verdict above is a substantive measurement
rather than an artifact-absence default.

### Coverage verdicts by language (full branch diff)

| Language | Changed files on branch | Repo-wide line coverage | Repo-wide branch coverage | Verdict |
|---|---|---|---|---|
| C# | 11 (`.cs`) plus one `.csproj` | 85.4146% | 79.5168% | **PASS** |
| PowerShell | 0 `.ps1`/`.psm1` files changed; no coverage obligation arises | — | — | **PASS** |
| Python | 0 `.py` files changed; no coverage obligation arises | — | — | **PASS** |
| TypeScript | 0 `.ts`/`.tsx` files changed; no coverage obligation arises | — | — | **PASS** |

The language enumeration is taken from the branch diff itself, not from the stale
`pr_context.summary.txt`. Changed-file extensions on the branch are `.cs` (11), `.csproj` (1) and
`.md` (42).

## 6. Test Execution Metrics

| Metric | Baseline (`[P0-T9]`) | Post-change (`[P5-T5]`) | Verdict |
|---|---|---|---|
| Total tests | 6985 | 6995 | PASS (delta +10 = exactly the planned net-new count) |
| Passed | 6985 | 6995 | PASS |
| Failed | 0 | 0 | PASS |
| Skipped | 0 | 0 | PASS |
| Test assemblies discovered | 9 | 9, identical set | PASS |
| Wall-clock | — | 39.6579 s | PASS |

Net-new decomposition verified against the delivered test files: 2 topology methods + 7 cleanup
methods + 1 volatile proxy method + 0 net from finding 3 (one method deleted, one added) = 10.
This reviewer counted the `[TestMethod]` attributes in the three new files and the two modified
files and reproduces the same total, which confirms that no pre-existing test was silently removed.

Filtered fail-before / pass-after runs recorded: finding 1 (dossier + 2 passing), finding 2 (3 of 7
failing before, 7 passing after), finding 3 (1 failing before, passing after), finding 4 (1 failing
before, passing after, plus the two issue-#286 tests still passing).

## 7. Code Quality Checks

| Gate | Command | Result | Artifact |
|---|---|---|---|
| Format (apply) | `dotnet tool run csharpier format .` | `EXIT_CODE: 0` | `evidence/qa-gates/csharpier-format.md` |
| Format (verify) | `dotnet tool run csharpier check .` | `EXIT_CODE: 0`, 1577 files, 0 unformatted | `evidence/qa-gates/csharpier-check.md` |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | `EXIT_CODE: 0`, 0 warnings, 0 errors | `evidence/qa-gates/msbuild-analyzers.md` |
| Type check / nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | `EXIT_CODE: 0`, 0 warnings, 0 errors | `evidence/qa-gates/msbuild-nullable.md` |
| Tests + coverage | vstest console over 9 assemblies with `coverage.config` | `EXIT_CODE: 0`, 6995/6995 | `evidence/qa-gates/mstest-coverage.md` |
| File size / diff bound | line counts + anchored numstat | recorded; see finding PA-2 | `evidence/qa-gates/file-size-audit.md` |
| Scope boundary | anchored `--name-status` + `--untracked-files=all` porcelain | PASS, residual = the 12-path write set exactly | `evidence/qa-gates/scope-boundary.md` |

Gate ordering verified by artifact timestamps: 14:27 format → 14:28 check → 14:29 analyzers →
14:30 nullable → 14:33 tests. Monotonic, no restart recorded, no interleaved source edit within
that window.

## 8. Gaps and Exceptions

### PA-1 — Two production files carry class-level `[ExcludeFromCodeCoverage]` (Advisory; pre-existing repository condition, **not** Blocking for this change)

- **Location:** `QuickFiler/Controllers/QfcCollectionController.cs:21`,
  `QuickFiler/Controllers/QfcDatamodel.cs:25`.
- **Provenance:** introduced by commit `a564add0`, 2026-06-13,
  `refactor(coverage): exempt COM/VSTO/WinForms code from coverage metric (#197)`. Both attributes
  are present on `origin/main` at the merge-base and are untouched by this branch.
- **The conflict.** `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy opens with
  "No production file may be excluded from coverage measurement," and its enforcement clause
  directs feature-review agents to treat a production-path exclusion as **Blocking**. CLAUDE.md
  § UT2 grants a "COM/VSTO/WinForms coverage exemption (testable denominator)" and states that the
  "Exemption is applied via `[ExcludeFromCodeCoverage]` attributes in source code (reviewable in
  PRs)," ratified by the project maintainer and tracked in `feature/csharp-coverage-uplift`.
- **Ruling.** **Not Blocking, for two independent reasons.**
  1. *Authority.* `policy-compliance-order` places CLAUDE.md at level 1 and `.claude/rules/*` at
     level 3. Where the two collide, CLAUDE.md governs, and CLAUDE.md's clause is the more specific
     one: it names the exact mechanism, the exact class of code, and a named ratifying authority.
  2. *Operative text.* The rules file's enforcement clause is written against coverage-tool
     configuration: it enumerates "Permitted `exclude` entries" and "Prohibited `exclude` entries"
     as glob paths (`dist/**`, `src/**`, `node_modules/**`, `jest.config.cjs`) and instructs the
     reviewer to treat "any `exclude` entry that matches a production source path" as Blocking. A
     C# source attribute is not an `exclude` entry in a coverage config. On the operative text the
     two documents do not collide; only their preambles do.
- **Scope of the ruling, stated explicitly as requested.** This is a **pre-existing repository
  condition**, not a defect of this change. It belongs in a separate documentation-reconciliation
  issue reconciling CLAUDE.md § UT2 against `.claude/rules/general-unit-test.md`, alongside the
  already-known 80/90-versus-85/75 threshold divergence between the same two documents. It does
  **not** belong in issue #731's remediation loop.
- **Material consequence, recorded rather than waived.** Because `QfcCollectionController.cs` is
  uninstrumented, the finding-4 change — the one behavioural production edit to that file — has
  **no coverage observation of any kind**, neither covered nor uncovered. Its regression test is a
  source-inspection structural proxy that the author himself disclaims in `<remarks>` as "not a
  proof that the race is eliminated." Execution of the guarded method *is* nonetheless proven: the
  two issue-#286 reentrancy tests drive `RemoveSpecificControlGroupAsync` and pass. The residual
  unverified property is memory ordering alone, which no deterministic unit test can establish.

### PA-2 — Two production files exceed the 500-line ceiling and each grew by two lines (FAIL row; non-blocking)

- **Location:** `QuickFiler/Controllers/QfcQueue.cs` 505 → **507**;
  `QuickFiler/Controllers/QfcCollectionController.cs` 2327 → **2329**. Line counts re-measured by
  this reviewer with `[System.IO.File]::ReadAllLines().Count` against both the merge-base blob and
  the HEAD working file.
- **Rule:** `.claude/rules/general-code-change.md` § File Size Limit — "No production code, test
  code, or reusable script file may exceed 500 lines." Comments and blank lines are not exempted.
- **Disposition: non-blocking.** Both files were already over the ceiling at the merge-base; the
  growth is two non-executable lines per file (one explanatory comment required by AC1, plus one
  blank line the formatter mandates above it). Splitting either is an explicit, spec-ratified
  non-goal (`spec.md:70`, `spec.md:250`). Blocking here would require the branch to discharge
  pre-existing debt it deliberately scoped out.
- **Sub-finding (Low).** `spec.md` § Scope & Non-Goals discloses `QfcCollectionController.cs`'s
  overage but is silent on `QfcQueue.cs`, and `spec.md` § Rollout & Follow-up records only the
  `QfcCollectionController` split. The plan's foot (`plan.2026-09-02T12-02.md:346`) does record the
  `QfcQueue` split. `evidence/qa-gates/file-size-audit.md:78` states the split is "recorded as a
  follow-up in spec.md and at the foot of the plan"; the spec half of that sentence is inaccurate.
- **Resolution:** record the `QfcQueue.cs` split as a potential entry / issue at PR time, and
  correct the one-clause inaccuracy in `file-size-audit.md`. Neither requires a code change.
- All new and modified test files are compliant: 187, 399, 120, 371, 498, and the frozen 496. The
  partial-class split of `QfcCollectionControllerDefects468Tests` to hold the finding-4 proxy is a
  correct response to the ceiling rather than a workaround of it.

### PA-3 — A formatter probe mutated and restored a source file after the passing toolchain loop (Advisory, Low)

- **Location:** `QuickFiler/Controllers/QfcQueue.cs`, last written 2026-09-03 14:38:25.
- **Observation:** the toolchain loop concluded with the test run at 14:33. Two CSharpier probes ran
  at 14:37:17 and 14:37:47 (`coverage/blankline-probe.log`, `blankline-probe2.log`), the file was
  restored at 14:38:25, and only `csharpier check` was re-run at 14:38:36
  (`coverage/p5t2-recheck.log`, `Checked 1577 files`). The analyzer build, the nullable build and
  the test run were **not** repeated. `.claude/rules/general-code-change.md` requires restarting the
  loop from step 1 after any stage changes files.
- **Impact: none demonstrable.** The mutation was confined to one comment and one blank line, which
  cannot affect IL, analyzer diagnostics, nullable flow or test outcomes. Independently verified by
  this reviewer: the `QuickFiler` rows of `coverage/p5t1-numstat-before.txt` /
  `p5t1-numstat-after.txt` (recorded 14:27, post-format, pre-probe) are byte-identical to the live
  numstat at HEAD, including `2  0  QuickFiler/Controllers/QfcQueue.cs`, and CSharpier reports the
  restored tree clean at the same 1577-file count.
- **Sub-finding:** `evidence/qa-gates/csharpier-check.md` records only the 14:28 run and does not
  mention the 14:38 re-check. The probe is disclosed only in `evidence/qa-gates/file-size-audit.md`
  (written 15:15). The disclosure exists but is not where a reader of the format gate would look.
- **Resolution:** in future runs, place formatter experiments before the final loop, or restart the
  loop after any post-pass tree mutation. No action required on this branch.
- **Credit where due:** the probe itself is good practice. It converted "the formatter mandated this
  extra line" from an assertion into a reproducible observation, and this reviewer read both probe
  logs and confirms both variants (plain comment and `///` doc-comment) reproduce exactly.

### PA-4 — Coverage collected through a dot-sourced function rather than the packaged script (Advisory, Low; correctly justified)

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` was dot-sourced and `Invoke-DotnetCoverageCollection`
called directly, because that script's assembly-discovery predicate at `:301` excludes any path
containing a `\.claude\` segment and this review worktree lives under `.claude/worktrees/`
(repository issue **#752**). The five mandatory parameters were supplied explicitly and the same
`coverage.config` was used, so the measured denominator is the standard one. The substitution is
disclosed in `evidence/qa-gates/mstest-coverage.md:10-11`. Accepted.

### PA-5 — Spec-declared follow-ups not yet promoted (Advisory, Low; owed at PR time)

`spec.md:248-250` and `plan.2026-09-02T12-02.md:342-346` name three post-fix follow-ups: make the
reentrancy counter instance-scoped, split `QfcCollectionController.cs`, and split `QfcQueue.cs`. No
corresponding entry exists under `docs/features/potential/`; the newest entries there are dated
2026-09-02, before this execution. Prose in a feature folder does not survive merge. **Resolution:**
run all three through the potential-entry / issue promotion lifecycle before the PR is opened.

### Deviations already disclosed by the executor and confirmed adequate

| Deviation | Where disclosed | Reviewer assessment |
|---|---|---|
| AC15 names an `evidence/coverage` directory, which is not a canonical evidence kind | `evidence/qa-gates/ac-traceability.md:65`, recorded as `EVIDENCE_LOCATION_OVERRIDE_REJECTED` | Correct. `evidence-and-timestamp-conventions` recognises only `baseline/`, `regression-testing/`, `qa-gates/`, `issue-updates/`, `other/`, `remediation-baseline/`. `qa-gates/` is the right kind for an artifact produced by the mandatory final QA gate. |
| AC16 says "both new test files"; three were registered | `evidence/qa-gates/ac-traceability.md:67` | Correct and favourable. The third file exists because the 500-line ceiling forbade growing the host file. |
| AC19's "one statement and one comment line" landed as 3 insertions / 1 deletion | `evidence/qa-gates/file-size-audit.md:63-70` | Correct. The extra insertion is a formatter-mandated blank line, reproduced by probe. The bound remains capable of reporting a breach. |
| `evidence/baseline/p0-t11-blocked.md` retained | its own text, and `evidence/qa-gates/scope-boundary.md:169` | Correct. Retaining a superseded-and-annotated record of a correct execution block improves the audit trail. |

## Evidence Location Compliance

Every evidence artifact this branch adds lives under
`docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/evidence/`
in the canonical kinds `baseline/` (8), `qa-gates/` (9) and `regression-testing/` (8).

Branch diff scanned for the forbidden output paths `artifacts/baselines/`, `artifacts/baseline/`,
`artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`,
`artifacts/regression-testing/` and `artifacts/post-change/`. **Zero matches.** No violation.

`validate_evidence_locations.py --root .` was not run: no such script exists in this repository
(`Glob **/validate_evidence_locations.py` returns nothing). The scan above was performed directly
against the full branch diff path list instead.

`EVIDENCE_LOCATION_OVERRIDE_REJECTED` entries carried forward from execution: one, for AC15's
`evidence/coverage` spelling, replaced with `evidence/qa-gates/`. Confirmed correct.

## Artifact Hygiene

The full branch diff and the entire feature folder were searched for the account name, the short
account name and drive-letter home paths. **Zero matches.** The absolute paths that do appear are
confined to `C:\Program Files\Microsoft Visual Studio\18\Community\…` for MSBuild and
`vstest.console.exe`, which the executor's own path-hygiene rule permits as external build-tool
executables outside the worktree containing no account name. This reviewer agrees: those paths leak
no host or account identity.

Committed Cobertura documents: none. `coverage/` is gitignored except `.gitkeep`, so no 12 MB XML
blob and no `filename=`-attribute account leak enters the history.

## 9. Summary of Changes

| Group | Files | Insertions | Deletions |
|---|---|---|---|
| Production C# | 6 | 44 | 13 |
| Test C# | 5 | 747 | 71 |
| Test project file | 1 | 3 | 0 |
| Feature-folder documents | 4 | 1558 | 0 |
| Evidence artifacts | 26 | 1834 | 0 |
| `.claude/agent-memory/` | 12 | 258 | 0 |
| **Total** | **54** | | |

Production changes, all five findings:

1. Three per-owner `IEmailMoveMonitor` comments plus one corrected class comment on
   `EmailMoveMonitor`. No behaviour change. The shared-instance option was rejected on evidence
   this reviewer independently verified.
2. `Cleanup()` now signals with `CompleteAdding()` and defers `Dispose()` onto a
   `TaskScheduler.Default` continuation that reads and logs the antecedent's fault.
3. Dead `scoreLoader` and `globals` constructor parameters removed from
   `QfcRemainingQueueAdmission`, with the sole production call site and the test factory updated.
4. The reentrancy-counter guard read now goes through `Volatile.Read`; the field is deliberately
   not marked `volatile`.
5. Evidence only.

**`.claude/agent-memory/` audit.** Twelve paths, 258 insertions, no deletions: six new memory files
and six index-line additions across the atomic-executor, atomic-planner and task-researcher stores.
These were **not** excluded from this audit despite the executor's scope gate subtracting them.
Reviewed for content and hygiene: all six new files are legitimate cross-session engineering notes
(coverage-denominator nondeterminism, self-referential evidence enumeration, porcelain collapsing
untracked directories, the #731 plan seams), all six index additions are single lines in the
prescribed `- [Title](file.md) — hook` form, and the diff contains zero account names, host names or
absolute home paths. No production or policy file is touched. No finding.

## 10. Compliance Verdict

**PASS.**

- Blocking findings: **0**.
- FAIL rows recorded: 4 (PA-1 exclusion attributes; PA-2 file-size ceiling; three sub-floor
  modified-file coverage rows). Every one is a pre-existing repository condition that this branch
  does not worsen in any measurable way, and each carries a stated non-blocking disposition.
- Advisory findings: 9 total across this artifact and the code review. None requires a code change
  before merge.
- Acceptance criteria: **19 of 19 PASS** (see `feature-audit.2026-09-03T15-35.md`).
- Remediation inputs: **not produced**, because no finding is Blocking.

Recommended actions before opening the PR, none of which gate merge:

1. Regenerate `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` for this
   branch and base; both currently describe unrelated branches.
2. Promote the three spec-declared follow-ups (PA-5) into potential entries or issues.
3. Correct the one inaccurate clause in `evidence/qa-gates/file-size-audit.md:78`.
4. Consider the two code-quality items CR-1 and CR-2 from the code review; both are small and
   neither blocks.

## Appendix A: Test Inventory

New test files (3):

| File | Lines | Methods | Purpose |
|---|---|---|---|
| `QuickFiler.Test/Controllers/QfcMoveMonitorTopologyTests.cs` | 187 | 2 | Finding 1 — pins the three-owner monitor topology by source count and by assembly reflection. |
| `QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs` | 399 | 7 | Finding 2 — running / parked / absent / faulted consumer, repeated cleanup, completion-before-disposal, and a no-synchronous-wait source guard. |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.Volatile.cs` | 120 | 1 | Finding 4 — structural proxy for the `Volatile.Read` guard, with an explicit not-a-proof disclaimer. |

Modified test files (2):

| File | Lines (base → head) | Change |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | 401 → 371 | Factory reduced to the new constructor; the issue-#233 admission-scoring test replaced by a structural equivalent carrying the original rationale verbatim. |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | 498 → 498 | `class` → `partial class`, one word. No test body altered. |

Frozen file confirmed untouched: `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs`,
496 lines at both the merge-base and HEAD, absent from the branch diff.

Net-new `[TestMethod]` count: 2 + 7 + 1 + (1 added − 1 removed) = **10**, matching the observed
6985 → 6995 suite delta exactly.

## Appendix B: Toolchain Commands Reference

```
dotnet tool restore
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
Invoke-DotnetCoverageCollection -OutputPath coverage/postchange.cobertura.raw.xml -CoverageConfig coverage.config -VsTestPath <vstest.console.exe> -TestAssembly <9 assemblies> -RunSettingsPath scripts/vscode/TaskMaster.cli.runsettings
ConvertTo-KoverageCoberturaXml -XmlContent <raw> -RepoRoot <worktree root>
```

Reviewer verification commands (read-only, no mutation):

```
git -C <worktree> merge-base HEAD origin/main
git -C <worktree> diff --numstat 35583f7c…HEAD
git -C <worktree> diff 35583f7c…HEAD -- QuickFiler/ QuickFiler.Test/QuickFiler.Test.csproj
git -C <worktree> log -1 --format="%h %ad %s" -S "[ExcludeFromCodeCoverage]" -- <two paths>
pwsh: [System.IO.File]::ReadAllLines(<path>).Count for 12 source paths at HEAD and at the merge-base
pwsh: XmlDocument load of coverage/{baseline,postchange}.cobertura.processed.xml, separator-anchored
      per-filename line maps with max-hits de-duplication over ./lines/line + ./methods/method/lines/line
```

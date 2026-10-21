# Policy Audit — Issue #798

- Timestamp: 2026-09-07T19-05
- Issue: #798
- Branch: `bug/quickfiler-crash-column-add-timeout-swallowed-keynotfound-798`
- Head commit: `39433790e27585df3c4c2fab73fdd3227763e402`
- Base commit: `c431dc3297e864041d829e8d79b348960b8d8019` (origin/main)
- Work mode: `full-bug` (marker read from `issue.md` line 12)
- Acceptance-criteria source: `spec.md` only

## Executive Summary

The change is compliant with the governing policies. Zero Blocking findings and eleven Non-blocking
findings were recorded. The final toolchain pass is clean in a single pass across all four stages,
the branch diff is exactly the sixteen declared paths, and the two new production modules clear the
90 percent new-code line obligation.

Two policy rows are recorded as FAIL with a Non-blocking disposition, both pre-existing at the base
commit and both improved rather than worsened by this change: repository-wide branch coverage stands
at 66.12 percent against the 75 percent uniform floor, and
`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` stands at 869 lines against the 500-line file cap.
Neither originates in this change and neither is remediable inside a targeted bugfix without
widening scope, which `CLAUDE.md` prohibits.

The repository-wide branch-coverage figure is supplied by this review. No executor artifact reports a
branch-coverage value on either side; `evidence/qa-gates/coverage-delta.md` reports line coverage
only. The figure was parsed directly from the committed Cobertura documents.

## Scope

The audit scope is the full branch diff of `39433790` against `c431dc32`. It is not the scope of any
plan, task or phase.

Resolved by direct enumeration of the supplied diff and by reading the committed feature folder, the
branch changes:

- Sixteen code paths across three production assemblies and three test assemblies.
- The `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/`
  feature folder, including all requirement documents and all evidence artifacts.
- The promoted potential entry under `docs/features/potential/promoted/`.

## Rejected Scope Narrowing

The caller supplied the code diff as a pre-materialized patch and described it as follows:

> "It is `git diff c431dc32 HEAD` over the whole tree excluding the `.claude` and `docs` trees — that
> is, exactly the sixteen code paths under change."

The `docs` tree is committed on this branch and is therefore part of the branch diff. The exclusion
was not accepted as a limit on audit scope. The feature folder was audited by direct read of
`issue.md`, `spec.md`, `user-story.md`, `plan.2026-09-06T22-00.md` and every artifact under
`evidence/`, and the findings below draw on that material. The `.claude` tree carries no committed
change on this branch: the checkpoint records the agent-memory edits as uncommitted working-tree
state excluded by the mandated pathspec, and no `.claude` path appears in either commit.

No other narrowing was attempted. The caller's statement that AC6 is expected to remain unverified is
not a narrowing; it restates `spec.md`, and this review reached the same conclusion independently
because AC6 requires a live Outlook launch that cannot be exercised from a code review.

## 1. General Unit Test Policy Compliance

Source: `.claude/rules/general-unit-test.md`.

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence | PASS | New classes hold no cross-test state. `DfDeedleQfcColumnTimeoutTests` carries `[DoNotParallelize]` because it mutates process-wide log4net state. |
| Isolation | PASS | Each test targets one behaviour of one method. |
| Fast execution | PASS | The consolidated 7048-test suite completes in the recorded final run with no hang. |
| Determinism | PASS | See section 1.1. Banned-API scan returned zero matches. |
| Readability | PASS | Every test carries a doc comment naming its AC and its scenario; Arrange-Act-Assert sections are labelled. |
| Line coverage >= 85% | PASS | Repository-wide post-change line rate 85.87%. |
| Branch coverage >= 75% | FAIL | Repository-wide post-change branch rate 66.12%. Pre-existing 66.05%; improved. Disposition Non-blocking, finding NB-1. |
| No regression on changed lines | PASS | Relocation-adjusted pair 157 + 164 = 321 covered against a baseline of 226. |
| Coverage Exclusion Policy | PASS | This change adds no coverage exclusion and removes none. See section 1.3. |
| Scenario completeness | PARTIAL | Two error-handling scenarios and two invalid-input scenarios in `RibbonCommandBoundary` are untested. Finding NB-2. |
| Arrange-Act-Assert | PASS | All 25 added tests use labelled sections. |
| No external dependencies | PASS | COM boundaries are mocked with Moq; no network, database or process is touched. |
| No temporary files | PASS | Banned-pattern scan of the five test files returned no filesystem write. |
| Test file location | PASS | All five test files mirror the production tree under their assembly's test project. |

### 1.1 Determinism Infrastructure

The banned-API rule was verified rather than assumed.

- SearchScope: the seven new or modified `.cs` files in the branch diff —
  `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs`,
  `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs`,
  `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`,
  `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs`,
  `QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs`,
  `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`,
  `TaskMaster/Ribbon/RibbonCommandBoundary.cs`.
- SearchPatterns: `Thread\.Sleep`, `Task\.Delay`, `DateTime\.Now`, `DateTime\.UtcNow`,
  `DateTimeOffset\.Now`.
- SearchResult: 0 matches across 0 files.

The timeout tests drive a `FakeTimeProvider` through an `ArmingBarrierTimeProvider` that forwards
every member to the inner fake provider and completes a signal after forwarding `CreateTimer`. The
arrangement was evaluated for genuine determinism rather than accepted on its documentation:

1. `FireOneDeadlineAsync` calls `barrier.ReArm()` before `barrier.Advance(...)`. The next-deadline
   signal therefore exists before the production loop can possibly arm the next timer, so no arming
   event can be missed. This ordering is the load-bearing property.
2. `probe.Entered` is deliberately not re-armed in that helper. The corrected production loop enters
   the adder exactly once, so a re-armed entry signal could never be set. This is the repaired D1
   defect recorded in the checkpoint and the repair is correct.
3. Both signals use `TrySetResult`, which tolerates the loop arming one more timer than a test drives.
4. The cancellation test's assertion holds under both orderings of the `Cancel()` versus
   `IsCancellationRequested` race: on one ordering the loop returns immediately, on the other it arms
   one further deadline that the drain fires. Neither produces the column-add `TimeoutException` the
   test asserts absent. The drain is bounded at two iterations and guarded by
   `Task.WhenAny(barrier.Armed, call)`, of which at least one member always completes.
5. The only blocking primitive is `ManualResetEventSlim.Wait()`, which has no time component and is
   released in a `finally` on both the pass and the fail path.

Verdict: the tests are deterministic in outcome. Finding NB-6 records the residual that a regression
in the arming behaviour would present as a suite hang rather than a test failure.

### 1.2 Coverage Metrics

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 16 | 7048 | 7048 passed, 0 failed, 0 skipped | 85.83% lines / 66.05% branch | 85.87% lines / 66.12% branch | 95.6% lines |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |
| PowerShell | 0 | 0 | N/A | N/A | N/A | N/A |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/coverage-baseline.cobertura.xml`
- C# post-change coverage artifact: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: the per-language coverage comparison block below

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.83% lines (169,184/197,122) -> Post-change: 85.87% lines (169,857/197,810). Change: +0.04% lines (673 additional covered lines over 688 additional valid lines). New/changed-code coverage: 95.6%. Disposition: PASS. Evidence: the two Cobertura documents named in the checklist above, parsed directly by this review.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Coverage artifact | Verdict | Disposition |
|---|---|---|---|
| C# | Committed feature-evidence Cobertura, both sides | PASS | Canonical `artifacts/csharp/coverage.xml` is absent; the committed feature-evidence Cobertura is a complete Cobertura document carrying repository-wide counters and was parsed directly by this review, satisfying the artifact-presence obligation. |
| TypeScript | N/A | N/A | Zero changed files. |
| Python | N/A | N/A | Zero changed files. |
| PowerShell | N/A | N/A | Zero changed files. |

The repository-wide branch figures were derived by this review from the `<coverage>` root element of
each document. Baseline: `branch-rate="0.6605220330495744"`, 21,105 of 31,952 branches. Post-change:
`branch-rate="0.6611926319075866"`, 21,178 of 32,030 branches. The 75 percent uniform floor in
`.claude/rules/quality-tiers.md` is not met on either side. The condition is pre-existing, the
direction of movement is upward, and the denominator is the raw document denominator including test
assemblies and host-bound code. Recorded as FAIL with a Non-blocking disposition, finding NB-1.

### 1.3 Coverage Exclusion Policy

`.claude/rules/general-unit-test.md` states that no production file may be excluded from coverage
measurement and directs feature reviewers to treat an `exclude` entry matching a production source
path as a Blocking finding. Three files in the write set read `NOT INSTRUMENTED` on both sides.

- SearchScope: `QuickFiler/Controllers/QfcDatamodel.cs`,
  `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs`, `TaskMaster/Ribbon/RibbonViewer.cs`,
  `TaskMaster/Ribbon/RibbonCommandBoundary.cs`.
- SearchPattern: `ExcludeFromCodeCoverage`.
- SearchResult: 2 matches — `TaskMaster/Ribbon/RibbonViewer.cs` line 32 and
  `QuickFiler/Controllers/QfcDatamodel.cs` line 25. The latter is class-level on the `QfcDatamodel`
  declaration and therefore covers both partials. `RibbonCommandBoundary.cs` carries none.

Adjudication:

1. Both attributes are pre-existing. The branch diff contains no addition, removal or relocation of
   an `ExcludeFromCodeCoverage` attribute; the literal does not appear as an added or removed line
   anywhere in the sixteen-path diff.
2. The rule's Blocking clause is scoped to `exclude` entries in coverage tooling configuration; its
   permitted and prohibited lists enumerate configuration globs such as `dist/**`, `node_modules/**`
   and `jest.config.cjs`. It does not name the `[ExcludeFromCodeCoverage]` attribute.
3. `CLAUDE.md`, which ranks first in the policy compliance order, explicitly ratifies
   `[ExcludeFromCodeCoverage]` for VSTO ribbon classes and Outlook Interop event-handler classes in
   `QuickFiler` and `TaskMaster`, and records the exemption as maintainer-ratified. Both attributed
   types fall squarely inside that ratified class.
4. This change actively complies with the remedy the rule prescribes. The rule directs that untestable
   files be handled by extracting logic into host-neutral testable modules and leaving only the
   thinnest wiring in the host-bound entry point. The change does exactly that: the ribbon decision
   logic was extracted into `RibbonCommandBoundary`, which carries no exemption attribute and is
   measured at 90.16 percent, while only the `MessageBox.Show` presentation call and the handler
   one-liners remain in the exempt shim.

Verdict: PASS. A documented tension between two governing documents exists and is recorded as
finding NB-8, but it is pre-existing, it is not created or widened by this change, and it is not this
change's to resolve.

## 2. General Code Change Policy Compliance

Source: `.claude/rules/general-code-change.md`.

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | The recursive retry was replaced with a single held task and a bounded loop; the fix removes indirection rather than adding it. |
| Reusability | PASS | `ValidateRequiredEmailColumns` is a pure helper called from both entry points rather than duplicated. |
| Extensibility | PASS | The two new parameters are optional, so no caller breaks. `AddQfcColumnsAsync` widens from private to internal. |
| Separation of concerns | PASS | Decision logic sits in `RibbonCommandBoundary`; presentation is an injected sink. |
| Toolchain loop in order | PASS | Format, analyze, type-check, test, clean on pass 1. See section 7 and Appendix B. |
| File size limit 500 lines | FAIL | `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` at 869 lines. Pre-existing at 882; strictly decreased. Disposition Non-blocking, finding NB-7. |
| Error handling fails fast | PASS | The exhausted column-add budget now throws instead of returning normally; the validator throws before the row builder indexes. |
| No silent error suppression | PASS | The three `catch` clauses in `RibbonCommandBoundary` are a defined boundary with a documented rationale in source remarks; each records the failure through the log sink. |
| Logging pattern | PASS | The existing `LogDfTiming` helper and `logger.Error(string, Exception)` shape are reused; format, prefix and level are unchanged. |
| Naming | PASS | PascalCase types and members, camelCase locals and private fields throughout. |
| No breaking public API change | PASS | No public signature changes. `AddQfcColumnsAsync` widens access. |
| No new dependencies | PASS | No package reference added; `Microsoft.Bcl.TimeProvider` and the testing package were already referenced. |
| I/O boundaries isolated | PASS | The COM column-add is behind an injectable `Action<object, object>` seam. |

### 2.1 File size audit

Eleven `.cs` files are created or modified. Counts were independently corroborated for three of them
by direct read of the file tail; the remaining eight are taken from the two committed measurements in
`evidence/qa-gates/p7-ac13-line-cap.md`, which agree with each other before and after the final
formatting pass.

| Path | Lines | Rule | Result |
|---|---|---|---|
| `UtilitiesCS/Extensions/DfDeedle.cs` | 314 | <= 500 | PASS |
| `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` | 297 | <= 500 | PASS |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | 154 | <= 500 | PASS |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 483 | <= 500 | PASS |
| `TaskMaster/Ribbon/RibbonCommandBoundary.cs` | 176 | <= 500 | PASS |
| `TaskMaster/Ribbon/RibbonViewer.cs` | 432 | <= 500 | PASS |
| `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` | 500 | <= 500 | PASS at the cap, zero headroom, finding NB-10 |
| `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs` | 218 | <= 500 | PASS |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 869 | <= 500 | FAIL, pre-existing, Non-blocking, finding NB-7 |
| `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs` | 249 | <= 500 | PASS |
| `QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs` | 157 | <= 500 | PASS |

Independent corroboration: `DfDeedleQfcColumnTimeoutTests.cs` last content line is 500;
`DfDeedle_COM_Tests.cs` last content line is 869; `DfDeedleRequiredColumnValidationTests.cs` last
content line is 218.

### 2.2 Adjudication of the 500-line-cap reasoning

The reasoning recorded in `evidence/baseline/line-cap-preexisting.md` and in AC13 was evaluated
rather than accepted.

The argument has three legs. One of them is circular and two are not.

- Circular leg: "bringing the file under the cap would add a seventeenth path outside the write set
  that AC13 pins." AC13 was authored by this change's own specification, so citing it as the
  constraint that prevents fixing the cap does not by itself justify anything.
- Independent leg one: `CLAUDE.md`'s Bugfix Workflow requires the minimal targeted fix and states
  "change only what is needed... If you uncover deeper design problems, open a new issue instead of
  widening scope." Splitting an 882-line pre-existing test file that this change touches only to
  repair two reflection call sites is precisely the opportunistic refactor that clause prohibits.
- Independent leg two: `spec.md` records that a downstream extractor derives this item's change
  footprint from the backtick-delimited path tokens in order to schedule the item against three
  concurrently-prepared sibling items. An additional path therefore carries a real scheduling and
  conflict cost external to this change's own documents.

The conclusion survives removal of the circular leg. The reasoning is accepted. Two conditions attach:
the file's count must strictly decrease, which it does by 13 lines, and the pre-existing violation
must be promoted as a follow-up, which is recorded in `evidence/other/followup-promotions.md` but has
not yet been created as an issue. Finding NB-9 carries that obligation forward.

## 3. Language-Specific Code Change Policy Compliance

Source: `.claude/rules/csharp.md` and the C# sections of `CLAUDE.md`.

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting via `dotnet tool run` | PASS | `dotnet tool run csharpier check .` exit 0, "Checked 1593 files". A pre-format check was run before each format pass, which discriminates a clean pass from a repairing one. |
| `dotnet format` not used | PASS | No project file was rewritten; the five `.csproj` diffs are single added `<Compile Include>` lines each. |
| Analyzers with `/t:Rebuild` | PASS | Exit 0, 0 errors, 0 warnings, against a baseline of 0. 71 of 74 CoreCompile executions confirm analyzers ran. |
| Nullable analysis with `/t:Rebuild` | PASS | Exit 0, 0 errors, 0 warnings. The new partial carries `#nullable enable` and participates. |
| `/p:Nullable=enable` not passed | PASS | The recorded commands match the CI shape exactly. |
| Banned symbols | PASS | Zero `Task.Delay` and zero `Thread.Sleep` in the change; the analyzer step is the gate that would reject a `Task.Delay`-based timeout. |
| Null safety | PASS | The new partial annotates `columnAdder` and `timeProvider` as nullable; the folder-name capture uses an explicit null-coalesce with a documented reason. |
| Exceptions fail fast | PASS | Descriptive `TimeoutException` and `InvalidOperationException` naming the folder and the step. |
| Broad catch only at a boundary | PASS | The three `catch (System.Exception)` clauses are confined to `RibbonCommandBoundary`, which is the defined boundary. |
| `internal` preferred for non-public API | PASS | `RibbonCommandBoundary`, `AddQfcColumnsAsync` and `ValidateRequiredEmailColumns` are all `internal`. |
| XML documentation on non-obvious contracts | PASS | Every new type and member carries doc comments; several carry `<remarks>` recording why. |
| Time seam via `TimeProvider` | PASS | `TimeProvider?` is threaded through the column-add method and the existing `TimeoutAfter` overload. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]` and `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting` in all five files. |
| Moq for mocking | PASS | `Table`, `Columns`, `MAPIFolder`, `UserDefinedProperties`, `Explorer`, `NameSpace`, `IOlObjects` and `IApplicationGlobals` are all Moq mocks. |
| FluentAssertions preferred | PASS | Every assertion in the 25 added tests uses FluentAssertions. |
| Arrange-Act-Assert | PASS | Sections labelled in every added test. |
| No external dependencies | PASS | No COM object, no network, no filesystem. |
| New module coverage >= 90% | PASS | 97.62% and 90.16%. See section 5. |
| No coverage regression on changed lines | PASS | Relocation-adjusted comparison 321 against 226. |

## 5. Test Coverage Detail

Per-file figures for the two new production modules, taken from the committed post-change Cobertura
document and independently re-derived by this review from the class-level counters.

| Path | Covered | Valid | Line rate | Obligation and result |
|---|---|---|---|---|
| `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` | 164 | 168 | 97.62% | >= 90%, PASS |
| `TaskMaster/Ribbon/RibbonCommandBoundary.cs` | 55 | 61 | 90.16% | >= 90%, PASS with a one-line margin |

Independent re-derivation of the `RibbonCommandBoundary.cs` figure, which the caller flagged for
scrutiny because 54 of 61 would fail. The file contributes two class elements to the document:

- `TaskMaster.RibbonCommandBoundary`: `line-rate="0.8846153846153846"`, which is 46 of 52. The six
  uncovered lines are 109, 110, 112 inside `SafeLog` and 155, 156, 157 inside `CollectDetail`,
  confirmed by reading the per-line `hits` values.
- `TaskMaster.RibbonCommandBoundary.<RunAsync>d__3`: `line-rate="1"`, 9 of 9.

46 + 9 = 55 covered, 52 + 9 = 61 valid, 55/61 = 0.901639. The executor's figure is correct and the
margin is real. Branch coverage for the primary class element is `branch-rate="0.75"`, that is 9 of
12 conditions, exactly at the uniform floor rather than above it. The three uncovered conditions are
the two `?? throw new ArgumentNullException` guards at lines 46 and 47 and one jump at line 154.

Finding NB-2 records the four specific tests that would take this file to 61 of 61 lines and 11 of 12
branches and remove the margin exposure entirely.

## 6. Test Execution Metrics

| Metric | Value |
|---|---|
| Total tests, final run | 7048 |
| Passed | 7048 |
| Failed | 0 |
| Skipped | 0 |
| Tests added by this change | 25 |
| Baseline total | 7023 |
| Arithmetic reconciliation | 7023 + 25 = 7048 |
| Rerun count | 0 |
| Toolchain restarts required in the final pass | 0 |

The baseline was green: `PREEXISTING_FAILURE_SET` is empty and no `BASELINE NOT GREEN` marker was
recorded, so the zero-failure acceptance is reachable without a separate remediation.

## 7. Code Quality Checks

| Check | Command or method | Result |
|---|---|---|
| Confidentiality masking scan | Read of all committed evidence artifacts for absolute host paths, account names and machine names | PASS. Artifacts use `<worktree>`, `<user>`, `<machine>`, `<vs-install>` and `<redacted>` tokens. |
| Suppression scan (added lines) | Search of the sixteen-path diff for `#pragma warning disable`, `SuppressMessage` and `ExcludeFromCodeCoverage` | PASS. Zero matches on added lines. |
| Workflow change scan | Enumeration of the branch diff for `.github/workflows/**` | PASS. Zero workflow files changed, so no green-run gate applies. |

Additional checks, recorded in prose rather than as table rows:

- Evidence location. All evidence is written under
  `docs/features/active/<feature>/evidence/<kind>/` using the kinds `baseline`, `qa-gates`,
  `regression-testing`, `issue-updates` and `other`. SearchScope: the full branch diff.
  SearchPatterns: `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`,
  `artifacts/coverage/`. SearchResult: 0 matches. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` was
  required. Verdict PASS.
- Write-set integrity. The sixteen paths were re-derived by enumerating every `diff --git` header in
  the supplied patch. The count is 16 and the paths match the `## Write Set` declaration
  element for element.
- Compile-entry completeness. Six new `.cs` files; six added `<Compile Include>` entries across the
  five project files, one each in `QuickFiler.Test`, `TaskMaster.Test` and `TaskMaster`, two in
  `UtilitiesCS.Test` and one in `UtilitiesCS`. Each insertion is a single added line adjacent to a
  neighbouring entry, so no project file was rewritten.

## Findings

| ID | Severity | Area | Summary |
|---|---|---|---|
| NB-1 | Non-blocking | Coverage | Repository-wide branch coverage 66.12% is below the 75% uniform floor. Pre-existing at 66.05%; improved. Not reported by any executor artifact. |
| NB-2 | Non-blocking | Test completeness | Four untested scenarios in `RibbonCommandBoundary` hold the file at a one-line line-coverage margin and at exactly the branch floor. |
| NB-3 | Non-blocking | Design | `RibbonViewer._commandBoundary` is not injectable, so the presentation sink is hard-wired to `MessageBox.Show` even under the internal test constructor. |
| NB-4 | Non-blocking | Correctness | The exhaustion message hardcodes "9000 ms" while the loop length depends on the `counter` argument; `3000` is a magic literal beside the named `AttemptLimit`. |
| NB-5 | Non-blocking | Correctness | A `TaskCanceledException` fault with the token unsignalled causes three immediate re-awaits and a misleading timeout message. Not reachable from the production call site. |
| NB-6 | Non-blocking | Test robustness | The timeout tests use unbounded awaits, so an arming regression presents as a suite hang rather than a failure. |
| NB-7 | Non-blocking | File size | `DfDeedle_COM_Tests.cs` at 869 lines against the 500-line cap. Pre-existing at 882; strictly decreased. |
| NB-8 | Non-blocking | Policy conflict | `.claude/rules/general-unit-test.md` Coverage Exclusion Policy against `CLAUDE.md`'s ratified `[ExcludeFromCodeCoverage]` exemption. Pre-existing and unresolved at repository level. |
| NB-9 | Non-blocking | Process | Three follow-up findings are recorded but deliberately not promoted on this branch. The promotion obligation transfers to the caller after merge. |
| NB-10 | Non-blocking | File size | `DfDeedleQfcColumnTimeoutTests.cs` sits at exactly 500 lines with zero headroom. |
| NB-11 | Non-blocking | Test isolation | `CaptureDfDeedleLog` mutates process-wide log4net `Hierarchy.Configured` state for the duration of the capture. |

Blocking findings: 0. Non-blocking findings: 11.

## Verdict

**PASS.** No remediation cycle is required and no `remediation-inputs` artifact is produced.

Two conditions should be carried forward by the caller rather than closed here: the three follow-up
promotions recorded in `evidence/other/followup-promotions.md` must be created after merge, and AC6
requires manual verification by the maintainer on the reproduction folder before the fix can be
considered confirmed in the field.

## Appendix A: Test Inventory

Twenty-five tests added across four new test classes, plus two repaired tests in an existing class.

| Test class | Assembly | Tests | Criteria covered |
|---|---|---|---|
| `DfDeedleQfcColumnTimeoutTests` | UtilitiesCS.Test | 8 | AC1, AC2, AC7 |
| `DfDeedleRequiredColumnValidationTests` | UtilitiesCS.Test | 9 | AC3, AC9 |
| `RibbonCommandBoundaryTests` | TaskMaster.Test | 7 | AC5, AC11 |
| `QfcDatamodelRethrowTests` | QuickFiler.Test | 1 | AC4 |
| `DfDeedle_COM_Tests` (repaired) | UtilitiesCS.Test | 2 of 26 | Signature-widening repair |

Scenario coverage by category:

- Positive flows: three column-add completion variants, one all-keys-present validation case, one
  boundary success case.
- Negative flows: five single-missing-key cases, one multi-missing-key case, one case-variant case.
- Edge and boundary: cancellation requested mid-loop, exactly-once adder invocation across three
  deadlines, `AggregateException` inner-detail rendering.
- Error handling: exhausted budget throws naming folder and step; presentation sink throws and is
  contained; failure does not propagate to an `async void` caller.
- Concurrency: the non-overlap invariant is asserted directly by the invocation count.
- State transitions: the three-deadline sequence is driven one deadline at a time through the barrier.

Gaps identified by this review and recorded as NB-2: a throwing log sink, an `AggregateException`
with zero inner exceptions, and the two null-sink constructor guards.

## Appendix B: Toolchain Commands Reference

Run in this order, restarting from step 1 on any failure or file rewrite. The final pass was clean at
the first attempt.

1. `dotnet tool run csharpier check .` then `dotnet tool run csharpier format .` — exit 0, "Checked
   1593 files in 6008ms."
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — exit 0, 0 errors, 0 warnings.
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` — exit 0, 0 errors, 0 warnings.
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` — exit 0, "Test Run Successful.",
   7048 passed of 7048.

`/t:Rebuild` is used in steps 2 and 3 rather than `/t:Build` because a warm local worktree skips
`CoreCompile` under MSBuild incrementality and the gate cannot fail. The recorded 71 of 74
`CoreCompile` executions across 19 projects confirm the analyzers actually ran.

# Feature Audit — issue #508 (wpf-dispatcher-yield-test-order-dependent)

Timestamp: 2026-08-08T17-45
Reviewer: feature-review
Work mode: `minor-audit` (marker at `issue.md:3`)

## Scope and Baseline

Baseline: `main` at merge base `003c5715055d7d1933db68a742531332756e30b2` (recomputed at review time
via `git merge-base HEAD origin/main`; matched the caller-supplied value).
Head: `7466096d73ef86f3cc5b9d5da6648cf156c02d6f`.
Range audited: `003c5715055d7d1933db68a742531332756e30b2..7466096d73ef86f3cc5b9d5da6648cf156c02d6f`
— the full branch diff, 56 files, of which 2 are source.

Acceptance-criteria source, resolved from the `minor-audit` work-mode marker: the
`## Acceptance Criteria` section of
`docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/issue.md`,
lines 124-146, containing AC1 through AC9.

`spec.md` and `user-story.md` do not exist. Under `minor-audit` that is correct by design and is not
treated as a gap.

## Acceptance Criteria Inventory

| AC | Line | Criterion (abridged) |
|---|---|---|
| AC1 | 124-127 | The test arranges its own dispatcher-free precondition; result no longer depends on pooled thread, execution order, or prior `UiThread.Initialize()` |
| AC2 | 128-130 | Strict contract preserved and not weakened; still throws `InvalidOperationException`, test still asserts exactly that |
| AC3 | 131-133 | Coverage pins all three resolution branches |
| AC4 | 134-136 | Production change is minimal, justified in the PR body, and preserves runtime resolution order and exception contract for all existing call sites |
| AC5 | 137 | None of the "Prohibited Fixes" approaches used |
| AC6 | 138-139 | Fail-before evidence recorded |
| AC7 | 140-142 | At least three consecutive full parallel runs, identical and fully green for `WpfDispatcherYieldTests` |
| AC8 | 143-144 | Full C# toolchain passes in order in a single final pass with per-step evidence |
| AC9 | 145-146 | Repo-wide line coverage does not regress; changed-line coverage does not decrease |

Total: 9.

## Acceptance Criteria Evaluation

### AC1 — Test arranges its own precondition — **PASS**

Verified structurally, not only empirically. `WpfDispatcherYield.cs:60-61` resolves through
`_currentThreadDispatcherProvider()` and `_fallbackDispatcherProvider()`, both assigned once at
`WpfDispatcherYield.cs:42-46`. `WpfDispatcherYieldTests.cs:123-128` supplies
`CountingDispatcherProvider(null)` for both through the `internal` constructor, so the default
lambdas that read `Dispatcher.FromThread(Thread.CurrentThread)` and `UtilitiesCS.UiThread.Dispatcher`
are never assigned. Neither ambient operand is in the object graph for that test.

This satisfies the criterion for **both** operands. The alternative shape the issue considered and
rejected (`issue.md:69-72`) would have arranged only the thread-affinitized operand and left the
process-global fallback unarranged; the implemented shape does not have that weakness.

Empirical corroboration: three consecutive full parallel runs, 4667/4667/0 each, all four tests
green in every run, with per-test durations varying 12/21/33 ms — proving scheduling genuinely
differed between runs while outcomes did not.

Verification: read `WpfDispatcherYield.cs:42-46,60-61` and `WpfDispatcherYieldTests.cs:118-142`;
`evidence/qa-gates/repeat-run-comparison.2026-08-08T17-03.md`.

### AC2 — Strict contract preserved, not weakened — **PASS**

`WpfDispatcherYieldTests.cs:134` is exactly `.ThrowAsync<InvalidOperationException>();` — one
occurrence, unchanged from the baseline capture. Not softened to `NotThrowAsync`, to a base
`Exception`, to an `Or` condition, or to any predicate that holds regardless of the precondition.

Production side: the `if (dispatcher is null)` guard and its message text at
`WpfDispatcherYield.cs:62-67` are byte-identical to the merge-base text.

Verification: `git diff <base>..HEAD -- UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`
(the guard and message appear in no hunk); direct read of `WpfDispatcherYieldTests.cs:130-134`.

### AC3 — All three resolution branches pinned — **PASS**

| Branch | Test | Line | Order assertion |
|---|---|---|---|
| Thread-affinitized present | `YieldAsync_ThreadAffinitizedDispatcherPresent_YieldsWithoutFallback` | 53 | thread count 1, fallback count **0** |
| Thread absent, fallback present | `YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher` | 85 | thread count 1, fallback count 1 |
| Both absent (throws) | `YieldAsync_WithoutDispatcher_RemainsStrict` | 118 | both counts 1, `InvalidOperationException` |

Stronger than the criterion requires: the tests pin resolution *order* through invocation counting,
not merely the outcome. A fourth test (`YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield`, line
16) additionally pins that the cancellation guard precedes both lookups, asserted by both counts
being 0.

Mechanically confirmed by 100% (2/2) condition coverage on line 60 (the `??` resolution) and line 62
(the null guard).

Verification: read `WpfDispatcherYieldTests.cs:15-142`;
`evidence/qa-gates/coverage-changed-lines.2026-08-08T17-06.md`.

### AC4 — Minimal, justified, behavior-preserving, no call-site changes — **PASS** (with a merge-time obligation)

Four clauses, evaluated separately.

*Minimal* — PASS. 37 added, 4 removed lines in one file. Four hunks: remove one `using`, remove one
attribute, add two fields and two constructors, swap the two `??` operands for the seam calls.

*Preserves runtime resolution order and exception contract* — PASS. The `??` remains in the same
position with the same operand order. Short-circuiting is unchanged, so the process-global fallback
is still read only when the thread lookup returns null. The default lambdas reproduce the pre-change
expressions exactly, and `new WpfDispatcherYield()` passes `(null, null)`, selecting both. The
default lambda evaluates `Thread.CurrentThread` when invoked inside `YieldAsync`, not at
construction, so it still observes the calling thread. Exception type and message are byte-identical.

*No call-site changes required* — PASS, verified by grep rather than assertion:

```
grep -rn "new WpfDispatcherYield" --include=*.cs .
  TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:365
  UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs:55
```

Neither file appears in the branch diff, and both still bind to a public parameterless constructor.
The explicit `public WpfDispatcherYield() : this(null, null) { }` at `WpfDispatcherYield.cs:21-22` is
required because adding any constructor removes C#'s implicit one; it restores the identical
signature, preserving binary compatibility. The seam constructor is `internal`
(`WpfDispatcherYield.cs:37`), reachable from tests only through the pre-existing
`[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` at `UtilitiesCS/Properties/AssemblyInfo.cs:19`
(present at the merge base; that file is not in the diff). This is a legitimate testability
mechanism, not a widened public surface — the assembly's public API is unchanged in signature terms.

*Justified in the PR body* — **pending, non-blocking.** No PR body exists at review time
(`artifacts/pr_body*` absent; `pr-author` has not run). The clause is by construction unsatisfiable
before the PR is authored, so it is recorded as a merge-time obligation rather than as a deficiency
in the change. The technical substance is fully written up in
`evidence/qa-gates/no-behavior-change.2026-08-08T17-08.md` and in the plan's seam-shape design
section, and the PR body must draw on it.

The AC is left checked because all clauses that describe the code are satisfied and verified, and
the outstanding clause describes a downstream artifact rather than a gap in the delivered work. The
obligation is restated in the Summary and in the reviewer's final report so it cannot be lost.

### AC5 — No prohibited fixes — **PASS**

Verified independently by the reviewer rather than read from the executor's audit:

```
git diff <base>..HEAD -- '*.cs' \
  | grep -nE "DoNotParallelize|\[Ignore|Thread\.Sleep|Task\.Delay|GetTempPath|GetTempFileName|Retry|retry"
exit 1   (zero matches)
```

| Prohibited approach (`issue.md:116-120`) | Used | Basis |
|---|---|---|
| `[DoNotParallelize]` | no | 0 hits; `UtilitiesCS.Test/Properties/AssemblyInfo.cs` is not in the diff, so `Parallelize(Workers = 0, Scope = ClassLevel)` is intact and the tests still run under class-level parallelization |
| Retry, sleep, or timing hack | no | 0 hits; `BannedSymbols.txt:4-7` already bans `Thread.Sleep` and `Task.Delay` analytically |
| `[Ignore]` or deleting the test | no | 0 hits; test count rose 6293 -> 6295 and `YieldAsync_WithoutDispatcher_RemainsStrict` still exists at line 118 |
| Weakened assertion | no | see AC2 |
| Temporary files in tests | no | 0 hits for temp-path APIs; the tests use in-memory delegates and one owned thread |

The `[Timeout(30000)]` used during the fail-before probe was temporary and does not survive into the
final diff (`grep -c "Timeout" WpfDispatcherYieldTests.cs` -> 0).

### AC6 — Fail-before evidence — **PASS**

A genuine failing run was produced, so no exception dossier is required.

The probe's mechanism is sound: with no seam available pre-change, it arranges the ambient state
instead by marshalling the unchanged call `new WpfDispatcherYield().YieldAsync(CancellationToken.None)`
onto an owned pumping STA thread, where `Dispatcher.FromThread` resolves and the unchanged assertion
therefore fails. That is the defect stated positively.

Result: exit 1, `Failed: 1`, "Expected a `<System.InvalidOperationException>` to be thrown, but no
exception was thrown" — the FluentAssertions did-not-throw failure, not a compile error, not an
infrastructure error, not a timeout, and bounded at 235 ms.

The stale-assembly false pass is ruled out by an explicit DLL-mtime proof:
`UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll` advanced from `16:18:36.0992567-04:00` to
`16:24:18.7130626-04:00` across the probe rebuild, so the executed assembly contained the probe
edit. The assertion and the call under test were both left unchanged during the probe.

Verification: `evidence/regression-testing/fail-before.2026-08-08T16-26.md`,
`evidence/regression-testing/fail-before-method.2026-08-08T16-27.md`.

### AC7 — Three consecutive green parallel runs — **PASS**

| Run | Exit | Total | Passed | Failed |
|---|---|---|---|---|
| 1 | 0 | 4667 | 4667 | 0 |
| 2 | 0 | 4667 | 4667 | 0 |
| 3 | 0 | 4667 | 4667 | 0 |

Identical command in all three, no intervening rebuild or edit, class-level parallelization from the
assembly's own unmodified `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`.
All four `WpfDispatcherYieldTests` methods passed in all three runs (12 of 12 green observations).

The only `/TestCaseFilter` is `TestCategory!=LiveOutlook`, which excludes live-Outlook integration
tests and no test in scope — so the green result was not obtained by filtering.

The duration variance across runs (12/21/33 ms on one test) is the evidence that scheduling and
thread assignment genuinely differed while outcomes stayed constant, which is precisely the property
under test.

Verification: `evidence/qa-gates/repeat-run-{1,2,3}.*.md`,
`evidence/qa-gates/repeat-run-comparison.2026-08-08T17-03.md`.

### AC8 — Full toolchain, single clean pass, per-step evidence — **PASS**

Pass 4 is attested as a single clean pass in the required order, with per-step artifacts:

| Step | Command | Exit | Result | Artifact |
|---|---|---|---|---|
| 1 format | `csharpier format` | 0 | 1488 files, 0 rewritten | `csharpier-format.2026-08-08T16-48.md` |
| 1v check | `csharpier check <2 files>` | 0 | 0 unformatted | `csharpier-check.2026-08-08T16-48.md` |
| 2 lint | analyzer msbuild | 0 | 6 warn / 0 err, CoreCompile ran (13.61s, CS2002 present) | `msbuild-analyzers.2026-08-08T16-49.md` |
| 3 type-check | nullable msbuild | 0 | 5 warn / 0 err | `msbuild-nullable.2026-08-08T16-50.md` |
| 4 test | `Invoke-MSTestWithCoverage.ps1` | 0 | 6295/6295/0 | `tests-coverage.2026-08-08T16-55.md` |

No step was skipped and no file was rewritten within the pass.

Two disclosed qualifications were examined by the reviewer rather than accepted at face value:

1. **The type-check step is an incremental no-op.** MSBuild's `/t:Build` up-to-date check ignores
   `-p:` property changes, so after step 2 built everything, step 3 recompiled nothing (1.25s, no
   `CoreCompile`). The reviewer tested this directly with a forced rebuild of the changed project
   under `-p:Nullable=enable -p:TreatWarningsAsErrors=true`, which surfaced the pre-existing
   repository-wide nullable debt and returned **zero** diagnostics located in the changed production
   file (`grep -cE "WpfDispatcherYield\.cs\([0-9]+,[0-9]+\)" -> 0`). The effective type-check on the
   changed code is step 2, which is non-vacuous because both changed files carry a file-scoped
   `#nullable enable` on line 1, so nullable flow analysis ran on them during the recompile that
   step 2 demonstrably performed. The claim is confirmed by independent measurement.
2. **Four passes were required.** Passes 1-2 failed on two pre-existing out-of-boundary
   `QuickFiler.Test` failures, and pass 3 was abandoned after a stale-build condition was detected.
   Both are disclosed in the attestation rather than concealed. The stale-build detection is
   particularly good practice: `Copy-Item` preserved `LastWriteTime`, MSBuild skipped `CoreCompile`,
   and the executor caught it from the missing `CS2002`/`CoreCompile` signals instead of banking a
   false pass. SHA-256 of both files is unchanged across the experiment, so only filesystem metadata
   moved, and the adjustment preceded pass 4.

Verification: `evidence/qa-gates/toolchain-clean-pass.2026-08-08T16-56.md` plus each per-step
artifact; reviewer's independent `csharpier check` (exit 0) and forced nullable rebuild.

### AC9 — No coverage regression — **PASS**

Recomputed independently by the reviewer by re-summing the committed JaCoCo counters. The reviewer
did not rerun coverage generation.

| Metric | Baseline | Post-change | Change | Floor | Margin |
|---|---|---|---|---|---|
| Repo-wide line | 95274/111021 = 85.8162% | 95325/111059 = 85.8328% | +0.0166 pp | 85% | +0.83 pp |
| Repo-wide branch | 22070/27862 = 79.2118% | 22093/27884 = 79.2318% | +0.0200 pp | 75% | +4.23 pp |

These reproduce the reported figures exactly, and `artifacts/csharp/coverage.xml` re-sums to
byte-identical totals, confirming the gate artifact is a faithful copy rather than a separate
measurement.

Changed-line coverage cannot have decreased: the class was attribute-excluded at baseline and absent
from the baseline report entirely, so the comparand is "unmeasured". Post-change it measures 96.43%
line (27/28 deduped) and 100% branch, against the stricter 90% new-code bar.

The 38-line denominator growth is fully explained by removing `[ExcludeFromCodeCoverage]`, offset by
45 newly covered lines.

One honest qualification the reviewer adds: the +0.0166 pp delta is smaller than observed
measurement noise — the `QuickFiler` package, with zero changed lines on this branch, shows 6 lines
flipping between the two reports. The non-regression conclusion should therefore rest on the
absolute figures clearing their floors with margin, which they do by 0.83 pp and 4.23 pp, rather
than on the sign of a sub-noise delta. Both readings support PASS.

Verification: `evidence/baseline/coverage-baseline.jacoco.xml`,
`evidence/qa-gates/coverage-postchange.jacoco.xml`,
`evidence/qa-gates/coverage-delta.2026-08-08T17-04.md`,
`evidence/qa-gates/coverage-changed-lines.2026-08-08T17-06.md`, `artifacts/csharp/coverage.xml`.

## Acceptance Criteria Check-off

All nine items in `issue.md` lines 124-146 were already `- [x]` at review time. Each check-off was
independently re-verified against the evidence and the source, and each was found to be earned. No
item was un-checked, and no item required a new check-off.

| AC | State in `issue.md` | Reviewer verdict | Action |
|---|---|---|---|
| AC1 | `[x]` | PASS | retained |
| AC2 | `[x]` | PASS | retained |
| AC3 | `[x]` | PASS | retained |
| AC4 | `[x]` | PASS (merge-time obligation on the PR-body clause) | retained; obligation recorded |
| AC5 | `[x]` | PASS | retained |
| AC6 | `[x]` | PASS | retained |
| AC7 | `[x]` | PASS | retained |
| AC8 | `[x]` | PASS | retained |
| AC9 | `[x]` | PASS | retained |

No criterion text was modified and no AC item was added or removed.

### Acceptance Criteria Status

```
- Source: docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/issue.md
- Total AC items: 9
- Checked off (delivered): 9
- Remaining (unchecked): 0
- Items remaining: none
```

## Out-of-Boundary Items (reported, not defects in this change)

- **Two `QuickFiler.Test` pump-host failures** (`InitializeBool_ThroughThePumpHost_*`,
  `InitializeNineArgOverload_ThroughThePumpHost_*`). The attribution reasoning was sanity-checked and
  holds: run D of the controlled experiment reverts both changed files to the merge base, rebuilds,
  and reproduces the same two failures at 6293/6291/2 — matching the pre-work baseline recorded at
  `issue.md:53` before this branch existed. The failure path is WinForms
  `Control.MarshaledInvoke` with no WPF `Dispatcher` and no code from the diff. Tracked by issue
  **#511**, confirmed OPEN via `gh issue view 511`.
- **195 pre-existing repository-wide nullable errors** under a forced rebuild with
  `-p:Nullable=enable -p:TreatWarningsAsErrors=true`. Independently confirmed that none is located in
  either changed file. Predates the merge base; remediation would be a repository-wide refactor.
- **`UiThread.Dispatcher` annotation** (`UtilitiesCS/Threading/UiThread.cs:135-140`) is declared
  non-nullable but backed by `null!`. Contained by the changed code's defensive `Dispatcher?` local;
  a follow-up candidate.
- **`StaDispatcherHost` duplication** across nine test files. Consolidation requires a
  `<Compile Include>` edit to a legacy non-SDK `.csproj`, which the scope boundary forbade;
  following the established pattern was correct here.

## Summary

All nine acceptance criteria are **PASS**. Every pre-existing check-off in `issue.md` was
re-verified and found to be earned; none was un-checked.

The change fixes the root cause rather than masking it, and it does so for both ambient operands —
the thread-affinitized dispatcher and the process-global `UiThread.Dispatcher` — not just the first.
The public API is preserved exactly, the seam is `internal` and rides on a pre-existing
`InternalsVisibleTo`, runtime behavior and resolution order are unchanged, and no call site was
touched. None of the prohibited approaches was used. The `[ExcludeFromCodeCoverage]` removal is
required by policy once the class became testable, and it was carried out honestly by growing the
measured denominator rather than by substituting a different exclusion.

**Blocking findings: 0** (0 FAIL, 0 blocking PARTIAL).

**Verdict: ready to merge**, subject to one non-blocking merge-time obligation:

> AC4 requires the production change to be justified in the PR body. No PR body exists yet
> (`pr-author` has not run). The justification substance is recorded in
> `evidence/qa-gates/no-behavior-change.2026-08-08T17-08.md`; the PR body must carry it before merge.

Six advisory items are recorded in `code-review.2026-08-08T17-45.md` and
`policy-audit.2026-08-08T17-45.md`. None blocks the merge.

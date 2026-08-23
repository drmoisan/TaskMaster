# Policy Audit — issue #508 (wpf-dispatcher-yield-test-order-dependent)

Timestamp: 2026-08-08T17-45
Reviewer: feature-review
Work mode: `minor-audit` (marker at `issue.md:3`)
Branch: `bug/wpf-dispatcher-yield-test-order-dependent-508`
Base branch: `main`
Merge base: `003c5715055d7d1933db68a742531332756e30b2`
Head: `7466096d73ef86f3cc5b9d5da6648cf156c02d6f`
Review range: `003c5715055d7d1933db68a742531332756e30b2..7466096d73ef86f3cc5b9d5da6648cf156c02d6f`

## Executive Summary

The branch changes exactly two source files. It converts the two ambient dispatcher lookups in
`WpfDispatcherYield` into injectable `Func<Dispatcher?>` seams defaulted to the pre-change
expressions, removes a now-indefensible `[ExcludeFromCodeCoverage]`, and replaces one
order-dependent test with four tests that arrange their own preconditions.

All policy gates evaluated PASS. **Blocking findings: 0.** Six advisory observations are recorded;
none blocks merge. One merge-time obligation is carried forward (AC4's PR-body justification
clause, which cannot be satisfied until `pr-author` runs).

Two review-time corrections were made to non-source artifacts and are disclosed in full below: the
PR context artifacts were stale and misclassified the source changes, and they were regenerated.

## Scope Determination

The audit scope is the full branch diff against the resolved base branch, recomputed at review time
rather than taken on trust:

```
git merge-base HEAD origin/main  ->  003c5715055d7d1933db68a742531332756e30b2
git rev-parse HEAD               ->  7466096d73ef86f3cc5b9d5da6648cf156c02d6f
```

The caller-supplied merge base matched the recomputed value.

56 files changed (+4307/-15). Classification:

| Class | Count | Detail |
|---|---|---|
| Source (`.cs`) | 2 | `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` (+37/-4); `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` (+164/-2) |
| Feature docs and evidence (`.md`, `.xml`) | 45 | the `<FEATURE>` folder, `issue.md`, `plan.2026-08-08T15-23.md`, evidence artifacts |
| Agent memory (`.md`) | 9 | `.claude/agent-memory/**`, tracked in this repository |

No `.csproj`, `.sln`, `.props`, `.targets`, `.ps1`, `.py`, or `.ts` file changed:

```
git diff --name-only <base>..HEAD -- '*.csproj' '*.sln' '*.props' '*.targets' '*.ps1' '*.py' '*.ts'
(empty)
```

`spec.md` and `user-story.md` do not exist. Under the `minor-audit` work mode that is correct by
design and is not recorded as a finding; the sole acceptance-criteria source is the
`## Acceptance Criteria` section of `issue.md`.

## Rejected Scope Narrowing

None. The delegating prompt supplied a reduced-audit directive but did not attempt to restrict the
audit to a plan, task, or phase, did not exclude any changed file, and did not ask for any language
verdict to be suppressed. The directive in fact mandated the opposite — an explicit per-language
verdict with no hedging — which is consistent with the scope invariant.

For completeness, the audit was performed against the full branch diff regardless, and every
changed file listed in the Scope Determination table was inspected.

## 1. Toolchain Compliance (CLAUDE.md § C#1, `.claude/rules/general-code-change.md`)

Required order: format -> lint -> type-check -> test.

| Step | Command | Evidence | Reviewer verification | Verdict |
|---|---|---|---|---|
| 1 Format | `csharpier check <2 files>` | `evidence/qa-gates/csharpier-check.2026-08-08T16-48.md` | Re-run independently at review time: `Checked 2 files in 736ms`, exit 0 | PASS |
| 2 Lint | `msbuild TaskMaster.sln -t:Build -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` | `evidence/qa-gates/msbuild-analyzers.2026-08-08T16-49.md` (exit 0, 6 warn / 0 err, 13.61s, CS2002 present) | Non-vacuity signals confirmed in the artifact; both pre-existing warning families identified | PASS |
| 3 Type-check | `msbuild TaskMaster.sln -t:Build -p:Nullable=enable -p:TreatWarningsAsErrors=true` | `evidence/qa-gates/msbuild-nullable.2026-08-08T16-50.md` (exit 0, 1.25s, no CoreCompile) | Independently re-verified — see below | PASS |
| 4 Test | `Invoke-MSTestWithCoverage.ps1` | `evidence/qa-gates/tests-coverage.2026-08-08T16-55.md` (exit 0, 6295/6295/0) | Counts cross-checked against the three repeat runs and the attribution experiment | PASS |

Single clean pass attested at `evidence/qa-gates/toolchain-clean-pass.2026-08-08T16-56.md` (pass 4).
Passes 1-3 and the reasons they restarted are disclosed in that same artifact rather than concealed,
which is the behavior the policy requires.

### 1.1 Independent verification of the type-check step

The executor disclosed that step 3 is an incremental no-op in this repository: MSBuild's
`/t:Build` up-to-date check ignores `-p:` property changes, so after step 2 has just built every
project nothing recompiles (1.25s elapsed, `CoreCompile` skipped, `CS2002` absent). Taking a
self-reported vacuous gate on trust would be inadequate, so the claim was tested directly.

A forced rebuild of the changed project was run by the reviewer:

```
MSBuild.exe UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU \
  -p:Nullable=enable -p:TreatWarningsAsErrors=true
```

Result: exit 1, as expected, exposing the pre-existing repository-wide nullable debt
(CS8766 x260, CS8618 x46, CS8625 x24, CS8600 x18, CS8601 x16, CS8604 x14, CS8602 x6, CS8603 x4,
CS8714 x2 log occurrences). Diagnostics whose source location is the changed production file:

```
grep -cE "WpfDispatcherYield\.cs\([0-9]+,[0-9]+\)" nullable.log  ->  0
```

**Zero.** The only two occurrences of the filename in the log are `csc.exe` command lines listing it
as a compilation input. The executor's claim is confirmed by independent measurement rather than
accepted on assertion.

The effective type-check on the changed code is step 2, and it is genuinely non-vacuous: both
changed files carry a file-scoped `#nullable enable` on line 1 (`WpfDispatcherYield.cs:1`,
`WpfDispatcherYieldTests.cs:1`), so nullable flow analysis runs on them in the ordinary analyzer
build irrespective of the `-p:Nullable=enable` property, and that build did recompile both projects.

Build outputs were restored to a consistent state after this verification
(`MSBuild.exe UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU`,
0 errors). `git status --porcelain` is empty; no tracked file was modified by the review.

The 195-error pre-existing nullable debt predates the merge base, is untouched by this branch, and
remediating it would be a repository-wide refactor. It is reported here, not absorbed.

## 2. Coverage Compliance

Thresholds applied (`.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`
Authoritative Decision #2, uniform across T1-T4): repo-wide line >= 85%, branch >= 75%; new/changed
code line >= 85%, branch >= 75%; no regression on changed lines. CLAUDE.md § UT2 additionally sets
>= 90% for new modules, classes, and methods, which is the stricter figure and is the one applied to
the changed class.

### 2.1 Artifact inventory

| Language | Changed files | Canonical artifact | Present |
|---|---|---|---|
| CSharp | 2 | `artifacts/csharp/coverage.xml` | yes (JaCoCo, gitignored per `.gitignore:57`) |
| TypeScript | 0 | `coverage/lcov.info` | measurement not required, zero changed files of this type |
| Python | 0 | `artifacts/python/lcov.info` | measurement not required, zero changed files of this type |
| PowerShell | 0 | `artifacts/pester/powershell-coverage.xml` | measurement not required, zero changed files of this type |

Committed evidence is package-level JaCoCo (`evidence/baseline/coverage-baseline.jacoco.xml`,
`evidence/qa-gates/coverage-postchange.jacoco.xml`) rather than raw Cobertura. That substitution
follows the convention established by commit `d0955dc4` for issue #503 (verified by
`git show --stat d0955dc4`) and is documented at
`evidence/qa-gates/coverage-artifact-substitution.2026-08-08T17-30.md`. It keeps roughly 20 MB and
378,000 lines out of permanent history. The artifact is present and parseable, so this is not a gap.

### 2.2 Repo-wide figures, recomputed by the reviewer

The reviewer did not rerun coverage generation. The committed JaCoCo counters were re-summed
independently:

| Metric | Baseline | Post-change | Change | Floor | Margin |
|---|---|---|---|---|---|
| Line | 95274/111021 = 85.8162% | 95325/111059 = 85.8328% | +0.0166 pp | 85% | +0.83 pp |
| Branch | 22070/27862 = 79.2118% | 22093/27884 = 79.2318% | +0.0200 pp | 75% | +4.23 pp |

These reproduce the executor's reported figures exactly. `artifacts/csharp/coverage.xml` was also
re-summed and yields byte-identical totals (95325/111059 line, 22093/27884 branch), confirming the
gate artifact is a faithful copy of the committed post-change evidence rather than a separate,
unexplained measurement.

Denominator growth of 38 lines is fully explained: removing `[ExcludeFromCodeCoverage]` added the
changed class to the measured denominator. 45 newly covered lines more than offset it.

Honest framing of the delta: the +0.0166 pp movement is smaller than the observed run-to-run
measurement noise. The `QuickFiler` package, which has zero changed lines on this branch, shows 6
lines flipping from missed to covered between the two reports (3097/14338 -> 3091/14344). The
non-regression conclusion should therefore rest on the absolute figures clearing their floors with
margin — which they do, by 0.83 pp and 4.23 pp — rather than on the sign of a sub-noise delta. Both
readings support the same verdict.

### 2.3 Changed-class figures

`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, per
`evidence/qa-gates/coverage-changed-lines.2026-08-08T17-06.md`:

| Metric | Value | Threshold | Verdict |
|---|---|---|---|
| Line, deduped distinct source lines | 96.43% (27/28) | >= 90% | PASS |
| Line, tool-reported class attribute | 97.37% (37/38) | >= 90% | PASS |
| Branch | 100% | >= 75% | PASS |
| Uncovered lines | exactly 1 (line 46) | — | assessed below |
| Baseline comparand | absent (attribute-excluded, 0 matched elements) | no regression possible | PASS |

Because the raw Cobertura report is no longer committed, the per-class figure cannot be re-derived
directly from committed artifacts. It was corroborated arithmetically instead. Within the
`UtilitiesCS` package the totals moved missed 7537 -> 7530 and covered 69205 -> 69250 (total +38).
A class contributing 38 lines at 37 covered / 1 missed leaves +8 covered and -8 missed elsewhere in
the package — a net-zero line-count shift of exactly the magnitude independently demonstrated by the
unchanged `QuickFiler` package. The reported figure is arithmetically consistent with the committed
package totals. This is corroboration, not proof; recorded as advisory A3.

### 2.4 Assessment of the single uncovered line

Line 46 is the body of the default fallback lambda `() => UtilitiesCS.UiThread.Dispatcher`. The
reviewer independently assessed whether this residual is avoidable.

It is not, without defeating the purpose of the change. The lambda body executes only when the
parameterless constructor is used **and** the thread-affinitized lookup returns null. Reaching that
state requires a test whose outcome depends on the value of the process-global static
`UiThread.Dispatcher` — which is precisely the ambient, set-once, order-dependent state this issue
exists to eliminate. A test written to execute that line would reintroduce the exact defect under
repair. The residual is genuinely irreducible and is correctly accepted rather than chased.

Confirmed by reading `UtilitiesCS/Threading/UiThread.cs:135-140`: `Dispatcher` is a plain static
property over a `null!`-initialized backing field with no initialization side effect, so the lambda
is safe to leave unexercised and cannot show a window if it ever runs.

### 2.5 Per-language verdicts

| Language | Changed files | Repo-wide line | Repo-wide branch | New/changed-code coverage | Verdict |
|---|---|---|---|---|---|
| CSharp | 2 | 85.83% (floor 85%) | 79.23% (floor 75%) | 96.43% line, 100% branch | **PASS** |
| TypeScript | 0 | — | — | — | no changed files of this type; measurement not required |
| Python | 0 | — | — | — | no changed files of this type; measurement not required |
| PowerShell | 0 | — | — | — | no changed files of this type; measurement not required |

Per-language comparison lines:

- CSharp — Baseline: 85.8162% line / 79.2118% branch. Post-change: 85.8328% line / 79.2318% branch.
  Change: +0.0166 pp line, +0.0200 pp branch. New/changed-code coverage: 96.43%. Disposition: PASS,
  both repo-wide floors cleared with margin and the changed class clears the stricter 90% bar.
  Evidence: `evidence/qa-gates/coverage-postchange.jacoco.xml`,
  `evidence/baseline/coverage-baseline.jacoco.xml`,
  `evidence/qa-gates/coverage-changed-lines.2026-08-08T17-06.md`, `artifacts/csharp/coverage.xml`.

## 3. Coverage Exclusion Policy (`.claude/rules/general-unit-test.md`)

The policy states plainly that no production file may be excluded from coverage measurement, and
that the correct response to untestable lines is to refactor for testability rather than to exclude.

This branch removes `[ExcludeFromCodeCoverage]` from `WpfDispatcherYield` (and the now-unused
`using System.Diagnostics.CodeAnalysis;`) at the same time as it makes the class genuinely testable.
That is the policy-preferred direction, and `issue.md:83-84` had flagged in advance that leaving the
attribute in place by inertia would be wrong. The removal is not merely permitted; it is required
once testability is established.

The reviewer specifically checked that the change does not substitute one exclusion for another: no
`[ExcludeFromCodeCoverage]`, no `coverage.config` edit, and no `exclude` entry was added anywhere in
the branch diff. Verdict: **PASS**.

## 4. Prohibited Fixes (`issue.md:111-120`, `.claude/rules/csharp.md`)

Verified independently by the reviewer, not read from the executor's audit. Command:

```
git diff <base>..HEAD -- '*.cs' \
  | grep -nE "DoNotParallelize|\[Ignore|Thread\.Sleep|Task\.Delay|GetTempPath|GetTempFileName|Retry|retry"
exit 1  (no match)
```

| Prohibited approach | Present | Basis |
|---|---|---|
| `[DoNotParallelize]` | no | 0 grep hits; `UtilitiesCS.Test/Properties/AssemblyInfo.cs` is not in the diff, so `Parallelize(Workers = 0, Scope = ClassLevel)` is intact |
| Retry / sleep / timing hack | no | 0 hits for `Retry`, `Thread.Sleep`, `Task.Delay`; `BannedSymbols.txt` lines 4-7 already ban both sleep APIs analytically |
| `[Ignore]` or test deletion | no | 0 hits; test count rose 6293 -> 6295, and `YieldAsync_WithoutDispatcher_RemainsStrict` still exists at `WpfDispatcherYieldTests.cs:118` |
| Weakened assertion | no | `WpfDispatcherYieldTests.cs:134` is still exactly `.ThrowAsync<InvalidOperationException>();`, and the production guard and message at `WpfDispatcherYield.cs:62-67` are byte-identical to the merge-base text |
| Temporary files in tests | no | 0 hits for temp-path APIs; the test uses in-memory delegates and one owned thread |

The `[Timeout(30000)]` attribute used during the fail-before probe was temporary and does not appear
in the final diff (`grep -c "Timeout" WpfDispatcherYieldTests.cs` -> 0). Verdict: **PASS**.

Note on grep scoping: the scan above is restricted to `'*.cs'`. That is not a narrowing of audit
scope — the full 56-file diff was inspected — but a defect avoidance. `.claude/agent-memory/**` is
tracked in this repository and its prose contains the literal token `DoNotParallelize` in an
unrelated memory entry, which produces a false positive on an unscoped grep. The two `.cs` files are
the entire source diff, so nothing is lost.

## 5. Test Policy (`.claude/rules/general-unit-test.md`, CLAUDE.md § UT1-UT5, § CUT1-CUT2)

| Requirement | Assessment | Verdict |
|---|---|---|
| MSTest framework | `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting` | PASS |
| FluentAssertions | `.Should()`, `.ThrowAsync<>()`, `.NotThrowAsync()` throughout | PASS |
| Moq where needed | not needed; hand-written `CountingDispatcherProvider` records invocation order, which Moq would express less directly | PASS |
| Independence | each test constructs its own providers and its own host; no shared or static state | PASS |
| Isolation | one behavior per test | PASS |
| Determinism | see § 5.1 | PASS |
| Arrange-Act-Assert | explicit `// Arrange` / `// Act` / `// Assert` comments in all four tests | PASS |
| Failure messages | every assertion carries a `because` reason string | PASS |
| No external dependencies | no network, database, filesystem, or external process | PASS |
| No temporary files | none | PASS |
| Test file location | `UtilitiesCS.Test/OutlookObjects/Folder/` mirrors `UtilitiesCS/OutlookObjects/Folder/` | PASS |
| Banned APIs in tests | no `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, or wall-clock wait | PASS |

### 5.1 `StaDispatcherHost` and UT4

`StaDispatcherHost` (`WpfDispatcherYieldTests.cs:172-199`) starts a real WPF `Dispatcher` pumping on
an STA thread the test class owns. The reviewer assessed this against UT4 specifically, because a
naive reading of "no external processes" might be thought to exclude it.

It is acceptable:

1. **Not an external dependency.** UT4 prohibits databases, networks, remote APIs, and external
   processes. This is an in-process thread owned by the test, with no resource outside the test host.
2. **It cannot create a visible window.** This is a WPF `System.Windows.Threading.Dispatcher`, not
   `System.Windows.Forms.Application.Run` and not a `Window`. `Dispatcher.Run()` starts a message
   loop only; no `Window`, `Form`, or `Control` is ever instantiated in this file. The distinction
   matters because issue #511 tracks a WinForms pump-host defect, and that mechanism is not present
   here.
3. **Deterministically torn down.** The constructor blocks on `_ready.WaitOne()` until the thread has
   published its dispatcher; `Dispose` calls `BeginInvokeShutdown(DispatcherPriority.Send)` then
   `Join()` then disposes the event. Every use site wraps the host in `using`.
4. **Established repo precedent.** The identical pattern exists at
   `FolderTreeSnapshotBuilderYieldTests.cs:118-147` and in seven other test files. The new copy is
   strictly safer than the precedent because it additionally sets `IsBackground = true`
   (`WpfDispatcherYieldTests.cs:185`), which the precedent omits.
5. **A real pump is required, not decorative.** `YieldAsync` posts at `DispatcherPriority.Background`;
   an operation posted to a non-pumping dispatcher never completes. A non-pumping fake would hang.

One robustness gap is recorded as advisory A1 rather than as a violation: `Join()` is unbounded and
the tests carry no `[Timeout]`, so a hypothetical shutdown failure would hang the run rather than
fail it.

### 5.2 Memory visibility

`CountingDispatcherProvider._invocationCount` is a plain `int` incremented inside the delegates and
read after `await`. This was checked rather than assumed. Both delegate invocations occur in the
synchronous prologue of `YieldAsync` (`WpfDispatcherYield.cs:61`), before the first suspension point
at line 69, so they run on the awaiting test's own thread; the subsequent `await` establishes the
happens-before edge for the assertion read. No `Interlocked` or `volatile` is required. Correct as
written.

## 6. General Code Change Policy

| Rule | Assessment | Verdict |
|---|---|---|
| File size <= 500 lines | `WpfDispatcherYield.cs` 44 -> 77; `WpfDispatcherYieldTests.cs` 39 -> 201 (`awk END{print NR}`) | PASS |
| Simplicity first | two `Func<>` fields and one constructor; no framework, no container, no interface indirection | PASS |
| Separation of concerns | resolution policy stays in the class; only the two lookups are externalized | PASS |
| Public API stability | see § 6.1 | PASS |
| Naming | `_currentThreadDispatcherProvider`, `_fallbackDispatcherProvider`, `CountingDispatcherProvider`, `StaDispatcherHost` — descriptive, conventional | PASS |
| XML documentation on non-obvious API | both constructors and both parameters documented (`WpfDispatcherYield.cs:17-36`) | PASS |
| Comment why, not what | the pre-existing resolution-order rationale comment at lines 53-59 is preserved verbatim; new doc comments explain the seam's purpose | PASS |
| Error handling unchanged | `InvalidOperationException` guard and message text byte-identical | PASS |
| No new dependencies | none | PASS |
| Reusability / no copy-paste | see advisory A2 | PASS with advisory |

### 6.1 Public API preservation

Verified by grep, not by assertion:

```
grep -rn "new WpfDispatcherYield" --include=*.cs .
  TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:365          new WpfDispatcherYield()
  UtilitiesCS.Test/.../OutlookFolderTreeServiceConcurrencyTests.cs:55  new WpfDispatcherYield()
  UtilitiesCS.Test/.../WpfDispatcherYieldTests.cs:22,60,93,125         new WpfDispatcherYield(<seam args>)
```

Neither pre-existing call site is in the branch diff, and both still bind to a public parameterless
constructor. Adding any constructor removes C#'s implicit one, so the explicit
`public WpfDispatcherYield() : this(null, null) { }` at `WpfDispatcherYield.cs:21-22` is mandatory
to preserve the signature — and it does preserve it exactly, including binary compatibility.

The seam constructor is `internal` (`WpfDispatcherYield.cs:37`), reachable from tests solely through
the pre-existing `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` at
`UtilitiesCS/Properties/AssemblyInfo.cs:19` (present at the merge base; `AssemblyInfo.cs` is not in
the diff). This is a legitimate testability mechanism and not a widened public surface: the public
API of the assembly is unchanged in signature terms.

## 7. Compliance Summary

| # | Area | Verdict | Blocking |
|---|---|---|---|
| 1 | Toolchain order and clean pass | PASS | no |
| 2 | CSharp coverage thresholds and non-regression | PASS | no |
| 3 | Coverage exclusion policy | PASS | no |
| 4 | Prohibited fixes | PASS | no |
| 5 | Unit test policy (general and C#) | PASS | no |
| 6 | General code change policy | PASS | no |
| 7 | Evidence location compliance | PASS | no |
| 8 | Acceptance criteria (see feature audit) | PASS | no |

**Blocking findings: 0.**

## Evidence Location Compliance

All evidence artifacts produced by this feature live under
`docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/<kind>/`,
using the canonical kinds `baseline/`, `qa-gates/`, `regression-testing/`, `issue-updates/`, and
`other/`.

Scan for non-canonical evidence paths in the branch diff:

```
git diff --name-only <base>..HEAD | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"
exit 1  (no match)
```

Zero violations. No file is written to `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/`, or `artifacts/coverage/`.

`scripts/dev_tools/validate_evidence_locations.py` does not exist in this repository, so the scripted
check could not be run; the equivalent scan was performed directly with `git diff --name-only` as
shown above. This is recorded as a tooling gap in the review procedure, not as a defect in the
change.

The one artifact written outside the feature folder is `artifacts/csharp/coverage.xml`, which is the
canonical gate path mandated by `.claude/hooks/validate-feature-review-coverage.ps1` and is
gitignored (`.gitignore:57`). It is correctly regenerated rather than committed.

## Advisory Findings (non-blocking)

| # | Severity | Location | Finding |
|---|---|---|---|
| A1 | Advisory | `WpfDispatcherYieldTests.cs:196` | `_thread.Join()` is unbounded and no test carries `[Timeout]`. A failure to process `BeginInvokeShutdown` would hang the suite rather than fail it. `IsBackground = true` protects process exit but not the blocking `Join`. Recommend `Join(TimeSpan.FromSeconds(10))` plus an assertion, or a class-level `[Timeout]`. |
| A2 | Advisory | `WpfDispatcherYieldTests.cs:172-199` | `StaDispatcherHost` is now duplicated in nine test files across `UtilitiesCS.Test` and `TaskMaster.Test`. Extraction to shared test support would require adding a file to a legacy non-SDK `.csproj` with explicit `<Compile Include>` items, which the scope boundary forbade. Following the existing pattern was the right call here; the duplication is a repository-level follow-up. |
| A3 | Advisory | `evidence/qa-gates/coverage-changed-lines.2026-08-08T17-06.md` | Cites `evidence/qa-gates/coverage-postchange.cobertura.xml` as its source, which the artifact substitution removed. The per-class 96.43% figure is not directly re-derivable from committed artifacts; it was corroborated arithmetically (§ 2.3). Recommend that future substitutions also record the per-changed-file line and branch counts inline so downstream review stays self-contained. |
| A4 | Advisory | `UtilitiesCS/Threading/UiThread.cs:135-140` | `public static Dispatcher Dispatcher` is annotated non-nullable but backed by a `null!`-initialized field. The changed code defends against this correctly by declaring the local `Dispatcher?`, but the annotation itself is inaccurate. Pre-existing, out of the two-file boundary; a follow-up candidate. |
| A5 | Advisory | `WpfDispatcherYield.cs:46` | The single uncovered line was assessed and accepted as irreducible (§ 2.4). Recorded so the acceptance is explicit and reviewable rather than silent. |
| A6 | Advisory | `artifacts/pr_context.summary.txt` | Was stale and misclassified the source changes. Regenerated during review; see below. |

## Review-Time Corrections to PR Context Artifacts

The PR context artifacts were stale and materially wrong, and were regenerated before the audit
proceeded, as the reviewer contract requires. Both files are gitignored build artifacts; no policy
document, source file, or feature document was modified.

1. **Stale head.** The summary recorded head `69d3867164edaecaa5dcc2a8ed414454f85439bc` against an
   actual `HEAD` of `7466096d73ef86f3cc5b9d5da6648cf156c02d6f`. The difference is real, not
   cosmetic: the recorded head still contained the two ~10 MB raw Cobertura reports that the final
   commit replaced with JaCoCo summaries (`git diff 69d3867..HEAD --stat` shows 374,314 deletions).
   Refreshed to the true head.
2. **Source changes misclassified.** The overview reported `Core logic changes: 0 files` and filed
   both `.cs` files under docs and tooling. This is a recurring generator defect, and it has a
   concrete consequence: `Get-ChangedLanguageSet` in the coverage hook derives the changed-language
   set from those overview bullets, so the misclassification would have caused the hook to skip
   enforcement for this branch entirely. Corrected to list both source files in the required
   `- <path> (+N/-N)` form. Verified by dot-sourcing the hook and calling
   `Get-ChangedLanguageSet` against the regenerated file, which now returns `CSharp`.
3. **GitHub CLI status wrong.** The summary asserted `gh` is not installed. It is installed and
   authenticated; `gh issue view 511` returned
   `{"number":511,"state":"OPEN","title":"Bug: winformspumphost-tests-load-flaky-visible-window"}`.
   Corrected.
4. **False auto-close candidates.** The summary listed `#503`, `#507`, `#508`, and the literal token
   `#ISO-8601` as author-asserted closing issues. Those are a text scan of evidence documents, not
   author intent. This branch closes `#508` only. Corrected, and recorded here as a generator defect
   rather than a defect in the change.

## Adjudicated Context (sanity-checked, not re-litigated)

- **Two `QuickFiler.Test` pump-host failures.** `InitializeBool_ThroughThePumpHost_*` and
  `InitializeNineArgOverload_ThroughThePumpHost_*` fail intermittently with
  `InvalidOperationException: Invoke or BeginInvoke cannot be called on a control until the window
  handle has been created` from `QfcItemController.FocusAndTheme.cs:256`. The attribution reasoning
  at `evidence/regression-testing/preexisting-failure-attribution.2026-08-08T16-52.md` was checked
  and holds: run D reverts both changed files to the merge base, rebuilds, and reproduces the same
  two failures at 6293/6291/2 — matching the pre-work baseline recorded at `issue.md:53` before this
  branch existed. The failure path involves WinForms `Control.MarshaledInvoke`, no WPF `Dispatcher`,
  and no code in the diff. Tracked by issue #511 (confirmed OPEN). Out of the boundary of this
  change, and the branch neither causes nor conceals it.
- **Pass-3 abandonment.** The stale-build detection is sound and the disclosure is the correct
  behavior. `Copy-Item` preserves `LastWriteTime`, so restored files were older than build outputs
  and MSBuild's up-to-date check skipped `CoreCompile`; the executor detected this from the missing
  `CS2002`/`CoreCompile` signals and restarted rather than banking a false pass. SHA-256 of both
  files is unchanged across the experiment, so only filesystem metadata moved. This is precisely the
  failure mode that produces bogus green gates, and it was caught.

## Merge-Time Obligation

AC4 requires that the production change "is justified in the PR body". No PR body exists at review
time (`artifacts/pr_body*` absent; `pr-author` has not run). The technical substance of the
justification is fully recorded in `evidence/qa-gates/no-behavior-change.2026-08-08T17-08.md` and in
the plan's seam-shape design section. **The PR body must carry that justification before merge.**
This is a downstream authoring step, not a deficiency in the code, so it is tracked here as an
obligation rather than as a blocking finding.

## Appendix A — Commands Run by the Reviewer

All commands were check-only. No tracked file was modified; `git status --porcelain` is empty at the
end of the review.

```
git rev-parse HEAD
git merge-base HEAD origin/main
git diff --stat  <base>..HEAD
git diff --numstat <base>..HEAD -- '*.cs'
git diff --name-only <base>..HEAD -- '*.csproj' '*.sln' '*.props' '*.targets' '*.ps1' '*.py' '*.ts'
git diff --name-only <base>..HEAD | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"
git diff <base>..HEAD -- '*.cs' | grep -nE "DoNotParallelize|\[Ignore|Thread\.Sleep|Task\.Delay|GetTempPath|GetTempFileName|Retry|retry"
git show 003c5715:UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs | wc -l
git show --stat d0955dc4
grep -rn "new WpfDispatcherYield" --include=*.cs .
grep -rn "InternalsVisibleTo" UtilitiesCS/Properties/AssemblyInfo.cs
grep -rln "class StaDispatcherHost" --include=*.cs .
awk 'END{print NR}' <each changed file>
csharpier.exe check UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs
MSBuild.exe UtilitiesCS.Test/UtilitiesCS.Test.csproj -t:Build -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true
MSBuild.exe UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Nullable=enable -p:TreatWarningsAsErrors=true
MSBuild.exe UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU   # restore
gh issue view 511 --json number,title,state
python  # re-sum JaCoCo counters in baseline, post-change, and the canonical gate artifact
pwsh    # dot-source validate-feature-review-coverage.ps1 and call Get-ChangedLanguageSet
```

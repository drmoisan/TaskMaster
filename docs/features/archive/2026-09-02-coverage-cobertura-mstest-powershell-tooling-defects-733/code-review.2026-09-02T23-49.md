# Code Review — issue #733 (coverage-cobertura-mstest-powershell-tooling-defects)

- Timestamp: 2026-09-02T23-49
- Branch: bug/coverage-cobertura-mstest-powershell-tooling-defects-733
- Base: origin/main @ 8be5a6aacb (merge base recomputed, matches caller)
- Head: 6c9329a3599a590ac7699d48d103f96de0d0ac5d
- Scope reviewed: the full branch diff, 63 paths, of which 14 are `.ps1`

## What Changed

Six production files and eight test files under `scripts/vscode/` and `tests/scripts/vscode/`.

| File | Nature of change |
|---|---|
| Invoke-MSTestWithCoverage.Helpers.ps1 | Two dot-sources added; `Get-CoberturaCoverageSummary` refactored to delegate per package; union-append loop for `<methods>`; package-level rate recomputation; stale comment corrected; `Assert-CoberturaLineCoverageThreshold` relocated out |
| Invoke-MSTestWithCoverage.PackageRate.ps1 | New. One pure function, `Get-CoberturaPackageLineSummary` |
| Invoke-MSTestWithCoverage.Threshold.ps1 | New. `Assert-CoberturaLineCoverageThreshold` relocated verbatim, comment-based help added |
| Invoke-MSTestWithCoverage.ps1 | One added `-notmatch '\\\.claude\\'` clause in the discovery predicate |
| Invoke-MSTestWithCoverage.ClosureFilter.ps1 | Comment-only. Two `.DESCRIPTION` addenda |
| Invoke-MSTest.ps1 | `Get-VsTestConsolePath` seam added; `Get-MSTestAssemblyPathList` extracted; whole top-level body extracted into `Invoke-MSTestMain`; dot-source-guarded wiring |
| Eight test files | 22 net new It cases, 2 deliberately reversed assertions, 2 ceiling-driven file splits |

## Design and Structure

**Separation of concerns — strong.** The `Invoke-MSTest.ps1` restructuring is the clearest
improvement in the change. The file previously mixed a bare host-bound script body with three
helper functions; every guard, error message, and ordering decision was unreachable from a test.
It now follows the shape its sibling `Invoke-MSTestWithCoverage.ps1` already used: helpers, a
named main function, and a two-line dot-source-guarded entry point. That is the structure
`.claude/rules/general-unit-test.md`'s Coverage Exclusion Policy explicitly prescribes, and it
moved the file from 68.89% to 94.00% command coverage with only three commands left uncovered,
all of them irreducibly host-bound.

**Seam discipline — correct.** `Get-VsTestConsolePath` follows the wrapper-function seam pattern
in `.claude/rules/powershell.md` section "Design Seams", matching the existing `Invoke-VsTestExe`
in the same file and `Invoke-VsWhereExe` in the sibling. No injectable-delegate or runner framework
was introduced, which the rules discourage.

**Reuse — the stated goal is met.** `Get-CoberturaPackageLineSummary` has exactly the two callers
the spec named: `Get-CoberturaCoverageSummary` sums one summary per package into the document
totals, and `Merge-CoberturaClassesByFilename` recomputes a package's rates after the merge. The
rounding expression and the `'0'` zero-denominator fallback are byte-identical to the ones the
document-level summarizer already used, which is the invariant the spec's Boundaries section
required.

**File placement — driven by a hard constraint, correctly resolved.** Two production files and two
test files exist only because `Helpers.ps1` (492 lines) and `Helpers.Tests.ps1` (498 lines) had no
room. Choosing `Assert-CoberturaLineCoverageThreshold` as the extraction unit was the right pick:
it is the only function in Helpers.ps1 with no in-file caller and no in-file dependency, so the
move is a pure relocation with no coupling consequence, and its tests moved with it into a matched
sibling name.

**Documentation quality — above the repo norm.** Every new function carries comment-based help with
`.SYNOPSIS`, `.DESCRIPTION`, `.PARAMETER`, and `.OUTPUTS`. The comments explain *why* (the
500-line ceiling, the return-enumeration hazard, the safe failure direction) rather than restating
*what*, which is what `.claude/rules/general-code-change.md`'s Naming section asks for.

## Correctness Review of Each Production Change

### Finding 1 — package rate recomputation (Helpers.ps1 lines 397-401)

Placed at the end of the per-package loop, after every filename group in that package has been
merged and after the stale class nodes have been removed, so the recomputation sees the final
class set. Ordering inside `ConvertTo-KoverageCoberturaXml` is also correct: `Merge-...` at line
440 runs before `Get-CoberturaCoverageSummary` at line 453, so the document rate is derived from
merged content. Verified by reading, and pinned by the extended assertions in "computes the merged
per-file line-rate from the merged rollup alone".

### Finding 2 — union-append of `<methods>` (Helpers.ps1 lines 299-307)

Deep-clones every non-primary group member's `./methods/method` children into the merged class's
methods node, with no deduplication key, matching the spec's explicit prohibition. Correct.

The double-count risk this creates was checked and does not materialise:
`Get-CoberturaClassLineSummary` enumerates the class-level rollup and the method-level view into
one map keyed by line number, resolving repeats by maximum hits. The union'd method lines therefore
merge with, rather than add to, the class rollup. Confirmed by reading lines 190-232 and by the
`LinesValid | Should -Be '2'` assertion in the new overload-collision pinning test, where four
`<line>` elements across two classes reduce to two counted lines.

`Where-Object { $_ -ne $primaryNode }` relies on reference equality between `XmlElement` instances.
That holds here because both sides come from the same `$group` array built from one `SelectNodes`
call. Correct, though implicit.

### Finding 3 — `.claude` discovery exclusion (Invoke-MSTestWithCoverage.ps1 line 301)

One added clause in the existing style, inside the existing `@(...)` wrapper, which is unchanged.
The regex `'\\\.claude\\'` requires a full path segment (backslash on both sides), so it will not
match a project literally named something ending in `.claude`. Correct and minimal.

### Findings 5 and 6 — ClosureFilter.ps1 documentation

Comment-only, zero executable lines changed. The finding-6 addendum names both failure directions
explicitly and records why a signature re-key is infeasible, which is exactly what the plan's P3-T2
acceptance required. The prose is accurate against the code it describes.

### Finding 7 — `Get-MSTestAssemblyPathList` (Invoke-MSTest.ps1 lines 97-127)

The plan's task P4-T4 specified `return @(...)`. The implementation is `return , @(...)`.

The reviewer verified the semantics independently in a clean `pwsh -NoProfile` session rather than
accepting the executor's account:

```
function a { return   @() }   -> caller receives $null
function b { return , @() }   -> caller receives Object[] of Count 0
function c { return   @('x') } -> caller receives System.String
function d { return , @('x') } -> caller receives Object[] of Count 1
```

and, under `Set-StrictMode -Version Latest`, `.Count` on both a bare `String` and on `$null` throws
`PropertyNotFoundException`. The plan's literal `return @(...)` would therefore not have fixed
finding 7 at all: the array would have been unwrapped again at the return boundary and
`$testAssemblies.Count` at line 181 would still throw on a single-match run. **The deviation is not
merely warranted, it is required for the fix to work.**

Documentation of the deviation is adequate: the function's `.DESCRIPTION` at lines 107-108 states
"A function return enumerates its output, which would unwrap the array again, so the unary comma
below is what delivers the same array shape to the caller." The evidence artifact
`case-10-assembly-discovery-array-shape-discriminating.2026-09-02T22-57.md` records a two-run
measurement with the comma removed, in which the two shape assertions fail and the three older
`@($result).Count` assertions pass.

### The `Invoke-MSTestMain` extraction and the dot-source guard

Every guard, `throw` message, ordering decision, and the `-NoExecute` early return were compared
line by line against the pre-change body and are semantically unchanged; `$PSScriptRoot` became an
injectable `ScriptRoot` parameter defaulting to `$PSScriptRoot`, which is the only substantive
difference and is the seam that makes the guards testable.

The new entry point is `if ($MyInvocation.InvocationName -ne '.') { Invoke-MSTestMain @PSBoundParameters }`.
The reviewer checked every caller in the repository for a regression:

- `.vscode/tasks.json` line 179-180 invokes with `pwsh -File scripts/vscode/Invoke-MSTest.ps1`.
- `.codex/codex-web-setup.sh` line 343 invokes with `pwsh -NoProfile -ExecutionPolicy Bypass -File ...`.

Both are `-File` invocations, where `InvocationName` is the script path, so the guard passes and
`Invoke-MSTestMain` runs. No CLI regression. The three test files dot-source the script, where the
guard correctly suppresses execution — which also let `Invoke-MSTest.RunSettings.Tests.ps1` drop
its previous `try { . $script:mstestScript -NoExecute } catch { ... }` swallow-all wrapper, a real
improvement: a genuine parse or load failure will now surface instead of being written to verbose
output.

## Test Review

### Determinism, isolation, and independence

- Reviewer re-ran the full suite: **92 passed, 0 failed, 0 skipped** across 10 files, Pester 5.6.1.
- Reviewer ran each of the 10 test files **individually and in reverse-alphabetical order**. Every
  file produced its standalone count unchanged (2, 5, 11, 27, 12, 20, 2, 2, 5, 6 = 92). No
  order dependence, no cross-file leakage.
- Two consecutive full runs produced identical counts and identical per-file coverage figures.
- Zero `Start-Sleep`, zero retries, zero timing hacks in the changed test tree.
- Zero temporary files. Every fixture is an inline here-string. The only `Set-Content` and
  `Remove-Item` references in the tree are `Mock` registrations and `Should -Invoke` assertions,
  all pre-existing.

### No external process is launched

This was checked three ways rather than assumed:

1. By reading. `vswhere.exe` is reachable only through `Get-VsTestConsolePath`, which
   `Invoke-MSTest.Main.Tests.ps1` line 61 mocks in a `BeforeEach`. `vstest.console.exe` is
   reachable only through `Invoke-VsTestExe`, mocked at line 63. `Invoke-MSTestWithCoverageMain`'s
   tests mock `Invoke-VsWhereExe` and `Invoke-DotnetCoverageCollection`.
2. By coverage. `Get-VsTestConsolePath`'s external pipeline at Invoke-MSTest.ps1 lines 93-94 is one
   of only three uncovered commands in the entire file. If any test had launched `vswhere.exe`,
   those lines would show as executed. They do not.
3. By the one apparent exception. `Invoke-MSTest.Main.Tests.ps1` line 41 calls the real
   `Invoke-VsTestExe` with `-VsTestPath 'Join-Path'`. `Join-Path` is an in-process cmdlet, not an
   executable; the call proves the splatting contract without spawning anything. The test's own
   comment says so, and the returned value `'C:\alpha\beta'` confirms it.

### Discriminating power of each regression test

| Test | Can it fail on the defect it pins | Evidence |
|---|---|---|
| package rate assertions in "computes the merged per-file line-rate from the merged rollup alone" | Yes | `case-03` records the package node holding the fixture's stale `'0'` pre-fix against the asserted `'0.6'` |
| "preserves the primary class methods subtree..." (reversed) | Yes | `case-04` records `methodNodes.Count` = 1 pre-fix against the asserted 2 |
| "unions the methods of every group member into the merged class" | Yes | `case-05` records only method `M` present pre-fix against the asserted `M,N,O` |
| "takes the higher hits value when the second class seen..." | **No, by design** | Deliberately not tagged expect-fail. Production already handled `max(hits)` correctly; this closes a coverage gap identified by finding 4. The fixture is nonetheless well built: the second-seen entry is strictly higher, so a first-seen-wins or last-seen-wins implementation would both be distinguishable from `max()`. Disclosed in the spec's corrected scope for finding 4 |
| "excludes assemblies discovered under a .claude worktree segment" | Yes | `case-07` and `expect-fail-run-phase2` record both paths present in the captured array pre-fix |
| "retains a closure whose bare member name collides with a non-exempt overload" | **No, by design** | A characterization test pinning an accepted limitation in its safe under-exclusion direction. It cannot fail on a defect because no defect is being fixed; it fails if someone flips the behavior toward over-exclusion, which is its purpose. Disclosed in the spec's corrected scope for finding 6 |
| the three original `@($result).Count` array-safety cases | **No, on array shape** | The `@(...)` at the assertion site restores shape locally. They fail pre-fix only on `CommandNotFoundException` |
| the two `($result -is [array])` shape cases | Yes | `case-10` records a direct measurement with the comma removed: these two fail while the three above still pass |

The executor found and closed the non-discriminating-assertion gap itself, in task H1, and recorded
the two-run proof. That is the right handling and is credited here rather than raised as a finding.

### Test structure

Arrange-Act-Assert is followed throughout. Every new It carries either a descriptive name that
states the scenario and expectation, or a leading comment explaining the scenario, or both. Several
comments do genuine work — for example the ClosureFilter pinning test explains why the XPath
predicate uses unescaped `<>` (predicates compare parsed attribute values) and why the line count
is scoped to the closure class's own rollup rather than counted unscoped.

## Findings

No blocking defect was found. The following are advisory.

### CR-1 — Package rate is not recomputed for a package with no `<classes>` child (Low)

`Merge-CoberturaClassesByFilename` line 267-269 `continue`s when a package has no `./classes` node,
which skips the new recomputation at lines 397-401. Such a package keeps whatever `line-rate` the
input document carried. Every other package now gets a freshly computed rate, so the document is
internally inconsistent in that one case. A package with no classes is degenerate and its correct
rate is `'0'`, which the helper would produce. Suggested change: move the recomputation above the
`continue`, or recompute in a second pass over `//package`.

### CR-2 — Union-appended `<method>` nodes retain their source class's stale rate attributes (Low)

The clones appended at line 305 carry the `line-rate` and `branch-rate` the source class computed
for them. The merged class's own rate, the package rate, and the document rate are all recomputed
and are unaffected, because `Get-CoberturaClassLineSummary` derives everything from `<line>`
elements and ignores method-level rate attributes. The exposure is limited to a downstream
Cobertura report viewer that reads method-level rates and would see values that no longer
correspond to the merged class's context. Suggested change: recompute each appended method's rate,
or state in the union-append comment that method-level rates are intentionally left as-is.

### CR-3 — Tests mutate `$global:LASTEXITCODE` (Low)

`Invoke-MSTest.Main.Tests.ps1` lines 67 and 139 set `$global:LASTEXITCODE` so the
`if ($LASTEXITCODE -ne 0)` guard can be exercised. `.claude/rules/powershell.md` line 31 says to
avoid global state. The mutation is hard to avoid here, since `$LASTEXITCODE` is an automatic
variable the production code reads directly, and the `BeforeEach` re-registers the mock so
within-file ordering is safe. The reviewer confirmed empirically that the file passes standalone
and that no other file's result changes with it present. Left as advisory; the alternative
(threading the exit code through another seam) would add indirection for little gain.

### CR-4 — `Invoke-MSTest.ps1` still lacks the `.claude` discovery exclusion its sibling gained (Medium, out of scope for this item)

Finding 3's exclusion was applied only to `Invoke-MSTestWithCoverage.ps1`, per the spec's explicit
scoping. `Get-MSTestAssemblyPathList` retains only the `bin/<Configuration>`, `obj`, and `ref`
clauses. The consequence is concrete: `Invoke-MSTest.ps1 -SearchRoot .` run from the repository
root — which is exactly what `.vscode/tasks.json` line 181-182 does — will discover and run test
assemblies built inside `.claude/worktrees/` agent worktrees. The two scripts now have
asymmetric discovery semantics, which is a maintenance hazard.

This is not a defect in the delivered change: the spec, the plan's Scope Prohibitions, and the
issue all scope finding 3 to the coverage script only, and widening it here would have been an
out-of-scope edit. Recommend promoting it to its own issue rather than leaving it as prose in a
feature folder that disappears at merge.

### CR-5 — `Get-CoberturaCoverageSummary` package enumeration narrowed (Informational)

The loop changed from `$packagesNode.ChildNodes` filtered to Element nodes, with
`$pkg.SelectNodes('.//class')`, to `$packagesNode.SelectNodes('./package')`. For valid Cobertura
these are equivalent, since `<packages>` has only `<package>` children. A non-`package` element
child would now be silently skipped rather than searched for descendant classes. No behavior change
against any real input; noted only so the narrowing is on the record.

### CR-6 — Evidence artifact line citation has drifted (Informational)

`case-10-...md` cites `Invoke-MSTest.ps1` line 100 for the `return , @(...)` statement; it now sits
at line 120. The citation was almost certainly accurate when written at 22:57, before the
`Invoke-MSTestMain` extraction at roughly 23:21 shifted the file. Harmless, but a reader following
the citation today lands in the wrong place.

### CR-7 — Two comment-only inaccuracies (Informational)

The `Get-CoberturaPackageLineSummary` `.SYNOPSIS` says it "Reduces one Cobertura `<package>` element
to a deduplicated line and branch summary." The deduplication happens inside
`Get-CoberturaClassLineSummary`, per class; this function only sums those results. The wording is
defensible but slightly overstates what this function does.

### CR-8 — Pre-existing absolute host paths with an account name in a test fixture (Medium, pre-existing)

`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` lines 41, 42, 104 and 124 embed
`C:\Users\<account>\repos\TaskMaster` and `C:\Users\<account>\repos\TaskMaster-wt-2026-07-04-12-57`
as fixture data. The reviewer confirmed by `git show origin/main:...` that all four are present
verbatim at the identical line numbers on the base branch. **Not introduced by this change**, and
the file's other changes are two assertion updates elsewhere in the file.

Every fixture path added by this change uses the synthetic `C:\repo\...` form, which is the correct
pattern. Recommend a separate cleanup issue to convert the four pre-existing occurrences to the
same synthetic form; converting them here would have been an unrelated edit.

## Best-Practice Checklist

| Practice | Verdict |
|---|---|
| Simplicity first, no clever indirection | PASS. The one non-obvious construct (the unary comma) is required and is explained in place |
| Reusability, no copy-paste | PASS. The package summarizer has two callers and reuses the existing rounding expression rather than duplicating it |
| Extensibility, stable public surface | PASS. No existing function signature changed. Three functions added, none removed. `Assert-CoberturaLineCoverageThreshold` remains resolvable through the Helpers.ps1 dot-source chain, verified by the still-passing `Mock Assert-CoberturaLineCoverageThreshold` in RunSettings.Tests.ps1 |
| Separation of pure logic from I/O | PASS. `Get-CoberturaPackageLineSummary` and `Get-MSTestAssemblyPathList` are the pure and near-pure units; every process launch sits behind a named seam |
| Fail fast, explicit errors | PASS. Every `throw` message preserved verbatim; no new broad catch introduced; one swallow-all `catch` in a test BeforeAll was removed |
| Comment why, not what | PASS. Comments cite issue #733 and the specific finding, and explain the reasoning |
| Cohesive modules, small public surface | PASS. Two new files each hold exactly one function |
| Existing tests treated as part of the spec | PASS. The one reversed assertion is called out in spec.md's Risks and Mitigations as a deliberate, spec-approved change, and the test's own comment was rewritten to say what it now locks |
| No dependency added | PASS. Zero new modules or tools |

## Verdict

**PASS.** Zero blocking findings. Eight advisory findings, of which two (CR-4, CR-8) describe
pre-existing conditions this item correctly declined to widen its scope to fix and which should be
promoted to their own issues.

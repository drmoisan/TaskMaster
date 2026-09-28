---
name: project-871-qfcqueue-enqueue-seams-plan-seams
description: Issue #871 planning seams — the coverage runner writes the artifact before its repo-wide 80% assertion, a PowerShell try/catch cannot catch an external process failure, vstest.console never compiles, and a relocated-vs-new classification must be diff-derived not judgment-based
metadata:
  type: project
---

Planning seams found while authoring the atomic plan for issue #871 (QfcQueue enqueue-path
injectable seams), re-derived against the tree at merge commit 2405a829d.

**1. The coverage runner writes the artifact BEFORE it throws its own threshold assertion.**
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` post-processes and writes the Cobertura document at
line 342 and only then calls `Assert-CoberturaLineCoverageThreshold` at line 344. That assertion
lives in `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1:52-55` and throws whenever the
DOCUMENT-level line-rate is under 80 percent. A single-assembly run instruments every loaded module,
so the document rate is far below 80 even when the assembly under test is well covered, and the
script exits non-zero with `$ErrorActionPreference = 'Stop'`. **How to apply:** wrap the invocation
in try/catch, record the terminating message with `ExpectedExitCode: 1`, and place the gate on
artifact CONTENT (the post-processor's repo-relative `filename=` attributes prove the post-process
ran) rather than on the exit code.

**2. The single-assembly `-SearchRoot` defect does NOT apply to the coverage runner.**
[[reference_invoke_mstest_single_searchroot_defect]] is about the sibling `Invoke-MSTest.ps1`.
`Invoke-MSTestWithCoverage.ps1:296` wraps discovery in `@( ... )`, so `-SearchRoot QuickFiler.Test`
is safe there. Do not carry the `-SearchRoot .` rule across to the coverage runner — repo-wide is
what stalls this host on the shell-icon classes.

**3. There is no `.config/dotnet-tools.json` in this repo.** The CSharpier 1.2.6 manifest is at the
repository ROOT as `dotnet-tools.json`. A Phase 0 acceptance clause that Test-Paths the `.config`
form fails on a correct tree.

**4. A new-code >=90% floor is unreachable when a seam relocates an untestable body.**
#871 moves three UI-marshalling bodies into a new adapter class. Those bodies read the process-wide
WPF dispatcher and can never be covered headlessly; counting them as "new code" puts the new-code
rate near 57 percent. **How to apply:** author a mechanical classification rule — a line is
RELOCATED when the same statement appears verbatim, modulo indentation and receiver name, in the
tree at the recorded anchor, otherwise NEW — report both rates, gate only the new rate, and route
every relocated-and-still-uncovered statement into the residual record. Never reach for
`[ExcludeFromCodeCoverage]`: the repo's coverage-exclusion policy forbids it and the maintainer
decision on #727 sub-finding 4 rules it out by name.

**5. A repository-wide coverage floor cannot be measured on this host.** Four shell-icon test
classes in another assembly stall vstest, and the coverage runner hard-codes its TestCaseFilter so
they cannot be filtered out. Plan a PROJECTION instead: take lines-covered/lines-valid from the most
recent committed repo-wide Cobertura in the tree (the 2026-09-08 item-825 qa-gates artifact, 56029
and 65402 at `:2`), add the package-level delta measured between two identically-scoped runs, and
gate the quotient. Deltas from two same-scope runs make the stale reference safe.

**6. `QfcQueue.LoadControllersViewersAsync` is `private` and returns `ValueTask<List<QfcItemGroup>>`.**
A non-zero-`start` index-mapping test cannot call it directly; it needs a reflection invoke plus a
cast of the boxed return before awaiting. `QfcPreScoredItem` has a public three-argument constructor
taking the mail item, a folder path string and an `IFolderSearchHandler`
(`QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs:297`).

**7. The Write Set permitted exactly ONE new test file**, so ~26 tests had to fit under 500 lines.
Preflight round 1 rejected that: 28 test cases plus a 120-180-line harness does not fit, and the
mitigation had nowhere to compress to. The Write Set was widened to a sibling `.Harness.cs` PART
FILE of the same partial test class. **How to apply:** when a suite is near the ceiling, plan the
part file UP FRONT and set the interim trigger below the ceiling (470, not 480) because the repo-wide
format can ADD physical lines; and give the authoritative post-format measurement a remediation
branch, never an unconditional acceptance. See [[test-fixture-sizing-lines-per-test]].

## Preflight round 1 findings (2026-09-12)

**8. vstest.console.exe never compiles anything.** A run of N incremental test-authoring tasks that
each edit a `.cs` file and then invoke the runner directly observes a STALE assembly for every task
but the one after the last build. Sixteen tasks failed this way. **How to apply:** interleave a
`msbuild /t:Build` step (plain build target, correct here precisely because it is NOT a gate) into
every task that authors a test and then runs it, and gate on its `0 Error(s)` summary line rather
than on its exit code.

**9. A PowerShell `try`/`catch` does not catch an external process failure.** Wrapping
`& pwsh -File runner.ps1` in try/catch produces a catch block that never runs, so an `EXIT_CODE:`
field sourced from it has no source. **How to apply:** merge the child's error stream with `2>&1`,
capture the output, read `$LASTEXITCODE`, and match the terminating message text out of the captured
output. And never hard-code `ExpectedExitCode: 1` for an unobserved branch: the only committed
evidence of this runner used the repository ROOT as search root and its assertion PASSED (exit 0), so
the single-assembly branch is unmeasured. Record the branch as an observation with a paired
consistency check; an artifact declaring `ExpectedExitCode: 1` that exits 0 normalises to fail.

**10. Cobertura package and class elements carry NO `lines-covered` and NO `lines-valid`.** The
string occurs exactly once per document, on the root `<coverage>` element. Any task demanding those
two figures at package or class level names a number with no source. **How to apply:** dot-source
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (which dot-sources the PackageRate file at its
line 3, so one dot-source resolves both) and call `Get-CoberturaPackageLineSummary` /
`Get-CoberturaClassLineSummary`, reading `LinesCovered`, `LinesValid`, `LineRate`.

**11. A relocated-vs-new rule stated as "same statement modulo indentation and the receiver name" is
an unbounded executor escape hatch**, and it also contradicts its own population (the population is
lines NOT in the anchor tree; the rule classifies by lines that ARE). **How to apply:** derive the
table from an anchored, rename-disabled, zero-context diff and classify a `+` line as relocated iff
its whitespace-stripped text equals that of some `-` line. No judgment clause.

**12. A projected repo-wide rate over a 65402-line denominator cannot fail.** Adding ~300 valid lines
to 56029/65402 = 0.856686 leaves the projection at 0.8528 in the worst case. **How to apply:** make
the DELTA rate (covered delta over valid delta, >= 0.80) the discriminating clause and keep the
projection as a recorded floor, stating in the artifact that it cannot discriminate.

**13. An absence-of-test proof must scope to FILES, not to a folder.** `EnqueueAsync` occurs 7 times
in the QuickFiler test Controllers folder (two QfcHomeController iteration files) and 0 times across
the three QfcQueue test files. A folder-scoped `SearchScope:` with a required `SearchResult:` of zero
is unsatisfiable. Record both counts so the distinction is auditable.

**14. Reflecting-test counts are METHOD counts, not file counts.** "Three existing tests reflect on
`_moveMonitor`" was six `SetPrivateField` call sites: `QfcQueueCoverageExpansionTests.cs` 119/145/207
and `QfcQueuePurePathsTests.cs` 126/176/244.

**15. `git add -A -N` inside a diff command is inert AND harmful.** A `git diff --name-status A..B`
is a commit-to-commit comparison an intent-to-add cannot affect, while the intent-to-add stages every
pre-existing residual and every agent-memory file, so a later stage-everything commit sweeps them
onto the branch. Drop the staging span; keep the porcelain span for untracked visibility.

**16. A scope-lock rule must carry an ANCHOR carve-out.** A promotion-lifecycle worktree is routinely
dirty at the anchor (a deleted potential entry plus untracked ones), so every commit gate fails on
arrival. Have P0-T2 record the porcelain output verbatim under a `PreExistingWorktreePaths:` line and
admit exactly that set by rule.

**17. `.csharpierignore` here excludes only evidence, cobertura/coverage/trx and `*.csproj`/`*.props`
/`*.targets`.** CSharpier 1.2.6 also processes `*.xml` and `packages.config`, so a repo-wide format
CAN rewrite out-of-Write-Set files. A plan cannot simultaneously require "every new path satisfies
the scope lock" and "any rewritten out-of-set path was already on the drift baseline" without an
explicit admit-and-record line; and the admission must be carried into the final AC rather than
waived.

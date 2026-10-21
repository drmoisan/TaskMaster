# Feature Audit — Issue #752

- Timestamp: 2026-09-03T12-23
- Work mode: `full-bug` (`issue.md` line 12) — AC source is `spec.md` `## Acceptance Criteria` only (6 items)
- Branch: `bug/coverage-assembly-discovery-excludes-own-worktree-root-752`
- Head: `80d07a1c26122a5cede04edc5833c964d663d8b7`
- Baseline: merge base with `origin/main`, `87233f867ad60c0a5c0d19b09cc121ae536d7ba1`
- `issue.md` supplies repro and background and is not an AC source in this work mode.

Blocking findings in this artifact: **0**.

Every criterion below was re-verified against the diff, the committed evidence, and the working
tree rather than accepted from the executor's check-off. All six were already marked `[x]` in
`spec.md`; this reviewer's independent evaluation confirms all six as PASS, so no checkbox required
a change and none was reverted.

## Acceptance criteria evaluation

### AC1 — PASS

> `scripts/vscode/Invoke-MSTestWithCoverage.ps1`'s assembly-discovery predicate excludes a candidate
> path based on that candidate's path relative to `$resolvedSearchRoot`, not the candidate's
> absolute `FullName`.

- Verified directly on disk: `scripts/vscode/Invoke-MSTestWithCoverage.ps1:301` reads
  `([System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)) -notmatch '(^|\\)\.claude\\'`.
  The match target is the `GetRelativePath` result; `$_.FullName` appears only as that call's second
  argument.
- `$resolvedSearchRoot` is assigned at line 272 and reaches the `Where-Object` block by closure,
  the same way `$Configuration` already does at line 298.
- Diff shape confirms it is the only production change:
  `git -C <repo-root> diff --numstat 87233f86..HEAD` reports `1  1  scripts/vscode/Invoke-MSTestWithCoverage.ps1`.
- Supporting evidence: `evidence/other/fix-diffstat.2026-09-03T07-23.md` (1 file, 1 insertion,
  1 deletion), `evidence/other/predicate-line-shape.2026-09-03T07-23.md`
  (`LINECOUNT=350`, `INDENT=16`, `TRIMMED=` character-identical to the line on disk).

### AC2 — PASS

> Running the coverage wrapper from a checkout whose search root is located under
> `.claude\worktrees\agent-<id>\` discovers the test assemblies built directly beneath that root,
> and does not throw "No test assemblies found ... Build first." when assemblies exist there.

- Fail-before: `evidence/regression-testing/pre-fix-new-suite.2026-09-03T07-23.md` records
  `PESTER Passed=1 Failed=2 Total=3` with `EXIT_CODE: 1` and `ExpectedExitCode: 1`, and the
  self-exclusion case listed as `Result=Failed`. The failure-mode proof in the same artifact
  captures exactly one message,
  `FAILMSG No test assemblies found under 'C:\repo\.claude\worktrees\agent-7\.' for configuration 'Debug'. Build first.`,
  which is the string thrown at `Invoke-MSTestWithCoverage.ps1:306`. That ties the failure to the
  reported defect rather than to a harness error.
- Pass-after: `evidence/regression-testing/post-fix-new-suite.2026-09-03T07-23.md` records
  `PESTER Passed=3 Failed=0 Total=3` from the identical command.
- Reviewer's own derivation, not taken from the artifacts: with root `C:\repo\.claude\worktrees\agent-7\.`
  and candidate `...\agent-7\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, `GetRelativePath`
  returns `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, which the anchored pattern does not match,
  so the candidate survives and the throw at line 306 cannot fire.
- Note on RED-first provenance: the fix and the new test file landed in a single commit
  (`eea3bb9b`), so commit ordering does not itself demonstrate fail-before. The fail-before claim
  rests on the recorded pre-fix run artifact plus its failure-message proof, which is the accepted
  equivalent form. The claim is internally consistent: the pre-fix run reports the third case
  failing too, and only the pre-fix predicate can produce that combination.

### AC3 — PASS

> A sibling agent worktree nested beneath the search root ... remains excluded from discovery,
> preserving the existing regression test `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:416-442`
> unmodified and passing.

- Unmodified, verified two ways by this reviewer:
  (a) `git -C <repo-root> diff --numstat 87233f86..HEAD` does not list the file at all;
  (b) `git -C <repo-root> diff --stat HEAD -- tests/scripts/vscode` returns empty, so it is not
  modified in the working tree either.
- Executor's corroborating record: `evidence/qa-gates/runsettings-tests-unmodified.2026-09-03T07-23.md`
  reports blob `4b168b07967b692fdb0574aefd7a5734dfeb0d9c`, equal to the
  `PHASE0 HEAD BLOB HASH` in `evidence/baseline/runsettings-tests-blob-hash.2026-09-03T07-23.md`,
  with an empty porcelain output.
- Passing: `evidence/regression-testing/preserved-original-test.2026-09-03T07-23.md` records
  `PESTER Passed=1 Failed=0 NotRun=26 Total=27` and
  `TEST Result=Passed Name=excludes assemblies discovered under a .claude worktree segment`.
- Content read at `Invoke-MSTest.RunSettings.Tests.ps1:416-442` confirms the case asserts that only
  `C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` survives when a nested
  `.claude\worktrees\agent-1` copy is also present, which is the behaviour the criterion names.
- Why this criterion is non-trivial here: the anchor `(^|\\)` exists precisely so this test keeps
  passing. Without it the relative path `.claude\worktrees\agent-1\...` would not match and the
  nested worktree would be retained. `evidence/regression-testing/getrelativepath-probe.2026-09-03T07-23.md`
  measures that directly (`OLD_REGEX_MATCH=False` on the nested case).

### AC4 — PASS

> A new Pester regression test file covers both the self-exclusion fix (AC2) and the continued
> nested-sibling exclusion (AC3), using in-memory fixtures only (no temporary files), consistent
> with the repository's unit-test policy.

- File exists at `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`,
  99 lines, added by this branch.
- Covers AC2 at line 44 (`includes an assembly directly beneath a search root that is itself under a .claude worktree segment`)
  and AC3 at line 59 (`excludes a nested sibling worktree beneath a non-dot-claude search root`); a
  third case at line 74 covers the double-nested edge where both behaviours must hold for one root.
- In-memory fixtures only, verified by reading the whole file: every `Get-ChildItem` mock returns
  literal `[pscustomobject]` records; there is no `New-Item`, no `Out-File`, no `Set-Content` target
  other than the mocked no-op, and none of the temp-path patterns the
  `check-powershell-test-purity.ps1` gate forbids at lines 105-109.
- Unit-test policy conformance is itemised in `policy-audit.2026-09-03T12-23.md` sections 1 and 4;
  all rows PASS.
- All three cases pass: `evidence/regression-testing/post-fix-new-suite.2026-09-03T07-23.md`.

### AC5 — PASS

> No other file in the repository is found to carry the same absolute-path-vs-`.claude` discovery
> defect within the scope of this fix ...

- The executor's sweep (`evidence/qa-gates/sibling-defect-sweep.2026-09-03T07-23.md`) is scoped to
  `scripts/` and returns five lines, classifying one as an exclusion predicate and four as
  inclusion roots or documentation prose.
- This reviewer re-derived the claim independently and over a **wider** scope than the plan used: a
  regular-expression search across every `*.ps1`, `*.psm1`, and `*.psd1` file in the repository for
  a comparison operator (`-match`, `-notmatch`, `-like`, `-notlike`, `-ne`) followed by a quoted
  literal containing `.claude` returns exactly one hit — `scripts/vscode/Invoke-MSTestWithCoverage.ps1:301`,
  the fixed line itself. A second search for the doubled-backslash `.claude` regex shapes returns
  the same single hit.
- The four non-predicate hits in the executor's sweep were confirmed by direct reading:
  `scripts/vscode/Invoke-MSTest.ps1:142` is prose inside a comment naming a rules file, and
  `scripts/bash/shell_qc_lib.sh` lines 76, 85, 335 add `.claude/lib/bash` to a discovery or coverage
  **include** set for a different tool.
- Conclusion: the criterion holds, and holds more broadly than the plan asserted.

### AC6 — PASS

> Full PowerShell toolchain (PoshQC format -> PoshQC analyze -> Pester test) passes in a single
> clean pass after the change, with no unrelated file modified.

Toolchain, single clean pass:

- Format: `evidence/qa-gates/poshqc-format.iter1.2026-09-03T07-23.md` — `ok: true`, and the SHA-256
  of both Write Set files is identical pre- and post-run, so `WRITE SET REWRITTEN BY FORMATTER: NONE`
  and `RESTORED PATHS: NONE`. No restart was triggered.
- Analyze: `evidence/qa-gates/poshqc-analyze.iter1.2026-09-03T07-23.md` — 16 issues against a
  16-issue baseline, with `ExpectedExitCode: 1` declared and justified by pre-existing unsuppressed
  Warnings. The rule-level comparison in `evidence/qa-gates/pssa-diagnostic-set.iter1.2026-09-03T07-23.md`
  records `NEW DIAGNOSTICS: NONE` with a line-for-line identical 16-item set; neither changed file
  contributes a diagnostic.
- Test: `evidence/qa-gates/poshqc-test.iter1.2026-09-03T07-23.md` — `MCP RESULT OK: true`;
  `evidence/qa-gates/final-clean-pass.2026-09-03T07-23.md` — 95 passed / 0 failed / 0 skipped, and
  `CLEAN PASS ITERATION: 1`. The 95 reconciles exactly with the 92-test baseline plus the three new
  cases.
- Only `.iter1` artifacts exist; no `.iter2` file is present in the diff, consistent with a
  first-pass clean loop.

No unrelated file modified:

- Reviewer's own enumeration from `git -C <repo-root> diff --numstat 87233f86..HEAD`: 39 paths, of
  which two are the source files named in the Write Set, 36 are feature-folder documents and
  evidence, and one is `docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md`,
  added by the pre-plan promotion commit `5375bcc9` and allow-listed with that rationale at
  `plan.2026-09-03T07-23.md` lines 251-253.
- `git -C <repo-root> diff --stat HEAD` over the whole tree returns empty, so nothing was left
  uncommitted.
- Corroborating record: `evidence/qa-gates/changed-file-audit.2026-09-03T07-23.md`.

Reservation recorded, not affecting the verdict: the stage *ordering* asserted by the artifacts'
`Timestamp:` fields is not reliable, because nine of those fields postdate the commit that contains
them (see `policy-audit.2026-09-03T12-23.md`, POL-3). The ordering claim is instead carried here by
an order-independent fact — the format stage left both Write Set files byte-identical, so no
sequence of the three stages could have read different content — and by the two committed
post-change JaCoCo XMLs, which both instrument the fixed line and therefore cannot predate the fix.
The criterion's substance is satisfied on evidence that does not depend on the timestamps.

## Relationship to the baseline

The baseline is the merge base `87233f86`, at which `scripts/vscode` and `tests/scripts/vscode` are
byte-identical to this branch's starting commit (`evidence/baseline/pre-change-tree-state.2026-09-03T07-23.md`
records an empty `diff --stat` over both directories). Behaviour delta versus that baseline:

| Behaviour | Baseline | Head |
|---|---|---|
| Discovery from a checkout under `.claude/worktrees/` | empty set, throws "No test assemblies found ... Build first." | assemblies beneath the root are discovered |
| Discovery of a sibling worktree nested under the search root | excluded | excluded (unchanged) |
| Discovery from the main checkout, no nested worktree | unchanged | unchanged |
| Test population over `tests/scripts/vscode` | 92 | 95 |
| Line coverage over `scripts/vscode` | 78.3042394014963 percent | 78.3312577833126 percent |
| Coverage state of the changed line | no per-line counter emitted | `<line nr="301" mi="0" ci="1" ...>` present in both post-change JaCoCo artifacts |

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: none

No checkbox was newly checked by this review (all six were already `[x]` and all six were confirmed
PASS on independent evaluation) and none was reverted.

## Verdict

**PASS** on all six acceptance criteria. Zero blocking findings in this artifact. One blocking
finding exists elsewhere in this review set — `POL-2` in `policy-audit.2026-09-03T12-23.md`, a
repository-hygiene violation in a file inside the branch diff — which is unrelated to the delivered
behaviour but must be cleared before merge.

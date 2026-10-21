# Code Review - Issue #961 (re-run after remediation Phase 3)

- Timestamp: 2026-10-02T06-18
- Base: 94287369908cc920b21b0e3256314f988ad7d2f5; Head: 03f4b37c110222149e8a3f12300de22b60594d4d
- Files reviewed: `scripts/hygiene/Test-RepositoryHygiene.ps1`, `scripts/hygiene/Test-RepositoryHygiene.Rules.ps1`, `tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1`, `tests/scripts/hygiene/Test-RepositoryHygiene.Rules.Tests.ps1`, `.gitignore`, `.github/workflows/README.md`, three deleted `.bak` files. Supersedes `code-review.2026-10-02T05-30.md` for the final tree.

## Executive Summary

The three prior low findings are resolved. CR-1 is closed: the lookalike test is a single `-ForEach` `It` over five one-per-line hashtables, with assertions on lines, count and exit code, each carrying a `-Because` that interpolates the case name. CR-2 is closed: every `Should` assertion in both modified test files carries `-Because`. CR-3 is closed: `.gitignore` has one `*.bak` line in place of the two specific lines, and AC-2 still holds with discriminating controls. Production code is unchanged since the first review (diff from b00fd1f19 to HEAD touches only `.gitignore` and `Test-RepositoryHygiene.Tests.ps1`). No blocking finding and no new finding in touched files. Two Info items remain.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | .claude/agent-memory/orchestrator/ | `MEMORY.md`, `poshqc-gates-observed-outputs-for-scripts-hygiene.md` | CR-4 (carried, RELATED): two orchestrator memory files are in the branch diff outside the declared footprint. | Orchestrator confirms they belong in this pull request; no change required from this item. | Governance-directory content, excluded by the guard, no host paths. | `git diff --name-status origin/main...HEAD -- .claude` |
| Info | tests/scripts/hygiene/Test-RepositoryHygiene.Git.Tests.ps1 | 7 `Should` lines | CR-6 (UNRELATED): 8 of 15 assertions carry `-Because`; the file is not in the branch diff and the gap pre-dates this branch. | Not remediated here; report for filing only if the maintainer wants uniformity. | The defect is in an untouched file with a different subject (the Git adapter); it is not caused by and does not affect this change. | Grep counts: 15 `Should -` lines, 8 with `-Because` |

Resolved from the prior review: CR-1 (Low), CR-2 (Low), CR-3 (Low). CR-5 (Info, bare `.bak` file name is a backup) remains accepted: it matches the stated rule and has a covering test.

## Verification of the Specific Remediation Items

### CR-1 - lookalike test split

- `Test-RepositoryHygiene.Tests.ps1` lines 171 to 186: `It 'reports zero findings for the backup-lookalike name <Name>' -ForEach @( ... )` with five hashtables on separate lines: `docs/bak/notes.md`, `notes.bakery`, `notes.bak.md`, `backup`, `docs/features/x/Makefile`.
- Three assertions per case: the exact `@($result.Lines) | Should -Be @('HYGIENE Findings=0')`, `FindingCount` 0, `ExitCode` 0. Each `-Because` is a double-quoted string that interpolates `$Name`, so a failure names the case.
- The five names match the five negative names covered at predicate level in the Rules tests; the old title no longer exists.
- Execution: P3-T21 junit shows `tests="49"`, five passed cases with expanded names (no unexpanded `<Name>` text), zero failures.
- Pester detail checked: `$Name` is resolved by `-ForEach` data in the `It` body; `BeforeEach` resets `$script:Content` and `$script:Listing` before each case, so cases are independent.

### CR-2 - `-Because` coverage

- Reviewer counts: `Should -` lines equal `Should -.*-Because` lines in `Test-RepositoryHygiene.Tests.ps1` (29, 29) and `Test-RepositoryHygiene.Rules.Tests.ps1` (31, 31).
- The four orchestration tests and the related siblings in that file carry reasons that state the expected behavior in terms of the rule, for example the governance test: the backup record is dropped, so only the zero-findings total is printed.
- The Rules test file is unchanged by Phase 3 and already complete.

### CR-3 - `.gitignore`

- Lines 254 to 257 now read `UpgradeLog*.XML`, `UpgradeLog*.htm`, `ServiceFabricBackup/`, `*.bak`. No `*.rptproj.bak` or `*.csproj.bak` remains.
- The line is one `*.bak` rule, so `*.sln.bak`, `*.vbproj.bak`, `*.csproj.bak` and `*.rptproj.bak` all remain covered; P3-T14 shows all three sampled names attributed to `.gitignore:257:*.bak`.
- Negative control present and discriminating: P3-T9 and P3-T10 (rule absent, `foo.bak` unmatched, exit 1); P3-T16 (`README.md` not ignored, exit 1); P3-T12 (restoration byte-identical by hash). Reviewer re-run of `git check-ignore -q TaskMaster.sln.bak` exits 0.
- Removal is safe for the redundancy claim: the two specific patterns are strict subsets of `*.bak`.

## Correctness Analysis

- Predicate and orchestration code are unchanged. `Test-BackupFilePath` uses `[System.IO.Path]::GetExtension($RelativePath) -ieq '.bak'`; the finding line is emitted after the `.claude/` prefix skip and before the content scan.
- Reviewer guard run on the final tree: `HYGIENE Findings=0`, exit 0.
- Test determinism: no clock, random, file system or process use in added or changed tests; the only seam is the mocked `Invoke-GitExe` with signature parity.

## Design and Style

- File sizes read from disk: 93, 146, 209, 288 lines; all under 500.
- Arrange-Act-Assert separation holds in the `-ForEach` test.
- Tonality: comments, `-Because` text and artifacts are factual; no humor, hyperbole or metaphor.
- No suppressions and no dependencies added.

## Documentation Review

The `_hygiene.yml` README row names the backup rule, the case-insensitive final-extension condition and the `HYGIENE backup-file <path>` line, and it is unchanged by Phase 3. Plan version 1.4 records the three statements Phase 3 supersedes (the `-ForEach` statement, the specific-lines decision, the 45-test count), so the plan does not contradict the final tree.

## Follow-ups

None required from this item. The only open item is the pull-request-time CI figure: the `_pester.yml` LINE figure for the current head is pending and is read by the orchestrator. The earlier head measured `scripts/hygiene` at 94.23 percent and production lines are unchanged since.

## Acceptance Criteria Inventory

AC source: `## Acceptance Criteria` in `issue.md` (work mode `minor-audit`): AC-1 to AC-7.

## Acceptance Criteria Evaluation

All seven criteria verified against the diff and evidence. See `feature-audit.2026-10-02T06-18.md`.

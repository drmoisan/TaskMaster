# Feature Audit - Issue #961 (re-run after remediation Phase 3)

- Timestamp: 2026-10-02T06-18
- Work Mode: minor-audit; AC source: `## Acceptance Criteria` in `issue.md` only
- Base: 94287369908cc920b21b0e3256314f988ad7d2f5; Head: 03f4b37c110222149e8a3f12300de22b60594d4d
- Supersedes for the final tree: `feature-audit.2026-10-02T05-30.md` (not overwritten)

## Scope and Baseline

- Audit scope is the full branch diff against base 94287369908cc920b21b0e3256314f988ad7d2f5 (`git merge-base origin/main HEAD`; equal to `origin/main`, so the three-dot and two-dot diffs agree).
- Required footprint: deletions of `TaskMaster.sln.bak`, `TaskTree/TaskTree.vbproj.bak`, `TaskVisualization/TaskVisualization.vbproj.bak`; modifications of `.gitignore`, `.github/workflows/README.md`, the two hygiene scripts and the two hygiene test files; feature-folder documents and the promoted record.
- Observed footprint (`git diff --numstat origin/main...HEAD`): exactly those nine paths, the feature folder, the promoted record `docs/features/potential/promoted/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule.md`, and two orchestrator memory files under `.claude/agent-memory/orchestrator/` (extra, non-blocking, carried from the first cycle). Phase 3 added no path to the declared footprint (P3-T24; reviewer `git diff --stat b00fd1f19..HEAD` over `scripts tests .gitignore .github` lists only `.gitignore` and `Test-RepositoryHygiene.Tests.ps1`).
- Baseline: 31 tests in `tests/scripts/hygiene`; guard output `HYGIENE Findings=0` with three tracked `.bak` files present (the rule did not exist).
- Working tree at review: `git status --short` is empty; `git ls-files -- "*.bak"` prints nothing.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/issue.md`, section `## Acceptance Criteria` (7 items, all `- [x]`).

- AC-1: No tracked backup file remains; three named files deleted from index and worktree.
- AC-2: `.gitignore` carries a `*.bak` rule so `git check-ignore -q TaskMaster.sln.bak` exits 0.
- AC-3: Guard reports `HYGIENE backup-file <path>` for every tracked `.bak` path (case-insensitive), counts each as a finding, exits 1; `.claude/` stays excluded.
- AC-4: Pester tests prove the rule: negative control and positive cases for lookalikes and a clean listing.
- AC-5: Guard on the final tree prints `HYGIENE Findings=0` and exits 0.
- AC-6: PoshQC format, analyze and test pass; junit `errors="0" failures="0"` with a count above 31; line coverage of `scripts/hygiene` measured by CI; locally every added statement exercised by a named test.
- AC-7: `.github/workflows/README.md` describes the backup-file rule in the `_hygiene.yml` row.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence verified by this review |
|---|---|---|
| AC-1 | PASS | Diff status `D` for all three files; reviewer `git ls-files -- "*.bak"` prints nothing; worktree clean. Prior evidence P0-T5 (positive control) and P2-T6, P2-T7 stands; Phase 3 did not touch these paths. |
| AC-2 | PASS | After CR-3, `.gitignore` line 257 is `*.bak`, the only `.bak` rule (reviewer read lines 250 to 261). Reviewer `git check-ignore -q TaskMaster.sln.bak` exits 0. P3-T14: `foo.csproj.bak`, `foo.rptproj.bak`, `foo.bak` all attributed to `.gitignore:257:*.bak`. Negative controls: P3-T16 `README.md` exits 1; P3-T9 and P3-T10 with `*.bak` temporarily removed `foo.bak` matches no rule and exits 1, so the pass is attributable to `*.bak`; P3-T12 restored the file byte-identically. The earlier P2-T9 and P2-T11 citations of `.gitignore:259` are superseded by the P3 evidence, as plan 1.4 records. |
| AC-3 | PASS | `Invoke-RepositoryHygieneMain` emits `'HYGIENE backup-file ' + $record.Path` when `Test-BackupFilePath` is true, after the `.claude/` skip; `-ieq` gives case-insensitivity; the finding is counted by the existing logic so `Findings=<n>` and exit 1 follow. Production unchanged in Phase 3. Real-tree control (P1-T9): three lines, `HYGIENE Findings=3`, exit 1. |
| AC-4 | PASS | Negative control test asserts the exact two lines and exit 1. Positive cases: five lookalike names now run as five `-ForEach` cases, each asserting `HYGIENE Findings=0`, count 0 and exit 0; the existing clean-listing test covers a clean listing; ten predicate tests cover case, depth, directory segment, `.bakery`, `.bak.md`. RED run (P1-T4): 12 expected failures. Final: P3-T21 tests=49, failures=0. |
| AC-5 | PASS | Reviewer ran the guard under `pwsh` from the worktree root: `HYGIENE Findings=0`, exit 0. Matches P3-T23. |
| AC-6 | PASS (CI figure for the current head pending) | PoshQC format: six hashes identical (P3-T19). PoshQC analyze: ok result; tool reports no finding count (P3-T20). PoshQC test: tests=49 (above 31), errors=0, failures=0 (P3-T21). Added statements 3 of 3 exercised. CI measurement: run 36990562362 at head b00fd1f19 reported `scripts/hygiene` 98 of 104 lines (94.23 percent) and `COVERAGE LinePercent=94.64`; production scripts are byte-unchanged since that head, and the Phase 3 test edit retains every case. The CI run for head 03f4b37c1 is pending and is read by the orchestrator. |
| AC-7 | PASS | README `_hygiene.yml` row names the backup-file rule, the final-extension `.bak` case-insensitive condition and the `HYGIENE backup-file <path>` line; unchanged by Phase 3. |

## Acceptance Criteria Check-off

- Source file: `docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/issue.md`.
- Newly checked off by this review: none. The 7 items were already checked; each was independently verified above and no checkbox is contradicted by the evidence.
- Unchecked: none.
- `issue.md` was not edited by this review.

## Remediation Verification Summary

| Item | Status | Basis |
|---|---|---|
| CR-1 lookalike test split into `-ForEach`, five cases, each with `-Because` | Closed | Tests.ps1 lines 171 to 186; P3-T21 five expanded passed names |
| CR-2 `-Because` on array-equality assertions and related siblings | Closed | 29 of 29 and 31 of 31 assertions carry `-Because` |
| CR-3 `.gitignore` reduced to `*.bak`; AC-2 intact; negative-control evidence | Closed | `.gitignore` line 257; P3-T9 to P3-T16 |

## Summary

Verdict: PASS. No blocking findings. Remaining findings: CR-4 Info, RELATED (two orchestrator memory files outside the declared footprint, orchestrator to confirm); CR-6 Info, UNRELATED (7 assertions without `-Because` in `Test-RepositoryHygiene.Git.Tests.ps1`, a file this branch does not touch).

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/issue.md`
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

### Pending at pull-request time

1. CI Pester LINE figure for `scripts/hygiene` at head 03f4b37c1 (`_pester.yml`, `pester-coverage` artifact) is pending; the orchestrator reads the CI run. A changed file below the 85 percent line floor would turn the conditional PowerShell coverage verdict into FAIL. Expected outcome is unchanged from 94.23 percent because no production line changed.
2. The ruleset update that would make the hygiene check required remains an operator action tracked by #927.

No remediation is required, so no `remediation-inputs` artifact is produced.

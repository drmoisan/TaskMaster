# Feature Audit - Issue #961 (remaining tracked backup files and hygiene guard rule C)

- Timestamp: 2026-10-02T05-30
- Work Mode: minor-audit; AC source: `## Acceptance Criteria` in `issue.md` only
- Base: 94287369908cc920b21b0e3256314f988ad7d2f5; Head: f93005d38288020a2b0ebcfd2a51c1d713f7a357

## Scope and Baseline

- Audit scope is the full branch diff against base 94287369908cc920b21b0e3256314f988ad7d2f5 (`git merge-base origin/main HEAD` recorded by P2-T14).
- Required footprint: deletions of `TaskMaster.sln.bak`, `TaskTree/TaskTree.vbproj.bak`, `TaskVisualization/TaskVisualization.vbproj.bak`; modifications of `.gitignore`, `.github/workflows/README.md`, `scripts/hygiene/Test-RepositoryHygiene.ps1`, `scripts/hygiene/Test-RepositoryHygiene.Rules.ps1`, `tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1`, `tests/scripts/hygiene/Test-RepositoryHygiene.Rules.Tests.ps1`; feature-folder documents and the promoted record.
- Observed footprint (reviewer `git diff --name-status base HEAD`): exactly the three deletions and six modifications above, plus the feature folder, the promoted record `docs/features/potential/promoted/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule.md`, and two orchestrator memory files under `.claude/agent-memory/orchestrator/` (extra, non-blocking; see code review CR-4).
- Baseline: 31 tests in `tests/scripts/hygiene`, guard output `HYGIENE Findings=0` with three tracked `.bak` files present (the rule did not exist).
- Working tree at review: `git status --short` is empty; `git ls-files -- "*.bak"` prints nothing.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/issue.md`, section `## Acceptance Criteria`.

- AC-1: No tracked backup file remains; the three named files are deleted from the index and the worktree.
- AC-2: `.gitignore` carries a `*.bak` rule so `git check-ignore -q TaskMaster.sln.bak` exits 0.
- AC-3: The guard reports `HYGIENE backup-file <path>` for every tracked `.bak` path (case-insensitive), counts each as a finding, exits 1; `.claude/` stays excluded.
- AC-4: Pester tests prove the rule: negative control with finding line and exit 1, positive cases with zero findings for lookalikes and a clean listing.
- AC-5: The guard on the final tree prints `HYGIENE Findings=0` and exits 0.
- AC-6: PoshQC format, analyze and test pass; junit `errors="0" failures="0"` with a count above 31; line coverage of `scripts/hygiene` measured by CI; locally every added statement is exercised by a named test.
- AC-7: `.github/workflows/README.md` describes the backup-file rule in the `_hygiene.yml` row.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence verified by this review |
|---|---|---|
| AC-1 | PASS | Diff status `D` for all three files; reviewer `git ls-files -- "*.bak"` printed nothing; reviewer search for `sln.bak` and `vbproj.bak` outside `docs/`, `.claude/` and `artifacts/` found no reader. Evidence: P0-T5 (positive control printed three paths), P2-T6, P2-T7 (reads return file-not-found, glob finds none). |
| AC-2 | PASS | `.gitignore` diff adds `*.bak` at line 259 (P2-T11 exact-line grep). `git check-ignore -q TaskMaster.sln.bak` exit 0 (P2-T8); `-v` output attributes each path to `.gitignore:259:*.bak` (P2-T9); negative control `README.md` exits 1 (P2-T10). The same query with `--no-index` printed nothing at baseline (P0-T10), so the pass is attributable to the new line. |
| AC-3 | PASS | `Invoke-RepositoryHygieneMain` emits `'HYGIENE backup-file ' + $record.Path` when `Test-BackupFilePath` is true, adds it to the finding list (so `Findings=<n>` and exit 1 follow from the existing count logic), and runs after the `.claude/` prefix skip. Case-insensitivity via `-ieq`. Real-tree control (P1-T9): three `backup-file` lines, `HYGIENE Findings=3`, exit 1. |
| AC-4 | PASS | Negative control test `reports a tracked backup file as a finding and fails the guard` asserts the exact two lines `HYGIENE backup-file TaskMaster.sln.bak` and `HYGIENE Findings=1` and `ExitCode` 1. Positive cases: lookalike test asserts `HYGIENE Findings=0`, count 0, exit 0 for five names; the existing clean-listing test covers a clean listing (P1-T8 counted its pass). Ten unit tests cover the predicate including case, depth, directory segment, `.bakery`, `.bak.md`. RED-first run (P1-T4): 12 expected failures; GREEN (P2-T3): 0 failures. |
| AC-5 | PASS | Reviewer ran the guard under `pwsh` from the worktree root: output `HYGIENE Findings=0`, exit 0. Matches P2-T17. |
| AC-6 | PASS (CI figure pending) | PoshQC format unchanged (six hashes identical, P2-T1); PoshQC analyze: pass (0 findings); tool reports no count (P2-T2); PoshQC test tests=45 (above 31), errors=0, failures=0 (P2-T3). Statement-to-test map shows 3 of 3 added statements exercised (P2-T4; re-derived from the diff). The `scripts/hygiene` LINE figure is, by the wording of this criterion, measured by CI (`_pester.yml`, LINE asserted at 80); it is not available locally (P0-T16: zero `scripts/hygiene` entries in the PoshQC document) and remains a pull-request-time gate. |
| AC-7 | PASS | README `_hygiene.yml` row (line 24) now names the backup-file rule, the final-extension `.bak` case-insensitive condition and the `HYGIENE backup-file <path>` finding line; one grep hit (P2-T12). Wording matches the implementation. |

## Acceptance Criteria Check-off

- Source file: `docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/issue.md`.
- Newly checked off by this review: none. The executor had already checked AC-1 to AC-7; this review independently verified each against the diff and evidence and found no checkbox that the evidence contradicts.
- Unchecked: none.
- Note on AC-6: the checkbox is supported by every locally verifiable component. The CI-measured coverage component is pending and is carried as a follow-up rather than a blocker.

## Summary

Verdict: PASS. No blocking findings.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/issue.md`
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

### Residual items and follow-ups (not filed)

1. CI-sourced Pester LINE figure for `scripts/hygiene` (`_pester.yml`, `pester-coverage` artifact) is pending; read it after the push. A figure below the floor would turn the conditional PowerShell coverage verdict into FAIL.
2. Two orchestrator memory files under `.claude/agent-memory/orchestrator/` are in the branch diff outside the stated footprint; the orchestrator should confirm they belong in this pull request.
3. Optional test polish (low): add `-Because` to the array-equality assertions and split the five-name lookalike test into `-ForEach` cases (code review CR-1, CR-2).
4. After merge, the ruleset update that would make the hygiene check required remains an operator action tracked by #927; this change does not alter it.

No remediation is required, so no `remediation-inputs` artifact is produced.

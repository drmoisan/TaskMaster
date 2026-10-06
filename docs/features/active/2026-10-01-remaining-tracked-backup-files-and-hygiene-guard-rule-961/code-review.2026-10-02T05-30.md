# Code Review - Issue #961 (hygiene guard rule C and backup-file removal)

- Timestamp: 2026-10-02T05-30
- Base: 94287369908cc920b21b0e3256314f988ad7d2f5; Head: f93005d38288020a2b0ebcfd2a51c1d713f7a357
- Files reviewed: `scripts/hygiene/Test-RepositoryHygiene.ps1`, `scripts/hygiene/Test-RepositoryHygiene.Rules.ps1`, `tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1`, `tests/scripts/hygiene/Test-RepositoryHygiene.Rules.Tests.ps1`, `.gitignore`, `.github/workflows/README.md`, three deleted `.bak` files.

## Executive Summary

The change is small, cohesive and correct. `Test-BackupFilePath` compares the final extension of the path with `.bak` using `[System.IO.Path]::GetExtension(...) -ieq '.bak'`, which matches the requirement (case-insensitive, final extension only, directory segments and longer extensions excluded). The orchestrator adds the finding line before content scanning, so a backup file is still scanned for profile paths, and the governance skip still runs first. Output format and exit decision are unchanged apart from the new line kind. No blocking defect was found. Three low-severity observations and two follow-ups are recorded below.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1 | `It 'reports zero findings for backup-lookalike names'` | CR-1: five lookalike names are asserted in one `It`, which departs from the one-behavior-per-`It` rule; a failure would not name the offending path. | Optional: convert to `-ForEach` cases, or accept as is because the rule tests in the sibling file already cover each name individually. | Each name is separately proven by `Test-BackupFilePath` tests 5 to 9, so the bundled case adds integration confidence only; diagnosis cost is small. | Added lines in the Tests.ps1 diff; Rules.Tests.ps1 tests 5 to 10 |
| Low | tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1 | Array-equality assertions in the four added tests | CR-2: `@($result.Lines) \| Should -Be @(...)` carries no `-Because`; in the lookalike and governance tests the `ExitCode` assertion is also without one. | Optional: add `-Because` to the Lines assertions for consistency with the rest of the file. | The expected arrays are self-describing and the neighbouring assertions carry reasons, so failure output is still actionable. | Tests.ps1 diff, lines 159 to 203 |
| Low | .gitignore | lines 257 to 259 | CR-3: `*.rptproj.bak` and `*.csproj.bak` are now redundant with `*.bak`. | Leave them; removal is unrelated churn and the narrower lines document the earlier #951 intent. | No functional effect. The ignore rule is also not a barrier to `git add -f`, which is why rule C exists. | `.gitignore` diff |
| Info | .claude/agent-memory/orchestrator/ | `MEMORY.md`, `poshqc-gates-observed-outputs-for-scripts-hygiene.md` | CR-4: two orchestrator memory files are part of the branch diff though not in the stated footprint. | Orchestrator to confirm they are intended for this pull request. | Governance-directory content is excluded by the guard and contains no host paths; scope creep is limited to agent memory. | `git diff --stat` for `.claude` |
| Info | scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 | `Test-BackupFilePath` | CR-5: a path whose final segment is `.bak` (for example `dir/.bak`) is reported as a backup file because `GetExtension` returns `.bak`. | Accept: it matches the stated rule and is covered by the bare dot-bak test. | A literal file named `.bak` is plausibly a backup artifact; no false-positive tracked file exists on the tree (guard run reports 0). | Rules.Tests.ps1 test 4; reviewer guard run |

## Correctness Analysis

- Case handling: `-ieq` makes `Notes.BAK` a match; test 3 asserts it.
- Directory segment: `docs/bak/notes.md` has final extension `.md`; test 5 asserts false.
- Longer extension and compound names: `notes.bakery` and `notes.bak.md` return false; tests 6 and 7.
- Ordering: the finding line is emitted before the content read, so an unreadable backup file yields both `backup-file` and `unreadable` lines and counts two findings, which is consistent with the existing one-finding-per-rule behavior. The existing rule ordering (raw-document, unreadable, profile-path) is preserved after the new line.
- Governance exclusion: `.claude/` records are dropped before the new `if`; the added test proves `.claude/agent-memory/notes.bak` yields zero findings.
- Cross-platform: the git listing uses forward slashes, and `GetExtension` treats both separators, so the `ubuntu-latest` job behaves the same as the local Windows run.
- Real-tree control: P1-T9 printed `HYGIENE backup-file` for the three files and `HYGIENE Findings=3` (exit 1) before deletion; P2-T17 and the reviewer re-run print `HYGIENE Findings=0` after deletion.

## Design and Style

- Single responsibility: predicate in the Rules file, orchestration in the main script; no new coupling.
- Documentation: comment-based help states synopsis, description, parameter and output; the header comment of the guard and the `.DESCRIPTION` were updated to name rule C.
- Naming follows the existing `Verb-Noun` and `HYGIENE <rule> <path>` conventions.
- File sizes: 93, 146, 204 and 288 lines; all below 500.
- No suppressions, no new dependencies, no mutable script-scope state in production code.

## Documentation Review

The `_hygiene.yml` README row now names the backup rule, the case-insensitive final-extension condition and the finding-line format, and keeps the output-restriction sentence accurate (rule name, path, and a line number for a profile path only). The wording matches the implemented behavior.

## Follow-ups (not filed)

1. Read the CI `_pester.yml` LINE figure and `pester-coverage` JaCoCo artifact for `scripts/hygiene` after the push and record it against the policy audit; a figure below 85% would convert the conditional coverage verdict to FAIL.
2. Optional hygiene: after the first green CI run, confirm the `hygiene / Repository hygiene guard` check reports `Findings=0` on the pull request head (the guard is wired in `ci.yml` and `_hygiene.yml` and was not changed).

## Acceptance Criteria Inventory

The AC source is the `## Acceptance Criteria` section of `issue.md` (work mode `minor-audit`): AC-1 to AC-7. Per-criterion evaluation is in the feature audit of the same timestamp.

## Acceptance Criteria Evaluation

All seven criteria were verified against the diff and evidence; AC-6 carries a CI-sourced coverage component that is pending. See `feature-audit.2026-10-02T05-30.md`.

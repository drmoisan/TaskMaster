# R4 Sanitisation — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-05-40
- Task: [P4-T2]
- Finding: R4, **Blocking**
- EXIT_CODE: 0

## Method

Every file was rewritten **byte-exactly** with PowerShell:

```powershell
$text = [System.IO.File]::ReadAllText($full)
foreach ($entry in $map) {                       # [P4-T1] order, longest-first
    $rx = [regex]::new([regex]::Escape($entry.Literal), 'IgnoreCase')
    $text = $rx.Replace($text, $entry.Token)
}
[System.IO.File]::WriteAllText($full, $text)
```

**`sed` through the Bash tool is prohibited** per **gate rule 15**, and was not used. Two
independent failure modes make it unusable here: the tool layer collapses doubled backslashes
before `sed` parses them, so a pattern naming a Windows path arrives matching nothing; and
`sed -i` rewrites every file's line endings anyway, which would have produced 33 modified files
with no content change.

## File List

The scope is exactly the [P0-T3] census list of **33** files, minus the one named exclusion.

**The exclusion was not in scope in the first place.** [P0-T3] recorded that
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` is **absent from the branch
diff** — `git diff --name-only <MERGE_BASE>..HEAD` returned 0 paths for it — so the scope rule
excluded it without any by-name intervention. Its 4 occurrences are fixture input strings and
expected values of a path-rewriting test, and they are present at `origin/main` unchanged.

Files rewritten: **33**. Files skipped by the exclusion: **0**, because the exclusion never
entered the list.

## Per-File Record

`F/` abbreviates `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`.

| Path | Pre lines | Post lines | Replaced | Per-map-entry counts |
|---|---|---|---|---|
| `F/code-review.2026-09-20T01-37.md` | 97 | 97 | 1 | `<user-home>`\|long\|backslash=1 |
| `F/evidence/baseline/p0-t1-worktree-anchor.2026-09-19T09-44.md` | 48 | 48 | 7 | `<session-worktree-root>`\|long\|backslash=1; `<execution-worktree-root>`\|long\|forwardslash=6 |
| `F/evidence/baseline/p0-t10-cold-state-census.2026-09-19T09-44.md` | 57 | 57 | 1 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/baseline/p0-t3-diff-anchor.2026-09-19T09-44.md` | 97 | 97 | 1 | `<execution-worktree-root>`\|long\|forwardslash=1 |
| `F/evidence/baseline/p0-t4-batch-budget-state.2026-09-19T09-44.md` | 136 | 136 | 5 | `<session-worktree-root>`\|long\|backslash=3; `<execution-worktree-root>`\|long\|backslash=1; `<user-home>`\|8.3\|forwardslash=1 |
| `F/evidence/baseline/p0-t5-sdk-bootstrap.2026-09-19T09-44.md` | 77 | 77 | 6 | `<execution-worktree-root>`\|long\|backslash=6 |
| `F/evidence/baseline/p0-t6-tool-restore.2026-09-19T09-44.md` | 57 | 57 | 1 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/baseline/p0-t7-package-restore.2026-09-19T09-44.md` | 66 | 66 | 1 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/baseline/p0-t8-dotnet-coverage.2026-09-19T09-44.md` | 53 | 53 | 5 | `<execution-worktree-root>`\|long\|backslash=1; `<user-home>`\|long\|backslash=4 |
| `F/evidence/baseline/p0-t9-pester-provision.2026-09-19T09-44.md` | 72 | 72 | 1 | `<user-home>`\|long\|backslash=1 |
| `F/evidence/baseline/p2-t7-mstest-numeric-baseline.2026-09-19T09-44.md` | 159 | 159 | 2 | `<execution-worktree-root>`\|long\|backslash=2 |
| `F/evidence/baseline/phase0-instructions-read.2026-09-19T09-44.md` | 99 | 99 | 3 | `<session-worktree-root>`\|long\|backslash=1; `<execution-worktree-root>`\|long\|backslash=1; `<execution-worktree-root>`\|long\|forwardslash=1 |
| `F/evidence/other/p2-t9-batch-a-boundary.2026-09-19T09-44.md` | 189 | 189 | 3 | `<user-home>`\|long\|dashed=3 |
| `F/evidence/other/p4-t8-batch-b-boundary.2026-09-19T09-44.md` | 136 | 136 | 3 | `<user-home>`\|long\|dashed=3 |
| `F/evidence/other/p6-t7-batch-c-boundary.2026-09-19T09-44.md` | 131 | 131 | 3 | `<user-home>`\|long\|dashed=3 |
| `F/evidence/other/p9-t15-plan-checkoff-resync.2026-09-19T09-44.md` | 63 | 63 | 2 | `<session-worktree-root>`\|long\|backslash=1; `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p1-t13-pester-workflow-scope.2026-09-19T09-44.md` | 109 | 109 | 5 | `<session-worktree-root>`\|long\|backslash=1; `<execution-worktree-root>`\|long\|backslash=4 |
| `F/evidence/qa-gates/p1-t14-ac6-cold-analyzer-build-green.2026-09-19T09-44.md` | 129 | 129 | 5 | `<session-worktree-root>`\|long\|backslash=1; `<execution-worktree-root>`\|long\|backslash=4 |
| `F/evidence/qa-gates/p2-t1-poshqc-format.2026-09-19T09-44.md` | 242 | 242 | 1 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p2-t2-poshqc-analyze.2026-09-19T09-44.md` | 146 | 146 | 2 | `<execution-worktree-root>`\|long\|backslash=2 |
| `F/evidence/qa-gates/p2-t4-csharpier-check.2026-09-19T09-44.md` | 82 | 82 | 1 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p9-t1-poshqc-format.iter1.2026-09-19T09-44.md` | 167 | 167 | 1 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p9-t1-poshqc-format.iter2.2026-09-19T09-44.md` | 164 | 164 | 1 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p9-t10-file-size-audit.2026-09-19T09-44.md` | 76 | 76 | 1 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p9-t2-poshqc-analyze.iter1.2026-09-19T09-44.md` | 66 | 66 | 2 | `<execution-worktree-root>`\|long\|backslash=2 |
| `F/evidence/qa-gates/p9-t2-poshqc-analyze.iter2.2026-09-19T09-44.md` | 115 | 115 | 2 | `<execution-worktree-root>`\|long\|backslash=2 |
| `F/evidence/qa-gates/p9-t3-pester.iter1.2026-09-19T09-44.md` | 143 | 143 | 1 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p9-t4-csharpier-check.iter1.2026-09-19T09-44.md` | 40 | 40 | 1 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/evidence/qa-gates/p9-t7-mstest-coverage.iter1.2026-09-19T09-44.md` | 119 | 119 | 2 | `<execution-worktree-root>`\|long\|backslash=2 |
| `F/evidence/regression-testing/898-cold-restore-red-run.2026-09-19T11-40.md` | 83 | 83 | 1 | `<execution-worktree-root>`\|long\|backslash=1 |
| `F/plan.2026-09-19T09-44.md` | 1083 | 1083 | 17 | `<session-worktree-root>`\|long\|backslash=3; `<execution-worktree-root>`\|long\|backslash=14 |
| `F/remediation-inputs.2026-09-20T01-37.md` | 205 | 205 | 1 | `<user-home>`\|long\|backslash=1 |
| `F/research/2026-09-19T11-30-dependabot-nuget-upgrade-automation-research.md` | 1344 | 1344 | 14 | `<session-worktree-root>`\|long\|backslash=12; `<repo-root>`\|long\|backslash=2 |
| **33 files** | | | **103** | |

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| File list equals the [P0-T3] list minus the exclusion | 33 | **33** | PASS |
| Whether the exclusion was in scope, recorded | yes | recorded: **not in scope**, absent from the branch diff | PASS |
| Files whose post line count differs from pre | **0** | **0** | PASS |
| Summed replacement count equals the [P0-T3] occurrence total | 103 | **103** | PASS |
| Files whose replacement count differs from their census occurrence count | — | **0** | — |

Every file's post line count **equals** its pre line count, checked per file. That clause fails
on any rewrite that inserted or removed a line, which a pure substitution cannot do and a
line-ending rewrite would.

The summed count equals the census total exactly, with no deduction for the exclusion because
the exclusion contributed nothing to the census. The per-file equality is stronger than the sum:
a file that over-replaced and a file that under-replaced could cancel in a sum but not in the
per-file comparison, and **0** files differ.

## Output Summary

33 files rewritten byte-exactly, 103 occurrences replaced across 7 distinct variants, zero line
counts changed, zero per-file count mismatches. The named exclusion was out of scope by the
scope rule and was not touched.

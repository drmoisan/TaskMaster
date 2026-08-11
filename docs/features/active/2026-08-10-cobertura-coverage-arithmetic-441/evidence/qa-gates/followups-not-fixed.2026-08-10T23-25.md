# Follow-Ups Not Fixed In This Change (P6-T6)

Timestamp: 2026-08-10T23-25

Confirms that none of the four follow-up candidates recorded in
`<FEATURE>/evidence/issue-updates/followups-441.2026-08-10T23-25.md` was fixed by this change.

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
git diff --name-only edf3d34c -- scripts tests
git diff --name-only edf3d34c -- scripts/vscode/Invoke-MSTestWithCoverage.ps1 scripts/temp-extract-coverage.ps1
git diff --name-only edf3d34c -- .claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md
```

EXIT_CODE: 0

Output Summary:

```
=== scope-lock re-check ===
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
=== follow-up files (Invoke-MSTestWithCoverage.ps1, temp-extract-coverage.ps1) ===
=== agent-memory file ===
=== end (empty = unchanged) ===
```

## Verdict

| Check | Required | Observed | Verdict |
| --- | --- | --- | --- |
| P4-T8 scope-lock output still lists exactly two source files | exactly 2 | exactly 2 (`Invoke-MSTestWithCoverage.Helpers.ps1`, `Invoke-MSTestWithCoverage.Helpers.Tests.ps1`) | **PASS** |
| `git diff --name-only edf3d34c -- scripts/vscode/Invoke-MSTestWithCoverage.ps1 scripts/temp-extract-coverage.ps1` | empty | **empty** | **PASS** |

## Per-candidate confirmation

| # | Candidate | File(s) that a fix would touch | Changed? |
| --- | --- | --- | --- |
| 1 | Package-level rates never recomputed | `Invoke-MSTestWithCoverage.Helpers.ps1` (`ConvertTo-KoverageCoberturaXml` root/merged-class attribute writes) and its consumer `scripts/temp-extract-coverage.ps1:47` | **not fixed** — `temp-extract-coverage.ps1` is unchanged, and the diff to `Helpers.ps1` is confined to four hunks (old lines 122-132, an insertion after 166, 270-273 and 275-276), none of which touches a `<package>` attribute. No `SetAttribute` on a package node exists anywhere in the file, before or after. |
| 2 | Merged class retains only the primary `<methods>` | `Merge-CoberturaClassesByFilename` `<methods>` handling | **not fixed** — the `<methods>` handling at old lines 202-206 is untouched (no hunk covers that range), and fixture **F6 actively pins** the existing behaviour: the merged class must keep exactly the primary's one `<method>` child. F6 passes. |
| 3 | `Invoke-MSTestWithCoverage.ps1` lacks a `\.claude\` discovery exclusion | `scripts/vscode/Invoke-MSTestWithCoverage.ps1:296-302` | **not fixed** — the file does not appear in the diff at all (empty output above; independently verified by P2-T6). |
| 4 | Agent memory records an incorrect generalization | `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md` | **not fixed** — the file does not appear in the diff (empty output above). |

All four candidates remain open and unfixed, exactly as `spec.md` § Rollout & Follow-up requires
("**NOT to be fixed in this feature**"). AC-20 remains unchecked because no issue number could be
obtained; see the `POSTING BLOCKED` record.

# Host-Identity Scan (P4-T4)

- **Issue:** #635
- **Plan task:** [P4-T4]

Timestamp: 2026-08-29T06-41

## Output Summary

Twenty-three files under this item's feature folder were scanned for absolute host paths and account-name
path fragments. No file matched. The scanned file count is greater than zero, which makes the zero-hit
result non-vacuous.

SCANNED_FILES: 23
HOST_IDENTITY_HITS: 0

## Command

Command:

```
pwsh -NoProfile -Command '$root = "docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635"; $f = Get-ChildItem -Path $root -Recurse -File -Name | Where-Object { $_ -notlike "*p4-t4-host-identity-scan*" -and $_ -notlike "*plan.2026-08-29T00-23.md" }; Write-Output ("SCANNED_FILES=" + $f.Count); $n = 0; foreach ($p in $f) { $m = @(Select-String -LiteralPath (Join-Path $root $p) -SimpleMatch -Pattern "C:\","c:\","C:/","c:/","\Users\","/Users/" -ErrorAction SilentlyContinue); if ($m.Count -gt 0) { Write-Output ("LEAK " + $p + " " + $m.Count); $n = $n + $m.Count } }; Write-Output ("HOST_IDENTITY_HITS=" + $n)'
```

Output, verbatim:

```
SCANNED_FILES=23
HOST_IDENTITY_HITS=0
```

EXIT_CODE: 0

No `LEAK` line was printed. The `pwsh -NoProfile -Command` wrapper exits `0` regardless of what runs
inside it, so only the printed values are asserted. The asserted value is `HOST_IDENTITY_HITS=0`.

The command was run before this artifact was written.

## The exclusion filter

The filter names exactly two files, and both are stated here with their reason:

1. **This task's own artifact file.** It does not yet exist when the command runs, so the filter has no
   effect on the run recorded above; it is present defensively so that a re-run of the same command
   after this artifact exists produces the same result. Without it, a re-run would report a hit on this
   artifact's own reproduction of the pattern list.
2. **`docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/plan.2026-08-29T00-23.md`.**
   The plan quotes this scan's pattern list verbatim inside the [P4-T4] command block.

Each of those two files quotes this scan's pattern list verbatim, so including either would make the
gate report a hit on a line that is the gate's own pattern list rather than a leak, and the gate could
never pass. No other file is excluded. Both excluded files were checked by hand at planning time and
carry no absolute host path, account name, or machine name outside that pattern list.

## Scan coverage

The file list is enumerated from disk rather than from the tracked index, so the scan covers the Phase 4
artifacts that were still untracked when it ran — the [P4-T2] no-modification proof and the [P4-T3]
toolchain gate — as well as the eighteen artifacts committed by [P4-T1] and the three authored documents
`issue.md`, `spec.md` and `research/reflective-caller-closure.md`. Twenty-four files were present under
the feature folder; twenty-three were scanned and one, the plan file, was excluded by the filter.

The scan cannot cover an artifact written after it runs. The only such artifact is the [P4-T7]
reconciliation record, whose sole command is a repository-relative read of the specification file:
`Get-Content -LiteralPath "docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md"`.
That path is repository-relative and carries no host identity, and the command prints only two computed
counts, so the artifact has no source from which a host path, account name, or machine name could enter
it.

## Patterns searched

The six fixed strings are the two case variants of a Windows drive-letter prefix in backslash form, the
two case variants of the same prefix in forward-slash form, and the two slash forms of a user-profile
directory segment. They are matched with `-SimpleMatch`, so the backslash characters are matched
literally rather than interpreted as regular-expression escapes.

## Auditable-absence record

SearchScope: every file under `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/`, enumerated recursively from disk with `Get-ChildItem -Recurse -File -Name`, less the two files named by the exclusion filter. The measured scope size is 23 files.

SearchPatterns: the six fixed strings reproduced verbatim in the command block above, matched with `Select-String -SimpleMatch`.

SearchResult: none. No file matched any pattern, so no `LEAK` line was printed and the accumulated hit count is zero.

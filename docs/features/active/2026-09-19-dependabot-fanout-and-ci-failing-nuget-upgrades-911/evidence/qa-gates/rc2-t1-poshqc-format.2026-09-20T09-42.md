# In-Place Corrections Cycle — PoshQC Format

- Timestamp: 2026-09-20T09-53-10
- Cycle: 2026-09-20T09-42 in-place corrections (R-C2-1 through R-C2-5)
- Command: `mcp__drm-copilot__run_poshqc_format` with `workspace_root` set to the execution
  worktree root and `scan_folders` omitted
- EXIT_CODE: 0
- ExpectedExitCode: 0

## Two Invocations Were Required

The first invocation **did rewrite files** and the loop was restarted from step 1 accordingly.
The rewrite was a line-ending normalisation, not a reformatting of any statement: the edits in
this cycle were written with LF terminators into files whose existing terminators were CRLF, and
PoshQC normalised each affected file to LF throughout. Four files were rewritten:

- `scripts/dependencies/ProjectConsistency.psm1`
- `scripts/dependencies/Repair-PackageManifestConsistency.ps1`
- `tests/scripts/dependencies/ProjectConsistency.Tests.ps1`
- `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`

The rewrite does not reach the committed blob. `git check-attr text -- <path>` reports
`text: auto` for these paths, so Git normalises terminators on write into the object database
and restores CRLF on checkout. `git diff --numstat` therefore reports content lines only. No
byte-order mark was added or removed by the rewrite: none of the four carries a BOM at `HEAD`
and none carries one now.

## Idempotence Check, Second Invocation

The check is an aggregate SHA-256 over the per-file SHA-256 of every `*.ps1`, `*.psm1` and
`*.psd1` under `scripts/`, `tests/` and `.github/`, taken immediately before and immediately
after the second invocation.

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $f = Get-ChildItem -Path "scripts","tests",".github" -Recurse -File -Include *.ps1,*.psm1,*.psd1 | Sort-Object FullName; "COUNT=" + $f.Count; $agg = ($f | ForEach-Object { (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }) -join ""; "AGG=" + [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($agg))).Replace("-","")'
```

| Point | COUNT | AGG |
|---|---|---|
| Before second invocation | 48 | `5C7E5EA13F2FCD812E7663376361F39B2D9D2B8F14004072AC5A946D2795F3DF` |
| After second invocation | 48 | `5C7E5EA13F2FCD812E7663376361F39B2D9D2B8F14004072AC5A946D2795F3DF` |

| Clause | Required | Measured | Result |
|---|---|---|---|
| Rewrites on the invocation that closes the loop | 0 | **0**, aggregate hash identical over 48 files | PASS |
| Files outside the write set left modified | 0 | **0**, `git status --porcelain` lists only the 7 files this cycle edited | PASS |

## Why an Exit Code Alone Would Not Settle This

A formatter rewrites tracked source and still exits 0, so its exit code reads the same on a
clean run and on a repairing one. The observation recorded above is therefore a
before-and-after tree observation over file bytes, not the exit code. It is what establishes
that the second invocation changed nothing, and it is also what established that the first one
did.

## Output Summary

Two invocations. The first normalised line endings in four files and forced a restart of the
loop; the second rewrote nothing, evidenced by a byte-identical aggregate hash over all 48
PowerShell files in scope. Step 1 of the loop passes on the second invocation.

# Baseline PoshQC Format (P0-T14)

Timestamp: 2026-08-10T22-30

Command:

```
mcp__drm-copilot__run_poshqc_format
    workspace_root = 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a'
    scan_folders   = ['scripts/vscode', 'tests/scripts/vscode']
```

Change detection uses the two-instrument method defined in P4-T1: SHA-256 content hashes for the two
in-scope files (which are clean at baseline but would otherwise be indistinguishable in porcelain),
plus a porcelain before/after difference to detect modification of any file outside those two paths.

EXIT_CODE: 0

MCP payload `ok`: `true`. MCP payload `summary` (verbatim):
`Ran bundled PoshQC format against 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a' with 2 selected scan folder(s).`

Output Summary:

```
scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1: before=9D129B0F22CEAC6B535059769CF3AA345E3B5F3B081C351553E495843E4DD2A1
tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1: before=46A0FA220FC219338A2E11A54088FD78FE56D8D3200370820596C9BDCF2340C0
--- porcelain before ---
 M docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/plan.2026-08-10T14-07.md
?? docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/

scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1: after=9D129B0F22CEAC6B535059769CF3AA345E3B5F3B081C351553E495843E4DD2A1
tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1: after=46A0FA220FC219338A2E11A54088FD78FE56D8D3200370820596C9BDCF2340C0
--- porcelain after ---
 M docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/plan.2026-08-10T14-07.md
?? docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/
```

## Files changed by the formatter

**None — the set is empty.**

- In-scope: both SHA-256 hashes are byte-identical before and after, so neither in-scope file was
  rewritten.
- Out-of-scope: the two porcelain listings are identical, so no file outside the two in-scope paths
  was modified. No `git checkout --` restoration was required and none was performed.
- `.claude/agent-memory/**` did not appear in either listing on this invocation.

## Note carried forward to P4-T1

P0-T14 recorded **zero** out-of-scope formatter-modified files. The plan's bounded-restart clause in
P4-T1 therefore has no pre-existing formatting drift to exempt: if the Phase 4 format call modifies
any file other than the two in-scope paths and `.claude/agent-memory/**`, it is **not** pre-existing
drift and **does** trigger a restart of the Phase 4 loop.

# Final QA — PoshQC Format (P4-T1)

Timestamp: 2026-08-10T23-10

Toolchain step 1 of 3 for PowerShell (format -> analyze -> test; type checking is not applicable to
PowerShell and is intentionally absent). This is an unconditional command task; `EXIT_CODE: SKIPPED`
is not a valid outcome.

Command:

```
# part 1 (before)
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
$targets = @('scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1',
             'tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1')
foreach ($t in $targets) { '{0}: before={1}' -f $t, (Get-FileHash -LiteralPath (Join-Path $root $t) -Algorithm SHA256).Hash }
(git status --porcelain) | Out-String

# the format call
mcp__drm-copilot__run_poshqc_format
    workspace_root = 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a'
    scan_folders   = ['scripts/vscode', 'tests/scripts/vscode']

# part 2 (after) — same two measurements
```

EXIT_CODE: 0

MCP payload `ok`: `true`. MCP payload `summary` (verbatim):
`Ran bundled PoshQC format against 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a' with 2 selected scan folder(s).`

Output Summary:

```
scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1: before=5D2961CEEA163EF32F9DC9D6439B8B20C20A8B569124E8B8A6DE905AD1D3E1D0
tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1: before=D2BC1BE28579A6E322E86CC3789175FA1278FBD6CDF10B710AB4A144798782DD
--- porcelain before ---
 M docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/plan.2026-08-10T14-07.md
 M scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
?? docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/

scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1: after=5D2961CEEA163EF32F9DC9D6439B8B20C20A8B569124E8B8A6DE905AD1D3E1D0
tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1: after=D2BC1BE28579A6E322E86CC3789175FA1278FBD6CDF10B710AB4A144798782DD
--- porcelain after ---
 M docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/plan.2026-08-10T14-07.md
 M scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
?? docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/
```

## Set of files changed by the formatter

**Empty. Zero files changed.**

Change detection uses two instruments because neither alone suffices here. The MCP payload reports
no changed-file count at all. And by Phase 4 both in-scope files are already ` M` relative to `HEAD`,
so if the formatter had rewritten one of them its porcelain line would be *identical* before and
after and the porcelain difference would be empty — the gate would report "0 files changed" exactly
when a file did change.

| Instrument | Scope | Result |
| --- | --- | --- |
| SHA-256 content hash, before vs after | the two in-scope files | **identical for both** — neither was rewritten |
| `git status --porcelain`, before vs after | every other path | **identical listings** — no new entry, so no out-of-scope file was modified |

No `git checkout --` restoration was required and none was performed. No entry under
`.claude/agent-memory/**` appeared in either listing.

## Restart determination

P0-T14 recorded **zero** out-of-scope formatter-modified files, so there is no pre-existing
formatting drift to exempt. This invocation likewise modified nothing out of scope, so the bounded
restart clause is not triggered: the phase proceeds to P4-T2 without restarting.

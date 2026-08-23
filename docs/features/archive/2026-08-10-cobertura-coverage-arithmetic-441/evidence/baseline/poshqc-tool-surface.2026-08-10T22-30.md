# PoshQC Tool-Surface Confirmation (P0-T13)

Timestamp: 2026-08-10T22-30

Confirms that the tool surface described in the plan's § Test Verdict and Coverage Measurement
Contract still holds in the executing worktree
(`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a`).

Command:

```powershell
# (1) MCP call
mcp__drm-copilot__run_poshqc_test
    workspace_root = 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a'
    scan_folders   = ['scripts/vscode', 'tests/scripts/vscode']

# (2) immediately afterwards
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
git status --porcelain
Test-Path -LiteralPath (Join-Path $root 'scripts\powershell\PoshQC\settings\pester.runsettings.psd1')
Test-Path -LiteralPath (Join-Path $root 'config\poshqc-scan.json')
(Get-Module -ListAvailable Pester | Sort-Object Version -Descending | Select-Object -First 1).Version
```

EXIT_CODE: 0

Output Summary:

```
MCP payload: {"ok":true,"tool":"run_poshqc_test",
  "workspace_root":"C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a1cc35d4011888c2a",
  "summary":"Ran bundled PoshQC test against 'C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a1cc35d4011888c2a' with 2 selected scan folder(s)."}

--- porcelain immediately after scoped run_poshqc_test ---
 M docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/plan.2026-08-10T14-07.md
?? docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/
--- end ---
pester-settings-exists=False
poshqc-scan-json-exists=False
pester-version=5.6.1
```

## Acceptance items

**(a) `scan_folders` accepted by all three tools.** `mcp__drm-copilot__run_poshqc_format`,
`mcp__drm-copilot__run_poshqc_analyze` and `mcp__drm-copilot__run_poshqc_test` each declare an
optional `scan_folders` array parameter alongside the required `workspace_root`. This plan passes
`["scripts/vscode", "tests/scripts/vscode"]` to every one of them at every invocation. The call
above accepted the array and reported "2 selected scan folder(s)", confirming the parameter is
honoured rather than ignored.

**(b) `run_poshqc_test` returns a summary string only and reports no verdict.** The payload carries
exactly four keys — `ok`, `tool`, `workspace_root`, `summary` — with no test counts, no test names,
no exit code and no coverage figures. `git status --porcelain` immediately after the call is
byte-identical to the P0-T9 baseline porcelain (the same one modified plan file and the same
untracked feature `evidence/` directory this run created). **Consequence, applied throughout this
plan:** every pass/fail verdict, failure count, per-fixture actual value and coverage number is
attributed to the direct `Invoke-Pester` run. The MCP call is executed wherever the plan names it
and its `ok`/`summary` are recorded verbatim as non-probative, satisfying the No-SKIPPED rule.

> **CORRECTION appended 2026-08-10T23-20 (P5-T5 finding).** The sentence originally recorded here —
> "and it writes no coverage artifact into the workspace, evidenced by an empty
> `git status --porcelain` immediately afterwards" — was **wrong**, and the porcelain check that
> appeared to establish it is not a valid instrument for this claim. `run_poshqc_test` **does** write
> tool output, to `artifacts/pester/pester-junit.xml`,
> `artifacts/pester/powershell-coverage.xml` and `artifacts/pester/powershell-coverage.koverage.xml`.
> Those paths are invisible to `git status --porcelain` because `.gitignore:57` ignores `artifacts/`
> wholesale, so an empty porcelain proves only that nothing *tracked* changed.
>
> This does not affect any conclusion drawn in this plan, and it is not an evidence-location
> violation. Those files are **producer tool output**, not evidence this plan writes: they are
> gitignored, untracked, never committed, and no artifact of this feature is read from or written to
> them. Every evidence artifact this plan produces lives under `<FEATURE>/evidence/<kind>/`, and the
> coverage numbers this plan cites come from the direct `Invoke-Pester` runs whose `OutputPath` is an
> explicit `<FEATURE>/evidence/<kind>/` path. The correction is recorded rather than silently fixed,
> because the original sentence stated a measured fact that measurement did not support. See
> `<FEATURE>/evidence/other/evidence-location-audit.2026-08-10T23-20.md` for the full analysis.

**(c) Neither settings file exists; bundled MCP settings are used.**
`scripts/powershell/PoshQC/settings/pester.runsettings.psd1` -> absent.
`config/poshqc-scan.json` -> absent. No attempt is made to create, restore, or point at either. The
absence of `config/poshqc-scan.json` is exactly why this plan mandates an explicit `scan_folders`
argument on every call instead of relying on an unscoped default set.

**(d) Pester version.** 5.6.1, which satisfies the `>= 5.0` requirement.

All four items match the plan's recorded values. No halt condition is triggered and no invocation
needs to be improvised.

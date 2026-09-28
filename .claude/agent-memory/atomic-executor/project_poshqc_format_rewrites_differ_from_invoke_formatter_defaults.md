---
name: poshqc-format-rewrites-differ-from-invoke-formatter-defaults
description: The PoshQC MCP formatter and a bare `Invoke-Formatter` under PSScriptAnalyzer defaults disagree about which files are dirty; a plan premise of the form "the formatter rewrites file X" measured with the wrong one silently invalidates every downstream task that expects X modified
metadata:
  type: project
---

Measured on issue #911, P0-T15, over `scripts/vscode` and `tests/scripts/vscode` (32 files):

- `Invoke-Formatter` under PSScriptAnalyzer defaults (the planner's preflight measurement) reported
  three files dirty: `Invoke-MSTest.ps1`, `Invoke-MSTestWithCoverage.ps1`,
  `Sync-PackageReferences.ps1`.
- `mcp__drm-copilot__run_poshqc_format` with the same `scan_folders` rewrote **0 of 32**. SHA-256
  before and after were identical for every file and `git status --porcelain` was empty.

The two use different rule sets and the PoshQC one is the tool the toolchain loop actually runs.

**Why it matters beyond the count.** A plan can build structure on "file X will be modified from
this point onward". Here Scope Decision 8 kept `Sync-PackageReferences.ps1` out of the revert set
precisely so the Batch A commit would carry it, and two later tasks hard-asserted its presence —
one requiring `git show --name-only HEAD` to list it, one requiring an exact production-file count
of 2 naming it. With 0 rewrites the file is clean, no Batch A task edits it, so it cannot enter that
commit and both assertions become unsatisfiable. The empty revert set itself degraded gracefully
(the plan pre-authorised `REVERT-SET: empty`); the *keep* half did not.

**A 0-rewrite result is not self-validating.** Prove the formatter is live with a bounded reverted
control before recording it: perturb one out-of-scope file (over-indent a line) with
`[System.IO.File]::WriteAllText` — not the `Write`/`Edit` tool, so no batch-budget slot is consumed
by a transient — re-run the same MCP call, confirm the hash changed, then `git checkout --` it and
confirm the hash returns to its original value.

**Side effect of any rewriting run:** PoshQC also strips the UTF-8 BOM and converts CRLF to LF on
files it rewrites, over and above the formatting fix. It does neither on a run that rewrites
nothing. `.claude/rules/powershell.md` requires the BOM, so a rewrite can leave the file
non-compliant. See [[project_poshqc_format_strips_bom_and_crlf_only_when_it_rewrites]].

**How to apply:**
- In preflight, reject any plan premise of the form "the formatter rewrites X" whose provenance row
  names a different tool than the one the plan's own command reference invokes.
- Treat "which files the formatter touches" as run-time derived, never hard-coded — and check that
  the *empty* derivation is handled by every task that consumes it, not just by the revert task.

Related: [[project_count_idiom_pitfalls_csharpier_and_measureobject]],
[[project_new_cs_files_guarantee_a_format_loop_restart]],
[[project_directory_scoped_format_breaks_ownership_gates]].

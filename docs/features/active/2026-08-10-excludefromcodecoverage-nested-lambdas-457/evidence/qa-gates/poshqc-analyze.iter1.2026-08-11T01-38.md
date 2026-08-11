# [P3-T3] PoshQC analyze — toolchain loop iteration 1 (GATE FAILED, loop restarts)

Timestamp: 2026-08-11T01-38
Iteration: **1**
Command: `mcp__drm-copilot__run_poshqc_analyze` over the `[P3-T2]` iteration-1 file set
(`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`,
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`),
paired with `pwsh -NoProfile -Command 'Invoke-ScriptAnalyzer -Path "<file>"'` per file
EXIT_CODE: 1

MCP Result (verbatim):

```json
{
  "ok": false,
  "tool": "run_poshqc_analyze",
  "workspace_root": "C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a",
  "summary": "Command exited with code 1.",
  "stderr_excerpt": "Exception: PSScriptAnalyzer reported 2 issue(s)."
}
```

## GATE RESULT: FAILED

The `[P0-T7]` baseline diagnostic set has exactly one member. This run reports **2**. The gate passes
only when the diagnostic set is identical to the baseline set, so `EXIT_CODE: 1` is not acceptable
here and the loop restarts from `[P3-T2]` at iteration 2.

## Diagnostic list (from the paired direct runs)

| # | Rule | Severity | File | Line | In baseline? |
|---|---|---|---|---|---|
| 1 | `PSUseBOMForUnicodeEncodedFile` | Warning | `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | (file-level) | **NO — new** |
| 2 | `PSUseSingularNouns` | Warning | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 140 | yes (`[P0-T7]`) |

Verbatim, `Invoke-ScriptAnalyzer -Path "scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1"`:

```
RuleName   : PSUseBOMForUnicodeEncodedFile
Severity   : Warning
ScriptName : Invoke-MSTestWithCoverage.ClosureFilter.ps1
Line       :
Message    : Missing BOM encoding for non-ASCII encoded file 'Invoke-MSTestWithCoverage.ClosureFilter.ps1'
```

Verbatim, `Invoke-ScriptAnalyzer -Path "tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1"`:

```
(no output — zero diagnostics)
```

Count reconciliation: the MCP surface reports only a total. 1 (ClosureFilter.ps1) + 0
(ClosureFilter.Tests.ps1) + 1 (Helpers.ps1, the `[P0-T7]` baseline `PSUseSingularNouns`) + 0
(Helpers.Tests.ps1, zero at `[P0-T7]`) = **2**, matching the MCP total exactly.

`PSUseSingularNouns` on `Get-CoberturaLineConditionCoverageParts` remains the accepted pre-existing
baseline diagnostic. Renaming that function is out of scope: it would exceed the two permitted edits
fixed by `[P2-T8]`, `[P2-T9]` and spec AC 13.

## Remediation applied before iteration 2

**Cause.** `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` contained four occurrences of
the Unicode horizontal-ellipsis character U+2026 (`…`, UTF-8 `E2 80 A6`) in its comment-based help and
in one inline comment, at lines 46, 49, 201 and 248. That made the file non-ASCII, and
PSScriptAnalyzer requires a UTF-8 BOM on any non-ASCII file.

**Fix chosen.** The four ellipses were replaced with the ASCII `...`, making the file pure ASCII, in
preference to adding a BOM. An ASCII file needs no BOM, so the rule is satisfied at the source rather
than worked around, and the file's encoding matches every other `.ps1` in `scripts/vscode`. Verified:
`grep -c -P "[^\x00-\x7F]"` now returns 0 matches (exit 1). The doc-comment name-shape table was
realigned, since the replacement is three characters wide rather than one. No behaviour changed; every
edit is inside a comment.

**Two further changes made in the same remediation**, so that iteration 2 is a single clean pass
rather than two more iterations:

1. **Dead branch removed.** `$presentMembers = if ($presence.Contains($key)) { $presence[$key] } else { $null }`
   had an unreachable `else`: the presence set is built over the same class-node set the removal loop
   walks, so a closure class's own key always exists. It is now `$presentMembers = $presence[$key]`,
   and the corresponding `$null -ne $presentMembers -and` guard was dropped. This is a simplification
   of unreachable code, not a behaviour change.
2. **Coverage tests added.** `[P3-T1]` measured the module at
   `ClosureFilterCommands=113 Executed=95 Percent=84.07`, below the 85% floor `[P3-T4]` gates on.
   Two tests were added to `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
   exercising the rebuild path's remaining branches — a closure class with no class-level `<lines>`
   element (the rollup-creation branch), a line number shared by two retained methods (the
   de-duplication precedence rules for hits, branch and condition-coverage), a non-zero branch
   denominator, and the zero-denominator `'0'` rate fallback. The remedy is added tests, never a
   threshold adjustment.

The ten named regression cases are untouched by this remediation; the two additions are separate,
individually named tests and do not alter the ten-case set required by spec AC 10.

## Output Summary

Gate FAILED at iteration 1: 2 diagnostics against a 1-diagnostic baseline. The new diagnostic is
`PSUseBOMForUnicodeEncodedFile` on the new module, caused by four U+2026 characters in comments. They
were replaced with ASCII `...`. A dead branch was removed and two coverage tests were added in the
same remediation. Files changed, so the loop restarts from `[P3-T2]` at iteration 2.

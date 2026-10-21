# P0-T7 — Pester Test Baseline

Timestamp: 2026-09-09T10-44
Task: [P0-T7]
EXIT_CODE: 0

## Command 1

Command: `mcp__drm-copilot__run_poshqc_test` with `scan_folders` = [`tests/scripts/vscode`]
EXIT_CODE: 0

```
{"ok":true,"tool":"run_poshqc_test","summary":"Ran bundled PoshQC test against the workspace with 1 selected scan folder(s)."}
```

The `workspace_root` value the tool echoes is an absolute host path and is omitted from the
transcription above.

## Command 2

Command: `pwsh -NoProfile -Command '[xml]$j = Get-Content -Raw -LiteralPath "artifacts/pester/pester-junit.xml"; $r = $j.DocumentElement; if (-not $r.HasAttribute("tests")) { throw "junit root carries no tests attribute" }; Write-Output ("TESTS=" + $r.GetAttribute("tests") + " ERRORS=" + $r.GetAttribute("errors") + " FAILURES=" + $r.GetAttribute("failures"))'`
EXIT_CODE: 0

```
TESTS=96 ERRORS=0 FAILURES=0
```

## Recorded baseline

BASELINE_TESTS = 96

Output Summary: The PoshQC test tool returned ok, and the bundled JUnit report at
`artifacts/pester/pester-junit.xml` records 96 tests with `ERRORS=0` and `FAILURES=0`. The observed
`TESTS` integer equals the expected value of 96 the plan records as measured on this branch head, so
there is no discrepancy to note. The same 96 was measured by the orchestrator with `scan_folders`
omitted, establishing that `tests/scripts/vscode` is the whole tracked Pester suite. This baseline
was captured before Phase 1 adds any test; P5-T6 asserts a strictly greater total against it.

# P5-T4 — Final QA Loop, Stage 2: Analyze `scripts/vscode`

Timestamp: 2026-09-09T11-24
Task: [P5-T4]
Command: `mcp__drm-copilot__run_poshqc_analyze` with `scan_folders` = [`scripts/vscode`]
EXIT_CODE: 1
ExpectedExitCode: 1

Tool return:

```
{"ok":false,"tool":"run_poshqc_analyze","summary":"Command exited with code 1.",
 "stderr_excerpt":"Exception: PSScriptAnalyzer reported 16 issue(s)."}
```

The `workspace_root` value the tool echoes is an absolute host path and is omitted from the
transcription above. ANSI colour escapes present in the raw `stderr_excerpt` are also omitted.

## Comparison against the recorded baseline

| Measurement | Findings |
| --- | --- |
| Baseline, `evidence/baseline/p0-t5-analyze-scripts-baseline.md` | 16 |
| Post-change, this run | **16** |
| Delta | **0** |

The observed count is **equal to** the baseline, satisfying the acceptance condition that it be less
than or equal to it.

## Why the equality establishes zero findings attributed to this feature

This feature touches three files in this folder — it creates
`Invoke-MSTestWithCoverage.FirstParty.ps1` and adds exactly one line to each of
`Invoke-MSTestWithCoverage.Helpers.ps1` and `Invoke-MSTestWithCoverage.ps1` — and it removes no
existing code, as `evidence/qa-gates/p4-t5-file-line-counts.md` establishes with an anchored numstat
showing one added and zero removed lines on each modified file. An analyzer finding in this folder
therefore either pre-existed in a file this feature did not touch, or originates in a file this
feature added or modified. Since the total did not rise above 16, **no finding is attributed to any
file this feature adds or modifies.**

A demand for zero findings across `scripts/vscode` is explicitly **not** made, and would not be
satisfiable: the folder carries a pre-existing inherited baseline of 16 findings that predate this
issue, were independently recorded on 2026-08-04 by the issue 400 delivery, and are out of scope.
Nothing was suppressed, and no analyzer severity or rule configuration was changed to reach this
result.

## Second loop pass, 2026-09-09T11-31

The loop restarted from P5-T1 after P5-T6 failed on its first pass. The repair changed two files
under `tests/scripts/vscode` only and no file under `scripts/vscode`, and the re-run confirms the
count did not move.

| Field | Value |
| --- | --- |
| Re-run timestamp | 2026-09-09T11-31 |
| Re-run exit code | 1 |
| Re-run finding count | 16 |
| Re-run tool return | `{"ok":false,"tool":"run_poshqc_analyze","summary":"Command exited with code 1.","stderr_excerpt":"Exception: PSScriptAnalyzer reported 16 issue(s)."}` |

Output Summary: PoshQC analyze over `scripts/vscode` reports 16 issues and exits 1, exactly matching
the pre-change baseline of 16 captured on this branch head. The delta is zero, so zero findings are
attributed to this feature's added or modified files. This is the analyzer half of AC9 for the
production folder; `evidence/qa-gates/p5-t5-analyze-tests.md` carries the tests folder.

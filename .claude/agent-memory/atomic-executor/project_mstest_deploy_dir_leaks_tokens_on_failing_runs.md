---
name: mstest-deploy-dir-leaks-tokens-on-failing-runs
description: A FAILING vstest run drops an empty Deploy_<account> <ts>\In\<MACHINE> tree into /ResultsDirectory; a passing run does not. It breaks both "exactly one TRX" and the name-based sanitisation gate.
metadata:
  type: project
---

A vstest.console.exe run that FAILS leaves an empty MSTest deployment scratch directory inside
whatever path `/ResultsDirectory:` names. A run that PASSES leaves only the TRX.

The directory tree is `Deploy_<account-name> <timestamp>\In\<MACHINE-NAME>` plus a sibling `Out`.
It contains no files.

**Why:** the deployment folder is created per test host and retained when the run does not complete
cleanly. Its generated names embed the local account name and the machine name.

**How to apply:** when an `[expect-fail]` task writes its TRX into the feature evidence tree, delete
the deployment tree immediately after the run and before writing the artifact. Two separate gates
fail otherwise:

- the task's own "that results directory holds exactly one TRX and no others" acceptance, and
- the Phase 5 sanitisation gate that requires ZERO file or directory NAMES under `evidence/`
  containing the account or machine token — see [[project_evidence_sanitisation_capture_time_gate]]
  and the sibling content-sweep note.

Recursive delete idioms are blocked; use `[System.IO.Directory]::Delete($path, $true)`. Fold the
cleanup into the shared test-runner helper so every run is covered, not just the ones you remember:

```powershell
$deploy = @(Get-ChildItem -LiteralPath $resultsDir -Directory -Force |
    Where-Object { $_.Name -like 'Deploy_*' })
foreach ($d in $deploy) { [System.IO.Directory]::Delete($d.FullName, $true) }
```

Describe the removal by role in the evidence artifact. Do not quote the directory name — that would
reintroduce both tokens into a file the sweep has already cleared.

Observed on issue #735, where P1-T2 and P3-T5 each produced one, and the P0-T8, P1-T7, P1-T8, P2-T8,
P3-T11, P3-T12 and P4-T3 passing runs produced none.

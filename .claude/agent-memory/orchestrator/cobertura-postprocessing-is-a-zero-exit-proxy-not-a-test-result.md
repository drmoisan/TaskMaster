---
name: cobertura-postprocessing-is-a-zero-exit-proxy-not-a-test-result
description: A post-processed Cobertura file from Invoke-MSTestWithCoverage.ps1 proves the collect exited 0, but carries no PASS/FAIL or test counts — re-run the gate rather than inferring the result
metadata:
  type: project
---

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws on a non-zero `dotnet-coverage` exit *before* it
post-processes the Cobertura XML. So a coverage file that shows the post-processing fingerprints —
an injected `<sources><source>.</source></sources>` block, workspace-relative paths, and no leftover
`<output>.effective-coverage.config` sibling (the `finally` block removes it) — proves the inner
vstest returned 0. `Assert-CoberturaLineCoverageThreshold` also runs after post-processing, so a
`Done. Coverage artifact:` line additionally means the line-coverage floor passed.

**Why:** On child 825 a previous orchestrator's test gate wrote `coverage/orch-verify-825.cobertura.xml`
and then died without reporting PASS/FAIL. Plan decision D8 deliberately writes **no .trx**, so there was
no test-outcome artifact anywhere under the worktree — a `**/*.trx` glob returns only other features'
committed evidence. The post-processing fingerprint was the only on-disk signal, and it did correctly
predict the result (a re-run returned `Test Run Successful.`, 7213/7213).

**How to apply:**

- Treat the fingerprint as a *control-flow inference about the exit code*, never as the test result.
  Cobertura carries no outcome element and no test counts, so it can never distinguish "all passed"
  from "the run was green enough to exit 0" in any counted way, and it cannot report which tests ran.
- When a gate's PASS/FAIL was never reported, **re-run it**. The full suite here takes roughly two to
  three minutes wall-clock (about 30s of test time plus collection and rebuild-free startup), which is
  cheap against the cost of merging on an unverified gate. Write to a *new* `-CoverageOutput` path so the
  earlier artifact stays independently inspectable.
- The runner's all-green shape emits **no `Failed:` and no `Skipped:` line at all** (plan decision D10
  for this feature records the same rule). Do not read their absence as missing data; grep for
  `Test Run Successful.` plus `Total tests:` / `Passed:`.
- `coverage/*.xml` is gitignored, so a re-run leaves `git status --porcelain` empty and cannot strand
  anything on a merged branch.
- Expect small run-to-run drift in `lines-covered` and `branches-covered` (2 lines / 2 branches across
  two runs at an identical head); `lines-valid` and `branches-valid` are stable. See
  [[coverage-lines-covered-is-nondeterministic]].

# Full-Suite Test and Coverage Baseline — Issue #503 (P0-T9)

Timestamp: 2026-08-08T13-11

Command:
```
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -Configuration Debug -CoverageOutput docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\evidence\baseline\coverage-baseline.cobertura.xml
```
(run from `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55`)

EXIT_CODE: 0

Coverage artifact: `docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\evidence\baseline\coverage-baseline.cobertura.xml` (10,410,066 bytes)

## Output Summary

### Test counts

| Metric | Value |
|---|---|
| Result | `Test Run Successful.` |
| Total tests | **6293** |
| Passed | **6293** |
| Failed | **0** |
| Skipped | **0** |
| Total time | 39.8451 seconds |

The runner emits `Passed:` only when `Failed:` and `Skipped:` are zero; `Test Run Successful.` confirms zero failures. The pre-existing order-dependent flake `YieldAsync_WithoutDispatcher_RemainsStrict` (issue #508) did **not** reproduce on this baseline run.

### Measured root `<coverage>` attributes (verbatim from the emitted XML)

| Attribute | Measured value |
|---|---|
| `line-rate` | **0.858477** |
| `branch-rate` | **0.79237** |
| `lines-covered` | **95309** |
| `lines-valid` | **111021** |
| `branches-covered` | 22077 |
| `branches-valid` | 27862 |
| `complexity` | 24646 |

### Divergence from the plan's reference values — recorded as a finding

The plan (P0-T9) records these expected merge-base reference values: total 6293, `line-rate` 0.7042636529201906, `branch-rate` 0.5849570138678879, `lines-covered` 56458, `lines-valid` 80166.

The test count matches exactly (6293). The four coverage attributes do **not** match. The measured values are recorded verbatim above and have **not** been overwritten with the reference values, per the explicit instruction in P0-T9.

Assessment: the divergence is a counting-method / instrumentation-scope difference, not a code difference. HEAD is byte-identical to the merge-base for all `.cs`, `.csproj`, `.xml`, and `.sln` paths (P0-T4 binary outcome), so no source change can account for it. The reference figures were produced by a different extraction path than `Invoke-MSTestWithCoverage.ps1`'s post-processed Cobertura output (the script's final step is `Post-processing coverage XML for Koverage compatibility`, which rewrites the document; the denominator this repository's `dotnet-coverage` path produces is known to vary with instrumentation scope).

Impact on the plan: **none, and no gate is weakened.** AC24 (P6-T8) is a baseline-versus-final comparison, and P6-T6 runs the identical command with the identical post-processing, so both sides of the comparison are produced by the same counting method. The absolute repo-wide figure is a record-and-report obligation under D6, not a blocking floor. The blocking coverage gate for #503 remains the >= 0.90 per-type line rate for the four new types (P6-T7), which is extracted from the same document by the same query on both sides.

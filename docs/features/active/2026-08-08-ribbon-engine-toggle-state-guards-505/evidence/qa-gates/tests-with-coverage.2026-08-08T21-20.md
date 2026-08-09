# P5-T6 — Full Test Suite with Coverage (final QC)

Timestamp: 2026-08-08T21-20

Command:

```
pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -Configuration Debug -SearchRoot . -CoverageOutput coverage\coverage-final-505.cobertura.xml
```

run from `<REPO>`. Executed through a scratchpad `.ps1` wrapper so the `coverage\...` argument
survives the shell wrapper with its backslash intact (the P0-T9 artifact records the Git Bash
wrapper defect that stripped it on the baseline run; this invocation avoids it, and the dump was
written directly into `coverage\` with no relocation needed).

EXIT_CODE: 0

## Output Summary

### Discovery

- **Discovered test assemblies: 9** — the expected count per plan rule 8 (a count of 0 would be a
  filter/tooling bug, never an empty suite). Script output line: `Discovered 9 test assemblies.`
- vstest resolved by the script:
  `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`

### Test results

| Metric | Value |
|---|---|
| Total | 6435 |
| Passed | 6435 |
| Failed | 0 |
| Skipped | 0 |
| Wall time | 38.0110 s |

`Test Run Successful.` Zero lines in the run output match `^\s*(Failed|Skipped) `.

Delta versus the P0-T9 merge-base baseline: 6399 -> 6435 total, **+36 tests**, all passing. The
36 new tests are this delivery's R1/R2/R5 reflection pins, the `EngineToggleCatalog` suite, and
the `EngineToggleStateCoordinator` suite.

### Reconciliation against the P0-T10 pre-existing failing set

The P0-T10 recorded set is exactly:

```
UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict
```

This run's failure set is **empty**, which is a subset of the recorded set (and satisfies the
strictest reading of the P0-T10 pass rule: the baseline was fully green, so zero failures is the
expected outcome). The #508 order-dependent flake did not surface in this run. Skipped count is
0, matching the P0-T10 recorded skipped set (also empty).

**No regression.** No failure lies outside the recorded set, so the Phase 5 loop does not restart
at P5-T1.

### Coverage (root `<coverage>` attributes, read verbatim from the emitted document)

| Attribute | P0-T9 baseline | **P5-T6 final** |
|---|---|---|
| `line-rate` | 0.858904 | **0.859154** |
| `branch-rate` | 0.793353 | **0.79346** |
| `lines-covered` | 95706 | **95989** |
| `lines-valid` | 111428 | **111725** |
| `branches-covered` | 22225 | **22274** |
| `branches-valid` | 28014 | **28072** |
| `<package>` node count | 9 | **9** |

Values are read directly from the post-processed Cobertura document; none is substituted from an
expectation.

### Evidence hygiene (plan rule 9)

The raw Cobertura dump (`coverage\coverage-final-505.cobertura.xml`, 17,640,273 bytes) stays under
the gitignored `coverage\` directory and is never committed. `git status --porcelain` shows no
`coverage`-rooted entry, so no raw Cobertura XML is stakeable for commit. No
`artifacts\csharp\coverage.xml` was created.

Binary outcome: **PASS**.

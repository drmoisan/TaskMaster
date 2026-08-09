# P0-T9 — Full-Suite Test and Coverage Baseline

Timestamp: 2026-08-08T20-44

Command:

```
pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -Configuration Debug -SearchRoot . -CoverageOutput coverage\coverage-baseline-505.cobertura.xml
```

run from `<REPO>`.

EXIT_CODE: 0

## Output Summary

### Discovery

- **Discovered test assemblies: 9** (the expected count per plan rule 8; a count of 0 would be a
  filter/tooling bug, never an empty suite). Verified independently with the script's `-NoExecute`
  switch, which reports `Discovered 9 test assemblies.`
- vstest resolved by the script: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`

### Test results

| Metric | Value |
|---|---|
| Total | 6399 |
| Passed | 6399 |
| Failed | 0 |
| Skipped | 0 |
| Wall time | 38.4574 s |

`Test Run Successful.` The merge-base suite is fully green in this run.

### Coverage (root `<coverage>` attributes, measured verbatim)

| Attribute | Value |
|---|---|
| `line-rate` | **0.858904** |
| `branch-rate` | **0.793353** |
| `lines-covered` | **95706** |
| `lines-valid` | **111428** |
| `branches-covered` | 22225 |
| `branches-valid` | 28014 |
| `<package>` node count | 9 |

Values are read directly from the emitted post-processed Cobertura document; none is substituted
from an expectation.

### Artifact-location note (recorded for audit fidelity)

The invocation was issued through a Git Bash tool wrapper, which stripped the backslash from the
`-CoverageOutput coverage\...` argument, so the script wrote the dump to
`<REPO>\coveragecoverage-baseline-505.cobertura.xml` (repo root) rather than into `coverage\`. The
10,456,337-byte dump was relocated to `<REPO>\coverage\coverage-baseline-505.cobertura.xml`
immediately, before any commit. The relocation is a path correction only; the file content is the
byte-identical output of the recorded command. `git status --porcelain` after the move shows no
`coverage`-rooted entry, so no raw Cobertura XML is stakeable for commit (plan rule 9). Subsequent
invocations pass the path with forward slashes to avoid the wrapper defect.

Binary outcome: PASS.

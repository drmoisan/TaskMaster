# [P0-T3] Phase 0 feature requirement documents read

Timestamp: 2026-08-11T00-12
Command: file reads only (no shell command)
EXIT_CODE: 0

Work Mode: full-bug
AC Source: `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/spec.md` (sole source)

`user-story.md` exists in the feature folder but carries no `## Acceptance Criteria` section and is
NOT an acceptance-criteria source under work mode `full-bug`. `issue.md` carries an
`## Acceptance Criteria` section of 7 items; under `full-bug` that section is not the AC source
either. Only `spec.md` § Acceptance Criteria (16 checkbox items, lines 504-553) is tracked.

## Files Read

| # | Repo-relative path | Lines |
|---|---|---|
| 1 | `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/issue.md` | 129 |
| 2 | `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/spec.md` | 629 |
| 3 | `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/research/2026-08-10T14-10-excludefromcodecoverage-nested-lambdas-fix-surface.md` | 521 |
| 4 | `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md` | 217 |
| 5 | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 348 |
| 6 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 455 |
| 7 | `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | 468 |
| 8 | `coverage.config` | 24 |

## Load-bearing facts extracted

- `spec.md` § Acceptance Criteria contains exactly 16 `- [ ]` checkbox items, all currently unchecked.
- `spec.md` § Proposed Fix selects Candidate 1c and fixes the hard ordering constraint:
  `Remove-CoberturaExemptClosureCoverage` runs after the `//class[@filename]` normalization loop and
  before `Merge-CoberturaClassesByFilename`, inside `ConvertTo-KoverageCoberturaXml`.
- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` is 455 lines post-#441; the insertion point
  for the filter call is between the `foreach ($classNode in $xml.SelectNodes('//class[@filename]'))`
  loop and the `Merge-CoberturaClassesByFilename -XmlDocument $xml` call inside
  `ConvertTo-KoverageCoberturaXml`.
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` is 468 lines, leaving 31 lines
  of headroom under the 500-line ceiling for regression case 6. Recorded here because the plan's
  `[P3-T10]` acceptance re-measures every test file against that ceiling after formatting.
- Deviation observed and recorded, not acted on: `[P3-T1]` states "Baseline for that file, measured
  at preflight: Passed=8". The post-#441 file contains 19 `It` blocks across three `Describe` blocks
  (12 + 3 + 4). That plan figure was measured against the pre-#441 form of the file; #441 (PR #538)
  added tests. The plan expectation is documented here as a deviation and the actual measured
  baseline is captured by `[P0-T8]`. No test is removed or weakened to match the plan's figure.
- Pre-existing tests that constrain the filter's behavior:
  - 'merges duplicate class entries that point to the same source file' and 'normalizes stale
    TaskMaster roots before merging duplicate production class entries' both carry a `<>c` closure
    class with an EMPTY `<methods />` element. `[P2-T7]` requires such a class to be left untouched.
  - 'computes the merged per-file line-rate from the merged rollup alone' and 'preserves the primary
    class methods subtree and every hits value when merging' both carry a `Ns.Foo.<>c` closure class
    whose method is named `N` — a plain name that yields no derived declaring member, so the
    fail-safe retention path keeps it and both tests remain unaffected.
- `coverage.config` declares only `<ModulePaths><Exclude>` and no `<Attributes>` block, so the
  collector's default attribute excludes (including
  `^System\.Diagnostics\.CodeAnalysis\.ExcludeFromCodeCoverageAttribute$`) are in force. This file is
  not modified by this feature.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` emits `Post-processing coverage XML for Koverage
  compatibility...` (line 338) and `Done. Coverage artifact: <path>` (line 343) through `Write-Output`
  with no timestamps, and overwrites the raw Cobertura file in place at line 342. This confirms the
  `[P0-T11]` requirement to stamp stdout at emission.
- The runner prints `Discovered $($testAssemblies.Count) test assemblies.` (line 315) and never the
  paths, and its discovery filter is
  `$_.FullName -match "\\bin\\$Configuration\\" -and $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\ref\\'`
  (lines 296-302), matching the reproduction rule in `[P0-T11]`.

## Output Summary

Eight feature and pipeline documents read. Work mode confirmed `full-bug` from the `- Work Mode:
full-bug` marker in `issue.md` line 4. AC source confirmed as `spec.md` only, 16 items. One plan
expectation (`[P3-T1]` Passed=8 baseline for the helpers test file) is recorded as written against
the pre-#441 file and is documented as a deviation rather than acted on.

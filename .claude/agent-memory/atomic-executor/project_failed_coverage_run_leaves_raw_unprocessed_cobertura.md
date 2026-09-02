---
name: failed-coverage-run-leaves-raw-unprocessed-cobertura
description: A non-zero Invoke-MSTestWithCoverage run leaves coverage.cobertura.xml RAW (absolute class/@filename, no root lines-covered/lines-valid, all-modules line-rate ~0.70 not ~0.85); recover in memory with ConvertTo-KoverageCoberturaXml
metadata:
  type: project
---

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws on a non-zero inner vstest exit code
(`:235-237`) BEFORE the Koverage post-processing block (`:339-343`). `dotnet-coverage` has
already written `coverage\coverage.cobertura.xml` by then, so the file exists but is the RAW
collector output:

- `class/@filename` is still an ABSOLUTE path, so a lookup keyed on the repo-relative
  `QuickFiler\Viewers\Foo.cs` form finds nothing.
- The root `coverage` node carries no `lines-covered` / `lines-valid`; those are set only at
  `Invoke-MSTestWithCoverage.Helpers.ps1:442-445`.
- The root `line-rate` is the ALL-MODULES rate, not the first-party allowlist rate. Measured
  gap: issue #608's failed run recorded `line-rate="0.7017"` / `lines-valid="81570"`, against
  ~0.853 / ~64k on processed runs of comparable trees. Reading the raw number as a coverage
  regression is a false conclusion.

**Why:** the throw is not caught (only a `finally` that removes derived settings), so it
propagates past `Set-Content` and nothing rewrites the document.

**How to apply:** when a coverage run exits non-zero but you still need the figures, do not read
the emitted file directly. Dot-source `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and
convert in memory:
`ConvertTo-KoverageCoberturaXml -XmlContent (Get-Content coverage\coverage.cobertura.xml -Raw -Encoding UTF8) -RepoRoot (Get-Location).Path`
(both parameters are mandatory strings), then read the six values from the converted document.
Note the same ordering means `Assert-CoberturaLineCoverageThreshold` never runs on a failed run,
so a sub-80% raw rate is not itself the cause of the non-zero exit.

Related: [[project_dotnet_coverage_denominator_nondeterminism]],
[[project_koverage_cobertura_postprocessing_shape]].

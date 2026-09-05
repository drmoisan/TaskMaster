---
name: dotsourcing-invoke-mstest-clobbers-coverageoutput-param
description: A wrapper script that dot-sources Invoke-MSTestWithCoverage.ps1 has its own $CoverageOutput parameter silently overwritten by that script's param-block default, so the Cobertura lands at coverage\coverage.cobertura.xml instead of the requested path
metadata:
  type: project
---

A throwaway wrapper that takes `-CoverageOutput` and then dot-sources
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` loses its own parameter value. That script's
`param` block declares `[string]$CoverageOutput = "coverage\coverage.cobertura.xml"` (line 9 as
of 2026-09-05), and dot-sourcing a script with a `param` block re-creates its parameter
variables in the CALLING scope with their defaults. Any later `dotnet-coverage collect --output
$CoverageOutput` therefore writes to the script's default path, not the caller's.

Observed on issue #781 [P0-T8]: run reported `Code coverage results: coverage\coverage.cobertura.xml.`
with `COLLECT_EXIT_CODE: 0` while `coverage\baseline-781.cobertura.xml` never existed. The
downstream post-processing task read the requested path and would have failed on a missing file.

**Why:** the collector exits 0 and the tests all pass, so nothing in the run signals the
deviation. Only the one-line `Code coverage results:` message names the real path, and a plan
that checks only the exit code and the test counts never reads it.

**How to apply:** a parameter name that also appears in a dot-sourced script's `param` block is
unsafe. Either capture the wrapper's value into a differently-named variable BEFORE the
dot-source (`$outPath = $CoverageOutput`) and use that at the collect line, or grep the run log
for `Code coverage results:` and copy the produced document to the path later tasks read. The
clobbering recurs on every run of the same wrapper shape, so the baseline and the final pass
both need the same correction. `$ResultsDirectory` is unaffected because that script has no such
parameter. Related: [[project_failed_coverage_run_leaves_raw_unprocessed_cobertura]],
[[project_coverage_runner_throws_before_postprocessing]].

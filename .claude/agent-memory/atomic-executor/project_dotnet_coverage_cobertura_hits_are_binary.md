---
name: dotnet-coverage-cobertura-hits-are-binary
description: dotnet-coverage Cobertura output records line hits as 0 or 1 (a covered flag), so any gate comparing hit counts between two lines (e.g. guard hits > record hits) is unsatisfiable; prove both branch outcomes via the guard line's condition-coverage instead
metadata:
  type: project
---

The `dotnet-coverage collect --output-format cobertura` documents this repo's coverage route produces
(both the RUNNER and the DIRECT route, after `ConvertTo-KoverageCoberturaXml`) carry `hits="0"` or
`hits="1"` only. Measured on issue 948 P0-T17 (2026-10-01): all 133464 `<line>` elements of the
baseline document had MAX-HITS=1, GT1=0, even for `CompletePrime` lines executed by dozens of tests.

**Why it matters:** plan 948's D-8 / P3-T10 required "guard line hits strictly greater than record
line hits" to prove the skip branch ran. With binary hits both read 1, so the clause can never hold —
a planner-side unobservable-output defect (the plan inferred an execution count the tool never
prints). The run stopped at P3-T10 for a plan correction.

**How to apply:**
- At preflight, reject any acceptance clause that compares or thresholds `hits` above 1, or that
  infers "executed more often" from hits.
- The working proof of both outcomes of an `if` is the guard line's own branch data:
  `Get-CoberturaClassLineSummary` LineMap entry `Branch=True`, `Covered == Total`, `Total >= 2`
  (observed `covered=2 total=2` on the 948 guard line), plus a named test asserting the
  suppressed-repeat behaviour.
- `Hits >= 1` (covered / not covered) remains a valid gate.

Related: [[project_cobertura_filename_maps_to_several_class_nodes]], [[project_package_rate_not_lower_gate_trips_on_unrelated_file_variance]]

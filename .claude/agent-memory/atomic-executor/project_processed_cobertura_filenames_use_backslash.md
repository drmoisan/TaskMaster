---
name: processed-cobertura-filenames-use-backslash
description: The Koverage-processed Cobertura document keys <class filename> on repo-relative Windows paths with backslashes, so a plan that matches the forward-slash spelling gets zero rows and an unevaluable coverage gate
metadata:
  type: project
---

`ConvertTo-KoverageCoberturaXml` rewrites every `<class @filename>` to a repository-relative path using
`-PathSeparator`, which defaults to `[System.IO.Path]::DirectorySeparatorChar` — `\` on Windows — and the
`\` branch of `ConvertTo-KoverageRelativePath` ends `return $relativePath.Replace('/', '\')`. So the key a
changed-line or new-file coverage task must match is `QuickFiler\Controllers\EfcDataModel.cs`, never
`QuickFiler/Controllers/EfcDataModel.cs`. Confirmed against a committed processed artifact
(`...-439/evidence/qa-gates/issue-439-final.normalized.cobertura.xml`), whose raw sibling in the same folder
still carries absolute `C:\...` filenames — the raw and processed forms are distinguishable at a glance by
that attribute alone.

**Why:** plan prose spells repository paths with `/` because that is the git and Write-Set convention, and
a coverage task written in that prose inherits the spelling into its matching predicate. The failure is
silent in the worst way: the match returns zero rows, so the changed-coverable denominator is 0, and a
`>= 90.00%` floor becomes unevaluable (or divides by zero) rather than failing. Nothing in the task's own
acceptance can distinguish that from a file with no coverable lines.

**How to apply:** in preflight, whenever a task derives per-file figures from a Cobertura document, require
the plan to pin the separator once and quote at least one full key verbatim. Also check *which* document the
task will actually read: a run that throws before `Set-Content` leaves the RAW document at the output path,
with absolute filenames, third-party packages still present and classes un-merged, so a downstream task that
assumes the processed form gets neither the right key nor a comparable rate.

Related: [[project_coverage_delta_reproduce_baseline_counting_method]],
[[project_failed_coverage_run_leaves_raw_unprocessed_cobertura]],
[[project_koverage_cobertura_postprocessing_shape]].

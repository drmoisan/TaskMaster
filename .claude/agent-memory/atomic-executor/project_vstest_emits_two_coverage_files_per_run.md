---
name: vstest-emits-two-coverage-files-per-run
description: A single vstest /EnableCodeCoverage run leaves TWO *.coverage files under its ResultsDirectory, so any plan gate demanding "exactly one" is unsatisfiable by construction
metadata:
  type: project
---

One `vstest.console.exe ... /EnableCodeCoverage /ResultsDirectory:<dir>` run leaves **two** files matching
`Get-ChildItem -Path <dir> -Recurse -Filter '*.coverage'`:

1. the published attachment, under `<dir>\<guid>\<acct>_<machine>_<timestamp>.coverage`
2. an in-run copy, under `<dir>\<acct>_<machine>_<timestamp>\In\<machine>\<same-name>.coverage`

Both have identical byte length. The vstest console `Attachments:` section prints only the first, so reading
the console output suggests there is one.

**Why:** In item #751 the plan required the locate search to "return exactly one file" and routed any other
count to a `COVERAGE_CAPTURE_BLOCKED` rung that forbids converting an arbitrary member of the set. The count
was 2 on both the baseline run and the final-QC run, so the numeric coverage pair was unobtainable and the
coverage criterion came out remediation-required — even though `dotnet-coverage` was installed and working.
The gate could never have passed; it was not a property of the change under test.

**How to apply:** At preflight, flag any acceptance condition asserting exactly one `*.coverage` file under a
vstest results directory. The correct predicate selects the published attachment specifically (exclude paths
containing `\In\<machine>\`, or take the one that is a direct descendant of a GUID-named subdirectory), or
merges both with `dotnet-coverage merge`. During execution do NOT self-repair a plan that says "exactly one"
— record the observed count and the blocked outcome, since the plan explicitly forbids picking one.

Note the attachment filenames embed the account name and machine name, so never transcribe those paths into
an artifact. Related: [[_shared_no_absolute_host_paths]],
[[project_csharp_canonical_coverage_artifact_conversion]],
[[project_failed_coverage_run_leaves_raw_unprocessed_cobertura]].

---
name: pwsh-param-name-case-collision-flattens-log-array
description: In a helper .ps1 with a [string]$Log parameter, assigning $log = Get-Content ... reuses the SAME variable (case-insensitive) and flattens the line array into one space-joined string, so every ^-anchored pattern silently misses and the artifact records -1 / blank fields
metadata:
  type: project
---

PowerShell variable names are case-insensitive, and a typed `param([string]$Log)` keeps its
`[string]` constraint for the whole script scope. A later `$log = Get-Content -LiteralPath $file`
therefore does not create a new array variable: it converts the array into ONE string joined by
spaces. Non-anchored `-match` patterns still hit inside that single string, but every `^`-anchored
pattern (`'^RUNNER-EXIT_CODE: '`, `'^\s*Total tests: '`) matches only the first line, so the
extractor reports a sentinel (`-1`) or an empty transcription while the log plainly holds the line.

**Why:** Observed 2026-09-17 on item-792 [P0-T12]: the coverage-baseline artifact was first written
with `EXIT_CODE: -1` and no `Total tests:`/`Passed:` rows although `RUNNER-EXIT_CODE: 1` sat at log
line 1462; a byte probe showed plain ASCII, and the only difference between the patterns that hit
and those that missed was the `^` anchor. Renaming the local to `$runLines` fixed it without any
other change. The failure is silent and produces a schema-complete artifact with wrong values, which
is the dangerous shape (see [[feedback_never_predict_an_observation_into_an_artifact]]).

**How to apply:**
- In any multi-mode helper, never reuse a `param()` name (in any casing) as a local; name locals
  `$runLines`, `$logLines`, etc., and keep parameters typed only when the type is wanted everywhere.
- When an artifact extractor reports a sentinel or an empty field while a Grep of the same log finds
  the line, suspect the variable type before the regex or the encoding.
- A cheap tripwire: print `$lines.Count` right after `Get-Content`; a count of 1 on a 1,000-line log
  is the fingerprint.

Related: [[project_pwsh_nested_quotes_in_subexpression_fail_to_parse]] (the sibling helper-authoring
trap hit in the same run), [[project_pwsh_file_starts_in_session_root_needs_workingdirectory]]
(`-File` relative paths resolve against the launching shell, so pass the helper by absolute path).

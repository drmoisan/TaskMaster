# [P3-T4] Nullable gate

Timestamp: 2026-09-06T15-05

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

FINAL-NULLABLE-WARNINGS: 0
FINAL-NULLABLE-ERRORS: 0

Output Summary:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:13.33
```

## Comparison against the [P0-T9] baseline

| Measure | Baseline [P0-T9] | This run | Delta |
|---|---|---|---|
| Warnings | 0 | 0 | 0 |
| Errors | 0 | 0 | 0 |

The error count is 0, which is this task's acceptance.

## Command form

The command is character-for-character the CLAUDE.md nullable gate and the command in
`.github/workflows/_build-nullable.yml`. Two properties were preserved rather than "restored":

- `/p:Nullable=enable` was **not** added. No project in this repository carries a `<Nullable>`
  element and there is no `Directory.Build.props`, so the property is a solution-wide opt-in that
  conscripts every file which has never adopted the `#nullable enable` pragma. Adding it would
  produce hundreds of errors unrelated to this change, and CI omits it deliberately.
- `/t:Build` was **not** substituted. MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` would return exit 0 having skipped `CoreCompile`
  on every project, and the gate could not fail.

## Relevance to this change

`/p:TreatWarningsAsErrors=true` is what makes D9 load-bearing: an unread `private readonly` field
raises CS0414, a warning, which this command would promote to an error. The two new gate bounds are
therefore internal get-only auto-properties, whose compiler-generated backing fields are read by
their getters. This run's zero-warning result confirms that choice across the whole change, not just
at the point [P1-T4] first observed it.

This is step 3 of the uninterrupted toolchain pass.

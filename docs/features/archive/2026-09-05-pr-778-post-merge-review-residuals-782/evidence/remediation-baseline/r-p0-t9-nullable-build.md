# [P0-T9] Baseline — nullable build

Timestamp: 2026-09-06T01-34

Command:

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

Run from the worktree root. This is character-for-character the command
`.github/workflows/_build-nullable.yml` runs. Two properties of it are load-bearing and were not
altered:

- `/p:Nullable=enable` is **not** passed. No project in this repository carries a `<Nullable>`
  element and there is no `Directory.Build.props`, so that property would be a solution-wide opt-in
  conscripting every file that has never adopted the `#nullable enable` pragma.
- `/t:Build` is **not** substituted for `/t:Rebuild`. MSBuild's up-to-date check does not invalidate
  on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped and
  the gate cannot fail.

EXIT_CODE: 0

Output Summary: the build succeeded with no diagnostics promoted to errors. The final summary lines,
verbatim:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

BASELINE-NULLABLE-WARNINGS: 0
BASELINE-NULLABLE-ERRORS: 0

## Consumer

[P4-T4] re-runs this command as the type-check step of the final toolchain pass and requires the same
three figures.

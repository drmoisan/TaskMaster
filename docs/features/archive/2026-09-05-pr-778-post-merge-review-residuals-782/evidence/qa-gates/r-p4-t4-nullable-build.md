# [P4-T4] Final QC step 4 — nullable build

Timestamp: 2026-09-06T01-50

Command:

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

Run from the worktree root, in the same uninterrupted toolchain pass as [P4-T1] through [P4-T3].
This is character-for-character the command `.github/workflows/_build-nullable.yml` runs.
`/p:Nullable=enable` is not passed and `/t:Build` is not substituted for `/t:Rebuild`.

EXIT_CODE: 0

Output Summary: the build succeeded with no diagnostic promoted to an error. The final summary lines,
verbatim:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

FINAL-NULLABLE-WARNINGS: 0
FINAL-NULLABLE-ERRORS: 0

The three figures are identical to the [P0-T9] baseline. Neither edited file carries a
`#nullable enable` pragma change, and neither introduces a nullable-flow diagnostic.

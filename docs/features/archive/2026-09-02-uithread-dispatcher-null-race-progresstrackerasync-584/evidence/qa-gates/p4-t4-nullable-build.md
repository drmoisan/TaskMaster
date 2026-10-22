# P4-T4 — Type-check / nullable build (second pass)

Timestamp: 2026-09-03T21-48

Command:
```text
env -C <worktree-root> MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

EXIT_CODE: 0

## Output Summary

Trailing MSBuild summary, verbatim:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:16.00
```

- Errors: **0**
- Warnings: **0**

Quoted command line, for the three clauses below:

```text
env -C <worktree-root> MSYS_NO_PATHCONV=1 msbuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

## Acceptance

Satisfied on all four clauses:

1. `EXIT_CODE: 0`.
2. `0 Error(s)`.
3. The quoted command line CONTAINS `msbuild.exe TaskMaster.sln`. The clause is worded as `contains`
   rather than `begins with` because the recorded line begins with the `env -C <worktree-root> `
   working-directory prefix and the `MSYS_NO_PATHCONV=1 ` assignment, and it records only the
   executable spelling this shell requires (constraint 3 of "Shell constraints measured in this
   worktree").
4. The quoted command line contains no `Nullable=enable` substring and uses `/t:Rebuild`. These are
   the substantive checks and both hold. The switch set is character-for-character the one CLAUDE.md
   mandates and the one `.github/workflows/ci.yml` runs.

This gate is what proves AC2's real value. With the backing field now declared `Dispatcher?` in a
file that opts into nullable analysis via its line-1 `#nullable enable` directive, a getter that
returned the field without narrowing it would raise `CS8603`, which
`/p:TreatWarningsAsErrors=true` promotes to a build error. The build reports `0 Error(s)`, so the
guarded getter narrows the field correctly.

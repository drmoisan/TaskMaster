# Baseline — Type-check gate ([P0-T10])

- Issue: #644
- Task: `[P0-T10]`
- Timestamp: 2026-08-29T08-15

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
Working directory: repository root (`<repo-root>`)
Shell: PowerShell (`pwsh -NoProfile`)
EXIT_CODE: 0

## Command-shape constraints honoured

- `/p:Nullable=enable` was **not** added. No project in this repository carries a `<Nullable>`
  element and there is no `Directory.Build.props`, so the property is a solution-wide opt-in that
  conscripts every file which has never adopted the `#nullable enable` pragma. CI omits it
  deliberately.
- `/t:Build` was **not** substituted. A warm `/t:Build` returns exit 0 with `CoreCompile` skipped,
  so the gate could not fail. That compilation actually ran was verified: the captured log
  contains **36 `csc.exe` invocations**.

## msbuild final summary block

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:18.63
```

**Warning count: 5. Error count: 0.**

All five warnings are the same pre-existing `System.Reactive` `packages.config` advisory recorded
in `[P0-T9]`, emitted by a package targets file and carrying no diagnostic identifier. They are
not promoted to errors by `/p:TreatWarningsAsErrors=true` because that switch promotes compiler
diagnostics, and this advisory is an MSBuild task warning with no code.

## CS0414 statement (explicitly required by this task)

Command: fixed-string search for the token `CS0414` over the captured build log.

```
cs0414-hits=0
```

**No `CS0414` diagnostic appears anywhere in this build.** This is the expected pre-change state:
at the base commit `_registeredDigits` is both assigned in `RegisterNavigation()` and read by the
`var format = _registeredDigits == 2 ? "00" : "";` expression in `UnregisterNavigation()`, so the
field is not write-only and CS0414 does not fire.

This baseline is what makes the plan's indivisibility argument checkable. `[P2-T3]` deletes the
`format` expression, the assignment, and the field declaration in one edit. Deleting only the
`format` expression would leave the field assigned and never read, which is CS0414, which this
gate promotes to an error. `[P2-T4]` and `[P4-T4]` re-run this same command and re-assert the
zero-`CS0414` property after the edit.

## Gate outcome

The command exited **0**, so the `REMEDIATION-REQUIRED` reporting branch this task authorizes was
**not** taken and Phase 1 may proceed. `[P1-T3]`, `[P4-T3]`, and `[P4-T4]` all require this build
to be green and it is.

Output Summary: `/t:Rebuild` type-check gate green at the pre-change base. **0 errors, 5
warnings**, all five the pre-existing `System.Reactive` `packages.config` advisory. **Zero
`CS0414` diagnostics** in the build output. 36 `csc.exe` invocations confirm the compilation was
not skipped by MSBuild incrementality.

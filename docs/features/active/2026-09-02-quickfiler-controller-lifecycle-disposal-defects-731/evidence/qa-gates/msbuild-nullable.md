# Final QA gate 4 — type check with warnings as errors

Timestamp: 2026-09-03T14-30

Task: [P5-T4]
Issue: #731

## Command

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

MSBuild executable actually invoked: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` (MSBuild 18.9.1.35102), recorded in full under the Evidence path-hygiene rule's stated exception for an external build-tool executable.

This is character-for-character the command in `.github/workflows/ci.yml`. Two properties of it are load-bearing and were not altered:

- `/p:Nullable=enable` was **not** added. No project in this repository carries a `<Nullable>` element and there is no repository-root nullable opt-in, so forcing the property would conscript every file that has never adopted the `#nullable enable` pragma. CI omits it deliberately.
- `/t:Build` was **not** substituted for `/t:Rebuild`. MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on every project, and the gate could not fail.

The recorded `Command:` above contains neither the text `Nullable=enable` nor the text `/t:Build`.

EXIT_CODE: 0

## Output Summary

Build summary lines, as observed:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.37
```

- Observed error count: **0**
- Observed warning count: **0**

Nullable enforcement in this repository is per-file opt-in: a file participates when it carries a `#nullable enable` directive, and `/p:TreatWarningsAsErrors=true` then promotes its `CS86xx` diagnostics to build errors. No file this change touches gained or lost that directive, and no `CS86xx` diagnostic was produced.

This gate is also what would have caught the `volatile` variant of the finding-4 fix that `[P4-T5]` deliberately avoided: adding the `volatile` modifier to `removespecificcontrolgroupcounter` would produce CS0420 at both `Interlocked` call sites, and under `TreatWarningsAsErrors` those two warnings would become build errors. The fix uses `Volatile.Read` on an unmodified field instead, and this build confirms the result is clean.

## Comparison against the [P0-T8] baseline

`EVIDENCE/baseline/msbuild-nullable.md` recorded exit 0 with 0 warnings and 0 errors for the identical command on the pre-change tree. The post-change result is identical.

## Verdict

PASS. `EXIT_CODE: 0`, and the recorded `Command:` contains neither `Nullable=enable` nor `/t:Build`.

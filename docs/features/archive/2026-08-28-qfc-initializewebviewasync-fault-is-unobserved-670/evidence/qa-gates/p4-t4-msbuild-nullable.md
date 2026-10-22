# P4-T4 — Toolchain step 3 of 4: type checking (nullable analysis)

Timestamp: 2026-09-01T20-14
Command:

    & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true

Resolved MSBuild executable: `<vs-install>\MSBuild\Current\Bin\MSBuild.exe`

EXIT_CODE: 0

## Output Summary

    Build succeeded.
        5 Warning(s)
        0 Error(s)

    Time Elapsed 00:00:11.90

**Warning count: 5. Error count: 0.** Identical to the P0-T11 baseline.

The five warnings are the pre-existing System.Reactive `packages.config` diagnostic. It is emitted by an MSBuild target rather than by the compiler and carries no diagnostic code, which is why `/p:TreatWarningsAsErrors=true` does not promote it: that property promotes compiler diagnostics, and an uncoded target-level `warning :` message is outside its reach. This is how a five-warning count coexists with a zero-error count under warnings-as-errors.

A search of the build log for a coded diagnostic — matching `: error [A-Z]+[0-9]+:` — returns **zero**, and the same search for `: warning [A-Z]+[0-9]+:` also returns zero. No `CS86xx` nullable-flow diagnostic was produced.

## Command fidelity — both load-bearing properties preserved

This is character-for-character the command in `.github/workflows/ci.yml` for the step that builds with nullable warnings treated as errors.

- **`/p:Nullable=enable` was not added.** No project in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so the property is a solution-wide opt-in that would conscript every file which has never adopted the `#nullable enable` pragma. Nullable enforcement here is per-file opt-in, and omitting the property loses no enforcement over any file that has opted in. CI omits it deliberately.
- **`/t:Build` was not substituted.** A warm `/t:Build` returns exit 0 having skipped `CoreCompile` on every project, because MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, so the gate could not fail.

That the rebuild genuinely compiled is verified rather than assumed: the log contains **67** `CoreCompile:` target executions.

## The new file's nullable posture

`QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` deliberately carries **no** `#nullable enable` directive, verified in P1-T1 by a search returning zero matches. Neither sibling partial carries one, the repository is per-file opt-in, and adding the directive would have conscripted the file into this gate for no benefit. The file is therefore outside nullable analysis, consistent with every other partial of the same type.

## Position in the Phase 4 pass

This is stage 3 of the single uninterrupted toolchain pass P4-T1 through P4-T5. Stages 1 and 2 rewrote no file and produced no failure, so no restart was triggered at any point.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.

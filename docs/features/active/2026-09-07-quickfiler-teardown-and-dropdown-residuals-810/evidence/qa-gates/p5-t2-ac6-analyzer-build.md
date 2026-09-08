# [P5-T2] AC6 Analyzer Build After the Accessor Deletion

Timestamp: 2026-09-08T10-10
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (the [P0-T9] command, verbatim)
EXIT_CODE: 0
Output Summary: The full-solution rebuild with the analyzer properties succeeded with zero warnings and zero errors after the deletion of the dead `SearchOwnsDropDownDismissal` accessor. Both counts equal the [P0-T9] baseline of zero, so the deletion raised no new diagnostic anywhere in the solution and in particular none on the retained backing field.

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:25.74
```

ANALYZER-WARNINGS: 0
ANALYZER-ERRORS: 0

CS0649-OR-CS0169: NONE

## Comparison against the baseline

| Counter | [P0-T9] baseline | This build | Relation |
| --- | --- | --- | --- |
| Warnings | 0 | 0 | equal, so less than or equal holds |
| Errors | 0 | 0 | equal, so less than or equal holds |

The baseline is a zero ceiling, so the required "less than or equal to the corresponding `BASELINE-ANALYZER-` value" relation admits only zero. Both counts are zero.

## Why the retained field raises no diagnostic

The build output was searched for `CS0649` and `CS0169`. Match count: 0. The backing field `_searchOwnedDismissal` cannot regress to either diagnostic because every one of its live sites remains: the write sites at `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:183`, `:220`, `:234`, `:257` and `:265`, and the read site at `:263`. CS0649 reports a field that is never assigned and CS0169 a field that is never used; neither condition holds.

## AC6 evidence

This build is the AC6 evidence for the controller half. It establishes that the accessor was genuinely dead: removing it compiles the whole solution clean, so no production or test call site depended on it. The `#796` AC4 re-pin test reaches the retained field by reflection on its name rather than through the deleted accessor, which [P5-T3] confirms behaviourally.

## D5 file-lock check

The build output was searched for `MSB3061` and `MSB3021`. Match count: 0. The D5 stop condition did not fire and no process was terminated.

## D4 note

`/t:Rebuild` was used, not `/t:Build`. A warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project because MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, and would therefore run no analyzers at all. `/p:Nullable=enable` was not added.

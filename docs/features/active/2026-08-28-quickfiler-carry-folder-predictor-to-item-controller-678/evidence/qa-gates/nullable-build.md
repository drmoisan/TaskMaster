# P2-T4 — Nullable / type-check build

Timestamp: 2026-09-01T23-49

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

## Output Summary

MSBuild summary lines, reproduced verbatim:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

**No `CS86` diagnostic was introduced relative to the P0-T7 baseline enumeration.** A scan of the
full build log for the pattern `CS86[0-9][0-9]` returned **no match**. The P0-T7 baseline enumerated
the empty set, so the post-change set is equal to it, not merely a subset: the delta is zero.

`0 Error(s)` under `/p:TreatWarningsAsErrors=true` confirms that nothing at all was promoted to an
error, which is the stronger statement — had any nullable-flow warning appeared in a file carrying
`#nullable enable`, the flag would have turned it into a build error and the exit code would have
been non-zero.

The five warnings are the same uncoded System.Reactive `packages.config` warnings the baseline
recorded; none is a compiler or nullable-flow diagnostic.

## Acceptance conditions

1. **`EXIT_CODE: 0`.** Recorded above.
2. **`Output Summary:` states that no `CS86` diagnostic was introduced relative to the P0-T7
   baseline enumeration.** Stated above.

## Non-vacuity control

`/t:Rebuild` was used rather than `/t:Build`, verified directly: the build log contains **60**
`CoreCompile:` target executions, so compilation and nullable-flow analysis actually ran. MSBuild's
up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` would have
returned exit 0 with `CoreCompile` skipped on every project and the gate could not have failed.

`/p:Nullable=enable` was deliberately **not** added. This command is character-for-character the one
in `.github/workflows/ci.yml`. No project carries a `<Nullable>` element and there is no
`Directory.Build.props`, so the property would be a solution-wide opt-in conscripting every file that
has never adopted the `#nullable enable` pragma. Omitting it loses no enforcement over any file that
has opted in.

Two files this change edits, `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs` excepted as
it was not edited, carry no `#nullable enable` pragma, so the new `IFolderSearchHandler` members and
parameters this change adds are outside per-file nullable analysis. That is stated here so the clean
result is not read as stronger evidence than it is: it means no nullable regression was introduced in
a file that had opted in, not that the new members were nullable-analysed.

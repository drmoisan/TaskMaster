# AC5 verification — LoadOpCodes retained, MethodBodyReader_Tests.cs unchanged (Issue #824, task P4-T2)

Timestamp: 2026-09-09T15-48

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $b = git merge-base HEAD origin/main; git add -N .; git status --porcelain --untracked-files=all; Write-Output "---"; git diff --stat $b -- "UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs"'`, issued with a `HEAD`-anchored companion span per the adaptation recorded in `evidence/other/executor-deviations.2026-09-09T15-28.md`, and with the porcelain listing filtered to entries naming the gated path.

EXIT_CODE: 0

## Output Summary

Grep results over `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`:

| Pattern | Count | Required |
|---|---|---|
| `RuntimeHelpers.RunClassConstructor` | 1 | exactly 1 |
| `public static void LoadOpCodes\(\)` | 1 | exactly 1 |

`LoadOpCodes()` is therefore neither removed nor left with an empty body. Its body is the single
statement `RuntimeHelpers.RunClassConstructor(typeof(ILGlobals).TypeHandle);`, and the
`using System.Runtime.CompilerServices;` directive that statement requires was verified present by
P2-T3.

Diff and porcelain observations for
`UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs`:

| Observation | Result |
|---|---|
| `git diff --stat <merge-base> -- <path>` | **empty** |
| `git diff --stat HEAD -- <path>` | **empty** |
| `git status --porcelain --untracked-files=all`, entries naming that path | **none** |
| Re-measured line count | **489** |

Both anchors agree and both are empty, so this gate's acceptance holds exactly as the plan writes
it. The `HEAD`-anchored span is a redundant second observation here rather than a substitution: the
P0-T15 measurement established that this path does not appear in the inherited listing, so the
merge-base form was never at risk of attributing a sibling's change to this feature.

`git add -N .` was run before the diff, so a newly created file would appear in the diff's
name-listing rather than being invisible to it. The porcelain span covers the untracked case
independently.

The line count of 489 is unchanged from the P0-T14 baseline, so the file retains its 11 lines of
headroom against the 500-line cap and its `ILGlobals.LoadOpCodes()` call at line 364 inside
`CreateReader` compiles unchanged against the now-`readonly` fields. No new test was placed in this
file.

# AC9 verification — no synchronisation primitive, MethodBodyReader.cs unchanged (Issue #824, task P4-T6)

Timestamp: 2026-09-09T15-52

Command: a `Grep` for `lock\s*\(|volatile|Lazy<` over `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`, then `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $b = git merge-base HEAD origin/main; git add -N .; git status --porcelain --untracked-files=all; Write-Output "---"; git diff --stat $b -- "UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs"'`, issued with a `HEAD`-anchored companion span per the adaptation recorded in `evidence/other/executor-deviations.2026-09-09T15-28.md`, and with the porcelain listing filtered to entries naming the gated path.

EXIT_CODE: 0

## Output Summary

`Grep` for `lock\s*\(|volatile|Lazy<` over `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`:
**0 matches**.

None of the three rejected designs was partially adopted. The fix is publication-based: the tables
are built in locals and assigned once from the type initializer, whose once-only-with-blocking
behaviour the CLR already guarantees, so no lock, no `Lazy<T>` and no `volatile` modifier is needed
or present.

Observations for `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs`:

| Observation | Result |
|---|---|
| `git diff --stat <merge-base> -- <path>` | **empty** |
| `git diff --stat HEAD -- <path>` | **empty** |
| `git status --porcelain --untracked-files=all`, entries naming that path | **none** |
| Re-measured line count | **299** |

Both anchors agree and both are empty, so this gate's acceptance holds exactly as the plan writes
it. The P0-T15 measurement established that this path does not appear in the inherited listing, so
the merge-base form was never at risk here.

The consumer file is byte-for-byte unchanged. Its two unsynchronised reads keep their exact current
expression form, no lock was introduced on the instruction-decode path, and no `catch` in that file
was broadened. The line count of 299 is unchanged from the P0-T14 baseline.

The reads at those sites are now safe not because they changed but because what they read changed:
the field they dereference can no longer hold a partially populated array at any point in any
execution.

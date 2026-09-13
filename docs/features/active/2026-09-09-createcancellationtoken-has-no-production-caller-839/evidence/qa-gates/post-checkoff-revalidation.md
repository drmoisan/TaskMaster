# Post-check-off toolchain re-validation — issue #839

Timestamp: 2026-09-13T06-23
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; dotnet tool run csharpier check .; exit $LASTEXITCODE'
Command: git status --porcelain -- QuickFiler QuickFiler.Test
EXIT_CODE: 0

## Output Summary

    Checked 1624 files in 4714ms.

Zero `Was not formatted` lines, exit code 0.

The scoped porcelain span over the two source trees prints NOTHING.

That empty result is the assertion this task exists to make: no source file changed between the clean toolchain pass recorded by [P3-T6] and this point, so the twelve acceptance check-offs and the artifacts written after [P3-T12] did not disturb the validated source state. The check-off tasks edit only spec.md, which lies under the feature folder and outside this span, and the formatter confirms the tree it checked is still formatter-clean at the same file count as the [P3-T1] pass.

Both owned source files remain committed at COMMIT-1-SHA 3b6cd70b468603a9b89af7791c4d6b18f3abc019 with no working-tree residue, so the toolchain result recorded by [P3-T6] still describes the tree as it now stands and no loop restart is owed.

## Command-transport note

CMD-FORMAT-CHECK, which the plan writes as the bare invocation `dotnet tool run csharpier check .`, was run wrapped as a single `pwsh -NoProfile -Command` invocation with a `Set-Location` prefix and a trailing `exit $LASTEXITCODE`. This is a transport change forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one; the bare form would have checked the wrong tree. The command semantics are unchanged: the same manifest-pinned CSharpier 1.2.6 resolved through `dotnet tool run` over the same `.` scope, with the exit code propagated unaltered. The `git status` span was addressed to the assigned worktree with a repository-location option; its switches and both pathspecs are exactly as the plan writes them.

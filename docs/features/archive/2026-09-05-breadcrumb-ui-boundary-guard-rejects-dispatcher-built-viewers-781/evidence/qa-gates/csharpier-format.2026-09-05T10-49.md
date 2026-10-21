# Final QC Step 1 — CSharpier format (issue #781)

Timestamp: 2026-09-05T16-53

Task: [P2-T1]

Command: `dotnet tool run csharpier format .`, issued from the repository root inside a
`pwsh -NoProfile -Command` process.

EXIT_CODE: 0

## Output Summary

Final summary line, quoted verbatim:

`Formatted 71772 files in 57280ms.`

The exit code alone cannot distinguish a clean run from a repairing one, which is why the
summary line and the two tree observations below are all recorded.

### Porcelain capture before the run

`git status --porcelain --untracked-files=all` reported **29** entries: 11 tracked modifications
(`M`) or additions (`A`) and 18 untracked (`??`) paths. The tracked entries were the four Write
Set source paths (`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`,
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`,
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`,
`QuickFiler.Test/QuickFiler.Test.csproj`), the feature `issue.md` and `plan.2026-09-05T10-49.md`,
`artifacts/orchestration/orchestrator-state.json` (pre-existing at session start and not touched
by this plan), and four agent-memory files under `.claude/agent-memory/`. The untracked entries
were this plan's evidence artifacts and four agent-memory notes.

### Porcelain capture after the run

`git status --porcelain --untracked-files=all` reported the **same 29 entries with the same
status codes in the same order**. The two captures do not differ, so no file changed its tracked
status and no previously clean path was rewritten into a dirty one.

### What the formatter changed

The formatter did rewrite content inside one already-dirty path:
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`, where a lambda assignment
that had been written across three lines was collapsed onto one. That path is inside the Write
Set, so the [P2-T1] acceptance condition is satisfied. No path outside the Write Set was
rewritten, so no revert-and-rerun was required.

This result is consistent with the baseline recorded in
`FEATURE/evidence/baseline/csharpier-check.2026-09-05T10-49.md`, where `csharpier check .`
exited 0 with zero files needing formatting before any edit in this plan was made. The
repository carried no pre-existing formatting drift, so the only content this run could repair
was this plan's own new file.

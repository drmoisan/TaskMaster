# Phase 5 Step 1 — Format (CSharpier), Final Toolchain Loop

Recorded by `[P5-T2]`, with `[P5-T3]` and `[P5-T4]` appended below. This artifact records the
Revision R7 re-execution of the loop. The earlier pass is superseded: `[P4-T13]` and `[P4-T14]`
changed compiled source in `UtilitiesCS.Test` after it ran, and each attempt overwrites its own
artifact.

Timestamp: 2026-09-14T12-47

Build lock: ACQUIRED 879 at 2026-09-14T12:47:13, RELEASED by 879 at 2026-09-14T12:47:22.

Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree-root>
dotnet tool run csharpier format .'`

EXIT_CODE: 0

Output Summary:

```
Formatted 1639 files in 2187ms.
FORMAT_EXIT=0
```

The file count rose from 1638 in the superseded pass to 1639, which is the one file Revision R7
adds. This is a write-mode command whose exit code is identical whether it rewrote files or
not, so the acceptance condition observes the tree rather than the exit code. The tree
observation below is that observation.

Tree Observation:

Command: `git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs/features"`

```
 M UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs
```

The single listed path is item 14 of `## Authorised Write Set`, the Revision R7 sibling test
file created by `[P4-T13]`. No other tracked file was rewritten, so no `git checkout --`
restoration was required and the loop was not restarted. The two files the superseded pass
listed, `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` and
`TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs`, were already at the formatter's fixpoint
from that pass and are absent here, which is the expected result.

The span is repository-wide rather than scoped to the four source directories, excluding only
`.claude` and `docs/features`, which are the two path classes the inherited-path rule places
outside every scope assertion in this plan.

The rewrite is confined to line endings. `git diff -- UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackEdgeCaseTests.cs`
and `git diff --stat` on the same path both return no output, so git's normalized content is
unchanged; the file is listed by porcelain because CSharpier rewrote it with CRLF terminators
against the LF content recorded in the index. The line count is unchanged at 284 before and
after the format pass.

The formatter's rewrite of a write-set file is the anticipated outcome of a write-mode step and
is not a restart trigger. The restart trigger this task states is a formatter rewrite of a file
outside the write set, which did not occur. That the tree now sits at the formatter's fixpoint
is established independently by the read-only `check` run recorded below, which reports zero
unformatted files; a further `format` invocation would therefore rewrite nothing.

## Step 1 Verification (recorded per `[P5-T3]`)

Build lock: ACQUIRED 879 at 2026-09-14T12:47:58, RELEASED by 879 at 2026-09-14T12:48:10.

Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree-root>
dotnet tool run csharpier check .'`

Check EXIT_CODE: 0

```
Checked 1639 files in 5115ms.
CHECK_EXIT=0
```

Unformatted file count reported: 0. CSharpier 1.2.6 reports each unformatted file individually
and prints none here, and the exit code of the read-only `check` subcommand distinguishes a
clean tree from a drifted one.

## Config Numstat (recorded per `[P5-T4]`)

Commands, in this order:

```
git add -- TaskMaster/app.config
git diff --numstat origin/main...HEAD -- TaskMaster/app.config
git diff --numstat --cached -- TaskMaster/app.config
```

Merge-base diff output:

```
4	0	TaskMaster/app.config
```

Staged diff output:

```
NONE
```

The `git add` span is the companion the name-listing diff needs. The staged diff is empty
because the Phase 3 edit to `TaskMaster/app.config` is already committed on this branch, so the
whole of the change is visible to the merge-base diff and nothing remains in the
not-yet-committed state the second diff covers. The union of the two outputs therefore reports:

- deletion count for `TaskMaster/app.config`: 0
- addition count for `TaskMaster/app.config`: 4

The deletion count of 0 is the acceptance condition: CSharpier rewrote no existing line of that
file in this pass either. The addition count of 4 lies within the permitted range of 1 to 12
inclusive and is the `dependentAssembly` block `[P3-T4]` added. No rewrite of the new block was
required and the loop was not restarted.

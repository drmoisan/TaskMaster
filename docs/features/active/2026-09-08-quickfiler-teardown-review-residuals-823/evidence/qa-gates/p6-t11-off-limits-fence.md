# Phase 6 — Off-limits fence

Timestamp: 2026-09-09T14-54

Task: [P6-T11]

Command: `git diff --name-only d636b0f28f548181685260d929de6d7d2940d1da...HEAD`
Command: `git status --porcelain --untracked-files=all`

The 40-character SHA is transcribed from the `BASE-SHA:` field of
`evidence/baseline/p0-t2-branch-and-base.md` per D2. The porcelain span is present because a
name-listing diff cannot report an untracked path. The union of the two spans was tested against
the D20 off-limits list, against the two path classes `.claude/` and `docs/features/epics/`, and
against the single path `CLAUDE.md`.

EXIT_CODE: 0

## The union

The diff span lists nine source paths, the issue-812 `spec.md`, and this feature's own committed
evidence artifacts. The porcelain span lists this feature's plan file as modified and eleven of
this feature's own evidence artifacts as untracked. No path in either span lies outside
`UtilitiesCS/`, `UtilitiesCS.Test/`, `QuickFiler/`, `QuickFiler.Test/`,
`docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/`
or `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/`.

## The thirteen concrete D20 off-limits paths

Neither span lists any of them. Both spans were additionally re-run scoped to the thirteen
pathspecs together and both printed nothing.

UNCHANGED: QuickFiler/Controllers/QfcItemController.FolderHandling.cs
UNCHANGED: QuickFiler/Controllers/QfcHomeController.cs
UNCHANGED: QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
UNCHANGED: UtilitiesCS/Threading/ProgressViewer.cs
UNCHANGED: UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs
UNCHANGED: UtilitiesCS/NewtonsoftHelpers/SDIL Reader/
UNCHANGED: UtilitiesCS.Test/Properties/AssemblyInfo.cs
UNCHANGED: UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs
UNCHANGED: UtilitiesCS/Threading/TimeOutTask.cs
UNCHANGED: UtilitiesCS/Extensions/DfDeedle.cs
UNCHANGED: UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs
UNCHANGED: .editorconfig
UNCHANGED: BannedSymbols.txt

## The three path classes

DOTCLAUDE-DIFF-PATHS: NONE
DOTCLAUDE-UNTRACKED-PATHS: NONE
DOTCLAUDE-MODIFIED-PATHS: NONE

The three fields are kept apart because AC26's two clauses are not the same test: the diff clause
forbids a committed `.claude/` path, the porcelain clause forbids an untracked addition under
`.claude/`, and a modification to a file that was already tracked at [P0-T2] falls outside both.
All three are empty here.

D25 anticipates that the executing agent's own persistent-memory writes would land under
`.claude/agent-memory/` and be reported in one of these fields. No such write was made during this
run. That was a deliberate decision recorded here rather than an accident: AC26 forbids any changed
path beginning `.claude/`, and a memory write would have forced AC26 to `NOT MET` for a reason
unrelated to the delivery. The decision is that the delivery's footprint accounting takes
precedence over the agent's own note-keeping, and no `.claude/` path was touched.

CLAUDE-MD-PRESENT: NO
EPIC-PATHS: NONE
CSPROJ-PATHS: NONE

`CSPROJ-PATHS: NONE` is established by inspecting every path in the union: none ends in `.csproj`.
That is consistent with D20, which records that all three target test files already carry
`Compile Include` entries, so this plan adds no new source file and edits no project file.

No `.claude/` path outside `.claude/agent-memory/` appeared, so the D21 stop-and-report branch was
not taken.

Output Summary: All thirteen concrete off-limits paths are unchanged in both spans. No `.claude/`
path, no `docs/features/epics/` path, no `CLAUDE.md` and no `.csproj` appears in the committed diff
or in the porcelain status.

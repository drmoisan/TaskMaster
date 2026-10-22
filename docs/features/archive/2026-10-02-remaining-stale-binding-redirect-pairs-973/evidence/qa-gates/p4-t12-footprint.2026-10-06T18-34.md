# P4-T12 footprint and hygiene (AC16, AC15, AC6, AC19, AC21, AC22, AC23)

Timestamp: 2026-10-06T18-34
Command: git -C <execution-worktree-root> diff --name-only a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- . ; git -C <execution-worktree-root> status --porcelain --untracked-files=all ; git -C <execution-worktree-root> diff --name-only 993fdd01566dee82e5f37acb761a600feaaa1454 -- . (CMD-FOOTPRINT), plus the scoped diff, status, numstat, ls-files --eol, BOM and Grep clauses listed below
EXIT_CODE: 0
Output Summary: The evaluated footprint, after removing the agent-memory and feature-folder paths, is exactly the 33 section 5 repository files (positive control 33 of 33; no path outside the Write Set union). The BASE-SHA companion, after removing the INHERITED list and feature-folder paths, gives the same 33-path set. HOST-PATH-RESIDUALS: 0. Every scoped diff, porcelain, numstat, line-count, EOL, BOM, netstandard and Microsoft.Graph clause holds. No DIRECTIVE-RESTORED line exists, so RESTORE-ADJUSTED does not apply.

## CMD-FOOTPRINT

PLAN-START-HEAD: a6915d62fe9d85218e5453fc5ac5cd5674b04984
BASE-SHA: 993fdd01566dee82e5f37acb761a600feaaa1454 (P0-T2 MERGE-BASE)
MERGES-SINCE-PLAN-START: none (git log --merges PLAN-START-HEAD..HEAD printed nothing)

Capture 1, git diff --name-only PLAN-START-HEAD -- . (paths outside the feature folder, verbatim; every remaining line is a path under docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/):

    CLAUDE.md
    QuickFiler.Test/app.config
    QuickFiler/QuickFiler.csproj
    QuickFiler/app.config
    QuickFiler/packages.config
    Tags.Test/app.config
    Tags/app.config
    TaskMaster.Test/app.config
    TaskMaster/TaskMaster.csproj
    TaskMaster/app.config
    TaskMaster/packages.config
    TaskTree.Test/app.config
    TaskTree/app.config
    TaskVisualization.Test/app.config
    TaskVisualization/app.config
    ToDoModel.Test/app.config
    ToDoModel/ToDoModel.csproj
    ToDoModel/app.config
    ToDoModel/packages.config
    UtilitiesCS.Test/UtilitiesCS.Test.csproj
    UtilitiesCS.Test/app.config
    UtilitiesCS.Test/packages.config
    UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.ConditionalEngine.cs
    UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs
    UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs
    UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs
    UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs
    UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs
    UtilitiesCS/UtilitiesCS.csproj
    UtilitiesCS/app.config
    UtilitiesCS/packages.config
    VBFunctions.Test/app.config
    tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1

Capture 2, git status --porcelain --untracked-files=all: (empty; every artifact up to P4-T11 was committed before this capture)

EVALUATED-SET (captures 1 and 2 united, `.claude/agent-memory/` paths removed, feature-folder paths removed): the 33 paths above
WRITE-SET-POSITIVE-CONTROL: 33 of 33 (15 app.config, 5 packages.config, 5 csproj, 6 .cs, 1 test file, CLAUDE.md; each matches a section 5 entry)
OUTSIDE-WRITE-SET: none

Companion, git diff --name-only BASE-SHA -- .: the 33 paths above, plus the 23 `.claude/agent-memory/` paths and the 12 other paths of the P0-T2 INHERITED list (11 feature-folder files and docs/features/potential/promoted/2026-10-02-remaining-stale-binding-redirect-pairs.md), plus the feature-folder paths of capture 1. With the INHERITED list subtracted and feature-folder paths removed, the result is the same 33 paths.
EVALUATED-SETS-EQUAL: True

## Host-path residuals

Grep tool, case-insensitive, pattern `[A-Za-z]:[\\/]+Users[\\/]|/c/Users[\\/]`, path docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973 (plan included): no match
HOST-PATH-RESIDUALS: 0

## Scoped clauses

Pathspec note: the first `*.md` capture of this task was issued unquoted, and Bash expanded the glob against the session directory before git ran. That capture was discarded. Every wildcard pathspec below was issued single-quoted, so git received the pattern literally. The quoted `*.cs` diff and porcelain reproduce the P4-T10 captures exactly, and the quoted porcelain over the P4-T11 path set (with `*packages.config` and `*app.config` added) is empty.

git diff --name-only PLAN-START-HEAD -- '*.cs':

    UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.ConditionalEngine.cs
    UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs
    UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs
    UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs
    UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs
    UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs

(exactly the six section 5 .cs paths; UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs absent)
git status --porcelain --untracked-files=all -- '*.cs': (empty)

git diff --name-only PLAN-START-HEAD -- SVGControl/app.config SVGControl.Test/app.config scripts/dependencies: (empty)
git status --porcelain --untracked-files=all -- SVGControl SVGControl.Test scripts/dependencies: (empty)

git diff --name-only PLAN-START-HEAD -- '*.md': CLAUDE.md plus feature-folder paths only (no `.claude/agent-memory/` path appears, so none was removed)
git status --porcelain --untracked-files=all -- '*.md': (empty)

git diff --name-only PLAN-START-HEAD -- .claude: (empty)

git diff --name-status PLAN-START-HEAD -- '*.ps1' '*.psm1' '*.psd1':

    M	tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1

(no A row)
git status --porcelain --untracked-files=all -- '*.ps1' '*.psm1' '*.psd1' '*.xml' '*.trx' '*.coverage': (empty)
git diff --name-only PLAN-START-HEAD -- '*.xml' '*.trx' '*.coverage': (empty)

## CMD-NUMSTAT at PLAN-START-HEAD

    1	1	CLAUDE.md
    9	0	UtilitiesCS.Test/UtilitiesCS.Test.csproj
    1	98	UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs
    0	1	UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs
    0	1	UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs
    0	1	UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs
    0	1	UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs
    10	0	UtilitiesCS/UtilitiesCS.csproj

RESTORE-ADJUSTED: none (no DIRECTIVE-RESTORED line in P4-T5 or P4-T6)

## Category files (AC23)

CategoryClassifierGroup.cs: LINECOUNT 442, CRCOUNT 442 (under 500; equal to the P3-T24 after-value)
CategoryClassifierGroup.ConditionalEngine.cs: LINECOUNT 106, CRCOUNT 106 (under 500; equal to the P3-T24 after-value)

## Microsoft.Graph (AC22)

Grep `name="Microsoft\.Graph(\.Core)?"` -A 1 over glob `*/app.config`: 17 blocks, block for block equal to the P0-T20 GRAPH-REDIRECT capture. Fifteen are Microsoft.Graph.Core `0.0.0.0-4.0.1.0 -> 4.0.1.0`, one in each of the fifteen Write Set configs. The other two are Microsoft.Graph `0.0.0.0-6.7.0.0 -> 6.7.0.0`, in UtilitiesCS.Test/app.config and UtilitiesCS/app.config.
Grep `Include="Microsoft\.Graph(\.Core)?, ` -n over glob `*/*.csproj`, equal to the four fact 14 lines:

    UtilitiesCS.Test/UtilitiesCS.Test.csproj:678 <Reference Include="Microsoft.Graph, Version=6.7.0.0, ...">
    UtilitiesCS.Test/UtilitiesCS.Test.csproj:682 <Reference Include="Microsoft.Graph.Core, Version=4.0.1.0, ...">
    UtilitiesCS/UtilitiesCS.csproj:131 <Reference Include="Microsoft.Graph, Version=6.7.0.0, ...">
    UtilitiesCS/UtilitiesCS.csproj:135 <Reference Include="Microsoft.Graph.Core, Version=4.0.1.0, ...">

## Test file

tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1: LINECOUNT 402, CRCOUNT 402 (under 500)
CMD-HUNKS at PLAN-START-HEAD: HUNK @@ -293,18 +293,2 @@ ; HUNK @@ -331,2 +315,3 @@ ; HUNK @@ -334,0 +320,82 @@ (old-start minimum 293)

## The 15 Write Set configs

CONFIG <path> numstat=<a>/<d> (section 8 post-P3 expectation) eol bom

    Tags/app.config 14/18 (P=13, A yes: 14/18) w/crlf 239,187,191
    TaskTree/app.config 14/18 (14/18) w/crlf 239,187,191
    TaskVisualization/app.config 14/18 (14/18) w/crlf 239,187,191
    QuickFiler/app.config 13/17 (P=12, A yes: 13/17) w/crlf 239,187,191
    TaskMaster/app.config 13/17 (13/17) w/crlf 239,187,191
    ToDoModel/app.config 13/17 (13/17) w/crlf 239,187,191
    UtilitiesCS/app.config 1/5 (P=0, A yes: 1/5) w/crlf 239,187,191
    VBFunctions.Test/app.config 10/14 (P=9, A yes: 10/14) w/crlf 239,187,191
    Tags.Test/app.config 9/9 (P=8, A no: 9/9) w/crlf 239,187,191
    TaskTree.Test/app.config 9/9 (9/9) w/crlf 239,187,191
    QuickFiler.Test/app.config 10/14 (10/14) w/crlf 239,187,191
    TaskMaster.Test/app.config 10/14 (10/14) w/crlf 239,187,191
    TaskVisualization.Test/app.config 10/14 (10/14) w/crlf 239,187,191
    ToDoModel.Test/app.config 10/14 (10/14) w/crlf 239,187,191
    UtilitiesCS.Test/app.config 2/6 (P=1, A yes: 2/6) w/crlf 239,187,191

Every BOM equals P0-T5 (239,187,191 for all fifteen; evidence/baseline/p0-t5-eol-baseline.2026-10-03T10-43.md).

## TaskMaster/app.config netstandard

Grep `name="netstandard"` -A 1: line 38 `<assemblyIdentity name="netstandard" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />`, line 39 `<bindingRedirect oldVersion="0.0.0.0-2.1.0.0" newVersion="2.0.0.0" />`
CMD-HUNKS old-starts: 73, 83, 95, 115, 123, 127, 131, 135, 139, 191, 211, 223, 227, 231 (none between 36 and 41)

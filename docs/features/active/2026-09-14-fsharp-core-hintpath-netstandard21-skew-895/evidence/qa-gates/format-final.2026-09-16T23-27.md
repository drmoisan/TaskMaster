# Phase 4 — Toolchain Step 1: Format (Issue #895)

Timestamp: 2026-09-17T01-25
Task: [P4-T5]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: acquired before this task (`ACQUIRED 895`, exit 0) and held across `[P4-T6]`.

`dotnet tool run csharpier format .` is a write-mode command whose exit code is identical on a clean
run and on a repairing run, so the observation recorded here is a content hash of the three `.cs`
files this plan writes, taken before and after, together with the anchored numstat of the tree.

Command: the `[P4-T5]` payload, run inside a WT-PREAMBLE `pwsh -NoProfile -Command` payload.

EXIT_CODE: 0
ExpectedExitCode: 0

## Output Summary:

```
Formatted 1641 files in 5308ms.
FORMAT_EXIT=0
```

`N` in `Formatted N files in` is the processed count, never a rewrite count. It is 1641, which is
the `[P0-T4]` baseline of 1639 plus the two new countable files.

Content hashes:

```
BEFORE TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs 64731012865D036F77E36A42C1410D786C669689E8464E8AC4DC435F20D3FE54
BEFORE TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs 62691388BA550BD117ED51C1B63269CC4B404766A8061F552038BABA239B17EB
BEFORE TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs DB5BB40CE26989C551D626C0AC6C4FFD845630318BF4B302CA5EBD9D981D9F46
AFTER TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs 64731012865D036F77E36A42C1410D786C669689E8464E8AC4DC435F20D3FE54
AFTER TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs 62691388BA550BD117ED51C1B63269CC4B404766A8061F552038BABA239B17EB
AFTER TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs DB5BB40CE26989C551D626C0AC6C4FFD845630318BF4B302CA5EBD9D981D9F46
FORMAT_CHANGED_TREE=False
```

REWRITTEN-COUNT: 0

Each `AFTER` hash equals its `BEFORE` hash, so the formatter rewrote none of the three files. The
anchored numstat of the whole tree is byte-identical before and after, so it rewrote no other
tracked file either.

## Tree Observation:

```
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler/QuickFiler.csproj
 M TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs
 M ToDoModel/ToDoModel.csproj
```

Four paths, each one of the seven Write Set paths:
`QuickFiler/QuickFiler.csproj` (1), `QuickFiler.Test/QuickFiler.Test.csproj` (2),
`ToDoModel/ToDoModel.csproj` (3) and `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`
(7). The remaining three Write Set paths, the two new test files and
`TaskMaster.Test/TaskMaster.Test.csproj`, were committed at `[P1-T8]` and so are correctly absent
from a porcelain listing.

## Acceptance

- `FORMAT_EXIT=0`: yes.
- Every path in `Tree Observation:` is one of the seven Write Set paths: yes, all four.
- `FORMAT_CHANGED_TREE` recorded: `False`.
- `REWRITTEN-COUNT` recorded: 0.

Neither restart branch applies. `REWRITTEN-COUNT` is 0 and `FORMAT_CHANGED_TREE` is `False`, so the
loop does not restart from this step, and no rewritten path lies outside the Write Set, so neither
the `[P0-T4]` drift-list halt nor the restore-and-restart branch is taken. The `[P0-T4]` drift list
was empty (`UNFORMATTED-FILE-COUNT: 0`), which is consistent with a formatter that found nothing to
repair here.

---

## [P4-T6] Toolchain step 1 verification (read-only)

Timestamp: 2026-09-17T01-25
Task: [P4-T6]

Command:

```
pwsh -NoProfile -Command '
<WT-PREAMBLE>
dotnet tool run csharpier check .
$LASTEXITCODE'
```

Check EXIT_CODE: 0

```
Checked 1641 files in 5450ms.
```

Lower bound: the `[P0-T4]` figure is 1639, so the bound is 1641. The observed figure is 1641, so the
bound holds with equality.

CHECKED-DELTA-RESIDUAL: 0

Acceptance: `Check EXIT_CODE: 0` and the lower bound holds. Both hold.

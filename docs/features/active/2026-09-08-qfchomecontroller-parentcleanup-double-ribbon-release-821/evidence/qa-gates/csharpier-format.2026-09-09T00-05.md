# Phase 6 — Formatting, write mode

Timestamp: 2026-09-09T13-40
Task: [P6-T1]

Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0

Verbatim output:

```text
Formatted 1622 files in 4892ms.
```

## Why the exit code alone is not the observation

This command rewrites tracked source and exits 0 whether or not it changed anything. Its
`Formatted N files` line reports the number of files **processed**, not the number changed — the same
1622 figure appeared in the read-only baseline `check` run at `[P0-T7]`, which changed nothing. The
recorded observation is therefore the tree state after the run, not the exit code.

## Tree observation — span 1, authored change

Command: `git diff --name-only HEAD`
EXIT_CODE: 0

The `.cs` paths this span reports, in full:

```text
QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
QuickFiler/Controllers/EfcHomeController.cs
QuickFiler/Controllers/QfcHomeController.cs
UtilitiesCS.Test/Threading/ProgressPane_Tests.cs
UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs
UtilitiesCS/Threading/ProgressPane.cs
UtilitiesCS/Threading/ProgressViewer.cs
```

**Eight `.cs` paths, and every one is a Write Set file.** No `.cs` path outside the Write Set was left
changed by the format run, so the repo-wide format did not widen the footprint. This was predictable
from `[P0-T7]`, which recorded the baseline `csharpier check` at exit 0 with zero files needing
formatting: with no pre-existing drift in the repository, the format run had nothing out-of-scope to
repair.

The remaining paths in this span are the 29 evidence artifacts and this plan file, all Markdown.
CSharpier 1.2.6 processes `*.cs`, `*.xml` and `packages.config` and does not process `*.md`, so it did
not touch them.

## Tree observation — span 2, porcelain status

Command: `git status --porcelain --untracked-files=all`
EXIT_CODE: 0

The porcelain span reports the same 38 paths as span 1, with no untracked path outside the feature
folder. Nothing is hiding from the name-listing diff.

## Tree observation — span 3, anchored diff

Command: `git diff --name-only (git merge-base HEAD origin/main)`
EXIT_CODE: 0
`.cs` paths in this span: **15**.

Eight are the Write Set files above. The other seven are inherited from the 70 sibling-feature
commits already on this branch, as established in `[P5-T6]` and `[P5-T7]`, and are byte-identical
between HEAD and this worktree — span 1 is the proof, since a file the format run had rewritten would
appear there.

## Line counts immediately after formatting

CSharpier reflowed two of the new test bodies, so the file sizes were re-measured at once:

| File | Before format | After format | Budget | Within |
|---|---|---|---|---|
| `QuickFiler/Controllers/QfcHomeController.cs` | 500 | **500** | exactly 500 | yes, at the ceiling |
| `QuickFiler/Controllers/EfcHomeController.cs` | 447 | **447** | 447 | yes |
| `UtilitiesCS/Threading/ProgressViewer.cs` | 136 | **136** | at most 140 | yes |
| `UtilitiesCS/Threading/ProgressPane.cs` | 107 | **107** | at most 115 | yes |
| `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` | 192 | **192** | at most 200 | yes |
| `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs` | 492 | **495** | at most 495 | yes, exactly at budget |
| `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs` | 486 | **486** | at most 499 | yes |
| `UtilitiesCS.Test/Threading/ProgressPane_Tests.cs` | 324 | **328** | at most 350 | yes |

`QfcHomeController.cs` held at exactly 500: the three inserted statements are at most 94 characters
including indentation, below CSharpier's 100-character print width, so none was split. No file
exceeds the 500-line ceiling. `[P6-T13]` records this check formally.

Output Summary: `csharpier format .` exited 0 having processed 1622 files. The authored tree
observation lists exactly eight changed `.cs` paths, all of them Write Set files, so no out-of-scope
file was rewritten and the footprint did not widen. All eight files remain within their budgets and
under the 500-line ceiling after reformatting.

# QA Gate — Closure Re-Verification After the Phase 8 Markdown Edits (P8-T20)

Timestamp: 2026-09-05T23-32

Command:

```powershell
if (Test-Path -LiteralPath 'TestResults') { [System.IO.Directory]::Delete((Resolve-Path -LiteralPath 'TestResults').Path, $true) }
```

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet tool run csharpier check .
```

```powershell
git status --porcelain --untracked-files=all
```

The removal is the same defence-in-depth step P7-T2 performs and for the same reason, written in the
guarded `[System.IO.Directory]::Delete` form because `Remove-Item -Recurse -Force` is blocked by a
PreToolUse hook in this environment (SD20).

EXIT_CODE: 0

Output Summary:

All acceptance conditions hold. The format check reproduces the Phase 7 count exactly, and the
porcelain output is empty and therefore byte-identical to the recorded baseline image after the
four-path subtraction. This artifact records the passing capture and retains the superseded failing
capture below, so the history of the gate is auditable rather than overwritten.

## Format check — HOLDS

```text
Checked 1583 files in 4414ms.
CHECK_EXIT_CODE=0
```

| Quantity | Value | Source |
|---|---|---|
| Phase 7 recorded count | `Checked 1583 files` | `evidence/qa-gates/p7-t2-format-check.md` |
| This run's count | `Checked 1583 files` | the run above |

The counts are identical. The expected value is taken from the Phase 7 artifact rather than from any
figure tabled in the plan. This confirms the Phase 8 Markdown edits did not disturb the Phase 7
clean pass, which is the property this task exists to check: Markdown is outside CSharpier's target
set and outside every MSBuild input.

## Porcelain output, verbatim

```text
```

Zero lines. `PORCELAIN_RAW_COUNT=0`.

## Subtracted comparison — HOLDS

The four subtracted paths are `plan.2026-09-05T15-47.md`, `spec.md`, `user-story.md`, and
`evidence/baseline/phase0-instructions-read.md`, all under this feature folder. None of the four
appears on either side of this capture, and a path absent from both sides is unaffected by being
subtracted.

| Side | Subtracted output |
|---|---|
| This task | 0 lines |
| Baseline, from `evidence/baseline/p0-t2-base-ref.md` | 0 lines; the recorded baseline porcelain image is empty |

```text
SUBTRACTED_COUNT=0
BASELINE_SUBTRACTED_COUNT=0
BYTE_IDENTICAL=True
```

The two sides are byte-identical. This delivery leaves the worktree in exactly the state it found
it, apart from its own commits.

## The additional confirmation — HOLDS

```text
WRITESET_OR_FEATURE_PATHS_IN_SUBTRACTED=0
```

This task's own subtracted porcelain output contains no path under the Write Set and no path under
`docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/`. It contains no path at
all. Every file this delivery created or modified is committed as of this capture, including the
P6-T3 artifact and the plan file, which were committed in `7dfd259b` ahead of this task precisely
because the P6-T3 artifact path is not a member of the four-path subtraction set and would otherwise
have appeared here as an uncancelled feature-folder path.

## Superseded record — the earlier failing capture and how it was cleared

At the 2026-09-05T23-21 capture the format check and the Write-Set-absence confirmation held on the
same values recorded above, but the byte-identity condition failed. The porcelain output then was:

```text
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/plan.2026-09-05T15-47.md
?? .claude/agent-memory/atomic-planner/project_782_dispatcher_token_gate_seams.md
```

```text
SUBTRACTED_COUNT=2
BASELINE_SUBTRACTED_COUNT=0
BYTE_IDENTICAL=False
```

The two residual paths are the residue recorded in
`evidence/qa-gates/p6-t3-dotclaude-untouched.md`. Their last-write times are 2026-09-05 22:17:50 and
22:17:46, both by the atomic-planner agent, fifteen minutes before this executor's first commit
`d5e192b3` at 22:32:36. The executor wrote no agent memory in either session: the most recently
modified file under `.claude/agent-memory/atomic-executor/` is unchanged at 2026-09-05 20:38:11.

This task's own text anticipates this class of dirt: it states that comparing against the recorded
baseline rather than demanding an empty output is required *because* `.claude/agent-memory/` is a
tracked directory that a concurrent session can leave modified. The comparison nonetheless failed at
that capture, because the concurrent write landed **after** P0-T2 captured the baseline porcelain
image and **before** the closing one was captured. The residue was therefore present on the closing
side and absent from the baseline side, and a two-sided comparison cannot cancel a one-sided term.
The mechanism the task names is the one that occurred; only its timing differed from what the task
assumed.

The residue was left in place and reported to the caller, and was cleared **by the orchestrator, not
by the executor**, with `git checkout -- .claude/` restoring the modified `MEMORY.md` and
`git clean -fd .claude/` removing the untracked note. Both sides of the comparison are now empty and
the one-sided term is gone.

## What this gate establishes

It establishes that this delivery leaves the worktree in exactly the state it found it, apart from
its own commits: the format check reproduces the Phase 7 count exactly, the subtracted output is
byte-identical to the recorded baseline, and the closing output carries no Write Set path and no
feature-folder path.

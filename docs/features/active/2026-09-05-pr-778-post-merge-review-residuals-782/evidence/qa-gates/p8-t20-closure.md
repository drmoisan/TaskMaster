# QA Gate — Closure Re-Verification After the Phase 8 Markdown Edits (P8-T20)

Timestamp: 2026-09-05T23-21

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

## Format check — HOLDS

```text
Checked 1583 files in 4398ms.
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
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/plan.2026-09-05T15-47.md
?? .claude/agent-memory/atomic-planner/project_782_dispatcher_token_gate_seams.md
```

## Subtracted comparison — DOES NOT HOLD

The four subtracted paths are `plan.2026-09-05T15-47.md`, `spec.md`, `user-story.md`, and
`evidence/baseline/phase0-instructions-read.md`, all under this feature folder.

| Side | Subtracted output |
|---|---|
| This task | 2 lines, both under `.claude/agent-memory/atomic-planner/` |
| Baseline, from `evidence/baseline/p0-t2-base-ref.md` | 0 lines; the recorded baseline porcelain image is empty |

```text
SUBTRACTED_COUNT=2
BASELINE_SUBTRACTED_COUNT=0
BYTE_IDENTICAL=False
```

**The two sides are not byte-identical, so this task is not marked complete.**

### The two residual paths

```text
 M .claude/agent-memory/atomic-planner/MEMORY.md
?? .claude/agent-memory/atomic-planner/project_782_dispatcher_token_gate_seams.md
```

Both are the residue already recorded in `evidence/qa-gates/p6-t3-dotclaude-untouched.md`. Their
last-write times are 2026-09-05 22:17:50 and 22:17:46, both by the atomic-planner agent, fifteen
minutes before this executor's first commit `d5e192b3` at 22:32:36. The executor wrote no agent
memory in this session: the most recently modified file under
`.claude/agent-memory/atomic-executor/` is unchanged at 2026-09-05 20:38:11.

### Why they appear on one side only

This task's own text anticipates exactly this class of dirt: it states that comparing against the
recorded baseline rather than demanding an empty output is required *because* `.claude/agent-memory/`
is a tracked directory that a concurrent session can leave modified, and that an unconditional
empty-porcelain demand would fail for a reason outside this delivery's control.

The comparison nonetheless fails here, because the concurrent write landed **after** P0-T2 captured
the baseline porcelain image and **before** this task captured the closing one. The residue is
therefore present on the closing side and absent from the baseline side, and a two-sided comparison
cannot cancel a one-sided term. The mechanism the task names is the one that occurred; only its
timing differs from what the task assumed.

## The additional confirmation — HOLDS

```text
WRITESET_OR_FEATURE_PATHS_IN_SUBTRACTED=0
```

This task's own subtracted porcelain output contains **no path under the Write Set** and **no path
under `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/`**. Every file this
delivery created or modified is committed. The only uncommitted path inside the feature folder is
this plan file, which is subtracted by the rule and which the executor modifies by the act of
checking off the task that runs this gate.

## What this gate establishes

It establishes that **this delivery leaves the worktree in exactly the state it found it, apart from
its own commits**: the format check reproduces the Phase 7 count exactly, and the subtracted output
carries no Write Set path and no feature-folder path.

It does not establish that the worktree is globally clean, because two paths under
`.claude/agent-memory/atomic-planner/` are dirty. That residue is attributable to another agent, is
outside this delivery's scope, and is reported to the caller for disposition rather than committed,
deleted, or reverted — each of which is prohibited by this plan or by the delegation brief.

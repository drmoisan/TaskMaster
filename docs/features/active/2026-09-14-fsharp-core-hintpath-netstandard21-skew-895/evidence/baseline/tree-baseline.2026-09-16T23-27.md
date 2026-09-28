# Phase 0 — Tree Baseline and Diff Anchors (Issue #895)

Timestamp: 2026-09-17T01-16
Task: [P0-T8]
WORKTREE-LEAF: agent-a8bc4dc5978785885

The anchor for every diff, footprint and containment gate in this plan is `origin/main` after an
explicit fetch, never local `main`. In a worktree-per-item run only the pulling checkout advances
local `main`, so a gate anchored there would be vacuous or unsatisfiable by construction.

Commands, in the order run:

```
git fetch origin
git rev-parse --verify origin/main
git merge-base HEAD origin/main
git rev-parse HEAD
git diff --numstat origin/main -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj ToDoModel/ToDoModel.csproj UtilitiesCS/UtilitiesCS.csproj UtilitiesCS.Test/UtilitiesCS.Test.csproj ToDoModel.Test/ToDoModel.Test.csproj
git diff --name-only origin/main...HEAD
git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs/features"
git status --porcelain --untracked-files=all
```

EXIT_CODE: 0 (`git fetch origin` exit 0; `git rev-parse --verify origin/main` exit 0)
ExpectedExitCode: 0

ORIGIN-MAIN-SHA: 91746d2e4776a59ee1db1856c5c490a009c4958b
MERGE-BASE-SHA: 91746d2e4776a59ee1db1856c5c490a009c4958b
HEAD-SHA: f1c53f9b88fddb4c69920ff37b3acb13718f7983

These three SHAs are recorded as observations. None of them is a plan expectation, and none is
gated. The merge base equals `origin/main`, so at this point the two-dot and three-dot diff forms
coincide.

## Baseline Six-File Numstat:

NONE

The command printed no output. No HintPath-bearing project file differs from `origin/main` at
baseline, so AC3's "exactly one changed line each" clause is satisfiable as worded and the
stop-and-report branch of this task was not taken.

## INHERITED-CLAUSE-A:

```
.claude/agent-memory/atomic-executor/project_plan_line_locators_stale_after_doc_edit.md
.claude/agent-memory/atomic-planner/MEMORY.md
.claude/agent-memory/atomic-planner/project_895_fsharp_core_hintpath_plan_seams.md
.claude/agent-memory/prd-feature/MEMORY.md
.claude/agent-memory/prd-feature/reference_repo_walking_tests_exclude_claude_worktrees.md
.claude/agent-memory/task-researcher/MEMORY.md
.claude/agent-memory/task-researcher/project_fsharp_core_hintpath_skew_895.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/issue.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/plan.2026-09-16T23-27.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/research/2026-09-16T23-30-fsharp-core-hintpath-research.md
docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md
```

Eleven paths, all of them either under the `.claude/agent-memory/` prefix (clause B) or under this
issue's own feature-folder prefix. This is the captured clause-A set that the `[P4-T14]`
`OUT-OF-WRITE-SET:` gate subtracts. No Write Set path appears in it.

## Baseline Porcelain Status:

NONE

The excluded-pathspec porcelain span (`. ":(exclude).claude" ":(exclude)docs/features"`) printed no
output, so no path ending `.cs`, `.csproj`, `packages.config` or `app.config` is modified or
untracked outside the two excluded prefixes. The bootstrap steps wrote only into git-ignored
directories (`packages/`, `.dotnet-sdk/`, `coverage/`, `TestResults/`).

## Baseline Porcelain Full:

```
 M docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/plan.2026-09-16T23-27.md
?? docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/analyzer-baseline.2026-09-16T23-27.md
?? docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/channel-probe.2026-09-16T23-27.md
?? docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/format-baseline.2026-09-16T23-27.md
?? docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/nullable-baseline.2026-09-16T23-27.md
?? docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/phase0-instructions-read.2026-09-16T23-27.md
?? docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/test-coverage-baseline.2026-09-16T23-27.md
?? docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/toolchain-bootstrap.2026-09-16T23-27.md
```

This is the output as observed when the command ran, which was before this artifact and the
`[P0-T9]` census had been written; those two files are therefore not listed in it.

The modified plan file carries this run's task check-offs; the untracked files are the Phase 0
evidence artifacts written by `[P0-T1]` through this task. Both are expected and are committed by
`[P0-T10]`.

## Output Summary:

`origin/main` resolved and the fetch succeeded; the six HintPath-bearing project files are
byte-identical to `origin/main`; eleven inherited paths were captured as clause A; the source-tree
porcelain span is empty.

---

## Source Census:

Task: [P0-T9]
Timestamp: 2026-09-17T01-17
EXIT_CODE: 0
ExpectedExitCode: 0

Payload: the `[P0-T9]` census block, run inside a WT-PREAMBLE `pwsh -NoProfile -Command` payload.

Transport note on the final line. The plan's `CSPROJ_FILES` expression filters with the regular
expression `\\(\.[^\\]+|packages|bin|obj|node_modules)\\`. The Bash tool's argument layer collapses
each doubled backslash to a single one before `pwsh` receives the payload, which turns `[^\\]` into
`[^\]` and makes the character class unterminated; PowerShell then raised
`Invalid pattern ... Unterminated [] set.` once per enumerated file and the count printed as 0. The
measurement was therefore re-run with the identical filter semantics expressed without literal
backslash escapes: the path relative to the repository root is split on `[char]92`, and a file is
kept when none of its directory components begins with `.` or equals `packages`, `bin`, `obj` or
`node_modules`. That is the same predicate the plan states, and the unfiltered enumeration count is
recorded alongside the filtered one so the filter can be seen to be non-vacuous. This is a
transport-layer workaround, not a change to what the plan asserts.

```
QuickFiler/QuickFiler.csproj NS21=1 NS20=0
QuickFiler.Test/QuickFiler.Test.csproj NS21=1 NS20=0
ToDoModel/ToDoModel.csproj NS21=1 NS20=0
UtilitiesCS/UtilitiesCS.csproj NS21=0 NS20=1
UtilitiesCS.Test/UtilitiesCS.Test.csproj NS21=0 NS20=1
ToDoModel.Test/ToDoModel.Test.csproj NS21=0 NS20=1
UNSATISFIABLE_COUNT=2
DISPLAY_NAME_TESTS_COUNT=0
DONOTPARALLELIZE_COUNT=2
TESTMETHOD_COUNT=9
BECAUSE_206_COUNT=1
NETSTANDARDBIND_LINES=466
TASKMASTER_TEST_CSPROJ_LINES=400
BOOTSTRAP_CS_FILES=3
SHAPE_A_PRESENT=False
SHAPE_B_PRESENT=False
CSPROJ_FILES_UNFILTERED=18
CSPROJ_FILES=18
```

The eighteen kept project files are `QuickFiler`, `QuickFiler.Test`, `SVGControl`, `SVGControl.Test`,
`Tags`, `Tags.Test`, `TaskMaster`, `TaskMaster.Test`, `TaskTree`, `TaskTree.Test`,
`TaskVisualization`, `TaskVisualization.Test`, `ToDoModel`, `ToDoModel.Test`, `UtilitiesCS`,
`UtilitiesCS.Test`, `VBFunctions` and `VBFunctions.Test`, each at `<name>/<name>.csproj`.

### Census acceptance

- Three edited files read `NS21=1 NS20=0`: yes (`QuickFiler`, `QuickFiler.Test`, `ToDoModel`).
- Three untouched files read `NS21=0 NS20=1`: yes (`UtilitiesCS`, `UtilitiesCS.Test`,
  `ToDoModel.Test`).
- `UNSATISFIABLE_COUNT=2`: yes. This is AC5's pre-fix observation. The two occurrences are the
  line-281 `NegativeControl_WithoutInstall_Netstandard21Throws` summary, which stays true after the
  fix and is retained, and the stale sentence at line 366 that `[P3-T1]` replaces.
- `DISPLAY_NAME_TESTS_COUNT=0`: yes; the token `display-name tests` is absent before `[P3-T1]`.
- `DONOTPARALLELIZE_COUNT=2`: yes.
- `TESTMETHOD_COUNT=9`: yes.
- `BECAUSE_206_COUNT=1`: yes; the `because` message at lines 206-207 is present and is never
  touched by this plan.
- `NETSTANDARDBIND_LINES=466`: yes; `[P3-T2]` gates the transition to 470.
- `BOOTSTRAP_CS_FILES=3`: yes; `[P1-T4]` gates the transition to 5.
- `SHAPE_A_PRESENT=False` and `SHAPE_B_PRESENT=False`: yes; neither new file exists yet.
- `CSPROJ_FILES=18`: yes.

Every value matches the plan's expectation, so the tree is the one this plan was authored against
and the stop-and-report branch was not taken.

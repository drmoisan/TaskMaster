# Research: tracked *.csproj.bak files (Issue #951)

Date: 2026-10-01
Work Mode: minor-audit
Requirements source: `docs/features/active/2026-09-30-tracked-csproj-bak-files-carry-stale-project-content-951/issue.md` (AC-1 to AC-5).

## Method and limitation

The research session had only Read, Grep and Glob tools. No `git` execution was available. All findings below were obtained by searching the working tree of the feature worktree checkout, not by running `git grep` against `origin/main`. The working tree is a checkout of the repository; whether it equals `origin/main` byte for byte was not verified. Commands in section 5 are therefore reasoned from git semantics and were not executed. The plan should execute the exact `git grep ... origin/main` and `git ls-files` commands as baseline evidence in Phase 0.

Grep honors `.gitignore`, so only tracked or non-ignored files were searched. The `.bak` files are not ignored today (see section 2).

## 1. References to the .bak files

Search: Grep, pattern `\.bak\b`, whole worktree, excluding `docs/**`.

Hits (complete list):

| Location | Nature |
|---|---|
| `.gitignore:257` | `*.rptproj.bak` ignore rule (not a reader) |
| `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs:412` | `MissingOwnedPath("file-backup.bak")`: a string literal naming a non-existent test path. Unrelated to the project backups. |
| `.claude/agent-memory/task-researcher/project_quickfiler_interface_only_files_433.md:34` | prose (agent memory) |
| `.claude/agent-memory/feature-review/project_635-review-residuals.md:27` | prose (agent memory) |
| `.claude/agent-memory/atomic-planner/project_433_f7_qfchomecontroller_plan_seams.md:14` | prose (agent memory) |
| `.claude/agent-memory/atomic-planner/project_635_reflective_caller_audit_plan_seams.md:41` | prose (agent memory) |

Result: zero hits in `scripts/`, `.github/workflows/`, `.claude/hooks`, `.claude/lib`, `.claude/skills`, `.claude/config`, `tests/`, any `*.csproj`, `*.sln`, `*.targets`, `*.props`, `coverage.config`, `quality-tiers.yml`, `*.ps1`, `*.sh`, `*.py`, or `*.json`. No Get-ChildItem, Include or glob usage of `*.bak` exists outside the `.gitignore` line. The four agent-memory hits are descriptive prose, not functional. Note that `.claude/agent-memory/` is tracked and references the `.bak` files by name; deleting the files does not break those notes functionally, and they must not be edited in this change (AC-5 limits the change set).

Docs hits: Grep count over `docs/` for `\.bak\b` found 157 occurrences across 45 files (plans, specs, research, evidence, audits from features 434, 433, 435, 796, 945, 929, 635, 799, 491, and others). These are historical prose and evidence. Feature 929 evidence (`p0-t1-worktree-anchor`) and its audits are the origin of this issue. None are functional. `docs/features/` edits fall inside the AC-5 permitted set, but historical prose does not need to be changed.

Conclusion: nothing functional reads, copies, globs or references the `.bak` files. AC-4's fixed-string search of `scripts/`, `.github/`, `.claude/lib/`, `*.csproj`, `*.sln`, `*.targets` for `.csproj.bak` is expected to return no match on the end-state tree. The repository hygiene guard (`scripts/hygiene/*`) contains no `bak` token.

Caveat for AC-4 execution: the string `.csproj.bak` will appear in `docs/features/**` (including this research file and `issue.md`), and in `.claude/agent-memory/**`. AC-4 scopes the search to `scripts/`, `.github/`, `.claude/lib/` and the project files, so those do not affect it. Keep the AC-4 path list exactly as written; do not widen it to `.claude/` or `docs/`.

Altcover check (Grep, case-insensitive, `**/*.{csproj,bak,vbproj}`): matches only `QuickFiler.Test/QuickFiler.Test.csproj.bak` (6) and `QuickFiler/QuickFiler.csproj.bak` (6). No live `.csproj` contains `altcover`. This agrees with the issue.

## 2. Ignore rule: `*.csproj.bak` versus `*.bak`

Glob `**/*.bak` in the worktree returns eleven files: the eight `*.csproj.bak` files plus `TaskMaster.sln.bak`, `TaskTree/TaskTree.vbproj.bak`, `TaskVisualization/TaskVisualization.vbproj.bak`.

`.gitignore` contains only `*.rptproj.bak` (line 257). The only `!` negations in the file are at lines 137, 151, 199 and 225 (`.axoCover/settings.json`, `coverage/.gitkeep`, `**/[Pp]ackages/build/`, `?*.[Cc]ache/`). None re-includes any `.bak` path, so a new rule added below them is not overridden.

Semantics of `git ls-files -ci --exclude-standard -- "*.bak"`: lists tracked (cached) files that match an ignore rule. Today the only `.bak` rule is `*.rptproj.bak`, which matches none of the eleven tracked names, so the baseline output is expected to be empty (reasoned, not executed).

- Rule `*.bak`: would match all eleven tracked files. After deleting the eight, the command would still print the three remaining tracked files, so AC-3 (empty output, and the three files unshadowed) would fail.
- Rule `*.csproj.bak`: matches only the eight deleted names. The remaining three (`.sln.bak`, `.vbproj.bak` x2) do not match, so the AC-3 command prints nothing.

Recommendation: add the exact line `*.csproj.bak` (as AC-2 specifies), placed next to `*.rptproj.bak` in the "Backup & report files from converting an old project file" block (line 257). Do not use `*.bak`. The issue's "(or `*.bak`)" and Expected Behavior text ("`.gitignore` excludes `*.bak`") conflict with AC-3; the AC section is authoritative and requires `*.csproj.bak`.

## 3. The three other tracked .bak files

- `TaskMaster.sln.bak`: Grep count of `^Project(` lines is 22 in the backup versus 19 in the live `TaskMaster.sln`. Materially different (stale solution backup with three more project entries). Nothing reads it (section 1).
- `TaskTree/TaskTree.vbproj.bak` and `TaskVisualization/TaskVisualization.vbproj.bak`: no live `TaskTree.vbproj` or `TaskVisualization.vbproj` exists (Glob returned only `TaskTree/TaskTree.csproj` and `TaskMaster.sln` from the checked set). The `.vbproj.bak` files are orphans of VB projects that were converted to C# projects. `TaskTree.vbproj.bak` has 7 `Compile Include` lines. Nothing reads them (section 1).

Report only. Follow-up note: these three could be handled by a separate issue (delete, and widen the ignore rule to `*.sln.bak` and `*.vbproj.bak`), which is outside #951's AC-3 and AC-5 scope.

## 4. The #927 repository hygiene guard

Located. Issue #927 is `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/`. The guard exists in the tree:

- `scripts/hygiene/Test-RepositoryHygiene.ps1`, `scripts/hygiene/Test-RepositoryHygiene.Git.ps1`, `scripts/hygiene/Test-RepositoryHygiene.Rules.ps1`.
- Pester tests: `tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1`, `.Git.Tests.ps1`, `.Rules.Tests.ps1`.
- CI: `.github/workflows/_hygiene.yml` (job "Repository hygiene guard", runs `./scripts/hygiene/Test-RepositoryHygiene.ps1`), wired in `.github/workflows/ci.yml` lines 36-38, described in `.github/workflows/README.md` lines 24, 184, 195.

Current rules (per README line 24 and `Test-RepositoryHygiene.Rules.ps1` functions `Get-UserProfilePathPattern`, `Find-UserProfilePathMatch`, `Get-RawEvidenceDocumentKind`): raw test-platform or coverage-collector documents (classified by content) and Windows user-profile paths. It has no tracked-backup-file rule. Extending it is the optional item in the issue and is out of scope for the plan. If done later, it would need a new rule, tests (with a negative control), and README update.

## 5. End-state check commands (reasoned; not executed)

1. `git ls-files -- "*.csproj.bak"`: pathspec glob (git's `*` matches across `/` in pathspecs, so subdirectory files match). Expected before: eight paths; after deletion from the index: empty. Works on Windows git; quote the pattern so PowerShell passes it literally.
2. `git check-ignore -v -- <path>`: by default `check-ignore` does not report paths that are tracked in the index (it needs `--no-index` for those). Once the eight paths are removed from the index, the command evaluates them purely by pattern, and a file need not exist on disk. Expected output per path: `.gitignore:<line>:*.csproj.bak<TAB><path>` with exit code 0. Run it in a state where the index no longer contains the path (after `git rm`), or add `--no-index` to make it order-independent.
3. `git ls-files -ci --exclude-standard -- "*.bak"`: empty both before (no matching rule) and after the change (the rule matches no tracked file once the eight are removed). Sequence caveat: if `.gitignore` is edited before the eight are untracked, this command prints the eight tracked paths. This is the intended signal that the files are still tracked; it is expected to be empty only at the end state.
4. Negative control (for AC-3): `git check-ignore -v --no-index -- TaskTree/TaskTree.vbproj.bak` should print nothing and exit 1 with the `*.csproj.bak` rule, demonstrating the rule does not shadow the vbproj backup. Likewise `TaskMaster.sln.bak`. Positive control: `git check-ignore -v --no-index -- ToDoModel/ToDoModel.csproj.bak` should print the `.gitignore` line with exit 0 even before deletion. Note `*.rptproj.bak` is not a useful control for these names, since `ToDoModel/ToDoModel.csproj.rptproj.bak` is not an existing path and only tests the pre-existing rule.
5. AC-5: `git diff --name-status origin/main...HEAD` should show only eight `D` entries, `M .gitignore`, and paths under `docs/features/`.

## Recommended approach

Use `git rm` on the eight paths (removes index entry and working-tree file), add `*.csproj.bak` to `.gitignore` at line 257's block, then run the three end-state commands plus the AC-4 fixed-string search. Rejected alternative: `*.bak`, because it shadows the three other tracked backups and fails AC-3. Rejected alternative: leave ignore rule out and delete only, because it permits Visual Studio to recreate backups that could be re-committed.

## Open items for the plan

- Execute the git commands above in Phase 0 for baseline evidence (this research could not run git).
- Verify the worktree base equals `origin/main` before stating the AC-5 diff.
- Do not edit `.claude/agent-memory/**` references to the deleted files.

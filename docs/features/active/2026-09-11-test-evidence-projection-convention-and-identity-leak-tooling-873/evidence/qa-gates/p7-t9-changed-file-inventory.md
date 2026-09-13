# P7-T9 — Changed-File Inventory For The Delivery

Timestamp: 2026-09-13T07-19
Task: [P7-T9]

## Commands

1. `git add -- docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873 scripts/vscode tests/scripts/vscode CLAUDE.md TaskMaster/TaskMaster.csproj .vscode/settings.json .claude/agent-memory`
2. `git diff --name-status refs/base-anchor-873`
3. `git status --porcelain --untracked-files=all`

EXIT_CODE: 0

The staging span is required and is limited to the paths this plan names. A name-listing diff
enumerates tracked changes only, so without it the six files this delivery creates under the two
script folders and the many evidence artifacts it creates under this feature folder would be
invisible to the diff. `git add --all` across the repository was not used, because a blanket add
sweeps unrelated untracked files that other agents leave in this worktree.

The diff is anchored to `refs/base-anchor-873`, which resolves to
`5cc7dcd6330b44e3d2238ffe7fa1e5b3317d9ee1`, so it reports the whole delivery rather than only the
uncommitted remainder.

UNION_PATH_COUNT: 100
DIFF_PATH_COUNT: 100
STATUS_PATH_COUNT: 13

Every path the porcelain status reports is also reported by the diff, because the staging span put
each of them in the index, so the union equals the diff set.

## EXECUTOR_MEMORY_PATHS:

```
none
```

EXECUTOR_MEMORY_PATH_COUNT: 0

No path in the union sits beneath `.claude/agent-memory/atomic-executor/`. The agent-memory carve-out
that P7-T9 provides for is therefore unused on this delivery, and P7-T16's delivery pathspec set is
the union with nothing subtracted. Recorded explicitly rather than omitted, so the carve-out is
auditable as empty rather than silently absent.

Verification: `@($union | Where-Object { $_ -like '.claude/agent-memory/atomic-executor/*' }).Count`
returned 0.

## Project files in the union

```
TaskMaster/TaskMaster.csproj
```

PROJECT_FILE_COUNT: 1

The inventory contains no project file other than `TaskMaster/TaskMaster.csproj`, which P5-T1 names.
This is the load-bearing negative observation for the analyzer-skew hazard and for the Pester
hint-path hazard: fifteen other tracked project files carry a stale `<Analyzer Include>` version and
`tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` executes the repository hint-path synchroniser during
a Pester run, and neither condition left a single project file in this inventory.

## Union by category

| Category | Count | Admitted by |
|---|---|---|
| Beneath this feature folder | 78 | "sits beneath this feature folder" |
| Under `scripts/vscode` | 5 | named as backticked repository-relative paths |
| Under `tests/scripts/vscode` | 7 | named as backticked repository-relative paths |
| Under `.claude/agent-memory`, excluding the executor's own directory | 6 | named as backticked repository-relative paths |
| `.vscode/settings.json` | 1 | named by P5-T2 |
| `CLAUDE.md` | 1 | named by P5-T3 and P5-T4 |
| `TaskMaster/TaskMaster.csproj` | 1 | named by P5-T1 |
| `docs/features/potential/promoted/...` | 1 | not named; see the section below |
| **Total** | **100** | |

### The five script-folder production and part files

```
M	scripts/vscode/Invoke-MSTest.ps1
A	scripts/vscode/Invoke-MSTest.TrxSummary.ps1
M	scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
A	scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1
M	scripts/vscode/Invoke-MSTestWithCoverage.ps1
```

### The seven test files

```
M	tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1
A	tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1
M	tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
A	tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1
M	tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
A	tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1
A	tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1
```

Four created and three repaired, which is the seven-file count the spec's Write Set carries and which
P0-T16 verified.

### The six agent-memory files

```
M	.claude/agent-memory/_shared_no_absolute_host_paths.md
M	.claude/agent-memory/epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md
M	.claude/agent-memory/feature-review/project_464-review-residuals.md
M	.claude/agent-memory/feature-review/project_488-review-residuals.md
M	.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md
M	.claude/agent-memory/orchestrator/collect-pr-context-lands-in-main-checkout.md
```

P5-T5 names the first. P5-T6 through P5-T10 name the other five. None of them is the executor's own
memory directory.

### The three remaining named single files

```
M	.vscode/settings.json
M	CLAUDE.md
M	TaskMaster/TaskMaster.csproj
```

## The one path the acceptance clause does not name

```
R095	docs/features/potential/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling.md	docs/features/potential/promoted/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling.md
```

This is a rename, 95 percent similarity, of the potential-feature document into the `promoted`
subdirectory. It sits neither beneath this feature folder nor beneath the executor's agent-memory
directory, and this plan does not name it as a backticked repository-relative path. It is recorded
here rather than omitted, because an inventory that silently dropped a path it could not classify
would not be an inventory.

Provenance, established rather than asserted:

```
Command: git log --oneline refs/base-anchor-873..HEAD -- docs/features/potential
46b1fb3e4 docs(873): preparation artifacts for test evidence projection convention and identity leak tooling
```

```
Command: git log --oneline refs/base-anchor-873..HEAD
031db9b6f Record the Phase 6 assembly inventory and abort the default-output run on a pre-existing defect (#873 Phase 6, partial)
aa6bf5f30 Record the committed-evidence convention and clear the named identifier leaks (#873 Phase 5)
e88c61879 Wire the plain MSTest entry point to an explicit results directory and trx summary (#873 Phase 4)
03dc4741f feat(873): wire the coverage entry point to the projection, summary and conditional discard
fb65b0f2c feat(873): add the test-result summary part file and its tests
8eddcf27e fix(873): re-measure C# baselines and add the JaCoCo projection part file
456c5647a chore(873): capture Phase 0 baselines, bootstrap worktree, reconcile spec footprint
30430f314 Merge remote-tracking branch 'origin/main' into bug/test-evidence-projection-convention-and-identity-leak-tooling-873
46b1fb3e4 docs(873): preparation artifacts for test evidence projection convention and identity leak tooling
```

The rename was authored by commit `46b1fb3e4`, which is the preparation commit at the base of the
branch. It precedes `456c5647a`, the Phase 0 commit, so it predates every task in this plan. It is
the feature-promotion lifecycle move that created this feature folder in the first place: the
promotion tooling moves the potential-feature entry into `promoted` when it opens the active folder.
No task in this plan performed it, and no Phase 7 action touched it.

Verdict, stated plainly: P7-T9's acceptance clause as written does not admit this path, because the
clause enumerates three admitting conditions and this path meets none of them. The clause was written
against the delivery's own footprint and did not anticipate inherited branch content from the
promotion that created the folder. The path is inherited rather than authored here, it is a
documentation move with no bearing on any gate, and it was already committed before Phase 0 began, so
it is reported as a classified exception rather than treated as a footprint violation. It is carried
in the P7-T16 commit set only to the extent it is already committed; no Phase 7 action stages or
re-stages it.

## Output Summary

UNION_PATH_COUNT: 100, of which 78 sit beneath this feature folder, 12 are the script-folder
production and test files this plan names, 9 are the other single files this plan names, and 1 is the
inherited promotion rename classified above. EXECUTOR_MEMORY_PATHS is empty, so P7-T16's delivery
pathspec set is the union with nothing subtracted. Exactly one project file appears,
`TaskMaster/TaskMaster.csproj`, which P5-T1 names.

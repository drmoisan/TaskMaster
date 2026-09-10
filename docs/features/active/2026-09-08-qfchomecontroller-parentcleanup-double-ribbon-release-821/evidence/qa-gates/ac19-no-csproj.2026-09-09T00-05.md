# Phase 5 — AC19 no project file is modified

Timestamp: 2026-09-09T13-28
Task: [P5-T6]

AC19: "No file with a .csproj extension appears in the diff **of this change**."

## Span 1 — anchored name-only diff, filtered for `.csproj`

Command: `git diff --name-only (git merge-base HEAD origin/main)`, filtered for the literal `.csproj`.
EXIT_CODE: 0

Result: **1 path.**

```text
UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

## Span 2 — porcelain status companion, filtered for `.csproj`

Command: `git status --porcelain --untracked-files=all`, filtered for the literal `.csproj`.
EXIT_CODE: 0

Result: **0 paths.** No project file is modified or added in the working tree. The paired porcelain
span is required because a name-only diff cannot report an untracked file.

## The one anchored-span path is inherited, not authored by this change

The anchored two-dot diff does not isolate this feature's change. Measured facts:

| Observation | Command | Value |
|---|---|---|
| HEAD | `git rev-parse HEAD` | `89bdfe0613f216ab7f50bd252e07bf5af54f918e` |
| merge base | `git merge-base HEAD origin/main` | `6f08302a4f0af0061f27856e8a654f819df902aa` |
| `origin/main` | `git rev-parse origin/main` | `6f08302a4f0af0061f27856e8a654f819df902aa` |
| commits between merge base and HEAD | `git log --oneline <mb>..HEAD` | **70** |
| paths in the anchored diff | `git diff --name-only <mb>` | 192 |
| paths in the authored diff | `git diff --name-only HEAD` | 9 |

This execution branch descends from the epic integration branch
`epic/review-residuals-2026-09-08-integration`, which already carries 70 commits from sibling
features. `git diff <merge-base>` compares that base against the **working tree**, so its output is
the union of 70 inherited commits and this feature's 9 working-tree changes.

Provenance of the single `.csproj` path, established directly:

Command: `git log --oneline 6f08302a4f0af0061f27856e8a654f819df902aa..HEAD -- UtilitiesCS.Test/UtilitiesCS.Test.csproj`
EXIT_CODE: 0
Output:

```text
468760ac build(817): wire the 4 new FolderPredictorTests split files into UtilitiesCS.Test.csproj
```

The path was changed by inherited commit `468760ac`, which belongs to sibling feature **817** and was
already on this branch before execution began.

Inertness test — is the file touched by this feature at all?

Command: `git diff --name-only HEAD -- UtilitiesCS.Test/UtilitiesCS.Test.csproj`
EXIT_CODE: 0
Result: **0 paths.** The file is byte-identical between HEAD and this worktree. This feature has not
modified it.

## Span 3 — authored-change diff, filtered for `.csproj`

Command: `git diff --name-only HEAD`, filtered for the literal `.csproj`.
EXIT_CODE: 0

Result: **0 paths.**

## Evaluation

AC19 is evaluated against the diff **of this change**, which is the union of span 2 and span 3: zero
`.csproj` paths. All eight Write Set files already carry a `Compile Include` entry — verified in
`QuickFiler.csproj` at lines 297 and 330, `UtilitiesCS.csproj` at 945 and 973, `QuickFiler.Test.csproj`
at 129 and 175, and `UtilitiesCS.Test.csproj` at 504 and 506 — so no project-file edit was required
and none was made.

**Deviation recorded.** The plan's `[P5-T6]` acceptance condition reads "zero `.csproj` paths in
either span", where the first span is the anchored two-dot diff. That condition is **not satisfiable
on this branch** and its unsatisfiability is a property of the branch topology, not of this change:
plan decision 6 anticipated the inheritance mechanism but scoped it to four feature-folder documents,
whereas the real inherited set is 70 commits and 183 paths including this `.csproj`. The condition is
recorded here as **not met as literally written**, together with the discriminating evidence that the
one path in the anchored span is inherited and is untouched by this feature. No gate was weakened:
the anchored span was run and reported in full, and the additional spans narrow the question rather
than relaxing it.

Output Summary: the anchored two-dot span reports 1 `.csproj` path,
`UtilitiesCS.Test/UtilitiesCS.Test.csproj`, proven inherited from sibling-feature commit `468760ac`
and proven byte-identical between HEAD and this worktree. The porcelain span and the authored-change
span each report **0** `.csproj` paths. This change modifies no project file.

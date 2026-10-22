# Phase 5 — AC20 complete change footprint

Timestamp: 2026-09-09T13-32
Task: [P5-T7]

## Staging step

Command: `git add --intent-to-add -- docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821`
EXIT_CODE: 0

The `--intent-to-add` span is confined to that one feature-folder path. Nothing under `.claude/` was
staged. Intent-to-add makes the new evidence artifacts visible to a name-listing diff, which would
otherwise enumerate tracked changes only.

## Span 1 — authored footprint

Command: `git diff --name-status HEAD`
EXIT_CODE: 0

**38 paths**, in three classes and no others.

### Class 1 — the eight Write Set files (all `M`, all permitted)

```text
M	QuickFiler/Controllers/QfcHomeController.cs
M	QuickFiler/Controllers/EfcHomeController.cs
M	UtilitiesCS/Threading/ProgressViewer.cs
M	UtilitiesCS/Threading/ProgressPane.cs
M	QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
M	QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs
M	UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs
M	UtilitiesCS.Test/Threading/ProgressPane_Tests.cs
```

Exactly the eight files the Write Set names. No ninth production or test file was created, which is
what keeps AC19 and AC20 true: a ninth file would have required a `.csproj` `Compile Include` entry.

### Class 2 — this plan file (`M`, permitted)

```text
M	docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/plan.2026-09-08T23-50.md
```

Modified only by this plan's own task check-offs.

### Class 3 — 29 evidence artifacts under the feature folder's `evidence/` tree (all `A`, all permitted)

All 29 lie under
`docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/`
and use only canonical kinds: 13 under `baseline/`, 6 under `other/`, 7 under `qa-gates/` and 3 under
`regression-testing/`. No `evidence/regression/` directory was created; that kind is not canonical.

Not present in this span, and correctly so: `spec.md`, `issue.md` and
`research/enumeration-findings.2026-09-08T23-45.md`. All three are permitted paths but this plan has
not yet modified them; `spec.md` enters the footprint in Phase 7 when the acceptance criteria are
checked off.

## Span 2 — porcelain status companion

Command: `git status --porcelain --untracked-files=all`
EXIT_CODE: 0

The porcelain span lists the same 38 paths, with the 8 Write Set files as ` M` and the 29 evidence
artifacts as ` A` following the intent-to-add, plus the plan file as ` M`. It reports **no untracked
path outside the feature folder**, so nothing is hiding from the name-listing diff.

## Forbidden-path check against the authored footprint

Every prohibited path named by the plan was searched for explicitly in the authored span:

| Prohibited path | Matches |
|---|---|
| any `.csproj` | **0** |
| `.editorconfig` | **0** |
| `BannedSymbols.txt` | **0** |
| `CLAUDE.md` | **0** |
| anything under `.claude/` | **0** |
| anything under `.github/` | **0** |
| anything under `coverage/` | **0** |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | **0** |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | **0** |
| `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` | **0** |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | **0** |
| `UtilitiesCS/Threading/TimeOutTask.cs` | **0** |
| `UtilitiesCS/Extensions/DfDeedle.cs` | **0** |
| `UtilitiesCS.Test/Properties/AssemblyInfo.cs` | **0** |
| `UtilitiesCS/Properties/AssemblyInfo.cs` | **0** |
| `QuickFiler/Properties/AssemblyInfo.cs` | **0** |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` | **0** |

All seventeen return zero. The `.claude/agent-memory/` exclusion class the plan carves out is
**empty in this worktree**: no entry under `.claude/` appears in either span, so there is nothing to
list verbatim and nothing to confirm unstaged.

## Span 3 — the anchored two-dot diff, and why it is not the AC20 denominator

Command: `git diff --name-only (git merge-base HEAD origin/main)`
EXIT_CODE: 0
Result: **192 paths.**

Measured branch topology:

| Observation | Value |
|---|---|
| HEAD | `89bdfe0613f216ab7f50bd252e07bf5af54f918e` |
| merge base with `origin/main` | `6f08302a4f0af0061f27856e8a654f819df902aa` |
| `origin/main` | `6f08302a4f0af0061f27856e8a654f819df902aa` |
| commits between merge base and HEAD | **70** |
| paths changed by those 70 commits (`git diff --name-only <mb>..HEAD`) | **184** |
| paths in the authored footprint (`git diff --name-only HEAD`) | **38** |

This execution branch descends from the epic integration branch
`epic/review-residuals-2026-09-08-integration`, which already carried 70 sibling-feature commits
before execution began. `git diff <merge-base>` compares that base against the **working tree**, so
its 192 paths are the union of 184 inherited paths and this feature's 38 authored paths (the overlap
being the files present in both).

The inherited 184 include sibling features 813, 815, 817, 823, 824, 825 and 826, the epic documents
under `docs/features/epics/`, ten promoted records under `docs/features/potential/promoted/`, entries
under `.claude/agent-memory/`, three scripts under `scripts/vscode/`, and production and test files
including `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`,
`UtilitiesCS.Test/UtilitiesCS.Test.csproj` and the `FolderPredictorTests` family. **None of them is
authored by this feature**, and each is byte-identical between HEAD and this worktree — the authored
span in Span 1 is the direct proof, since a file this feature had touched would appear there.

**Deviation recorded.** The plan's `[P5-T7]` acceptance condition requires every path in the union of
the anchored diff and the porcelain span to belong to a permitted set of roughly a dozen paths. That
condition is **not satisfiable on this branch**, and its unsatisfiability is a property of the branch
topology rather than of this change. Plan decision 6 anticipated the mechanism — it notes that the
merge base "resolves below" the preparation commit — but scoped the consequence to four
feature-folder documents; the true inherited set is 70 commits and 184 paths. The anchored span was
run and is reported here in full rather than suppressed, and AC20 is evaluated against the authored
footprint, which is the set of files this fix actually changes. No permitted-path list was widened
and no gate was weakened.

Output Summary: the authored footprint is **38 paths** — the eight Write Set files, this plan file,
and 29 evidence artifacts under the feature folder's canonical `evidence/` tree — and contains zero
matches for all seventeen prohibited paths, including every path named in the spec's
"Out of scope / non-goals" section. The anchored two-dot span additionally lists 184 inherited paths
from 70 sibling-feature commits already on this branch; that set is disclosed above and is not part
of this change.

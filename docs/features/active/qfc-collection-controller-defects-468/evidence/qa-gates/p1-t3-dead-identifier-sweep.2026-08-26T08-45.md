# [P1-T3] AC-2 dead-identifier sweep

Timestamp: 2026-08-26T08-45

Command: `grep -cF '<identifier>' QuickFiler/Controllers/QfcCollectionController.cs` — run once per
identifier, fixed-string matching (`-F`), scoped to that single file.

EXIT_CODE: 0

ExpectedExitCode: 0

(Each individual `grep -cF` returns exit code 1, grep's "no lines selected", which is the
**intended** result for every one of the thirteen searches. The task's exit code is 0: all thirteen
searches ran and all thirteen produced the required zero.)

## Output Summary

**All thirteen identifiers return zero hits in `QuickFiler/Controllers/QfcCollectionController.cs`.
Each zero is contrasted below against its non-zero P0-T15 baseline count.**

| # | Identifier | P0-T15 baseline hits | Baseline line numbers | Post-P1-T2 hits | Zeroed |
|---|---|---|---|---|---|
| 1 | `WireUpKeyboardHandler` | 1 | 1254 | **0** | yes |
| 2 | `AnyOpenDropDownsAsync` | 1 | 1324 | **0** | yes |
| 3 | `LoadGroups_02cAsync` | 1 | 587 | **0** | yes |
| 4 | `LoadGroups_02bAsync` | 2 | 402, 635 | **0** | yes |
| 5 | `LoadGroup_03bAsync` | 2 | 647, 654 | **0** | yes |
| 6 | `LoadConversationsAndFoldersAsync` | 1 | 761 | **0** | yes |
| 7 | `LoadItemGroup(` | 2 | 772, 776 | **0** | yes |
| 8 | `LoadSequentialAsync` | 1 | 827 | **0** | yes |
| 9 | `LoadGroupSequential` | 2 | 838, 842 | **0** | yes |
| 10 | `CacheTlpForMove` | 2 | 865, 872 | **0** | yes |
| 11 | `SwapTlp` | 1 | 870 | **0** | yes |
| 12 | `CaptureTlpTemplate` | 1 | 1991 | **0** | yes |
| 13 | `_templateTlp` | 3 | 70, 1994, 1995 | **0** | yes |
| | **Total** | **20** | | **0** | |

Every baseline count is strictly positive, so each zero is a real state change produced by P1-T2 and
not a search that could never have matched. The baseline figures are taken verbatim from
`evidence/baseline/p0-t15-source-facts.2026-08-26T08-25.md` §3.

### Scoping

Every search is scoped to the single named file `QuickFiler/Controllers/QfcCollectionController.cs`,
as the plan's `### Literals asserted by acceptance conditions` convention requires. No search is
repository-wide, because `LoadSequentialAsync` names three unrelated **live** members in
`TaskMaster/AppGlobals/` and `docs/features/**` quotes every identifier; a repository-wide zero-hit
condition would be unsatisfiable by construction. The evidence that no *other* file references the
removed members is carried by P1-T1's scoped sweep, not by this task.

### Identifier 7 uses the parenthesised form deliberately

`LoadItemGroup(` is asserted, not the bare stem `LoadItemGroup`. The bare stem still matches the
**live** member `LoadItemGroupsAndViewers_02`, which survives at two sites in the post-edit file and
which AC-3 requires to be preserved. Confirmed after the edit:

```
$ grep -cF 'LoadItemGroup('  QuickFiler/Controllers/QfcCollectionController.cs
0
```

while `LoadItemGroupsAndViewers_02` is still present (see P1-T4). The parenthesis is what makes this
assertion both satisfiable and meaningful.

### Identifier 11 disambiguation

`SwapTlp` returns 0 while the unrelated live call `_formViewer.SwapItemTableLayout(tlp)` remains in
`ActivateQueuedTlp`. The two are distinct literals; `SwapTlp` is not a substring of
`SwapItemTableLayout`.

### File-level effect of P1-T2

| Metric | Baseline | After P1-T2 | Delta |
|---|---|---|---|
| `QfcCollectionController.cs` line count | 2349 | **2108** | **-241** |
| `git diff --stat` | | `241 ----` | 241 deletions, **0 insertions** |

The 241 removed lines are the twelve member declarations (229 lines, per
`research/qfc-collection-controller-defects.md` §9), their twelve trailing blank separator lines, the
`_templateTlp` field at `:70`, and the commented-out reference at `:402`. The drop of 241 exceeds the
plan's "at least 200 lines" threshold for P1-T2.

`git diff` reports **0 insertions**, confirming the edit is a pure deletion: no line was rewritten,
reindented, or reflowed, so the renumbering is a single reviewable hunk set as D15 requires.

### Ownership discipline

`git status --porcelain -- QuickFiler QuickFiler.Test` after the edit reports exactly one entry:

```
 M QuickFiler/Controllers/QfcCollectionController.cs
```

`git diff --stat -- QuickFiler/Controllers/KbdActions.cs` is empty. **D2 is satisfied: zero lines
were changed in `QuickFiler/Controllers/KbdActions.cs`**, which is owned by sibling epic child #444.
Removing `WireUpKeyboardHandler` deleted a *caller* only.

Result: PASS. AC-2's source-search half is satisfied.

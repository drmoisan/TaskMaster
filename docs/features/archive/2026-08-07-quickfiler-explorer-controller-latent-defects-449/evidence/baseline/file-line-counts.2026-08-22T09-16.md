# Baseline — File Line Counts (Issue #449, [P0-T14], [P0-T15])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`
Merge-base SHA: `c551eabab0aa0a6b1a284252811a2e1de819634e` (HEAD equals it at baseline)

Command:
```
grep -c '' QuickFiler/Controllers/QfcExplorerController.cs \
           QuickFiler/Interfaces/IQfcExplorerController.cs \
           QuickFiler.Test/QuickFiler.Test.csproj \
           UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs \
           QuickFiler/Legacy/QuickFileController.cs
wc -l QuickFiler.Test/QuickFiler.Test.csproj
```
EXIT_CODE: 0

`grep -c ''` is used rather than `wc -l` because `wc -l` counts newline CHARACTERS and therefore
under-reports by one for any file lacking a terminating newline. `QuickFiler.Test/QuickFiler.Test.csproj`
is exactly such a file.

---

## [P0-T14] — Pre-change line counts of the three files AC-16 measures

| File | Plan expectation | **Measured** | Match |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcExplorerController.cs` | 323 | **323** | yes |
| `QuickFiler/Interfaces/IQfcExplorerController.cs` | 15 | **15** | yes |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 484 | **484** | yes |

Raw output:
```
QuickFiler/Controllers/QfcExplorerController.cs:323
QuickFiler/Interfaces/IQfcExplorerController.cs:15
QuickFiler.Test/QuickFiler.Test.csproj:484
```

### The `QuickFiler.Test.csproj` terminating-newline caveat, confirmed empirically

```
$ wc -l QuickFiler.Test/QuickFiler.Test.csproj
483 QuickFiler.Test/QuickFiler.Test.csproj

$ grep -c '' QuickFiler.Test/QuickFiler.Test.csproj
484
```

The two tools disagree by exactly one, which confirms the plan's note that the file has no
terminating newline: `wc -l` reports **483** against a true count of **484**. Every figure this plan
states for that file — 484 pre-change, 485 after the single [P1-T2] append, or 486 after a second
append should [P6-T14] force a split — is a TRUE count in the `grep -c ''` sense. Any later
verification of that file's size must use `grep -c ''`, not `wc -l`, or it will appear off by one.

### Post-change expectations recorded for later comparison

| File | Pre-change | Expected post-change | Source |
| --- | --- | --- | --- |
| `QfcExplorerController.cs` | 323 | 323 after [P2-T1] (one-line replacement, no count change); ~317 after [P3-T2]; ~178 after [P4-T1]; then further reduced by [P4-T2]/[P5-T2] and increased by the [P5-T3] seam | measured at each phase |
| `IQfcExplorerController.cs` | 15 | 14 after [P3-T1] | [P3-T1] acceptance |
| `QuickFiler.Test.csproj` | 484 | 485 after [P1-T2]; 486 only if [P6-T14] forces a split | [P1-T2] / [P6-T14] acceptance |

---

## [P0-T15] — Pre-existing 500-line-cap violations that this change does NOT touch

| File | **Measured lines** | Edited by this change? | Appears in the diff? |
| --- | --- | --- | --- |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | **1429** | **No** | **No** |
| `QuickFiler/Legacy/QuickFileController.cs` | **1065** | **No** | **No** |

Raw output:
```
UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs:1429
QuickFiler/Legacy/QuickFileController.cs:1065
```

**Neither file is edited by this change and neither will appear in the diff.** Both are pre-existing
violations of the 500-line file cap in `.claude/rules/general-code-change.md`. This statement is
recorded pre-emptively so that a later reviewer measuring the repository does not attribute either
violation to issue #449. [P7-T13] carries the same attribution statement against the actual diff.

Why each is out of scope:

- `SortEmail.cs` (1,429 lines) is the surviving maintained copy of the helpers duplicated inside the
  dead region that [P4-T1] deletes. It carries its own tests in `UtilitiesCS.Test`. Consolidating the
  three copies is a separate, larger change and is explicitly not planned here. No split refactor is
  performed on it.
- `QuickFileController.cs` (1,065 lines) sits in `QuickFiler/Legacy/` and is **not compiled** —
  `QuickFiler/QuickFiler.csproj` contains no `Compile Include` entry for the `Legacy/` directory. It
  is therefore invisible to every build gate. No split refactor is performed on it.

### Correction of a premise carried in the epic kickoff

The epic kickoff described `QuickFiler/Controllers/QfcExplorerController.cs` as 1,065 lines and
predicted that this change would produce a 500-line-cap violation requiring a partial-class split.
That is a misattribution. The measurements above show:

- `QuickFiler/Controllers/QfcExplorerController.cs` measures **323** lines, comfortably under the
  500-line cap both before and after this change (it only ever SHRINKS, to roughly 178 after the
  [P4-T1] dead-region deletion, before the small [P5-T3] seam addition).
- The **1,065** figure actually belongs to `QuickFiler/Legacy/QuickFileController.cs`, the uncompiled
  legacy file above, which this change does not edit.

**No partial-class split is needed and none is in scope.** The kickoff's 1,065 figure is not acted
on. This agrees with the plan's own [P0-T15] and [P7-T13] attribution.

---

## Output Summary

All five measured counts match the plan's stated expectations exactly. AC-16 files pre-change:
`QfcExplorerController.cs` **323**, `IQfcExplorerController.cs` **15**,
`QuickFiler.Test.csproj` **484** (true count; `wc -l` reports 483 because the file has no terminating
newline, confirmed empirically). Pre-existing over-cap files not touched by this change:
`SortEmail.cs` **1429** and `QuickFiler/Legacy/QuickFileController.cs` **1065** — neither is edited
and neither will appear in the diff. The epic kickoff's claim that `QfcExplorerController.cs` is 1,065
lines is a misattribution of the legacy file's count; the controller is 323 lines and no
partial-class split is required or in scope.

# Pre-Change Fact Capture (P0-T10) — remediation cycle 1, issue #614

Timestamp: 2026-08-26T21-25

Command: `git rev-parse HEAD`; `git status --porcelain`; `(Get-Content <path>).Count` for the five
rule-10 files; an ordinal fixed-string hit count for every row of the plan's "Verified pre-change
literal-gate table". The gate literals were extracted **programmatically** from the plan's own
backtick spans (the table rows were parsed out of `remediation-plan.2026-08-26T21-00.md` and each
token was taken verbatim from cell 1, its scope from cell 2) rather than re-typed into a shell
string, so no quoting layer could alter a literal.

EXIT_CODE: 0

Output Summary: HEAD recorded; working tree carries only this cycle's own plan checkbox edit and its
new evidence folder; all five file line counts match the plan; all 18 literal-gate rows match the
plan's recorded pre-change hit counts exactly. No discrepancy; execution may proceed.

## 1. HEAD sha

```
0fb0efec635cee7bd93dc975440f67aa1d72c4ce
```

The plan header records entry HEAD `6bbb18e7`. The actual entry HEAD is `0fb0efec`, its descendant.
This is recorded, not gated (plan header line 12 states the entry HEAD is "recorded, not gated on;
gates use tree invariants"), and every tree invariant the plan does gate on was re-measured below
against `0fb0efec` and matches.

## 2. `git status --porcelain`

```
 M docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/remediation-plan.2026-08-26T21-00.md
?? docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/remediation-baseline/
```

Both entries are under `<FEATURE>/**` and are this cycle's own Phase 0 output (the P0-T1..T5
checkbox updates and the new `evidence/remediation-baseline/` folder). No `.claude/agent-memory/**`
entry is present. No production or test source file is modified. The tree was clean at session
entry.

## 3. Rule-10 file line counts

| File | Plan-recorded baseline | Measured | Ceiling | Match |
| --- | ---: | ---: | ---: | --- |
| `QuickFiler/Controllers/EfcFormController.cs` | 1072 | **1072** | 1084 | yes |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | 38 | **38** | 500 | yes |
| `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` | 87 | **87** | 500 | yes |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 596 | **596** | unmodified | yes |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | 694 | **694** | unmodified | yes |

## 4. Literal-gate table re-verification

Repo-wide scope resolves to 1570 `*.cs` files (excluding `\bin\`, `\obj\`, `\packages\`, `\.git\`).
Counts are ordinal, fixed-string, occurrence counts (not line counts).

| Token (verbatim) | Scope | Plan-recorded pre-change hits | Measured | Match |
| --- | --- | ---: | ---: | --- |
| `value.Length >= 3` | `QuickFiler/Controllers/EfcSelectionGuard.cs` | 1 | **1** | yes |
| `IsValidFilingSelection(selectedFolder)` | `QuickFiler/Controllers/EfcFormController.cs` | 1 | **1** | yes |
| `IsValidFilingSelection(selectedFolder, archiveRoot)` | `QuickFiler/Controllers/EfcFormController.cs` | 0 | **0** | yes |
| `IsValidFilingSelection(SelectedFolder)` | `QuickFiler/Controllers/EfcFormController.cs` | 1 | **1** | yes |
| `IsValidCreationSelection(SelectedFolder)` | `QuickFiler/Controllers/EfcFormController.cs` | 0 | **0** | yes |
| `() => _globals.Ol.ArchiveRootPath` | `QuickFiler/Controllers/EfcFormController.cs` | 0 | **0** | yes |
| `IsValidCreationSelection` | repo-wide `*.cs` | 0 | **0** | yes |
| `MinimumCreationLength` | repo-wide `*.cs` | 0 | **0** | yes |
| `ResolveArchiveRootOrEmpty` | repo-wide `*.cs` | 0 | **0** | yes |
| `RootUnavailableDiagnostic` | repo-wide `*.cs` | 0 | **0** | yes |
| `TryMakeArchiveRelative` | `QuickFiler/Controllers/EfcSelectionGuard.cs` | 0 | **0** | yes |
| `InvalidOperationException` | `QuickFiler/Controllers/EfcSelectionGuard.cs` | 0 | **0** | yes |
| `IsValidFilingSelection_TwoCharacterSelection_IsRejected` | `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` | 1 | **1** | yes |
| `IsValidCreationSelection_TwoCharacterSelection_IsRejected` | repo-wide `*.cs` | 0 | **0** | yes |
| `IsValidFilingSelection_TwoCharacterRelativeStem_IsAccepted` | repo-wide `*.cs` | 0 | **0** | yes |
| `IsValidFilingSelection_SingleCharacterRelativeStem_IsAccepted` | repo-wide `*.cs` | 0 | **0** | yes |
| `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsAccepted` | repo-wide `*.cs` | 0 | **0** | yes |
| `IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted` | repo-wide `*.cs` | 0 | **0** | yes |

18 of 18 rows match. Every zero-hit-after gate currently returns >= 1, and every at-least-N-hit gate
for something the executor must create currently returns 0. No gate is unfailable and no premise is
stale.

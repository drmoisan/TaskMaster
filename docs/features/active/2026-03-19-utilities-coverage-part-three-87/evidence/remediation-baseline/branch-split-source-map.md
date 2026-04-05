# Branch Split Source Map

Timestamp: 2026-03-26T16:10:00-04:00
Source: `.git/branch_analysis_issue87.txt`

## Issue #97 Commits (linear replay candidates)

| SHA | Date | Message |
|---|---|---|
| a19ac86 | 2026-03-25 | fix(qfc-metrics): guard null calendar and appointment in metrics path |
| ad4ae95 | 2026-03-25 | feat: code review and remediation plan |

Merge commit (do NOT replay): c448819 — Merge pull request #98 from drmoisan:getmovediagnostics-null-guard-97

## Issue #96 Commits

| SHA | Date | Message |
|---|---|---|
| bd8fc03 | 2026-03-25 | fix(qfc-item-controller): restore Keys.Right registration to expand conversation on focus |
| 3b472b2 | 2026-03-25 | feat: audit the bug fix for issue 96 |

## Residual Excluded-Work Commits

| SHA | Date | Message |
|---|---|---|
| 52742b8 | 2026-03-21 | fix: align codex web workflow with linux setup |
| 4d5f476 | 2026-03-21 | ci: run codex web setup test on branch updates |
| 60408b0 | 2026-03-23 | fix(concurrent-observable): relay wrapper as CollectionChanged sender |
| 16d7d5d | 2026-03-23 | fix(QfcItemController): propagate OperationCanceledException from conversation load |
| 0c9a045 | 2026-03-23 | fix(EfcHomeController): guard ExecuteMovesAsync against re-entrant cleanup and fix metrics predicate |
| 66220df | 2026-03-25 | codex: convert feature review to codex skill and sub-agents |
| ea0206e | 2026-03-24 | chore: upgrade ribbon |

## Clean Issue #87 Commits (pure/primary issue-87 work)

| SHA | Date | Message |
|---|---|---|
| 078fd77 | 2026-03-23 | fix(sco-collection): restore items when loading or replacing lists |
| 3206593 | 2026-03-23 | fix(serializable-list): capture writer delegate before async serialization |
| cce7c5a | 2026-03-23 | bug: removed file system dependency from ScoCollection_Tests |
| fff20c7 | 2026-03-23 | fix(serializable-list): capture file-system seams before queued IO |
| d65320b | 2026-03-23 | test(utilitiescs): add early UtilitiesCS coverage phases and baseline evidence |
| 2326734 | 2026-03-23 | fix(InputBoxViewer): guard DpiAware against already-initialized WinForms |
| 5f90762 | 2026-03-24 | test(utilitiescs): add SubjectMap and DfDeedle coverage tests |
| 27639bf | 2026-03-24 | test(utilitiescs): add helper and config coverage tests |
| 5afe10d | 2026-03-24 | test(utilitiescs): add EmailIntelligence and Threading coverage tests |
| ee9e4d9 | 2026-03-25 | test(utilitiescs): expand coverage for classifier and helper flows |
| 4009d1c | 2026-03-25 | test(utilitiescs): expand coverage for progress, store, stream, and classifiers |
| 5661a47 | 2026-03-26 | fix(utilitiescs): harden coverage edge cases across UtilitiesCS |
| 4830958 | 2026-03-26 | feat: final qc |
| 6e5d01d | 2026-03-26 | feat: code review and remediation plan 1st draft |

## Mixed Commits (contain paths from multiple scopes, require bootstrap for reconstruction)

| SHA | Date | Message | Scopes |
|---|---|---|---|
| ee92dd6 | 2026-03-22 | test(utilities-coverage): extend phase 2 UtilitiesCS coverage | #87 + QuickFiler |
| 4634ac5 | 2026-03-23 | fix(reusable-typeclasses): prevent lock-recursive collection change reads | #87 + TaskMaster |
| a8d24b2 | 2026-03-24 | fix(utilities-coverage): stabilize coverage tests and VSTO build gating | #87 + TaskMaster |
| 5fb07f7 | 2026-03-24 | test(utilitiescs): expand coverage tests and retire dead deprecated stubs | #87 (pure) |
| 221e76f | 2026-03-25 | test(utilitiescs): expand coverage for classifier and helper workflows | #87 + TaskMaster |
| a19ac86 | 2026-03-25 | fix(qfc-metrics): guard null calendar and appointment in metrics path | #97 + .codex + .github |

## Planning/Chore Commits (plan file updates, archiving)

| SHA | Date | Message |
|---|---|---|
| 5a7831b | 2026-03-22 | chore: archive v1 plan and evidence |
| 77546ac | 2026-03-23 | chore: replan phases 1 - 12 |
| dbdce98 | 2026-03-23 | chore: phases 13 -25 |
| 4010818 | 2026-03-23 | chore: phase 26 - 48 |
| c853a88 | 2026-03-23 | chore: planning phase 48 - 89 |
| da0ed13 | 2026-03-23 | chore: plan tweaks |
| cc3009f | 2026-03-23 | delete faulty file |

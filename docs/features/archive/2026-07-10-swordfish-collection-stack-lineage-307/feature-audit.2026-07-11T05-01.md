# Feature Audit — Remediation Cycle 1 Exit (#307 swordfish-collection-stack-lineage)

- Timestamp: 2026-07-11T05-01
- Reviewer: feature-reviewer
- Feature branch: feature/swordfish-collection-stack-lineage
- HEAD: d34b6636
- Diff range: `origin/epic/swordfish-removal-integration...HEAD`
- Work Mode (issue.md): full-feature -> AC sources are `spec.md` AND `user-story.md`

## Scope and Baseline

Baseline for this cycle-exit audit is the epic integration tip (618954b8). The prior feature-review (F2 tip 78684e65) verified the full F2 acceptance-criteria set (all boxes checked in `spec.md` and `user-story.md`). This cycle re-confirms that set still holds on the merged tree after the two conflict resolutions, since the merge could in principle have altered the F2 interface retype or the compile set. The re-confirmation is a spot-check per criterion against the merged HEAD, not a re-execution of the full suite (toolchain re-verified via `evidence/remediation-baseline/merge-reverify.2026-07-11T04-47.md`).

## Acceptance Criteria Inventory

- Source 1: `spec.md` `## Acceptance Criteria` — 20 criteria (lines 236-285), all `[x]`.
- Source 2: `user-story.md` `## Acceptance Criteria` — 8 criteria (lines 90-122), all `[x]`.
- Total: 28 criteria across both sources.

## Acceptance Criteria Evaluation

The evaluation focuses on the criteria most exposed to the merge (the interface retype and the compile-set/deletion criteria), with the remainder re-confirmed as unchanged by the merge.

| # | Source | Criterion (abbrev) | Verdict | Evidence on merged HEAD |
|---|---|---|---|---|
| S1 | spec | Clean `ConcurrentObservableCollection<T>` base created with full member surface | PASS | `ConcurrentObservableCollection.cs` (+169) and `.Serialization.cs` present and compiled; toolchain green |
| S2 | spec | Clean collection serializes as bare JSON array (no `[JsonObject]`) | PASS | `CollectionRoundTrip_Tests.cs` (+232) passes in the 4667 suite |
| S3 | spec | `CtfMap`/`SubjectMapSco` re-based | PASS | No `ScoCollection<` reference remains; both compile |
| S4 | spec | Direct consumers re-pointed (`Filters`, `PrefixList`, `OlFolderClassifierGroup`) | PASS | Diff shows re-points; suite green |
| S5 | spec | Interface members `IAppAutoFileObjects.Filters` and `IToDoObjects.PrefixList`/`LoadPrefixList` retyped | PASS (merge-exposed) | Merged `IToDoObjects.cs` lines 26-27 typed `ConcurrentObservableCollection<IPrefix>`; union preserved F1's dictionary members |
| S6 | spec | `SloStack<T>` positional surface | PASS | `SloStack.cs` (+260) 100% covered; `SloStack_Tests.cs` (+402) |
| S7 | spec | `SloStack<T>` `SerializeAsync` + typed `Static.Deserialize` | PASS | Covered by `SloStack_Tests.cs` / `SloStackUndoContract_Tests.cs` |
| S8 | spec | All `ScoStack<IMovedMailInfo>` consumers migrated | PASS | QuickFiler controllers/interfaces, AppAutoFileObjects, SortEmail, EmailFiler re-pointed; no `ScoStack<` remains |
| S9 | spec | MovedMails construction reconciled to `Static.Deserialize` | PASS | Prior review verified; unchanged by merge |
| S10 | spec | JSON round-trip test per persisted collection | PASS | `CollectionRoundTrip_Tests.cs` present; suite green |
| S11 | spec | Undo behavior preserved (SortEmail + QfcFormController) | PASS | `SloStackUndoContract_Tests.cs` (+111); positional contract covered |
| S12 | spec | `RecentsList<T>` dead code deleted | PASS | `RecentsList.cs` and `RecentsList_Tests.cs` deleted; compile entry removed |
| S13 | spec | Legacy `ScoCollection.cs`/`ScoStack.cs` + tests deleted after grep | PASS (merge-exposed) | csproj entries removed in the resolution; source files deleted; grep confirms no first-party reference |
| S14 | spec | Migrated tests compile and pass | PASS | 4667/4667 pass |
| S15 | spec | New members meet new-code coverage bar | PASS | 98.0% new-code (>= 90%); `evidence/qa-gates/coverage-delta.md` |
| S16 | spec | Full C# toolchain passes in order | PASS | merge-reverify: csharpier 0, analyzers 0 first-party, nullable baseline-identical, tests 0 fail |
| S17 | spec | Scope boundary held (no Swordfish/ProjectReference/sln/F1-F3-F5 type changes) | PASS (merge-exposed) | Three-dot diff filtered for forbidden paths returned none; csproj has no ProjectReference edits |
| S18-S20 | spec | Serialization guardrail / interchange / ordering data-state criteria | PASS | Covered by round-trip and serialization tests; unchanged by merge |
| U1-U8 | user-story | Maintainer/end-user outcome criteria (clean base, re-base, SloStack surface, consumer migration, round-trip test, undo preserved, RecentsList removed, toolchain+coverage) | PASS | Map 1:1 to S1-S16 above; all confirmed on merged HEAD |

Merge-exposed criteria (S5, S13, S17) are the ones the two conflict resolutions could have broken; each is verified PASS on the merged tree.

## Regression Check

- Tests: 4667/4667 pass on merged HEAD (baseline 4680; reduction is merged sibling test-file deletions, not F2). Zero-failure no-regression bar met.
- New-code coverage: 98.0%, no regression on changed lines (type-only re-points with unchanged control flow).

## Acceptance Criteria Check-off

All 28 criteria across `spec.md` and `user-story.md` were already checked `[x]` by the prior review and remain satisfied on the merged tree. No criterion changed status; no new criteria added. No unchecked items remain.

## Summary

### Acceptance Criteria Status
- Source: `spec.md` (20) and `user-story.md` (8)
- Total AC items: 28
- Checked off (delivered): 28
- Remaining (unchecked): 0
- Items remaining: none

Feature-audit verdict: PASS. The F2 acceptance-criteria set holds on the merged tree; the two conflict resolutions introduced no regression and no scope violation. Blocking findings: 0.

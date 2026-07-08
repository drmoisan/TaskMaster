# Final QC — AC10 Acceptance-Criteria Check-Off (issue #211)

Timestamp: 2026-06-24T19-55

Work Mode: full-bug -> AC source is `spec.md`.

## AC10 Status Table

| Component of AC10 | Required | Delivered | Status |
| --- | --- | --- | --- |
| Minimal TaskMaster-side fix | direct-navigation replacing `new FolderTree(Root)` in both `LoadJunk*` sites | `JunkFolderPathNavigator.ResolvePath` via `[ExcludeFromCodeCoverage]` `OutlookFolderNode` adapter; `FolderTree.cs` unchanged | PASS |
| Behavior/ordering invariant unit test | test asserting the invariant the fix relies on | `ResolvePath_AccessesOnlyFoldersAlongThePath_NotEntireTree` (enumeration-bound, path-touch invariant) + 5 correctness tests (single/nested/case/root-BFS/unmatched) + 4 edge tests | PASS |
| Correctness equivalence (binding) | direct nav resolves IDENTICAL folder as legacy `FindSequentialNode` | BFS-from-root first segment, direct-child subsequent, ordinal case-sensitive `Name ==`; verified by correctness tests | PASS |
| Red-before-green (bugfix workflow) | failing-first regression captured before fix | RED: 785 enumerations vs budget 4 (`red-run-enumeration-bound-2026-06-24T17-30.md`, EXIT 1); GREEN: <=4, all pass (`green-run-enumeration-bound-2026-06-24T17-30.md`, EXIT 0) | PASS |
| Not-found fallback preserved verbatim | null/empty early return; MyBox; PickFolder; WriteJunk*Setting + Save | preserved byte-identical in both methods | PASS |
| Full C# toolchain in order | CSharpier -> analyzers -> nullable/TWAE -> MSTest+coverage | one clean pass: 1108 files formatted, 0 analyzer errors, 0 nullable errors, 4109/4109 tests | PASS |
| New-code coverage >= 90% | JunkFolderPathNavigator.cs | 94.92% (112/118); aggregate 95.00% | PASS |
| No repo-wide regression | whole-process not regressed | 61.84% -> 61.90% (improved) | PASS |
| File-size <= 500 (all touched) | production + test | JunkFolders 186; Navigator 159; Tests 351 | PASS |
| Runtime re-capture confirming latency reduction | non-debugger cold-start `[spam-init]` ValidatePathsSet.JunkCertain/JunkPotential ms | MAINTAINER-GATED: instructions + placeholder under `evidence/other/` | PENDING (maintainer) |

## Verdict

- Automated portion of AC10: COMPLETE and VERIFIED. AC10 checkbox set to `[x]` in `spec.md` with
  the supersession note and automated-portion verification appended (criterion text preserved).
- Runtime portion of AC10: maintainer-gated, not CI-automatable; placeholder + instructions
  provided (P4-T1, P4-T2). This is the only outstanding item and requires a live Outlook cold start.

### Acceptance Criteria Status
- Source: docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/spec.md
- AC10: automated portion checked off `[x]`; runtime re-capture maintainer-gated (pending).
- This plan's scope was AC10 only; other ACs in spec.md are out of this plan's scope and unchanged.

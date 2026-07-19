---
name: 349-efcviewer-breadcrumb-plan-seams
description: 'Epic child 9102 (#349) plan decisions: P0-T6 halt-gate on 9101 IFolderHierarchyProvider presence; evidence/repro/ kind authorized by spec+caller; EfcViewer3 fixed to mechanical swap; Newtonsoft types UtilitiesCS-only'
metadata:
  type: project
---

Plan `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/plan.2026-07-16T21-52.md` (issue #349, epic child 9102, C4, depends_on 9101) encodes these fixed decisions:

- **9101 dependency gate:** provider types (`IFolderHierarchyProvider`, `FolderSegmentInfo`) were verified ABSENT on the planning branch (grep 2026-07-17); 9101 merges first during epic execution, so Phase 0 task P0-T6 records the actual merged surface and HALTS (blocked report, no Phase 1) if absent. Any shape deviation is absorbed only at the row-builder/router input via one adapter class under a plan revision.
- **`evidence/repro/` kind:** the spec AC and the delegating orchestrator both mandate `<FEATURE>/evidence/repro/` for the percentage-defect runtime reproduction, in addition to the canonical kinds. Treat `repro` as an authorized kind for this feature; the [expect-fail] repro task (P1-T2) has a fail-before-exception dossier fallback under `evidence/regression-testing/` when live Outlook is unavailable.
- **EfcViewer3 disposition:** fixed to mechanical Designer-only swap (own phase, P7), NOT removal, for plan determinism; it is dead code (sole viewer construction `new EfcViewer()` at `QuickFiler/Helper Classes/EfcViewerQueue.cs:83`).
- **Newtonsoft placement:** bridge contracts + codec live in `UtilitiesCS` (already references Newtonsoft 13.0.4); `QuickFiler` gains no Newtonsoft reference — the router consumes typed codec outputs. Shared with sibling 9103 per epic.
- **No new testable logic in `EfcFormController`** (wholly `[ExcludeFromCodeCoverage]`, pre-existing over the 500-line cap); all logic goes in the non-exempt router/model classes.

**Why:** These are the caller-mandated and spec-mandated constraints most likely to be second-guessed during preflight revision loops; re-deriving them costs a full spec/research re-read.

**How to apply:** On any #349 plan revision, keep the same plan file path, keep P0-T6 as a halt gate, do not move Newtonsoft-consuming code into QuickFiler, and do not convert P7 into behavioral wiring. Related: [[legacy-csproj-explicit-compile-include]], [[plan-validator-task-id-sequential-constraint]].

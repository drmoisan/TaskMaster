---
name: project-351-quickfiler-breadcrumb-plan-seams
description: "#351 plan (plan.2026-07-16T21-53.md) load-bearing decisions: JSON code must live in UtilitiesCS (QuickFiler has no Newtonsoft ref), P2-T1 blocked-if-9101-absent gate, evidence/repro/ rejected, coordinator pattern"
metadata:
  type: project
---

Plan `docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351/plan.2026-07-16T21-53.md` (8 phases, 52 tasks) encodes these non-obvious decisions; keep them stable across preflight revision loops:

- **JSON placement:** `Newtonsoft.Json` is referenced only by `UtilitiesCS.csproj` (13.0.4), NOT by `QuickFiler.csproj`. All bridge message serialization (`BreadcrumbBridgeMessages.cs`, router) lives in `UtilitiesCS/OutlookObjects/Folder/`; the QuickFiler-side `BreadcrumbBridgeCoordinator` handles raw JSON strings only (no-new-packages guardrail G2).
- **9101 gate:** P0-T8 records `9101-CONTRACT: PRESENT|ABSENT`; P2-T1 reconciles (`DIRECT-CONSUME` vs `ADAPTER-REQUIRED`) and halts BLOCKED if absent — the plan never re-implements the live Outlook query. P2-T2/T3 carry explicitly authorized skip branches tied to the P2-T1 decision (compatible with the no-SKIPPED rule).
- **Evidence override:** spec.md FR-5/AC-6 names `evidence/repro/` — rejected; repro goes to `evidence/regression-testing/`, post-fix to `evidence/qa-gates/`, with an `EVIDENCE_LOCATION_OVERRIDE_REJECTED:` line in the plan header. P1-T1 allows a fail-before exception dossier when live-host capture is impossible.
- **Testable layering:** host-neutral core in `UtilitiesCS/OutlookObjects/Folder/` (StateModel, RenderProjection, BridgeMessages, BridgeRouter, SelectionMap) mirroring `FolderTreeStateModel`; QuickFiler seams `IWebViewMessenger` + exempt `WebView2Messenger` + non-exempt `BreadcrumbBridgeCoordinator` (tested with Moq'd messenger). Legacy arrow fall-throughs preserved via `UnhandledArrow` event rerouted in `KeyboardHandler.cs:543-583` (reroute, not removal).

**Why:** these came from repo verification (csproj greps) and spec/guardrail reconciliation during planning; a revision loop that moves JSON code into QuickFiler or drops the P2-T1 gate would break G2/G6.

**How to apply:** when revising this plan after `PREFLIGHT: REVISIONS REQUIRED`, update the same file in place and re-verify these four decision points survive the delta. Related: [[evidence-path-normalization]], [[plan-validator-task-id-sequential-constraint]], [[project_legacy_csproj_explicit_compile_include]].

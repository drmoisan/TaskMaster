# F4 Preflight Validation (atomic-executor)

- Timestamp: 2026-07-07T18-00
- Plan: docs/features/active/2026-07-07-store-lockup-detect-notify-264/plan.2026-07-07T18-00.md
- Directive: PREFLIGHT VALIDATION ONLY (no edits, no toolchain, no Phase 0 execution)
- Verdict: PREFLIGHT: REVISIONS REQUIRED

## Structure — PASS
- Canonical `### Phase N — <Title>` headings, Phases 0–9.
- Sequential `[P#-T#]` IDs per phase (P0-T1..T10, P1..P9 sequential).
- Phase 0 includes policy reads (P0-T1/T2), AC-source confirmation (P0-T3), hard fail-closed prerequisite gate (P0-T4), and baselines (P0-T5..T10: git, csharpier, analyzer, nullable, test+coverage, file-size).
- Final QA loop present (Phase 9: P9-T1..T4 restart rule, coverage delta P9-T5, file-size P9-T6, AC reconciliation P9-T7).
- Evidence paths all resolve to `<FEATURE>/evidence/<kind>/`; no forbidden `artifacts/*` evidence paths.

## Atomicity + AC mapping — PASS
- AC1–AC10 each mapped in the traceability table; tasks are single-outcome.

## Grounding of existing seams — VERIFIED
- UtilitiesCS/Threading/ThreadMonitor.cs: dormant watchdog, `Thread.Sleep`-driven, no injected clock (ctor lacks TimeProvider). Matches plan.
- UtilitiesCS/Threading/UiThread.cs:48 constructs `new ThreadMonitor(...)`; ThisAddIn.cs:28 `UiThread.Init(monitorUiThread: false)`. Matches plan P8-T1/T2.
- IUiDispatcher.cs exposes `BeginInvoke(Action)`, `Invoke`, `InvokeAsync`. Matches plan.
- MyBox.cs = 415 lines; `ReplaceButtons`/`AppendButtonInColumn` are `internal static` (justifies in-assembly composition). Minor grounding inaccuracy: plan/spec state `ActionButton` is `internal`, but `ActionButton` is `public` (UtilitiesCS/Dialogs/ActionButton.cs:13). The InternalsVisibleTo rationale still holds via the internal ReplaceButtons/AppendButtonInColumn. Non-blocking.
- EfcHomeController.cs:294 `internal Action<EfcViewer> ViewerShowAction { get; set; } = viewer => viewer.Show();`. Matches the injectable showAction pattern.
- StoreWrapper.Init(): DisplayName@36, GetRootFolder@42, GetSmtpAddressFromStore@60. Matches P7-T1.
- StoresWrapper.RewireOlObjectsAsync@83: Task.Yield@98, storeDisplayName@102, Init()@108/Restore@114. Matches P7-T2.
- AppOlObjects.EmitPerStoreInboxAttribution@204 (`internal static`, COM-free via injected delegates): displayName@211, getDefaultFolder@229. Matches P7-T3/T5. AppOlObjects.cs = 525 lines (over 500 cap); P7-T4 remediation is correctly conditional.
- csproj wiring: legacy explicit `<Compile Include>` confirmed (UtilitiesCS.csproj 436 items incl. Threading\*; UtilitiesCS.Test.csproj 396 items; TaskMaster.csproj explicit AppGlobals items). Each new-file task requires an explicit `<Compile Include>`.
- Epic contract confirms F1 service (StoreIdentity.Resolve, DisableSessionOnly/DisableForFutureSessions/ReenableAsync/IsDisabled) and F1-orchestrates-F3 rehook. Matches plan.

## Dependency check — FAIL (blocking)
- `Microsoft.Bcl.TimeProvider` 10.0.7 IS referenced by both UtilitiesCS and UtilitiesCS.Test (provides `System.TimeProvider`).
- `Microsoft.Extensions.TimeProvider.Testing` (provides `FakeTimeProvider`) is referenced ONLY by QuickFiler.Test (QuickFiler.Test.csproj:210-212, package version 9.0.0, net462). It is NOT referenced by UtilitiesCS.Test.
- The plan places FakeTimeProvider-based tests (P3-T3, ThreadMonitorTests) in UtilitiesCS.Test and includes no package-add task. spec.md Detection Design claims "FakeTimeProvider, already referenced by UtilitiesCS.Test" — this claim is false and will cause P3-T3 to fail to compile.

## Design fidelity / Determinism — PASS
- ThreadMonitor extended in place; injected TimeProvider + lockupAttributionThresholdMs=5000 + onLockupDetected; pure LockupStallDecider. Static volatile CurrentStoreContext (not AsyncLocal). guard->DisableSessionOnly->WARN->BeginInvoke modeless notify calling only F1. Three buttons to F1, never F3. monitorUiThread:true flagged RISK with single-line rollback. AppOlObjects partial split respected.
- No real waits/timers, no temp files, no live Outlook in test tasks.

## Re-check after revision — PASS (verdict upgraded to ALL CLEAR)

- Timestamp (re-check): 2026-07-07T18-00
- The prior blocking Dependency-check FAIL is resolved. Re-validation of the four fix confirmations:

1. New P3-T1 (package add) — VERIFIED.
   - packages.config entry `<package id="Microsoft.Extensions.TimeProvider.Testing" version="9.0.0" targetFramework="net481" />`; net481 matches every other UtilitiesCS.Test/packages.config entry and QuickFiler.Test/packages.config line 66-69.
   - `<Reference>`/`<HintPath>` in the task text is byte-exact to QuickFiler.Test.csproj:210-212 (`Version=9.0.0.0`, `PublicKeyToken=31bf3856ad364e35`, HintPath `..\packages\Microsoft.Extensions.TimeProvider.Testing.9.0.0\lib\net462\Microsoft.Extensions.TimeProvider.Testing.dll`).
   - Restore step present (`nuget restore` / `msbuild /t:Restore`).
   - Downstream dependency declared: P3-T4 (`Depends on P3-T1`), any Phase 5/6 clock-driven tests, and Phase 9 QA build/test.
   - Confirmed UtilitiesCS.Test does NOT currently reference the testing package (absent from packages.config and csproj); `Microsoft.Bcl.TimeProvider` IS present in both UtilitiesCS and UtilitiesCS.Test, so the "no production dependency change" claim holds.

2. Phase 3 IDs sequential — VERIFIED. P3-T1..P3-T4 with no gaps; no dangling references to the old numbering anywhere in the plan (grep `P3-T\d` = lines 66-69 tasks + line 123 AC1 row only). AC1 traceability row updated and matches: `P2-T1/T3, P3-T1, P3-T2/T3, P3-T4`.

3. spec.md corrected — VERIFIED. Detection Design now states the testing package "is currently referenced only by QuickFiler.Test; this feature therefore ADDS Microsoft.Extensions.TimeProvider.Testing (9.0.0) to UtilitiesCS.Test". The false "already referenced" claim is gone.

4. No new inconsistencies — VERIFIED. Canonical `### Phase N — <Title>` headings intact; Phase 0 prerequisite gate (P0-T4, fail-closed) + baselines (P0-T5..T10) intact; determinism preserved (FakeTimeProvider only, no real waits/timers, no temp files, no live Outlook); monitorUiThread:true remains the P8-T1 RISK task with single-line rollback and risk artifact. ActionButton correctly described as `public` (ActionButton.cs:13) with the in-assembly rationale carried by the internal ReplaceButtons/AppendButtonInColumn helpers.

- F1/F3 contract absence on this branch is expected (wave 2 barrier gated fail-closed by P0-T4); not a preflight failure.

- Re-check verdict: PREFLIGHT: ALL CLEAR

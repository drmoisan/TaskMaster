# [P0-T2] Phase 0 Instructions Read — Baseline Evidence

- **Issue:** #424
- **Task:** [P0-T2]
- **Work Mode:** full-bug (marker `- Work Mode: full-bug` confirmed at `issue.md:12`)
- **AC Source:** `spec.md` `## Acceptance Criteria` only (13 items) — per `acceptance-criteria-tracking` full-bug row

Timestamp: 2026-08-06T22-16

Policy Order:
1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/architecture-boundaries.md`

## Files read (in order)

### Policy documents

| # | Path | Read | Key constraints extracted for this work |
|---|---|---|---|
| 1 | `CLAUDE.md` | yes | Policy compliance order; C# toolchain order format → lint → type-check → test with restart-on-change; MSTest + Moq + FluentAssertions mandatory; 500-line file limit; tone policy. |
| 2 | `.claude/rules/general-code-change.md` | yes | Simplicity first; separation of pure logic from I/O; fail fast, no silent error swallowing; 500-line hard limit for production, test, and reusable script files; no temp files in tests. |
| 3 | `.claude/rules/general-unit-test.md` | yes | Independence, isolation, determinism; AAA structure; banned in test code: `Thread.Sleep`, `Task.Delay`, real wall-clock waits; `TimeProvider`/`FakeTimeProvider` required for time; tests live in a mirroring test tree, never colocated in production source. |
| 4 | `.claude/rules/csharp.md` | yes | CSharpier formatting (no `dotnet format`); the five-analyzer stack; nullable + `TreatWarningsAsErrors` gate; repo line coverage >= 80%, new module/class/method >= 90%, changed-line regression is blocking; DI seam preference order (interface > injectable delegate > adapter); `TimeProvider` seam guidance; prohibited: weakening assertions, adding sleeps/retries to mask flakiness. |
| 5 | `.claude/rules/architecture-boundaries.md` | yes | No-COM assertions for **new runtime code** — no `Microsoft.Office.Interop.Outlook`, no VSTO `Microsoft.Office.Tools.*`, no `[ComVisible(true)]`, no new Outlook event-stream dependencies. Applied to this work: the new `QfcScanProgressBandMapper` module must contain zero COM/VSTO/UI references (enforced by [P4-T2] acceptance). Pre-existing legacy VSTO code in `QuickFiler` is not newly introduced runtime code. |

### Requirements documents

| # | Path | Read | Role |
|---|---|---|---|
| 6 | `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/spec.md` | yes | Authoritative AC source — 13 unchecked acceptance criteria; scope/non-goals; proposed fix design summary; boundaries and invariants; test strategy. |
| 7 | `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/issue.md` | yes | Intake, repro steps, environment, work-mode marker. |
| 8 | `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/research/2026-08-06T22-00-quickfiler-high-confidence-queue-init-stall-research.md` | yes | Root-cause validation; latency model (§3); latent `sourceActive` defect (§2.8); recommended approach (§7); behavior semantics (§8); settings-surface rationale (§9); test strategy (§10); explicit out-of-scope list (§11). |

## Conflicts identified and resolution

1. **Coverage thresholds.** `.claude/rules/csharp.md:39-41` states repo line >= 80% and new module >= 90%. `.claude/rules/general-unit-test.md` states a uniform >= 85% line / >= 75% branch across tiers. Per plan Decisions Record item 7 the binding gate for this plan is **80/90/no-regression**; both figure sets are recorded numerically in `[P6-T5]` and the conflict is restated there for the reviewer. No policy document is modified.
2. **CSharpier invocation.** `.claude/rules/csharp.md:14` and `CLAUDE.md` both give `dotnet tool run csharpier .` (v0 syntax). The installed tool is csharpier **1.2.6**, whose CLI requires `format` / `check` subcommands. Per plan Decisions Record item 11 the runnable equivalents `csharpier format .` / `csharpier check .` are used. This is an invocation-form adaptation, not a policy deviation; the policy requirement (all `*.cs` formatted by CSharpier, never `dotnet format`) is satisfied unchanged.

## Out-of-scope items acknowledged (record only, do not fix)

Per `spec.md` Scope & Non-Goals and research §11, and reaffirmed by the execution directive:
- `EmailMoveMonitor` hook retention for gate-rejected items.
- Post-`Show()` double-scoring of accepted items / dormant `QfcPreScoredItem` carrier path.
- `Worker_RunWorkerCompleted` early UI enablement and any further `BackgroundWorker` lifecycle rework.
- Legacy synchronous `Run()`/`Iterate()`/`DequeueNextItemGroup` paths.
- Frame building (`InitDf*`/`DfDeedle`).
- Any change under `.claude/rules/**`, policy documents, `QfSettings`/`IAppQuickFilerSettings`, `Settings.Designer.cs`, or `TaskMaster/Ribbon/`.

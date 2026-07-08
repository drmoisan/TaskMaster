# QA Gate 03 — Nullable / TreatWarningsAsErrors (P9-T3)

Timestamp: 2026-07-08T08-27

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

Output Summary:
- Build succeeded. 0 Warning(s), 0 Error(s). Up-to-date incremental no-op (~0.8s), identical to the
  P0-T8 baseline behavior: this legacy net48 solution's `/t:Build` up-to-date check does not
  recompile on the `Nullable`/`TreatWarningsAsErrors` property change alone. Gate result is
  unchanged baseline -> post-change (PASS -> PASS); F4 does not regress it.

No-regression evidence (diagnostic Rebuild):
- A `/t:Rebuild /p:Nullable=enable` (TreatWarningsAsErrors OFF, to enumerate rather than fail-fast)
  was run to inspect F4's new files under an active nullable context. F4's new files emit only the
  same nullable-context-off pattern warnings the existing codebase emits (CS8618 uninitialized
  field, CS8625 null default argument, CS8603/CS8600 null conversion) — identical to F1's
  `StoreIdentity.Resolve(..., string filePathFallback = null)`. No new nullable-flow bug was
  introduced; the one CS8602 in `ThreadMonitor` (line 144) is inside the preserved, unchanged
  original `[ExcludeFromCodeCoverage]` diagnostic host shell (`dispatcher.InvokeAsync`), not new
  code.
- The whole-repo `/t:Rebuild /p:Nullable=enable` reported 5 pre-existing `CS8630` errors ("Invalid
  'nullable' value: 'Enable' for C# 7.3") from legacy C# 7.3 projects (e.g. QuickFiler.Test) that
  cannot compile under global `Nullable=enable`. These are pre-existing and repo-wide, NOT F4 files.
  This is why the plan's `/t:Build` nullable gate is a no-op by design.

# P4-T16 — Analyzer/Nullable Build After Full Wiring (Issue #181) — BLOCKING FINDING

Timestamp: 2026-06-08T12-58

## Step 3 — nuget restore
Command: nuget.exe restore TaskMaster.sln
EXIT_CODE: 0
Output Summary: clean; all 6 analyzer packages present in packages/.

## Step 4 — Analyzer / code-style build (full analyzer wiring)
Command: msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary: Build succeeded. 0 errors. 466 warnings (new analyzer diagnostics surfacing as messages/warnings — expected, analyzers are active and loading EXCEPT SecurityCodeScan). NOTE: this step also emits CS8032 analyzer-load warnings from SecurityCodeScan (960 instances across the full rebuild) but does not fail because this step does not set TreatWarningsAsErrors.

## Step 5 — Nullable TreatWarningsAsErrors build (PROTECTED GATE) — REGRESSION
Command: msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 1
Output Summary: 100 errors total (baseline P0-T5 = 84). REGRESSION of +16 errors.
- Baseline 84 errors: UtilitiesSwordfish.NET.General.csproj (50) + SVGControl.csproj (34) — vendored, unchanged.
- NEW 16 errors: all CS8032 in VBFunctions.csproj (the first-party project that recompiled in the rebuild). CS8032 = analyzer instance cannot be created.

## Root cause
- SecurityCodeScan.VS2019 5.6.7 analyzer types fail to initialize: TypeInitializationException -> FileNotFoundException for `YamlDotNet, Version=11.0.0.0, PublicKeyToken=ec19458f3c15af5e`.
- YamlDotNet 11.0.0.0 IS co-located in packages\SecurityCodeScan.VS2019.5.6.7\analyzers\dotnet\YamlDotNet.dll and WAS additionally wired as an <Analyzer Include> in all 15 first-party .csproj, but the Roslyn 5.6 analyzer load context does NOT resolve SecurityCodeScan's `Assembly.Load(YamlDotNet, 11.0.0.0)` from a sibling <Analyzer> entry. SecurityCodeScan.VS2019 5.6.7 is incompatible with the Roslyn 5.6.0 analyzer loader in this build toolchain (VS18). This matches the research's flagged MEDIUM risk (open question #2: SecurityCodeScan.VS2019 vs current Roslyn compatibility).
- CS8032 is a COMPILER warning (not an analyzer rule), so it is NOT controllable via `.editorconfig` analyzer-rule severities (dotnet_diagnostic.<rule>.severity = suggestion). Under /p:TreatWarningsAsErrors=true it is promoted to an error and breaks the protected gate.

## Why this is a BLOCKING finding (scope-change stop)
The plan fixes the 6-package set including SecurityCodeScan.VS2019 and requires the nullable gate to NOT regress. SecurityCodeScan.VS2019 5.6.7 cannot load under this Roslyn without emitting CS8032, which cannot be neutralized by the plan's authorized mechanism (analyzer-rule severity = suggestion). Resolving it requires a decision the plan does not authorize:
- Option A: Suppress CS8032 (e.g., `.editorconfig` dotnet_diagnostic.CS8032.severity = none, or per-project <WarningsNotAsErrors>$(WarningsNotAsErrors);CS8032</WarningsNotAsErrors>). This is an UNAUTHORIZED SUPPRESSION; it also masks genuine analyzer-load failures for ALL analyzers, and SecurityCodeScan would silently never run.
- Option B: Remove SecurityCodeScan.VS2019 from the 6-package set (or replace with the non-VS2019 `SecurityCodeScan` package id / a Roslyn-5.x-compatible variant). This is a SCOPE REDUCTION/CHANGE to the fixed package set defined in the plan.
- Option C: Pin the build to an older Roslyn that SecurityCodeScan 5.6.7 supports. Out of scope (changes build toolchain) and not feasible (toolchain is the installed VS18).

Per the executor scope-change rule, execution STOPS here and returns control with this finding. No unauthorized suppression has been applied. All other 5 analyzers (Meziantou, SonarAnalyzer, Roslynator, AsyncFixer, BannedApiAnalyzers) load and run correctly and do NOT regress the gate (they are all at suggestion severity).

## State on disk at stop
- .editorconfig: severities for all 6 packages at suggestion (Phase 2 complete, verified non-regressing at P2-T9).
- BannedSymbols.txt: created (Phase 3).
- All 15 first-party packages.config: 6 analyzer entries each (Phase 3 complete, restore clean at P3-T17).
- All 15 first-party .csproj: analyzer ItemGroup wired including SecurityCodeScan + YamlDotNet (Phase 4 P4-T1..T15 applied).
- Vendored projects: untouched (verified).
- The ONLY thing preventing the protected nullable gate from holding at the 84-error baseline is the SecurityCodeScan CS8032 load failure (+16 errors, all CS8032).

## Recommended plan delta (for atomic-planner / approver)
Replace the SecurityCodeScan wiring approach in Phase 3/Phase 4 with ONE of:
1. Remove SecurityCodeScan.VS2019 from the 6-package set for this rollout (reduce to 5 analyzer packages), documenting it as deferred follow-up pending a Roslyn-5.x-compatible SecurityCodeScan variant. This keeps the gate clean with no suppression.
2. Authorize a narrowly-scoped, documented suppression of CS8032 SPECIFICALLY for the SecurityCodeScan YamlDotNet load failure (e.g., per-project WarningsNotAsErrors=CS8032), accepting that SecurityCodeScan will not actually run until a compatible variant is available. (Less preferred: masks load failures.)
3. Identify and substitute a SecurityCodeScan package id/version that loads under Roslyn 5.6 (requires planner authorization to change the fixed package id).

---

# REVISION 2.0 — Post-Cleanup Verification (SecurityCodeScan dropped)

Timestamp: 2026-06-08T13-30

Revision 2.0 adopted plan delta option 1: SecurityCodeScan.VS2019 is removed entirely from the rollout (5-analyzer stack). Phase 2 (P2-T8) removed the SCS `.editorconfig` severities; Phase 3 (P3-T2..P3-T16) removed the SecurityCodeScan.VS2019 `<package>` entries from all 15 first-party packages.config; Phase 4 (P4-T1..P4-T15) removed the SecurityCodeScan + co-located YamlDotNet `<Analyzer Include>` items from all 15 first-party .csproj. This section re-verifies the protected gate after that cleanup. No CS8032 suppression was introduced.

## P4-T16 — No-regression verification after SecurityCodeScan removal

### Step 1/2 — format/tool restore
`dotnet tool restore` EXIT_CODE 0. CSharpier verification handled in final QA (P6-T1); see the P2-T9 note regarding the 30 v1.0-edited XML project files and the 1 pre-existing baseline `.cs` file.

### Step 3 — nuget restore
Command: `nuget.exe restore TaskMaster.sln`
EXIT_CODE: 0
Output Summary: clean; the 5 in-scope analyzer packages present in packages/.

### Step 4 — Analyzer / code-style build (5-analyzer wiring)
Command: `msbuild TaskMaster.sln -t:Rebuild -p:Configuration=Debug "-p:Platform=Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Error(s), 61 Warning(s). **0 instances of CS8032** (SecurityCodeScan no longer loaded). The 5 in-scope analyzers are active at suggestion severity (message level). No in-scope analyzer diagnostic is promoted to error.

### Step 5 — Nullable TreatWarningsAsErrors build (PROTECTED GATE)
Command: `msbuild TaskMaster.sln -t:Rebuild -p:Configuration=Debug "-p:Platform=Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true`
EXIT_CODE: 1
Output Summary: **84 Error(s)** — equal to the Phase 0 baseline (P0-T5 = 84). **0 instances of CS8032.** The +16 CS8032 regression recorded in the v1.0 blocking finding above is ELIMINATED.
- All 84 errors confined to the two vendored projects: SVGControl (68 distinct error lines) + UtilitiesSwordfish.NET.General (100 distinct error lines); each error emitted twice under parallel build, reconciling to the 84-error MSBuild summary.
- First-party error check: a filter of all `error CS` lines excluding SVGControl/UtilitiesSwordfish returned ZERO first-party project error entries.

Verdict P4-T16: PASS. Analyzer/code-style build succeeds with 0 errors and no CS8032; the protected nullable gate returns to the 84-error vendored-only baseline with no regression and no CS8032. Success condition met.

## P4-T17 — TaskMaster VSTO/COM diagnostic check

From the P4-T16 step-4 analyzer build output, filtered to TaskMaster project diagnostics: NO in-scope analyzer diagnostic (MA*, S####, RCS*, AsyncFixer*, RS0030) from the 5 packages is promoted to error in `TaskMaster\TaskMaster.csproj` (the VSTO/COM interop project). The suggestion-severity mitigation held; the analyzer build reports 0 errors overall, and no TaskMaster `.cs` line produced an in-scope analyzer error. Verdict P4-T17: PASS.

## P4-T18 — RS0030 activation check

RS0030 is at `suggestion` severity in the committed `.editorconfig`; MSBuild's console logger does not surface message/suggestion-severity diagnostics at any verbosity. To deterministically confirm RS0030 is active and that BannedSymbols.txt doc-IDs resolve, RS0030 was TEMPORARILY elevated to `warning` for a single verification build of `UtilitiesCS\UtilitiesCS.csproj` (a project P1-T7 identified as containing banned-symbol usages), then immediately reverted to `suggestion`.

Command (temporary): `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` with `dotnet_diagnostic.RS0030.severity = warning` set temporarily.
Result: 60 RS0030 diagnostics emitted in UtilitiesCS. Representative matches (banned symbol + the BannedSymbols.txt remediation message):
- `IGenericTimer.cs(19,20): RS0030: The symbol 'DateTime.Now' is banned ... Inject System.TimeProvider and call GetLocalNow() ...`
- `AsyncSerialization.cs(41,27): RS0030: The symbol 'Task.Delay(TimeSpan)' is banned ... Inject a time abstraction (System.TimeProvider).`

This confirms RS0030 fires against known banned-symbol call sites and that the BannedSymbols.txt doc-IDs are correct. The `.editorconfig` was reverted to `dotnet_diagnostic.RS0030.severity = suggestion` immediately after; verified by `grep` (severity = suggestion) and `git diff` (no net RS0030 severity change committed). At the committed suggestion severity, RS0030 is a non-build-breaking message, consistent with the ordering invariant. Verdict P4-T18: PASS.

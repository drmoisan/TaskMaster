# Phase 0 — Policy Instructions Read (Issue #207 corrective fix, large path)

Timestamp: 2026-06-22T16-51

Policy Order:
1. CLAUDE.md (standing instructions — always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific toolchain and coding standards — files in scope are *.cs / *.csproj)
5. .claude/rules/ci-workflows.md (GitHub Actions pwsh exit-code rule — read for completeness per plan P0-T1)

Files read (explicit list):
- c:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\CLAUDE.md
- c:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\general-code-change.md
- c:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\general-unit-test.md
- c:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\csharp.md
- c:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\ci-workflows.md

Work Mode: full-bug (large path). AC source: spec.md `## Acceptance Criteria` (AC1-AC13).

Key constraints recorded for this fix (AC9, AC10, AC11):
- net48 target: no positional `record struct` (CS0518 IsExternalInit unavailable). Use enum / class / readonly struct with explicit constructor.
- Banned APIs (BannedApiAnalyzers RS0030, severity=suggestion): DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay. AC10 requires remediation within touched production files (incl. pre-existing Task.Delay(100) in AppEvents.ProcessNewInboxItemsAsync). Time -> injected System.TimeProvider (Microsoft.Bcl.TimeProvider backport present in UtilitiesCS); Task.Delay/Thread.Sleep -> DispatcherDelay.WaitAsync.
- C# toolchain order (restart on any change): CSharpier -> analyzers (EnableNETAnalyzers/EnforceCodeStyleInBuild) -> nullable/TreatWarningsAsErrors -> vstest with coverage filtered TestCategory!=LiveOutlook.
- Coverage: repo-wide >= 80%, new testable type >= 90%, no regression on changed lines. COM/VSTO + generated code exempt per CLAUDE.md.
- File size cap 500 lines (production/test).
- Cross-assembly visibility (Risk-(c) resolution verified): UtilitiesCS/Properties/AssemblyInfo.cs grants InternalsVisibleTo only to UtilitiesCS.Test and ToDoModel.Test, not TaskMaster; TaskMaster consumes UtilitiesCS by ProjectReference. UtilitiesCS seam types (IOutlookReadinessGate, OutlookReadinessGate) and the transient-HRESULT constants must be `public` for cross-assembly use from TaskMaster.

Output Summary: All five policy files read in the mandated order for the #207 corrective fix (full-bug large path). Preflight ALL CLEAR; Risk-(c) cross-assembly visibility delta verified against live AssemblyInfo.cs and TaskMaster.csproj ProjectReference. Scope locked to 15 files per plan scope-lock.

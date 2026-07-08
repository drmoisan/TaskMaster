# Remediation Inputs — Cycle 6 (Issue #181)

Entry timestamp: 2026-06-09T11-31
Feature folder: docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181
Branch: feature/csharp-analyzer-stack-181 (fold into existing #181 branch per user direction)
Base: main

## Trigger

User-reported defects that emerged during this development: test code uses prohibited
non-deterministic timing primitives (`Thread.Sleep`, `ManualResetEventSlim.Wait(<timeout>)`
/ `signal.Wait(<timeout>)`, and equivalent wall-clock waits), violating the repository
deterministic-test policy (`.claude/rules/csharp.md` "Deterministic Test Rules" and
"Prohibited Behaviors"; CLAUDE.md General/C# Unit Test Policy). The named failing test is
`Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite` in
`UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableBase_Tests.cs`. The user directed
converting ALL such usages to deterministic equivalents via mocking and/or minimal
production seams. These are treated as in-scope defects on the #181 branch (this supersedes
the earlier "pre-existing flaky timer tests, out of scope" deferral note from cycles 4-5).

## Authoritative design source

`artifacts/research/2026-06-09-deterministic-test-timer-seams.md` (task-researcher output)
contains the complete inventory (24 occurrences, 12 test files, groups A-L), the
test-only-vs-production-seam classification, the six production seams (S1-S6), and the
batching strategy. The plan MUST be built from this research; verify each row against
current source before editing.

## Working-tree preconditions (read carefully)

- `artifacts/` is gitignored; the research doc and orchestrator-state are untracked.
- THREE files are already modified in the working tree at cycle entry:
  1. `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableBase_Tests.cs` — user changed
     `signal.Wait(1000)` -> `signal.Wait(5000)` and added `Thread.Sleep(50)` to
     `Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite`. This is the IN-SCOPE
     current baseline for that test; convert it deterministically.
  2. `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs` and
  3. `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` — UNRELATED user WIP
     (DeleteMiddle single-element fix + regression test). These are OUT OF SCOPE: do NOT
     modify, do NOT revert, and do NOT include them in any commit produced by this cycle.
     Stage only the specific timer-determinism files; never `git add -A`.

## Scope — Deterministic conversion via six seams (from research S1-S6)

All seams are behavior-preserving: production defaults to current behavior; deterministic
behavior occurs only when a test injects a factory/hook. Existing interfaces
`ITimerWrapper`/`IGenericTimer` (`UtilitiesCS/Interfaces/`) are reused; NO new NuGet
packages (`FakeTimeProvider` is not required). The only new test artifact is a
`ManualFireTimerWrapper` test helper inside the test project.

- S1 — `protected Func<TimeSpan, ITimerWrapper> TimerFactory` seam on `SmartSerializableBase.cs`
  and `SmartSerializable.cs` (covers the named failing test first: groups A1-A3, plus C-group
  SmartSerializable cases). Add a deferred-write completion hook if needed for deterministic
  assertion without `signal.Wait`.
- S2 — `internal Func<TimeSpan, ITimerWrapper> TimerFactory` on `TimedQueueOfActions.cs`.
- S3 — optional `timerFactory` parameter on the `AsyncMultiTasker.cs` overloads (distinguish
  work-simulation sleeps from real synchronization per research).
- S4 — optional `onItemCompleted` hook in `IEnumerableExtensions` `ToList`/`WithProgressReporting`
  and `SubjectMapSco.Orchestration.cs` `Consume` (check current #181 partial state first).
- S5 — `int timeoutMs = <default>` parameter on `OlTableExtensions.TableAccess.cs`
  `GetTableInViewAsync` (partial seam) to eliminate the 2100 ms sleep.
- S6 — expose a timer-factory constructor on `FolderRemapTree.cs` batch notifier.
- Test-only fixes (no production change): TimerWrapper_Tests deterministic drive, ConfigController
  STA pump, the work-simulation `Thread.Sleep` removals, SegmentStopWatch (confirm whether it needs
  an injected clock seam or can assert deterministically), Bayesian classifier sleeps, etc. — per
  research classification (13 occurrences).

## Constraints (hard)

- Allowed delegates this cycle: `atomic-planner`, `atomic-executor`, `feature-review` only.
  No direct typed-engineer worker invocation by the orchestrator.
- Convert prohibited timing to deterministic seams ONLY. Do NOT weaken or delete assertions;
  do NOT re-add `[Ignore]`; do NOT add sleeps/retries/timing slack as the "fix"; do NOT mask
  flakiness — the assertions' intent (completion ordering, debounce/interval behavior,
  elapsed-time thresholds, async signal) must be preserved deterministically.
- Production seams must be behavior-preserving (default to current runtime behavior; no
  banned symbol introduced; nullable-clean; analyzer-clean).
- No new NuGet packages; no `.editorconfig`/`.globalconfig`/vendored/`BannedSymbols.txt`/
  analyzer-wiring/`.claude/rules/` changes.
- Respect the C# typed-work per-batch cap (3 production + 3 test files per batch); phase the
  plan into batches accordingly, ordered to fix
  `Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite` FIRST (S1/batch 1).
- Use MSTest + Moq + FluentAssertions. No temp files. No external dependencies.
- Mandatory C# toolchain in exact order, passing in one final pass: `dotnet tool run
  csharpier .` -> `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` ->
  `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` -> `vstest.console.exe
  <first-party Test.dll> /EnableCodeCoverage /InIsolation`. Restart from csharpier on any
  change/failure. (Note: `/InIsolation` is required for Moq assemblies per cycle-5 evidence.)
- Zero regression across the full first-party suite; coverage must not drop for changed lines
  (>= 80% repo, >= 90% new/changed). Required CI check must be green against branch head after
  push.
- Do NOT relocate or reorganize existing committed feature-folder artifacts; keep the flat
  artifact layout.
- Preserve the OUT-OF-SCOPE StackGeek working-tree changes untouched and excluded from commits.

## Acceptance for Cycle Exit

- `Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite` passes deterministically with
  no `Thread.Sleep`/`signal.Wait(<timeout>)`.
- Every prohibited timing occurrence cataloged in the research doc is converted to a
  deterministic equivalent (or, if any single occurrence genuinely cannot be made deterministic
  without an out-of-scope refactor, it is HALTED as a scope-change finding for a new cycle, not
  silently left or masked).
- Full toolchain passes in one final pass; zero regression; coverage gate met.
- Three reaudit artifacts (code-review, feature-audit, policy-audit) by `feature-review` with
  `blocking_count == 0`.
- Required CI check green against branch head after push.

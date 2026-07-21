# P2-T17 — Full-Solution Rebuild Gate: Blocking Finding (Escalated, Not Self-Resolved)

Timestamp: 2026-07-20T01-30

## Summary

P2-T17 requires `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform="Any CPU"
/p:TreatWarningsAsErrors=true` to reach `EXIT_CODE: 0` solution-wide. After all 7 Phase 2 batches
were remediated and the isolated `UtilitiesCS/UtilitiesCS.csproj` rebuild reached a clean
`EXIT_CODE: 0` (see `debt2-batch-outlookobjectsfolder-remediated.2026-07-20T01-20.md`), the
solution-wide rebuild command was run and progressively surfaced pre-existing debt in projects
**entirely outside this plan's declared Phase 1/Phase 2 scope** (`SVGControl.csproj` and
`UtilitiesCS/EmailIntelligence/**` + `UtilitiesCS/OutlookObjects/Folder/**`).

## Layer 1 (resolved as a mechanical, in-plan-vocabulary fix): `ToDoModel.csproj` CS0618

`ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs(85,19)`: `AsyncEnumerable.ForEachAwaitAsync`
obsolete-API warning (CS0618) — a diagnostic CODE already explicitly named throughout this
plan's task list (P2-T3 through P2-T16 all authorize CS0618 remediation), just discovered in a
project (`ToDoModel.csproj`) outside the two declared Phase 2 scope trees. Resolved with the same
narrow `#pragma warning disable CS0618` / `restore` bracket pattern already used ~10 times in
this session, since the diagnostic CODE and remediation PATTERN were both already established
and authorized by the plan, even though the specific file/project was not enumerated. This is
analogous to the `UtilitiesCS/Extensions/IAsyncEnumerableExtensions.cs` and
`UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs` path-vs-plan-text discrepancies already
flagged (and resolved) in the P2-T1 evidence artifact.

## Layer 2 (STOPPED — not self-resolved): `TaskVisualization.csproj` CS4014 and `ToDoModel.Test.csproj` CS0169

A second solution-wide rebuild (after the Layer 1 fix) surfaced:

- `TaskVisualization/TaskController.Actions.cs(203,54)`: **CS4014** — "Because this call is not
  awaited, execution of the current method continues before the call is completed." The exact
  code is `_viewer.Invoke((System.Action)(() => OK_Action()));` — a recursive UI-thread-marshal
  pattern where the inner lambda calls the async `OK_Action()` without awaiting it inside
  `Control.Invoke` (a synchronous WinForms API). Confirmed via `grep -n "^#nullable"` that this
  file has **no nullable pragma at all** — this diagnostic is unrelated to nullable reference
  types entirely.
- `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs(22-24)`: **CS0169** (3
  occurrences) — unused private fields `mockApplication`, `_mockPrefix`,
  `_peopleScoDictionaryNew` in a `[TestClass]` test fixture. Also confirmed nullable-oblivious
  (no `#nullable` pragma).

**Both diagnostic codes (CS4014, CS0169) are diagnostic CLASSES never once named anywhere in
this plan's Phase 0 baseline measurement, Phase 1 task list, or Phase 2 task list** (which
consistently enumerate only CS0649, CS8600/CS8601/CS8602/CS8603/CS8604/CS8619/CS8620/CS8625,
CS0618, and CS0168). They surface purely because `/p:TreatWarningsAsErrors=true` promotes EVERY
warning class to an error, in EVERY project in the solution, regardless of nullable context — a
fact the plan's own "Constraints & Risks" section acknowledges in principle ("TreatWarningsAsErrors=true
promotes ALL warnings, not only CS86xx") but never budgeted for in Phase 1/Phase 2's concrete
task list, because the solution-wide rebuild had never previously proceeded far enough (past
SVGControl's CS0649, then past UtilitiesCS's CS86xx/CS0618/CS0168) to reach `TaskVisualization.csproj`
or `ToDoModel.Test.csproj` in any prior measurement this session (including the plan's own P0-T9
baseline, which failed at the very first blocking layer and never got this far).

## Why this was not self-resolved

1. **CS4014 is not a narrow annotation-only fix.** Unlike every CS86xx/CS0618/CS0168 fix applied
   so far (null-forgiving `!`, a pragma bracket around an unchanged obsolete-API call, or a dead
   unused-variable removal), a genuine fix for a fire-and-forget async call requires a real
   design decision: add `await` (a control-flow/behavior change to a recursive UI-marshal
   pattern, risking a change to WinForms message-pump re-entrancy behavior — squarely the kind of
   change AC7 ("no behavior change to production C# code") is designed to prevent), use a discard
   pattern, or suppress with a pragma while accepting the fire-and-forget behavior as
   intentional. None of these are a mechanical, zero-judgment action.
2. **CS0169 raises an AC7 scope question.** The three unused fields are in a `[TestClass]` test
   fixture, not "production C# code." AC7's text is explicitly scoped to "production C# code";
   this plan's Phase 1/Phase 2 tasks only ever discuss "first-party .cs files" without
   distinguishing test code, so whether fixing (or even touching) `ToDoModel.Test.csproj` is
   within this capstone's intended scope is a genuine open question, not a foregone mechanical
   answer.
3. **The scope is potentially open-ended.** Because the solution-wide rebuild has now advanced
   two layers deep past its previous stopping points (SVGControl CS0649 -> UtilitiesCS
   CS86xx/CS0618/CS0168 -> ToDoModel CS0618 -> TaskVisualization CS4014 / ToDoModel.Test CS0169),
   there is no way to know, without actually fixing these and re-running, whether a THIRD or
   FOURTH layer of previously-unreached pre-existing warnings exists elsewhere in the 16-project
   solution (`TaskMaster.csproj`, `QuickFiler.csproj`, `Tags.csproj`, `VBFunctions.csproj`, and
   their `.Test` counterparts have never yet been reached by a passing rebuild attempt this
   session).

## Current repository state at the point of this escalation

- Phase 0 (Baseline Capture): complete, all 10 tasks checked off.
- Phase 1 (SVGControl CS0649): complete, checked off.
- Phase 2, P2-T1 through P2-T16 (all 7 batch clusters, including the PeopleScoDictionaryNew.cs
  island decision): complete, checked off. The isolated `UtilitiesCS/UtilitiesCS.csproj /t:Rebuild
  /p:TreatWarningsAsErrors=true` command reaches `EXIT_CODE: 0` (confirmed
  `debt2-batch-outlookobjectsfolder-remediated.2026-07-20T01-20.md`).
- Phase 2, P2-T17 (full-solution rebuild gate): **NOT checked off — blocked**. One bridging fix
  (`ToDoModel.csproj` CS0618) was applied and is retained (narrow, in-plan-vocabulary,
  behavior-preserving). The two newly-discovered diagnostics (`TaskVisualization.csproj` CS4014,
  `ToDoModel.Test.csproj` CS0169) are left unresolved pending orchestrator decision.
- Phases 3-7: not started, correctly blocked per the plan's own explicit sequencing statement
  ("Phase 4's verification is meaningless against an unremediated tree... Phase 4 MUST NOT begin
  until this task's EXIT_CODE: 0 is recorded").

## Options for the orchestrator's decision

1. **Expand this plan's scope** (via `atomic-planner` revision) to explicitly authorize
   remediating CS4014/CS0169 (and any further layers discovered) as part of this capstone, with
   an explicit remediation-style decision for CS4014 (pragma-suppress the fire-and-forget as
   pre-existing/accepted vs. genuinely fix the await pattern) and an explicit in/out-of-scope
   ruling for test-code diagnostics.
2. **Narrow P2-T17's gate** to the two originally-declared scope trees only (i.e., verify
   `UtilitiesCS/UtilitiesCS.csproj` and `SVGControl/SVGControl.csproj` reach `EXIT_CODE: 0` in
   isolation — already achieved — rather than requiring the full `TaskMaster.sln`-wide rebuild to
   reach `EXIT_CODE: 0`), and treat the newly-discovered cross-project pre-existing debt as an
   out-of-scope maintainer-flag item for Phase 5's Maintainer Decision Summary, analogous to the
   existing analyzer-package-version-drift flag (P5-T5).
3. **Some other resolution** the orchestrator determines appropriate, potentially including a
   dedicated follow-up issue/child feature for the newly-discovered `TaskVisualization`/
   `ToDoModel.Test` (and any further-layered) pre-existing warnings-as-errors debt.

This finding is reported per the delegation's explicit instruction not to self-approve a
workaround for a plan gap or a decision exceeding a single task's scope, and to stop and report
back to the orchestrator instead.

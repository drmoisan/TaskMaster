# P2-T17 — ToDoModel.csproj CS0618 Remediation (Layer 1, confirm/record)

Timestamp: 2026-07-20T02-00

## Summary

Confirms and formally records the Layer 1 fix already applied this session (see
`p2t17-blocking-finding.2026-07-20T01-30.md`) to
`ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs` line 85, the
`AsyncEnumerable.ForEachAwaitAsync` obsolete-API call (CS0618).

## Pattern applied

Narrow `#pragma warning disable CS0618` / `#pragma warning restore CS0618` bracket around the
single obsolete-API call, with an inline rationale comment citing that replacing the call with
`await foreach` would be a control-flow change (out of scope per AC7 no-behavior-change), whereas
the pragma bracket preserves the exact pre-existing behavior. No other line in the file is
changed.

## Verification

Command: `MSBuild.exe ToDoModel/ToDoModel.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

Output Summary: Build succeeded, 0 Warning(s), 0 Error(s). CS0618 no longer appears for
`ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs`. `git diff` for this file (confirmed
separately) shows only the pragma bracket and its inline rationale comment added around the
existing `ForEachAwaitAsync` call at line ~85; no other line differs from the pre-edit state.

## Rationale

CS0618 is a diagnostic code already explicitly authorized for remediation throughout this plan's
Phase 2 task list (P2-T3 through P2-T16 all permit CS0618 fixes via the same narrow-pragma
pattern); this file/project was simply not enumerated in the plan's originally-declared scope
trees. The fix uses an already-established, non-behavioral pattern.

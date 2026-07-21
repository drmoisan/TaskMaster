# Phase 0 — Upstream Ordering Precondition (#363 Batch D)

- Timestamp: 2026-07-19T10-53
- Task: [P0-T3]
- Issue: #374

## Precondition

Phase 1 execution must not begin until issue #363 (`utilitiescs-nullable-extensions`) Batch D
(`UtilitiesCS/Extensions/WinFormsExtensions.cs`) has merged into this branch, so that the
`WinFormsExtensions.Clone<T>() where T : Control` signature this cluster compiles against is
already annotated. This cluster consumes that contract from `ActionButton.cs`, `DelegateButton.cs`,
`FunctionButton.cs`, and `MyBox.cs` (`ButtonTemplate` setter).

## Evidence

Command: `grep -n "#nullable" UtilitiesCS/Extensions/WinFormsExtensions.cs`

Result:
```
19:#nullable enable
```

`UtilitiesCS/Extensions/WinFormsExtensions.cs` carries `#nullable enable` at line 19.

## Finding

PRECONDITION SATISFIED. Issue #363 Batch D has merged into
`feature/utilitiescs-nullable-dialogs-misc-374` (based on the epic integration branch tip
`dffadd5a`, which includes #363's merged PR #379). Phase 1 execution is unblocked; the annotated
`Clone<T>` signature is present for this cluster to compile against.

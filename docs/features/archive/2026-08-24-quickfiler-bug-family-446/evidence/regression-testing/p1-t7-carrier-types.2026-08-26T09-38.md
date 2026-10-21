# [P1-T7] Carrier Types Introduced With No Consumer

Timestamp: 2026-08-26T09-38

Task: [P1-T7]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler/Interfaces/IQfcDatamodel.cs`:

- Added `public enum QfcDequeueStop { QuantitySatisfied, SourceExhausted, DeadlineExpired }`.
- Added `public readonly struct QfcDequeueBatch` with a constructor taking
  `IList<MailItem> items`, `IList<QfcPreScoredItem> preScored` and `QfcDequeueStop stop`.
  `Items` and `PreScored` each return an empty list when the backing field is null, so a defaulted
  struct returned by an unconfigured loose Moq setup is inert rather than a null-reference trap.
- Added `using QuickFiler.Controllers;`, required to reference `QfcPreScoredItem`.

`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`:

- Added `internal readonly struct QfcGateBatch` with `Accepted`, `Stop` and `Scanned` members.
  `Accepted` likewise coerces a null backing field to an empty list.
- Added `using QuickFiler.Interfaces;`, required to reference `QfcDequeueStop`.

All three types are plain `readonly struct` / `enum` with get-only properties, because `net481`
has no `IsExternalInit` and therefore no `record`, `record struct` or `init` accessor.

No consumer was added by this task: `DequeueAsync` still returns `Task<IList<MailItem>>` at this
point, and nothing constructs a `QfcGateBatch` or a `QfcDequeueBatch` yet.

## Verification

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0
Output Summary: build succeeded; no line matching `: error` in the output.

Command: `wc -l "QuickFiler/Interfaces/IQfcDatamodel.cs"`
EXIT_CODE: 0

| Path | Baseline | Post-change | Condition | Result |
| --- | --- | --- | --- | --- |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 59 | **118** | at most 500 | satisfied |
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 177 | **221** | (no gate this task) | recorded |

## Output Summary

Three carrier types added in the two owned files D2 designates for them, with no new production
file and no project-file edit. Compile exit 0. `IQfcDatamodel.cs` at 118 of 500.

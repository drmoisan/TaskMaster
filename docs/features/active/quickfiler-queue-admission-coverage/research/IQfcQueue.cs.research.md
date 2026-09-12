# Research: `QuickFiler/Controllers/IQfcQueue.cs`

- Parent epic: #136 (`quickfiler-per-file-coverage`)
- Child feature: #431 F2 (`quickfiler-queue-admission-coverage`)
- File under research: `QuickFiler/Controllers/IQfcQueue.cs` (41 lines, verified by direct read)
- Evidence basis: direct read of the file; direct read of `QuickFiler/Controllers/QfcQueue.cs` (the sole
  implementer); grep confirming no dedicated test file targets this interface directly.

## Current structure

- `public interface IQfcQueue : INotifyCollectionChanged, INotifyPropertyChanged` — declaration-only,
  no default interface members, no executable logic. Members: `Count`, `JobsRunning`, `TlpStates`,
  `TlpTemplate`, `ChangeIterationSize`, `CompleteAddingAsync`, `Dequeue`, `EnqueueAsync`, `GrowEntry`,
  `JobsToFinish`, `RemoveItem`, `RenumberGroups`, `TryDequeueAsync`.
- Sole implementer in production code: `QfcQueue` (`public class QfcQueue(...) : IQfcQueue`).
- No dependencies, no COM references, no concurrency/RNG/clock surface — an interface has no
  executable statements.

## Existing test coverage

Per the General Unit Test Policy's coverage exclusion clarification and the repository's own
`.claude/rules/general-unit-test.md`: "Type-only / interface-only modules with no executable behavior
may be omitted from coverage measurement... This is a clarification only; it does not lower any coverage
threshold." No dedicated test file targets `IQfcQueue.cs` directly, and none is needed — interface
declarations contain no lines a coverage tool can mark as covered or uncovered beyond the member
signatures themselves (which most coverage tools, including the Cobertura output this repo's
`Invoke-MSTestWithCoverage.ps1` produces, do not instrument for interface declarations at all).
`IQfcQueue`'s members are exercised indirectly through every test that calls a member of `QfcQueue`
(documented in the `QfcQueue.cs.research.md` artifact for this child), which is the implementing type's
coverage, not this file's.

## Coverage gap

None applicable. There is no executable behavior in this file to gap-analyze.

## `[ExcludeFromCodeCoverage]` disposition

Not applicable — this file carries no such attribute and needs none; it is already outside the
executable-coverage denominator by the nature of being interface-only, per the repository's own
coverage-exclusion policy clarification (no `[ExcludeFromCodeCoverage]` attribute is required or
appropriate for a pure interface — the attribute is for excluding otherwise-instrumentable executable
code, and there is none here to exclude).

## Seam requirements

None. This file already **is** the interface seam that lets `QfcQueue`'s consumers depend on an
abstraction rather than the concrete type (e.g., `IQfcCollectionController`/`QfcCollectionController`
tests that construct or mock queue collaborators).

## Candidate test cases

None. No test-case authoring is warranted against this file; F1's per-file ledger/harness should record
it as an interface-only module exempt from line-coverage measurement per the repository's own coverage
exclusion clarification, not as a file requiring `>= 80%` coverage evidence.

## Determinism constraints

Not applicable.

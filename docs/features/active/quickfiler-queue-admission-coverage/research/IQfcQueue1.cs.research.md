# Research: `QuickFiler/Controllers/IQfcQueue1.cs`

- Parent epic: #136 (`quickfiler-per-file-coverage`)
- Child feature: #431 F2 (`quickfiler-queue-admission-coverage`)
- File under research: `QuickFiler/Controllers/IQfcQueue1.cs` (44 lines, verified by direct read)
- Evidence basis: direct read of the file; direct read of `IQfcQueue.cs` for comparison; a repository-wide
  grep for `IQfcQueue1` that returned only this file itself, the `QuickFiler.csproj` compile-include
  entry, and documentation references (issue/spec/epic files created for this very child) — no
  production `.cs` file implements or references `IQfcQueue1`.

## Current structure

- `public interface IQfcQueue1` — declaration-only, no default interface members. Member set is
  **near-identical** to `IQfcQueue.cs`: same `Count`, `JobsRunning`, `TlpStates`, `TlpTemplate`,
  `ChangeIterationSize`, `CompleteAddingAsync`, `Dequeue`, `EnqueueAsync`, `GrowEntry`, `JobsToFinish`,
  `RemoveItem`, `RenumberGroups`, `TryDequeueAsync` signatures.
- The only structural difference from `IQfcQueue.cs`: `IQfcQueue1` does **not** inherit
  `INotifyCollectionChanged`/`INotifyPropertyChanged`; instead it declares the two events
  (`CollectionChanged`, `PropertyChanged`) explicitly inline (lines 19-20).
- **No class in the production codebase implements `IQfcQueue1`.** `QfcQueue` implements `IQfcQueue`
  (not `IQfcQueue1`). A repository-wide grep confirms `IQfcQueue1` has exactly one production reference
  besides its own declaration: the `<Compile Include=...>` entry in `QuickFiler.csproj` that causes it
  to build at all. This interface is dead/orphaned code — most plausibly an abandoned earlier draft of
  `IQfcQueue` (perhaps predating the switch to inheriting the two `System.ComponentModel`/
  `System.Collections.Specialized` notify interfaces) that was never deleted after `IQfcQueue.cs`
  superseded it.
- No dependencies, no COM references, no concurrency/RNG/clock surface — an interface has no executable
  statements.

## Existing test coverage

None, and none is needed for the same reason as `IQfcQueue.cs`: interface-only declarations have no
executable behavior for a coverage tool to instrument. Additionally, because no production type
implements `IQfcQueue1`, there is no implementing type whose tests could even indirectly exercise this
interface's members the way `QfcQueue`'s tests indirectly exercise `IQfcQueue`'s.

## Coverage gap

None applicable — no executable behavior exists in this file, and (unlike `IQfcQueue.cs`) there is not
even an implementing type through which indirect exercise could occur.

## `[ExcludeFromCodeCoverage]` disposition

Not applicable — this file carries no such attribute and needs none, for the same reason as
`IQfcQueue.cs`.

## Seam requirements

None. This file is not a seam for anything currently — it is unreferenced by any consumer.

## Recommendation for the atomic-planner (dead-code disposition, not a coverage task)

Because `IQfcQueue1` is a near-duplicate of `IQfcQueue` with zero production implementers or consumers,
it is a candidate for **removal** rather than for coverage work — removing it would not be a "behavior
change to observable QuickFiler flows" (the acceptance criterion this child must satisfy), since nothing
depends on it. However, deletion is a repo-wide file-removal action, not itself a coverage task, and
this research does not have authority to decide it; it is out of this research artifact's scope to
recommend a specific action beyond flagging the finding. The atomic-planner should treat this as an
explicit decision point: either (a) delete `IQfcQueue1.cs` and remove its `QuickFiler.csproj` compile
entry as a zero-risk cleanup task within this child (justified because it eliminates a genuinely
unreachable file from the coverage denominator entirely, which trivially satisfies this file's portion
of the epic's acceptance criteria), or (b) leave it in place and record it on F1's exemption ledger as
an interface-only module requiring no coverage evidence (the same treatment as `IQfcQueue.cs`). Option
(a) is recommended as the lower-maintenance outcome, but the decision belongs to the plan, not this
research artifact.

## Candidate test cases

None, regardless of which disposition above is chosen — an interface has no behavior to test, and if
deleted, there is nothing left to test.

## Determinism constraints

Not applicable.

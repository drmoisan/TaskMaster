# QfcItemController.TestSupport.cs Migrated (P2-T1)

Timestamp: 2026-08-27T10-48
Task: [P2-T1]
Command: `Select-String -SimpleMatch -Pattern 'typeof(UiThread)' -Path 'QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs'` and `Select-String -SimpleMatch -Pattern 'GetDedicatedDispatcher'` against the same path
EXIT_CODE: 0
Output Summary: Both required searches return zero matches. `typeof(UiThread)` count is 0 and
`GetDedicatedDispatcher` count is 0, so the reflection swap and the parked-dispatcher factory are
gone from this file. `StartRunningDispatcher` and `ShutdownDispatcher` remain (2 matches each,
declaration plus `<see cref>` reference), as required by their three unowned callers. The file went
from 489 to 440 lines; `git diff --stat` reports 10 insertions and 59 deletions.

## Acceptance verification

| Search (`Select-String -SimpleMatch`) | Required | Observed |
| --- | --- | --- |
| `typeof(UiThread)` | 0 matches | 0 |
| `GetDedicatedDispatcher` | 0 matches | 0 |

Supplementary counts recorded for completeness:

| Search | Observed | Meaning |
| --- | --- | --- |
| `_dedicatedDispatcher` | 0 | both field declarations removed |
| `StartRunningDispatcher` | 2 | retained, as required |
| `ShutdownDispatcher` | 2 | retained, as required |

A repo-wide search for `GetDedicatedDispatcher` and `_dedicatedDispatcher` across all `*.cs` files
outside `.dotnet-sdk` returns zero matches, confirming no caller anywhere was orphaned by the
deletion.

## Edits made

1. **`EnsureUiThreadDispatcher` collapsed to a delegating expression member.** Its declaration is now
   `internal static IDisposable EnsureUiThreadDispatcher() => UiThreadDispatcherFixture.EnsureDispatcher();`,
   wrapped across two lines by the repository's formatter width. The return type changed from `void`
   to `IDisposable`.
2. **XML doc comment retained and extended.** The existing four-sentence rationale is preserved
   verbatim and a second `<para>` block added, stating that the return value is a scope whose
   `Dispose` conditionally reverts the seeding and that discarding it is permitted.
3. **`_dedicatedDispatcher` and `_dedicatedDispatcherLock` deleted.** Their renamed replacements
   `_parkedDispatcher` and `ParkedDispatcherLock` were created by `P1-T1` in the new fixture file.
4. **`GetDedicatedDispatcher` deleted.** Its renamed replacement `GetParkedDispatcher` was created by
   `P1-T1`.
5. **The orphaned XML doc block deleted.** The block described a dispatcher-pumping helper and sat
   immediately above the two field declarations without documenting either, immediately followed by a
   second doc block. Deleting only the fields would have left it attached to nothing. This has no
   build effect, because `QuickFiler.Test.csproj` sets no `DocumentationFile` and `CS1587` therefore
   cannot fire, but it would have left the file incoherent to a reader.

Every other member of the file, including `StartRunningDispatcher`, `ShutdownDispatcher`,
`HarnessController`, and the `Issue #480` / `Issue #483` / `Issue #485` shared arrange helpers, is
unchanged.

## Line citations actually used

The plan cites lines `238-249` for the helper, `221-222` for the field pair, and `213-220` for the
orphaned doc block. At `BASE_SHA` the actual spans are `241-252`, `224-225`, and `216-223`
respectively — the uniform `+3` shift recorded in
`<FEATURE>/evidence/baseline/file-inventory-baseline.2026-08-27T10-18.md`. The edit was applied by
matching the members' exact source text, not by line offset, so the shift affected nothing. The
contiguous region actually replaced was lines `216-288`, which covers all four deletions and the
insertion in one edit.

## Diff stat

```
.../Controllers/QfcItemController.TestSupport.cs   | 69 ++++------------------
 1 file changed, 10 insertions(+), 59 deletions(-)
```

Line count: 489 before, **440** after. The 500-line ceiling is measured formally by `P4-T3` after the
final formatter pass.

## Using directives

No using directive was removed from this file. The plan directs using-directive deletions only for
`QfcItemController.InitializationTests.Part2.cs` (`P2-T2`). `System.Reflection` remains live here
because `BindingFlags` is still used by the `typeof(Theme).GetField(...)` reflection in
`BuildTheme`, and `System` is now additionally required by the `IDisposable` return type.

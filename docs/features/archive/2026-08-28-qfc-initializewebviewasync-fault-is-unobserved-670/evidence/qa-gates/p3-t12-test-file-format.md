# P3-T12 — Formatting of the two touched test files

Timestamp: 2026-09-01T20-09
Command: `dotnet tool run csharpier format <file>` for each of the two files, then the read-only `dotnet tool run csharpier check <file>` for each
EXIT_CODE: 0 (both `check` invocations)

## The four SHA-256 values

| File | SHA-256 before `format` | SHA-256 after `format` | Rewritten |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | `6671B12D462C1FC9670C589E3D408C6B555DA83C6D5F8CB411AF012F093CCBC7` | `6671B12D462C1FC9670C589E3D408C6B555DA83C6D5F8CB411AF012F093CCBC7` | no |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | `D027DA9C25052C149F4DBD604C833EBC1326D4BCAD94A251E6A4447151387A10` | `D027DA9C25052C149F4DBD604C833EBC1326D4BCAD94A251E6A4447151387A10` | no |

Neither hash changed, so neither `format` invocation rewrote its file. Recording the hashes rather than the exit code is required because `csharpier format` is write-mode and exits 0 whether or not it rewrote anything: its exit code is identical on a clean run and on a repairing one, so it cannot distinguish the two.

## Read-only verification

    dotnet tool run csharpier check QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs   →  EXIT 0
    dotnet tool run csharpier check QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs         →  EXIT 0

Both exit 0. This gate is demonstrably capable of failing: the same command exited **1** against the newly authored production file in P1-T5 before that file was formatted, printing the specific expected-versus-actual hunk it objected to.

## Why `Part3.cs` was already stable

`Part3.cs` had already been formatted once earlier in Phase 3, as a micro-action while resolving the file-size budget. After the third test was added the file measured 510 lines, over the 500-line ceiling and over the plan's 100-added-line budget, and the formatter was run at that point to establish the formatter-stable line count before deciding how much documentation to compact. Measuring the ceiling against an unformatted draft would have been meaningless, since the ceiling applies to the formatted file. The file was compacted to 498 lines after that run, and this task confirms the compacted text is still formatter-stable.

`QfcItemController.InitializationTests.cs` was never formatted before this task and was nevertheless already stable, because the added test was authored to the surrounding file's existing style and no added line approaches the formatter's 100-column print width.

## Post-format line counts

    QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs  =  498 lines
    QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs        =  261 lines

Both are below the 500-line ceiling. These are the counts P3-T13 records against the P0-T8 baseline.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.

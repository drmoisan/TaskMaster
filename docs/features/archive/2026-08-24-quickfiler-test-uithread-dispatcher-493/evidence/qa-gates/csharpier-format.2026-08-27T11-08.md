# CSharpier Format — Owned Paths Only (P3-T1)

Timestamp: 2026-08-27T11-08
Task: [P3-T1]
Command: `dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`
EXIT_CODE: 0
Output Summary: `Formatted 4 files in 1893ms.` Two of the four paths were rewritten — the two new
files created in Phase 1 — and two were already formatter-clean. A second, confirming invocation of
the identical command left all four SHA-256 values unchanged, so the formatting is idempotent.

The command names four explicit **file paths**, never a directory, so it cannot rewrite any file
outside this plan's owned set. In particular
`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` and
`UtilitiesCS/Threading/UiThread.cs` are outside the argument list and were not passed to the
formatter.

## SHA-256 before and after

| Path | SHA-256 before | SHA-256 after | Rewritten |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | `0c7ba2696018a2bd6dccca747170f3c1968ccd17387fd6b8e4a7c0440eacc0a9` | `b90ccbfcc51840bcb93616fb834551664f6b4f84ce51dae9200c6f8c41b36784` | yes |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | `32dfb2e02f0c77f5afe9e579b37aa25883f06b26a09fcb004964321fd88f78a3` | `6ec0af64110a2f26757b743ae66d13df8f0f31977bc77de51183bf6fdcbe27ee` | yes |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | `6ddacd2ec8ded8c83320f3f65e0a61c0be16283a7bb2d277963c46bda9b13779` | `6ddacd2ec8ded8c83320f3f65e0a61c0be16283a7bb2d277963c46bda9b13779` | no |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | `013fb0ea81664643a5bda8c5bb75aac71fc458e5c82a3751377b3245085c9707` | `013fb0ea81664643a5bda8c5bb75aac71fc458e5c82a3751377b3245085c9707` | no |

A rewrite is visible above as a changed pair, which is why the acceptance condition asks for hashes
rather than for the tool's processed-file count. That count is `4` for both invocations regardless of
how many files actually changed, so it gates nothing on its own.

## Idempotence confirmation

The identical command was run a second time immediately afterwards. It again reported
`Formatted 4 files in 1657ms.` and exit code 0, and the SHA-256 of both previously rewritten files
was byte-identical to the "after" column above
(`b90ccbfc…` and `6ec0af64…`). No file changed on the second pass, so no further Phase 3 restart is
required on formatting grounds.

## Post-format line counts

| Path | Lines after formatting |
| --- | --- |
| `QfcItemController.UiThreadDispatcherFixture.cs` | 278 |
| `QfcItemController.UiThreadDispatcherFixtureTests.cs` | 346 |
| `QfcItemController.TestSupport.cs` | 440 |
| `QfcItemController.InitializationTests.Part2.cs` | 393 |

The regression-test file grew from 337 to 346 lines as CSharpier rewrapped three
`.Should().NotThrow(...)` and assignment expressions. `P4-T3` performs the formal 500-line audit.

## Line-oriented gate tokens re-verified after formatting

Formatting can rewrap a line and silently break a line-oriented search, so the three counts the
earlier tasks asserted were re-measured against the formatted files:

| Search | Path | Count |
| --- | --- | --- |
| `[TestMethod]` | `QfcItemController.UiThreadDispatcherFixtureTests.cs` | 6 |
| `[Timeout(GateTimeoutMs)]` | `QfcItemController.UiThreadDispatcherFixtureTests.cs` | 6 |
| `typeof(UiThread)` | `QfcItemController.UiThreadDispatcherFixture.cs` | 1 |

All three are unchanged from their pre-format values, so `P1-T1`'s and `P1-T3`'s acceptance
conditions still hold against the formatted tree.

# File Sizes After Issue #485

Timestamp: 2026-08-26T09-15
Task: [P2-T7]

Command: `wc -l QuickFiler/Controllers/QfcItemController.ViewerSetup.cs <the five owned test files>`
EXIT_CODE: 0

| File | Baseline | Current | At most 500 |
|---|---|---|---|
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 430 | **481** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | **497** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 374 | **374** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 474 | **474** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 184 | **320** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 365 | **410** | yes |

Every recorded value is at most 500.

## Capacity note (risk R1)

`QfcItemController.ViewerSetup.cs` reached **505 lines** after the `[P2-T5]` guards were first written
with full explanatory prose, breaching the 500-line ceiling that risk R1 predicted for this file. The
General Code Change Policy limit is binding, so the additions were compacted in place rather than
relocated:

- the two-line `string.IsNullOrEmpty(requestedId)` and `contentIdMap is null` guards were merged into one
  `||` condition (both return false with null `out` values, and each case retains its own regression
  test);
- the `TryResolveCidResource` XML doc was reduced from a summary plus four `param` tags and a `returns`
  tag to a five-line summary that states the same contract;
- the three multi-line "why" comments were each shortened by one line;
- the lambda adapter's `TryResolveCidResource` call was brought onto a single line by shortening the local
  variable name, and its inline comment was hoisted above the lambda.

The file is now **481 lines**, leaving **19 lines** of headroom for the remaining production additions to
this file: the `[P4-T5]` timer disposal (1 line), the `[P4-T6]` mail-actions nulling (1 line), the
`[P5-T4]` `UnwireEvents()` call site (2 lines), and the `[P5-T10]`
`DetachWebResourceRequestedHandler` method. That method must therefore be written tightly; capacity is
re-checked at `[P4-T9]`, `[P5-T13]`, and `[P7-T8]`.

No `.csproj` was edited, no file was created, and no forbidden file was written.

Output Summary: All six recorded files are at most 500 lines after the #485 change.
`QfcItemController.ViewerSetup.cs` is at 481 with 19 lines of headroom remaining.

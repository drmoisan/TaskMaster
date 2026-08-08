# P8-T2 — Post-Format File-Size Audit

Issue: #230
Task: [P8-T2]
Phase 8 loop iteration: 1

- Timestamp: 2026-08-07T23-46
- Command: `wc -l` over every non-markdown file added or modified by this feature,
  measured **after** the P8-T1 `dotnet tool run csharpier format .` pass (D8:
  post-format counts are the authoritative measurement)
- EXIT_CODE: 0
- Output Summary: **All 10 files are at or below the 500-line repository limit.**
  Largest is `QuickFiler/Controllers/QfcItemController.Initialization.cs` at 489
  (11 lines of headroom); the seam itself is 482 (18 lines of headroom).

| Lines | File | Limit | Status |
|---:|---|---:|---|
| 482 | `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` | 500 | PASS |
| 443 | `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` | 500 | PASS |
| 209 | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | 500 | PASS |
| 364 | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 500 | PASS |
| 290 | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 500 | PASS |
| 467 | `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 500 | PASS |
| 436 | `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | 500 | PASS |
| 489 | `QuickFiler/Controllers/QfcItemController.Initialization.cs` | 500 | PASS |
| 430 | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 500 | PASS |
| 476 | `QuickFiler.Test/QuickFiler.Test.csproj` | 500 | PASS |

## D8 overflow files

`QfcItemController.InitializationTests.Part2.cs` and
`QfcItemController.InitializationTests.Part3.cs` are the D8 overflow files created
during Phase 3, when the combined `QfcItemController.InitializationTests.cs`
reached 529 lines. Both are `partial` continuations of the same
`QfcItemController_InitializationTests` class (no second `[TestClass]` attribute)
and both carry a `<Compile Include>` entry in
`QuickFiler.Test/QuickFiler.Test.csproj`, verified independently in P7-T8.

## Result

No violation; the D8 overflow refactor is not required again and Phase 8 does not
restart from P8-T1 on account of file size.

---

## Phase 8 loop iteration 2 (after the P8-T5 isolation fix)

Re-measured after the iteration-2 `csharpier format .` pass. Only
`QfcItemController.InitializationTests.Part2.cs` changed (364 -> 409 lines) from
the `SemaphoreSlim` gate and its documentation.

- Timestamp: 2026-08-08T00-01
- Command: `wc -l` over the same ten files
- EXIT_CODE: 0
- Output Summary: **All 10 files remain at or below 500 lines.**

| Lines | File | Limit | Status |
|---:|---|---:|---|
| 482 | `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` | 500 | PASS |
| 443 | `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` | 500 | PASS |
| 209 | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | 500 | PASS |
| 409 | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 500 | PASS |
| 290 | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 500 | PASS |
| 467 | `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 500 | PASS |
| 436 | `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | 500 | PASS |
| 489 | `QuickFiler/Controllers/QfcItemController.Initialization.cs` | 500 | PASS |
| 430 | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 500 | PASS |
| 476 | `QuickFiler.Test/QuickFiler.Test.csproj` | 500 | PASS |

This is the authoritative final-pass measurement for this task.

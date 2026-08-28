# [P2-T13] Phase 2 formatting, with both literal gates re-asserted after the mutating pass

Timestamp: 2026-08-27T09-45

## Mutating pass

Command:

```
dotnet tool run csharpier format QuickFiler\Controllers\QfcCollectionController.cs QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs
```

Output (verbatim):

```
Formatted 2 files in 1209ms.
```

EXIT_CODE: 0

## Read-only verification

Command:

```
dotnet tool run csharpier check QuickFiler\Controllers\QfcCollectionController.cs QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs
```

Output (verbatim):

```
Checked 2 files in 889ms.
```

EXIT_CODE: 0

No per-file warning line was emitted, so the unformatted-file count is **0**.

## Re-assertion after the mutating pass

Formatting can rewrap a long expression, so both literal gates are re-run against the post-format file
rather than trusting the pre-format measurement.

`[P2-T9]`'s gate, re-run:

```powershell
@(Select-String -SimpleMatch -Pattern '_registeredDigits == 2 ? "00" : ""' -Path QuickFiler\Controllers\QfcCollectionController.cs).Count
```

```
COUNT = 1        (line 1188: var format = _registeredDigits == 2 ? "00" : "";)
```

Still exactly `1`, and still a complete statement on one physical line. Hoisting the conditional into a
standalone local rather than embedding it in the `Remove(...)` argument is what keeps it inside
CSharpier's 100-column print width; embedded, CSharpier would have wrapped it and this literal would
no longer exist.

`[P2-T8]`'s gate, re-run over the post-format `UnregisterNavigation` body:

```
post-format Digits read count = 0     (slice 1184..1194, forward)
```

Still exactly `0`.

## Acceptance evaluation

- The `check` invocation reports `EXIT_CODE: 0` and zero unformatted files. PASS.
- The format-selection literal count is still exactly `1` after the mutating pass. PASS.
- The `[P2-T8]` zero-read count is still exactly `0` after the mutating pass. PASS.

Output Summary: format exit 0, check exit 0 with zero unformatted files; both post-format
re-assertions hold — literal count 1 and `Digits` read count 0.

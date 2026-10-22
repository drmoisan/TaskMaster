# [P3-T16] Phase 3 formatting

Timestamp: 2026-08-27T09-45

## Mutating pass

Command:

```
dotnet tool run csharpier format QuickFiler\Controllers\QfcItemController.Navigation.cs QuickFiler.Test\Controllers\QfcItemController.NavigationTests.cs
```

Output (verbatim):

```
Formatted 2 files in 1099ms.
```

EXIT_CODE: 0

## Read-only verification

Command:

```
dotnet tool run csharpier check QuickFiler\Controllers\QfcItemController.Navigation.cs QuickFiler.Test\Controllers\QfcItemController.NavigationTests.cs
```

Output (verbatim):

```
Checked 2 files in 817ms.
```

EXIT_CODE: 0

No per-file warning line was emitted, so the unformatted-file count is **0**.

## What the mutating pass changed

One statement in the test file's shared arrangement builder was rewrapped. The pre-format form spread
the delegate cast across three lines; CSharpier's preferred form uses two:

```csharp
            var set =
                (Action<string, object>)((n, v) => QfcItemControllerTestSupport.SetField(c, n, v));
```

That is a one-line reduction, so `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs`
went from 499 lines to 498. `QuickFiler/Controllers/QfcItemController.Navigation.cs` was unchanged in
length at 252 lines.

## Acceptance evaluation

- The `check` invocation reports `EXIT_CODE: 0` and zero unformatted files. PASS.

Output Summary: format exit 0, check exit 0 with zero unformatted files; one statement rewrapped in the
test file, reducing it by one line.

# P1-T2 — Split the Helper Methods region into QfcQueue.UiIdle.cs

Timestamp: 2026-09-13T15-15
Command: (Get-Content -LiteralPath <P>).Count over the paths below, plus a literal search for `#region Helper Methods` and for the three member names over QuickFiler/Controllers
EXIT_CODE: 0
Output Summary: The whole Helper Methods region moved verbatim out of the base part into a new
partial part. The region literal now occurs zero times in the base part and exactly once in the new
file. All 34 relocated lines compare identical to the recorded anchor.

## Recorded anchor

BASE_SHA: 8213826f695439e86e3ed34faa575de493a11ec7

## Region literal occurrences

- `QuickFiler/Controllers/QfcQueue.cs` occurrences of `#region Helper Methods`: 0
- `QuickFiler/Controllers/QfcQueue.UiIdle.cs` occurrences of `#region Helper Methods`: 1 (line 29)

## The three relocated members in the new file

- `UiIdleCallAsync` taking a System.Action, at line 31
- `UiIdleCallAsync` generic overload taking Func of T, at line 39
- `UiIdleAsyncCallAsync` generic, taking Func of Task of T, at line 47

## Verbatim-move verification

Lines 472 through 505 inclusive of QuickFiler/Controllers/QfcQueue.cs at the recorded anchor were
compared line by line, in order, against lines 29 through 62 inclusive of the new file, using
Compare-Object with a sync window of zero over the anchor content read with `git show`. The result
was no differences across all 34 lines, so the three member bodies are byte-identical to the anchor
including their indentation, which is unchanged because the nesting depth of the new file matches
the base part.

Verification output: `VERBATIM-MOVE: IDENTICAL (34 lines)`

## Measured line counts (CMD-LINECOUNT)

- QuickFiler/Controllers/QfcQueue.cs = 251
- QuickFiler/Controllers/QfcQueue.UiIdle.cs = 64

## Breadcrumb left in the base part

```
        // The Helper Methods region lives in the partial part QfcQueue.UiIdle.cs; see that file.
```

## Using directives

The new file carries fifteen of the sixteen directives of QuickFiler/Controllers/QfcQueue.cs at the
recorded anchor. The QuickFiler interfaces directive is deliberately omitted, as this task's text
requires: nothing in this file references that namespace until P2-T5 adds the adapter, and adding it
here would produce an unnecessary-using diagnostic against a file this task just created. No nullable
pragma was added.

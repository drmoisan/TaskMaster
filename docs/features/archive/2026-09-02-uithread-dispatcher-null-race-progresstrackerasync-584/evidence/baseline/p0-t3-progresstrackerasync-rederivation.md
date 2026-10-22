# P0-T3 — Re-derivation of the AC3 hypothesis in UtilitiesCS/Threading/ProgressTrackerAsync.cs

Timestamp: 2026-09-03T08-20

Command:
```text
sed -n '1,60p' UtilitiesCS/Threading/ProgressTrackerAsync.cs
sed -n '59,109p' UtilitiesCS/Threading/ProgressTrackerAsync.cs
wc -l UtilitiesCS/Threading/ProgressTrackerAsync.cs
```

EXIT_CODE: 0

The field carries a single integer. All three commands this task ran exited 0.

## Output Summary

The file is 109 lines and was read in full.

- **Assignment of `UiThread.Dispatcher` to the instance field — line 33**, verbatim:

  ```csharp
              UiDispatcher = UiThread.Dispatcher;
  ```

  It is the first statement of `public async Task<ProgressTrackerAsync> InitializeAsync()`, which
  opens at line 31. `UiDispatcher` is the internal property declared at lines 86-90 over the
  protected backing field `_uiDispatcher` at line 85.

- **First statement that dereferences that instance field — line 35**, verbatim:

  ```csharp
              await UiDispatcher.InvokeAsync(() =>
  ```

  No statement between line 33 and line 35 reads `UiDispatcher`; line 34 is blank.

## Conclusion

The property read `UiThread.Dispatcher` on line 33 is evaluated strictly before the dereference on
line 35. Once `UtilitiesCS/Threading/UiThread.cs` is fixed so that the `Dispatcher` getter throws
`InvalidOperationException` when its backing field is null, that exception is raised at line 33 — at
the property access, with a message naming `UiThread.Initialize()` — and control never reaches the
`InvokeAsync` dereference on line 35, which is where the current unhelpful
`NullReferenceException` originates. The consumer therefore receives a self-diagnosing exception at
the correct site without any edit to this file.

`UtilitiesCS/Threading/ProgressTrackerAsync.cs` is consequently NOT added to this plan's write-target
list. AC3's "left unmodified" wording holds. No overturned conclusion to report.

Incidental observation (recorded, not acted on): this file carries three further `null!`
suppressions of the same shape, at lines 69 (`_progressViewer`), 76 (`_jobName`), and 85
(`_uiDispatcher`). They are outside this plan's scope and are not edited.

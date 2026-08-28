# qfc-metrics-flush-writes-empty-session-file (Issue #646)

- Date captured: 2026-08-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-metrics-flush-writes-empty-session-file/ (Issue #646)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #646
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/646
- Last Updated: 2026-08-27
## Summary

`QfcHomeController.WriteMetricsAsync` invokes the injected `MetricsFileWriter` unconditionally, even
when the null-and-whitespace filter leaves the diagnostic-line array empty. The default writer
`FileIO2.WriteTextFileAsync` opens an append `StreamWriter`, so a QuickFiler session that produces no
diagnostic lines now creates, or touches, a zero-content session-metrics file.

The EFC sibling path already guards against this. `EfcHomeController.Metrics.cs` returns early:

```csharp
if (dataLines.Length == 0)
{
    return;
}
```

`QfcHomeController.Metrics.cs` has no matching guard at the point where it awaits the writer.

**This is a narrow regression introduced by the #442 flush fix, not a pre-existing defect.** Before
that fix the QFC metrics queue was never drained, so nothing was ever written and the empty-array
case could not manifest. Making the flush work made it reachable.

The remedy is one guard in an owned file, mirroring the EFC form:

```csharp
if (lines.Length == 0)
{
    return;
}
```

It was deliberately not applied inside feature `quickfiler-home-controller-metrics-442`. That
feature's plan was complete and its toolchain green at the time the finding was raised by
feature-review (finding CR-1, Minor, non-blocking), and the repository's General Code Change Policy
directs opening a new issue rather than widening the scope of work in flight.

## Environment

- OS/version: Windows 11, Outlook VSTO add-in host
- Python version: not applicable (C# / .NET Framework 4.8)
- Command/flags used: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
- Data source or fixture: the session-metrics CSV in the user's `MyDocuments` folder

## Steps to Reproduce

1. Run a QuickFiler filing session in which `GetMoveDiagnostics` returns an array whose every
   element is `null` or whitespace — for example a session in which no item is actually moved.
2. Let the session reach its metrics write.
3. Inspect the session-metrics file in `MyDocuments`.

## Expected Behavior

No write occurs and no file is created or touched, because there is no diagnostic content to record.
This matches the EFC path's behavior for the same input.

## Actual Behavior

`MetricsFileWriter` is invoked with an empty array. The default writer opens the target for append,
so the file is created if absent and its last-write timestamp is updated if present, in both cases
recording nothing.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: the unconditional call is the closing statement of `WriteMetricsAsync`:
  `await MetricsFileWriter(filename, lines, myDocuments, CancellationToken.None);`
  reached whatever the length of `lines`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low. The worst outcome is a spurious empty or unchanged-content file in the user's `MyDocuments`
folder. No data is lost, no exception is raised, and no downstream consumer exists — the
session-metrics CSV has no in-repo reader.

## Suspected Cause / Notes

Asymmetry between the two controllers' metrics writers. The EFC path acquired its empty-array guard
when its writer was extracted behind `_dependencies.MetricsLineWriter`; the QFC path acquired its
writer seam later, during #442, and the guard was not carried across.

Raised as finding CR-1 in
`docs/features/active/quickfiler-home-controller-metrics-442/code-review.2026-08-27T14-35.md`.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: add a test in
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` that mocks `GetMoveDiagnostics` to
  return an all-null array and asserts the injected `MetricsFileWriter` delegate captures **zero**
  invocations. The existing `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` test already
  establishes the capture harness; this is the zero-line boundary case it does not cover.
- [x] Integration scenario to retest: the full QuickFiler suite must stay green; the new guard must
  not alter `WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce`, which supplies non-empty lines.
- [x] Manual verification notes: confirm the two controllers' guards are textually equivalent after
  the change, so the asymmetry does not reappear.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

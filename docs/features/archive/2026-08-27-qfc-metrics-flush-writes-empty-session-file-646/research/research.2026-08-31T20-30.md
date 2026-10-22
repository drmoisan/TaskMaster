# Research — Issue #646: QFC metrics flush writes empty session file

- Issue: #646
- Feature folder: `docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/`
- Researcher: task-researcher agent
- Date: 2026-08-31

## 0. Critical constraint — concurrently in-flight sibling item #647

Issue #647 (branch `bug/fileio2-write-retry-reports-success-on-final-failure-647`) rewrites the
same method, `WriteMetricsAsync` in `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, and per
the task brief merges into `main` **before** this item (#646) executes. The #647 rewrite:

- Changes the `MetricsFileWriter` delegate return type from `Task` to `Task<bool>`.
- Replaces the single-line `await MetricsFileWriter(...)` statement with a multi-line assignment
  (`bool metricsWritten = await MetricsFileWriter(...)`) followed by an `if (!metricsWritten) {
  logger.Error(...); }` failure-logging branch.

**This item (#646) must not touch the delegate signature or add/modify any failure-logging
branch.** Those are owned exclusively by #647. The #646 fix is limited to inserting one early-return
guard between the point where the filtered `lines` array is computed and the point where the
writer is invoked, regardless of how many statements that invocation later expands into.

**Anchoring rule for the atomic plan:** anchor the guard's insertion point on (a) the statement
that assigns the filtered array (currently `var lines = strOutput.Where(...).ToArray();`, line 174)
and (b) the start of the writer-invocation statement (currently `await MetricsFileWriter(filename,
lines, myDocuments, CancellationToken.None);`, line 179) — **not** on the literal single-line
await text and **not** on the absolute line number 179, because #647 will change both the literal
text (into a multi-statement assignment + branch) and the downstream line numbers.

**Citations invalidated by the #647 merge** (must be re-derived at execution time, not trusted from
this document):
- §2 "exact statement text of the writer-invocation line" (line 179) — text and line number both
  change.
- §2 "line count of the method body" and all line numbers below 179 that are relative to a
  postulated-unchanged file (line numbers 180-216 in this document's citations shift downward by
  however many net lines #647 adds).
- §1 "`MetricsFileWriter` delegate signature: `Func<string, string[], string, CancellationToken,
  Task>`" (lines 28-34) — return type changes to `Task<bool>`.
- Any statement in this document asserting the writer call is currently a single `await` statement
  with no following branch — after #647 it is not.

**Citations that remain valid regardless of #647:**
- The filtered-array computation (`strOutput.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray()`)
  is untouched by #647's scope (delegate signature + failure branch only); its line number may
  shift but its position immediately before the writer call does not change.
- The EFC sibling guard at `QuickFiler/Controllers/EfcHomeController.Metrics.cs:72-75` — that file
  is not touched by #647.
- The test-harness patterns in `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
  (`BuildLooseMetricsController`, the `MetricsWrite` capture record, the delegate-assignment
  pattern) — these will need updating for #647's `Task<bool>` return type as part of #647's own
  work, but the *pattern* documented here (capture list + assert-on-count) survives.
- `FileIO2.WriteTextFileAsync`'s append-mode `StreamWriter` behavior (§1) — #647 changes the
  *caller's* handling of this writer's outcome, not the writer itself in `FileIO2.cs`.

## 1. The `MetricsFileWriter` seam

Declared in `QuickFiler/Controllers/QfcHomeController.Metrics.cs:28-34` (current state, pre-#647):

```csharp
internal Func<
    string,
    string[],
    string,
    CancellationToken,
    Task
> MetricsFileWriter { get; set; } = FileIO2.WriteTextFileAsync;
```

Parameter order: `filename, lines, folderRoot, cancellationToken`. Default-bound to
`FileIO2.WriteTextFileAsync`, a static method in `UtilitiesCS/To Depricate/FileIO2.cs:50-89`.

`WriteTextFileAsync` behavior with an empty `strOutput` array (`UtilitiesCS/To Depricate/FileIO2.cs:59-73`):

```csharp
string filepath = Path.Combine(folderpath, filename);
...
while (!success)
{
    try
    {
        token.ThrowIfCancellationRequested();
        using (var sw = new StreamWriter(filepath, true, System.Text.Encoding.UTF8))
        {
            success = true;
            foreach (var output in strOutput)
                await sw.WriteLineAsync(output);
        }
    }
    ...
}
```

The `StreamWriter(filepath, append: true, Encoding.UTF8)` constructor opens the target file — and
creates it if it does not exist — **before** the `foreach` loop runs. With an empty `strOutput`
array the loop body never executes and no line is written, but the `using` block still opens and
closes the file, which is sufficient to create a zero-byte file (if absent) or update the
last-write timestamp of an existing zero-content file. This confirms the defect mechanism stated
in the issue: the guard must exist upstream of the delegate call, because the default writer
implementation has no length check of its own and none should be added to it (that would touch a
file outside this item's scope and duplicate a decision EFC already made at the call site).

## 2. Current body of `WriteMetricsAsync` and structural anchors

Full current method, `QuickFiler/Controllers/QfcHomeController.Metrics.cs:107-180` (line numbers
are pre-#647 and will shift after that merge; see §0):

- Line 107: `public async Task WriteMetricsAsync(string filename)` — method signature, unchanged
  by both #646 and #647.
- **Early return #1** (line 131-134): `if (!Globals.FS.SpecialFolders.TryGetValue("MyDocuments",
  out var myDocuments)) { return; }` — this is the only `return` statement already present in the
  method. It aborts before any diagnostics are computed or the writer is touched.
- Lines 137-160: duration computation, `WriteMoveToCalendar(...)` call (COM calendar write, not
  gated by the lines-empty condition and not in scope for #646).
- Lines 162-169: `string[] strOutput = _formController.Groups.GetMoveDiagnostics(...)`.
- **Anchor A — filtered-array computation** (line 174): `var lines = strOutput.Where(line =>
  !string.IsNullOrWhiteSpace(line)).ToArray();` preceded by a comment (lines 171-173) explaining
  the filter defends against a non-null-guaranteed interface contract.
- **Anchor B — writer invocation start** (line 179, preceded by a comment at lines 176-178
  explaining the deliberate `CancellationToken.None` choice): `await MetricsFileWriter(filename,
  lines, myDocuments, CancellationToken.None);` — the closing statement of the method body.

The new guard belongs between Anchor A and Anchor B, textually equivalent to the EFC form:

```csharp
if (lines.Length == 0)
{
    return;
}
```

This becomes early return #2 in the method. It is structurally consistent with the existing early
return #1: both are unconditional `return;` (no value, matching the `Task`-returning
`async Task` signature — no `return null;`/`return default;` needed), both abort before any I/O
that the guarded condition makes meaningless, and neither interacts with `WriteMoveToCalendar`'s
already-completed COM calendar write (the calendar item is written before the diagnostics array is
computed, so guard #2 cannot suppress or duplicate the calendar entry — only the metrics-file
write is skipped, matching the issue's Expected Behavior).

## 3. Existing test harness — `QfcHomeControllerMetricsTests.cs`

File: `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` (453 lines).

### Controller construction and writer capture

`BuildLooseMetricsController` (lines 72-135) is the single fixture-builder used by every test in
this class. Verbatim load-bearing excerpt:

```csharp
private static (
    QfcHomeController controller,
    Mock<IQfcCollectionController> groups
) BuildLooseMetricsController(string[] diagnostics = null, bool withMyDocuments = true)
{
    ...
    var mockGroups = new Mock<IQfcCollectionController>(MockBehavior.Loose);
    mockGroups.SetupGet(x => x.EmailsToMove).Returns(1);
    mockGroups
        .Setup(x =>
            x.GetMoveDiagnostics(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                ref It.Ref<AppointmentItem>.IsAny
            )
        )
        .Returns(diagnostics ?? Array.Empty<string>());
    var mockFormController = new Mock<IQfcFormController>(MockBehavior.Loose);
    mockFormController.SetupGet(x => x.Groups).Returns(mockGroups.Object);

    mockGlobals.SetupGet(x => x.AF.CancelToken).Returns(CancellationToken.None);

    var controller = new QfcHomeController(mockGlobals.Object, () => { });
    controller.CreateCancellationToken();
    // Replace the production file writer with a no-op. The default seam value is
    // FileIO2.WriteTextFileAsync, which probes a real path and retries 100 times over ten
    // seconds when the folder is absent; a unit test must not touch the filesystem or wait
    // on wall-clock time. Tests that assert on the flush override this with a capturing
    // delegate of their own.
    controller.MetricsFileWriter = (filename, lines, folderRoot, token) =>
        Task.CompletedTask;
    SetPrivateField(controller, "_formController", mockFormController.Object);
    SetPrivateField(controller, "_stopWatchMoved", new Stopwatch());
    return (controller, mockGroups);
}
```

`GetMoveDiagnostics` is stubbed via the optional `diagnostics` parameter — pass an array (e.g. all
`null`/whitespace entries) to control what `WriteMetricsAsync` filters. The default fixture
assigns a no-op `MetricsFileWriter`; individual tests overwrite `controller.MetricsFileWriter`
directly (a public/internal settable property, not constructor-injected) with a capturing lambda.

`SetPrivateField` (lines 58-64) uses reflection
(`GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(...)`) to set
`_formController` and `_stopWatchMoved`, both otherwise-private fields.

### Capture record

```csharp
private sealed class MetricsWrite
{
    internal MetricsWrite(
        string filename,
        string[] lines,
        string folderRoot,
        CancellationToken token
    )
    {
        Filename = filename;
        Lines = lines;
        FolderRoot = folderRoot;
        Token = token;
    }

    internal string Filename { get; }
    internal string[] Lines { get; }
    internal string FolderRoot { get; }
    internal CancellationToken Token { get; }
}
```

Used at lines 304-323, populated by capturing-lambda assignments to `controller.MetricsFileWriter`
at lines 335 (`WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce`), 359
(`WriteMetricsAsync_CompletesWriterTaskBeforeReturning`, an async lambda using `await Task.Yield()`
to prove ordering without a wall-clock wait), 382 (`WriteMetricsAsync_PassesUncancelledTokenToWriter`),
and 409 (`WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`).

### Closest existing template for the new zero-line boundary test

Two candidates were evaluated per the task brief:

- **`WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`** (lines 402-424): stubs
  `GetMoveDiagnostics` to return `new[] { "line-one", "   ", null, "line-two" }`, captures the
  invocation, and asserts `captures[0].Lines` equals the filtered non-empty set. This test
  demonstrates the filtering mechanics and the capture pattern but asserts on invocation
  *content*, not *absence of invocation* — it is not directly reusable as a template because its
  assertion shape (`captures.Should().ContainSingle()`) is the opposite of what AC3 requires.

- **`WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter`** (lines 430-449): stubs
  `GetMoveDiagnostics` to return `new[] { "line-one" }`, sets `withMyDocuments: false`, uses a
  boolean `invoked` flag (not a capture list) set inside the writer lambda, and asserts
  `invoked.Should().BeFalse(...)` after the call. Verbatim:

  ```csharp
  [TestMethod]
  public async Task WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter()
  {
      var (controller, _) = BuildLooseMetricsController(
          new[] { "line-one" },
          withMyDocuments: false
      );
      var invoked = false;
      controller.MetricsFileWriter = (filename, written, folderRoot, token) =>
      {
          invoked = true;
          return Task.CompletedTask;
      };

      await controller.WriteMetricsAsync("metrics.csv");

      invoked
          .Should()
          .BeFalse("the guard must abort before any write when MyDocuments is absent");
  }
  ```

  **This is the correct template.** Its assertion shape — a boolean `invoked` flag flipped inside
  the writer lambda, asserted `BeFalse()` after the act — is exactly the zero-invocation outcome
  AC3 requires. The only change needed for the new test is the arrange: keep `withMyDocuments`
  default (`true`, so the MyDocuments guard at line 131 does not itself cause the early return —
  the new guard being tested must be the one that fires) and pass an all-null-or-whitespace
  `diagnostics` array (e.g. `new[] { "   ", null, "\t" }` or `Array.Empty<string>()`) so that
  `GetMoveDiagnostics` returns content that the existing filter at Anchor A reduces to a
  zero-length `lines` array.

  Note: `BuildLooseMetricsController`'s `mockGroups.SetupGet(x => x.EmailsToMove).Returns(1)` is
  fixed at `1` regardless of the `diagnostics` argument, so the duration-divide-by-emailsLoaded
  branch (`WriteMetricsAsync`, lines 145-148) behaves identically to every other test in this file
  and needs no special handling for the new test.

## 4. Other callers / dependents on unconditional writer invocation

Repository-wide search for `MetricsFileWriter` across `*.cs` files found exactly two matches:

- `QuickFiler/Controllers/QfcHomeController.Metrics.cs` — the property declaration and default
  binding (production).
- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` — all fixture/test usages
  documented in §3.

A third match, `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:260-274`, contains a fully
commented-out test (`//public async Task WriteMetricsAsync_ExecutesCorrectly() ...`) that is dead
code, not a live caller.

Repository-wide search for `WriteMetricsAsync` (all extensions) found no other production callers
beyond the method's own definition and doc-file references in the two promoted-potential markdown
files already read (§ this document's context). The only live test-code callers are the six tests
in `QfcHomeControllerMetricsTests.cs` already enumerated in §3 (`WriteMetricsAsync_ReadsMovedStopwatchForDuration`,
`WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator`,
`WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`,
`WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce`,
`WriteMetricsAsync_CompletesWriterTaskBeforeReturning`,
`WriteMetricsAsync_PassesUncancelledTokenToWriter`,
`WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`,
`WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter`).

**Determination: the guard cannot break any existing test.** Every existing test that asserts a
writer invocation happened (`WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce`,
`WriteMetricsAsync_CompletesWriterTaskBeforeReturning`,
`WriteMetricsAsync_PassesUncancelledTokenToWriter`,
`WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`) supplies a `diagnostics` array
containing at least one non-null, non-whitespace element (`"line-one"`, `{"line-one","line-two"}`,
or the filtered subset `{"line-one","line-two"}` from a mixed array), so the filtered `lines`
array is never empty for any of them and the new guard never fires in those paths. The three tests
that do not assert on the writer at all (`WriteMetricsAsync_ReadsMovedStopwatchForDuration`,
`WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator`,
`WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`) supply no `diagnostics` override
(default `Array.Empty<string>()` from `BuildLooseMetricsController`'s `diagnostics ??
Array.Empty<string>()`), meaning `GetMoveDiagnostics` returns an empty array and — after the guard
is added — `WriteMetricsAsync` will return earlier than it currently does. This is safe: none of
these three tests assert anything about the writer or about method completion timing beyond
`await controller.WriteMetricsAsync(...)` returning normally; they assert only on the
`GetMoveDiagnostics` call arguments via `groups.Verify(...)`, and that call happens before Anchor A
in all cases, so the new guard (which runs strictly after `GetMoveDiagnostics` is called) has no
effect on those assertions.

`WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter` is unaffected because its early
return already fires at the pre-existing MyDocuments guard (line 131), before Anchor A is ever
reached.

## 5. Build/run facts

- Test assembly output path (Debug|AnyCPU, the configuration named in both the promoted-potential
  doc and AC8): `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`. Confirmed via
  `QuickFiler.Test\QuickFiler.Test.csproj`: `<AssemblyName>QuickFiler.Test</AssemblyName>`,
  `<OutputPath>bin\Debug\</OutputPath>` under the Debug|AnyCPU property group;
  `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`.
- Framework/library versions, from `QuickFiler.Test\packages.config`:
  - `MSTest.TestFramework` 4.3.3, `MSTest.TestAdapter` 4.3.3, `MSTest.Analyzers` 4.3.3.
  - `Moq` 4.20.72.
  - `FluentAssertions` 8.10.0.
  - `Microsoft.Extensions.TimeProvider.Testing` 10.9.0 (source of `FakeTimeProvider`, already used
    by this test class for the `#222` clock-seam tests).

## Behavior semantics (AC-derived)

- **Success condition**: when the filtered `lines` array (post null/whitespace filter, Anchor A)
  has `Length == 0`, `WriteMetricsAsync` returns without invoking `MetricsFileWriter` — matching
  AC1/AC2 and the EFC sibling's contract at `EfcHomeController.Metrics.cs:72-75`.
- **Failure/regression condition**: any invocation of `MetricsFileWriter` when `lines.Length == 0`
  is a defect recurrence; the new MSTest regression test asserts this via a `bool invoked` flag
  (per the §3 template), matching AC3.
- **Ordering rule**: the new guard must execute after Anchor A (the filter must run first so the
  guard observes the *filtered* count, not the raw `strOutput` count) and before Anchor B (must
  prevent the writer call, not run concurrently with or after it).
- **Non-interaction with calendar write**: `WriteMoveToCalendar` (lines 154-160) executes before
  Anchor A and is unconditional in both the pre-fix and post-fix method; the new guard does not
  suppress the Outlook calendar appointment, only the file write. This is consistent with the
  issue's Expected Behavior section, which addresses only the session-metrics file.
- **Out-of-scope boundary (AC6)**: the `MetricsFileWriter` delegate's `Task` vs. `Task<bool>`
  return type and any failure-logging branch belong to #647, not this item; this item's fix is a
  single `return;` statement inserted between Anchor A and Anchor B, structurally independent of
  what Anchor B's statement looks like after #647 lands.

## Candidate approaches

1. **Insert `if (lines.Length == 0) { return; }` between Anchor A and Anchor B (recommended).**
   Textually mirrors the EFC guard exactly, as directed by AC2 and the issue's "Proposed Fix"
   section. Minimal diff, single early return, consistent with the existing early-return #1 at
   line 131. No interaction with the `Task`/`Task<bool>` signature question owned by #647.
2. **Add the same guard inside `FileIO2.WriteTextFileAsync` instead of the caller.** Rejected: it
   would touch `UtilitiesCS/To Depricate/FileIO2.cs`, a file outside this item's owned-files list
   (AC7), would affect every other caller of that shared writer (broader blast radius than the
   issue describes), and the issue explicitly frames the remedy as "one guard in an owned file,
   mirroring the EFC form" at the call site, not inside the shared writer.

**Rejected alternatives**: approach 2 above (guard inside the shared writer) — out of scope file,
broader blast radius, contradicts the issue's stated remedy location.

## Testing implications

- One new MSTest test in `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, in the
  `#region Issue #442 — metrics flush tests` block (or a new adjoining region referencing #646),
  following the `WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter` template
  identified in §3: `BuildLooseMetricsController` with an all-null/whitespace `diagnostics` array
  and default `withMyDocuments: true`; a `bool invoked` flag set in the `MetricsFileWriter` lambda;
  assert `invoked.Should().BeFalse(...)` after `await controller.WriteMetricsAsync(...)`.
- Per AC4, capture fail-before evidence (test run against the unguarded implementation showing the
  new test fails) under `docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/regression-testing/`.
- No mocks/stubs beyond what `BuildLooseMetricsController` already provides are needed; no
  filesystem or wall-clock access occurs in the new test (the writer lambda is a synchronous
  boolean-flag setter returning `Task.CompletedTask`, consistent with UT4/CUT2 policy).
- Per AC5, `WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce` and
  `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` must not be modified; §4 established
  neither test's `diagnostics` input produces an empty filtered array, so no modification is
  needed for them to keep passing.
- Full toolchain per CLAUDE.md / CUT3: `dotnet tool run csharpier format .` (verify with `check`),
  the analyzer `msbuild /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`,
  the nullable `msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true`, then
  `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`.


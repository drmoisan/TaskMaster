# Code Review — Issue #646 (qfc-metrics-flush-writes-empty-session-file)

Timestamp: 2026-09-01T12-53

| Field | Value |
|---|---|
| Branch | `bug/qfc-metrics-flush-writes-empty-session-file-646` |
| HEAD | `0fe0668f146236c65aa93514fcb9756d366a6940` |
| Base | `origin/main` at `8996b28746d32f9f5996a037e0ca76be78b7684d` |
| Source changed | 2 files, 27 insertions, 0 deletions |
| Blocking findings | **0** |

## Change Under Review

`QuickFiler/Controllers/QfcHomeController.Metrics.cs`, a four-line insertion at lines 175-178
inside `WriteMetricsAsync`:

```csharp
var lines = strOutput.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
if (lines.Length == 0)
{
    return;
}
```

`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, a 23-line insertion adding one
MSTest method, `WriteMetricsAsync_WithAllNullOrWhitespaceDiagnostics_DoesNotInvokeWriter`.

Both are pure insertions. `git diff origin/main...HEAD` reports zero deletions across the whole
branch, so no pre-existing line in either file was altered.

## Quality Assessment

### Design and simplicity — good

The fix is the smallest change that resolves the reported defect. It introduces no new type, no
new seam, no configuration, and no indirection. It closes a stated asymmetry by copying the
shape that already exists in the sibling controller, which is the correct instinct: the two
metrics writers now fail closed the same way for the same input.

The placement is well chosen. The guard sits immediately after the statement that computes
`lines` and before the three-line `CancellationToken.None` explanatory comment, so that comment
stays adjacent to the `await MetricsFileWriter(...)` statement it explains. Hugging the
computing statement also reuses the file's existing blank line as the separator, which is why
the diff is four lines rather than five. The same structural relationship holds in
`EfcHomeController.Metrics.cs`, where the guard follows `var dataLines = ...` with no
intervening blank line.

### Correctness — verified

The guard's condition is total over the domain of `lines`: `ToArray()` on a LINQ `Where` always
yields a non-null array, so `lines.Length` cannot throw and the two outcomes partition the
input completely. Both outcomes are exercised:

- True outcome — the new test supplies `{ "   ", null, "\t" }`, all of which the filter removes,
  and asserts the writer flag stays `false`.
- False outcome — `WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce` supplies two valid
  lines and `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` supplies a partially
  null array that filters down to two. Both still reach the writer and both still pass.

That second pair is the check that matters most for an early return: it bounds the guard from
the over-broad side and demonstrates the guard did not convert the partial-filter case into a
no-op. Recorded in `evidence/regression-testing/existing-tests-pass.2026-08-31T20-04.md`.

### Test quality — good

The new test follows Arrange-Act-Assert with visual separation, carries a six-line XML summary
explaining both the scenario and why the pre-existing MyDocuments guard is not what produces
the early return, and uses the repository-mandated MSTest, Moq, and FluentAssertions triple.
It touches no filesystem, creates no temporary file, performs no wall-clock wait, and depends
on no external service. The `because` reason string on the assertion produces an actionable
failure message, which the RED run demonstrates verbatim:

```
Expected invoked to be False because an empty filtered array must not reach the writer at all, but found True.
```

The RED-before evidence is genuine rather than a harness artifact. The run took 346 ms rather
than sub-millisecond, the message is the assertion's own text rather than empty, and the
fixture supplied `MyDocuments`, so the pre-existing folder guard was not masking the result.
The GREEN run used a byte-identical `/TestCaseFilter`, so the only variable between the two
runs is the four-line guard.

### Naming, comments, and documentation — good

The method name states the input condition and the expected outcome. The XML comment explains
*why* the writer must not be reached (the default writer appends, which creates or touches an
empty file) rather than restating what the code does. No comment was added to the production
file, which is appropriate: the guard is self-evident and the surrounding comments about the
interface contract and the cancellation token remain accurate.

## Findings

All findings below are **non-blocking**. Severity is stated alongside reachability — what would
have to happen for the finding to bite in production or CI.

### CR-1 — Remaining null-array asymmetry with the EFC sibling (Minor, non-blocking)

`strOutput` is dereferenced without a null check at line 174:

```csharp
var lines = strOutput.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
```

The in-file comment directly above reasons explicitly that
`IQfcCollectionController.GetMoveDiagnostics` "carries no XML documentation and therefore no
non-null element guarantee, so this filter defends the interface contract rather than a known
producer defect." That reasoning defends against null *elements* but not against a null
*array*. If an implementation returned `null`, this line throws `NullReferenceException` before
the new guard is ever evaluated.

This is notable because the EFC sibling — the file AC2 makes the reference implementation —
does guard the null case. `EfcHomeController.BuildQuickFileMetricLines` opens with
`if (moved is null || moved.Count == 0) { return Array.Empty<string>(); }`, covering null and
empty together. So while this change closes the empty-array asymmetry, a narrower null-array
asymmetry between the two controllers remains.

**Reachability:** requires an `IQfcCollectionController` implementation whose
`GetMoveDiagnostics` returns `null`. The only production implementation in the repository is
`QfcCollectionController`, which does not, so this is not reachable today. It becomes reachable
if a second implementation is added or the existing one is changed. The interface carries no
contract documentation forbidding it, which is precisely the gap the existing comment
identifies.

**Disposition:** pre-existing, present identically on `origin/main`, and outside AC7's permitted
edit set is not the reason it is left alone — line 174 is in an owned file. It is left alone
because changing it would expand a four-line guard into a behavioural change to the
enclosing method that has no test and no reported defect behind it. Recommend a follow-up that
either documents the non-null contract on the interface or extends the guard to
`strOutput is null || lines.Length == 0`.

### CR-2 — The guard does not suppress the calendar-appointment side effect (Minor, non-blocking)

`WriteMoveToCalendar(...)` is called at line 154, twenty lines before the guard. In a session
where every diagnostic entry is null or whitespace, the guard now prevents the metrics-file
write but the Outlook calendar appointment has already been created.

**Reachability:** every session that reaches this method with an all-null diagnostic array —
the same input the issue describes. The user still gets a calendar entry for a session that
recorded no diagnostics.

**Disposition:** not a regression. This ordering is identical on `origin/main`; before this
change such a session produced both a calendar appointment and an empty metrics file, and now
it produces the appointment alone. The issue's Expected Behavior is explicitly scoped to the
metrics file ("No write occurs and no file is created or touched"), and AC1 is scoped to
`MetricsFileWriter`. Widening the guard to cover the calendar write would be a behavioural
change beyond the reported defect and would violate the Bugfix Workflow's minimal-fix rule.
Worth a follow-up decision on whether an empty session should produce a calendar entry at all.

### CR-3 — Test file at 477 of 500 lines (Minor, non-blocking)

`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` is 477 lines after this change,
up from 454. The repository caps production code, test code, and reusable scripts at 500 lines,
and the cap applies to test files. This change is compliant with 23 lines of headroom.

**Reachability:** the next test method added to this file breaches the cap. The method added
here cost 23 lines including its documentation comment, so a single comparable addition would
exceed 500.

**Disposition:** compliant as delivered. The executor flagged this forward risk in
`evidence/other/test-file-line-count.2026-08-31T20-04.md`, which is the right handling.
Recommend the file be split by region before the next addition.

### CR-4 — New test does not pin the injectable clock (Nit, non-blocking)

`QfcHomeController` exposes an injectable `TimeProvider` seam defaulting to
`TimeProvider.System`, and `WriteMetricsAsync` reads `TimeProvider.GetLocalNow()` at line 124.
The new test does not override it, so that call reads the real system clock.

**Reachability:** none for flakiness. The test asserts only on a boolean invocation flag and
makes no assertion that depends on the returned time, so no clock value can change the outcome.
The determinism rule in `.claude/rules/general-unit-test.md` targets tests whose results depend
on wall-clock time; this one's does not.

**Disposition:** consistent with every sibling test in the same region, which also leave the
seam at its default. No change recommended in isolation.

### CR-5 — Two evidence artifacts quote superseded figures, reconciled elsewhere (Informational)

`evidence/other/test-file-diff-scope.2026-08-31T20-04.md` records 25 insertions and blob
`2d93e1ae`, and `evidence/other/test-file-line-count.2026-08-31T20-04.md` records 479 lines. The
delivered state is 23 insertions, blob `93c9c4d2`, and 477 lines. Similarly,
`evidence/other/anchor-rederivation.2026-08-31T20-04.md` predicts the guard at lines 176-179
whereas it was delivered at 175-178.

**Reachability:** an auditor comparing those two artifacts against the head tree finds figures
that do not match and could read it as undisclosed drift.

**Disposition:** both discrepancies are explicitly reconciled elsewhere in the same evidence
set, so this is a cross-reference cost rather than an evidence gap.
`evidence/qa-gates/csharpier-format.2026-08-31T20-04.md` records the CSharpier pass-1 rewrite
that collapsed the assertion chain, states the resulting 479-to-477 line change, and notes it
does not affect the earlier task's acceptance. `evidence/other/production-diff-scope.2026-08-31T20-04.md`
tabulates the anchor prediction against the re-derived truth and explains the off-by-one as a
deliberate placement choice that keeps the diff at four lines. Both superseding artifacts name
the artifact they supersede. Verified independently: `wc -l` and `awk NR` both report 477, and
`git diff --numstat` reports 23 insertions and 0 deletions.

### CR-6 — Branch history contains an add-then-revert outside the AC7 allowed set (Informational)

Commit `9f578b3c` added two files under `.claude/agent-memory/orchestrator/` (22 insertions).
Commit `8a2054cd` removed them (22 deletions).

**Reachability:** none at merge. `git diff --stat origin/main...HEAD -- .claude` returns empty,
so the merged tree gains nothing under that path. AC7 is evaluated against the branch diff, and
the diff is clean.

**Disposition:** the branch self-corrected before delivery and the correcting commit says so in
its message. Recorded because the history, unlike the diff, shows the excursion.

## Assessment Against the General Code Change Policy

| Principle | Assessment |
|---|---|
| Simplicity first | The minimal correct change. No abstraction introduced for a four-line guard. |
| Reusability | The guard shape is duplicated from the EFC sibling rather than factored out. Correct here — extracting a two-line early return across two controllers with different data types and different writer seams would add indirection without removing meaningful duplication. |
| Extensibility | No public API changed. The `MetricsFileWriter` delegate signature is byte-identical to base, verified by re-reading lines 28-34 of the post-change file. |
| Separation of concerns | Preserved. The writer remains an injectable delegate, which is exactly what makes the new test possible without a filesystem. |
| Error handling | The guard is a no-content short circuit, not an error suppressor. The writer-failure logging branch at lines 189-195 is untouched, so genuine write failures still surface. |
| Interaction with existing code | Matches the sibling controller's established form, as AC2 requires, and matches the test file's existing harness and naming conventions. |

## Verdict

**PASS with 0 blocking findings.**

The change is correct, minimal, well placed, and properly evidenced by a genuine RED-then-GREEN
pair. The six findings above are two pre-existing behavioural asymmetries worth a follow-up
(CR-1, CR-2), one forward-looking file-size risk that is compliant as delivered (CR-3), one nit
with no reachable consequence (CR-4), and two documentation-hygiene observations (CR-5, CR-6).
None requires a change to this branch.

# efcdatamodel-success-path-test-uses-incidental-crash-as-barrier (Issue #699)

- Date captured: 2026-08-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efcdatamodel-success-path-test-uses-incidental-crash-as-barrier/ (Issue #699)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #699
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/699
- Last Updated: 2026-08-29
## Summary

`MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` terminates its success path on a
`NullReferenceException` raised by the `EmailFiler` collaborator rather than on a deliberate stopping
point. The test is correct today, but its failure message would misdirect a future maintainer if
`EmailFiler.SortAsync` ever gains a null guard.

## Environment

- OS/version: Windows 11, .NET Framework 4.8.1
- Python version: not applicable; this is an MSTest C# unit test
- Command/flags used: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~EfcDataModelArchiveRootTests"`
- Data source or fixture: `TestableEfcDataModel` with a strict `Mock<IOlObjects>` and a parameterless `MailItemHelper`

## Steps to Reproduce

1. Add a null guard to `EmailFiler.SortAsync` so it returns `false` instead of dereferencing a null `FolderInfo`.
2. Re-run `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`.

## Expected Behavior

The test terminates the success path deliberately and asserts only the invariant it exists to pin,
which is `olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Once())`. A failure reports a
problem with the archive-root read count.

## Actual Behavior

The test asserts `await act.Should().ThrowAsync<NullReferenceException>()` before the `VerifyGet`. That
exception is not a property of the unit under test: it is the `EmailFiler` collaborator dereferencing
a `MailItemHelper` whose folder information is null, several frames past the code issue 638 touched.
Once `EmailFiler` no longer throws there, the test fails with a message about a missing
`NullReferenceException` and points at the wrong subsystem.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: not captured; the condition is latent and does not reproduce against the current tree.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

Raised as finding CR-1 in `code-review.2026-08-29T13-06.md` for issue 638 and dispositioned there as
Minor and non-blocking. Citations:

- `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs:172-186` — the test body.
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:133` — the dereference that raises the exception the test relies on.
- `QuickFiler/Controllers/EfcDataModel.cs:339` — `OlAncestor = olAncestor,` on the move path.

The arrangement was deliberate and its rationale is documented in the fixture's XML doc comment; the
concern is durability rather than correctness. Note that this test is the only one reaching
`EfcDataModel.cs:339`, so deleting it rather than replacing it would drop changed-line coverage for
issue 638 from 93.10 percent to roughly 89.7 percent, below the 90 percent floor. Any fix must
preserve coverage of that line.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: introduce a filer-construction seam on `EfcDataModel` so the success path can terminate deliberately, then assert only the `VerifyGet` and drop the exception assertion.
- [ ] Integration scenario to retest: the full `EfcDataModelArchiveRootTests` class, confirming 11 of 11 still pass.
- [ ] Manual verification notes: re-measure changed-line coverage for `EfcDataModel.cs` and confirm line 339 remains covered.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Origin: finding CR-1 of the issue 638 code review. Proposed labels: test-quality, quickfiler, follow-up.

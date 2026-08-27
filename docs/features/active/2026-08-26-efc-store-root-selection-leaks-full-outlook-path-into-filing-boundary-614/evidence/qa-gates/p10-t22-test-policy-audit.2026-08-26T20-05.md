# P10-T22 - Test-policy audit (#614; AC22)

Timestamp: 2026-08-26T20-05

Scope: every test file this change created or modified.

## Banned-API search

- `SearchScope:` the 11 test files listed in the per-file table below, read whole-file.
- `SearchPatterns:` fixed-string searches for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`,
  `Random.Shared`, `Path.GetTempPath`, `Path.GetTempFileName`.
- `SearchResult:` `none` for all six.

| Banned API | Hits |
| --- | ---: |
| `Thread.Sleep` | **0** |
| `Task.Delay` | **0** |
| `DateTime.Now` | **0** |
| `Random.Shared` | **0** |
| `Path.GetTempPath` | **0** |
| `Path.GetTempFileName` | **0** |

A companion search for `File.Create` and `Directory.Create` across the same files also returns
**0**, so no test creates a temporary file or directory.

## Per-file table

All 11 files live in the mirrored `*.Test` project trees, never alongside production source.

| File | Project | Status | Tests | MSTest | Moq | FluentAssertions | AAA |
| --- | --- | --- | ---: | --- | --- | --- | --- |
| `Controllers/BreadcrumbBridgeRouterIssue614Tests.cs` | `QuickFiler.Test` | new | 8 | yes | yes | yes | yes |
| `Controllers/BreadcrumbBridgeRouterTests.cs` | `QuickFiler.Test` | edited (AC18 test added) | 17 | yes | yes | yes | yes |
| `Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | `QuickFiler.Test` | edited (P3-T4 spec correction) | 10 | yes | yes | yes | yes |
| `Controllers/EfcSelectionGuardTests.cs` | `QuickFiler.Test` | new | 9 | yes | n/a (pure predicate) | yes | yes |
| `Controllers/EfcDataModelIssue614Tests.cs` | `QuickFiler.Test` | new | 8 | yes | yes | yes | yes |
| `AppGlobals/AppOlObjectsArchiveRootValidationTests.cs` | `TaskMaster.Test` | new | 6 | yes | yes | yes | yes |
| `AppGlobals/AppFileSystemFolderPathsOneDriveResolutionTests.cs` | `TaskMaster.Test` | new | 7 | yes | n/a (delegate seam) | yes | yes |
| `EmailIntelligence/EmailFilerConfig_Tests.cs` | `UtilitiesCS.Test` | edited (5 tests added) | 18 | yes | yes | yes | yes |
| `OutlookObjects/Folder/ArchiveStemContractTests.cs` | `UtilitiesCS.Test` | new | 23 | yes | n/a (pure contract) | yes | yes |
| `OutlookObjects/Folder/FolderConverterIssue614Tests.cs` | `UtilitiesCS.Test` | new | 19 | yes | yes | yes | yes |
| `OutlookObjects/Folder/FolderConverterTests.cs` | `UtilitiesCS.Test` | edited (`:329` assertion only) | 22 | yes | yes | yes | pre-existing style |

"Moq: n/a" marks the three files whose subject is a pure static function with no collaborator to
mock. Repository policy requires Moq for mocks and stubs, not that every test file use it; adding a
mock where there is no collaborator would reduce clarity. This is called out here as the single
deliberate deviation, per the policy's "call out the exception explicitly" rule.

`FolderConverterTests.cs` uses the pre-existing bare Arrange/Act/Assert layout without section
comments throughout. This change's only edit to it is the single `:329` assertion line required by
AC11, so its house style is left as it was rather than being reformatted.

## Core-principle audit

- **Independence.** No test depends on another's state. The one order-sensitivity found during
  Phase 9 - exact-count assertions over a log4net `MemoryAppender`, which log4net binds per TYPE
  and therefore shares with router tests in other classes - was fixed by replacing the counts with
  existence assertions plus a "every matching message is value-free" assertion. Concurrency can
  only add events, never remove them, so the resulting claim is order-independent. The reasoning is
  documented in the test file itself.
- **Isolation.** Every test targets a single unit. All Outlook, WebView2, filesystem, and
  application-globals collaborators are Moq seams or injected delegates; no test creates a form, a
  WebView2 control, a live COM object, or a message pump.
- **Fast execution.** The full 6569-test suite completes in 37.7 s; the slowest test added by this
  change runs in 64 ms.
- **Determinism.** No wall-clock wait, no `Thread.Sleep`, no `Task.Delay`, no ambient clock read,
  no unseeded randomness, no temporary file, and no process-environment mutation. The environment
  reads that D7 introduced are supplied through an injected `Func<string, string>` over an
  in-memory dictionary, exactly so that no test writes to the process environment.
- **Readability.** Every test name states the subject, the condition, and the expected outcome.
  Every new test carries explicit `// Arrange`, `// Act` and `// Assert` comments, and each
  non-obvious case carries a one-line rationale.

## Scenario completeness

Positive, negative, edge and error cases are covered for each unit: valid relative stems versus
rooted values of three shapes; null, empty and whitespace inputs; exact-root and out-of-root
boundaries; the `Archive2` separator-boundary near-miss; repeated-ancestor substrings;
case-differing ancestors; the separator-only root; UNC and sub-three-character ancestors; each of
the four per-segment folder-name rules with its legal counterpart; and message-content assertions
proving no diagnostic embeds a mailbox address or a user-profile path.

## Result

AC22 is satisfied: all six banned-API searches return zero hits, every new or modified test uses
MSTest with FluentAssertions (and Moq wherever a collaborator exists) in Arrange-Act-Assert form,
all files live in the mirrored `*.Test` trees, and no test creates a temporary file or depends on
mutable global state.

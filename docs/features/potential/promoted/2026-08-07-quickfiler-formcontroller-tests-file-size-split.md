# quickfiler-formcontroller-tests-file-size-split (Issue #450)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-formcontroller-tests-file-size-split/ (Issue #450)
- Found during: research for issue #435 (child F6 of epic #136, QuickFiler per-file coverage)

- Issue: #450
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/450
- Last Updated: 2026-08-08
## Problem / Why

`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` is **827 lines** across 42 test methods. The
repository file-size limit is 500 lines and it explicitly covers test code:

> `.claude/rules/general-code-change.md`, "File Size Limit": *No production code, test code, or
> reusable script file may exceed 500 lines.* The listed exceptions are throwaway agent-session
> scripts, raw text fixtures for language-processing test data, and Markdown documentation. A
> long-lived MSTest fixture is none of those.

This is a pre-existing violation, not one introduced by any current work.

## Why It Is Filed Separately Rather Than Fixed In F6

Child F6 of epic #136 adds a substantial number of new test cases for the `QfcFormController` partial
family. Two options were considered during F6 research:

1. **Split inside F6.** Rejected. F6's plan runs one phase per production file, and the four
   `QfcFormController.*` partials are covered by four separate phases. Splitting the shared 827-line
   fixture would require all four phases to edit the same file, serialising work that is otherwise
   independent and creating a high-conflict hotspot inside a single child.
2. **Add new files, leave the existing one alone.** Selected. F6 creates new disjoint test files and
   does not append to `QfcFormControllerTests.cs`, so F6 neither worsens nor repairs the violation.

Filing this separately keeps the known violation visible instead of letting it disappear when the F6
feature folder merges.

## Proposed Behavior

Split `QfcFormControllerTests.cs` into files under 500 lines each, grouped by the production partial
they exercise, mirroring the production tree per `.claude/rules/general-unit-test.md` ("Test File
Location"). Move tests only — do not rewrite assertions, rename tests, or change coverage.

## Constraints & Risks

- **Sequence after epic #136 wave 1.** Children F6 and F7 both touch `QfcFormController` /
  `QfcHomeController` test territory. Running this split concurrently would conflict.
- **Behavior-neutral.** The same set of test methods must exist before and after, and the pass count
  must be identical. This is a pure move.
- `QuickFiler.Test/QuickFiler.Test.csproj` is a non-SDK project with an explicit `<Compile Include>`
  list, so every new file needs an entry.
- Watch for shared private helpers and `[TestInitialize]` setup in the current fixture; they must move
  to a shared `TestSupport` partial rather than being duplicated per split file.

## Related Prior Art

`QuickFiler.Test/Controllers/` already contains split fixtures following this pattern, for example
`QfcStreamingDequeueConfidenceGateTests.cs` with `.Part2.cs` and `.Part3.cs`, and the
`QfcItemController.*Tests.cs` family split by production partial. Prefer the
`QfcItemController.*Tests.cs` style (split by production partial, descriptive suffix) over the
`.Part2/.Part3` style (split by arbitrary overflow).

## Acceptance Criteria (early draft)

- [ ] No file under `QuickFiler.Test/Controllers/` matching `QfcFormController*Tests.cs` exceeds 500 lines.
- [ ] The set of test method names is identical before and after the split.
- [ ] The test pass count is identical before and after the split.
- [ ] Shared setup and private helpers live in one shared partial, not duplicated across split files.
- [ ] `QuickFiler.Test.csproj` has a `<Compile Include>` entry for every new file.
- [ ] Full C# toolchain passes: csharpier, analyzer build, nullable build, coverage-enabled vstest.

## Test Conditions to Consider

- [ ] Before/after test-name inventory diff is empty.
- [ ] Before/after pass-count comparison is equal.
- [ ] No coverage regression on `QfcFormController.*` production files.

## Next Step

- [ ] Promote to GitHub issue (feature request template)

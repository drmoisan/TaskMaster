# quickfiler-collection-controller-coverage

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/ (Issue #454)
- Parent epic issue: #136 (https://github.com/drmoisan/TaskMaster/issues/136)
- Parent epic manifest: docs/features/epics/quickfiler-per-file-coverage/epic.md (child F11, wave 1)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Upstream dependency: F1 `quickfiler-coverage-ledger` (issue #432, wave 0)

> Recreated by the orchestrator after promotion. The MCP promotion tool created issue #454 and
> populated the active feature folder's `issue.md`, but did not leave the potential markdown on
> disk. This file restores the audit trail.

## Problem / Why

`QuickFiler/Controllers/QfcCollectionController.cs` is the single largest production file in the
repository at 2,349 lines. It carries a real `[ExcludeFromCodeCoverage]` attribute, so it is absent
from every Cobertura report the repository produces: it is **unmeasured, not covered**. Its
interface, `QuickFiler/Interfaces/IQfcCollectionController.cs` (118 lines), completes the pair.

Three repository policies are violated or unenforced against this file today:

1. `.claude/rules/general-code-change.md` sets a 500-line ceiling for production files. At 2,349
   lines the file breaches it by a factor of nearly five.
2. `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy states that no production file
   may be excluded from coverage measurement, and that the correct response to untestable lines is
   refactoring, not exclusion.
3. Issue #136 AC1 requires every compiled QuickFiler file to reach >= 80% line coverage or sit on a
   ratified exemption ledger with a file-specific rationale.

The epic manifest makes this file its own child precisely because closing the gap requires three
substantial pieces of work in strict sequence: a partial split into at least five files to satisfy
the 500-line rule, seam extraction so the logic is reachable without live COM or WinForms, and only
then the coverage itself.

## Proposed Behavior

No observable behavior change to QuickFiler flows. The work is a testability refactor plus test
authorship:

1. Split `QfcCollectionController.cs` into coherent partial-class files along logical responsibility
   seams (not mechanical 500-line chops), each under 500 lines.
2. Extract seams — interface seam first, injectable delegate second, adapter third — so the
   controller's logic is reachable from MSTest without constructing live forms, showing popups,
   touching the UI thread, or instantiating Outlook COM objects.
3. Remove `[ExcludeFromCodeCoverage]` and author MSTest/Moq/FluentAssertions tests bringing each
   resulting partial to >= 80% line and >= 75% branch coverage, with newly created files reaching
   >= 90% line coverage per the `CLAUDE.md` new-module rule.

## Acceptance Criteria (early draft)

- [ ] `QfcCollectionController` and every partial it is split into reaches >= 80% line and >= 75%
      branch coverage, verified with F1's per-file harness and recorded under
      `<FEATURE>/evidence/qa-gates/`.
- [ ] `[ExcludeFromCodeCoverage]` is removed from `QfcCollectionController.cs` and the code is
      genuinely covered. A blanket re-exemption of the whole file is not acceptable; an irreducible
      remainder is acceptable only where F1's ledger ratifies it with a file-specific rationale.
- [ ] No production file in scope exceeds 500 lines.
- [ ] Every file newly created by this work reaches >= 90% line coverage.
- [ ] Tests use MSTest, Moq, and FluentAssertions; are deterministic and isolated; create no
      temporary files; contact no external services; construct no live forms; raise no popups.
- [ ] The full C# toolchain (csharpier, analyzers, nullable, MSTest with coverage) is green in the
      final form, and repository-wide coverage is retained or improved.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- **`QuickFiler.csproj` edits are guaranteed.** The project is a legacy non-SDK project with no
  globbing; every new partial needs an explicit `<Compile Include=...>` entry. Per epic.md
  "Cross-Child Constraints" section 1: only this child's own entries, minimal adjacent hunks, and
  CRLF must be preserved. An additive fan-in conflict with siblings is anticipated, not a defect.
- **Upstream dependency on F1** (issue #432) for the per-file coverage harness and the ledger at
  `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. A ledger row must be
  appended for every new file per epic.md "Mid-Wave File Creation".
- **Sibling boundaries.** This controller is consumed by `QfcHomeController` (F7, #433) and consumes
  `IQfcDatamodel` (F5, #436) and `IQfcQueue` (F2, #431). No sibling files may be edited. Seams
  should stay `internal` where possible so F7's conclusion that it needs no contract additions
  remains true.
- **Moq cannot proxy internal QuickFiler types.** `QuickFiler/Properties/AssemblyInfo.cs:5` grants
  `InternalsVisibleTo("QuickFiler.Test")` and nothing else; `DynamicProxyGenAssembly2` is not
  granted. Internal seams are visible to tests but must be injectable delegates or hand-written
  fakes rather than Moq-mocked internal interfaces.
- **No `InternalsVisibleTo` grant** from `UtilitiesCS` to `QuickFiler.Test`
  (`UtilitiesCS/Properties/AssemblyInfo.cs:18-20`). A local seam must be built rather than editing
  that file.
- **Known defects out of scope for fixing.** Issue #444 (`KbdActions` enumerable constructor
  bypasses the duplicate guard, reached via this controller's keyboard registration) and issue #286
  (`RemoveSpecificControlGroupAsync` static reentrancy counter leaks on exception because the
  decrement is not in a `finally`). Both are characterized, not fixed, under the epic's
  no-behavior-change NFR.
- **Starting coverage is unknown and likely near zero.** The exemption removes the file from
  instrumentation entirely, so no historical number exists. Two test files
  (`QfcCollectionControllerTests.cs` at 500 lines, `QfcCollectionControllerDarkModeTests.cs` at 155)
  already exist; what they actually reach given the exemption is established during research.

## Test Conditions to Consider

- [ ] Per-partial unit coverage for every responsibility group produced by the split.
- [ ] Seam-level tests exercising injected delegates and adapters without live COM or forms.
- [ ] Characterization of the #444 duplicate-`KaKey` registration and the #286 counter leak without
      changing behavior.
- [ ] STA last-resort tests, if unavoidable, isolated in dedicated `*.StaTests.cs` files.
- [ ] Branch-coverage-sensitive scenarios (the 75% branch gate is independent of the 80% line gate).

## Next Step

- [x] Promote to GitHub issue (feature request template) -> #454
- [x] Create `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/` folder
      from the template

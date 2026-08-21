# quickfiler-item-controller-coverage (Potential)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> Issue #453, active folder
  `docs/features/active/2026-08-07-quickfiler-item-controller-coverage-453/`
- Parent epic: #136 (QuickFiler Per-File 80% Coverage)
- Epic child: F10 (wave 1, complexity band C3)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`

> Audit-trail note: this file was recreated by the orchestrator after promotion. The MCP
> `potential_to_issue` tool reported a `destination_path` under `potential/promoted/` but did not
> leave the file on disk for this feature entry (the six sibling bug promotions in the same run did
> persist). Content below is the promoted source text as submitted.

## Problem / Why

Epic #136 requires every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj` to reach
at least 80% line coverage and 75% branch coverage, or to sit on an explicitly ratified exemption
ledger. The `QfcItemController` partial-class family is one of the largest single clusters in that
denominator: 10 production partials plus one interface file, 3,180 lines in total.

The family is already substantially tested — 17 existing test files with 166 test methods exist
under `QuickFiler.Test/Controllers/` — so this is a **gap-closure and exemption-removal** exercise
rather than a build-from-zero effort.

## Proposed Behavior

Raise every `testable` file in the `QfcItemController` family to at least 80% line and 75% branch
coverage, verified with the per-file coverage harness delivered by epic child F1 (#432), and reduce
the `[ExcludeFromCodeCoverage]` boundary where reduction is legitimately available.

No observable behavior change to QuickFiler flows. Testability refactors follow the epic's seam
hierarchy: interface seam, then injectable delegate, then adapter.

## Acceptance Criteria (early draft)

Superseded by `spec.md` AC-1..AC-21 and `user-story.md` US-1..US-14 in the active feature folder,
which are the authoritative acceptance-criteria sources for `full-feature` work mode.

- [ ] Every `testable` file in scope reaches >= 80% line and >= 75% branch coverage.
- [ ] The `[ExcludeFromCodeCoverage]` boundary is reduced where legitimately available and every
      retained attribute is justified against a recorded authority.
- [ ] No production file in scope exceeds 500 lines; any newly created file reaches >= 90% line
      coverage.
- [ ] Tests use MSTest, Moq, and FluentAssertions; deterministic, isolated, no temporary files,
      no external services, no live forms, no popups.
- [ ] Full C# toolchain green in final form; repository-wide coverage retained or improved.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- **One partial-class family.** All 10 partials declare the same type, deliberately kept in a single
  epic child so no two children share a type or a test fixture.
- **A maintainer-ratified exemption boundary already exists.** Preparation research established that
  18 of the family's 19 attributes were ratified under issue #227 on 2026-07-02 after five
  remediation cycles reduced the boundary from 103 members to 19. Nine of those are blocked by an
  unbuilt WinForms message-pump test seam tracked as open issue #230, explicitly deferred and
  explicitly not a merge condition. The realistic outcome is 19 -> 15, not 19 -> 0.
- **Sibling boundaries.** `ConversationResolver` belongs to F4 (#434) and is invoked positionally.
  `KeyboardHandler.cs` belongs to F3 (#430). `QfcCollectionController.cs` belongs to F11 (#454).
  Required upstream changes are cross-child contract notes, not sibling edits.
- **State-transition invariants.** Event wiring, navigation, and initialization carry ordering,
  re-entrancy, and dispose-before-setup invariants.
- **Determinism.** Injected clock and fake timers only.
- **Upstream dependency.** F1 (#432) delivers the per-file coverage harness and the exemption ledger.
  A Phase 0 execution-time halt gate covers both.
- **Branch coverage is the binding gate**, not line coverage — it fails on seven of ten files.

## Next Step

- [x] Promote to GitHub issue (feature request template) — Issue #453
- [x] Create the active feature folder from the template

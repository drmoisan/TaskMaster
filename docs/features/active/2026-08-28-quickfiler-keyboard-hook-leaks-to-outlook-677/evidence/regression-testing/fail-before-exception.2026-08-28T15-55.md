# Fail-Before Exception Dossier (P1-T6) — Issue #677

Timestamp: 2026-08-28T15-55
Command: N/A (dossier; the underlying command is recorded in `p1-t5-expectfail-build.md`)
EXIT_CODE: 0

WhyFailingRunImpossible: The seventeen regression tests authored in Phase 1 reference a
guard surface that does not exist in the pre-fix codebase — the internal `MayTakeFocus` property on
`BreadcrumbDropDownHost`, the `FormDeactivated` / `IsWebView2Focused` / `ParkFocusOffWebView2`
members on `IQfcFormViewer`, and `CancelBreadcrumbSelector` on `IItemViewer` and
`IQfcItemController`. Those references are compile-time (typed) references, not reflection lookups,
so `QuickFiler.Test` cannot be compiled at all before the Phase 2/3 fix lands. No test assembly is
produced, therefore no test host can load it and no failing test *run* can be recorded. The
conventional red-then-green run is structurally impossible here, and the equivalent proof is a
build that fails with exactly the missing-member diagnostics naming the absent surface.

## Alternative proof — absence-of-surface proof from the compiler

The absence proof is the P1-T5 build, recorded at
`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p1-t5-expectfail-build.md`
with the full teed output at
`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p1-t5-expectfail-build.msbuild.txt`.

Facts established by that run:

1. `EXIT_CODE: 1` against a declared `ExpectedExitCode: 1`.
2. 22 unique compiler diagnostics: 20 `CS1061` (missing member) plus 2 `CS1503` collateral
   argument-type failures on the two Moq `Returns(...)` setups whose property is itself one of the
   missing members.
3. Every one of the 22 diagnostics is located in one of the three test files created by
   P1-T1..P1-T3. Zero diagnostics are raised in any production file, and only the `QuickFiler.Test`
   project node failed; every production assembly compiled cleanly. The build is therefore red
   *because the guard is absent*, not because the tree is broken.
4. The distinct missing-member messages name all five required members:
   `MayTakeFocus`, `FormDeactivated`, `IsWebView2Focused`, `ParkFocusOffWebView2`, and
   `CancelBreadcrumbSelector` (on both `IItemViewer` and `IQfcItemController`).

## Why the compile-time reference is load-bearing

Decision D2 requires the new test code to reference the guard surface with typed references so the
absence is a compile error. `PredicateHarness` therefore constructs the concrete
`BreadcrumbDropDownHost` directly through its internal nine-argument constructor (reachable via
`[assembly: InternalsVisibleTo("QuickFiler.Test")]`) and assigns
`Host.MayTakeFocus = () => AllowFocus;` in typed code. Had the harness reached the property by
reflection instead, the file would have compiled at baseline and the "fail-before" signal would have
degraded from a compiler-proved absence to a runtime `NullReferenceException` that proves much less.

## Green-flip counterpart

The matching green-flip is P3-T10, which re-runs the identical command after the Phase 2 and Phase 3
production edits and the single sanctioned structural-enabler test edit (P3-T7, the
`FakeQfcItemController` interface-completion member). No assertion, test method, or `[TestMethod]`
body is changed between P1-T5 and P3-T10.

## Negative-claim auditability

SearchScope: `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/`
SearchPatterns: `fail-before-exception.*.md`
SearchResult: `fail-before-exception.2026-08-28T15-55.md` (this file) — exactly one match.

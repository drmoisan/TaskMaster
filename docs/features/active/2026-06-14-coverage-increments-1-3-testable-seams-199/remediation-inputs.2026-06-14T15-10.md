# Remediation / Scope-Change Inputs — Issue #199 (2026-06-14T15-10)

- Canonical issue number: 199
- Trigger: Maintainer-directed scope change (chose option B). The prior feature-review (2026-06-14T14-30) returned GO with 0 blocking findings; two ACs were PARTIAL (spec-authorized Flag-and-Stop gaps).
- Active folder: `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/`
- Branch: `refactor/coverage-increments-1-3-199`

## Directive

The maintainer has AUTHORIZED minimal production seams to close the two Flag-and-Stop coverage gaps that were previously left open under the test-only constraint. The spec's "zero production change" Non-Goal is lifted ONLY for the two narrow seams below. Introduce the smallest seam that enables reliable, deterministic unit testing, following the repo design principles (separation of concerns, minimal DI, no broad refactors, no behavior change).

## Gap 1 — ProjectEntry malformed-ID dialog branches (AC1)

`ProjectEntry.SetProjectId` (and a `CompareTo` tie-break) route through the static `MyBox.ShowDialog`. `MyBox` already exposes an injectable dialog seam (`MyBox.DialogInvoker`) but it is `internal` to `UtilitiesCS` and currently only visible to `UtilitiesCS.Test` via `InternalsVisibleTo`.

Authorized minimal seam:
- Add `[assembly: InternalsVisibleTo("ToDoModel.Test")]` to `UtilitiesCS` (assembly-attribute change only) so `ToDoModel.Test` can set `MyBox.DialogInvoker` to a deterministic test stub.
- If `MyBox.DialogInvoker` is not in fact a settable injection point usable from a test (verify), introduce the smallest settable seam on `MyBox` to allow a test to supply a dialog result without showing a WinForms dialog. Prefer the existing seam; only add one if required, and flag if the change is larger than an assembly attribute + a settable internal property.
- Then add `ToDoModel.Test` cases covering the malformed-ID validation branch, the change-confirmation branch, and the `CompareTo` length tie-break — injecting the stub dialog invoker so no real dialog is shown.

Constraints: no behavior change to production logic; the dialog seam must default to the real dialog in production; tests must be deterministic with no WinForms message loop.

## Gap 2 — AppFileSystemFolderPaths.MatchBestSpecialFolder (AC3)

`MatchBestSpecialFolder(string)` is pure LINQ over the `SpecialFolders` collection, but every accessible constructor runs `LoadFolders()` (which calls `Directory.CreateDirectory`, a filesystem mutation) and `SpecialFolders` has only a `protected` setter, so the method is unreachable in isolation without prohibited filesystem use.

Authorized minimal seam (engineer selects the smallest that fits repo design principles):
- Preferred: extract the pure matching logic into a static/pure method (e.g. `MatchBestSpecialFolder` delegating to a pure helper that takes the folder collection as a parameter), testable directly without constructing the object or touching the filesystem; OR
- Make `SpecialFolders` settable from tests via an `internal` setter (or an `internal` test-only constructor that skips `LoadFolders`) plus `[assembly: InternalsVisibleTo("TaskMaster.Test")]` on `TaskMaster` if not already present.
- Do NOT add filesystem access, temp files, or external dependencies to the tests.
- Then add `TaskMaster.Test` cases for `MatchBestSpecialFolder` (best-match, case/trailing-separator, no-match) per the method contract — confirm exact matching semantics against the method body before asserting edge behavior.

Constraints: no behavior change to production matching logic; the seam is structural only.

## Cross-cutting requirements

- Production changes are limited to the two seams above (UtilitiesCS assembly attribute + optional MyBox seam; TaskMaster MatchBestSpecialFolder seam + optional assembly attribute). Any production change beyond these is a flag-and-stop.
- New/changed production code must meet >= 90% coverage; no coverage regression on changed lines.
- Full C# toolchain green (csharpier, analyzers, nullable, MSTest); deterministic tests; MSTest + Moq + FluentAssertions; AAA; no temp files; no external deps; no live Outlook/WinForms.
- Update `spec.md`: lift the zero-production-change Non-Goal for these two seams, and re-point AC1 and AC3 to the new tasks so they become fully PASS.
- Do not touch the already-completed increment test files except to add the newly-enabled cases.

## Acceptance for this cycle

- `UtilitiesCS` exposes the dialog seam to `ToDoModel.Test`; `ProjectEntry` malformed-ID / confirmation / `CompareTo` tie-break branches covered deterministically. AC1 fully PASS.
- `AppFileSystemFolderPaths.MatchBestSpecialFolder` is unit-tested without filesystem mutation via the minimal seam. AC3 fully PASS.
- Toolchain green; coverage strictly increases vs the prior #199 state; no production behavior change.

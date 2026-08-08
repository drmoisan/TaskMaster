# quickfiler-efc-form-item-controller-coverage (Issue #452)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Active — preparation (epic child F9)

- Issue: #452
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/452
- Parent epic issue: #136
- Epic: `quickfiler-per-file-coverage`
- Epic child: F9 (wave 1)
- Depends on: F1 `quickfiler-coverage-ledger` (issue #432)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Last Updated: 2026-08-07
- Work Mode: full-feature

## Problem / Why

Three of the four production files compiled from the EFC form/item controller cluster carry a real
`[ExcludeFromCodeCoverage]` attribute. That attribute removes them from instrumentation entirely, so
they do not appear in any coverage report at all. **They are unmeasured, not covered.** Their absence
from the committed Cobertura report must not be read as coverage.

This is the heaviest seam-extraction child in the epic:

| File | Lines | `[ExcludeFromCodeCoverage]` | Notes |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/EfcItemController.cs` | 1,170 | Yes (line 25, verified) | Breaches 500-line limit |
| `QuickFiler/Controllers/EfcFormController.cs` | 1,086 | Yes (line 27, verified) | Breaches 500-line limit |
| `QuickFiler/Viewers/EfcViewer.cs` | 162 | Yes (line 20, verified) | Form-derived |
| `QuickFiler/Viewers/EfcViewer.Designer.cs` | 4,276 | No attribute | Generated; exempt-candidate |

~2,418 testable lines across three files. Both controllers additionally breach the repository
500-line file-size limit and require partial splits.

Per the epic's policy reconciliation, the `CLAUDE.md` §UT2 COM/VSTO/WinForms exemption qualifier
"without an injectable seam" is a live obligation rather than a standing permission. These three
attributes are therefore treated as unratified: each must either be removed and the file covered by
seam extraction, or justified by F1's ledger against the irreducible-remainder standard with a
file-specific rationale.

## Proposed Behavior

Extract injectable seams from the EFC form and item controllers and the EFC viewer so their logic is
reachable by deterministic unit tests; split both oversized controllers into cohesive partial files
under the 500-line limit; remove the three `[ExcludeFromCodeCoverage]` attributes; and bring every
testable file to the epic's per-file coverage floors with numeric evidence. No observable QuickFiler
behavior changes.

## Acceptance Criteria

- [ ] AC1 — Every file classified `testable` in F1's ledger within F9's scope reaches **>= 80% line coverage**, verified with F1's per-file harness, recorded as numeric evidence under `docs/features/active/2026-08-07-quickfiler-efc-form-item-controller-coverage-452/evidence/qa-gates/`.
- [ ] AC2 — Every such file also reaches **>= 75% branch coverage**. Line and branch are independent gates and both are reported.
- [ ] AC3 — `[ExcludeFromCodeCoverage]` is removed from `EfcItemController.cs`, `EfcFormController.cs`, and `EfcViewer.cs`, and each reaches the floors via seam extraction — unless F1's ledger ratifies a specific irreducible remainder with a file-specific rationale meeting the irreducible-remainder standard.
- [ ] AC4 — No production file in F9's scope exceeds **500 lines** after refactor. `EfcViewer.Designer.cs` is exempt from this limit as generated code.
- [ ] AC5 — Every production file newly created by F9 (partial splits, seam types) reaches **>= 90% line coverage** per the `CLAUDE.md` §UT2 new-module rule, and has a `<Compile Include>` entry in `QuickFiler/QuickFiler.csproj` plus an appended row in the epic coverage ledger, added in the same change.
- [ ] AC6 — All tests use **MSTest**, **Moq**, and **FluentAssertions**, follow Arrange–Act–Assert, and are deterministic and isolated: no temporary files, no external services, no live forms, no popups, no message pumps, no `Thread.Sleep`/`Task.Delay`, no unseeded randomness, no direct wall-clock reads.
- [ ] AC7 — Any test relying on the epic's STA last-resort clause is confined to a dedicated `*.StaTests.cs` file, constructs only never-shown in-memory WinForms controls, and documents why no seam could isolate the logic.
- [ ] AC8 — The full C# toolchain passes in order in its final form: `csharpier` → msbuild analyzers → msbuild nullable → vstest with coverage.
- [ ] AC9 — Repository-wide line coverage is **retained or improved** against the measured baseline on this branch.
- [ ] AC10 — **No behavior change** to observable QuickFiler flows. Characterization tests pin current behavior; open defect #439 is explicitly not fixed by this feature.
- [ ] AC11 — Latent defects discovered during research or execution are promoted to GitHub issues via the MCP promotion lifecycle, not left as feature-folder prose.

## Constraints & Risks

### Sibling boundaries (do not edit)

- `Controllers/EfcHomeControllerDependencies.cs` and `Controllers/EfcHomeControllerDependencyFactories.cs` belong to **F8 (#437)** and are the injection-seam contract for the whole EFC family. F8's preparation concluded all its changes are additive and test-only, so F9 needs no edit from F8 and depends on the existing shapes.
- `Helper Classes/EfcViewerQueue.cs` belongs to **F4 (#434)**. `EfcViewerQueue.Dequeue()` is consumed as a **method group**, so any new behavior there must be a **new overload**, never an optional parameter — an optional parameter breaks method-group conversion. Record as a cross-child contract note in `spec.md`; do not edit the file.
- `Controllers/BreadcrumbBridgeRouter.cs` and the breadcrumb viewer surface belong to **F12 (#1012)**.

### Shared-file constraint

`QuickFiler/QuickFiler.csproj` is a legacy non-SDK project with **no globbing**; every source file is
an explicit `<Compile Include>` entry. F9's partial splits require editing it. Rules: entries for
F9-owned files only, no property/reference/ordering changes, minimal adjacent hunks, and
**preserve CRLF** — never a git-bash `sed -i`, which strips CRLF and produces a whole-file diff
guaranteed to conflict at fan-in.

### Assembly-boundary constraint

`UtilitiesCS/Properties/AssemblyInfo.cs` grants `InternalsVisibleTo` to `DynamicProxyGenAssembly2`,
`UtilitiesCS.Test`, and `ToDoModel.Test` — but **not** to `QuickFiler.Test`. Any `UtilitiesCS`
internal is unreachable from a QuickFiler test. Build a local seam in F9's own assignment; do not
widen the internals grant.

### Open-issue conflict risks

Found by open-issue keyword search, not a folder scan — a promoted-but-not-yet-active issue is
invisible to `docs/features/active/` scanning:

- **#439 `Bug: efcviewer-missing-lineage-and-segment-navigation`** (High) — an open behavior defect whose mechanism runs directly through `EfcViewer.cs` and `EfcFormController.cs`. F9 must **not** fix it under the no-behavior-change NFR, but F9's refactor touches those exact paths. Characterization tests must pin **current** behavior, not the behavior #439 requests, or the two will conflict semantically.
- **#441 `Cobertura post-processing double-counts <line> nodes`** — open, and directly threatens the numeric acceptance evidence for AC1/AC2/AC9. F9 must confirm its measurement is not inflated.
- **#230** — WinForms message-pump test seam work; relevant precedent for AC7's STA determination.
- **#450 `Refactor: quickfiler-formcontroller-tests-file-size-split`** — verify whether it concerns this type's tests or `QfcFormController`'s.

### Upstream dependency

F1 (#432) delivers the per-file coverage harness and the ratified exemption ledger at
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. F1 is prepared concurrently
and its outputs do not exist yet. F9's plan consumes that contract and asserts a Phase 0 halt gate on
its deliverables.

## Test Conditions to Consider

- [ ] Per-member positive, negative, edge, error, and state-transition scenarios for both controllers
- [ ] Characterization tests pinning current breadcrumb/lineage behavior (guarding AC10 against #439)
- [ ] Seam-substitution tests proving each extracted interface/delegate is honored
- [ ] STA last-resort tests, isolated to `*.StaTests.cs`, only where no seam can isolate the logic
- [ ] Per-file line and branch coverage measurement for all four scope files
- [ ] Repository-wide before/after coverage comparison

## Next Step

- [x] Promote to GitHub issue (#452)
- [x] Create active feature folder
- [ ] Complete per-file research
- [ ] Author `spec.md` and `user-story.md`
- [ ] Produce the atomic plan and clear atomic-executor preflight

# quickfiler-breadcrumb-dropdown-webview-coverage (Promoted)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> Issue #455 -> `docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455/`
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/455
- Work Mode: full-feature
- Parent epic: #136 (QuickFiler per-file 80% coverage)
- Epic child: F13 of `quickfiler-per-file-coverage`
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Upstream dependency: F1 `quickfiler-coverage-ledger` (issue #432)

> Audit-trail note: the MCP promotion tool reported this destination path in its receipt but the
> file was not present on disk afterwards. It is reconstructed here so the promotion lifecycle
> remains auditable. The authoritative record is issue #455 and the active feature folder.

## Problem / Why

Child F13 of epic #136 owns the QuickFiler breadcrumb drop-down surface and the WebView2 host
(15 compiled files, ~3,111 lines under `QuickFiler/Viewers/`). Eight coordinator files are already
well covered; three WebView2 files carry class-level `[ExcludeFromCodeCoverage]` and are absent
from instrumentation entirely; one file carries seven method-level exemptions; four are
interface-only declarations.

## Proposed Behavior

Raise every `testable` file in the F13 assignment to at least 80% line and 75% branch coverage,
verified with F1's per-file harness, and either remove the `[ExcludeFromCodeCoverage]` attributes
with the code genuinely covered or retain only a file-specific irreducible remainder argued against
the exemption grounds in the epic's Shared Design section 1. No observable behavior change to
QuickFiler flows.

## Outcome of Preparation (2026-08-08)

Promotion, twelve per-file research artifacts, `spec.md` (AC-1..AC-25), `user-story.md`
(US-1..US-11), and a 14-phase / 220-task atomic plan were produced. The plan passed the MCP plan
validator and cleared `atomic-executor` preflight on the third round.

Research materially corrected several premises carried in the epic manifest and the original
delegation brief; those corrections are recorded as D1-D13 in the active folder's `issue.md` and in
`spec.md` section 11.

Six latent defects were promoted to their own issues rather than left as feature-folder prose, per
the epic's Latent Defect Promotion rule: **#457, #458, #462, #475, #476, #477**.

## Next Step

- [x] Promote to GitHub issue (#455)
- [x] Create the active feature folder
- [ ] Atomic execution — deferred to `epic-orchestrator` after F1 (#432) merges

# quickfiler-datamodel-coverage (Promoted)

- Date captured: 2026-08-07
- Date promoted: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/`
- Issue: [#436](https://github.com/drmoisan/TaskMaster/issues/436)
- Work Mode: full-feature
- Parent epic issue: [#136](https://github.com/drmoisan/TaskMaster/issues/136)
- Epic: `quickfiler-per-file-coverage` (child F5, wave 1)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Upstream dependency: F1 `quickfiler-coverage-denominator-and-exemption-ledger`

> **Provenance note.** This document was recreated by the orchestrator after promotion. The
> `mcp__drm-copilot__potential_to_issue` receipt reported this `destination_path`, but neither the
> original potential entry nor the promoted copy persisted on disk. GitHub issue #436 was created
> successfully and the active feature folder was populated, so promotion itself succeeded; only the
> local markdown artifact was missing. It is restored here to keep the lifecycle audit trail intact.

## Problem / Why

Parent epic issue #136 requires every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj`
to reach at least 80% line coverage or to sit on an explicitly ratified exemption ledger. This child
covers the QuickFiler data-model cluster: the `QfcDatamodel` partial-class family, the `EfcDataModel`
class, and the `IQfcDatamodel` contract.

## Scope (5 files, 1,283 lines)

| File | Lines | `[ExcludeFromCodeCoverage]` |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 496 | yes (type-scoped) |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 177 | no |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | 154 | no |
| `QuickFiler/Controllers/EfcDataModel.cs` | 397 | no |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 59 | no |

## Outcome of Preparation

Preparation completed on 2026-08-08. Deliverables live in the active feature folder:

- `issue.md`, `spec.md`, `user-story.md` (8 acceptance criteria, mirrored across both AC sources)
- Five per-file research artifacts under `research/`, satisfying the issue #136 per-file mandate
- `plan.2026-08-07T20-42.md` — 13 phases, 329 atomic tasks, 157 individual test-case tasks
- Executor preflight: `PREFLIGHT: ALL CLEAR` after three iterations

Atomic execution is deferred to `epic-orchestrator`.

## Next Step

- [x] Promote to GitHub issue (feature request template)
- [x] Create the active feature folder from the template
- [ ] Execute the atomic plan (deferred to `epic-orchestrator`)

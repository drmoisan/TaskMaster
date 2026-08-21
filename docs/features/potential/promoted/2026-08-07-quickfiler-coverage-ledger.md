# quickfiler-coverage-ledger (Potential — Promoted)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> Issue [#432](https://github.com/drmoisan/TaskMaster/issues/432) -> `docs/features/active/2026-08-07-quickfiler-coverage-ledger-432/`
- Parent epic issue: [#136](https://github.com/drmoisan/TaskMaster/issues/136)
- Epic: `quickfiler-per-file-coverage` (child F1, wave 0)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Promotion type: feature
- Work mode: full-feature

> Recreated for the lifecycle audit trail. The `potential_to_issue` MCP operation created issue #432
> and populated the active feature folder, but did not leave this file on disk. The authoritative
> current content lives in
> `docs/features/active/2026-08-07-quickfiler-coverage-ledger-432/issue.md`, `spec.md`, and
> `user-story.md`.

## Problem / Why

Epic #136 requires that every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj`
reach at least 80% line coverage or sit on an explicitly ratified exemption ledger. Fifteen sibling
child features and the capstone are blocked on three shared prerequisites that must be settled
exactly once:

1. **The denominator is undefined.** No authoritative per-file classification exists stating which
   compiled files are `testable` and which are `ratified-exempt`. Without it a child cannot state
   its own acceptance criteria.
2. **The existing `[ExcludeFromCodeCoverage]` attributes are unratified.** Until each has a recorded
   disposition, children would independently and inconsistently decide whether to remove or keep
   them. An attribute on a testable seam is a Blocking finding per the epic manifest.
3. **There is no per-file coverage measurement.** `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
   emits a Cobertura report, but nothing derives per-file line-coverage percentages from it.

Aggregate assembly coverage does not satisfy issue #136, which measures success per production file.

## Proposed Behavior

The wave-0 enabler for epic #136. No QuickFiler production behavior changes; no file under
`QuickFiler/` is modified. Three deliverables:

1. A per-file classification ledger at
   `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`, with a machine-readable
   sidecar at `coverage-ledger.json`, covering every file listed as `<Compile Include=...>` in
   `QuickFiler/QuickFiler.csproj`.
2. A recorded disposition for every existing `[ExcludeFromCodeCoverage]` attribute usage in the
   compiled surface — either `ratified` with rationale, or `remove` naming the owning child.
3. A repeatable per-file coverage report harness in PowerShell that consumes the existing Cobertura
   output and exits non-zero when a `testable` file is below 80%.

## Outcome of Promotion

- GitHub issue: [#432](https://github.com/drmoisan/TaskMaster/issues/432)
- Active feature folder: `docs/features/active/2026-08-07-quickfiler-coverage-ledger-432/`
- Child branch: `feature/quickfiler-coverage-ledger`
- Spawned tracking issue: [#441](https://github.com/drmoisan/TaskMaster/issues/441), a latent
  repo-wide Cobertura `lines-valid` double-count defect surfaced during research and deliberately
  scoped out of #432.

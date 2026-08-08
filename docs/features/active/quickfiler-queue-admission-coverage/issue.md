# quickfiler-queue-admission-coverage (Issue #431)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-queue-admission-coverage/ (Issue #431)
- Parent epic: `quickfiler-per-file-coverage` (epic issue #136), child F2, wave 1

- Issue: #431
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/431
- Last Updated: 2026-08-08
- Work Mode: full-feature

## Problem / Why

Issue #136 requires every testable production file compiled by `QuickFiler.csproj` to reach at
least 80% line coverage, or to sit on a ratified exemption ledger, while retaining or improving
repository-wide coverage. This child, F2 of the epic, covers the QuickFiler queue and admission
logic: `QfcQueue.cs`, `FilerQueue.cs`, `QfcRemainingQueueAdmission.cs`,
`QfcStreamingDequeueConfidenceGate.cs`, `QfcHighConfidencePreFilter.cs`,
`QfcScanProgressBandMapper.cs`, `BreadcrumbOutboundQueue.cs`, `EmailSorter.cs`,
`QfcItemGroup.cs`, `IQfcQueue.cs`, `IQfcQueue1.cs` (11 files, ~1,471 lines). Two of these
(`QfcHighConfidencePreFilter.cs`, `QfcScanProgressBandMapper.cs`) currently carry
`[ExcludeFromCodeCoverage]`, which per the epic's policy reconciliation must be removed unless
the F1 exemption ledger ratifies it as irreducible. `QfcQueue.cs` also exceeds the repository's
500-line file limit and needs a partial-class split.

## Proposed Behavior

No observable behavior change to QuickFiler flows. Deliver per-file line coverage >= 80% for
every testable file in scope, verified with F1's per-file coverage harness, with numeric evidence
committed under this feature's `evidence/qa-gates/`. Split `QfcQueue.cs` into compliant partials.
Remove `[ExcludeFromCodeCoverage]` from any file in scope unless F1's ledger ratifies it as
irreducible. Research and planning proceed one production file at a time per issue #136's mandate:
a separate research artifact per file, a separate atomic-plan phase per file, and each individual
test case as its own atomic task.

## Acceptance Criteria (early draft)

- [ ] Every testable file in scope reaches >= 80% line coverage, verified with F1's per-file
      harness, recorded as numeric evidence under `evidence/qa-gates/`.
- [ ] `QfcQueue.cs` (610 lines) is split so no production file in scope exceeds 500 lines.
- [ ] Any `[ExcludeFromCodeCoverage]` attribute in scope is removed with the file covered, or
      retained only where F1's ledger ratifies it as irreducible.
- [ ] New/modified tests use MSTest, Moq, and FluentAssertions; deterministic, isolated, no
      temporary files, external services, or live forms; injected clock and seeded RNG.
- [ ] Coverage per file spans positive path, invalid-input, boundary, and error-handling behavior.
- [ ] Full C# toolchain (csharpier, analyzer build, nullable build, coverage-enabled vstest) passes.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- Depends on F1 (`quickfiler-coverage-ledger`, wave 0) for the exemption ledger and the per-file
  coverage measurement harness; F1's outputs do not exist on disk at preparation time and are
  expected to merge to the integration branch before this child executes.
- `QfcQueue` and the admission/confidence-gate types carry concurrency and ordering invariants;
  tests require an injected clock and seeded RNG, never `Thread.Sleep`/`Task.Delay`/real waits.
- Seam hierarchy: interface seam, then injectable delegate, then adapter, per the epic's shared
  design; STA last-resort clause applies only to never-shown WinForms controls, not in scope here.
- In-flight issue #424 (`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424`)
  touches `QfcStreamingDequeueConfidenceGate.cs` and related high-confidence queue admission paths;
  research must read that feature folder and account for whichever version has merged.
- Substantial partial test coverage already exists; research must establish actual current
  per-file coverage and target only genuine gaps, not duplicate existing tests.

## Test Conditions to Consider

- [ ] Unit coverage areas: queue admission ordering, confidence-gate scan/deadline behavior,
      high-confidence pre-filter thresholds, scan-progress band mapping, breadcrumb outbound
      queue, email sorting, item grouping.
- [ ] Integration scenarios: none (host-neutral, COM-independent logic; no external services).
- [ ] CLI/API examples: n/a.

## Next Step

- [ ] Promote to GitHub issue (feature request template), citing parent epic issue #136.
- [ ] Create `docs/features/active/quickfiler-queue-admission-coverage/` folder from the template.

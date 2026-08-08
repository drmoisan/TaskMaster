# quickfiler-queue-admission-coverage — Spec

- **Issue:** #431
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08
- **Status:** Draft
- **Version:** 0.1

## Overview

Issue #136 requires every testable production file compiled by `QuickFiler.csproj` to reach at
least 80% line coverage, or to sit on a ratified exemption ledger, while retaining or improving
repository-wide coverage. This child, F2 of the epic, covers the QuickFiler queue and admission
logic: `QfcQueue.cs`, `FilerQueue.cs`, `QfcRemainingQueueAdmission.cs`,
`QfcStreamingDequeueConfidenceGate.cs`, `QfcHighConfidencePreFilter.cs`,
`QfcScanProgressBandMapper.cs`, `BreadcrumbOutboundQueue.cs`, `EmailSorter.cs`,
`QfcItemGroup.cs`, `IQfcQueue.cs`, `IQfcQueue1.cs` (11 files, ~1,471 lines).

Research corrects two items from the epic's file-level table. First, only one of the two files the
epic marks `[X]` actually carries `[ExcludeFromCodeCoverage]` today: `QfcHighConfidencePreFilter.cs`
carries it, but only on the inner `FolderScoringService` adapter (a COM/classifier-bound seam
endpoint), not on the file's primary testable surface, which this research recommends the F1 ledger
ratify as irreducible. `QfcScanProgressBandMapper.cs` carries **no** `[ExcludeFromCodeCoverage]`
attribute at all on the current worktree — the epic's `[X]` marker for this file is stale; the file
was extracted by issue #424 specifically to be testable and already sits at 100% line/branch
coverage per that issue's own feature audit. Second, issue #424's changes to
`QfcStreamingDequeueConfidenceGate.cs` (the deadline/progress-callback/liveness design) are
confirmed present on disk in this worktree, with a comprehensive 21-test suite already in place;
this child's remaining work on that file is two narrow, pre-existing gaps (`quantity <= 0`
early-return, constructor null-guards) that predate and are unrelated to #424. `QfcQueue.cs` also
exceeds the repository's 500-line file limit (610 lines) and needs a partial-class split.

## Behavior

No observable behavior change to QuickFiler flows. Deliver per-file line coverage >= 80% for
every testable file in scope, verified with F1's per-file coverage harness, with numeric evidence
committed under this feature's `evidence/qa-gates/`. Split `QfcQueue.cs` into compliant partials.
Remove `[ExcludeFromCodeCoverage]` from any file in scope unless F1's ledger ratifies it as
irreducible. Research and planning proceed one production file at a time per issue #136's mandate:
a separate research artifact per file, a separate atomic-plan phase per file, and each individual
test case as its own atomic task.


## Inputs / Outputs

This is a coverage-only, no-behavior-change child: it introduces no new CLI flags, environment
variables, or runtime inputs/outputs. The relevant inputs/outputs are development-time and
evidentiary:

- Inputs (files, artifacts): the 11 production files in scope (see Overview); F1's per-file
  coverage measurement harness (`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and F1's per-file
  wrapper around it once merged — the script itself is confirmed on disk as a working, non-placeholder
  tool); F1's ratified exemption ledger at
  `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` (confirmed absent from disk
  at research time; authoritative once F1 merges); the on-disk, #424-complete state of
  `QfcStreamingDequeueConfidenceGate.cs`, treated as a fixed input, not a target for change.
- Outputs (artifacts): new/modified MSTest test files under `QuickFiler.Test/Controllers/`
  mirroring the production tree; the `QfcQueue.cs` -> `QfcQueue.cs` + `QfcQueue.TlpManipulation.cs`
  partial-class split; numeric per-file coverage evidence committed under this feature's
  `evidence/qa-gates/`; the recorded ledger-disposition recommendation for
  `QfcHighConfidencePreFilter.cs`'s `FolderScoringService` adapter (ratify as irreducible) and for
  `QfcScanProgressBandMapper.cs` (no open exemption to record).
- Config keys and defaults: none introduced. No new configuration surface.
- Versioning or backward-compatibility constraints: none. This is an internal test/coverage change;
  every file in scope keeps its existing public/internal member signatures. The `QfcQueue.cs`
  partial split redistributes members across two files without changing the compiled public
  surface of the `QfcQueue` type.

## API / CLI Surface

Not applicable. This feature makes no change to any public API, CLI, or user-facing surface.
Every class in scope retains its existing public/internal member signatures; the only structural
change is the `QfcQueue` partial-class split, which is source-organizational only. The nearest
thing to a "CLI" this feature touches is F1's coverage-harness script
(`scripts/vscode/Invoke-MSTestWithCoverage.ps1`), which this child invokes to produce evidence and
does not modify.

## Data & State

Not applicable. This feature introduces no new data model, persistence, caching, or migration
concern:

- Data transformations and invariants: none changed. The admission-never-scores invariant in
  `QfcRemainingQueueAdmission.cs` (issue #233/#424 contract) and the deadline/progress-callback
  invariants in `QfcStreamingDequeueConfidenceGate.cs` are preserved exactly as delivered; this
  child adds tests around them, not changes to them.
- Caching or persistence details: none. No file in scope reads or writes durable state.
- Migration or backfill requirements: none. The only structural change is the `QfcQueue.cs`
  partial-class split, which is a source-file reorganization, not a runtime data or schema change.

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
- **Coverage-floor policy precedence (documented choice, not a silent resolution).** Two repository
  documents state different coverage floors: `CLAUDE.md` § UT2 ratifies an 80% per-file/repo-wide
  floor with a named COM/VSTO/WinForms testable-denominator exemption, naming QuickFiler explicitly;
  `.claude/rules/general-unit-test.md` separately states a uniform 85%/75% line/branch floor tied to
  a `quality-tiers.yml` tier system that does not exist in this repository. Issue #431, issue #136,
  and this epic's own "Policy reconciliation" section all use the 80% figure. This spec treats
  `CLAUDE.md`'s ratified 80% floor, as reconciled by the epic, as the operative target for this
  child's acceptance criteria, and records that choice here explicitly rather than resolving the
  conflict silently. If a future audit needs the 85%/75% figures instead, that is a repository-wide
  policy decision outside this child's scope, not something F2 should unilaterally apply.

## Implementation Strategy

- **Implementation scope (what changes, not sequencing).** For each of the 11 files, close only the
  specific coverage gap the research identifies for that file — no file in this scope needs coverage
  built "from zero"; substantial partial coverage already exists everywhere except the two
  interface-only files. Add the `_itemViewerFactory` delegate seam and reuse the existing
  uninitialized-`QfcHomeController` + reflection pattern in `QfcQueue.cs`; reuse `EmailFiler`'s
  existing `virtual SortAsync()` override as the test seam for `FilerQueue.ConsumeAsync`. Make no
  seam changes to `QfcRemainingQueueAdmission.cs`, `QfcStreamingDequeueConfidenceGate.cs`,
  `BreadcrumbOutboundQueue.cs`, `EmailSorter.cs`, or `QfcItemGroup.cs` — every collaborator in those
  files is already an interface, an injected delegate, or `TimeProvider`. Make no test-authoring
  change to `QfcScanProgressBandMapper.cs` beyond a defensive re-verification against F1's harness.
  Record `QfcHighConfidencePreFilter.cs`'s `FolderScoringService` exemption as a ledger-ratification
  recommendation, not a removal. Leave `QfcHighConfidencePreFilter`/`QfcPreScoredItem`/
  `IFolderScoringService`'s own small gap (struct null-coalescing, out-of-order-completion ordering
  guarantee) as ordinary test-writing.
- **New classes/functions/commands to add or update.** `QfcQueue.TlpManipulation.cs` — a new
  partial-class file that moves the existing "Tlp Manipulation" region (`TlpTemplate`,
  `ActivateTlpTemplate`, `TlpStates`, `AddAsync`, `AddViewerToTlp`, `AdjustTlp`,
  `LoadControllersViewersAsync`, `ChangeIterationSize`, `RenumberGroups`, `GrowEntry`) verbatim out of
  `QfcQueue.cs`, leaving the retained `QfcQueue.cs` with the primary constructor, "Queue Functions",
  "INotify", and "Helper Methods" regions; both resulting files land at roughly 280-350 lines, under
  the 500-line limit. A new private `_itemViewerFactory` delegate field on `QfcQueue` (default bound
  to `ItemViewerQueue.Dequeue`, the existing method group). No other new production
  classes/functions. Test-side additions follow each per-file research artifact's "Candidate test
  cases" table (for example: `FilerQueueTests.cs` additions for `Enqueue`/`ConsumeAsync`; a new
  direct `BreadcrumbOutboundQueueTests.cs`; `QfcQueueCoverageExpansionTests.cs` additions for
  `RemoveItem`/`EnqueueAsync`/`ChangeIterationSize`/`AddAsync`/`AddViewerToTlp`/
  `LoadControllersViewersAsync`; direct struct-level tests for `QfcPreScoredItem`; guard-clause tests
  for `QfcRemainingQueueAdmission`'s constructor and `QfcStreamingDequeueConfidenceGate`'s
  `quantity <= 0` path; `EmailSorterTests.cs` additions for the `-1` fallback branch and the
  `Options` setter; direct isolated tests for `QfcItemGroup`'s parameterless constructor and
  `ItemViewer` property).
- **Dependency changes (new/removed packages) and rationale.** None. MSTest, Moq, FluentAssertions,
  and `Microsoft.Extensions.Time.Testing`'s `FakeTimeProvider` are already referenced by
  `QuickFiler.Test` and are sufficient for every gap identified in research.
- **Logging/telemetry additions and locations.** None. No production logging changes. Tests may
  assert that existing `log4net` call sites are reached (for example `FilerQueue.ConsumeAsync`'s
  catch-and-log branch) without adding new log statements or log levels.
- **Rollout plan (feature flags, staged deploys, fallback path).** Not applicable. This is a
  test/coverage-only change with no runtime behavior gate; it ships as a normal merge to the epic
  integration branch once its own toolchain pass and per-file coverage evidence are green. The
  `IQfcQueue1.cs` dead-code disposition (delete the orphaned interface and its `.csproj` entry, versus
  leave it in place and record it on F1's ledger as an interface-only module) is an explicit decision
  point the atomic-planner must resolve; this spec does not resolve it and treats it as a plan-level
  choice consistent with the "no behavior change" constraint either way.

## Acceptance Criteria

- [ ] Every testable file in this child's 11-file scope reaches >= 80% line coverage (per the
      `CLAUDE.md`-ratified floor, as reconciled by the epic and documented in this feature's
      `spec.md` Constraints & Risks section), verified with F1's per-file coverage harness, recorded
      as numeric evidence under
      `evidence/qa-gates/`; `IQfcQueue.cs` and `IQfcQueue1.cs` are interface-only and excluded from
      line-coverage measurement per the repository's own coverage-exclusion clarification.
- [ ] `QfcQueue.cs` (610 lines) is split into `QfcQueue.cs` (primary constructor, Queue Functions,
      INotify, Helper Methods) and `QfcQueue.TlpManipulation.cs` (the Tlp Manipulation region) so
      neither resulting file exceeds 500 lines, with no behavior change.
- [ ] `QfcHighConfidencePreFilter.cs`'s `FolderScoringService` adapter exemption is recorded for F1's
      ledger as a recommendation to ratify it as irreducible (COM-bound classifier scoring behind an
      existing `IFolderScoringService` seam), and `QfcScanProgressBandMapper.cs` — which carries no
      `[ExcludeFromCodeCoverage]` attribute despite the epic's stale `[X]` marker — is recorded as
      requiring no ledger entry; both dispositions are treated as this child's best-effort
      classification, superseded by F1's actual ledger at execution time.
- [ ] New/modified tests use MSTest, Moq, and FluentAssertions; are deterministic and isolated; use
      no temporary files, external services, or live/shown forms; use injected `TimeProvider` or
      delegate seams (never `Thread.Sleep`/`Task.Delay`/real waits) wherever timing is involved; and
      introduce no new RNG usage.
- [ ] Coverage per file spans positive path, invalid-input, boundary, and error-handling behavior,
      per each file's research-identified gap (for example: `QfcQueue.cs`'s `EnqueueAsync`,
      `ChangeIterationSize`, `AddAsync`, and `LoadControllersViewersAsync`; `FilerQueue.cs`'s
      `Enqueue`/`ConsumeAsync` drain, error-log, and guard-reset behavior;
      `QfcStreamingDequeueConfidenceGate.cs`'s `quantity <= 0` early return and constructor
      null-guards; `EmailSorter.cs`'s `GetSortKey` `-1` fallback branch; `BreadcrumbOutboundQueue.cs`'s
      direct post-vs-buffer branches; `QfcItemGroup.cs`'s parameterless constructor and `ItemViewer`
      property).
- [ ] The full C# toolchain (csharpier, analyzer build, nullable build, coverage-enabled vstest)
      passes.
- [ ] No behavior change to observable QuickFiler flows, including no re-testing or re-implementation
      of issue #424's already-delivered deadline/progress-callback/liveness design in
      `QfcStreamingDequeueConfidenceGate.cs`; F1's ledger remains the execution-time authority for any
      exemption disposition this child records as a recommendation only.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (format → lint → type-check → test)

## Seeded Test Conditions (from potential)
- [ ] Unit coverage areas: queue admission ordering, confidence-gate scan/deadline behavior,
- [ ] high-confidence pre-filter thresholds, scan-progress band mapping, breadcrumb outbound
- [ ] queue, email sorting, item grouping.
- [ ] Integration scenarios: none (host-neutral, COM-independent logic; no external services).
- [ ] CLI/API examples: n/a.

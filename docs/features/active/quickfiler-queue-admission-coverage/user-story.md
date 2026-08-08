# `quickfiler-queue-admission-coverage` — User Story

- Issue: #431
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-08

## Story Statement

- As an autonomous coding agent (or human engineer) maintaining QuickFiler's queue and admission
  logic, I want every testable file in F2's 11-file scope to have deterministic MSTest coverage of
  its actual behavior gaps — including `QfcQueue.cs` split into 500-line-compliant partials and the
  `[ExcludeFromCodeCoverage]` disposition on `QfcHighConfidencePreFilter.cs`/
  `QfcScanProgressBandMapper.cs` settled against F1's ledger — so that a future change to queue
  admission ordering, confidence-gate scanning, or email sorting is caught by a failing test before
  it reaches a live mailbox, rather than being discovered only after it ships.
- As a reviewer verifying this child closes issue #431 against issue #136's per-file mandate, I want
  numeric per-file coverage evidence committed under `evidence/qa-gates/`, produced by F1's per-file
  coverage harness, so that I can confirm the 80% floor is met for each of the nine testable
  production files in scope without re-running ad hoc coverage collection myself.

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


## Personas & Scenarios

- Persona: the engineering team and future autonomous coding agents that maintain QuickFiler, per
  the epic's stated business outcome ("makes the project safe for autonomous agentic maintenance").
  - Who they are: whoever next touches queue admission, confidence-gate scanning, or email-sorting
    logic in QuickFiler — today a human engineer, going forward increasingly an autonomous agent
    operating without a human reviewing every line before merge.
  - What they care about: being able to change this code with confidence that a regression will be
    caught by the test suite, not by a user's misfiled email or a stalled queue in production.
  - Their constraints: no live Outlook process, no shown UI, no external services available during
    automated test/CI runs; changes must not alter observable QuickFiler behavior.
  - Their goals and frustrations: today, several code paths in this file set (for example
    `QfcQueue.EnqueueAsync`'s exception-swallow branch, or `FilerQueue.ConsumeAsync`'s drain loop)
    have no test at all, so a mistake there is invisible until it manifests as a user-facing defect.
  - Their context and motivations: this work sits inside a larger epic (#136) whose premise is that
    per-file coverage, not just aggregate coverage, is what makes autonomous maintenance of this
    121-file surface safe.
- Scenario: a regression in a queue/admission code path escapes today, versus is caught after this
  child ships.
  - Who is acting: an engineer or agent modifying `QfcQueue.ChangeIterationSize` (for example, to
    change how it grows the visible row count) some months from now.
  - What triggered the action: a bug report or feature request touching the iteration-size behavior.
  - What steps do they take today: they make the change, run the existing test suite, see it pass
    (because `ChangeIterationSize` has zero existing test coverage per this child's research), and
    merge — the regression (for example, an off-by-one in row growth, or a failure to discard the
    duplicate top element) ships silently.
  - What steps do they take after this child ships: the same change now runs against tests covering
    `ChangeIterationSize`'s growth/shrink paths and its boundary condition (zero items returned from
    the datamodel dequeue); a regression fails a specific, named test before the change ever reaches
    review or merge.
  - What outcome do they expect: the test suite, not production usage, is the first place a queue/
    admission regression surfaces.


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

## Non-Goals

- No behavior change to any observable QuickFiler flow. This child is coverage-only: seam
  extraction (for example, `QfcQueue.cs`'s `_itemViewerFactory` delegate) is designed to be
  behavior-preserving, not a functional change.
- No coverage work on any file outside F2's 11-file list. In particular,
  `QfcDatamodel.QueueProcessing.cs` (F5's file) is discussed in research only to establish that the
  shared issue #424 surface is settled; it is not touched by this child.
- No resolution of the F1 exemption ledger itself. This child records its own best-effort
  disposition recommendations (ratify `FolderScoringService` as irreducible; no ledger entry needed
  for `QfcScanProgressBandMapper.cs`) for F1's ledger to ratify, override, or supersede at execution
  time — it does not itself constitute the ledger.
- No coordination changes to issue #424's already-delivered code. `QfcStreamingDequeueConfidenceGate.cs`'s
  deadline/progress-callback/liveness design, confirmed present on disk with its own 21-test suite,
  is treated as a fixed, unmodified input; this child adds only the two narrow, pre-existing gaps
  research identifies (the `quantity <= 0` guard and constructor null-guards), not any change to
  #424's delivered behavior or tests.

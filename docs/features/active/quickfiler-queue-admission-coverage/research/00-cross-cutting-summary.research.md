# Cross-Cutting Summary — F2 `quickfiler-queue-admission-coverage` (Issue #431)

- Parent epic: #136 (`quickfiler-per-file-coverage`)
- Child feature: #431 F2, wave 1
- Companion artifacts: one research file per production file in scope, at
  `docs/features/active/quickfiler-queue-admission-coverage/research/<file-basename>.research.md`.
- Evidence basis: direct reads of all 11 production files under `QuickFiler/Controllers/`, their
  corresponding test files under `QuickFiler.Test/Controllers/`, the epic document, issue #424's
  feature folder (`issue.md`, `spec.md`, `feature-audit.2026-08-06T23-40.md`), and
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.

## F1 dependency

F1 (`quickfiler-coverage-denominator-and-exemption-ledger`, wave 0) is responsible for two artifacts
this child consumes:

1. **The per-file coverage measurement harness.** Confirmed on disk:
   `scripts/vscode/Invoke-MSTestWithCoverage.ps1` already exists and is a working script, not a
   placeholder. It: resolves `vstest.console.exe` via `vswhere.exe`, discovers every `*.Test.dll` under
   a search root filtered to the requested build configuration, invokes `dotnet-coverage collect` with
   Cobertura output wrapping that vstest invocation (using a derived, output-adjacent copy of the
   repo's `coverage.config` that adds a `.*\.Test\.dll$` module-exclusion so test assemblies are not
   self-instrumented), and finally post-processes the resulting Cobertura XML to rewrite absolute paths
   to workspace-relative native-separator paths and strip non-solution `<package>` elements for
   Koverage-viewer compatibility. This script produces an assembly-wide Cobertura report; F1 is expected
   to layer a *per-file* extraction/report on top of this script's output (the script itself does not
   currently slice by file), which is why F1, not F2, owns the per-file harness deliverable. F2's plan
   should call this script (or F1's per-file wrapper around it, once merged) to produce the numeric
   evidence this child's acceptance criteria require, rather than inventing a second coverage-collection
   mechanism.
2. **The ratified exemption ledger** at
   `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. **Confirmed: this path does
   not exist on disk in the current worktree** (verified by glob). F1 is being prepared concurrently
   with this child's research, per the epic's wave-0/wave-1 dependency structure.

**Consequence for F2's plan:** the plan must cite the ledger as the *authoritative* source for exemption
disposition **at execution time**, after F1 has merged to the integration branch. This research
documents F2's own best-effort classification now, for planning purposes only, and that classification
may be superseded by F1's actual ledger entries:

- `QfcHighConfidencePreFilter.cs` — the exemption is on the inner `FolderScoringService` adapter only
  (not the file's primary testable surface); F2's best-effort recommendation is **ratify as
  irreducible** (see the per-file research artifact for the full irreducible-remainder analysis).
- `QfcScanProgressBandMapper.cs` — carries **no** `[ExcludeFromCodeCoverage]` attribute on the current
  worktree at all; the epic's file-level `[X]` marker for this file is stale. F2's best-effort
  recommendation is that this file requires **no** ledger entry as an open exemption — it is already
  fully covered.

Both findings should be flagged to F1 (or re-verified against F1's ledger once it exists) rather than
assumed correct without cross-check at execution time, per the instruction that follows F1's authority.

## The #424 conflict-risk disposition (definitive)

**The current on-disk `QfcStreamingDequeueConfidenceGate.cs` and `QfcDatamodel.QueueProcessing.cs`
already reflect issue #424's changes in full.** This is stated definitively, not provisionally, based
on direct reads of both files in this worktree:

- `QfcStreamingDequeueConfidenceGate.cs` (171 lines, matching the epic's line-count table) declares
  `internal static readonly TimeSpan DefaultFirstBatchDeadline = TimeSpan.FromSeconds(12);`, the
  `firstBatchDeadline`/`progressCallback` constructor parameters, the deadline-exit check inside the
  `DequeueAsync` loop, and the `_progressCallback?.Invoke(scanned, accepted.Count, quantity)` call — all
  described in #424's `spec.md` "Proposed Fix" section as the target design, and all independently
  re-verified as delivered by #424's `feature-audit.2026-08-06T23-40.md` (recommendation: "go — ready
  for PR").
- `QfcDatamodel.QueueProcessing.cs` (177 lines, matching the epic's line-count table) declares the
  `volatile bool _remainingLoadActive` producer-liveness flag (with the exact doc-comment language from
  #424's spec), the four-argument `DequeueNextItemGroupAsync` overload, and `WaitForQueue` consuming the
  flag — again matching #424's delivered design exactly.

**What this means for F2's coverage plan:** the #424 test suite
(`QfcStreamingDequeueConfidenceGateTests.cs` + `.Part2.cs` + `.Part3.cs`, 21 tests total) is not
"in-flight, might not be present" — it is present, comprehensive, and already reviewed. F2's plan must
**not** re-test #424's already-covered surface (the deadline lifecycle, the progress-callback contract,
the honest liveness signal). The per-file research for `QfcStreamingDequeueConfidenceGate.cs` in this
child identifies exactly two genuine, narrow gaps left after #424 (the `quantity <= 0` early return and
the constructor's null-guards for `tryTakeNext`/`scoreLoader`) — both pre-existing gaps that #424's work
had no reason to touch, not regressions or omissions from #424 itself. `QfcDatamodel.QueueProcessing.cs`
is **out of F2's scope** (it belongs to F5, `quickfiler-datamodel-coverage`, per the epic's file
assignment table) and is discussed here only to establish that the shared #424 surface is settled, not
because F2's plan should touch it.

Whether issue #424's pull request has been formally merged via GitHub at the time of this research is
a separate, weaker question than whether its code changes are present on disk; this research answers
the on-disk question definitively (yes) and treats that as the operative fact for planning, consistent
with the framing that this worktree branched from the epic integration branch off current `main`.

## Per-file table

| File | Lines (verified) | Dedicated test file(s) | `[ExcludeFromCodeCoverage]` | Seam work required | Partial-split required |
|---|---|---|---|---|---|
| `QfcQueue.cs` | 610 | `QfcQueueTests.cs`, `QfcQueuePurePathsTests.cs`, `QfcQueueCoverageExpansionTests.cs` (partial coverage; `EnqueueAsync`/`ChangeIterationSize`/`AddAsync`/`LoadControllersViewersAsync` uncovered) | N | Y (item-viewer-factory delegate; dispatcher via existing test-support helper; uninitialized-`QfcHomeController` reflection technique) | **Y** — split into `QfcQueue.cs` + `QfcQueue.TlpManipulation.cs` |
| `FilerQueue.cs` | 83 | `FilerQueueTests.cs` (deliberately excludes `Enqueue`/`ConsumeAsync`) | N | N (existing virtual `EmailFiler.SortAsync()` override suffices) | N |
| `QfcRemainingQueueAdmission.cs` | 48 | none dedicated; covered via `QfcDatamodelTests.cs`'s `CreateQueueAdmission` helper | N | N (already all injected delegates) | N |
| `QfcStreamingDequeueConfidenceGate.cs` | 171 | `QfcStreamingDequeueConfidenceGateTests.cs` + `.Part2.cs` + `.Part3.cs` (21 tests; #424-complete, two narrow pre-existing gaps remain) | N | N | N |
| `QfcHighConfidencePreFilter.cs` | 191 | `QfcHighConfidencePreFilterTests.cs` (9 tests; testable surface already near-complete) | Y — but only on the inner `FolderScoringService` adapter, not the file's primary type | N (recommend ratify existing exemption) | N |
| `QfcScanProgressBandMapper.cs` | 79 | `QfcScanProgressBandMapperTests.cs` (12 tests; 100% line/branch per #424 audit) | **N** (epic's `[X]` marker is stale) | N | N |
| `BreadcrumbOutboundQueue.cs` | 67 | none dedicated; covered indirectly via `BreadcrumbBridgeRouterQueueTests.cs` | N | N | N |
| `EmailSorter.cs` | 85 | `EmailSorterTests.cs` (6 tests; one branch — `return -1` fallback — uncovered) | N | N | N |
| `QfcItemGroup.cs` | 52 | none dedicated; covered as an incidental fixture in other files' tests | N | N | N |
| `IQfcQueue.cs` | 41 | none (interface-only; policy-exempt from line-coverage measurement) | N | N (already the seam) | N |
| `IQfcQueue1.cs` | 44 | none (interface-only; also **orphaned** — no production implementer) | N | N | N |

"Dedicated test file" means a file whose name/doc-comment identifies it as targeting that production
file; several files above are covered adequately, or exclusively, through another file's test fixtures.
Zero-dedicated-coverage files that nonetheless have adequate line coverage through fixture usage are not
the same as zero-coverage files — see below.

**Files with genuinely zero existing test exercise of their own logic (not counting interface-only
files, which are policy-exempt):** none. Every one of the 9 non-interface production files in this
child's scope has at least some existing test exercising it, whether via a dedicated test file or via
legitimate fixture reuse in another file's tests (`QfcItemGroup.cs` via `QfcQueueCoverageExpansionTests`;
`BreadcrumbOutboundQueue.cs` via `BreadcrumbBridgeRouterQueueTests`). The two interface files
(`IQfcQueue.cs`, `IQfcQueue1.cs`) have no test coverage and need none per the repository's own
interface-only coverage-exclusion clarification. This finding — **substantial partial coverage already
exists across all 11 files, consistent with the epic's own framing** — is itself the headline result of
this research: F2's remaining work is closing narrow, well-identified gaps and performing the
`QfcQueue.cs` partial split, not building coverage from zero anywhere in this file set.

## Automation Feasibility

Per the autonomous-execution mandate in `.claude/skills/orchestrate/SKILL.md`, this research states
plainly: **this work requires no human interaction.** None of the 11 files in scope requires a live
Outlook process to reach its coverage target:

- Every file's dependency on `Microsoft.Office.Interop.Outlook.*` types is either absent entirely
  (`FilerQueue.cs`, `QfcRemainingQueueAdmission.cs`, `QfcStreamingDequeueConfidenceGate.cs`,
  `QfcHighConfidencePreFilter.cs`'s testable surface, `QfcScanProgressBandMapper.cs`,
  `BreadcrumbOutboundQueue.cs`, `EmailSorter.cs`, `QfcItemGroup.cs`, `IQfcQueue.cs`, `IQfcQueue1.cs`), or
  confined to the already-mockable `MailItem` COM interface flowing through as a data parameter
  (`QfcQueue.cs` and most of the others), or isolated behind an already-existing injectable seam that
  is itself the intended irreducible remainder (`QfcHighConfidencePreFilter.cs`'s `FolderScoringService`
  adapter, which this research recommends ratifying as exempt rather than covering).
- `QfcQueue.cs` is the one file whose full coverage requires WinForms-control construction
  (`ItemViewer`/`TableLayoutPanel`) and WPF-dispatcher marshaling (`UiThread.Dispatcher`). Both are
  already-established, no-human-interaction patterns in this codebase: headless (never-shown)
  `ItemViewer` construction is confirmed safe by prior research (issue #227, the `ProgressPane`
  precedent), and a dedicated background WPF `Dispatcher` is already provisioned by
  `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`'s `EnsureUiThreadDispatcher`/
  `StartRunningDispatcher`/`ShutdownDispatcher` helpers. Neither requires showing a form, a popup, or any
  third-party UI, and neither requires a live Outlook process.
- No file in scope requires network access, a database, or any other external service.

No step in this child's execution — research, planning, test authoring, or verification — requires a
human to click through a UI, approve a live-Outlook manual test, or make a judgment call that cannot be
resolved from the source and test evidence already gathered in this research (with the single named
exception of the `IQfcQueue1.cs` dead-code disposition, which is a plan-level decision between two
policy-compliant options, not a human-interaction requirement).

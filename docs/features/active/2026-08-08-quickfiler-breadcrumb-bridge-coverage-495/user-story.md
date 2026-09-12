# `quickfiler-breadcrumb-bridge-coverage` — User Story

- Issue: #495
- Parent: epic #136 `quickfiler-per-file-coverage`, child F12
- Owner: drmoisan
- Status: Prepared
- Last Updated: 2026-08-08T02-45
- Work Mode: `full-feature` (`spec.md` + `user-story.md` are the authoritative AC sources)

## Story Statement

- As the **maintainer of QuickFiler**, I want the breadcrumb bridge, messenger, and lifecycle
  coordination cluster to meet both the 80% line and the 75% branch floor per file, so that a
  regression in the concurrency and ordering logic behind the folder selector is caught by the test
  suite rather than by a user filing mail to the wrong folder.
- As an **autonomous agent maintaining this repository**, I want the untaken guard, disposal, and
  out-of-order transition branches to be covered by contract-bearing tests, so that I can change this
  cluster without silently breaking an invariant that no test asserts.

## Problem / Why

Child F12 owns five production files totalling 2,183 physical lines. Every file clears the 80% line
floor, so on line coverage alone this child looks like a near-no-op.

It is not. `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` sits at **66.44% branch across 146
branch points with exactly 49 untaken outcomes** — the largest single branch gap in the epic —
against a 75% floor. Line and branch coverage are independent gates.

The gap is concentrated exactly where it matters least visibly and costs most: 30 of the 49 untaken
outcomes and 28 of the 30 uncovered lines sit in two types the brief never mentioned, and
`BreadcrumbPopupLifecycleOperations.CreateCollapsedCandidate` is **0% covered end to end** while
appearing in no brief, spec, or epic document.

This cluster carries the concurrency and ordering invariants for the folder selector. Preparation
research verified five real defects here that are out of scope to fix under the epic's
no-behavior-change NFR, now tracked as **#498** (an unvalidated segment index can crash the Outlook
host), **#499** (a stale selected-folder path can file mail to an unseen folder), **#500** (a WebView2
post executes under two nested locks), **#501** (a throwing surface starves later attachments while
the cache claims delivery), and **#502** (a superseded lease silently skips a suggestion upgrade).
Four of those five are in branches that no test currently reaches. That is the concrete cost of the
branch gap.

## Personas & Scenarios

**Persona — the repository maintainer.** Owns a legacy VSTO/WinForms add-in undergoing incremental
testability work, and is accountable for not shipping regressions to users who file mail all day.
Constraints: cannot afford behavior changes smuggled in under coverage work, and cannot manually
review 49 branch outcomes. Cares that coverage numbers mean something — that a covered branch was
covered by a test asserting a contract, not by a shape assertion written to move a percentage.

**Scenario.** A future change reorders disposal in the breadcrumb lifecycle coordinator. Before this
child, the double-invoke and post-disposal arms are untested, so the suite stays green and the defect
ships. After this child, the reordering trips a test whose name and assertion state the contract that
was broken, and the failure is diagnosable from the test name alone.

**Persona — an autonomous agent.** Executes atomic plans against this repository without human
review of each step. Needs the existing test suite to be the specification, because it has no other
way to know which orderings are load-bearing.

**Scenario.** The agent is asked to simplify `TryRunCurrent`. The suite pins the currency invariant
across a re-entrant mutation, so the simplification either preserves the invariant or fails loudly.

## Acceptance Criteria

- [ ] **US-1** The branch gate passes on every one of the five files, not just the line gate, and both
      figures are reported per file. A file that passes one and fails the other is reported as failing.
- [ ] **US-2** `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` moves from 66.44% branch to at
      least 75%, and the evidence states the achieved figure numerically rather than as a pass mark.
- [ ] **US-3** Each new test pins a stated behavioral contract. No test is written purely to move a
      coverage number, and no shape-assertion test is written for a declaration-only construct.
- [ ] **US-4** The maintainer can read the coverage evidence and see, per file, the before figure, the
      after figure, the command used, and an explicit statement that no emitted Cobertura `line-rate`
      or `branch-rate` attribute was relied upon.
- [ ] **US-5** No observable QuickFiler behavior changes. The five promoted defects (#498–#502) and
      open #440 remain unfixed, their current behavior is pinned rather than corrected, and each
      affected test names the issue in an in-code comment so a future fix knows to update it.
- [ ] **US-6** The six structurally unreachable outcomes are documented as excluded with proofs, so a
      later reviewer does not mistake them for an incomplete job or attempt them again.
- [ ] **US-7** No sibling child is broken. The frozen contracts consumed by F9 (#452), F13 (#455), and
      F14 (#456) keep their signatures, and F14's request to retain live `ItemViewer` construction in
      the breadcrumb harnesses is honoured.
- [ ] **US-8** Coverage of `BreadcrumbPopupLifecycleOperations` and `BreadcrumbNavigationSubscription`
      is owned by F12's own tests rather than inherited from an F13-owned test file that F13's plan
      rewrites.

## Non-Goals

- Fixing any defect promoted from this child's research (#498, #499, #500, #501, #502) or open #440.
  All are observable behavior changes and are excluded by the epic's no-behavior-change NFR.
- Editing any production `.cs` file. Research established that no production edit is required on any
  of the five files.
- Editing any F13-owned (#455) or F14-owned (#456) production or test file, or anything under
  `UtilitiesCS/`.
- Removing or adding any `[ExcludeFromCodeCoverage]` attribute. None of the five files carries one,
  so this child has no exemption-disposition work.
- Building the WinForms message-pump seam tracked by #230, or introducing any STA apparatus. No file
  in this cluster requires one.
- Reaching 100% on any file. The floors are 80% line and 75% branch; the projected figures exceed
  them, but exceeding them is not itself a requirement.

# `quickfiler-per-file-coverage-capstone` — User Story

- Issue: #497
- Owner: drmoisan
- Status: Prepared (preparation mode)
- Last Updated: 2026-08-08T01-10
- Epic: `quickfiler-per-file-coverage`, parent epic issue #136
- Epic child: F16 (capstone), wave 2

## Story Statement

- As the **project maintainer**, I want a single committed report that proves every compiled
  QuickFiler file either meets its coverage gate or sits on a ledger row with a named ratified
  ground, so that I can close issue #136 on evidence rather than on the assertion that fifteen
  children each said they were done.
- As the **epic orchestrator**, I want the capstone to fail closed and name the owning child for any
  gap, so that a shortfall is routed back to the child that owns it instead of being silently
  absorbed or exempted away at the end of the epic.
- As a **future agent maintaining QuickFiler autonomously**, I want the exemption ledger to be
  trustworthy — every exemption on a ground that textually exists, and no exemption on a testable
  seam — so that a 0% file tells me the code is untested rather than merely invisible.

## Problem / Why

Fifteen sibling children each measure only their own assignment. That is the correct decomposition
for parallel execution, and it is exactly why the epic cannot prove its own outcome. Five failure
modes survive fifteen green merges:

1. **A file assigned to nobody.** The ledger is authored against the 121 files compiled at planning
   time, but F2, F3, F7, F9, and F11 all add `<Compile Include>` entries mid-wave. A file created
   after the ledger exists has no row unless its creating child appended one.
2. **An exemption on a ground that does not exist.** The epic itself found that `CLAUDE.md` §UT2's
   three grounds textually cover none of the WebView2 files, and had to ratify a fourth ground to
   legitimise exactly one of the three attributes it found there.
3. **A line-pass, branch-fail file.** Twelve files in the planning-time baseline clear 80% line and
   miss 75% branch. A child reading only the line column concludes it is finished.
4. **A phantom repository-wide improvement.** Comparing a raw artifact against a post-processed one
   produced a fifteen-point improvement nobody earned, and the epic's own explanation of why is
   itself wrong.
5. **A silently inherited exemption.** An attribute on a partial *type* suppresses every partial.
   One attribute on `ItemViewer.cs` removes seven files including a 6,224-line designer. Absence from
   a coverage report is not coverage.

None of these is detectable from inside the child that causes it. All are detectable from outside,
once, at the end — which is what this capstone is.

## Personas & Scenarios

- **Persona: the project maintainer (drmoisan).**
  - Cares about: whether issue #136 can actually be closed; whether the coverage numbers mean what
    they say; whether the exemption ledger is a genuine irreducible remainder or a convenience list.
  - Constraints: cannot personally re-derive per-file coverage for 121+ files; has already
    adjudicated one exemption family (issue #227, closed) and deliberately deferred nine attributes
    (issue #230, open) and does not want either decision re-litigated.
  - Goal: close #136 with numeric evidence and a defensible ledger.
  - Frustration: an epic that reports success by asserting it.

- **Scenario: closing issue #136.**
  - *Who acts:* `epic-orchestrator` runs F16 after all fifteen siblings have merged to the
    integration branch.
  - *Trigger:* wave 1 fan-in completes.
  - *Steps:* F16 confirms F1's ledger and harness exist on the branch; inspects the harness for the
    descendant-axis and emitted-rate defects before trusting a single figure; re-derives the compiled
    set from the csproj at that moment; captures repository-wide coverage; runs the full C#
    toolchain; captures repository-wide coverage again with the identical command; recomputes both
    figures from class-level `<line>` elements; reconciles every compiled file against the ledger;
    re-runs the attribute census per type; audits the fifteen siblings for per-file research
    artifacts and atomic test steps; verifies the defect-promotion trail against the live issue
    index; then maps each of #136's eight criteria to the artifact that closes it.
  - *Obstacles and decisions:* F1's ledger may not carry a fourth-ground enum value for
    `WebView2CoreInitializer`. A file may be absent from the report for three different reasons and
    the ledger must name which. A sibling may have left an attribute on a testable seam. F13 appears
    to have a four-file AC2 shortfall on its interface files.
  - *Expected outcome:* either a clean reconciliation that closes #136, or a precise list of Blocking
    findings each naming the child that owns it — and in the second case F16 fixes nothing.

## Acceptance Criteria

The authoritative, fully-worded criteria are in `spec.md` § Acceptance Criteria (AC1-AC12). They are
restated here in user-facing form; the two lists are checked off together.

- [ ] **AC1** — Every compiled file, derived from the csproj at execution time, appears in a committed
      reconciliation report with its bucket, its line percentage, and its branch percentage; a
      compiled file with no ledger row fails the report closed.
- [ ] **AC2** — Both gates are reported for every file: 80% line and 75% branch for `testable`, 90%
      line for epic-created files, with zero-branch files reported N/A rather than 0%.
- [ ] **AC3** — Every exemption names exactly one of the four ratified grounds and carries that
      ground's required evidence, keyed on the type and listing every file the type is declared in.
- [ ] **AC4** — Third-bucket files are reported N/A with zero-coverable-lines evidence and carry no
      exemption attribute; the instrumentation positive control is re-run rather than inherited;
      `measured-not-gated` rows are confirmed to be genuinely generated code.
- [ ] **AC5** — The re-derived attribute census shows nothing left on a testable seam, with #227's
      ratified attributes recorded by provenance and #230's nine deferrals explicitly not treated as
      a gap.
- [ ] **AC6** — Repository-wide coverage is shown retained or improved as a self-consistent
      same-session before/after pair, recomputed rather than transcribed, with no imported figure.
- [ ] **AC7** — The full C# toolchain passes in a single uninterrupted pass in the mandated order,
      with each stage's command, exit code, and output summary recorded.
- [ ] **AC8** — Each of issue #136's eight criteria is marked closed with a citation to the evidence
      that closes it, mirrored locally under `evidence/issue-updates/`.
- [ ] **AC9** — Every gate failure is a Blocking finding naming the owning child; F16 fixes no
      sibling's coverage and grants no exemption to close a gap.
- [ ] **AC10** — F1's harness is inspected and confirmed free of the descendant-axis and
      emitted-rate defects before any figure it emits is used.
- [ ] **AC11** — The deferred-defect promotion trail is verified asymmetrically: F4's production
      defects must have open issues, and its two in-scope test-policy violations must be fixed in
      code rather than deferred to an issue.
- [ ] **AC12** — F6's dead-region deletion and the epic manifest's resolved issue numbers are
      confirmed landed, each unmet condition reported against its owner.

## Non-Goals

- **Producing coverage.** F16 writes few if any tests and owns no production files. Raising a
  sibling's coverage is that sibling's work.
- **Granting exemptions.** F16 may reject an exemption row; it may never create one to close a gap.
  This is the precise failure mode the epic's policy reconciliation exists to prevent.
- **Fixing the harness defects.** #441 and #478 remain open in
  `Invoke-MSTestWithCoverage.Helpers.ps1`. F16 records that they are live and routes around them by
  recomputing; it does not close them.
- **Re-litigating maintainer decisions.** Issue #227's ratified attributes stand. Issue #230's nine
  deferred attributes are tracked, deliberate, and explicitly not a merge condition. No task builds
  the #230 message-pump seam.
- **Amending policy.** The csharpier subcommand deviation and the conflicting repository-wide
  thresholds (#494) are recorded in evidence. `CLAUDE.md` and `.claude/rules/` are not edited.
- **Repairing the epic manifest.** `epic.md` is not a per-child owned file; F16 reports the stale
  placeholder issue numbers and verifies the repair.
- **Promoting a sibling's defects on its behalf.** F4's eight follow-ups are F4's epic-assigned
  obligation; F16 verifies and reports absence as Blocking.

# quickfiler-per-file-coverage-capstone (Issue #497)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497/ (Issue #497)

- Issue: #497
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/497
- Last Updated: 2026-08-08
- Work Mode: full-feature
- Epic: `quickfiler-per-file-coverage` (parent epic issue #136)
- Epic child: F16 (capstone), wave 2
- Integration branch: `epic/quickfiler-per-file-coverage-integration`

## Problem / Why

Epic #136 brings every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj` to at least
80% line coverage, or onto an explicitly ratified exemption ledger, across fifteen sibling children.
Nothing in that decomposition proves the result. Each child measures only its own assignment, so a
compiled file assigned to no child, a file whose exemption was granted on a ground that does not
exist, a file passing the line floor while failing the branch floor, or a repository-wide figure
compared against an incompatible baseline would all survive fourteen successful child merges
undetected.

This capstone is the verification gate. It re-derives the coverage denominator from the csproj at
execution time, reconciles every compiled file against the ledger, verifies every exemption against
a ratified ground, measures repository-wide coverage as a self-consistent before/after pair, runs
the full C# toolchain in final form, and closes each of issue #136's eight acceptance criteria with
numeric evidence.

## Proposed Behavior

The capstone produces a committed reconciliation report and the evidence set that closes issue #136.
It adds no new production files and writes few if any tests. Its verification is mechanical and
re-runnable, not a narrative assertion.

The capstone verifies six things:

1. **Denominator completeness.** Re-derive the compiled set from the `<Compile Include=...>` entries
   in `QuickFiler/QuickFiler.csproj` at execution time, never from a frozen file list. Fail if any
   compiled file lacks a ledger row.
2. **Per-file gate compliance.** `>= 80%` line and `>= 75%` branch for every `testable` file;
   `>= 90%` line for files this epic created; `ratified-exempt` only against one of the four grounds;
   `interface-only / not-measured` reported N/A, never as 0% and never as a failure.
3. **No `[ExcludeFromCodeCoverage]` on a testable seam.** Verify every disposition in F1's census
   landed, including files suppressed by partial-type propagation rather than by their own attribute.
4. **Repository-wide coverage retained or improved**, measured on this branch as a self-consistent
   before/after pair using an identical command and identical post-processing.
5. **Full C# toolchain green in final form** — formatting, analyzer build, nullable build, and
   coverage-enabled test execution.
6. **Issue #136's eight acceptance criteria** each closed with a citation to numeric evidence.

## Acceptance Criteria

- [ ] AC1 — A committed reconciliation report enumerates every file in the execution-time
      `<Compile Include=...>` set of `QuickFiler/QuickFiler.csproj` with its ledger bucket, its
      numeric line percentage, and its numeric branch percentage. The report fails closed if any
      compiled file has no ledger row. The re-derived file count is recorded and compared against the
      121-file figure the epic recorded at planning time; a divergence is expected and is reported,
      not treated as an error.
- [ ] AC2 — Every `testable` file is shown at `>= 80%` line and `>= 75%` branch, and every file this
      epic created is shown at `>= 90%` line. Both figures are reported for every file; a line figure
      alone is never accepted as proof of compliance.
- [ ] AC3 — Every `ratified-exempt` row carries an explicit exemption ground drawn from the four
      ratified grounds, with the evidence that ground requires. A row citing no ground, or a ground
      the file does not satisfy, is a Blocking finding.
- [ ] AC4 — Every `interface-only / not-measured` row is reported N/A with the zero-coverable-lines
      evidence that places it in that bucket, and carries no `[ExcludeFromCodeCoverage]` attribute.
- [ ] AC5 — A re-derived `[ExcludeFromCodeCoverage]` census over `QuickFiler/` shows no attribute
      remaining on a testable seam, distinguishing type-level from member-level usages and files
      suppressed by partial-type propagation from files carrying their own attribute.
- [ ] AC6 — Repository-wide line coverage is shown retained or improved as a self-consistent
      before/after pair captured on this branch with an identical command and identical
      post-processing, with both figures and the command recorded in the evidence artifact. No
      repository-wide figure is imported from another branch, tool, or artifact.
- [ ] AC7 — The full C# toolchain passes in final form and each stage's exit code and output summary
      is recorded as evidence: `dotnet tool run csharpier format .`, the analyzer build, the nullable
      build, and coverage-enabled test execution.
- [ ] AC8 — Each of issue #136's eight acceptance criteria is explicitly marked closed with a
      citation to the specific evidence artifact that closes it, and issue #136's checkboxes are
      checked off accordingly.
- [ ] AC9 — Any file failing a gate is reported as a Blocking finding naming the owning child. The
      capstone does not fix a sibling's coverage and does not grant an exemption to close a gap.
- [ ] AC10 — The coverage harness this capstone relies on is verified to address both open harness
      defects (#441 descendant-axis double-count and #478 union/primary-method blend) before any
      figure it emits is trusted. A harness that addresses neither or only one is a Blocking finding.
- [ ] AC11 — The deferred-defect promotion trail is verified: every latent defect the epic records as
      requiring MCP promotion has a real GitHub issue, and any that does not is promoted via the MCP
      promotion lifecycle rather than left as feature-folder prose.

## Constraints & Risks

- **Verification, not remediation.** If a sibling's work is short, the capstone reports it and names
  the owning child. Absorbing a sibling's work, or granting a convenience exemption to close a gap,
  is the exact failure mode the epic's policy reconciliation exists to prevent.
- **No production files are owned.** The only writes are this feature folder, its evidence, and any
  capstone-specific verification tooling. Tooling requires its own tests at the mirrored `tests/`
  path per `.claude/rules/general-unit-test.md` § Test File Location.
- **Repository-wide figures must be measured, never imported.** A raw-versus-post-processed
  comparison produced a fifteen-point phantom improvement before it was caught.
- **Absence from a coverage report is not coverage.** An `[ExcludeFromCodeCoverage]` on a partial
  *type* suppresses every partial of that type, so a file can be missing from a report because it is
  suppressed rather than because it is clean.
- **Method-level exemptions do not suppress nested lambdas (#457).** A thin-forwarder adapter using
  method-level attributes keeps its closures in the denominator, permanently uncovered and silently
  capping the file.
- **Branch is a separate, frequently-binding gate.** Twelve files pass the line floor and fail the
  branch floor in the planning-time baseline.
- **The denominator is dynamic.** Siblings create production files mid-wave (F2, F3, F7, F9, F11 at
  minimum), so the denominator must be re-derived at execution time.
- **Two upstream conditions must be confirmed, not assumed.** F6's approved plan required a revision
  to delete the dead `#region Email Sorting To Rewrite` in `QfcExplorerController.cs`; F4's execution
  run was required to promote eight deferred defects via the MCP lifecycle. Both are capstone
  verification obligations.

## Test Conditions

- [ ] Denominator reconciliation against a csproj compile set parsed at execution time, including a
      case where a file present in the csproj has no ledger row.
- [ ] Per-file gate evaluation covering a passing file, a line-pass/branch-fail file, and a
      third-bucket file that must report N/A rather than 0%.
- [ ] Exemption-ground evaluation covering an accepted row under each of the four grounds and a
      rejected row citing no ground.
- [ ] `[ExcludeFromCodeCoverage]` census detection distinguishing a real attribute from a doc-comment
      mention, a type-level from a member-level usage, and propagation-suppressed partials.
- [ ] Repository-wide before/after capture on this branch with an identical command and identical
      post-processing.

## Source

Epic manifest: `docs/features/epics/quickfiler-per-file-coverage/epic.md` (child F16).
Parent epic issue: https://github.com/drmoisan/TaskMaster/issues/136

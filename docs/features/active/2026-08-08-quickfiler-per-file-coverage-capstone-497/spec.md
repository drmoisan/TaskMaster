# quickfiler-per-file-coverage-capstone — Spec

- **Issue:** #497
- **Parent:** epic `quickfiler-per-file-coverage`, parent epic issue #136
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08T01-10
- **Status:** Prepared (preparation mode; execution deferred to `epic-orchestrator`)
- **Version:** 1.0
- **Work Mode:** full-feature
- **Epic child:** F16 (capstone), wave 2
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`

## Overview

Epic #136 brings every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj` to at least
80% line coverage, or onto an explicitly ratified exemption ledger, across fifteen sibling children.
Nothing in that decomposition proves the result. Each child measures only its own assignment, so all
of the following survive fifteen successful child merges undetected:

- a compiled file assigned to no child, or created mid-wave after F1 authored its ledger;
- an exemption granted on a ground that does not textually exist;
- a file passing the 80% line floor while failing the independent 75% branch floor;
- a repository-wide figure compared against an incompatible baseline;
- an `[ExcludeFromCodeCoverage]` left on a testable seam, or inherited silently by partial-type
  propagation.

F16 is the verification gate that closes issue #136. It owns no production files, adds no coverage,
and remediates nothing. Its deliverable is evidence.

## Behavior

The capstone produces a committed reconciliation report and the evidence set that closes issue #136.
Its verification is mechanical and re-runnable, not a narrative assertion. It verifies six things:

1. **Denominator completeness.** Re-derive the compiled set from `<Compile Include=...>` in
   `QuickFiler/QuickFiler.csproj` at execution time. Fail if any compiled file lacks a ledger row.
2. **Per-file gate compliance.** `>= 80%` line and `>= 75%` branch for every `testable` file;
   `>= 90%` line for files this epic created; `ratified-exempt` only against one of the four grounds;
   `interface-only / not-measured` reported N/A; `measured-not-gated` only for genuinely generated
   files; a file whose `branches-valid` is 0 reports branch N/A, never 0%.
3. **No `[ExcludeFromCodeCoverage]` on a testable seam**, except where a prior maintainer
   ratification governs.
4. **Repository-wide coverage retained or improved**, as a self-consistent before/after pair.
5. **Full C# toolchain green in final form.**
6. **Issue #136's eight acceptance criteria** each closed with numeric evidence.

## Inputs / Outputs

**Inputs (all read at execution time, never inherited from a planning-time figure):**

- `QuickFiler/QuickFiler.csproj` — the authoritative, dynamic coverage denominator.
- `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.json` and `.md` — F1's ledger.
- `scripts/vscode/Get-PerFileCoverage.ps1` and `Get-PerFileCoverage.Helpers.ps1` — F1's harness.
- `QuickFiler/**/*.cs` source, for the attribute census and the third-bucket screen.
- `QuickFiler.Test/**/*.cs`, for the convention and determinism scans.
- The fifteen sibling feature folders, for the AC2/AC3 per-file artifact audits.
- The GitHub issue index, for the defect-trail audit.

**Outputs — all under `<FEATURE>/evidence/`, per `evidence-and-timestamp-conventions`:**

| Artifact | Path |
| --- | --- |
| Repository-wide before | `evidence/baseline/repo-coverage-before.<TS>.md` + `.cobertura.xml` |
| Repository-wide after + comparison | `evidence/qa-gates/repo-coverage-after.<TS>.md`, `repo-coverage-comparison.<TS>.md` |
| Per-file reconciliation | `evidence/qa-gates/per-file-coverage-reconciliation.<TS>.md` |
| Attribute census | `evidence/qa-gates/exclude-attribute-census.<TS>.md` |
| #457 lambda-trap scan | `evidence/qa-gates/ac457-lambda-residual-scan.<TS>.md` |
| AC2/AC3/AC4/AC5/AC6 audits | `evidence/qa-gates/ac{2,3,4,5,6}-*.<TS>.md` |
| Toolchain stages | `evidence/qa-gates/toolchain-{format,analyze,nullable,test}.<TS>.md` |
| Issue #136 closure map | `evidence/qa-gates/issue-136-ac-closure.<TS>.md` |
| Issue update mirror | `evidence/issue-updates/issue-136.<TS>.md` |

**Config:** per-file line floor 80.0, per-file branch floor 75.0, epic-created-file line target 90.0.
Repository-wide gate is *retain or improve*, not an absolute floor.

## API / CLI Surface

No public API change. The capstone may add at most one PowerShell reconciliation tool — an entry-point
script plus its dot-sourced `.Helpers.ps1` module, matching the repository's existing
`Invoke-MSTestWithCoverage.ps1` / `.Helpers.ps1` and F1's `Get-PerFileCoverage.ps1` / `.Helpers.ps1`
layout — within the `.claude/rules/powershell.md` budget of 2 production PowerShell files plus their
tests. Research
established that PowerShell is the only viable language (the repository has no Python CI job, no
Python toolchain wiring, and no `.claude/rules/python.md`), that the script belongs at
`scripts/vscode/<Name>.ps1`, and that its tests belong at `tests/scripts/vscode/<Name>.Tests.ps1`.

Verified command forms (deviations from `CLAUDE.md`, recorded not amended):

```
dotnet tool restore
dotnet tool run csharpier format .        # csharpier 1.2.6 requires a subcommand; bare `csharpier .` fails
msbuild TaskMaster.sln /t:Build   /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration 'Debug' -CoverageOutput '<evidence path>.cobertura.xml'
```

`/t:Rebuild` is required for the nullable gate because MSBuild's incremental up-to-date check does
not invalidate on a command-line property change alone (issue #492 tracks this). MSBuild and
`vstest.console.exe` are not on PATH on this host and resolve through `vswhere`; `dotnet-coverage`
must be on PATH as a global tool.

## Data & State

The capstone mutates no production state. It reads a Cobertura report and a JSON ledger and emits
Markdown evidence.

**The one computation the capstone owns outright** is the repository-wide line rate. Research
established that no correct producer exists in the repository: `Get-CoberturaCoverageSummary` carries
#441, `Merge-CoberturaClassesByFilename` carries #478, `temp-extract-coverage.ps1` is worse than
either, the JaCoCo feature-review hook is the wrong format, and F1's harness is package-scoped to
QuickFiler by design and cannot emit a repository-wide figure at all.

The recomputation recipe, applied identically to the before and after artifacts:

1. Group `<class>` elements by `filename` within each `<package>`.
2. Union **only** the class-level `./lines/line` children — never a `.//lines/line` descendant axis,
   which double-counts every line appearing in both the class rollup and a method block (#441).
3. Deduplicate by `@number`, taking `MAX(@hits)`; retain the richer `condition-coverage`.
4. Line rate = covered / total. Branch rate = `sum(covered) / sum(total)` parsed from
   `condition-coverage` with `\(([0-9]+)/([0-9]+)\)` across `@branch="True"` lines only — never the
   mean of per-line percentages.
5. `sum(total) == 0` reports `n/a`, never `0%`.

## Constraints & Risks

- **Verification, not remediation.** If a sibling's work is short, the capstone reports it and names
  the owning child. Absorbing a sibling's work, or granting a convenience exemption to close a gap,
  is the exact failure mode the epic's policy reconciliation exists to prevent.
- **Never read emitted `line-rate` / `branch-rate` attributes.** They are corrupt in both directions,
  so no correction factor exists. `FocusAndTheme.cs` emits a rate over 373 lines for a 326-line file;
  `MailActions.cs` emits `branch-rate="0.75"`, falsely passing the gate against a true 72.7%;
  `QfcHomeController.Iteration.cs` emits `0.8625` where the true class-level union is `80.36%` — on
  a file whose gate is 80%.
- **Absence from a report is not coverage**, and it is three-way ambiguous: an
  `[ExcludeFromCodeCoverage]` on the type *or on any partial of it*, zero coverable IL, or a
  type-level `[DebuggerNonUserCode]`. The ledger must name which. `Resources.Designer.cs` is absent
  for the third reason and has no ledger disposition today.
- **Method-level exemptions do not suppress nested lambdas (#457).** `BreadcrumbPopupUiOperations.cs`
  is already in this state with three exposed members and >= 15 lifted lambda lines.
- **Branch is frequently the binding gate** — twelve files pass line and fail branch in the baseline.
- **The denominator is dynamic.** F2, F3, F7, F9, and F11 all add `<Compile Include>` entries.
- **The compile set is not the file set.** 156 `.cs` files exist under `QuickFiler/` against 121
  compiled. A filesystem glob would falsely flag `Helper Classes/FormFocusListener.cs` and 20 orphan
  viewer files as unledgered. Derive from the csproj only.
- **Repository-wide figures must be measured, never imported**, and both artifacts must come from the
  same complete pipeline. `<sources>` presence is a one-glance raw-versus-post-processed
  discriminator.
- **Stale worktree builds.** The runner's test-assembly discovery has no `.claude` exclusion. Invoke
  it from inside the feature worktree, where `.claude/worktrees/` does not exist, rather than from the
  canonical repo root.

## Documented Deviations from the Delegation Brief

Recorded per the epic's instruction to plan against reality when research disproves the brief.

- **DEV-1 — `spec.md` and `user-story.md` were not salvaged content.** Both were unfilled scaffold
  templates. Only `issue.md` and the two research artifacts carried content. Authored in this run.
- **DEV-2 — F1's harness does not fix #441 or #478, and that is not a Blocking finding.** F1's plan
  places both explicitly out of scope and asserts zero changes to the defective helper; it only
  avoids reproducing them in its own new computation. Both confirmed OPEN on 2026-08-08. No issue
  #136 acceptance criterion requires them closed. The exposure is real but narrower than the brief
  states: it falls entirely on the repository-wide figure, which the capstone therefore recomputes
  itself. A Blocking finding is raised only if F1's delivered harness reads a descendant axis or an
  emitted rate attribute.
- **DEV-3 — the epic's explanation of the 70.19% -> 85.65% swing is wrong.** The stated second cause
  ("the two runs did not even instrument the same body of code") is refuted: the artifacts are raw
  versus post-processed stages of one pipeline, and the `lines-valid` growth is the #441 double
  count, proven by a literal `<line number=` count matching `lines-valid` exactly. Repeating the
  epic's explanation would propagate an error into the closing evidence.
- **DEV-4 — the denominator-instability claim is unconfirmed**, as the brief cautioned. Since the
  `lines-valid` growth is now fully explained by post-processing, the claim is unsupported. The plan
  captures the before/after pair in one session regardless, so the pair is defensible either way.
- **DEV-5 — the 24-file suppression figure is right but its stated mechanism is incomplete.**
  `QfcHighConfidencePreFilter.cs` declares four types and carries one type-level attribute on a
  secondary type only. It is *partially* suppressed and contributes **0** to the 24, not 1. A naive
  one-attribute-equals-one-file implementation computes 25 and then "corrects" the epic in the wrong
  direction. The census must be per-type, with a `21 + 5 + 7 = 33` partition cross-check.
- **DEV-6 — the epic manifest still carries one placeholder issue number.** Re-verified 2026-08-08:
  F12 has been back-filled to **495** and F15 to **496**, and F16's own `depends_on` list now names
  real issue numbers throughout, carrying no `1012`/`1015` entry. The sole remaining defect is F16's
  own entry — `issue_num: 1016` against the real **497**, and
  `feature_folder: quickfiler-per-file-coverage-capstone` against the real
  `2026-08-08-quickfiler-per-file-coverage-capstone-497`. Any epic-orchestrator gate keyed on F16's
  own manifest entry will fail or silently skip. This is an epic-sequencing finding for the manifest
  owner; F16 verifies the repair and does not perform it, because `epic.md` is not a per-child owned
  file.
- **DEV-7 — AC2's per-file obligation for third-bucket files is unsettled.** F13 has 11 research
  artifacts against 15 assigned files; the four missing are all interface-only. F2, F3, F6, and F7
  all produced artifacts for their interface files, and F7 gave them their own plan phases. The
  consistent reading is that the obligation does extend, because a research artifact is precisely
  what *establishes* third-bucket membership. Under that reading F13 has a four-file shortfall,
  reported as a finding naming F13 rather than closed by F16 writing the artifacts.

## Implementation Strategy

Phased verification, no remediation. Each phase emits its own evidence artifact.

1. **Phase 0** — policy reads, NuGet restore, toolchain availability probe, and confirmation that
   F1's ledger and harness actually exist on the branch. These are execution-time dependencies; no
   planning-time or preflight-time existence assertion is made, per F7's precedent.
2. **Harness trust gate** — inspect F1's delivered harness for a descendant axis and for any read of
   an emitted rate attribute *before* trusting any figure it emits.
3. **Denominator re-derivation** — parse `<Compile Include=` with namespace-aware or line-oriented
   parsing (22 entries are long-form with child elements; a self-closing-tag parser misses them),
   map to Cobertura `filename` by literal `"QuickFiler\" + Include` concatenation, and diff against
   the ledger in both directions.
4. **Measurement** — repository-wide before, full toolchain, repository-wide after, per-file figures.
5. **Reconciliation** — per-file gates, exemption grounds, third bucket, `measured-not-gated`,
   attribute census, #457 residuals.
6. **Cross-sibling audits** — AC2 through AC6, the defect trail, and the upstream conditions (F6's
   dead-region deletion, F4's eight promotions).
7. **Closure** — map each of issue #136's eight criteria to its evidence, check off the boxes.

No dependency changes. No logging changes. No feature flags.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to evidence artifacts
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Capstone-owned tooling (if any) has tests at the mirrored `tests/scripts/vscode/` path
- [ ] Edge cases covered: unledgered file, line-pass/branch-fail, zero-branch N/A, third-bucket N/A
- [ ] Docs updated (epic manifest verification finding reported)
- [ ] Toolchain pass completed (format -> analyze -> nullable -> coverage test)

## Acceptance Criteria

- [ ] **AC1** — A committed reconciliation report enumerates every file in the execution-time
      `<Compile Include=...>` set of `QuickFiler/QuickFiler.csproj` with its ledger bucket, its
      numeric line percentage, and its numeric branch percentage. The report fails closed if any
      compiled file has no ledger row. The re-derived file count is recorded and compared against the
      121-file planning-time figure; a divergence is expected and is reported, not treated as an
      error. The denominator is derived from the csproj, never from a filesystem glob.
- [ ] **AC2** — Every `testable` file is shown at `>= 80%` line and `>= 75%` branch, and every file
      this epic created is shown at `>= 90%` line. Both figures are reported for every file; a line
      figure alone is never accepted as proof of compliance. A file whose `branches-valid` is 0
      reports branch `N/A`, never `0%`, and never counts as a failure.
- [ ] **AC3** — Every `ratified-exempt` row carries exactly one explicit exemption ground drawn from
      the four ratified grounds, with the evidence that ground requires, keyed on the *type* and
      enumerating every file that type is declared in. A row citing no ground, a ground the file does
      not satisfy, or an unresolved disjunction is a Blocking finding. Disposition count reconciles
      to the attribute-usage count (40 at planning time), not the declaring-file count.
- [ ] **AC4** — Every `interface-only / not-measured` row is reported N/A with zero-coverable-lines
      evidence, and carries no `[ExcludeFromCodeCoverage]`. The `MailItemActionsAdapter` positive
      control is re-run to prove `Interfaces/` was instrumented, rather than inheriting F7's result.
      Every `measured-not-gated` row is verified to be genuinely generated code and not a testable
      file parked in a convenient bucket.
- [ ] **AC5** — A re-derived `[ExcludeFromCodeCoverage]` census over the compiled set shows no
      attribute remaining on a testable seam, distinguishing type-level from member-level usages,
      real attributes from doc-comment mentions, and files suppressed by partial-type propagation
      from files carrying their own attribute. Attributes traceable to closed maintainer-ratification
      issue **#227** are recorded with provenance and not re-litigated. The nine attributes deferred
      under open issue **#230** are explicitly **not** a gap and **not** a merge condition.
- [ ] **AC6** — Repository-wide line coverage is shown retained or improved as a self-consistent
      before/after pair captured on this branch in one session with an identical command and
      identical post-processing, recomputed from class-level `<line>` elements rather than
      transcribed from the corrupt root attributes, with both figures and the command recorded. No
      repository-wide figure is imported from another branch, tool, or artifact. The absolute policy
      floors are reported as informational alongside the delta gate, citing open issue **#494**
      rather than re-adjudicating the conflict.
- [ ] **AC7** — The full C# toolchain passes in final form in a single uninterrupted pass in the
      mandated order, and each stage's exit code and output summary is recorded:
      `dotnet tool run csharpier format .`, the analyzer build, the nullable build (`/t:Rebuild`),
      and coverage-enabled test execution. The csharpier subcommand deviation from `CLAUDE.md` is
      recorded in evidence; `CLAUDE.md` is not amended.
- [ ] **AC8** — Each of issue #136's eight acceptance criteria is explicitly marked closed with a
      citation to the specific evidence artifact that closes it, and issue #136's checkboxes are
      checked off accordingly with a local mirror under `evidence/issue-updates/`.
- [ ] **AC9** — Any file failing a gate is reported as a Blocking finding naming the owning child.
      The capstone does not fix a sibling's coverage and does not grant an exemption to close a gap.
- [ ] **AC10** — F1's delivered harness is inspected before any figure it emits is trusted, and is
      confirmed to read only the class-level `<lines>` block, to union classes sharing a filename
      with max-hits per line, and never to read an emitted `line-rate` / `branch-rate` attribute. A
      harness violating any of these is a Blocking finding. That #441 and #478 remain open in
      `Invoke-MSTestWithCoverage.Helpers.ps1` is recorded explicitly in evidence but is **not**
      itself a Blocking finding against F1.
- [ ] **AC11** — The deferred-defect promotion trail is verified against the live issue index: F4's
      eight recorded defects are checked asymmetrically — items 1-6 must have open issues (absence is
      Blocking naming F4), items 7-8 must be fixed in code (`MailItemInfoTests.cs:25` banned
      `DateTime.Now` removed, `ConversationResolverTests.cs` at or under 500 lines), and an issue for
      either of those two is itself a finding, being deferral of in-scope work.
- [ ] **AC12** — The two upstream conditions are confirmed landed: F6's plan revision deleting the
      dead `#region Email Sorting To Rewrite` from `QfcExplorerController.cs` (lines 183-321 of a
      323-line file), and the epic manifest's placeholder issue numbers for F12/F15/F16 resolved to
      495/496/497. Each unmet condition is reported as a finding against its owner, not repaired by
      F16.

## Test Conditions

- [ ] Denominator reconciliation against a csproj compile set parsed at execution time, including a
      case where a file present in the csproj has no ledger row, and a case exercising a long-form
      `<Compile>` entry with child elements.
- [ ] Per-file gate evaluation covering a passing file, a line-pass/branch-fail file, a zero-branch
      file that must report branch N/A, and a third-bucket file that must report N/A rather than 0%.
- [ ] Exemption-ground evaluation covering an accepted row under each of the four grounds and a
      rejected row citing no ground.
- [ ] `[ExcludeFromCodeCoverage]` census detection distinguishing a real attribute from a doc-comment
      mention, the fully-qualified spelling from the short spelling (20 of 40 usages are
      fully-qualified; a pattern anchored on the short form misses exactly half), a type-level from a
      member-level usage, propagation-suppressed partials, and the partially-suppressed multi-type
      file that must contribute 0 rather than 1.
- [ ] Repository-wide before/after capture on this branch with an identical command and identical
      post-processing, plus a negative case rejecting a raw-versus-post-processed comparison.

## Source

- Epic manifest: `docs/features/epics/quickfiler-per-file-coverage/epic.md` (child F16).
- Parent epic issue: https://github.com/drmoisan/TaskMaster/issues/136
- Feature issue: https://github.com/drmoisan/TaskMaster/issues/497
- Research: `research/measurement-harness-and-denominator.2026-08-08T00-45.md`,
  `research/exemption-reconciliation-and-ac-closure.2026-08-08T00-45.md`

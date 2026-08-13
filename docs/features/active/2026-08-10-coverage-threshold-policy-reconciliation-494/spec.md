# 2026-08-10-coverage-threshold-policy-reconciliation-494 (Spec)

- **Issue:** #494
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/494
- **Epic:** `build-ci-coverage-gate-fidelity` (wave 2; depends on #457, transitively #441/#478)
- **Integration branch:** `epic/build-ci-coverage-gate-fidelity-integration`
- **Feature branch:** `bug/coverage-threshold-policy-reconciliation-494`
- **Owner:** drmoisan
- **Work Mode:** `full-bug`
- **Last Updated:** 2026-08-10T16-10
- **Status:** Specified
- **Version:** 1.0

## Document Status and Acceptance-Criteria Authority

This document is the sole acceptance-criteria source for issue #494. The persisted `full-bug`
work-mode marker resolves acceptance-criteria ownership to `spec.md` under the
`acceptance-criteria-tracking` protocol. `issue.md` is contextual and its checkboxes are not
checked off by execution.

A `user-story.md` also exists in this feature folder. It exists only because the epic
preparation route requires it as a deliverable; it carries **no** acceptance criteria and is
**not** an acceptance-criteria authority. The AC authority for this feature is `issue.md` plus
this `spec.md`.

Governance-document line numbers in this document are labelled **"as of `edf3d34c`"** and were
re-verified against the working tree while this specification was written. Tooling locators are
anchored on function and symbol names only, because features #441, #457 and #478 modify
`scripts/vscode/Invoke-MSTestWithCoverage*.ps1` before this feature executes.

## User-Authorized Scope Correction

This section supersedes every prior delivery instruction in this document that would edit
TaskMaster `CLAUDE.md`, `.claude/**`, or `.agents/skills/**`, obtain an upstream receipt, or write
to any external repository. The existing TaskMaster artifact
`evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md` is the complete
local deliverable for all such prohibited runtime changes. Future application is expressly
deferred outside TaskMaster and does not block this feature.

The active TaskMaster implementation scope is the permitted coverage runner and its deterministic
Pester tests. Existing post-#441/#478/#457 measurements remain reusable evidence only after
schema and applicability validation; they are not authority to choose or lower a threshold. Any
earlier traceability, file inventory, test strategy, or rollout text describing local Claude
runtime edits is retained as historical research context and is non-executable under this scope
correction.

## Evidence Discipline

Claims below carry one of three labels. The distinction is load-bearing and must not be
flattened when this document is summarised.

- **[VERIFIED]** — read directly from the working tree while authoring this specification.
- **[RESEARCH]** — verified by the research record
  `research/2026-08-10T15-40-coverage-threshold-policy-reconciliation-research.md` in a prior
  session and not independently re-checked here.
- **[UNVERIFIED]** — could not be checked in the authoring session; the reason and the required
  verification step are stated at each occurrence.

**Tooling limitation, recorded explicitly.** The session that authored this specification had no
shell tool (available tools: Read, Grep, Glob, Write, Edit). **`git log` and `git blame` could not
be run.** Every commit SHA, commit date, and diff statistic appearing in this document was supplied
by the orchestrator and is therefore **[UNVERIFIED]** here. Each such claim is marked, and D1
records what happens if execution-time `git log` contradicts it.

---

## Context

Two always-loaded policy surfaces state incompatible coverage policies, and neither defers to the
other.

| Source (as of `edf3d34c`) | Line | Branch | New/changed code | Denominator |
|---|---|---|---|---|
| `CLAUDE.md` § UT2, lines 296-306 | >= 80% repo-wide | not stated | >= 90% for new modules/classes/methods | testable denominator, COM/VSTO/WinForms exempted |
| `.claude/rules/general-unit-test.md` lines 23-24, 31-46 | >= 85% all tiers | >= 75% all tiers | not stated | every production file, no exclusions |
| `.claude/rules/quality-tiers.md` lines 33-34, 51 | >= 85% | >= 75% | not stated | not stated |

All three sites re-verified **[VERIFIED]**.

The conflict is not only numeric. `CLAUDE.md:298-303` defines the metric's denominator as the
*testable* denominator, removing three named categories of production code before the floor
applies. `.claude/rules/general-unit-test.md:33` states the opposite as an absolute: "Every
production source file is in the denominator of the coverage metric, regardless of whether its
lines are reachable in the test environment." These are contradictory definitions of the same
quantity **[VERIFIED]**.

`CLAUDE.md:24` instructs agents to halt and notify the user on any conflicting instruction. A
conflict embedded in the policy documents themselves puts every agent in an unresolvable position
on nearly every code change. Agents have improvised rather than halting: issue #424 established an
in-repo precedent (change-scoped gates blocking, repo-wide figures reported non-blocking), and
issue #230 / PR #479 applied it by analogy in plan decisions D5 and D12 **[RESEARCH]**. A precedent
carried between runs by agent memory and prior-plan archaeology is not a policy: it is invisible to
reviewers, unenforced by tooling, and it drifts.

**Severity and frequency.** Every C# or PowerShell change in this repository is affected, because
`.claude/rules/general-unit-test.md` carries `paths: ["**"]` **[VERIFIED]** and `CLAUDE.md` is
always loaded. The defect is deterministic, not intermittent.

## Verified Current State

These facts constrain the fix. Each was re-verified while authoring this specification unless
labelled otherwise.

1. **The divergence spans five normative surfaces, not three documents.** Claude rules, Claude
   skills and agent definitions, the Codex `.agents/` bundle, the Copilot
   `.github/instructions/` tree, and root `AGENTS.md` **[RESEARCH]**; individual sites
   re-verified as noted in the Files and Sites section.
2. **`.claude/agents/feature-review.md` contradicts itself internally.** Lines 112-114 set
   85/75 for new files, modified files, and repo-wide. Lines 126-128 instruct the same agent, in
   the same numbered procedure, to flag FAIL at repo-wide below 80, new-file below 90, and
   modified-file below 80 **[VERIFIED]**. This is a live agent definition, not documentation, and
   it is absent from `issue.md`'s inventory.
3. **`.github/instructions/general-unit-test.instructions.md:39-40` states 80/90** **[VERIFIED]** —
   a fourth protected policy surface, also absent from `issue.md`'s inventory.
4. **The `.agents/` bundle is a stale snapshot, not a mirror.** `.agents/skills/powershell/SKILL.md:64-65`
   states 80/90 while `.claude/rules/powershell.md:63-64` states 85/75;
   `.agents/skills/powershell-qa-gate/SKILL.md:45` states >= 90% while its `.claude/` counterpart
   states 85/75; `.agents/skills/feature-review-workflow/SKILL.md:101-103` states 90/80/80 while
   its `.claude/` counterpart states 85/75 **[VERIFIED]**. A Codex session and a Claude session
   applying the same nominal policy today reach different verdicts on PowerShell work and on
   feature review.
5. **The only live numeric gate is a review hook, and it is internally inconsistent.**
   `.claude/hooks/validate-feature-review-coverage.ps1` documents "below 80 percent" in its
   `.SYNOPSIS` (line 29) and enforces `85.0` (line 313) and `$BranchFloor = 75.0` (line 323) in
   `Test-LanguageCoverageRow` **[VERIFIED]**.
6. **That gate skips its numeric checks when its input is absent.** `Get-LanguageRepoCoverage`
   returns `$null` when the artifact file is missing, and both numeric branches in
   `Test-LanguageCoverageRow` are guarded by `$null -ne` **[VERIFIED]**. The gate therefore
   silently passes when the coverage artifact is withheld. Committed agent memory records this as
   accepted practice **[RESEARCH]**.
7. **The gate's C# input has no committed producer.** The hook reads JaCoCo from
   `artifacts/csharp/coverage.xml` **[VERIFIED]**; `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
   emits Cobertura to the `-CoverageOutput` path, and no committed script or workflow produces
   `artifacts/csharp/coverage.xml` **[RESEARCH]**. The only recorded producer was an uncommitted
   scratchpad converter **[RESEARCH]**.
8. **The hook's line and branch checks are asymmetric.** The line check requires only that the
   policy-audit text contain a FAIL token; the branch check returns `Ok = $false` unconditionally
   and never inspects the audit **[VERIFIED]**. A sub-75 branch figure blocks subagent termination
   with no available disposition.
9. **The coverage runner enforces no threshold at all.** `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
   contains no threshold comparison; its only failure path is the test-process exit code
   **[RESEARCH]**. AC4 therefore requires new gate logic, not the edit of an existing constant.
10. **`quality-tiers.yml`, the `tier-classification` CI stage, and `docs/ci.research.md` are all
    absent.** Glob for `quality-tiers.y*ml` returns no files; glob for `**/ci.research*` returns no
    files; `.github/workflows/` contains only `ci.yml` and `codex-web-setup-test.yml` **[VERIFIED]**.
    `.claude/rules/quality-tiers.md:9,20-21` asserts all three **[VERIFIED]**.
11. **A fourth site asserts the missing tier file.** `.claude/rules/general-code-change.md:29`
    states "Every project must be classified in `quality-tiers.yml` at repo root" **[VERIFIED]**.
    Discovered during specification and absent from `issue.md`'s original inventory; `issue.md` AC6
    was widened on 2026-08-10T16-10 to name it, and it is now in scope. See D6.
12. **The 85/75 vocabulary does not fit this codebase.** `.claude/rules/general-unit-test.md:37-44`
    names `dist/**`, `lib-amd/**`, `**/*.test.ts`, `src/test-support/**`, `jest.config.cjs`,
    `eslint.config.mjs`, `.dependency-cruiser.cjs`, `webpack.config.js`, `node_modules/**`, and
    scopes its prohibition to "Any path under `src/`" **[VERIFIED]**. Glob for `**/*.ts` returns no
    files and glob for `**/package.json` returns no files **[VERIFIED]**. Every named example is
    inapplicable.
13. **`coverage.config` excludes no production assembly today.** It excludes only third-party and
    F#/mixed-mode modules **[RESEARCH]**. The `CLAUDE.md` exemption mechanism is a stated permission
    not currently exercised against any production assembly.
14. **`tests/scripts/powershell/` does not exist; `tests/scripts/vscode/` does**, with four Pester
    files **[VERIFIED]**.
15. **`policy-compliance-order` is the only document stating precedence between `CLAUDE.md` and
    `.claude/rules/`.** Its lines 19-28 place `CLAUDE.md` first **[VERIFIED]**. It is a skill, read
    on demand, not auto-loaded. `CLAUDE.md`'s own "Policy Compliance Order" (lines 9-16) ranks only
    `CLAUDE.md`'s own embedded sections and never mentions `.claude/rules/` **[VERIFIED]**.

---

## Decision Record

Each decision below is the recorded, explicit decision the epic charter and `issue.md`
"Governance-Document Authorization" require before any threshold may be changed. Each states the
decision, the reasoning, the evidence, and the alternatives rejected.

### D1 — Governing thresholds: line >= 80% repository-wide, >= 90% for new modules, classes, and methods

**Decision.** The governing coverage thresholds are **>= 80% line coverage repository-wide** and
**>= 90% line coverage for new modules, classes, and methods**, applied to the testable denominator
defined in D2. Branch coverage is dispositioned separately in D3.

**Reasoning.**

1. `CLAUDE.md:297` currently states "Repository-wide line coverage must remain `>= 80%`"
   **[VERIFIED]**. The orchestrator records this text as unchanged since commit `25684df8`
   (2026-03-21) **[UNVERIFIED — no shell tool in the authoring session; must be confirmed with
   `git log -L 292,306:CLAUDE.md` at execution time]**.
2. A persisted maintainer record of the #178 / PR #179 governance sync
   (`chore/sync-claude-hardening`) states, verbatim: **Kept** — "80% line / 90% new-module coverage
   (line-only, no branch gate)"; **Deliberately EXCLUDED** — "85% line / 75% branch coverage, the
   7-stage toolchain, the T1-T4 `quality-tiers.yml` system, `rules/architecture-boundaries.md` (it
   bans COM/VSTO — contradicts this codebase)", and "If a future `.claude` file references
   `quality-tiers.md` or 85/75, that is reference-repo leakage to revert" **[VERIFIED — read
   directly from the persisted memory record]**. This is a documentary record of a maintainer
   decision, not a commit; its authority derives from being a written record of the maintainer's
   stated directive, and it is corroborated by the in-tree evidence in point 4.
3. The 85/75 cluster is nevertheless present in the tree today **[VERIFIED]**. The orchestrator
   records that it entered at commit `48e46387` (2026-08-05, `(chore): push down claude ecosystem`),
   a 55-file, +4374/-641 bulk ecosystem sync that created `.claude/rules/quality-tiers.md`, rewrote
   `.claude/rules/general-unit-test.md`, **and did not touch `CLAUDE.md`**
   **[UNVERIFIED — must be confirmed at execution time]**. A bundle sync that leaves the
   conflicting document untouched makes no reconciliation decision; it imports one side of a
   conflict without adjudicating it.
4. The in-tree evidence that the 85/75 cluster is foreign is independent of any commit history and
   is **[VERIFIED]**:
   - `.claude/rules/general-unit-test.md`'s permitted-exclude list names `dist/**`,
     `node_modules/**`, `jest.config.cjs`, `**/*.test.ts`, and scopes its prohibition to `src/`.
     This repository has no `src/` directory, no `package.json`, no `node_modules/`, and zero `.ts`
     production files.
   - `.claude/rules/architecture-boundaries.md:22` bans new runtime references to
     `Microsoft.Office.Interop.Outlook`. TaskMaster is a VSTO Outlook add-in built on that API.
   - `.claude/rules/orchestrator-state.md` § "Foreign Schema Warning" names a foreign `$id` origin
     (`drmoisan.github.io/mix-calculator/`) outright and prohibits copying it verbatim — in-repo,
     committed proof that a foreign governance snapshot was imported and that at least one artifact
     from it was caught and quarantined.
   - `.claude/rules/quality-tiers.md:9` cites `docs/ci.research.md` as the tier system's source of
     truth; that document does not exist anywhere in the repository. Its tier examples name
     `TaskMaster.Domain`, `TaskMaster.Application`, a Graph adapter, and Office.js — none of which
     exist in `TaskMaster.sln`, and its own heading reads "Examples (No-COM architecture)".

**What this decision is, stated plainly.** Reconciling to 80/90 **restores a standing recorded
decision and removes un-reconciled import leakage. It does not lower a bar.** The epic NFR
(`epic.md:17-18`) forbids lowering a coverage threshold to accommodate a corrected denominator
without an explicit recorded decision, and `issue.md` requires that a reconciled number lower than a
current document's number be identified in those words and justified. The 85 in
`.claude/rules/general-unit-test.md` is numerically higher than the 80 in `CLAUDE.md`, so this
decision must be read carefully: it does not accommodate a moved measurement, and no measurement
figure is an input to it. It restores the number the maintainer recorded as kept and removes the
number the same record identifies as leakage to revert. If the higher number had ever been adopted
by a decision of this repository, this decision would be a lowering and would require separate
justification; the evidence in points 2-4 is that it was not.

**Evidence.** `CLAUDE.md:297,304` **[VERIFIED]**; persisted #178 governance-sync record
**[VERIFIED]**; the four in-tree foreignness indicators in point 4 **[VERIFIED]**; commit SHAs and
diff statistics **[UNVERIFIED]**.

**Execution-time verification gate (blocking).** Before any number is written into a governance
document, the executor must run and record in `evidence/baseline/`:

```
git log --follow --oneline -- .claude/rules/quality-tiers.md
git log --follow --oneline -- .claude/rules/general-unit-test.md
git log -L 23,24:.claude/rules/general-unit-test.md
git log -L 31,46:.claude/rules/general-unit-test.md
git log -L 292,306:CLAUDE.md
git show --stat 48e46387
```

If that history shows the 85/75 reintroduction was an explicit maintainer reconciliation decision
that also adjudicated `CLAUDE.md` — for example a commit touching both surfaces with a message
stating the change of policy — **D1's premise is falsified and D1 must be re-opened before any edit
is applied.** In that case the executor must halt and escalate rather than proceed. This gate is the
reason D1 can be taken autonomously: it is falsifiable at execution time by a cheap, deterministic
check.

**Alternatives considered and rejected.**

- *Adopt 85/75.* Rejected. It would ratify an import that the recorded maintainer decision named as
  leakage to revert, using vocabulary (`src/`, `node_modules`, Jest) that matches no code in this
  repository, and it would declare five of nine production assemblies failing on day one
  **[RESEARCH: per-package figures]** without a remediation path.
- *Adopt a third, negotiated number.* Rejected. Any third number would be an agent-originated policy
  invention with no recorded decision behind it, which is precisely the failure mode #494 exists to
  end.
- *Defer the number and reconcile only the exclusion policy.* Rejected. AC1 requires a single set of
  thresholds across the three documents; deferring the number does not satisfy it.

### D2 — The COM/VSTO/WinForms testable-denominator exemption survives; the blanket "no production file may be excluded" clause is superseded

**Decision.** The reconciled denominator rule is the **testable denominator** defined in
`CLAUDE.md` § UT2. The clause at `.claude/rules/general-unit-test.md:33` ("No production file may be
excluded from coverage measurement") and the § "Coverage Exclusion Policy" block at lines 31-46 are
**superseded** and removed from that document, which will instead cite the authority named in D4.
The carve-out at `CLAUDE.md:303` is preserved verbatim in substance: testable seams within
otherwise-COM-bound assemblies (`ToDoLoader`, `IDList` arithmetic, `KbdActions<>`, path and settings
helpers) are **NOT** exempt and must meet the floor.

**Reasoning.**

1. The exemption is already maintainer-ratified. `CLAUDE.md:303` states "**Authority**: This
   exemption must be ratified by the project maintainer and is tracked in
   `feature/csharp-coverage-uplift`" **[VERIFIED]**, and the exemption stands in the document as
   ratified text. **Retaining an already-ratified exemption requires no new ratification.** Revoking
   it would.
2. The exemption exists because COM/VSTO/WinForms code genuinely cannot be unit-tested in-process
   without a live Outlook host. Removing it without a replacement mechanism would make any floor
   unreachable rather than more rigorous, which is the failure mode the potential-feature entry
   already identified.
3. The competing clause arrived in the same foreign bundle as D1's 85/75 and is expressed entirely
   in TypeScript/Node terms **[VERIFIED — see Verified Current State item 12]**.
4. The competing clause does not literally bind the mechanism this repository uses. It regulates
   `exclude` **entries in a coverage configuration file**. The mechanism that actually removes
   production lines from the C# denominator here is the `[ExcludeFromCodeCoverage]` **attribute**,
   honoured by default by the VS Code Coverage collector and named explicitly in two per-project
   runsettings **[RESEARCH]**. A reconciled rule must therefore be written in terms of *which
   production lines leave the denominator by any mechanism*, not in terms of glob entries, or it is
   evadable by construction.

**Scope discipline.** D2 ratifies nothing new. It does **not** widen the exemption, does **not**
approve any new `[ExcludeFromCodeCoverage]` boundary, and does **not** disturb the recorded
maintainer denial of a blanket 103-member exemption boundary on issue #227 **[RESEARCH]**. Any new
exemption boundary remains a maintainer-ratification question under the retained `CLAUDE.md:303`
authority clause.

**Reconciled normative text (to be stated once, in the authority document).** The planner should
treat the following as the substance to be encoded, not as final wording:

> Coverage is measured against the **testable denominator**: all first-party production code except
> (a) VSTO add-in lifecycle classes, (b) WinForms form-derived and Designer-generated classes, and
> (c) Outlook Interop event-handler classes with no injectable seam, as enumerated in this section.
> Production lines may leave the denominator **only** through one of these enumerated categories,
> and only by a mechanism recorded in the repository — an `[ExcludeFromCodeCoverage]` attribute
> visible in a pull-request diff, or a `coverage.config` / runsettings exclusion. Removing
> production lines from the denominator by any other means, or for any category not enumerated
> here, is a Blocking finding. Testable seams inside otherwise-COM-bound assemblies (for example
> `ToDoLoader`, `IDList` arithmetic, `KbdActions<>`, path and settings helpers) are explicitly not
> exempt. Widening the enumerated categories requires maintainer ratification.

**Alternatives considered and rejected.**

- *Revoke the exemption and adopt refactor-don't-exclude.* Rejected for this feature. It is a
  maintainer decision (the `CLAUDE.md:303` authority clause reserves it), it would make the floor
  unreachable today with no transition path, and it is a substantially larger programme of work
  than reconciling documents. It is recorded as a candidate follow-up in Rollout and Follow-up.
- *Keep both clauses and scope them by language.* Rejected. The languages the exclusion clause names
  do not exist in this repository, so the scoping would be vacuous while leaving the contradictory
  text in an always-loaded rule file.
- *Keep the exemption but retain the "Blocking on production-path `exclude` entry" enforcement
  sentence.* Rejected as directly self-contradictory: `CLAUDE.md:303` authorises exactly such
  entries.

### D3 — Branch coverage is measured and reported, but is not a gating threshold

**Decision.** No branch-coverage floor is adopted. Branch coverage **must** be measured and reported
in coverage evidence artifacts and in feature-review policy audits, and it **must not** be used as a
blocking gate. This is recorded as an explicit decision, not as silence.

**Reasoning.**

1. `CLAUDE.md` § UT2 states no branch threshold **[VERIFIED]**. The persisted #178 record describes
   the kept policy as "80% line / 90% new-module coverage (**line-only, no branch gate**)"
   **[VERIFIED]**. The absence of a branch number in `CLAUDE.md` is therefore a recorded position,
   not an omission — which is the distinction this decision turns on.
2. Importing 75% by default from the superseded cluster would silently inherit a number from the
   same bundle D1 and D2 supersede, with no independent justification.
3. There is no measured evidence that supports 75% as a floor this repository can hold. The most
   recent committed repository-wide branch figure is 79.22% **[RESEARCH]**, but the per-package
   breakdown shows ToDoModel at 48.82%, SVGControl at 47.02%, TaskMaster at 65.18%, and QuickFiler
   at 74.65% **[RESEARCH]** — four of nine packages below 75% at package scope. Adopting 75% would
   declare four assemblies failing on day one with no remediation programme attached.
4. Branch coverage inherits the same measurement non-reproducibility documented in D5. The two #424
   runs differ by 20.7 branch-rate points (58.30% versus 79.00%) **[RESEARCH]** — a spread larger
   than the distance between any candidate branch floor and the current figure.

**What "reported but not gated" means concretely.** The AC4 gate computes and emits branch coverage
alongside line coverage. It does not fail on branch coverage. `.claude/hooks/validate-feature-review-coverage.ps1`
must have its unconditional branch-fail path (`$BranchFloor = 75.0`, `Test-LanguageCoverageRow`)
removed rather than re-numbered, which also resolves the line/branch asymmetry recorded in Verified
Current State item 8.

**Consistency requirement.** This decision must be stated identically in `CLAUDE.md` § UT2,
`.claude/rules/general-unit-test.md`, and `.claude/rules/quality-tiers.md` — in the latter two by
citation under D4, not by restatement.

**Alternatives considered and rejected.**

- *Adopt 75%.* Rejected: no independent justification, four of nine packages fail immediately, and
  the measurement cannot currently reproduce to that precision.
- *Adopt a lower branch floor derived from today's figures.* Rejected: it would be a number chosen
  to be passable rather than a number chosen to be meaningful, and the epic NFR forbids exactly that
  posture.
- *Say nothing about branch coverage.* Rejected: silence is what allowed 75% to enter unchallenged.
  The decision must be explicit and identical across the three documents.

### D4 — `CLAUDE.md` § UT2 is the single authority; all other documents cite it and state no number

**Decision.** `CLAUDE.md` § UT2 ("Coverage and Scenarios") is the **single authoritative source** for
coverage thresholds, for the coverage denominator, and for the coverage exclusion/exemption policy.
Every other document — rules, skills, agent definitions, instructions, and the `.agents/` bundle —
states the policy **by reference only** and contains no coverage-threshold numeral of its own. This
is the **cite-do-not-restate** convention.

**Reasoning.**

1. `CLAUDE.md` is always loaded. `.claude/rules/general-unit-test.md` carries `paths: ["**"]` and is
   also always loaded **[VERIFIED]**; those two are the only qualifying candidates. A path-scoped
   rule cannot hold the authority, because sessions touching non-matching files would see citations
   without the authority.
2. The exemption text that D2 reconciles already lives in `CLAUDE.md` § UT2, so placing the
   authority there requires moving no content.
3. `.claude/skills/policy-compliance-order/SKILL.md:19-28` places `CLAUDE.md` first when policies
   conflict **[VERIFIED]**, so this choice contradicts no existing precedence statement.
4. Every existing citation in the repository is of the degenerate "cite **and** restate" form —
   `.claude/rules/powershell.md:63`, `.claude/skills/powershell-qa-gate/SKILL.md:45`,
   `.claude/skills/feature-review-workflow/SKILL.md:111-114`,
   `.claude/rules/general-unit-test.md:26` **[RESEARCH]**. Restating a number next to its citation
   is the drift vector itself: the citation survives the copy while the number goes stale. No
   instance of citation *without* restatement was found anywhere in the repository **[RESEARCH]**.

**Correction to a common reading, recorded because it changes what must be written.** `issue.md` and
the potential-feature entry both state that "`CLAUDE.md`'s own Policy Compliance Order places itself
first, which would make 80/90 authoritative." That list (`CLAUDE.md:9-16`) ranks only `CLAUDE.md`'s
own embedded sections and never mentions `.claude/rules/` **[VERIFIED]**. It is a reasonable reading
but not what the document says. The only document that states precedence between `CLAUDE.md` and
`.claude/rules/` is the `policy-compliance-order` **skill**, which is read on demand and is not
auto-loaded **[VERIFIED]**. **Therefore the authority rule must be written explicitly into
`CLAUDE.md` § UT2 itself**, in the always-loaded surface, rather than relying on the existing
precedence list or on the skill. This is the substantive gap AC3 must close; a plan that merely
points at the existing precedence order does not close it.

**Conflict-resolution rule to be written (substance, not final wording).**

> Coverage thresholds, the coverage denominator, and the coverage exclusion policy are defined in
> this section (`CLAUDE.md` § UT2) and nowhere else. This section is authoritative for coverage
> policy and takes precedence over any other document in this repository, including files under
> `.claude/rules/`, `.claude/skills/`, `.claude/agents/`, `.github/instructions/`, `.agents/`, and
> `AGENTS.md`. Other documents may cite this section; they must not restate a coverage numeral. If
> another document states a coverage numeral that differs from this section, this section governs,
> the other document is defective, and the divergence must be filed as an issue. An agent
> encountering such a divergence resolves it by this rule and does **not** halt.

The last clause matters: it converts a case that currently triggers `CLAUDE.md`'s halt directive
into a case resolvable by rule, which is precisely what AC3 asks for.

**Mechanical guard.** Because the convention is a statement about the absence of numerals outside
one file, it is checkable by a repository scan. The AC4 test suite includes an authority-consistency
test (see Test Strategy) that asserts no coverage-threshold numeral appears in the governance
surface outside `CLAUDE.md` § UT2 and the enforcement script's named constants. This makes AC3
provable rather than aspirational.

**Alternatives considered and rejected.**

- *Name `.claude/rules/general-unit-test.md` authoritative.* Rejected. It is the document carrying
  the foreign content D1 and D2 supersede; naming it authoritative would preserve the import.
- *Author a new dedicated `coverage-policy.md` rule file.* Rejected. It would add a sixth normative
  surface to a defect whose cause is too many normative surfaces, and a new `.claude/rules/` file
  would need path scoping that cannot make it always-loaded more reliably than `CLAUDE.md` already
  is.
- *Rely on the existing `policy-compliance-order` precedence list.* Rejected: it is not auto-loaded,
  and it ranks documents rather than establishing single-source ownership of a numeral.

### D5 — The #424 / #230 precedent is ratified as the written rule, split by scope

**Decision.** The improvised precedent is **ratified**, not superseded, with an explicit split:

- **Change-scoped gates are blocking, unconditionally and from the moment this feature lands.**
  (i) No coverage regression on changed lines, measured against a baseline captured in the same
  change. (ii) New or changed modules, classes, and methods meet the >= 90% line bar from D1.
- **The repository-wide floor from D1 is measured, reported, and tracked, and becomes blocking only
  when measurement reproducibility is demonstrated to be within the tolerance stated below.**
  Until that condition is met, a repository-wide figure below 80% is a reported finding that must
  appear in the policy audit, and is not on its own a Blocking finding.

**Reasoning.**

1. **The measurement is not currently reproducible.** Two full-suite runs of the same command form
   on essentially the same tree, roughly 26 hours apart, produced denominators differing by 38.6%
   (79,957 versus 110,849 valid lines) and line rates of 70.19% versus 85.65% **[RESEARCH]**. The
   #424 evidence itself diagnoses this as non-deterministic assembly instrumentation — which
   assemblies get instrumented, and therefore how much uninstrumented vendored code lands in the
   denominator, varies between runs **[RESEARCH]**. Features #441/#478 correct *how* lines are
   counted and #457 corrects *which* lines are counted; **neither addresses instrumentation
   non-determinism**. A numeric floor is only enforceable against a reproducible measurement.
2. **The change-scoped half is compatible with every camp and with the measurement instability.**
   "No regression on changed lines" appears verbatim in seven governance documents **[RESEARCH]**,
   and the 90% new-unit bar is the 80/90 camp's own number, which the 85/75 camp does not contradict
   because it states no new-code number at all. Change-scoped gates compare a narrow, stable set of
   lines and are not exposed to whole-repository denominator drift in the same way.
3. **The precedent's own stated premise was factually wrong when written, and that argues for
   ratifying the *rule* while discarding the *justification*.** #424 plan item 13 asserted the
   baseline was "already below the 80% floor" at 70.19% line; the same feature's final measurement
   two hours later, with the same command form, was 85.65% **[RESEARCH]**, and the feature's own
   evidence calls the difference a measurement artifact. The correct conclusion is not that the
   floor is unreachable — it is that the measurement cannot presently establish whether the floor is
   met. That is the conclusion this decision encodes.
4. **Ratification with a named exit condition is strictly better than the status quo.** Today
   "reported, non-blocking" is the de facto universal state because no tooling enforces any
   repo-wide number **[RESEARCH]**. This decision converts that from an invisible improvisation into
   a written rule with a stated tolerance and a stated condition under which the floor becomes
   blocking — which is exactly what AC5 asks for.

**Reproducibility tolerance (the condition that makes the repo-wide floor blocking).** The
repository-wide line-coverage floor becomes a Blocking gate when, and only when, the following is
demonstrated and captured as evidence:

- **Three consecutive full-suite coverage runs** are executed against an **unchanged working tree**
  (no source, test, or project-file change between runs), using the same command form and the same
  toolchain versions; and
- the **maximum-minus-minimum repository-wide line rate across the three runs is <= 1.0 percentage
  point**; and
- the **maximum-minus-minimum `lines-valid` across the three runs is <= 0.5% of the median
  `lines-valid`**.

The second condition is required in addition to the first because a stable *rate* over an unstable
*denominator* is a coincidence, not reproducibility. Both conditions must hold. If either fails, the
repository-wide floor remains reported-and-tracked, and the measurement-determinism defect is filed
as a separate blocking issue (see Rollout and Follow-up).

The tolerance is stated as an absolute number rather than derived from the observed spread on
purpose: 1.0 percentage point is roughly one-fifteenth of the observed 15.5-point spread, so meeting
it is a genuine demonstration of a fixed defect rather than a restatement of current behaviour.

**Framing.** This is **ratification** of the precedent, not supersession. The change-scoped half is
adopted verbatim as written policy. The repository-wide half is adopted with a named exit condition
that the precedent lacked. The precedent's factual justification (the 70.19% figure) is explicitly
not carried forward and the reason is recorded above.

**Alternatives considered and rejected.**

- *Ratify the precedent wholesale, including "raw repo-wide figures non-blocking" with no exit
  condition.* Rejected. It would write a permanent exemption into policy with no path back, and it
  is incompatible with both camps' "must remain" phrasing.
- *Supersede the precedent entirely and make the repo-wide floor blocking immediately.* Rejected.
  It would make a Blocking gate depend on a measurement with a documented ±15-point run-to-run
  spread, producing both false failures and false passes. That is the precise failure mode this epic
  exists to eliminate.
- *Make the repo-wide floor blocking at package scope instead of repository scope.* Rejected for
  this feature. It is a materially different gate design, three of nine packages fail an 80% bar
  today **[RESEARCH]**, and it would require a remediation programme that is out of scope here. It
  is recorded as a candidate follow-up.

### D6 — The `quality-tiers.yml` / `tier-classification` / `docs/ci.research.md` claims are removed

**Decision.** The false assertions in `.claude/rules/quality-tiers.md` are **removed**, not
authored. Specifically: the `quality-tiers.yml` mapping-file claim (lines 9, 20), the
`tier-classification` CI-stage claim (line 21), and the `docs/ci.research.md` source-of-truth
citation (line 9) are deleted. The tier examples at lines 13-16, which name projects that do not
exist, are deleted or rewritten to name real projects.

**Reasoning.**

1. All three claimed artifacts are absent **[VERIFIED]**: glob for `quality-tiers.y*ml` returns no
   files; glob for `**/ci.research*` returns no files; `.github/workflows/` contains only `ci.yml`
   and `codex-web-setup-test.yml`, and neither mentions a tier stage. The document asserts three
   things that do not exist.
2. Authoring them would change no coverage gate. `.claude/rules/quality-tiers.md:25` and `:51` both
   state that line and branch coverage thresholds are **uniform across all tiers** **[VERIFIED]**.
   A classification that changes no threshold delivers no coverage-gate fidelity.
3. The tier-dependent gates that a classification *would* feed require four capabilities this
   repository does not have: an architecture-boundary checker, a property-testing library, a
   mutation-testing runner, and a golden-corpus harness **[RESEARCH]**. Authoring a mapping file,
   a CI stage, a validator, and the validator's test in order to feed gates that cannot run is cost
   with no return — and this epic exists specifically to stop gates from claiming things they do not
   do.

**Carve-out: dangling tier references.** The T1-T4 vocabulary is referenced outside
`quality-tiers.md`. Removing the false claims must not leave dangling references. Disposition:

| Site | Text | Disposition |
|---|---|---|
| `.claude/rules/architecture-boundaries.md:10` **[VERIFIED]** | "uniform gate across all tiers (T1-T4)" | **Leave unchanged.** The reference is to the tier vocabulary as a descriptive taxonomy, states no coverage numeral, and does not assert the missing file. Retaining the taxonomy without an asserted enforcement mechanism keeps the reference coherent. |
| `.claude/rules/powershell.md:63-64` **[VERIFIED]** | "line coverage >= 85% across all tiers (T1-T4) per `.claude/rules/quality-tiers.md`" / "branch coverage >= 75%" | **Deferred to the AC10 follow-up issue** (D7). It is a coverage-numeral site, so it is governed by D4's cite-do-not-restate rule; the follow-up replaces both lines with a citation of the authority. |
| `.claude/rules/general-unit-test.md:89,91` **[VERIFIED]** | "tier-dependent obligations per `.claude/rules/quality-tiers.md`" / "required for all tiers (T1-T4)" | **In scope.** This file is already being edited by this feature; the references are retained as taxonomy but must not cite removed claims. |
| `.claude/rules/general-code-change.md:29` **[VERIFIED]** | "Every project must be classified in `quality-tiers.yml` at repo root." | **In scope (AC6 widening 2026-08-10T16-10).** The sentence asserting the absent `quality-tiers.yml` is deleted; the preceding sentence citing `.claude/rules/quality-tiers.md` is retained. No other content in this file is edited. See the widening record below. |
| `.agents/skills/quality-tiers/SKILL.md:15,26-27` **[RESEARCH]** | duplicates all three false claims | **Split disposition (AC6 widening 2026-08-10T16-10).** The three false artifact claims (`quality-tiers.yml`, `tier-classification`, `docs/ci.research.md`) are **in scope** and removed here, because AC6 requires that no governance document assert an absent file and this file is the canonical Codex runtime surface. The file's 85/75 **coverage numerals** remain **deferred to FU-A** under D7, because converting numerals to citations is AC10 work, not AC6 work. |

**AC6 widening recorded (2026-08-10T16-10; supersedes the earlier deferral).**
`.claude/rules/general-code-change.md:29` asserts the missing `quality-tiers.yml` **[VERIFIED]**, and
`.agents/skills/quality-tiers/SKILL.md:27` duplicates all three false claims. An earlier revision of
this specification deferred both sites to the AC10 follow-up on the grounds that AC6 as originally
worded scoped the resolution to `.claude/rules/quality-tiers.md` alone. That deferral is **withdrawn**.
`issue.md` AC6 was widened on 2026-08-10T16-10 to cover every site carrying the claim, on the recorded
rationale that resolving one always-loaded rule file while leaving the identical false claim live in a
second always-loaded rule file reproduces the exact defect this feature exists to remove. The
supporting site inventory is verified at
`evidence/other/threshold-provenance-verification.2026-08-10T16-10.md` § E7.

The widening is deliberately narrow. It authorises the removal of the **false-artifact assertions
only**. It does not authorise any coverage-numeral edit at either site, does not authorise any other
content change in `.claude/rules/general-code-change.md`, and does not disturb the D7 deferral of the
numeral-to-citation conversion. Boundaries item 4 is read subject to this: the governance-edit
authorization covers the coverage-threshold and coverage-exclusion content `issue.md` enumerates,
**and** the tier-claim content AC6 enumerates, and nothing else.

**Alternatives considered and rejected.**

- *Author `quality-tiers.yml`, `docs/ci.research.md` § 1, the CI stage, the validator, and its
  Pester test.* Rejected on cost-versus-benefit as reasoned above.
- *Delete `.claude/rules/quality-tiers.md` entirely.* Rejected for this feature. It would leave four
  dangling references across three other rule files and would exceed the governance-edit
  authorization, which is scoped to the coverage-threshold and coverage-exclusion content
  `issue.md` enumerates. Recorded as a candidate follow-up.

### D7 — Disposition of every threshold-stating site

**Decision.** Every site that states a coverage numeral or asserts a coverage-policy mechanism is
assigned exactly one of three dispositions: **aligned here** (edited by this feature), **deferred**
(assigned to a named follow-up issue filed through the MCP promotion lifecycle before this feature
merges), or **non-normative** (declared to state no policy under the D4 authority rule). The full
table is in the Files and Sites section below.

**Reasoning.** `issue.md`'s inventory is incomplete. Research found three sites the inventory
missed, two of which are live behavioural surfaces rather than documentation **[VERIFIED]**:

1. **`.claude/agents/feature-review.md` states both camps fourteen lines apart** — 85/75 at lines
   112-114, and 90/80/80 FAIL instructions at lines 126-128 — inside a single numbered procedure in
   a live agent definition **[VERIFIED]**. Two agents reading the same file, or one agent reading
   both halves, produce different verdicts on the same evidence. This is a live behavioural
   contradiction, not a documentation nit.
2. **`.github/instructions/general-unit-test.instructions.md:39-40`** states 80/90 **[VERIFIED]**.
   It is a fourth protected policy surface, named by `policy-compliance-order`'s hard constraint
   alongside `.claude/rules/`.
3. **The `.agents/` bundle is a stale snapshot, not a mirror.** Three files state the opposite camp
   from their `.claude/` counterparts **[VERIFIED — see Verified Current State item 4]**, and
   `.agents/README.md:5` declares the directory "the canonical Codex runtime surface"
   **[RESEARCH]**. Claude sessions and Codex sessions therefore reach different coverage verdicts on
   the same code today.

**Note on the 512 boundary.** `.claude/rules/csharp.md:39-41`,
`.claude/skills/csharp-qa-gate/SKILL.md:46`, and the `CLAUDE.md` C# toolchain command blocks are
owned by sibling feature `csharp-toolchain-gate-fidelity-512` and are **out of bounds** for this
feature. Their coverage statements are 80/90 **[VERIFIED for `.claude/rules/csharp.md:39-40`]**,
which **already agrees with D1**. The coordination risk between #494 and #512 is therefore reduced
from a live policy conflict to a documentation-consistency item: after this feature lands, those
sites state the correct numbers but restate them rather than citing the authority, which the AC10
follow-up corrects.

### D8 — AC4 enforcement: a committed producer, a fail-closed gate, and a two-case negative-path proof

**Decision.** AC4 is satisfied by authoring new gate logic, not by editing a constant. The gate must
satisfy three requirements:

1. **Committed, reproducible producer.** The coverage artifact the gate consumes must be produced by
   a committed script or workflow step. No gate may depend on an artifact whose only recorded
   producer is an uncommitted scratchpad tool.
2. **Fail closed on missing input.** When the coverage artifact is absent, unreadable, or malformed,
   the gate returns a **failing** verdict with a distinguishable reason. Skipping the numeric check
   is prohibited.
3. **Two-case negative-path proof.** The AC4 acceptance evidence must demonstrate a non-zero result
   for **both** (a) a below-threshold input and (b) an absent-artifact input.

**Reasoning.**

1. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` performs no threshold comparison; its only failure
   path is the test-process exit code **[RESEARCH]**. There is no existing constant to edit.
2. `.claude/hooks/validate-feature-review-coverage.ps1` is the only numeric gate. It hard-codes
   `85.0` and `$BranchFloor = 75.0` while its `.SYNOPSIS` documents 80 **[VERIFIED]** — the AC8
   target.
3. That hook reads JaCoCo from `artifacts/csharp/coverage.xml` **[VERIFIED]**, but the toolchain
   emits Cobertura and no committed producer of the JaCoCo artifact exists **[RESEARCH]**.
4. **The decisive point.** `Get-LanguageRepoCoverage` returns `$null` when the artifact is absent,
   and both numeric checks in `Test-LanguageCoverageRow` are guarded by `$null -ne` **[VERIFIED]**,
   so the gate silently passes when its input is withheld. Committed agent memory records
   "deliberately not producing coverage.xml is a valid tactic" **[RESEARCH]**. **Without the
   artifact-absent case, the AC4 proof demonstrates nothing**: it would show only that a gate which
   can be bypassed produces the right answer when it is not bypassed.
5. Corroboration that fail-closed is the intended behaviour and not a new invention:
   `.claude/agents/feature-review.md:129` already instructs that a missing coverage artifact for a
   language with changed files is a **FAIL** **[VERIFIED]**. The hook contradicts the agent
   definition it exists to police. Making the hook fail closed aligns the two.

**Recommended producer decision (for the planner; substance, not sequencing).** Point the C# path of
the gate at the **Cobertura** artifact the repository actually produces, rather than adding a
Cobertura-to-JaCoCo converter to feed the existing JaCoCo path. Rationale: it removes a whole class
of format drift and eliminates the uncommitted-producer dependency in one step. If the JaCoCo path
must be retained for PowerShell, Python, or TypeScript, retain it; only the C# path changes. The
planner may choose the converter route instead if it re-verifies the trade-off at execution time,
but either way requirement 1 above is binding.

**Related defect to close in the same change.** The hook's line check requires only a FAIL token in
the audit text, while its branch check returns `Ok = $false` unconditionally **[VERIFIED]**. D3
removes the branch gate, which resolves the asymmetry. The line check must be re-expressed so that a
below-floor figure is itself the failure condition, not merely a requirement that the audit text
mention failure.

**Alternatives considered and rejected.**

- *Prove AC4 by running the real C# suite and observing a real regression.* Rejected. The
  measurement has a documented ±15-point run-to-run spread (D5), so the acceptance evidence would be
  hostage to the instability this epic has not yet fixed. It is also slow and non-deterministic,
  violating the unit-test policy.
- *Prove AC4 by mutating a source file and reverting.* Rejected. It mutates the working tree, is not
  repeatable in CI, and is not a unit test.
- *Edit the hook's constants and call that "tooling enforces the agreed thresholds".* Rejected: it
  leaves the artifact-absent bypass open, so the gate still cannot fail when it matters.

### D9 — Measurement sequencing: re-measure before writing, more than once, and never hard-code

**Decision.** No coverage numeral may be written into a governance document until repository-wide
coverage has been re-measured under post-#441/#478 and post-#457 arithmetic, at execution time, in
this feature's own branch, **at least three times**, with the spread recorded. Any figure captured
today is an **input to be refreshed**, never a value to hard-code.

**Reasoning.**

1. Every committed coverage figure in the repository was computed under the defective arithmetic
   **[RESEARCH]**: `ConvertTo-KoverageCoberturaXml` overwrites the root `<coverage>` attributes with
   values from `Get-CoberturaCoverageSummary`, which selects over the `.//lines/line` descendant
   axis — the #441 double count. None of the five committed repository-wide figures is the number
   this decision will be validated against.
2. The three-run requirement follows directly from D5. A single re-measurement cannot distinguish a
   corrected figure from an instrumentation artifact. The spread artifact is what makes the epic
   NFR ("no threshold lowered to accommodate a corrected denominator **without an explicit, recorded
   decision**") recordable rather than merely assertable.
3. The relationship between the measurement and D1 must be stated precisely: **the re-measurement
   does not choose the number.** D1 is decided on governance evidence, not on measurement. The
   re-measurement's role is to establish (a) what the corrected figures are, (b) whether the
   measurement is reproducible within D5's tolerance, and (c) which assemblies fall below the
   governing floor and therefore need a named remediation path. See Risks for what happens if the
   re-measurement contradicts D1.

**Operational hazards that the plan must handle (all confirmed by research).**

| Hazard | Handling |
|---|---|
| Aggregate `vstest` runs can crash the test host | `/InIsolation` is already passed unconditionally by `Get-DotnetCoverageArgumentList` **[RESEARCH]**. The residual crash cause in the repository record is concurrent test runs from sibling agent worktrees. The plan must require that no other worktree is executing tests during the re-measurement, and must record `Total tests:` explicitly so an `Unknown` outcome is detected rather than silently accepted. Per-assembly isolation is the fallback if the aggregate run still crashes. |
| Recursive `*.Test.dll` discovery collects stale agent builds | The script filters only on `\bin\<Config>\`, `\obj\`, `\ref\` **[RESEARCH]**; it does **not** exclude `\.claude\`. A naive substring test is wrong when running inside a worktree, because the workspace root is itself under `\.claude\worktrees\`. **Correct assertion:** every discovered path begins with the workspace-root prefix, **and** no discovered path contains a `\.claude\worktrees\` segment *after* that prefix. Record the full assembly list and count in the evidence artifact. |
| MSBuild `/t:Build` skips `CoreCompile` | Precede the coverage run with `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`. Do **not** add `/p:Nullable=enable` — issue #522 records it as a defective gate producing roughly 200-414 errors red on a clean `main`; it is out of scope per `issue.md`. |
| Toolchain drift between research and execution | The executor must re-read `Get-CoberturaCoverageSummary`, `Merge-CoberturaClassesByFilename`, `ConvertTo-KoverageCoberturaXml`, and `Get-KoverageProjectAllowlist` and record their then-current signatures in `evidence/baseline/` before running anything. Anchor on symbol names, never line numbers. |

**Alternatives considered and rejected.**

- *Re-measure once.* Rejected: it cannot establish reproducibility, which D5 makes a condition of the
  repository-wide floor becoming blocking.
- *Write the numbers first and re-measure afterwards as confirmation.* Rejected: AC7 requires the
  re-measurement to be captured as evidence **before** the numbers are written.
- *Reuse the most recent committed figure from #230.* Rejected: it was computed under the defective
  arithmetic and predates both #441/#478 and #457.

---

## Files and Sites

### In-scope changes

Tooling locators are function and symbol names. Governance-document line numbers are **as of
`edf3d34c`** and must be re-resolved at execution time.

| # | Path | Locator | Change |
|---|---|---|---|
| 1 | `CLAUDE.md` | § UT2 "Coverage and Scenarios", lines 292-306 | Restate thresholds per D1 and D3; restate the reconciled denominator rule per D2; add the authority declaration and conflict-resolution rule per D4; add the ratified change-scoped and repo-wide rules per D5. Lines 308-315 ("Scenario Completeness") are not touched. |
| 2 | `.claude/rules/general-unit-test.md` | § "Coverage Requirements" (lines 21-29) and § "Coverage Exclusion Policy" (lines 31-46) | Replace both blocks with a citation of `CLAUDE.md` § UT2 stating no numeral, per D2 and D4. Retain the non-coverage content. Ensure the tier references at lines 89 and 91 do not cite claims removed by D6. |
| 3 | `.claude/rules/quality-tiers.md` | Lines 9, 20-21 (false claims); 13-16 (non-existent project examples); 33-34 and 51 (numerals) | Remove the false claims per D6; replace the coverage numerals with a citation per D4. |
| 3a | `.claude/rules/general-code-change.md` | The sentence "Every project must be classified in `quality-tiers.yml` at repo root." (line 29 as of `edf3d34c`; re-locate by quoted text) | **AC6 widening only.** Delete that one sentence; retain the preceding sentence citing `.claude/rules/quality-tiers.md`. No coverage numeral and no other content in this file is edited. |
| 3b | `.agents/skills/quality-tiers/SKILL.md` | The `quality-tiers.yml`, `tier-classification`, and `docs/ci.research.md` claims (lines 15, 26-27 as of `edf3d34c`; re-locate by quoted text) | **AC6 widening only.** Remove the three false artifact claims per D6; leave the tier taxonomy intact. The file's 85/75 coverage numerals are **not** edited here and remain deferred to FU-A. |
| 4 | `.claude/hooks/validate-feature-review-coverage.ps1` | `.SYNOPSIS`; `Test-LanguageCoverageRow`; `Get-LanguageRepoCoverage`; `Get-LanguageBranchCoverage` | AC8: make documented behaviour and enforced constants agree and equal the D1 numbers. Extract the floor to a named script-scope constant with a comment citing the authority. Remove the unconditional branch-fail path per D3. Make the artifact-absent path fail closed per D8. Adjust the C# artifact path/format per D8's producer decision. |
| 5 | New gate script (path chosen by the plan; `scripts/vscode/` recommended) | new pure function plus thin I/O wrapper | AC4: the threshold gate. Pure function takes coverage figures or a coverage-XML string plus floors and returns a structured verdict; wrapper reads the artifact. Only the wrapper touches the filesystem. |
| 6 | Committed coverage-artifact producer (path chosen by the plan) | — | AC4/D8 requirement 1: a committed, reproducible producer for the artifact the gate consumes. May be satisfied by re-pointing the gate at the existing Cobertura output rather than by adding a converter. |
| 7 | `tests/scripts/vscode/<Name>.Tests.ps1` (and, if the hook is directly tested, `tests/.claude/hooks/validate-feature-review-coverage.Tests.ps1`) | new | AC9: Pester tests for the gate. See Test Strategy for the AC9 path restatement. |
| 8 | `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/**` | `baseline/`, `qa-gates/`, `regression-testing/` | Evidence artifacts. No `artifacts/**` evidence path is permitted. |

### Out-of-scope sites and their dispositions (AC10)

Three dispositions are used. **Aligned** — not applicable in this table; all aligned sites are in the
in-scope table above. **Deferred** — assigned to follow-up issue **FU-A** (see Rollout and
Follow-up), which must be filed through the MCP promotion lifecycle **before this feature merges**,
because prose in a feature folder does not survive the merge. **Non-normative** — declared under the
D4 authority rule to state no policy; a stale numeral there is incorrect but not authoritative.

| # | Site (line numbers as of `edf3d34c`) | Current statement | Disposition | Note |
|---|---|---|---|---|
| 1 | `AGENTS.md:372-373` | 80/90, **without** the COM/VSTO exemption | **Deferred (FU-A)** | Root Codex instructions. Its numerals agree with D1 but it states a third distinct position by omitting the exemption, so it must be replaced by a citation, not left as-is. |
| 2 | `.github/instructions/general-unit-test.instructions.md:39-40` **[VERIFIED]** | 80/90 | **Deferred (FU-A)** | Fourth protected policy surface. Not in `issue.md`'s inventory. Numerals agree with D1. |
| 3 | `.claude/rules/python.md:16,88-89` | 80/90 | **Non-normative** | No Python production code exists in this repository **[RESEARCH]**; the rule governs nothing. FU-A should still replace the numerals with a citation for consistency, but the site cannot produce a wrong verdict today. |
| 4 | `.claude/rules/typescript.md:42-43` | 80/90 | **Non-normative** | No TypeScript production code exists **[VERIFIED: glob `**/*.ts` returns no files]**. Same treatment as #3. |
| 5 | `.claude/rules/powershell.md:63-64` **[VERIFIED]** | 85/75 | **Deferred (FU-A)** | **Live and wrong after this feature lands.** PowerShell production code exists (10 scripts under `scripts/`, 33 hooks). Highest-priority FU-A item. |
| 6 | `.claude/skills/powershell-qa-gate/SKILL.md:45` | 85/75 | **Deferred (FU-A)** | Same. |
| 7 | `.claude/skills/python-qa-gate/SKILL.md:46` | >= 90% new units | **Non-normative** | Agrees with D1; no Python code. |
| 8 | `.claude/skills/feature-review-workflow/SKILL.md:112-114` | 85/75 | **Deferred (FU-A)** | Live feature-review behaviour. |
| 9 | `.claude/agents/feature-review.md:112-114` and `:126-128` **[VERIFIED]** | 85/75 **and** 90/80/80 in one procedure | **Deferred (FU-A), flagged highest severity** | A live agent definition that contradicts itself fourteen lines apart. Not in `issue.md`'s inventory. FU-A must replace both blocks with a single citation of the authority. Until then, D4's conflict-resolution rule makes `CLAUDE.md` § UT2 govern, so the contradiction is resolvable rather than halting — but the agent still contains two different numeric procedures and will produce inconsistent verdicts. |
| 10 | `.claude/agents/feature-review.md:129` **[VERIFIED]** | missing artifact = FAIL | **No change needed** | Already fail-closed; corroborates D8 and is the behaviour the hook must be brought into line with. |
| 11 | `.agents/skills/general-unit-test/SKILL.md:29-30` **[VERIFIED]** | 85/75 | **Deferred (FU-A)** | Codex runtime surface. |
| 12 | `.agents/skills/quality-tiers/SKILL.md:15,26-27,39-40,49,57` **[VERIFIED for 39-40,57]** | 85/75 plus all three false tier claims | **Split: false tier claims aligned here (in-scope row 3b); 85/75 numerals deferred (FU-A)** | The three false artifact claims are removed by this feature under the widened AC6. The coverage numerals are AC10 work and stay with FU-A. |
| 13 | `.agents/skills/powershell/SKILL.md:64-65` **[VERIFIED]** | 80/90 | **Deferred (FU-A)** | **Diverges from its `.claude/` counterpart (85/75).** Numerals happen to agree with D1; the divergence itself is the defect. |
| 14 | `.agents/skills/powershell-qa-gate/SKILL.md:45` **[VERIFIED]** | >= 90% | **Deferred (FU-A)** | Diverges from its `.claude/` counterpart (85/75). |
| 15 | `.agents/skills/feature-review-workflow/SKILL.md:101-103` **[VERIFIED]** | 90/80/80 | **Deferred (FU-A)** | Diverges from its `.claude/` counterpart (85/75). |
| 16 | `.agents/skills/csharp/SKILL.md:42-43`, `python/SKILL.md:17,89-90`, `typescript/SKILL.md:43-44`, `csharp-qa-gate/SKILL.md:48`, `python-qa-gate/SKILL.md:46` **[VERIFIED]** | 80/90 | **Deferred (FU-A)** | Agree with D1; must still be converted to citations. |
| 17 | `.claude/rules/csharp.md:39-41` **[VERIFIED for 39-40]** | 80/90 | **Deferred (FU-A); file owned by 512** | **Out of bounds for this feature.** Numerals already agree with D1, which reduces the 512 coordination risk to a documentation-consistency item. Sits at lines 39-41, outside 512's stated edit range (lines 14-16 and 83), so no merge conflict is expected in either order. |
| 18 | `.claude/skills/csharp-qa-gate/SKILL.md:46` | >= 90% new units | **Deferred (FU-A); file owned by 512** | Same. 512's stated edit range is line 32. |
| 19 | `CLAUDE.md` C# toolchain command blocks (regions 181-208, 377-386, 397-402) | toolchain commands | **Out of bounds; owned by 512** | Provably disjoint from § UT2 (292-306); nearest approach is 71 lines **[RESEARCH]**. No merge conflict expected. |
| 20 | `.claude/rules/general-code-change.md:29` **[VERIFIED]** | asserts `quality-tiers.yml` exists | **Aligned here (in-scope row 3a)** | Named by AC6 as widened on 2026-08-10T16-10. The single false-assertion sentence is deleted by this feature; the file's remaining content is untouched. The earlier FU-A deferral of this site is withdrawn. |
| 21 | `.claude/rules/architecture-boundaries.md:10` **[VERIFIED]** | "all tiers (T1-T4)", no numeral | **No change needed** | Taxonomy reference only; states no coverage numeral and asserts no missing file. |
| 22 | `.claude/agent-memory/**` entries asserting coverage authority | prose assertions that `CLAUDE.md` 80/90 governs | **Superseded on landing** | Once D4 is written into `CLAUDE.md`, these memory entries are redundant. They are not policy and must not be cited as such. FU-A should prune or annotate them. |
| 23 | `scripts/temp-extract-coverage.ps1` | `if ($lr -lt 0.80)` categorisation, hard-coded output path to a non-existent feature folder | **Deferred (separate follow-up FU-C)** | Not a threshold site; a committed throwaway script and a latent-cleanup candidate. |

---

## Boundaries and Invariants

The following must not change. A violation of any item is a Blocking finding.

1. **512-owned files and regions are not edited by this feature**: the `CLAUDE.md` C# toolchain
   command blocks, `.claude/rules/csharp.md`, and `.claude/skills/csharp-qa-gate/SKILL.md`. This is
   a hard limit from `issue.md` and from the epic's per-issue Execution Authorization.
2. **No implicit threshold re-tuning.** The epic non-goal (`epic.md:81-84`) forbids re-tuning a
   threshold to accommodate a corrected denominator as an implicit act. Every threshold in this
   change traces to D1 or D3, and neither takes a measurement as an input.
3. **No policy may be relaxed to make a gate pass.** `epic.md:196-197` states this explicitly. If
   the re-measurement shows the repository below the D1 floor, the response is the D5
   reported-and-tracked disposition plus a named remediation path — **not** a lower number.
4. **The governance-edit authorization is narrow.** It suspends the `policy-compliance-order` hard
   constraint against editing `.claude/rules/` **only** for the coverage-threshold and
   coverage-exclusion content `issue.md` enumerates, **and** for the tier-claim content AC6
   enumerates as widened on 2026-08-10T16-10 (the `quality-tiers.yml` / `tier-classification` /
   `docs/ci.research.md` assertions at `.claude/rules/quality-tiers.md`,
   `.claude/rules/general-code-change.md:29`, and `.agents/skills/quality-tiers/SKILL.md:27`). No
   other content in those files may be edited for any purpose. In particular, the AC6 widening
   authorises **no** coverage-numeral edit at the two newly added sites.
5. **The `CLAUDE.md:303` maintainer-ratification clause for the exemption is retained.** D2 keeps an
   already-ratified exemption; it does not grant new exemption authority to agents, and it does not
   disturb the recorded #227 denial precedent.
6. **`CLAUDE.md` § UT2's "Scenario Completeness" sub-block (lines 308-315) is not touched.**
7. **Existing unit tests are part of the spec.** The four Pester files under `tests/scripts/vscode/`
   must continue to pass unchanged.
8. **No temporary files anywhere in tests.** `CLAUDE.md` records "Currently approved exceptions:
   none."
9. **Evidence goes to `<FEATURE>/evidence/<kind>/` only.** `artifacts/baselines/`, `artifacts/qa/`,
   `artifacts/coverage/`, and `artifacts/evidence/` are prohibited and are blocked by the
   `enforce-evidence-locations.ps1` PreToolUse hook.
10. **The `/p:Nullable=enable` type-check command is not a gate for this feature.** Issue #522
    records it as defective, producing roughly 200-414 errors red on a clean `main`; it is fixed by
    sibling feature 512.

---

## Test Strategy

### AC4 negative-path proof — both cases required

The acceptance evidence for AC4 is a captured transcript showing the gate returning non-zero for
**both** of the following. A proof covering only the first case is insufficient and does not satisfy
AC4, for the reason recorded in D8.

- **Case A — below-threshold input.** A synthetic coverage document whose repository-wide line rate
  is below the D1 floor produces a failing verdict and a non-zero exit code.
- **Case B — absent artifact.** With the coverage artifact absent (simulated through an injectable
  reader, not by deleting a file), the gate produces a failing verdict and a non-zero exit code. It
  must not skip the numeric check and must not return success.

### Pester test cases

Each `It` uses an inline here-string fixture — the pattern already proven at
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`, which builds Cobertura XML in a
`@'...'@` here-string and passes it to a pure function. Zero new infrastructure is required.

1. Line rate above the floor → pass verdict, exit 0.
2. Line rate one basis point **below** the floor → fail verdict, non-zero. *(boundary; Case A)*
3. Line rate **exactly at** the floor → pass. *(boundary; pins `>=` against `>`)*
4. Artifact absent or unreadable → fail verdict, non-zero. *(Case B; uses the injectable reader so no
   filesystem access occurs)*
5. Malformed XML → fail verdict with a distinguishable reason. Fail closed, never fail open.
6. Branch rate below any historical branch floor, line rate above the line floor → **pass**, because
   D3 removes the branch gate. This test pins D3 against silent reintroduction.
7. **Authority-consistency test (AC3).** Assert that no coverage-threshold numeral appears in the
   governance surface outside `CLAUDE.md` § UT2 and the enforcement script's named constants, and
   that the gate's floor constant equals the number stated in the authority. Implement as a pure
   function over supplied document text so the test remains deterministic and filesystem-free where
   possible; where a repository scan is required, it must read committed files only and create
   nothing. This makes AC3 mechanically provable rather than aspirational.

### AC9 test location — restatement recorded as a deliberate correction

AC9 states that Pester tests live at `tests/scripts/powershell/<Name>.Tests.ps1`. That literal path
derives from the **example** in `.claude/rules/general-unit-test.md` § "Test File Location", which
states a **mirroring** rule; `.claude/rules/powershell.md` gives a different example
(`tests/scripts/dev-tools/ScriptName.Tests.ps1`) for the same rule **[RESEARCH]**. This repository
has no `scripts/powershell/` directory, and `tests/scripts/vscode/` already exists with four Pester
files **[VERIFIED]**.

**Restatement.** AC9 is satisfied by placing each Pester test at the mirror of its subject:

- gate at `scripts/vscode/<Name>.ps1` → test at `tests/scripts/vscode/<Name>.Tests.ps1` *(preferred;
  joins the established tree)*
- gate at `scripts/dev-tools/<Name>.ps1` → test at `tests/scripts/dev-tools/<Name>.Tests.ps1`
- test for `.claude/hooks/validate-feature-review-coverage.ps1` → mirror at
  `tests/.claude/hooks/validate-feature-review-coverage.Tests.ps1`

The literal `tests/scripts/powershell/` path is satisfied only if the gate is placed at
`scripts/powershell/`, which would create a third script directory for no reason. This restatement is
recorded so that a reviewer sees a deliberate correction rather than a deviation. AC9's substantive
requirements — deterministic, no temporary files — are unchanged and binding.

### Determinism and prohibited practices

- Tests must be independent, isolated, fast, and deterministic; any order, any run, same result.
- **No temporary files.** No `New-Item -ItemType File` in a temp path, no `New-TemporaryFile`, no
  scratch directory. Fixtures are inline here-strings or committed files.
- No external services, no network, no test-host process spawning, no wall-clock dependence.
- Filesystem access in the gate is confined to the thin wrapper; the pure function is tested without
  touching disk.
- Assertions use Pester's `Should` with messages that identify the failing condition.

### Toolchain

PowerShell changes: format → PSScriptAnalyzer → Pester. No type-check stage applies to PowerShell.
No C# source changes are expected, so the C# toolchain runs only as the coverage re-measurement of
D9. If any C# file is touched, the full four-stage C# toolchain applies, excluding
`/p:Nullable=enable` per Boundaries item 10.

### Evidence

- `evidence/baseline/` — pre-change coverage re-measurement (three runs plus the spread artifact),
  the D1 execution-time `git log` verification output, and the recorded signatures of the four
  coverage functions.
- `evidence/regression-testing/` — the AC4 two-case negative-path proof transcript, with
  `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` that names Case A and Case B
  individually.
- `evidence/qa-gates/` — final toolchain pass.

---

## Risks and Mitigations

| # | Risk | Likelihood / impact | Mitigation |
|---|---|---|---|
| 1 | **Measurement non-reproducibility.** The repository-wide figure has a documented ±15-point run-to-run spread caused by non-deterministic assembly instrumentation, which neither #441/#478 nor #457 fixes **[RESEARCH]**. | High / high | D5 makes the repository-wide floor reported-and-tracked with a stated numeric tolerance and a named exit condition, so no Blocking gate depends on an irreproducible number. D9 requires three runs and a recorded spread. If the tolerance is not met, file the determinism defect as a blocking issue (FU-B) rather than ratifying an unenforceable floor. |
| 2 | **Re-measurement under corrected arithmetic contradicts D1** — for example the corrected repository-wide figure lands well below 80%, or the corrected figure makes 85% comfortably achievable. | Medium / high | **This specification must not assume its own conclusion.** D1 is decided on governance evidence and takes no measurement as an input, so a corrected figure cannot by itself change the number. What a contradicting measurement changes is the *disposition*: (a) if the corrected repository-wide figure is below 80%, D1 stands unchanged and D5's reported-and-tracked disposition applies, with the failing assemblies enumerated in the evidence artifact and a remediation issue filed — the number is **not** lowered, per Boundaries item 3 and the epic NFR. The "remediation issue filed" clause is satisfied by the existing potential entry `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` recorded under § Rollout ("Candidate follow-ups, not required before merge"); no new issue is filed by this feature under disposition (a); (b) if the corrected figure is comfortably above 85%, that is **not** a reason to adopt 85, because D1 rests on the recorded decision and the import-leakage finding, not on achievability — but the executor must record the observation in the evidence artifact so a maintainer can revisit it deliberately; (c) if the D1 execution-time `git log` gate is falsified (see D1), the executor halts and escalates before applying any edit. |
| 3 | **The stale `.agents/` snapshot causes cross-toolchain verdict divergence.** Three `.agents/` files state the opposite camp from their `.claude/` counterparts **[VERIFIED]**, and `.agents/` is the canonical Codex runtime surface. After this feature lands, `.claude/` is reconciled and `.agents/` is not, so a Codex session and a Claude session still reach different verdicts. | High / medium | The divergence is **not closed by this feature**; it is deferred to FU-A, which must be filed before merge. Mitigating factor: D4's conflict-resolution rule names `CLAUDE.md` § UT2 authoritative over `.agents/` explicitly, so the divergence is resolvable by rule rather than by halting. Residual exposure: a Codex session that does not read `CLAUDE.md` would not see the rule. This residual risk is accepted and recorded rather than mitigated, because editing `.agents/` is outside the governance-edit authorization `issue.md` grants. |
| 4 | **Merge coordination with sibling feature 512.** Both features edit `CLAUDE.md`. | Low / low | The regions are provably disjoint: § UT2 ends at line 306 and 512's nearest region begins at 377 — a 71-line separation **[RESEARCH]**. `.claude/rules/csharp.md:39-41` sits outside 512's stated edit range (14-16 and 83). Merge order is immaterial and no wave edge is required. The residual item is semantic, not textual: `.claude/rules/csharp.md:39-40` and `.claude/skills/csharp-qa-gate/SKILL.md:46` already state 80/90, which agrees with D1, so the coordination risk is a documentation-consistency item handled by FU-A. |
| 5 | **The governance-edit authorization is exceeded.** Editing coverage content in a `.claude/rules/` file is authorized; editing anything else there is not. | Medium / high | Boundaries item 4. Every edit must trace to a row in the in-scope Files and Sites table. Feature review must treat an unlisted `.claude/rules/` edit as Blocking. |
| 6 | **The AC4 gate is authored but is not wired into any executing path**, leaving a gate that exists and never runs — the exact failure mode this epic exists to end. | Medium / high | D8 requirement 1 makes a committed producer mandatory. The plan must name the executing path (which script or hook invokes the gate) as a task acceptance criterion, not leave it implicit. |
| 7 | **Removing the branch gate (D3) is misread as relaxing a policy to make a gate pass.** | Medium / medium | D3 records the reasoning, the #178 "line-only, no branch gate" record, and the four-of-nine package evidence explicitly, and requires the decision be stated identically in all three documents. The removed branch gate was never a decision of this repository; it arrived with the same import D1 supersedes. |
| 8 | **A false `quality-tiers.yml` claim survives in a second always-loaded rule file** after AC6 removes it from `quality-tiers.md`. | Closed by the AC6 widening | Withdrawn as a residual gap. AC6 was widened on 2026-08-10T16-10 to name `.claude/rules/general-code-change.md:29` and `.agents/skills/quality-tiers/SKILL.md:27` explicitly; both are in-scope rows 3a and 3b, and the final verification grep must return zero hits for `quality-tiers.yml`, `tier-classification`, and `ci.research` across all AC6 sites. The residual exposure is now limited to any site not present in the verified inventory at `evidence/other/threshold-provenance-verification.2026-08-10T16-10.md` § E7; the execution-time inventory task re-runs that search to detect one. |
| 9 | **Concurrent agent worktrees corrupt the re-measurement** through test-host contention or stale `*.Test.dll` discovery. | Medium / high | D9's hazard table: require no concurrent test execution, assert the worktree-prefix condition on every discovered assembly, record the assembly list and count, and record `Total tests:` so an `Unknown` outcome is detected. |

---

## Automation Feasibility

**Determination: this feature executes autonomously. No blocking human interaction is required.**

The research record concluded that the feature "cannot execute fully autonomously" because two
decisions — the governing numbers and the survival of the exemption — were characterised as
maintainer decisions. That characterisation has been **reframed on the evidence**, and the reframing
is what makes autonomous execution defensible:

- **The governing-number choice is not a new maintainer decision.** It was framed as one on the
  assumption that an agent would be adjudicating between two competing maintainer-adjacent
  positions. The evidence shows only one recorded decision exists (#178: keep 80/90, reject 85/75 as
  leakage to revert) **[VERIFIED]**, and that the competing cluster entered through a bulk ecosystem
  sync that left the conflicting document untouched **[UNVERIFIED — the D1 execution-time gate
  tests this]**. Choosing 80/90 therefore **restores a standing recorded decision and removes
  un-reconciled import leakage**. It changes no maintainer decision and reverses only unattributed
  drift.
- **The exemption question is not a ratify-or-revoke decision here.** The exemption is already
  ratified in `CLAUDE.md:303` **[VERIFIED]**. D2 retains it. **Retaining an already-ratified
  exemption requires no new ratification**; revoking it would, which is why revocation is deferred
  rather than performed.
- **The tier-system scope choice (AC6) is decided by the document's own text, not by preference.**
  `quality-tiers.md` states that coverage thresholds are uniform across tiers **[VERIFIED]**, so
  authoring the classification would change no coverage gate. The cost-benefit is determinate, not a
  matter of taste.
- **Branch coverage (D3) follows the same recorded decision** — "line-only, no branch gate"
  **[VERIFIED]** — rather than requiring a new one.

**Residual confirmatory maintainer note — explicitly non-blocking.** A short note to the maintainer
is appropriate on landing, recording that 80/90 was restored, that the exemption was retained, that
branch coverage is reported and not gated, that `CLAUDE.md` § UT2 is now the single authority, and
that the tier claims were removed. This note is **informational and does not gate execution, review,
or merge**. It exists so the maintainer can object deliberately rather than discover the change
later.

**What would make it blocking.** The note becomes a blocking prerequisite if any one of the
following becomes true:

1. **The D1 execution-time `git log` gate is falsified** — history shows the 85/75 reintroduction was
   an explicit maintainer reconciliation that also adjudicated `CLAUDE.md`. Two competing recorded
   decisions then exist and an agent may not adjudicate between them. **Executor halts.**
2. **The decision would revoke or narrow the COM/VSTO/WinForms exemption**, or would approve any new
   `[ExcludeFromCodeCoverage]` boundary. `CLAUDE.md:303` reserves that authority to the maintainer,
   and the #227 denial shows the reservation is live **[RESEARCH]**.
3. **The reconciled number would be lower than a number this repository actually adopted by
   decision** — as opposed to lower than a number that arrived by import. That would be a genuine
   lowering under the epic NFR and requires a maintainer decision.
4. **The decision would make a gate Blocking against a measurement that has not met D5's
   reproducibility tolerance.** Declaring assemblies failing on the basis of an irreproducible
   number is a policy act, not an engineering one.

None of these four conditions holds on the evidence available at specification time. Condition 1 is
the only one that is testable rather than settled, and D9 sequences that test before any edit.

---

## Traceability

This is the pre-scope-correction traceability record. It is retained for research provenance only;
the User-Authorized Scope Correction replaces its local Claude-runtime change directives.

| AC | Decisions | Concrete changes | Verification |
|---|---|---|---|
| **AC1** — one set of thresholds across `CLAUDE.md` § UT2, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, no numeric disagreement | D1, D3, D4 | In-scope files 1, 2, 3. `CLAUDE.md` § UT2 states 80/90 and the branch disposition; the other two documents state no numeral and cite the authority | Authority-consistency Pester test (Test Strategy case 7): no coverage numeral outside the authority. Numeric disagreement is impossible when only one document carries a numeral |
| **AC2** — exclusion/exemption policy stated once, in the authority, with the testable-denominator exemption and the "no production file may be excluded" clause reconciled | D2, D4 | In-scope files 1 and 2. `CLAUDE.md` § UT2 carries the single reconciled denominator rule; `.claude/rules/general-unit-test.md` § "Coverage Exclusion Policy" is removed and replaced by a citation | Feature review confirms exactly one denominator/exclusion statement exists across the three documents and that it names the mechanism-independent rule from D2 |
| **AC3** — documents name the authority; non-authoritative documents cite rather than restate | D4 | In-scope files 1, 2, 3. Authority declaration and conflict-resolution rule written into `CLAUDE.md` § UT2 itself, not inherited from the existing precedence list or the non-auto-loaded `policy-compliance-order` skill | Authority-consistency Pester test (case 7). The conflict-resolution rule is readable in `CLAUDE.md` § UT2 and states that a divergence is resolved by rule and does not trigger a halt |
| **AC4** — tooling enforces the thresholds; a deliberately introduced regression fails the gate; negative-path proof under `evidence/regression-testing/` | D8, D1, D3, D5 | In-scope files 4, 5, 6, 7. New gate (pure function plus wrapper), committed producer, fail-closed artifact handling, hook constants aligned | Two-case proof: Case A (below-threshold input) **and** Case B (absent artifact). Transcript captured to `evidence/regression-testing/` with `EXIT_CODE:` and both cases named individually |
| **AC5** — the #424/#230 precedent ratified or superseded, in writing, in the authority | D5 | In-scope file 1. `CLAUDE.md` § UT2 gains the ratified split: change-scoped gates blocking; repository-wide floor reported-and-tracked with the stated reproducibility tolerance and exit condition | The written rule is present in `CLAUDE.md` § UT2, is labelled a ratification, states the tolerance numerically, and states the condition under which the repo-wide floor becomes blocking |
| **AC6** — `quality-tiers.yml` / `tier-classification` / `docs/ci.research.md` claims resolved at **every** site carrying them; no governance document asserts an absent file | D6 | In-scope files 3, **3a**, **3b**. In `quality-tiers.md`: lines 9, 20-21 and the non-existent project examples at 13-16 removed. In `.claude/rules/general-code-change.md`: the single `quality-tiers.yml` sentence deleted. In `.agents/skills/quality-tiers/SKILL.md`: the three false artifact claims removed | Grep across all three AC6 sites returns zero hits for `quality-tiers.yml`, `tier-classification`, and `ci.research`. Every T1-T4 reference left dangling by the removals (`.claude/rules/architecture-boundaries.md`, `.claude/rules/powershell.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`) carries an explicit recorded disposition. **No residual gap:** the earlier FU-A deferral of `general-code-change.md:29` is withdrawn |
| **AC7** — governing numbers **validated** against coverage re-measured post-#441/#478 and post-#457, captured before any number is written, and treated as an execution-time input rather than a hard-coded figure | D9, D5, D1 | Evidence artifacts under `evidence/baseline/`: three coverage runs, the spread artifact, the recorded function signatures, and the D1 `git log` verification | Task ordering: the re-measurement tasks precede every governance-document edit task in the plan. No plan task writes a figure captured during preparation; every figure is captured at execution time and cited by artifact path. **Binding nuance:** the re-measurement validates and contextualises D1 and identifies which assemblies fail; it does **not** select the number (D9 reasoning point 3). A re-measurement that contradicts D1 routes to Risk 2 — halt and escalate — never to a silently re-tuned threshold |
| **AC8** — `.claude/hooks/validate-feature-review-coverage.ps1` internally consistent, and its constants equal the reconciled thresholds | D8, D1, D3 | In-scope file 4. `.SYNOPSIS` prose, the `85.0` literal, the `$BranchFloor = 75.0` literal, and both message strings brought into agreement; floor extracted to a named constant citing the authority; the branch path removed per D3; the line/branch asymmetry resolved | Grep confirms exactly one line-floor numeral in the file, in a named constant, equal to the D1 number, and that `.SYNOPSIS` states the same number. Pester test asserts the constant equals the authority's number |
| **AC9** — Pester tests at the mirrored path, deterministic, no temporary files | D8 | In-scope file 7 | Tests placed at the mirror of their subject (restatement recorded in Test Strategy). Determinism verified by repeated runs. Grep confirms no temp-file API in any added test |
| **AC10** — out-of-scope threshold sites enumerated with a recorded disposition | D7, D6 | Files and Sites § "Out-of-scope sites and their dispositions" — 23 rows, each with exactly one disposition | Every site in the research inventory appears in the table, including the three sites `issue.md` missed (`.claude/agents/feature-review.md`, `.github/instructions/general-unit-test.instructions.md`, the `.agents/` divergences) and one site the research inventory did not flag for AC6 (`.claude/rules/general-code-change.md:29`). FU-A must be filed through the MCP promotion lifecycle **before merge** |

---

## Active Scope-Corrected Traceability

| AC | TaskMaster delivery | Verification |
|---|---|---|
| AC1 | Retain the existing upstream prompt as the local deliverable; defer application outside TaskMaster. | Prompt-deliverable validation and final scope validation show the prompt is present and no local `CLAUDE.md` or `.claude/**` path changed. |
| AC2 | Retain the prompt's upstream exclusion-policy requirement; no local Claude-runtime edit. | Prompt-deliverable validation. |
| AC3 | Retain the prompt's upstream authority-source requirement; no local Claude-runtime edit. | Prompt-deliverable validation. |
| AC4 | Add the permitted Cobertura threshold evaluator and runner invocation with deterministic Pester coverage. | Fail-before and pass-after evidence, plus final per-file coverage evidence. |
| AC5 | Retain the prompt's upstream precedent-disposition requirement; no local Claude-runtime edit. | Prompt-deliverable validation. |
| AC6 | Retain the prompt's false-claim-resolution requirement; no local Claude or Codex runtime-policy edit. | Prompt-deliverable and final scope validation. |
| AC7 | Reuse only schema-valid corrected-arithmetic evidence as contextual input. | Reused-baseline validation records the evidence and its no-threshold-selection restriction. |
| AC8 | Retain the prompt's upstream hook-reconciliation requirement; no local hook edit. | Prompt-deliverable and final scope validation. |
| AC9 | Add deterministic Pester cases alongside the TaskMaster coverage helper. | Pester pass-after and final per-file coverage evidence. |
| AC10 | Retain the prompt's future-affected-path and upstream-disposition requirement; no protected runtime-policy edit. | Prompt-deliverable and final scope validation. |

## Acceptance Criteria

These are the active, scope-corrected acceptance criteria for the `full-bug` feature. Do not
check an item until its individual TaskMaster evidence is verified. The deferred criteria are
satisfied locally only by validating the existing upstream prompt; they do not require, request,
or prove external execution.

- [x] AC1 — The existing upstream prompt is retained as the complete TaskMaster deliverable for
      reconciling coverage thresholds in the upstream source of truth; no TaskMaster `CLAUDE.md`
      or `.claude/**` file is changed, and future application is deferred outside TaskMaster.
- [x] AC2 — The existing upstream prompt explicitly requires the upstream owner to reconcile the
      coverage exclusion/exemption policy; TaskMaster records that deferred requirement without
      editing any local Claude-runtime path.
- [x] AC3 — The existing upstream prompt explicitly requires one authoritative upstream coverage
      policy source and non-conflicting generated references; TaskMaster records that deferred
      requirement without editing any local Claude-runtime path.
- [x] AC4 — TaskMaster coverage tooling rejects a valid synthetic Cobertura result below 80%,
      accepts the exact 80% boundary, and has deterministic negative-path evidence under
      `evidence/regression-testing/`; upstream Claude-hook reconciliation remains deferred.
- [x] AC5 — The existing upstream prompt carries the requirement to ratify or supersede the
      #424/#230 precedent in the authoritative upstream policy; TaskMaster records that deferred
      requirement without editing any local Claude-runtime path.
- [x] AC6 — The existing upstream prompt carries the requirement to resolve the false
      `quality-tiers.yml`, `tier-classification`, and `docs/ci.research.md` claims at their
      upstream-owned runtime sites; TaskMaster records that deferred requirement without editing
      `.claude/**` or `.agents/skills/**`.
- [x] AC7 — Corrected-arithmetic remeasurement evidence is retained and validated as an
      execution-time input before the local threshold gate is implemented; it does not select or
      lower a threshold.
- [x] AC8 — The existing upstream prompt carries the requirement to reconcile the upstream
      feature-review coverage hook's documentation and behavior; TaskMaster records that deferred
      requirement without editing `.claude/hooks/**`.
- [x] AC9 — Added Pester tests mirror their TaskMaster coverage-tooling subjects, are deterministic,
      and create no temporary files.
- [x] AC10 — The existing upstream prompt identifies the future affected TaskMaster paths and
      requires upstream coverage-site disposition; TaskMaster records the deferral without editing
      the protected Claude or Codex runtime policy surfaces.

---

## Rollout and Follow-up

### Follow-up issues to be filed through the MCP promotion lifecycle **before this feature merges**

Prose in a feature folder does not survive the merge. Each of the following must exist as a real
issue.

- **FU-A — Convert all remaining coverage-numeral sites to citations of `CLAUDE.md` § UT2.** Covers
  rows 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12 (numerals only), 13, 14, 15, 16, 17, 18 and 22 of the
  out-of-scope table. Must explicitly include: `.claude/agents/feature-review.md` (the
  self-contradicting live agent definition, highest priority), `.claude/rules/powershell.md` (the
  only deferred site whose numerals are both live and wrong after this feature lands), the three
  divergent `.agents/` files, and `.github/instructions/general-unit-test.instructions.md`.
  **Rows 20 and the tier-claim half of row 12 are no longer FU-A scope**: the AC6 widening of
  2026-08-10T16-10 brings `.claude/rules/general-code-change.md:29` and the false tier claims in
  `.agents/skills/quality-tiers/SKILL.md` into this feature. FU-A retains only the 85/75 coverage
  numerals in `.agents/skills/quality-tiers/SKILL.md`.
- **FU-B — Coverage measurement determinism.** File if and only if D9's three-run spread exceeds
  D5's tolerance. Scope: non-deterministic assembly instrumentation in the `dotnet-coverage`
  collection path, which neither #441/#478 nor #457 addresses. This issue is the named blocker on
  the repository-wide floor becoming a Blocking gate.
- **FU-C — Remove or relocate `scripts/temp-extract-coverage.ps1`**, a committed throwaway script
  whose default output path targets a feature folder that no longer exists under `active/`.

### Candidate follow-ups, not required before merge

- Revisit whether the COM/VSTO/WinForms exemption should be narrowed or replaced by the
  refactor-don't-exclude posture. This is a maintainer decision under `CLAUDE.md:303` and a
  substantial programme of work; D2 defers it deliberately.
- Remediation paths for assemblies below the D1 floor, enumerated from D9's re-measurement. A
  related open entry already exists at
  `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`.
- Whether to retain the T1-T4 taxonomy at all once its false enforcement claims are removed.

### Notes for the epic orchestrator

- The epic charter (`epic.md:124-126`) states that 512 edits `CLAUDE.md` lines "185-206 and
  381-401". The range `381-401` encloses `## Tone Policy` (lines 390-395), which belongs to neither
  feature **[RESEARCH]**. The 512 plan should cite regions 377-386 and 397-402 separately and
  exclude 388-396. This is a note for the epic, not a blocker for #494.
- Epic #136's twenty-one unmerged branches gate on per-file rates computed by the defective
  arithmetic (`epic.md:201-209`). Their evidence will not reproduce after this epic lands. That
  sequencing decision sits with the epic, not with #494.

### Links

- Issue: https://github.com/drmoisan/TaskMaster/issues/494
- Epic: `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md`
- Research: `research/2026-08-10T15-40-coverage-threshold-policy-reconciliation-research.md`
- Promoted from: `docs/features/potential/promoted/2026-08-07-conflicting-coverage-thresholds-across-policy-docs.md`

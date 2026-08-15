# Epic Audit: build-ci-coverage-gate-fidelity

- Epic folder: `docs/features/epics/build-ci-coverage-gate-fidelity/`
- Worktree: `c:\Users\DanMoisan\repos\TaskMaster-wt\epic-build-ci-coverage-gate-fidelity`
- Branch: `epic/build-ci-coverage-gate-fidelity-integration`
- Head verified: `22b5de02325331b0dcd660222dba33a1f1b66450` (confirmed against
  `.git/refs/heads/epic/build-ci-coverage-gate-fidelity-integration` in the shared repository, since
  this worktree's `.git` is a pointer file to
  `C:\Users\DanMoisan\repos\TaskMaster\.git\worktrees\epic-build-ci-coverage-gate-fidelity`)
- Audit timestamp: 2026-08-15T09-30
- Stage: fan-in — all five child features merged into the integration branch; final
  integration-to-`main` PR not yet opened (confirmed: `epic-status.md`'s Integration PR table has all
  fields at `—`).

## Tooling caveat (read this before the findings below)

This session's available tool set did not include a shell/`git`/`gh` CLI invocation tool, only
file-read, grep, and glob primitives. Section 3 below therefore verifies git reality by reading the
worktree's git plumbing directly (`refs/heads/*`, the integration branch's `logs/HEAD` reflog, and
`.git/COMMIT_EDITMSG`) rather than by running `git log`, `git branch --merged`, or `gh pr view` as
the task requested. This is a materially weaker verification method than a live `gh pr view
--json state,mergedAt,mergeCommit` call: it can confirm which commits exist and their ancestry as
recorded in the reflog, and it can cross-check contemporaneous agent-memory records, but it cannot
independently query GitHub's PR state, merged-at timestamp, or authoritative merge-commit field for
a PR. Findings in Section 3 are qualified accordingly; where a discrepancy is flagged, the
recommended remediation is an authoritative `gh pr view` re-check by an agent/session with CLI
access.

---

## 1. `epic.md` Frontmatter Manifest

**Verified well-formed.**

- `epic: build-ci-coverage-gate-fidelity`, `integration_branch:
  epic/build-ci-coverage-gate-fidelity-integration`, `created_at: 2026-08-10T14-05` — all present,
  correctly typed.
- `intent` block present with all four required children: `epic_type`,
  `business_outcome_hypothesis`, `leading_indicators` (4 entries), `nfrs` (2 entries).
- `features[]` has five entries, each carrying `issue_num`, `feature_folder`, `depends_on`:

  | issue_num | feature_folder | depends_on |
  |---|---|---|
  | 441 | `2026-08-10-cobertura-coverage-arithmetic-441` | `[]` |
  | 457 | `2026-08-10-excludefromcodecoverage-nested-lambdas-457` | `[441]` |
  | 512 | `2026-08-10-csharp-toolchain-gate-fidelity-512` | `[]` |
  | 494 | `2026-08-10-coverage-threshold-policy-reconciliation-494` | `[457]` |
  | 394 | `2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394` | `[]` |

- **`depends_on` resolution:** both non-empty entries (`457 -> [441]`, `494 -> [457]`) resolve to
  another `features[]` entry by `issue_num`. No dangling reference.
- **Cycle check:** the dependency graph is a simple chain `441 -> 457 -> 494` plus two isolated
  nodes (`512`, `394`). No cycle is possible with three or fewer edges in a chain shape; confirmed
  by inspection.

## 2. Wave Layering

Formula: `wave(f) = 0` when `depends_on` is empty, else `1 + max(wave(d))`.

- `wave(441) = 0` (empty `depends_on`)
- `wave(512) = 0` (empty `depends_on`)
- `wave(394) = 0` (empty `depends_on`)
- `wave(457) = 1 + wave(441) = 1`
- `wave(494) = 1 + wave(457) = 1 + 1 = 2`

This matches both the expected layering in the task (441/512/394 in wave 0, 457 in wave 1, 494 in
wave 2) and what `epic.md` § Wave Assignment and `epic-status.md` § Waves both state. **No
discrepancy.**

## 3. `epic-status.md` Fidelity Against Git Reality

### 3.1 Per-row merge verification

| Feature | Claimed PR | Claimed merge commit | Verification method | Result |
|---|---|---|---|---|
| 441 | #538 | `fb257cd6` | Reflog/commit-graph inspection only (no direct `gh pr view`) | Not independently confirmed this session; no contradicting evidence found. `git status` context at session start showed unrelated recent commits; the reflog's local "Merge prepared feature" trial-merge commits for 441 (`27be6ec5`) are pre-`fetch/reset` artifacts and are expected to differ from the real PR merge SHA — this is normal, not a defect (see 3.2). |
| 512 | #540 | `22eaee84` | Same as above, plus contemporaneous agent-memory corroboration | Corroborated: `.claude/agent-memory/orchestrator/poshqc-test-drops-coverage-xml-at-repo-root.md` and `project_claudemd_nullable_command_diverges_from_ci.md` both independently reference "issue #512 / PR #540" as the merge that landed the corrected toolchain commands, written contemporaneously by a different agent session than the one that produced `epic-status.md`. No contradiction found. |
| 394 | #533 | `c1fe3565` | Reflog/commit-graph inspection only | Not independently confirmed this session. Local trial-merge commit `868b4385` for the same branch appears earlier in the reflog and is superseded by a `reset: moving to origin` a few steps later (normal trial-merge-then-reset pattern, see 3.2). No contradicting evidence found. |
| 457 | #542 | `ee082ba1` | Reflog/commit-graph inspection **plus** a found discrepancy | **See 3.3 — flagged discrepancy.** |
| 494 | #551 | `85ff0c34` | Directly confirmed via reflog | **Confirmed.** The integration-branch worktree's reflog (`.git/worktrees/epic-build-ci-coverage-gate-fidelity/logs/HEAD`) records: `d863a5c3712776eee81bbf811e45523f13a380cb 85ff0c34a60b13e0399fc7a6c8e4f9ded7e397f0 ... pull --tags origin epic/build-ci-coverage-gate-fidelity-integration: Fast-forward`. The exact SHA `85ff0c34` that `epic-status.md` claims as PR #551's merge commit is the literal fast-forward landing point pulled from `origin`, immediately after which the epic orchestrator merged `main` into the integration branch and made further commits (see 3.4). |

### 3.2 Local trial-merge commits are not a discrepancy

The reflog shows a repeating pattern: a local commit titled `Merge prepared feature: <name>` or
`merge <branch>: Merge made by the 'ort' strategy` (e.g., `27be6ec5` for 441, `5447e802` for 512,
`868b4385` for 394, `abe7d02b` for 457), followed later by a `reset: moving to origin/...` entry
that lands the branch on a different SHA. This is consistent with an orchestrator workflow that
performs a local verification merge before the real GitHub PR is opened/merged, then fetches and
resets to the authoritative `origin` state once the real PR lands. The local trial-merge SHAs are
therefore expected to differ from the PR merge SHAs recorded in `epic-status.md`, and their
divergence is not evidence of a misstatement.

### 3.3 Flagged discrepancy — feature 457's recorded PR/merge commit is superseded by a later PR from the same branch

The integration branch's actual commit history (visible both in this session's initial `git status`
"Recent commits" listing and in the worktree reflog) contains, in chronological order:

1. `ee082ba1c078c5f2721b3ef5306172047daa1a09` — commit message `Merge pull request #542 from
   drmoisan/bug/excludefromcodecoverage-nested-lambdas-457`. This is what `epic-status.md` records
   for feature 457.
2. `ce94f227` — `chore(memory): record two orchestrator lessons from the #457 child run` (a
   memory-only commit made directly to the integration branch, not from the feature branch).
3. `8d0d1fec03012b52af46724275e43adc5c850e57` — commit message `Merge pull request #543 from
   drmoisan/bug/excludefromcodecoverage-nested-lambdas-457`. **A second PR merge from the same
   source branch, landing after PR #542.**
4. `c7d398c2aa0da6963de239ff6719b4b23a7d3f45` — `docs(epic): 457 merged, wave 1 complete, wave 2
   opened for 494`. This is the epic orchestrator's own "457 merged" declaration commit, and its
   reflog entry (`8d0d1fec... c7d398c2... commit: docs(epic): 457 merged...`) shows it was authored
   directly on top of the PR #543 merge commit, not the PR #542 merge commit.

`epic-status.md`'s row for `2026-08-10-excludefromcodecoverage-nested-lambdas-457` cites only PR
#542 / `ee082ba1` and makes no mention of PR #543 / `8d0d1fec`. Corroborating evidence:

- The commit-message text for #543 names the identical branch
  (`bug/excludefromcodecoverage-nested-lambdas-457`), so it is the same feature, not an unrelated PR
  that happens to share a number range.
- `.git/COMMIT_EDITMSG` (read directly; a leftover artifact, not necessarily live) shows an
  interactive-rebase pick list that includes, in order, `0105e71c # fix(coverage): exclude nested
  lambdas...`, `63a3ff17 # docs(457): record feature review artifacts and promote two review
  findings`, `ce94f227 # chore(memory): record two orchestrator lessons from the #457 child run`,
  `c7d398c2 # docs(epic): 457 merged...`. This is consistent with PR #543 carrying the feature's own
  review-artifact commit (`63a3ff17`, matching the feature-audit/code-review/policy-audit files
  dated `2026-08-11T01-33` already present in the child folder) rather than a second substantive
  code change — but this could not be confirmed with certainty without a live `gh pr view 543`.
- A contemporaneous orchestrator memory record
  (`.claude/agent-memory/orchestrator/project_457_coverage_moved_up_not_down.md`, written before
  PR #543 apparently existed) states "Issue #457 ... merged as PR #542, merge commit `ee082ba1`" —
  consistent with `epic-status.md`, but written before the later PR #543 merge landed, so it does
  not settle whether #542 or #543 is authoritative for the tree's final state.

**Recommendation:** before opening the final integration PR, the epic orchestrator should run
`gh pr view 542 --json state,mergedAt,mergeCommit` and `gh pr view 543 --json
state,mergedAt,mergeCommit` to determine (a) whether both PRs merged, (b) what #543 actually
contains, and (c) update `epic-status.md`'s `pr_url`/`merge_commit_sha` for feature 457 to name
whichever PR's merge commit is the true ancestor of the current integration-branch tip. On current
evidence this is most likely PR #543 (it is chronologically later and directly precedes the "457
merged" declaration), which would make the `epic-status.md` row understate/misstate the correct PR
number, though not the substance of what merged.

### 3.4 `epic-status.md`'s own "Last updated" timestamp is stale relative to its own table content

`epic-status.md` line 11 states `Last updated: 2026-08-14T22-55`. Its own Features table records,
for feature 494, `merge_confirmed_at: 2026-08-15T02-27` — a timestamp roughly 3.5 hours **later**
than the document's self-reported freshness marker. This is an internal inconsistency: a document
cannot have been "last updated" at a point in time before a data value it currently contains was
produced.

Reflog corroboration: after the `85ff0c34` fast-forward (3.1 above), the reflog records three more
commits on the integration branch: a merge of `main` (`fb8eff9b`), a commit titled `@` (`56d7a6b4`
— an apparently placeholder/empty commit message, worth a maintainer's attention on its own), and a
commit **explicitly titled** `docs(epic): refresh epic-status projection from git and gh`
(`6c0a0e84`), followed by the current tip `22b5de02` (`chore(457): promote the three AC15 residual
potential entries to issues`). A commit that names itself as refreshing the epic-status projection
exists in the history, yet the on-disk `epic-status.md` still shows the older `Last updated`
marker. Either that refresh commit updated a different revision of the file that was later
superseded without updating the header, or the header field is not being maintained in step with
the table body. This does not misstate which features are merged, but it does mean the document's
own freshness claim cannot be trusted at face value.

### 3.5 Everything else in `epic-status.md` matches available evidence

- The `Waves` table matches the `depends_on`-derived layering (Section 2).
- The `Integration PR` table's all-`—` fields correctly reflect that the final PR has not been
  opened.
- The "Verification Note — CI Coverage of Child PRs" and "Integrated-Tree CI Gate" sections'
  narrative (child PRs against the integration branch are ineligible for `ci.yml`'s `pull_request`
  trigger, so a separate `workflow_dispatch` gate was used) is internally consistent with
  `.github/workflows/ci.yml`'s trigger list, which is not disputed elsewhere in this audit. The
  claimed green run `31493339489` at head `c7d398c2` could not be independently re-queried against
  the GitHub Actions API in this session (no CLI tool); this is recorded as unverified rather than
  confirmed.
- The "Dispositioned: Main-Inherited Intermittent Test" section's four-sample table is internally
  coherent (failure originates pre-epic on `main`, epic-child code does not touch the affected
  test file) and is consistent with the general-unit-test.md determinism rules being cited
  correctly (real wall-clock wait, no `FakeTimeProvider`) — this is a defect description, not a
  status claim, so it carries no separate fidelity risk.

---

## 4. Child Feature Folder Artifact Completeness and Acceptance-Criteria Fidelity

All five children carry the `- Work Mode: full-bug` marker in `issue.md`, so per
`acceptance-criteria-tracking`, `spec.md` is each feature's sole authoritative AC source (not
`issue.md`, and `user-story.md` is narrative-only where present).

| Feature | `spec.md` | `user-story.md` | Plan | Review artifacts | AC count | Checked |
|---|---|---|---|---|---|---|
| 441 | present | present (justified: "included because the epic planner ... explicitly requested one") | `plan.2026-08-10T14-07.md` | code-review, feature-audit, policy-audit (2026-08-10T23-35) | 20 | 20/20 `[x]` |
| 512 | present | absent | `plan.2026-08-10T14-08.md` | code-review, feature-audit, policy-audit (2026-08-10T23-51) | 13 | 13/13 `[x]` |
| 457 | present | absent | `plan.2026-08-10T14-08.md` | code-review, feature-audit, policy-audit (2026-08-11T01-33) | 16 | 16/16 `[x]` |
| 494 | present | present (no explicit epic-planner-request justification found, unlike 441's) | `plan.2026-08-10T14-10.md`, plus two remediation plans | five code-review/feature-audit/policy-audit cycles through 2026-08-13T18-06 | 10 (current, scope-corrected) | 10/10 `[x]` |
| 394 | present | present (justified: "included because the epic planner ... explicitly requested one") | `plan.2026-08-10T14-09.md` | code-review, feature-audit, policy-audit (2 rounds, incl. remediation) | 8 | 8/8 `[x]` |

Minor, non-blocking note: 494's `user-story.md` presence does not carry the same explicit
epic-planner-request justification line that 394's and 441's do (`feature-promotion-lifecycle`
states a `full-bug` user story "should be absent unless the requirements explicitly justify it" —
guidance, not a hard gate). Not treated as a finding on its own.

### 4.1 Spot-checked AC/evidence pairs — no premature or missing check-offs found in the sampled set

- **441 AC-20** ("follow-ups filed"): evidence file
  `evidence/issue-updates/followups-441.2026-08-10T23-25.md` originally recorded `POSTING BLOCKED`
  and explicitly states "AC-20 is left UNCHECKED" at 2026-08-10T23-25 because the executing
  session's MCP tool surface lacked the promotion tools. A superseding `RESOLUTION` section
  (2026-08-10T23-50), written by the orchestrator session which did have the promotion tools, records
  four issues filed (`#529`–`#532`), and `evidence/other/ac-status-summary.2026-08-10T23-30.md`
  documents the supersession transparently (original state retained, dated resolution appended).
  **This is a properly audited, legitimately resolved check-off, not a violation** — worth
  recording because it is exactly the failure mode the task asked me to hunt for, and in this case
  the trail is honest.
- **441 AC-1/AC-2** (generator parity, pre/post): independently re-derivable from
  `evidence/qa-gates/postchange-generator-parity.2026-08-10T23-15.md`, which shows the corrected
  post-processor reproducing the Cobertura generator's own root attributes exactly (`lines-valid
  79957` etc., versus the pre-change run's doubled `161086`). Matches the checked-off state.
- **457 AC "repository baseline re-captured"**: `feature-audit.2026-08-11T01-33.md` records a
  measured before/after table (`lines-valid 62873 -> 62401`, `line-rate 0.853514 -> 0.855355`)
  cross-checked against two independent evidence artifacts. Matches the checked-off state.
- **457 AC15** ("three residuals ... recorded and handed off as follow-up references"): at review
  time (2026-08-11T01-33) the feature-audit explicitly notes the three potential-entry files existed
  on disk but had **not yet** been promoted to GitHub issues, and records this as "a tracked
  follow-through, not a gap in this branch," owed by the epic orchestrator at epic close. This audit
  confirms the follow-through was in fact completed: the current HEAD commit (`22b5de02`,
  `chore(457): promote the three AC15 residual potential entries to issues`) exists, and all three
  entries now live under `docs/features/potential/promoted/` (e.g.,
  `2026-08-11-exempt-async-member-lambdas-remain-counted.md`, now Issue #558) rather than
  `docs/features/potential/`. **Confirmed discharged, not merely claimed.**
- **512 AC4** (negative-path proof): both a preparation-phase feasibility run
  (`evidence/regression-testing/negative-path-proof-dry-run.2026-08-10T15-20.md`) and a plan-execution
  QA-gate artifact (`qa-gates/typecheck-negative-control.2026-08-10T23-58.md`, referenced from
  `completion-attestation.2026-08-11T01-08.md`) independently demonstrate `EXIT_CODE: 1` /
  `error CS8603` on a deliberately introduced nullable violation, with the perturbation reverted and
  verified. Matches the checked-off state. Production code confirms the corresponding documented
  command exists verbatim in the current `CLAUDE.md` and `.claude/rules/csharp.md`.
- **494 AC4** (Cobertura threshold gate): production code confirms
  `function Assert-CoberturaLineCoverageThreshold` exists at
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:459`, and
  `evidence/regression-testing/threshold-gate-fail-before.2026-08-13T15-51.md` /
  `threshold-gate-pass-after.2026-08-13T16-00.md` show 6 failing / 51 passing before and after,
  respectively. Matches the checked-off state.
- **394**: production `.csproj` confirms exactly one
  `<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />` entry remains. Matches
  the checked-off state.

### 4.2 Substantive finding — feature 494's actual delivery no longer matches `epic.md`'s stated scope for it

`epic.md` § Scope lists feature `coverage-threshold-policy-reconciliation-494`'s "Primary surface"
as `CLAUDE.md § UT2, .claude/rules/general-unit-test.md, .claude/rules/quality-tiers.md`, and §
Execution Authorization Required explicitly pre-authorizes this feature to edit exactly those three
governance documents.

The feature's **current, merged** `spec.md` (the version now checked off 10/10) instead states, in
its "Active Scope-Corrected Traceability" section and its restated Acceptance Criteria, that a
**user-directed scope correction** replaced the original governance-document edits with: (a) a local
Cobertura line-coverage threshold evaluator (`Assert-CoberturaLineCoverageThreshold`, enforcing an
80% floor) plus deterministic Pester tests, and (b) an "upstream prompt" document
(`evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md`) that defers the
actual reconciliation of `CLAUDE.md`, `.claude/rules/general-unit-test.md`, and
`.claude/rules/quality-tiers.md` to a **separate upstream repository**, explicitly stating "Do not
apply the requested `CLAUDE.md` or `.claude/**` changes directly in the TaskMaster repository."

The remediation plan (`remediation-plan.2026-08-13T16-24.md`) enforces this as a hard constraint:
"`CLAUDE.md`, every non-memory `.claude/**` path ... `.agents/skills/**` ... are prohibited," with
only six pre-existing, unrelated agent-memory files carved out as exceptions. Multiple review
artifacts independently confirm the resulting diff touches none of `CLAUDE.md`,
`.claude/rules/general-unit-test.md`, or `.claude/rules/quality-tiers.md` (e.g., code-review
2026-08-13T18-06: "exact range lists only six listed agent-memory paths and shows no prohibited
runtime ... path").

**Consequence, confirmed directly against the current file contents in this worktree:**
`CLAUDE.md` § UT2 still states "Repository-wide line coverage must remain `>= 80%`" with a
tier-independent COM/VSTO exemption model, while `.claude/rules/general-unit-test.md` states "Line
coverage must remain >= 85% ... Branch coverage must remain >= 75% across all tiers (T1–T4) ...
Tier-specific lower coverage thresholds are not used in this repository," and
`.claude/rules/quality-tiers.md` independently states the same 85%/75% uniform figures with an
explicit "Rationale (uniform coverage thresholds)" section. **These two pairs of numbers (80% vs.
85%/75%) still directly contradict each other on the merged integration-branch tree**, and the newly
added tooling (`Assert-CoberturaLineCoverageThreshold`) enforces the lower, older 80% figure — not
the 85%/75% figures the other two documents assert.

This is exactly the class of defect `epic.md`'s own Goal section names ("the documented gate
commands do not execute what they claim" / governance documents "currently contradict each other on
both thresholds") for the toolchain-and-threshold half of the epic, and it remains unresolved for
this specific triple of documents after feature 494 merged. It is not visible from
`epic-status.md`, which records feature 494 simply as `merged` with no note of the scope reduction,
nor from `epic.md`, which was never updated to reflect that its own Scope table and Execution
Authorization section for 494 describe work that was ultimately not performed in this repository.

This finding is scoped narrowly: it does not allege that 494's own (reduced) acceptance criteria are
falsely checked off — they are honestly worded around the deferral and are supported by evidence,
as confirmed in 4.1. The finding is that **the epic-level manifest (`epic.md`) and status projection
(`epic-status.md`) are stale with respect to what feature 494 actually delivered**, and that **one
of the four leading indicators is only partially discharged** (see Section 5, indicator 2).

---

## 5. Leading Indicators — Discharge Assessment

`epic.md`'s `intent.leading_indicators`:

1. **"A deliberately introduced nullable violation fails the documented type-check gate."**
   **Discharged**, by feature `csharp-toolchain-gate-fidelity-512`. Direct evidence:
   `evidence/regression-testing/negative-path-proof-dry-run.2026-08-10T15-20.md` and
   `evidence/qa-gates/typecheck-negative-control.2026-08-10T23-58.md` both show the documented
   command (`msbuild ... /t:Rebuild ... /p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`)
   passing on a clean tree and failing with `CS8603` on a deliberately introduced nullable violation
   in an opted-in file, with the perturbation reverted. The documented command in the current
   `CLAUDE.md` and `.claude/rules/csharp.md` matches what was proven.

2. **"A deliberately introduced coverage regression fails the documented coverage gate."**
   **Partially discharged**, by feature `coverage-threshold-policy-reconciliation-494`. The new
   `Assert-CoberturaLineCoverageThreshold` function mechanically fails a synthetic below-threshold
   Cobertura result and passes at the exact boundary (evidence: fail-before 6 failing / pass-after
   51 passing, 2026-08-13). Mechanically, the indicator's literal claim holds. However, the
   threshold this gate enforces (80%) is not the threshold two of the three governance documents
   this feature was chartered to reconcile currently assert (85% line / 75% branch in
   `general-unit-test.md` and `quality-tiers.md`) — see 4.2. The indicator is discharged for "does a
   regression fail," not for "does the gate enforce the reconciled, single, agreed-on figure,"
   because that reconciliation was deferred upstream and out of this repository's scope.

3. **"Reported repository-wide lines-valid equals the distinct-line count, not twice it."**
   **Discharged**, by feature `cobertura-coverage-arithmetic-441`. Direct evidence:
   `evidence/qa-gates/postchange-generator-parity.2026-08-10T23-15.md` shows the corrected
   post-processor reproducing the underlying `dotnet-coverage` document's own root attributes
   exactly (`lines-valid=79957`, matching ground truth), versus the pre-change run's `161086` (almost
   exactly double).

4. **"The documented C# toolchain commands execute successfully against a clean main."**
   **Discharged**, by feature `csharp-toolchain-gate-fidelity-512`. Direct evidence:
   `evidence/baseline/baseline-ci-parity-on-main.2026-08-10T15-05.md` shows, via `gh run list`/`gh
   api` output captured at plan time, that all three of the corrected documented steps (formatting,
   analyzer build, nullable-warnings build) passed on `main`'s tip (`a682c7a2`); the one failing step
   on that run (`Run MSTest suite with coverage`) is explicitly out of this feature's scope and
   attributed to a different, sibling concern. This evidence predates the current HEAD and was not
   re-verified against the present `main` tip in this session (no CLI access), but it directly
   supports the claim as stated for the commands this feature corrected.

---

## Summary of Discrepancies (as requested)

**Between `epic-status.md` and git/gh reality:**

1. **Feature 457 row cites only PR #542 (`ee082ba1`).** The integration branch's own commit history
   contains a second, later merge from the identical source branch — PR #543 (`8d0d1fec`) —
   landing immediately before the epic orchestrator's own "457 merged" declaration commit
   (`c7d398c2`). `epic-status.md` does not mention PR #543. Recommend a `gh pr view 542`/`gh pr view
   543` re-check before opening the final integration PR, and updating the row if #543 is the true
   ancestor.
2. **`epic-status.md`'s "Last updated: 2026-08-14T22-55" predates data in its own table**
   (feature 494's `merge_confirmed_at: 2026-08-15T02-27`), and predates a later commit on the branch
   explicitly titled "refresh epic-status projection from git and gh" (`6c0a0e84`). The freshness
   marker is not synchronized with the file's own content.
3. (Not a discrepancy, but a related staleness issue.) **`epic.md`'s Scope and Execution
   Authorization sections still describe feature 494 as editing `CLAUDE.md`, `general-unit-test.md`,
   and `quality-tiers.md`.** It does not; a user-directed scope correction deferred that work
   upstream. `epic.md` was not updated to reflect this, and `epic-status.md` records feature 494
   simply as `merged` with no note of the reduced scope.

**Acceptance criteria whose check-off state is not supported by evidence:** none found in the
sampled set (Section 4.1 and 4.2). The one item that momentarily looked like a violation — 441's
AC-20, and 457's AC15 — both turned out, on reading the full evidence trail, to be honestly and
verifiably resolved rather than prematurely checked. No AC was found checked `[x]` with contradicted
or absent evidence, and no AC was found left `[ ]` despite present evidence, across the ACs sampled
in Section 4.1. This audit did not re-verify all 67 AC items across the five features line-by-line;
it sampled the highest-risk items (fail-before/pass-after claims, follow-up-filing claims, and the
four items that map directly to the epic's leading indicators).

**Substantive scope finding, not an AC-fidelity finding:** feature 494's merged content diverges
from `epic.md`'s stated scope for it (Section 4.2), and this leaves the coverage-threshold
governance-document contradiction (80% in `CLAUDE.md` vs. 85%/75% in
`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`) unresolved on the current
tree, despite feature 494 being merged. This should be resolved or explicitly re-scoped as a
follow-on item before the epic is presented as fully discharging its stated goal.

# Policy Compliance Audit — ci-parallel-job-split (Issue #553), Re-Audit (Cycle 2)

- **Component:** GitHub Actions CI pipeline (`.github/workflows/`) + branch-protection ruleset migration
- **Date:** 2026-08-14 (artifact timestamp 2026-08-14T17-12)
- **Reviewer:** feature-review agent (cycle 2; cycle-1 artifacts at `*.2026-08-14T10-21.md`)
- **Base branch:** `main` (resolved `origin/main`, fetched fresh)
- **Merge base:** `35e02895c29c5b65302f7d921431082c9ae6ce09` — **recomputed**. The caller-supplied merge base `2073f717` is stale: the branch was rebased onto `main` after PR #552 merged. All caller-cited SHAs (`d83bf377`, probe pairs `5a606895`/`072e19ca`, `fc4f2be6`/`9415ad31`, `a55ccdfc`/`ad28ea81`) belong to the pre-rebase lineage; the current-lineage equivalents were identified and audited instead.
- **Branch head:** `feature/ci-parallel-job-split-553` @ `9c00e37a79657505266dc47c514f136d9cebf1bc` (matches remote head via `git ls-remote`)
- **PR context:** refreshed by this review at 2026-08-14T21:12Z against the true merge base (prior artifacts recorded pre-rebase head `0b016c81` — stale)
- **Work mode:** `full-feature`; AC sources: `spec.md` and `user-story.md`
- **Files under audit:** full branch diff vs true merge base — 65 files (7 workflow-directory files, 49 feature-folder docs/evidence, 2 archival promoted-potential copies, 9 agent-memory files). Zero files in any coverage-bearing language.

## Executive Summary

Cycle-1 blocking finding **B1 (`modified-workflow-needs-green-run`) is RESOLVED** — but not by the run the coordinator cited. Run `31814562839` @ `d83bf377` is green (verified: `workflow_dispatch`, all five jobs success) but `d83bf377` is not the current branch head and, after the rebase, not even an ancestor of it, so it does not meet the rule's definition ("a workflow run whose head SHA matches the current branch head"). This review resolved the gap factually: it dispatched run **[31840944277](https://github.com/drmoisan/TaskMaster/actions/runs/31840944277)** at the exact current head `9c00e37a` (`gh workflow run ci.yml --ref feature/ci-parallel-job-split-553`), watched it to completion, and recorded **conclusion success with all five jobs success** (job conclusions verified via the runs API). The rule explicitly blesses `workflow_dispatch` runs, so B1 is satisfied at the current head with no interpretive stretch.

All other cycle-1 findings and the new-since-cycle-1 work verify cleanly:

- **Probe fault isolation: PASS** — each of the three probe runs turned exactly its own gate red (per-job conclusions independently pulled from the runs API for runs `31810574239`, `31811211865`, `31811867381`); all three probe/revert pairs on the current lineage (`26b9f7b5`/`6f73cf43`, `5dba9206`/`d8da0a07`, `d372c5e9`/`1437ea09`) net to a byte-empty diff, and the branch diff at the true merge base contains zero C# files — no probe artifact survives.
- **Ruleset migration: PASS** — evidence triple verified (pre-PUT GET, writable-fields-only PUT payload, independent post-PUT GET), and this review issued its **own live GET** of ruleset 18572843: five contexts exactly matching the verbatim check-run names, `strict_required_status_checks_policy: true` retained, all four rule types preserved, `enforcement: active`.
- **Cycle-1 Minor findings resolved:** F2 (README context-name form corrected, five verbatim context strings now listed) and F3 (`baseline.provenance.json` + `post-split-timing.provenance.json` added with `runner_class`, `host_signature`, `workflow_run_url`).
- **Timing outlier classification: adequately supported** (section 7.6).

**Blocking finding count: 0.** Residual non-blocking observations: the repository is currently in the documented over-blocking window (ruleset migrated before the feature PR was opened), and plan Phase 6 checkboxes lag the already-executed migration. Verdict: PASS.

No caller scope narrowing was detected. The caller's factual notes contained stale lineage identifiers (recorded above and corrected), which is a data-staleness issue, not a scope-narrowing attempt; the audit scope is the full branch diff vs the recomputed base.

## 1. General Unit Test Policy Compliance

No test files changed in the net branch diff. The test-failure probe (`d372c5e9`) temporarily modified `UtilitiesCS.Test/Extensions/ExtToChar_Tests.cs` and was fully reverted (`1437ea09`; pair diff empty). The MSTest suite's CI execution semantics are unchanged (vstest block byte-identical, re-confirmed in cycle 1).

### 1.1 Changed-language enumeration (full branch diff at true merge base)

Enumeration command: `git diff --name-only 35e02895..9c00e37a` filtered per extension. Result: **zero** changed files for C# (`.cs`/`.csproj`/`.props`/`.targets`), PowerShell (`.ps1`/`.psm1`/`.psd1`), Python (`.py`), and TypeScript (`.ts`/`.tsx`). Probe commits net out; the executor's own `no-csharp-diff.2026-08-14T11-14.md` reached the same result on the pre-rebase lineage, and this review re-established it on the post-rebase lineage.

## 2. General Code Change Policy Compliance

Unchanged from cycle 1 (the workflow files are byte-identical between the cycle-1-audited tree and HEAD — verified `git diff d83bf377 HEAD -- .github/` is empty, and the cycle-1 byte-identity verification of gate blocks vs the pre-split `ci.yml` therefore still holds):

| Check | Verdict | Evidence |
| --- | --- | --- |
| 500-line file limit | PASS | Workflow files 29–96 lines; README 195 lines (Markdown exempt); executor `file-size-audit.2026-08-14T11-14.md` concurs |
| Fail fast / error handling | PASS | Guards preserved verbatim (cycle-1 independent verification stands) |
| No new dependencies | PASS | No new actions introduced since cycle 1 |
| Supporting docs updated | PASS | README updated with verbatim context strings; provenance files added |
| Policy documents modified | PASS (none) | Diff touches no `.claude/rules/` or `.github/instructions/` file |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 C# Code Change Policy

Zero net C# changes (section 1.1). The three probe commits deliberately introduced C# violations and were each perfectly reverted in the immediately following commit; no toolchain obligation attaches to a net-zero diff. The probes themselves served as negative-path verification of the CI gates, not as retained code.

### 3.2 `.claude/rules/ci-workflows.md`

Unchanged workflow content since the cycle-1 assessment: no `pwsh` step uses the deliberately-failing nested-command pattern. The probes were probe **commits** (reverted), not workflow steps, exactly as spec.md's seeded-test-conditions note requires — no deliberately-failing nested command entered the committed pipeline. Executor's `lastexitcode-review.2026-08-14T11-14.md` concurs. **Verdict: PASS.**

### 3.3 `.claude/rules/benchmark-baselines.md`

Cycle-1 verdict stands (rule's rejection conditions scoped to baselines consumed by a benchmark regression gate; runner parity satisfied). Since cycle 1, sibling provenance files were added anyway: `evidence/baseline/baseline.provenance.json` and `evidence/qa-gates/post-split-timing.provenance.json`, both carrying `runner_class`, `host_signature`, and `workflow_run_url` plus collection commands and per-sample records. Cycle-1 Minor F3 is resolved beyond what the rule requires for this artifact class. **Verdict: PASS.**

## 4. Language-Specific Unit Test Policy Compliance

Not triggered — no net test-file changes in any language.

## 5. Test Coverage Detail

Per section 1.1, the branch diff at the true merge base contains **zero changed files in every coverage-bearing language**, verified directly against `git diff --name-only 35e02895..9c00e37a` (and cross-checked against the probe-pair reverts). No per-language coverage row, artifact, or threshold applies to this branch:

- C# — 0 changed files net; no C# coverage measurement required for this branch.
- PowerShell — 0 changed `.ps1`/`.psm1` files; no PowerShell coverage measurement required.
- Python — 0 changed files; no Python coverage measurement required.
- TypeScript — 0 changed files; no TypeScript coverage measurement required.

The CI coverage-producing step and the `test-results` artifact upload remain byte-identical to pre-split; run `31840944277` uploaded the artifact per the pipeline definition, and the executor's `test-results-artifact.2026-08-14T11-10.md` verified name/paths parity on the post-probe green run.

## 6. Test Execution Metrics

Runner-executed verification (all conclusions pulled by this review from the GitHub API, not taken from committed evidence):

| Run | Head | Event | Conclusion | Purpose |
| --- | --- | --- | --- | --- |
| 31809697953 | `0b016c81` (pre-rebase) | workflow_dispatch | success (5/5) | First green run of the split pipeline; tailored-setup assumption held (fallback not required) |
| 31810574239 | `5a606895` (pre-rebase) | workflow_dispatch | failure | Format probe: **only** `format-check / Verify formatting` failed; other four success |
| 31811211865 | `fc4f2be6` (pre-rebase) | workflow_dispatch | failure | Nullable probe: **only** `build-nullable / ...` failed; other four success |
| 31811867381 | `a55ccdfc` (pre-rebase) | workflow_dispatch | failure | MSTest probe: **only** `mstest-coverage / ...` failed; other four success |
| 31812508684 | `ad28ea81` (pre-rebase) | workflow_dispatch | success (5/5) | Post-probe green; timing measurement of record (433s) |
| 31813885124 | `df49d208` (pre-rebase) | workflow_dispatch | success (5/5) | Pre-migration green; context-name reference; timing sample C (245s) |
| 31814562839 | `d83bf377` (pre-rebase) | workflow_dispatch | success (5/5) | Most recent pre-rebase green; the run the coordinator cited |
| **31840944277** | **`9c00e37a` (current head)** | workflow_dispatch | **success (5/5)** | **Dispatched by this review; satisfies `modified-workflow-needs-green-run` at the current head** (wall clock 296s, 21:06:18Z–21:11:14Z — a fourth timing sample, within the observed variance band) |

## 7. Code Quality Checks

### 7.1 Head and lineage verification

- `git rev-parse HEAD` = `9c00e37a...` = remote head (`git ls-remote origin`). PR context refreshed accordingly.
- `git merge-base HEAD origin/main` = `35e02895` (PR #552 merge). The caller-supplied `2073f717` would have pulled main-side C# changes (`UtilitiesCS/Threading/TimeOutTask.cs`, test files, `.claude/rules/csharp.md`) into the audit scope that are not part of this feature; the recomputed base excludes them correctly.
- `d83bf377` (coordinator's "green-run head"): `git merge-base --is-ancestor` returns false — not in the current lineage. However, `git diff d83bf377 HEAD -- .github/` is **empty**: the workflow content that ran green pre-rebase is byte-identical to HEAD's. This supported the decision to resolve B1 by re-dispatch rather than by remediation loop.

### 7.2 Probe integrity (current lineage)

For each pair, `git diff <probe>~1 <revert>` is empty (verified for `26b9f7b5`/`6f73cf43` — formatting violation in `UtilitiesCS/EmailIntelligence/IntelligenceFilters.cs`; `5dba9206`/`d8da0a07` — nullable violation, same file; `d372c5e9`/`1437ea09` — test assertion in `UtilitiesCS.Test/Extensions/ExtToChar_Tests.cs`). Combined with the zero-C#-file net diff, no probe artifact survives in the tree.

### 7.3 `modified-workflow-needs-green-run` — B1 resolution

- Trigger: still fires (diff matches `.github/workflows/**`).
- Rule text: satisfied by "a workflow run whose head SHA matches the current branch head and whose conclusion is success"; "A green `workflow_dispatch` run against the branch head also satisfies the rule, not only a PR-context run."
- Adjudication: the coordinator-cited run (`31814562839` @ `d83bf377`) does **not** satisfy the rule for head `9c00e37a` (head mismatch; non-ancestor after rebase). Rather than dispositioning around the mismatch, this review dispatched run `31840944277` at `9c00e37a` and watched it to success (5/5 jobs). The rule is satisfied at the current head by its explicit `workflow_dispatch` provision. **Verdict: PASS. B1 closed.**
- Forward note: any further commits (including the commit that will land these review artifacts) advance the head again. The migrated ruleset makes this safe — the five required contexts must report green on the PR head before any merge, so a green run at the final merge head is structurally guaranteed (fail-closed).

### 7.4 Ruleset migration audit

Evidence at `evidence/other/ruleset-migration/` (`ruleset-pre.json`, `ruleset-new.json`, `ruleset-post.json`, `ruleset-migration.2026-08-14T15-58.md`):

- **Payload correctness:** `ruleset-new.json` contains only the six writable fields (`name`, `target`, `enforcement`, `bypass_actors`, `conditions`, `rules`); all eight read-only GET fields are absent (verified by inspection). Relative to the pre-PUT object's writable projection, only the required-contexts array differs; `deletion`, `non_fast_forward`, and `pull_request` rules and their parameters are preserved verbatim; `strict_required_status_checks_policy: true` and `do_not_enforce_on_create: false` retained.
- **Atomicity:** single PUT of the full writable object; no remove-then-add window.
- **Context strings:** the five payload contexts are verbatim equal to the check-run names this review pulled from the runs API (`actionlint / actionlint`, `format-check / Verify formatting`, `build-analyzers / Build with analyzers and code style enforcement`, `build-nullable / Build with nullable warnings treated as errors`, `mstest-coverage / Run MSTest suite with coverage`) — captured live, not assumed.
- **Independent corroboration:** this review's own `GET repos/drmoisan/TaskMaster/rulesets/18572843` returned the same five contexts, `strict: true`, `enforcement: active`, `updated_at 2026-08-14T17:00:07.992-04:00`, matching `ruleset-post.json` on every material field. The committed post-PUT evidence is authentic against live state.
- **Authorization:** the migration record states explicit repository-owner authorization for the orchestrator-confirmation-gated task [P6-T3]. This is a committed, PR-visible statement (not confined to gitignored state).
- **Under-gating:** none. Before the PUT the old contexts over-blocked; the swap was atomic; after the PUT all five gates are required and all five report. **Verdict: PASS.**

### 7.5 Cycle-1 finding dispositions

| Cycle-1 finding | Status | Evidence |
| --- | --- | --- |
| B1 (Blocking) green run | **Resolved** | Run 31840944277 @ current head, success 5/5 (section 7.3) |
| F2 (Minor) README `CI / <gate>` wording | **Resolved** | README caveat 2 now uses `<caller job> / <callee job>`; the five verbatim context strings and the `<caller job id> / <callee job name>` form rule are now listed explicitly |
| F3 (Minor) baseline provenance sibling | **Resolved** | `baseline.provenance.json` and `post-split-timing.provenance.json` present with `runner_class`, `host_signature`, `workflow_run_url` |
| F4 (Info) actionlint checksum | Unchanged, accepted | Deliberately not fixable within byte-identity AC; follow-up candidate |
| F5 (Info) spurious autoclose tokens | Superseded | This review's refreshed summary lists only #553 |

### 7.6 Timing evidence and the outlier classification

`ci-split-timing-comparison.2026-08-14T11-10.md` + addendum report all three samples (259s, 433s, 245s vs 444s baseline). Assessment of the run-B (433s, `31812508684`) outlier classification:

- The step-level table shows compute-bound steps scaled uniformly (~1.6x build, ~1.6x test, 5.3x NuGet restore) while fixed-cost steps were flat — a signature consistent with a slow runner instance/contended I/O, not workflow structure (the YAML is byte-identical across all samples; confirmed by lineage diff).
- Queueing is excluded on evidence: all five jobs started within seconds of each other in every sample.
- Two of three samples cluster at 245–259s; the addendum retains run B as the measurement of record (per plan designation) rather than discarding it, and gates no numeric threshold on the figures. My dispatched run adds a fourth de-facto sample at 296s, between the cluster and the outlier, consistent with the hosted-runner-variance explanation.
- **Assessment: the outlier classification is adequately supported**, and the reporting is conservative (unfavourable sample retained as measurement of record; median reported separately).

### 7.7 Evidence Location Compliance

`git diff --name-only 35e02895..HEAD | grep -E '^artifacts/(baselines|qa|evidence|coverage)/'` returns zero files. All new evidence (ruleset-migration, probes, timing, provenance) lives under the canonical `docs/features/active/2026-08-14-ci-parallel-job-split-553/evidence/<kind>/` tree. **Verdict: PASS.** No evidence-location overrides were requested or rejected.

## 8. Gaps and Exceptions

1. **Stale caller inputs (documented, corrected):** supplied merge base and all cited SHAs were pre-rebase. Recomputed base `35e02895`; current-lineage probe pairs audited; B1 resolved at the actual head. No scope was narrowed by this correction — it widened precision, not reduced coverage.
2. **Open over-blocking window (non-blocking risk):** the ruleset now requires the five new contexts, but the feature PR is not yet open (plan P3-T3 pending). Until this branch merges, every other PR to `main` is blocked (their heads run the old pipeline and cannot report the new contexts). This is the documented fail-closed state; the README instructs keeping the interval short. Recommendation: open and merge the PR promptly (code review observation R2).
3. **Plan bookkeeping lags execution (non-blocking):** the ruleset migration ([P6-T1..T4]) is executed and evidenced, but the plan checkboxes remain unchecked and the evidence filenames deviate from the plan-specified names (`evidence/other/ruleset-migration/ruleset-pre.json` vs planned `evidence/other/ruleset-pre-put.<TS>.json`, etc.). Substance over form: the artifacts carry the required content. The executor should reconcile plan checkboxes at P7 (code review observation R3).
4. **MCP template/validator tooling unavailable in-session** (same as cycle 1): headings reproduced from `policy-audit-template-usage` prose; documented assumption.
5. **Referenced validator scripts absent from repository** (same as cycle 1): green-run and evidence-location checks performed manually with `git`/`gh` equivalents.
6. **PR-context collector unavailable in-session:** summary and appendix hand-authored at 2026-08-14T21:12Z from `git diff --numstat` against the recomputed base, per the documented fallback.

## 9. Summary of Changes (since cycle-1 audit)

- Three probe/revert commit pairs demonstrating per-gate fault isolation (all net-zero; runner-verified one-red-gate-per-probe).
- Phase 3–5 evidence: first green run, tailored-setup confirmation (assumption held; fallback not required), post-probe green, timing comparison + addendum (3 samples), test-results artifact parity, final actionlint, `$LASTEXITCODE` review, file-size audit, no-C#-diff check, check-run-name capture, pre-migration green.
- Cycle-1 review artifacts and AC check-offs committed; README context-form fix; two provenance JSONs.
- Branch-protection ruleset migration executed under explicit owner authorization, with pre/payload/post evidence.
- Branch rebased onto `main` @ `35e02895` (PR #552); ruleset-migration evidence committed on the rebased lineage.

## 10. Compliance Verdict

**PASS — zero blocking findings.** Cycle-1 B1 is resolved by a green `workflow_dispatch` run at the exact current head (dispatched and verified by this review). The ruleset migration is atomic, evidence-complete, and corroborated against live state. Probe fault isolation is empirically proven and fully reverted. All cycle-1 Minor findings are resolved. Remaining pre-merge work is procedural: open the PR (P3-T3), let the five required contexts report on the PR head, merge promptly to close the over-blocking window, then run the post-merge dispatch smoke (P7-T1) and reconcile plan/DoD checkboxes (P7-T2..T9).

## Appendix A: Test Inventory

No net test-file changes. The deliberate-test-failure probe modified one assertion in `UtilitiesCS.Test/Extensions/ExtToChar_Tests.cs` and was reverted byte-exactly. CI executes the pre-existing MSTest suite with the unchanged vstest invocation; run `31840944277` executed it green at the current head.

## Appendix B: Toolchain Commands Reference

Commands executed by this review (cycle 2):

1. `git fetch origin main`; `git rev-parse HEAD`; `git merge-base HEAD origin/main`; `git ls-remote origin refs/heads/feature/ci-parallel-job-split-553` — head/base/lineage verification.
2. `git log --oneline <old-base>..HEAD`; `git merge-base --is-ancestor d83bf377 HEAD`; `git diff d83bf377 HEAD -- .github/` — rebase detection and workflow-content identity across lineages.
3. `git diff --name-status|--name-only|--numstat 35e02895..HEAD` (with language-extension filters and evidence-location filter) — true-scope enumeration; zero coverage-language files; zero prohibited evidence paths.
4. `git diff <probe>~1 <revert>` for the three probe pairs — net-zero revert verification.
5. `gh run view 31814562839 --json ...`; `gh run list --branch ...`; `gh api .../runs/{31810574239,31811211865,31811867381,31840944277}/jobs` — run/job conclusion verification, fault-isolation corroboration.
6. `gh api repos/drmoisan/TaskMaster/rulesets/18572843` — independent live ruleset GET.
7. `gh workflow run ci.yml --ref feature/ci-parallel-job-split-553`; `gh run watch 31840944277 --exit-status` — reviewer-initiated green-run resolution of B1 at the current head.
8. PR-context refresh: hand-authored `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` from `git diff` against the recomputed base.

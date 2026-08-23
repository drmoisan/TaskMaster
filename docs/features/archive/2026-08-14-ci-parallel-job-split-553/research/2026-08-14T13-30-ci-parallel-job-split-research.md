# Research — CI Parallel Job Split (Issue #553)

- **Issue:** #553
- **Feature folder:** `docs/features/active/2026-08-14-ci-parallel-job-split-553/`
- **Date:** 2026-08-14T13-30
- **Author:** task-researcher agent
- **Status:** Complete

## Input Verification and Gaps

All findings below are grounded in files read this session or in cited external documentation.

| Input | Status |
| --- | --- |
| `.github/workflows/ci.yml` | Read. 2 jobs: `actionlint` (ubuntu-latest) and `quality-gates` named `Format, build, analyze, and test` (windows-latest). |
| `evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md` | Read. Treated as authoritative measured data. |
| `docs/features/potential/promoted/2026-08-14-ci-parallel-job-split.md` | **GAP: file does not exist** at the stated path (verified by glob of `docs/features/potential/**`). The promoted content is present in the feature folder at `issue.md` (whose status line reads "Promoted"), which was used as the substitute source for problem statement, draft acceptance criteria, and constraints. |
| `.claude/rules/ci-workflows.md` | Read. |
| `.claude/rules/benchmark-baselines.md` | In session context. |
| `.claude/skills/orchestrate/SKILL.md` § GitHub Actions Reusable Workflows | Read (line 159). |
| `.github/workflows/README.md` | **GAP: does not exist.** `.github/workflows/` contains only `ci.yml` and `codex-web-setup-test.yml`. The skill section references this README; acceptance criteria require creating it. |
| `TaskMaster.sln` | Read. **Contains 18 projects, not 19** as stated in the delegation and baseline prose (counted: Tags, ToDoModel, TaskVisualization, UtilitiesCS, QuickFiler, TaskTree, TaskMaster, SVGControl, VBFunctions, plus 9 `*.Test` projects: ToDoModel.Test, UtilitiesCS.Test, QuickFiler.Test, TaskVisualization.Test, Tags.Test, TaskTree.Test, SVGControl.Test, VBFunctions.Test, TaskMaster.Test). The discrepancy is not load-bearing for the design. Legacy `packages.config` / .NET Framework solution built with `msbuild`, confirmed. |
| Nesting-depth note | The skill records a reusable-workflow nesting cap of 4. Current GitHub documentation states "a maximum of ten levels of workflows — that is, the top-level caller workflow and up to nine levels of reusable workflows" (docs.github.com, Reusing workflows, fetched 2026-08-14). The repo convention of one level satisfies both; the skill figure is stale relative to current docs but imposes the binding constraint here. |

## Current State Analysis

`ci.yml` (161 lines) defines:

- Workflow-level `concurrency: group: ci-${{ github.workflow }}-${{ github.event.pull_request.number || github.ref }}`, `cancel-in-progress: true`.
- `permissions: contents: read`.
- Job `quality-gates` (timeout 60 min) with steps, measured durations from the baseline in parentheses:
  - Setup: checkout (41s), setup-dotnet 10.0.x (35s), setup-msbuild (5s), setup-nuget (0s), cache `packages` keyed `nuget-<os>-hashFiles('**/packages.config')` (15s), `nuget restore` (11s), cache `~/.nuget/packages` keyed `dotnet-tools-<os>-hashFiles('dotnet-tools.json')` (16s), `dotnet tool restore` (5s). Fixed setup total: **130s**.
  - Gate 1 — `dotnet csharpier check .` (**15s**). CSharpier 1.2.6 is pinned in root-level `dotnet-tools.json`; it is a file-based formatter and does not consume restored NuGet packages.
  - Gate 2 — `msbuild /t:Build /m /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (**101s**), with explicit `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }`.
  - Gate 3 — `msbuild /t:Rebuild /m /p:TreatWarningsAsErrors=true` (**98s**), same exit guard. The in-file comment documents that `/t:Rebuild` is deliberate: MSBuild's incremental up-to-date check does not invalidate on a command-line property change alone.
  - Gate 4 — vswhere-located `vstest.console.exe` over recursively discovered `*.Test.dll` filtered to `\bin\Debug\`, excluding `\obj\` and `\ref\`, with `/EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` and a `throw` guard on zero discovered assemblies (**88s**).
  - Teardown: upload `TestResults/**/*.trx` + `*.coverage` as artifact `test-results` (2s), post-cache/post-checkout (~10s).
- Baseline wall clock: **444s** (7m24s); gate work 302s strictly serial; setup 130s; teardown ~12s.

Branch protection (verified fact supplied by the orchestrator via `gh api repos/drmoisan/TaskMaster/rulesets/18572843`): the `main` ruleset requires exactly two contexts, `actionlint` and `Format, build, analyze, and test`, with `strict_required_status_checks_policy: true`.

---

## Q1. Decomposition Topology

Three topologies evaluated. All estimates below are **estimates derived arithmetically from the measured baseline**, not measurements of a split pipeline. Assumption common to all: per-step durations are stable across runs of the same runner class (satisfies `.claude/rules/benchmark-baselines.md` runner parity, since baseline and future comparison are both GitHub-hosted `windows-latest`).

### (a) Four fully independent jobs, each with the full 130s setup

| Job | Estimate | Composition |
| --- | --- | --- |
| format | ~150s | 130 setup + 15 gate + ~5 teardown |
| analyzer build | ~241s | 130 + 101 + ~10 |
| nullable build | ~238s | 130 + 98 + ~10 |
| MSTest | ~333s | 130 + ~101 own build + 88 + ~14 |

Wall clock ≈ **333s (5m33s)**, bounded by the MSTest job. Billed `windows-latest` ≈ 962s ≈ **16.0 min** (vs 7.4 baseline), ~2.2x before the Windows 2x billing multiplier. Assumption: the MSTest job's own build takes about as long as the analyzer build (101s); a plain build without analyzers is plausibly faster, but no measurement exists.

### (b) Build-once, upload `bin` output, downstream test job downloads

Chain: build job (130 setup + 101 build + upload T_u) → `needs:` → test job (~45s minimal setup + download T_d + 88 test + ~14).

Wall clock ≈ 245 + T_u + T_d + 147 ≈ **392s + transfer time even if transfer were free would be ~392s**, i.e. worse than topology (a)'s 333s before any transfer cost is added. The serial `needs:` edge means the test path pays build-job setup + build + test **in sequence**, which is structurally the same critical path as topology (a)'s test job plus artifact transfer overhead. Artifact sharing cannot beat own-build on wall clock here (see Q3 for the cost analysis and fragility). Rejected.

### (c) Hybrid (recommended): four independent jobs with per-job tailored setup; test job performs its own plain build

Setup is trimmed to what each job actually consumes:

- **format**: checkout (41) + setup-dotnet (35) + dotnet-tools cache (16) + `dotnet tool restore` (5). Drops setup-msbuild, setup-nuget, packages cache, and `nuget restore` (CSharpier does not consume NuGet packages). Estimated ~119s total (≈98 setup + 15 gate + ~5 teardown).
- **analyzer / nullable / MSTest**: checkout (41) + setup-msbuild (5) + setup-nuget (0) + packages cache (15) + `nuget restore` (11) ≈ 73s setup. Drops setup-dotnet, dotnet-tools cache, and `dotnet tool restore`. **Assumption requiring verification in the first green run:** nothing in the msbuild build path depends on the pinned .NET 10 SDK; a legacy `packages.config` msbuild build should not, but this is unverified.

| Job | Estimate |
| --- | --- |
| format | ~119s |
| analyzer build | ~185s (73 + 101 + ~10) |
| nullable build | ~182s (73 + 98 + ~10) |
| MSTest | ~277s (73 + ~101 build + 88 + ~14) |

Wall clock ≈ **277s (4m37s)**, an estimated **~38% latency reduction** vs 444s. Billed `windows-latest` ≈ 763s ≈ **12.7 min** (~1.7x baseline). If the tailored-setup assumption fails and full setup is needed everywhere, topology (c) degrades gracefully to topology (a)'s figures (~333s wall, ~16 min billed, ~25% reduction) — still an improvement.

**Recommendation: topology (c).** Rejected alternatives: (a) is (c) without the setup trim — strictly dominated; (b) is rejected on critical-path arithmetic plus the fragility documented in Q3.

A further optional optimization — moving the format gate to `ubuntu-latest` (CSharpier is cross-platform; Linux minutes bill at 1x vs Windows 2x) — is flagged but **not recommended for this change**: it introduces a platform-parity question (line endings, tool behavior) that should not be bundled with the topology split. Record as a potential follow-up.

## Q2. The Two-Full-Compiles Question

**Concurrent on separate runners: yes, trivially safe.** Each job checks out its own workspace on its own runner; there is no shared mutable state. The `/t:Rebuild` rationale (incremental up-to-date check not invalidating on property change) exists to defeat *within-runner* staleness from the preceding `/t:Build`; on a fresh runner there is no prior output, so `/t:Build` and `/t:Rebuild` are equivalent in cost and effect. **Keep `/t:Rebuild` anyway**: it preserves the documented in-file intent, costs nothing on a clean runner, and keeps the standalone `workflow_dispatch` path correct regardless of runner reuse assumptions. The explanatory comment must move with the step.

**Merging into one compile: technically feasible, but it would change enforcement semantics of both gates; do not merge.** A single `msbuild /t:Rebuild /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /p:TreatWarningsAsErrors=true` invocation would:

1. Promote analyzer and code-style **warnings** to errors. Today the analyzer gate fails only on errors and passes with warnings; the merged gate would either fail currently-green code or require `WarningsNotAsErrors` carve-outs that risk exempting nullable diagnostics — the carve-out path is a **weakening** of the nullable gate, and the no-carve-out path is an unratified **strengthening** of the analyzer gate. Either direction alters a gate.
2. Collapse the independent failure signal, which is one of the two stated objectives of issue #553 ("a red check does not identify which gate failed").
3. Halve the compile cost only on the billed-minutes axis (~98s saved) while the wall clock is already bounded by the MSTest job, so the merge buys no latency.

Conclusion: run the two compiles concurrently in separate jobs, unmodified.

## Q3. Artifact-Sharing Viability

Cost estimate for uploading/downloading `bin\Debug` output of this solution:

- No local build output exists in this worktree to measure (verified: glob of `*/bin/Debug/*.dll` returns nothing). The following is an **estimate**: a legacy .NET Framework solution with `packages.config` uses CopyLocal semantics, so each of the 9 test projects' `bin\Debug` contains a full copy of its dependency closure (Interop assemblies, Moq, FluentAssertions, MSTest adapter, Newtonsoft, project references, plus `.pdb` files required for coverage attribution). Plausible aggregate size: **0.5–2 GB uncompressed**, compressing to perhaps 150–600 MB.
- `actions/upload-artifact@v4` packs the artifact as a single compressed archive before upload (an improvement over v3's per-file chunking, so "many small assemblies" is less pathological than under v3, but compression of hundreds of MB of binaries still costs CPU time on the runner and the archive still transfers both ways). Estimated upload 30–120s, download 15–60s. These are estimates; variance on hosted runners is high.
- Critical-path arithmetic (Q1b) shows own-build wins **even at zero transfer cost**, because the `needs:` edge serializes build-job setup + compile ahead of the test job. Transfer cost only widens the gap.
- Fragility beyond latency: (i) coverage silently degrades if `.pdb` files are excluded to shrink the upload; (ii) any CopyLocal dependency missing from the upload glob surfaces as runtime assembly-load failures inside test runs, not as a clear infrastructure error; (iii) `upload-artifact@v4` strips the least common ancestor of the matched paths — if a future edit narrows the glob to one project, the preserved directory structure silently changes and the discovery filter breaks.

**Conclusion: per-job rebuild beats artifact sharing for this solution.** Evidence that would settle it definitively: one instrumented run that uploads `**/bin/Debug/**` and records archive size plus upload/download step durations. That experiment is not required to justify the recommendation, because the critical-path argument is independent of transfer speed.

## Q4. Test-Assembly Discovery

Under the recommended topology the test job builds in its own workspace, so the existing discovery block is **unchanged**: recurse `$env:GITHUB_WORKSPACE` for `*.Test.dll`, match `\bin\Debug\` (via `$env:BUILD_CONFIGURATION`), exclude `\obj\` and `\ref\`, `throw` on zero results. The zero-assembly `throw` guard must be preserved verbatim — it is the fail-closed protection against a discovery regression.

If a future change adopts artifact download instead, the required alterations and failure modes are:

- The filter root must become the `actions/download-artifact` destination path (or the artifact must be downloaded to the workspace root with structure preserved, in which case the existing filter continues to match — contingent on the upload glob's least-common-ancestor being the workspace root).
- Silent-break modes: (i) upload glob mismatch → zero assemblies downloaded → the `throw` guard fires (fail-closed, acceptable); (ii) a job that both builds *and* downloads could discover duplicate or stale assemblies — a split must never mix the two acquisition modes in one job; (iii) missing `.pdb`/dependency files degrade coverage or fail tests at runtime (see Q3).
- The known local hazard (recursive discovery picking up stale `.claude/worktrees` builds) does not apply on a fresh hosted runner, but the exclusion discipline should not be weakened, since the same script text is the reference for local runs.

## Q5. Caching Under Concurrency

Documented behavior (actions/cache README, fetched 2026-08-14): "if the provided `key` matches an existing cache, a new cache is not created", and caches are immutable ("You cannot change the contents of an existing cache"). Consequences for four concurrent jobs sharing the keys `nuget-<os>-<hash>` and `dotnet-tools-<os>-<hash>`:

- **Steady state (key exists):** all consuming jobs restore concurrently (reads are safe) and every post-job save is skipped. No race.
- **Cache-miss state (e.g., the PR edits `packages.config`):** each consuming job misses, falls back to the `restore-keys` prefix, runs its own `nuget restore` (~11s), and attempts to save the same new key at job end. Caches are immutable and creation is first-writer-wins; the losing jobs emit a reservation warning and skip the save. The precise race mechanics ("Unable to reserve cache" warning) are **not documented** in the README or docs pages fetched this session — they are known runtime behavior widely observed in the wild. What the documentation does establish is immutability, which guarantees no corruption. A lost save never fails the job.
- **Format job:** should **not** restore the NuGet packages cache and should not run `nuget restore` at all — CSharpier reads source text only. It needs only the `~/.nuget/packages` cache keyed on `dotnet-tools.json` for `dotnet tool restore`. Dropping the packages cache from this job removes ~26s of setup and one gratuitous concurrent reader.
- The `~/.nuget/packages` cache is needed only by the format job under the tailored-setup recommendation, which incidentally eliminates concurrent saves of that key entirely.

## Q6. Concurrency Group Behavior Across the Reusable-Workflow Boundary

Established, documented facts: jobs of a called workflow run as part of the **caller's** workflow run, and a workflow-level `concurrency` group governs the entire run. Therefore the existing declaration in `ci.yml` continues to cover all callee jobs after the split, with `cancel-in-progress: true` cancelling the whole run (all four gate jobs) when a newer run for the same PR/ref starts.

The behavior of a **workflow-level `concurrency` key declared inside a callee** when invoked via `workflow_call` is **not clearly documented**: the GitHub docs pages fetched this session (Using concurrency; Reusing workflows) do not specify it, and community reports of unexpected queueing/deadlock exist. Design consequence:

- **Declare the group in the caller (`ci.yml`) only, exactly as today.**
- **Declare no `concurrency` in any callee.** When a callee runs standalone via `workflow_dispatch`, it forms its own run with no concurrency group — acceptable for a manual dispatch path.
- Job-level `concurrency` on the caller's `uses:` jobs is supported but unnecessary here.

## Q7. Windows Runner Concurrency Ceiling

Per GitHub's Actions limits reference (fetched 2026-08-14): maximum concurrent jobs on standard GitHub-hosted runners are **Free 20, Pro 40, Team 60, Enterprise 500**; only macOS carries a separate lower cap (5 for Free/Pro/Team). There is **no separate Windows cap**. The split needs at most 5 concurrent jobs per run (4 × `windows-latest` + 1 × `ubuntu-latest`), which fits with margin under even the Free tier — provided other runs across the account are not consuming the budget simultaneously. The limit is account-wide; multiple concurrent PRs/pushes multiply the demand (e.g., 4 simultaneous runs × 5 jobs = 20 jobs, saturating a Free account and queueing the excess).

How to check: the account's plan is not readable from repository data. `gh api user --jq .plan.name` reports the authenticated user's plan for a personal account; the billing settings page is authoritative. Whether concurrent load from other repositories under the account matters is org/account-level operational data that cannot be determined from this repository — if that data is unavailable, the honest statement is: the per-run demand (5 jobs) is safely below every plan's ceiling, and the realized speedup could be bounded only by cross-run contention, which is unknowable from here.

## Q8. Required-Check Migration Sequencing

Key structural facts:

1. For `pull_request` events, the PR's own head-ref workflow files execute, so the split PR itself runs the **new** pipeline and reports the **new** contexts.
2. A required context that is never reported stays "expected" and **blocks** merging (fail-closed). Both orderings of PUT-vs-merge therefore over-block rather than under-gate.
3. For reusable workflows, the check-run context name takes the form `<caller job name> / <callee job name>`. The exact strings must be captured from a real run, not assumed. This is the single most likely source of a botched migration.
4. The only under-gating hazard is a PUT whose new contexts set omits a gate (for example, a two-step edit that removes `Format, build, analyze, and test` before adding the four replacements, leaving `actionlint` as the sole required check). The mitigation is a **single atomic PUT** that replaces the old set with the complete new set in one request. `PUT /repos/{owner}/{repo}/rulesets/{ruleset_id}` replaces the ruleset content, so the request must carry the full writable object (name, target, enforcement, bypass_actors, conditions, rules), not a partial patch.

Correct order of operations:

1. Open the split PR. Its run reports the four new contexts; it is blocked by the still-required old context (expected, fail-closed).
2. Confirm the run is green and capture the **exact** check-run names from the PR head: `gh api repos/drmoisan/TaskMaster/commits/<head-sha>/check-runs --jq '.check_runs[].name'`.
3. Fetch the current ruleset, construct the updated body, and apply it in one PUT:

   ```
   gh api repos/drmoisan/TaskMaster/rulesets/18572843 > ruleset-current.json
   # Build ruleset-new.json from the writable fields of ruleset-current.json
   # (name, target, enforcement, bypass_actors, conditions, rules), replacing the
   # required_status_checks rule's parameters.required_status_checks with:
   #   [{"context": "actionlint"}, {"context": "<new-1>"}, {"context": "<new-2>"},
   #    {"context": "<new-3>"}, {"context": "<new-4>"}]
   # and retaining strict_required_status_checks_policy: true.
   gh api --method PUT repos/drmoisan/TaskMaster/rulesets/18572843 --input ruleset-new.json
   ```

   Read-only fields returned by GET (`id`, `node_id`, `created_at`, `updated_at`, `_links`, `source`, `source_type`, `current_user_can_bypass`) must not be treated as part of the writable payload.
4. Immediately merge the split PR. `strict_required_status_checks_policy: true` requires the branch to be up to date with `main`; update/rebase first if needed so the green run is against the current base.
5. Verify by GET that the ruleset holds exactly the five intended contexts, and record the response as evidence.

Between steps 3 and 4, every *other* open PR (still running the old pipeline from its own head ref) reports the old context and lacks the new ones, so it is blocked until it updates its branch past the merged split — over-blocking, never under-gating. `strict` further guarantees any such PR must take the new `main` (and therefore, after updating, still runs its own head's workflow — note that a stale branch that merely merges in `main` acquires the new workflow files and reports the new contexts on its next run).

Rollback: a single PUT restoring the previous contexts set reverts the merge policy; reverting the workflow change is an ordinary revert PR.

## Q9. `$LASTEXITCODE` Hygiene Review

Per `.claude/rules/ci-workflows.md`, the mandatory pattern applies to steps that **intentionally invoke a failing nested command**. Review of every `pwsh` step that moves into a callee:

| Step | Last external command on success path | Verdict |
| --- | --- | --- |
| `nuget restore` | `nuget restore` (exit 0 on success) | Compliant; failure should propagate. No change. |
| `dotnet tool restore` | `dotnet tool restore` | Compliant. No change. |
| `dotnet csharpier check .` | The gate command itself | Compliant; a non-zero exit **is** the gate signal. No reset permitted. |
| Analyzer build | `msbuild` followed by explicit `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` | Compliant; retain the guard verbatim when relocating. |
| Nullable build | Same pattern | Compliant; retain, including the `/t:Rebuild` rationale comment. |
| vstest step | `vstest.console.exe`, then `throw` on non-zero; earlier external commands (`vswhere`) run before the gate | Compliant; on success the script ends after a passing `if`, leaving exit 0. Retain the zero-assembly `throw`. |

**No step in the current pipeline uses the deliberately-failing-nested-command pattern, so no `exit 0` / reset additions are required.** The rule becomes load-bearing only if the implementation adds negative-path self-validation steps (e.g., a step that asserts a gate catches a synthetic violation); any such step must reset `$LASTEXITCODE` or `exit 0` explicitly.

## Q10. Recommendation

**Topology (c): four independent reusable-workflow jobs with per-job tailored setup; no build-output artifact sharing; the MSTest job performs its own plain in-workspace build.**

Proposed shape (names illustrative; final required-context strings are captured from the first real run per Q8):

- `.github/workflows/_format-check.yml` — checkout, setup-dotnet, dotnet-tools cache, `dotnet tool restore`, `dotnet csharpier check .`.
- `.github/workflows/_build-analyzers.yml` — checkout, setup-msbuild, setup-nuget, packages cache, `nuget restore`, analyzer `/t:Build`.
- `.github/workflows/_build-nullable.yml` — same setup, nullable `/t:Rebuild` with its rationale comment.
- `.github/workflows/_mstest-coverage.yml` — same setup, plain `msbuild /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`, unchanged discovery + vstest step, unchanged `test-results` artifact upload (`if: always()`).
- Each callee declares `on: workflow_call:` **and** `on: workflow_dispatch:`, its own `permissions: contents: read`, a right-sized `timeout-minutes` (format ~10; builds ~30; test ~30), and **no** `concurrency`.
- `ci.yml` becomes the orchestrator: keeps `on:`, `permissions`, and the existing workflow-level `concurrency` block; contains only the `actionlint` job (which may itself be extracted to `_actionlint.yml` for convention consistency — note its required-context name would then also change and must be included in the ruleset PUT) and four `uses: ./.github/workflows/_<name>.yml` jobs with **no `needs:` edges** and no inline `steps:`.
- `.github/workflows/README.md` is created, documenting per-stage `workflow_dispatch` and the branch-protection rename procedure (Q8), closing the reference gap in `orchestrate/SKILL.md`.

Expected outcomes (estimates, to be verified against a measured post-split run per the acceptance criteria):

- **Latency:** ~277s wall clock vs 444s baseline (~38% reduction) if the tailored setup holds; worst case ~333s (~25% reduction) with full setup per job.
- **Billed cost:** `windows-latest` seconds rise from ~444 to ~763 (tailored) or ~962 (full setup), i.e. ~1.7–2.2x before GitHub's Windows 2x multiplier.
- **Independent failure signal and per-gate re-dispatch:** achieved by construction (four contexts; each callee dispatchable standalone).

Residual risks:

1. **Context-name mismatch in the ruleset PUT** — highest-likelihood failure; mitigated by capturing names from the live run before the PUT (Q8 step 2) and by fail-closed blocking if a name is wrong.
2. **Tailored-setup assumption** (msbuild jobs without setup-dotnet; format job without nuget restore) is unverified until the first green run; fallback is restoring the dropped steps at ~56s/job cost.
3. **Estimated timings are estimates**; hosted-runner variance (checkout and setup steps in particular) can shift per-job durations by tens of seconds.
4. **Cross-run account-level runner contention** could queue jobs and erode the realized speedup (Q7); unknowable from repository data.
5. **Undocumented callee-level concurrency semantics** are avoided rather than relied upon (Q6).

---

## Behavior Semantics

- **Success:** all five required contexts (`actionlint` + four gates) report success on the PR head; each gate's pass criterion is byte-identical to the corresponding step in the monolithic job.
- **Failure:** exactly the violated gate's context reports failure; the other gates complete independently (no `needs:` coupling), preserving full diagnostic signal per run.
- **Ordering:** none between gates. `cancel-in-progress` cancels all jobs of a superseded run together via the caller's group.
- **Edge cases:** zero discovered test assemblies → `throw` (fail-closed); `nuget restore` failure fails the three msbuild-consuming jobs independently; a cache-save race loses silently and harmlessly (Q5); a callee dispatched standalone runs without a concurrency group.

## Requirements Mapping (draft acceptance criteria → design)

| Acceptance criterion (issue.md) | Design element |
| --- | --- |
| Four gates as separate jobs, no serializing `needs:` | Four caller jobs, zero `needs:` edges (no artifact consumption exists to justify one) |
| Each gate a `_<name>.yml` with `workflow_call` + `workflow_dispatch` | Four callee files as specified |
| `ci.yml` orchestrator with no inline `steps:` | Requires also extracting `actionlint` into `_actionlint.yml`; if it remains inline, this criterion fails as written — the plan must either extract it (and include its possibly-renamed context in the PUT) or the criterion must be amended. Recommended: extract it. |
| Cross-job files via explicit artifacts | Only `test-results` upload remains (to workflow storage, not cross-job); no cross-job file sharing exists in the recommended topology |
| Ruleset contexts updated with no bypass window | Q8 single-PUT procedure |
| `.github/workflows/README.md` documents dispatch + rename procedure | New file (currently absent) |
| Green run vs branch head (`modified-workflow-needs-green-run`) | The split PR's own run; S9 CI-green gate |
| No gate dropped/weakened/made non-required | Gate commands byte-identical; Q2 forbids the compile merge; all four contexts required |

## Testing Implications

Workflow YAML has no unit-test harness in this repo; verification is by CI itself, consistent with existing practice:

- `actionlint` must pass over all new/modified workflow files (already a required check; runs in the same PR).
- Seeded negative-path conditions from the spec (deliberate formatting violation fails only the format gate; deliberate nullable violation fails only the nullable gate; deliberate test failure fails only the MSTest gate) are exercised as **temporary probe commits on the PR branch, then reverted** — not as permanent workflow steps, so no deliberately-failing nested commands enter the committed pipeline and no `$LASTEXITCODE` resets become necessary.
- Standalone `workflow_dispatch` of each callee, once merged, verifies the dispatch path.
- The post-split timing evidence is captured with the same `gh api .../runs/<id>/jobs` method as the baseline, satisfying runner-environment parity.
- No C# code changes; the C# toolchain loop applies only if the implementation unexpectedly touches `*.cs`/`*.csproj`.

## Automation Feasibility

Assessment of every step of the proposed change for unattended execution:

| Step | Unattended? | Basis |
| --- | --- | --- |
| Author 4–5 callee workflows + orchestrator `ci.yml` + README | Yes | File edits only |
| Local actionlint validation | Yes | Binary download + run, same as the CI job does |
| Commit, PR creation | Yes | Existing `pr-author` skill + `gh` flow |
| Observe green run, capture per-job timings and exact check-run names | Yes | `gh api` polling (S9 gate already automates this) |
| Branch-protection ruleset PUT | **Yes — confirmed sufficient** | The orchestrator verified the session token holds the `repo` scope and `repos/drmoisan/TaskMaster` reports `permissions.admin: true`. The REST endpoint `PUT /repos/{owner}/{repo}/rulesets/{ruleset_id}` requires repository admin permission, which a classic-token `repo` scope held by an admin satisfies. Caveats: (i) if the ambient credential were a fine-grained PAT it would instead need the "Administration" repository permission (write) — not the verified case here; (ii) the PUT is full-replace, so the automation must round-trip the writable fields (Q8 step 3) to avoid clobbering other rules in the ruleset. A pre-check `GET` plus a dry construction of the payload can be validated automatically (e.g., assert the new body differs from the old only in the contexts array) before the mutating call. |
| Merge the split PR | Yes | `gh pr merge` once the five new-set contexts are green and the branch is up to date (`strict` policy) |
| Post-merge verification (ruleset GET, dispatch smoke of each callee, timing evidence capture) | Yes | `gh api` / `gh workflow run` |
| Rollback (restore old contexts via PUT; revert PR) | Yes | Same mechanisms |

**No step technically requires a human.** The one step warranting deliberate handling rather than blind automation is the ruleset PUT: it is an outward-facing change to the repository's merge policy (the feature folder's issue.md says it "must be applied deliberately, in the correct order"). The Q8 sequencing plus the payload-diff assertion above constitute that deliberate procedure in automatable form; the orchestrator should record the pre-PUT ruleset JSON, the PUT payload, and the post-PUT GET response as evidence in the feature folder's `evidence/` tree. If the project maintainer wishes to reserve merge-policy mutations to a human, that is a policy choice, not a technical necessity — no such reservation is currently recorded in repo policy.

## Rejected Alternatives (summary)

- **Topology (a)** — four jobs with untrimmed full setup: strictly dominated by (c); serves as (c)'s fallback bound.
- **Topology (b)** — build-once + artifact sharing: loses on critical-path arithmetic even at zero transfer cost; adds `.pdb`/dependency-closure fragility and a silent path-structure hazard in `upload-artifact@v4`.
- **Merged analyzer+nullable compile:** alters enforcement semantics of both gates and defeats the independent-signal objective (Q2).
- **Format gate on `ubuntu-latest`:** deferred as a follow-up; introduces a platform-parity question out of scope for the split.

## Evidence Sources

- `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-14T09-01\.github\workflows\ci.yml` (read in full)
- `docs/features/active/2026-08-14-ci-parallel-job-split-553/evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md` (measured baseline; authoritative)
- `docs/features/active/2026-08-14-ci-parallel-job-split-553/issue.md`, `spec.md` (promoted content; substitute for the absent promoted-entry file)
- `.claude/rules/ci-workflows.md`, `.claude/rules/benchmark-baselines.md`
- `.claude/skills/orchestrate/SKILL.md` line 159 (reusable-workflow convention)
- `TaskMaster.sln` (18 projects, 9 test projects), `dotnet-tools.json` (CSharpier 1.2.6, root-level)
- GitHub docs fetched 2026-08-14: Reusing workflows (10-level nesting statement); actions/cache README (exact-key save skip; cache immutability); Actions limits (concurrency per plan: Free 20 / Pro 40 / Team 60 / Enterprise 500; macOS-only separate cap)
- Ruleset facts (`18572843`, two required contexts, `strict_required_status_checks_policy: true`) supplied by the orchestrator as verified via `gh api`; not re-collected this session

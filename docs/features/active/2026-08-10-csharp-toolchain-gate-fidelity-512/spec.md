# 2026-08-10-csharp-toolchain-gate-fidelity-512 (Spec)

- **Issue:** #512
- **Also closes:** #492, #509, #522
- **Parent (optional):** epic `build-ci-coverage-gate-fidelity` (Lane A, Wave 0)
- **Owner:** drmoisan
- **Work Mode:** full-bug (this file is the authoritative acceptance-criteria source)
- **Last Updated:** 2026-08-11T01-05
- **Status:** Complete — all 13 acceptance criteria delivered and verified; see `plan.2026-08-10T14-08.md` and `evidence/qa-gates/completion-attestation.2026-08-11T01-08.md`
- **Version:** 1.0

## Context

- **Summary of the bug and its impact.** Two of the four commands in the repository's mandatory C#
  toolchain do not do what they claim, and a third is defective by the same mechanism as one of
  them.
  - **Defect A — the type-check (nullable) gate.** The documented command
    `msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` cannot
    fail when outputs are current (MSBuild skips `CoreCompile` on every project and returns exit 0
    in under two seconds), and cannot pass when compilation is forced (`/p:Nullable=enable`
    conscripts every file that has not opted in to nullable analysis). Both failure modes were
    reproduced in this worktree on 2026-08-10.
  - **Defect B — the format gate.** The documented command `dotnet tool run csharpier .` is
    CSharpier v0 syntax. `dotnet-tools.json` pins CSharpier 1.2.6, which requires a subcommand. The
    documented step 1 returns exit 1 and formats nothing.
  - **Defect C — the analyzer gate (found during baseline capture, not enumerated by any issue).**
    The documented analyzer command is vacuous by exactly the same incremental-build mechanism as
    Defect A: re-run warm it returns exit 0 in 1.5 s having skipped `CoreCompile` on 18 of 18
    projects. Analyzer diagnostics are produced during compilation, so a build that skips
    compilation runs no analyzers.
- **Observed environment.** Windows 11 Pro 10.0.26200; .NET Framework 4.8.1; MSBuild from Visual
  Studio 18 (Community) at
  `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`;
  repo-pinned .NET SDK 8.0.205 installed to `.dotnet-sdk` by
  `scripts/vscode/Install-RepoDotNetSdk.ps1`; CSharpier pinned to 1.2.6 in `dotnet-tools.json`;
  CI runner class `windows-latest`.
- **Who is affected, how often, how bad.** Every agent session and every contributor who runs the
  documented toolchain, on every C#-touching change. Severity is High, not Blocker: the nullable
  debt the corrected gate exposes is pre-existing, and `.github/workflows/ci.yml` already runs the
  correct commands, so no defective artifact has reached `main` through this path. The cost is
  twofold and recurring:
  1. Every recorded "nullable gate passed" result in prior feature evidence overstates what was
     verified, because the gate did not compile.
  2. When an agent does force a compile, the documented flag manufactures a false blocking finding.
     Deliveries #507 and #508 each required a human-level override of a spurious `CS8603` blocker on
     2026-08-08.
- **First observed / versions impacted.** Defect A recorded 2026-08-07 (#492) and 2026-08-08 (#512,
  #522). Defect B recorded 2026-08-08 (#509). Defect C measured 2026-08-10 during this feature's
  baseline capture. All three are present at the branch head.

## Repro & Evidence

All figures below are measured in this worktree on 2026-08-10 and are recorded in the feature's
baseline artifacts. Where an issue or a research artifact quotes a different figure, the baseline
artifact is authoritative.

### Defect B — format gate

Evidence: `evidence/baseline/baseline-csharpier-documented-command.2026-08-10T14-25.md` and
`evidence/baseline/baseline-csharpier-replacement-forms.2026-08-10T14-45.md`.

| Form | Command | EXIT_CODE | Result |
|---|---|---|---|
| documented | `dotnet tool run csharpier .` | 1 | `Required command was not provided.` / `Unrecognized command or argument '.'` |
| documented (global) | `csharpier .` (global 1.3.0 on this machine) | non-zero | same rejection |
| CI form | `dotnet csharpier check .` | 0 | `Checked 1517 files in 5183ms.` |
| explicit manifest form | `dotnet tool run csharpier check .` | 0 | `Checked 1517 files in 5703ms.` |

The repository is presently CSharpier-clean, so correcting the documented command requires no
formatting churn.

### Defect A — type-check gate

Evidence: `evidence/baseline/baseline-nullable-gate-vacuity.2026-08-10T14-25.md`.

| Run | Command | EXIT | Elapsed | `Skipping target "CoreCompile"` | Errors |
|---|---|---|---|---|---|
| M1 | documented analyzer step, `/t:Build` (cold) | 0 | 25.8 s | 0 | 0 |
| M2 | **documented type-check step, `/t:Build` + `/p:Nullable=enable`** (warm) | **0** | **1.8 s** | **18 of 18 projects** | 0 |
| M3 | **CI's actual command**, `/t:Rebuild /m`, no `/p:Nullable=enable` | **0** | 20.0 s | 0 (74 `CoreCompile` executions) | 0 |
| M4 | `/t:Rebuild /m` **retaining** `/p:Nullable=enable` | **1** | 4.3 s | 0 | **195** |

M2 is the defect: the documented gate passed in 1.8 s having compiled nothing. M3 shows CI's command
both compiles genuinely and passes. M4 shows `/p:Nullable=enable` is what makes the gate unpassable;
its 195 errors are all in `UtilitiesCS.csproj` and reproduce issue #492's per-diagnostic breakdown
exactly (CS8766 x130, CS8618 x23, CS8625 x12, CS8600 x9, CS8601 x8, CS8604 x7, CS8602 x3, CS8603 x2,
CS8714 x1).

### Defect C — analyzer gate

Evidence: `evidence/baseline/baseline-analyzer-step-vacuity.2026-08-10T14-55.md`.

| Run | Command | EXIT | Elapsed | `Skipping target "CoreCompile"` |
|---|---|---|---|---|
| A1 | documented analyzer step, `/t:Build` | 0 | 22.1 s | 3 |
| A2 | **the same command run again immediately** | **0** | **1.5 s** | **18 of 18** |
| A3 | analyzer properties under `/t:Rebuild /m` | 0 | 19.0 s | 0 |

### Supporting evidence

- `evidence/baseline/baseline-nullable-pragma-inventory.2026-08-10T14-35.md` — 458 `.cs` files carry
  a `#nullable enable` pragma (`UtilitiesCS` 390, `QuickFiler` 22, `UtilitiesCS.Test` 21,
  `SVGControl` 17, `TaskMaster.Test` 4, `SVGControl.Test` 3, `TaskMaster` 1). `UtilitiesCS`'s 390
  opted-in files are already clean under CI's command (M3). The 195 errors therefore originate in
  `UtilitiesCS` files that have never opted in.
- `evidence/baseline/baseline-ci-parity-on-main.2026-08-10T15-05.md` — on `main`'s tip
  (`a682c7a2`), the CI steps `Verify formatting`, `Build with analyzers and code style enforcement`
  and `Build with nullable warnings treated as errors` all **succeeded**. The only failing step is
  `Run MSTest suite with coverage`, which is outside this feature's scope. This settles that the
  commands this feature adopts are passable against a clean checkout.

### Expected vs actual

- **Expected.** Each documented command executes against the pinned toolchain, performs the work it
  claims, enforces the policy it names, and agrees with the corresponding step in
  `.github/workflows/ci.yml`. A failure an agent observes is a real regression it introduced.
- **Actual.** Step 1 returns exit 1 and formats nothing. Steps 2 and 3 return exit 0 having compiled
  nothing whenever outputs are current. Step 3 additionally carries a property CI deliberately omits,
  so the moment it does compile it reports a failure that is not the agent's.

### Frequency / determinism

Deterministic in all three cases. Defect B fails on every invocation. Defects A and C are vacuous on
every invocation made against a tree whose outputs are current — that is, on every toolchain-loop
pass after the first compile, which is the normal steady state of a working tree.

## Scope & Non-Goals

### In scope

- Reconcile every documented site of the C# **format**, **analyzer** and **type-check** commands
  inside the epic's authorized surface (`CLAUDE.md`, `.claude/rules/csharp.md`,
  `.claude/skills/csharp-qa-gate/SKILL.md`) so that the documented text matches a command that
  executes, enforces, and agrees with `ci.yml`.
- Correct factually wrong rationale prose immediately adjacent to those commands.
- Correct the executable carriers of the same defect: `scripts/vscode/Invoke-VSBuild.ps1`,
  `.vscode/tasks.json`, and the Pester assertions in
  `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` that currently pin the defect as expected
  behavior.
- Produce the negative-path proof required by #512, with a paired positive control and a recorded
  revert.
- Record the nullable-debt figure the corrected gate leaves un-enforced, for the follow-on
  burn-down epic.

### Out of scope / non-goals

- **Fixing the nullable diagnostics.** Issue #492 and the epic charter separate gate fidelity from
  debt burn-down. Only the former is delivered here. The measured figure is recorded, not resolved.
- **Coverage thresholds and coverage exclusion policy.** `CLAUDE.md` § UT2,
  `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` **must not be edited**;
  they belong to sibling feature #494.
- **Issue #513** (`collect_pr_context` misclassification), which the epic excludes as upstream work
  in `drm-copilot`.
- **The Codex/Copilot instruction mirrors.** See scope decision 1 below.
- **Adding a new verification script under `scripts/`.** See "Options considered and rejected".

### Explicitly excluded systems and files

`AGENTS.md`, `.agents/**`, `.codex/**`, `.github/instructions/**`, `.github/agents/**`,
`.github/workflows/ci.yml`, `CLAUDE.md` § UT2, `.claude/rules/general-unit-test.md`,
`.claude/rules/quality-tiers.md`.

## Scope Decisions (explicit)

Each decision below is stated with its rationale and, where the decision is to exclude, a named
follow-up action. None of these is left implicit.

### SD1 — The `AGENTS.md` / `.agents/` / `.github/instructions/` mirror sites: EXCLUDE

**Decision.** Exclude the entire mirror tree from this feature. This adopts research recommendation
D5.

**Rationale (in descending strength).**

1. **`.github/instructions/` sits under an unsuspended hard constraint.**
   `.claude/skills/policy-compliance-order/SKILL.md:32` states: "Do NOT modify policy documents
   under `.claude/rules/` **or `.github/instructions/`**." The epic's "Execution Authorization
   Required" section (`docs/features/epics/build-ci-coverage-gate-fidelity/epic.md`, lines 183-197)
   suspends that constraint only for `CLAUDE.md`, `.claude/rules/csharp.md`,
   `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`.
   `.github/instructions/` is named in the constraint and not named in the authorization. The
   correct response to an unsuspended hard constraint is to stop, not to widen scope.
2. **`AGENTS.md` forbids manual editing and its generator does not exist here.** `AGENTS.md` lines
   3-27 declare the file generated from seventeen `.github/**` sources, say "Do not edit this file
   manually", and name `scripts/dev-tools/sync-agents-from-instructions.ps1` as the regeneration
   command. `scripts/dev-tools/` contains exactly one file, `run-actionlint.ps1`. Hand-editing
   violates the file's own contract; regenerating is impossible in this repository.
3. **`.agents/` and `.codex/` are inbound artifacts of a different repository.** They self-describe
   as Codex push-down resources installed by the `drm-copilot` MCP tool
   `push_down_codex_and_agents_customizations`. An edit here is not durable; the next push-down
   overwrites it. This is the same upstream-ownership reasoning the epic already applied to exclude
   issue #513.
4. **`.github/agents/**` is excluded on ground 3 only.** These files are not named in the
   `policy-compliance-order` constraint, so the exclusion rests on the weaker ownership argument.
   If a reviewer disputes `drm-copilot` ownership of `.github/agents/`, the follow-up issue below is
   the correct place to resolve it.

**Cost of excluding, stated plainly.** A Codex- or Copilot-driven session reading a mirror will
still run the unpassable command and can still manufacture the false blocking findings that required
human override on #507 and #508. After this feature merges, the mirrors become the only sites that
disagree with CI, so the divergence becomes host-dependent and harder to diagnose. This is accepted
because the alternative is an unauthorized edit and a non-durable one.

**Required follow-up action.** File one follow-up issue through the MCP promotion lifecycle
(`new_potential_bug_entry` then `potential_to_issue`), titled to the effect of "Codex/Copilot
instruction mirrors still document the CSharpier v0 command and the unpassable nullable command".
The issue must enumerate these sites, verified by grep on 2026-08-10:

| Path | Lines | Defect |
|---|---|---|
| `AGENTS.md` | 466 (false rationale), 469, 470 (format), 487, 488 (type-check), 660 (format), 662 (type-check) | all |
| `.github/instructions/csharp-code-change.instructions.md` | 29 (false rationale), 32, 33 (format), 50, 51 (type-check) | all |
| `.github/instructions/csharp-unit-test.instructions.md` | 45 (format), 47 (type-check) | format + type-check |
| `.agents/skills/csharp/SKILL.md` | 17 (format), 19 (type-check) | format + type-check |
| `.agents/skills/csharp-qa-gate/SKILL.md` | 32 (format), 34 (type-check) | format + type-check |
| `.github/agents/csharp-typed-engineer.agent.md` | 172 (format), 174 (type-check) | format + type-check |
| `.github/agents/csharp-atomic-executor.agent.md` | 258 (format), 260 (`dotnet build -p:Nullable=enable`) | format + type-check |
| `.codex/codex-web-setup.sh` | 342 (printed follow-up command inside the heredoc ending at line 348) | type-check |

The issue must name `drm-copilot` as the owning repository for `.agents/`, `.codex/` and
`.github/agents/`, and must state that `.github/instructions/` requires a separate authorization
grant equivalent to the one this epic issued for `.claude/rules/csharp.md`.

**Note on `.codex/codex-web-setup.sh:342`.** Research classified this as an executable carrier. It is
not: lines 336-347 sit inside a heredoc terminated at line 348 and are printed as "useful follow-up
commands", not executed. It is a documentation carrier. Its practical consequence is handled by
decision SD3.

### SD2 — The analyzer-step vacuity (AC13): INCLUDE, and record the widening

**Decision.** Correct the documented analyzer command alongside the type-check command. This
**rejects** research recommendation D7 (out of scope) and takes the first of the three positions
enumerated in `evidence/baseline/baseline-analyzer-step-vacuity.2026-08-10T14-55.md`.

**Rationale.**

1. **The defect is measured, not inferred.** Run A2 returned exit 0 in 1.5 s with `CoreCompile`
   skipped on 18 of 18 projects. The mechanism is identical to Defect A, and the corrected command
   (A3) is measured green at 19.0 s.
2. **Correcting step 3 makes step 2 systematically worse.** Under the corrected loop, step 3's
   `/t:Rebuild` regenerates every project's outputs *without* the analyzer properties. A subsequent
   verification pass in an unmodified tree then finds step 2's outputs current and skips
   `CoreCompile` on all 18 projects — so the last compile of every unchanged project was performed
   with the analyzer properties absent. Leaving step 2 alone would ship a correction that degrades
   its neighbor.
3. **The lines are the same lines.** The analyzer command is bullet 5 of the same four-bullet
   "Approved commands" block being rewritten at `CLAUDE.md:198-199`, and step 2 of the same numbered
   lists at `CLAUDE.md:382`, `CLAUDE.md:400`, `.claude/rules/csharp.md:15` and
   `.claude/skills/csharp-qa-gate/SKILL.md:31`. A reader of the corrected block would otherwise see a
   defective neighbor presented as approved.
4. **Authorization holds.** The epic requires that edits stay inside "its own issue's acceptance
   criteria". AC13 is an acceptance criterion of this issue and explicitly contemplates correcting
   the analyzer command as one of its two permitted resolutions. The widening is therefore
   authorized by AC13 itself, and is recorded here as required.

**Consequence that must be documented in-line at each site (AC5).** The corrected documented
analyzer command uses `/t:Rebuild /m`, whereas `.github/workflows/ci.yml:98-100` uses `/t:Build /m`.
This is a deliberate difference: a CI runner checkout is always cold, so `/t:Build` there is a
genuine compile; a local working tree is warm, so `/t:Build` there is not. The rationale sentence
below must appear adjacent to the command at each site, or the corrected documentation is
internally misleading in a new way.

**Measured cost.** Two sequential full rebuilds per toolchain pass: 19.0 s (analyzer) plus 20.0 s
(type-check), against 1.5 s plus 1.8 s for the two vacuous warm builds being replaced. The added
cost is approximately 36 s per pass and is small relative to step 4 (`vstest.console.exe`). Issue
#492 asked for this measurement before committing to `/t:Rebuild`; it is recorded here and accepted.

**No follow-up issue is required for AC13**, because the second branch of AC13 (leave unchanged, file
a follow-up) is not taken.

### SD3 — The executable carriers (`Invoke-VSBuild.ps1`, `tasks.json`, the Pester test): INCLUDE

**Decision.** Correct all three. This adopts research recommendation D4.

**Rationale.**

1. `.claude/skills/policy-compliance-order/SKILL.md:34` directs agents to "Prefer repo-defined
   tasks/commands when running checks." The repo-defined task
   `type-check: TaskMaster.sln (nullable warnings as errors)` (`.vscode/tasks.json:141-167`) invokes
   `scripts/vscode/Invoke-VSBuild.ps1`, which hardcodes `'/t:Build'` at line 64 and maps
   `-EnableNullable` to `Nullable=enable` at lines 106-108. Correcting the prose while leaving the
   task defective would leave the *preferred* execution path unable to fail.
2. `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1:60` asserts that `-EnableNullable` produces
   `'Nullable=enable'`. The defect is currently pinned as an asserted expectation, so any future
   correction would be reported as a test regression.
3. **This supplies the red-before-green regression test the Bugfix Workflow requires.** The existing
   Pester file already dot-sources the script with `-NoExecute` (line 6) and unit-tests
   `Get-MSBuildBuildArguments` and `Get-RequestedMSBuildProperties` as pure functions with no
   mocking. Two new or modified assertions fail against the current implementation and pass against
   the corrected one, which is a genuine red-to-green regression test for #512 and #522 at a cost of
   one production file plus one test file.

**Obligations that follow, per `.claude/rules/powershell.md`.**

- The PowerShell toolchain must be run in addition to the C# toolchain, in order format → analyze →
  test, using the MCP functions `mcp__drm-copilot__run_poshqc_format`,
  `mcp__drm-copilot__run_poshqc_analyze` and `mcp__drm-copilot__run_poshqc_test`. VS Code task
  wrappers must not be substituted (rule line 20).
- Line coverage >= 85% and branch coverage >= 75%; no coverage regression on changed lines (rule
  lines 63-65). The changed lines are inside the already-covered pure region of the script (lines
  30-115); the uncovered I/O tail (lines 117-157) is unchanged and pre-existing.
- No new executable seam is required, because both changed functions are pure and are already
  exercised through the `-NoExecute` seam. The mocking rule (rule line 80) is therefore not engaged.
- Change budget: one production `.ps1` plus its test file, within the direct-mode limit of two
  production PowerShell files (rule line 39).

### SD4 — `.github/workflows/ci.yml`: NO CHANGE

**Decision.** Confirm research recommendation D6. `ci.yml` is not edited.

**Rationale.** `ci.yml` is the reference implementation this feature converges onto, not a defect.
Line 93 already uses `dotnet csharpier check .`, which resolves the manifest-pinned 1.2.6 because
line 89 runs `dotnet tool restore` first. Lines 103-116 already use `/t:Rebuild /m` with
`/p:Nullable=enable` deliberately omitted, and carry a six-line in-line comment (lines 106-112)
stating both rationales. All three steps passed on `main`'s tip
(`evidence/baseline/baseline-ci-parity-on-main.2026-08-10T15-05.md`).

**Correction to a premise stated in `issue.md` § Constraints & Risks.** The issue argues that a green
workflow run may be unobtainable on an epic-child branch because `ci.yml` triggers only on pull
requests to `main`/`development`. That premise is incomplete: `ci.yml` carries a `workflow_dispatch:`
trigger, and the `modified-workflow-needs-green-run` rule in
`.claude/skills/feature-review-workflow/SKILL.md` accepts a green `workflow_dispatch` run against the
branch head. A green run **would** be obtainable. The decision not to change `ci.yml` therefore rests
on it already being correct, not on the review gate being unsatisfiable. The procedural cost (a
Blocking review rule, a manual dispatch, and a long `windows-latest` job) is a secondary
reinforcement only.

## Root Cause Analysis

### Confirmed root cause — the general mechanism

**Outputs built under one property set are silently accepted as validating a different property
set.** MSBuild's incremental up-to-date check compares input and output file timestamps. It does not
invalidate on a command-line `/p:` change. When a project's outputs are newer than its sources,
`CoreCompile` is skipped regardless of which properties the current invocation passed, and the
target returns success.

The mandatory toolchain loop makes this concrete. The loop runs format → analyzer → type-check →
test and restarts from step 1 whenever a step changes files. Steps 2 and 3 differ only in their
`/p:` property sets and both use `/t:Build`. On every pass after the first compile, whichever of the
two did not most recently force a compile is validated against binaries produced with the other
one's properties. Because step 2 always precedes step 3, step 3's properties — the nullable and
warnings-as-errors properties — never reach the compiler at all in a warm tree.

Two consequences follow, and they mask each other:

- While the up-to-date check suppresses compilation, the gate reports a **false pass** (M2, A2).
- The moment compilation is forced, `/p:Nullable=enable` opts in every file that has never carried
  a `#nullable enable` pragma, and the gate reports a **false failure** (M4).

In neither state does the gate report the truth about the change under test.

The format defect is unrelated in mechanism and simpler in kind: the documented command is syntax
for a major version the repository does not use.

### Signals supporting the diagnosis

- 18 of 18 `Skipping target "CoreCompile"` occurrences with exit 0 at 1.8 s (M2) and 1.5 s (A2).
- 0 occurrences and 74 `CoreCompile` executions at 20.0 s under `/t:Rebuild` (M3).
- 458 files carry `#nullable enable`; `UtilitiesCS`'s 390 opted-in files pass M3 with zero errors,
  while M4's 195 errors are attributed to `UtilitiesCS.csproj` — i.e. to its un-opted files.
- No `.csproj` in the repository carries a `<Nullable>` element and no `Directory.Build.props`
  exists, which is why the property has to be supplied on the command line to have any effect, and
  why supplying it is a solution-wide opt-in rather than a targeted one.

### Affected components

| Component | Role |
|---|---|
| `CLAUDE.md` §§ C#1, CUT3, "C# Toolchain (run in this exact order)" | primary documented toolchain |
| `.claude/rules/csharp.md` § Toolchain, § Severity-first ordering invariant | path-scoped auto-loaded rule |
| `.claude/skills/csharp-qa-gate/SKILL.md` § Toolchain Execution Sequence | final QA gate procedure |
| `scripts/vscode/Invoke-VSBuild.ps1` | executable carrier of `/t:Build` and `Nullable=enable` |
| `.vscode/tasks.json` | repo-defined task surface invoking the above |
| `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` | pins the defect as asserted behavior |

## Proposed Fix

Preparation modifies nothing outside this feature folder. This section **proposes** edits; a later
execution phase applies them.

### Design summary (what changes where)

1. Replace the documented **format** command with the CSharpier v1 two-role form
   (`format` to apply, `check` to verify), delete the global-install alternative, and state the
   manifest-pinning rule.
2. Replace the documented **analyzer** and **type-check** commands with `/t:Rebuild /m` forms,
   adopting `ci.yml`'s spelling character-for-character for the type-check step, and carrying a
   condensed rationale so a future agent does not restore `/p:Nullable=enable` or `/t:Build`.
3. Correct the two adjacent rationale statements that are factually wrong or misleading.
4. Add a `-Target` parameter to `Invoke-VSBuild.ps1`, neutralize `-EnableNullable`, repoint the
   `lint:` and `type-check:` VS Code tasks, and update the Pester assertions.

### Canonical replacement strings

These four strings are used verbatim throughout the table below.

- **FORMAT-APPLY:** `dotnet tool run csharpier format .`
- **FORMAT-VERIFY:** `dotnet tool run csharpier check .`
- **ANALYZE:** `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- **TYPECHECK:** `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

At the two sites that use a solution placeholder, substitute `<solution>.sln` for
`TaskMaster.sln`.

**Platform spelling.** All sites adopt `"/p:Platform=Any CPU"`, which is `ci.yml`'s spelling
(lines 99, 114), so a reader can diff the documented command against the workflow character for
character. The currently documented PowerShell form `/p:Platform='Any CPU'` and the form
`/p:Platform="Any CPU"` both reach MSBuild as the same argument; the change is for parity, not
correctness, and this equivalence is asserted from prior successful use rather than re-measured
here.

### Per-site replacement table — Tier 1 documentation (in scope)

| # | Path | Line(s) | Current text | Replacement |
|---|---|---|---|---|
| 1 | `CLAUDE.md` | 188 | ``- `csharpier` is file-based and formats only `*.cs` without touching project files.`` | Block R1 below |
| 2 | `CLAUDE.md` | 190-192 | ``- Approved commands:`` / ``- `dotnet tool run csharpier .``` / ``- or `csharpier .` (if installed globally)`` | Block R1 below |
| 3 | `CLAUDE.md` | 198-199 | ``- Approved commands (PowerShell):`` / `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Block R2 below (ANALYZE + rationale) |
| 4 | `CLAUDE.md` | 203 | ``- Enable nullable reference types and fail builds on warnings for touched code paths.`` | Block R3 below |
| 5 | `CLAUDE.md` | 205-206 | ``- Approved commands (PowerShell):`` / `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Block R3 below (TYPECHECK + rationale) |
| 6 | `CLAUDE.md` | 381 | ``1. `csharpier .``` | ``1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)`` |
| 7 | `CLAUDE.md` | 382 | `2. msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | ``2. `` + ANALYZE |
| 8 | `CLAUDE.md` | 383 | `3. msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | ``3. `` + TYPECHECK |
| 9 | `CLAUDE.md` | 399 | ``1. **Format**: `dotnet tool run csharpier .` (or `csharpier .` if installed globally)`` | ``1. **Format**: `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`; always via `dotnet tool run`, never a global install)`` |
| 10 | `CLAUDE.md` | 400 | ``2. **Analyze**: msbuild ... /t:Build ...`` | ``2. **Analyze**: `` + ANALYZE |
| 11 | `CLAUDE.md` | 401 | ``3. **Type-check**: msbuild ... /t:Build ... /p:Nullable=enable ...`` | ``3. **Type-check**: `` + TYPECHECK |
| 12 | `.claude/rules/csharp.md` | 14 | ``... Command: `dotnet tool run csharpier .` or `csharpier .``` | ``1. **Formatting — CSharpier**: All C# source files must be formatted with CSharpier. Do not use `dotnet format`. Apply: `dotnet tool run csharpier format .` Verify (CI parity, read-only): `dotnet tool run csharpier check .` Always invoke through `dotnet tool run` so the `dotnet-tools.json` pinned version is used; do not invoke a globally installed `csharpier`.`` |
| 13 | `.claude/rules/csharp.md` | 15 | ``... Command: `msbuild <solution>.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true``` | same sentence with ANALYZE (placeholder form) plus the `/t:Rebuild` rationale sentence R4 |
| 14 | `.claude/rules/csharp.md` | 16 | ``3. **Type Checking — Nullable Analysis**: Enable nullable reference types and fail on warnings. Command: `msbuild <solution>.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true``` | ``3. **Type Checking — Nullable Analysis**: Nullable analysis is per-file opt-in via `#nullable enable`; the gate promotes the resulting diagnostics to errors. Command: `` + TYPECHECK (placeholder form) + rationale sentences R4 and R5 |
| 15 | `.claude/rules/csharp.md` | 83 | "...because the type-check toolchain step runs `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`, which promotes any `warning`-severity analyzer diagnostic to a build error." | "...because the type-check toolchain step runs `msbuild ... /t:Rebuild /m ... /p:TreatWarningsAsErrors=true`, which promotes any `warning`-severity analyzer diagnostic to a build error." **The severity-first invariant itself is preserved verbatim; only the embedded command string changes.** |
| 16 | `.claude/skills/csharp-qa-gate/SKILL.md` | 30 | ``1. `dotnet tool run csharpier .``` | ``1. `dotnet tool run csharpier format .` (then `dotnet tool run csharpier check .` to verify)`` |
| 17 | `.claude/skills/csharp-qa-gate/SKILL.md` | 31 | ``2. `msbuild <solution>.sln /t:Build ...``` | ``2. `` + ANALYZE (placeholder form) |
| 18 | `.claude/skills/csharp-qa-gate/SKILL.md` | 32 | ``3. `msbuild <solution>.sln /t:Build ... /p:Nullable=enable ...``` | ``3. `` + TYPECHECK (placeholder form) |
| 19 | `.claude/skills/csharp-qa-gate/SKILL.md` | 60-69 (§ Evidence Storage) | *(no non-vacuity requirement)* | Append bullet R6 below |

#### Block R1 — `CLAUDE.md` lines 186-192 (format), full replacement

```
   - All C# source files (`*.cs`) must be formatted with `csharpier`.
   - Do **not** use `dotnet format` — it loads the solution/project model and can mis-handle legacy VSTO / .NET Framework projects by rewriting `.csproj` files.
   - `csharpier` is file-based and does not load the solution or project model, so it cannot rewrite a `.csproj` as a side effect of parsing the build graph. It is **not** restricted to `*.cs`: CSharpier 1.2.6 also accepts and processes `*.xml` and `packages.config`. `*.csproj`, `*.props` and `*.targets` are kept out of the check by `.csharpierignore`, not by any inherent CSharpier behavior.
   - Do not hand-format; if a diff disagrees with `csharpier`, formatter output wins.
   - Run `dotnet tool restore` once per clone or worktree before the first invocation.
   - Approved commands (CSharpier is pinned to 1.2.6 by `dotnet-tools.json`; v1 requires a subcommand, so the bare-path form does not run):
     - Apply formatting: `dotnet tool run csharpier format .`
     - Verify, read-only, CI parity: `dotnet tool run csharpier check .`
   - Always invoke through `dotnet tool run` so the manifest-pinned version is used. Do not invoke a globally installed `csharpier`: a different global version produces diffs that disagree with `.github/workflows/ci.yml`, which runs the pinned version after `dotnet tool restore`.
```

Line 187 (the `dotnet format` warning) and line 189 (formatter-output-wins) are **verified correct
and retained verbatim**.

#### Block R2 — `CLAUDE.md` lines 198-199 (analyzer), full replacement

```
   - Approved commands (PowerShell):
     - `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
   - Use `/t:Rebuild`, not `/t:Build`. Analyzer diagnostics are produced during compilation, and MSBuild's incremental up-to-date check compares timestamps without invalidating on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no analyzers. `.github/workflows/ci.yml` uses `/t:Build /m` for its analyzer step because a runner checkout is always cold; a local working tree is not.
```

#### Block R3 — `CLAUDE.md` lines 202-206 (type-check), full replacement

```
   - Treat C# compiler diagnostics and nullable-flow warnings as first-class type-safety checks.
   - Nullable enforcement in this repository is **per-file opt-in**: a file participates in nullable analysis when it carries a `#nullable enable` directive, and `/p:TreatWarningsAsErrors=true` then promotes its `CS86xx` diagnostics to build errors.
   - Avoid introducing nullable warnings; fix the root null-state issue instead.
   - Approved commands (PowerShell):
     - `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
   - This is character-for-character the command in `.github/workflows/ci.yml` (step "Build with nullable warnings treated as errors"). Two properties of it are load-bearing and must not be "restored":
     - **Do not add `/p:Nullable=enable`.** No project in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so the property is a solution-wide opt-in that conscripts every file which has never adopted the pragma. Forcing it produced 195 errors in `UtilitiesCS.csproj` on 2026-08-10 against zero errors without it, and CI omits it deliberately. Removing it loses no enforcement over any file that has opted in.
     - **Do not use `/t:Build`.** MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on every project: the gate cannot fail.
```

#### R4 — analyzer rationale sentence for the two condensed sites

> Use `/t:Rebuild` so the step always performs a genuine recompile; a warm `/t:Build` skips
> `CoreCompile` and runs no analyzers. CI uses `/t:Build /m` because a runner checkout is cold.

#### R5 — type-check rationale sentence for the two condensed sites

> This is `ci.yml`'s command verbatim. Do not add `/p:Nullable=enable` (no project carries a
> `<Nullable>` element; the flag opts in every un-annotated file at once and makes the gate
> unpassable) and do not use `/t:Build` (a warm build skips `CoreCompile` and the gate cannot fail).

#### R6 — bullet appended to `.claude/skills/csharp-qa-gate/SKILL.md` § Evidence Storage

```
- For steps 2 and 3, capture an MSBuild file log (`/fl "/flp:logfile=<path>;verbosity=normal"`) and record in the evidence artifact that the log contains **zero** occurrences of `Skipping target "CoreCompile"`. A step that reports exit 0 with a non-zero skip count compiled nothing and is **unverified**, not passed.
```

### Per-site replacement table — executable carriers (in scope)

| # | Path | Line(s) | Current text | Replacement |
|---|---|---|---|---|
| 20 | `scripts/vscode/Invoke-VSBuild.ps1` | 20-21 | ``[Parameter(Mandatory = $false)]`` / ``[switch]$EnableNullable`` | Retained, with a comment marking it deprecated and no-op (see SD3 note below) |
| 21 | `scripts/vscode/Invoke-VSBuild.ps1` | after 9 (new parameter) | *(absent)* | ``[Parameter(Mandatory = $false)]`` / ``[ValidateSet('Build', 'Rebuild')]`` / ``[string]$Target = 'Build'`` |
| 22 | `scripts/vscode/Invoke-VSBuild.ps1` | 47-67 (`Get-MSBuildBuildArguments`) | hardcoded ``'/t:Build',`` at line 64 | new parameter ``[ValidateSet('Build','Rebuild')] [string]$Target = 'Build'``; line 64 becomes ``"/t:$Target",`` |
| 23 | `scripts/vscode/Invoke-VSBuild.ps1` | 106-108 | ``if ($EnableNullable) { $properties += 'Nullable=enable' }`` | ``if ($EnableNullable) { Write-Warning 'The -EnableNullable switch is deprecated and has no effect. This repository enforces nullability per file via #nullable enable; /p:Nullable=enable is deliberately absent from CI and makes the gate unpassable. See CLAUDE.md C#1 item 3.' }`` |
| 24 | `scripts/vscode/Invoke-VSBuild.ps1` | 148 | call to `Get-MSBuildBuildArguments` | add `-Target $Target` |
| 25 | `.vscode/tasks.json` | 114-140 (`lint:` task) | args end `-EnableNETAnalyzers`, `-EnforceCodeStyleInBuild` | insert `"-Target", "Rebuild"` before the switches |
| 26 | `.vscode/tasks.json` | 141-167 (`type-check:` task) | args end `-EnableNullable`, `-TreatWarningsAsErrors` | replace with `"-Target", "Rebuild", "-TreatWarningsAsErrors"` |
| 27 | `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` | 23-44 | one `It` asserting the default argument array (line 36 asserts `'/t:Build'`) | **retained unchanged** (the default target remains `Build`), plus a new `It` asserting that `-Target Rebuild` emits `'/t:Rebuild'` in the same position |
| 28 | `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` | 56-63 | ``It 'maps nullable switches to the expected MSBuild properties'`` asserting `@('Nullable=enable', 'TreatWarningsAsErrors=true')` | assert `@('TreatWarningsAsErrors=true')` only, and rename the `It` to state that `-EnableNullable` emits no property |

**Correction to research D4.** D4 states that the assertion at
`tests/scripts/vscode/Invoke-VSBuild.Tests.ps1:36` must be inverted. It must not. That assertion
exercises `Get-MSBuildBuildArguments` without a `-Target` argument, and the default remains `Build`
so that the `build:` task (`.vscode/tasks.json:86-113`) is unchanged. The red-before-green
regression tests are items 27 (new case, currently fails because the parameter does not exist) and
28 (modified case, currently fails because the property is still emitted).

**SD3 note — why `-EnableNullable` is retained as a deprecated no-op rather than deleted.**
`.codex/codex-web-setup.sh:342` prints an `Invoke-VSBuild.ps1 ... -EnableNullable` invocation as a
recommended follow-up command, and `.codex/` is excluded from this feature under SD1. Deleting the
parameter would turn that printed instruction into a parameter-binding failure with no in-repo fix
available. Retaining the switch as a warning-emitting no-op keeps the printed command runnable while
making the change of behavior explicit rather than silent. Removal of the parameter is deferred to
the SD1 follow-up issue, after the mirror is corrected upstream.

### Boundaries and invariants to preserve

- The four-stage toolchain order (format → analyze → type-check → test) and its restart-from-step-1
  rule are unchanged.
- The severity-first ordering invariant at `.claude/rules/csharp.md:83` is preserved verbatim; only
  the command string it quotes changes. `.claude/rules/csharp.md:87` (the SecurityCodeScan / CS8032
  rationale) is unaffected because `/p:TreatWarningsAsErrors=true` is retained.
- `CLAUDE.md:187` and `CLAUDE.md:189` are verified correct and retained verbatim.
- `.csharpierignore` is not modified. Its comment at lines 9-11 repeats the same false premise as
  `CLAUDE.md:188` ("CSharpier formats C# source only (per CLAUDE.md C#1)"), but the ignore rules
  themselves are correct and the file is outside the enumerated documentation sites. Recorded as a
  known residual; folded into the SD1 follow-up issue.
- `CLAUDE.md:215` ("Keep nullable reference types enabled") and `.claude/rules/csharp.md:24` (same
  wording) are design guidance rather than commands, are defensible under a per-file opt-in reading,
  and are outside the enumerated-site authorization. Reviewed and left unchanged.
- No threshold is lowered, no mandatory step is removed, and no suppression is added.

### Why removing `/p:Nullable=enable` is a strengthening, not a relaxation

A reviewer will reasonably challenge this, so the argument is stated here rather than left to be
reconstructed.

1. **The property was never enforced by any merge gate.** `.github/workflows/ci.yml` has never
   passed it. Branch protection consumes CI's checks, so no merge has ever depended on it.
2. **The documented gate currently performs zero enforcement.** M2 measured exit 0 in 1.8 s with
   `CoreCompile` skipped on 18 of 18 projects. The property never reaches the compiler in a warm
   tree, which is the normal state of a working tree during a toolchain loop.
3. **The corrected gate performs strictly more enforcement than the current one.** It compiles
   genuinely (M3: 0 skips, 74 `CoreCompile` executions) and promotes every compiler warning —
   nullable and non-nullable alike — to an error via `/p:TreatWarningsAsErrors=true`. The delta
   against today is from nothing to CI parity.
4. **No opted-in file loses coverage.** 458 files carry `#nullable enable`, 390 of them in
   `UtilitiesCS`, and that project passes M3 with zero errors. Those files remain fully analyzed:
   the pragma, not the property, is what enrolls them. What the corrected command stops doing is
   conscripting files that never opted in.
5. **The alternative reading is the actual relaxation.** A documented mandatory command that cannot
   pass is not a gate; it is a permanent blocker that every session must override. Two deliveries on
   2026-08-08 did exactly that. A gate routinely overridden by a human enforces less than a gate
   that runs.

### Dependencies or blocked work

- No dependency on other epic children. This feature is Wave 0 and `depends_on: []`.
- Feature #494 also edits `CLAUDE.md`, in disjoint sections (§ UT2). No ordering edge; the sections
  merge cleanly provided this feature does not touch § UT2.
- The follow-on nullable-debt burn-down epic depends on the figure this feature records (AC12).

### Options considered and rejected

| Option | Verdict | Reason |
|---|---|---|
| **(a) Adopt CI's exact type-check command** — `/t:Rebuild /m ... /p:TreatWarningsAsErrors=true` | **Adopted** | The only option that executes, enforces, and agrees with CI simultaneously. Measured green locally (M3) and on `main`'s tip. |
| **(b) `/t:Rebuild` while keeping `/p:Nullable=enable`** | Rejected | Measured EXIT 1 with 195 errors (M4), all pre-existing debt this feature is charged not to fix. It would *increase* divergence from CI, contradicting the required outcome. A routinely-failing `/t:Rebuild` also deletes every project's `bin`/`obj` before failing, leaving step 4 with no assemblies to test. This is not a policy relaxation question: the debt burn-down is an explicit epic Non-Goal, so the gate would be unpassable within this feature's authorized scope. |
| **(c) Keep `/t:Build` and add a non-vacuous-compile assertion over an `/fl` log** | Rejected as the *documented command*; **adopted as the verification technique** | As policy text it replaces one command with a command plus a log-parsing procedure that every agent must implement identically, violating the simplicity principle in `.claude/rules/general-code-change.md`, and it still diverges from CI. It is retained as the evidence mechanism (see below) where a procedure is appropriate. |
| **(d) Persist `<Nullable>` in `Directory.Build.props` or per-`.csproj`** | Rejected | Would make the property participate in the up-to-date check and so solve the vacuity, but it opts in every file of the affected projects at once — which *is* the debt burn-down the epic excludes. It would also introduce the repository's first `Directory.Build.props`, a build-topology change disproportionate to a documentation-fidelity fix. |
| **(e) Scope `/p:Nullable=enable` to opted-in projects only** | Rejected | No project can opt in cleanly today: the 195 measured errors are attributed to `UtilitiesCS.csproj`, the largest and most foundational project. Any clean opt-in would be the first increment of the out-of-scope burn-down, and the per-project figure has proven unstable across sessions. |
| **(f) Collapse steps 2 and 3 into one `/t:Rebuild` carrying all three properties** | Rejected | `/p:TreatWarningsAsErrors=true` would promote analyzer and code-style *warnings* to errors, which is why `ci.yml` keeps the steps separate. It would also erase the format→lint→type-check stage distinction the general policy requires, and would diverge from CI's two-step shape (AC5). |
| **(g) Add a new `scripts/` verification script for AC7** | Rejected | `.claude/rules/powershell.md` would require it to meet 85%/75% coverage and to route every external executable through a mockable wrapper seam (rule line 80), forcing `Invoke-MSBuildExe`/`Invoke-CSharpierExe` seams into a documentation-fidelity feature. AC7's claim is an execution-evidence claim, not a unit-testable property. It is satisfied by ordered plan tasks writing to `<FEATURE>/evidence/qa-gates/`, plus the already-existing Pester coverage of command *shape* through `Invoke-VSBuild.ps1`'s `-NoExecute` seam. |
| **(h) Change `ci.yml`** | Rejected | See SD4. |
| **(i) Edit the `AGENTS.md` / `.agents/` / `.github/instructions/` mirrors** | Rejected | See SD1. |

### The non-vacuity assertion mechanism

Exit code alone cannot distinguish a genuine pass from a vacuous one, so every analyzer and
type-check evidence artifact must carry a compile assertion. The required mechanism is:

1. Add `/fl "/flp:logfile=<path>;verbosity=normal"` to the invocation. These switches capture a file
   log and do not alter build semantics.
2. Assert that the log contains **zero** occurrences of the literal string
   `Skipping target "CoreCompile"`.
3. Record the assertion, with the counted value, in the evidence artifact alongside `Timestamp:`,
   `Command:`, `EXIT_CODE:` and `Output Summary:`.

This assertion cleanly separates the vacuous runs from the genuine ones in the measured baseline:
M2 and A2 produced 18 skips each; M3, M4 and A3 produced 0.

**Two counting traps that must not be repeated.**

- **Counting `csc.exe` does not work at `verbosity=normal`.** All four runs in
  `evidence/baseline/baseline-nullable-gate-vacuity.2026-08-10T14-25.md` report zero `csc.exe`
  occurrences, including M3 and M4, which genuinely compiled. A zero `csc.exe` count is therefore
  not evidence of vacuity.
- **`CoreCompile:` header lines print even when the target is skipped.** Counting those headers as
  "executions" is what produced the contradictory historical artifact
  `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/nullable-build-baseline.2026-08-06T22-23.md`,
  which claims 18 `CoreCompile` executions and 0 short-circuits under a warm `/t:Build` with
  `/p:Nullable=enable` — the exact opposite of M2. The skip-count assertion is not vulnerable to this
  ambiguity because the skip message is emitted only when the target is actually skipped.

**Recorded deviation from AC2's parenthetical.** AC2 names "a `csc.exe` invocation count greater than
zero in an MSBuild file log" as the proof mechanism. Measurement shows that count is zero even for
genuine compiles at `verbosity=normal`, so the parenthetical as literally written is not satisfiable
by the described log. AC2's substantive requirement — a non-vacuous compile assertion, not exit code
alone — is satisfied by the zero-skip assertion above, which is strictly more discriminating. This
deviation is recorded here rather than by renumbering or rewriting the acceptance criteria. Execution
must record it in the evidence artifact so the substitution is auditable.

## Assumptions, Constraints, Dependencies

- **Assumptions.**
  - Visual Studio 18 MSBuild and `vswhere.exe` are present on the executing machine; the repo-pinned
    SDK is installed via `scripts/vscode/Install-RepoDotNetSdk.ps1` and `dotnet tool restore` has
    been run.
  - `main`'s three toolchain steps remain green; the currently failing `Run MSTest suite with
    coverage` step is unrelated to this feature and is not inherited by it.
  - `.claude/rules/csharp.md` line numbers 14, 15, 16 and 83, `CLAUDE.md` line numbers 188-206 and
    381-401, and `.claude/skills/csharp-qa-gate/SKILL.md` line numbers 30-32 were read on
    2026-08-10. They have drifted before; execution must re-locate each site by its text, not by its
    line number.
- **Constraints.**
  - The epic's authorization is limited to the toolchain command text and its surrounding rationale
    at the enumerated sites, extended by AC13 to the adjacent analyzer command. No other governance
    edit is permitted.
  - `CLAUDE.md` § UT2, `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`
    must remain byte-identical to the merge base.
  - Both the C# and PowerShell toolchains apply, because the change touches `*.ps1`.
  - Measured toolchain-loop cost after the change: approximately 39 s of MSBuild per pass (19.0 s
    analyzer plus 20.0 s type-check), against approximately 3.3 s of vacuous MSBuild today.
- **External dependencies.** CSharpier 1.2.6 via `dotnet-tools.json`; MSBuild via Visual Studio;
  `vstest.console.exe`; the `drm-copilot` MCP server for the PowerShell toolchain and for the SD1
  promotion lifecycle.

## Data / API / Config Impact

- **User-facing / CLI changes.** `scripts/vscode/Invoke-VSBuild.ps1` gains a `-Target` parameter
  (`Build` | `Rebuild`, default `Build`, so existing callers are unaffected). `-EnableNullable`
  remains bindable but becomes a no-op that emits a warning. Two `.vscode/tasks.json` task
  definitions change their arguments; the task labels are unchanged, so any external reference by
  label continues to resolve.
- **Data / migration.** None.
- **Logging / telemetry.** One new `Write-Warning` on the deprecated switch. No other logging change.
- **Compatibility notes.** The documented format command changes shape (`csharpier .` →
  `csharpier format .` / `csharpier check .`). Any artifact, plan, or agent memory quoting the old
  form is stale; correcting those is out of scope here but should be noted in the PR body so
  reviewers do not treat quoted historical evidence as a contradiction.

## Test Strategy

### Regression tests to add or update

The `full-bug` workflow requires a failing test before the fix. Both live in the existing Pester
file, which needs no new seam.

| Test | File | Red before, green after |
|---|---|---|
| `Get-MSBuildBuildArguments` emits `/t:Rebuild` when `-Target Rebuild` is supplied | `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` (new `It` in the existing `Describe` at lines 23-44) | Red: the parameter does not exist, so the call fails |
| `-EnableNullable` emits no MSBuild property | `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` (modified `It` at lines 56-63) | Red: the current implementation emits `'Nullable=enable'` |
| `Get-MSBuildBuildArguments` still emits `/t:Build` by default | `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` lines 23-44 | Unchanged; guards against an accidental default change |

No C# unit tests are added: no C# production code changes.

### Negative-path proof design (AC4)

The proof is a three-run sequence. All three runs use the identical corrected command, which is the
point: one command serves as gate, positive control and negative control.

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:logfile=<repo>\coverage\typecheck.log;verbosity=normal"
```

1. **Positive control (must run first, on an unperturbed tree at the branch head).**
   Required outcome: `EXIT_CODE 0`, MSBuild's summary reporting `0 Error(s)`, and **zero**
   occurrences of `Skipping target "CoreCompile"` in the log. Record elapsed time. Write to
   `<FEATURE>/evidence/qa-gates/typecheck-positive-control.<timestamp>.md`.
   If this returns non-zero, **stop and report**. A red positive control invalidates the central
   design assumption and must not be worked around.
2. **Negative control (the perturbation).**
   - **Primary target: `UtilitiesCS/Extensions/QueueExtensions.cs`.** Read on 2026-08-10: 21 lines,
     `#nullable enable` at line 9, a concrete `public static class QueueExtensions` at lines 11-20.
     `UtilitiesCS` is the foundational project and compiles first, so the diagnostic appears within
     a few seconds.
   - **Fallback target: `SVGControl/ISvgResource.cs`.** Read on 2026-08-10: 31 lines,
     `#nullable enable` at line 1, and a concrete `public class SvgResource` at lines 18-30 whose
     closing brace is line 30.
   - **Perturbation (one appended line, inside the existing class body):**
     ```csharp
     public static string NullableGateNegativeControl() => null;
     ```
     This form is chosen because it produces `CS8603` (possible null reference return)
     deterministically in an enabled nullable context; it is `public` and `static`, so no
     unused-member diagnostic fires; it adds no field, so it cannot produce `CS8618` and confuse
     attribution; and it is a single appended line, so the revert is verifiable by line count and no
     existing line number moves. Note that `QueueExtensions` is a static class, so the member must be
     `static`; if the fallback target is used, the member is added to `SvgResource` and `static` is
     retained for the same reason.
   - Required outcome: `EXIT_CODE 1` with at least one diagnostic line matching
     `error CS8603` attributed to the perturbed file. Record the literal diagnostic text.
   - **Non-vacuity condition (an acceptance condition, not an assumption).** The log must contain
     zero occurrences of `Skipping target "CoreCompile"`, and the perturbed file's project must
     appear among the projects the run compiled. Under `/t:Rebuild` every project is recompiled, so
     this holds by construction, but it must be asserted from the log rather than assumed. An
     earlier project aborting the graph before the perturbed project compiles is a real hazard in
     this repository's topology and is what the positive control in step 1 rules out.
   - Write to `<FEATURE>/evidence/qa-gates/typecheck-negative-control.<timestamp>.md`, recording the
     file, the exact perturbation, the command, the diagnostic, the exit code, and the revert
     confirmation.
3. **Revert and restore.**
   - `git checkout -- <perturbed file>`. The perturbation is never committed.
   - Re-run the positive control to confirm the tree is green again. This is also the mandatory
     **build-output restoration** step: a failed `/t:Rebuild` issues `Clean` to every project before
     the first `CoreCompile`, so the negative control leaves every project's `bin`/`obj` deleted.
     Without this re-run, `vstest.console.exe` finds no assemblies. It must be an ordered plan task,
     not an implicit consequence.

### Edge cases and negative scenarios

- Warm tree versus cold tree for both MSBuild steps — the skip-count assertion must be recorded in
  both states during verification, since the defect is only visible warm.
- `dotnet tool restore` not yet run in a fresh worktree: the corrected format command must be
  documented with the restore prerequisite, and the verification sequence must exercise it.
- A globally installed CSharpier of a different version present on `PATH` (1.3.0 is present on the
  measuring machine): the corrected documentation must route through `dotnet tool run`.
- `-Target` supplied with an invalid value: `ValidateSet` rejects it at bind time.
- `-EnableNullable` supplied by an existing caller: binds, warns, emits no property.

### Coverage impact and targets

- No C# production code changes, so C# coverage is unchanged. The repository-wide C# floor is not
  engaged by this feature.
- PowerShell: the changed lines are inside `Invoke-VSBuild.ps1`'s pure region (lines 30-115), which
  the existing Pester file already covers. Line coverage must remain >= 85% and branch coverage
  >= 75%, with no regression on changed lines.

### Toolchain commands to run

Both toolchains, each in its own order, using the **corrected** C# commands (this feature is its own
first consumer):

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

PowerShell: `mcp__drm-copilot__run_poshqc_format` → `mcp__drm-copilot__run_poshqc_analyze` →
`mcp__drm-copilot__run_poshqc_test`.

### Manual validation steps

- Run the two corrected VS Code tasks (`lint:` and `type-check:`) from the task surface and confirm
  each performs a genuine rebuild and that `type-check:` no longer passes `Nullable=enable`.
- Execute a repository-wide grep for the CSharpier bare-path form and for
  `/t:Build ... /p:Nullable=enable`, and record the result, with the deliberately-excluded mirror
  sites enumerated (AC6).

## Reconciling the historical error counts (195 / 220 / ~414)

Three figures appear in the record and none of them agreed with the others. Two mechanisms explain
the spread, and both were measured on 2026-08-10.

1. **Double counting.** A naive `Select-String 'error CS'` over an MSBuild file log at
   `verbosity=normal` returns **390** — exactly twice the true 195 — because each error is printed
   once inline with a node-id prefix (for example `19>`) and once again in the terminal error summary
   block. 195 lines carry the node prefix and 195 do not. The reliable figure is MSBuild's own
   `N Error(s)` summary line, or a count restricted to node-prefixed lines. A session that counted
   naively and one that counted correctly would report 390 and 195 for the same build. This plausibly
   accounts for the ~414 figure reported in issue #507, which is close to twice a ~207 true count.
2. **Different termination points.** **195 is a lower bound, not a solution-wide total.** M4
   terminated after 16 `CoreCompile` executions against M3's 74. `UtilitiesCS` is a foundational
   dependency; once it failed, its dependents were never compiled and their nullable diagnostics were
   never counted. A session whose build reached a different point before aborting would report a
   different total and would attribute errors to a different project — which is exactly the observed
   disagreement between issue #492 (`UtilitiesCS.csproj`) and issue #512 (`TaskMaster.csproj`).

**What this feature records as authoritative.** Under `/t:Rebuild` with `/p:Nullable=enable` on
2026-08-10 at this branch head: **195 errors, all in `UtilitiesCS.csproj`**, with the per-diagnostic
breakdown reproduced above, and the explicit caveat that the solution-wide figure is **>= 195 and
unmeasured** because the build aborted before dependents compiled. AC12 is satisfied by recording
this figure with its attribution and its lower-bound qualification; it is not satisfied by quoting
195 as a total.

**Consequence for the follow-on burn-down epic.** Sizing must begin by measuring the solution-wide
figure, which requires either fixing `UtilitiesCS` first or building with
`/p:ContinueOnError` semantics per project. That measurement is not performed here.

## Acceptance Criteria

These are mirrored verbatim from `issue.md` and are **not** renumbered. `issue.md` remains the source
of the numbering; this file is the check-off surface for the `full-bug` work mode. Recorded
deviations are listed after the list and do not modify any criterion's text.

- [x] AC1 — Every site that documents the C# format command uses a command that executes successfully
      against the CSharpier version pinned in `dotnet-tools.json`, verified by running each documented
      form and recording `EXIT_CODE: 0`.
- [x] AC2 — Every site that documents the C# type-check command uses a command that performs a genuine
      compilation, proven by a non-vacuous compile assertion (a `csc.exe` invocation count greater than
      zero in an MSBuild file log), not by exit code alone.
- [x] AC3 — The documented type-check command returns `EXIT_CODE: 0` against an unperturbed clean
      checkout of this branch. The gate is passable.
- [x] AC4 — **Negative-path proof (#512).** A deliberately introduced nullable violation in a
      production file that carries a `#nullable enable` pragma causes the corrected type-check gate to
      return a non-zero exit code with the expected `CS86xx` diagnostic. The evidence artifact records
      the file, the exact perturbation, the command, the diagnostic, the exit code, and confirmation
      that the perturbation was reverted. This proof must be non-vacuous: the perturbed file's project
      must be one the corrected command genuinely recompiles.
- [x] AC5 — The documented format command, the documented analyzer command, and the documented
      type-check command are each consistent with the corresponding step in
      `.github/workflows/ci.yml`. Any deliberate difference between a documented command and CI's
      command is stated in-line with its rationale.
- [x] AC6 — The complete site inventory is reconciled. No site anywhere in the repository still
      documents the CSharpier v0 bare-path form, and no site still documents a `/t:Build`-based
      nullable type-check command. Verified by a repository-wide grep recorded in the evidence
      artifact. Sites deliberately left unchanged are enumerated with rationale.
- [x] AC7 — A verification step exists that proves each documented command runs green against a clean
      checkout, and it has been executed with its output recorded as evidence.
- [x] AC8 — No policy requirement is relaxed, weakened, or deleted. The diff contains no reduction of
      any threshold, no removal of any mandatory step, and no new suppression.
- [x] AC9 — `CLAUDE.md` § UT2, `.claude/rules/general-unit-test.md`, and
      `.claude/rules/quality-tiers.md` are unmodified, verified by a zero-line diff against the merge
      base for those files and sections.
- [x] AC10 — Factually incorrect rationale prose adjacent to the corrected commands is either
      corrected or explicitly recorded as verified-correct. In particular the claim at `CLAUDE.md:188`
      that CSharpier "formats only `*.cs` without touching project files" is resolved against measured
      behavior and `.csharpierignore`.
- [x] AC11 — Baseline evidence under `evidence/baseline/` and final-QC evidence under
      `evidence/qa-gates/` exist for every command step, each recording `Timestamp:`, `Command:`,
      `EXIT_CODE:` and `Output Summary:`.
- [x] AC12 — The nullable diagnostics exposed by the corrected gate are recorded as a measured figure
      with per-project attribution for the follow-on burn-down epic. They are **not** fixed here.
- [x] AC13 — The documented analyzer step's vacuity is resolved by an explicit, recorded decision in
      `spec.md`: either the analyzer command is corrected alongside the type-check command, or it is
      deliberately left unchanged with the asymmetry explained in-line at the documentation site and a
      follow-up issue filed. Silent inaction does not satisfy this criterion.

### How each criterion is satisfied by this design

| AC | Satisfied by |
|---|---|
| AC1 | Sites 1-2, 6, 9, 12, 16 of the replacement table; verification runs both `format` and `check` and records `EXIT_CODE: 0` (both already measured at 0). |
| AC2 | The non-vacuity assertion mechanism above, applied to every documented MSBuild step. See the recorded deviation. |
| AC3 | Positive control run 1; independently corroborated by CI on `main`'s tip. |
| AC4 | The three-run negative-path proof design. |
| AC5 | R2/R4 state the deliberate analyzer difference from `ci.yml:98-100`; the type-check command is `ci.yml:113-115` verbatim; the format command is `ci.yml:93` in its explicit `dotnet tool run` spelling, with the reason stated. |
| AC6 | The repository-wide grep task plus the SD1 exclusion table, which enumerates every deliberately-unchanged site with rationale. |
| AC7 | The ordered evidence tasks writing to `<FEATURE>/evidence/qa-gates/`, plus the Pester tests covering command shape. Option (g) explains why no new script is added. |
| AC8 | The "strengthening, not a relaxation" argument, plus a diff review confirming no threshold, step, or suppression change. |
| AC9 | Zero-line diff check against the merge base for the three excluded files/sections. |
| AC10 | Sites 1 and 4 of the replacement table; `CLAUDE.md:187`, `:189`, `:215` and `.claude/rules/csharp.md:24`, `:83`, `:87` recorded as reviewed, with the first four verified-correct and unchanged. |
| AC11 | Six baseline artifacts already exist; the plan adds qa-gate artifacts for each of the four toolchain steps plus the two controls. |
| AC12 | The reconciliation section records 195, attributed to `UtilitiesCS.csproj`, with the lower-bound qualification. |
| AC13 | SD2 — decided explicitly, resolution recorded, widening recorded, in-line rationale for the residual CI difference required. |

### Recorded deviations

1. **AC2's parenthetical mechanism.** The `csc.exe` count is zero at `verbosity=normal` even for
   genuine compiles. The substantive requirement is met by the zero-`Skipping target "CoreCompile"`
   assertion, which is strictly more discriminating. No criterion text is changed.
2. **AC6's scope.** The mirror tree is deliberately left unchanged under SD1, which AC6's final
   sentence permits, provided the sites are enumerated with rationale. They are, in SD1's table.

## Risks & Mitigations

| Risk | Likelihood / impact | Mitigation |
|---|---|---|
| A future agent "restores" `/p:Nullable=enable` or `/t:Build`, believing the removal was a relaxation | Moderate / high — this has already happened once in the record | The in-line rationale (R3, R5) is carried at every corrected site; the "strengthening, not a relaxation" argument is recorded in this spec and must be summarized in the PR body |
| The type-check `/t:Rebuild` fails mid-build and leaves all `bin`/`obj` deleted, so step 4 has no assemblies | Low if the gate is green / high impact when it fires | An ordered restorative build is a required plan task after any failing `/t:Rebuild`, including after the AC4 negative control |
| Toolchain-loop time increases by roughly 36 s per pass (two full rebuilds) | Certain / low | Measured and accepted; small relative to `vstest.console.exe`. Recorded here because #492 required the measurement before committing to `/t:Rebuild` |
| Line numbers drift between spec authoring and execution | Moderate / low | Every site is identified by its exact current text as well as its line number; execution must match on text |
| Excluded mirrors keep manufacturing false findings in Codex/Copilot sessions | Certain until the follow-up lands / moderate | SD1's follow-up issue, filed through the MCP promotion lifecycle in the same delivery |
| `main`'s currently failing `Run MSTest suite with coverage` step blocks the PR for an unrelated reason | Moderate / moderate | Documented as pre-existing and out of scope; the three steps this feature documents are green on `main`'s tip |
| Removing `-EnableNullable` breaks an unknown caller | Low / low | The switch is retained as a warning-emitting no-op rather than deleted |
| Editing governance documents is normally hard-blocked | n/a | The epic's "Execution Authorization Required" section supplies the authorization; SD1 keeps the unsuspended part of the constraint intact |

## Rollout & Follow-up

1. **Order of work.** Capture the positive control before any edit (already partly captured in the
   baseline artifacts) → apply the two red Pester assertions → apply the `Invoke-VSBuild.ps1` and
   `tasks.json` changes → apply the documentation replacements → run both toolchains → run the AC4
   negative-path proof → revert and restore → run the repository-wide grep for AC6 → file the SD1
   follow-up issue → check off the acceptance criteria in this file.
2. **Branch and merge.** Epic-child branch targeting
   `epic/build-ci-coverage-gate-fidelity-integration`. No workflow file changes, so the
   `modified-workflow-needs-green-run` rule is not engaged.
3. **Post-merge follow-ups.**
   - SD1 follow-up issue for the Codex/Copilot mirror tree, including `.csharpierignore`'s comment
     at lines 9-11 and `.codex/codex-web-setup.sh:342`.
   - Follow-on nullable-debt burn-down epic, sized against a solution-wide measurement that this
     feature deliberately does not perform.
   - Stale agent-memory records that propagate the pre-2026-08-10 claim that a warm `/t:Build`
     compiles genuinely should be corrected by the agents that own them. Flagged, not edited here.
4. **Links.** Issues #492, #509, #512, #522; epic
   `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md`; research
   `research/toolchain-gate-fidelity.2026-08-10T14-40.md`; baseline artifacts under
   `evidence/baseline/`.

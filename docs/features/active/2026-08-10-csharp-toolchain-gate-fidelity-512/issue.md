# C# Toolchain Gate Fidelity (Issues #492, #509, #512, #522)

- Work Mode: full-bug
- Primary Issue: #512
- Also Closes: #492, #509, #522
- Issue URLs:
  - https://github.com/drmoisan/TaskMaster/issues/492
  - https://github.com/drmoisan/TaskMaster/issues/509
  - https://github.com/drmoisan/TaskMaster/issues/512
  - https://github.com/drmoisan/TaskMaster/issues/522
- Epic: `build-ci-coverage-gate-fidelity` (Wave 0)
- Integration Branch: `epic/build-ci-coverage-gate-fidelity-integration`
- Complexity Band: C3
- Last Updated: 2026-08-10

## Summary

The repository's documented mandatory C# toolchain contains two defective commands. Neither does what
it claims, and both diverge from what `.github/workflows/ci.yml` actually runs.

**Defect A — the type-check (nullable) gate.** The documented command

```
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
```

fails in two independent ways at once:

1. **It cannot fail (#512, #492).** MSBuild's incremental up-to-date check does not invalidate on a
   command-line `/p:` change alone. When outputs are already current from a prior `/t:Build`,
   `CoreCompile` is skipped entirely and the gate returns exit 0 without ever running nullable
   analysis. Forcing recompilation of identical source under identical properties returns exit 1.
2. **It can never pass (#522).** `/p:Nullable=enable` is deliberately absent from CI. The repository
   uses a per-file `#nullable enable` opt-in convention; `UtilitiesCS.csproj` and `SVGControl.csproj`
   carry no project-level `<Nullable>` element. Forcing the flag solution-wide opts in every file at
   once and produces several hundred `CS86xx` errors that are red on a clean `main`.

The two failure modes mask each other. While the incremental check suppresses compilation the gate
reports a false pass; the moment compilation is forced it reports a false failure. In neither state
does it report the truth.

**Defect B — the format gate (#509).** The documented command `dotnet tool run csharpier .` is
CSharpier v0 syntax. `dotnet-tools.json` pins CSharpier **1.2.6**, which exposes only the subcommands
`format <directoryOrFile>`, `check <directoryOrFile>`, `pipe-files` and `server`. The bare-path form
does not run the documented format step. CI already uses the correct form (`dotnet csharpier check .`).

## Environment

- OS: Windows 11 Pro 10.0.26200; CI runner class `windows-latest`
- Runtime: .NET Framework 4.8.1; MSBuild via Visual Studio 18 (Community)
- Repo-local SDK: 8.0.205, installed to `.dotnet-sdk` by `scripts/vscode/Install-RepoDotNetSdk.ps1`
- Formatter: CSharpier pinned to `1.2.6` in `dotnet-tools.json`
- Verification worktree: `bug/csharp-toolchain-gate-fidelity-512`, branched from
  `origin/epic/build-ci-coverage-gate-fidelity-integration`

## Steps to Reproduce

### Defect A (nullable gate)

1. Run the documented analyzer step (CLAUDE.md step 2):
   `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
2. Immediately run the documented type-check step (CLAUDE.md step 3):
   `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
   Observe exit 0 with no compilation performed.
3. Force recompilation of the same source under the same properties:
   `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
   Observe exit 1 with a large `CS86xx` population.
4. Run CI's actual command and observe exit 0:
   `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

### Defect B (format gate)

1. `pwsh ./scripts/vscode/Install-RepoDotNetSdk.ps1`
2. `./.dotnet-sdk/dotnet.exe tool restore`
3. `./.dotnet-sdk/dotnet.exe tool run csharpier .`

## Expected Behavior

The documented toolchain commands are commands that actually execute, actually enforce, and agree with
what `.github/workflows/ci.yml` runs. A clean checkout passes every documented gate, and any failure an
agent observes is a real regression it introduced.

## Actual Behavior

### Measured 2026-08-10 in this worktree

Defect B is empirically confirmed:

```
$ ./.dotnet-sdk/dotnet.exe tool run csharpier --version
1.2.6
EXIT_CODE: 0

$ ./.dotnet-sdk/dotnet.exe tool run csharpier .
'.' was not matched. Did you mean one of the following?
Required command was not provided.
Unrecognized command or argument '.'
Commands:
  format <directoryOrFile>  Format files.
  check <directoryOrFile>   Check that files are formatted. Will not write any changes.
  pipe-files                ...
  server                    ...
EXIT_CODE: 1
```

Defect A is empirically confirmed. Full detail in
`evidence/baseline/baseline-nullable-gate-vacuity.2026-08-10T14-25.md`:

| Run | Command | EXIT | Elapsed | `Skipping target "CoreCompile"` | Errors |
|---|---|---|---|---|---|
| M1 | documented analyzer step, `/t:Build` | 0 | 25.8 s | 0 (cold) | 0 |
| M2 | **documented type-check step, `/t:Build` + `/p:Nullable=enable`** (warm) | **0** | **1.8 s** | **18 of 18 projects** | **0** |
| M3 | **CI's actual command**, `/t:Rebuild /m`, no `/p:Nullable=enable` | **0** | 20.0 s | 0 | 0 |
| M4 | `/t:Rebuild /m` **retaining** `/p:Nullable=enable` | **1** | 4.3 s | 0 | **195** |

M2 is the defect: the documented gate passed in 1.8 seconds having skipped `CoreCompile` on every
project in the solution. M3 shows CI's command both compiles genuinely (74 `CoreCompile` executions)
and passes. M4 shows the documented flag is what makes the gate unpassable.

M4's 195 errors are all in `UtilitiesCS.csproj` and reproduce issue #492's breakdown exactly:
CS8766 x130, CS8618 x23, CS8625 x12, CS8600 x9, CS8601 x8, CS8604 x7, CS8602 x3, CS8603 x2, CS8714 x1.

Two caveats recorded for anyone re-measuring:

- A naive grep of the MSBuild file log returns 390, exactly double, because each error is printed once
  inline and once in the terminal summary block. Trust MSBuild's own `N Error(s)` line.
- **195 is a lower bound.** M4 terminated after 16 `CoreCompile` executions against M3's 74, because
  `UtilitiesCS` is a foundational dependency and its dependents never compiled. The solution-wide
  figure is unmeasured. This plausibly explains why historical figures disagree (195, 220, ~414).

### Prior recorded measurements

- Forced `/t:Rebuild` with `/p:Nullable=enable`: **195 errors, all in `UtilitiesCS.csproj`**, zero in
  `QuickFiler`/`QuickFiler.Test`. Breakdown: CS8766 x130, CS8618 x23, CS8625 x12, CS8600 x9,
  CS8601 x8, CS8604 x7, CS8602 x3, CS8603 x2, CS8714 x1 (issue #492). Evidence artifact:
  `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/baseline/baseline-nullable.2026-08-07T21-45.md`
- A separate session measured 195 errors, 64 of them `CS86xx`, attributed to `TaskMaster.csproj`, and
  another counted 220 `CS86xx` (issue #512). Issue #507 measured roughly 414 errors under the forced
  flag. **The attribution and the count are not stable across sessions and must be re-measured
  rather than quoted.**
- `.github/workflows/ci.yml` step "Build with nullable warnings treated as errors" already carries an
  in-line comment documenting the exact `/t:Build` skip behavior and uses `/t:Rebuild` for that reason.

### Divergent site inventory measured 2026-08-10

The four issues enumerated six nullable sites and five csharpier sites. Direct measurement finds more.
The following counts include the `AGENTS.md` / `.agents/` Codex-agent mirror of the same governance
content, whose in-scope status is a research question for this feature.

Nullable `/t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`:

| # | Site | Enumerated by an issue |
|---|---|---|
| 1 | `CLAUDE.md:206` | yes |
| 2 | `CLAUDE.md:383` | yes |
| 3 | `CLAUDE.md:401` | yes |
| 4 | `.claude/rules/csharp.md:16` | yes |
| 5 | `.claude/rules/csharp.md:83` (prose reference) | yes |
| 6 | `.claude/skills/csharp-qa-gate/SKILL.md:32` | yes |
| 7 | `.agents/skills/csharp/SKILL.md:19` | no |
| 8 | `.agents/skills/csharp-qa-gate/SKILL.md:34` | no |
| 9 | `AGENTS.md:487` | no |
| 10 | `AGENTS.md:488` | no |
| 11 | `AGENTS.md:662` | no |

CSharpier v0 bare-path syntax:

| # | Site | Enumerated by an issue |
|---|---|---|
| 1 | `CLAUDE.md:191` | yes |
| 2 | `CLAUDE.md:192` (`csharpier .` global form) | yes |
| 3 | `CLAUDE.md:381` | yes |
| 4 | `CLAUDE.md:399` | yes |
| 5 | `.claude/rules/csharp.md:14` | yes |
| 6 | `.claude/skills/csharp-qa-gate/SKILL.md:30` | no |
| 7 | `.agents/skills/csharp/SKILL.md:17` | no |
| 8 | `.agents/skills/csharp-qa-gate/SKILL.md:32` | no |
| 9 | `AGENTS.md:469` | no |
| 10 | `AGENTS.md:470` | no |
| 11 | `AGENTS.md:660` | no |

Additionally, `CLAUDE.md:188` asserts that CSharpier "formats only `*.cs` without touching project
files". Repository evidence records that CSharpier v1.x also formats `*.csproj`, `packages.config` and
`*.xml`. This adjacent rationale sentence is under evaluation as factually wrong.

### Additional finding — the documented ANALYZER step is vacuous by the same mechanism

Measured 2026-08-10, detail in `evidence/baseline/baseline-analyzer-step-vacuity.2026-08-10T14-55.md`.
None of the four issues enumerates the analyzer command, but it occupies the same documented block and
fails the same required-outcome test.

| Run | Command | EXIT | Elapsed | `Skipping target "CoreCompile"` |
|---|---|---|---|---|
| A1 | documented analyzer step, `/t:Build` | 0 | 22.1 s | 3 |
| A2 | **the same command run again immediately** | **0** | **1.5 s** | **18 of 18** |
| A3 | analyzer properties under `/t:Rebuild /m` | 0 | 19.0 s | 0 |

Analyzer diagnostics are produced during compilation, so a build that skips compilation runs no
analyzers. The general defect is that **outputs produced under one property set are silently accepted
as validating a different property set**, because MSBuild's up-to-date check compares timestamps and
does not invalidate on a command-line `/p:` change. CI is not affected in the same way: its analyzer
step runs on a fresh checkout where the build is genuinely cold.

**This requires an explicit scope decision in `spec.md`** — correct the analyzer command as well,
leave it and file a follow-up issue, or correct it and record the widening. If it is left unchanged,
the rationale for step 2 using `/t:Build` while step 3 uses `/t:Rebuild` must be stated in-line, or
the corrected documentation becomes internally misleading in a new way.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

This is the epic's Wave 0 prerequisite. Every recorded "nullable gate passed" result across prior
features overstates what was verified, because the gate did not run. Conversely, both #507 and #508
required a human-level override of a subagent's false `CS8603` blocker on 2026-08-08; without the
override each would have shipped a spurious remediation cycle. The cost recurs on every C#-touching
run until the documentation is corrected, and no other bug in the backlog can be certified against
evidence produced by a gate that cannot fail.

Severity is High rather than Blocker because the nullable debt the corrected gate exposes is
pre-existing rather than newly introduced, and CI's `/t:Rebuild` step does genuinely exercise
`TreatWarningsAsErrors`.

## Scope

### In scope

- Reconcile every site documenting the C# format command and the C# type-check command so the
  documented text matches a command that executes, enforces, and agrees with `ci.yml`.
- Correct factually wrong rationale prose immediately adjacent to those commands.
- Add a verification step that proves each documented command runs green against a clean checkout.
- Produce the negative-path proof required by #512.

### Out of scope

- **Fixing the nullable diagnostics the corrected gate exposes.** Issue #492 states the separation
  explicitly: first make the gate report truthfully, then decide how to burn down the debt. Only the
  first half is delivered here. The burn-down is a follow-on epic sized against whatever figure the
  corrected gate reports.
- Coverage thresholds and coverage exclusion policy. `CLAUDE.md` § UT2,
  `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` **must not be edited**;
  they belong to sibling feature #494 (issue 494) and editing them here would conflict.
- Issue #513 (`collect_pr_context` misclassification), which is fixed upstream in `drm-copilot`.

## Governance-Document Authorization

This feature must edit `CLAUDE.md`, `.claude/rules/csharp.md`, and
`.claude/skills/csharp-qa-gate/SKILL.md`. The `policy-compliance-order` skill hard constraint normally
prohibits modifying documents under `.claude/rules/`. The epic charter
(`docs/features/epics/build-ci-coverage-gate-fidelity/epic.md`, section "Execution Authorization
Required") suspends that constraint for this feature only, and only for the specific sites the four
issues enumerate, because the defect *is* that those documents are wrong.

Hard limits:

- Edit only the toolchain command text and its surrounding rationale at the enumerated sites.
- Do NOT relax, weaken, or delete any policy requirement in order to make a gate pass.
- Do NOT touch `CLAUDE.md` § UT2, `.claude/rules/general-unit-test.md`, or
  `.claude/rules/quality-tiers.md`.

## Acceptance Criteria

- [ ] AC1 — Every site that documents the C# format command uses a command that executes successfully
      against the CSharpier version pinned in `dotnet-tools.json`, verified by running each documented
      form and recording `EXIT_CODE: 0`.
- [ ] AC2 — Every site that documents the C# type-check command uses a command that performs a genuine
      compilation, proven by a non-vacuous compile assertion (a `csc.exe` invocation count greater than
      zero in an MSBuild file log), not by exit code alone.
- [ ] AC3 — The documented type-check command returns `EXIT_CODE: 0` against an unperturbed clean
      checkout of this branch. The gate is passable.
- [ ] AC4 — **Negative-path proof (#512).** A deliberately introduced nullable violation in a
      production file that carries a `#nullable enable` pragma causes the corrected type-check gate to
      return a non-zero exit code with the expected `CS86xx` diagnostic. The evidence artifact records
      the file, the exact perturbation, the command, the diagnostic, the exit code, and confirmation
      that the perturbation was reverted. This proof must be non-vacuous: the perturbed file's project
      must be one the corrected command genuinely recompiles.
- [ ] AC5 — The documented format command, the documented analyzer command, and the documented
      type-check command are each consistent with the corresponding step in
      `.github/workflows/ci.yml`. Any deliberate difference between a documented command and CI's
      command is stated in-line with its rationale.
- [ ] AC6 — The complete site inventory is reconciled. No site anywhere in the repository still
      documents the CSharpier v0 bare-path form, and no site still documents a `/t:Build`-based
      nullable type-check command. Verified by a repository-wide grep recorded in the evidence
      artifact. Sites deliberately left unchanged are enumerated with rationale.
- [ ] AC7 — A verification step exists that proves each documented command runs green against a clean
      checkout, and it has been executed with its output recorded as evidence.
- [ ] AC8 — No policy requirement is relaxed, weakened, or deleted. The diff contains no reduction of
      any threshold, no removal of any mandatory step, and no new suppression.
- [ ] AC9 — `CLAUDE.md` § UT2, `.claude/rules/general-unit-test.md`, and
      `.claude/rules/quality-tiers.md` are unmodified, verified by a zero-line diff against the merge
      base for those files and sections.
- [ ] AC10 — Factually incorrect rationale prose adjacent to the corrected commands is either
      corrected or explicitly recorded as verified-correct. In particular the claim at `CLAUDE.md:188`
      that CSharpier "formats only `*.cs` without touching project files" is resolved against measured
      behavior and `.csharpierignore`.
- [ ] AC11 — Baseline evidence under `evidence/baseline/` and final-QC evidence under
      `evidence/qa-gates/` exist for every command step, each recording `Timestamp:`, `Command:`,
      `EXIT_CODE:` and `Output Summary:`.
- [ ] AC12 — The nullable diagnostics exposed by the corrected gate are recorded as a measured figure
      with per-project attribution for the follow-on burn-down epic. They are **not** fixed here.
- [ ] AC13 — The documented analyzer step's vacuity is resolved by an explicit, recorded decision in
      `spec.md`: either the analyzer command is corrected alongside the type-check command, or it is
      deliberately left unchanged with the asymmetry explained in-line at the documentation site and a
      follow-up issue filed. Silent inaction does not satisfy this criterion.

## Constraints & Risks

- **The measured error count is not stable across sessions.** Recorded figures range from 195 to 414
  and disagree on whether the errors originate in `UtilitiesCS.csproj` or `TaskMaster.csproj`. Any
  figure this feature records must be re-measured in this worktree, not quoted from an issue.
- **`/t:Rebuild` costs more than `/t:Build`.** Issue #492 requires measuring the added toolchain loop
  time before committing to it rather than assuming it is acceptable.
- **A workflow change is expensive.** `.claude/rules/ci-workflows.md` and the
  `modified-workflow-needs-green-run` policy rule require a green workflow run against the branch head
  before any workflow change can merge. Epic-child pull requests target the integration branch and
  `ci.yml` triggers only on pull requests to `main`/`development`, so a green run may be unobtainable
  on this branch. Changing `ci.yml` is therefore a design option requiring explicit justification, not
  a default.
- **Scope creep into the debt burn-down is the principal risk.** A corrected, executing, passing gate
  is the deliverable. Fixing diagnostics is not.

## Source

- `docs/features/potential/promoted/2026-08-07-nullable-gate-masked-by-incremental-build.md` (#492)
- `docs/features/potential/promoted/2026-08-08-csharpier-documented-command-incompatible-with-pinned-version.md` (#509)
- `docs/features/potential/promoted/2026-08-08-nullable-gate-cannot-fail-incremental-build.md` (#512)
- `docs/features/potential/promoted/2026-08-08-claudemd-nullable-gate-diverges-from-ci.md` (#522)

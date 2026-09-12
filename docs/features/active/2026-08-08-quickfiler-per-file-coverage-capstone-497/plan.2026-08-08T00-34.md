# Atomic Plan — quickfiler-per-file-coverage-capstone (Issue #497, epic child F16)

- **Feature folder (`<FEATURE>`):** `docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497`
- **Epic:** `quickfiler-per-file-coverage` (parent epic issue #136), child **F16** (capstone), wave 2, band C3
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`
- **Branch:** `feature/quickfiler-per-file-coverage-capstone-r2`
- **Work Mode:** `full-feature` — `spec.md` **and** `user-story.md` are together the authoritative acceptance-criteria source (AC1-AC12 in each, checked off together), plus `spec.md` § Definition of Done
- **Worktree:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a24c84de174a27784`
- **Plan file (updated in place):** `<FEATURE>/plan.2026-08-08T00-34.md`
- **Upstream dependencies:** all fourteen sibling children merged to the integration branch; F1's ledger and harness — consumed at **execution time**, never at planning or preflight time
- `EVIDENCE_LOCATION_OVERRIDE_REJECTED:` none — the delegation supplied only canonical `<FEATURE>/evidence/<kind>/` paths.

## What This Child Is

F16 **verifies a finished state**. It owns no production `.cs` file, produces no coverage, and
remediates nothing. Its deliverable is evidence. If a sibling's work is short, this plan reports it
as a Blocking finding naming the owning child. **No task in this plan may fix a sibling's coverage,
and no task may grant an exemption to close a gap.** That is the exact failure mode the epic's
policy reconciliation exists to prevent (`epic.md` § Policy reconciliation; `spec.md` AC9;
`user-story.md` § Non-Goals).

## Phase Map and Mechanical Task Counts

Counts are the number of lines matching `^- \[ \] \[P<n>-T\d+\]` in each phase.

| Phase | Title | Tasks | Primary ACs closed |
| --- | --- | --- | --- |
| 0 | Compliance, Restore, Upstream Dependency Confirmation, and Baseline Capture | 21 | baseline contract |
| 1 | Harness Trust Gate for F1's Delivered Coverage Harness | 8 | AC10 |
| 2 | Execution-Time Denominator Re-Derivation and Ledger Reconciliation | 9 | AC1 |
| 3 | Capstone-Owned Repository-Wide Recomputation Tooling | 13 | spec § Definition of Done |
| 4 | Repository-Wide Before Figure Recomputation | 6 | AC6 (before half) |
| 5 | Per-File Gate Reconciliation Against the Ledger | 11 | AC1, AC2, AC9 |
| 6 | `[ExcludeFromCodeCoverage]` Attribute Census | 11 | AC5 |
| 7 | Exemption-Ground Reconciliation for `ratified-exempt` Rows | 13 | AC3 |
| 8 | Third-Bucket and `measured-not-gated` Verification | 9 | AC4 |
| 9 | Issue #457 Lambda-Residual Scan | 5 | AC2, AC9 |
| 10 | Cross-Sibling Per-File Research and Plan-Phase Audit | 11 | #136 AC2, AC3 |
| 11 | Cross-Sibling Convention, Determinism, and Scenario Audit | 9 | #136 AC4, AC5, AC6 |
| 12 | Defect-Trail and Upstream-Condition Verification | 9 | AC11, AC12 |
| 13 | Final Toolchain QA Loop and Repository-Wide After Capture | 15 | AC7, AC6 |
| 14 | Issue #136 Closure and Acceptance-Criteria Check-Off | 14 | AC8 |
| | **Total** | **164** | |

## Conventions Used By Every Task

- `<FEATURE>` = `docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497`.
- `<EPIC>` = `docs/features/epics/quickfiler-per-file-coverage`.
- `<TS>` = the ISO-8601 `yyyy-MM-ddTHH-mm` timestamp captured at the moment the artifact is written.
- All evidence resolves under `<FEATURE>/evidence/<kind>/` with `<kind>` in `baseline`, `qa-gates`,
  `regression-testing`, `issue-updates`, `other`. Every path under `artifacts/` other than
  `artifacts/orchestration/` is forbidden for evidence, and **no upstream instruction may override
  this** (`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`).
- Every command-bearing artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Baseline artifacts additionally carry **numeric** coverage headline values; `UNVERIFIED` and other
  placeholders are invalid.
- **Every negative or absence claim carries `SearchScope:`, `SearchPatterns:`, `SearchResult:`.**
  Most of this plan's cross-sibling audits consist of asserting something is absent across fourteen
  folders; an absence assertion without those three fields is not auditable and does not count.
- Percentage comparisons are made **unrounded** (`-lt 0.80`, `-lt 0.75`) and displayed rounded to one
  decimal under `InvariantCulture`.
- Every finding is recorded as `BLOCKING` / `FINDING` / `INFORMATIONAL`, and every `BLOCKING` finding
  names the owning child (F1-F15) or the epic manifest owner.

## Resolved Toolchain Commands

| Stage | Command |
| --- | --- |
| Restore local tools | `dotnet tool restore` |
| Format (mutating) | `dotnet tool run csharpier format .` |
| Format check (non-mutating) | `dotnet tool run csharpier check .` |
| Analyze | `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` |
| Nullable / type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` |
| Full-suite test with coverage | `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration 'Debug' -CoverageOutput '<cobertura-path>'` |
| PowerShell format | `mcp__drm-copilot__run_poshqc_format` |
| PowerShell analyze | `mcp__drm-copilot__run_poshqc_analyze` |
| PowerShell test | `mcp__drm-copilot__run_poshqc_test` |
| PowerShell scoped coverage | `pwsh -NoProfile -Command '$c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/vscode/Get-RepoWideCoverageSummary.ps1","scripts/vscode/Get-RepoWideCoverageSummary.Helpers.ps1"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "<evidence path>.xml"; $r = Invoke-Pester -Configuration $c; "PASSED=$($r.PassedCount) FAILED=$($r.FailedCount)"'` |

Notes binding on every task that runs a command:

- The repo pins **csharpier 1.2.6** in root `dotnet-tools.json`. Its v1 CLI **requires a
  subcommand**; the bare `csharpier .` form in `CLAUDE.md` §C#1/§CUT3 is v0 syntax and fails. The
  deviation is recorded in evidence ([P13-T14]); **`CLAUDE.md` is not amended**.
- The nullable gate uses **`/t:Rebuild`**. MSBuild's incremental up-to-date check does not invalidate
  on a command-line property change alone, so `/t:Build` silently skips recompilation and never
  enforces the gate (issue **#492**). **Do not pass `/p:Nullable=enable` solution-wide** — the
  solution-wide form emits `CS8630` on `QuickFiler.Test` (C# 7.3, no `<LangVersion>`). CI's proven
  form relies on each file's own `#nullable enable` pragma plus `TreatWarningsAsErrors`.
- `Platform` must be quoted with the space: `"/p:Platform=Any CPU"`.
- `msbuild` and `vstest.console.exe` are **not on PATH** and resolve through
  `${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe`. `dotnet-coverage` must be
  a **global** tool on PATH; the runner throws if `Get-Command 'dotnet-coverage'` fails.
- The coverage runner's test-assembly discovery filters only `\bin\<Config>\`, `\obj\`, `\ref\` — it
  has **no `.claude/worktrees` exclusion**. Invoke it from **inside this worktree**, where
  `.claude/worktrees/` does not exist, never from the canonical repo root. The guard in [P0-T18] is
  expressed **repo-relative** to `(Resolve-Path .)`, not as an absolute `\.claude\` match, because an
  absolute match would flag this worktree's own path and could never pass.
- `.csharpierignore` excludes `*.csproj`, `*.props`, `*.targets`, `**/evidence/**`, `*.cobertura.xml`,
  `*.coverage`, `*.coveragexml`, `*.trx`. It does **not** exclude `*.cs`. It does **not** exclude
  `*.ps1` or `*.md`, but csharpier only formats `*.cs`, so Markdown evidence written after the final
  pass cannot invalidate it.

## Decisions Record — Settled, Do Not Re-Open

| # | Decision | Basis |
| --- | --- | --- |
| D-1 | **F1's ledger and harness are execution-time dependencies.** No task in this plan carries a planning-time or preflight-time assertion that `<EPIC>/coverage-ledger.json`, `<EPIC>/coverage-ledger.md`, `scripts/vscode/Get-PerFileCoverage.ps1`, or `scripts/vscode/Get-PerFileCoverage.Helpers.ps1` exists. If any is genuinely absent when [P0-T15]/[P0-T16] run, that is an **epic-orchestrator sequencing failure to be raised at that moment**, not a planning or preflight defect. | F7 precedent (`plan.2026-08-07T20-41.md:68`); `spec.md` § Implementation Strategy 1; research 2 § 0.1 |
| D-2 | **F1 does not fix #441 or #478, and that is NOT a Blocking finding.** F1's plan places both explicitly out of scope and asserts zero changes to `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`; it only avoids reproducing them in its own new computation. A Blocking finding arises **only** if F1's *delivered* harness itself reads a descendant axis or an emitted rate attribute. | `spec.md` DEV-2; research 1 § Q2; F1 `plan:78`, `:565`, `spec.md:234` |
| D-3 | **The epic's explanation of the 70.19% → 85.65% swing is wrong and must not be repeated.** The stated second cause ("the two runs did not even instrument the same body of code") is refuted: the two artifacts are the raw and post-processed stages of one pipeline, and the `lines-valid` growth is the #441 double count, proven by a literal `<line number=` count matching `lines-valid` exactly. Repeating the epic's explanation would propagate an error into the closing evidence. | `spec.md` DEV-3; research 1 § Q3 |
| D-4 | **The denominator-instability claim is unconfirmed.** Since the `lines-valid` growth is fully explained by post-processing, the claim is unsupported. The before/after pair is still captured in one session with the identical command, so the pair is defensible either way. | `spec.md` DEV-4; research 1 § Unverified |
| D-5 | **The attribute census is per-TYPE, never per-file.** `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` declares four types and carries one type-level attribute on a secondary type only; it is *partially* suppressed and contributes **0** to the suppressed-file count, not 1. A one-attribute-equals-one-file implementation computes 25 and then "corrects" the epic in the wrong direction. | `spec.md` DEV-5; research 2 § Q1.4 |
| D-6 | **The epic manifest's placeholder issue numbers are REPORTED, not repaired.** `epic.md` YAML front matter currently reads `issue_num: 1012` (F12), `1015` (F15), `1016` (F16) against real **495**, **496**, **497**, and F16's own `depends_on` list names `1012` and `1015`. `epic.md` is not a per-child owned file; F16 verifies the repair and never performs it. | `spec.md` DEV-6; research 2 § Q7.1; epic.md:55-83 |
| D-7 | **AC2's per-file obligation extends to third-bucket files.** F2, F3, F6, and F7 all produced research artifacts for their interface files and F7 gave them their own plan phases; a research artifact is precisely what *establishes* third-bucket membership. Under that reading F13's four missing interface artifacts are a shortfall **reported as a finding naming F13**, never closed by F16 writing the four artifacts. | `spec.md` DEV-7; research 2 § Q5.2 |
| D-8 | **The repository-wide figure is recomputed by capstone-owned tooling, never transcribed.** No correct producer exists in the repository: `Get-CoberturaCoverageSummary` carries #441 (descendant axis at `Invoke-MSTestWithCoverage.Helpers.ps1:122`), `Merge-CoberturaClassesByFilename` carries #478 (`:200`, `:270-276`), `scripts/temp-extract-coverage.ps1` is worse than either, the JaCoCo feature-review hook is the wrong format, and F1's harness is package-scoped to QuickFiler by design and cannot emit a repo-wide figure at all. | `spec.md` § Data & State; research 1 § Q2, § Q6, § Findings 1 |
| D-9 | **Capstone-owned tooling is PowerShell, at `scripts/vscode/`, with tests at `tests/scripts/vscode/`.** Every repo-tooling script lives at `scripts/vscode/`; F1's harness is PowerShell; `.claude/rules/powershell.md` supplies a full toolchain and testing standard; `tests/` must mirror the production tree and colocation in `scripts/` is prohibited. The change budget cap of 2 production PowerShell files is met exactly. | research 1 § Q6; `.claude/rules/powershell.md:37-41`, `:57-58`; `.claude/rules/general-unit-test.md` § Test File Location |
| D-10 | **The denominator is derived from `<Compile Include=` only, never from a filesystem glob.** 156 `.cs` files exist under `QuickFiler/` against 121 compiled at planning time; a glob falsely flags `QuickFiler/Helper Classes/FormFocusListener.cs` plus 20 orphan viewer files as unledgered. | `epic.md` § Mid-Wave File Creation rule 1; research 1 § Q1 |
| D-11 | **Per-file reconciliation runs against the Phase 0 before-artifact, and Phase 13 proves verdict stability against the after-artifact.** F16 changes no production code, so the two must agree; [P13-T13] is the empirical confirmation and any divergence is itself a finding. | `spec.md` § Behavior 4; AC6 |
| D-12 | **The AC8 closure phase runs AFTER the final QA loop.** AC8 requires each of issue #136's eight criteria to cite the artifact that closes it, and two of those artifacts (the AC7 toolchain stages and the AC6 after-figure comparison) are produced by the final QA loop. Placing closure first would force a forward reference to artifacts that do not yet exist. Only Markdown under `<FEATURE>/evidence/` is written after the final pass, and `.csharpierignore` excludes `**/evidence/**`, so nothing written in Phase 14 can invalidate Phase 13. | `spec.md` AC7, AC8; `.csharpierignore` |

## Hard Prohibitions — Binding On Every Task

1. **No task builds the issue #230 WinForms message-pump seam.** That work is tracked, deliberate,
   and deferred (`epic.md` § Correction: a prior maintainer ratification).
2. **No task re-litigates issue #227's ratified attributes.** Record provenance
   (`ratified-by-maintainer (#227)`) and report; do not remove and do not re-adjudicate.
3. **No task treats #230's nine deferred attributes as a gap or a merge condition.**
4. **No task reads an emitted `line-rate` / `branch-rate` / `lines-valid` / `branches-valid`
   attribute as a per-file or repository-wide figure.** They are corrupt in both directions, so no
   correction factor exists: `FocusAndTheme.cs` emits a rate over 373 lines for a 326-line file;
   `MailActions.cs` emits `branch-rate="0.75"`, falsely passing the gate against a true 72.7%;
   `QfcHomeController.Iteration.cs` emits `0.8625` where the true class-level union is `80.36%` — on
   a file whose gate is 80%. Emitted attributes may be reported **only** labelled as the tool's own
   defective figure, alongside the recomputed value.
5. **No task imports a repository-wide figure from another branch, tool, or artifact.** The withdrawn
   70.19% merge-base figure is not a valid reference.
6. **No task edits `docs/features/epics/quickfiler-per-file-coverage/epic.md`, `CLAUDE.md`, or
   anything under `.claude/rules/`.**
7. **No task promotes F4's defects on F4's behalf.** Promotion is the owning child's obligation;
   F16's mandate is verification, and absence is a Blocking finding naming F4.
8. **No task adds an `[ExcludeFromCodeCoverage]` attribute, a `coverage.config` exclude, a
   `.runsettings` exclude, or a ledger `ratified-exempt` row.**
9. **No task writes a test whose purpose is to manufacture coverage for a third-bucket file**
   (interface shape-assertion and reflection-shape tests are prohibited).

## Files This Child May Write

- Everything under `<FEATURE>/` (this plan, `spec.md` and `user-story.md` checkbox state, and all
  evidence).
- `scripts/vscode/Get-RepoWideCoverageSummary.Helpers.ps1` and
  `scripts/vscode/Get-RepoWideCoverageSummary.ps1` (new, capstone-owned).
- `tests/scripts/vscode/Get-RepoWideCoverageSummary.Helpers.Tests.ps1` and
  `tests/scripts/vscode/Get-RepoWideCoverageSummary.Tests.ps1` (new, capstone-owned).

No other file in the repository is written by this child. In particular: no `QuickFiler/**/*.cs`, no
`QuickFiler.Test/**/*.cs`, no `QuickFiler/QuickFiler.csproj`, no `QuickFiler.Test/QuickFiler.Test.csproj`,
no `<EPIC>/coverage-ledger.json`, no `<EPIC>/coverage-ledger.md`, no
`scripts/vscode/Invoke-MSTestWithCoverage*.ps1`, no `scripts/vscode/Get-PerFileCoverage*.ps1`, and no
sibling feature folder.

---

### Phase 0 — Compliance, Restore, Upstream Dependency Confirmation, and Baseline Capture

- [ ] [P0-T1] Read `CLAUDE.md` in full and create `<FEATURE>/evidence/baseline/phase0-instructions-read.<TS>.md` containing `Timestamp:`, `Policy Order:` (the `policy-compliance-order` sequence), and an explicit list of files read with `CLAUDE.md` as the first entry.
  - Acceptance: the artifact exists at that exact path, contains all three fields, and records the §UT2 coverage exemption grounds (a), (b), (c) and the §CUT3 toolchain command list as the two clauses that bind this child.
- [ ] [P0-T2] Read `.claude/rules/general-code-change.md` in full and append it to the file list in `<FEATURE>/evidence/baseline/phase0-instructions-read.<TS>.md`, recording the 500-line file-size limit and the mandatory toolchain-loop ordering.
  - Acceptance: the artifact lists `.claude/rules/general-code-change.md` and names both clauses.
- [ ] [P0-T3] Read `.claude/rules/general-unit-test.md` in full and append it to the same artifact, recording the >= 85% line / >= 75% branch thresholds, the Coverage Exclusion Policy, the Test File Location rule, and the Determinism Infrastructure banned-API list.
  - Acceptance: the artifact lists `.claude/rules/general-unit-test.md` and names all four clauses.
- [ ] [P0-T4] Read `.claude/rules/csharp.md` in full and append it to the same artifact, recording the seam hierarchy (interface > injectable delegate > adapter) and the stated toolchain command forms.
  - Acceptance: the artifact lists `.claude/rules/csharp.md` and records that its `csharpier .` form is stale for the pinned 1.2.6 tool.
- [ ] [P0-T5] Read `.claude/rules/powershell.md` in full and append it to the same artifact, recording the format → analyze → test order, the `*.Tests.ps1` mirrored-path convention, the 500-line limit, and the change budget of at most 2 production PowerShell files plus tests.
  - Acceptance: the artifact lists `.claude/rules/powershell.md` and names all four clauses.
- [ ] [P0-T6] Read `.claude/rules/quality-tiers.md` in full and append it to the same artifact, recording the uniform line >= 85% / branch >= 75% figures and the tier-dependent gate matrix.
  - Acceptance: the artifact lists `.claude/rules/quality-tiers.md` and records both uniform thresholds.
- [ ] [P0-T7] Read `.claude/skills/policy-compliance-order/SKILL.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, and `.claude/skills/acceptance-criteria-tracking/SKILL.md` and append all four to the same artifact.
  - Acceptance: the artifact lists all four skill paths and records the five canonical evidence sub-paths plus the `SearchScope:` / `SearchPatterns:` / `SearchResult:` requirement for negative claims.
- [ ] [P0-T8] Read `<FEATURE>/issue.md`, `<FEATURE>/spec.md`, and `<FEATURE>/user-story.md` in full and append all three to the same artifact with the acceptance-criterion counts found in each.
  - Acceptance: the artifact records `Work Mode: full-feature`, 12 acceptance criteria in `spec.md` (AC1-AC12), 12 in `user-story.md` (AC1-AC12), 6 `## Definition of Done` checkboxes in `spec.md`, and 5 `## Test Conditions` checkboxes in `spec.md`. Any divergence from those counts halts and is recorded before any later phase runs.
- [ ] [P0-T9] Read `<FEATURE>/research/measurement-harness-and-denominator.2026-08-08T00-45.md` and `<FEATURE>/research/exemption-reconciliation-and-ac-closure.2026-08-08T00-45.md` in full and append both to the same artifact.
  - Acceptance: the artifact lists both research paths and records the six items in research 1 § "Unverified — requires execution at capstone time" and the nine items in research 2 § "what could NOT be verified" as the claims this plan must re-establish at execution time rather than inherit.
- [ ] [P0-T10] Read `docs/features/epics/quickfiler-per-file-coverage/epic.md` in full and append it to the same artifact.
  - Acceptance: the artifact lists the epic path and enumerates the rulings F16 must verify: the third ledger bucket, the **five numbered harness correctness requirements listed under `epic.md` § "Two harness correctness requirements" (`epic.md:638-663`)** — recording the heading-versus-list-count discrepancy (a heading naming two over a list of five) as an `INFORMATIONAL` finding against the epic manifest owner, reported and not repaired, since `epic.md` is not edited by this child — the fourth exemption ground, the #457 trap, DEC-1 (STA Form construction), DEC-5 (`measured-not-gated`), the zero-branch N/A rule, the F6 dead-region deletion, the F4 promotion list, and the five Mid-Wave File Creation rules.
- [ ] [P0-T11] Record the branch state into `<FEATURE>/evidence/baseline/branch-state.<TS>.md` using `git rev-parse HEAD`, `git rev-parse --abbrev-ref HEAD`, `git merge-base HEAD origin/epic/quickfiler-per-file-coverage-integration`, and `git status --porcelain`.
  - Acceptance: the artifact records the HEAD SHA **as an observation, not as an expectation**, the branch name, the merge-base SHA, the **verbatim `git status --porcelain` output**, and all four schema fields. The cleanliness gate is **scoped, never absolute**: no path outside `<FEATURE>/` may be modified or untracked at this point, and the entries under `<FEATURE>/evidence/` created by [P0-T1] through [P0-T10] are expected, are enumerated in the artifact, and do not fail the gate. `<FEATURE>/evidence/` is not gitignored, so an absolute empty-porcelain assertion could never pass once Phase 0 has written its first artifact. Later tasks gate on the same scoped invariant — no `.cs`, `.csproj`, `packages.config`, or `app.config` diff against this recorded SHA, and no modification outside the write-allowed set in § Files This Child May Write — never on the SHA's literal value and never on absolute porcelain emptiness.
- [ ] [P0-T12] Run `dotnet tool restore` from the worktree root and record `<FEATURE>/evidence/baseline/tool-restore.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`, and `Output Summary:` records the resolved csharpier version from root `dotnet-tools.json` (expected `1.2.6`). This task must complete before any task that runs csharpier.
- [ ] [P0-T13] Probe and record the toolchain into `<FEATURE>/evidence/baseline/toolchain-resolution.<TS>.md`: resolve `msbuild` via `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe'`; resolve `vstest.console.exe` via the same `vswhere.exe` with `-find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`; confirm `Get-Command dotnet-coverage` resolves; confirm `Get-Command pwsh` resolves; run `dotnet tool run csharpier --version`.
  - Acceptance: the artifact records a resolved absolute path or a version string for all five, each with its own `EXIT_CODE`. If `dotnet-coverage` is absent, install it with `dotnet tool install --global dotnet-coverage` and record that command and its exit code in the same artifact. This task must complete before any later toolchain task.
- [ ] [P0-T14] Confirm the fourteen sibling children have merged by recording, into `<FEATURE>/evidence/baseline/sibling-fan-in.<TS>.md`, the resolved feature-folder path for each `features[]` entry in `<EPIC>/epic.md` other than F16.
  - Acceptance: the artifact lists all fourteen `feature_folder` values from the manifest, the directory each resolves to under `docs/features/active/` or `docs/features/completed/`, and `SearchScope:` / `SearchPatterns:` / `SearchResult:` for every one that does not resolve. Manifest folder names are known to be stale for at least F2, F12, and F15, so resolution is by issue-number suffix first and slug match second, and the resolution rule used is written into the artifact. An unresolved child is recorded as a `BLOCKING` epic-sequencing finding, not repaired.
- [ ] [P0-T15] Confirm F1's ledger exists on the branch by reading `<EPIC>/coverage-ledger.json` and `<EPIC>/coverage-ledger.md` and recording their existence, byte size, row count, `schema_version`, `generated_from`, `source_commit`, `threshold_percent`, `branch_threshold_percent`, `new_file_line_target_percent`, and the observed set of `classification` and `exempt_ground` enum values into `<FEATURE>/evidence/baseline/f1-ledger-contract.<TS>.md`.
  - Acceptance: the artifact records all listed fields verbatim from the delivered file and the complete key set of one representative `files[]` row. **This is an execution-time dependency read.** If either file is absent, stop and raise an epic-orchestrator sequencing failure at that moment; do not author, synthesise, or substitute a ledger. Record whether the delivered `exempt_ground` enum carries a value for the epic's fourth ground (prohibited-to-execute adapters) — F1's spec enum as written has only three values, and `WebView2CoreInitializer.cs` would then have no valid value to carry.
- [ ] [P0-T16] Confirm F1's harness exists by recording the existence, byte size, line count, and SHA-256 of `scripts/vscode/Get-PerFileCoverage.ps1` and `scripts/vscode/Get-PerFileCoverage.Helpers.ps1` into `<FEATURE>/evidence/baseline/f1-harness-presence.<TS>.md`.
  - Acceptance: both files are recorded with all four values plus the `Get-FileHash` command and `EXIT_CODE`. **Execution-time dependency**; absence is an epic-orchestrator sequencing failure raised at that moment. No task in this plan constructs a substitute per-file harness.
- [ ] [P0-T17] Run `dotnet tool run csharpier check .` and record `<FEATURE>/evidence/baseline/csharpier-check.<TS>.md`.
  - Acceptance: all four schema fields present, with the count of files reported unformatted in `Output Summary:`. This is the baseline the Phase 13 formatting stage is compared against.
- [ ] [P0-T18] Verify the stale-worktree guard before any coverage run: assert that `Join-Path (Resolve-Path .) '.claude/worktrees'` does not exist, and record the result into `<FEATURE>/evidence/baseline/stale-worktree-guard.<TS>.md`.
  - Acceptance: the artifact records the resolved current directory, the tested path, the boolean result, and `SearchScope:` / `SearchPatterns:` / `SearchResult:`. The test is **repo-relative to `(Resolve-Path .)`**, never an absolute match on `\.claude\`, because an absolute match would flag this worktree's own path and could never pass after a build. If the path exists, the coverage runner must not be invoked from here.
- [ ] [P0-T19] Run the analyzer build `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record `<FEATURE>/evidence/baseline/msbuild-analyze.<TS>.md`.
  - Acceptance: all four schema fields with the warning and error counts in `Output Summary:`. This is the baseline [P13-T5] is compared against; a Phase 13 non-zero exit is evaluated **baseline-relative** (new diagnostics only), and any pre-existing diagnostic is recorded as `INFORMATIONAL` with its owning file.
- [ ] [P0-T20] Run the nullable build `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` and record `<FEATURE>/evidence/baseline/msbuild-nullable.<TS>.md`.
  - Acceptance: all four schema fields with the warning and error counts in `Output Summary:`. This is the baseline [P13-T6] is compared against, evaluated the same baseline-relative way. `/t:Rebuild` is required because MSBuild's incremental up-to-date check does not invalidate on a command-line property change alone (issue **#492**), and `/p:Nullable=enable` is deliberately not passed solution-wide.
- [ ] [P0-T21] Run the coverage-enabled full suite from inside this worktree with `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration 'Debug' -CoverageOutput 'docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497/evidence/baseline/repo-coverage-before.<TS>.cobertura.xml'` and record `<FEATURE>/evidence/baseline/repo-coverage-before.<TS>.md`.
  - Acceptance: the Cobertura XML exists at the stated canonical path; the `.md` carries all four schema fields plus **numeric** headline values — total tests, passed, failed, skipped, the `<package>` name list and count, and the emitted root `line-rate` / `branch-rate` / `lines-valid` / `branches-valid` labelled verbatim `TOOL-EMITTED (#441-corrupted, NOT the authoritative figure)`. The authoritative recomputed figure is produced in Phase 4 from the same XML. This is the exact command re-used verbatim in [P13-T7]; the only difference permitted between the two invocations is the `-CoverageOutput` path.

---

### Phase 1 — Harness Trust Gate for F1's Delivered Coverage Harness

No figure emitted by F1's harness is used anywhere in this plan until this phase returns `TRUSTED`.

- [ ] [P1-T1] Create `<FEATURE>/evidence/qa-gates/harness-trust-gate.<TS>.md` with the harness identity: path, byte size, line count, and SHA-256 of `scripts/vscode/Get-PerFileCoverage.ps1` and `scripts/vscode/Get-PerFileCoverage.Helpers.ps1`, carried forward from [P0-T16], plus `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  - Acceptance: the artifact exists with both identities recorded and the four schema fields.
- [ ] [P1-T2] Scan both harness files for a descendant-axis line selection and append the result to `<FEATURE>/evidence/qa-gates/harness-trust-gate.<TS>.md`.
  - Patterns: `\.//lines/line`, `\.//line`, `SelectNodes\('\.//`, `SelectNodes\("//`, `//lines/line`, `GetElementsByTagName\('line'\)`, `\.iter\('line'\)`, `descendant::line`.
  - Acceptance: the appended section carries `SearchScope:` (both file paths), `SearchPatterns:` (the eight patterns verbatim), and `SearchResult:` (matching `file:line` or `none`). **Any match is a `BLOCKING` finding naming F1** and the phase verdict is `BLOCKED`.
- [ ] [P1-T3] Scan both harness files for any **read** of an emitted rate or count attribute and append the result to the same artifact.
  - Patterns: `line-rate`, `branch-rate`, `lines-valid`, `lines-covered`, `branches-valid`, `branches-covered`.
  - Acceptance: every hit is individually classified as `READ` (used as a coverage figure), `WRITE` (emitted by the harness), or `MENTION` (comment or string literal), with the surrounding line quoted. **Any `READ` classification is a `BLOCKING` finding naming F1.** `SearchScope:` / `SearchPatterns:` / `SearchResult:` are required even when the result is `none`.
- [ ] [P1-T4] Confirm the harness reads only the class-level `<lines>` block by locating the axis it actually uses and quoting the `file:line`, and append to the same artifact.
  - Acceptance: an explicit `./lines/line` (or equivalent direct-child) selection is quoted with its `file:line`. Absence of any such selection, or presence of a different axis, is `BLOCKING` naming F1.
- [ ] [P1-T5] Confirm the filename-union-with-max-hits behavior by locating (a) the grouping of `<class>` elements by `filename` and (b) the per-line-number deduplication taking `MAX(@hits)`, quoting both `file:line` spans, and append to the same artifact.
  - Acceptance: both behaviors are quoted with their `file:line`. A missing union, or a dedupe that takes first-wins or last-wins instead of max-hits, is `BLOCKING` naming F1. Also record whether the richer `condition-coverage` is retained on a collision (larger denominator, then larger numerator).
- [ ] [P1-T6] Record the live state of the two open harness defects and append to the same artifact: quote `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` at the `SelectNodes('.//lines/line')` site inside `Get-CoberturaCoverageSummary` (#441) and at the `$primaryNode.CloneNode($true)` / recomputation sites inside `Merge-CoberturaClassesByFilename` (#478), verifying the line numbers by reading the file at execution time rather than transcribing a research figure; then record the current state of issues **#441** and **#478** with `gh issue view 441 --json number,title,state` and `gh issue view 478 --json number,title,state`.
  - Acceptance: both code sites are quoted with re-verified line numbers, both issue states are recorded, and the artifact states explicitly and verbatim: **"That #441 and #478 remain open in `Invoke-MSTestWithCoverage.Helpers.ps1` is recorded as `INFORMATIONAL` and is NOT a Blocking finding against F1 (D-2)."** If `gh` is unavailable, record `POSTING BLOCKED`-style provenance with the substitute source used.
- [ ] [P1-T7] Record F1's delivered harness invocation contract into the same artifact by reading the delivered `param(...)` block and exit-code paths: every parameter name with its type, mandatory flag, and default; the exit-code semantics; and the field names emitted per file.
  - Acceptance: the contract is transcribed from the **delivered** file, not from F1's `spec.md`. The recorded invocation string is the one every later task uses; no later task invents parameters. Record explicitly whether the harness implements the `UNLEDGERED` and `NO DATA` row states and the zero-branch `N/A` rule.
- [ ] [P1-T8] Write the phase verdict `TRUSTED` or `BLOCKED` into `<FEATURE>/evidence/qa-gates/harness-trust-gate.<TS>.md`, closing `spec.md` AC10.
  - Acceptance: the verdict is one of the two literals, is justified by citing [P1-T2] through [P1-T5], and states that no figure from this harness is used before this verdict. A `BLOCKED` verdict stops every task that consumes a harness figure and is reported to the epic orchestrator as a Blocking finding naming F1.

---

### Phase 2 — Execution-Time Denominator Re-Derivation and Ledger Reconciliation

- [ ] [P2-T1] Parse `QuickFiler/QuickFiler.csproj` at execution time for every `<Compile Include="([^"]+)"` value and write the derived set with its total count and per-directory split (`Controllers\`, `Helper Classes\`, `Interfaces\`, `Properties\`, `Viewers\`) into `<FEATURE>/evidence/qa-gates/denominator-rederivation.<TS>.md`.
  - Parsing rules, all mandatory: key on the literal `<Compile Include=` and nothing else; use a line-oriented or namespace-aware read; the project namespace is the non-default `http://schemas.microsoft.com/developer/msbuild/2003`, so a bare `//Compile` XPath returns **zero** nodes; the file is CRLF-terminated, so no regex may be anchored with `/>$`.
  - Acceptance: the artifact records the total, the five per-directory counts summing to the total, the parsing rule used verbatim, and all four schema fields.
- [ ] [P2-T2] Prove the parser handles the long-form `<Compile>` entries that carry child elements rather than a self-closing tag, and append the proof to `<FEATURE>/evidence/qa-gates/denominator-rederivation.<TS>.md`.
  - Acceptance: the derived set contains `Properties\Resources.Designer.cs` (carries `AutoGen`/`DesignTime`/`DependentUpon`) and `Viewers\ItemViewer.Designer.cs` (carries `DependentUpon`), and the artifact records the count of long-form entries found. A parser requiring `/>` on the same line misses these; if either is absent from the derived set, the parser is wrong and must be corrected before any later task consumes the set.
- [ ] [P2-T3] Prove the parser excludes non-`Compile` item types, and append the proof to the same artifact.
  - Acceptance: the artifact records the count of `<(EmbeddedResource|None|Content|Page)\s` occurrences in `QuickFiler.csproj`, records the count of `<(Compile|EmbeddedResource|None|Content|Page)\s` occurrences, and asserts the intersection between the derived `Compile` set and the `EmbeddedResource`/`None`/`Content` item values is empty with `SearchScope:` / `SearchPatterns:` / `SearchResult:`. A broadened element pattern is known to pull in roughly 50 extra items.
- [ ] [P2-T4] Map every derived `Include` value to its Cobertura `filename` form by literal `"QuickFiler\" + Include` concatenation and append the mapping rule and a five-row sample to the same artifact.
  - Acceptance: the artifact states the concatenation rule verbatim, records that both sides are normalised to backslash and compared `OrdinalIgnoreCase`, and notes that a **raw** (un-post-processed) report instead carries an absolute worktree-rooted path, so the mapping is valid only against a post-processed artifact whose `<sources>` element is present.
- [ ] [P2-T5] Record the filesystem-glob negative control into the same artifact: count `.cs` files on disk under `QuickFiler/`, subtract the compiled count, and enumerate the difference.
  - Acceptance: the artifact records both counts, enumerates the uncompiled files, and states verbatim that **the denominator is derived from the csproj and never from a filesystem glob**. If `QuickFiler/Helper Classes/FormFocusListener.cs` and the orphan `Viewers/` files are present on disk and absent from the compile set, they are recorded as `INFORMATIONAL` — they are outside the denominator and outside every child's mandate, and a glob-based reconciliation would falsely flag them.
- [ ] [P2-T6] Diff the derived compile set against F1's ledger in the csproj → ledger direction and record every compiled file with **no** ledger row into `<FEATURE>/evidence/qa-gates/denominator-rederivation.<TS>.md`.
  - Acceptance: the artifact carries `SearchScope:` (`<EPIC>/coverage-ledger.json`), `SearchPatterns:` (the exact key used for row lookup, with the separator normalisation applied), and `SearchResult:` (the unledgered paths or `none`). **Any unledgered compiled file fails the report closed** and is a `BLOCKING` finding naming the child that created the file, determined in [P2-T8].
- [ ] [P2-T7] Diff in the ledger → csproj direction and record every ledger row whose file is no longer in the compile set into the same artifact.
  - Acceptance: the artifact records the orphaned rows or `none`, with the three search fields. An orphaned row is a `FINDING` naming the child that removed or renamed the file, not an error F16 repairs.
- [ ] [P2-T8] Compare the re-derived file count against the 121-file planning-time figure and attribute every added file to its creating child, appending the result to the same artifact.
  - Method: read `<EPIC>/coverage-ledger.json`'s `source_commit`, obtain that revision's csproj with `git show <source_commit>:QuickFiler/QuickFiler.csproj`, derive its `Compile Include` set with the identical parser, and diff the two sets; attribute each added path with `git log --follow --format='%h %an %s' -- <path>`.
  - Acceptance: the artifact records the re-derived count, the 121 planning-time figure, the signed divergence, and a per-added-file attribution table. A divergence is **expected and reported, never treated as an error** — F2, F3, F7, F9, and F11 all add `<Compile Include>` entries mid-wave. Every added file is checked for a ledger row and for the `>= 90%` new-file line target.
- [ ] [P2-T9] Write the phase verdict into `<FEATURE>/evidence/qa-gates/denominator-rederivation.<TS>.md`, closing the denominator half of `spec.md` AC1.
  - Acceptance: the verdict states the re-derived denominator count, the unledgered count (must be 0 to pass), the orphaned-row count, and the added-file count with each file's owning child. A non-zero unledgered count makes the phase verdict `BLOCKED` and every such file a Blocking finding naming its owner.

---

### Phase 3 — Capstone-Owned Repository-Wide Recomputation Tooling

This phase adds the one computation no sibling delivers. F1's harness selects
`<package name="QuickFiler">` by name and cannot emit a repository-wide figure at all.

- [ ] [P3-T1] Create `scripts/vscode/Get-RepoWideCoverageSummary.Helpers.ps1` containing only the advanced function `Get-CoberturaPipelineStage`, which takes a Cobertura XML path and returns `post-processed` when a `<sources>` element is present and `raw` when it is absent.
  - Acceptance: the file exists, declares `[CmdletBinding()]` with a mandatory validated `-Path`, has no side effects at dot-source time, and dot-sources cleanly in a fresh `pwsh -NoProfile` session.
- [ ] [P3-T2] Add the advanced function `Get-CoberturaFileLineUnion` to `scripts/vscode/Get-RepoWideCoverageSummary.Helpers.ps1`, which groups `<class>` elements by `filename` within each `<package>`, unions **only** the class-level `./lines/line` direct children, deduplicates by `@number` taking `MAX(@hits)`, and retains the richer `condition-coverage` on a collision (larger denominator first, then larger numerator).
  - Acceptance: the function never uses a `.//lines/line` descendant axis and never reads a `line-rate` or `branch-rate` attribute; both prohibitions are asserted by the tests in [P3-T7].
- [ ] [P3-T3] Add the advanced function `Measure-CoberturaRepositoryWideRate` to the same file, computing line rate as `covered / total` over the deduplicated union and branch rate as `sum(covered) / sum(total)` parsed with `\(([0-9]+)/([0-9]+)\)` from `condition-coverage` across `@branch="True"` lines only.
  - Acceptance: the function returns `n/a` (never `0%`) when `sum(total) == 0`; a `branch="True"` line with no `condition-coverage` attribute contributes nothing to either sum; the branch rate is the ratio of sums and **never** the mean of per-line percentages.
- [ ] [P3-T4] Add the advanced function `Compare-CoberturaRepositoryWideRate` to the same file, taking a before and an after measurement plus their pipeline stages and returning a comparison object carrying both line rates, both branch rates, the signed deltas, and a `retained-or-improved` boolean.
  - Acceptance: the function **throws** when the two pipeline stages differ, with a message naming the raw-versus-post-processed mismatch; it never emits a comparison across incompatible stages.
- [ ] [P3-T5] Create the entry point `scripts/vscode/Get-RepoWideCoverageSummary.ps1` with parameters `-BeforePath`, `-AfterPath` (optional), `-OutputPath`, dot-sourcing the helpers from `$PSScriptRoot`, writing a Markdown summary, and returning exit code `0` on retained-or-improved, `1` on regression, and `2` on input error including a pipeline-stage mismatch.
  - Acceptance: the entry point contains all file and console I/O and the exit-code decision; the helpers file contains no I/O. Running with `-BeforePath` alone emits a single-artifact measurement without a comparison and exits `0`.
- [ ] [P3-T6] Create `tests/scripts/vscode/Get-RepoWideCoverageSummary.Helpers.Tests.ps1` with `Describe`/`Context`/`It` blocks covering `Get-CoberturaPipelineStage`: a fixture with `<sources>` returns `post-processed`, a fixture without it returns `raw`, and a malformed XML input throws.
  - Acceptance: three `It` blocks, all fixtures declared inline as XML strings, **no temporary file is created** anywhere in the suite.
- [ ] [P3-T7] Add `Get-CoberturaFileLineUnion` cases to `tests/scripts/vscode/Get-RepoWideCoverageSummary.Helpers.Tests.ps1`: two `<class>` elements sharing one `filename` union to the distinct line set; a line number present in both with differing `hits` resolves to the max; a fixture whose method-level `<lines>` block duplicates the class-level block returns the class-level count and **not** twice it; and a collision on `condition-coverage` retains the richer value.
  - Acceptance: four `It` blocks; the third fixture is constructed so a descendant-axis implementation would return exactly double the correct count, making it a direct #441 regression test.
- [ ] [P3-T8] Add `Measure-CoberturaRepositoryWideRate` cases to the same suite: a known line rate; a branch rate computed as the ratio of sums where a 2-condition line and an 8-condition line would give a different answer under a mean-of-percentages implementation; a `branch="True"` line with no `condition-coverage` contributing nothing; and a zero-branch fixture returning `n/a` rather than `0%`.
  - Acceptance: four `It` blocks, each asserting an exact expected value computed by hand in the test body's comment.
- [ ] [P3-T9] Create `tests/scripts/vscode/Get-RepoWideCoverageSummary.Tests.ps1` covering the entry point: a retained-or-improved pair exits `0`; a regression pair exits `1`; a missing input path exits `2`; and a raw-versus-post-processed pair exits `2` with the mismatch named in the output.
  - Acceptance: four `It` blocks; the fourth is the negative case `spec.md` § Test Conditions requires; the suite creates no temporary file and starts no external process.
- [ ] [P3-T10] Run `mcp__drm-copilot__run_poshqc_format` and then `mcp__drm-copilot__run_poshqc_analyze` over the repository and record `<FEATURE>/evidence/qa-gates/capstone-tooling-poshqc.<TS>.md`.
  - Acceptance: the artifact records the formatter result, the analyzer diagnostic count attributable to the four new files (which must be **zero**), and `Timestamp:` / `Command:` / `EXIT_CODE:` / `Output Summary:`. Because the PoshQC MCP surface does not return a process exit code, `EXIT_CODE:` records the tool-reported status and `Output Summary:` records the diagnostic counts; pre-existing diagnostics on other files are recorded as `INFORMATIONAL` and are not this child's to fix. This task runs **before** the size measurement in [P3-T11] so the recorded counts are post-format counts.
- [ ] [P3-T11] Measure and record the physical line count of all four new files into `<FEATURE>/evidence/qa-gates/capstone-tooling-size.<TS>.md`.
  - Acceptance: `scripts/vscode/Get-RepoWideCoverageSummary.Helpers.ps1`, `scripts/vscode/Get-RepoWideCoverageSummary.ps1`, `tests/scripts/vscode/Get-RepoWideCoverageSummary.Helpers.Tests.ps1`, and `tests/scripts/vscode/Get-RepoWideCoverageSummary.Tests.ps1` are each recorded numerically and each is `<= 500`. The measurement is taken **after** [P3-T10] has formatted them, so the recorded count is the post-format count and a later formatting pass cannot push a file over the limit undetected. A file over 500 lines is split before [P3-T12] runs.
- [ ] [P3-T13] Run the scoped Pester coverage command from the Resolved Toolchain Commands table with `$c.CodeCoverage.OutputPath = "docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497/evidence/qa-gates/capstone-tooling-coverage.<TS>.xml"` and record `<FEATURE>/evidence/qa-gates/capstone-tooling-coverage.<TS>.md`.
  - Acceptance: `Output Summary:` records the numeric passed and failed counts (failed must be 0) and the **numeric** line-coverage percentage for the two new production scripts, which must be `>= 85.0`. The artifact also records verbatim that PoshQC's bundled coverage allow-list resolves in this repository to `.claude/hooks/`, `.claude/lib/`, and `.codex/hooks/` only, so `artifacts/pester/powershell-coverage.xml` carries no `scripts/vscode/` sourcefile, and that the emitted JaCoCo carries a `LINE` counter and **no `BRANCH` counter**, so the `.claude/rules/powershell.md` 75% branch floor is not measurable for PowerShell in this repository. That gap is pre-existing and recorded, not closed by this child.

---

### Phase 4 — Repository-Wide Before Figure Recomputation

- [ ] [P4-T1] Determine the pipeline stage of `<FEATURE>/evidence/baseline/repo-coverage-before.<TS>.cobertura.xml` with `Get-CoberturaPipelineStage` and record it into `<FEATURE>/evidence/baseline/repo-coverage-before-recomputed.<TS>.md`.
  - Acceptance: the artifact records `post-processed` or `raw` with the `<sources>` presence quoted as the discriminator, plus all four schema fields. The runner always post-processes and has no `-NoPostProcess` switch, so `post-processed` is expected; `raw` means the capture was not the runner's output and the capture must be repeated.
- [ ] [P4-T2] Recompute the repository-wide line rate from the before artifact with `scripts/vscode/Get-RepoWideCoverageSummary.ps1 -BeforePath <before-cobertura> -OutputPath <FEATURE>/evidence/baseline/repo-coverage-before-recomputed.<TS>.md` and record the numeric value.
  - Acceptance: the artifact records the recomputed covered-line count, total-line count, and line rate to four decimal places, together with the exact command and `EXIT_CODE`. The value is computed from class-level `./lines/line` unions and is the **authoritative** before figure.
- [ ] [P4-T3] Recompute the repository-wide branch rate from the same artifact and append the numeric value to the same file.
  - Acceptance: the artifact records the summed covered conditions, summed total conditions, and branch rate to four decimal places, and states that the value is the ratio of sums over `@branch="True"` lines and not the mean of per-line percentages.
- [ ] [P4-T4] Record the tool-emitted root attributes side by side with the recomputed values in the same artifact, with the delta between them.
  - Acceptance: the artifact carries a two-column table labelling the emitted values verbatim `TOOL-EMITTED (#441-corrupted)` and the recomputed values `AUTHORITATIVE`, records the signed delta for line rate and for `lines-valid`, and states that the emitted values are reported for reviewer traceability only and are used in no comparison.
- [ ] [P4-T5] Record the instrumented scope of the before artifact into the same file: the `<package>` name list and count, the number of `<class>` elements whose `filename` begins `QuickFiler\`, and the test pass/fail/skip counts from [P0-T21].
  - Acceptance: all four values are recorded numerically. The retained package set is expected to exclude the five vendored packages (`log4net`, `Mono.Reflection`, `Microsoft.IO.RecyclableMemoryStream`, `System.Interactive`, `System.Linq.Async`) that post-processing strips automatically; any divergence from that expectation is recorded as `INFORMATIONAL`, not corrected.
- [ ] [P4-T6] Record the absolute policy floors as informational context in the same artifact and state the operative gate.
  - Acceptance: the artifact records the recomputed before line rate against the `CLAUDE.md` 80% floor and the `.claude/rules/general-unit-test.md` 85% line / 75% branch floors, labels all three `INFORMATIONAL`, cites open issue **#494** (`Bug: conflicting-coverage-thresholds-across-policy-docs`) rather than re-adjudicating the conflict, and states verbatim that **issue #136 AC8's gate is retain-or-improve against this same-session pair, not an absolute floor** (`epic.md` § Coverage-Target Reconciliation). It also records D-3 verbatim so the epic's refuted explanation of the 70.19% → 85.65% swing is not propagated.

---

### Phase 5 — Per-File Gate Reconciliation Against the Ledger

Runs only after [P1-T8] returns `TRUSTED`. Every figure in this phase comes from F1's harness applied
to `<FEATURE>/evidence/baseline/repo-coverage-before.<TS>.cobertura.xml`.

- [ ] [P5-T1] Invoke F1's harness with the contract recorded in [P1-T7] against the before Cobertura artifact and record its raw output and exit code into `<FEATURE>/evidence/qa-gates/per-file-coverage-harness.<TS>.md`.
  - Acceptance: all four schema fields present, the exact invocation recorded, and the per-file row count recorded. No substitute harness is constructed and no figure is transcribed from `Get-CoberturaCoverageSummary`, `Merge-CoberturaClassesByFilename`, or `scripts/temp-extract-coverage.ps1`.
- [ ] [P5-T2] Build the reconciliation table into `<FEATURE>/evidence/qa-gates/per-file-coverage-reconciliation.<TS>.md` with exactly one row per file in the Phase 2 derived compile set, each row carrying `file`, `ledger bucket`, `owning child`, `line %`, `branch %`, `gate applied`, and `verdict`.
  - Acceptance: the row count equals the Phase 2 denominator count exactly; **both** a line figure and a branch figure appear on every row (a line figure alone is never accepted as proof of compliance); no row is omitted; all four schema fields present.
- [ ] [P5-T3] Evaluate the `>= 80.0%` line gate for every `testable` row and record pass/fail per row in the same artifact.
  - Acceptance: comparison is unrounded (`-lt 0.80`); every failing row is marked `BLOCKING` and names its owning child from the ledger's `owning_child` field cross-checked against `epic.md` § Feature File Assignments.
- [ ] [P5-T4] Evaluate the `>= 75.0%` branch gate for every `testable` row **independently of the line gate** and record pass/fail per row in the same artifact.
  - Acceptance: comparison is unrounded (`-lt 0.75`); the artifact records the count of rows that pass line and fail branch as its own line item. Twelve such files existed in the planning-time baseline, so a non-zero count here is expected to be attributable and each one is `BLOCKING` naming its owner.
- [ ] [P5-T5] Evaluate the `>= 90.0%` line target for every file this epic created, using the added-file set from [P2-T8], and record pass/fail per row in the same artifact.
  - Acceptance: every added file is listed with its creating child and its line percentage; a file below 90.0% is `BLOCKING` naming that child. A file added mid-wave with no ledger row was already caught in [P2-T6] and is cross-referenced rather than re-derived.
- [ ] [P5-T6] Verify the zero-branch rule for every row whose `branches-valid` is 0 and record the result in the same artifact.
  - Acceptance: every such row reports branch `N/A`, never `0%`, and never counts as a failure. A row reporting `0%` branch on a zero-branch file is a `BLOCKING` finding against F1's harness implementation. `ItemViewer.WebViewThread.cs`, `ItemViewer.Commands.cs`, and `ItemViewer.DisplayState.cs` are the epic's named instances and are checked explicitly by name.
- [ ] [P5-T7] Verify every `interface-only / not-measured` row reports `N/A` on both axes in the same artifact.
  - Acceptance: no third-bucket row carries `0%`, and no third-bucket row is marked `fail`. A harness keyed on `line-rate` would report all of them as 0% failures; a row in that state is `BLOCKING` against F1's harness. The zero-coverable-lines evidence for these rows is produced in Phase 8.
- [ ] [P5-T8] Verify every `measured-not-gated` row carries real numeric line and branch figures and is **not** gated on either floor, in the same artifact.
  - Acceptance: each such row shows numeric values and a `not gated` verdict, and carries no `[ExcludeFromCodeCoverage]`. A `measured-not-gated` row reported as `N/A`, or gated and failed, is a `FINDING` against F1's harness. The genuinely-generated test is applied in Phase 8.
- [ ] [P5-T9] Fail the report closed on any compiled file with no ledger row by carrying the [P2-T6] result into the same artifact as an explicit `UNLEDGERED` row per file.
  - Acceptance: the artifact contains one `UNLEDGERED` row per unledgered compiled file with its creating child, or an explicit statement that the unledgered count is zero carrying `SearchScope:` / `SearchPatterns:` / `SearchResult:`. A non-zero count sets the report verdict to `BLOCKED`.
- [ ] [P5-T10] Enumerate every Blocking finding produced by this phase into a findings register in the same artifact, one line per finding, each naming the owning child, the file, the gate missed, and the measured value.
  - Acceptance: the register exists, is complete against [P5-T3] through [P5-T9], and the artifact states verbatim: **"F16 does not fix a sibling's coverage and does not grant an exemption to close a gap."** No task exists in this plan that edits a `QuickFiler/**/*.cs` file, and that absence is asserted here.
- [ ] [P5-T11] Write the phase verdict into `<FEATURE>/evidence/qa-gates/per-file-coverage-reconciliation.<TS>.md`, closing `spec.md` AC1, AC2, and AC9.
  - Acceptance: the verdict records the total row count, the `testable` pass and fail counts on each axis independently, the `ratified-exempt` / `interface-only` / `measured-not-gated` counts, the epic-created-file count and its 90% pass count, the `UNLEDGERED` count, and the total Blocking-finding count. A non-zero Blocking count makes the phase verdict `BLOCKED` and the feature outcome remediation-required, never `PASS`.

---

### Phase 6 — `[ExcludeFromCodeCoverage]` Attribute Census

Per-**type**, never per-file. The three passes are deliberately separate; a single regex cannot answer
all three questions.

- [ ] [P6-T1] Restrict the census scope to the Phase 2 derived compile set and record the scope decision into `<FEATURE>/evidence/qa-gates/exclude-attribute-census.<TS>.md`.
  - Acceptance: the artifact records the denominator count, states that anything not in the compile set is out of scope even if it carries an attribute, and enumerates the uncompiled orphan files carrying a real attribute as `INFORMATIONAL` with `SearchScope:` / `SearchPatterns:` / `SearchResult:`.
- [ ] [P6-T2] Run pass (a) — real attribute versus doc-comment mention — over every denominator file and append the classification of every occurrence of the literal `ExcludeFromCodeCoverage` to the same artifact.
  - Rules: strip leading whitespace and classify as a **mention** when the remainder starts with `//`, `///`, or `*`, or the occurrence sits inside a `<see cref="…"/>` or `<c>…</c>` construct; otherwise require the occurrence to sit inside a bracketed attribute list (nearest preceding non-whitespace character is `[` or `,`, with a `]` on the same logical line). The rule must admit the short spelling, the fully-qualified `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`, the `Attribute`-suffixed spelling, and multi-attribute brackets in either order.
  - Acceptance: **every** occurrence in a file is evaluated, never only the first — two files are known to carry both a doc-comment mention and a real attribute, and a first-match classifier mis-files both. The artifact records the total occurrence count, the mention count, and the real-attribute count. A pattern anchored on `^\s*\[ExcludeFromCodeCoverage\]$` is prohibited; roughly half the usages are fully qualified.
- [ ] [P6-T3] Run pass (b) — type-level versus member-level — over every real attribute and append the classification to the same artifact.
  - Rule: **never** use indentation depth. From the attribute line, skip forward over further attribute lines, blank lines, comment lines, and modifier-only lines, and read the first declaration token sequence; a match on `(class|struct|record|interface|enum)\s+\w+` is **type-level**, anything else is **member-level**.
  - Acceptance: the artifact records the type-level count, the member-level count, and their sum equal to the real-attribute count from [P6-T2], with the declaring file and line for each.
- [ ] [P6-T4] Run pass (c) — suppressed-by-propagation versus self-declared — and append the type map to the same artifact.
  - Method: build `type fully-qualified name -> set of declaring files` by scanning every denominator file for `(class|struct|record|interface)\s+(\w+)` declarations with the enclosing `namespace`; a type is exempt when **any** declaring file carries a type-level attribute on it (a partial type may be annotated only once, CS0579 otherwise).
  - Acceptance: the artifact records the type map, the exempt-type set, and the per-file suppression state. The declaration scan must also match a bare unmodified `class X` at line start, not only modifier-prefixed declarations.
- [ ] [P6-T5] Classify each file as fully suppressed, partially suppressed, or not suppressed, and append the counts to the same artifact.
  - Rule: a file is **fully suppressed** iff every type declared in it is exempt, and **partially suppressed** iff some but not all are.
  - Acceptance: `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` is checked explicitly by name: it declares four types and carries one type-level attribute on the secondary `FolderScoringService` type only, so it is **partially** suppressed and contributes **0** to the fully-suppressed count, not 1. A run that computes one-attribute-equals-one-file and produces a fully-suppressed count one higher than the epic's 24 has implemented the rule wrongly and must be corrected before the phase proceeds (D-5).
- [ ] [P6-T6] Compute the partition cross-check `declaring files + comment-only files + uncompiled orphan files` and append the arithmetic to the same artifact.
  - Acceptance: the three parts are recorded individually, are shown to be disjoint, and their sum is recorded. The planning-time figure is `21 + 5 + 7 = 33`; the phase records the **re-derived** values and reports any divergence from `33` as `INFORMATIONAL` with the changed part identified, rather than forcing the sum to 33.
- [ ] [P6-T7] Cross-check the declaring-file set against the `[X]` markers in `<EPIC>/epic.md` § Feature File Assignments and append the comparison to the same artifact.
  - Acceptance: the artifact records the `[X]` count, the declaring-file count, the set difference in both directions, and `SearchScope:` / `SearchPatterns:` / `SearchResult:`. A divergence is a `FINDING` against the manifest owner, reported and not repaired — `epic.md` is not edited by this child.
- [ ] [P6-T8] Census `DebuggerNonUserCode` separately across the denominator and append the result to the same artifact.
  - Acceptance: every occurrence is classified type-level or member-level with its `file:line`. `QuickFiler/Properties/Resources.Designer.cs` and `QuickFiler/Properties/Settings.Designer.cs` are checked explicitly by name. The artifact states verbatim that `DebuggerNonUserCode` is **not** the ratified exemption mechanism named in `CLAUDE.md` §UT2 and that a file suppressed by it must not be classified `interface-only`.
- [ ] [P6-T9] Apply the testable-seam test to every surviving real attribute and append the result to the same artifact.
  - Rule: every surviving attribute must map to a ledger row classified `ratified-exempt` with a named ground, or to a `ratified-by-maintainer (#227)` provenance record, or to the #230 deferral set. An attribute on a file the ledger classifies `testable`, `interface-only / not-measured`, or `measured-not-gated` is a `BLOCKING` finding naming the owning child.
  - Acceptance: the artifact records the count of attributes remaining on a testable seam, which must be **zero** to pass, and names the owning child for each non-zero case.
- [ ] [P6-T10] Verify no attribute reaches a seam `CLAUDE.md`:303 names as explicitly not exempt and append the result to the same artifact.
  - Acceptance: `QuickFiler/Controllers/KbdActions.cs` and any path/settings helper in the denominator are checked explicitly; the artifact records `SearchScope:` / `SearchPatterns:` / `SearchResult:` and cites the `CLAUDE.md` clause verbatim, with its line number re-verified by reading the file at execution time rather than transcribed.
- [ ] [P6-T11] Write the phase verdict into `<FEATURE>/evidence/qa-gates/exclude-attribute-census.<TS>.md`, closing `spec.md` AC5.
  - Acceptance: the verdict records the real-attribute count, type-level and member-level counts, fully- and partially-suppressed file counts, the partition arithmetic, the `DebuggerNonUserCode` count, and the testable-seam count. It also records verbatim that attributes traceable to closed maintainer-ratification issue **#227** carry provenance and are **not re-litigated**, and that the nine attributes deferred under open issue **#230** are explicitly **not a gap and not a merge condition**, with no task in this plan building the #230 message-pump seam.

---

### Phase 7 — Exemption-Ground Reconciliation for `ratified-exempt` Rows

- [ ] [P7-T1] Extract every `ratified-exempt` row from `<EPIC>/coverage-ledger.json` with its `exempt_ground`, `rationale`, and `attribute_dispositions` into `<FEATURE>/evidence/qa-gates/ac3-exemption-ground-reconciliation.<TS>.md`.
  - Acceptance: the artifact records the row count, one verbatim row per exemption, and all four schema fields.
- [ ] [P7-T2] Record the four ratified grounds verbatim with their citations into the same artifact, re-verifying every quoted line number by reading the source at execution time.
  - Acceptance: grounds 1-3 are quoted from `CLAUDE.md` § UT2 (the VSTO add-in lifecycle clause, the WinForms form-derived and Designer-generated clause, and the Outlook Interop event-handler-without-an-injectable-seam clause) with their re-verified line numbers; ground 4 is quoted from `<EPIC>/epic.md` § "Epic Ruling: a fourth exemption ground for prohibited-to-execute adapters (F13)" with its four conjunctive conditions verbatim. The artifact records that ground 4 is ratified **for this epic only** and is not a `CLAUDE.md` amendment.
- [ ] [P7-T3] Apply universal condition U1 to every `ratified-exempt` row and record the result in the same artifact.
  - Acceptance: each row states exactly one of the four grounds. A row with no ground, or with an unresolved disjunction such as "COM/WinForms", is a `BLOCKING` finding naming the owning child.
- [ ] [P7-T4] Apply universal condition U2 to every row and record the result in the same artifact.
  - Acceptance: each row is keyed on the **type** and enumerates **every** file that type is declared in, cross-checked against the type map from [P6-T4]. A row keyed on a file rather than a type, or one whose file list is incomplete against the type map, is a `BLOCKING` finding naming the owning child. `QfcDatamodel` (3 files) and `ItemViewer` (7 files, including the 6,224-line Designer) are checked explicitly by name.
- [ ] [P7-T5] Apply universal condition U3 to every row and record the result in the same artifact.
  - Acceptance: each row records the attribute's `file:line` and whether it is type-level or member-level, and each recorded placement matches the [P6-T3] classification exactly. A mismatch is a `BLOCKING` finding naming the owning child.
- [ ] [P7-T6] Apply universal condition U4 by reconciling the disposition count against the live attribute-usage count from [P6-T2], not the declaring-file count, and record the result in the same artifact.
  - Acceptance: the artifact records the total disposition count and the total real-attribute-usage count and shows them equal. The planning-time figures are 40 usages against 21 declaring files; the phase reconciles to the **re-derived** usage count and reports a divergence rather than assuming 40.
- [ ] [P7-T7] Verify the ground-1 evidence for every row citing VSTO add-in lifecycle and record the result in the same artifact.
  - Acceptance: each such row cites the type by file path and name as an add-in entry point, ribbon event handler, or COM utility-registration class; names a symbol that cannot resolve without a live Outlook process; and states that no logic remains beyond host wiring with the extracted host-neutral module named. Research established that `QuickFiler.csproj` contains no such class, so **any** ground-1 row in this ledger is challenged by default and requires all three items before it is accepted; a row that cannot produce them is `BLOCKING` naming its owner.
- [ ] [P7-T8] Verify the ground-2 evidence for every row citing WinForms form-derived or Designer-generated code and record the result in the same artifact.
  - Acceptance: each such row quotes either the type declaration line showing a `System.Windows.Forms` base type, or shows the file is `*.Designer.cs` and carries `GeneratedCodeAttribute` or is the `InitializeComponent` partial; and names the type with every file it is declared in. A row failing both alternatives is `BLOCKING` naming its owner. Rows for generated designer files are additionally cross-checked against DEC-5: under the epic's ruling, generated designer files are **`measured-not-gated`, not `ratified-exempt`**, so a `ratified-exempt` classification on one is itself a `FINDING`.
- [ ] [P7-T9] Verify the ground-3 evidence for every row citing an Outlook Interop event handler without an injectable seam and record the result in the same artifact.
  - Acceptance: each such row quotes the `using` or type reference showing a direct dependency on `Microsoft.Office.Interop.Outlook.Application`, `MailItem`, `Store`, or `MAPIFolder`; names the event-handler members; carries a written, file-specific argument that no injectable seam is feasible, naming the seam considered and the concrete obstacle; and states that no logic behind the COM call remains in the file. A row lacking the seam-infeasibility argument is `BLOCKING` naming its owner, because "without an injectable seam" is read as a live obligation and not a standing permission.
- [ ] [P7-T10] Verify all four conjuncts of ground 4 for every row citing an irreducible prohibited-to-execute adapter and record the result in the same artifact.
  - Acceptance: each such row separately evidences (1) every member enumerated with its body quoted, each a 1:1 forward with no branch, computation, or state; (2) the specific `.claude/rules/general-unit-test.md` clause the execution would violate, named rather than gestured at; (3) the seam interface named by file path **and** a named test exercising a consumer against it; (4) the type declaration quoted showing `sealed` and not `partial`, with the attribute at type level. Any single conjunct failing is `BLOCKING` naming its owner. `QuickFiler/Viewers/WebView2CoreInitializer.cs` is expected to survive; `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` is expected **not** to, and a surviving attribute on it after F13 executes is `BLOCKING` naming F13.
- [ ] [P7-T11] Apply the nine rejection tests to every `ratified-exempt` row and record the per-row result in the same artifact.
  - Tests: (1) a seam exists or is feasible; (2) the ground does not textually cover the file; (3) the row reaches a `CLAUDE.md`:303 named non-exempt seam; (4) a ground-4 conjunct fails; (5) a member-level attribute is used to hide a residual without the #457 lambda analysis of Phase 9; (6) `ratified-exempt` is asserted on a zero-coverable-lines file; (7) the exemption closes a coverage gap discovered during execution; (8) a `coverage.config` or `.runsettings` assembly-level exclude touches QuickFiler; (9) a newly created file claims exemption without a ground meeting the standard.
  - Acceptance: each test is applied to each row with a recorded pass/fail; each failure is `BLOCKING` naming the owning child; and the artifact states verbatim that **none of the nine may be waived by F16 and F16 may never itself grant an exemption to close a gap**.
- [ ] [P7-T12] Verify no assembly-level coverage exclusion touches QuickFiler by reading repo-root `coverage.config`, repo-root `TaskMaster.runsettings`, and `scripts/vscode/TaskMaster.cli.runsettings`, and record the result in the same artifact.
  - Acceptance: the artifact records `SearchScope:` (the three file paths), `SearchPatterns:` (`QuickFiler`, `ModulePath`, `Exclude`), and `SearchResult:`. Any QuickFiler-matching exclude is a `BLOCKING` finding naming the child that introduced it, established with `git log -S` against the matching line. The planning-time state is that no such entry exists; this task re-verifies rather than inherits it.
- [ ] [P7-T13] Write the phase verdict into `<FEATURE>/evidence/qa-gates/ac3-exemption-ground-reconciliation.<TS>.md`, closing `spec.md` AC3.
  - Acceptance: the verdict records the `ratified-exempt` row count, the per-ground row counts, the U1-U4 pass counts, the nine rejection-test failure counts, and the total Blocking count with each owning child named. A non-zero Blocking count makes the phase verdict `BLOCKED`.

---

### Phase 8 — Third-Bucket and `measured-not-gated` Verification

- [ ] [P8-T1] Re-run the instrumentation positive control against `<FEATURE>/evidence/baseline/repo-coverage-before.<TS>.cobertura.xml` and record the result into `<FEATURE>/evidence/qa-gates/ac4-third-bucket-verification.<TS>.md`.
  - Acceptance: a `<class>` element with `filename="QuickFiler\Interfaces\MailItemActionsAdapter.cs"` is present and its class-level `<lines>` block contains more than zero `<line>` children, with the count recorded. **This control is re-run, not inherited from F7.** If it is absent, `Interfaces/` was not instrumented and every zero-lines verdict in that folder is a false positive, which makes the phase verdict `BLOCKED` and invalidates Phase 5's third-bucket rows.
- [ ] [P8-T2] Apply the measurement-side third-bucket test to every ledger row classified `interface-only / not-measured` and record the per-row result in the same artifact.
  - Test: after unioning all `<class>` elements whose `filename` equals the file, the class-level `<lines>` block contains **zero** `<line>` children, or the file is absent from the report entirely.
  - Acceptance: each row records the union's `<line>` child count. A row with a non-zero count is misclassified and is a `FINDING` naming its owning child.
- [ ] [P8-T3] Disambiguate every absent-or-zero-line file among the three causes and record the cause per file in the same artifact.
  - Causes: (a) the file emits no executable IL — the third bucket; (b) every type in the file carries `[ExcludeFromCodeCoverage]` — `ratified-exempt`; (c) every type carries type-level `DebuggerNonUserCode`.
  - Acceptance: the third-bucket test is conjoined as **zero `<line>` children AND no `[ExcludeFromCodeCoverage]` anywhere in the file (from [P6-T2]) AND no type-level `DebuggerNonUserCode` (from [P6-T8])**. Every absent file in the denominator is assigned exactly one cause, and the ledger is required to name which; a file with no ledger disposition for its absence is a `FINDING` naming its owning child. `QuickFiler/Properties/Resources.Designer.cs` is checked explicitly by name.
- [ ] [P8-T4] Corroborate each third-bucket row with the source-side screen and record the result in the same artifact.
  - Screen: outside comments and `using`/`namespace`/assembly-attribute lines, the file declares only `interface`, `enum`, and/or `delegate` types — it contains none of `=>`, `static`, `const`, `class`, `struct`, `record`, no `{ get; … } =` initializer, and no constructor.
  - Acceptance: the screen result is recorded per row alongside the measurement verdict, and the artifact states that the screen is a filter and the Cobertura test is the verdict. A file passing the measurement test but failing the screen, or the reverse, is recorded as a `FINDING` with both results shown.
- [ ] [P8-T5] Assert the intersection of the third-bucket set and the [P6-T2] declaring set is empty and record the result in the same artifact.
  - Acceptance: `SearchScope:` (the third-bucket row set), `SearchPatterns:` (`ExcludeFromCodeCoverage` in all admitted spellings), `SearchResult:` (matching files or `none`). A non-empty intersection is a `BLOCKING` finding naming the owning child — this is precisely the failure mode the third bucket exists to prevent, a child seeing a 0% line rate on an interface file and "fixing" it with an attribute. The planning-time intersection was empty; this task re-verifies rather than inherits it.
- [ ] [P8-T6] Screen every sibling's new test files for shape-assertion tests written to manufacture coverage for a third-bucket file and record the result in the same artifact.
  - Patterns: `typeof\(I\w+\)`, `GetMethods\(\)`, `GetProperties\(\)`, `GetInterfaces\(\)`, `Should\(\)\.Implement`, `Should\(\)\.BeAssignableTo<I\w+>`.
  - Acceptance: `SearchScope:` (`QuickFiler.Test/**/*.cs`), `SearchPatterns:` (the six patterns verbatim), `SearchResult:` (matching `file:line` or `none`). Each hit is cross-referenced against whether the asserted-about type lives in a third-bucket file, and each such hit is **read** before any finding is written — `BeAssignableTo` is legitimate in a factory test, so this screen has known false positives and an automatic finding is prohibited.
- [ ] [P8-T7] Verify every `measured-not-gated` row is genuinely generated code and record the per-row evidence in the same artifact.
  - Acceptance: each row shows the file is `*.Designer.cs` or a generated `Properties/` file **and** carries `GeneratedCodeAttribute`, `CompilerGeneratedAttribute`, or `DebuggerNonUserCodeAttribute`, with the `file:line` quoted; carries real numeric line and branch figures; and carries no `[ExcludeFromCodeCoverage]`. A hand-written, testable file parked in this bucket is a `BLOCKING` finding naming its owning child.
- [ ] [P8-T8] Verify the DEC-5 disposition boundary and record the result in the same artifact.
  - Acceptance: the artifact records that generated designer files are `measured-not-gated` and **not** `ratified-exempt` (the epic's own earlier ground-1 classification of them as exempt-candidates is superseded), that such files are instrumented, measured, contribute to repository-wide totals, and are not individually gated on either the 80% line or 75% branch floor, and that `measured-not-gated` is distinct from `interface-only / not-measured`, which has no denominator at all. Any designer file classified `ratified-exempt` in the delivered ledger is a `FINDING` naming its owning child.
- [ ] [P8-T9] Write the phase verdict into `<FEATURE>/evidence/qa-gates/ac4-third-bucket-verification.<TS>.md`, closing `spec.md` AC4.
  - Acceptance: the verdict records the positive-control result, the third-bucket row count, the per-cause absence counts, the third-bucket-with-attribute count (must be 0), the shape-assertion hit count with its read-verified dispositions, and the `measured-not-gated` row count with its generated-code evidence count.

---

### Phase 9 — Issue #457 Lambda-Residual Scan

- [ ] [P9-T1] Run stage 1 over every member-level attribute identified in [P6-T3] and record the candidate set into `<FEATURE>/evidence/qa-gates/ac457-lambda-residual-scan.<TS>.md`.
  - Method: determine each member's line span (expression-bodied members run from the declaration line to the terminating `;`; block-bodied members run to the matching `}` at the member's brace depth), then search the span for lambda-producing tokens — `=>` occurrences beyond the member's own expression-body arrow, `delegate` followed by `{` or `(`, local-function declarations, `async` lambdas, and query expressions.
  - Acceptance: the artifact records, per candidate, the attribute `file:line`, the member name, the span, and the lambda line numbers, plus all four schema fields. The scan covers the **whole** compiled set, not only the files known to be exposed at planning time, because the exposure set at fan-in is expected to be larger.
- [ ] [P9-T2] Distinguish compiler-generated closure classes from async state machines in the before Cobertura artifact and append the result to the same artifact.
  - Acceptance: `<class name>` values containing `<>c` or `<>c__DisplayClass` are recorded as closures; values matching `<…>d__` are recorded as async state machines and are explicitly **not** by themselves evidence of #457. The artifact records the counts of each within `filename` values beginning `QuickFiler\`.
- [ ] [P9-T3] Run stage 2 confirmation against the merged per-file class union for every stage-1 candidate and append the result to the same artifact.
  - Signature: inside one member's brace span, the member's own statement lines are **absent** from the union while lines lying inside the same span are **present with `hits="0"`**. An interleaving of present lines within an otherwise-absent member range is a lifted closure; ordinary uncovered code produces a contiguous block.
  - Acceptance: each candidate is marked `CONFIRMED`, `NOT CONFIRMED`, or `UNDETERMINED` with the matched line numbers recorded. Stage 1 alone is a candidate screen and must never be reported as a confirmation — a lambda reachable from a non-exempt caller may be covered anyway.
- [ ] [P9-T4] Compute the irreducible residual and the resulting coverage cap for every confirmed file and append the arithmetic to the same artifact.
  - Acceptance: the artifact records `residual` (the summed `hits="0"` lines matched in [P9-T3]), `total-lines-in-union`, and `1 - residual/total` per file. A file whose cap is below 0.80 **cannot** meet the line gate without changing its attribute placement, and that is a `BLOCKING` finding naming the owning child, never a ledger exemption. `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` is checked explicitly by name; it carried seven member-level attributes with at least fifteen lifted lambda lines across three members at planning time, and F13 owns it.
- [ ] [P9-T5] Write the phase verdict into `<FEATURE>/evidence/qa-gates/ac457-lambda-residual-scan.<TS>.md`.
  - Acceptance: the verdict records the stage-1 candidate count, the stage-2 confirmed count, the count of files whose cap falls below 0.80, and each owning child. The artifact states verbatim that **the disposition — a class-level-exempt, non-`partial` adapter type — belongs to the owning child and F16 performs no refactor**, and cites open issue **#457**.

---

### Phase 10 — Cross-Sibling Per-File Research and Plan-Phase Audit

Closes issue #136's second and third acceptance criteria.

- [ ] [P10-T1] Carry the sibling-folder resolution map from [P0-T14] into `<FEATURE>/evidence/qa-gates/ac2-per-file-artifact-audit.<TS>.md` and record, per child, the resolved folder path, the `research/` directory path, and the `plan.*.md` path.
  - Acceptance: all fourteen children are listed; any child with no resolved folder, no `research/` directory, or no plan file carries `SearchScope:` / `SearchPatterns:` / `SearchResult:` and is a `BLOCKING` epic-sequencing finding naming that child.
- [ ] [P10-T2] Build the production-file-to-child assignment map from `<EPIC>/epic.md` § Feature File Assignments and from the ledger's `owning_child` field, reconcile the two, and append the map to the same artifact.
  - Acceptance: every file in the Phase 2 denominator has exactly one owning child in each source; every disagreement between the two sources is recorded as a `FINDING` with both values shown; the reconciled map is the one used by [P10-T4] and [P10-T8]. Files added mid-wave that appear in neither source are cross-referenced to their creating child from [P2-T8].
- [ ] [P10-T3] Record the stem-normalization rule verbatim into the same artifact before any existence check runs.
  - Rule: lowercase; drop the directory; drop a trailing `.md`; strip a leading ordinal `^\d{2}-`; strip a leading ISO timestamp `^\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-`; strip a trailing `.research` and any trailing `.<timestamp>`; strip `.cs`; strip a leading folder qualifier `^(controllers|interfaces|viewers|helper ?classes)\.`; delete `.` and `-`.
  - Acceptance: the rule is recorded verbatim, and the artifact states that at least eight distinct artifact-naming schemes are in use across the children so normalization is mandatory rather than cosmetic.
- [ ] [P10-T4] Assert per-file research-artifact existence for every assigned production file using **stem equality**, and append the per-child matrix to the same artifact.
  - Acceptance: matching is stem equality and **never** `contains` — `ItemViewer` is a proper substring of `ItemViewerExpanded`, `QfcItemViewerExpanded`, and `ItemViewer.Designer`, so a `contains` check would falsely satisfy `ItemViewer.cs` with `ItemViewerExpanded.md`. Where more than one compiled file shares a normalized stem, the check additionally requires the artifact name to carry the folder segment; `QuickFiler/Controllers/IQfcFormController.cs` and `QuickFiler/Interfaces/IQfcFormController.cs` both normalize to `iqfcformcontroller` and are checked explicitly by name. Every missing artifact carries `SearchScope:` / `SearchPatterns:` / `SearchResult:`.
- [ ] [P10-T5] Strengthen the existence check mechanically and append the result to the same artifact.
  - Acceptance: each matched artifact is recorded with its byte size and a boolean for whether its body contains the production file's path at least once. An artifact under 1,024 bytes or not naming its file is recorded as a `FINDING` naming the owning child. The artifact states verbatim that this check proves an artifact with the right name exists and **does not** prove the artifact is about that file or contains analysis.
- [ ] [P10-T6] Read a disclosed sample of matched research artifacts and append the sampled review to the same artifact.
  - Acceptance: the sample size and the selection rule are stated explicitly; the sample includes at least one artifact from every child; each sampled artifact is judged as addressing its named file or not. The artifact states verbatim that this is a **sampled** review and its conclusion does not extend to unsampled artifacts.
- [ ] [P10-T7] Determine the per-file obligation for third-bucket files and record the determination and its consequence in the same artifact.
  - Acceptance: the artifact records D-7 verbatim — the obligation extends, because a research artifact is what establishes third-bucket membership, and F2, F3, F6, and F7 all produced artifacts for their interface files with F7 giving them their own plan phases. Any child short of an artifact for an assigned interface-only file is a `BLOCKING` finding naming that child. F13's four interface files (`Viewers/IBreadcrumbDropDownHost.cs`, `Viewers/IBreadcrumbWebHost.cs`, `Viewers/IWebViewCoreInitializer.cs`, `Viewers/IWebViewMessenger.cs`) are checked explicitly by name; **F16 does not write the missing artifacts**.
- [ ] [P10-T8] Assert per-file atomic-plan-phase coverage for every assigned production file and append the per-child matrix to the same artifact.
  - Method: grep each child's `plan.*.md` for `^### Phase \d+ — ` headings and assert that some phase heading names each assigned file. Phase 0 and the final QA phase are structural and are excluded from the mapping.
  - Acceptance: every assigned file maps to a named phase or is recorded as unmapped with the three search fields. The artifact states verbatim that this check confirms a phase is **named** for the file and not that the phase's tasks address it.
- [ ] [P10-T9] Discover each child's per-task test-naming convention **before** counting, and record the discovered convention per child into `<FEATURE>/evidence/qa-gates/ac3-atomic-test-step-audit.<TS>.md`.
  - Acceptance: one recorded convention per child, each derived by reading that child's plan rather than assuming F7's phrasing. The artifact states verbatim that only F7's plan was read in full during research, so applying F7's signature to another child without discovery produces a false negative.
- [ ] [P10-T10] Count test-adding plan tasks per child against the discovered convention and append the counts to `<FEATURE>/evidence/qa-gates/ac3-atomic-test-step-audit.<TS>.md`.
  - Acceptance: per child, the total task count matching `^- \[[ x]\] \[P\d+-T\d+\]`, the test-adding task count, and the count of test-adding tasks that introduce **more than one** named test identifier. A task introducing more than one named test violates the atomic-step discipline and is a `FINDING` naming that child.
- [ ] [P10-T11] Cross-check the planned test-task count against the delivered diff and write the phase verdicts into both artifacts.
  - Acceptance: the artifact records the `[TestMethod]` count added across the epic's diff (`git diff <merge-base>..HEAD -- 'QuickFiler.Test/**/*.cs'` using the merge-base recorded in [P0-T11]), the `[TestClass]` and file counts under `QuickFiler.Test/`, and the summed planned test-task count. The two need not match exactly — a scaffold task legitimately creates a `[TestClass]` with zero `[TestMethod]` members — but a large excess of `[TestMethod]`s over planned test tasks is a `FINDING` recorded with the per-child breakdown. Both artifacts carry an explicit verdict closing issue #136's second and third criteria.

---

### Phase 11 — Cross-Sibling Convention, Determinism, and Scenario Audit

Closes issue #136's fourth, fifth, and sixth acceptance criteria.

- [ ] [P11-T1] Scan `QuickFiler.Test/**/*.cs` for foreign test frameworks and record the result into `<FEATURE>/evidence/qa-gates/ac4-test-convention-scan.<TS>.md`.
  - Patterns: `using Xunit`, `using NUnit`, `\[Fact\]`, `\[Theory\]`, `\[Test\]\b`.
  - Acceptance: `SearchScope:`, `SearchPatterns:`, `SearchResult:` recorded; **the count must be 0**. This is the one absolute in this phase, and it is the `CLAUDE.md` §CUT1 prohibition. A non-zero count is `BLOCKING` naming the child attributed by `git log -S` against the matching line.
- [ ] [P11-T2] Record the MSTest, Moq, and FluentAssertions usage profile and the `.Should()`-to-`Assert.`-call ratio into the same artifact.
  - Acceptance: the artifact records that `using Microsoft.VisualStudio.TestTools.UnitTesting;` is present in every file declaring `[TestClass]` (any file missing it is a `FINDING`), the `using Moq;` / `new Mock<` counts, the `using FluentAssertions;` / `.Should()` counts, and the `Assert\.` count; then reports the `.Should()`-to-`Assert.` ratio for files added by this epic against the pre-epic ratio computed at the merge-base. A material regression is a `FINDING`; a small residue is not, because AC4 says "where practical". The artifact states verbatim that a grep proves the libraries are referenced, not that they are used idiomatically, and records the sampled reads that support the judgement.
- [ ] [P11-T3] Run the banned-API determinism scan over `QuickFiler.Test/**/*.cs` with comment stripping and record the result into `<FEATURE>/evidence/qa-gates/ac5-determinism-and-isolation-scan.<TS>.md`.
  - Symbols: `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`, `Path.GetTempPath`, `Path.GetTempFileName`, `File.WriteAllText`, `File.Create`, `Directory.CreateDirectory`, `Process.Start`, `new HttpClient`, `MessageBox`, `.Show()`, `UiThread.Init`, `SynchronizationContext.SetSynchronizationContext`.
  - Acceptance: `//`-prefixed and `///`-prefixed lines and the contents of `/* */` blocks are stripped **before** matching, and the artifact records both the raw hit count and the post-strip count. Without stripping, three known doc-comment false positives appear. All three search fields are recorded.
- [ ] [P11-T4] Compare the post-strip violation count against the pre-fan-in baseline of **1** and attribute any excess, appending the result to the same artifact.
  - Acceptance: the artifact records the measured count, the baseline of 1 (`QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:25`, `private DateTime now = DateTime.Now;`), and the signed difference. Any count above 1 is a new violation introduced by a child, attributed by `git blame` on the matching line and recorded as `BLOCKING` naming that child. A count of 0 means F4 fixed its in-scope violation and is recorded as such.
- [ ] [P11-T5] Audit the 500-line file limit across `QuickFiler/**/*.cs` and `QuickFiler.Test/**/*.cs` and append the result to the same artifact.
  - Acceptance: the artifact records every file over 500 lines with its owning child, and the **headroom distribution** for files between 480 and 500 lines, not merely the breach count. `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs` (578 at baseline, F4's in-scope obligation) and `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` (827 at baseline, already promoted as open issue **#450**) are checked explicitly by name. Generated `*.Designer.cs` files are exempt from the limit as generated code and are listed separately. Each remaining breach is `BLOCKING` naming its owning child.
- [ ] [P11-T6] Verify STA scoping and append the result to the same artifact.
  - Acceptance: the artifact records the count of `*.StaTests.cs` files and the count of `[STATestClass]` / `[STATestMethod]` occurrences under `QuickFiler.Test/`, with all three search fields. The pre-fan-in baseline is zero of each. Every STA-bound test must satisfy the file-name and attribute conditions **together**, and each must document why no seam is feasible; a violation of either is a `FINDING` naming the owning child. The DEC-1 conditions are checked on any test that constructs a Form: reuse of the `RunWithViewer` harness shape, never shown, `finally` dispose, `SynchronizationContext` save/restore, `ExceptionDispatchInfo` marshalling, and no `.Show()` / `.ShowDialog()` anywhere.
- [ ] [P11-T7] Record the first-run determinism figures and the comparison method into the same artifact using the [P0-T21] run.
  - Acceptance: the artifact records the [P0-T21] passed, failed, and skipped counts and the per-suite breakdown, states the comparison method (identical command, identical assembly set, second run captured by [P13-T7]), and states verbatim that a grep cannot detect a non-deterministic test that uses no banned symbol — an unseeded `Random`, a dictionary-ordering dependence, or a shared static mutated by two `[TestClass]`es — and that the repeated-run pass/fail delta is the only mechanical proxy available. The second run's figures and the delta are appended to this same artifact by [P13-T7]; this task does not itself run a second suite.
- [ ] [P11-T8] Produce the sampled scenario-completeness review into `<FEATURE>/evidence/qa-gates/ac6-scenario-completeness-review.<TS>.md`.
  - Method: for each per-file phase in each child's plan, record whether the phase's task set contains at least one task per scenario class, using the name-fragment screen — invalid input (`_Null`, `_Empty`, `_Invalid`, `ThrowsExactly<ArgumentNullException>`, `ThrowsExactly<ArgumentException>`), boundary (`_Zero`, `_Negative`, `_Max`, `_AtThreshold`, `_Boundary`, `_Twice`), error handling (`_Throws`, `_Rethrows`, `_DoesNotThrow`, `_Cancel`, `OperationCanceledException`, `_Failure`), and positive (no negative fragment, value or state assertion).
  - Acceptance: the artifact discloses the **sample size** and the selection rule for the manual half of the review, presents the name-fragment counts as a screen, and states verbatim that **a name-fragment count is not presented as proof of scenario completeness** — the screen counts labels, not behaviors, and cannot detect a scenario class the planner never considered.
- [ ] [P11-T9] Add the branch-coverage corroboration and write the phase verdicts into all three artifacts.
  - Acceptance: `<FEATURE>/evidence/qa-gates/ac6-scenario-completeness-review.<TS>.md` carries the per-file branch column from [P5-T4] and states that a file at `>= 80%` line and `< 75%` branch is prima facie evidence that its negative scenarios are thin, listing every such file with its owning child. The verdict for issue #136's sixth criterion is recorded as **closed by a documented, sampled, partly-manual review with branch coverage as corroboration**, with its limits stated. The other two artifacts each carry their own explicit verdict.

---

### Phase 12 — Defect-Trail and Upstream-Condition Verification

- [ ] [P12-T1] Enumerate the live issue index with `gh issue list --limit 500 --state all --json number,title,state` and record the full result into `<FEATURE>/evidence/qa-gates/ac11-defect-trail-verification.<TS>.md`.
  - Acceptance: all four schema fields present and the returned issue count recorded. A full index walk is required because the research pass rested on two keyword searches against a truncated web index; a defect promoted under wording that misses both queries would otherwise be missed. If `gh` is unavailable, record the blockage explicitly and the substitute source used, and mark every downstream absence claim `UNDETERMINED` rather than `none`.
- [ ] [P12-T2] Check F4's defects 1-6 asymmetrically against the enumerated index and append the per-defect result to the same artifact.
  - Defects: the leaked `BeforeItemMove` subscription on parent-folder change; the handler predicate reading live COM instead of the cached ID; the unsynchronised `Queue<T>` across the dispatcher boundary; the `Reset` double-dispose; the `DequeueChunk` unbounded regrowth; the missing `[Flags]` on `QfEnums.InitTypeEnum`.
  - Acceptance: each defect records the matching open issue number and title, or `SearchScope:` / `SearchPatterns:` / `SearchResult:` proving absence. **Absence is a `BLOCKING` finding naming F4.** Defects 1 and 2 are additionally checked against open issue **#426** (`emailmovemonitor-rejected-item-hook-retention`), which the epic says is plausibly the same underlying defect — folding into #426 is acceptable if F4 recorded that determination, but silently dropping them on the assumption that #426 covers them is not. **F16 does not promote any of the six on F4's behalf.**
- [ ] [P12-T3] Verify F4's promotion receipts exist in F4's own evidence and append the result to the same artifact.
  - Acceptance: `SearchScope:` (F4's resolved feature folder from [P0-T14], its `evidence/` tree), `SearchPatterns:` (promotion-receipt and issue-update mirror filename patterns including `issue-*.md`), `SearchResult:` (paths or `none`). Absence of a receipt for a defect that does have an issue is a `FINDING`; absence of both is already covered by [P12-T2].
- [ ] [P12-T4] Verify F4's defect 7 is fixed in code and append the result to the same artifact.
  - Acceptance: `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs` contains no banned `DateTime.Now`, cross-referenced against the [P11-T4] count. A surviving occurrence is a `BLOCKING` finding naming F4. The file and line are re-read at execution time rather than transcribed from the planning-time `:25`.
- [ ] [P12-T5] Verify F4's defect 8 is fixed in code and append the result to the same artifact.
  - Acceptance: `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs` is measured and its line count is at or under 500. A count above 500 is a `BLOCKING` finding naming F4. If the file was split, every resulting file is measured and recorded.
- [ ] [P12-T6] Verify that no GitHub issue exists for F4's defects 7 or 8 and append the result to the same artifact.
  - Acceptance: `SearchScope:` (the [P12-T1] index), `SearchPatterns:` (`MailItemInfoTests`, `DateTime.Now`, `ConversationResolverTests`, `500-line`, `file-size-split`), `SearchResult:`. Defects 7 and 8 are test-policy violations in existing tests and are in-scope for F4's own execution, so **an issue for either is itself a `FINDING`** — it is deferral of in-scope work. Open issue **#450** (`quickfiler-formcontroller-tests-file-size-split`) covers a different file and is excluded from this test by name.
- [ ] [P12-T7] Verify F6's dead-region deletion landed and record the result into `<FEATURE>/evidence/qa-gates/ac12-upstream-conditions.<TS>.md`.
  - Acceptance: `QuickFiler/Controllers/QfcExplorerController.cs` contains no `#region Email Sorting To Rewrite`, with `SearchScope:` / `SearchPatterns:` / `SearchResult:`; the file's current line count is recorded against the planning-time 323; and the six members of the region (five `private static`, plus `internal static StripTabsCrLf`) are confirmed absent from the file. A surviving region is a `BLOCKING` finding naming F6. Open issue **#449** is recorded as narrowed to its two remaining findings (`ExplConvView_Cleanup` throwing on a public interface member, and `OpenQFItem` re-resolving the explorer), and F16 does not act on either.
- [ ] [P12-T8] Verify the epic manifest's placeholder issue numbers are resolved and append the result to the same artifact.
  - Acceptance: `<EPIC>/epic.md` YAML front matter is read at execution time and each of `issue_num: 1012` (F12), `issue_num: 1015` (F15), and `issue_num: 1016` (F16) is checked against its real number **495**, **496**, and **497** respectively, together with the `1012` and `1015` entries inside F16's own `depends_on` list. Every unresolved placeholder is a `FINDING` naming the epic manifest owner, recorded with the reason it matters: an epic-orchestrator dependency gate keyed on `depends_on` will fail to find a nonexistent issue and will either error or silently skip the dependency. **`epic.md` is not edited by this child** (D-6), and `git status --porcelain` is re-run to prove it is unmodified.
- [ ] [P12-T9] Record the state of every issue the epic names and write the phase verdicts into both artifacts.
  - Acceptance: `<FEATURE>/evidence/qa-gates/ac12-upstream-conditions.<TS>.md` records the number, title, and state of **#136**, **#227**, **#230**, **#426**, **#441**, **#449**, **#450**, **#457**, **#478**, **#492**, and **#494** from the [P12-T1] enumeration. Issue **#494** is recorded as an incomplete promotion if its body remains an unpopulated template, as a `FINDING` and not a repair. Both artifacts carry an explicit verdict closing `spec.md` AC11 and AC12 with the Blocking counts and the owning child named for each.

---

### Phase 13 — Final Toolchain QA Loop and Repository-Wide After Capture

Runs as **one uninterrupted pass** in the mandated order. If any stage mutates a file, the pass
restarts at [P13-T2] and only the final clean pass counts. `EXIT_CODE: SKIPPED` is not a passing
outcome for any task in this phase.

- [ ] [P13-T1] Record the pre-pass tree state into `<FEATURE>/evidence/qa-gates/toolchain-prepass-state.<TS>.md` using `git status --porcelain` and `git diff --name-only <sha-from-P0-T11>..HEAD`.
  - Acceptance: the artifact proves that no `*.cs`, no `*.csproj`, no `packages.config`, and no `app.config` file differs from the state recorded in [P0-T11]; that `CLAUDE.md`, `.claude/rules/**`, and `<EPIC>/epic.md` are unmodified; and that the only modified or added paths are under `<FEATURE>/`, `scripts/vscode/Get-RepoWideCoverageSummary*.ps1`, and `tests/scripts/vscode/Get-RepoWideCoverageSummary*.Tests.ps1`. The gate is on these tree invariants, not on the literal SHA value.
- [ ] [P13-T2] Run `dotnet tool restore` and record `<FEATURE>/evidence/qa-gates/toolchain-restore.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0` with all four schema fields and the resolved csharpier version in `Output Summary:`.
- [ ] [P13-T3] Run `dotnet tool run csharpier format .` and record `<FEATURE>/evidence/qa-gates/toolchain-format.<TS>.md`.
  - Acceptance: all four schema fields plus the count of files reformatted. If the count is non-zero, the pass restarts at [P13-T2] after the mutation is recorded, and the restart is logged in the artifact. `.csharpierignore` already excludes `*.csproj`, `*.props`, `*.targets`, and `**/evidence/**`, so this command must not perturb `QuickFiler/QuickFiler.csproj` or any evidence artifact; a diff on either is a `BLOCKING` finding and the pass halts.
- [ ] [P13-T4] Run `dotnet tool run csharpier check .` and append the non-mutating confirmation to `<FEATURE>/evidence/qa-gates/toolchain-format.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0` and zero files reported unformatted, compared against the [P0-T17] baseline count.
- [ ] [P13-T5] Run `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record `<FEATURE>/evidence/qa-gates/toolchain-analyze.<TS>.md`.
  - Acceptance: all four schema fields with warning and error counts. The verdict is **baseline-relative** against [P0-T19]: a non-zero exit or a diagnostic count above the baseline is a `BLOCKING` finding with each **new** diagnostic listed and attributed to its file; a pre-existing diagnostic carried unchanged from the baseline is `INFORMATIONAL`. F16 does not fix a sibling's diagnostic; it reports it naming the owning child.
- [ ] [P13-T6] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` and record `<FEATURE>/evidence/qa-gates/toolchain-nullable.<TS>.md`.
  - Acceptance: all four schema fields with warning and error counts, evaluated baseline-relative against [P0-T20]. The artifact records verbatim that `/t:Rebuild` is required because MSBuild's incremental up-to-date check does not invalidate on a command-line property change alone (issue **#492**), and that `/p:Nullable=enable` is deliberately **not** passed solution-wide because that form emits `CS8630` on `QuickFiler.Test`.
- [ ] [P13-T7] Run the coverage-enabled full suite with the command from [P0-T21], changing only `-CoverageOutput` to `docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497/evidence/qa-gates/repo-coverage-after.<TS>.cobertura.xml`, and record `<FEATURE>/evidence/qa-gates/toolchain-test.<TS>.md`.
  - Acceptance: the Cobertura XML exists at that canonical path; the artifact carries all four schema fields plus **numeric** headline values — total tests, passed, failed, skipped, and the emitted root rates labelled `TOOL-EMITTED (#441-corrupted, NOT the authoritative figure)`. `Output Summary:` records the exact command string and shows it differs from [P0-T21]'s only in `-CoverageOutput`; any other difference invalidates the pair. The run is invoked from inside this worktree per the [P0-T18] guard. This task additionally appends this run's passed, failed, and skipped counts and the signed per-suite delta against [P0-T21] to `<FEATURE>/evidence/qa-gates/ac5-determinism-and-isolation-scan.<TS>.md`, completing the two-run determinism signal begun in [P11-T7]; a non-zero delta is a `FINDING` naming the flaky test and, where `git blame` attributes it, the owning child.
- [ ] [P13-T8] Run `mcp__drm-copilot__run_poshqc_format` and record `<FEATURE>/evidence/qa-gates/toolchain-ps-format.<TS>.md`.
  - Acceptance: the artifact records the formatter result and the four schema fields, with `EXIT_CODE:` carrying the tool-reported status because the MCP surface returns no process exit code. If any of the four capstone-owned PowerShell files is mutated, the C# and PowerShell passes both restart at [P13-T2] and the restart is logged.
- [ ] [P13-T9] Run `mcp__drm-copilot__run_poshqc_analyze` and record `<FEATURE>/evidence/qa-gates/toolchain-ps-analyze.<TS>.md`.
  - Acceptance: the artifact records the diagnostic count attributable to `scripts/vscode/Get-RepoWideCoverageSummary.Helpers.ps1`, `scripts/vscode/Get-RepoWideCoverageSummary.ps1`, `tests/scripts/vscode/Get-RepoWideCoverageSummary.Helpers.Tests.ps1`, and `tests/scripts/vscode/Get-RepoWideCoverageSummary.Tests.ps1`, which must be **zero**. Pre-existing diagnostics on other files are `INFORMATIONAL`.
- [ ] [P13-T10] Run `mcp__drm-copilot__run_poshqc_test` and record `<FEATURE>/evidence/qa-gates/toolchain-ps-test.<TS>.md`.
  - Acceptance: the artifact records the passed and failed counts (failed must be 0) and confirms the two new suites appear as `<testsuite>` entries in the emitted Pester JUnit output. The scoped coverage figure for the capstone-owned scripts is the one recorded in [P3-T13] and is cited here rather than recomputed.
- [ ] [P13-T11] Determine the pipeline stage of the after artifact and recompute its repository-wide line and branch rates with `scripts/vscode/Get-RepoWideCoverageSummary.ps1`, recording the result into `<FEATURE>/evidence/qa-gates/repo-coverage-after.<TS>.md`.
  - Acceptance: the artifact records the stage, which must equal the before artifact's stage from [P4-T1]; the recomputed covered-line count, total-line count, and line rate to four decimal places; and the recomputed branch numerator, denominator, and rate. A stage mismatch makes the pair invalid and the phase verdict `BLOCKED` — `<sources>` presence is the one-glance raw-versus-post-processed discriminator.
- [ ] [P13-T12] Produce the before/after comparison into `<FEATURE>/evidence/qa-gates/repo-coverage-comparison.<TS>.md` by running `scripts/vscode/Get-RepoWideCoverageSummary.ps1 -BeforePath <before-cobertura> -AfterPath <after-cobertura> -OutputPath <that path>`.
  - Acceptance: the artifact records both recomputed line rates, both recomputed branch rates, the signed deltas, the `retained-or-improved` verdict, the identical command used for both captures, both `<package>` sets, and the exit code. It states verbatim that **no repository-wide figure is imported from another branch, tool, or artifact**, records D-3 so the epic's refuted explanation of the 70.19% → 85.65% swing is not propagated, and reports the absolute `CLAUDE.md` 80% and `.claude/rules/general-unit-test.md` 85% / 75% floors as `INFORMATIONAL` alongside the delta gate, citing open issue **#494** rather than re-adjudicating the conflict.
- [ ] [P13-T13] Prove per-file verdict stability by re-running F1's harness against the after artifact with the [P1-T7] contract and recording the comparison into `<FEATURE>/evidence/qa-gates/per-file-verdict-stability.<TS>.md`.
  - Acceptance: the artifact records the per-file verdict set from the after artifact and shows it identical to the Phase 5 verdict set derived from the before artifact. F16 changes no production code, so the two must agree; any divergence is itself a `FINDING`, is enumerated file by file, and the after-artifact verdicts become the ones cited in Phase 14.
- [ ] [P13-T14] Record the toolchain deviations from `CLAUDE.md` into `<FEATURE>/evidence/qa-gates/toolchain-deviations.<TS>.md`.
  - Acceptance: the artifact records the csharpier subcommand deviation (pinned 1.2.6 requires `format` / `check`; the bare `csharpier .` in `CLAUDE.md` §C#1 and §CUT3 is v0 syntax and fails), the `/t:Rebuild` requirement for the nullable gate citing issue **#492**, and the deliberate omission of `/p:Nullable=enable`. It states verbatim that **`CLAUDE.md` is not amended**, and `git status --porcelain` is re-run to prove `CLAUDE.md` and `.claude/rules/**` are unmodified.
- [ ] [P13-T15] Write the single-uninterrupted-pass attestation into `<FEATURE>/evidence/qa-gates/toolchain-pass-attestation.<TS>.md`, closing `spec.md` AC7.
  - Acceptance: the artifact lists every stage of the final pass in mandated order with its command, exit code, and artifact path; records the number of restarts and the reason for each; and states that the recorded results are all from the **final clean pass**. No stage records `EXIT_CODE: SKIPPED`.

---

### Phase 14 — Issue #136 Closure and Acceptance-Criteria Check-Off

Runs after Phase 13 per D-12, because two of the artifacts this phase cites are produced by the final
QA loop. Only Markdown under `<FEATURE>/evidence/` is written here, and `.csharpierignore` excludes
`**/evidence/**`, so nothing in this phase can invalidate Phase 13.

- [ ] [P14-T1] Create `<FEATURE>/evidence/qa-gates/issue-136-ac-closure.<TS>.md` with issue #136's eight acceptance criteria quoted verbatim from `gh issue view 136 --json number,title,state,body`, together with all four schema fields.
  - Acceptance: exactly eight criteria are quoted verbatim from the live issue body, and the issue state is recorded. If `gh` is unavailable, record the blockage and the substitute source, and mark the transcription provenance explicitly.
- [ ] [P14-T2] Map #136's first criterion ("All production `.cs` files in QuickFiler.csproj reach minimum 80% line coverage") to its closing evidence in the same artifact.
  - Acceptance: cites `<FEATURE>/evidence/qa-gates/per-file-coverage-reconciliation.<TS>.md` and `<FEATURE>/evidence/qa-gates/denominator-rederivation.<TS>.md`, and records the numeric row count, the `testable` pass count on **both** the line and branch axes, the `UNLEDGERED` count, and the Blocking count. A criterion is marked `CLOSED` only when its Blocking count is zero; otherwise it is marked `NOT CLOSED` with each Blocking finding and its owning child listed.
- [ ] [P14-T3] Map #136's second criterion ("Coverage research and planning happens per-file with separate research artifacts") to its closing evidence in the same artifact.
  - Acceptance: cites `<FEATURE>/evidence/qa-gates/ac2-per-file-artifact-audit.<TS>.md` with the per-child matrix totals, the normalization rule used, the disclosed sample size for the manual half, and the D-7 determination with any shortfall named against its owning child.
- [ ] [P14-T4] Map #136's third criterion ("Each test case executes as an atomic step within per-file phases") to its closing evidence in the same artifact.
  - Acceptance: cites `<FEATURE>/evidence/qa-gates/ac3-atomic-test-step-audit.<TS>.md` with the per-child discovered convention, the test-adding task counts, the multi-test-task count, and the `[TestMethod]`-versus-planned-task cross-check.
- [ ] [P14-T5] Map #136's fourth criterion ("Tests follow MSTest conventions, use Moq for mocking, and FluentAssertions for assertions where practical") to its closing evidence in the same artifact.
  - Acceptance: cites `<FEATURE>/evidence/qa-gates/ac4-test-convention-scan.<TS>.md`, records the foreign-framework count (must be 0) as the one absolute, and reports the `.Should()`-to-`Assert.` ratio against the pre-epic ratio with the sampled reads that support the judgement.
- [ ] [P14-T6] Map #136's fifth criterion ("All tests remain deterministic, isolated, and independent of external dependencies or temp files") to its closing evidence in the same artifact.
  - Acceptance: cites `<FEATURE>/evidence/qa-gates/ac5-determinism-and-isolation-scan.<TS>.md`, records the post-strip banned-API count against the baseline of 1, the 500-line breach count and headroom distribution, the STA scoping result, and the two-run pass/fail delta from [P11-T7].
- [ ] [P14-T7] Map #136's sixth criterion ("Coverage includes positive paths, invalid inputs, boundary conditions, and error handling") to its closing evidence in the same artifact.
  - Acceptance: cites `<FEATURE>/evidence/qa-gates/ac6-scenario-completeness-review.<TS>.md`, records the disclosed sample size and the branch-coverage corroboration, and states verbatim that this criterion is closed as a **documented, sampled, partly-manual review** and that no name-fragment count is presented as proof.
- [ ] [P14-T8] Map #136's seventh criterion ("C# validation passes (formatting, analyzers, nullable safety, coverage execution)") to its closing evidence in the same artifact.
  - Acceptance: cites `<FEATURE>/evidence/qa-gates/toolchain-format.<TS>.md`, `toolchain-analyze.<TS>.md`, `toolchain-nullable.<TS>.md`, `toolchain-test.<TS>.md`, `toolchain-pass-attestation.<TS>.md`, and `toolchain-deviations.<TS>.md`, and records each stage's exit code from the final clean pass.
- [ ] [P14-T9] Map #136's eighth criterion ("Repository-wide coverage expectations are maintained or improved") to its closing evidence in the same artifact.
  - Acceptance: cites `<FEATURE>/evidence/baseline/repo-coverage-before-recomputed.<TS>.md` and `<FEATURE>/evidence/qa-gates/repo-coverage-comparison.<TS>.md`, records both recomputed figures and the signed delta, records that both artifacts came from the same complete pipeline with the identical command, and reports the absolute policy floors as `INFORMATIONAL` citing issue **#494**.
- [ ] [P14-T10] Write the consolidated Blocking-findings register into `<FEATURE>/evidence/qa-gates/issue-136-ac-closure.<TS>.md`, one line per finding across every phase.
  - Acceptance: each line names the finding, the owning child (F1-F15 or the epic manifest owner), the file or artifact, and the phase that produced it. The register is complete against Phases 1, 2, 5, 6, 7, 8, 9, 10, 11, 12, and 13. The artifact states verbatim: **"F16 fixed no sibling's coverage, granted no exemption, promoted no sibling's defect, and repaired no shared manifest."**
- [ ] [P14-T11] Check off the acceptance criteria in `<FEATURE>/spec.md` § Acceptance Criteria per `.claude/skills/acceptance-criteria-tracking/SKILL.md`.
  - Acceptance: each of AC1-AC12 is changed from `- [ ]` to `- [x]` **only** when its closing evidence exists and its Blocking count is zero; every AC left unchecked carries a one-line reason and a citation to the Blocking finding that prevents it. No other text in `spec.md` is altered.
- [ ] [P14-T12] Check off the matching criteria in `<FEATURE>/user-story.md` § Acceptance Criteria and the checkboxes in `<FEATURE>/spec.md` § Definition of Done and § Test Conditions.
  - Acceptance: `user-story.md` AC1-AC12 mirror `spec.md` AC1-AC12 exactly — the two lists are checked off together; the six `## Definition of Done` checkboxes and the five `## Test Conditions` checkboxes in `spec.md` are each checked only against a named artifact; and no other text in either document is altered.
- [ ] [P14-T13] Create the issue-update mirror at `<FEATURE>/evidence/issue-updates/issue-136.<TS>.md` and apply the corresponding check-offs to issue #136.
  - Acceptance: the mirror carries `Timestamp:`, the exact text intended or posted, and `PostedAs: body` or `PostedAs: comment` with the GitHub URL and `IssueUpdatedAt:` where posted. If posting is not possible, the mirror carries a `POSTING BLOCKED` header and the reason. Each of #136's eight checkboxes is checked only where [P14-T2] through [P14-T9] marked it `CLOSED`; an unclosed criterion stays unchecked with its Blocking finding named in the same update.
- [ ] [P14-T14] Write the feature outcome verdict into `<FEATURE>/evidence/qa-gates/issue-136-ac-closure.<TS>.md`, closing `spec.md` AC8.
  - Acceptance: the verdict is `PASS` only when every one of #136's eight criteria is `CLOSED`, every one of `spec.md` AC1-AC12 is checked, and the consolidated Blocking count is zero. **Any non-zero Blocking count makes the outcome `REMEDIATION-REQUIRED`, never `PASS`**, and the verdict lists the owning child for each finding so the epic orchestrator can route each one back to the child that owns it.

---

## Test Plan

- **Unit (PowerShell, Pester 5.x):** `tests/scripts/vscode/Get-RepoWideCoverageSummary.Helpers.Tests.ps1`
  (pipeline-stage discrimination; filename union with max-hits dedupe; the #441 descendant-axis
  regression fixture; condition-coverage richness on collision; ratio-of-sums branch rate; the
  zero-branch `n/a` case) and `tests/scripts/vscode/Get-RepoWideCoverageSummary.Tests.ps1`
  (entry-point exit codes 0/1/2, including the raw-versus-post-processed rejection). No temporary
  files, no external processes, all fixtures inline.
- **Integration:** none. This child adds no production C# and integrates with nothing at runtime.
- **Manual / CLI:** the full C# toolchain of Phase 13 and the two coverage runs of [P0-T21] and
  [P13-T7], which together also serve as the repeated-run determinism signal for issue #136's fifth
  criterion.
- **Coverage evidence:**
  - Baseline: `<FEATURE>/evidence/baseline/repo-coverage-before.<TS>.cobertura.xml`,
    `<FEATURE>/evidence/baseline/repo-coverage-before.<TS>.md`,
    `<FEATURE>/evidence/baseline/repo-coverage-before-recomputed.<TS>.md`.
  - Post-change: `<FEATURE>/evidence/qa-gates/repo-coverage-after.<TS>.cobertura.xml`,
    `<FEATURE>/evidence/qa-gates/repo-coverage-after.<TS>.md`,
    `<FEATURE>/evidence/qa-gates/toolchain-test.<TS>.md`.
  - Comparison: `<FEATURE>/evidence/qa-gates/repo-coverage-comparison.<TS>.md` and
    `<FEATURE>/evidence/qa-gates/per-file-verdict-stability.<TS>.md`.
  - Per-file: `<FEATURE>/evidence/qa-gates/per-file-coverage-harness.<TS>.md` and
    `<FEATURE>/evidence/qa-gates/per-file-coverage-reconciliation.<TS>.md`.
  - Capstone-owned PowerShell: `<FEATURE>/evidence/qa-gates/capstone-tooling-coverage.<TS>.md` and
    `<FEATURE>/evidence/qa-gates/capstone-tooling-coverage.<TS>.xml`.

## Open Questions / Notes

1. **F1's `exempt_ground` enum may not carry a fourth-ground value.** F1's spec enum has three values
   (`generated-designer`, `interface-only`, `irreducible-host-wiring`) and the epic later ratified a
   fourth ground for prohibited-to-execute adapters. [P0-T15] reads the **delivered** ledger schema.
   If `QuickFiler/Viewers/WebView2CoreInitializer.cs` has no valid enum value to carry, that is a
   `FINDING` naming F1, recorded in [P7-T10]; F16 does not widen the enum.
2. **`.claude/rules/python.md` exists in this checkout.** Research asserted it did not. The choice of
   PowerShell for capstone-owned tooling (D-9) does not rest on that claim: it rests on the fact that
   every repo-tooling script lives at `scripts/vscode/` in PowerShell, that F1's harness is
   PowerShell, and that `.claude/rules/powershell.md` supplies the toolchain and testing standard the
   new scripts are gated by. No task in this plan asserts the absence of a Python rules file.
3. **The five research-recorded sibling-folder absences are stale.** At planning time thirteen of the
   fourteen sibling folders resolve on this branch and only F15
   (`quickfiler-form-viewers-bayesian-coverage`, issue 496) does not. [P0-T14] re-derives the map at
   execution time rather than relying on any planning-time list.
4. **Three manifest placeholders remain, not five.** `epic.md` currently carries `1012` (F12), `1015`
   (F15), and `1016` (F16); F9, F10, F13, and F14 have since been back-filled to 452, 453, 455, and
   456. [P12-T8] checks the three that remain and re-reads the manifest rather than trusting this
   count.
5. **PowerShell branch coverage is not measurable in this repository.** The emitted JaCoCo carries a
   `LINE` counter and no `BRANCH` counter, so the `.claude/rules/powershell.md` 75% branch floor
   cannot be reported for the capstone-owned scripts. The gap is pre-existing, is recorded in
   [P3-T12], and is not closed by this child.
6. **`scripts/temp-extract-coverage.ps1` is deleted by F1's plan, not by F16.** If it survives at
   execution time, that is an F1 finding; no task in this plan deletes it, and no task cites any
   figure it produces.

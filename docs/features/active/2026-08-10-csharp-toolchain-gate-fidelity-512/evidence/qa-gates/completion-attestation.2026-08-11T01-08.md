# Completion attestation ([P7-T17])

Timestamp: 2026-08-11T01-08
Command: (none — analysis artifact)
EXIT_CODE: (none — analysis artifact)

Feature: `docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512`
Issue #512 (also closes #492, #509, #522). Work mode `full-bug`; AC source is `spec.md`.
Branch `bug/csharp-toolchain-gate-fidelity-512`; `MERGE_BASE` = `a5e336e5ae3443d4197caf5f87036fae1d538f89`.

---

## 1. Final-pass toolchain results, both languages

### C#

| Step | Command | `EXIT_CODE` | Non-vacuity | Artifact |
|---|---|---|---|---|
| 1 Format | `./.dotnet-sdk/dotnet.exe tool run csharpier format .` | **0** (`Formatted 1517 files`) | n/a | `final-csharpier-format.2026-08-11T00-48.md` |
| 1v Verify | `./.dotnet-sdk/dotnet.exe tool run csharpier check .` | **0** (`Checked 1517 files`) | n/a | `final-csharpier-check.2026-08-11T00-49.md` |
| 2 Analyze | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:m /fl "/flp:logfile=coverage/final-analyze.log;verbosity=normal"` | **0** (`0 Error(s)`) | `Skipping target "CoreCompile"` count **0** | `final-analyze.2026-08-11T00-51.md` |
| 3 Type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /v:m /fl "/flp:logfile=coverage/final-typecheck.log;verbosity=normal"` | **0** (`0 Error(s)`) | `Skipping target "CoreCompile"` count **0** | `final-typecheck.2026-08-11T00-52.md` |
| 4 Test | `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` | **not run — recorded scope deviation** | n/a | `csharp-test-step-scope.2026-08-11T00-55.md` |

**C# step-4 scope deviation (from [P6-T9]).** This feature modifies **no** `*.cs`, `*.csproj`,
`*.props` or `*.targets` file, verified by `git diff --name-only <MERGE_BASE>` and by an empty
`git status --porcelain -- '*.cs' '*.csproj' '*.props' '*.targets'`. `vstest.console.exe` and C#
coverage capture would therefore validate nothing this feature altered, and are deliberately not run.
The step is additionally failing on `main`'s tip for unrelated reasons
(`baseline-ci-parity-on-main.2026-08-10T15-05.md`). **This is a recorded deviation, not a skipped
planned command**: no plan task schedules a `vstest.console.exe` run, and the prohibited unexecuted-
command token is used nowhere as the value of an `EXIT_CODE:` field in this feature's evidence
(see § 3).

### PowerShell

| Step | Command | `EXIT_CODE` | Artifact |
|---|---|---|---|
| 1 Format | `mcp__drm-copilot__run_poshqc_format`, `scan_folders = ["scripts/vscode","tests/scripts/vscode"]` | **0** (equal to the merge-base value; zero files rewritten) | `final-poshqc-format.2026-08-11T00-34.md` |
| 2 Analyze | `mcp__drm-copilot__run_poshqc_analyze`, same scan folders | **1** — RED at the merge base; acceptance is zero new findings, **16 = 16**, multiset identical | `final-poshqc-analyze.2026-08-11T00-36.md` |
| 3 Test | `mcp__drm-copilot__run_poshqc_test`, `scan_folders = ["tests/scripts/vscode"]` | **0** | `final-poshqc-test.2026-08-11T00-40.md` |
| 3d Test (direct, coverage) | Pester 5.6.1 with coverage on `scripts/vscode/Invoke-VSBuild.ps1` | **0** — 41 tests, 41 passed, 0 failed; **85.71% line coverage** | `final-poshqc-test.2026-08-11T00-40.md` |
| Coverage delta | (analysis) | n/a | `powershell-coverage-delta.2026-08-11T00-45.md` |

**PoshQC analyze `EXIT_CODE: 1` is not a failure of this feature.** It is the pre-existing merge-base
condition recorded at [P0-T15]; the acceptance condition is a zero-delta finding multiset, which is
met.

**Every command listed above passed its stated acceptance condition in the final pass**, and the
loop-closure snapshot comparison in `final-toolchain-attestation.2026-08-11T00-57.md` shows the final
pass modified no tracked file in the feature's edit surface, so no restart was required.

---

## 2. Working-tree state

```
$ git status --porcelain -- '*.cs' '*.csproj' '*.props' '*.targets'
(empty)
```

**Empty**, as required. The only `*.cs` file touched at any point was
`UtilitiesCS/Extensions/QueueExtensions.cs`, perturbed by [P5-T5] and reverted by [P5-T6] with three
independent verifications.

`git status --porcelain` restricted to the feature's edit surface (as enumerated in [P6-T10]:
`CLAUDE.md .claude/rules/csharp.md .claude/skills/csharp-qa-gate/SKILL.md scripts/vscode
tests/scripts/vscode .vscode/tasks.json`) contains exactly the **six intended source edits**:

```
 M .claude/rules/csharp.md
 M .claude/skills/csharp-qa-gate/SKILL.md
 M .vscode/tasks.json
 M CLAUDE.md
 M scripts/vscode/Invoke-VSBuild.ps1
 M tests/scripts/vscode/Invoke-VSBuild.Tests.ps1
```

Everything else uncommitted is this feature's evidence artifacts, its plan and spec files, and the
promoted SD1 potential entry.

**Exclusion stated:** `.claude/agent-memory/**` is tracked in this repository and is excluded from
this gate, because agent-memory writes are not this feature's change. It shows no modification in the
current status output.

---

## 3. AC11 evidence-field verification

Total evidence artifacts under `FEATURE/evidence/`: **59**.

**All 50 artifacts produced by this plan** — the Phase 0 artifacts named by [P0-T1] through [P0-T18],
every `evidence/qa-gates/` artifact named by Phases 2, 5, 6 and 7, and the two
`evidence/regression-testing/` artifacts named by [P1-T3] and [P2-T7] — carry `Timestamp:`,
`Command:`, `EXIT_CODE:` and `Output Summary:`, using the literals
`Command: (none — analysis artifact)` and `EXIT_CODE: (none — analysis artifact)` where the plan's
preamble prescribes them ([P0-T1], [P0-T2], [P5-T12], [P5-T15], [P5-T16], [P6-T4], [P6-T9],
[P6-T10], [P7-T17], and — because both MCP routes were unavailable — [P7-T1]). Verified mechanically
by `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/check-evidence-fields.ps1`, which reports
`TCEO` for all 50.

**Every `EXIT_CODE:` field in this feature's evidence carries either an integer or the prescribed
`(none — analysis artifact)` literal. The prohibited unexecuted-command token appears as the value of
no `EXIT_CODE:` field anywhere in this feature's evidence.**

### The nine pre-existing artifacts, listed separately

These predate this plan's fail-closed field convention. Their non-compliance is **not** an AC11
failure and is **not** remediated by editing them.

| # | Artifact | Fields carried | Re-capture |
|---|---|---|---|
| 1 | `baseline/baseline-analyzer-step-vacuity.2026-08-10T14-55.md` | `Timestamp:` only | Re-captured by **[P0-T9]**, **[P0-T10]** and **[P0-T12]** (DOC-ANALYZE cold, DOC-ANALYZE warm, ANALYZE) in compliant form |
| 2 | `baseline/baseline-ci-parity-on-main.2026-08-10T15-05.md` | `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` | **`NO_RECAPTURE: baseline-ci-parity-on-main.2026-08-10T15-05.md` — [P6-T9] cites the `gh run list` / `gh api` measurement of CI on `main` rather than re-running it** |
| 3 | `baseline/baseline-csharpier-documented-command.2026-08-10T14-25.md` | all four | Re-captured by **[P0-T7]** |
| 4 | `baseline/baseline-csharpier-replacement-forms.2026-08-10T14-45.md` | `Timestamp:`, `EXIT_CODE:`, `Output Summary:` (commands appear in a table, not on a line-anchored `Command:` field) | Re-captured by **[P0-T8]**, **[P5-T1]** and **[P5-T2]**; its CSharpier file-type probes are cited by **[P5-T12]** |
| 5 | `baseline/baseline-mirror-provenance.2026-08-10T15-30.md` | all four | Its mirror-site enumeration is re-captured by **[P0-T17]** and **[P5-T11]**; its provenance findings are carried into the [P7-T1] entry and issue #535 |
| 6 | `baseline/baseline-nullable-gate-vacuity.2026-08-10T14-25.md` | all four | Re-captured by **[P0-T9]**, **[P0-T11]**, **[P0-T13]** and **[P0-T14]** (runs M1-M4) |
| 7 | `baseline/baseline-nullable-pragma-inventory.2026-08-10T14-35.md` | all four | **`NO_RECAPTURE: baseline-nullable-pragma-inventory.2026-08-10T14-35.md` — [P0-T13] measures the nullable error population, not the per-file `#nullable` pragma inventory** |
| 8 | `baseline/baseline-powershell-toolchain.2026-08-10T15-40.md` | all four | Re-captured by **[P0-T15]** (PSScriptAnalyzer, 16 findings) and **[P0-T16]** (Pester) |
| 9 | `regression-testing/negative-path-proof-dry-run.2026-08-10T15-20.md` | all four | Re-captured by **[P5-T5]**, a Phase 5 task |

---

## 4. Acceptance-criteria status summary

### Acceptance Criteria Status
- Source: `docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/spec.md`
- Total AC items: 13
- Checked off (delivered): 13
- Remaining (unchecked): 0
- Items remaining: none

**Check-off protocol note.** `acceptance-criteria-tracking` rule 3 requires that a check-off change
**only** `- [ ]` to `- [x]` and never modify the criterion text. Evidence pointers are therefore
recorded **here**, not inline in `spec.md`.

| AC | Evidence pointer | Key result |
|---|---|---|
| AC1 | `qa-gates/csharpier-format.2026-08-10T23-48.md`, `qa-gates/csharpier-check.2026-08-10T23-50.md` | both `EXIT_CODE: 0`; `Formatted 1517 files` / `Checked 1517 files` against the manifest-pinned CSharpier 1.2.6 |
| AC2 | `qa-gates/final-analyze.2026-08-11T00-51.md`, `qa-gates/final-typecheck.2026-08-11T00-52.md` | both `EXIT_CODE: 0` with a **zero** `Skipping target "CoreCompile"` count. **Recorded deviation:** AC2's `csc.exe` parenthetical measures zero at `verbosity=normal` even for genuine compiles, so the substantive non-vacuity requirement is met by the zero-skip assertion, which is strictly more discriminating; `CoreCompile:` header lines are not counted because they print even when the target is skipped. Recorded in `spec.md` § "The non-vacuity assertion mechanism" and § "Recorded deviations" item 1. |
| AC3 | `qa-gates/typecheck-positive-control.2026-08-10T23-55.md` | `EXIT_CODE: 0`, `0 Error(s)`, zero skip count |
| AC4 | `qa-gates/typecheck-negative-control.2026-08-10T23-58.md` (including the [P5-T6] revert confirmation appended in place), `qa-gates/typecheck-restore.2026-08-11T00-02.md` | `EXIT_CODE: 1` with `error CS8603` at `UtilitiesCS\Extensions\QueueExtensions.cs(25,20)` attributed to `UtilitiesCS.csproj`; zero skip count; project proven compiled from the log; perturbation reverted and verified three ways |
| AC5 | `qa-gates/ci-parity.2026-08-11T00-28.md` | type-check identical to `ci.yml` modulo the solution token; one deliberate analyzer difference (`/t:Rebuild` vs `/t:Build`) with in-line rationale at every edited site |
| AC6 | `qa-gates/site-inventory-reconciled.2026-08-11T00-18.md`; SD1 follow-up **issue #535** (https://github.com/drmoisan/TaskMaster/issues/535) | pattern (a) 10 hits all SD1 (before-state 10); pattern (b) 9 hits all SD1 (before-state 9); pattern (c) 14 hits = 10 SD1 (before-state 10) + 4 permitted residuals; zero half-corrected sites |
| AC7 | `qa-gates/csharpier-format.*`, `qa-gates/csharpier-check.*`, `qa-gates/analyze-rebuild.*`, `qa-gates/typecheck-positive-control.*`, `qa-gates/typecheck-negative-control.*`, `qa-gates/typecheck-restore.*`, `qa-gates/vscode-task-lint.*`, `qa-gates/vscode-task-typecheck.*`, `qa-gates/enablenullable-noop-proof.*`, `qa-gates/final-csharpier-format.*`, `qa-gates/final-csharpier-check.*`, `qa-gates/final-analyze.*`, `qa-gates/final-typecheck.*` | ordered verification artifacts for all three documented commands, each executed and recorded |
| AC8 | `qa-gates/no-relaxation-review.2026-08-11T00-25.md` | no threshold reduced, no mandatory step removed, zero added suppression tokens |
| AC9 | `qa-gates/protected-files-zero-diff.2026-08-11T00-22.md` (and `qa-gates/claudemd-ut2-guard.2026-08-10T23-42.md`) | zero-line diff for both protected rule files, identical SHA-256s, byte-identical § UT2 extract |
| AC10 | `qa-gates/rationale-prose-resolution.2026-08-11T00-20.md` | the false CSharpier-scope claim corrected against measured probes; six adjacent prose items recorded verified-correct; `.csharpierignore` recorded as a residual folded into issue #535 |
| AC11 | directory listings of `evidence/baseline/` and `evidence/qa-gates/`, plus § 3 above | all 50 plan-produced artifacts carry the four fields; the nine pre-existing artifacts are listed separately with per-artifact field status and re-capture or `NO_RECAPTURE:` |
| AC12 | `qa-gates/nullable-debt-record.2026-08-11T00-30.md` | **195 errors**, all `UtilitiesCS.csproj`, CS8766 x130 / CS8618 x23 / CS8625 x12 / CS8600 x9 / CS8601 x8 / CS8604 x7 / CS8602 x3 / CS8603 x2 / CS8714 x1; explicit lower bound (`>= 195`, solution-wide total unmeasured, build aborted after 22 of 73 `CoreCompile` executions); **not fixed here** |
| AC13 | `spec.md` § "SD2 — The analyzer-step vacuity (AC13): INCLUDE, and record the widening"; corrected analyzer sites edited by **[P3-T2]** (`CLAUDE.md` § C#1 item 2, Block R2), **[P4-T2]** (`.claude/rules/csharp.md` § Toolchain item 2) and **[P4-T5]** (`.claude/skills/csharp-qa-gate/SKILL.md` step 2) | explicit recorded decision to correct the analyzer command alongside the type-check command; the widening is recorded; no follow-up issue is required because the second branch of AC13 is not taken |

---

## Output Summary

All 84 plan tasks are complete and checked off. The final toolchain pass is green for both languages
in a single pass: C# format `EXIT_CODE: 0`, C# analyze `EXIT_CODE: 0` with a zero `CoreCompile` skip
count, C# type-check `EXIT_CODE: 0` with a zero skip count, PoshQC format `EXIT_CODE: 0`, PoshQC
analyze `EXIT_CODE: 1` with zero new findings against the RED merge-base baseline, and PoshQC/Pester
41 passed / 0 failed at 85.71% line coverage. C# step 4 is a recorded scope deviation because no
`*.cs`, `*.csproj`, `*.props` or `*.targets` file is modified. `git status --porcelain` for those
globs is empty, the feature's edit surface contains exactly the six intended source edits, and all 13
acceptance criteria in `spec.md` are checked off with the evidence pointers tabulated above.

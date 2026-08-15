# Code Review — `build-ci-coverage-gate-fidelity` epic fan-in

- **Timestamp:** 2026-08-15T05-11
- **Range:** `git diff origin/main...HEAD` (`0569ac0b` → `22b5de02`), 404 files
- **Code surface reviewed:** 8 PowerShell files, 1 `.csproj`, 1 `.vscode/tasks.json`, plus the three
  hand-resolved Markdown policy documents from merge `fb8eff9b`

## Executive Summary

The PowerShell work is of good quality. `Get-CoberturaClassLineSummary` and
`Remove-CoberturaExemptClosureCoverage` are pure, well-documented, single-responsibility functions
with explicit precedence rules and non-obvious behaviour explained in comments that say *why*. Test
coverage on the changed production surface is 91.76% aggregate with the new file at 100%. The
`Invoke-VSBuild.ps1` deprecation of `-EnableNullable` is handled correctly: the parameter is retained
so existing callers still bind, made inert, and emits a warning naming the governing policy section.

Three defects are worth acting on. The most consequential is an ordering defect at the new
threshold-gate call site (CR-1) that inverts the intended fail-closed behaviour of the coverage
artifact. The second is a merge-resolution asymmetry that left `CLAUDE.md` asserting three facts
about `ci.yml` that the CI split made false (CR-2). The third is that the epic now ships two live
numeric coverage gates enforcing different floors (CR-3).

**Blocking: 0. Major: 5. Minor: 7.**

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | line 341 | `Assert-CoberturaLineCoverageThreshold` is invoked **before** `Set-Content`, so when the threshold is not met the function throws and the processed XML is never written. The file left at `-CoverageOutput` is the raw `dotnet-coverage` output: absolute paths, third-party packages unfiltered, duplicate classes unmerged, no `<sources>` element. Downstream consumers then read a wrong-shaped, vendor-inflated artifact, and the exception message quotes a percentage computed from a document that does not exist on disk. | Move the assertion to after `Set-Content` (and after the `Done.` message, or emit the path first), so the corrected artifact is always persisted and the gate fails on a written, inspectable file. | The gate's purpose is to fail a below-threshold run, not to suppress the corrected artifact that a reviewer needs in order to diagnose why. Writing then asserting is strictly more useful and equally strict. | `Invoke-MSTestWithCoverage.ps1:339-343`; `ConvertTo-KoverageCoberturaXml` recomputes root attributes at `Helpers.ps1:441-447`, so the processed and unprocessed roots differ materially |
| Major | `CLAUDE.md` | lines 194, 202, 210 | Three references assert that `.github/workflows/ci.yml` contains the CSharpier step, the analyzer `msbuild` step, and a step named "Build with nullable warnings treated as errors". After merge `fb8eff9b` brought in the #553 CI split, `ci.yml` contains **no** `msbuild` and **no** `csharpier` invocation; it is a caller dispatching five reusable workflows. The merge updated `.claude/rules/csharp.md` to name `_build-analyzers.yml` and `_build-nullable.yml` but did not apply the same correction to `CLAUDE.md`. | Update the three references to `_format-check.yml`, `_build-analyzers.yml`, and `_build-nullable.yml` respectively, matching the wording already adopted in `.claude/rules/csharp.md`. | `CLAUDE.md` is always loaded. An agent following line 210's instruction to compare against `ci.yml` finds no such step and cannot verify the parity claim the line makes. This is the exact cross-child interaction that per-child review could not catch. | `ci.yml` full text contains only five `uses:` job entries; `_build-analyzers.yml:50` has `/t:Build /m`; `_build-nullable.yml:47,57` has the named step and `/t:Rebuild /m` |
| Major | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` + `.claude/hooks/validate-feature-review-coverage.ps1` | `Helpers.ps1:487` vs `hook:313,323` | The repository now has two live numeric coverage gates on the same metric enforcing different floors: the runner throws below **80%**, the review hook FAILs below **85.0** and blocks below a `$BranchFloor = 75.0`. Any repository-wide line figure in `[80, 85)` passes one gate and fails the other. The hook's own `.SYNOPSIS:29` still documents "below 80 percent" while enforcing 85.0. | Select one floor and apply it in both gates, or record in `CLAUDE.md` § UT2 which gate is authoritative for which decision. Track under the existing upstream reconciliation prompt. | The epic's stated goal is that gates report truthfully. Two gates returning opposite verdicts on one artifact is the defect class the epic exists to remove, now expressed in executable code rather than only in prose. | `Assert-CoberturaLineCoverageThreshold` throws at `$percentage -lt 80`; hook line 313 `$RepoWidePct -lt 85.0`; `CLAUDE.md:303` says 80, `.claude/rules/general-unit-test.md:23-24` says 85/75 |
| Major | repository-wide (PowerShell) | — | Repository-wide PowerShell line coverage is 71.51% (502/702) against the 85% floor. | No action required in this branch. Track the five uncovered scripts as a separate remediation item. | The entire shortfall sits in five scripts absent from this diff, and the branch raises the figure from roughly 67.2% to 71.5%. Changed-line coverage is 100%. | `.../494/evidence/baseline/powershell-baseline.jacoco.xml`; per-file table in the policy audit § 1.2 |
| Major | `.github/workflows/**` (unchanged) | — | No green CI run covers the current head. The last green integrated-tree run (31493339489) was against `c7d398c2`; `#494` (`85ff0c34`) and the `main` merge (`fb8eff9b`) landed after it. | Let the integration PR against `main` supply the green signal before merge, as `epic-status.md` already prescribes. | The check-run names changed with the CI split, so the prior green run is not evidence for the current job set either. | `epic-status.md` § Integrated-Tree CI Gate |
| Minor | `.claude/agent-memory/feature-review/MEMORY.md` | index | Nine memory files lost their own index lines in the conflict resolution: `code-review-findings-table-header.md`, `code-review-required-headings.md`, `feature-audit-checkoff-heading-case.md`, `feature-audit-requires-summary-heading.md`, `policy-audit-comparison-line-schema.md`, `policy-audit-numeric-new-code-coverage.md`, `policy-audit-required-structure.md`, `policy-audit-section7-row-label-parser.md`, `policy-audit-validator-uses-full-template.md`. | Leave as is, or delete the nine files if the consolidating entry is intended to replace them. | Not information loss: all nine files still exist and the `taskmaster-validator-memories-are-cross-repo` entry names every one of them. The consolidation was deliberate on the integration side. | `git show fb8eff9b^2:...MEMORY.md` link-target set minus merge-result set |
| Minor | `.claude/skills/csharp-qa-gate/SKILL.md` | line 35 | "CI may retain `/t:Build` on a cold checkout" is imprecise after the CI split: true for `_build-analyzers.yml`, false for `_build-nullable.yml`, which uses `/t:Rebuild /m`. | Mirror the per-job precision already adopted in `.claude/rules/csharp.md`. | The sentence is hedged with "may", so it is imprecise rather than false, but it now diverges from the sibling rule file that the same merge corrected. | `_build-nullable.yml:57` |
| Minor | `.../494/evidence/baseline/coverage-remeasurement-run{1,2,3}` | `.raw.` / `.corrected.` pairs | Each `.raw.` file is **byte-identical** to its `.corrected.` sibling (`cmp` reports no difference for all three pairs), and both carry `lines-valid="62401"` — a post-filter figure, not an unfiltered one. The `.raw.` label is inaccurate and roughly 31 MB is duplicated. | Either regenerate genuine pre-correction files or drop the `.raw.` copies and rename. | The three-run reproducibility conclusion is unaffected (three independent runs remain), but an evidence file labelled `raw` that is post-processed misleads a later reader. | `cmp -s` on all three pairs; root `lines-valid=62401` versus the unfiltered figures 110849 / 161086 recorded in `#441` AC-2/AC-3 |
| Minor | `scripts/vscode/Invoke-VSBuild.ps1` | — | Two committed artifacts disagree on this file's coverage: 85.71% (42/49 commands, `#512` targeted Pester run) and 83.72% (36/43 lines, `#494` full-suite JaCoCo). | Note the metric difference where either figure is cited. | Pester's command-based metric and the JaCoCo LINE counter have different denominators, and the two runs selected different test sets. Neither figure is wrong; citing either without its basis is. | `#512` `powershell-coverage-delta.2026-08-11T00-45.md`; `#494` `powershell-baseline.jacoco.xml` |
| Minor | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | whole file | 491 and 498 lines against the 500-line limit — 9 and 2 lines of headroom. | Plan the next split before the next change to either file. | Both comply today, but a two-line addition to the test file breaches a hard policy limit. | `awk 'END{print NR}'`; baselines were 357 and 222 |
| Minor | `artifacts/pr_context.summary.txt` | line 997 | `Core logic changes: 8 files` counts only the `.ps1` files and omits `.vscode/tasks.json` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. | Treat `git diff --numstat` as authoritative for scope, as this review did. | Does not affect the coverage hook's language enumeration, since neither extension maps to a language in `Get-ChangedLanguageSet`. | `git diff --numstat origin/main...HEAD` returns 10 non-Markdown, non-evidence files |
| Minor | `.agents/**`, `.github/instructions/**`, `.github/agents/**`, `AGENTS.md` | 19 sites | Still prescribe the CSharpier v0 bare-path form and `/t:Build ... /p:Nullable=enable`. A Codex or Copilot session following them runs a nullable gate that cannot fail. | Track under the existing follow-up issue #535. | Deliberately excluded under `#512`'s SD1 with a four-ground rationale, and `#512` AC6's final sentence permits enumerated exclusions. Recorded here because the epic's goal is gate fidelity and this residual defeats it for two of three agent runtimes. | `#512` `site-inventory-reconciled.2026-08-11T00-18.md`; re-verified against the current tree |

## Positive Observations

These are recorded because they are load-bearing for the verdict, not as praise.

- `Get-CoberturaClassLineSummary` (`Helpers.ps1:162-259`) is a genuinely pure function: no I/O, no
  mutation of the source document, an explicit documented precedence rule for repeated line keys
  (max hits, branch-if-either, larger condition-coverage denominator with a covered-count tiebreak),
  and a comment explaining why `GetAttribute` is used instead of property access under
  `Set-StrictMode -Version Latest`. The reason for the duplicated rate expressions at lines 368-373
  is stated in-code rather than left for a reader to infer.
- The `-EnableNullable` deprecation keeps the parameter bindable, makes it inert, and emits a warning
  that names `CLAUDE.md` C#1 item 3. This is the correct shape for a non-breaking deprecation and it
  is directly tested.
- `Remove-CoberturaExemptClosureCoverage` is invoked at `Helpers.ps1:427`, before
  `Merge-CoberturaClassesByFilename` — the ordering `#457` AC8 requires, and the ordering that makes
  the merge operate on already-filtered classes rather than re-introducing filtered lines.
- Test placement follows `.claude/rules/general-unit-test.md`: `scripts/vscode/X.ps1` maps to
  `tests/scripts/vscode/X.Tests.ps1` for all four changed production scripts. No colocation.
- The `#394` fix is minimal and verified complete: `PercentageFormatterTests.cs` `<Compile Include>`
  entries went from 2 to 1, and a sweep for duplicate `<Compile Include>` values across the whole
  project file returns none.

## Toolchain Status

No toolchain stage was executed by this review, which is check-only by contract. Toolchain results
are taken from committed evidence:

- CSharpier / analyzer / nullable gates: `#512` `evidence/qa-gates/` records a clean pass, including
  the negative-path proof that a deliberately introduced nullable violation fails the corrected gate.
- PoshQC format / analyze / test: recorded per child in each feature's `evidence/qa-gates/`.
  `#441` carries an amended AC-15 (no-new-findings against a Phase 0 baseline) because
  `Get-CoberturaLineConditionCoverageParts` carries a pre-existing `PSUseSingularNouns` finding that
  cannot be cleared without renaming an exported function the spec forbids changing.
- No `.cs` file is changed in this diff, so the C# toolchain has no changed input to re-verify.

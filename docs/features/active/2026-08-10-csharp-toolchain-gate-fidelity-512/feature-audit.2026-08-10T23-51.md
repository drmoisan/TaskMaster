# Feature Audit — 2026-08-10-csharp-toolchain-gate-fidelity-512

- **Artifact:** `feature-audit.2026-08-10T23-51.md`
- **Branch:** `bug/csharp-toolchain-gate-fidelity-512` (head `9773d6f5`)
- **Companion artifacts:** `policy-audit.2026-08-10T23-51.md`, `code-review.2026-08-10T23-51.md`

## Scope and Baseline

- **Base branch:** `origin/epic/build-ci-coverage-gate-fidelity-integration` (`143984a7`); merge base recomputed by this reviewer as `a5e336e5ae3443d4197caf5f87036fae1d538f89`, matching the caller-supplied value and every evidence artifact's recorded `MERGE_BASE`.
- **Work mode:** `full-bug` per the `- Work Mode: full-bug` marker in `issue.md`. Authoritative AC source: `spec.md` § Acceptance Criteria (AC1–AC13, mirrored verbatim from `issue.md`, which retains numbering authority). All 13 were checked off by the executor; each check-off is verified below against evidence rather than trusted.
- **Delivery:** single commit `9773d6f5`. Diff: 60 files — `CLAUDE.md`, `.claude/rules/csharp.md`, `.claude/skills/csharp-qa-gate/SKILL.md`, `.vscode/tasks.json`, `scripts/vscode/Invoke-VSBuild.ps1`, `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1`, plus the feature folder (spec/plan check-offs, 53 evidence artifacts) and the promoted SD1 potential entry.

## Acceptance Criteria Inventory

13 criteria, AC1–AC13, all currently `[x]` in `spec.md`. Two recorded deviations are declared in `spec.md` § "Recorded deviations" (AC2 counting mechanism; AC6 SD1 mirror exclusion) and are adjudicated inside the corresponding evaluations below.

## Acceptance Criteria Evaluation

| AC | Verdict | Verification performed |
|---|---|---|
| AC1 — documented format command executes against pinned CSharpier | **PASS** | Diff confirms every in-scope site now documents `dotnet tool run csharpier format .` / `check .`; both forms measured `EXIT_CODE: 0` against the manifest-pinned 1.2.6 (`qa-gates/csharpier-format.2026-08-10T23-48.md`, `csharpier-check.2026-08-10T23-50.md`, re-run in the final pass at `final-csharpier-format/check.2026-08-11T00-48/49.md`). The bare-path form's failure is captured verbatim in `issue.md` and baseline evidence |
| AC2 — type-check performs a genuine compilation, proven non-vacuously | **PASS** (with the recorded deviation accepted) | The `csc.exe`-count parenthetical is unsatisfiable at `verbosity=normal` (measured zero even for genuine compiles), so the executor substituted a zero-count assertion on `Skipping target "CoreCompile"`, recorded as a formal deviation in `spec.md`. Adjudicated at least as discriminating as the criterion's intent: it separates the vacuous runs (18 skips) from genuine ones (0 skips), and `skip count == 0` is stronger than `csc count > 0` (no project short-circuited vs at least one compiled). Applied and recorded in every MSBuild evidence artifact (`final-analyze`, `final-typecheck`, both controls) |
| AC3 — corrected type-check passes on an unperturbed clean checkout | **PASS** | `qa-gates/typecheck-positive-control.2026-08-10T23-55.md`: `EXIT_CODE: 0`, `0 Error(s)`, zero skips, 15.0 s; corroborated pre-change (`baseline-typecheck-rebuild.2026-08-10T22-59.md`) and by CI's green step on `main`'s tip |
| AC4 — negative-path proof (#512), non-vacuous, reverted | **PASS** | `qa-gates/typecheck-negative-control.2026-08-10T23-58.md`: perturbation appended to `UtilitiesCS/Extensions/QueueExtensions.cs` (carries `#nullable enable`); corrected command returned `EXIT_CODE: 1` with exactly one diagnostic, `error CS8603` at the perturbed file, attributed to `UtilitiesCS.csproj`; non-vacuity asserted from the log (project built under `Rebuild` on node 19, zero skips). Revert proven three ways (empty porcelain status, zero grep hits, line count 21 restored). Independently confirmed by this reviewer: no `*.cs` file appears anywhere in the branch diff. The perturbed project is genuinely recompiled by the corrected command by construction (`/t:Rebuild`) and by log evidence, so the proof is non-vacuous |
| AC5 — documented commands consistent with ci.yml; deliberate differences stated in-line | **PASS** | This reviewer independently compared the corrected commands against `.github/workflows/ci.yml`: type-check is character-for-character CI's command modulo the solution token; format resolves the same pinned tool with the explicit `tool run` spelling and stated rationale; analyzer carries exactly one deliberate difference (`/t:Rebuild` vs CI's `/t:Build`) with in-line rationale at the normative sites (`CLAUDE.md` § C#1 item 2; `.claude/rules/csharp.md` items 2–3). A Minor non-blocking observation about the three condensed sites lacking the R4 sentence is recorded in `code-review.2026-08-10T23-51.md` |
| AC6 — repository-wide site inventory reconciled; deliberate exclusions enumerated with rationale | **PASS** | This reviewer re-ran all three grep patterns from `qa-gates/site-inventory-reconciled.2026-08-11T00-18.md` and reproduced the results exactly: pattern (a) 10 hits, all inside the SD1 allowlist; pattern (b) 9 hits, all SD1; pattern (c) 14 hits = 10 SD1 + 4 permitted residuals (two prohibition-prose lines introduced by R3/R5, the deprecation-warning text, and the pre-existing byte-identical comment at `TaskMaster/Ribbon/EngineCommandCatalog.cs:93`). Zero in-scope sites carry the CSharpier v0 bare-path form or a `/t:Build`-based nullable command. The SD1 exclusion is enumerated site-by-site with rationale in `spec.md` and the evidence artifact |
| AC7 — verification step exists, executed, recorded | **PASS** | Ordered qa-gate artifacts exist for every documented command (apply, verify, analyze, type-check), for both VS Code tasks, and for the deprecated-switch proof; each records command, exit code, and the non-vacuity count where applicable. Spec option (g) records why no new `scripts/` verification script was added |
| AC8 — no policy requirement relaxed, weakened, or deleted | **PASS** | Adjudicated independently in `policy-audit.2026-08-10T23-51.md` § 3: the `/p:Nullable=enable` removal is a strengthening on the measured evidence (vacuous-when-warm, unpassable-when-forced, negative control proves opted-in enforcement retained, CI parity). Hunk-by-hunk review confirms no threshold reduction, no step removal, zero added suppression tokens; the diff adds constraints (R6 evidence obligation, `ValidateSet`, manifest-pinning rule) |
| AC9 — protected files/sections unmodified | **PASS** | Independently verified by this reviewer: zero-length diff against the merge base for `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`; `CLAUDE.md` § UT2 extract byte-identical between base and head. Corroborates `qa-gates/protected-files-zero-diff.2026-08-11T00-22.md` and `claudemd-ut2-guard.2026-08-10T23-42.md` |
| AC10 — adjacent rationale prose corrected or recorded verified-correct | **PASS** | The false claim at `CLAUDE.md:188` ("formats only `*.cs`") is replaced with measured behavior; this reviewer verified the replacement text against `.csharpierignore` (which does exclude `*.csproj`/`*.props`/`*.targets` and does not exclude `*.xml`, consistent with the corrected prose and the 1517-file check count). Retained-verbatim items and the `.csharpierignore` comment residual (folded into #535) are recorded in `qa-gates/rationale-prose-resolution.2026-08-11T00-20.md` |
| AC11 — baseline and final-QC evidence with required fields for every command step | **PASS** | 26 baseline artifacts and 30 qa-gate artifacts exist; all 50 plan-produced artifacts carry the four required fields (mechanical attestation in `completion-attestation.2026-08-11T01-08.md` § 3; spot-checked). The nine pre-spec artifacts are listed separately with per-artifact field status and re-capture or `NO_RECAPTURE:` disposition, per the plan's convention |
| AC12 — nullable debt recorded as a measured figure with attribution, not fixed | **PASS** | `qa-gates/nullable-debt-record.2026-08-11T00-30.md`: 195 errors from MSBuild's own `N Error(s)` line, all attributed to `UtilitiesCS.csproj`, full per-diagnostic breakdown, explicit `>= 195` lower-bound qualification (build aborted before dependents compiled), double-counting trap documented. No `.cs` change in the diff, so nothing was "fixed here" |
| AC13 — analyzer-step vacuity resolved by explicit recorded decision | **PASS** | SD2 in `spec.md` records the INCLUDE decision, its rationale, the authorization argument, and the measured cost; the analyzer command is corrected at all in-scope sites (verified in the diff); the second branch (follow-up issue) is correctly not taken |

## Scope-Boundary Verifications (correct-by-design items)

- **Nullable debt burn-down deferred:** confirmed out of scope per spec/epic; the 195-error figure is recorded for the follow-on epic and no `.cs` change exists in the diff.
- **No MSTest/vstest run:** the deviation is adequately recorded as a reasoned artifact (`qa-gates/csharp-test-step-scope.2026-08-11T00-55.md`), grounded in the zero-C#-change diff and the pre-existing unrelated failure of that CI step on `main`'s tip, with the `.csproj` sync-guard log proving no project file was left modified. Evaluated as an acceptable, well-documented deviation.
- **PoshQC analyze RED at merge base:** verified as pre-existing (16 findings) with a zero-delta multiset at final; not a regression.
- **Mirror-tree residual (SD1):** follow-up issue **#535 verified to exist** (OPEN, title "Codex/Copilot instruction mirrors still document the CSharpier v0 command and the unpassable nullable command") via `gh issue view`; the promoted potential entry is in the diff at `docs/features/potential/promoted/2026-08-11-codex-copilot-instruction-mirrors-document-defective-csharp-toolchain-commands.md`; the exclusion is enumerated with per-path rationale.

## Check-Off Reconciliation

All 13 criteria evaluate **PASS**, so all 13 `[x]` check-offs in `spec.md` are confirmed correct and are left as-is. No criterion was unchecked, and no criterion text was modified.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/spec.md`
- Total AC items: 13
- Checked off (delivered): 13
- Remaining (unchecked): 0
- Items remaining: none

## Summary

All 13 acceptance criteria pass verification against evidence, with the two spec-recorded deviations (AC2 counting mechanism, AC6 SD1 mirror exclusion) adjudicated and accepted on their merits. The reviewer independently reproduced the AC6 greps, the AC9 protected-file diffs, the AC5 CI comparison, and the absence of any `.cs` change in the branch diff. Zero blocking findings. The delivery is complete relative to its baseline and ready for PR against the epic integration branch.

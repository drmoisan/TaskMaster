# PR #704 CSharpier Recovery Plan

## Scope and invariants

This plan resumes `artifacts/orchestration/orchestrator-state.json` at `S3_plan`; it does not replay completed issue #469 lifecycle, research, feature-document, implementation, or historical QA work. The recovery is limited to the exact 35 paths reported by GitHub Actions run `33396149197`, job `99501030607`:

```
QuickFiler.Test/app.config                 QuickFiler.Test/packages.config
QuickFiler/app.config                      QuickFiler/packages.config
SVGControl.Test/app.config                 SVGControl.Test/packages.config
SVGControl/app.config                      SVGControl/packages.config
Tags.Test/app.config                       Tags.Test/packages.config
Tags/app.config                            Tags/packages.config
TaskMaster.Test/app.config                 TaskMaster.Test/packages.config
TaskMaster/app.config                      TaskMaster/packages.config
TaskTree.Test/app.config                   TaskTree.Test/packages.config
TaskTree/app.config                        TaskTree/packages.config
TaskVisualization.Test/app.config          TaskVisualization.Test/packages.config
TaskVisualization/app.config               TaskVisualization/packages.config
ToDoModel.Test/app.config                  ToDoModel.Test/packages.config
ToDoModel/app.config                       ToDoModel/packages.config
UtilitiesCS.Test/app.config                UtilitiesCS.Test/packages.config
UtilitiesCS/app.config                     UtilitiesCS/packages.config
VBFunctions.Test/app.config                VBFunctions.Test/packages.config
VBFunctions/app.config                     VBFunctions/packages.config
```

The authoritative pre-recovery set proof is `evidence/remediation-baseline/p1-t2-csharpier-baseline-enumeration.2026-08-31T10-00.md`; `evidence/qa-gates/p2-t2-csharpier-set-comparison.2026-08-31T10-15.md` proves that the current set is identical and excludes the four issue #469 C# implementation/test paths. Do not modify, delete, or regenerate either artifact. Do not access the user's main checkout or older dirty source worktree, remove/prune any worktree, push, update the PR, or merge.

All new evidence uses `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/<kind>/` and includes `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. The full local C# pass is: CSharpier format plus check, analyzer Rebuild, nullable/type Rebuild, then coverage-enabled MSTest. If a command changes files or exits nonzero, record exact evidence, correct only the supported scope, and restart from the formatting task. A genuine unrelated failure is a blocker; record it and stop without manual fallback.

### Phase 0 — Recovery Baseline and Policy Capture

- [x] [P0-T1] Read `AGENTS.md`, `.agents/skills/policy-compliance-order/SKILL.md`, `.agents/skills/csharp/SKILL.md`, `.agents/skills/csharp-qa-gate/SKILL.md`, `.agents/skills/atomic-plan-contract/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, `.agents/skills/acceptance-criteria-tracking/SKILL.md`, `.agents/skills/orchestrator-state/SKILL.md`, and `.agents/skills/commit-message-conventions/SKILL.md` in policy order. Record the files read and order in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/baseline/phase0-instructions-read.2026-08-31T00-00.md`. Acceptance: the artifact contains `Timestamp:` and `Policy Order:` and identifies every named file.

- [x] [P0-T2] Re-read `artifacts/orchestration/orchestrator-state.json` and the two pre-recovery CSharpier-set artifacts named in this plan. Record the 35-path allowlist and the exclusions in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/baseline/p0-t2-recovery-scope.2026-08-31T00-00.md`. Acceptance: exactly 35 listed relative paths, all ending `app.config` or `packages.config`, and zero issue #469 C# implementation/test paths.

- [x] [P0-T3] Run `dotnet tool restore` from the repository root and record the manifest-pinned tool restoration in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/baseline/p0-t3-dotnet-tool-restore.2026-08-31T00-00.md`. Acceptance: `EXIT_CODE: 0`; `Output Summary:` includes the output of `dotnet tool run csharpier --version`.

- [x] [P0-T4] Run the read-only baseline command `dotnet tool run csharpier check .` from the repository root and record it in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/baseline/p0-t4-csharpier-check.2026-08-31T00-00.md`. Acceptance: `ExpectedExitCode: 1`, `EXIT_CODE: 1`, and the output lists exactly the P0-T2 allowlist; any addition or removal is a blocker.

- [x] [P0-T5] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record analyzer baseline diagnostics in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/baseline/p0-t5-analyzer-rebuild.2026-08-31T00-00.md`. Acceptance: the artifact records every warning/error summary and `EXIT_CODE`; a nonzero result is retained as baseline evidence but blocks recovery if the final result adds a diagnostic.

- [x] [P0-T6] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` and record compiler/nullable baseline diagnostics in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/baseline/p0-t6-nullable-rebuild.2026-08-31T00-00.md`. Acceptance: the artifact records every warning/error summary and `EXIT_CODE`; do not add `/p:Nullable=enable`.

- [x] [P0-T7] Run `pwsh -NoProfile -File 'scripts/vscode/Invoke-MSTestWithCoverage.ps1' -SearchRoot .` and record the complete baseline coverage-enabled MSTest outcome in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/baseline/p0-t7-mstest-coverage.2026-08-31T00-00.md`. Acceptance: `Output Summary:` records the MSTest summary, numeric Cobertura line-rate percentage read from `coverage/coverage.cobertura.xml`, and a per-file coverage-status table for every P0-T2 configuration path marked `NOT APPLICABLE — configuration-only formatter scope`; failure or unavailable coverage is a blocker.

### Phase 1 — Authorized Formatter Recovery

- [x] [P1-T1] Capture SHA-256 hashes for each P0-T2 allowlisted path and capture `git status --porcelain` before modification. Store both observations in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t1-pre-format-hashes.2026-08-31T00-00.md`. Acceptance: the artifact names all 35 paths and no other mutable source path.

- [x] [P1-T2] Run `dotnet tool run csharpier format` followed by exactly the 35 P0-T2 relative paths, from the repository root. Record the literal command, output, exit code, and before/after hashes in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t2-csharpier-format.2026-08-31T00-00.md`. Acceptance: `EXIT_CODE: 0`; only allowlisted paths have changed hashes; if a non-allowlisted path changes, stop and record a blocker without staging it.

- [x] [P1-T3] Run `dotnet tool run csharpier check .` and record the result in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t3-csharpier-check.2026-08-31T00-00.md`. Acceptance: `EXIT_CODE: 0` and no unformatted path is reported. If it fails, record the exact newly reported set; only a tool-result-supported expansion may be considered, otherwise stop.

- [x] [P1-T4] Run `git diff --name-only d69a572b2f1ce3d65866fd9e09c8028b55545ee7 --` and `git status --porcelain`, then compare their union with the P0-T2 allowlist plus the plan/checkpoint/evidence artifacts created by this plan. Record the comparison in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t4-scope-gate.2026-08-31T00-00.md`. Acceptance: every changed configuration path is on the 35-path allowlist, no issue #469 implementation/test path changed, and no out-of-scope source/configuration path appears.

### Phase 2 — Final C# QA Loop

- [x] [P2-T1] Begin the final loop with `dotnet tool run csharpier format` followed by exactly the 35 P0-T2 paths, then `dotnet tool run csharpier check .`. Write one artifact per command to `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p2-t1-csharpier-format.2026-08-31T00-00.md` and `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p2-t1-csharpier-check.2026-08-31T00-00.md`. Acceptance: both exit 0 and the before/after hashes of all 35 paths are identical for this final-pass formatting command; otherwise restart P2-T1.

- [x] [P2-T2] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the outcome in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p2-t2-analyzer-rebuild.2026-08-31T00-00.md`. Acceptance: `EXIT_CODE: 0` and zero new analyzer diagnostics against P0-T5; on failure, record exact diagnostics, make no unsupported change, and restart P2-T1 only after a permitted correction.

- [x] [P2-T3] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` and record the outcome in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p2-t3-nullable-rebuild.2026-08-31T00-00.md`. Acceptance: `EXIT_CODE: 0` and zero new compiler/nullable diagnostics against P0-T6; on failure, record exact diagnostics, make no unsupported change, and restart P2-T1 only after a permitted correction.

- [x] [P2-T4] Run `pwsh -NoProfile -File 'scripts/vscode/Invoke-MSTestWithCoverage.ps1' -SearchRoot .` and record the complete final coverage-enabled MSTest outcome in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p2-t4-mstest-coverage.2026-08-31T00-00.md`. Acceptance: `EXIT_CODE: 0`, zero new failing tests against P0-T7, and `Output Summary:` includes the numeric post-change Cobertura line-rate percentage plus a per-file coverage-status table for every P0-T2 configuration path marked `NOT APPLICABLE — configuration-only formatter scope`. If it fails, record the exact failing result and restart P2-T1 only after a supported correction.

- [x] [P2-T5] Compare P0-T5 through P0-T7 with P2-T2 through P2-T4 and write the delta verdict to `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p2-t5-zero-regression-delta.2026-08-31T00-00.md`. Acceptance: zero new analyzer diagnostics, zero new compiler/nullable diagnostics, zero new test failures, post-change overall coverage is greater than or equal to baseline, and a 35-row per-file comparison marks every allowlisted configuration path `NOT APPLICABLE — configuration-only formatter scope` with `NoAdverseDelta: true`; changed-line coverage is `NOT APPLICABLE — configuration-only formatter rewrites`.

### Phase 3 — Checkpoint, Commit, and Local Stop

- [x] [P3-T1] Update `artifacts/orchestration/orchestrator-state.json` with provisional recovery completion data: approved plan path, preflight/validator receipts, final command/evidence references through P2-T5, final scope result, and the provisional local-commit transition. Run `mcp__drm-copilot__validate_orchestration_artifacts` for `artifacts/orchestration/orchestrator-state.json` with `artifact_type: orchestrator-state`, `workspace_root` set to this worktree, `require_codex_topology: true`, `require_codex_model_routing: true`, and `require_model_routing: true`; persist the exact invocation inputs and JSON response in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p3-t1-checkpoint-validator.2026-08-31T00-00.md` with required evidence fields and in the checkpoint MCP-call receipt data. Acceptance: it retains prior issue #469 receipts and all prior baseline evidence references unchanged, reports no push/PR/merge/worktree-removal action, and the validator returns `ok: true` without `require_complete`.

- [x] [P3-T2] Stage only the 35 allowlisted configuration paths, `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/formatter-recovery-plan.2026-08-31T00-00.md`, recovery evidence artifacts through P2-T5 under that feature's `evidence/` folders, `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p3-t1-checkpoint-validator.2026-08-31T00-00.md`, and the provisional `artifacts/orchestration/orchestrator-state.json`. Acceptance: `git diff --cached --name-only` contains no other path and `git diff --cached --check` exits 0.

- [x] [P3-T3] Select a repository-compliant conventional commit message from the staged index using `.agents/skills/commit-message-conventions/SKILL.md`, then create one provisional local commit for the verified formatter recovery. Acceptance: the selected message identifies CI-format recovery, the commit contains only P3-T2 paths, and P3-T4 records the selected staged-index message; do not create the post-commit receipt until P3-T4.

- [x] [P3-T4] After P3-T3, write the provisional SHA, selected staged-index conventional message, and `FinalAmendedSHA: REPORTED_AFTER_COMMIT` marker to `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/other/p3-t3-local-commit.2026-08-31T00-00.md`, write the completed forbidden-action audit and local stop boundary to `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/other/p3-t4-local-stop.2026-08-31T00-00.md`, and update `artifacts/orchestration/orchestrator-state.json` with final checkpoint data. Before staging or amending, rerun `mcp__drm-copilot__validate_orchestration_artifacts` against that final checkpoint with `artifact_type: orchestrator-state`, this worktree as `workspace_root`, `require_codex_topology: true`, `require_codex_model_routing: true`, and `require_model_routing: true`; persist exact inputs and JSON response in `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p3-t4-final-checkpoint-validator.2026-08-31T00-00.md`. Do not modify the checkpoint after this validation. Stage only those two post-commit evidence paths, the final checkpoint-validator artifact, and the final checkpoint, verify `git diff --cached --name-only` and `git diff --cached --check`, then perform exactly one `git commit --amend --no-edit`. Acceptance: both checkpoint validations return `ok: true` without `require_complete`, the amendment contains only those post-commit evidence/checkpoint paths in addition to P3-T3 content, and it preserves the conventional commit message.

- [ ] [P3-T5] Read-only after the amendment: obtain the amended `HEAD` SHA, verify `git status --porcelain` is empty, verify no push, PR update, merge, worktree removal, or worktree prune command was run, and report the amended SHA externally. Acceptance: the task modifies no receipt, checkpoint, or tracked file after the amendment; handoff is limited to the amended commit SHA, changed paths, final-pass commands/results, checkpoint-validator state, and any blocker.

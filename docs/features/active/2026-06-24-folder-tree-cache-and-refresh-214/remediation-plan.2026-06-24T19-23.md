# Remediation Plan - folder-tree-cache-and-refresh (Issue #214)

- Timestamp: 2026-06-24T19-23
- Planner: feature-review remediation planning handoff using `atomic-planner` rules
- Primary remediation requirements: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/remediation-inputs.2026-06-24T19-23.md`
- Review artifacts:
  - `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/policy-audit.2026-06-24T19-23.md`
  - `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/code-review.2026-06-24T19-23.md`
  - `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/feature-audit.2026-06-24T19-23.md`
- PR context:
  - `artifacts/pr_context.summary.txt`
  - `artifacts/pr_context.appendix.txt`
- Original plan: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/plan.2026-06-24T15-42.md`
- Remediation planning handoff receipt: `mcp__drm_copilot.resolve_atomic_plan_prompt` returned `ok: true` for this target plan path on 2026-06-24T19-23.

### Phase 0 — Baseline and Policy Confirmation

- [x] [P0-T1] Read `AGENTS.md`, `.agents/skills/policy-compliance-order/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, `.agents/skills/atomic-plan-contract/SKILL.md`, `.agents/skills/csharp/SKILL.md`, `.agents/skills/csharp-qa-gate/SKILL.md`, `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/remediation-inputs.2026-06-24T19-23.md`, `artifacts/pr_context.summary.txt`, and `artifacts/pr_context.appendix.txt`; then write `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/remediation-baseline/phase0-instructions-read.2026-06-24T19-23.md` with `Timestamp:`, `Policy Order:`, and exact files read.
- [x] [P0-T2] Run `dotnet tool run csharpier format .` from the repository root and write `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/remediation-baseline/remediation-baseline-csharpier.2026-06-24T19-23.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/remediation-baseline/remediation-baseline-dotnet-analyzers.2026-06-24T19-23.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T4] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/remediation-baseline/remediation-baseline-nullable.2026-06-24T19-23.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T5] Run coverage-enabled MSTest with `TestCategory!=LiveOutlook` and write `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/remediation-baseline/remediation-baseline-mstest-coverage.2026-06-24T19-23.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, total tests, pass/fail counts, and repository coverage headline.

### Phase 1 — Cooperative Traversal and Notification Wiring

- [x] [P1-T1] Add a failing regression test proving that a deep hierarchy reader yields through the dispatcher during live traversal before all folder records are materialized; tag the task evidence as an expected fail-before result under `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/regression-testing/`.
- [x] [P1-T2] Refactor `IOutlookFolderHierarchyReader`, `OutlookFolderHierarchyReader`, and `FolderTreeSnapshotBuilder` so dispatcher-yield cadence and cancellation/deadline checks occur during live folder traversal, while preserving STA execution and avoiding `Task.Run` for live COM enumeration.
- [x] [P1-T3] Add or update tests for cancellation/deadline behavior during traversal and for no partial snapshot publication after cancellation or deadline expiration.
- [x] [P1-T4] Add a failing regression test proving that the public `OutlookFolderNotificationSink(Outlook.NameSpace)` path creates production subscription owners instead of an empty subscription list.
- [x] [P1-T5] Implement production Outlook store/folder notification subscription construction and deterministic disposal in `OutlookFolderNotificationSink`, using testable adapters or factories so unit tests do not require live Outlook COM.
- [x] [P1-T6] Add or update tests for folder add, remove, move, rename, store add, store remove, and dispose unsubscription behavior through fake notification sources.

### Phase 2 — Request Scope and Multi-Store Cache Correctness

- [x] [P2-T1] Add failing regression tests for request-scope mismatch: store A snapshot followed by all-store request, and store A snapshot followed by store B request.
- [x] [P2-T2] Update `OutlookFolderTreeService`, `FolderTreeSnapshot`, and request-scope metadata so a cached snapshot is reused only when it covers the requested store scope.
- [x] [P2-T3] Add failing regression tests proving that store A refresh after an all-store snapshot preserves store B nodes.
- [x] [P2-T4] Implement store-scoped refresh behavior that either merges refreshed store nodes into the existing all-store snapshot or schedules an all-store refresh when localized merge is not sufficient.
- [x] [P2-T5] Add tests for invalidation during an in-flight build to prove exactly one follow-up refresh and correct final snapshot scope.

### Phase 3 — Caller Migration and Evidence Corrections

- [x] [P3-T1] Replace or remove the direct `FolderTree` construction paths in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.FolderExtraction.cs` so issue #214 EmailDataMiner full-enumeration behavior uses the shared cached hierarchy service.
- [x] [P3-T2] Add or update EmailDataMiner tests proving scrape and folder extraction paths use `GetOlFolderSnapshotAsync` or the shared service instead of `GetOlFolderTree`.
- [x] [P3-T3] Update caller-migration evidence to scan `EmailDataMiner*.cs`, Ribbon, FilterOlFolders, and SubjectMap files with patterns that catch explicit and target-typed `FolderTree` construction.
- [x] [P3-T4] Re-run banned API, no-live-Outlook COM test, startup-scope exclusion, no out-of-scope issue reference, and file-size checks; write updated evidence under the active feature folder's canonical evidence paths.
- [x] [P3-T5] Run `git diff --check main..HEAD`; normalize generated text evidence where practical and document any retained machine-generated TRX/XML diagnostics as generated-output exceptions if they cannot be normalized without losing evidence fidelity.
- [x] [P3-T6] Update `spec.md` and `user-story.md` acceptance criteria checkboxes only after each corresponding criterion is verified by the remediation evidence.

### Phase 4 — Final QA and Review Handoff

- [x] [P4-T1] Run `dotnet tool run csharpier format .`; if it changes files or fails, fix the cause and restart the final QA loop from this task. Write `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/remediation-final-csharpier.2026-06-24T19-23.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P4-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; if it fails, fix the cause and restart the final QA loop from P4-T1. Write `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/remediation-final-dotnet-analyzers.2026-06-24T19-23.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P4-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; if it fails, fix the cause and restart the final QA loop from P4-T1. Write `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/remediation-final-nullable.2026-06-24T19-23.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P4-T4] Run coverage-enabled MSTest with `TestCategory!=LiveOutlook`; if it fails or coverage gates fail, fix the cause and restart the final QA loop from P4-T1. Write `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/remediation-final-mstest-coverage.2026-06-24T19-23.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, total tests, pass/fail counts, repository coverage, and issue-scoped coverage.
- [x] [P4-T5] Compare baseline and final coverage using the repository's issue #214 coverage comparison method and write `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/remediation-final-coverage-comparison.2026-06-24T19-23.md` with numeric baseline/final coverage and changed/new-code thresholds.
- [x] [P4-T6] Refresh PR context with base `main`, then run the repository feature-review workflow again for issue #214. The re-review must report PASS only if the remediation findings are closed and the required audit validators pass.

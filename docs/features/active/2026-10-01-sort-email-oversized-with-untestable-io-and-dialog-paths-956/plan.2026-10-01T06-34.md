# 2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths (Plan)

INCOMPLETE: stopped for quota

- **Issue:** #956
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-01T08-30 (initial authoring, stopped for quota before completion)
- **Status:** INCOMPLETE. The phase skeleton, decisions and blast radius below are authored; the Command Reference payloads, the full token tables and the planner self-review record are not yet written. This file must not be sent to executor preflight in this state.
- **Version:** 0.2 (draft)
- **Work Mode:** full-bug (issue.md line 12). AC source: FEATURE/spec.md section `## Acceptance Criteria`, AC1 to AC17 (spec.md lines 257 to 273).
- **Research:** FEATURE/research/2026-10-01T07-00-sort-email-oversized-with-untestable-io-and-dialog-paths-research.md
- **Branch:** bug/sort-email-oversized-with-untestable-io-and-dialog-paths-956 (worktree HEAD ref observed in the git worktree HEAD file). Base: the `MERGE-BASE:` value P0-T3 records (`git merge-base HEAD origin/main`); every diff gate is a two-dot comparison against that recorded SHA.
- **Execution session requirement:** the executor runs later, non-isolated, with pwsh available. Every command-bearing task runs a `git -C WORKTREE` invocation or one `pwsh -NoProfile -Command` payload whose first statement is `Set-Location -LiteralPath "WORKTREE"`. The executor never edits artifacts/orchestration/orchestrator-state.json, never runs `git update-index`, never edits hooks or permissions.

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

---

## Blast Radius

The Write Set is exactly these eleven files plus FEATURE/** (evidence, plan check-offs, spec AC check-offs). The footprint gate (P4-T13) enforces exactly this set.

- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (modify)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs` (new)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` (new)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs` (new)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` (new)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs` (new)
- `UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs` (new)
- `UtilitiesCS/UtilitiesCS.csproj` (modify: six Compile Include entries)
- `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` (new)
- `UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs` (new)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (modify: two Compile Include entries)

Not edited: UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs (byte-identical to merge-base, AC11), every caller, every other project file, scripts, config, runsettings, coverage.config. Local git-ignored scratch: coverage/ (except coverage/.gitkeep), trx and cobertura documents.

## Orchestrator Decisions (binding, not reopened)

- D1 split into six partial files, verbatim whole-member moves, `#nullable enable` plus the full merge-base using block (lines 2 to 19) per file, each under 500 lines after CSharpier.
- D2 seam design A: YesNoToAllPromptSession, five-argument core, three-argument forward without the exclusion, two-argument wrapper keeps it, ClearReadOnlyAttributeOnDisk adapter, static readonly RemoveReadOnlyPrompt, Cleanup_Files resets it. No settable static seam, no retry bound.
- D3 exactly two new test files (T1 to T11, S1 to S7); SortEmail_Tests stays byte-identical.
- D4 DIRECT coverage route; Level-1 comparison aggregates EmailParsingSorting SortEmail* classes in the final document against SortEmail.cs in the baseline; new type and new core each at least 90 percent line coverage.
- D5 no commits by this plan. D6 L1 to L4 and F1 to F3 not fixed and not promoted.

## Planner Decisions (authored this pass)

- PD-1 Phase order follows the CLAUDE.md bugfix workflow literally: the regression tests are written first and observed compile-red against production code that is byte-identical to merge-base (Phase 1, AC12), then the mechanical split (Phase 2, proven by a verbatim member census and a green production-project build; the test project cannot compile until Phase 3), then the seam (Phase 3).
- PD-2 Verbatim-move proof: Phase 0 copies SortEmail.cs to the git-ignored backup coverage\control-956\SortEmail.mergebase.bak (hash equal to the pre-edit hash). A census payload strips all whitespace and, for 36 merge-base member segments (S01 to S36, line ranges re-derived in this pass: 27-29, 31-40, 42-74, 76-110, 112-179, 181-208, 210-301, 303-453, 455-553, 555-561, 563-618, 624-627, 628, 630, 636-659, 661-699, 701-760, 762-823, 825-837, 839-886, 888-892, 893-904, 906-984, 986-1005, 1007-1016, 1018-1057, 1059-1103, 1105-1114, 1116-1123, 1125-1336, 1341-1351, 1353-1366, 1368-1384, 1386-1396, 1398-1429, 1431-1452), counts each segment in its destination file (must be 1) and in the other five (must be 0), and requires each file residual after removing its segments, header and closing braces to be empty. Phase 3 re-runs it over 32 segments (S10 Cleanup_Files, S13 the _removeReadOnly field, S21 the wrapper doc and S23 the core excluded) plus the prescribed Cleanup_Files token.
- PD-3 The three-argument overload becomes a non-async one-statement forward (async is not part of the signature; name, parameters, return type and accessibility unchanged).
- PD-4 Negative control: remove the statement `clearReadOnly(directory);` from the core; predicted failed set T2, T3, T4, T8, T9, T11 and passed set T1, T5, T6, T7, T10; restore by backup copy proven by SHA-256.
- PD-5 Session tests use no DataRow, so the TRX total is exactly 7; the combined SortEmail_ filter total is exactly 26 (15 existing plus 11 new).

## Phases

### Phase 0 — Policy Reads, Preconditions, Bootstrap and Baseline Capture

- [ ] [P0-T1] Read the policy files in order (CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, .claude/rules/quality-tiers.md, .claude/rules/csharp.md, .claude/rules/tonality.md, .claude/rules/plan-acceptance-gates.md, the four skills) and FEATURE/spec.md, FEATURE/issue.md and the research file, and write FEATURE/evidence/baseline/phase0-instructions-read.md with `Timestamp:`, `Policy Order:` and one line per file with its line count.
- [ ] [P0-T2] Verify full-bug preconditions read-only and write FEATURE/evidence/baseline/p0-t2-mode-preconditions.<TS>.md (issue.md carries `- Work Mode: full-bug`; spec.md has 17 unchecked AC lines; no user-story.md).
- [ ] [P0-T3] Record branch, `MERGE-BASE:`, inherited Clause A paths, source porcelain for UtilitiesCS/ and UtilitiesCS.Test/ and checkpoint readiness in FEATURE/evidence/baseline/p0-t3-worktree-context.<TS>.md.
- [ ] [P0-T4] Probe the pwsh channel, bootstrap SDK and tools, record runsettings and pre-edit SHA-256 values in FEATURE/evidence/baseline/p0-t4-channel-and-toolchain.<TS>.md.
- [ ] [P0-T5] Restore NuGet packages and write FEATURE/evidence/baseline/p0-t5-nuget-restore.<TS>.md.
- [ ] [P0-T6] Capture the baseline CSharpier check in FEATURE/evidence/baseline/p0-t6-csharpier-check.<TS>.md.
- [ ] [P0-T7] Capture the baseline analyzer rebuild in FEATURE/evidence/baseline/p0-t7-msbuild-analyzers.<TS>.md.
- [ ] [P0-T8] Capture the baseline nullable rebuild in FEATURE/evidence/baseline/p0-t8-msbuild-nullable.<TS>.md.
- [ ] [P0-T9] Run the stall probe once and record the exclusion in FEATURE/evidence/baseline/p0-t9-stall-probe.<TS>.md.
- [ ] [P0-T10] Capture the baseline scoped run (filter FullyQualifiedName~EmailIntelligence.SortEmail_, total 15) in FEATURE/evidence/baseline/test-run-baseline.md.
- [ ] [P0-T11] Capture the baseline coverage (DIRECT route) in FEATURE/evidence/baseline/coverage-baseline.md.
- [ ] [P0-T12] Census UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs (1454 lines; ExcludeFromCodeCoverage 28, YesNoToAll.ShowDialog( 9, _removeReadOnly 13, TrySaveAttachmentAsync( 7, File.Delete( 5, File.Exists( 6) and create the merge-base backup, writing FEATURE/evidence/baseline/p0-t12-pre-edit-census.<TS>.md.
- [ ] [P0-T13] Run the partition check of the 36 segments on the backup (each occurring once, residual exactly two closing braces) and write FEATURE/evidence/baseline/p0-t13-partition.<TS>.md.

### Phase 1 — Regression Tests First and Compile-Red Fail-Before

- [ ] [P1-T1] Create UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs with S1 to S7 (no DataRow).
- [ ] [P1-T2] Create UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs with T1 to T11 (sandbox literal C:\Sortemail956Sandbox).
- [ ] [P1-T3] Add the two Compile Include entries to UtilitiesCS.Test/UtilitiesCS.Test.csproj (numstat 2 added, 0 deleted).
- [ ] [P1-T4] Format the two new test files with scoped CSharpier and record hashes in FEATURE/evidence/other/p1-t4-scoped-format.<TS>.md.
- [ ] [P1-T5] Census the two test files in FEATURE/evidence/other/p1-t5-test-census.<TS>.md.
- [ ] [P1-T6] [expect-fail] Build UtilitiesCS.Test/UtilitiesCS.Test.csproj against unmodified production and record the compile-red run in FEATURE/evidence/regression-testing/fail-before-exception.<TS>.md.

### Phase 2 — Mechanical Split

- [ ] [P2-T1] Create UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs (S03, S04, S08, S09, S26).
- [ ] [P2-T2] Create UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs (S12, S10, S15 to S20, S24, S25, S28, S29).
- [ ] [P2-T3] Create UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs (S13, S21, S22, S23).
- [ ] [P2-T4] Create UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs (S14, S30).
- [ ] [P2-T5] Create UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs (S11, S31 to S36).
- [ ] [P2-T6] Edit UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs to `public static partial class SortEmail` retaining S01, S02, S05, S06, S07, S27 and dropping the class-level regions.
- [ ] [P2-T7] Add the five SortEmail Compile Include entries to UtilitiesCS/UtilitiesCS.csproj after line 817.
- [ ] [P2-T8] Format the six SortEmail files with scoped CSharpier, recording hashes in FEATURE/evidence/other/p2-t8-scoped-format.<TS>.md.
- [ ] [P2-T9] Run the verbatim move census (36 segments) and line counts in FEATURE/evidence/other/p2-t9-move-census.<TS>.md.
- [ ] [P2-T10] Build UtilitiesCS/UtilitiesCS.csproj green and record FEATURE/evidence/other/p2-t10-production-build.<TS>.md.

### Phase 3 — Prompt Seam, Session Type, Green Runs and Negative Control

- [ ] [P3-T1] Create UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs per D2.
- [ ] [P3-T2] Add the Dialogs\YesNoToAllPromptSession.cs entry after line 574 of UtilitiesCS/UtilitiesCS.csproj (numstat 6 added, 0 deleted).
- [ ] [P3-T3] Convert the three-argument method in UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs into the five-argument core.
- [ ] [P3-T4] Add the non-async three-argument forward in UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs.
- [ ] [P3-T5] Replace the _removeReadOnly field with RemoveReadOnlyPrompt in UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs.
- [ ] [P3-T6] Add ClearReadOnlyAttributeOnDisk with its justification comment in UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs.
- [ ] [P3-T7] Rewrite the wrapper documentation and justification comment in UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs.
- [ ] [P3-T8] Change Cleanup_Files in UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs to call RemoveReadOnlyPrompt.Reset().
- [ ] [P3-T9] Format the nine changed or new .cs files under UtilitiesCS/ and UtilitiesCS.Test/ with scoped CSharpier.
- [ ] [P3-T10] Run the post-edit census (32-segment move census, TrySave, session and test tokens) in FEATURE/evidence/other/p3-t10-post-edit-census.<TS>.md.
- [ ] [P3-T11] Build UtilitiesCS.Test/UtilitiesCS.Test.csproj green.
- [ ] [P3-T12] Run the SortEmail_ scoped filter (total 26) and write FEATURE/evidence/regression-testing/p3-t12-scoped-run.<TS>.md.
- [ ] [P3-T13] Run the YesNoToAllPromptSession_Tests filter (total 7) and write FEATURE/evidence/regression-testing/p3-t13-session-run.<TS>.md.
- [ ] [P3-T14] Apply the negative control to UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs after a backup copy.
- [ ] [P3-T15] [expect-fail] Build and run the SortEmail_TrySaveAttachment_Tests filter; record FEATURE/evidence/regression-testing/negative-control-clearreadonly-removed.md.
- [ ] [P3-T16] Restore UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs from the backup and prove the SHA-256.
- [ ] [P3-T17] Confirm the restored state (11 of 11 Passed) in FEATURE/evidence/regression-testing/p3-t17-post-restore-run.<TS>.md.

### Phase 4 — Final QC Loop, Coverage Comparison, Footprint and Acceptance Check-Off

- [ ] [P4-T1] Run `dotnet tool run csharpier format .` with Write Set hashes before and after; FEATURE/evidence/qa-gates/p4-t1-csharpier-format.<TS>.md.
- [ ] [P4-T2] Run `dotnet tool run csharpier check .`; FEATURE/evidence/qa-gates/p4-t2-csharpier-check.<TS>.md.
- [ ] [P4-T3] Run the analyzer rebuild; FEATURE/evidence/qa-gates/p4-t3-msbuild-analyzers.<TS>.md.
- [ ] [P4-T4] Run the nullable rebuild; FEATURE/evidence/qa-gates/p4-t4-msbuild-nullable.<TS>.md.
- [ ] [P4-T5] Run the SortEmail_ scoped test filter (26); FEATURE/evidence/regression-testing/test-run-final.md.
- [ ] [P4-T6] Run the session scoped test filter (7); FEATURE/evidence/regression-testing/p4-t6-session-run.<TS>.md.
- [ ] [P4-T7] Run the final coverage pass; FEATURE/evidence/qa-gates/coverage-post-change.md.
- [ ] [P4-T8] Compare coverage (Level 1 aggregate, package band); FEATURE/evidence/qa-gates/coverage-comparison.md.
- [ ] [P4-T9] Measure the core and session line coverage (at least 90 percent each) appended to FEATURE/evidence/qa-gates/coverage-comparison.md.
- [ ] [P4-T10] Post-format census and line counts; FEATURE/evidence/qa-gates/p4-t10-post-format-census.<TS>.md.
- [ ] [P4-T11] Write the TRX-derived summary FEATURE/evidence/regression-testing/test-results-summary.md.
- [ ] [P4-T12] Close the toolchain loop in FEATURE/evidence/qa-gates/toolchain-pass.md.
- [ ] [P4-T13] Verify the footprint against the eleven Write Set paths; FEATURE/evidence/qa-gates/p4-t13-scope-boundary.<TS>.md.
- [ ] [P4-T14] Sweep FEATURE/ for host identifiers and raw documents; FEATURE/evidence/qa-gates/p4-t14-hygiene-sweep.<TS>.md.
- [ ] [P4-T15] Check off AC1 to AC17 in FEATURE/spec.md (to be split into one task per AC in the completed revision).

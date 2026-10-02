# 2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths (Plan)

- **Issue:** #956
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-01 (revision 1.2, written after the P4-T8 stop record of 2026-10-01T21-26: the coordinator ruling AC15 option (a) applied in place to FEATURE/spec.md AC15, PD-7, CMD-SORTEMAIL-COMPARE, P4-T8, the P4-T9 prediction wording, P4-T30, the AC15 citation and mapping lines and the new Revision Log; revision 1.1 of 2026-10-01T14-00 is superseded at those locations only)
- **Status:** Revision 1.2 awaiting a confirming preflight (DIRECTIVE: PREFLIGHT VALIDATION ONLY). Phases 0 to 3 and P4-T1 to P4-T7 are executed and checked and are not altered by this revision. P4-T8 stopped at ITERATION 1 with `AC15: NOT MET` (adjusted delta 2, raw delta 3); under the coordinator ruling AC15 option (a) it is re-run read-only over the ITERATION 1 coverage documents once this revision clears preflight. No production or test code changes in this revision.
- **Version:** 1.2
- **Work Mode:** full-bug (issue.md line 12, `- Work Mode: full-bug`). AC source: FEATURE/spec.md section `## Acceptance Criteria` (spec.md line 256), AC1 to AC17 at spec.md lines 257 to 273, all unchecked at authoring time. No user-story.md exists or is required.
- **Research:** FEATURE/research/2026-10-01T07-00-sort-email-oversized-with-untestable-io-and-dialog-paths-research.md
- **Branch:** bug/sort-email-oversized-with-untestable-io-and-dialog-paths-956, cut from origin/main 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f (the #945 merge; artifacts/orchestration/orchestrator-state.json `base_sha`). Base: the `MERGE-BASE:` value P0-T3 records (`git merge-base HEAD origin/main`); every diff gate is a two-dot comparison of the working tree against that recorded SHA.
- **Execution session requirement:** the executor runs later, non-isolated, from the item worktree, with `pwsh` available (an isolated agent is refused `pwsh` in every form). Every command-bearing task runs either one `git -C WORKTREE ...` invocation or one `pwsh -NoProfile -Command '<payload>'` process whose first statement is `Set-Location -LiteralPath "WORKTREE"`. P0-T4 probes that channel first and stops with `CHANNEL UNAVAILABLE` if it is refused. The executor never edits artifacts/orchestration/orchestrator-state.json, never runs `git update-index`, and never edits hook, permission or policy files.
- **Pre-implementation gate requirement:** the hook .claude/hooks/enforce-orchestration-preimplementation-gate.ps1 denies an edit of a `.cs` file unless artifacts/orchestration/orchestrator-state.json is seeded. At authoring time it carries `issue-num` `956`, `feature-folder` docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956, `route_id` `preparation` and `lifecycle_ready` `true`. P0-T3 records the readiness fields read-only and stops with `PRE-IMPLEMENTATION GATE NOT SEEDED` when they are absent; a PreToolUse refusal at any later `.cs` edit is `PRE-IMPLEMENTATION GATE BLOCKED`, reported verbatim, and stops the run.
- **Task Count:** 75 (Phase 0: 13, Phase 1: 6, Phase 2: 10, Phase 3: 13, Phase 4: 33)

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

---

## Blast Radius and Write Set

The Write Set is exactly these eleven files plus FEATURE/** (evidence artifacts, this plan's check-offs, and the seventeen AC check-off boxes of FEATURE/spec.md). The footprint gate P4-T13 enforces exactly this set.

- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (modify)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs` (new)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` (new)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs` (new)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` (new)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs` (new)
- `UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs` (new)
- `UtilitiesCS/UtilitiesCS.csproj` (modify: six Compile Include entries, numstat 6 added, 0 deleted)
- `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` (new)
- `UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs` (new)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (modify: two Compile Include entries, numstat 2 added, 0 deleted)

Not edited (scope boundaries): UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs (byte-identical to MERGE-BASE, AC11); UtilitiesCS/Dialogs/YesNoToAll.cs; every caller (TaskMaster/Ribbon/RibbonController.cs line 230, TaskMaster/AppGlobals/AppOlObjects.cs line 301, QuickFiler/Controllers/EfcDataModel.cs line 309, UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs line 445); UtilitiesCS/Properties/AssemblyInfo.cs; every other project file; scripts/, config/, coverage.config, TaskMaster.runsettings, scripts/vscode/TaskMaster.cli.runsettings, .csharpierignore, .editorconfig, .gitignore; FEATURE/spec.md text other than the seventeen check-off boxes; FEATURE/issue.md and FEATURE/research/. Local, git-ignored, never staged: coverage/ (except coverage/.gitkeep, .gitignore lines 150 to 151), every trx (line 146) and cobertura-named XML document (line 147), packages/, .dotnet-sdk/ (line 356), bin and obj.

## Orchestrator Decisions (binding, not reopened)

- D1 split into six partial files, verbatim whole-member moves, `#nullable enable` plus the full merge-base using block (lines 2 to 19) per file, each under 500 lines after CSharpier.
- D2 seam design A: YesNoToAllPromptSession, five-argument core, three-argument forward without the exclusion, two-argument wrapper keeps it, ClearReadOnlyAttributeOnDisk adapter, static readonly RemoveReadOnlyPrompt, Cleanup_Files resets it. No settable static seam, no retry bound.
- D3 exactly two new test files (T1 to T11, S1 to S7); SortEmail_Tests stays byte-identical.
- D4 DIRECT coverage route; Level-1 comparison aggregates EmailParsingSorting SortEmail* classes in the final document against SortEmail.cs in the baseline; new type and new core each at least 90 percent line coverage.
- D5 no commits by this plan. D6 L1 to L4 and F1 to F3 not fixed and not promoted.

## Planner Decisions

- PD-1 (kept) Phase order follows the CLAUDE.md bugfix workflow literally: the regression tests are written first and observed compile-red against production code that is byte-identical to MERGE-BASE (Phase 1, AC12); then the mechanical split (Phase 2, proven by an exact whitespace-stripped file equality per partial file and a green production-project rebuild; the test project cannot compile until Phase 3); then the seam (Phase 3).
- PD-2 (revised this pass) Verbatim-move proof. P0-T12 copies SortEmail.cs to the git-ignored backup coverage\control-956\SortEmail.mergebase.bak. The 36 merge-base segments S01 to S36 of the Segment Table (line ranges re-derived in this pass) are proven to partition the backup by P0-T13 (`PARTITION-EXACT: True`: the whitespace-stripped backup equals header lines 1 to 22, the class line, the six class-level region lines, the 36 segments in file order and the two closing braces). Each partial file is then proven by `CMD-MOVE-CENSUS` to equal, after whitespace removal, header lines 1 to 22 + `publicstaticpartialclassSortEmail{` + its segments in Segment Table order + `}}` (`FILE-EXACT ... = True`), which proves verbatim content, order, the header and that no other text is present. In the seam state (Phase 3 onward) the same exact equality holds for five files, with SortEmail.AttachmentSaving.cs using `S10P` (S10 with its last statement replaced by `RemoveReadOnlyPrompt.Reset();`); SortEmail.TrySaveAttachment.cs is proven by `TOKENS-TRYSAVE` and by S21 and S22 each occurring exactly once (the wrapper's documentation and body stay verbatim; the justification comment is inserted above S21 rather than rewriting it, which corrects the draft's "S21 excluded" wording: 33 segments stay verbatim in the seam state, and only S10, S13 and S23 change).
- PD-3 (kept) The three-argument overload becomes a non-async one-statement forward (async is not part of the signature; name, parameters, return type and accessibility unchanged). Exceptions thrown synchronously inside the async core are still captured into the returned task, so callers observe the same faulted task.
- PD-4 (revised this pass) Negative control: replace the single statement `clearReadOnly(directory);` in SortEmail.TrySaveAttachment.cs with the comment `// NEGATIVE-CONTROL-956` using the Edit tool (statement removed; the comment carries no census token), after a byte copy to coverage\control-956\SortEmail.TrySaveAttachment.fixed.bak. Predicted failed set {T2, T3, T4, T8, T9, T11}, passed set {T1, T5, T6, T7, T10} (Control Prediction table). Restore by the inverse Edit, proven by SHA-256 equality with `FIX-HASH-TRYSAVE`; only if the hashes differ, `CMD-RESTORE` copies the backup back and the hash is re-proven.
- PD-5 (kept) The session tests use no DataRow, so their TRX total is exactly 7; the combined `FullyQualifiedName~EmailIntelligence.SortEmail_` filter total is exactly 15 before Phase 3 and 26 after (15 existing plus 11 new).
- PD-6 (new) Phase 3 rewrites SortEmail.TrySaveAttachment.cs with one Write of Listing L-TRYSAVE rather than five Edit tasks. Reason, observed against the tree: after Phase 2 the file holds two identical `        [ExcludeFromCodeCoverage]` lines and three `        /// <summary>`/`        /// </summary>` pairs, so an Edit anchored on the attribute or the documentation boundary is not unique, and the field, forward, core, adapter and comment changes share one file. One whole-file write to a fixed listing is one verifiable outcome (proven by `TOKENS-TRYSAVE` and by S21/S22 verbatim). The skeleton's tasks P3-T3 to P3-T7 are therefore consolidated into P3-T3, and the later Phase 3 tasks are renumbered.
- PD-7 (coverage measurement rule for AC15; revised 2026-10-01, revision 1.2, under the coordinator ruling AC15 option (a)) The repository's closure filter keys exemption by bare member name per (declaring type, file) (scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 lines 171 to 184, "bare-name overload collision"). After this change the non-excluded three-argument forward and five-argument core share the name `TrySaveAttachmentAsync` with the excluded two-argument wrapper in the same file, so the wrapper's lambda `path => System.IO.Directory.CreateDirectory(path)` (merge-base line 902, unchanged, inside an `[ExcludeFromCodeCoverage]` member, and filtered at baseline because both merge-base overloads were excluded) is retained by the filter as an uncovered line; that line cannot be covered by a unit test without creating a real directory (UT4). The ITERATION 1 run of P4-T8 (FEATURE/evidence/qa-gates/coverage-comparison.md, 2026-10-01T21-26, findings F-A to F-E) observed two further uncovered lines, both in UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs and both inside the five-argument core that AC4 brought into the denominator: the closing brace that follows `throw;` in the final `else` branch of the `catch (System.UnauthorizedAccessException e)` block, and the closing brace that closes that catch block (the line after it is `catch (System.Exception)`). No execution path reaches either brace after the rethrow, and AC6 (TOKENS-TRYSAVE A15) requires the rethrows unchanged, so no test can cover them. `CMD-SORTEMAIL-COMPARE` therefore computes the exempt set as the union of three content-identified sets over the SortEmail.TrySaveAttachment.cs source lines, each printed separately: `EXEMPT-LAMBDA-LINES`/`EXEMPT-LAMBDA-COUNT` (lines containing `System.IO.Directory.CreateDirectory(path)`); `EXEMPT-ELSE-BRACE-LINES`/`EXEMPT-ELSE-BRACE-COUNT` (a line whose trimmed text is `}` and whose three preceding trimmed lines are, in order, `else`, `{`, `throw;`); `EXEMPT-CATCH-BRACE-LINES`/`EXEMPT-CATCH-BRACE-COUNT` (a line whose trimmed text is `}`, whose preceding line is an EXEMPT-ELSE-BRACE line, whose following trimmed line starts with `catch (System.Exception)`, and whose nearest preceding line at the same indentation width whose trimmed text starts with `catch (` is `catch (System.UnauthorizedAccessException e)`; the indentation key is required because the nearest preceding `catch (` line regardless of indentation is the inner boundary `catch (System.Exception inner)`); then `EXEMPT-LINES` (the union) and `EXEMPT-LINE-COUNT` (the union count, exactly 3: one line per set). Each exemption is identified by file and containing construct, never by line number alone, so a shifted line cannot silently take an exemption; the outer `catch (System.Exception) { throw; }` closing brace matches neither brace rule, because its third preceding trimmed line is `catch (System.Exception)` rather than `else` and its preceding line is not an EXEMPT-ELSE-BRACE line. The payload prints `SORTEMAIL-UNCOVERED-DELTA-RAW:` (final aggregate uncovered minus baseline) and `SORTEMAIL-UNCOVERED-DELTA:` (final aggregate uncovered minus the uncovered members of the exempt union minus baseline), followed by an in-memory negative control (`CONTROL-LINE:`, `CONTROL-DELTA:`, `CONTROL-VERDICT:`, `CONTROL-RAW-DELTA:`, `CONTROL-RAW-VERDICT:`) that treats the lowest-numbered covered, non-exempt line of the file as uncovered and recomputes both deltas; both verdicts must read `FAIL`, which proves the check still fails when any other changed line is uncovered. AC15's Level-1 clause is gated on `SORTEMAIL-UNCOVERED-DELTA:` at most 0 and on `SORTEMAIL-UNCOVERED-DELTA-RAW:` at most 3; any other excess fails. FEATURE/spec.md AC15 states the three exemptions and the two bounds (a planner spec amendment made under the ruling before P4-T8 is re-run; P0-T2's literals `ninety percent` and `System.IO.Directory.CreateDirectory(path)` remain on the line). The ruling permits no production or test code change. The coordinator ruling is recorded verbatim here for P4-T8 to quote:
  - "COORDINATOR RULING, AC15 OPTION (a) APPROVED (binding; quote verbatim in the dated notes where indicated):"
  - "AC15 carries exactly three named exemptions and no others: (1) the unchanged wrapper lambda from the SortEmail attachment item containing System.IO.Directory.CreateDirectory(path) (PD-7, already approved); (2) the closing brace that follows `throw;` in the final else branch of SortEmail.TrySaveAttachment.cs (line 154 at ef790798d); (3) the closing brace that follows `throw;` in the catch (System.UnauthorizedAccessException) block of SortEmail.TrySaveAttachment.cs (line 155 at ef790798d)."
  - "Thresholds: adjusted change at most 0, raw change at most 3."
  - "Rationale to record: those braces are unreachable after a rethrow, and AC6 (token A15) requires the rethrows to stay unchanged; overall coverage improved, lines 85.33% to 85.36% and branches 79.71% to 79.75%."
  - "Conditions: (a) identify each exemption by file AND content (the containing construct), not by line number alone, so that a shifted line cannot silently take an exemption; (b) a negative control must show that the AC15 check still fails if any other changed line is uncovered; (c) update AC15 in spec.md, P4-T8 and P4-T30 in the plan, each with a dated note citing this coordinator ruling, and record each edit in the plan revision log; (d) record the ruling in evidence/qa-gates/coverage-comparison.md; (e) NO production or test code changes."
  - "If applying the ruling would require any change beyond the three exemptions and the two thresholds, STOP and report."
- PD-8 (new) Package-level and repository-level rate comparisons are recorded as observations only (they move by a few lines between identical runs in untouched files); the gates are the Level-1 per-file aggregate of PD-7, the per-member 90 percent figures of P4-T9 and the first-party floors (line at least 80, branch at least 75).
- PD-9 (new) Test-design facts that remove executor judgment: the TrySave test file imports Microsoft.Office.Interop.Outlook, which declares types named `Exception` and `Action`, so the listing writes `System.Exception` and never a bare `Action` (SortEmail_Tests.cs uses `System.Action` at lines 45, 58 and 178 for the same reason); the five-argument call is made through one private helper `SaveAsync`; every prompt answer is scripted through a private nested `Seams` recorder whose queue throws `InvalidOperationException` when an unscripted prompt occurs; the private nested type may expose the internal `YesNoToAllPromptSession` because the test assembly holds internals access (UtilitiesCS/Properties/AssemblyInfo.cs line 19).
- PD-10 (new) Source and project files are created and edited only with the Write and Edit tools, so the pre-implementation gate observes every source write. `pwsh` payloads read source files; they write only under the git-ignored coverage/ directory (backups, logs, results). The single exception is the stop-path restore `CMD-RESTORE` of PD-4, which copies a byte-identical backup over the file it was taken from.

## Execution Conventions

- `FEATURE` denotes docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956. `WORKTREE` denotes the absolute item-worktree root supplied by the delegation prompt, without a trailing separator. Both tokens are expanded by the executor; `WORKTREE` is never written into any artifact. Every `git` command in this plan is written in its canonical form and is issued as `git -C WORKTREE <arguments>`; `Command:` fields record the canonical form. Uppercase placeholder tokens (`PATHS`, `TOKENS`, `TASKID`, `FILTERARG`, `STAGE`, `STATE`, `ONLY`, `EXCLUSION`, `INHERITED`, `MERGE-BASE`, `GATEARGS`) are substituted by the executor as each task states; no shell variable survives a task boundary, so a recorded value is substituted as text.
- **No commits by this plan (D5).** This plan runs no `git add`, `git commit`, `git checkout`, `git reset`, `git stash`, `git merge`, `git rebase` or `git update-index`. Every diff gate is a two-dot comparison against the recorded `MERGE-BASE:` paired with a `git status --porcelain` listing, so each gate is valid whether or not the orchestrator later commits.
- **Listings.** Every file this plan creates or rewrites wholesale is given verbatim in the Listings section as an indented block: each listing line is the file line prefixed by exactly four spaces. The executor strips exactly four leading spaces from every line, writes an empty file line for an empty listing line, and ends the file with one newline. No listing line is paraphrased, reordered or completed by judgment.
- **Evidence paths.** Every artifact is written under FEATURE/evidence/baseline/, FEATURE/evidence/regression-testing/, FEATURE/evidence/qa-gates/ or FEATURE/evidence/other/. Committed test evidence is limited to the JaCoCo package projection, the one-line first-party summary and TRX-derived summaries (CLAUDE.md "Committed Test Evidence Format"); no trx, Cobertura, collector or `.coverage` document is copied into FEATURE/.
- **Artifact filenames.** Fixed names: baseline/phase0-instructions-read.md, baseline/test-run-baseline.md, baseline/coverage-baseline.md, regression-testing/negative-control-clearreadonly-removed.md, regression-testing/test-run-final.md, regression-testing/test-results-summary.md, qa-gates/coverage-post-change.md, qa-gates/coverage-comparison.md, qa-gates/toolchain-pass.md. Every other artifact is `<task-id>-<name>.<TS>.md` (the fail-before dossier is regression-testing/fail-before-exception.<TS>.md), where `<TS>` is the write time in `yyyy-MM-ddTHH-mm` and equals the artifact's `Timestamp:` field; a later task locates it with the glob `<task-id>-<name>.*.md`, which must match exactly one file (the highest `ITERATION:` when the Phase 4 loop restarted).
- **Artifact fields.** Every command-step artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. `ExpectedExitCode:` is written only where a task says so, once per artifact, equal to the observed value it explains. An artifact that records several commands names the invocation its `EXIT_CODE:` row is scoped to and records the others as named `Output Summary:` lines.
- **Command channel.** Every payload is run as `pwsh -NoProfile -Command '<payload>'`: outer single quotes, the payload's lines joined by `; `, first statement `Set-Location -LiteralPath "WORKTREE"`. No payload contains a single-quote character (`[char]39` supplies one); every string literal is double-quoted; a double quote inside one is written `[char]34` or doubled; no double-quoted literal ends with a backslash before its closing quote. A .NET static file API is never given a relative path (payloads use cmdlets with `-LiteralPath`, which follow `Set-Location`). A child's exit code is read from `$LASTEXITCODE`.
- **Long-running payloads.** `CMD-COVERAGE-DIRECT` (P0-T11, P4-T7) may run longer than a ten-minute foreground call. The executor starts it as a background invocation of the same `pwsh -NoProfile -Command '<payload>'` form and polls its captured stdout with read-only calls until the final `TRX_PRESENT:` line appears. No sleep is added. A run still in progress 120 minutes after start is `COVERAGE RUN STALLED`: stop and report.
- **File hashes.** Every SHA-256 is the `Hash` property of `Get-FileHash -Algorithm SHA256 -LiteralPath <repository-relative path>`; the `Path` property is never recorded.
- **Tool resolution.** vswhere.exe is `Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"`; vstest.console.exe is `& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1`; MSBuild.exe is `& $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1`. `Command:` records the CLAUDE.md-canonical `msbuild ...` form with the note `resolved through vswhere`.
- **MSBuild switches.** Every msbuild invocation carries /nodeReuse:false and a normal-verbosity file logger `/flp:LogFile=coverage\logs\TASKID.msbuild.log;Verbosity=normal`; neither changes which targets run or which diagnostics are reported. `Command:` records the canonical command plus the notes `plus /nodeReuse:false` and `plus a normal-verbosity file logger`. Analyzer and nullable gates use `/t:Rebuild` and never add a Nullable property override.
- **Test runs.** Every direct run uses scripts/vscode/TaskMaster.cli.runsettings (Workers 0 at line 5, Scope ClassLevel at line 6) with `/InIsolation`, an explicit results directory under coverage\test-results\956\ and a quoted trx logger with an explicit file name. vstest.console.exe exits 0 on a zero-match filter, so every scoped run asserts its expected `total`. No test is retried, serialized or edited to pass; a failure is reported with its TRX message.
- **Token census.** Every occurrence count is produced by `CMD-CENSUS`, which removes ALL whitespace from each file and counts case-sensitive, non-overlapping occurrences of each token with `[regex]::Matches($content, [regex]::Escape($t)).Count`. Census tokens are written without whitespace; counts include comments and string literals; the comments in the Listings were written so that they repeat no census token. `Select-String` is never used for a count.
- **Line endings.** .gitattributes declares `* text=auto` and .editorconfig sets `end_of_line = crlf` (line 669). The Write tool may write LF; the scoped CSharpier pass of each phase normalizes endings, and every content gate is whitespace-insensitive. Numstat gates are unaffected because `text=auto` normalizes endings in `git diff`.
- **Inherited paths (rule, not list).** Clause A: every path already changed relative to `MERGE-BASE:` when P0-T3 captures it, before P0-T3 writes its own artifact, recorded as `INHERITED-CLAUSE-A:` (the orchestrator's preparation commits under FEATURE/, the promoted record docs/features/potential/promoted/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths.md if listed, and the P0-T1 and P0-T2 artifacts). Clause B: every path under .claude/agent-memory/ (tracked, written by agents during the run). Footprint gates subtract Clause A and Clause B, record the subtraction, and never subtract a Write Set path.
- **Artifact hygiene.** Before text is written into an artifact, an absolute path is replaced by `<repo-root>` (or `<user-profile>`), the account name by `<user>` and the machine name by `<host>`. The sandbox literals `C:\Sortemail956Sandbox` (new tests) and `C:\Sortemail945Sandbox` (existing tests) are not host paths and are recorded as written.
- **Sandbox literals.** No code in this plan creates either sandbox directory. Every `CMD-VSTEST` run prints `SANDBOX-956-EXISTS-BEFORE:`, `SANDBOX-956-EXISTS-AFTER:`, `SANDBOX-945-EXISTS-BEFORE:` and `SANDBOX-945-EXISTS-AFTER:` from read-only `Test-Path` calls; any `True` before a run is `SANDBOX PRESENT` and any `True` after a run is `SANDBOX CREATED BY RUN`: stop and report.
- **Stop discipline.** A named stop string in a task means: stop at that task, write the artifact with the observed values, and report the string verbatim. The executor never edits a test, a gate, a listing or a policy file to make a task pass, and never re-runs a test to obtain a different outcome.

## Verified Repository Facts (re-derived in this pass with Read, Grep and Glob against WORKTREE)

1. UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs (alias SRC) is 1,454 lines (last line 1454 is the namespace `}`). Line 1 `#nullable enable`; usings 2 to 19 (`System` through `using Outlook = Microsoft.Office.Interop.Outlook;`); line 20 blank; `namespace UtilitiesCS` 21; `{` 22; `public static class SortEmail` 23; `{` 24. Class-level regions: `#region Public Methods` 25 / `#endregion` 620, `#region Private Static Variables` 622 / 632, `#region Helper Methods` 634 / 1338. Method-internal regions 1136/1146 and 1152/1199 (inside `SaveAttachmentsOld`). Closing braces 1453 (class) and 1454 (namespace). The forty blank lines outside every segment are 26, 30, 41, 75, 111, 180, 209, 302, 454, 554, 562, 619, 621, 623, 629, 631, 633, 635, 660, 700, 761, 824, 838, 887, 905, 985, 1006, 1017, 1058, 1104, 1115, 1124, 1337, 1339, 1340, 1352, 1367, 1385, 1397 and 1430; the 36 segments of the Segment Table cover the remaining 1,382 lines except header lines 1 to 24 and the six region lines (24 + 6 + 2 + 40 + 1,382 = 1,454).
2. SRC occurrence facts (Grep, one occurrence per line): `ExcludeFromCodeCoverage` 28 (42, 76, 112, 181, 210, 303, 455, 564, 636, 661, 701, 762, 825, 839, 893, 912, 986, 1007, 1024, 1059, 1105, 1116, 1125, 1341, 1353, 1368, 1398, 1431); `YesNoToAll.ShowDialog(` 9 (710, 733, 773, 796, 853, 856 inside a comment, 936, 1263, 1280); `_removeReadOnly` 13 (560, 628, 930 comment, 932, 936, 940, 941, 956, 958, 964, 965, 969, 971); `TrySaveAttachmentAsync(` 7 (callers 819, 864, 879; wrapper 894 and its inner call 899; core 913; retry 961); `File.Delete(` 5 (245, 369, 515, 1214, 1218); `File.Exists(` 6 (704, 767, 1212, 1216, 1227, 1406); `File.` 11; `Directory.` 1 (902); `DirectoryInfo` 1 (944); `FileAttributes` 1 (947); `FileIO2.WriteTextFile(` 1 (1425); no `FileInfo`; `#region` 5 and `#endregion` 5. The wrapper is 888 to 904 (documentation 888 to 892, attribute 893, lambda 902); the core is 906 to 984 (documentation 906 to 911, attribute 912, body 919 to 983, message 934 to 935, `new DirectoryInfo` 944 outside the inner `try` 945, retry 961, outer `catch (System.Exception) { throw; }` 980 to 983). `Cleanup_Files` 555 to 561 resets `_removeReadOnly` at 560. L1 cases at 996 and 999; L4 `File.Exists(Path.Combine(strFileName, strFileLocation))` at 1406.
3. UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs (alias TST) is 457 lines with 15 `[TestMethod]` attributes; class `SortEmail_Tests` at 33 in namespace `UtilitiesCS.Test.EmailIntelligence` (15); the two #945 try-save tests are at 246 and 273 and call the three-argument overload at 257 and 281; the sandbox constant `C:\Sortemail945Sandbox\attachments` is at 238; the file carries no `#nullable` directive and writes `System.Action` at 45, 58 and 178. No other class in the worktree's `*.cs` files is named `SortEmail_*`, and no `YesNoToAllPromptSession` or `SortEmail_TrySaveAttachment` identifier exists anywhere.
4. UtilitiesCS/Dialogs/YesNoToAll.cs: `#nullable enable` 10; namespace `UtilitiesCS` 12; `public enum YesNoToAllResponse { Empty = 0, Yes = 1, No = 2, YesToAll = 4, NoToAll = 8 }` 14 to 21; a single `public static YesNoToAllResponse ShowDialog(string message)` at 65, so the method group converts to `Func<string, YesNoToAllResponse>`.
5. UtilitiesCS/Properties/AssemblyInfo.cs lines 18 and 19 grant `InternalsVisibleTo` to `DynamicProxyGenAssembly2` and `UtilitiesCS.Test` (line 20 `ToDoModel.Test`).
6. UtilitiesCS/UtilitiesCS.csproj: legacy non-SDK (line 2 `ToolsVersion="15.0"`), `LangVersion` 12.0 (10), `TargetFrameworkVersion` v4.8.1 (16). Compile entries: 573 `Dialogs\NotImplementedDialog.cs`, 574 `Dialogs\YesNoToAll.cs`, 575 `EmailIntelligence\Bayesian\Obsolete\BayesianClassifier.cs`; 816 `EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs`, 817 `EmailIntelligence\EmailParsingSorting\SortEmail.cs`, 818 `OutlookObjects\Folder\FolderPredictor.cs`. Form: four-space indent, backslash separators, self-closing.
7. UtilitiesCS.Test/UtilitiesCS.Test.csproj: legacy non-SDK (line 2), v4.8.1 (17), `LangVersion` Latest (18). Compile entries 97 `EmailIntelligence\Triage_OlLogic_Tests.cs`, 98 `EmailIntelligence\SortEmail_Tests.cs`, 99 `EmailIntelligence\FilterOlFoldersController_Tests.cs`; 441 `Dialogs\YesNoToAll_Test.cs`, 442 `Dialogs\YesNoToAll_Tests.cs`, 443 `ReusableTypeClasses\AsyncLazy_Tests.cs`. UtilitiesCS.Test/packages.config: FluentAssertions 8.11.0 (9), Moq 4.21.0 (65), MSTest.TestFramework 4.4.1 (68). Dialog tests use namespace `UtilitiesCS.Test.Dialogs` (MyBox_Tests.cs 10, NotImplementedDialog_Tests.cs 7). Moq void `SetupSequence(...).Throws(...).Pass()` precedent: UtilitiesCS.Test/OutlookObjects/Item/OutlookItemFlaggableTests.cs 203 to 206.
8. Callers (Grep over `*.cs`): `UtilitiesCS.SortEmail.UndoAsync` TaskMaster/Ribbon/RibbonController.cs 230; `SortEmail.WriteCSV_StartNewFileIfDoesNotExist` TaskMaster/AppGlobals/AppOlObjects.cs 301; `SortEmail.Cleanup_Files` QuickFiler/Controllers/EfcDataModel.cs 309 (compiled: QuickFiler.csproj 289) and QuickFiler/Legacy/QfcController.cs 792 (not in QuickFiler.csproj); extension `attachment.SaveAttachmentAsync(Config.SaveFsPath!)` EmailFiler.cs 445. ToDoModel/Email Utilities/SortItemsToExistingFolder.cs 391 declares an unrelated `Cleanup_Files`. `TrySaveAttachmentAsync` and `_removeReadOnly` occur outside SRC only in TST (246, 257, 273, 281). No caller changes.
9. scripts/vscode/TaskMaster.cli.runsettings is 9 lines (Workers 0 at 5, Scope ClassLevel at 6, no collector). scripts/vscode/Invoke-MSTestWithCoverage.ps1: filter 91, `ConvertTo-DerivedCoverageSettingsXml` 97, defaults 297 to 298, entry guard 459. Helpers: Invoke-MSTestWithCoverage.Helpers.ps1 `Get-CoberturaClassLineSummary` 160, `Merge-CoberturaClassesByFilename` 260, `ConvertTo-KoverageCoberturaXml` 407 (closure filter then merge at 441 to 442); Invoke-MSTestWithCoverage.ClosureFilter.ps1 `Get-CoberturaInstrumentedMemberName` 134 (bare-name collision note 171 to 184), `Remove-CoberturaExemptClosureCoverage` 235; Threshold.ps1 3 and 58; FirstParty.ps1 123; Projection.ps1 14 and 83; Invoke-MSTest.TrxSummary.ps1 `Get-TrxRunSummary` 12 (parameter `TrxContent` 44) and `Format-TrxRunSummary` 103 (parameter `Summary` 128).
10. Configuration: global.json SDK 8.0.205 with `.dotnet-sdk` path (lines 3 and 7); dotnet-tools.json pins csharpier 1.2.6; scripts/vscode/Install-RepoDotNetSdk.ps1 and scripts/vscode/Invoke-Restore.ps1 exist; coverage.config exists at the root; .csharpierignore excludes `**/evidence/**`, cobertura, coverage and trx documents (4 to 8) and project files (12 to 14); .editorconfig sets `dotnet_analyzer_diagnostic.severity = suggestion` (27) with `MSTEST0032` at warning (29); .gitignore lines 146 (trx), 147 (cobertura), 150 to 151 (coverage/), 356 (`.dotnet*/`).
11. The four classes that stall a local vstest run exist: UtilitiesCS.Test/HelperClasses/ShellUtilities_Tests.cs 10, ShellUtilitiesStatic_Tests.cs 10, SysImageListHelperTests.cs 12, UtilitiesCS.Test/EmailIntelligence/OSBrowser_Tests.cs 27. `TryAddValuesAsync_UpdatesExistingValue` is the known intermittent test of issue #780 and is never retried.
12. artifacts/orchestration/orchestrator-state.json (git-ignored): `route_id` preparation (3), `issue-num` 956 (10), `work-mode` full-bug (11), `feature-folder` (12), `plan-path` (13), `base_sha` 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f (15), `lifecycle_ready` true (16).
13. Prediction anchor (not a gate): the #945 final comparison recorded `SORTEMAIL-FILE baseline valid=25 covered=24 uncovered=1` for SortEmail.cs (docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/evidence/qa-gates/coverage-final.md lines 91 to 92), the state this branch starts from.

## Segment Table (merge-base SRC line ranges, re-derived in this pass)

Each segment is a whole member including its attribute line and any leading comment or XML documentation. `Lines` is the inclusive range; `N` is its line count. Destination file names are under UtilitiesCS/EmailIntelligence/EmailParsingSorting/.

| ID | Lines | N | Member | Destination |
| --- | --- | --- | --- | --- |
| S01 | 27-29 | 3 | `logger` field | SortEmail.cs |
| S02 | 31-40 | 10 | `InitializeSortToExisting` | SortEmail.cs |
| S03 | 42-74 | 33 | `SortAsync` (Explorer selection) | SortEmail.MailItemSort.cs |
| S04 | 76-110 | 35 | `SortAsync` (IList of MailItem, 7 parameters) | SortEmail.MailItemSort.cs |
| S05 | 112-179 | 68 | `SortAsync` (IList of MailItemHelper) | SortEmail.cs |
| S06 | 181-208 | 28 | `UpdatePredictiveEngineAsync` | SortEmail.cs |
| S07 | 210-301 | 92 | `ProcessMailItemAsync` | SortEmail.cs |
| S08 | 303-453 | 151 | `SortAsync` (IList of MailItem, 9 parameters) | SortEmail.MailItemSort.cs |
| S09 | 455-553 | 99 | `Sort` | SortEmail.MailItemSort.cs |
| S10 | 555-561 | 7 | `Cleanup_Files` | SortEmail.AttachmentSaving.cs |
| S11 | 563-618 | 56 | `UndoAsync` (with leading comment 563) | SortEmail.UndoAndMoveLog.cs |
| S12 | 624-627 | 4 | `_responseSaveFile`, `_attachmentsOverwrite`, `_attachmentsAltName`, `_picturesOverwrite` | SortEmail.AttachmentSaving.cs |
| S13 | 628 | 1 | `_removeReadOnly` field | SortEmail.TrySaveAttachment.cs (Phase 2); removed in Phase 3 |
| S14 | 630 | 1 | `MAX_PATH` constant | SortEmail.LegacyAttachmentSaving.cs |
| S15 | 636-659 | 24 | `GetAttachmentsInfo` | SortEmail.AttachmentSaving.cs |
| S16 | 661-699 | 39 | `GetAttachmentsInfoAsync` | SortEmail.AttachmentSaving.cs |
| S17 | 701-760 | 60 | `SaveAttachment` | SortEmail.AttachmentSaving.cs |
| S18 | 762-823 | 62 | `SaveAttachmentAsync(this AttachmentHelper)` | SortEmail.AttachmentSaving.cs |
| S19 | 825-837 | 13 | `SaveAttachmentAsync(this AttachmentHelper, string)` | SortEmail.AttachmentSaving.cs |
| S20 | 839-886 | 48 | `SaveCaseAsync` | SortEmail.AttachmentSaving.cs |
| S21 | 888-892 | 5 | two-argument wrapper documentation | SortEmail.TrySaveAttachment.cs |
| S22 | 893-904 | 12 | two-argument wrapper (attribute and body) | SortEmail.TrySaveAttachment.cs |
| S23 | 906-984 | 79 | three-argument overload (documentation, attribute, body) | SortEmail.TrySaveAttachment.cs (Phase 2); replaced in Phase 3 |
| S24 | 986-1005 | 20 | `SaveCase` | SortEmail.AttachmentSaving.cs |
| S25 | 1007-1016 | 10 | `IsPicture` | SortEmail.AttachmentSaving.cs |
| S26 | 1018-1057 | 40 | `ResolvePaths(IList of MailItem, ...)` (with commented legacy signature 1018-1023) | SortEmail.MailItemSort.cs |
| S27 | 1059-1103 | 45 | `ResolvePaths(Folder, ...)` | SortEmail.cs |
| S28 | 1105-1114 | 10 | `SaveMessageAsMsgAsync` | SortEmail.AttachmentSaving.cs |
| S29 | 1116-1123 | 8 | `SaveMessageAsMSG` | SortEmail.AttachmentSaving.cs |
| S30 | 1125-1336 | 212 | `SaveAttachmentsOld` | SortEmail.LegacyAttachmentSaving.cs |
| S31 | 1341-1351 | 11 | `PushToUndoStack` | SortEmail.UndoAndMoveLog.cs |
| S32 | 1353-1366 | 14 | `CaptureMoveDetails` | SortEmail.UndoAndMoveLog.cs |
| S33 | 1368-1384 | 17 | `SanitizeArrayLineTSV` | SortEmail.UndoAndMoveLog.cs |
| S34 | 1386-1396 | 11 | `StripTabsCrLf` | SortEmail.UndoAndMoveLog.cs |
| S35 | 1398-1429 | 32 | `WriteCSV_StartNewFileIfDoesNotExist` | SortEmail.UndoAndMoveLog.cs |
| S36 | 1431-1452 | 22 | `SanitizeArray` | SortEmail.UndoAndMoveLog.cs |

Sum of N: 1,382. The `SEGMENTS` literal used by the payloads is: `"S01:27:29", "S02:31:40", "S03:42:74", "S04:76:110", "S05:112:179", "S06:181:208", "S07:210:301", "S08:303:453", "S09:455:553", "S10:555:561", "S11:563:618", "S12:624:627", "S13:628:628", "S14:630:630", "S15:636:659", "S16:661:699", "S17:701:760", "S18:762:823", "S19:825:837", "S20:839:886", "S21:888:892", "S22:893:904", "S23:906:984", "S24:986:1005", "S25:1007:1016", "S26:1018:1057", "S27:1059:1103", "S28:1105:1114", "S29:1116:1123", "S30:1125:1336", "S31:1341:1351", "S32:1353:1366", "S33:1368:1384", "S34:1386:1396", "S35:1398:1429", "S36:1431:1452"`.

## File Assembly Rule (Phase 2) and File Map

Each Phase 2 file is assembled from the merge-base SRC (still unmodified when P2-T1 to P2-T5 run) exactly as follows, with no other text: (1) SRC lines 1 to 22 verbatim (`#nullable enable`, the eighteen using lines, the blank line 20, `namespace UtilitiesCS`, `{`); (2) the line `    public static partial class SortEmail`; (3) the line `    {`; (4) the file's segments in the order of the table below, each copied verbatim (every line, including leading comments, attributes, `#pragma` lines at column zero and method-internal `#region` lines), with exactly one empty line between consecutive segments except that S22 follows S21 with no empty line between them; (5) the line `    }`; (6) the line `}`, followed by one newline. The three class-level region pairs (SRC 25/620, 622/632, 634/1338) and every blank line outside a segment are not copied.

| File | Segment order | Predicted lines (assembled; after P2-T8 formatting the content is identical) |
| --- | --- | --- |
| SortEmail.cs (rewritten) | S01, S02, S05, S06, S07, S27 | 24 + 246 + 5 + 2 = 277 |
| SortEmail.MailItemSort.cs | S03, S04, S08, S09, S26 | 24 + 358 + 4 + 2 = 388 |
| SortEmail.AttachmentSaving.cs | S12, S10, S15, S16, S17, S18, S19, S20, S24, S25, S28, S29 | 24 + 305 + 11 + 2 = 342 |
| SortEmail.TrySaveAttachment.cs | S13, S21, S22, S23 | 24 + 97 + 2 + 2 = 125 (Phase 3 rewrites it to Listing L-TRYSAVE, 172 lines) |
| SortEmail.LegacyAttachmentSaving.cs | S14, S30 | 24 + 213 + 1 + 2 = 240 |
| SortEmail.UndoAndMoveLog.cs | S11, S31, S32, S33, S34, S35, S36 | 24 + 163 + 6 + 2 = 195 |

Every file is under 500 lines with at least 111 lines of headroom. A CSharpier pass over verbatim members at unchanged nesting depth does not change their non-whitespace text; every content gate is whitespace-insensitive in any case.

## Listings

### Listing L-SESSION (UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs, 70 lines, created by P3-T1)

    #nullable enable
    using System;

    namespace UtilitiesCS
    {
        /// <summary>
        /// Holds the answer to one Yes/No/YesToAll/NoToAll prompt across calls, so that a "ToAll"
        /// answer is reused without asking again while a single answer is used once. The prompt is
        /// supplied as a delegate, which lets a caller replace the modal dialog (for example, a unit
        /// test that must not show a form). Instances are not synchronized; one session serves one
        /// caller at a time.
        /// </summary>
        internal sealed class YesNoToAllPromptSession
        {
            private readonly Func<string, YesNoToAllResponse> _showDialog;

            /// <summary>
            /// Creates a session whose prompts are answered by <paramref name="showDialog"/>.
            /// </summary>
            /// <param name="showDialog">Shows the prompt message and returns the answer.</param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="showDialog"/> is null.
            /// </exception>
            internal YesNoToAllPromptSession(Func<string, YesNoToAllResponse> showDialog)
            {
                _showDialog = showDialog ?? throw new ArgumentNullException(nameof(showDialog));
            }

            /// <summary>
            /// Gets the answer the session holds. <see cref="YesNoToAllResponse.Empty"/> means that
            /// the next call to <see cref="Ask"/> shows the prompt.
            /// </summary>
            internal YesNoToAllResponse Response { get; private set; }

            /// <summary>
            /// Returns the held answer, showing the prompt first when the session holds no answer.
            /// </summary>
            /// <param name="message">The prompt text, shown only when no answer is held.</param>
            /// <returns>The held answer, or the answer the prompt returned.</returns>
            internal YesNoToAllResponse Ask(string message)
            {
                if (Response == YesNoToAllResponse.Empty)
                {
                    Response = _showDialog(message);
                }

                return Response;
            }

            /// <summary>
            /// Releases a single-use answer (<see cref="YesNoToAllResponse.Yes"/> or
            /// <see cref="YesNoToAllResponse.No"/>) and keeps a "ToAll" answer.
            /// </summary>
            internal void ReleaseSingleAnswer()
            {
                if (Response == YesNoToAllResponse.Yes || Response == YesNoToAllResponse.No)
                {
                    Response = YesNoToAllResponse.Empty;
                }
            }

            /// <summary>
            /// Clears any held answer, including a "ToAll" answer.
            /// </summary>
            internal void Reset()
            {
                Response = YesNoToAllResponse.Empty;
            }
        }
    }

### Listing L-TRYSAVE (UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs, 172 lines, written by P3-T3)

Lines 1 to 22 are SRC lines 1 to 22 verbatim; lines 35 to 39 are S21 verbatim (SRC 888 to 892) and lines 40 to 51 are S22 verbatim (SRC 893 to 904). They are written out in full below so that the listing is the complete file.

    #nullable enable
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Deedle;
    using Microsoft.Office.Interop.Outlook;
    using SDILReader;
    using UtilitiesCS;
    using UtilitiesCS.EmailIntelligence;
    using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
    using UtilitiesCS.OutlookExtensions;
    using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
    using Outlook = Microsoft.Office.Interop.Outlook;

    namespace UtilitiesCS
    {
        public static partial class SortEmail
        {
            // Production answer state of the read-only-removal prompt. Tests pass their own session to
            // the five-argument overload, so no test reads or writes this instance; nothing can
            // replace it. Cleanup_Files resets it after each filing operation.
            private static readonly YesNoToAllPromptSession RemoveReadOnlyPrompt = new(
                YesNoToAll.ShowDialog
            );

            // Excluded from coverage: the only behavior of this wrapper is wiring the real
            // directory-creation default, and calling it from a test would create a real directory,
            // which the unit-test policy prohibits (UT4).
            /// <summary>
            /// Saves the attachment to <paramref name="filePathSave"/> and creates the destination
            /// directory on disk. The overload that takes a directory-creation delegate is the test
            /// seam.
            /// </summary>
            [ExcludeFromCodeCoverage]
            internal static Task<bool> TrySaveAttachmentAsync(
                this Attachment attachment,
                string filePathSave
            )
            {
                return TrySaveAttachmentAsync(
                    attachment,
                    filePathSave,
                    path => System.IO.Directory.CreateDirectory(path)
                );
            }

            /// <summary>
            /// Saves the attachment to <paramref name="filePathSave"/>. The
            /// <paramref name="createDirectory"/> delegate receives the destination directory before
            /// the attachment is saved, so a caller can replace directory creation (for example, a
            /// unit test that must not touch the file system). The read-only prompt uses the
            /// production session and the read-only attribute is cleared on disk.
            /// </summary>
            internal static Task<bool> TrySaveAttachmentAsync(
                this Attachment attachment,
                string filePathSave,
                Action<string> createDirectory
            )
            {
                return TrySaveAttachmentAsync(
                    attachment,
                    filePathSave,
                    createDirectory,
                    ClearReadOnlyAttributeOnDisk,
                    RemoveReadOnlyPrompt
                );
            }

            /// <summary>
            /// Saves the attachment to <paramref name="filePathSave"/> through injected seams.
            /// <paramref name="createDirectory"/> receives the destination directory before each save
            /// attempt. When the save is denied, <paramref name="removeReadOnlyPrompt"/> supplies the
            /// answer to the read-only prompt, asking only while it holds no answer, and
            /// <paramref name="clearReadOnly"/> clears the read-only attribute of the destination
            /// directory before the save is retried.
            /// </summary>
            /// <returns>
            /// True when the attachment was saved; false when the answer declined the change or the
            /// attribute could not be cleared. A cancelled prompt rethrows the original exception.
            /// </returns>
            internal static async Task<bool> TrySaveAttachmentAsync(
                this Attachment attachment,
                string filePathSave,
                Action<string> createDirectory,
                Action<string> clearReadOnly,
                YesNoToAllPromptSession removeReadOnlyPrompt
            )
            {
                try
                {
                    createDirectory(Path.GetDirectoryName(filePathSave));
                    await Task.Run(() => attachment.SaveAsFile(filePathSave));
                    return true;
                }
                catch (System.UnauthorizedAccessException e)
                {
                    Debug.WriteLine(e.Message);

                    // Exception usually is thrown when readonly folder attribute is set.
                    // When the session holds no answer yet, ask whether the user wants to remove the
                    // readonly attribute and retry saving.
                    if (removeReadOnlyPrompt.Response == YesNoToAllResponse.Empty)
                    {
                        var message =
                            $"The folder {Path.GetDirectoryName(filePathSave)} is read-only. Do you want to remove the readonly attribute?";
                        removeReadOnlyPrompt.Ask(message);
                    }

                    if (
                        (removeReadOnlyPrompt.Response == YesNoToAllResponse.Yes)
                        || (removeReadOnlyPrompt.Response == YesNoToAllResponse.YesToAll)
                    )
                    {
                        var directory = Path.GetDirectoryName(filePathSave);
                        try
                        {
                            clearReadOnly(directory);
                        }
                        catch (System.Exception inner)
                        {
                            Debug.WriteLine(inner.Message);
                            return false;
                        }
                        finally
                        {
                            removeReadOnlyPrompt.ReleaseSingleAnswer();
                        }
                        return await TrySaveAttachmentAsync(
                            attachment,
                            filePathSave,
                            createDirectory,
                            clearReadOnly,
                            removeReadOnlyPrompt
                        );
                    }
                    else if (
                        (removeReadOnlyPrompt.Response == YesNoToAllResponse.No)
                        || (removeReadOnlyPrompt.Response == YesNoToAllResponse.NoToAll)
                    )
                    {
                        Debug.WriteLine($"The file {filePathSave} was not saved.");
                        removeReadOnlyPrompt.ReleaseSingleAnswer();
                        return false;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (System.Exception)
                {
                    throw;
                }
            }

            // Excluded from coverage: a file-system adapter whose execution requires the real file
            // system, which unit tests must not touch (UT4). Tests pass their own delegate to the
            // five-argument overload instead.
            [ExcludeFromCodeCoverage]
            private static void ClearReadOnlyAttributeOnDisk(string directoryPath)
            {
                var di = new DirectoryInfo(directoryPath);
                di.Attributes &= ~System.IO.FileAttributes.ReadOnly;
            }
        }
    }

Behavior equivalence with SRC 919 to 983 (AC6): the `try` prefix, the `Debug.WriteLine` calls, the message text, the `catch (System.Exception inner)` arm, the `throw;` of the Cancel arm and the outer `catch (System.Exception) { throw; }` are unchanged; the three reads of the static field become `removeReadOnlyPrompt.Response`; the conditional `YesNoToAll.ShowDialog(message)` assignment becomes `removeReadOnlyPrompt.Ask(message);` inside the same `Empty` guard; both reset sites become `removeReadOnlyPrompt.ReleaseSingleAnswer();` (the `finally` arm can only hold Yes or YesToAll and the No arm only No or NoToAll, so releasing only Yes and No reproduces SRC 956 to 959 and 969 to 972); `Path.GetDirectoryName` is computed into `directory` before the inner `try`, as SRC 944 computes it outside the inner `try`; `clearReadOnly(directory)` runs inside the inner `try`; the retry passes all three seams. No `catch` is added and no retry bound is added (L2 is preserved).

### Listing L-TEST-SESSION (UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs, created by P1-T1)

    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace UtilitiesCS.Test.Dialogs
    {
        /// <summary>
        /// Unit tests for <see cref="YesNoToAllPromptSession"/>: delegate validation, prompting only
        /// while no answer is held, releasing single-use answers, keeping "ToAll" answers and
        /// resetting. Every prompt is a test delegate; no dialog is shown and no state is shared
        /// between tests.
        /// </summary>
        [TestClass]
        public class YesNoToAllPromptSession_Tests
        {
            /// <summary>
            /// S1. Scenario: the constructor receives a null prompt delegate. Expected:
            /// ArgumentNullException naming the showDialog parameter.
            /// </summary>
            [TestMethod]
            public void Constructor_WhenShowDialogIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                Func<string, YesNoToAllResponse> showDialog = null;

                // Act
                Action act = () => _ = new YesNoToAllPromptSession(showDialog);

                // Assert
                act.Should().Throw<ArgumentNullException>().WithParameterName("showDialog");
            }

            /// <summary>
            /// S2. Scenario: Ask is called on a new session, which holds no answer. Expected: the
            /// prompt receives the message once, its answer is returned and the session holds it.
            /// </summary>
            [TestMethod]
            public void Ask_WhenNoAnswerIsHeld_InvokesPromptAndStoresAnswer()
            {
                // Arrange
                var messages = new List<string>();
                var session = new YesNoToAllPromptSession(message =>
                {
                    messages.Add(message);
                    return YesNoToAllResponse.Yes;
                });
                var initial = session.Response;

                // Act
                var answer = session.Ask("first");

                // Assert
                initial.Should().Be(YesNoToAllResponse.Empty);
                answer.Should().Be(YesNoToAllResponse.Yes);
                session.Response.Should().Be(YesNoToAllResponse.Yes);
                messages.Should().Equal("first");
            }

            /// <summary>
            /// S3. Scenario: Ask is called while the session holds an answer. Expected: the held
            /// answer is returned and the prompt is not invoked again.
            /// </summary>
            [TestMethod]
            public void Ask_WhenAnswerIsHeld_ReturnsItWithoutInvokingPrompt()
            {
                // Arrange
                var calls = 0;
                var session = new YesNoToAllPromptSession(_ =>
                {
                    calls++;
                    return YesNoToAllResponse.NoToAll;
                });
                _ = session.Ask("first");

                // Act
                var answer = session.Ask("second");

                // Assert
                answer.Should().Be(YesNoToAllResponse.NoToAll);
                calls.Should().Be(1);
            }

            /// <summary>
            /// S4. Scenario: ReleaseSingleAnswer is called on sessions holding Yes and No. Expected:
            /// both sessions hold no answer afterwards.
            /// </summary>
            [TestMethod]
            public void ReleaseSingleAnswer_WhenAnswerIsYesOrNo_ClearsIt()
            {
                // Arrange
                var yesSession = new YesNoToAllPromptSession(_ => YesNoToAllResponse.Yes);
                var noSession = new YesNoToAllPromptSession(_ => YesNoToAllResponse.No);
                _ = yesSession.Ask("question");
                _ = noSession.Ask("question");

                // Act
                yesSession.ReleaseSingleAnswer();
                noSession.ReleaseSingleAnswer();

                // Assert
                yesSession.Response.Should().Be(YesNoToAllResponse.Empty);
                noSession.Response.Should().Be(YesNoToAllResponse.Empty);
            }

            /// <summary>
            /// S5. Scenario: ReleaseSingleAnswer is called on sessions holding YesToAll and NoToAll.
            /// Expected: both sessions keep their answer.
            /// </summary>
            [TestMethod]
            public void ReleaseSingleAnswer_WhenAnswerIsYesToAllOrNoToAll_KeepsIt()
            {
                // Arrange
                var yesSession = new YesNoToAllPromptSession(_ => YesNoToAllResponse.YesToAll);
                var noSession = new YesNoToAllPromptSession(_ => YesNoToAllResponse.NoToAll);
                _ = yesSession.Ask("question");
                _ = noSession.Ask("question");

                // Act
                yesSession.ReleaseSingleAnswer();
                noSession.ReleaseSingleAnswer();

                // Assert
                yesSession.Response.Should().Be(YesNoToAllResponse.YesToAll);
                noSession.Response.Should().Be(YesNoToAllResponse.NoToAll);
            }

            /// <summary>
            /// S6. Scenario: Reset is called while the session holds a "ToAll" answer. Expected: the
            /// session holds no answer and the next Ask invokes the prompt again.
            /// </summary>
            [TestMethod]
            public void Reset_WhenToAllAnswerIsHeld_ClearsItSoThePromptIsShownAgain()
            {
                // Arrange
                var calls = 0;
                var session = new YesNoToAllPromptSession(_ =>
                {
                    calls++;
                    return YesNoToAllResponse.YesToAll;
                });
                _ = session.Ask("first");

                // Act
                session.Reset();
                var afterReset = session.Response;
                _ = session.Ask("second");

                // Assert
                afterReset.Should().Be(YesNoToAllResponse.Empty);
                calls.Should().Be(2);
            }

            /// <summary>
            /// S7. Scenario: the prompt returns Empty (the Cancel button). Expected: the session
            /// holds no answer and the next Ask invokes the prompt again.
            /// </summary>
            [TestMethod]
            public void Ask_WhenPromptReturnsEmpty_HoldsNoAnswerAndAsksAgain()
            {
                // Arrange
                var messages = new List<string>();
                var session = new YesNoToAllPromptSession(message =>
                {
                    messages.Add(message);
                    return YesNoToAllResponse.Empty;
                });

                // Act
                var first = session.Ask("first");
                var second = session.Ask("second");

                // Assert
                first.Should().Be(YesNoToAllResponse.Empty);
                second.Should().Be(YesNoToAllResponse.Empty);
                session.Response.Should().Be(YesNoToAllResponse.Empty);
                messages.Should().Equal("first", "second");
            }
        }
    }

### Listing L-TEST-TRYSAVE (UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs, created by P1-T2)

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Office.Interop.Outlook;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    namespace UtilitiesCS.Test.EmailIntelligence
    {
        /// <summary>
        /// Unit tests for the read-only-folder handling of the five-argument attachment save overload
        /// of <see cref="SortEmail"/>. Every test passes its own recording seams: a directory-creation
        /// delegate, a read-only-clear delegate and a fresh <see cref="YesNoToAllPromptSession"/>
        /// whose prompt returns scripted answers. The paths are rooted in-memory literals; no test
        /// touches the file system, shows a dialog or reads a static member of the class under test.
        /// </summary>
        [TestClass]
        public class SortEmail_TrySaveAttachment_Tests
        {
            // Rooted literal paths used only as in-memory values. The injected delegates record them
            // instead of acting on them, so nothing is created or changed on disk.
            private const string SandboxDirectory = @"C:\Sortemail956Sandbox\attachments";
            private const string SandboxFilePath = @"C:\Sortemail956Sandbox\attachments\saved.txt";
            private const string ExpectedPrompt =
                @"The folder C:\Sortemail956Sandbox\attachments is read-only. Do you want to remove the readonly attribute?";

            /// <summary>
            /// T1. Scenario: the first save succeeds. Expected: true; no prompt and no read-only clear;
            /// the directory is created once; the session still holds no answer.
            /// </summary>
            [TestMethod]
            public async Task TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly()
            {
                // Arrange
                var seams = new Seams();
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment.SetupSequence(x => x.SaveAsFile(SandboxFilePath)).Pass();

                // Act
                bool saved = await SaveAsync(attachment, seams);

                // Assert
                saved.Should().BeTrue();
                seams.PromptMessages.Should().BeEmpty();
                seams.ClearedDirectories.Should().BeEmpty();
                seams.CreatedDirectories.Should().Equal(SandboxDirectory);
                seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
                attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Once);
            }

            /// <summary>
            /// T2. Scenario: the first save is denied and the answer is Yes. Expected: true; one prompt
            /// naming the folder; the directory cleared once; two saves; the Yes answer released.
            /// </summary>
            [TestMethod]
            public async Task TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer()
            {
                // Arrange
                var seams = new Seams(YesNoToAllResponse.Yes);
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment
                    .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                    .Throws(new UnauthorizedAccessException("denied"))
                    .Pass();

                // Act
                bool saved = await SaveAsync(attachment, seams);

                // Assert
                saved.Should().BeTrue();
                seams.PromptMessages.Should().Equal(ExpectedPrompt);
                seams.ClearedDirectories.Should().Equal(SandboxDirectory);
                seams.CreatedDirectories.Should().Equal(SandboxDirectory, SandboxDirectory);
                seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
                attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(2));
            }

            /// <summary>
            /// T3. Scenario: the first save is denied and the answer is YesToAll. Expected: true; the
            /// directory cleared once; two saves; the YesToAll answer stays held.
            /// </summary>
            [TestMethod]
            public async Task TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer()
            {
                // Arrange
                var seams = new Seams(YesNoToAllResponse.YesToAll);
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment
                    .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                    .Throws(new UnauthorizedAccessException("denied"))
                    .Pass();

                // Act
                bool saved = await SaveAsync(attachment, seams);

                // Assert
                saved.Should().BeTrue();
                seams.PromptMessages.Should().Equal(ExpectedPrompt);
                seams.ClearedDirectories.Should().Equal(SandboxDirectory);
                seams.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);
                attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(2));
            }

            /// <summary>
            /// T4. Scenario: two denied saves on one session after a YesToAll answer. Expected: both
            /// calls return true and the prompt is shown once; the second call reuses the answer.
            /// </summary>
            [TestMethod]
            public async Task TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt()
            {
                // Arrange
                var seams = new Seams(YesNoToAllResponse.YesToAll);
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment
                    .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                    .Throws(new UnauthorizedAccessException("denied"))
                    .Pass()
                    .Throws(new UnauthorizedAccessException("denied"))
                    .Pass();

                // Act
                bool first = await SaveAsync(attachment, seams);
                bool second = await SaveAsync(attachment, seams);

                // Assert
                first.Should().BeTrue();
                second.Should().BeTrue();
                seams.PromptMessages.Should().Equal(ExpectedPrompt);
                seams.ClearedDirectories.Should().Equal(SandboxDirectory, SandboxDirectory);
                seams.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);
                attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(4));
            }

            /// <summary>
            /// T5. Scenario: the first save is denied and the answer is No. Expected: false; no clear;
            /// one save; the No answer released.
            /// </summary>
            [TestMethod]
            public async Task TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer()
            {
                // Arrange
                var seams = new Seams(YesNoToAllResponse.No);
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment
                    .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                    .Throws(new UnauthorizedAccessException("denied"));

                // Act
                bool saved = await SaveAsync(attachment, seams);

                // Assert
                saved.Should().BeFalse();
                seams.PromptMessages.Should().Equal(ExpectedPrompt);
                seams.ClearedDirectories.Should().BeEmpty();
                seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
                attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Once);
            }

            /// <summary>
            /// T6. Scenario: two denied saves on one session after a NoToAll answer. Expected: both
            /// calls return false, the prompt is shown once and the NoToAll answer stays held.
            /// </summary>
            [TestMethod]
            public async Task TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt()
            {
                // Arrange
                var seams = new Seams(YesNoToAllResponse.NoToAll);
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment
                    .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                    .Throws(new UnauthorizedAccessException("denied"))
                    .Throws(new UnauthorizedAccessException("denied"));

                // Act
                bool first = await SaveAsync(attachment, seams);
                bool second = await SaveAsync(attachment, seams);

                // Assert
                first.Should().BeFalse();
                second.Should().BeFalse();
                seams.PromptMessages.Should().Equal(ExpectedPrompt);
                seams.ClearedDirectories.Should().BeEmpty();
                seams.Session.Response.Should().Be(YesNoToAllResponse.NoToAll);
                attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(2));
            }

            /// <summary>
            /// T7. Scenario: the first save is denied and the prompt is cancelled (Empty). Expected:
            /// the access exception propagates; no clear; the session holds no answer.
            /// </summary>
            [TestMethod]
            public async Task TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException()
            {
                // Arrange
                var seams = new Seams(YesNoToAllResponse.Empty);
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment
                    .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                    .Throws(new UnauthorizedAccessException("denied"));

                // Act
                Func<Task> act = () => SaveAsync(attachment, seams);

                // Assert
                await act.Should().ThrowAsync<UnauthorizedAccessException>();
                seams.PromptMessages.Should().Equal(ExpectedPrompt);
                seams.ClearedDirectories.Should().BeEmpty();
                seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            }

            /// <summary>
            /// T8. Scenario: Yes is answered but clearing the attribute throws. Expected: false; no
            /// retry; the Yes answer released.
            /// </summary>
            [TestMethod]
            public async Task TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer()
            {
                // Arrange
                var seams = new Seams(YesNoToAllResponse.Yes)
                {
                    ClearException = new IOException("attribute locked"),
                };
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment
                    .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                    .Throws(new UnauthorizedAccessException("denied"));

                // Act
                bool saved = await SaveAsync(attachment, seams);

                // Assert
                saved.Should().BeFalse();
                seams.PromptMessages.Should().Equal(ExpectedPrompt);
                seams.ClearedDirectories.Should().Equal(SandboxDirectory);
                seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
                attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Once);
            }

            /// <summary>
            /// T9. Scenario: YesToAll is answered but clearing the attribute throws. Expected: false;
            /// no retry; the YesToAll answer stays held.
            /// </summary>
            [TestMethod]
            public async Task TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer()
            {
                // Arrange
                var seams = new Seams(YesNoToAllResponse.YesToAll)
                {
                    ClearException = new IOException("attribute locked"),
                };
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment
                    .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                    .Throws(new UnauthorizedAccessException("denied"));

                // Act
                bool saved = await SaveAsync(attachment, seams);

                // Assert
                saved.Should().BeFalse();
                seams.PromptMessages.Should().Equal(ExpectedPrompt);
                seams.ClearedDirectories.Should().Equal(SandboxDirectory);
                seams.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);
                attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Once);
            }

            /// <summary>
            /// T10. Scenario: the save throws an exception other than an access denial. Expected: the
            /// exception propagates unchanged and no prompt or clear occurs.
            /// </summary>
            [TestMethod]
            public async Task TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt()
            {
                // Arrange
                var seams = new Seams();
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment
                    .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                    .Throws(new IOException("disk failure"));

                // Act
                Func<Task> act = () => SaveAsync(attachment, seams);

                // Assert
                await act.Should().ThrowAsync<IOException>();
                seams.PromptMessages.Should().BeEmpty();
                seams.ClearedDirectories.Should().BeEmpty();
                seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            }

            /// <summary>
            /// T11. Scenario: Yes is answered, the retry is denied again and the second answer is No.
            /// Expected: false after two prompts, one clear and two saves.
            /// </summary>
            [TestMethod]
            public async Task TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse()
            {
                // Arrange
                var seams = new Seams(YesNoToAllResponse.Yes, YesNoToAllResponse.No);
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment
                    .SetupSequence(x => x.SaveAsFile(SandboxFilePath))
                    .Throws(new UnauthorizedAccessException("denied"))
                    .Throws(new UnauthorizedAccessException("denied"));

                // Act
                bool saved = await SaveAsync(attachment, seams);

                // Assert
                saved.Should().BeFalse();
                seams.PromptMessages.Should().Equal(ExpectedPrompt, ExpectedPrompt);
                seams.ClearedDirectories.Should().Equal(SandboxDirectory);
                seams.Session.Response.Should().Be(YesNoToAllResponse.Empty);
                attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(2));
            }

            /// <summary>
            /// Calls the five-argument overload under test with the sandbox file path and the seams
            /// of the given recorder.
            /// </summary>
            private static Task<bool> SaveAsync(Mock<Attachment> attachment, Seams seams)
            {
                return attachment.Object.TrySaveAttachmentAsync(
                    SandboxFilePath,
                    seams.CreateDirectory,
                    seams.ClearReadOnly,
                    seams.Session
                );
            }

            /// <summary>
            /// Records every seam call made by the method under test and answers each prompt with the
            /// next scripted response. Each test creates its own instance, so no state is shared
            /// between tests; an unscripted prompt fails the test through the empty queue.
            /// </summary>
            private sealed class Seams
            {
                private readonly Queue<YesNoToAllResponse> _answers;

                public Seams(params YesNoToAllResponse[] answers)
                {
                    _answers = new Queue<YesNoToAllResponse>(answers);
                    Session = new YesNoToAllPromptSession(Prompt);
                }

                public YesNoToAllPromptSession Session { get; }
                public List<string> CreatedDirectories { get; } = new List<string>();
                public List<string> ClearedDirectories { get; } = new List<string>();
                public List<string> PromptMessages { get; } = new List<string>();
                public System.Exception ClearException { get; set; }

                public void CreateDirectory(string path)
                {
                    CreatedDirectories.Add(path);
                }

                public void ClearReadOnly(string path)
                {
                    ClearedDirectories.Add(path);
                    if (ClearException is not null)
                    {
                        throw ClearException;
                    }
                }

                private YesNoToAllResponse Prompt(string message)
                {
                    PromptMessages.Add(message);
                    return _answers.Dequeue();
                }
            }
        }
    }

### Test inventory (spec Test Strategy mapping)

| ID | Method | Scenario | Assertions | Branches |
| --- | --- | --- | --- | --- |
| T1 | `TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly` | save succeeds | true; no prompt; no clear; directory created once; Response Empty; one save | B0 |
| T2 | `TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer` | denial, Yes, retry succeeds | true; exactly one prompt equal to the expected message (contains the directory and `is read-only`); clear once with the directory; directory created twice; Response Empty; two saves | B1, B3, B5 |
| T3 | `TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer` | as T2 with YesToAll | true; one prompt; clear once; Response YesToAll; two saves | B1, B3, B6 |
| T4 | `TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt` | two denied calls on one session | both true; exactly one prompt; clear twice; Response YesToAll; four saves | B2, B3 |
| T5 | `TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer` | denial, No | false; one prompt; no clear; Response Empty; one save | B1, B7 reset arm |
| T6 | `TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt` | denial, NoToAll, second denied call | both false; one prompt; no clear; Response NoToAll; two saves | B1, B2, B7 sticky arm |
| T7 | `TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException` | denial, Empty (Cancel) | throws UnauthorizedAccessException; one prompt; no clear; Response Empty | B1, B8 |
| T8 | `TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer` | denial, Yes, clear throws | false; one clear; Response Empty; one save | B4, B5 |
| T9 | `TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer` | denial, YesToAll, clear throws | false; one clear; Response YesToAll; one save | B4, B6 |
| T10 | `TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt` | non-access exception | IOException propagates; no prompt; no clear; Response Empty | B9 |
| T11 | `TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse` | Yes, retry denied, then No | false; two prompts; one clear; Response Empty; two saves | B3, B5, B1, B7 |
| S1 | `Constructor_WhenShowDialogIsNull_ThrowsArgumentNullException` | null delegate | ArgumentNullException with parameter name `showDialog` | constructor guard |
| S2 | `Ask_WhenNoAnswerIsHeld_InvokesPromptAndStoresAnswer` | Ask from Empty | initial Empty; answer Yes returned and held; prompt received the message once | Ask true arm |
| S3 | `Ask_WhenAnswerIsHeld_ReturnsItWithoutInvokingPrompt` | Ask from a held answer | held NoToAll returned; prompt invoked once in total | Ask false arm |
| S4 | `ReleaseSingleAnswer_WhenAnswerIsYesOrNo_ClearsIt` | release Yes and No | both Empty | release true arm (both operands) |
| S5 | `ReleaseSingleAnswer_WhenAnswerIsYesToAllOrNoToAll_KeepsIt` | release YesToAll and NoToAll | both kept | release false arm |
| S6 | `Reset_WhenToAllAnswerIsHeld_ClearsItSoThePromptIsShownAgain` | Reset a sticky answer | Empty after Reset; prompt invoked again (two calls) | Reset |
| S7 | `Ask_WhenPromptReturnsEmpty_HoldsNoAnswerAndAsksAgain` | prompt returns Empty | both answers Empty; Response Empty; prompt received both messages | Ask with Empty answer |

No test scripts YesToAll together with a retry that keeps being denied: the YesToAll tests T3 and T4 follow every denial with a passing save, and T9 fails the clear so no retry occurs (L2 is not exercised). Each test creates its own `Seams` recorder, session and mock; no test reads or writes a static member of `SortEmail`.

### Control Prediction (PD-4, P3-T11)

With `clearReadOnly(directory);` replaced by `// NEGATIVE-CONTROL-956`, the filter `FullyQualifiedName~EmailIntelligence.SortEmail_TrySaveAttachment_Tests` reports total 11, failed 6, passed 5. Failed set (exactly): T2 `TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer`, T3 `TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer`, T4 `TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt` (each fails on the `ClearedDirectories` equality), T8 `TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer` and T9 `TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer` (the clear no longer throws, the retry reaches an exhausted Loose sequence and returns true, so `BeFalse` fails), T11 `TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse` (fails on the `ClearedDirectories` equality). Passed set (exactly): T1, T5, T6, T7, T10, which never reach the clear. Any other outcome is `MUTATION PREDICTION MISMATCH`: restore (P3-T12) and stop; never adjust a test.

## Token Census Expectations

All counts are `CMD-CENSUS` outputs (whitespace removed, case-sensitive). File aliases: SE = SortEmail.cs, MIS = SortEmail.MailItemSort.cs, AS = SortEmail.AttachmentSaving.cs, TS = SortEmail.TrySaveAttachment.cs, LAS = SortEmail.LegacyAttachmentSaving.cs, UML = SortEmail.UndoAndMoveLog.cs (all under UtilitiesCS/EmailIntelligence/EmailParsingSorting/). Every count not listed for a file is 0 in that file. Every value is gated as an exact equality.

### TOKENS-SORTEMAIL

Payload literal: `"[ExcludeFromCodeCoverage]", "YesNoToAll.ShowDialog(", "YesNoToAll.ShowDialog", "_removeReadOnly", "TrySaveAttachmentAsync(", "File.Delete(", "File.Exists(", "File.", "Directory.", "DirectoryInfo", "FileAttributes", "FileIO2.WriteTextFile(", "newFileInfo(", "publicstaticclassSortEmail", "publicstaticpartialclassSortEmail", "#region", "#endregion", "#nullableenable", "staticAction<", "staticFunc<", "RemoveReadOnlyPrompt", "YesNoToAllPromptSession", "RemoveReadOnlyPrompt.Reset();", "case(YesNoToAllResponse.NoToAll|YesNoToAllResponse.No):", "case(YesNoToAllResponse.Yes|YesNoToAllResponse.YesToAll):", "File.Exists(Path.Combine(strFileName,strFileLocation))", "catch(System.UnauthorizedAccessException", "internalstaticvoidSaveAttachmentsOld(", "internalstaticboolIsPicture("`.

| # | Token | PRE (SRC, P0-T12) | SPLIT per file (P2-T9) | SPLIT TOTAL | SEAM per file (P3-T6, P4-T10) | SEAM TOTAL |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | `[ExcludeFromCodeCoverage]` | 28 | SE 4, MIS 5, AS 10, TS 2, LAS 1, UML 6 | 28 | SE 4, MIS 5, AS 10, TS 2, LAS 1, UML 6 | 28 |
| 2 | `YesNoToAll.ShowDialog(` | 9 | AS 6, TS 1, LAS 2 | 9 | AS 6, LAS 2 | 8 |
| 3 | `YesNoToAll.ShowDialog` | 9 | AS 6, TS 1, LAS 2 | 9 | AS 6, TS 1, LAS 2 | 9 |
| 4 | `_removeReadOnly` | 13 | AS 1, TS 12 | 13 | none | 0 |
| 5 | `TrySaveAttachmentAsync(` | 7 | AS 3, TS 4 | 7 | AS 3, TS 6 | 9 |
| 6 | `File.Delete(` | 5 | SE 1, MIS 2, LAS 2 | 5 | SE 1, MIS 2, LAS 2 | 5 |
| 7 | `File.Exists(` | 6 | AS 2, LAS 3, UML 1 | 6 | AS 2, LAS 3, UML 1 | 6 |
| 8 | `File.` | 11 | SE 1, MIS 2, AS 2, LAS 5, UML 1 | 11 | SE 1, MIS 2, AS 2, LAS 5, UML 1 | 11 |
| 9 | `Directory.` | 1 | TS 1 | 1 | TS 1 | 1 |
| 10 | `DirectoryInfo` | 1 | TS 1 | 1 | TS 1 | 1 |
| 11 | `FileAttributes` | 1 | TS 1 | 1 | TS 1 | 1 |
| 12 | `FileIO2.WriteTextFile(` | 1 | UML 1 | 1 | UML 1 | 1 |
| 13 | `newFileInfo(` | 0 | none | 0 | none | 0 |
| 14 | `publicstaticclassSortEmail` | 1 | none | 0 | none | 0 |
| 15 | `publicstaticpartialclassSortEmail` | 0 | 1 in each of the six | 6 | 1 in each of the six | 6 |
| 16 | `#region` | 5 | LAS 2 | 2 | LAS 2 | 2 |
| 17 | `#endregion` | 5 | LAS 2 | 2 | LAS 2 | 2 |
| 18 | `#nullableenable` | 1 | 1 in each of the six | 6 | 1 in each of the six | 6 |
| 19 | `staticAction<` | 0 | none | 0 | none | 0 |
| 20 | `staticFunc<` | 0 | none | 0 | none | 0 |
| 21 | `RemoveReadOnlyPrompt` | 0 | none | 0 | AS 1, TS 2 | 3 |
| 22 | `YesNoToAllPromptSession` | 0 | none | 0 | TS 2 | 2 |
| 23 | `RemoveReadOnlyPrompt.Reset();` | 0 | none | 0 | AS 1 | 1 |
| 24 | `case(YesNoToAllResponse.NoToAll\|YesNoToAllResponse.No):` (L1) | 1 | AS 1 | 1 | AS 1 | 1 |
| 25 | `case(YesNoToAllResponse.Yes\|YesNoToAllResponse.YesToAll):` (L1) | 1 | AS 1 | 1 | AS 1 | 1 |
| 26 | `File.Exists(Path.Combine(strFileName,strFileLocation))` (L4) | 1 | UML 1 | 1 | UML 1 | 1 |
| 27 | `catch(System.UnauthorizedAccessException` | 1 | TS 1 | 1 | TS 1 | 1 |
| 28 | `internalstaticvoidSaveAttachmentsOld(` (F2) | 1 | LAS 1 | 1 | LAS 1 | 1 |
| 29 | `internalstaticboolIsPicture(` (F2) | 1 | AS 1 | 1 | AS 1 | 1 |

(In rows 24 and 25 the backslash before the pipe is Markdown table escaping only; the census token contains a bare pipe, exactly as in the payload literal above.)

AC13 reading of the SEAM column: the direct file-system call sites after the change are `File.Delete(` SE 1 (SRC 245, J1), MIS 2 (369 and 515, J1), LAS 2 (1214 and 1218, J4); `File.Exists(` AS 2 (704 J2, 767 J3), LAS 3 (1212, 1216, 1227, J4), UML 1 (1406, J5); `FileIO2.WriteTextFile(` UML 1 (1425, J5); `Directory.` TS 1 (902, the #945 wrapper default); `DirectoryInfo` and `FileAttributes` TS 1 each, located only in `ClearReadOnlyAttributeOnDisk` (TOKENS-TRYSAVE A6). The totals equal the PRE totals, so no direct file-system call is introduced.

### TOKENS-TRYSAVE (UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs)

Payload literal: `"[ExcludeFromCodeCoverage]internalstaticTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave)", "</summary>internalstaticTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave,Action<string>createDirectory)", "</returns>internalstaticasyncTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave,Action<string>createDirectory,Action<string>clearReadOnly,YesNoToAllPromptSessionremoveReadOnlyPrompt)", "[ExcludeFromCodeCoverage]privatestaticvoidClearReadOnlyAttributeOnDisk(stringdirectoryPath)", "privatestaticreadonlyYesNoToAllPromptSessionRemoveReadOnlyPrompt=new(YesNoToAll.ShowDialog);", "vardi=newDirectoryInfo(directoryPath);di.Attributes&=~System.IO.FileAttributes.ReadOnly;", "returnTrySaveAttachmentAsync(attachment,filePathSave,path=>System.IO.Directory.CreateDirectory(path));", "returnTrySaveAttachmentAsync(attachment,filePathSave,createDirectory,ClearReadOnlyAttributeOnDisk,RemoveReadOnlyPrompt);", "returnawaitTrySaveAttachmentAsync(attachment,filePathSave,createDirectory,clearReadOnly,removeReadOnlyPrompt);", "try{createDirectory(Path.GetDirectoryName(filePathSave));awaitTask.Run(()=>attachment.SaveAsFile(filePathSave));returntrue;}catch(System.UnauthorizedAccessExceptione){Debug.WriteLine(e.Message);", "if(removeReadOnlyPrompt.Response==YesNoToAllResponse.Empty){varmessage=", "isread-only.Doyouwanttoremovethereadonlyattribute?", "removeReadOnlyPrompt.Ask(message);}if((removeReadOnlyPrompt.Response==YesNoToAllResponse.Yes)||(removeReadOnlyPrompt.Response==YesNoToAllResponse.YesToAll)){vardirectory=Path.GetDirectoryName(filePathSave);try{clearReadOnly(directory);}catch(System.Exceptioninner){Debug.WriteLine(inner.Message);returnfalse;}finally{removeReadOnlyPrompt.ReleaseSingleAnswer();}", "}elseif((removeReadOnlyPrompt.Response==YesNoToAllResponse.No)||(removeReadOnlyPrompt.Response==YesNoToAllResponse.NoToAll)){Debug.WriteLine(", "removeReadOnlyPrompt.ReleaseSingleAnswer();returnfalse;}else{throw;}}catch(System.Exception){throw;}}", "catch(", "throw;", "clearReadOnly(directory);", "//NEGATIVE-CONTROL-956", "Excludedfromcoverage:", "(UT4)", "asyncTask<bool>", "Path.GetDirectoryName(filePathSave)", "ShowDialog("`.

| ID | Token (abbreviated where long; the payload literal is authoritative) | Proves | SEAM (P3-T6, P4-T10) | CONTROL (P3-T10) |
| --- | --- | --- | --- | --- |
| A1 | `[ExcludeFromCodeCoverage]internalstaticTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave)` | wrapper keeps the attribute and signature | 1 | 1 |
| A2 | `</summary>internalstaticTask<bool>TrySaveAttachmentAsync(...,Action<string>createDirectory)` | three-argument signature unchanged, no attribute, non-async | 1 | 1 |
| A3 | `</returns>internalstaticasyncTask<bool>TrySaveAttachmentAsync(...,YesNoToAllPromptSessionremoveReadOnlyPrompt)` | five-argument core signature, no attribute | 1 | 1 |
| A4 | `[ExcludeFromCodeCoverage]privatestaticvoidClearReadOnlyAttributeOnDisk(stringdirectoryPath)` | adapter carries the attribute | 1 | 1 |
| A5 | `privatestaticreadonlyYesNoToAllPromptSessionRemoveReadOnlyPrompt=new(YesNoToAll.ShowDialog);` | single production initializer, not settable | 1 | 1 |
| A6 | `vardi=newDirectoryInfo(directoryPath);di.Attributes&=~System.IO.FileAttributes.ReadOnly;` | attribute write only in the adapter | 1 | 1 |
| A7 | `returnTrySaveAttachmentAsync(attachment,filePathSave,path=>System.IO.Directory.CreateDirectory(path));` | #945 lambda kept in the wrapper | 1 | 1 |
| A8 | `returnTrySaveAttachmentAsync(attachment,filePathSave,createDirectory,ClearReadOnlyAttributeOnDisk,RemoveReadOnlyPrompt);` | one-statement forward | 1 | 1 |
| A9 | `returnawaitTrySaveAttachmentAsync(attachment,filePathSave,createDirectory,clearReadOnly,removeReadOnlyPrompt);` | retry passes all three seams | 1 | 1 |
| A10 | `try{createDirectory(...);awaitTask.Run(...);returntrue;}catch(System.UnauthorizedAccessExceptione){Debug.WriteLine(e.Message);` | try prefix and createDirectory outside the handler unchanged | 1 | 1 |
| A11 | `if(removeReadOnlyPrompt.Response==YesNoToAllResponse.Empty){varmessage=` | prompt guarded by the Empty state | 1 | 1 |
| A12 | `isread-only.Doyouwanttoremovethereadonlyattribute?` | message text unchanged | 1 | 1 |
| A13 | `removeReadOnlyPrompt.Ask(message);}if((...Yes)\|\|(...YesToAll)){vardirectory=Path.GetDirectoryName(filePathSave);try{clearReadOnly(directory);}catch(System.Exceptioninner){...returnfalse;}finally{removeReadOnlyPrompt.ReleaseSingleAnswer();}` | Ask replaces the dialog; directory computed before the inner try; clear inside it; boundary unchanged | 1 | 0 |
| A14 | `}elseif((...No)\|\|(...NoToAll)){Debug.WriteLine(` | No arm unchanged | 1 | 1 |
| A15 | `removeReadOnlyPrompt.ReleaseSingleAnswer();returnfalse;}else{throw;}}catch(System.Exception){throw;}}` | release, Cancel rethrow and outer rethrow unchanged | 1 | 1 |
| A16 | `catch(` | no catch added (SRC core has 3) | 3 | 3 |
| A17 | `throw;` | two rethrows, as at SRC | 2 | 2 |
| A18 | `clearReadOnly(directory);` | the controlled statement | 1 | 0 |
| A19 | `//NEGATIVE-CONTROL-956` | control marker | 0 | 1 |
| A20 | `Excludedfromcoverage:` | two justification comments (D2) | 2 | 2 |
| A21 | `(UT4)` | justification names the policy | 2 | 2 |
| A22 | `asyncTask<bool>` | only the core is async | 1 | 1 |
| A23 | `Path.GetDirectoryName(filePathSave)` | three uses as at SRC | 3 | 3 |
| A24 | `ShowDialog(` | no dialog call in the file | 0 | 0 |

(In the table the backslashes before pipes are Markdown escaping only.)

### TOKENS-SESSION (UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs)

Payload literal: `"#nullableenable", "namespaceUtilitiesCS{", "internalsealedclassYesNoToAllPromptSession", "privatereadonlyFunc<string,YesNoToAllResponse>_showDialog;", "internalYesNoToAllPromptSession(Func<string,YesNoToAllResponse>showDialog)", "_showDialog=showDialog??thrownewArgumentNullException(nameof(showDialog));", "internalYesNoToAllResponseResponse{get;privateset;}", "internalYesNoToAllResponseAsk(stringmessage){if(Response==YesNoToAllResponse.Empty){Response=_showDialog(message);}returnResponse;}", "internalvoidReleaseSingleAnswer(){if(Response==YesNoToAllResponse.Yes||Response==YesNoToAllResponse.No){Response=YesNoToAllResponse.Empty;}}", "internalvoidReset(){Response=YesNoToAllResponse.Empty;}", "static", "ShowDialog", "ExcludeFromCodeCoverage"`. Expected: the first ten tokens 1 each; `static`, `ShowDialog` and `ExcludeFromCodeCoverage` 0 each (no static member, so no settable static seam; no dialog reference; no exclusion).

### TOKENS-TEST-T (UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs)

Payload literal: `"[TestMethod]", "[TestClass]", "namespaceUtilitiesCS.Test.EmailIntelligence{", "publicclassSortEmail_TrySaveAttachment_Tests", "C:\Sortemail956Sandbox\attachments", "TrySaveAttachmentAsync(", "SaveAsync(attachment,seams)", "newSeams(", "newSeams(YesNoToAllResponse.YesToAll)", "newYesNoToAllPromptSession(Prompt)", ".Throws(newUnauthorizedAccessException(", ".Throws(newIOException(", "ThrowAsync<UnauthorizedAccessException>()", "ThrowAsync<IOException>()", "seams.Session.Response.Should().Be(", "newMock<Attachment>(MockBehavior.Loose)", "System.Exception", "SortEmail.", "typeof(", "RemoveReadOnlyPrompt", "Cleanup_Files", "DoNotParallelize", "[DataRow", "File.", "Directory.", "Path.", "Thread.Sleep", "Task.Delay", "YesNoToAll.ShowDialog", "GetTemp", "Xunit", "NUnit"`. Expected, in order: 11, 1, 1, 1, 3, 1, 13, 11, 3, 1, 12, 1, 1, 1, 11, 11, 1, then 0 for each of the fifteen remaining tokens (`SortEmail.` through `NUnit`).

### TOKENS-TEST-S (UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs)

Payload literal: `"[TestMethod]", "namespaceUtilitiesCS.Test.Dialogs{", "publicclassYesNoToAllPromptSession_Tests", "newYesNoToAllPromptSession(", "WithParameterName(", ".ReleaseSingleAnswer();", ".Reset();", "[DataRow", "DoNotParallelize", "SortEmail", "YesNoToAll.ShowDialog", "File.", "Thread.Sleep", "Task.Delay", "Xunit", "NUnit"`. Expected, in order: 7, 1, 1, 9, 1, 4, 1, then 0 for each of the nine remaining tokens.

## Command Reference

Each block is a payload in the sense of the command-channel convention: one complete statement per line (a braced block on one line is one statement), no line continuation, no single-quote character, no comment line. The executor joins the lines with `; ` inside `pwsh -NoProfile -Command '...'`.

Filters. `FILTER-SORTEMAIL` is `"/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_"` (expected `total` 15 before Phase 3, 26 after); `FILTER-TRYSAVE` is `"/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_TrySaveAttachment_Tests"` (11); `FILTER-SESSION` is `"/TestCaseFilter:FullyQualifiedName~Dialogs.YesNoToAllPromptSession_Tests"` (7); `FILTER-STALL` is `"/TestCaseFilter:FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests"`. `EXCLUSION` (fixed by P0-T9) is empty under `STALL-PROBE: CLEAR` and is `&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests` under `REPRODUCES`.

Path sets. `PATHS-SRC` is `"UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs"`. `PATHS-SIX` is `"UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs"`. `PATHS-TRYSAVE` is the fourth entry of `PATHS-SIX`. `PATHS-SESSION` is `"UtilitiesCS\Dialogs\YesNoToAllPromptSession.cs"`. `PATHS-TEST-T` is `"UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs"`. `PATHS-TEST-S` is `"UtilitiesCS.Test\Dialogs\YesNoToAllPromptSession_Tests.cs"`. `PATHS-TESTS` is `PATHS-TEST-T` and `PATHS-TEST-S` joined by `, `. `PATHS-NINE` is `PATHS-SIX`, `PATHS-SESSION` and `PATHS-TESTS` joined by `, `.

**CMD-CENSUS** (`PATHS` and `TOKENS` substituted; prints `TOKEN <token> @ <path> = <n>` per file, `TOKEN <token> @ TOTAL = <n>`, then `LINES` and `SHA256` per file):

    Set-Location -LiteralPath "WORKTREE"
    $paths = @(PATHS)
    $tokens = @(TOKENS)
    $content = @{}
    foreach ($p in $paths) { $content[$p] = [regex]::Replace((Get-Content -LiteralPath $p -Raw -Encoding UTF8), "\s+", "") }
    foreach ($t in $tokens) { $total = 0; foreach ($p in $paths) { $n = [regex]::Matches($content[$p], [regex]::Escape($t)).Count; $total += $n; Write-Output ("TOKEN " + $t + " @ " + $p + " = " + $n) }; Write-Output ("TOKEN " + $t + " @ TOTAL = " + $total) }
    foreach ($p in $paths) { Write-Output ("LINES " + $p + " = " + @(Get-Content -LiteralPath $p -Encoding UTF8).Count); Write-Output ("SHA256 " + $p + " = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash) }

**CMD-BACKUP-MERGEBASE** (P0-T12; copies the unmodified SRC to the git-ignored backup):

    Set-Location -LiteralPath "WORKTREE"
    $src = "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs"
    $bak = "coverage\control-956\SortEmail.mergebase.bak"
    New-Item -ItemType Directory -Path "coverage\control-956" -Force | Out-Null
    Copy-Item -LiteralPath $src -Destination $bak -Force
    Write-Output ("SRC-HASH: " + (Get-FileHash -Algorithm SHA256 -LiteralPath $src).Hash)
    Write-Output ("BACKUP-HASH: " + (Get-FileHash -Algorithm SHA256 -LiteralPath $bak).Hash)
    Write-Output ("BACKUP-LINES: " + @(Get-Content -LiteralPath $bak -Encoding UTF8).Count)

**CMD-PARTITION** (P0-T13; proves that header, class line, region lines, the 36 segments and the two closing braces partition the backup):

    Set-Location -LiteralPath "WORKTREE"
    $L = @(Get-Content -LiteralPath "coverage\control-956\SortEmail.mergebase.bak" -Encoding UTF8)
    $strip = { param($s) [regex]::Replace($s, "\s+", "") }
    $defs = @("S01:27:29", "S02:31:40", "S03:42:74", "S04:76:110", "S05:112:179", "S06:181:208", "S07:210:301", "S08:303:453", "S09:455:553", "S10:555:561", "S11:563:618", "S12:624:627", "S13:628:628", "S14:630:630", "S15:636:659", "S16:661:699", "S17:701:760", "S18:762:823", "S19:825:837", "S20:839:886", "S21:888:892", "S22:893:904", "S23:906:984", "S24:986:1005", "S25:1007:1016", "S26:1018:1057", "S27:1059:1103", "S28:1105:1114", "S29:1116:1123", "S30:1125:1336", "S31:1341:1351", "S32:1353:1366", "S33:1368:1384", "S34:1386:1396", "S35:1398:1429", "S36:1431:1452")
    $seg = [ordered]@{}
    $sum = 0
    foreach ($r in $defs) { $p = $r.Split(":"); $a = [int]$p[1]; $b = [int]$p[2]; $sum += $b - $a + 1; $seg[$p[0]] = & $strip ([string]::Join("", @($L[($a - 1)..($b - 1)]))) }
    $H = & $strip ([string]::Join("", $L[0..21]))
    $j = { param($names) [string]::Join("", @($names | ForEach-Object { $seg[$_] })) }
    $exp = $H + "publicstaticclassSortEmail{" + "#regionPublicMethods" + (& $j @("S01", "S02", "S03", "S04", "S05", "S06", "S07", "S08", "S09", "S10", "S11")) + "#endregion" + "#regionPrivateStaticVariables" + (& $j @("S12", "S13", "S14")) + "#endregion" + "#regionHelperMethods" + (& $j @("S15", "S16", "S17", "S18", "S19", "S20", "S21", "S22", "S23", "S24", "S25", "S26", "S27", "S28", "S29", "S30")) + "#endregion" + (& $j @("S31", "S32", "S33", "S34", "S35", "S36")) + "}}"
    $whole = & $strip ([string]::Join("", $L))
    Write-Output ("BACKUP-LINES: " + $L.Count)
    Write-Output ("SEGMENT-COUNT: " + $seg.Count)
    Write-Output ("SEGMENT-LINES: " + $sum)
    Write-Output ("PARTITION-EXACT: " + ($whole -ceq $exp))
    foreach ($k in $seg.Keys) { Write-Output ("SEG-UNIQUE " + $k + " = " + [regex]::Matches($whole, [regex]::Escape($seg[$k])).Count) }

**CMD-MOVE-CENSUS** (`STATE` is `split` or `seam`; `ONLY` is one file name such as `SortEmail.MailItemSort.cs`, or `ALL`; proves each partial file equals its assembly rule exactly and, under `ALL`, prints the segment matrix):

    Set-Location -LiteralPath "WORKTREE"
    $L = @(Get-Content -LiteralPath "coverage\control-956\SortEmail.mergebase.bak" -Encoding UTF8)
    $strip = { param($s) [regex]::Replace($s, "\s+", "") }
    $defs = @("S01:27:29", "S02:31:40", "S03:42:74", "S04:76:110", "S05:112:179", "S06:181:208", "S07:210:301", "S08:303:453", "S09:455:553", "S10:555:561", "S11:563:618", "S12:624:627", "S13:628:628", "S14:630:630", "S15:636:659", "S16:661:699", "S17:701:760", "S18:762:823", "S19:825:837", "S20:839:886", "S21:888:892", "S22:893:904", "S23:906:984", "S24:986:1005", "S25:1007:1016", "S26:1018:1057", "S27:1059:1103", "S28:1105:1114", "S29:1116:1123", "S30:1125:1336", "S31:1341:1351", "S32:1353:1366", "S33:1368:1384", "S34:1386:1396", "S35:1398:1429", "S36:1431:1452")
    $seg = [ordered]@{}
    foreach ($r in $defs) { $p = $r.Split(":"); $seg[$p[0]] = & $strip ([string]::Join("", @($L[([int]$p[1] - 1)..([int]$p[2] - 1)]))) }
    $seg["S10P"] = $seg["S10"].Replace("_removeReadOnly=YesNoToAllResponse.Empty;", "RemoveReadOnlyPrompt.Reset();")
    $P = (& $strip ([string]::Join("", $L[0..21]))) + "publicstaticpartialclassSortEmail{"
    $dir = "UtilitiesCS\EmailIntelligence\EmailParsingSorting"
    $map = [ordered]@{ "SortEmail.cs" = @("S01", "S02", "S05", "S06", "S07", "S27"); "SortEmail.MailItemSort.cs" = @("S03", "S04", "S08", "S09", "S26"); "SortEmail.AttachmentSaving.cs" = @("S12", "S10", "S15", "S16", "S17", "S18", "S19", "S20", "S24", "S25", "S28", "S29"); "SortEmail.TrySaveAttachment.cs" = @("S13", "S21", "S22", "S23"); "SortEmail.LegacyAttachmentSaving.cs" = @("S14", "S30"); "SortEmail.UndoAndMoveLog.cs" = @("S11", "S31", "S32", "S33", "S34", "S35", "S36") }
    $state = "STATE"
    if ($state -eq "seam") { $map["SortEmail.AttachmentSaving.cs"] = @("S12", "S10P", "S15", "S16", "S17", "S18", "S19", "S20", "S24", "S25", "S28", "S29") }
    $only = "ONLY"
    $targets = if ($only -eq "ALL") { @($map.Keys) } else { @($only) }
    $text = @{}
    foreach ($f in $targets) { $text[$f] = & $strip (Get-Content -LiteralPath (Join-Path $dir $f) -Raw -Encoding UTF8) }
    foreach ($f in $targets) { $exp = $P + [string]::Join("", @($map[$f] | ForEach-Object { $seg[$_] })) + "}}"; $exact = if ($state -eq "seam" -and $f -eq "SortEmail.TrySaveAttachment.cs") { "NOT-APPLICABLE" } else { [string]($text[$f] -ceq $exp) }; Write-Output ("FILE-EXACT " + $f + " = " + $exact); Write-Output ("HEADER-PREFIX " + $f + " = " + $text[$f].StartsWith($P, [System.StringComparison]::Ordinal)); Write-Output ("CLOSING-BRACES " + $f + " = " + $text[$f].EndsWith("}}", [System.StringComparison]::Ordinal)); Write-Output ("FIRST-LINE " + $f + " = " + ((Get-Content -LiteralPath (Join-Path $dir $f) -TotalCount 1 -Encoding UTF8) -ceq "#nullable enable")); Write-Output ("LINES " + $f + " = " + @(Get-Content -LiteralPath (Join-Path $dir $f) -Encoding UTF8).Count) }
    if ($only -eq "ALL") { foreach ($k in $seg.Keys) { $hits = @(); $tot = 0; foreach ($f in $targets) { $c = [regex]::Matches($text[$f], [regex]::Escape($seg[$k])).Count; if ($c -gt 0) { $hits += ($f + "=" + $c) }; $tot += $c }; Write-Output ("SEG " + $k + " TOTAL=" + $tot + " IN=" + ($hits -join ",")) } }

Expected under `ALL`. Split state: `FILE-EXACT ... = True` for all six; every `HEADER-PREFIX`, `CLOSING-BRACES` and `FIRST-LINE` True; `SEG Sxx TOTAL=1` with `IN=<destination of the Segment Table>=1` for S01 to S36; `SEG S10P TOTAL=0`. Seam state: `FILE-EXACT` True for the five files other than SortEmail.TrySaveAttachment.cs and `NOT-APPLICABLE` for it; every `HEADER-PREFIX`, `CLOSING-BRACES` and `FIRST-LINE` True; `SEG S10`, `SEG S13` and `SEG S23` TOTAL=0; `SEG S10P TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1`; S21 and S22 TOTAL=1 in SortEmail.TrySaveAttachment.cs; every other segment TOTAL=1 in its Segment Table destination.

**CMD-CSPROJ** (exact-line positions of the Compile entries in both project files):

    Set-Location -LiteralPath "WORKTREE"
    $q = [string][char]34
    $u = @(Get-Content -LiteralPath "UtilitiesCS\UtilitiesCS.csproj" -Encoding UTF8)
    $t = @(Get-Content -LiteralPath "UtilitiesCS.Test\UtilitiesCS.Test.csproj" -Encoding UTF8)
    foreach ($inc in @("Dialogs\NotImplementedDialog.cs", "Dialogs\YesNoToAll.cs", "Dialogs\YesNoToAllPromptSession.cs", "EmailIntelligence\Bayesian\Obsolete\BayesianClassifier.cs", "EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs", "EmailIntelligence\EmailParsingSorting\SortEmail.cs", "EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs", "EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs", "EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs", "EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs", "EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs", "OutlookObjects\Folder\FolderPredictor.cs")) { $want = "    <Compile Include=" + $q + $inc + $q + " />"; $hits = @(for ($i = 0; $i -lt $u.Count; $i++) { if ($u[$i] -ceq $want) { $i + 1 } }); Write-Output ("UCS " + $inc + " COUNT=" + $hits.Count + " LINE=" + ($hits -join ",")) }
    foreach ($inc in @("EmailIntelligence\Triage_OlLogic_Tests.cs", "EmailIntelligence\SortEmail_Tests.cs", "EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs", "EmailIntelligence\FilterOlFoldersController_Tests.cs", "Dialogs\YesNoToAll_Test.cs", "Dialogs\YesNoToAll_Tests.cs", "Dialogs\YesNoToAllPromptSession_Tests.cs", "ReusableTypeClasses\AsyncLazy_Tests.cs")) { $want = "    <Compile Include=" + $q + $inc + $q + " />"; $hits = @(for ($i = 0; $i -lt $t.Count; $i++) { if ($t[$i] -ceq $want) { $i + 1 } }); Write-Output ("UCT " + $inc + " COUNT=" + $hits.Count + " LINE=" + ($hits -join ",")) }

Expected positions (COUNT=1 for every entry that exists; COUNT=0 for an entry not yet added):

| Entry | MERGE-BASE | after P1-T3 / P2-T7 | after P3-T2 (final) |
| --- | --- | --- | --- |
| UCS `Dialogs\NotImplementedDialog.cs` | 573 | 573 | 573 |
| UCS `Dialogs\YesNoToAll.cs` | 574 | 574 | 574 |
| UCS `Dialogs\YesNoToAllPromptSession.cs` | absent | absent | 575 |
| UCS `EmailIntelligence\Bayesian\Obsolete\BayesianClassifier.cs` | 575 | 575 | 576 |
| UCS `...\MovedMailInfo.cs` | 816 | 816 | 817 |
| UCS `...\SortEmail.cs` | 817 | 817 | 818 |
| UCS `...\SortEmail.AttachmentSaving.cs` | absent | 818 | 819 |
| UCS `...\SortEmail.LegacyAttachmentSaving.cs` | absent | 819 | 820 |
| UCS `...\SortEmail.MailItemSort.cs` | absent | 820 | 821 |
| UCS `...\SortEmail.TrySaveAttachment.cs` | absent | 821 | 822 |
| UCS `...\SortEmail.UndoAndMoveLog.cs` | absent | 822 | 823 |
| UCS `OutlookObjects\Folder\FolderPredictor.cs` | 818 | 823 | 824 |
| UCT `EmailIntelligence\Triage_OlLogic_Tests.cs` | 97 | 97 | 97 |
| UCT `EmailIntelligence\SortEmail_Tests.cs` | 98 | 98 | 98 |
| UCT `EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs` | absent | 99 | 99 |
| UCT `EmailIntelligence\FilterOlFoldersController_Tests.cs` | 99 | 100 | 100 |
| UCT `Dialogs\YesNoToAll_Test.cs` | 441 | 442 | 442 |
| UCT `Dialogs\YesNoToAll_Tests.cs` | 442 | 443 | 443 |
| UCT `Dialogs\YesNoToAllPromptSession_Tests.cs` | absent | 444 | 444 |
| UCT `ReusableTypeClasses\AsyncLazy_Tests.cs` | 443 | 445 | 445 |

**CMD-SCOPED-FORMAT** (`PATHS` and `TASKID` substituted; formats the named files and verifies them read-only):

    Set-Location -LiteralPath "WORKTREE"
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $paths = @(PATHS)
    foreach ($p in $paths) { Write-Output ("BEFORE " + $p + " = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash) }
    $global:LASTEXITCODE = 0
    & dotnet tool run csharpier format @paths 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.csharpier-format.log"
    Write-Output ("FORMAT_EXIT_CODE: " + $LASTEXITCODE)
    foreach ($p in $paths) { Write-Output ("AFTER " + $p + " = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash) }
    $global:LASTEXITCODE = 0
    & dotnet tool run csharpier check @paths 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.csharpier-check.log"
    Write-Output ("CHECK_EXIT_CODE: " + $LASTEXITCODE)

The success observation is the read-only check after the write (`CHECK_EXIT_CODE: 0`) together with the recorded BEFORE and AFTER hashes; the formatter's own summary line is a processed-file count printed on every run and is recorded but never gated.

**CMD-BUILD-PROD** (P2-T10; rebuilds the production project alone with warnings as errors; `TASKID` substituted):

    Set-Location -LiteralPath "WORKTREE"
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $global:LASTEXITCODE = 0
    & $msbuild "UtilitiesCS\UtilitiesCS.csproj" /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\logs\TASKID.msbuild.log;Verbosity=normal"
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" -Raw -Encoding UTF8
    Write-Output ("PROD_CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\UtilitiesCS.dll")).Count)
    Write-Output ("ZERO_ERRORS_LINES: " + [regex]::Matches($log, [regex]::Escape(" 0 Error(s)")).Count)
    Write-Output ("ZERO_WARNINGS_LINES: " + [regex]::Matches($log, [regex]::Escape(" 0 Warning(s)")).Count)
    Write-Output ("CS8632_LINES: " + [regex]::Matches($log, [regex]::Escape("CS8632")).Count)
    Write-Output ("CS0111_LINES: " + [regex]::Matches($log, [regex]::Escape("CS0111")).Count)
    Write-Output ("CS0102_LINES: " + [regex]::Matches($log, [regex]::Escape("CS0102")).Count)

`Command:` records `msbuild UtilitiesCS\UtilitiesCS.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true` with the notes `resolved through vswhere`, `plus /nodeReuse:false` and `plus a normal-verbosity file logger`. `ZERO_ERRORS_LINES` counts the literal with its leading space because `0 Error(s)` is a substring of `10 Error(s)`.

**CMD-BUILD-TEST** (builds UtilitiesCS.Test, and UtilitiesCS through its project reference; classifies compiler errors by file; `TASKID` substituted):

    Set-Location -LiteralPath "WORKTREE"
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    $dll = "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll"
    $before = if (Test-Path -LiteralPath $dll) { (Get-Item -LiteralPath $dll).LastWriteTimeUtc } else { [datetime]::MinValue }
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $global:LASTEXITCODE = 0
    & $msbuild "UtilitiesCS.Test\UtilitiesCS.Test.csproj" /t:Build /m /nodeReuse:false /p:Configuration=Debug /p:Platform=AnyCPU "/flp:LogFile=coverage\logs\TASKID.msbuild.log;Verbosity=normal"
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" -Raw -Encoding UTF8
    $errs = @(Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" -Encoding UTF8 | Where-Object { $_ -like "*error CS*" })
    $apos = [string][char]39
    Write-Output ("CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\UtilitiesCS.Test.dll")).Count)
    Write-Output ("ZERO_ERRORS_LINES: " + [regex]::Matches($log, [regex]::Escape(" 0 Error(s)")).Count)
    Write-Output ("ERROR_LINES: " + $errs.Count)
    Write-Output ("ERROR_LINES_NEW_TEST_FILES: " + @($errs | Where-Object { $_ -like "*SortEmail_TrySaveAttachment_Tests.cs*" -or $_ -like "*YesNoToAllPromptSession_Tests.cs*" }).Count)
    Write-Output ("ERROR_LINES_OTHER_FILES: " + @($errs | Where-Object { $_ -notlike "*SortEmail_TrySaveAttachment_Tests.cs*" -and $_ -notlike "*YesNoToAllPromptSession_Tests.cs*" }).Count)
    Write-Output ("MISSING_SESSION_TYPE_LINES: " + @($errs | Where-Object { $_ -like "*CS0246*" -and $_ -like ("*" + $apos + "YesNoToAllPromptSession" + $apos + "*") }).Count)
    Write-Output ("CS1501_LINES: " + @($errs | Where-Object { $_ -like "*CS1501*" }).Count)
    Write-Output ("ERROR_CODES: " + ((@($errs | ForEach-Object { [regex]::Match($_, "error (CS\d{4})").Groups[1].Value }) | Sort-Object -Unique) -join ","))
    Write-Output ("DLL_ADVANCED: " + ((Test-Path -LiteralPath $dll) -and ((Get-Item -LiteralPath $dll).LastWriteTimeUtc -gt $before)))

`Command:` records `msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU` with the three notes. The `error CS` lines carry absolute file paths; only the counts and the error codes are transcribed.

**CMD-REBUILD** (solution rebuild gate; `GATEARGS` is `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzer gate) or `/p:TreatWarningsAsErrors=true` (nullable gate); `TASKID` substituted):

    Set-Location -LiteralPath "WORKTREE"
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $global:LASTEXITCODE = 0
    & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" GATEARGS "/flp:LogFile=coverage\logs\TASKID.msbuild.log;Verbosity=normal"
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" -Raw -Encoding UTF8
    Write-Output ("SKIP_CORECOMPILE_LINES: " + [regex]::Matches($log, [regex]::Escape("Skipping target ""CoreCompile""")).Count)
    Write-Output ("UCS_TEST_CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\UtilitiesCS.Test.dll")).Count)
    Write-Output ("UCS_CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\UtilitiesCS.dll")).Count)
    Write-Output ("ZERO_ERRORS_LINES: " + [regex]::Matches($log, [regex]::Escape(" 0 Error(s)")).Count)
    Write-Output ("WARNINGS: " + [regex]::Match($log, "(\d+) Warning\(s\)").Groups[1].Value)
    Write-Output ("ERRORS: " + [regex]::Match($log, "(\d+) Error\(s\)").Groups[1].Value)
    Write-Output ("WRITESET_DIAGNOSTIC_LINES: " + @(Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" -Encoding UTF8 | Where-Object { ($_ -like "*EmailParsingSorting\SortEmail*" -or $_ -like "*YesNoToAllPromptSession*" -or $_ -like "*SortEmail_Tests.cs*" -or $_ -like "*SortEmail_TrySaveAttachment_Tests.cs*") -and ($_ -like "*warning *" -or $_ -like "*error *") }).Count)

`Command:` records `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzer) or `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (nullable), each with the three notes. The same `WRITESET_DIAGNOSTIC_LINES` pattern is applied at baseline (where it can only match SortEmail.cs and SortEmail_Tests.cs) and at the end, so the two figures are comparable.

**CMD-VSTEST** (one assembly under the CLI runsettings with the isolation switch; `FILTERARG` and `TASKID` substituted):

    Set-Location -LiteralPath "WORKTREE"
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
    $assembly = (Resolve-Path -LiteralPath "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll").Path
    $settings = (Resolve-Path -LiteralPath "scripts\vscode\TaskMaster.cli.runsettings").Path
    $results = Join-Path (Get-Location).Path "coverage\test-results\956\TASKID"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    Write-Output ("RUNSETTINGS-HASH-NOW: " + (Get-FileHash -Algorithm SHA256 -LiteralPath "scripts\vscode\TaskMaster.cli.runsettings").Hash)
    Write-Output ("SANDBOX-956-EXISTS-BEFORE: " + (Test-Path -LiteralPath "C:\Sortemail956Sandbox"))
    Write-Output ("SANDBOX-945-EXISTS-BEFORE: " + (Test-Path -LiteralPath "C:\Sortemail945Sandbox"))
    $global:LASTEXITCODE = 0
    & $vstest $assembly "/Settings:$settings" /InIsolation FILTERARG "/ResultsDirectory:$results" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.vstest.log"
    Write-Output ("VSTEST_EXIT_CODE: " + $LASTEXITCODE)
    Write-Output ("SANDBOX-956-EXISTS-AFTER: " + (Test-Path -LiteralPath "C:\Sortemail956Sandbox"))
    Write-Output ("SANDBOX-945-EXISTS-AFTER: " + (Test-Path -LiteralPath "C:\Sortemail945Sandbox"))
    $trxPath = Join-Path $results "TASKID.trx"
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath $trxPath))
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    if (-not (Test-Path -LiteralPath $trxPath)) { Write-Output "TRX ABSENT: the run aborted before writing its result document"; exit 3 }
    [xml]$trx = Get-Content -LiteralPath $trxPath -Raw -Encoding UTF8
    $ns = New-Object System.Xml.XmlNamespaceManager($trx.NameTable)
    $ns.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
    $counters = $trx.SelectSingleNode("//t:ResultSummary/t:Counters", $ns)
    Write-Output ("COUNTERS total=" + $counters.GetAttribute("total") + " executed=" + $counters.GetAttribute("executed") + " passed=" + $counters.GetAttribute("passed") + " failed=" + $counters.GetAttribute("failed"))
    $all = @($trx.SelectNodes("//t:UnitTestResult", $ns))
    Write-Output ("RESULT_COUNT: " + $all.Count)
    foreach ($r in $all) { Write-Output ("RESULT " + $r.GetAttribute("testName") + " = " + $r.GetAttribute("outcome")) }
    foreach ($r in $all) { if ($r.GetAttribute("outcome") -eq "Failed") { $m = $r.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $ns); $txt = if ($null -eq $m) { "(no message)" } else { $m.InnerText }; Write-Output ("MESSAGE " + $r.GetAttribute("testName") + " :: " + $txt) } }

`Command:` records `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation <filter> "/ResultsDirectory:coverage\test-results\956\<task-id>" "/Logger:trx;LogFileName=<task-id>.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` with the note `resolved through vswhere`. A `MESSAGE` line is transcribed only after the hygiene substitution.

**CMD-COVERAGE-DIRECT** (the runner's inner collector invocation issued directly; `STAGE` is `baseline` or `final`; `EXCLUSION` substituted per P0-T9):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    foreach ($f in @("coverage\STAGE-956.cobertura.xml", "coverage\STAGE-956.trx", "coverage\STAGE-956.jacoco.xml")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    $canonical = Get-Content -LiteralPath "coverage.config" -Raw -Encoding UTF8
    $derived = ConvertTo-DerivedCoverageSettingsXml -CanonicalSettingsXml $canonical
    $effective = Join-Path $repo "coverage\effective-coverage-956.config"
    Set-Content -LiteralPath $effective -Value $derived -Encoding UTF8 -NoNewline
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
    $rootLen = $repo.TrimEnd([char]92).Length
    $asm = @(Get-ChildItem -Path $repo -Recurse -Filter "*.Test.dll" | Where-Object { $_.FullName -like "*\bin\Debug\*" -and $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\ref\*" -and $_.FullName.Substring($rootLen) -notlike "\.claude\*" } | Select-Object -ExpandProperty FullName)
    $filter = "TestCategory!=LiveOutlook" + "EXCLUSION"
    $output = Join-Path $repo "coverage\STAGE-956.cobertura.xml"
    $settings = Join-Path $repo "scripts\vscode\TaskMaster.cli.runsettings"
    $results = Join-Path $repo "coverage\test-results\956\STAGE"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    $global:LASTEXITCODE = 0
    & dotnet-coverage collect --output $output --output-format cobertura --settings $effective -- $vstest @asm "/Settings:$settings" /InIsolation "/TestCaseFilter:$filter" "/ResultsDirectory:$results" "/Logger:trx;LogFileName=STAGE-956.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-956.collect.log"
    Write-Output ("COLLECT_EXIT_CODE: " + $LASTEXITCODE)
    Write-Output ("ASSEMBLY_COUNT: " + $asm.Count)
    $asm | ForEach-Object { Write-Output ("ASSEMBLY: " + $_.Substring($rootLen)) }
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    if (Test-Path -LiteralPath (Join-Path $results "STAGE-956.trx")) { Copy-Item -LiteralPath (Join-Path $results "STAGE-956.trx") -Destination "coverage\STAGE-956.trx" -Force }
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\STAGE-956.trx"))

`Command:` records `dotnet-coverage collect --output coverage\<stage>-956.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-956.config -- vstest.console.exe <N test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:<filter>" "/ResultsDirectory:coverage\test-results\956\<stage>" "/Logger:trx;LogFileName=<stage>-956.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`. The route is DIRECT (D4): the runner's own functions, explicit test assemblies discovered with the repository-relative `.claude\` exclusion, the runner's derived collector settings, the parallel CLI runsettings and `/InIsolation`.

**CMD-COVERAGE-POST** (post-processes, projects and summarizes one stage with the runner's own functions; `STAGE` substituted; the TRX summary and `FAILED-SET:` print before the floors and the projection):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTest.TrxSummary.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    $summary = Get-TrxRunSummary -TrxContent (Get-Content -LiteralPath "coverage\STAGE-956.trx" -Raw -Encoding UTF8)
    Write-Output "SUMMARY-BEGIN"
    Write-Output (Format-TrxRunSummary -Summary $summary)
    Write-Output "SUMMARY-END"
    Write-Output ("FAILED-SET: " + (@($summary.FailedTestName) -join ", "))
    $doc = Get-Content -LiteralPath "coverage\STAGE-956.cobertura.xml" -Raw -Encoding UTF8
    $doc = ConvertTo-KoverageCoberturaXml -XmlContent $doc -RepoRoot $repo
    Set-Content -LiteralPath "coverage\STAGE-956.cobertura.xml" -Value $doc -Encoding UTF8 -NoNewline
    try { Assert-CoberturaLineCoverageThreshold -CoberturaXml $doc; Write-Output "LINE-FLOOR: MET" } catch { Write-Output ("LINE-FLOOR: NOT MET " + $_.Exception.Message) }
    try { Assert-CoberturaBranchCoverageThreshold -CoberturaXml $doc; Write-Output "BRANCH-FLOOR: MET" } catch { Write-Output ("BRANCH-FLOOR: NOT MET " + $_.Exception.Message) }
    Write-Output (Get-CoberturaFirstPartyCoverageReport -CoberturaXml $doc)
    [xml]$xml = $doc
    $projection = ConvertTo-JacocoPackageProjection -XmlDocument $xml
    Assert-JacocoProjectionReconciliation -XmlDocument $xml -ProjectionXml $projection
    Set-Content -LiteralPath "coverage\STAGE-956.jacoco.xml" -Value $projection -Encoding UTF8
    Write-Output "PROJECTION-BEGIN"
    Write-Output $projection
    Write-Output "PROJECTION-END"

The projection between `PROJECTION-BEGIN` and `PROJECTION-END`, the `First-party coverage:` line and the summary between `SUMMARY-BEGIN` and `SUMMARY-END` are the committed forms; they carry package names, counters and test names only.

**CMD-SORTEMAIL-COMPARE** (P4-T8; Level-1 aggregate of D4 with the three PD-7 exemptions and the in-memory negative control of the coordinator ruling AC15 option (a), plus observational package and repository figures; `STAGE` is `final`; revised 2026-10-01, revision 1.2):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    $b = [string][char]92
    $pattern = "*EmailParsingSorting" + $b + "SortEmail*.cs"
    $tsName = "UtilitiesCS" + $b + "EmailIntelligence" + $b + "EmailParsingSorting" + $b + "SortEmail.TrySaveAttachment.cs"
    $u = @{}
    $maps = @{}
    $d = $null
    foreach ($stage in @("baseline", "STAGE")) { $d = [xml](Get-Content -LiteralPath (Join-Path "coverage" ($stage + "-956.cobertura.xml")) -Raw -Encoding UTF8); $t = 0; $c = 0; foreach ($n in @($d.SelectNodes("//class"))) { $fn = $n.GetAttribute("filename"); if ($fn -like $pattern) { $s = Get-CoberturaClassLineSummary -ClassNode $n; $t += $s.TotalLines; $c += $s.CoveredLines; $maps[$stage + "|" + $fn] = $s.LineMap; Write-Output ("SORTEMAIL-CLASS " + $stage + " " + $fn + " valid=" + $s.TotalLines + " covered=" + $s.CoveredLines + " uncovered=" + ($s.TotalLines - $s.CoveredLines)) } }; $u[$stage] = $t - $c; Write-Output ("SORTEMAIL-AGG " + $stage + " valid=" + $t + " covered=" + $c + " uncovered=" + ($t - $c)) }
    Write-Output ("SORTEMAIL-DIR-CLASSES: " + @($d.SelectNodes("//class") | Where-Object { $_.GetAttribute("filename") -like ("*EmailParsingSorting" + $b + "*") }).Count)
    $src = @(Get-Content -LiteralPath "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs" -Encoding UTF8)
    $exemptLambda = @(for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Contains("System.IO.Directory.CreateDirectory(path)")) { $i + 1 } })
    $exemptElse = @(for ($i = 3; $i -lt $src.Count; $i++) { if ($src[$i].Trim() -ceq "}" -and $src[$i - 1].Trim() -ceq "throw;" -and $src[$i - 2].Trim() -ceq "{" -and $src[$i - 3].Trim() -ceq "else") { $i + 1 } })
    $exemptCatch = @(for ($i = 1; $i -lt ($src.Count - 1); $i++) { if ($src[$i].Trim() -ceq "}" -and ($exemptElse -contains $i) -and $src[$i + 1].TrimStart().StartsWith("catch (System.Exception)")) { $indent = $src[$i].Length - $src[$i].TrimStart().Length; $hdr = ""; for ($j = $i - 1; $j -ge 0; $j--) { if (($src[$j].Length - $src[$j].TrimStart().Length) -eq $indent -and $src[$j].TrimStart().StartsWith("catch (")) { $hdr = $src[$j].Trim(); break } }; if ($hdr -ceq "catch (System.UnauthorizedAccessException e)") { $i + 1 } } })
    $exempt = @(@($exemptLambda) + @($exemptElse) + @($exemptCatch) | Sort-Object -Unique)
    Write-Output ("EXEMPT-LAMBDA-LINES: " + ($exemptLambda -join ","))
    Write-Output ("EXEMPT-LAMBDA-COUNT: " + $exemptLambda.Count)
    Write-Output ("EXEMPT-ELSE-BRACE-LINES: " + ($exemptElse -join ","))
    Write-Output ("EXEMPT-ELSE-BRACE-COUNT: " + $exemptElse.Count)
    Write-Output ("EXEMPT-CATCH-BRACE-LINES: " + ($exemptCatch -join ","))
    Write-Output ("EXEMPT-CATCH-BRACE-COUNT: " + $exemptCatch.Count)
    $tsMap = $maps["STAGE|" + $tsName]
    Write-Output ("TRYSAVE-CLASS-FOUND: " + ($null -ne $tsMap))
    $exemptUncovered = 0
    if ($null -ne $tsMap) { foreach ($ln in $exempt) { if ($tsMap.Contains($ln) -and $tsMap[$ln].Hits -eq 0) { $exemptUncovered++ } } }
    Write-Output ("EXEMPT-LINES: " + ($exempt -join ","))
    Write-Output ("EXEMPT-LINE-COUNT: " + $exempt.Count)
    Write-Output ("EXEMPT-UNCOVERED: " + $exemptUncovered)
    Write-Output ("SORTEMAIL-UNCOVERED-DELTA-RAW: " + ($u["STAGE"] - $u["baseline"]))
    Write-Output ("SORTEMAIL-UNCOVERED-DELTA: " + ($u["STAGE"] - $exemptUncovered - $u["baseline"]))
    $ctrlLine = 0
    if ($null -ne $tsMap) { foreach ($k in @($tsMap.Keys | Sort-Object)) { if ($tsMap[$k].Hits -gt 0 -and -not ($exempt -contains $k)) { $ctrlLine = $k; break } } }
    $ctrlU = 0
    foreach ($mk in @($maps.Keys)) { if ($mk.StartsWith("STAGE|")) { $m = $maps[$mk]; foreach ($k in @($m.Keys)) { if ($m[$k].Hits -eq 0 -or ($mk -ceq ("STAGE|" + $tsName) -and $k -eq $ctrlLine)) { $ctrlU++ } } } }
    $ctrlDelta = $ctrlU - $exemptUncovered - $u["baseline"]
    Write-Output ("CONTROL-LINE: " + $ctrlLine)
    Write-Output ("CONTROL-DELTA: " + $ctrlDelta)
    Write-Output ("CONTROL-VERDICT: " + $(if ($ctrlDelta -gt 0) { "FAIL" } else { "PASS" }))
    $ctrlRaw = ($u["STAGE"] - $u["baseline"]) + 1
    Write-Output ("CONTROL-RAW-DELTA: " + $ctrlRaw)
    Write-Output ("CONTROL-RAW-VERDICT: " + $(if ($ctrlRaw -gt 3) { "FAIL" } else { "PASS" }))
    [xml]$jb = Get-Content -LiteralPath "coverage\baseline-956.jacoco.xml" -Raw -Encoding UTF8
    [xml]$jf = Get-Content -LiteralPath "coverage\STAGE-956.jacoco.xml" -Raw -Encoding UTF8
    $q = [string][char]39
    foreach ($type in @("LINE", "BRANCH")) { $xp = "/report/package[@name=" + $q + "UtilitiesCS" + $q + "]/counter[@type=" + $q + $type + $q + "]"; $bc = $jb.SelectSingleNode($xp); $fc = $jf.SelectSingleNode($xp); if ($null -eq $bc -or $null -eq $fc) { Write-Output ("PACKAGE UtilitiesCS " + $type + " MISSING"); continue }; $bCov = [int]$bc.GetAttribute("covered"); $bVal = $bCov + [int]$bc.GetAttribute("missed"); $fCov = [int]$fc.GetAttribute("covered"); $fVal = $fCov + [int]$fc.GetAttribute("missed"); $bRate = if ($bVal -gt 0) { [math]::Round($bCov / $bVal, 6) } else { 0 }; $fRate = if ($fVal -gt 0) { [math]::Round($fCov / $fVal, 6) } else { 0 }; Write-Output ("PACKAGE UtilitiesCS " + $type + " baseline=" + $bCov + "/" + $bVal + " rate=" + $bRate + " final=" + $fCov + "/" + $fVal + " rate=" + $fRate + " DEFICIT-POINTS=" + [math]::Round(($bRate - $fRate) * 100, 4) + " WITHIN-BAND=" + ([math]::Round(($bRate - $fRate) * 100, 4) -le 0.10)) }
    Write-Output ("BASELINE-FIRST-PARTY: " + [string](Get-CoberturaFirstPartyCoverageReport -CoberturaXml (Get-Content -LiteralPath "coverage\baseline-956.cobertura.xml" -Raw -Encoding UTF8)))
    Write-Output ("FINAL-FIRST-PARTY: " + [string](Get-CoberturaFirstPartyCoverageReport -CoberturaXml (Get-Content -LiteralPath "coverage\STAGE-956.cobertura.xml" -Raw -Encoding UTF8)))

The Helpers script sets `Set-StrictMode -Version Latest`; every variable the payload reads is assigned before use (`$ctrlLine`, `$ctrlU`, `$hdr` and `$indent` are initialized before their loops). The `PACKAGE`, `BASELINE-FIRST-PARTY` and `FINAL-FIRST-PARTY` lines are observations (PD-8). `SORTEMAIL-DIR-CLASSES:` is the positive control of the filename pattern: a value of 0 is `SORTEMAIL FILENAME MATCH UNPROVEN`: stop and report. The three `EXEMPT-*-LINES` sets are derived from the source text of SortEmail.TrySaveAttachment.cs by containing construct (PD-7), and `$exempt` is their sorted union, so the existing `EXEMPT-LINES`, `EXEMPT-LINE-COUNT` and `EXEMPT-UNCOVERED` lines keep their meaning over the union. `CONTROL-VERDICT:` and `CONTROL-RAW-VERDICT:` are the negative controls the coordinator ruling AC15 option (a) requires: the payload takes the lowest-numbered line of the final SortEmail.TrySaveAttachment.cs line map (`$tsMap`, keyed by line number, `.Hits`) with `Hits` greater than 0 that is not in the exempt union (`CONTROL-LINE:`; 0 means no such line and is `AC15 CONTROL LINE ABSENT`: stop), treats it as uncovered, recounts the final aggregate uncovered lines over every final-stage line map in memory and recomputes the adjusted delta (`CONTROL-DELTA:`, which equals `SORTEMAIL-UNCOVERED-DELTA:` plus one because `CoveredLines` is the count of entries with `Hits` greater than 0), and prints `CONTROL-RAW-DELTA:` as the raw delta plus one; the verdicts read `FAIL` when the control adjusted delta exceeds 0 and when the control raw delta exceeds 3. Nothing is written to disk by the control. Predictions from the ITERATION 1 documents, which no later task regenerates (recorded for the reader, not acceptance conditions): `EXEMPT-LINES: 49,154,155`, `EXEMPT-UNCOVERED: 3`, `SORTEMAIL-UNCOVERED-DELTA-RAW: 3`, `SORTEMAIL-UNCOVERED-DELTA: 0`, `CONTROL-LINE: 28`, `CONTROL-DELTA: 1`, `CONTROL-RAW-DELTA: 4`.

**CMD-METHOD-COVERAGE** (P4-T9; line coverage of the five-argument core span and of the session file in the final document):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    $b = [string][char]92
    $d = [xml](Get-Content -LiteralPath "coverage\final-956.cobertura.xml" -Raw -Encoding UTF8)
    $tsName = "UtilitiesCS" + $b + "EmailIntelligence" + $b + "EmailParsingSorting" + $b + "SortEmail.TrySaveAttachment.cs"
    $ssName = "UtilitiesCS" + $b + "Dialogs" + $b + "YesNoToAllPromptSession.cs"
    $tsNodes = @($d.SelectNodes("//class") | Where-Object { $_.GetAttribute("filename") -ceq $tsName })
    $ssNodes = @($d.SelectNodes("//class") | Where-Object { $_.GetAttribute("filename") -ceq $ssName })
    Write-Output ("TRYSAVE-CLASS-NODES: " + $tsNodes.Count)
    Write-Output ("SESSION-CLASS-NODES: " + $ssNodes.Count)
    $src = @(Get-Content -LiteralPath "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs" -Encoding UTF8)
    $starts = @(for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Trim() -ceq "internal static async Task<bool> TrySaveAttachmentAsync(") { $i + 1 } })
    Write-Output ("CORE-START-MATCHES: " + $starts.Count)
    $s0 = $starts[0]
    $end = 0
    for ($i = $s0; $i -lt $src.Count; $i++) { if ($src[$i] -ceq "        }") { $end = $i + 1; break } }
    Write-Output ("CORE-SPAN: " + $s0 + "-" + $end)
    $map = (Get-CoberturaClassLineSummary -ClassNode $tsNodes[0]).LineMap
    $cv = 0
    $cc = 0
    $unc = @()
    foreach ($k in @($map.Keys)) { if ($k -ge $s0 -and $k -le $end) { $cv++; if ($map[$k].Hits -gt 0) { $cc++ } else { $unc += $k } } }
    $cp = if ($cv -gt 0) { [math]::Round(100 * $cc / $cv, 2) } else { 0 }
    Write-Output ("CORE-VALID: " + $cv)
    Write-Output ("CORE-COVERED: " + $cc)
    Write-Output ("CORE-PERCENT: " + $cp)
    Write-Output ("CORE-UNCOVERED-LINES: " + ((@($unc) | Sort-Object) -join ","))
    $sm = Get-CoberturaClassLineSummary -ClassNode $ssNodes[0]
    $sp = if ($sm.TotalLines -gt 0) { [math]::Round(100 * $sm.CoveredLines / $sm.TotalLines, 2) } else { 0 }
    Write-Output ("SESSION-VALID: " + $sm.TotalLines)
    Write-Output ("SESSION-COVERED: " + $sm.CoveredLines)
    Write-Output ("SESSION-PERCENT: " + $sp)

The core span runs from the signature line `internal static async Task<bool> TrySaveAttachmentAsync(` to the first following line that is exactly eight spaces and `}` (the method's closing brace; every brace inside the body is indented further). The post-processed document holds one merged class element per file (Merge-CoberturaClassesByFilename), so the async state machine and the closure lines of the core are included in the TrySave file's line map.

**CMD-FORMAT-REPO** (P4-T1; repository-wide format with a before-and-after tree observation; `TASKID` substituted):

    Set-Location -LiteralPath "WORKTREE"
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $ws = @("UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs", "UtilitiesCS\Dialogs\YesNoToAllPromptSession.cs", "UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs", "UtilitiesCS.Test\Dialogs\YesNoToAllPromptSession_Tests.cs", "UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs")
    $before = @{}
    foreach ($p in $ws) { $before[$p] = (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash }
    $pb = @(git status --porcelain --untracked-files=all)
    $global:LASTEXITCODE = 0
    & dotnet tool run csharpier format . 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.csharpier-format.log"
    Write-Output ("FORMAT_EXIT_CODE: " + $LASTEXITCODE)
    $changed = 0
    foreach ($p in $ws) { if ((Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash -ne $before[$p]) { $changed++; Write-Output ("WRITESET-CHANGED: " + $p) } }
    Write-Output ("WRITESET-CHANGED-COUNT: " + $changed)
    $pa = @(git status --porcelain --untracked-files=all)
    Write-Output ("PORCELAIN-BEFORE-COUNT: " + $pb.Count)
    Write-Output ("PORCELAIN-AFTER-COUNT: " + $pa.Count)
    Write-Output ("PORCELAIN-SAME: " + (($pb -join [string][char]10) -ceq ($pa -join [string][char]10)))

**CMD-CHECK-REPO** (P4-T2; read-only repository-wide verification; `TASKID` substituted):

    Set-Location -LiteralPath "WORKTREE"
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $global:LASTEXITCODE = 0
    & dotnet tool run csharpier check . 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.csharpier-check.log"
    Write-Output ("CHECK_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\TASKID.csharpier-check.log" -Raw -Encoding UTF8
    Write-Output ("CHECKED-LINE: " + [regex]::Match($log, "Checked \d+ files[^\r\n]*").Value)

**CMD-TRX-SUMMARY** (P4-T11; TRX-derived summaries of the two final scoped runs):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTest.TrxSummary.ps1")
    foreach ($task in @("p4-t5", "p4-t6")) { $p = Join-Path "coverage\test-results\956" (Join-Path $task ($task + ".trx")); Write-Output ("SUMMARY-BEGIN " + $task); Write-Output (Format-TrxRunSummary -Summary (Get-TrxRunSummary -TrxContent (Get-Content -LiteralPath $p -Raw -Encoding UTF8))); Write-Output ("SUMMARY-END " + $task) }

**CMD-FOOTPRINT** (P4-T13; union of the anchored tracked diff and the untracked porcelain paths, minus Clause A and Clause B; `MERGE-BASE` and `INHERITED` substituted, `INHERITED` being the quoted `INHERITED-CLAUSE-A:` paths of P0-T3 joined by `, `, or nothing):

    Set-Location -LiteralPath "WORKTREE"
    $tracked = @(git diff --name-only MERGE-BASE)
    $untracked = @(git status --porcelain --untracked-files=all | Where-Object { $_.StartsWith("?? ") } | ForEach-Object { $_.Substring(3) })
    $all = @(@($tracked) + @($untracked) | Sort-Object -Unique)
    $inherited = @(INHERITED)
    $write = @("UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs", "UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs", "UtilitiesCS/UtilitiesCS.csproj", "UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs", "UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs", "UtilitiesCS.Test/UtilitiesCS.Test.csproj")
    $feature = "docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/"
    $rest = @($all | Where-Object { ($inherited -notcontains $_) -and (-not $_.StartsWith(".claude/agent-memory/")) })
    $outside = @($rest | Where-Object { ($write -notcontains $_) -and (-not $_.StartsWith($feature)) })
    $missing = @($write | Where-Object { $all -notcontains $_ })
    Write-Output ("FOOTPRINT-PATHS: " + $all.Count)
    Write-Output ("SUBTRACTED-CLAUSE-A: " + @($all | Where-Object { $inherited -contains $_ }).Count)
    Write-Output ("SUBTRACTED-CLAUSE-B: " + @($all | Where-Object { $_.StartsWith(".claude/agent-memory/") }).Count)
    Write-Output ("OUTSIDE-WRITE-SET: " + $outside.Count)
    $outside | ForEach-Object { Write-Output ("OUTSIDE: " + $_) }
    Write-Output ("WRITE-SET-MISSING: " + $missing.Count)
    $missing | ForEach-Object { Write-Output ("MISSING: " + $_) }
    Write-Output ("RAW-DOC-PATHS: " + @($all | Where-Object { $_ -match "\.(trx|coverage|coveragexml)$" -or $_ -match "cobertura[^/]*\.xml$" }).Count)
    Write-Output ("NUMSTAT-UCS: " + (@(git diff --numstat MERGE-BASE -- UtilitiesCS/UtilitiesCS.csproj) -join " "))
    Write-Output ("NUMSTAT-UCT: " + (@(git diff --numstat MERGE-BASE -- UtilitiesCS.Test/UtilitiesCS.Test.csproj) -join " "))

**CMD-TST-IDENTITY** (P4-T14; `MERGE-BASE` substituted):

    Set-Location -LiteralPath "WORKTREE"
    Write-Output ("TST-HASH-NOW: " + (Get-FileHash -Algorithm SHA256 -LiteralPath "UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs").Hash)
    $global:LASTEXITCODE = 0
    git diff --exit-code MERGE-BASE -- UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs | Out-Null
    Write-Output ("TST-DIFF-EXIT: " + $LASTEXITCODE)
    Write-Output ("TST-PORCELAIN-LINES: " + @(git status --porcelain -- UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs).Count)

**CMD-SWEEP** (P4-T15; host-identifier and raw-document sweep over FEATURE including this plan; the tokens are derived at run time and never written into an artifact):

    Set-Location -LiteralPath "WORKTREE"
    $folder = "docs\features\active\2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956"
    $all = @(Get-ChildItem -LiteralPath $folder -Recurse -File)
    $account = [regex]::Escape($env:USERNAME)
    $profileLeaf = [regex]::Escape((Split-Path -Leaf $env:USERPROFILE))
    $machine = [regex]::Escape($env:COMPUTERNAME)
    $root = [regex]::Escape((Get-Location).Path)
    $b = [regex]::Escape([string][char]92)
    Write-Output ("FILES: " + $all.Count)
    Write-Output ("ACCOUNT-TOKEN-MATCHES: " + @($all | Select-String -Pattern ("(?i)\b" + $account + "\b")).Count)
    Write-Output ("PROFILE-LEAF-MATCHES: " + @($all | Select-String -Pattern ("(?i)\b" + $profileLeaf + "\b")).Count)
    Write-Output ("MACHINE-TOKEN-MATCHES: " + @($all | Select-String -Pattern ("(?i)\b" + $machine + "\b")).Count)
    Write-Output ("WORKTREE-ROOT-MATCHES: " + @($all | Select-String -Pattern ("(?i)" + $root)).Count)
    Write-Output ("USERS-PATH-MATCHES: " + @($all | Select-String -Pattern ("(?i)[a-z]:[" + $b + "/]users[" + $b + "/]")).Count)
    Write-Output ("RAW-DOCUMENT-FILES: " + @($all | Where-Object { $_.Extension -in @(".trx", ".xml", ".coverage", ".coveragexml") }).Count)

**CMD-CONTROL-BACKUP** (P3-T10; byte copy of the fixed TrySave file before the control):

    Set-Location -LiteralPath "WORKTREE"
    $p = "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs"
    $bak = "coverage\control-956\SortEmail.TrySaveAttachment.fixed.bak"
    New-Item -ItemType Directory -Path "coverage\control-956" -Force | Out-Null
    Copy-Item -LiteralPath $p -Destination $bak -Force
    Write-Output ("FIX-HASH-TRYSAVE: " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash)
    Write-Output ("BACKUP-HASH-TRYSAVE: " + (Get-FileHash -Algorithm SHA256 -LiteralPath $bak).Hash)

**CMD-HASH** (`PATH` substituted): `Set-Location -LiteralPath "WORKTREE"` then `Write-Output ("SHA256: " + (Get-FileHash -Algorithm SHA256 -LiteralPath "PATH").Hash)`.

**CMD-RESTORE** (stop-path fallback of PD-4 only):

    Set-Location -LiteralPath "WORKTREE"
    $p = "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs"
    $bak = "coverage\control-956\SortEmail.TrySaveAttachment.fixed.bak"
    Copy-Item -LiteralPath $bak -Destination $p -Force
    Write-Output ("RESTORED-HASH-TRYSAVE: " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash)
    Write-Output ("BACKUP-HASH-NOW: " + (Get-FileHash -Algorithm SHA256 -LiteralPath $bak).Hash)

Test name sets (TRX `testName` values are bare method names). `NAMES-TST` (the fifteen existing SortEmail_Tests methods, UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs lines 42, 55, 80, 105, 141, 158, 175, 183, 210, 246, 273, 292, 317, 342, 360): `InitializeSortToExisting_AlwaysThrows_NotImplementedException`, `InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException`, `SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException`, `SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException`, `StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString`, `StripTabsCrLf_WithPlainText_ReturnsOriginalString`, `Cleanup_Files_DoesNotThrow`, `GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments`, `GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments`, `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile`, `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave`, `SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath`, `SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath`, `SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine`, `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows`. `NAMES-T` is the eleven T method names and `NAMES-S` the seven S method names of the Test inventory.

## Phases

### Phase 0 — Policy Reads, Preconditions, Bootstrap and Baseline Capture

- [x] [P0-T1] Read, in this exact order, CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, .claude/rules/quality-tiers.md, .claude/rules/csharp.md, .claude/rules/tonality.md, .claude/rules/plan-acceptance-gates.md, .claude/skills/policy-compliance-order/SKILL.md, .claude/skills/atomic-plan-contract/SKILL.md, .claude/skills/acceptance-criteria-tracking/SKILL.md, .claude/skills/evidence-and-timestamp-conventions/SKILL.md, FEATURE/issue.md, FEATURE/spec.md, FEATURE/research/2026-10-01T07-00-sort-email-oversized-with-untestable-io-and-dialog-paths-research.md and this plan, and write FEATURE/evidence/baseline/phase0-instructions-read.md with `Timestamp:`, `Policy Order:` (the ordered list above) and one line per file giving its repository-relative path and its integer line count (the last numbered line the Read tool reports). Acceptance: the artifact exists at that exact path and lists all fifteen files, each with an integer line count.

- [x] [P0-T2] Verify the full-bug preconditions read-only (Read, Grep and Glob only) and write FEATURE/evidence/baseline/p0-t2-mode-preconditions.<TS>.md. Acceptance, all five required: FEATURE/issue.md contains the exact line `- Work Mode: full-bug`; FEATURE/spec.md contains the heading line `## Acceptance Criteria`; the regex `^- \[ \] AC([1-9]|1[0-7])\. ` matches exactly 17 lines of FEATURE/spec.md and the regex `^- \[x\] AC` matches 0 lines; FEATURE/user-story.md does not exist; the AC15 line contains the literal `ninety percent` and the literal `System.IO.Directory.CreateDirectory(path)` and the AC12 line contains the literal `fail to compile` (otherwise `AC TEXT MISMATCH`: stop; never edit spec.md here). Record `PRE-EXISTING-EVIDENCE:` as each path already under FEATURE/evidence/ or `NONE` (not a failure). Any other failure is `MODE PRECONDITION FAILED`: stop and report.

- [x] [P0-T3] Record the working context, the diff anchor, the inherited-path set and the pre-implementation gate readiness in FEATURE/evidence/baseline/p0-t3-worktree-context.<TS>.md using `git -C WORKTREE` invocations, one per command, in this order: `git rev-parse --abbrev-ref HEAD` (`BRANCH:`); `git rev-parse HEAD` (`BASE-SHA:`); `git rev-parse origin/main` (`ORIGIN-MAIN-SHA:`, observation); `git merge-base HEAD origin/main` (`MERGE-BASE:`, derived once here and never re-derived); `git merge-base --is-ancestor MERGE-BASE HEAD` (`MERGE-BASE-IS-ANCESTOR-EXIT:`); `git diff --exit-code 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f MERGE-BASE -- UtilitiesCS UtilitiesCS.Test scripts/vscode` (`CITED-TREE-EXIT:`, which proves the plan's line citations, made against the #945 merge, hold at the recorded anchor); `git diff --exit-code MERGE-BASE HEAD -- UtilitiesCS UtilitiesCS.Test` (`BRANCH-SOURCE-EXIT:`); `git diff --name-only MERGE-BASE` together with `git status --porcelain --untracked-files=all` (the union of their paths, taken before this task writes its own artifact, is `INHERITED-CLAUSE-A:`; record also `INHERITED-OUTSIDE-FEATURE:` as every Clause A path outside FEATURE/ and outside .claude/agent-memory/, or `NONE`); `git status --porcelain --untracked-files=all -- UtilitiesCS UtilitiesCS.Test` (`SOURCE-PORCELAIN:`, `EMPTY` when nothing is printed). Then read artifacts/orchestration/orchestrator-state.json with the Read tool (never edit it) and record `CHECKPOINT-ISSUE-NUM:`, `CHECKPOINT-FEATURE-FOLDER:`, `CHECKPOINT-ROUTE:` (`route_id`, else `path_selected`, else `ABSENT`), `CHECKPOINT-LIFECYCLE-READY:` and `PRE-IMPLEMENTATION GATE READY:` (`YES` only when the issue number is `956`, the folder begins docs/features/active/, the route is not `ABSENT` and lifecycle_ready is `true`). Acceptance, all eight required: `BRANCH:` equals bug/sort-email-oversized-with-untestable-io-and-dialog-paths-956 (otherwise `BRANCH MISMATCH`: stop; never create or switch branches); `BASE-SHA:` and `MERGE-BASE:` are 40-character hexadecimal values and `MERGE-BASE-IS-ANCESTOR-EXIT: 0`; `CITED-TREE-EXIT: 0` (otherwise `CITED TREE ADVANCED`: stop and report, because every line citation of this plan would need re-derivation); `BRANCH-SOURCE-EXIT: 0` (otherwise `BRANCH SOURCE NOT AT BASE`: stop); `INHERITED-CLAUSE-A:` lists no Write Set path (otherwise `WRITE SET ALREADY DIRTY`: stop); `SOURCE-PORCELAIN: EMPTY`; `PRE-IMPLEMENTATION GATE READY: YES` (otherwise `PRE-IMPLEMENTATION GATE NOT SEEDED`: stop); the artifact contains no absolute path.

- [x] [P0-T4] Probe the command channel FIRST, then bootstrap the C# toolchain and record pre-edit hashes in FEATURE/evidence/baseline/p0-t4-channel-and-toolchain.<TS>.md. Part 1: run `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; Write-Output ("PROBE-OK " + (Test-Path -LiteralPath "TaskMaster.sln"))'`; `CHANNEL: COMMAND` when `PROBE-OK True` is printed, else `CHANNEL: UNAVAILABLE` with the refusal text (hygiene applied): report `CHANNEL UNAVAILABLE` and stop. Part 2, one payload: record `PRE-EDIT-HASH-SRC:` (UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs), `PRE-EDIT-HASH-TST:` (UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs), `PRE-EDIT-HASH-UCS-CSPROJ:` (UtilitiesCS\UtilitiesCS.csproj), `PRE-EDIT-HASH-UCT-CSPROJ:` (UtilitiesCS.Test\UtilitiesCS.Test.csproj), `PRE-EDIT-HASH-YESNOTOALL:` (UtilitiesCS\Dialogs\YesNoToAll.cs) and `RUNSETTINGS-HASH:` (scripts\vscode\TaskMaster.cli.runsettings) as `Get-FileHash -Algorithm SHA256 -LiteralPath` values, and `RUNSETTINGS-WORKERS-LINE:` and `RUNSETTINGS-SCOPE-LINE:` as lines 5 and 6 of the runsettings file trimmed; then run `& pwsh -NoProfile -File (Join-Path (Get-Location).Path "scripts\vscode\Install-RepoDotNetSdk.ps1")`, record `SDK-MARKER:` (`Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205"`), `dotnet --version`, `dotnet tool restore`, `dotnet tool list --local`, `MSBUILD-RESOLVED:` and `VSTEST-RESOLVED:` (vswhere, `YES` or `NO`) and `dotnet-coverage --version` (when not found, run `dotnet tool install --global dotnet-coverage` and re-run `dotnet-coverage --version` in a separate invocation). Acceptance, all eight required: `CHANNEL: COMMAND`; the six hashes are 64-character hexadecimal values; `RUNSETTINGS-WORKERS-LINE: <Workers>0</Workers>` and `RUNSETTINGS-SCOPE-LINE: <Scope>ClassLevel</Scope>`; `SDK-MARKER: True`; `dotnet --version` and `dotnet tool restore` exit 0; the `csharpier` row of the tool list shows `1.2.6`; `MSBUILD-RESOLVED: YES` and `VSTEST-RESOLVED: YES`; `dotnet-coverage --version` exits 0 with its version recorded.

- [x] [P0-T5] Restore NuGet packages with one payload whose statements after the `Set-Location` are `New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null`, `$env:MSBUILDDISABLENODEREUSE = "1"`, `& pwsh -NoProfile -File (Join-Path (Get-Location).Path "scripts\vscode\Invoke-Restore.ps1") 2>&1 | Tee-Object -FilePath "coverage\logs\p0-t5.restore.log"`, `Write-Output ("RESTORE_EXIT_CODE: " + $LASTEXITCODE)` and `Write-Output ("PACKAGE-DIR-COUNT: " + @(Get-ChildItem -LiteralPath "packages" -Directory).Count)`, and write FEATURE/evidence/baseline/p0-t5-nuget-restore.<TS>.md (`Command:` `pwsh -NoProfile -File scripts\vscode\Invoke-Restore.ps1` with the note that the absolute script path was resolved at run time and MSBUILDDISABLENODEREUSE was 1; `EXIT_CODE:` the printed `RESTORE_EXIT_CODE:`). Acceptance: `EXIT_CODE: 0` and `PACKAGE-DIR-COUNT:` at least 1.

- [x] [P0-T6] Capture the baseline formatting state by running `CMD-CHECK-REPO` with `TASKID` `p0-t6` and write FEATURE/evidence/baseline/p0-t6-csharpier-check.<TS>.md (`Command:` `dotnet tool run csharpier check .`; `EXIT_CODE:` the printed `CHECK_EXIT_CODE:`; `Output Summary:` the `CHECKED-LINE:` value verbatim). Acceptance: `EXIT_CODE: 0` and `CHECKED-LINE:` matches `Checked <N> files` with a positive N. A non-zero exit is `FORMAT BASELINE NOT CLEAN`: stop and report the file list; never run `format` to repair it.

- [x] [P0-T7] Capture the baseline analyzer state with `CMD-REBUILD` (analyzer `GATEARGS`, `TASKID` `p0-t7`) and write FEATURE/evidence/baseline/p0-t7-msbuild-analyzers.<TS>.md with `EXIT_CODE:` (the printed `MSBUILD_EXIT_CODE:`) and `SKIP_CORECOMPILE_LINES:`, `UCS_TEST_CSC_OUT_LINES:`, `UCS_CSC_OUT_LINES:`, `ZERO_ERRORS_LINES:`, `WARNINGS:` (also `ANALYZE-BASELINE-WARNINGS:`), `ERRORS:` and `WRITESET_DIAGNOSTIC_LINES:` (also `ANALYZE-BASELINE-WRITESET-DIAGNOSTICS:`). Acceptance, all four required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; `UCS_TEST_CSC_OUT_LINES:` and `UCS_CSC_OUT_LINES:` each at least 1; `ERRORS: 0`. Otherwise `ANALYZER BASELINE NOT CLEAN`: stop and report.

- [x] [P0-T8] Capture the baseline nullable state with `CMD-REBUILD` (nullable `GATEARGS`, `TASKID` `p0-t8`; no Nullable property override, no incremental Build target) and write FEATURE/evidence/baseline/p0-t8-msbuild-nullable.<TS>.md with the P0-T7 field set (`NULLABLE-BASELINE-WRITESET-DIAGNOSTICS:`) plus `UCS-TEST-DLL-EXISTS:` (whether UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll exists afterwards). Acceptance, all five required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `_CSC_OUT_LINES:` values at least 1; `ERRORS: 0`; `UCS-TEST-DLL-EXISTS: True`. Otherwise `NULLABLE BASELINE NOT CLEAN`: stop and report.

- [x] [P0-T9] Run the stall probe once with `CMD-VSTEST` (`FILTER-STALL`, `TASKID` `p0-t9`) and write FEATURE/evidence/baseline/p0-t9-stall-probe.<TS>.md with `EXIT_CODE:` (the printed `VSTEST_EXIT_CODE:`, or 3 when the TRX is absent), `ExpectedExitCode:` equal to the observed value when it is non-zero (this task gates nothing on the exit code), the four `SANDBOX-` lines, `TRX_PRESENT:`, `SEQUENCE_FILES:`, the `COUNTERS` line when present and every `MESSAGE` line. Record exactly one `STALL-PROBE:` line (`CLEAR` when `EXIT_CODE: 0`, `failed=0` and `SEQUENCE_FILES: 0`; otherwise `REPRODUCES`), exactly one `EXCLUSION:` line (`NONE` under `CLEAR`; the Command Reference exclusion text verbatim under `REPRODUCES`) and exactly one `COVERAGE-ROUTE: DIRECT` line. Acceptance, all four required: `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`; every `SANDBOX-` value is `False`; the three single-value lines are present with the values the rule derives; the probe ran once.

- [x] [P0-T10] Capture the baseline scoped run with `CMD-VSTEST` (`FILTER-SORTEMAIL`, `TASKID` `p0-t10`) against UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll and write FEATURE/evidence/baseline/test-run-baseline.md (fixed name) with `Command:` (runsettings, isolation switch and filter verbatim), `EXIT_CODE:` and an `Output Summary:` with `RUNSETTINGS-HASH-NOW:`, the four `SANDBOX-` lines, the `COUNTERS` line, `RESULT_COUNT:`, `SEQUENCE_FILES:` and every `RESULT` line. Acceptance, all five required: `EXIT_CODE: 0`; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`; `COUNTERS total=15 executed=15 passed=15 failed=0` (another total is `STALE ASSEMBLY OR FILTER MISMATCH`, a failure is `BASELINE NOT GREEN`: stop); the fifteen `RESULT` lines are exactly the `NAMES-TST` names, each `= Passed`; every `SANDBOX-` value is `False` and `SEQUENCE_FILES: 0`.

- [x] [P0-T11] Capture the baseline repository-wide test and coverage run: run `CMD-COVERAGE-DIRECT` (`STAGE` `baseline`, the P0-T9 `EXCLUSION`) as a background invocation polled until its final `TRX_PRESENT:` line, then (unless branch (d0) applies) `CMD-COVERAGE-POST` (`STAGE` `baseline`), and write FEATURE/evidence/baseline/coverage-baseline.md (fixed name) with `Command:` (both payloads named, route `DIRECT`, the filter applied), `EXIT_CODE:` (the printed `COLLECT_EXIT_CODE:`), `ExpectedExitCode:` equal to the observed value when it is non-zero under branch (b), and an `Output Summary:` recording `COVERAGE-ROUTE: DIRECT`, `EXCLUSION:`, `ASSEMBLY_COUNT:` with every `ASSEMBLY:` line, `SEQUENCE_FILES:`, `TRX_PRESENT:`, `LINE-FLOOR:`, `BRANCH-FLOOR:`, the `First-party coverage:` line with `FIRST-PARTY-LINE-PERCENT:` and `FIRST-PARTY-BRANCH-PERCENT:`, the projection verbatim between `PROJECTION-BEGIN` and `PROJECTION-END`, the summary verbatim between `SUMMARY-BEGIN` and `SUMMARY-END`, `FAILED-SET:`, and `BASELINE-UCS-LINE:` and `BASELINE-UCS-BRANCH:` (`<covered>/<covered plus missed>` from the projection). Branches, checked in the order (d0), (c), (b), (a), (d): (d0) `SEQUENCE_FILES:` above 0 or `TRX_PRESENT: False` is `COVERAGE RUN ABORTED`: stop, do not post-process, do not re-run; (c) a `NOT MET` floor is `COVERAGE FLOOR BASELINE NOT MET`: stop; (b) a non-zero exit whose `FAILED-SET:` holds only `TryAddValuesAsync_UpdatesExistingValue` (issue #780, never retried): record and complete; (a) exit 0, both floors met, empty `FAILED-SET:`: complete; (d) anything else is `COVERAGE RUN ABORTED`. Acceptance, all five required: the projection holds a `UtilitiesCS` package with `LINE` and `BRANCH` counters; the `First-party coverage:` line is present with `FIRST-PARTY-LINE-PERCENT:` at least 80 and `FIRST-PARTY-BRANCH-PERCENT:` at least 75; the summary's first line begins `Test run outcome:`; `EXIT_CODE:` equals its declared expectation; the artifact contains no absolute path. coverage\baseline-956.cobertura.xml and coverage\baseline-956.jacoco.xml stay on disk, git-ignored, for P4-T8.

- [x] [P0-T12] Census the unmodified UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs and create the merge-base backup: run `CMD-CENSUS` with `PATHS-SRC` and `TOKENS-SORTEMAIL`, then `CMD-BACKUP-MERGEBASE`, and write FEATURE/evidence/baseline/p0-t12-pre-edit-census.<TS>.md with every `TOKEN ... @ TOTAL` line, `LINES`, `SHA256`, `SRC-HASH:`, `BACKUP-HASH:` and `BACKUP-LINES:`. Acceptance, all four required: every TOTAL equals the PRE column of TOKENS-SORTEMAIL (in particular `[ExcludeFromCodeCoverage]` 28, `YesNoToAll.ShowDialog(` 9, `_removeReadOnly` 13, `TrySaveAttachmentAsync(` 7, `File.Delete(` 5, `File.Exists(` 6); `LINES ... = 1454`; `SRC-HASH:`, `BACKUP-HASH:` and the census `SHA256` all equal `PRE-EDIT-HASH-SRC:`; `BACKUP-LINES: 1454`. A mismatch is `PRE-EDIT CENSUS MISMATCH`: stop and report.

- [x] [P0-T13] Prove the segment partition of coverage\control-956\SortEmail.mergebase.bak with `CMD-PARTITION` and write FEATURE/evidence/baseline/p0-t13-partition.<TS>.md with every printed line. Acceptance, all four required: `BACKUP-LINES: 1454`; `SEGMENT-COUNT: 36`; `SEGMENT-LINES: 1382`; `PARTITION-EXACT: True` and every `SEG-UNIQUE` value 1. Otherwise `SEGMENT TABLE MISMATCH`: stop and report (the Segment Table, not the source, is then wrong).

### Phase 1 — Regression Tests First and Compile-Red Fail-Before

- [x] [P1-T1] Create UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs with the Write tool, content exactly Listing L-TEST-SESSION (four leading spaces stripped); then run `CMD-CENSUS` with `PATHS-TEST-S` and `TOKENS-TEST-S` and write FEATURE/evidence/other/p1-t1-session-tests-census.<TS>.md. Acceptance: every TOTAL equals the TOKENS-TEST-S expectation (7, 1, 1, 9, 1, 4, 1, then nine zeros); the census is whitespace-insensitive, so it holds before and after formatting.

- [x] [P1-T2] Create UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs with the Write tool, content exactly Listing L-TEST-TRYSAVE (four leading spaces stripped); then run `CMD-CENSUS` with `PATHS-TEST-T` and `TOKENS-TEST-T` and write FEATURE/evidence/other/p1-t2-trysave-tests-census.<TS>.md. Acceptance: every TOTAL equals the TOKENS-TEST-T expectation (11, 1, 1, 1, 3, 1, 13, 11, 3, 1, 12, 1, 1, 1, 11, 11, 1, then fifteen zeros).

- [x] [P1-T3] Add the two Compile Include entries to UtilitiesCS.Test/UtilitiesCS.Test.csproj with two Edit calls: replace the line `    <Compile Include="EmailIntelligence\SortEmail_Tests.cs" />` by itself followed by a new line `    <Compile Include="EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs" />`, and replace the line `    <Compile Include="Dialogs\YesNoToAll_Tests.cs" />` by itself followed by a new line `    <Compile Include="Dialogs\YesNoToAllPromptSession_Tests.cs" />` (four-space indent, backslash separators, self-closing, no DependentUpon). Then run `CMD-CSPROJ` and `git diff --numstat MERGE-BASE -- UtilitiesCS.Test/UtilitiesCS.Test.csproj` together with `git status --porcelain -- UtilitiesCS.Test/UtilitiesCS.Test.csproj`, and write FEATURE/evidence/other/p1-t3-test-csproj.<TS>.md. Acceptance, all three required: every UCT line matches the "after P1-T3 / P2-T7" column (COUNT=1 at lines 97, 98, 99, 100, 442, 443, 444 and 445); the numstat line reads `2`, `0` and the path; the porcelain line shows the file modified.

- [x] [P1-T4] Format the two new test files with `CMD-SCOPED-FORMAT` (`PATHS-TESTS`, `TASKID` `p1-t4`) and write FEATURE/evidence/other/p1-t4-scoped-format.<TS>.md with the BEFORE and AFTER hashes, `FORMAT_EXIT_CODE:`, `CHECK_EXIT_CODE:` and the formatter's summary line as an observation. Acceptance: `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`.

- [x] [P1-T5] Census the formatted test files and write FEATURE/evidence/other/p1-t5-test-census.<TS>.md: run `CMD-CENSUS` with `PATHS-TEST-T` and `TOKENS-TEST-T`, and again with `PATHS-TEST-S` and `TOKENS-TEST-S`. Acceptance, all three required: every TOTAL equals its expectation (TOKENS-TEST-T: 11, 1, 1, 1, 3, 1, 13, 11, 3, 1, 12, 1, 1, 1, 11, 11, 1 and then fifteen zeros; TOKENS-TEST-S: 7, 1, 1, 9, 1, 4, 1 and then nine zeros); `LINES` for UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs is at most 499 (predicted about 375; above 499 is `TEST FILE SIZE LIMIT EXCEEDED`: stop and report to the orchestrator; never create a further test file); `LINES` for UtilitiesCS.Test\Dialogs\YesNoToAllPromptSession_Tests.cs is at most 499 (predicted about 181).

- [x] [P1-T6] [expect-fail] Build UtilitiesCS.Test/UtilitiesCS.Test.csproj against production code that is byte-identical to MERGE-BASE with `CMD-BUILD-TEST` (`TASKID` `p1-t6`), and immediately before it run `CMD-HASH` on UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs and `git diff --exit-code MERGE-BASE -- UtilitiesCS` (`PRODUCTION-DIFF-EXIT:`); write the fail-before dossier FEATURE/evidence/regression-testing/fail-before-exception.<TS>.md with `Timestamp:`, `Command:` (the CMD-BUILD-TEST canonical form), `EXIT_CODE:` (the printed `MSBUILD_EXIT_CODE:`), `ExpectedExitCode:` equal to that observed non-zero value, `Output Summary:` (`ERROR_LINES:`, `ERROR_LINES_NEW_TEST_FILES:`, `ERROR_LINES_OTHER_FILES:`, `MISSING_SESSION_TYPE_LINES:`, `CS1501_LINES:`, `ERROR_CODES:`, `DLL_ADVANCED:`, the SRC SHA-256 and `PRODUCTION-DIFF-EXIT:`), `WhyFailingRunImpossible:` (the tests call a five-argument overload and a session type that do not exist before the fix, so they cannot be executed red at run time; reaching the merge-base branch at run time would require the real modal YesNoToAll dialog and a real read-only directory, both prohibited in unit tests) and an `Alternative proof:` section naming the negative control of P3-T10 to P3-T13 as the runtime fail-before equivalent. Acceptance, all six required: `EXIT_CODE:` is non-zero and equals `ExpectedExitCode:`; `ERROR_LINES_NEW_TEST_FILES:` at least 1; `ERROR_LINES_OTHER_FILES: 0` (the red build is caused only by the new tests); `MISSING_SESSION_TYPE_LINES:` at least 1; the SRC SHA-256 equals `PRE-EDIT-HASH-SRC:` and `PRODUCTION-DIFF-EXIT: 0`; `DLL_ADVANCED: False`. A green build is `FAIL-BEFORE NOT OBSERVED`: stop and report.

### Phase 2 — Mechanical Split

All Phase 2 file content comes from UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs, which is still byte-identical to coverage\control-956\SortEmail.mergebase.bak until P2-T6 rewrites it; read the line ranges of the Segment Table with the Read tool and apply the File Assembly Rule exactly.

- [x] [P2-T1] Create UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs with the Write tool by the File Assembly Rule with segments S03, S04, S08, S09, S26; run `CMD-MOVE-CENSUS` (`STATE` `split`, `ONLY` `SortEmail.MailItemSort.cs`) and write FEATURE/evidence/other/p2-t1-mailitemsort-census.<TS>.md. Acceptance: `FILE-EXACT SortEmail.MailItemSort.cs = True`, `FIRST-LINE ... = True` and `LINES ... = 388`.

- [x] [P2-T2] Create UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs with the Write tool by the File Assembly Rule with segments S12, S10, S15, S16, S17, S18, S19, S20, S24, S25, S28, S29; run `CMD-MOVE-CENSUS` (`STATE` `split`, `ONLY` `SortEmail.AttachmentSaving.cs`) and write FEATURE/evidence/other/p2-t2-attachmentsaving-census.<TS>.md. Acceptance: `FILE-EXACT SortEmail.AttachmentSaving.cs = True`, `FIRST-LINE ... = True` and `LINES ... = 342`.

- [x] [P2-T3] Create UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs with the Write tool by the File Assembly Rule with segments S13, S21, S22, S23 (no empty line between S21 and S22); run `CMD-MOVE-CENSUS` (`STATE` `split`, `ONLY` `SortEmail.TrySaveAttachment.cs`) and write FEATURE/evidence/other/p2-t3-trysaveattachment-census.<TS>.md. Acceptance: `FILE-EXACT SortEmail.TrySaveAttachment.cs = True`, `FIRST-LINE ... = True` and `LINES ... = 125`.

- [x] [P2-T4] Create UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs with the Write tool by the File Assembly Rule with segments S14, S30; run `CMD-MOVE-CENSUS` (`STATE` `split`, `ONLY` `SortEmail.LegacyAttachmentSaving.cs`) and write FEATURE/evidence/other/p2-t4-legacyattachmentsaving-census.<TS>.md. Acceptance: `FILE-EXACT SortEmail.LegacyAttachmentSaving.cs = True`, `FIRST-LINE ... = True` and `LINES ... = 240`.

- [x] [P2-T5] Create UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs with the Write tool by the File Assembly Rule with segments S11, S31, S32, S33, S34, S35, S36; run `CMD-MOVE-CENSUS` (`STATE` `split`, `ONLY` `SortEmail.UndoAndMoveLog.cs`) and write FEATURE/evidence/other/p2-t5-undoandmovelog-census.<TS>.md. Acceptance: `FILE-EXACT SortEmail.UndoAndMoveLog.cs = True`, `FIRST-LINE ... = True` and `LINES ... = 195`.

- [x] [P2-T6] Rewrite UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs with the Write tool (after reading it) by the File Assembly Rule with segments S01, S02, S05, S06, S07, S27, which makes line 23 `    public static partial class SortEmail` and drops the three class-level region pairs; run `CMD-MOVE-CENSUS` (`STATE` `split`, `ONLY` `SortEmail.cs`) and write FEATURE/evidence/other/p2-t6-sortemail-census.<TS>.md. Acceptance: `FILE-EXACT SortEmail.cs = True`, `FIRST-LINE ... = True` and `LINES ... = 277`.

- [x] [P2-T7] Add the five SortEmail Compile Include entries to UtilitiesCS/UtilitiesCS.csproj with one Edit: replace the line `    <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.cs" />` by itself followed by these five lines in this order: `    <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs" />`, `    <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs" />`, `    <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs" />`, `    <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs" />`, `    <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs" />`. Then run `CMD-CSPROJ`, `git diff --numstat MERGE-BASE -- UtilitiesCS/UtilitiesCS.csproj` and `git status --porcelain -- UtilitiesCS/UtilitiesCS.csproj`, and write FEATURE/evidence/other/p2-t7-production-csproj.<TS>.md. Acceptance, all three required: every UCS line matches the "after P1-T3 / P2-T7" column (SortEmail.cs 817, the five new entries 818 to 822 in the order above, FolderPredictor.cs 823, YesNoToAllPromptSession.cs COUNT=0); the numstat line reads `5`, `0` and the path; the porcelain line shows the file modified.

- [x] [P2-T8] Format the six SortEmail files under UtilitiesCS/EmailIntelligence/EmailParsingSorting/ with `CMD-SCOPED-FORMAT` (`PATHS-SIX`, `TASKID` `p2-t8`) and write FEATURE/evidence/other/p2-t8-scoped-format.<TS>.md. Acceptance: `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0` (the BEFORE and AFTER hashes are recorded; a hash change from line-ending normalization is expected and is not a failure).

- [x] [P2-T9] Run the verbatim move census over the six files under UtilitiesCS/EmailIntelligence/EmailParsingSorting/: `CMD-MOVE-CENSUS` (`STATE` `split`, `ONLY` `ALL`) and `CMD-CENSUS` (`PATHS-SIX`, `TOKENS-SORTEMAIL`), and write FEATURE/evidence/other/p2-t9-move-census.<TS>.md with every printed line. Acceptance, all four required: `FILE-EXACT` True, `HEADER-PREFIX` True, `CLOSING-BRACES` True and `FIRST-LINE` True for all six files; `SEG S01` to `SEG S36` each `TOTAL=1` in its Segment Table destination and `SEG S10P TOTAL=0`; every TOKENS-SORTEMAIL per-file count and TOTAL equals the SPLIT columns; every `LINES` value is at most 499. A failure is `VERBATIM MOVE NOT PROVEN`: stop and report the failing lines.

- [x] [P2-T10] Rebuild UtilitiesCS/UtilitiesCS.csproj alone with warnings as errors using `CMD-BUILD-PROD` (`TASKID` `p2-t10`) and write FEATURE/evidence/other/p2-t10-production-build.<TS>.md. Acceptance, all four required: `MSBUILD_EXIT_CODE: 0`; `PROD_CSC_OUT_LINES:` at least 1; `ZERO_ERRORS_LINES:` and `ZERO_WARNINGS_LINES:` each at least 1; `CS8632_LINES: 0`, `CS0111_LINES: 0` and `CS0102_LINES: 0` (no missing `#nullable enable`, no duplicated member). The test project is not built in this phase because the new tests need Phase 3.

### Phase 3 — Prompt Seam, Session Type, Green Runs and Negative Control

- [x] [P3-T1] Create UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs with the Write tool, content exactly Listing L-SESSION (four leading spaces stripped); run `CMD-CENSUS` with `PATHS-SESSION` and `TOKENS-SESSION` and write FEATURE/evidence/other/p3-t1-session-census.<TS>.md. Acceptance: the first ten TOKENS-SESSION totals are 1 each and `static`, `ShowDialog` and `ExcludeFromCodeCoverage` are 0 each; `LINES ... = 70`.

- [x] [P3-T2] Add the session Compile Include entry to UtilitiesCS/UtilitiesCS.csproj with one Edit: replace the line `    <Compile Include="Dialogs\YesNoToAll.cs" />` by itself followed by a new line `    <Compile Include="Dialogs\YesNoToAllPromptSession.cs" />`. Then run `CMD-CSPROJ`, `git diff --numstat MERGE-BASE -- UtilitiesCS/UtilitiesCS.csproj` and `git status --porcelain -- UtilitiesCS/UtilitiesCS.csproj`, and write FEATURE/evidence/other/p3-t2-production-csproj.<TS>.md. Acceptance, all three required: every UCS and UCT line matches the "after P3-T2 (final)" column; the numstat line reads `6`, `0` and the path; the porcelain line shows the file modified.

- [x] [P3-T3] Rewrite UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs with the Write tool (after reading it), content exactly Listing L-TRYSAVE (four leading spaces stripped); this one write removes the `_removeReadOnly` field, adds the `RemoveReadOnlyPrompt` field, inserts the wrapper justification comment above the verbatim S21 and S22, makes the three-argument overload the non-async forward without the attribute, adds the five-argument core and adds `ClearReadOnlyAttributeOnDisk` with its justification comment (PD-6). Run `CMD-CENSUS` with `PATHS-TRYSAVE` and `TOKENS-TRYSAVE` and write FEATURE/evidence/other/p3-t3-trysave-census.<TS>.md. Acceptance: every TOKENS-TRYSAVE total equals the SEAM column (A1 to A15, A18, A20 to A23 as listed; A16 3; A17 2; A19 0; A24 0) and `LINES ... = 172`.

- [x] [P3-T4] Change `Cleanup_Files` in UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs with one Edit: replace the line `            _removeReadOnly = YesNoToAllResponse.Empty;` (the only `_removeReadOnly` occurrence in that file) with `            RemoveReadOnlyPrompt.Reset();`. Run `CMD-MOVE-CENSUS` (`STATE` `seam`, `ONLY` `SortEmail.AttachmentSaving.cs`) and write FEATURE/evidence/other/p3-t4-cleanup-files.<TS>.md. Acceptance: `FILE-EXACT SortEmail.AttachmentSaving.cs = True` in the seam state (the file differs from its Phase 2 content only by that one statement; the other three statements of `Cleanup_Files`, including the absent `_attachmentsAltName` reset of L3, are unchanged).

- [x] [P3-T5] Format the nine changed or new `.cs` files under UtilitiesCS/ and UtilitiesCS.Test/ with `CMD-SCOPED-FORMAT` (`PATHS-NINE`, `TASKID` `p3-t5`) and write FEATURE/evidence/other/p3-t5-scoped-format.<TS>.md. Acceptance: `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`.

- [x] [P3-T6] Run the post-edit census and write FEATURE/evidence/other/p3-t6-post-edit-census.<TS>.md with every printed line: `CMD-MOVE-CENSUS` (`STATE` `seam`, `ONLY` `ALL`); `CMD-CENSUS` with `PATHS-SIX` and `TOKENS-SORTEMAIL`; with `PATHS-TRYSAVE` and `TOKENS-TRYSAVE`; with `PATHS-SESSION` and `TOKENS-SESSION`; with `PATHS-TEST-T` and `TOKENS-TEST-T`; with `PATHS-TEST-S` and `TOKENS-TEST-S`; and `CMD-CSPROJ`. Acceptance, all five required: the seam-state expectations of `CMD-MOVE-CENSUS` hold (five `FILE-EXACT` True, TrySave `NOT-APPLICABLE`, every `HEADER-PREFIX`, `CLOSING-BRACES` and `FIRST-LINE` True, S10, S13 and S23 TOTAL=0, S10P TOTAL=1, every other segment TOTAL=1 in its destination); every TOKENS-SORTEMAIL per-file count and TOTAL equals the SEAM columns; TOKENS-TRYSAVE equals the SEAM column, TOKENS-SESSION, TOKENS-TEST-T and TOKENS-TEST-S equal their expectations; every UCS and UCT line matches the final column; every `LINES` value of the nine files is at most 499. A failure is `POST-EDIT CENSUS MISMATCH`: stop and report.

- [x] [P3-T7] Build UtilitiesCS.Test/UtilitiesCS.Test.csproj green with `CMD-BUILD-TEST` (`TASKID` `p3-t7`) and write FEATURE/evidence/other/p3-t7-test-build.<TS>.md. Acceptance, all four required: `MSBUILD_EXIT_CODE: 0`; `ERROR_LINES: 0`; `CSC_OUT_LINES:` and `ZERO_ERRORS_LINES:` each at least 1; `DLL_ADVANCED: True`.

- [x] [P3-T8] Run the scoped SortEmail filter with `CMD-VSTEST` (`FILTER-SORTEMAIL`, `TASKID` `p3-t8`) against UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll and write FEATURE/evidence/regression-testing/p3-t8-scoped-run.<TS>.md with the `CMD-VSTEST` field set and every `RESULT` line. Acceptance, all five required: `EXIT_CODE: 0`; `COUNTERS total=26 executed=26 passed=26 failed=0`; the 26 `RESULT` lines are exactly the `NAMES-TST` and `NAMES-T` names, each `= Passed`; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`; every `SANDBOX-` value is `False` and `SEQUENCE_FILES: 0`. A failing test is reported with its `MESSAGE` line and stops the run (`SCOPED RUN NOT GREEN`); no test is edited or retried.

- [x] [P3-T9] Run the session filter with `CMD-VSTEST` (`FILTER-SESSION`, `TASKID` `p3-t9`) against UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll and write FEATURE/evidence/regression-testing/p3-t9-session-run.<TS>.md. Acceptance, all four required: `EXIT_CODE: 0`; `COUNTERS total=7 executed=7 passed=7 failed=0`; the seven `RESULT` lines are exactly the `NAMES-S` names, each `= Passed`; every `SANDBOX-` value is `False`.

- [x] [P3-T10] Apply the negative control to UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs: run `CMD-CONTROL-BACKUP`, then one Edit replacing the text `clearReadOnly(directory);` (one occurrence) with `// NEGATIVE-CONTROL-956`, then `CMD-CENSUS` with `PATHS-TRYSAVE` and `TOKENS-TRYSAVE`, and write FEATURE/evidence/regression-testing/p3-t10-control-applied.<TS>.md with `FIX-HASH-TRYSAVE:`, `BACKUP-HASH-TRYSAVE:`, every TOKENS-TRYSAVE total and the census `SHA256`. Acceptance, all three required: `BACKUP-HASH-TRYSAVE:` equals `FIX-HASH-TRYSAVE:`; every TOKENS-TRYSAVE total equals the CONTROL column (A13 0, A18 0, A19 1, all others as in SEAM); the census `SHA256` differs from `FIX-HASH-TRYSAVE:`. If the run stops at any point before P3-T12 completes, the executor's last action is the P3-T12 restore and the stop report states the restored hash.

- [x] [P3-T11] [expect-fail] Build and run the controlled state: `CMD-BUILD-TEST` (`TASKID` `p3-t11`) then `CMD-VSTEST` (`FILTER-TRYSAVE`, `TASKID` `p3-t11`), and write FEATURE/evidence/regression-testing/negative-control-clearreadonly-removed.md with `Timestamp:`, `Command:` (both commands), `EXIT_CODE:` scoped to the vstest invocation (the printed `VSTEST_EXIT_CODE:`), `ExpectedExitCode:` equal to that observed non-zero value, and an `Output Summary:` with `MSBUILD_EXIT_CODE:`, `ERROR_LINES:`, the `COUNTERS` line, every `RESULT` line, every `MESSAGE` line (hygiene applied), the four `SANDBOX-` lines, and `CONTROL-OUTCOME: MATCHES PREDICTION` or `CONTROL-OUTCOME: MISMATCH`. Acceptance, all five required: `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0`; `COUNTERS total=11 executed=11 passed=5 failed=6`; the `Failed` results are exactly T2, T3, T4, T8, T9 and T11 and the `Passed` results exactly T1, T5, T6, T7 and T10 (Control Prediction, by method name); `EXIT_CODE:` is non-zero and equals `ExpectedExitCode:`; every `SANDBOX-` value is `False`. Any other outcome is `MUTATION PREDICTION MISMATCH`: run P3-T12, then stop and report.

- [x] [P3-T12] Restore UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs with one Edit replacing `// NEGATIVE-CONTROL-956` with `clearReadOnly(directory);`, then run `CMD-HASH` on that file and record `RESTORED-HASH-TRYSAVE:` and `RESTORE-ROUTE: EDIT`; only when that hash differs from `FIX-HASH-TRYSAVE:`, run `CMD-RESTORE` and record its `RESTORED-HASH-TRYSAVE:` with `RESTORE-ROUTE: COPY`. Then run `CMD-CENSUS` with `PATHS-TRYSAVE` and `TOKENS-TRYSAVE` and write FEATURE/evidence/regression-testing/p3-t12-control-restored.<TS>.md. Acceptance, all two required: the final `RESTORED-HASH-TRYSAVE:` equals `FIX-HASH-TRYSAVE:` (otherwise `CONTROL RESTORE MISMATCH`: stop and report); every TOKENS-TRYSAVE total equals the SEAM column.

- [x] [P3-T13] Confirm the restored state: `CMD-BUILD-TEST` (`TASKID` `p3-t13`) then `CMD-VSTEST` (`FILTER-TRYSAVE`, `TASKID` `p3-t13`), and write FEATURE/evidence/regression-testing/p3-t13-post-restore-run.<TS>.md. Acceptance, all four required: `MSBUILD_EXIT_CODE: 0` and `DLL_ADVANCED: True`; `EXIT_CODE: 0`; `COUNTERS total=11 executed=11 passed=11 failed=0` with the eleven `NAMES-T` results `= Passed`; every `SANDBOX-` value is `False`.

### Phase 4 — Final QC Loop, Coverage Comparison, Footprint and Acceptance Check-Off

Loop rule. P4-T1 to P4-T7 form one toolchain pass in CLAUDE.md order (format, check, analyzer rebuild, nullable rebuild, tests with coverage). Every Phase 4 artifact carries `ITERATION:` (1 on the first pass). When P4-T1 reports `WRITESET-CHANGED-COUNT:` above 0 (the formatter changed a Write Set file), the pass restarts at P4-T1 with `ITERATION:` incremented and every later Phase 4 artifact is re-written for the new iteration; at most three iterations run, and a fourth is `TOOLCHAIN LOOP NOT CONVERGING`: stop and report. Any other failure in P4-T1 to P4-T7 is a defect this plan did not predict: stop and report it with the failing output; the executor does not repair source, tests or gates.

- [x] [P4-T1] Run the repository-wide formatter with `CMD-FORMAT-REPO` (`TASKID` `p4-t1`; canonical command `dotnet tool run csharpier format .`) and write FEATURE/evidence/qa-gates/p4-t1-csharpier-format.<TS>.md with `FORMAT_EXIT_CODE:`, every `WRITESET-CHANGED:` line, `WRITESET-CHANGED-COUNT:`, both porcelain counts, `PORCELAIN-SAME:` and the formatter's summary line as an observation. Acceptance, all three required: `FORMAT_EXIT_CODE: 0`; `WRITESET-CHANGED-COUNT: 0` (the ten Write Set and SortEmail_Tests `.cs` hashes are identical before and after; a non-zero value triggers the loop rule); `PORCELAIN-SAME: True` (no file outside the Write Set changed; `False` is `FORMAT TOUCHED OUT-OF-SET FILE`: stop and report, do not revert).

- [x] [P4-T2] Verify formatting read-only with `CMD-CHECK-REPO` (`TASKID` `p4-t2`; canonical command `dotnet tool run csharpier check .`) and write FEATURE/evidence/qa-gates/p4-t2-csharpier-check.<TS>.md. Acceptance: `CHECK_EXIT_CODE: 0` recorded as `EXIT_CODE: 0`, and `CHECKED-LINE:` matches `Checked <N> files` with a positive N.

- [x] [P4-T3] Run the analyzer gate with `CMD-REBUILD` (analyzer `GATEARGS`, `TASKID` `p4-t3`) over TaskMaster.sln and write FEATURE/evidence/qa-gates/p4-t3-msbuild-analyzers.<TS>.md with the P0-T7 field set. Acceptance, all five required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; `UCS_TEST_CSC_OUT_LINES:` and `UCS_CSC_OUT_LINES:` each at least 1 (CoreCompile ran for both projects); `ERRORS: 0`; `WRITESET_DIAGNOSTIC_LINES:` at most `ANALYZE-BASELINE-WRITESET-DIAGNOSTICS:` of P0-T7.

- [x] [P4-T4] Run the type-check gate with `CMD-REBUILD` (nullable `GATEARGS`, `TASKID` `p4-t4`; no Nullable property override) over TaskMaster.sln and write FEATURE/evidence/qa-gates/p4-t4-msbuild-nullable.<TS>.md with the P0-T7 field set. Acceptance, all five required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `_CSC_OUT_LINES:` values at least 1; `ERRORS: 0`; `WRITESET_DIAGNOSTIC_LINES:` at most `NULLABLE-BASELINE-WRITESET-DIAGNOSTICS:` of P0-T8.

- [x] [P4-T5] Run the final scoped SortEmail filter with `CMD-VSTEST` (`FILTER-SORTEMAIL`, `TASKID` `p4-t5`) against UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll and write FEATURE/evidence/regression-testing/test-run-final.md (fixed name). Acceptance, all five required: `EXIT_CODE: 0`; `COUNTERS total=26 executed=26 passed=26 failed=0`; the 26 `RESULT` lines are exactly `NAMES-TST` and `NAMES-T`, each `= Passed`; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`; every `SANDBOX-` value is `False` and `SEQUENCE_FILES: 0`.

- [x] [P4-T6] Run the final session filter with `CMD-VSTEST` (`FILTER-SESSION`, `TASKID` `p4-t6`) against UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll and write FEATURE/evidence/regression-testing/p4-t6-session-run.<TS>.md. Acceptance, all four required: `EXIT_CODE: 0`; `COUNTERS total=7 executed=7 passed=7 failed=0`; the seven `RESULT` lines are exactly `NAMES-S`, each `= Passed`; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:` and every `SANDBOX-` value is `False`.

- [x] [P4-T7] Run the final repository-wide test and coverage pass: `CMD-COVERAGE-DIRECT` (`STAGE` `final`, the same `EXCLUSION` as P0-T11) as a background invocation polled until its final `TRX_PRESENT:` line, then `CMD-COVERAGE-POST` (`STAGE` `final`), and write FEATURE/evidence/qa-gates/coverage-post-change.md (fixed name) with the P0-T11 field set (`FINAL-UCS-LINE:` and `FINAL-UCS-BRANCH:` in place of the `BASELINE-` figures) and `NEWLY-FAILING:` (the names of `FAILED-SET:` that are absent from coverage-baseline.md's `FAILED-SET:`, or `NONE`). Branches as in P0-T11, with (c) named `COVERAGE FLOOR NOT MET`. Acceptance, all six required: the projection holds a `UtilitiesCS` package with `LINE` and `BRANCH` counters; `FIRST-PARTY-LINE-PERCENT:` at least 80 and `FIRST-PARTY-BRANCH-PERCENT:` at least 75 with `LINE-FLOOR: MET` and `BRANCH-FLOOR: MET`; `FAILED-SET:` contains none of the names of `NAMES-TST`, `NAMES-T` or `NAMES-S`; `NEWLY-FAILING: NONE`, or only `TryAddValuesAsync_UpdatesExistingValue` under branch (b); `EXIT_CODE:` equals its declared expectation; the artifact contains no absolute path. coverage\final-956.cobertura.xml and coverage\final-956.jacoco.xml stay on disk for P4-T8 and P4-T9.

- [x] [P4-T8] Compare coverage at Level 1 with `CMD-SORTEMAIL-COMPARE` (`STAGE` `final`) over coverage\baseline-956.cobertura.xml and coverage\final-956.cobertura.xml (the ITERATION 1 documents, unchanged on disk) and rewrite FEATURE/evidence/qa-gates/coverage-comparison.md (fixed name) with `Timestamp:`, `ITERATION: 1` (the Phase 4 loop did not restart) and `RERUN: revision 1.2, coordinator ruling AC15 option (a)`, `Command:`, `EXIT_CODE:` (the payload's exit code) and an `Output Summary:` holding every printed line, then a `Reading:` paragraph stating the PD-7 rule as revised (three content-identified exemptions, two bounds, negative control), then a section headed `## Coordinator ruling (AC15 option (a))` quoting verbatim the six quoted lines recorded under PD-7, then a section headed `## Prior run (stopped, superseded by the AC15 ruling)` holding the prior ITERATION 1 stop record of this artifact unchanged (its header fields, `Output Summary:`, acceptance evaluation, `STOP: AC15: NOT MET`, the PD-7 reading with the earlier ruling quotation, the diagnostic, findings F-A to F-E and the recorded deviation). Acceptance, all nine required: exactly one `SORTEMAIL-CLASS baseline` row and its filename ends with `SortEmail.cs`; `SORTEMAIL-AGG baseline` `valid=` at least 1 and `SORTEMAIL-AGG final` `valid=` greater than the baseline `valid=` (the forward and the core are now measured; equality is `SORTEMAIL CORE UNMEASURED`: stop); `SORTEMAIL-DIR-CLASSES:` at least 1 and `TRYSAVE-CLASS-FOUND: True`; `EXEMPT-LAMBDA-COUNT: 1`, `EXEMPT-ELSE-BRACE-COUNT: 1`, `EXEMPT-CATCH-BRACE-COUNT: 1` and `EXEMPT-LINE-COUNT: 3`; `SORTEMAIL-UNCOVERED-DELTA:` at most 0; `SORTEMAIL-UNCOVERED-DELTA-RAW:` at most 3; `CONTROL-VERDICT: FAIL`; `CONTROL-RAW-VERDICT: FAIL`; the artifact contains both headed sections named above. A failure of either delta is `AC15: NOT MET`: stop and report with every `SORTEMAIL-CLASS` row; a `PASS` value of either control verdict is `AC15 CHECK NOT DISCRIMINATING`: stop and report. The `PACKAGE` and `FIRST-PARTY` lines are recorded as observations (PD-8). (Revised 2026-10-01, revision 1.2, under the coordinator ruling AC15 option (a).)

- [x] [P4-T9] Measure per-member line coverage with `CMD-METHOD-COVERAGE` over coverage\final-956.cobertura.xml and append a section headed `## Per-member coverage (P4-T9)` with `Timestamp:`, `Command:`, `EXIT_CODE:` and every printed line to FEATURE/evidence/qa-gates/coverage-comparison.md. Acceptance, all five required: `TRYSAVE-CLASS-NODES: 1` and `SESSION-CLASS-NODES: 1`; `CORE-START-MATCHES: 1` and `CORE-SPAN:` start lower than end; `CORE-VALID:` at least 10 and `CORE-PERCENT:` at least 90; `SESSION-VALID:` at least 5 and `SESSION-PERCENT:` at least 90; `CORE-UNCOVERED-LINES:` recorded (predicted to list exactly the `EXEMPT-ELSE-BRACE-LINES` and `EXEMPT-CATCH-BRACE-LINES` values of P4-T8, the two rethrow-following braces inside the core span; the revision 1.1 prediction of an empty value is superseded by the coordinator ruling AC15 option (a); the recorded value is not an acceptance condition). A percentage below 90 is `NEW CODE COVERAGE BELOW 90`: stop and report the uncovered line numbers. (Wording revised 2026-10-01, revision 1.2, under the coordinator ruling AC15 option (a); no threshold changed.)

- [x] [P4-T10] Run the post-format census over UtilitiesCS/EmailIntelligence/EmailParsingSorting/, UtilitiesCS/Dialogs/ and the two test files and write FEATURE/evidence/qa-gates/p4-t10-post-format-census.<TS>.md with every printed line: the P3-T6 command set (`CMD-MOVE-CENSUS` seam `ALL`, the five `CMD-CENSUS` runs, `CMD-CSPROJ`) plus a Glob of the pattern `**/SortEmail*.cs` rooted at the directory UtilitiesCS/EmailIntelligence/EmailParsingSorting (the file names recorded as `SORTEMAIL-FILES:`). Acceptance, all four required: every P3-T6 acceptance condition holds on the post-format state; `SORTEMAIL-FILES:` lists exactly SortEmail.cs, SortEmail.AttachmentSaving.cs, SortEmail.LegacyAttachmentSaving.cs, SortEmail.MailItemSort.cs, SortEmail.TrySaveAttachment.cs and SortEmail.UndoAndMoveLog.cs; every `LINES` value of the nine files is at most 499 and is recorded in a `LINE-COUNTS:` block; the SortEmail.TrySaveAttachment.cs SHA-256 equals the `RESTORED-HASH-TRYSAVE:` of P3-T12 unless P4-T1 changed it under the loop rule.

- [x] [P4-T11] Write the TRX-derived summary FEATURE/evidence/regression-testing/test-results-summary.md from `CMD-TRX-SUMMARY` (TRX documents coverage\test-results\956\p4-t5\p4-t5.trx and coverage\test-results\956\p4-t6\p4-t6.trx) with `Timestamp:`, `Command:`, `EXIT_CODE:` and both summary blocks verbatim. Acceptance, all three required: the p4-t5 block reads `Total 26, executed 26, passed 26, failed 0.`; the p4-t6 block reads `Total 7, executed 7, passed 7, failed 0.`; both blocks state `Failed tests: none`; no trx document is copied into FEATURE/.

- [x] [P4-T12] Close the toolchain loop in FEATURE/evidence/qa-gates/toolchain-pass.md (fixed name): one row per step of the final iteration (P4-T1 format, P4-T2 check, P4-T3 analyzer rebuild, P4-T4 nullable rebuild, P4-T5 and P4-T6 scoped tests, P4-T7 tests with coverage) giving the canonical command, the artifact path, `EXIT_CODE:` and the CoreCompile evidence for the two rebuilds, plus `ITERATION:` and `LOOP-RESTARTS:`. Acceptance, all three required: every row of the final iteration reads `EXIT_CODE: 0`, except P4-T7 which reads 0 or its declared branch (b) expectation; both rebuild rows read `SKIP_CORECOMPILE_LINES: 0` with both `_CSC_OUT_LINES:` at least 1; the P4-T1 row reads `WRITESET-CHANGED-COUNT: 0`.

- [x] [P4-T13] Verify the footprint against the eleven Write Set paths and FEATURE/ with `CMD-FOOTPRINT` (`MERGE-BASE` and `INHERITED` substituted; the payload pairs `git diff --name-only MERGE-BASE` with `git status --porcelain --untracked-files=all`) and write FEATURE/evidence/qa-gates/p4-t13-scope-boundary.<TS>.md with every printed line. Acceptance, all five required: `OUTSIDE-WRITE-SET: 0`; `WRITE-SET-MISSING: 0`; `RAW-DOC-PATHS: 0`; `NUMSTAT-UCS:` reads `6 0 UtilitiesCS/UtilitiesCS.csproj` and `NUMSTAT-UCT:` reads `2 0 UtilitiesCS.Test/UtilitiesCS.Test.csproj` (tab separators may print as whitespace); the artifact records the subtracted Clause A and Clause B counts.

- [x] [P4-T14] Prove UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs byte-identical with `CMD-TST-IDENTITY` (`MERGE-BASE` substituted) and write FEATURE/evidence/qa-gates/p4-t14-tst-identity.<TS>.md. Acceptance, all three required: `TST-HASH-NOW:` equals `PRE-EDIT-HASH-TST:` of P0-T4; `TST-DIFF-EXIT: 0`; `TST-PORCELAIN-LINES: 0`.

- [x] [P4-T15] Sweep FEATURE/ (docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/) for host identifiers and raw documents with `CMD-SWEEP` and write FEATURE/evidence/qa-gates/p4-t15-hygiene-sweep.<TS>.md with the printed counts only (never the tokens). Acceptance: `ACCOUNT-TOKEN-MATCHES: 0`, `PROFILE-LEAF-MATCHES: 0`, `MACHINE-TOKEN-MATCHES: 0`, `WORKTREE-ROOT-MATCHES: 0`, `USERS-PATH-MATCHES: 0` and `RAW-DOCUMENT-FILES: 0`. A non-zero count is repaired by applying the artifact-hygiene substitution to the named artifact and re-running this task; a raw document is removed from FEATURE/ (it remains under coverage/).

- [x] [P4-T16] Check off AC1 in FEATURE/spec.md: precondition, from p4-t10-post-format-census: five `FILE-EXACT` True and TrySave `NOT-APPLICABLE`, all six `HEADER-PREFIX`, `CLOSING-BRACES` and `FIRST-LINE` True, `SORTEMAIL-FILES:` exactly the six files, every SortEmail file `LINES` at most 499, TOKENS-SORTEMAIL `publicstaticpartialclassSortEmail` TOTAL 6 and `publicstaticclassSortEmail` TOTAL 0, and every segment in its D1 destination. Edit: replace `- [ ] AC1. ` with `- [x] AC1. `. Acceptance: a fixed-string Grep of FEATURE/spec.md counts `- [x] AC1. ` once and `- [ ] AC1. ` zero times. If the precondition does not hold, leave the box unchecked and report `AC1 NOT MET`.

- [x] [P4-T17] Check off AC2 in FEATURE/spec.md: precondition: p4-t10 shows the five non-TrySave files `FILE-EXACT` True (every member they hold is verbatim, attributes included), `SEG S21` and `SEG S22` TOTAL=1 (wrapper verbatim), `SEG S13` and `SEG S23` TOTAL=0, TOKENS-TRYSAVE A2 1 (three-argument signature unchanged, no attribute), A3 1, A4 1 and A5 1 (the only additions), TOKENS-SORTEMAIL `_removeReadOnly` TOTAL 0; p4-t13 `OUTSIDE-WRITE-SET: 0` (no caller modified). Edit: replace `- [ ] AC2. ` with `- [x] AC2. `. Acceptance: Grep counts `- [x] AC2. ` once and `- [ ] AC2. ` zero times; otherwise report `AC2 NOT MET`.

- [x] [P4-T18] Check off AC3 in FEATURE/spec.md: precondition: p4-t10 TOKENS-SESSION equals its expectation for UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs (sealed internal class, constructor guard, `Response`, `Ask`, `ReleaseSingleAnswer`, `Reset`); p4-t6-session-run shows `Constructor_WhenShowDialogIsNull_ThrowsArgumentNullException = Passed`. Edit: replace `- [ ] AC3. ` with `- [x] AC3. `. Acceptance: Grep counts `- [x] AC3. ` once and `- [ ] AC3. ` zero times; otherwise report `AC3 NOT MET`.

- [x] [P4-T19] Check off AC4 in FEATURE/spec.md: precondition: p4-t10 TOKENS-TRYSAVE A1 1, A2 1, A3 1, A4 1, A20 2 and A21 2; TOKENS-SORTEMAIL `[ExcludeFromCodeCoverage]` per file SE 4, MIS 5, AS 10, TS 2, LAS 1, UML 6 (TOTAL 28) with the five non-TrySave files `FILE-EXACT` True. Edit: replace `- [ ] AC4. ` with `- [x] AC4. `. Acceptance: Grep counts `- [x] AC4. ` once and `- [ ] AC4. ` zero times; otherwise report `AC4 NOT MET`.

- [x] [P4-T20] Check off AC5 in FEATURE/spec.md: precondition: p4-t10 TOKENS-TRYSAVE A24 0, A5 1 and A6 1; TOKENS-SORTEMAIL `YesNoToAll.ShowDialog(` TOTAL 8 (AS 6, LAS 2, none in UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs), `DirectoryInfo` and `FileAttributes` TOTAL 1 each (TS only), `RemoveReadOnlyPrompt.Reset();` AS 1, `staticAction<` and `staticFunc<` TOTAL 0; TOKENS-SESSION `static` 0. Edit: replace `- [ ] AC5. ` with `- [x] AC5. `. Acceptance: Grep counts `- [x] AC5. ` once and `- [ ] AC5. ` zero times; otherwise report `AC5 NOT MET`.

- [x] [P4-T21] Check off AC6 in FEATURE/spec.md: precondition: p4-t10 TOKENS-TRYSAVE A9 to A15 each 1, A16 3, A17 2 and A23 3 (directory computed before the inner try, clear inside it, boundaries and rethrows unchanged, no catch and no retry bound added); test-run-final.md shows all eleven `NAMES-T` results Passed (the invariant's branches B0 to B9). Edit: replace `- [ ] AC6. ` with `- [x] AC6. `. Acceptance: Grep counts `- [x] AC6. ` once and `- [ ] AC6. ` zero times; otherwise report `AC6 NOT MET`.

- [x] [P4-T22] Check off AC7 in FEATURE/spec.md: precondition: p4-t10 `CMD-CSPROJ` lines match the final column for UtilitiesCS/UtilitiesCS.csproj and UtilitiesCS.Test/UtilitiesCS.Test.csproj; p4-t13 numstat `6 0` and `2 0`. Edit: replace `- [ ] AC7. ` with `- [x] AC7. `. Acceptance: Grep counts `- [x] AC7. ` once and `- [ ] AC7. ` zero times; otherwise report `AC7 NOT MET`.

- [x] [P4-T23] Check off AC8 in FEATURE/spec.md: precondition: test-run-final.md shows the eleven `NAMES-T` results of UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs Passed and p4-t10 TOKENS-TEST-T equals its expectation (the assertions of the Test inventory are those of Listing L-TEST-TRYSAVE). Edit: replace `- [ ] AC8. ` with `- [x] AC8. `. Acceptance: Grep counts `- [x] AC8. ` once and `- [ ] AC8. ` zero times; otherwise report `AC8 NOT MET`.

- [x] [P4-T24] Check off AC9 in FEATURE/spec.md: precondition: p4-t6-session-run shows the seven `NAMES-S` results of UtilitiesCS.Test/Dialogs/YesNoToAllPromptSession_Tests.cs Passed and p4-t10 TOKENS-TEST-S equals its expectation. Edit: replace `- [ ] AC9. ` with `- [x] AC9. `. Acceptance: Grep counts `- [x] AC9. ` once and `- [ ] AC9. ` zero times; otherwise report `AC9 NOT MET`.

- [x] [P4-T25] Check off AC10 in FEATURE/spec.md: precondition: p4-t10 TOKENS-TEST-T zeros for `SortEmail.`, `typeof(`, `RemoveReadOnlyPrompt`, `Cleanup_Files`, `DoNotParallelize`, `[DataRow`, `File.`, `Directory.`, `Path.`, `Thread.Sleep`, `Task.Delay`, `YesNoToAll.ShowDialog`, `GetTemp`, `Xunit` and `NUnit`, `newSeams(` 11 and `newMock<Attachment>(MockBehavior.Loose)` 11 (each test owns its session and mock), `newSeams(YesNoToAllResponse.YesToAll)` 3 (T3, T4 and T9, none of which scripts a repeatedly denied retry, so L2 is not exercised); TOKENS-TEST-S zeros likewise; test-run-final.md and p4-t6-session-run show `RUNSETTINGS-HASH-NOW:` equal to `RUNSETTINGS-HASH:` (Workers 0, Scope ClassLevel, recorded by P0-T4) and every `SANDBOX-` value `False` before and after. Edit: replace `- [ ] AC10. ` with `- [x] AC10. `. Acceptance: Grep counts `- [x] AC10. ` once and `- [ ] AC10. ` zero times; otherwise report `AC10 NOT MET`.

- [x] [P4-T26] Check off AC11 in FEATURE/spec.md: precondition: p4-t14-tst-identity shows UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs hash equal to `PRE-EDIT-HASH-TST:`, `TST-DIFF-EXIT: 0` and `TST-PORCELAIN-LINES: 0`; test-run-final.md shows the fifteen `NAMES-TST` results Passed. Edit: replace `- [ ] AC11. ` with `- [x] AC11. `. Acceptance: Grep counts `- [x] AC11. ` once and `- [ ] AC11. ` zero times; otherwise report `AC11 NOT MET`.

- [x] [P4-T27] Check off AC12 in FEATURE/spec.md: precondition: FEATURE/evidence/regression-testing/fail-before-exception.<TS>.md exists and satisfies every P1-T6 acceptance condition (non-zero build exit equal to its expectation, errors only in the two new test files, the missing session type reported, production byte-identical to MERGE-BASE). Edit: replace `- [ ] AC12. ` with `- [x] AC12. `. Acceptance: Grep counts `- [x] AC12. ` once and `- [ ] AC12. ` zero times; otherwise report `AC12 NOT MET`.

- [x] [P4-T28] Check off AC13 in FEATURE/spec.md: precondition: p4-t10 TOKENS-SORTEMAIL file-system rows (`File.Delete(`, `File.Exists(`, `File.`, `Directory.`, `DirectoryInfo`, `FileAttributes`, `FileIO2.WriteTextFile(`, `newFileInfo(`) per file equal the SEAM column and their TOTALs equal the PRE column (the AC13 reading of the token table maps each to its D5 row), TOKENS-TRYSAVE A6 1 (the DirectoryInfo construction and attribute write only in `ClearReadOnlyAttributeOnDisk`); a fixed-string Grep of FEATURE/spec.md counts `pre-existing, unchanged exclusion` on exactly 3 lines: 126 (J1), 127 (J2) and 269 (the AC13 criterion itself, whose word `exclusions` contains the literal); the J1 and J2 wording is unchanged. Edit: replace `- [ ] AC13. ` with `- [x] AC13. `. Acceptance: Grep counts `- [x] AC13. ` once and `- [ ] AC13. ` zero times; otherwise report `AC13 NOT MET`.

- [x] [P4-T29] Check off AC14 in FEATURE/spec.md: precondition: FEATURE/evidence/qa-gates/toolchain-pass.md satisfies every P4-T12 acceptance condition for one iteration in CLAUDE.md order, with each rebuild showing CoreCompile ran. Edit: replace `- [ ] AC14. ` with `- [x] AC14. `. Acceptance: Grep counts `- [x] AC14. ` once and `- [ ] AC14. ` zero times; otherwise report `AC14 NOT MET`.

- [x] [P4-T30] Check off AC15 in FEATURE/spec.md: precondition: FEATURE/evidence/qa-gates/coverage-comparison.md satisfies every P4-T8 acceptance condition (`EXEMPT-LAMBDA-COUNT: 1`, `EXEMPT-ELSE-BRACE-COUNT: 1`, `EXEMPT-CATCH-BRACE-COUNT: 1` and `EXEMPT-LINE-COUNT: 3`, the three content-identified exemptions AC15 names and no others; `SORTEMAIL-UNCOVERED-DELTA-RAW:` at most 3, which is AC15's unadjusted bound; `SORTEMAIL-UNCOVERED-DELTA:` at most 0 under PD-7 as revised, which is the adjusted count AC15 states; `CONTROL-VERDICT: FAIL` and `CONTROL-RAW-VERDICT: FAIL`, the negative controls the coordinator ruling requires) and every P4-T9 acceptance condition (`CORE-PERCENT:` and `SESSION-PERCENT:` each at least 90). Edit: replace `- [ ] AC15. ` with `- [x] AC15. `. Acceptance: Grep counts `- [x] AC15. ` once and `- [ ] AC15. ` zero times; otherwise report `AC15 NOT MET`. (Revised 2026-10-01, revision 1.2, under the coordinator ruling AC15 option (a).)

- [x] [P4-T31] Check off AC16 in FEATURE/spec.md: precondition: p4-t13 `RAW-DOC-PATHS: 0`; p4-t15 `RAW-DOCUMENT-FILES: 0`; the committed coverage and test evidence is limited to the JaCoCo projections and first-party lines of FEATURE/evidence/baseline/coverage-baseline.md and FEATURE/evidence/qa-gates/coverage-post-change.md and the TRX-derived FEATURE/evidence/regression-testing/test-results-summary.md and run summaries. Edit: replace `- [ ] AC16. ` with `- [x] AC16. `. Acceptance: Grep counts `- [x] AC16. ` once and `- [ ] AC16. ` zero times; otherwise report `AC16 NOT MET`.

- [x] [P4-T32] Check off AC17 in FEATURE/spec.md: precondition: p4-t10 TOKENS-SORTEMAIL rows 24 and 25 (L1) AS 1, row 26 (L4) UML 1, rows 28 and 29 (F2) 1, `YesNoToAll.ShowDialog(` AS 6 (F1 not applied); TOKENS-TRYSAVE A9 1 and A16 3 (L2 retry unbounded); `SEG S10P TOTAL=1` (L3: `Cleanup_Files` differs from merge base only by the one statement); `SEG S28`, `SEG S29`, `SEG S33` and `SEG S36` TOTAL=1 (F3 attributes untouched); a fixed-string Grep of FEATURE/spec.md counts each of `- L1:`, `- L2:`, `- L3:`, `- L4:`, `- F1:`, `- F2:` and `- F3:` once (Rollout & Follow-up lines 290 to 296). Edit: replace `- [ ] AC17. ` with `- [x] AC17. `. Acceptance: Grep counts `- [x] AC17. ` once and `- [ ] AC17. ` zero times; otherwise report `AC17 NOT MET`.

- [x] [P4-T33] Verify the acceptance inventory of FEATURE/spec.md read-only and write FEATURE/evidence/qa-gates/p4-t33-ac-inventory.<TS>.md listing each AC with its check-off task and evidence artifact. Acceptance: the regex `^- \[x\] AC([1-9]|1[0-7])\. ` matches exactly 17 lines of FEATURE/spec.md and the regex `^- \[ \] AC` matches 0 lines; any unchecked AC is reported with its `NOT MET` reason and the plan outcome is INCOMPLETE, never PASS.

## Revision Log

Revision 1.2 (2026-10-01, applied after the P4-T8 ITERATION 1 stop `AC15: NOT MET`; every entry cites the coordinator ruling AC15 option (a), recorded verbatim under PD-7). No checked task, listing or Phase 0 to 3 text was altered; no `.cs` or `.csproj` file was touched; coverage-comparison.md was not edited (P4-T8 rewrites it).

- FEATURE/spec.md line 271 (AC15): the Level-1 clause now names exactly three exemptions, each identified by file and containing construct (the wrapper lambda containing `System.IO.Directory.CreateDirectory(path)`; the closing brace after `throw;` in the final `else` branch inside the `catch (System.UnauthorizedAccessException e)` block; the closing brace of that catch block, before `catch (System.Exception)`), states the adjusted bound "not greater than the baseline count" and "the unadjusted count exceeds the baseline count by at most three", keeps the `ninety percent` clause and the `- [ ] AC15. ` prefix, and ends with the dated note "(Amended 2026-10-01 under the coordinator ruling AC15 option (a).)". Single-line edit: AC1 to AC17 stay at spec.md lines 257 to 273 and no other spec line changed. The planner made this amendment, so the executor's Write Set clause (spec.md text other than the check-off boxes is not edited by the run) stays true.
- Plan header (Last Updated, Status, Version 1.2): revision 1.2 awaiting a confirming preflight.
- PD-7: rewritten for the three content-identified exemptions, the bounds 0 and 3, the negative control, and the verbatim ruling quotation; the condition-(a) reason for keying the catch-brace rule on indentation is recorded.
- Command Reference CMD-SORTEMAIL-COMPARE: the single `$exempt` derivation is replaced by `$exemptLambda`, `$exemptElse`, `$exemptCatch` and their sorted union `$exempt`, with six new `EXEMPT-*` output lines; after `SORTEMAIL-UNCOVERED-DELTA:` the in-memory negative control prints `CONTROL-LINE:`, `CONTROL-DELTA:`, `CONTROL-VERDICT:`, `CONTROL-RAW-DELTA:` and `CONTROL-RAW-VERDICT:`; the prose paragraph after the payload documents the control, the StrictMode initializations and the ITERATION 1 predictions. Command channel rules kept: no single quote, `[char]92` for the backslash, double-quoted literals, no literal ending in a backslash, cmdlets with `-LiteralPath` only.
- P4-T8: acceptance now requires `EXEMPT-LAMBDA-COUNT: 1`, `EXEMPT-ELSE-BRACE-COUNT: 1`, `EXEMPT-CATCH-BRACE-COUNT: 1`, `EXEMPT-LINE-COUNT: 3`, adjusted delta at most 0, raw delta at most 3, `CONTROL-VERDICT: FAIL` and `CONTROL-RAW-VERDICT: FAIL` (a `PASS` is `AC15 CHECK NOT DISCRIMINATING`: stop); the artifact keeps the prior stop record under `## Prior run (stopped, superseded by the AC15 ruling)` and quotes the ruling under `## Coordinator ruling (AC15 option (a))`; dated note appended; task left unchecked.
- P4-T9: wording only; the `CORE-UNCOVERED-LINES:` prediction "empty" is superseded (predicted to equal the two brace exemptions); thresholds unchanged; dated note appended; task left unchecked.
- P4-T30: precondition updated to the three exemption counts, union count 3, raw at most 3, adjusted at most 0, both control verdicts FAIL, plus the unchanged P4-T9 conditions; dated note appended; task left unchecked.
- Planner Self-Review Record: a revision 1.2 enumeration is added before the bounded record; inside the bounded record the FEATURE/spec.md CITATION line is updated to the amended AC15 state, a CITATION for UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs is added, and the AC-MAPPING AC15 line is updated to the new bounds and controls. The completion-pass and revision 1.1 enumerations are left as written (they record earlier states).

## Planner Self-Review Record (completion pass, 2026-10-01)

SELF-REVIEW: RE-DERIVED THIS PASS

Every citation below was re-derived in this pass against WORKTREE with the Read, Grep and Glob tools (no shell was available to the planner, so git-state facts are observed by P0-T3 rather than asserted). Sibling-region checks performed in the same pass are noted per entry.

- UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs: full read of lines 1 to 1454; header 1 to 24; class-level regions 25/620, 622/632, 634/1338; method-internal regions 1136/1146 and 1152/1199; the 36 segment boundaries of the Segment Table (each start line is an attribute, comment, documentation or declaration line and each end line is the member's closing line, with the forty listed blank lines between them); Grep counts of `ExcludeFromCodeCoverage` (28), `YesNoToAll.ShowDialog(` (9 lines), `_removeReadOnly` (13 lines), `TrySaveAttachmentAsync(` (819, 864, 879, 894, 899, 913, 961), `File.` (11), `Directory.` (902), `DirectoryInfo` (944), `FileAttributes` (947), `#region` and `#endregion` (5 each). Sibling check: the wrapper documentation 888 to 892 is adjacent to its attribute 893 (so S21 and S22 are assembled without a blank line), the core documentation 906 to 911 belongs to S23, the commented legacy signature 1018 to 1023 belongs to S26, and the comment 563 belongs to S11.
- UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs: full read (457 lines, 15 test methods, names re-listed as `NAMES-TST`, sandbox constant 238, `System.Action` at 45, 58 and 178, no `#nullable`).
- UtilitiesCS/Dialogs/YesNoToAll.cs: full read (enum 14 to 21, single `ShowDialog` 65, namespace 12, `#nullable enable` 10).
- UtilitiesCS/Properties/AssemblyInfo.cs lines 18 to 20 (Grep).
- UtilitiesCS/UtilitiesCS.csproj lines 1 to 20, 570 to 577 and 812 to 821 (Read) and every `Dialogs\`, `SortEmail`, `EmailDataMiner` and `OlTableExtensions` entry (Grep); siblings 573, 575, 816 and 818 recorded for the position table.
- UtilitiesCS.Test/UtilitiesCS.Test.csproj lines 1 to 20, 94 to 101 and 438 to 444 (Read) and every `SortEmail` and `Dialogs\` entry (Grep); siblings 97, 99, 441 and 443 recorded.
- UtilitiesCS.Test/packages.config lines 9, 65 and 68; UtilitiesCS.Test/OutlookObjects/Item/OutlookItemFlaggableTests.cs lines 195 to 214; UtilitiesCS.Test/Dialogs namespace lines (Grep over the folder).
- Callers across `*.cs`: `TrySaveAttachmentAsync`, `_removeReadOnly`, `Cleanup_Files`, `SortEmail.UndoAsync`, `SortEmail.WriteCSV`, `.SaveAttachmentAsync(` (Grep); QuickFiler/QuickFiler.csproj line 289 and the absence of a QfcController entry; ToDoModel/Email Utilities/SortItemsToExistingFolder.cs line 391 (an unrelated `Cleanup_Files`).
- Identifier uniqueness: no `YesNoToAllPromptSession`, `SortEmail_TrySaveAttachment` or second `class SortEmail_` anywhere in `*.cs` (Grep).
- scripts/vscode/TaskMaster.cli.runsettings (full read); scripts/vscode/Invoke-MSTestWithCoverage.ps1 lines 91, 297, 298 and 459; scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 lines 1 to 470 (full read of the helpers this plan calls); scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 (full read; the overload-collision limitation at 171 to 184 is the basis of PD-7); function declarations of Threshold.ps1, FirstParty.ps1, Projection.ps1 and Invoke-MSTest.TrxSummary.ps1 (Grep, including the `TrxContent` and `Summary` parameters); scripts/vscode/Install-RepoDotNetSdk.ps1 and scripts/vscode/Invoke-Restore.ps1 (Glob).
- Root configuration: global.json (full read), dotnet-tools.json (full read), coverage.config (full read), .csharpierignore (full read), .editorconfig lines 1 to 60 and 655 to 677 plus a Grep for warning-severity entries, .gitignore entries for coverage, trx, cobertura, packages and `.dotnet*` (Grep).
- The four stall-probe classes (Grep for their declarations).
- artifacts/orchestration/orchestrator-state.json (full read).
- .claude/hooks/validate-planner-output.ps1 lines 1 to 389 (phase heading, task line, explicit-path and bounded-record rules applied to this plan: every task line carries a path on its own line, every listing and payload is an indented block with no column-zero `#` line, no `git` span carries a backslash).
- FEATURE/spec.md (full read: AC section 256, AC1 to AC17 at 257 to 273, J1 126, J2 127, L1 to F3 at 290 to 296, Write Set 299 to 313), FEATURE/issue.md (full read), the research document (full read), the #945 plan (Command Reference template lines 1 to 398 and Phase 0 tasks 409 to 429) and the #945 coverage-final.md (full read).
- Draft corrections made in this pass: PD-2 now keeps S21 verbatim (33 verbatim segments in the seam state, not 32); PD-4 now applies the control with the Edit tool and a comment marker; PD-6 consolidates the draft's P3-T3 to P3-T7 because the attribute and documentation anchors are not unique in the Phase 2 file; PD-7 records the closure-filter overload collision that would otherwise make AC15's raw Level-1 delta exceed 0 for a reason unrelated to the change; the draft's P0-T12 token list was extended to the 29 tokens of TOKENS-SORTEMAIL.

Revision pass 1.1 (preflight round 1 delta, 2026-10-01), re-derived in this pass against WORKTREE with the Read and Grep tools, including the orchestrator-amended FEATURE/spec.md:

- FEATURE/spec.md line 271 (AC15, amended): contains `System.IO.Directory.CreateDirectory(path)`, `ninety percent`, "after subtracting at most one uncovered line" and "the unadjusted count exceeds the baseline count by at most one"; AC1 to AC17 still at 257 to 273, AC section heading 256. Sibling check: line 97 also contains `System.IO.Directory.CreateDirectory(path)` (the D5 inventory), which does not affect P0-T2 because P0-T2 reads only the AC15 line; AC12 line 268 still contains `fail to compile`.
- FEATURE/spec.md `pre-existing, unchanged exclusion`: exactly lines 126, 127 and 269 (P4-T28 now counts 3). Sibling check: L1 to F3 still at 290 to 296 and Write Set at 299 to 313 (P4-T32 and the CITATION record unchanged in substance).
- PD-7, P4-T8, P4-T30 and AC-MAPPING AC15: the adjusted delta at most 0, the raw delta at most 1 and `EXEMPT-LINE-COUNT: 1` match the amended AC15 clauses one to one.
- CMD-FOOTPRINT `RAW-DOC-PATHS:` line: the Cobertura arm now requires a `.xml` file name containing `cobertura` in its last path segment, so tracked `.claude/agent-memory/` Markdown files whose names contain `cobertura` no longer match; the payload line contains no single quote and no double-quoted literal ending in a backslash (both literals end in `$`). Sibling check: P4-T13 and P4-T31 still read `RAW-DOC-PATHS: 0`; the other CMD-FOOTPRINT lines are unchanged.

Revision pass 1.2 (coordinator ruling AC15 option (a), 2026-10-01), re-derived in this pass against WORKTREE with the Read and Grep tools:

- UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs (full read, 172 lines): line 49 is the only line containing `System.IO.Directory.CreateDirectory(path)`; lines 151 to 155 trimmed read `else`, `{`, `throw;`, `}`, `}` (indentation 16, 16, 20, 16, 12); line 156 trimmed is `catch (System.Exception)` at indentation 12; lines 157 to 160 trimmed read `{`, `throw;`, `}`, `}` (indentation 12, 16, 12, 8); the `catch (` lines are 101 `catch (System.UnauthorizedAccessException e)` (indentation 12), 125 `catch (System.Exception inner)` (indentation 20) and 156. Rule results: EXEMPT-LAMBDA 49; EXEMPT-ELSE-BRACE 154 only (line 159 fails because line 156 is not `else`); EXEMPT-CATCH-BRACE 155 only (line 159 fails because line 158 is not an else-brace line); the nearest preceding `catch (` line to 155 regardless of indentation is 125, so the rule keys on the brace's indentation (12) and selects 101. Sibling check: line 28 to 30 (`RemoveReadOnlyPrompt` initializer) is the lowest mapped covered line of the file (ITERATION 1 diagnostic), so `CONTROL-LINE: 28` is predicted; TOKENS-TRYSAVE A15 to A17 (plan lines 1066 to 1068) pin `else{throw;}}catch(System.Exception){throw;}}`, three `catch(` and two `throw;`, so the exempted braces cannot be removed under AC6.
- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 `Get-CoberturaClassLineSummary` lines 160 to 258: `LineMap` is a hashtable keyed by `[int]` line number whose values carry `Hits`; `TotalLines` is `LineMap.Count` and `CoveredLines` counts entries with `Hits -gt 0`, so the control's recount of `Hits -eq 0` entries plus the control line equals the aggregate uncovered count plus one.
- FEATURE/evidence/qa-gates/coverage-comparison.md (ITERATION 1 stop record, 2026-10-01T21-26): `EXEMPT-LINES: 49`, `EXEMPT-UNCOVERED: 1`, `SORTEMAIL-UNCOVERED-DELTA-RAW: 3`, `SORTEMAIL-UNCOVERED-DELTA: 2`, baseline uncovered 1, final uncovered 4 (TrySaveAttachment uncovered 3: lines 49, 154, 155; MailItemSort line 153 the moved `ForEachAsync` statement); findings F-A to F-E at lines 57 to 61. Under the revised rule: exempt-uncovered 3, adjusted delta 0, raw delta 3, control adjusted delta 1 and control raw delta 4 (both `FAIL`).
- FEATURE/spec.md line 271 after the amendment: begins `- [ ] AC15. `, contains `System.IO.Directory.CreateDirectory(path)`, `ninety percent`, "not greater than the baseline count", "by at most three" and the dated note; does not contain `pre-existing, unchanged exclusion` (P4-T28 count stays 3 at lines 126, 127 and 269); AC1 to AC17 remain at lines 257 to 273 and `- L1:` to `- F3:` at 290 to 296 (single-line edit, no shift). Sibling check: line 97 (D5 inventory) still contains the lambda literal and is not read by P0-T2.
- Plan CMD-METHOD-COVERAGE (P4-T9) lines: `CORE-START-MATCHES` from the trimmed line `internal static async Task<bool> TrySaveAttachmentAsync(` (source line 87) and the span end at the first `        }` line after it (source line 160), so the span 87 to 160 contains lines 154 and 155; `CORE-UNCOVERED-LINES:` is predicted `154,155` and `CORE-PERCENT:` stays at least 90 (F-E).
- Plan P0-T2 (checked): asserts only `ninety percent` and `System.IO.Directory.CreateDirectory(path)` on the AC15 line, both retained.

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs | 1454 lines; header 1-24; regions 25/620, 622/632, 634/1338, 1136/1146, 1152/1199; segments S01-S36 per Segment Table; ExcludeFromCodeCoverage 28 lines; YesNoToAll.ShowDialog( 710, 733, 773, 796, 853, 856, 936, 1263, 1280; _removeReadOnly 13 lines incl. 560, 628, 932-971; TrySaveAttachmentAsync( 819, 864, 879, 894, 899, 913, 961; File.Delete 245, 369, 515, 1214, 1218; File.Exists 704, 767, 1212, 1216, 1227, 1406; Directory 902; DirectoryInfo 944; FileAttributes 947; FileIO2 1425; Cleanup_Files 555-561; L1 996 and 999
CITATION: UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs | 457 lines; class 33; 15 TestMethod; try-save tests 246 and 273 calling 257 and 281; sandbox constant 238; System.Action 45, 58, 178
CITATION: UtilitiesCS/Dialogs/YesNoToAll.cs | nullable 10; namespace 12; enum 14-21; ShowDialog 65
CITATION: UtilitiesCS/Properties/AssemblyInfo.cs | InternalsVisibleTo 18-20
CITATION: UtilitiesCS/UtilitiesCS.csproj | ToolsVersion 2; LangVersion 10; TargetFrameworkVersion 16; Compile 573, 574, 575, 816, 817, 818
CITATION: UtilitiesCS.Test/UtilitiesCS.Test.csproj | ToolsVersion 2; TargetFrameworkVersion 17; LangVersion 18; Compile 97, 98, 99, 441, 442, 443
CITATION: UtilitiesCS.Test/packages.config | FluentAssertions 9; Moq 65; MSTest.TestFramework 68
CITATION: UtilitiesCS.Test/OutlookObjects/Item/OutlookItemFlaggableTests.cs | SetupSequence Throws Pass 203-206
CITATION: UtilitiesCS.Test/Dialogs/MyBox_Tests.cs | namespace UtilitiesCS.Test.Dialogs 10
CITATION: TaskMaster/Ribbon/RibbonController.cs | UndoAsync caller 230
CITATION: TaskMaster/AppGlobals/AppOlObjects.cs | WriteCSV_StartNewFileIfDoesNotExist caller 301
CITATION: QuickFiler/Controllers/EfcDataModel.cs | Cleanup_Files caller 309
CITATION: QuickFiler/QuickFiler.csproj | EfcDataModel Compile 289; no QfcController entry
CITATION: UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs | SaveAttachmentAsync extension caller 445
CITATION: scripts/vscode/TaskMaster.cli.runsettings | 9 lines; Workers 5; Scope 6
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | filter 91; ConvertTo-DerivedCoverageSettingsXml 97; defaults 297-298; entry guard 459
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | Get-CoberturaClassLineSummary 160; Merge-CoberturaClassesByFilename 260; ConvertTo-KoverageCoberturaXml 407; filter then merge 441-442
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | Get-CoberturaInstrumentedMemberName 134; bare-name overload collision 171-184; Remove-CoberturaExemptClosureCoverage 235
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | Assert-CoberturaLineCoverageThreshold 3; Assert-CoberturaBranchCoverageThreshold 58
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 | Get-CoberturaFirstPartyCoverageReport 123
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1 | ConvertTo-JacocoPackageProjection 14; Assert-JacocoProjectionReconciliation 83
CITATION: scripts/vscode/Invoke-MSTest.TrxSummary.ps1 | Get-TrxRunSummary 12 with TrxContent 44; Format-TrxRunSummary 103 with Summary 128
CITATION: scripts/vscode/Install-RepoDotNetSdk.ps1 | present
CITATION: scripts/vscode/Invoke-Restore.ps1 | present
CITATION: UtilitiesCS.Test/HelperClasses/ShellUtilities_Tests.cs | class 10
CITATION: UtilitiesCS.Test/HelperClasses/ShellUtilitiesStatic_Tests.cs | class 10
CITATION: UtilitiesCS.Test/HelperClasses/SysImageListHelperTests.cs | class 12
CITATION: UtilitiesCS.Test/EmailIntelligence/OSBrowser_Tests.cs | class 27
CITATION: artifacts/orchestration/orchestrator-state.json | route_id 3; issue-num 10; work-mode 11; feature-folder 12; plan-path 13; base_sha 15; lifecycle_ready 16
CITATION: docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md | Acceptance Criteria 256; AC1-AC17 257-273; J1 126; J2 127; AC13 269 (third `pre-existing, unchanged exclusion` line); AC15 271 (amended 2026-10-01 under the coordinator ruling AC15 option (a): three content-identified exemptions including `System.IO.Directory.CreateDirectory(path)`, unadjusted excess at most three, `ninety percent`); L1-F3 290-296; Write Set 299-313
CITATION: UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs | 172 lines; wrapper lambda 49; core signature 87; catch UnauthorizedAccessException 101 (indent 12); inner catch 125 (indent 20); else 151; throw 153; else-brace 154; catch-brace 155; catch System.Exception 156; outer throw 158; outer brace 159; method end 160
CITATION: docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/qa-gates/coverage-comparison.md | ITERATION 1 stop record; Output Summary 8-26; diagnostic 48-52; F-A to F-E 57-61
CITATION: docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/issue.md | Work Mode 12
CITATION: docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/evidence/qa-gates/coverage-final.md | SORTEMAIL-FILE baseline and final 91-92
CITATION: docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/plan.2026-09-30T07-20.md | conventions 44-62; Command Reference 90-304; Phase 0 409-429
CITATION: .claude/hooks/validate-planner-output.ps1 | explicit path 95; bounded record 113-228; phase pattern 238; task pattern 239
CITATION: .claude/rules/plan-acceptance-gates.md | G1-G9 rule table
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11, AC12, AC13, AC14, AC15, AC16, AC17
AC-MAPPING: AC1 | IMPLEMENTATION: P2-T1 to P2-T6 (File Assembly Rule, partial declaration, nullable header) and P3-T3 | TESTS: P0-T13 PARTITION-EXACT; P2-T9 and P4-T10 FILE-EXACT, HEADER-PREFIX, FIRST-LINE, SORTEMAIL-FILES and LINES at most 499 | EVIDENCE: p0-t13-partition, p2-t9-move-census, p4-t10-post-format-census; check-off P4-T16
AC-MAPPING: AC2 | IMPLEMENTATION: verbatim moves P2-T1 to P2-T6; P3-T3 (field replaced, attribute removed from the forward, three additions); P3-T4 | TESTS: P4-T10 five FILE-EXACT, SEG S21 and S22, TOKENS-TRYSAVE A2 to A5, _removeReadOnly 0; P4-T13 OUTSIDE-WRITE-SET 0 | EVIDENCE: p4-t10-post-format-census, p4-t13-scope-boundary; check-off P4-T17
AC-MAPPING: AC3 | IMPLEMENTATION: P3-T1 Listing L-SESSION | TESTS: TOKENS-SESSION in P3-T1 and P4-T10; S1 to S7 in P4-T6 | EVIDENCE: p3-t1-session-census, p4-t10-post-format-census, p4-t6-session-run; check-off P4-T18
AC-MAPPING: AC4 | IMPLEMENTATION: P3-T3 Listing L-TRYSAVE (wrapper and adapter attributes with justification comments, forward and core without) | TESTS: P4-T10 TOKENS-TRYSAVE A1 to A4, A20, A21 and per-file ExcludeFromCodeCoverage counts | EVIDENCE: p4-t10-post-format-census; check-off P4-T19
AC-MAPPING: AC5 | IMPLEMENTATION: P3-T3 (Ask replaces the dialog, adapter holds the attribute write, single initializer) and P3-T4 (Reset) | TESTS: P4-T10 TOKENS-TRYSAVE A5, A6, A24; TOKENS-SORTEMAIL ShowDialog 8, DirectoryInfo 1, FileAttributes 1, Reset 1, staticAction and staticFunc 0; TOKENS-SESSION static 0 | EVIDENCE: p4-t10-post-format-census; check-off P4-T20
AC-MAPPING: AC6 | IMPLEMENTATION: P3-T3 five-argument core of Listing L-TRYSAVE | TESTS: P4-T10 TOKENS-TRYSAVE A9 to A17 and A23; T1 to T11 in P4-T5; negative control P3-T10 to P3-T13 | EVIDENCE: p4-t10-post-format-census, test-run-final.md, negative-control-clearreadonly-removed.md; check-off P4-T21
AC-MAPPING: AC7 | IMPLEMENTATION: P1-T3, P2-T7 and P3-T2 Compile Include edits | TESTS: CMD-CSPROJ position table in P4-T10; numstat 6 0 and 2 0 in P4-T13 | EVIDENCE: p1-t3-test-csproj, p2-t7-production-csproj, p3-t2-production-csproj, p4-t10-post-format-census, p4-t13-scope-boundary; check-off P4-T22
AC-MAPPING: AC8 | IMPLEMENTATION: P1-T2 Listing L-TEST-TRYSAVE | TESTS: P4-T5 eleven NAMES-T Passed; TOKENS-TEST-T in P1-T5 and P4-T10 | EVIDENCE: test-run-final.md, p1-t5-test-census, p4-t10-post-format-census; check-off P4-T23
AC-MAPPING: AC9 | IMPLEMENTATION: P1-T1 Listing L-TEST-SESSION | TESTS: P4-T6 seven NAMES-S Passed; TOKENS-TEST-S in P1-T5 and P4-T10 | EVIDENCE: p4-t6-session-run, p1-t5-test-census, p4-t10-post-format-census; check-off P4-T24
AC-MAPPING: AC10 | IMPLEMENTATION: Listings L-TEST-TRYSAVE and L-TEST-SESSION (own recorder, session and mock per test; no static access; no serialization; no file system) | TESTS: TOKENS-TEST-T and TOKENS-TEST-S zero rows and per-test counts in P4-T10; RUNSETTINGS-HASH-NOW and the four SANDBOX values in P4-T5 and P4-T6 | EVIDENCE: p4-t10-post-format-census, test-run-final.md, p4-t6-session-run, p0-t4-channel-and-toolchain; check-off P4-T25
AC-MAPPING: AC11 | IMPLEMENTATION: no edit of SortEmail_Tests.cs (Write Set boundary) | TESTS: P4-T14 hash, anchored diff exit and porcelain; P4-T5 fifteen NAMES-TST Passed | EVIDENCE: p4-t14-tst-identity, test-run-final.md; check-off P4-T26
AC-MAPPING: AC12 | IMPLEMENTATION: P1-T1 to P1-T3 written before any production edit | TESTS: P1-T6 expect-fail compile-red build with errors only in the new test files | EVIDENCE: regression-testing/fail-before-exception dossier; check-off P4-T27
AC-MAPPING: AC13 | IMPLEMENTATION: verbatim moves plus P3-T3 adapter (the only new file-system wrapper) | TESTS: P4-T10 TOKENS-SORTEMAIL file-system rows per file and totals equal to PRE; TOKENS-TRYSAVE A6; spec wording Grep (3 lines: J1 126, J2 127, AC13 269) | EVIDENCE: p0-t12-pre-edit-census, p4-t10-post-format-census; check-off P4-T28
AC-MAPPING: AC14 | IMPLEMENTATION: Phase 4 loop P4-T1 to P4-T7 in CLAUDE.md order | TESTS: P4-T12 rows with exit codes and CoreCompile evidence | EVIDENCE: toolchain-pass.md, p4-t1 to p4-t4 artifacts, coverage-post-change.md; check-off P4-T29
AC-MAPPING: AC15 | IMPLEMENTATION: T1 to T11 and S1 to S7 cover the core and the session; PD-7 measurement rule (three content-identified exemptions, revised 2026-10-01 under the coordinator ruling AC15 option (a)), stated by the amended spec AC15 | TESTS: P4-T8 EXEMPT-LAMBDA-COUNT 1, EXEMPT-ELSE-BRACE-COUNT 1, EXEMPT-CATCH-BRACE-COUNT 1, EXEMPT-LINE-COUNT 3, SORTEMAIL-UNCOVERED-DELTA at most 0, RAW at most 3, CONTROL-VERDICT FAIL and CONTROL-RAW-VERDICT FAIL; P4-T9 CORE-PERCENT and SESSION-PERCENT at least 90 | EVIDENCE: coverage-baseline.md, coverage-post-change.md, coverage-comparison.md (with the ruling and prior-run sections); check-off P4-T30
AC-MAPPING: AC16 | IMPLEMENTATION: evidence conventions (projections and TRX-derived summaries only; raw documents stay under coverage/) | TESTS: P4-T13 RAW-DOC-PATHS 0; P4-T15 RAW-DOCUMENT-FILES 0 | EVIDENCE: p4-t13-scope-boundary, p4-t15-hygiene-sweep, test-results-summary.md; check-off P4-T31
AC-MAPPING: AC17 | IMPLEMENTATION: no edit of the L1 to L4 and F1 to F3 code (D6) | TESTS: P4-T10 TOKENS-SORTEMAIL rows 24 to 29, ShowDialog AS 6, TOKENS-TRYSAVE A9 and A16, SEG S10P, S28, S29, S33, S36; spec Rollout Grep | EVIDENCE: p4-t10-post-format-census; check-off P4-T32
UNRESOLVED-GAPS: NONE
PREFLIGHT: VALIDATION REQUESTED (DIRECTIVE: PREFLIGHT VALIDATION ONLY through atomic-executor; the planner-side record above is not executor clearance)


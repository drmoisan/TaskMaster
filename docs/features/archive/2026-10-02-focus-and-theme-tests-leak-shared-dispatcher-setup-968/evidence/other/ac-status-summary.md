# Acceptance-criteria status summary (issue #968, task P8-T43)

Timestamp: 2026-10-03T03-36

### Acceptance Criteria Status
- Source: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md (section `## Acceptance Criteria`; work mode full-bug)
- Total AC items: 32
- Checked off (delivered): 31 (lines beginning `- [x] AC`, read from spec.md after P8-T42)
- Remaining (unchecked): 1 (lines beginning `- [ ] AC`)
- Items remaining:
  - AC22: Full toolchain pass: csharpier check, the analyzer rebuild, the warnings-as-errors rebuild and the MSTest coverage route complete in that order with every step passing in one uninterrupted pass after the last edit, the two rebuild logs contain no skipped compile target, and the commands with exit codes are recorded in this feature's qa-gates evidence folder.
    - AC22: NOT MET (ENVIRONMENTAL: COVERAGE-ROUTE DIRECT). Reason: P0-T16 recorded STALL-PROBE: REPRODUCES (the four UtilitiesCS.Test shell-icon classes did not stall, but `ShellUtilities_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension` failed with `Win32 handle that was passed to Icon is not valid or is the wrong type`), so the coverage step ran by the DIRECT route rather than the runner `scripts/vscode/Invoke-MSTestWithCoverage.ps1` verbatim. Every step of the final toolchain pass exited 0 in one iteration (SINGLE-PASS: YES), but RUNNER-GREEN is NO, so FEATURE/evidence/qa-gates/toolchain-final.md reads AC22-STATUS: NOT MET. The decision belongs to the orchestrator (D-6). This criterion is not CI-dependent as worded: it concerns the local runner route.

Check-off records:
- AC1 to AC21 and AC23 to AC32: MET and checked off by tasks P8-T11 to P8-T42, each against the evidence named in the plan's AC identity table.
- AC14 check-off (P8-T24): CLOSES-972-ITEM-5: YES (the `transactionA` try/finally in R4).
- AC23 (P8-T33): AC23-STATUS: MET (first-party lines 85.35% to 85.36%, branches 79.73% to 79.75%; deltas +0.01 and +0.02).
- AC29 (P8-T39): MET (`[ExcludeFromCodeCoverage]` 1; every test-file hit of the removed members is DOC-PROSE or OTHER-TYPE-SAME-NAME; AC23-STATUS: MET).

CI-dependent criteria: none of the remaining items requires a CI result to verify; AC22 is pending an orchestrator ruling on the environmental route.

## Spec check-off diff

Timestamp: 2026-10-03T03-37
Command: git -C WORKTREE diff --numstat HEAD -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md
Canonical command: git -C WORKTREE diff --numstat HEAD -- FEATURE/spec.md; git -C WORKTREE diff HEAD -- FEATURE/spec.md; git -C WORKTREE status --porcelain -- FEATURE/spec.md (separate Bash calls)
EXIT_CODE: 0
Output Summary:
- numstat (exit 0): `31	31	docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md`: added and deleted line counts are equal (31 and 31) and equal the checked-off count (31)
- diff (exit 0): a single hunk `@@ -272,38 +272,38 @@` inside `## Acceptance Criteria`; the 31 deleted lines are `- [ ] AC1:` to `- [ ] AC21:` and `- [ ] AC23:` to `- [ ] AC32:`, each beginning `- [ ] AC`; the 31 added lines are the same criteria beginning `- [x] AC` with the remaining text of each line identical to its deleted counterpart; the `- [ ] AC22:` line is an unchanged context line
- porcelain (exit 0): ` M docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md`
- spec.md changed only in its checkbox lines.

# AC17 evidence: full C# toolchain through the standard route (pull request CI)

Timestamp: 2026-10-02T06-10
Command: gh run view 36971702087 --repo drmoisan/TaskMaster --log --job 110726836511
EXIT_CODE: 0

## Authority

COORDINATOR RULING, AC17 OPTION (a) APPROVED (recorded verbatim from the relaunch directive):

- Rationale: the only local blocker is ShellUtilitiesStatic_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension, which fails deterministically on this workstation ("Win32 handle ... not valid"); it is a known environmental failure that reproduces on main, and CI runs the shell-icon classes. The local fallback run already passed 7361/7361 with coverage not below baseline (lines 85.35 to 85.36, branches 79.74 to 79.75).
- Conditions: (a) AC17 is checked off ONLY from this pull request's own CI run on the FINAL head; record the run ID, the head SHA, the mstest-coverage job pass/fail counts and its coverage figures, and the local fallback result alongside them in the AC17 evidence; (b) add a dated note in spec.md under AC17 citing this coordinator ruling - AC17 intent (full suite through the standard route) is unchanged, only the evidence source changes; (c) if CI fails ANY test, AC17 is NOT met: do not check it off, commit and push, STOP and report.

The dated spec.md note was committed at 523c93ee9.

## Pull request CI run (head at check-off)

- Pull request: 971 (drmoisan/TaskMaster), base main
- Head SHA: d46738a75347fff8ccd63571f424b077399c60cc
- Workflow run ID: 36971702087 (event pull_request), conclusion success
- Jobs, all success: format-check (csharpier verify), build-analyzers, build-nullable, mstest-coverage, pester, actionlint, hygiene
- mstest-coverage job ID: 110726836511 (5m6s)

Output Summary:

- Test Run Successful.
- Total tests: 7384
- Passed: 7384
- Failed: 0 (no Failed line printed; the console prints one only when a test fails)
- First-party coverage: lines 56612/65855 (85.96%), branches 13682/17078 (80.11%)
- The test named in the ruling, GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension, passed in CI (two passing rows, 5 ms and 2 ms).
- Transaction_SecondCallerCannotInstallUntilTheFirstRestores (R4) passed (7 ms); SetThemeDark_FromNormal_SelectsDarkNormalTheme and SetThemeLight_FromNormal_SelectsLightNormalTheme passed. No theme-test null-dispatcher exposure was observed.

## Local fallback result (for comparison)

- Route: Invoke-MSTestWithCoverage with the four shell-icon test classes excluded (environmental failure on this workstation; see evidence/baseline/stall-probe.md and evidence/qa-gates/toolchain-final-pass.md).
- Result: 7361 of 7361 passed.
- Coverage: lines 85.35% (baseline) to 85.36% (post-change); branches 79.74% to 79.75% (evidence/qa-gates/coverage-comparison.md).
- The CI total (7384) exceeds the local total (7361) by 23, consistent with the shell-icon classes running in CI and being excluded locally.

## Final-head confirmation

The commit that adds this file and checks off AC17 creates a new head whose diff against d46738a75 is documentation-only (this file, the AC17 checkbox in spec.md, and an addendum to evidence/other/ac-status-summary.md). The CI run on that head and its mstest-coverage figures are recorded below.

Timestamp: 2026-10-02T06-25
Command: gh run view 36972215199 --repo drmoisan/TaskMaster --log --job 110728374213
EXIT_CODE: 0

- Head SHA: e591b11b72bf1c4682ee43171008935887f8c7e6 (adds this file and the AC17 check-off; no code change against d46738a75)
- Workflow run ID: 36972215199 (event pull_request), conclusion success; all seven jobs success
- mstest-coverage job ID: 110728374213

Output Summary:

- Test Run Successful.
- Total tests: 7384
- Passed: 7384
- Failed: 0 (no Failed line printed)
- First-party coverage: lines 56612/65855 (85.96%), branches 13682/17078 (80.11%)
- GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension passed (two rows); R4 passed; both theme tests passed.

Recording this run creates one further documentation-only commit (this section). That commit cannot name its own run; its CI result is confirmed on the pull request and reported in the orchestration final report. Because every commit after d46738a75 changes only files under this feature folder, the test and coverage figures above apply to the code on the final head unchanged.

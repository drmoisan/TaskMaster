# Initial all-eight test-assembly failure record

- Timestamp (UTC): 2026-07-27T04:19Z
- Task: P8-T66
- Command: the required direct eight-assembly VSTest command with `scripts/vscode/TaskMaster.cli.runsettings`, `/InIsolation`, `TestCategory!=LiveOutlook`, and detailed console logging.
- Result: `EXIT_CODE=1`; 6,056 discovered, 6,055 passed, 1 failed, and no skipped count was printed.
- Failure identity and assertion: unavailable. The console stream was not retained before tool-output truncation.
- File/state changes: none were reported by VSTest. No production, test, project, coverage, settings, filter, exclusion, threshold, or postprocessor file was changed as part of the run.

This is incomplete failure evidence and does not establish determinism or a transient-harness classification. P8-T66 remains unchecked pending two captured direct all-eight passes.

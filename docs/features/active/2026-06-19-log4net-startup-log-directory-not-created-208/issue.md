# log4net-startup-log-directory-not-created (Issue #208)

- Date captured: 2026-06-19
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/log4net-startup-log-directory-not-created/ (Issue #208)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #208
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/208
- Last Updated: 2026-06-19
- Work Mode: minor-audit

## Summary

The log4net file appender cannot create or open its target log file because the configured relative `logs\` directory does not exist at the add-in's runtime working directory. Every log write throws `System.IO.DirectoryNotFoundException` followed by `log4net.Appender.FileAppender.LockingStream.LockStateException`, so file logging is silently non-functional and each log call pays a failed file-append round-trip.

## Environment

- OS/version: Windows; Outlook desktop (`outlook.exe` host)
- Runtime: .NET Framework Outlook VSTO add-in (TaskMaster), STA `VSTA_Main` thread
- Command/flags used: Normal add-in startup; log4net configured via `[assembly: log4net.Config.XmlConfigurator(ConfigFile="log4net.config")]`
- Data source or fixture: `TaskMaster/log4net.config` file appenders targeting a relative `logs\` path

## Steps to Reproduce

1. Launch Outlook with the TaskMaster add-in loaded from the ClickOnce/VSTO deployment location (working directory does not contain a `logs\` subdirectory).
2. Allow any component to emit a log statement during startup.
3. Observe repeated `DirectoryNotFoundException` and `LockStateException` first-chance exceptions on every log write.

## Expected Behavior

The log directory is created if missing (or the appender targets a guaranteed-writable absolute path), file logging succeeds, and no per-write exceptions are raised.

## Actual Behavior

On every log write the following first-chance exceptions are thrown:

```
Exception thrown: 'System.IO.DirectoryNotFoundException' in mscorlib.dll
Exception thrown: 'log4net.Appender.FileAppender.LockingStream.LockStateException' in log4net.dll
```

These recur throughout startup and per-item processing. File logging does not produce output.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: the two exception lines above, observed on essentially every `logger.Debug`/`logger.Info` call in the 2026-06-19 startup capture (issue #207 diagnostic run).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

File-based logging is non-functional, removing the primary post-hoc diagnostic record. The repeated first-chance exceptions also add overhead to every log statement and obscure genuine exceptions during debugging.

## Suspected Cause / Notes

`TaskMaster/log4net.config` file appenders use a relative `logs\` path with no directory-creation step. The VSTO/ClickOnce runtime working directory is not the project directory, so the `logs\` folder does not exist there. log4net's `FileAppender` does not create missing parent directories for this configuration. This is the "F6" finding referenced in the issue #207 diagnostic analysis. Candidate remedies: enable directory creation, set an absolute appender path under a known-writable location (for example `%LOCALAPPDATA%`), or create the directory during add-in initialization before `XmlConfigurator` runs.

Files to inspect:
- `TaskMaster/log4net.config`
- `TaskMaster/ThisAddIn.cs` (assembly-level `XmlConfigurator` attribute / startup wiring)

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: any extracted path-resolution/directory-ensure helper (pure logic, no live appender).
- [ ] Integration scenario to retest: launch with no pre-existing `logs\` directory and confirm a log file is created and no `DirectoryNotFoundException`/`LockStateException` is thrown.
- [ ] Manual verification notes: confirm the chosen log path is writable under the deployed VSTO working directory.

## Acceptance Criteria

- [x] When the configured log directory does not exist at add-in startup, the directory is created before any log4net appender attempts to open a file, and no `System.IO.DirectoryNotFoundException` or `log4net.Appender.FileAppender.LockingStream.LockStateException` is raised on any log call during startup or subsequent item processing.
- [x] The directory-ensure / path-resolution logic is extracted into a small, pure, testable unit (no live log4net appender, no live Outlook/COM dependency) and is covered by MSTest unit tests per repository policy, including: missing-directory (positive), directory-already-exists (edge case), and invalid/unwritable-path error handling.
- [x] All three existing configured appenders (`all_logs_file`, `important_logs_file`, `method_calls_log_file`) continue to write log output without regression once the directory exists.
- [x] No test added for this fix creates or depends on temporary files on the local filesystem; filesystem interaction is isolated behind a seam that can be exercised without a live appender or real Outlook process.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
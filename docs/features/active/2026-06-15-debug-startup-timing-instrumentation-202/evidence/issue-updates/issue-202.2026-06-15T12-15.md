# Issue Update Mirror — #202 Acceptance Criteria Verified

Timestamp: 2026-06-15T12-15

PostedAs: unknown

Note: This is a local mirror of the acceptance-criteria status update produced during plan
execution. It was not posted to GitHub by the executor (no posting step is in the plan; posting
is a downstream orchestration/PR step). If/when posted, update `PostedAs` and the GitHub URL.

## Acceptance Criteria — Verified Status

All five acceptance criteria are verified and checked off in `issue.md`, `spec.md`, and
`user-story.md`:

- [x] AC1 — A flag exists that enables or disables startup timing instrumentation; when disabled
  there is no behavioral or output change to startup.
- [x] AC2 — When enabled, each startup sub-component's elapsed wall-clock time is captured during
  startup.
- [x] AC3 — When enabled, a formatted plain-text table of sub-component names and elapsed times
  (plus a total row) is emitted after startup completes.
- [x] AC4 — The timing recorder/formatter is a testable unit (no Outlook/COM dependency) with
  MSTest coverage meeting the repository floor for new code (100% >= 90%).
- [x] AC5 — Instrumentation uses existing logging/output infrastructure and existing approved
  dependencies; it does not change functional startup behavior.

## Implementation summary

- New user setting `StartupTimingEnabled` (default `False`).
- New internal `IStartupTimingRecorder` with `StartupTimingRecorder` (own ordered span
  collection; reuses `PrettyPrinters.ToFormattedText`; summed TOTAL) and `NullStartupTimingRecorder`.
- `ApplicationGlobals.LoadAsync` reads the flag once, records `LoadBasic` (Stopwatch-measured at
  construction) plus the six sequential phases, and emits one `[Startup timing]` table via the
  log4net logger at the end of startup. No parallel-path instrumentation (out of scope).

## Verification

- Final toolchain (single clean pass): csharpier check (0), analyzer build (0 errors), nullable
  + TreatWarningsAsErrors build (0/0), full test suite 4194/4194 with coverage.
- New-code coverage 100% (>= 90%); repo-wide first-party coverage 75.12% (baseline 75.08%, no
  regression; metric below 80% reflects pre-existing COM/VSTO-exempt denominator per CLAUDE.md).

See: `evidence/qa-gates/final-*.2026-06-15T12-15.md`, `evidence/qa-gates/coverage-delta.2026-06-15T12-15.md`,
`evidence/issue-updates/ac-checkoff.2026-06-15T12-15.md`.

# quickfiler-session-metrics-twelve-hour-time-format-645 (Spec)

- **Issue:** #645
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T08-57
- **Status:** Draft
- **Version:** 0.2

## Write Set
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs`
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs`
- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
- `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`

## Context
- The QuickFiler session-metrics CSV writes its time-of-day field using the .NET custom format
  string `"hh:mm"`. `hh` is the 12-hour-clock specifier and the format string carries no `tt`
  (AM/PM) designator, so an afternoon timestamp such as 14:30 renders as `02:30`, which is
  byte-identical to 02:30 in the small hours. Every row written since the format was introduced
  carries an ambiguous time-of-day value.
- Observed environment(s): Windows 11, Outlook VSTO add-in host (QuickFiler and EFC/QuickFile
  session-metrics writers).
- Customer impact and severity: Medium. The emitted CSV row is silently wrong (not absent), but
  the artifact has no in-repo reader (confirmed: a repository-wide search for `EmailSession`
  returns only settings-plumbing declarations and the three writers, no parser or schema
  consumer). The affected population is any operator who later opens the session-metrics CSV
  outside the repository and needs to read the time-of-day column for a row logged at or after
  13:00 local time.
- First observed date and version(s) impacted: identified 2026-08-27 as cross-feature note CFN-4
  while delivering issues #442, #443, #451 (feature `quickfiler-home-controller-metrics-442`);
  deliberately deferred from that feature because it is a *content* defect (wrong digits) rather
  than a *row-shape* defect (the concern of that feature), and because fixing it would have broken
  three then-passing tests whose asserted literals encode the 12-hour rendering.

## Repro & Evidence
- Steps to reproduce:
  1. Run a QuickFiler filing session, or an EFC move session, whose metrics write occurs at any
     time of day at or after 13:00 local time.
  2. Open the session-metrics CSV that the run appends to.
  3. Read the time-of-day field of the appended row.
- Expected vs actual behavior: Expected — the time-of-day field unambiguously identifies the hour
  on a 24-hour clock (e.g. `14:30`) or a 12-hour clock with an explicit AM/PM designator (e.g.
  `02:30 PM`). Actual — the field renders `02:30` for a 14:30 event, with no AM/PM designator, so
  the recorded time cannot be recovered from the file.
- Logs/screenshots/error snippets: the offending literal is `"hh:mm"` at
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs:48` (interpolated inside `dataLineBeg`),
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs:127` (`curTimeText`), and
  `QuickFiler/Controllers/EfcHomeController.Metrics.cs:96` (`curTimeText`). All three lines and
  literals were directly verified against the current tree by the research pass supporting this
  spec (see docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/research/2026-09-02T08-47-twelve-hour-time-format-research.md,
  §1); a fourth, superficially similar line
  (QfcHomeController.Metrics.cs:46, `//var curTimeText = DateTime.Now.ToString("hh:mm");`) is
  commented-out dead code, not a live site, and is excluded from scope.
- Frequency / determinism: deterministic and always-reproducing for any write at or after 13:00
  local time (and, as a secondary ambiguity, also at exactly 00:00/midnight — see Root Cause
  Analysis). Not data-dependent or intermittent.

## Scope & Non-Goals
- In scope:
  - `QuickFiler/Controllers/QfcHomeController.Metrics.cs:48` — change `now:hh:mm` to `now:HH:mm`
    inside the interpolated `dataLineBeg` assignment.
  - `QuickFiler/Controllers/QfcHomeController.Metrics.cs:127` — change
    `now.ToString("hh:mm")` to `now.ToString("HH:mm")`.
  - `QuickFiler/Controllers/EfcHomeController.Metrics.cs:96` — change
    `currentDateTime.ToString("hh:mm")` to `currentDateTime.ToString("HH:mm")`.
  - `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:243` and `:278` — change
    `expectedLocal.ToString("hh:mm")` to `expectedLocal.ToString("HH:mm")` in
    `expectedDataLineBeg`, so the dynamically computed expected value continues to match the
    fixed production code under the same injected clock.
  - `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:227` and `:265` — update the
    XML doc comments that currently reference `"hh:mm"` so they describe the corrected
    `"HH:mm"` format (comment accuracy only; not required for test correctness).
  - `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs:53` — change the fixed asserted
    literal's time field from `01:05` to `13:05` (the fixture's `MetricsNow`, declared at line 25
    as `new DateTime(2026, 7, 4, 13, 5, 0)`, renders as `13:05` under `HH:mm`, not `01:05`).
- Out of scope / non-goals:
  - Adding `CultureInfo.InvariantCulture` to any of the three format calls above, or to the
    adjacent `curDateText`/`SentDate` calls in the same methods. The issue's proposed-fix note
    frames this as optional ("consider passing"), and the research pass confirmed the row's own
    cited target convention — `SentDate`'s `"HH:mm:ss"` in EfcHomeController.Metrics.cs:118-119
    — itself omits `CultureInfo.InvariantCulture`. Adding it only to the touched sites would make
    the row's culture-handling internally inconsistent rather than more consistent. This gap has
    been promoted separately as GitHub issue #742
    (quickfiler-date-time-format-missing-invariant-culture) and must not be folded into this
    issue's scope.
  - QuickFiler/Legacy/QuickFileController.cs:1013 (`curTimeText =
    DateTime.Now.ToString("hh:mm");`).
  - QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:703
    (`strDeletedDte = QF.Mail.SentOn.ToString(@"mm\dd\yyyy hh:mm");`) and :1307
    (`dataLine = dataLine + "," + QF.Mail.SentOn.ToString("hh:mm");`).
    These three Legacy-namespace sites are named explicitly by the issue as excluded — fixing
    them was judged likely to break other things and is not part of this defect's remit.
  - TaskVisualization/TaskViewer.Designer.cs:387,400 — these already carry the `tt` AM/PM
    designator (`"MM/dd/yyyy hh:mm tt"`), so they are not ambiguous and require no change.
  - The three already-correct, uppercase `HH:mm` sites noted by research as pre-existing and
    unaffected: QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:498,
    QuickFiler/Controllers/QfcCollectionController.cs:1294,2300, and
    QuickFiler/Controllers/EfcItemController.cs:612.
- Explicitly excluded systems, integrations, or datasets:
  - .claude/\*\*, .codex/\*\*, .agents/\*\*, config/blast-radius.json,
    config/orchestration-routing.json — these are push-down files owned by an upstream
    repository and must never be edited from within this feature.
  - Any file under QuickFiler/Legacy/ (see above).
  - The session-metrics CSV's historical/already-written rows are not migrated or rewritten; the
    fix changes only the format applied to rows written after the change (confirmed no in-repo
    reader exists, so there is no backward-compatibility contract to honor for prior rows).

## Root Cause Analysis
- Confirmed root cause: a format-string authoring error. `hh` (12-hour-clock specifier, no AM/PM
  designator) was used where `HH` (24-hour-clock specifier) was intended, at all three sites. The
  adjacent `SentDate` column in the same CSV rows already uses `"HH:mm:ss"`, indicating the
  12-hour spelling was an authoring mistake rather than a deliberate presentation choice.
- Signals/evidence supporting it: direct reads of both production files (see research §1) confirm
  the literal `"hh:mm"` at all three sites, and confirm `SentDate`'s pre-existing `"HH:mm:ss"`
  convention in the same row. The ambiguity is not limited to the afternoon case the issue's
  summary emphasizes: under `"hh:mm"`, both midnight (00:00) and noon (12:00) render identically
  as `12:00`; under `"HH:mm"` they render as `00:00` and `12:00` respectively, resolving the
  ambiguity at both boundaries.
- Affected components/modules (paths):
  - `QuickFiler/Controllers/QfcHomeController.Metrics.cs` (methods
    `QuickFileMetrics_WRITE(string filename)` and `WriteMetricsAsync`).
  - `QuickFiler/Controllers/EfcHomeController.Metrics.cs` (method
    `BuildQuickFileMetricLines`).

## Proposed Fix

### Design summary (what changes where):
Change the three production format-string literals from `"hh:mm"` to `"HH:mm"` at the exact
sites listed under Scope & Non-Goals above, and update the three dependent test sites to match.
No other logic, control flow, or CSV field order changes.

### Boundaries and invariants to preserve:
- CSV field count and column order for the session-metrics files are unchanged; only the digits
  rendered in the existing time-of-day column change.
- The numeric fields in the same rows (`durationText`, `durationMinutesText`), which already pass
  `CultureInfo.InvariantCulture`, are untouched.
- Tests must continue to drive their clock through `FakeTimeProvider` or the injected clock
  factory rather than the wall clock (both existing test files already satisfy this; the fix only
  changes the format-string argument and one asserted literal, not the clock-injection mechanism).

### Dependencies or blocked work:
None. The three production sites and three test sites are independently editable; no other file
in the repository asserts against these call sites' output (confirmed by research §3, a
repository-wide search for both the format literal and the two known test-asserted literals
found no additional dependents).

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs`
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs`
- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
- `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`

#### Functions/classes/CLI commands impacted:
- `QfcHomeController.Metrics.QuickFileMetrics_WRITE(string filename)` (`dataLineBeg`
  interpolation).
- `QfcHomeController.Metrics.WriteMetricsAsync` (`curTimeText` assignment).
- `EfcHomeController.Metrics.BuildQuickFileMetricLines` (`curTimeText` assignment).
- Test methods `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` and
  `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` in `QfcHomeControllerMetricsTests.cs`.
- Test method `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` in
  `EfcHomeControllerMetricsTests.cs`.

#### Data flow and validation changes:
None. This is a rendering-format change only; no new validation, branching, or data flow is
introduced.

#### Error handling and logging updates:
None required or proposed.

#### Rollback/feature-flag considerations (if applicable):
No feature flag needed. Rollback is a direct revert of the format-string and test-literal changes
if required; the CSV has no in-repo reader, so no data migration is entailed in either direction.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
- Input: the local `DateTime` value already computed at each of the three sites (`now` /
  `currentDateTime`), sourced from the injected clock in both production methods.
- Output: the time-of-day field of the session-metrics CSV row changes from ambiguous 12-hour
  (`hh:mm`, no AM/PM) to unambiguous 24-hour (`HH:mm`), consistent with the adjacent `SentDate`
  field's existing `"HH:mm:ss"` convention in the same row.

#### Required configuration keys and defaults:
None.

#### Backward-compatibility expectations:
No externally observed schema break. The session-metrics CSV has no in-repo reader (confirmed:
repository-wide search for `EmailSession` returns only settings-plumbing declarations and the
three writers). Field count and column order are unaffected; only the digits in the time column
change for rows written after the fix. Rows written before the fix are not rewritten.

#### Performance constraints (latency/throughput/memory):
None; the change is a literal format-string substitution with no performance implications.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access): the .NET custom date-and-time format specifier `HH`
  renders 24-hour ASCII digits regardless of the active culture (confirmed via Microsoft's custom
  format-string documentation, cited in research §2); the only culture-sensitive element of
  `"HH:mm"` is the `:` separator character, which is out of scope per the excluded
  `CultureInfo.InvariantCulture` follow-up (issue #742).
- Constraints (budget, performance, compatibility): none beyond the toolchain gates defined in
  CLAUDE.md.
- External dependencies (services, libraries, releases): none.

## Data / API / Config Impact
- User-facing or API changes: none. The change affects only the on-disk content of a session-
  metrics CSV that has no in-repo reader.
- Data or migration considerations: none; historical rows are not rewritten.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): the PR description must state that
  this change alters the emitted CSV content (the time-of-day column's rendering), since the
  artifact is consumed by a human-maintained spreadsheet outside the repository.

## Test Strategy
- Regression tests to add or update: no new tests are required. Update the existing literal-value
  assertions in `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` (lines 243, 278;
  doc comments at 227, 265) and `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`
  (line 53) to match the corrected 24-hour rendering.
- Unit tests (MSTest) for the fixed behavior and boundaries: the two clock-seam tests in
  `QfcHomeControllerMetricsTests.cs` already drive the fix sites through
  `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` and
  `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine`, and the fixed-clock test
  `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` in
  `EfcHomeControllerMetricsTests.cs` exercises the EFC site at a fixture time (13:05) that
  directly probes the 12-hour/24-hour boundary this defect is about.
- Edge cases and negative scenarios: the `EfcHomeControllerMetricsTests.cs` fixture already
  exercises the afternoon boundary (`MetricsNow` = 13:05, i.e. 1:05 PM) that is ambiguous under
  the old format and unambiguous under the new one. No additional edge-case tests are proposed;
  the midnight/noon boundary is not separately covered by an existing test and is not required by
  the issue's acceptance criteria.
- Error handling and logging verification: not applicable; no error paths are touched.
- Coverage impact and targets for changed lines/modules: no coverage regression expected; every
  changed production line is already exercised by an existing, passing test.
- Toolchain commands to run (format -> lint -> type-check -> test), per CLAUDE.md:
  1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <QuickFiler.Test assembly path> /EnableCodeCoverage`
- Manual validation steps (if required): none beyond the automated suite; the CSV has no in-repo
  reader, so no manual file inspection is required for correctness (though the PR body should
  still describe the content change per the Data / API / Config Impact note above).

## Acceptance Criteria
- [ ] `QuickFiler/Controllers/QfcHomeController.Metrics.cs:48` renders the time-of-day field
      using `"HH:mm"` (24-hour) instead of `"hh:mm"`.
- [ ] `QuickFiler/Controllers/QfcHomeController.Metrics.cs:127` renders `curTimeText` using
      `"HH:mm"` (24-hour) instead of `"hh:mm"`.
- [ ] `QuickFiler/Controllers/EfcHomeController.Metrics.cs:96` renders `curTimeText` using
      `"HH:mm"` (24-hour) instead of `"hh:mm"`.
- [ ] `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` (`expectedDataLineBeg` at
      lines 243 and 278) builds its expected literal via `expectedLocal.ToString("HH:mm")`
      rather than `"hh:mm"`, and both tests pass.
- [ ] `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` (line 53) asserts the
      time-of-day field as `13:05` (not `01:05`) for the fixture's `MetricsNow` of
      `2026-07-04 13:05:00`, and the test passes.
- [ ] No file under QuickFiler/Legacy/, no TaskVisualization/TaskViewer.Designer.cs, and no
      file matching .claude/\*\*, .codex/\*\*, .agents/\*\*, config/blast-radius.json, or
      config/orchestration-routing.json is modified by this change.
- [ ] None of the three fixed call sites gain a `CultureInfo.InvariantCulture` argument (or any
      other `CultureInfo` argument) as part of this change; that gap is tracked separately as
      issue #742.
- [ ] The full `QuickFiler.Test` assembly is green after the changes above (`vstest.console.exe`
      run with `/EnableCodeCoverage`, per the Toolchain commands in Test Strategy).
- [ ] Full toolchain pass completed in order (CSharpier format/check, analyzer rebuild, nullable
      rebuild, `QuickFiler.Test` vstest run) with no failures in the final pass.
- [ ] The PR description explicitly states that this change alters the emitted session-metrics
      CSV's time-of-day column content, since the artifact is read by a human-maintained
      spreadsheet outside the repository.

## Risks & Mitigations
- Technical or operational risks: a human-maintained spreadsheet outside the repository may have
  been built around the old, ambiguous 12-hour rendering (e.g. manual heuristics assuming
  business-hours data). Because there is no in-repo reader, this risk cannot be verified or
  mitigated from within the codebase.
- Mitigations and rollbacks: state the content change explicitly in the PR body (required AC
  above) so any downstream spreadsheet maintainer is notified. Rollback is a direct revert of the
  four changed files if the downstream impact proves unacceptable.

## Rollout & Follow-up
- Release/rollout steps: standard PR merge; no feature flag, migration, or staged rollout is
  required.
- Post-fix monitoring or clean-up tasks: none required by this issue. Two related, explicitly
  out-of-scope items exist and are tracked separately:
  - GitHub issue #742 (`quickfiler-date-time-format-missing-invariant-culture`) — apply
    `CultureInfo.InvariantCulture` uniformly to the date/time fields in these two files
    (including `SentDate` and `curDateText`), not just the three sites touched here.
  - The three `Legacy/`-namespace `"hh:mm"` sites and the already-correct-but-uncultured
    on-screen `HH:mm` sites noted in Scope & Non-Goals remain unaddressed by design; either may be
    promoted as a future issue but neither is part of this fix.
- Links: issue #645 (`https://github.com/drmoisan/TaskMaster/issues/645`); related follow-up
  issue #742; source research at
  docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/research/2026-09-02T08-47-twelve-hour-time-format-research.md.

# quickfiler-date-time-format-missing-invariant-culture (Spec)

- **Issue:** #742
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12T17-00
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-bug — this document is the sole authoritative acceptance-criteria source for this
  issue. `user-story.md` in this feature folder is a supporting narrative only and carries no acceptance
  criteria.

## Context

Five QuickFiler controller files render `System.DateTime` values through custom format strings whose
separators are culture-dependent, while the adjacent numeric fields in the same methods already pass
`CultureInfo.InvariantCulture`. In a .NET custom date/time format string, `:` is the `TimeSeparator`
custom specifier and `/` is the `DateSeparator` custom specifier; both resolve through the ambient
`DateTimeFormatInfo` rather than rendering as literal characters. Under a culture such as `it-IT`, whose
`TimeSeparator` is `.` rather than `:`, a call such as `now.ToString("HH:mm")` renders `13.05` instead of
`13:05` whenever the host machine's regional settings differ from `en-US`.

This is a distinct defect from issue #645 (twelve-hour time format). It was discovered while researching
that issue, and #645's own acceptance criteria do not mention culture-invariance; the two defects are
independent (12-hour ambiguity vs. culture-dependent separator) and are tracked separately.

Environment:
- OS/version: Windows 11, Outlook VSTO add-in host
- Runtime: C# / .NET Framework 4.8.1 (`TargetFrameworkVersion v4.8.1`)
- Command/flags used: not applicable (static code review and research-verified finding)
- Data source or fixture: the session-metrics CSV emitted by the QuickFiler and EFC metrics writers, and
  on-screen/exception summary strings built from `SentDate`

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium for the two CSV-writer files (`QfcHomeController.Metrics.cs`, `EfcHomeController.Metrics.cs`): a
wrong time or date separator corrupts a machine-read, write-only artifact in the same way the numeric
fields were already protected against. Low/cosmetic for the three UI-facing sites
(`QfcItemController.ViewerSetup.cs`, `QfcCollectionController.cs`, `EfcItemController.cs`), which only
affect on-screen or exception summary text. Filed as a single Medium entry since the writer sites
dominate.

## Repro & Evidence

Steps to Reproduce:
1. Set the Windows regional format (or thread `CurrentCulture`) to a locale whose
   `DateTimeFormatInfo.TimeSeparator` or `DateSeparator` differs from `en-US` (for example `it-IT`, whose
   `TimeSeparator` is `.`).
2. Run a QuickFiler filing session or an EFC move session so the session-metrics CSV writer executes, or
   trigger a code path that builds a `SentDate`/`SentTime` display string.
3. Inspect the emitted CSV date/time columns, or the on-screen/exception summary string.

Expected:

Every date/time field emitted by these files renders with a fixed, invariant separator regardless of the
host machine's regional settings, matching the invariant-culture handling already applied to the
adjacent numeric fields in the same methods (per the existing comment at
`EfcHomeController.Metrics.cs:101-103`: "the metrics file is machine-read, so numeric fields are rendered
with the invariant culture rather than the operator's locale").

Actual:

None of the date/time rendering operations in the affected files supply `CultureInfo.InvariantCulture`
(or any other `IFormatProvider`), so the rendered separator character is culture-dependent. The
authoritative site list below was re-derived against the current worktree on 2026-09-12 and supersedes
the stale line numbers previously recorded against this issue. It covers 5 production files, 13 distinct
source lines, and 17 individual date/time rendering operations:

| File | Lines | Operations | Notes |
|---|---|---|---|
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 48, 125, 127 | 4 (line 48 carries 2) | Line 48 is the interpolated form `$"{now:MM/dd/yyyy},{now:HH:mm},"`; an interpolated string cannot take an `IFormatProvider` argument, so this site is rewritten as two explicit two-argument `ToString` calls rather than an appended argument. |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 95, 96, 118, 119 | 4 | Two-argument call-form rewrite; `using System.Globalization;` already present. |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 465 | 2 | Two calls on one line (`SentDate.ToString("MM/dd/yyyy")`, `SentDate.ToString("HH:mm")`); no `using System.Globalization;` in this file today. |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 235, 1296, 2302 | 5 (1296 and 2302 carry 2 each) | No `using System.Globalization;` in this file today. |
| `QuickFiler/Controllers/EfcItemController.cs` | 607, 612 | 2 | No `using System.Globalization;` in this file today. |

The line-48 site is a further in-scope site identified during research, beyond the five-file,
fifteen-site list the maintainer originally confirmed. It is an interpolated format specifier rather than
a two-argument `ToString` call, and both of the current research pass's independently constructed search
strategies detected it, so its inclusion is not a single-strategy artifact. See the research artifact's
Q1, Q2, and Numeric Derivation Evidence sections for the full two-strategy sweep that produced the
13-line, 17-operation, 5-file total used throughout this document.

The CSV-writer sites carry the higher severity, since a wrong separator corrupts a machine-read artifact
in the same way the numeric fields were already protected against; the UI-facing sites are lower
severity, cosmetic-only.

Logs / Screenshots:
- [x] Attached minimal logs or snippet
- Snippet: none of the date/time rendering operations at the sites listed above pass a `CultureInfo`
  argument; contrast with the `durationText`/`durationMinutesText` calls in the same two Metrics.cs
  files, which already do (`.ToString("##0", CultureInfo.InvariantCulture)`).

## Scope & Non-Goals

- In scope:
  - The 13 source lines / 17 rendering operations in the five files listed in the authoritative table
    above.
  - Adding the missing `using System.Globalization;` directive to the three files that lack it.
  - Rewriting the two self-referential test-oracle assertions described in Test Strategy below.
  - Adding one new regression-test file with named tests covering each of the five production files.
  - The one line in the test project file needed to compile the new test file.

- Out of scope / non-goals (paths below are deliberately not backticked; this change does not touch
  them):
  - The UtilitiesCS and ToDoModel projects contain several genuine instances of the same defect class
    (date/time-typed receivers rendered through an uncultured format string with a separator character),
    but they sit in a different project boundary than the five named QuickFiler controller files this
    issue is scoped to, and fixing them would pull unrelated assemblies into this change. They are
    recommended as a separate follow-up issue and are not touched here.
  - A diagnostic log line inside a different partial-class file of the QfcHomeController class (the
    partial-class file that is not the Metrics file) renders a date/time value the same uncultured way,
    but the string is written only to a diagnostic log inside a caught-cancellation handler, never
    persisted or user-facing. It shares the defect mechanism but is deliberately excluded from this
    issue's scope; it is recommended as a separate minimal follow-up issue rather than being bundled into
    this diff.
  - The uncompiled sources under the QuickFiler Legacy folder contain several matches for the same
    format-string shape, but that folder is confirmed dead source: the QuickFiler project file has zero
    compile references into it, so the code is not part of the built assembly and fixing it would have
    no runtime effect. No action and no follow-up issue.
  - A sortable-key method in the EmailSorter file formats a date/time value with a format string that
    contains no separator characters at all (every component is a fixed-width numeric field with no
    culture-dependent punctuation), so the ambient culture cannot alter its output. It is not a defect of
    this class and needs no action.
  - Two other apparent matches for the culture-sensitive shape are TimeSpan format strings whose
    separators are backslash-escaped literals, which render as literal characters regardless of culture
    under .NET's custom-format-string escape rules; they are false positives for this defect class and
    need no fix.
  - Any change to the `[ExcludeFromCodeCoverage]` attributes already present on the QfcCollectionController
    and EfcItemController classes is out of scope; this issue neither adds nor removes that attribute.
  - The QuickFiler production project file needs no edit, because no production source file is added or
    deleted by this change; the set of compiled files is unchanged.

## Root Cause Analysis

Discovered during research for issue #645 ("Bug: quickfiler-session-metrics-twelve-hour-time-format")
while investigating whether `CultureInfo.InvariantCulture` should be added to the `hh:mm` -> `HH:mm`
sites that issue fixes. Tracing that issue's own cited target convention (`SentDate`'s `"HH:mm:ss"`)
showed that convention is itself uncultured, which means the gap is systemic across both Metrics.cs files
rather than isolated to the three sites issue #645 touches, and, per the follow-up research for this
issue, systemic across three additional controller files as well.

The `H`/`h`/`m`/`d`/`M`/`y` letter specifiers in these format strings render ASCII 0-9 digits regardless
of culture (they are not digit-substituting specifiers); the only culture-dependent element in
`"HH:mm"`/`"MM/dd/yyyy"`/`"HH:mm:ss"` is the separator character, which resolves via
`DateTimeFormatInfo.DateSeparator` / `DateTimeFormatInfo.TimeSeparator`. The adjacent numeric fields
(`durationText`, `durationMinutesText`) in the same methods already guard against exactly this failure
mode by passing `CultureInfo.InvariantCulture` explicitly (see the comment at
`EfcHomeController.Metrics.cs:101-103`); the date/time fields simply never received the same treatment.

Neither the repository's editor-configuration file nor its banned-symbols list carries an entry targeting
this defect class (CA1304, CA1305, CA1307) at any severity above the blanket suggestion default, so no
existing gate would have caught it. Neither of those two files is touched by this change, and both are
named here in plain prose without backticks so that the change-footprint harvester does not read them as
part of this issue's write footprint.

## Proposed Fix

### Design summary (what changes where)

For each of the 12 call-form operations (all sites except line 48 of
`QuickFiler/Controllers/QfcHomeController.Metrics.cs`), append `, CultureInfo.InvariantCulture` as the
second argument to the existing single-argument `ToString("...")` call, leaving the format string itself
unchanged.

For the one interpolated-form operation (line 48 of
`QuickFiler/Controllers/QfcHomeController.Metrics.cs`, `$"{now:MM/dd/yyyy},{now:HH:mm},"`), replace the
two interpolated specifiers with two explicit two-argument `ToString` calls, matching the shape already
used two lines below at lines 125 and 127 of the same file (`FormattableString.Invariant(...)` was
considered and rejected in favor of matching the file's own existing convention).

Add `using System.Globalization;` to the three files that currently lack it:
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, `QuickFiler/Controllers/QfcCollectionController.cs`,
and `QuickFiler/Controllers/EfcItemController.cs`. The two Metrics files already have the directive.

### Boundaries and invariants to preserve

- The CSV field count, field order, and delimiter character emitted by the two metrics writers must not
  change; only the separator character inside the date and time fields changes.
- No format string's letter specifiers (`MM`, `dd`, `yyyy`, `HH`, `mm`, `ss`) change; only the
  `IFormatProvider` argument is added or, for the interpolated site, the syntax used to supply it.
- No public method signature, property signature, or return type changes.

### Dependencies or blocked work

None. `DateTime.ToString(string, IFormatProvider)` has been available since .NET Framework 1.1 and is
present on every receiver in the five files; all receivers resolve to plain, non-nullable
`System.DateTime` (confirmed for the COM-interop receiver at `QfcCollectionController.cs:235` as well,
since the Outlook PIA marshals the VARIANT date to `System.DateTime` before the `.ToString` call
executes).

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

- `QuickFiler/Controllers/QfcHomeController.Metrics.cs` — 4 operations across lines 48, 125, 127.
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs` — 4 operations across lines 95, 96, 118, 119.
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` — 2 operations at line 465, plus a new
  `using System.Globalization;` directive.
- `QuickFiler/Controllers/QfcCollectionController.cs` — 5 operations across lines 235, 1296, 2302, plus a
  new `using System.Globalization;` directive.
- `QuickFiler/Controllers/EfcItemController.cs` — 2 operations across lines 607, 612, plus a new
  `using System.Globalization;` directive.

#### Functions/classes/CLI commands impacted

`QuickFileMetrics_WRITE`/`WriteMetricsAsync` (`QfcHomeController.Metrics.cs`),
`BuildQuickFileMetricLines` (`EfcHomeController.Metrics.cs`), `GetItemSummary`
(`QfcItemController.ViewerSetup.cs`), `TryGetMoveReadiness` / the `ToggleExpansionStyle` message builder /
the `GetMoveDiagnostics` diagnostics-line builder (`QfcCollectionController.cs`), and the `SentDate`/
`SentTime` properties (`EfcItemController.cs`). No CLI commands are impacted.

#### Data flow and validation changes

None. No new inputs, no new validation. The only observable change is the rendered separator character
in existing output strings.

#### Error handling and logging updates

None required; no exception path or log statement changes shape.

#### Rollback/feature-flag considerations (if applicable)

No feature flag. The change is a formatting-argument addition with no behavioral branch; rollback is a
plain source revert of the five files.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

Output format strings (`"MM/dd/yyyy"`, `"HH:mm"`, `"HH:mm:ss"`) are unchanged. Output separator
characters become fixed (`/` for date, `:` for time) regardless of the host machine's regional settings,
because every rendering operation now supplies `CultureInfo.InvariantCulture`.

#### Required configuration keys and defaults

None.

#### Backward-compatibility expectations

No public API changes. On a host whose regional settings already use `en-US`-equivalent separators
(`/` and `:`), rendered output is byte-for-byte identical before and after the fix. On a host whose
regional settings use different separators, rendered output changes from the previous, incorrect
culture-dependent separator to the fixed invariant separator; this is the intended defect fix, not a
compatibility break.

#### Performance constraints (latency/throughput/memory)

None; adding a second, already-allocated static `CultureInfo.InvariantCulture` argument has no measurable
performance impact.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access):
  - The test-host's default culture renders `en-US`-equivalent separators (`/` for date, `:` for time),
    which are identical to `CultureInfo.InvariantCulture`'s separators, so existing literal assertions
    elsewhere in the test suite that were not built from an uncultured `ToString(format)` call are
    unaffected by this change.
  - The five files' receivers are always plain `System.DateTime` (never `DateTime?`), confirmed for every
    site in this issue's research.
- Constraints (budget, performance, compatibility):
  - Both the production project and the test project target `TargetFrameworkVersion v4.8.1`; the
    two-argument `ToString(string, IFormatProvider)` overload is available on that target with no
    compatibility concern.
- External dependencies (services, libraries, releases): none. Only the `System.Globalization` namespace,
  already part of the base class library, is required.

## Data / API / Config Impact

- User-facing or API changes: none to method signatures or property types. The rendered text of the
  session-metrics CSV date/time fields and of the three UI-facing summary strings changes only on hosts
  whose regional settings differ from `en-US`-equivalent separators, and only in the direction of
  becoming culture-invariant.
- Data or migration considerations: the session-metrics CSV is a write-only, machine-read artifact with
  no stored history that needs migration; each run produces a fresh file.
- Logging/telemetry updates: none.
- Compatibility notes: no CLI flags, config schema, or versioning changes.

## Test Strategy

- Rewrite the two self-referential test oracles in
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
  (`WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` at line 243 and
  `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` at line 278) so that the expected value they
  build for comparison is constructed with `CultureInfo.InvariantCulture` rather than an uncultured
  `.ToString(format)` call. Today both tests build their expected string with the same uncultured call
  the unfixed production code uses, so after the production fix the two calls would diverge under any
  non-invariant-equivalent ambient culture; rewriting the expected-value construction to use
  `CultureInfo.InvariantCulture` restores them as a valid regression oracle.
- Add one new test file, `QuickFiler.Test/Controllers/QuickFilerInvariantCultureIssue742Tests.cs`, with
  one named regression test per production file (five total), each following the deterministic
  culture-swap pattern already established at lines 138-156 of the existing EfcHomeControllerMetricsTests
  file (not modified by this change) and
  `QfcHomeControllerMetricsTests.cs:186-211`: save `CultureInfo.CurrentCulture`, set a substitute culture
  inside a `try` block, and restore the original in a `finally` block.
  - The substitute culture must not be a named culture whose separator characters are asserted against
    directly, because `DateTimeFormatInfo` separator values for a named culture can vary across .NET and
    ICU versions, which would make the assertion environment-dependent. Instead, each new test clones a
    `CultureInfo` and assigns explicit sentinel characters to `DateTimeFormat.DateSeparator` and
    `DateTimeFormat.TimeSeparator` (any character other than `/` and `:`) before activating it as
    `CurrentCulture`. The assertion is then that production output contains `/` and `:` — the invariant
    separators — while the ambient culture, if it had been honored, would have produced the sentinel
    characters instead.
  - Each of the five new tests exercises its production file's documented seam: the injectable
    `TimeProvider` property for `QfcHomeController.Metrics.cs`; a direct `DateTime` parameter for
    `EfcHomeController.Metrics.cs`'s `BuildQuickFileMetricLines`; a directly constructed `MailItemHelper`
    with `SentDate` set for `QfcItemController.ViewerSetup.cs`'s `GetItemSummary`; Moq mocks of
    `IQfcItemController` (and, for the line-235 site only, a Moq mock of the Outlook interop `MailItem`)
    for `QfcCollectionController.cs`; and a directly assigned `MailItemHelper` for
    `EfcItemController.cs`'s `SentDate`/`SentTime` properties. None of the five requires a live Outlook
    process or a WinForms handle.
  - No test created for this issue uses a temporary file, `Thread.Sleep`, or `Task.Delay`, per repository
    policy.
- No change to any other existing test file; the sweep in the research artifact's Q4 section confirms no
  other existing test constructs an expected value from a date/time `.ToString` call on one of the five
  in-scope files.
- Toolchain commands to run, in order, restarting from the top if any step fails or auto-fixes files:
  1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`, run against the built
     QuickFiler.Test test assembly.
  Never add `/p:Nullable=enable` to step 2 or 3. Never substitute `/t:Build` for `/t:Rebuild` in step 2 or
  3. Close Outlook (never kill the process) before running step 2, 3, or 4, since the rebuild targets the
  QuickFiler VSTO add-in output that a running Outlook process can hold locked.
- Evidence handling, per the maintainer decision on issue #671 (effective 2026-09-11): commit projections
  only. Record the pre-fix discovery-count figures this document's acceptance criteria depend on (the
  14/4/1/3/2 per-file counts, the single `[{]now:` count, and the 4/2 `CultureInfo.InvariantCulture`
  counts) as a Markdown baseline artifact under the feature folder's evidence directory, kind baseline,
  written before any Write Set source file is modified. Record the toolchain pass result and the
  named-test pass/fail outcome as a Markdown summary under the feature folder's evidence directory, kind
  qa-gates. Record the fixed/regression test run outcome as a Markdown summary under the feature folder's
  evidence directory, kind regression-testing. Use the kind other for any evidence artifact that does not
  fit the first three kinds. Do not commit a raw test-results file carrying the trx extension, and do not
  commit a raw Cobertura coverage XML file, as an evidence artifact in any of these directories; discard
  the raw tool output after transcribing the pass/fail counts and coverage figures into the Markdown
  artifact.

## Acceptance Criteria

> Verification note, applicable to every item below: any check expressed as a `git grep` search against
> a file this change newly creates (the two new-file entries in the Write Set) must use
> `git grep --untracked`, or must run only after that file has been staged with `git add`; a plain
> `git grep` does not see an untracked file and reports nothing for it. Any check expressed as a
> zero-match expectation must be read as "the command prints no line for that path and exits 1," never as
> "the command returns 0," because `git grep -c` prints no line at all, and exits 1, for a path with zero
> matches.

- [ ] The interpolated site formerly at line 48 of `QuickFiler/Controllers/QfcHomeController.Metrics.cs`
  is rewritten as two explicit two-argument `ToString` calls, each passing `CultureInfo.InvariantCulture`.
  Checked two ways, because an absence claim alone is not a sufficient gate. First: a search of that file
  for the interpolated-specifier token, written with a bracketed character class as `[{]now:` so the
  pattern is not affected by shell metacharacter handling, prints no line for that file and exits 1 after
  the fix. On the unfixed tree that same search matches exactly 1 line, which is line 48 itself; that
  pre-fix figure of 1 is the discovery-count control proving the search can match, so the post-fix
  zero-match result is a real observation rather than a search that cannot match anything for an
  unrelated reason. Second: the count of lines in that file containing `CultureInfo.InvariantCulture`
  rises from exactly 4 on the unfixed tree (the pre-existing numeric-field sites at lines 70, 73, 150 and
  153) to a strictly greater number after the fix. The exact post-fix figure is deliberately not fixed
  here, because CSharpier decides whether the two replacement calls land on one line or two apiece, and a
  predicted figure would be a formatter artifact rather than a property of the change. Both pre-fix
  figures were measured on 2026-09-12 against the unfixed tree.
- [ ] Lines 125 and 127 of `QuickFiler/Controllers/QfcHomeController.Metrics.cs` each pass
  `CultureInfo.InvariantCulture` as the second argument to their `ToString` call.
- [ ] Lines 95, 96, 118, and 119 of `QuickFiler/Controllers/EfcHomeController.Metrics.cs` each pass
  `CultureInfo.InvariantCulture` as the second argument to their `ToString` call. As a discovery-count
  control, the count of lines in that file containing `CultureInfo.InvariantCulture` rises from exactly 2
  on the unfixed tree (the pre-existing numeric-field sites) to a strictly greater number after the fix;
  this pre-fix figure of 2 was measured on 2026-09-12 against the unfixed tree.
- [ ] Line 465 of `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` has both of its `ToString`
  calls passing `CultureInfo.InvariantCulture` as the second argument, and the file gains a
  `using System.Globalization;` directive (exactly one occurrence).
- [ ] Lines 235, 1296, and 2302 of `QuickFiler/Controllers/QfcCollectionController.cs` have all five of
  their `ToString` calls passing `CultureInfo.InvariantCulture` as the second argument, and the file
  gains a `using System.Globalization;` directive (exactly one occurrence).
- [ ] Lines 607 and 612 of `QuickFiler/Controllers/EfcItemController.cs` each pass
  `CultureInfo.InvariantCulture` as the second argument to their `ToString` call, and the file gains a
  `using System.Globalization;` directive (exactly one occurrence).
- [ ] A line-count search using the regular expression written with bracketed character classes as
  `[.]ToString[(]@\?"[^"]*[:/.\-][^"]*"[)]`, restricted to the five production files listed above, returns
  exactly 2 matching lines after the fix, both of them in
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs`. The residual 2 are the commented-out predecessor
  statements at lines 45 and 46 of that file, which this change deliberately does not edit. The same
  search on the unfixed tree returns 14 matching lines, distributed 4 / 4 / 1 / 3 / 2 across
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs`,
  `QuickFiler/Controllers/EfcHomeController.Metrics.cs`,
  `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`,
  `QuickFiler/Controllers/QfcCollectionController.cs` and
  `QuickFiler/Controllers/EfcItemController.cs`; that pre-fix figure of 14 is the discovery-count control
  that makes the post-fix figure of 2 a real observation rather than a search that matches nothing for an
  unrelated reason. Both figures were measured on 2026-09-12 against the unfixed tree. The `@` is written
  as `@\?` and not as `@?` because the search runs in basic-regular-expression mode, in which a bare
  question mark is a literal character rather than a quantifier; the unescaped form matches nothing in
  this repository whatever the executor does, which would make this criterion unfalsifiable. Both
  spellings were run against the unfixed tree on 2026-09-12: the escaped form printed the 4/4/1/3/2
  per-file distribution and exited 0, and the unescaped form printed no line and exited 1.
- [ ] In `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, the test method
  `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` and the test method
  `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` both build their expected date/time substring
  using `CultureInfo.InvariantCulture` rather than an uncultured `.ToString(format)` call, and both tests
  pass.
- [ ] The new file `QuickFiler.Test/Controllers/QuickFilerInvariantCultureIssue742Tests.cs` exists and
  contains exactly five test methods, one exercising each of the five production files named in this
  issue, each following the try/finally culture-swap pattern with an explicit sentinel `DateSeparator`
  and `TimeSeparator` (not a named culture's separators) and asserting that production output contains
  `/` and `:`; all five tests pass.
- [ ] `QuickFiler.Test/QuickFiler.Test.csproj` gains exactly one new `Compile Include` entry naming the
  new test file. Checked as a pair of counts rather than by eye: a line-count search of that file for
  `QuickFilerInvariantCultureIssue742Tests` returns exactly 1 after the fix and prints no line, exiting 1,
  before the fix, and a line-count search for `Compile Include` in that file returns exactly one more
  after the fix than it did before the fix. The new entry sits inside the single existing `ItemGroup`
  element that opens at line 57 of that file, which is the element holding every existing `Compile`
  entry for the Controllers folder.
- [ ] No new test-results file carrying the trx extension, and no new Cobertura coverage XML file,
  appears as a tracked evidence artifact in the repository as a result of this change. Per the maintainer
  decision on issue #671, effective
  2026-09-11, only Markdown evidence summaries are committed; the numeric pass/fail counts and coverage
  figures produced by the toolchain run are transcribed into Markdown artifacts under the feature
  folder's evidence directory, and the raw tool output is discarded rather than committed.
- [ ] A Markdown baseline evidence artifact exists under the feature folder's evidence directory, kind
  baseline, recording the pre-fix discovery-count figures this document's acceptance criteria depend on
  (the 14/4/1/3/2 per-file counts for the `[.]ToString[(]` sweep, the single `[{]now:` count, and the 4/2
  `CultureInfo.InvariantCulture` counts), and that artifact was written before any Write Set source file
  was modified.
- [ ] `dotnet tool run csharpier format .` followed by `dotnet tool run csharpier check .` both complete
  with no remaining diff.
- [ ] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  completes with zero analyzer errors.
- [ ] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  completes with zero errors.
- [ ] `vstest.console.exe QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`, run against the built
  QuickFiler.Test test assembly, reports zero failures for QuickFiler.Test, including the two rewritten tests and the
  five new tests. Outlook was closed, not killed, before the preceding rebuild steps.
- [ ] Within the QuickFiler and QuickFiler.Test source trees, the change touches exactly the eight paths
  listed in the Write Set section below and no other file. This is scoped to those two trees on purpose:
  the same change also adds Markdown evidence artifacts and updates planning documents inside this
  feature folder, and those additions are expected rather than a scope violation, so a repository-wide
  phrasing of this criterion would be unsatisfiable.

## Risks & Mitigations

- Risk: MSTest does not reset `CultureInfo.CurrentCulture` between test methods, and a new test that sets
  it without restoring it in a `finally` block can leak the changed culture into a sibling test running
  on the same worker thread afterward. Mitigation: every new test mirrors the exact try/finally restore
  shape already used by the two existing de-DE tests in this test suite.
- Risk: `QfcCollectionController` and `EfcItemController` both carry a type-level
  `[ExcludeFromCodeCoverage]` attribute, so the new assertions on lines 235/1296/2302/607/612 will not
  raise the Cobertura coverage percentage even though they are genuinely tested. Mitigation: rely on the
  named test pass/fail result, not the coverage percentage, as the quality signal for these two files;
  this issue does not change the attribute.
- Risk: an incorrect rewrite of the interpolated line 48 could shift the CSV field delimiter or field
  count. Mitigation: match the exact literal shape already used at lines 125 and 127 of the same file,
  which are of a known-correct construction (`ToString(format, CultureInfo.InvariantCulture)` on the same
  `now` value, comma-appended in the same position).

## Rollout & Follow-up

- Release/rollout steps: no feature flag or staged rollout; the fix ships in the same build as any other
  source change to these five files.
- Post-fix monitoring or clean-up tasks: none required; the change has no telemetry or config surface to
  monitor.
- Follow-up issues recommended by research (not part of this issue's scope): a separate issue for the
  UtilitiesCS and ToDoModel projects' equivalent sites, and a separate minimal follow-up issue for the
  diagnostic-only log line in the non-Metrics partial-class file of QfcHomeController.
- Links: issue #742; prior related issue #645 (twelve-hour time format, distinct defect); research
  artifact dated 2026-09-12T16-00 in this feature folder's research directory.

## Write Set

> Do not add or remove backticks in this section. This is the canonical list of paths this change will
> create, modify, or delete. Every backticked path elsewhere in this document repeats one of these eight
> entries or is an out-of-scope path deliberately left unbackticked in prose; do not backtick any path
> outside this list.

- `QuickFiler/Controllers/QfcHomeController.Metrics.cs`
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs`
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
- `QuickFiler/Controllers/QfcCollectionController.cs`
- `QuickFiler/Controllers/EfcItemController.cs`
- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
- `QuickFiler.Test/Controllers/QuickFilerInvariantCultureIssue742Tests.cs`
- `QuickFiler.Test/QuickFiler.Test.csproj`

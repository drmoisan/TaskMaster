# Research: Issue #742 — Date/time rendering missing `CultureInfo.InvariantCulture`

This is a re-verification pass. Every claim below was re-derived by reading the current
worktree (`C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6f0fc7cce6d28aaa`)
on 2026-09-12, not carried over from the prior research artifact
(`agent-a1324bb743d9f3fbe`) or from the issue body. Where a claim from the prior artifact
was checked and found accurate, it is retained; where it could not be verified as written,
that is noted explicitly. No drift was found: every line-numbered citation re-checked
against this worktree matched the prior artifact exactly, including the prior artifact's
own "Orchestrator Correction" addendum that added the interpolated site at
`QfcHomeController.Metrics.cs:48`.

## Q1. Exhaustive repository-wide sweep — two independent strategies

The defect is any rendering of a `System.DateTime` value through a custom format string
whose `MM/dd/yyyy`-, `HH:mm`-, or `HH:mm:ss`-style specifiers resolve `DateSeparator`/
`TimeSeparator` from the ambient culture, with no `IFormatProvider` supplied. That
rendering can occur through two distinct C# syntactic forms: a two-argument-eligible
`.ToString("format")` call, or a `$"...{value:format}..."` interpolation hole. A sweep
built only from the token `.ToString(` is structurally blind to the second form. The two
strategies below are anchored on different syntactic mechanisms so that this blind spot
is not shared between them.

**Strategy 1 — call-form and interpolation-hole pattern union.** Two Grep queries, run
separately over every `*.cs` file in the entire repository source tree and then unioned:

```
\.ToString\(@?"[^"]*[:/][^"]*"\)
\{[A-Za-z_][A-Za-z0-9_.\[\]() ]*:[^}"]*(MM|dd|yyyy|HH|hh|mm|ss)[^}"]*\}
```

The first query matches any single-argument `.ToString("...")`/`.ToString(@"...")` call
whose literal format string contains a `:` or `/` character (a call passing a second
`IFormatProvider` argument would not match, because the string literal is not immediately
followed by `)`). The second matches an interpolation hole whose format specifier contains
a date/time custom-format letter. **Stated blind spot:** this strategy requires the format
text to be a literal visible at the call/hole site; it is blind to any call that builds its
format string in a variable, passes it through a helper/extension method that does not
repeat the literal at the call site, or uses `ToShortDateString`/`ToShortTimeString`/
`ToLongDateString`/`ToLongTimeString`/`String.Format` (none of the latter four APIs contain
a `.ToString(` token or a colon-format hole in the caller's own source line).

Run against the whole repository, this returned 58 raw lines from the first query (34
live, non-comment) and 15 raw lines from the second (all but one either a backslash-escaped
`TimeSpan` literal or a separator-free `yyyyMMddHHmmss`/`yyyyMMddHHmmssf` stamp). Full live,
date/time-typed member set (comment lines excluded by inspecting each line's leading
characters):

- `QuickFiler/Legacy/QuickFileController.cs:757,1010,1013`
- `QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:703,732,770,1221,1307`
- `QuickFiler/Legacy/QfcController.cs:601`
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:465` (2 calls)
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs:48` (interpolated, 2 specifiers), `:125`, `:127`
- `QuickFiler/Controllers/QfcHomeController.cs:74`
- `QuickFiler/Controllers/QfcCollectionController.cs:235`, `:1296` (2 calls), `:2302` (2 calls)
- `QuickFiler/Controllers/EfcItemController.cs:607,612`
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs:95,96,118,119`
- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:243,278` (test-oracle construction, see Q4)
- `ToDoModel/Email Utilities/CaptureEmailDetailsModule.cs:29`
- `UtilitiesCS/OutlookObjects/MailItem/EmailDetails.cs:45`
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/MovedMailInfo.cs:156`
- `UtilitiesCS/OutlookObjects/Item/OlItemSummary.cs:82,125,138,151,164`

**False-positive/escape caveats confirmed by reading the literals:** `TaskMaster.Test/AppGlobals/StartupTimingRecorderTests.cs:89` and `UtilitiesCS/HelperClasses/SegmentStopWatch.cs:102` use `"%m\\:ss\\.ff"` on a `TimeSpan` receiver, where every backslash forces the following character to render literally regardless of culture; these are shape-matches, not live defects. `QuickFiler/Controllers/EmailSorter.cs:72` (`dateTime.ToString("yyyyMMddHHmmss")`) contains zero separator characters, so it did not match either query and is confirmed as a true negative.

**Strategy 2 — receiver-identifier anchor plus manual sequential read.** A Grep query
anchored on the receiver rather than the format text, run over the whole repository:

```
(SentDate|SentOn|GetLocalNow\(\)\.LocalDateTime|CreationTime|LastModificationTime|\.Start\.ToString|\bNow\.ToString|currentDateTime|itemInfo\.SentDate)
```

filtered to lines also containing `.ToString(` or an interpolation colon, corroborated by
a full sequential read of the entire body of each of the five files named in the issue
(`QfcHomeController.Metrics.cs`, `EfcHomeController.Metrics.cs`,
`QfcItemController.ViewerSetup.cs`, `QfcCollectionController.cs`, `EfcItemController.cs`
were each read start to end). **Stated blind spot:** this strategy depends on already
knowing which identifier names denote `DateTime` receivers; outside the five files it is
blind to receivers under unlisted names (e.g. `oMail_Current`, bare `Mail`, `DateTime.Now`
in the legacy files without an intervening named variable) and to verbatim-string calls
whose receiver name is not in the alternation, because the manual full-read mitigation was
applied only to the five named files, not to the whole repository.

Live member set from this strategy: `QuickFiler/Legacy/QuickFileController.cs:757`;
`QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:1221,1307`; `QuickFiler/Legacy/QfcController.cs:601`;
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:48,125,127`;
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:465` (2 calls);
`QuickFiler/Controllers/QfcHomeController.cs:74`;
`QuickFiler/Controllers/QfcCollectionController.cs:235,1296(×2),2302(×2)`;
`QuickFiler/Controllers/EfcItemController.cs:607,612`;
`QuickFiler/Controllers/EfcHomeController.Metrics.cs:95,96,118,119`;
`UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs:123` (`SentDate.ToString("g")`);
`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:149` (`SentDate.ToString("g")`);
`UtilitiesCS/OutlookObjects/Item/OlItemSummary.cs:82,125,138,151,164`;
`UtilitiesCS/EmailIntelligence/EmailParsingSorting/MovedMailInfo.cs:156`;
`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:243,278`.

**Member-set comparison (whole repository, both strategies unioned and normalized to
`path:line`):** the two sets are **not** equal outside the five in-scope files, and this
divergence is expected and diagnostic. Strategy 1 finds the verbatim-string sites
(`CaptureEmailDetailsModule.cs:29`, `EmailDetails.cs:45`) that Strategy 2's identifier
alternation and manual-read scope do not cover; Strategy 2 finds `MeetingItemHelper.cs:123`
and `MailItemHelper.cs:149` (the standard `"g"` format specifier, which has no `:`/`/`
character and so cannot match Strategy 1's character-class test) that Strategy 1 misses
entirely. **Intersection (the reliable core):** both strategies agree on all 13 live source
lines inside the five named controller files (17 individual rendering operations — see the
Numeric Derivation Evidence section), on the five `OlItemSummary.cs` sites, on
`MovedMailInfo.cs:156`, on the three `Legacy/` sites both regexes can reach, and on the two
test-oracle lines. Because the two strategies' disjoint members are attributable to each
strategy's own stated, distinct blind spot rather than to a shared one, the two-strategy
requirement is satisfied for this sweep.

## Q2. Scope recommendation

**(a) Maintainer-confirmed in-scope sites inside the five named files** — all 15 sites
from the issue's "VERIFIED CALL SITES" list, re-verified at their current line numbers by
direct reading:

| File | Lines | Calls |
|---|---|---|
| `QfcHomeController.Metrics.cs` | 125, 127 | 2 |
| `EfcHomeController.Metrics.cs` | 95, 96, 118, 119 | 4 |
| `QfcItemController.ViewerSetup.cs` | 465 | 2 (one line) |
| `QfcCollectionController.cs` | 235, 1296, 2302 | 5 (1296 and 2302 each carry 2) |
| `EfcItemController.cs` | 607, 612 | 2 |

**(b) Further in-scope site inside the five files not on the maintainer's list:** one.
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:48`:

```
dataLineBeg = $"{now:MM/dd/yyyy},{now:HH:mm},";
```

confirmed present in the current worktree, three lines below `var now =
TimeProvider.GetLocalNow().LocalDateTime;` (line 44) inside `QuickFileMetrics_WRITE`
(method opens line 36). Lines 45–47 immediately above are the commented-out predecessor
statement, which is why a reader scanning past comments can miss the live line. Both Q1
strategies were deliberately built to catch this form (Strategy 1's second regex, and
Strategy 2's identifier match on `now` combined with the manual full read); each did
independently detect it, so it is not a single-strategy artifact.

The remedy for this site differs from the call-form remedy: a C# interpolated string
cannot take an `IFormatProvider` argument. The two supported rewrites are to convert the
two specifiers to explicit two-argument `ToString` calls (recommended — it matches the
call shape already used two lines below at 125/127 in the same file and needs no new
`using` directive beyond the one the file already has), or to wrap the interpolation in
`FormattableString.Invariant(...)`.

**(c) Sites outside the five files**, classified:

- `QuickFiler/Controllers/QfcHomeController.cs:74` — `$"{controller.TimeProvider.GetLocalNow().LocalDateTime.ToString("mm:ss.fff")} "`, inside a `catch (OperationCanceledException)` diagnostic log line, never persisted or user-facing. Same class (`QfcHomeController`) as an in-scope file, but a different partial-class file the maintainer's list does not name. **Recommendation: follow-up issue**, kept separate so the reviewed diff for #742 stays confined to the five named files.
- `QuickFiler/Controllers/EmailSorter.cs:72` — `long.Parse(dateTime.ToString("yyyyMMddHHmmss"))`. **Not a defect of this class**: the format string has zero separator characters, confirmed by re-running both Q1 sweeps, neither of which matched this line. **Recommendation: no action.**
- `QuickFiler/Legacy/**` (`QuickFileController.cs`, `QfcGroupOperationsLegacy.cs`, `QfcController.cs`) — re-confirmed **not compiled**: a grep for `Legacy\QuickFileController`, `Legacy\QfcGroupOperationsLegacy`, `Legacy\QfcController` inside `QuickFiler/QuickFiler.csproj` returned zero matches. **Recommendation: dead code, no follow-up issue.**
- `UtilitiesCS/**` (`OlItemSummary.cs`, `MovedMailInfo.cs`, `MeetingItemHelper.cs`, `MailItemHelper.cs`, `SegmentStopWatch.cs`) and `ToDoModel/Email Utilities/CaptureEmailDetailsModule.cs` — confirmed live, compiled production code in different projects (`UtilitiesCS`/`ToDoModel`) than the five in-scope `QuickFiler` files. `SegmentStopWatch.cs:102` is a false positive (backslash-escaped `TimeSpan` literal). The remaining `SentOn`/`Start`/`CreationTime`/`LastModificationTime`/`SentDate.ToString("g")` sites are genuine instances of the same defect class. **Recommendation: separate follow-up issue** scoped to `UtilitiesCS`/`ToDoModel`, since these cross a project boundary the maintainer's issue text does not reach.

## Q3. The fix form

| File | `using System.Globalization;` present? | Call sites | Receiver type | Recommended rewrite |
|---|---|---|---|---|
| `QfcHomeController.Metrics.cs` | Yes — line 2 | 48 (interpolated, 2 specifiers), 125, 127 | `now` is `System.DateTime` (from `TimeProvider.GetLocalNow().LocalDateTime`, line 44; `LocalDateTime` on `DateTimeOffset` returns non-nullable `DateTime`) | Convert line 48's interpolation to two `now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)`/`now.ToString("HH:mm", CultureInfo.InvariantCulture)` calls; append `, CultureInfo.InvariantCulture` to the calls at 125 and 127 |
| `EfcHomeController.Metrics.cs` | Yes — line 3 | 95, 96, 118, 119 | `currentDateTime` is the method's `DateTime` parameter (`BuildQuickFileMetricLines(DateTime currentDateTime, ...)`, line 84); `itemInfo.SentDate` is `MailItemHelper.SentDate`, `public virtual DateTime SentDate` (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Properties.cs:238`) | Append `, CultureInfo.InvariantCulture` to all four calls |
| `QfcItemController.ViewerSetup.cs` | **Absent** (verified lines 1–22) | 465 (2 calls, one line) | `ItemHelper` is `MailItemHelper` (`QfcItemController.cs:135`); `ItemHelper.SentDate` is `DateTime` | Add `using System.Globalization;`; append `, CultureInfo.InvariantCulture` to both calls |
| `QfcCollectionController.cs` | **Absent** (verified lines 1–17) | 235, 1296 (×2), 2302 (×2) | Line 235: `grp.ItemController.Mail` is `Microsoft.Office.Interop.Outlook.MailItem` (`IQfcItemController.Mail`, `QuickFiler/Interfaces/IQfcItemController.cs:42`); `.SentOn` marshals from the COM VARIANT date to plain `System.DateTime`. Lines 1296/2302: `c.ItemHelper.SentDate`/`qf.ItemHelper.SentDate`, `MailItemHelper.SentDate`, `DateTime` | Add `using System.Globalization;`; append `, CultureInfo.InvariantCulture` to all five calls |
| `EfcItemController.cs` | **Absent** (verified lines 1–21) | 607, 612 | `_itemInfo` is `MailItemHelper` (field declared line 379); `_itemInfo.SentDate` is `DateTime` | Add `using System.Globalization;`; append `, CultureInfo.InvariantCulture` to both calls |

`DateTime.ToString(string, IFormatProvider)` is present on every receiver above (all
resolve to non-nullable `System.DateTime`). The only COM-interop-touched receiver
(`Mail.SentOn` at `QfcCollectionController.cs:235`) still returns plain `System.DateTime`
once dereferenced — the COM boundary affects how a test constructs `Mail`, not the
applicability of the two-argument overload. `QuickFiler.Test/QuickFiler.Test.csproj:18`
confirms `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`; the two-argument
overload is unaffected by this target.

## Q4. Existing test oracles

`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`:

- **Line 243** (method `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`, opens
  line 231): builds `expectedDataLineBeg = expectedLocal.ToString("MM/dd/yyyy") + "," +
  expectedLocal.ToString("HH:mm") + ","` from the same `FakeTimeProvider` instant given to
  the controller (lines 239–241), then asserts via `groups.Verify(...)` (lines 249–260)
  that `GetMoveDiagnostics` was called with that exact string.
- **Line 278** (method `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine`, opens line
  269): identical construction and assertion shape for the synchronous overload.

Both tests build their "expected" string with the same uncultured `.ToString(format)` call
the (unfixed) production code uses. Under the repository's default test-host culture, this
still passes after the production fix, because both sides render the same separators. Under
an `it-IT` test-host culture, the two sides would diverge post-fix (production renders
invariant separators; the test's own expected-value construction still renders culture-
driven separators), so the assertion would fail. **These two tests are not a regression
guard for the fix as written**; they would need to build the expected string with
`CultureInfo.InvariantCulture` (or assert the invariant literal directly) to remain valid
after the fix.

**Sweep for other tests asserting a date/time literal from one of the five in-scope files:**

- `QfcCollectionControllerTests.cs:44,99,125` — mocks `SentDate` and passes a literal
  `"01/01/2026,12:00,"` into `GetMoveDiagnostics`, but assertions never inspect the
  returned string's date/time substring.
- `QfcCollectionControllerDefects468MoveTests.cs:290,327,369,430` — sets `SentDate` to a
  fixed `DateTime` but asserts only on line count and null-freedom.
- `QfcCollectionControllerDefects468Tests.cs:411–470`
  (`TryGetMoveReadiness_WithUnassignedDestination_ReturnsFalseAndProducesNotificationText`)
  — re-verified: `GroupWithFolder` (line 411) mocks `Outlook.MailItem.SentOn` at line 414
  (`mail.SetupGet(m => m.SentOn).Returns(new DateTime(2026, 1, 1));`); the test method (line
  431) asserts `notifications.Should().StartWith(...).And.ContainAll("Subject 1", "Subject
  2", "Subject 3", "Subject 4")` (line 462) — never the date substring the notification
  text embeds at production line 235.
- `EfcHomeControllerMetricsTests.cs:333` — re-verified: `SentDate = new DateTime(2026, 6,
  30, 9, 45, 10)` inside the `MovedItems` helper; the tests that consume it
  (`BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration`,
  `..._UnderGermanCulture_RendersInvariantDecimalSeparator`, lines 121–158, re-verified)
  assert only on the numeric duration fields (`",90,1.50,"`, `",2.00,"`, field count), never
  on the date/time substring.

None of these other tests are a date/time-format oracle for the five in-scope files; only
the two lines above (243, 278) construct an expected value from a date/time `.ToString`
call.

## Q5. Test seams and determinism

- **`QfcHomeController.Metrics.cs`** — covered by `QfcHomeControllerMetricsTests.cs`.
  `now` is obtained from the injectable `TimeProvider` property (`internal TimeProvider
  TimeProvider { get; set; } = TimeProvider.System;`, line 19), which tests already replace
  with `FakeTimeProvider` (`FixedClock()`, lines 221–222).
- **`EfcHomeController.Metrics.cs`** — covered by `EfcHomeControllerMetricsTests.cs`.
  `currentDateTime` is a plain method parameter to `BuildQuickFileMetricLines(DateTime
  currentDateTime, ...)` (line 84); the seam is already fully injectable — the test file's
  `Build` helper (line 298) supplies the fixed value `MetricsNow` directly (line 301), with
  no `TimeProvider` object involved for this file.
- **`QfcItemController.ViewerSetup.cs:465` (`GetItemSummary`)** — no clock is read; the
  value comes from `ItemHelper.SentDate`, a plain `MailItemHelper` property. A test needs
  only `controller.ItemHelper = new MailItemHelper { SentDate = ... };` — `MailItemHelper`
  has a public parameterless constructor. No live Outlook COM object or WinForms handle is
  required. **Re-verified: a repo-wide grep for `GetItemSummary` inside `QuickFiler.Test/`
  returns zero matches** — the method is currently uncovered by any test.
- **`QfcCollectionController.cs:235` (`TryGetMoveReadiness`)** — needs
  `Mock<IQfcItemController>` with `.SetupGet(i => i.Mail).Returns(mockMailItem.Object)`
  where `mockMailItem` is `new Mock<Outlook.MailItem>(MockBehavior.Loose)` with
  `.SetupGet(m => m.SentOn).Returns(...)`. This exact pattern already exists in
  `QfcCollectionControllerDefects468Tests.cs:411–420` (`GroupWithFolder` helper,
  re-verified). No live Outlook process or WinForms handle is required; the mock is a COM
  interop mock, not a live COM object.
- **`QfcCollectionController.cs:1296`/`2302`** — needs a `Mock<IQfcItemController>`
  exposing `.ItemHelper` returning a plain `MailItemHelper` with `SentDate` set (no COM mock
  needed, since `ItemHelper` is the plain helper type). The existing helpers in
  `QfcCollectionControllerTests.cs` and `QfcCollectionControllerDefects468MoveTests.cs`
  already build this shape. **Coverage caveat:** `QfcCollectionController` carries a
  type-level `[ExcludeFromCodeCoverage]` attribute (re-verified, line 21), which hides the
  entire merged partial type from the Cobertura percentage even though the methods are
  demonstrably unit-tested today.
- **`EfcItemController.cs:607`/`612` (`SentDate`/`SentTime` properties)** — needs only a
  plain `MailItemHelper` assigned to the internal `_itemInfo` field (reachable via
  `dataModel.MailInfo`, line 273). `EfcItemController` also carries a type-level
  `[ExcludeFromCodeCoverage]` attribute (re-verified, line 25). **Re-verified: a grep for
  `SentDate|SentTime` inside `EfcItemControllerTests.cs` returns zero matches** — neither
  property is exercised by any existing test today.

**Deterministic culture-swap pattern already established in the repository:**
`EfcHomeControllerMetricsTests.cs:136–158`
(`BuildQuickFileMetricLines_UnderGermanCulture_RendersInvariantDecimalSeparator`) and
`QfcHomeControllerMetricsTests.cs:183–213`
(`WriteMetricsAsync_UnderGermanCulture_RendersInvariantDecimalSeparator`) both re-verified
to use:

```
var originalCulture = CultureInfo.CurrentCulture;
try { CultureInfo.CurrentCulture = new CultureInfo("de-DE"); /* act + assert */ }
finally { CultureInfo.CurrentCulture = originalCulture; }
```

A new regression test for this issue should mirror this exact try/finally shape (no temp
files, no `Thread.Sleep`) and should assert the invariant literal directly (e.g.
`.Should().Contain("06/30/2026")` under an active `it-IT` culture) rather than re-deriving
an "expected" string with another uncultured `.ToString(format)` call — the self-referential
oracle failure mode identified in Q4.

**MSTest thread/culture leak risk:** `CultureInfo.CurrentCulture` is process/thread-wide
and MSTest does not reset it between test methods. A test that sets it without a `finally`
restore can leak the changed culture into a sibling test on the same worker thread. Both
existing de-DE tests already restore correctly; any new test for this issue must do the
same.

## Q6. Project file mechanics

- `QuickFiler.Test/QuickFiler.Test.csproj` is non-SDK-style: it uses an explicit
  `<Import Project=... .props />` chain and `<Compile Include>` list rather than implicit
  globbing (re-verified).
- The single `<ItemGroup>` holding every `Compile Include` entry (covering `Controllers\`
  and every other folder in this project) opens at **line 57** and closes at **line 230**
  (re-verified by direct read of both boundary lines). `Controllers\QfcItemController.ViewerSetupTests.cs`
  — the existing test file that already covers `QfcItemController.ViewerSetup.cs` — is
  present inside that range at **line 199** (re-verified).
- A new file at `QuickFiler.Test/Controllers/<Name>.cs` requires exactly one new line
  inside that `<ItemGroup>`, in the literal form used by every sibling entry:
  `<Compile Include="Controllers\<Name>.cs" />`.
- `QuickFiler/QuickFiler.csproj` requires **no edit** for this fix, because the fix only
  changes the body of five already-`Compile`-included files (adding an argument to
  existing `.ToString(...)` calls, converting one interpolation to call form, and adding a
  `using System.Globalization;` directive to three of the five files); the set of compiled
  files does not change.

## Q7. Convention precedent

`EfcHomeController.Metrics.cs:101–103`, re-verified verbatim:

```
// The metrics file is machine-read, so numeric fields are rendered with the invariant
// culture rather than the operator's locale, which would emit a decimal comma and
// corrupt the CSV field count.
```

This comment sits immediately above line 104's
`duration.ToString("##0", CultureInfo.InvariantCulture)`. The parallel numeric-field
precedent recurs at `QfcHomeController.Metrics.cs:70,73` (re-verified:
`duration.ToString("##0", CultureInfo.InvariantCulture)` and
`(duration / 60d).ToString("##0.00", CultureInfo.InvariantCulture)`) and again inside
`WriteMetricsAsync` at lines 150 and 153, and inside `EfcHomeController.Metrics.cs:104–108`.
All of these numeric fields already pass `CultureInfo.InvariantCulture` explicitly; the
issue is that the sibling **date/time** fields in the same methods do not follow the
convention the numeric fields in those same methods already established.

**Analyzer/rule search, re-verified:** `.editorconfig:27` sets a blanket
`dotnet_analyzer_diagnostic.severity = suggestion`, the catch-all default for any analyzer
rule not given an explicit override. A repo-wide grep for `CA1304`, `CA1305`, and `CA1307`
inside `.editorconfig` returns zero hits. `BannedSymbols.txt` (repository root) contains no
entry matching `ToString`, `Culture`, or `Format` (re-verified by direct grep); its entries
target `DateTime.Now`/`UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`,
`CancellationTokenSource.CancelAfter`/constructor, and `WaitHandle.WaitOne`. **No analyzer
rule or banned-symbol entry currently targets this defect class at any severity above
`suggestion`.**

## Q8. Coverage posture (read-only; no coverage tool run)

- `QfcHomeController.Metrics.cs` (`QuickFileMetrics_WRITE`, `WriteMetricsAsync`) —
  exercised by `QfcHomeControllerMetricsTests.cs`, including the two clock-injection tests
  that call both methods end-to-end. The `dataLineBeg`/`curDateText`/`curTimeText` lines
  execute on every run, but no assertion inspects their culture-dependent content.
- `EfcHomeController.Metrics.cs` (`BuildQuickFileMetricLines`) — exercised by
  `EfcHomeControllerMetricsTests.cs`; the date/time lines execute on every call without a
  content assertion on the date/time substring.
- `QfcItemController.ViewerSetup.cs` (`GetItemSummary`) — **not covered at all**,
  re-verified (zero matches for `GetItemSummary` in `QuickFiler.Test/`).
- `QfcCollectionController.cs` (`TryGetMoveReadiness`, the `ToggleExpansionStyle`
  message-builder at line 1296, the `GetMoveDiagnostics` line builder at line 2302) — all
  three call sites are reached by existing tests, but none assert on the formatted
  date/time substring, and the type's `[ExcludeFromCodeCoverage]` attribute (line 21)
  hides this execution from the Cobertura percentage regardless. Re-verified: the only
  `ToggleExpansionStyle` reference in `QuickFiler.Test/` is a mock setup in
  `QfcItemController.NavigationTests.cs:416` for the distinct, `Async`-suffixed member
  `ToggleExpansionStyleAsync`; no existing test reaches line 1296 specifically.
- `EfcItemController.cs` (`SentDate`, `SentTime` properties) — **not covered**,
  re-verified (zero matches for `SentDate|SentTime` in `EfcItemControllerTests.cs`). Also
  excluded from the Cobertura percentage via the type-level `[ExcludeFromCodeCoverage]`
  attribute (line 25).

## Numeric Derivation Evidence

- Complete Family: MM/dd/yyyy, HH:mm, HH:mm:ss
- Exhaustive Search Scope: The entire repository source tree, covering all tracked files across every project, with the reported member set then confined to the five QuickFiler controller files named in the issue.
- Inclusion Rules: Live code only, excluding every line whose code begins with a comment marker; any rendering of a System.DateTime value through a custom format string containing MM/dd/yyyy, HH:mm, or HH:mm:ss, reached either by a two-argument-eligible ToString call or by an interpolated format specifier; with no IFormatProvider argument supplied.
- Exclusion Rules: Numeric receivers of type double or int that already supply CultureInfo.InvariantCulture at the durationText and durationMinutesText sites; TimeSpan format strings whose separators are backslash-escaped literals; separator-free stamps such as yyyyMMddHHmmss; commented-out code; files outside the five named QuickFiler controller files; the uncompiled sources under the QuickFiler Legacy folder.
- Primary Search Strategy or Query Expression: Grep tool run over the entire repository source tree unioning the call-form pattern ToString\("(MM/dd/yyyy|HH:mm|HH:mm:ss)"\) with the interpolation-hole pattern \{[A-Za-z_][^}"]*:(MM/dd/yyyy|HH:mm|HH:mm:ss)\}, then discarding every hit whose code begins with a comment marker and confining the result to the five named QuickFiler controller files.
- Primary Member Set: QuickFiler/Controllers/QfcHomeController.Metrics.cs:48, QuickFiler/Controllers/QfcHomeController.Metrics.cs:125, QuickFiler/Controllers/QfcHomeController.Metrics.cs:127, QuickFiler/Controllers/EfcHomeController.Metrics.cs:95, QuickFiler/Controllers/EfcHomeController.Metrics.cs:96, QuickFiler/Controllers/EfcHomeController.Metrics.cs:118, QuickFiler/Controllers/EfcHomeController.Metrics.cs:119, QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:465, QuickFiler/Controllers/QfcCollectionController.cs:235, QuickFiler/Controllers/QfcCollectionController.cs:1296, QuickFiler/Controllers/QfcCollectionController.cs:2302, QuickFiler/Controllers/EfcItemController.cs:607, QuickFiler/Controllers/EfcItemController.cs:612
- Primary Count: 13
- Cross-check Search Strategy or Query Expression: A receiver-identifier-anchored alternation over (SentDate|SentOn|now|currentDateTime|itemInfo) requiring either a ToString argument or an interpolation colon carrying MM/dd/yyyy or HH:mm or HH:mm:ss, corroborated by a full sequential read of the entire body of each of the five controller files to confirm no rendering of MM/dd/yyyy, HH:mm, or HH:mm:ss was missed by the alternation.
- Cross-check Member Set: QuickFiler/Controllers/EfcItemController.cs:612, QuickFiler/Controllers/EfcItemController.cs:607, QuickFiler/Controllers/QfcCollectionController.cs:2302, QuickFiler/Controllers/QfcCollectionController.cs:1296, QuickFiler/Controllers/QfcCollectionController.cs:235, QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:465, QuickFiler/Controllers/EfcHomeController.Metrics.cs:119, QuickFiler/Controllers/EfcHomeController.Metrics.cs:118, QuickFiler/Controllers/EfcHomeController.Metrics.cs:96, QuickFiler/Controllers/EfcHomeController.Metrics.cs:95, QuickFiler/Controllers/QfcHomeController.Metrics.cs:127, QuickFiler/Controllers/QfcHomeController.Metrics.cs:125, QuickFiler/Controllers/QfcHomeController.Metrics.cs:48
- Cross-check Count: 13
- Member-set Comparison: The primary and cross-check member sets are identical after order-insensitive, case-insensitive normalization to path:line form; every member of each set appears in the other and the two counts match at 13.

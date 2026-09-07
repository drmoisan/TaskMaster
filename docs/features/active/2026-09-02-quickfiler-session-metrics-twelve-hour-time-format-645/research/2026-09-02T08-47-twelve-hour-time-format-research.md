# Research: quickfiler-session-metrics-twelve-hour-time-format (Issue #645)

- Date: 2026-09-02
- Branch: bug/quickfiler-session-metrics-twelve-hour-time-format-645
- Work mode: full-bug

## 1. Current State Analysis — verified fix sites

All line numbers and literal text below were confirmed by reading the current tree directly (not
inferred from the issue text, which the issue itself flags as stale).

### Production sites (three, all under `QuickFiler/Controllers/`)

| File | Line | Current text |
|---|---|---|
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 48 | `dataLineBeg = $"{now:MM/dd/yyyy},{now:hh:mm},";` (inside `QuickFileMetrics_WRITE(string filename)`) |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 127 | `curTimeText = now.ToString("hh:mm");` (inside `WriteMetricsAsync`) |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 96 | `var curTimeText = currentDateTime.ToString("hh:mm");` (inside `BuildQuickFileMetricLines`) |

Line 46 of `QfcHomeController.Metrics.cs` (`//var curTimeText = DateTime.Now.ToString("hh:mm");`) is
a commented-out dead-code line, not a live site — confirmed, it is prefixed with `//` and its output
is never used (line 48 is the live assignment).

### Test sites

`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`:
- Line 227 (XML doc comment): `The formatted dataLineBeg ("MM/dd/yyyy","hh:mm") and the OlEndTime...`
- Lines 242–243 (`WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`):
  ```
  var expectedDataLineBeg =
      expectedLocal.ToString("MM/dd/yyyy") + "," + expectedLocal.ToString("hh:mm") + ",";
  ```
- Line 265 (XML doc comment): `The dataLineBeg ("MM/dd/yyyy","hh:mm") and the endTime...`
- Lines 277–278 (`QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine`): identical
  `expectedLocal.ToString("hh:mm")` construction as above.

`QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`:
- Line 25: `private static readonly DateTime MetricsNow = new DateTime(2026, 7, 4, 13, 5, 0);`
- Line 53 (`BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`), asserted literal:
  ```
  "07/04/2026,01:05,Quarterly Update,SingleSorted,120,2.00,Recipient,Sender,Email,Archive/Target,06/30/2026,09:45:10"
  ```
  Under 24-hour rendering, 13:05 renders as `13:05`, not `01:05`; the literal must change from
  `01:05` to `13:05` after the format-string fix. All other fields in this literal are unaffected
  (`SentDate` at 06/30/2026 09:45:10 is already 24-hour via `HH:mm:ss` and does not change).

All line numbers in the description supplied by the orchestrator (production sites 48/127/96; test
sites 227/242-243/265/277-278/25/53) are confirmed accurate against the current tree as of this
research pass. No drift was found.

Both test files are registered in `QuickFiler.Test/QuickFiler.Test.csproj`
(`<Compile Include="Controllers\EfcHomeControllerMetricsTests.cs" />` at line 125,
`<Compile Include="Controllers\QfcHomeControllerMetricsTests.cs" />` at line 165), and
`QuickFiler.Test` is registered as a project in `TaskMaster.sln` (line 25). Both files already
build and run under the repo's standard `vstest.console.exe` invocation with no special discovery
configuration; a scoped run can cite these two files directly (e.g. via `/Tests:` filters naming the
affected `[TestClass]`/`[TestMethod]` names, or a `/TestCaseFilter:` expression) for baseline and
final QC evidence.

## 2. CultureInfo.InvariantCulture — investigated, recommend NOT adding it

**Existing convention in both files is inconsistent, and the issue's suggested "match the numeric
fields" reading is wrong once traced to the sibling field the issue itself names as the target
convention.**

Verified via direct reads of both `.cs` files:
- The **numeric** fields in all three methods (`durationText`, `durationMinutesText` in
  `QfcHomeController.Metrics.cs:70,73,150,153` and `EfcHomeController.Metrics.cs:104-108`) all pass
  `CultureInfo.InvariantCulture` explicitly. A comment at `EfcHomeController.Metrics.cs:101-103`
  documents why: "the metrics file is machine-read, so numeric fields are rendered with the
  invariant culture rather than the operator's locale, which would emit a decimal comma and corrupt
  the CSV field count."
- The **date/time** fields in the same methods — `curDateText` (`"MM/dd/yyyy"`), the three `hh:mm`
  sites under fix, and, critically, the `SentDate` field the issue names as the format convention to
  match (`EfcHomeController.Metrics.cs:118-119`, `.ToString("MM/dd/yyyy")` /
  `.ToString("HH:mm:ss")`) — **none of them pass any `CultureInfo` argument, anywhere in either
  file.** This is a pre-existing, uniform pattern, not an oversight isolated to the three bug sites.

The issue's proposed-fix note says to "consider passing CultureInfo.InvariantCulture ... matching
what the numeric fields now do," but the format-string convention it asks the fix to match
(`SentDate`'s `"HH:mm:ss"`) itself omits the culture argument. Adding `CultureInfo.InvariantCulture`
only to the two/three sites being touched, while leaving `SentDate`'s `ToString("HH:mm:ss")` and
`curDateText`'s `ToString("MM/dd/yyyy")` uncultured in the same output row, would make the row
internally inconsistent in the opposite direction from what the issue is trying to fix (mixed
culture-handling within one CSV line, rather than a uniform 24-hour clock).

**Is the omission actually a no-op, or can it change observable output?** Confirmed via Microsoft's
custom date-and-time format string documentation: the `:` character in a .NET custom format string
is **not** a literal colon. It is the `":"` custom format specifier, which resolves to
`DateTimeFormatInfo.TimeSeparator` for whichever culture is in effect (current culture, if none is
passed explicitly). Microsoft's own worked examples show this varies by culture — e.g. `it-IT`
renders the time separator as `.` rather than `:`. This means `now.ToString("HH:mm")` under Italian
`CurrentCulture` would render e.g. `13.05` rather than `13:05`, which is a real (if narrow) risk to
a CSV artifact — not the same class of corruption as the numeric decimal-comma issue (this doesn't
add a comma or a field), but it would still make the file's time column inconsistent from row to
row if `CurrentCulture` ever changes between writes (e.g. under different OS regional settings on
different machines running the add-in), and would visually diverge from what an operator expects a
"24-hour `HH:mm`" convention to look like. The `H`/`h`/`m` letter specifiers themselves render
ASCII 0–9 digits regardless of culture (they are not digit-substituting specifiers), so the only
culture-dependent element in `"HH:mm"` is the separator character.

**Recommendation:** This is a real, evidence-backed edge case, but it is orthogonal to the reported
defect (12-hour ambiguity) and the issue explicitly frames it as optional ("consider passing"). Given:
(a) the row's own cited target convention (`SentDate`'s `HH:mm:ss`) does not use
`CultureInfo.InvariantCulture` either, so adding it only to the touched fields creates a new,
narrower inconsistency within the same emitted line; and (b) the issue's own acceptance criteria (§4
below) do not mention culture-invariance, only 24-hour rendering and the three test literals —
treat `CultureInfo.InvariantCulture` as **out of the minimal fix**. It is a legitimate adjacent
defect (all date/time fields in these two files are culture-sensitive to the `:`/`.` separator and
should arguably all be moved to `InvariantCulture` together, including `SentDate` and `curDateText`,
for the same CSV-machine-readability rationale already applied to the numeric fields) and should be
raised as its own follow-up issue rather than folded into this format-string fix, consistent with
the repo's "open a new issue rather than widening scope" convention.

## 3. Broader search for other assertions against these three call sites' output

Searched the full repository tree (not just the format-string literal) for both the literal pattern
`hh:mm`/`HH:mm` and, separately, for the two known test-asserted literals (`01:05` at
`EfcHomeControllerMetricsTests.cs:53`; `expectedLocal.ToString("hh:mm")` constructions in
`QfcHomeControllerMetricsTests.cs`).

Confirmed `hh:mm`/`HH:mm` matches across the whole repo, filtering out documentation/evidence/plan
files (which reference the bug narratively and require no code change) and this issue's own
`issue.md`:

- **In scope (already covered above):** `QuickFiler/Controllers/QfcHomeController.Metrics.cs`,
  `QuickFiler/Controllers/EfcHomeController.Metrics.cs`,
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`.
- **Explicitly out of scope per the issue, confirmed present and unaffected:**
  - `QuickFiler/Legacy/QuickFileController.cs:1013` — `curTimeText = DateTime.Now.ToString("hh:mm");`
  - `QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:703` —
    `strDeletedDte = QF.Mail.SentOn.ToString(@"mm\\dd\\yyyy hh:mm");`
  - `QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:1307` —
    `dataLine = dataLine + "," + QF.Mail.SentOn.ToString("hh:mm");`
  - `TaskVisualization/TaskViewer.Designer.cs:387,400` —
    `this.DtReminder.CustomFormat = "MM/dd/yyyy hh:mm tt";` /
    `this.DtDuedate.CustomFormat = "MM/dd/yyyy hh:mm tt";` (these already carry the `tt` AM/PM
    designator, so they are not ambiguous — the issue's own rationale for excluding them is
    correct).
- **Newly observed, already-correct 24-hour sites (not part of the bug, no action needed, listed
  for completeness since they weren't named in the background brief):**
  - `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:498` —
    `ItemHelper.SentDate.ToString("HH:mm")` (uppercase `HH`, already 24-hour).
  - `QuickFiler/Controllers/QfcCollectionController.cs:1294,2300` —
    `ItemHelper.SentDate.ToString("HH:mm")` / `qf.ItemHelper.SentDate.ToString("HH:mm")` (both
    uppercase, already 24-hour).
  - `QuickFiler/Controllers/EfcItemController.cs:612` — `_itemInfo.SentDate.ToString("HH:mm")`
    (uppercase, already 24-hour, exposed as the `SentTime` property).

No other test, snapshot, or documentation file elsewhere in the repository asserts against the
specific formatted output these three call sites produce. The two test files identified in the
background brief are the only places any literal derived from these three call sites is asserted.

**Adjacent defect worth flagging (out of this issue's scope, not to be fixed here):** The three
already-correct `HH:mm` sites above (`QfcItemController.ViewerSetup.cs:498`,
`QfcCollectionController.cs:1294,2300`, `EfcItemController.cs:612`) format `SentDate` for on-screen
summaries/exceptions using `HH:mm` with no seconds and no `CultureInfo.InvariantCulture` either —
these are UI-facing strings, not CSV fields, so the culture-sensitivity concern from §2 applies with
lower severity (a UI string rendering `13.05` instead of `13:05` under an Italian locale is a
cosmetic inconsistency, not a data-corruption risk), but it is the same class of latent defect. Not
raising a new issue for this per the task instructions (report-only); the orchestrator may choose to
promote it.

## 4. Acceptance-criteria conflict — flag for orchestrator

The issue's acceptance criteria (from `issue.md`, "Acceptance criteria for the resulting issue")
include: **"A repository search for the 12-hour format literal under `QuickFiler/` returns no
match."** This AC, read literally, will **fail** after the proposed fix, because
`QuickFiler/Legacy/QuickFileController.cs:1013` and `QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:703,1307`
are all under the `QuickFiler/` directory tree (specifically `QuickFiler/Legacy/`) and are
explicitly named by the same issue text as out of scope. A verbatim repository-wide search scoped to
`QuickFiler/` for the literal `hh:mm` will still return three matches after the fix, all in
`Legacy/`.

This is a genuine tension in the issue as written, not a research artifact to resolve unilaterally.
The orchestrator should either: (a) scope the AC's verification search to
`QuickFiler/Controllers/` (matching the three named production sites) rather than all of
`QuickFiler/`, or (b) scope it to exclude `QuickFiler/Legacy/` explicitly, or (c) restate the AC to
enumerate the three specific sites by path rather than a directory-wide grep. Any of the three
preserves the issue's intent (the three named sites are fixed) without asserting something the
issue's own scope boundary makes false.

## 5. Candidate approaches

**Approach A (recommended): change only the three format-string literals `"hh:mm"` -> `"HH:mm"`,
no `CultureInfo.InvariantCulture` addition.** Minimal, matches the issue's core ask, does not
introduce a new internal inconsistency within the same CSV row (§2), and does not touch any file
outside the three named production sites and the two named test files. Aligned with the Bugfix
Workflow's "minimal, targeted fix" instruction and the issue's own scope boundary.

**Rejected alternative: change the format strings AND add `CultureInfo.InvariantCulture` to all
three call sites.** Rejected because it is explicitly optional in the issue text ("consider"), it
does not match the cited target convention (`SentDate`'s culture-naked `HH:mm:ss`), and it would
touch a defect (date/time culture-sensitivity) that is broader than the three named sites (the same
gap exists on `curDateText` and `SentDate` in the same methods) — better handled as a single
follow-up issue applying `CultureInfo.InvariantCulture` uniformly to every date/time field these two
files emit, not piecemeal on this bug's three sites.

**Rejected alternative: `"hh:mm tt"` (12-hour with AM/PM designator) instead of `"HH:mm"`.**
Rejected per the issue's own reasoning: the adjacent `SentDate` field in the same row already uses
`HH:mm:ss` (24-hour), so `HH:mm` keeps the row internally consistent; `hh:mm tt` would introduce a
second time-format convention into the same CSV line.

## 6. Behavior semantics

- **Success condition:** all three named production sites render 13:00–23:59 events with an
  unambiguous 24-hour hour (e.g. `14:30`, not `02:30`); the two test files' asserted literals match
  the new rendering; a full `QuickFiler.Test` run is green.
- **No externally observed schema break:** confirmed via the issue's own investigation (repeated
  here, not independently re-verified for this pass since it is prior research already cited in the
  issue) that the session-metrics CSV has no in-repo reader — the field-count and column order are
  unaffected, only the digits in the time column change.
- **Edge case at the 12:00/00:00 boundary:** `hh:mm` renders 00:00 (midnight) as `12:00` and 12:00
  (noon) as `12:00` — both already ambiguous under the current defect. `HH:mm` renders these as
  `00:00` and `12:00` respectively, resolving the ambiguity at both boundaries, not just the
  afternoon case the issue's summary emphasizes.

## 7. Requirements mapping

| AC (issue.md) | Design element |
|---|---|
| All three sites render 24-hour | Change `"hh:mm"` -> `"HH:mm"` at `QfcHomeController.Metrics.cs:48,127` and `EfcHomeController.Metrics.cs:96` |
| Repo search for 12-hour literal under `QuickFiler/` returns no match | See §4 — needs scope qualification to `QuickFiler/Controllers/` or explicit `Legacy/` exclusion to be satisfiable as literally stated |
| Three affected test literals updated, full QuickFiler suite green | Update `QfcHomeControllerMetricsTests.cs:243,278` (and doc comments at 227,265) and `EfcHomeControllerMetricsTests.cs:53` (`01:05` -> `13:05`) |
| PR body states the change, since it alters emitted CSV content | Process step for PR authoring, not a code change |

No numeric count, enumeration, or population claim is being proposed in this research (the
"repository search returns no match" AC is qualitative, not a numeric claim), so the Numeric
Derivation Evidence section is not applicable here.

## 8. Testing implications

- No new tests are required; this is a pure literal-value update to existing, already-comprehensive
  clock-seam tests (`FakeTimeProvider`-driven in `QfcHomeControllerMetricsTests.cs`, fixed
  `MetricsNow` in `EfcHomeControllerMetricsTests.cs`). Both already avoid wall-clock reads, satisfying
  the determinism requirement.
- Scoped regression run: the two named test files build under `QuickFiler.Test.csproj` (confirmed
  registered in `TaskMaster.sln`), so `vstest.console.exe` can be invoked with a `/Tests:` or
  `/TestCaseFilter:` expression naming the affected `[TestClass]`s
  (`QfcHomeControllerMetricsTests`, `EfcHomeControllerMetricsTests`) for a fast baseline/final-QC
  gate ahead of a full-suite run.
- Full toolchain per CLAUDE.md (csharpier -> analyzers -> nullable rebuild -> vstest) still applies
  before merge; this is a small enough change that the full `QuickFiler.Test` assembly run is cheap
  to include as the final gate.

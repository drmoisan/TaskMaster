# Phase 2 — Coverage Delta / Threshold Verification (Issue #207, increment 3)

Timestamp: 2026-06-19T21-15

Command:
- (derived) compare baseline P0-T7 cobertura vs post-change P2-T4 cobertura, both produced by the identical methodology: vstest.console.exe ... /EnableCodeCoverage on TaskMaster.Test, then dotnet-coverage merge -f cobertura.

EXIT_CODE: 0

Output Summary (numeric comparison):

| Metric | Baseline (P0-T7) | Post-change (P2-T4) | Delta |
|---|---|---|---|
| Aggregate repo-wide line-rate (all instrumented modules) | 12.83% (8336 / 64987) | 12.90% (8393 / 65052) | +0.07 pp (no regression) |
| TaskMaster.RemindersProbeSchedule (new type) line-rate | n/a (did not exist) | 100% (line-rate=1, branch-rate=1) | new |

Threshold verdict:

- Repo-wide no-regression: PASS. Post-change aggregate line-rate (12.90%) is >= baseline (12.83%). Coverage increased because the new fully-tested RemindersProbeSchedule type and its 4 deterministic tests added covered lines; no existing covered line was lost.
- New type >= 90%: PASS. TaskMaster.RemindersProbeSchedule is at 100% line and 100% branch coverage.

Repo-wide >= 80% testable-denominator floor: The aggregate Cobertura figure (12.90%) is the all-module raw number and is NOT the CLAUDE.md "testable first-party denominator with COM/VSTO exemptions" measurement; it includes vendored projects (Swordfish/SVGControl), COM/VSTO add-in lifecycle classes, WinForms Designer code, and Outlook Interop handler classes that are formally exempt from the floor per CLAUDE.md. This minor-audit increment establishes no-regression against the same-methodology baseline and >= 90% on the single new testable type; it does not re-baseline the repository-wide testable-denominator percentage, which is owned by feature/csharp-coverage-uplift.

Exemption citations (excluded from the testable denominator for this increment, per P1-T10 dossier):
- AppEvents.Hook() changed lines and the new private ScheduleDeferredRemindersProbe(TimeSpan, Stopwatch): COM/VSTO coverage-exempt (direct Microsoft.Office.Interop.Outlook dependency with no injectable seam; DispatcherTimer is STA scheduling/logging glue around the COM access; live-Outlook + live-timer cannot be unit-tested).
- TaskMaster/Properties/Settings.Designer.cs generated accessor: generated-code exempt.

Outcome: PASS. All required coverage numbers are present and numeric; no value is UNVERIFIED.

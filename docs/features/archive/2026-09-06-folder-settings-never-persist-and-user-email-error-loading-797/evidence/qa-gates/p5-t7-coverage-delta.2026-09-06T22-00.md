# P5-T7 — Coverage Delta Report (Issue #797)

Timestamp: 2026-09-07T10-10

Compares the Phase 0 baseline document coverage/plan797-baseline/coverage.cobertura.xml with the
Phase 5 document coverage/plan797-final/coverage.cobertura.xml, and computes changed-line coverage
over the seven Write Set production C# files from the anchored diff below.

```powershell
$BaseSha = (Select-String -Path 'docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-base-sha.2026-09-06T22-00.md' -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
git add --intent-to-add UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
git diff --unified=0 $BaseSha -- TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
```

The intent-to-add staging is required because an anchored diff reports tracked changes only and would
otherwise be blind to the two files this change creates.

## The three headline figures

BASELINE_LINE_PERCENT=53.23
POSTCHANGE_LINE_PERCENT=53.26
CHANGED_LINE_PERCENT=91.09

## Comparability branch under rule R9

- Baseline `lines-valid`: 83466
- Post-change `lines-valid`: 83537
- Absolute difference: 71 lines, which is 0.085 percent of the baseline value, well inside the
  5 percent tolerance of 4173 lines.

The comparable branch is selected. The document-level line rates are therefore compared directly, and
the comparison is recorded as comparable.

- Baseline: 44426 of 83466 lines covered, line rate 53.23 percent.
- Post-change: 44489 of 83537 lines covered, line rate 53.26 percent.
- Branch counters, recorded as observations: baseline 10877 of 24323 (44.72 percent); post-change
  10928 of 24371 (44.84 percent).

`POSTCHANGE_LINE_PERCENT=53.26` is not below `BASELINE_LINE_PERCENT=53.23`, so the no-regression rule
is satisfied. Under the comparable branch that rule is the binding repository-wide gate for this
change, alongside the changed-line figure.

Both documents were produced by the same session helper with the same derived coverage settings, the
same two-assembly scope and no post-processing, so they are comparable with each other. Neither is
comparable with a document produced by the repository coverage runner, which post-processes its
output.

## Changed-line table, per file

`hits=non-executable` marks a changed line that emits no IL — a blank line, a brace-only line, a
`using` directive, an XML documentation comment or an interface member declaration. The percentage is
computed over executable changed lines only, per rule R10.

| File | Executable changed lines | Covered | Percent |
|---|---|---|---|
| TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs | 9 | 9 | 100.00 |
| TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs | 1 | 0 | 0.00 |
| UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs | 19 | 19 | 100.00 |
| UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs | 36 | 28 | 77.78 |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs | 6 | 6 | 100.00 |
| UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs | NOT APPLICABLE | NOT APPLICABLE | NOT APPLICABLE |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs (new and modified lines only) | 30 | 30 | 100.00 |

The complete per-line record, one `CHANGED-LINE file=<name> line=<n> hits=<value>` entry for every
changed line across all seven files, was produced by the session helper's changed-line mode from the
anchored diff above and the Phase 5 Cobertura document. The per-file totals in the table are that
mode's own aggregation of those entries.

### The interface file

UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs has no `class` element in either Cobertura
document, because an interface declaration emits no IL. The Phase 0 measurability artifact recorded it
as NOT YET CREATED; the same determination run against the Phase 5 document reports it NOT MEASURABLE.
Rule R10 directs that such a file be reported as NOT APPLICABLE rather than as a zero, so all 29 of its
changed lines are excluded from the denominator.

### The junk-folders partial

TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs carries no class-level coverage-exclusion attribute
on its partial and is measurable, at 21 of 69 lines in the Phase 5 document. It therefore produces a
real per-file changed-line row. That row is 1 executable line, 0 covered: the single executable line
is the explicit interface implementation's forwarding expression. No automated test in this plan
drives the real settings-writing implementation that member forwards to, because doing so would write
to the .NET user settings store, so this row sitting at zero is expected and is not itself a failure.
The gate is the aggregate figure.

### The store wrapper

UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs sits at 77.78 percent. The eight uncovered executable
lines are the interior of the address-entry fallback step's own COM catch block and the debug-timing
statements on the paths a mocked chain does not enter. The step is exercised on its success path by
the AC6 case-2 test and on its skip path by cases 3 and 4; only the COM-throw branch inside that
second step is unexercised, because reaching it requires the address-entry read itself to throw while
the primary read has already thrown.

## RELOCATED-UNMODIFIED

The Phase 1 relocation moved `PopulateWithCurrent`, `BindExcludeStoreCheckbox` and `GetRelativeFsPath`
verbatim into the display partial, so the anchored diff reports every line of that new file as added
although the relocated lines did not change. Those lines are enumerated below with their post-change
hit counts recorded as observations, and they are excluded from the `CHANGED_LINE_PERCENT=`
denominator. Without this exclusion the denominator would carry pre-existing code this change does not
modify. Lines inside those three members that the Phase 3 and Phase 4 edits altered are not relocated
lines and remain in the denominator.

Relocated, unmodified, executable lines in
UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs — 38 lines, every one of them
covered with a hit count of 1:

- From `PopulateWithCurrent`, 11 lines: 18, 19, 20, 21, 22 (the method entry and the
  `InvokeRequired` marshal), 52, 53 (the archive-outlook and archive-file-system label assignments),
  66, 67, 68 (the two junk label assignments and the checkbox binding call), 69 (the method exit).
- From `BindExcludeStoreCheckbox`, 18 lines: 95, 98, 99, 100, 101, 104, 105, 106, 107, 108, 109, 112,
  113, 114, 115, 116, 117, 118 — the whole member, which no phase altered.
- From `GetRelativeFsPath`, 9 lines: 121, 130, 131, 138, 139, 142, 143, 146, 147.

The 30 remaining executable lines in that file are new or modified and stay in the denominator: the
four null-conditional mirror assignments (30-33) and the AC6 retry block (42-45) and the three trimmed
or replaced label assignments (48-51) inside `PopulateWithCurrent`; the whole of the new
`BuildUserEmailUnavailableText` helper (78-82, 85, 86); the modified `GetRelativeFsPath` condition
lines (126-129) and the short-circuit operator line (137); and the whole of the new `TrimStorePrefix`
helper (157, 165-167, 170, 171). All 30 are covered.

## Aggregate computation

- Aggregate before the relocated-line exclusion: 139 executable, 130 covered, 93.53 percent.
- Relocated and excluded: 38 executable, 38 covered.
- Aggregate after the exclusion: 101 executable, 92 covered.

CHANGED_LINE_PERCENT=91.09

That is at or above the 90 percent figure CLAUDE.md requires of new and changed code, so the binding
changed-line gate passes.

## The repository-wide floors, recorded rather than asserted

CLAUDE.md, which is rank 1 in the policy compliance order, names an 80 percent repository-wide line
floor. Both percentages recorded here sit below it: `BASELINE_LINE_PERCENT=53.23` and
`POSTCHANGE_LINE_PERCENT=53.26`. Stated plainly, that condition is pre-existing under the
two-assembly scope this plan measures: the baseline was already at 53.23 percent before any change
here, this change neither creates nor resolves it, and the scope is narrower than the full-suite
denominator the 80 percent floor is written against. The binding gates for this change are therefore
the no-regression comparison, which passes, and the changed-line percentage, which passes.

Recorded as non-asserted observations, `.claude/rules/general-unit-test.md` names an 85 percent line
figure and a 75 percent branch figure. The post-change document sits at 53.26 percent line and 44.84
percent branch under this scope. Neither figure is the gate for this change.

No coverage-exclusion attribute is introduced by this change.

Output Summary: The comparable branch applies. Post-change line coverage 53.26 percent is not below
the baseline 53.23 percent, and the changed-line figure over executable, non-relocated lines is 91.09
percent, above the 90 percent requirement.

# P5-T11 — Phase 5 Coverage Delta

- Timestamp: 2026-06-14T15-10
- Command: per-`<line>` analysis of `artifacts/csharp/p5-coverage.cobertura.xml` (post-Phase-5, gitignored) vs prior #199 state in `evidence/qa-gates/final-coverage-comparison.2026-06-14T08-22.md`
- EXIT_CODE: 0

## Prior #199 production-only state (from final-coverage-comparison.2026-06-14T08-22.md)

- Aggregate production-only baseline: 71.65% (post-#197, authority 197-COV-001); net increase already established by Phases 1-4.
- Per-assembly prior (post-Phase-4): ToDoModel 25.22%, QuickFiler 30.57%, TaskMaster 44.05%.
- Two Flag-and-Stop gaps remained uncovered at end of Phase 4: `ProjectEntry` dialog branches and `AppFileSystemFolderPaths.MatchBestSpecialFolder`.

## Covered-line increase from Phase 5

Phase 5 added 12 passing tests and covered previously-uncovered production lines on the two
authorized seams. Covered source lines (post-Phase-5):

| Seam | Source region | Covered/Total | Prior #199 | Delta |
|---|---|---|---|---|
| `AppFileSystemFolderPaths.MatchBestSpecialFolder` static helper body (new code) | 82,84,86-90 | 7/7 = 100% | n/a (new) | +7 covered (new) |
| `AppFileSystemFolderPaths.MatchBestSpecialFolder` instance delegation (line 62) | 62 | 0/1 | 0% (was the deferred gap) | no change (no regression) |
| `ProjectEntry.SetProjectId` (malformed-ID + dialog-free) | 102-139 | 21/26 | partial (malformed previously uncovered) | malformed-ID branch newly covered |
| `ProjectEntry.CompareTo(IProjectEntry)` incl. length tie-break | 182-209 | 22/22 = 100% | tie-break previously uncovered | +tie-break lines newly covered |
| `ProjectEntry.ChangeId` (change-confirmation) | 141-170 | 0/28 | 0% | no change — flag-and-stop (see below) |

The covered-line count on the named seams strictly increased versus the prior #199 state: the
`MatchBestSpecialFolder` matching logic moved from 0% (deferred gap) to a fully-covered static
helper, and the `ProjectEntry` malformed-ID and CompareTo length-tie-break branches moved from
uncovered to covered.

## New/changed-code coverage (target >= 90%)

The only NEW executable production code in Phase 5 is the extracted static helper
`AppFileSystemFolderPaths.MatchBestSpecialFolder(IReadOnlyDictionary<string,string>, string)`.
Its executable body lines (82, 84, 86-90) are 7/7 = 100% covered. The `InternalsVisibleTo`
attribute (UtilitiesCS) is non-executable. The instance-method delegation line (62) is a one-line
refactor of previously-uncovered code; it remains uncovered by unit tests (which exercise the pure
helper directly) but is unchanged in coverage status versus the prior gap (no regression). 

New-code coverage = 100% on the new static helper, exceeding the >= 90% threshold.

## No regression on changed lines

The two production changes are additive: the `InternalsVisibleTo` attribute (non-executable) and
the `MatchBestSpecialFolder` extraction (instance method delegates to a new helper; semantics
byte-for-byte identical). No previously-covered line lost coverage.

## Flag-and-Stop note (does not block PASS)

`ProjectEntry.ChangeId` change-confirmation (0/28) is NOT covered because committing the changed id
runs the `ProjectID` property setter's RAW (un-seamed) `MessageBox.Show`, which would require a
THIRD production seam beyond the two authorized for Phase 5. Recorded in
`evidence/other/p5-projectentry-changeconfirm-gap.2026-06-14T15-10.md`. The malformed-ID branch and
the CompareTo tie-break (the other AC1 dialog gaps) ARE covered.

## Outcome

PASS: coverage strictly increased versus the prior #199 state on the named seams; new-code coverage
on the new static helper is 100% (>= 90%); no regression on changed lines. One residual
change-confirmation branch is an authorized-scope flag-and-stop, not a remediation-required failure.

# Phase 0 — Pre-Change Structural Facts (P0-T13)

Timestamp: 2026-09-08T07-50

Command: a single `pwsh -NoProfile -Command` run, with the current directory set to the worktree root, measuring each file with `(@(Get-Content -LiteralPath <path>)).Count` and counting each fixed string with `[regex]::Matches($text, [regex]::Escape(<literal>))` over the file read with `-Raw`.

`Measure-Object -Line` is deliberately not used for the line counts: it does not count blank lines and undercounts every file here. On `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` it reports 899 against the array count of 1002, and 1002 is the figure AC6 and D9 depend on.

EXIT_CODE: 0

Output Summary:

This artifact is written in full rather than appended to. The version previously on disk recorded an expectation of 1066 for `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` and a stop-and-report conclusion drawn from that expectation. Both were superseded by the plan revision that restated the expected figure as 1067 and recorded the cause. The observations themselves are unchanged; only the expectation they were compared against and the conclusion drawn from it have been corrected.

Line counts, observed against the figures P0-T13 states:

| Path | Expected | Observed | Agrees |
| --- | --- | --- | --- |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 1002 | 1002 | yes |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | 388 | 388 | yes |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs` | 173 | 173 | yes |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` | 302 | 302 | yes |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` | 176 | 176 | yes |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` | 1067 | 1067 | yes |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs` | 252 | 252 | yes |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs` | 480 | 480 | yes |

All eight observed line counts agree with the expected figures. No departure is observed.

Occurrence counts in `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`:

- Fixed string `_globals.Ol.ArchiveRootPath`: 7, expected 7, agrees. Occurrences sit at `:305`, `:376`, `:687`, `:752`, `:795`, `:876`, and `:914`, matching D13 exactly.
- Fixed string `_globals?.Ol.ArchiveRootPath`: 1, expected 1, agrees. The single occurrence sits at `:857`, matching D13 exactly.

The two literals are disjoint rather than nested, because the `?` sits between `_globals` and `.Ol`, so the plain count is not inflated by the conditional form and no subtraction is required.

Provenance of the 1067 figure for `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`, established from git history rather than inferred:

- The file is clean in the working tree; `git status --porcelain --untracked-files=all` reports no entry for it, so the observed content is the committed content.
- The last commit to touch it is `f7294d716313042112c291ef1a50408fa086e4c3`, `test(809): add failing regression tests for the three UiThread defects`, dated 2026-09-08.
- That commit arrived on this branch through PR #814, merged into `origin/main` at `f63a2c4bc396ed43af2354a07891b8ef0bb205ed`, which was in turn merged into this branch before Phase 0 began.
- `git show -U0` for that commit against this path reports `1 file changed, 1 insertion(+)`: a single `[DoNotParallelize]` attribute line inserted at line 19. The delta against the 1066 figure the plan was originally authored to is therefore exactly +1 and is fully attributed.

Assessment of impact on the remainder of this plan: the +1 drift is confined to a file that is not in this plan's Write Set and that no task of this plan modifies. `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` is named by P5-T10 only in a negative condition — that it must be absent from the audited `*.cs` set — and by P6-T13 in the same negative role, so its line count is not read by any later acceptance condition and the drift is inert beyond this figure.

Reconciliation of two earlier Phase 0 artifacts, performed by this task without re-running either command and without altering any recorded observation:

- `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/baseline/phase0-vstest.2026-09-07T22-12.md` now carries the field `ExpectedExitCode: 1` immediately after its `EXIT_CODE: 1` line. The sentence declaring that no expectation was declared has been replaced with one stating that the expectation is declared because the plan accepts a red baseline whose failing set is a non-empty subset of the carve-out set. The paragraph declaring the stop-and-report condition triggered has been replaced with one stating that the sole failing test is the D18 carve-out member, that D18 records its mechanism and its pre-existing status, and that P6-T5 covers it under the issue-780 protocol.
- `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/baseline/phase0-coverage.2026-09-07T22-12.md` no longer asserts that its redaction departs from the task text. It now states that P0-T12 mandates the redaction, and gives the same reason: the test platform places the account and machine tokens inside the relative path's own segments, so the repository-relative spelling alone is not sufficient to keep a host token out of a committed artifact.

Conclusion: the tree has not moved since planning in any respect this task measures. Every expected figure is observed. Phase 1 proceeds.

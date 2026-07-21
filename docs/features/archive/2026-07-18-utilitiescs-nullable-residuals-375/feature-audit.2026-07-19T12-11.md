# Feature Acceptance Audit — utilitiescs-nullable-residuals (Issue #375)

- Timestamp: 2026-07-19T12-11
- Branch under review: `feature/utilitiescs-nullable-residuals-375`
- Diff base: `dffadd5a102884dd811ed5731477de18417594f1`
- Feature HEAD: `c413e61cb32002bd802c4dc8e1f07f5a70729e55`
- Work Mode: `full-feature`
- AC sources: `docs/features/active/2026-07-18-utilitiescs-nullable-residuals-375/spec.md` and `docs/features/active/2026-07-18-utilitiescs-nullable-residuals-375/user-story.md` (AC1-AC8, identical in both)

## Scope and Baseline

Baseline is the epic-integration tip `dffadd5a`. The branch delivers a per-file `#nullable enable` opt-in across 37 hand-written `UtilitiesCS/*.cs` files, plus committed evidence. Baseline signals captured before edits (`baseline-pragma-build`, `baseline-tests-coverage`, both 2026-07-19T10-54): UtilitiesCS has zero CS86xx before any file is opted in (residual debt only surfaces once a file's own pragma is added); 4511 tests pass; UtilitiesCS assembly line 88.75% / branch 82.51%. Post-change signals are compared against this baseline throughout.

## Acceptance Criteria Inventory

- AC1: Every compiled in-scope hand-written file carries `#nullable enable` and compiles with zero CS86xx under the pragma-only build.
- AC2: No project/solution `<Nullable>` element introduced; verification uses no global `/p:Nullable=enable`.
- AC3: The 6 `*.Designer.cs` under OlFolderTools remain oblivious (no pragma) and are not cross-blocked.
- AC4: No behavior change — no new types, no post-condition attributes, no `record`/`record struct`/`init`, existing guards preserved, no new runtime guard beyond what reaching zero CS86xx strictly requires.
- AC5: Annotations consistent with upstream extensions/helperclasses/threading signatures (`TimeOutTask.RunWithTimeout` non-null `Task<TResult>`; `TryCopyToAsyncWithTimeout` `Task<bool>`; `IsNullOrEmpty` non-refining on net481).
- AC6: Clean baseline test run captured before edits; no test/coverage regression on changed lines attributable to this child.
- AC7: The six Maintainer Decisions and Flags recorded in `spec.md`, not silently resolved.
- AC8: No in-scope file exceeds 500 lines as a result of edits; three pre-existing >500-line files flagged, not split.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence and verification |
|---|---|---|
| AC1 | PASS | Independently verified: 37 `#nullable enable` file opt-ins present in the diff (38 enable-pragma lines counting the scoped region in PeopleScoDictionaryNew). Isolated pragma-only gate `UtilitiesCS.csproj /t:Rebuild /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`) exits 0 with zero CS86xx across all 37 files (`qc-nullable-pragma-gate` section B; per-batch `batch0..8`/`to-depricate`/`examples`-pragma-verify). |
| AC2 | PASS | `grep -c "<Nullable>" UtilitiesCS/UtilitiesCS.csproj` = 0 (independently reproduced); no `.csproj`/`.sln`/`.props`/`.targets` file in the name-only diff; no build command used `/p:Nullable=enable` (`qc-no-project-nullable`). |
| AC3 | PASS | Name-only diff contains no `*.Designer.cs`; the 6 OlFolderTools Designer files carry 0 `#nullable` pragmas and are unmodified; isolated gate reached zero CS86xx with only the hand-written halves opted in (`qc-designer-oblivious`). |
| AC4 | PASS | Full source-diff inspection confirms only pragmas, `?`, `= null!`, `!`, and one scoped `#nullable disable`/`enable` region; no new type, method, control-flow branch, or `throw`/guard statement; no post-condition attribute or `record`/`init`; existing guards unchanged. Branch coverage byte-identical confirms no new runtime branch (`qc-coverage-delta`; `qc-nullable-pragma-gate`). |
| AC5 | PASS | OneDrive edits add no null handling around the non-null `RunWithTimeout`/`TryCopyToAsyncWithTimeout` results; `IsNullOrEmpty` non-refinement resolved with justified `!` at guaranteed-non-null sites in FolderPredictorEvaluator and RecipientStatic (`batch4`/`batch5`/`batch3`-pragma-verify; `ac-mapping` AC5 row). |
| AC6 | PASS | Baseline `vstest` run captured before edits (`baseline-tests-coverage`, 4511 pass, UtilitiesCS 88.75%/82.51%); post-change run identical 4511 pass; delta table shows line +0.0000575, branch 0.000000 — no regression on changed lines (`qc-tests-coverage`, `qc-coverage-delta`). |
| AC7 | PASS | All six Maintainer Decisions present in `spec.md` (lines 224/233/239/245/249/258) and none silently resolved: dead duplicate left unmodified; MSDemoConv annotated-only; To Depricate files annotated-only with deletion flagged; MailResolution_ToRemove annotated in place, deletion-candidate flagged; #366 edge flagged and handled via the scoped disable region; three 500-line breaches flagged not split (`qc-maintainer-flags`). |
| AC8 | PASS | Post-edit line counts independently reproduced: MeetingItemHelper 849, RecipientStatic 774, UserDefinedFields 725 — all pre-existing breaches (were 847/773/722), flagged not split; next-largest non-breach SmithWaterman 377; no in-scope file newly crosses 500 (`qc-line-count`). |

## Acceptance Criteria Check-off

All eight ACs are evaluated PASS. Both AC source files (`spec.md` and `user-story.md`) already carry `- [x]` for AC1-AC8, so no check-off edit is required by this review. No unchecked AC remains.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-07-18-utilitiescs-nullable-residuals-375/spec.md`, `docs/features/active/2026-07-18-utilitiescs-nullable-residuals-375/user-story.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

## Summary

All eight acceptance criteria are satisfied and independently corroborated against the committed evidence and a direct read of the branch diff. The change is annotation-only with no behavior change, no test regression, and coverage-neutral results (UtilitiesCS assembly 88.75% line / 82.51% branch, above both the 85% and 75% floors). The three pre-existing >500-line files are flagged per the sanctioned plan behavior, not new violations, and the full-solution build's exit-1 is attributable solely to the pre-existing out-of-scope SVGControl CS0649 (epic child #368), not to this child. Feature-audit blocking_count: 0.

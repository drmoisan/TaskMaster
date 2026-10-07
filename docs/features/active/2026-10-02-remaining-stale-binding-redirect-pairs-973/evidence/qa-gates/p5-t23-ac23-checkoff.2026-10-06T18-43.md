# P5-T23 AC23 check-off

Timestamp: 2026-10-06T18-43
Command: Grep tool over spec.md `^- \[x\] AC23 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Read of the named artifacts
EXIT_CODE: 0
Output Summary: AC23 met and checked off. Both files are under 500 lines (442 and 106, CR counts equal). The member multisets are equal: 36 occurrences, 35 distinct texts. REGION-DIFFERENCES is 0 and MOVE-DIFFERENCES is 0, and ADDED-LINES is 1 (the partial declaration). The partial declaration appears in both files and the base list only in the original. The new file has `#nullable enable` at line 1, no Graph directive and `w/crlf`. The Compile Include is placed immediately after the existing item and the csproj has two hunks. AC11, AC13 and AC14 are green on the split state.

Artifacts read:
- evidence/other/category-classifier-group-split-census.md (EXIT_CODE 0): LINES-AFTER-ORIGINAL 442 (CR 442), LINES-AFTER-NEW 106 (CR 106); MEMBER-COUNT-BEFORE 36, MEMBER-COUNT-AFTER 36, MEMBER-ONLY-BEFORE/AFTER none, MEMBER-DISTINCT 35/35; coordinator-run CMD-VERBATIM-MOVE (maintainer standing approval of 2026-10-04) with REGION-BEFORE-LINES 93, REGION-AFTER-LINES 93, REGION-DIFFERENCES 0, REMOVED-LINES 98, ADDED-LINES 1 with `ADDED:     public partial class CategoryClassifierGroup : IConditionalEngine<MailItemHelper>`, REMOVED-USING-LINES 2, REMOVED-OTHER-LINES 95, BODY-LINES 93, REMOVED-OTHER-TRIMMED 93, BODY-TRIMMED 93, MOVE-DIFFERENCES 0 (the trim drops only blank lines at the two ends of each sequence; the two blank lines removed ahead of the moved `#region` are part of the moved block's removal under spec Planner Amendment 5); the partial-declaration Grep 1 in each file; the base-list Grep 1 in the original and 0 in the new file; `#nullable enable` at line 1; no Graph directive in the new file; the namespace Grep 1 in each file; the csproj two-hunk transcription.
- evidence/qa-gates/p3-t22-compile-include.2026-10-03T11-35.md (EXIT_CODE 0): the Compile Include for the new file is at line 620, directly after the existing CategoryClassifierGroup.cs item at 619.
- evidence/qa-gates/p3-t25-commit-d.2026-10-06T18-20.md (EXIT_CODE 0): CMD-EOL of the new file `w/crlf`.
- evidence/qa-gates/msbuild-analyzers.md and msbuild-treatwarningsaserrors.md (EXIT_CODE 0 each; NEWFILE_CSC_LINES 2): AC11 green.
- evidence/qa-gates/poshqc-test.md (EXIT_CODE 0): AC13 green.
- evidence/qa-gates/mstest-coverage-projection.md and p4-t10-coverage-comparison.2026-10-06T18-30.md (EXIT_CODE 0 each; floors MET; COMPARABILITY A within tolerance): AC14 green on the split state.

SPEC-LINE: `- [x] AC23 (CategoryClassifierGroup.cs under the file limit by a behaviour-preserving partial split; ...` (criterion text unchanged)

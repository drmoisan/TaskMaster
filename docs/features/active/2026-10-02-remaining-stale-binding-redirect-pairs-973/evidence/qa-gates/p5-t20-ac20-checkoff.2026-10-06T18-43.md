# P5-T20 AC20 check-off

Timestamp: 2026-10-06T18-43
Command: Grep tool over spec.md `^- \[x\] AC20 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Read of the named artifacts
EXIT_CODE: 0
Output Summary: AC20 met and checked off. Both Rebuild projections record EXIT_CODE 0, USING_DIAG_LINES 0 (CS0246/CS0103/CS0104/CS0234/CS1061), UTILITIESCS_CSC_LINES 2 (CoreCompile executed for UtilitiesCS), NEWFILE_CSC_LINES 2 and SKIP_CORECOMPILE_LINES 0. The CSharpier check after Parts F and H exited 0 with 1638 files checked, which is the baseline 1637 plus one.

Artifacts read:
- evidence/qa-gates/msbuild-analyzers.md: EXIT_CODE 0; USING_DIAG_LINES 0; UTILITIESCS_CSC_LINES 2; NEWFILE_CSC_LINES 2; SKIP_CORECOMPILE_LINES 0.
- evidence/qa-gates/msbuild-treatwarningsaserrors.md: EXIT_CODE 0; USING_DIAG_LINES 0; UTILITIESCS_CSC_LINES 2; NEWFILE_CSC_LINES 2; SKIP_CORECOMPILE_LINES 0.
- evidence/qa-gates/csharpier-check.md: EXIT_CODE 0; `CHECKED-COUNT: 1638`; `CSHARPIER-BASELINE-COUNT: 1637`; `CHECKED-BASELINE-PLUS-ONE: True`.

SPEC-LINE: `- [x] AC20 (compile and format proof for the directive removals; ...` (criterion text unchanged)

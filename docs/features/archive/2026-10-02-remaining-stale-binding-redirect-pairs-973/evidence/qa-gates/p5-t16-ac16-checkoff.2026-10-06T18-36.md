# P5-T16 AC16 check-off

Timestamp: 2026-10-06T18-36
Command: Grep tool over spec.md `^- \[x\] AC16 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Read of the named artifacts
EXIT_CODE: 0
Output Summary: AC16 met and checked off. The .cs diff is exactly the six section 5 paths (no directive restored): four Part F files at 0/1, CategoryClassifierGroup.cs at 1/98, and the new file. The two Category files are 442 and 106 lines, both under 500. No SVGControl config, nothing under scripts/dependencies and no new PowerShell file is in the diff. The test file is 402 lines, CRLF, with the minimum hunk at 293. Every edited config has the expected numstat, `w/crlf` and an unchanged BOM. The split census shows ADDED-LINES 1, MOVE-DIFFERENCES 0 and REMOVED-USING-LINES 2.

Artifacts read:
- evidence/qa-gates/p4-t12-footprint.2026-10-06T18-34.md (EXIT_CODE 0): the quoted `.cs` name list equals the six section 5 paths and `.cs` porcelain is empty; RESTORE-ADJUSTED none; the SVGControl and scripts/dependencies diff and porcelain are empty; `.ps1/.psm1/.psd1` name-status is only `M` for the test file; the test file is 402/402 with hunk old-starts 293, 331 and 334; the 15 configs match the section 8 numstat, `w/crlf` and the P0-T5 BOM.
- evidence/regression-testing/p1-t5-testfile-verification.2026-10-03T10-58.md (EXIT_CODE 0): w/crlf; every diff hunk starts at old line 293 or later, so the Fizzler It (lines 246-273) and the in-memory fixture Describe blocks are unchanged.
- evidence/regression-testing/graph-usings-grep-pass-after.md (EXIT_CODE 0): numstat 0/1 for the four Part F files with equal line and CR counts.
- evidence/other/category-classifier-group-split-census.md (EXIT_CODE 0): `ADDED-LINES: 1`, `MOVE-DIFFERENCES: 0` (the two blank lines ahead of the moved `#region` are counted in the moved block's removal under spec Planner Amendment 5), `REMOVED-USING-LINES: 2`.

SPEC-LINE: `- [x] AC16 (footprint and hygiene).` (criterion text unchanged)

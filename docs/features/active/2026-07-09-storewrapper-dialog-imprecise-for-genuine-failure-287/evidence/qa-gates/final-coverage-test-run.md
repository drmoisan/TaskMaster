Timestamp: 2026-09-01T05-10
Command: pwsh -NoProfile -Command '& "scripts/vscode/Invoke-MSTestWithCoverage.ps1" -SearchRoot . -Configuration Debug -CoverageOutput "coverage/post-change.cobertura.xml" *>&1 | Tee-Object -FilePath "coverage/p3-testrun.log"'; pwsh -NoProfile -Command '$x = [xml](Get-Content -Raw "coverage/post-change.cobertura.xml"); $c = $x.DocumentElement; "line-rate=" + $c.GetAttribute("line-rate"); "branch-rate=" + $c.GetAttribute("branch-rate"); "lines-covered=" + $c.GetAttribute("lines-covered"); "lines-valid=" + $c.GetAttribute("lines-valid")'
EXIT_CODE: 0
Output Summary: Discovery line: "Discovered 9 test assemblies." Total tests: 6912, Passed: 6912, Test Run Successful (equals BASELINE TOTAL 6900 plus twelve, the number of new [TestMethod] declarations added by this plan). Failed-test lines: none (grep for `^  Failed` returned zero lines). Both required post-processing lines present: "Post-processing coverage XML for Koverage compatibility..." and "Done. Coverage artifact: ...coverage\post-change.cobertura.xml".

Coverage comparison:
- baseline line-rate=0.853035 -> post-change line-rate=0.85297 (delta -0.000065, within the -0.0005 tolerance; post-change >= baseline - 0.0005 = 0.852535)
- baseline branch-rate=0.79289 -> post-change branch-rate=0.79293 (delta +0.00004, above baseline; post-change >= baseline - 0.0005 = 0.79239)
- baseline lines-covered=54851, lines-valid=64301 -> post-change lines-covered=54869, lines-valid=64327 (both covered and valid line counts increased, consistent with new tested code plus new test code entering the denominator)

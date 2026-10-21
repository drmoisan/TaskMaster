# CI Pester coverage record for issue 953 (AC6 coverage source)

Timestamp: 2026-10-02T07-55
Command: gh run view 36980297940 --job 110753053226 --log (Grep of the PESTER and COVERAGE summary lines); gh run download 36980297940 --name pester-coverage (JaCoCo document, class counters for scripts/dependencies/BindingRedirectVerification)
EXIT_CODE: 0
Output Summary:
- COVERAGE-SOURCE: CI (AC6: line coverage "is not measured locally").
- CI run ID: 36980297940 (pull request 974). Head SHA measured: 119888f0efc69732402f1fb3c034c53a05c42965.
- Pester job result: PESTER Passed=393 Failed=0 Skipped=0 Total=393.
- Whole-folder line coverage printed by the job: COVERAGE LinePercent=94.63 Covered=1762 Total=1862 (gate floor 80 in _pester.yml; repository rule 85).
- Per-file, from the JaCoCo artifact pester-coverage, class scripts/dependencies/BindingRedirectVerification: LINE missed=0 covered=41 (100 percent); INSTRUCTION missed=0 covered=52 (100 percent). Per-method LINE: script body 4 of 4, ConvertTo-ReferenceVersionMap 14 of 14, Find-StaleBindingRedirect 23 of 23.
- New-code target of 90 percent (CLAUDE.md UT2): met. Repository floor of 85 percent: met.
- Other jobs on the same run: actionlint, build-analyzers, build-nullable, format-check, hygiene, mstest-coverage all pass.

Timestamp: 2026-10-02T05-17
Command: Grep `<testcase name="Test-BackupFilePath\.[^"]*" status="Passed"` (count) and Grep of the alternation of the four Invoke-RepositoryHygieneMain names followed by `" status="Passed"` (count), both against artifacts/pester/pester-junit.xml
EXIT_CODE: 0
Output Summary: Test-BackupFilePath passed testcases = 10. The four Invoke-RepositoryHygieneMain passed testcases from P1-T3 = 4.
Statement-to-test map: the Test-BackupFilePath body is exercised by its 10 passing tests (true branch of the comparison by the 4 positive tests, false branch by the 6 negative tests). The new `if` in Invoke-RepositoryHygieneMain is exercised on its true side by the negative control (a) and the combined case (d), and on its false side by the lookalike case (b); the governance skip ahead of it by case (c). CI measures scripts/hygiene line coverage (_pester.yml, LINE at 80); this document does not.

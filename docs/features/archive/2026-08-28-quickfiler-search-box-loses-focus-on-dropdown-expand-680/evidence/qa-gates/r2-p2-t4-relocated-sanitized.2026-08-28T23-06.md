Timestamp: 2026-08-28T23-06
Command: D1 `Set-TrxSanitized` routine applied to evidence/regression-testing/r-p2-t3/p2-t3.trx; followed
by rg -a -i -F -- '<literal>' sweep (three D1 classes); [xml](Get-Content -Raw -Path <path>);
Select-String -SimpleMatch checks for raw and escaped placeholder forms
EXIT_CODE: 0
Output Summary: Routine completed with no exception. BytesBefore=50421, BytesAfter=47734 (strictly
less). Follow-up rg sweep for all three D1 literal classes returned 0 combined hits. XML parse: PASS (no
exception). Raw unescaped placeholder search: 0 matches. Escaped placeholder search: 111 matches
(>= 1 required). All acceptance conditions hold.

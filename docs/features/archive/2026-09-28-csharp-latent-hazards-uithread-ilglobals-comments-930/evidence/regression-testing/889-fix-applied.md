# #889 fix applied ([P1-T4])

Timestamp: 2026-09-29T09-11
Command: CMD-RETURN-LINE; git diff --numstat ac819907f479ee18026993054e714dc2e056142f -- UtilitiesCS/Threading/UiThread.cs; @(Get-Content -LiteralPath UtilitiesCS/Threading/UiThread.cs).Count
EXIT_CODE: 0
Output Summary:
- GUARD_COUNT=2 (1 before)
- RETURN_LINE_MATCHES=1 (RETURN_LINE_NUMBER=198)
- Line immediately after the return line (199): `                        && _dispatcher is not null`
- Numstat: `2	0	UtilitiesCS/Threading/UiThread.cs` (two added, zero deleted)
- Line count: 308

Edit: one comment line (`// The null test mirrors the captured-context exit: null must never match null.`) inserted after the original line 196, and the operand line `&& _dispatcher is not null` inserted after the return line, exactly as the "[P1-T4] production edit" block states. No original line changed.

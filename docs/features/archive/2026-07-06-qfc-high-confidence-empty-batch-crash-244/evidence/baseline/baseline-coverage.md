# Phase 0 — Coverage Baseline (Issue #244)

Timestamp: 2026-07-06T11-53

Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage

EXIT_CODE: 0

Output Summary: Total tests: 469, Passed: 469, Failed: 0. Test Run Successful.

Numeric coverage (Cobertura-format rerun, same test set, same result 469/469 passed): a supplemental run with a Cobertura-format `/Settings:` runsettings (`Format=Cobertura`, excluding `[ExcludeFromCodeCoverage]`/DebuggerHidden/DebuggerNonUserCode/CompilerGenerated/GeneratedCode-attributed members from the denominator) was used to obtain a numeric percentage, because the default `/EnableCodeCoverage` binary `.coverage` output does not print a percentage to the console. Per-package Cobertura line-rate for the production `QuickFiler` assembly (the package exercised by `QuickFiler.Test`): **72.46%** (line-rate `0.72456993268511594`, 913 complexity units). `QfcDatamodel` is `[ExcludeFromCodeCoverage]`-annotated at the class level, so its lines (including the `InitEmailQueue` method targeted by this fix) are excluded from this denominator both before and after the change; this baseline figure is the AC5 no-regression reference for Phase 3 (P3-T4).

Command (Cobertura variant, informational only): & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage /Settings:<cov.runsettings> — same 469/469 pass result, EXIT_CODE 0.

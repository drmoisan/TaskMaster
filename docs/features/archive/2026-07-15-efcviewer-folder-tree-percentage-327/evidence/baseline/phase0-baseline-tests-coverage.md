# Phase 0 Baseline — Tests + Coverage (P0-T5)

Timestamp: 2026-07-16T00-20

Command (coverage, Cobertura via dotnet-coverage wrapping vstest):
dotnet-coverage collect --settings cov.settings.xml --output baseline.cobertura.xml --output-format cobertura -- "vstest.console.exe" UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Settings:cov.runsettings

- vstest.console.exe: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
- cov.runsettings: MSTest Parallelize Workers=4, ClassLevel (deterministic; avoids the known UtilitiesCS.Test timing-test flakiness under high parallelism).
- cov.settings.xml: dotnet-coverage module excludes mirroring TaskMaster.runsettings (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest).

EXIT_CODE: 0

Output Summary:
- Test result: Total tests 4727, Passed 4727, Failed 0.
- Baseline repository LINE coverage: 77.46% (line-rate 0.7746413861667377; lines-covered 109085 / lines-valid 140820).
- Baseline repository BRANCH coverage: 52.94% (branch-rate 0.5294357137040958; branches-covered 13004 / branches-valid 24562).
- Packages in report: 13 (includes vendored Swordfish/SVGControl and other non-first-party modules, so the raw repo denominator is larger than the CLAUDE.md "testable denominator").

Flakiness note (pre-existing, not feature-related):
- Without any coverage instrumentation the full suite passes clean (vstest exit 0; the "Failed loading language 'eng'" lines are Tesseract OCR warnings, not test failures).
- Under coverage instrumentation WITHOUT module excludes, 20 Deedle/FSharp DataFrame tests fail (DeedleDoodles, FromArray2D_*, GetEmailDataInView*, DataFrame Exclude/DropFirstN, InitEmailQueue_*). These fail only because dotnet-coverage instruments the Deedle/FSharp assemblies; the repo's own TaskMaster.runsettings already excludes those modules from coverage. Applying the same module excludes to dotnet-coverage yields 4727/4727 passing. None of these tests relate to this feature's host-neutral modules (UtilitiesCS/OutlookObjects/Folder/).

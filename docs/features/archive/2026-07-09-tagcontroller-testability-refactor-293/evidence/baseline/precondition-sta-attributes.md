# Precondition — STA Test Attributes (P0-T11)

Timestamp: 2026-07-09T21-56

Command: `grep -a -o "STATestClassAttribute|STATestMethodAttribute" packages/MSTest.TestFramework.4.2.2/lib/net462/MSTest.TestFramework.dll | sort -u`
EXIT_CODE: 0

Output Summary: The `Tags.Test` package `MSTest.TestFramework 4.2.2` net462 assembly exports
both `Microsoft.VisualStudio.TestTools.UnitTesting.STATestClassAttribute` and
`STATestMethodAttribute`. Test-scoped STA is available.

Acceptance decision: `[STATestClass]` / `[STATestMethod]` ARE available in 4.2.2. Therefore the
dedicated STA files (P6-T5 `TagControllerRendering.StaTests.cs`, P6-T6
`CheckBoxControllerWiring.StaTests.cs`) MUST use the attribute approach (test-scoped STA),
NOT the `.runsettings` assembly-wide fallback. The runsettings fallback is not used.

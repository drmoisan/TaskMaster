# Fail-before exception dossier (P1-T3, expect-fail)

Timestamp: 2026-09-30T12-24
Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger)
EXIT_CODE: 1
ExpectedExitCode: 1

WhyFailingRunImpossible: A runtime-red run of the pre-fix behavior would execute the real Directory.CreateDirectory with a rooted path on the system drive, which creates a directory outside the repository or, when the create is denied, raises UnauthorizedAccessException and reaches the modal YesNoToAll.ShowDialog. The AC5 side-effect-free control of P1-T14 to P1-T17 is the runtime fail-before equivalent.

Output Summary:
The state built is the tests rewritten (P1-T1, P1-T2) with production untouched. The build failed because the three-argument call matches no overload.
MSBUILD_EXIT_CODE: 1
CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 0
NAMED_ERROR_LINES: 4
CS1501_LINES: 4 (observation: the compiler reported CS1501, no overload takes 3 arguments)
DLL_ADVANCED: False (observation)

ABSENCE-PROOF: P0-T12 recorded `Action<string>createDirectory` = 0 in UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs (no overload accepting a delegate exists) and `destinationPath=Path.Combine(GetRepositoryRoot()` = 1 in UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs (the pre-fix test derived its destination from the repository root).

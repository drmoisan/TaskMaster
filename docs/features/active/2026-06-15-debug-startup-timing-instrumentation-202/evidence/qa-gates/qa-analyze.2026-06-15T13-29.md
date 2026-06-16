# QA Gate — Analyze (.NET Analyzers) (Issue #202, P2-T2)

Timestamp: 2026-06-15T13-29

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

Output Summary:

- Build succeeded (MSBUILD exit 0). Solution compiled with analyzers enabled.
- No analyzer diagnostics were introduced by the split. Specifically: zero IDE0005/CS8019
  unused-using diagnostics and zero unused-private-member diagnostics in either
  `ApplicationGlobalsTests.cs` or `ApplicationGlobalsStartupTimingTests.cs`.
- The only warnings touching the two AppGlobals test files are CS8632 ("nullable annotation
  outside a #nullable context") on the `IList<string>?` field/parameter annotations in
  `TestableApplicationGlobals`. These are pre-existing: they were present on the original
  file's `TestableApplicationGlobals` before the split and were carried verbatim into the new
  file. They are warnings (not errors) under the analyze gate, the build passed, and they do
  not represent a new diagnostic class. Under the nullable gate (P2-T3, `/p:Nullable=enable`)
  the `#nullable` context is enabled, so CS8632 does not fire there (verified in P2-T3).
- No errors anywhere in the solution build.

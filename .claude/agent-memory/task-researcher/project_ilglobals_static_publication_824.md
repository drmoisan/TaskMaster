---
name: ilglobals-static-publication-824
description: "#824 ILGlobals race research: identity-stability gate is the only order-independent RED test; UtilitiesCS has no NetAnalyzers so CA rules cannot fire; MethodBodyReader_Tests.cs is 489/500"
metadata:
  type: project
---

Issue #824 (`ILGlobals.LoadOpCodes` unsynchronised static race), researched 2026-09-08.

**Findings that are not obvious from re-reading the code:**

1. **The deterministic gate is an identity test, not a concurrency test.**
   `var before = ILGlobals.singleByteOpCodes; ILGlobals.LoadOpCodes(); after.Should().BeSameAs(before);`
   fails on the unfixed tree in **both** possible orderings (if no prior class called
   `LoadOpCodes`, `before` is null and `after` is a fresh array; if one did, `after` is a
   *different* array). That order-independence is what makes it a real gate. A threaded
   sampling test passes on the buggy code most of the time and is disqualified.
   **Why:** the failure was seen once in ten full-suite runs, so any run-count-keyed gate
   cannot fail in one direction.
   **How to apply:** whenever a "publish-once" static is the defect, look for an invariant
   that is violated on *every* code path (here: reallocation), not for a way to provoke the race.

2. **`/p:EnableNETAnalyzers=true` is inert for the legacy non-SDK projects here.**
   `UtilitiesCS.csproj` references only Meziantou, Roslynator, AsyncFixer, BannedApiAnalyzers
   and SonarAnalyzer as `<Analyzer Include>` items — no `Microsoft.CodeAnalysis.NetAnalyzers`.
   So **no `CA`-prefixed diagnostic (CA2211 etc.) can ever be produced for UtilitiesCS**, and
   `.editorconfig`'s `dotnet_analyzer_diagnostic.severity = suggestion` catch-all holds every
   third-party rule below the error bar anyway (MSTEST0032 is still the only rule above suggestion).
   **How to apply:** do not cite a CA rule as a blocker or a justification in any
   UtilitiesCS/TaskMaster legacy-csproj analysis without first checking the `<Analyzer Include>` list.

3. **`UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs` was 489/500 lines.**
   Its sibling `ILGlobals_Tests.cs` was 133. New SDILReader tests belong in the latter, which
   also avoids any `.csproj` `<Compile Include>` edit.

4. Production folder is `NewtonsoftHelpers\SDIL Reader\` (with a space); the test folder is
   `NewtonsoftHelpers\SDILReader\` (no space). Easy to get wrong in a csproj entry or a glob.

5. `ILGlobals.modules` has **zero** references repo-wide; `ILGlobals.Cache` has exactly one
   (a test null-check). Both are latent unsynchronised public mutable statics — flagged as a
   follow-up, deliberately out of #824's scope.

Related: [[feedback-exemption-audit-check-proven-techniques]]

---
name: console-out-and-rs0030-promotion-826
description: Issue #826 research — Directory.Build.props DOES exist (CLAUDE.md says it doesn't), no GenerateDocumentationFile anywhere so IDE0005 never fires, TimeoutAfter is repo-local and is the prescribed remedy not a hazard (verified 2026-09-08)
metadata:
  type: project
---

Verified 2026-09-08 researching issue #826 (console-out aggressors + banned-symbol promotion).
Five facts that repeatedly flip the answer to "will this change break a gate".

1. **`Directory.Build.props` EXISTS at repo root** (18 lines, sets only
   `RxUseUnsupportedPackagesConfig`, added under #730), and `Directory.Build.targets` too (signing).
   `CLAUDE.md` §C#1.3 asserts "there is no `Directory.Build.props`" — that clause is STALE. The
   `<Nullable>` half of the same sentence is still true. This matters because Directory.Build.props
   is the only solution-wide MSBuild property injection point; reject `WarningsNotAsErrors` there on
   the merits, not on a false absence claim.

2. **No `GenerateDocumentationFile` in ANY of the 18 `.csproj`.** Therefore IDE0005 (unnecessary
   using directive) is never reported by a command-line msbuild for any project. Unused `using`
   directives left behind by a deletion CANNOT fail a gate. Generalizes the QuickFiler.Test-only
   version of this in [[project_analyzer_severity_ceiling_and_runsettings_split]].

3. **The real deletion hazard is CS0169/CS0414, not IDE0005.** Those are COMPILER warnings, unaffected
   by the `.editorconfig` `suggestion` ceiling, and `/p:TreatWarningsAsErrors=true` (step 3 /
   `_build-nullable.yml`) promotes them to errors. Deleting a field's last consumer without deleting
   the field breaks the nullable gate.

4. **`TimeoutAfter` is repository-local, not BCL** — 4 overloads in `UtilitiesCS/Threading/TimeOutTask.cs`
   (`namespace UtilitiesCS`, `public static class TimeOutTask`), two of which take a `TimeProvider`
   and are documented as the FakeTimeProvider determinism seam. It is the cure the BannedSymbols
   messages point toward, so it must never be banned. Its 62 hits are adoption, not debt.

5. **`_build-analyzers.yml` passes NO `TreatWarningsAsErrors`**, so analyzer severity promotions break
   the NULLABLE gate, not the analyzer gate. Counter-intuitive; check which workflow before claiming
   a gate failure.

Also: `new CancellationTokenSource(` splits 157 parameterless / 13 parameterized, and all 13
parameterized are production-only with 10 of them inside `TimeOutTask.cs` itself.

**Why:** #826 asked to promote RS0030 above `suggestion`; that is unreachable without clearing ~143
usages or bulk-suppressing, and `.claude/rules/csharp.md:87` records an explicit precedent of
DECLINING `<WarningsNotAsErrors>` (for CS8032 / SecurityCodeScan).

**How to apply:** before proposing any analyzer-severity change or any "delete the now-unused
declaration" cleanup in this repo, check (2) and (3) first — they invert the usual expectations. See
also [[project_analyzer_severity_ceiling_and_runsettings_split]] for the severity ceiling itself.

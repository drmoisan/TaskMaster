---
name: 398-test-split-gate-gotchas
description: #398 test-file split remediation — pre-existing CS2002 duplicate Compile, /EnableCodeCoverage yields no branch% + empty cobertura, cobertura-runsettings needs Workers=4
metadata:
  type: project
---

Issue #398 minor-audit remediation (split two >500-line breadcrumb test files into `sealed partial`
pairs + wire csproj + regenerate artifacts/csharp/coverage.xml JaCoCo). Non-obvious gate gotchas:

- **Pre-existing CS2002 duplicate Compile Include.** UtilitiesCS.Test.csproj carries
  `OutlookObjects\Folder\PercentageFormatterTests.cs` TWICE (lines 290 + 340 at HEAD; `git show HEAD:`
  confirms). It is latent — surfaces as CS2002 *only when UtilitiesCS.Test is recompiled*. A test-file
  edit forces that recompile, so the analyzer build (no TWAE) shows a NEW warning vs the incremental
  baseline. It is out of the R1 scope lock (csproj limited to the 2 additions), does NOT fail the gates
  (analyzer build has no TWAE; the ratified nullable gate is the solution *incremental* Build, which
  leaves UtilitiesCS.Test up-to-date). Left unfixed, escalated. **A full solution Rebuild under
  Nullable+TWAE is a known pre-existing-blocker** (UtilitiesCS Obsolete/BayesianClassifier.cs CS8618/
  CS8766 debt + this CS2002), so never Rebuild to "verify" — use the incremental solution Build.

- **/EnableCodeCoverage gives no branch% and its .coverage won't convert.** The default vstest collector
  records only *block* coverage (line_coverage/block_coverage per module), no branch. `dotnet-coverage
  merge <file>.coverage -f cobertura` produces `<packages />` (empty) even though `-f xml` yields a 33MB
  native file with data. To get true line%+branch% use the **Cobertura-output form of the same Code
  Coverage collector** via a throwaway runsettings (DynamicCoverageDataCollector, `<Format>Cobertura`,
  `ModulePaths Include` UtilitiesCS.dll+QuickFiler.dll, `Attributes Exclude` ExcludeFromCodeCoverage).
  Its Cobertura root `lines-covered/valid` + `branches-covered/valid` are the authoritative first-party
  denominator (line 86.54%, branch 80.26–80.85%). Convert that root aggregate to JaCoCo with a SINGLE
  report-level `<counter type=LINE>`+`<counter type=BRANCH>` (the hook sums all `//counter`, so single
  level = exact, no double-count); per-`<line>` dedup drifts ~0.2pp from the tool's own aggregate.

- **Cobertura runsettings run needs MSTest Workers=4.** At default parallelism + coverage instrumentation
  the timing test `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` throws
  TaskCanceledException after ~22s (documented UtilitiesCS.Test flake). Add
  `<RunConfiguration><MaxCpuCount>4</MaxCpuCount></RunConfiguration>` + `<MSTest><Parallelize><Workers>4`
  to the runsettings → deterministic 5061/5061. `/EnableCodeCoverage` passed 5061 without it (timing luck).

- MSYS_NO_PATHCONV=1 makes vstest/dotnet-coverage receive `/c/Users/...` literally → files land under
  `C:\c\Users\...` (reachable as `/c/c/Users/...`); search there for the emitted `.coverage`/`.cobertura.xml`.

# QA Gate — Issue #176 (ci-flaky-test-isolation)

- Timestamp (UTC): 2026-06-08T13-29-05Z (test runs through 2026-06-08T13-35-38Z)
- Branch: bug/ci-flaky-test-isolation-176
- Baseline compared: evidence/baseline/2026-06-08T13-21-38Z

## Scope (only files changed)

- UtilitiesCS.Test/EmailIntelligence/OlFolderClassifierGroup_Tests.cs
- UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs

No production files changed. Confirmed via `git diff --name-only`.

## Toolchain results

1. CSharpier `format .` then `check` on scoped files: clean (Checked 2 files, exit 0).
2. Analyzers: Build succeeded, 0 Error(s). 19 pre-existing test-project warnings,
   0 in scoped files.
3. Nullable + TreatWarningsAsErrors: 905 Error(s) total — identical to baseline
   (905). The single scoped-file diagnostic is the pre-existing
   `OlFolderClassifierGroup_Tests.cs(29,49)` present on baseline. Delta = 0.
4. MSTest with coverage:
   - Affected-class set (OlFolder + adapter + wrapper) with coverage: 14/14 passed,
     5 consecutive runs, deterministic.
   - PhysicalFileSystemAdapters_Tests in isolation: 4/4 passed, 10 consecutive runs.

## Delta vs baseline (zero-regression gate)

- Analyzer delta: 0 new findings.
- Compiler/nullable delta: 0 new diagnostics (905 == 905; same single pre-existing
  scoped diagnostic).
- Per-file coverage delta (cobertura line-rate), all equal to baseline:
  - OlFolderClassifierGroup.cs: 0.3889 == 0.3889
  - BayesianClassifierGroup.cs: 0.1783 == 0.1783
  - PhysicalFileInfoAdapter.cs: 0.8909 == 0.8909 (write lines 75/103/114 still hits=1)
  - PhysicalDirectoryInfoAdapter.cs: 0.8659 == 0.8659
  - FileInfoWrapper.cs: 1.0 == 1.0
- Full-assembly MSTest delta: baseline 863 unique failing names vs post-change 864.
  The only added name is `GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension`
  (ShellUtilities_Tests, out of scope), which fails with
  `System.ArgumentException: Win32 handle ... is not valid` — a flaky Win32 shell-icon
  test that flips between runs under heavy parallel load. It is unrelated to the two
  changed files. Both target tests behave identically to baseline.

## Target test outcomes

- BuildClassifiersAsync_WithFixtureAndFolderConfig_StoresBuiltFolderClassifier:
  passes deterministically (14/14 over 5 coverage runs) when its binding context loads.
  In the uninstrumented full-assembly run it is blocked by the pre-existing
  `System.Threading.Tasks.Extensions 4.2.0.1` Moq binding-redirect issue, identically
  on baseline — not the concurrency defect and not introduced by this change.
- PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo: passes
  deterministically (10/10 in isolation, and in coverage runs). No longer opens a
  write/append handle on the shared TaskMaster.sln.

## Environmental caveat (verification limitation)

The local sandbox cannot run the full Moq-dependent `UtilitiesCS.Test` assembly to a
clean pass without coverage instrumentation, because the vstest host intermittently
fails to apply the `System.Threading.Tasks.Extensions 4.2.0.1 -> 4.2.4.0` binding
redirect (TypeInitializationException in Moq.Async.AwaitableFactory). This is a
pre-existing host/config interaction present identically on baseline and unrelated to
the source change. The zero-regression comparison above is made against the same
environment on the same commit, so the binding noise cancels in the delta.

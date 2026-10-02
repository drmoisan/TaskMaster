# P1-T15 Suite run after the config edits (AC5 pass-after evidence; AC4 evidence through test 14)

Timestamp: 2026-10-02T03-21
Command: pwsh -NoProfile -Command 'Remove-Item -LiteralPath <execution-worktree-root>/artifacts/pester/pester-junit.xml -ErrorAction SilentlyContinue' (CMD-JUNIT-DELETE); MCP mcp__drm-copilot__run_poshqc_test with workspace_root `<execution-worktree-root>` and scan_folders `["tests/scripts/dependencies"]`; CMD-JUNIT-READ Greps (`<testsuites `, `<testsuite `, `status="Failed"` with -A 2) over `<execution-worktree-root>/artifacts/pester/pester-junit.xml`
EXIT_CODE: 0
ExpectedExitCode: 0

CMD-JUNIT-DELETE: the pwsh invocation printed nothing and reported no error. GLOB-BLIND: the Glob tool does not return this gitignored file (recorded at P0-T12), so the absence check is not discriminating. The JUnit root and testsuite elements in this worktree's document carry no `timestamp` attribute, so the timestamp comparison named in the delegation is not available; freshness is shown instead by the document content differing from the P1-T3 document read just before the delete: JUNIT-ROOT failures 0 now against 2 at P1-T3 and root `time` 5.460 against 5.998. A document surviving the delete could not report both changes.

Payload (verbatim, worktree root replaced per C4):

```text
{"ok":true,"tool":"run_poshqc_test","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC test against '<execution-worktree-root>' with 1 selected scan folder(s)."}
```

EXIT_CODE derivation (C3): payload `ok` true and JUNIT root `failures=0`, so 0.

CMD-JUNIT-READ:

```text
JUNIT-ROOT tests=151 failures=0 errors=0 disabled=0
JUNIT-SUITE AnalyzerItemRepair.Tests.ps1 tests=13 failures=0 skipped=0
JUNIT-SUITE BindingRedirectVerification.Tests.ps1 tests=14 failures=0 skipped=0
JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=14 failures=0 skipped=0
JUNIT-SUITE DependabotConfig.Tests.ps1 tests=17 failures=0 skipped=0
JUNIT-SUITE PackageCompatibility.Tests.ps1 tests=8 failures=0 skipped=0
JUNIT-SUITE PackageGraph.Tests.ps1 tests=32 failures=0 skipped=0
JUNIT-SUITE ProjectConsistency.Tests.ps1 tests=18 failures=0 skipped=0
JUNIT-SUITE Repair-PackageManifestConsistency.Tests.ps1 tests=31 failures=0 skipped=0
JUNIT-SUITE RepositoryTreeConsistency.Tests.ps1 tests=4 failures=0 skipped=0
JUNIT-NOTPASSED: none
```

The `<testsuites ` Grep (a) matched exactly one line (line 2). The `<testsuite ` Grep (b) matched 9 lines (3, 29, 56, 83, 113, 134, 179, 210, 254), none reported as omitted. The `status="Failed"` Grep (c) returned no match.

RepositoryTreeConsistency.Tests.ps1 (4) and every other pre-existing suite equal their P0-T12 BASELINE-SUITE counts (13, 14, 17, 8, 32, 18, 31, 4) with `failures=0`. The root total 151 equals 137 baseline plus 14 new tests.

Test 14 passed (no KNOWN-DEBT-DRIFT): over the 17 app.config files and 18 csproj files the stale-redirect set equals the 15 recorded section 7 pairs, none is Fizzler or Unsafe, the unverifiable set equals the recorded three names, and the examined count equals the independent `<bindingRedirect` element count.

COVERAGE-MEASUREMENT: deferred to the CI Pester job (_pester.yml); no local figure

Acceptance: `ok` true; JUNIT-ROOT failures=0; `BindingRedirectVerification.Tests.ps1 tests=14 failures=0 skipped=0`; every other suite equals its baseline; `JUNIT-NOTPASSED: none` after (a) matched exactly one line; coverage literal present.

Output Summary: Green after the sweep: 151 tests, 0 failures, 0 errors; tests 13 and 14 pass; eight pre-existing suites unchanged from baseline. AC5 pass-after and AC4 (known-debt ratchet) evidence. Coverage is deferred to the CI Pester job.

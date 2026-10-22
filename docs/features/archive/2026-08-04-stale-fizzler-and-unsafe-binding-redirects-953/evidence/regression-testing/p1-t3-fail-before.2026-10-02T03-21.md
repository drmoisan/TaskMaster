# P1-T3 [expect-fail] Suite run before the config edits (AC5 fail-before evidence)

Timestamp: 2026-10-02T03-21
Command: pwsh -NoProfile -Command 'Remove-Item -LiteralPath <execution-worktree-root>/artifacts/pester/pester-junit.xml -ErrorAction SilentlyContinue' (CMD-JUNIT-DELETE); MCP mcp__drm-copilot__run_poshqc_test with workspace_root `<execution-worktree-root>` and scan_folders `["tests/scripts/dependencies"]`; CMD-JUNIT-READ Greps (`<testsuites `, `<testsuite `, `status="Failed"` with -A 2) over `<execution-worktree-root>/artifacts/pester/pester-junit.xml`, then the Read tool at the two failed testcase lines
EXIT_CODE: 1
ExpectedExitCode: 1

CMD-JUNIT-DELETE: the pwsh invocation printed nothing and the Bash tool reported no exit-code error (the document did exist from the Phase 0 run, so the delete had an effect). GLOB-BLIND: the Glob tool does not return this gitignored file (recorded at P0-T12), so the absence check is not discriminating; freshness is shown by the document content, which carries a `BindingRedirectVerification.Tests.ps1` suite that did not exist at the P0-T12 run (9 suites, 151 tests now against 8 suites, 137 tests then).

Payload (verbatim, worktree root replaced per C4; the tool returned it as an error result):

```text
{"ok": false, "tool": "run_poshqc_test", "workspace_root": "<execution-worktree-root>", "summary": "Command exited with code 2."}
```

EXIT_CODE derivation (C3): payload `ok` false and JUNIT root `failures=2`, so 1. The tool's own exit code 2 is the count of failed tests as the runner reported it and is recorded as an observation, not as this artifact's EXIT_CODE.

CMD-JUNIT-READ:

```text
JUNIT-ROOT tests=151 failures=2 errors=0 disabled=0
JUNIT-SUITE AnalyzerItemRepair.Tests.ps1 tests=13 failures=0 skipped=0
JUNIT-SUITE BindingRedirectVerification.Tests.ps1 tests=14 failures=2 skipped=0
JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=14 failures=0 skipped=0
JUNIT-SUITE DependabotConfig.Tests.ps1 tests=17 failures=0 skipped=0
JUNIT-SUITE PackageCompatibility.Tests.ps1 tests=8 failures=0 skipped=0
JUNIT-SUITE PackageGraph.Tests.ps1 tests=32 failures=0 skipped=0
JUNIT-SUITE ProjectConsistency.Tests.ps1 tests=18 failures=0 skipped=0
JUNIT-SUITE Repair-PackageManifestConsistency.Tests.ps1 tests=31 failures=0 skipped=0
JUNIT-SUITE RepositoryTreeConsistency.Tests.ps1 tests=4 failures=0 skipped=0
JUNIT-NOTPASSED Repository binding redirects (issue 953).names 1.3.1.0 in every Fizzler binding redirect across the repository app.config files
JUNIT-NOTPASSED Repository binding redirects (issue 953).reports exactly the recorded known-debt set and unverifiable set over every app.config against every csproj Reference
```

The `<testsuites ` Grep (a) matched exactly one line (line 2). The `<testsuite ` Grep (b) matched 9 lines (3, 29, 62, 89, 119, 140, 185, 216, 260), none reported as omitted (the testsuite lines were printed in full). The `status="Failed"` Grep (c) matched lines 53 and 57. The Grep context lines for the two failures were reported as `[Omitted long context line]`, so the messages were transcribed from the Read tool at offsets 53 and 57 (limit 3), as the long-line rule requires.

JUNIT-MESSAGE (test 13, Read tool, line 54; worktree prefix not present in the message attribute):

```text
Expected 0, because these configs redirect Fizzler to another version: QuickFiler=1.3.0.0; QuickFiler.Test=1.3.0.0; SVGControl.Test=1.3.0.0; Tags=1.3.0.0; TaskMaster=1.3.0.0; TaskTree=1.3.0.0; TaskVisualization=1.3.0.0; TaskVisualization.Test=1.3.0.0; ToDoModel=1.3.0.0; ToDoModel.Test=1.3.0.0; UtilitiesCS.Test=1.3.0.0, but got 11.
```

It begins `Expected `, contains `but got 11` and `1.3.0.0`, and names each of the eleven directories QuickFiler, QuickFiler.Test, SVGControl.Test, Tags, TaskMaster, TaskTree, TaskVisualization, TaskVisualization.Test, ToDoModel, ToDoModel.Test, UtilitiesCS.Test.

JUNIT-MESSAGE (test 14, Read tool, line 58; the line is long and was read in full, only the failure `message` attribute is transcribed; the `Expected @(...)` and `but got @(...)` parts are Pester-truncated renderings with `...N more` and carry no gated content):

```text
Expected @('Azure.Core|1.62.0.0', 'Microsoft.Bcl.Memory|10.0.0.7', ... 5 more), because the stale redirect set must equal the recorded known debt; observed: Azure.Core|1.62.0.0; Fizzler|1.3.0.0; Microsoft.Bcl.Memory|10.0.0.7; Microsoft.Bcl.Numerics|10.0.0.5; Microsoft.Extensions.Diagnostics.Abstractions|10.0.0.5; Microsoft.Identity.Client.Extensions.Msal|4.89.0.0; Microsoft.Identity.Client|4.89.0.0; Microsoft.IdentityModel.Abstractions|8.22.0.0; Microsoft.IdentityModel.JsonWebTokens|8.22.0.0; Microsoft.IdentityModel.Logging|8.22.0.0; Microsoft.IdentityModel.Protocols.OpenIdConnect|8.22.0.0; Microsoft.IdentityModel.Protocols|8.22.0.0; Microsoft.IdentityModel.Tokens|8.22.0.0; Microsoft.IdentityModel.Validators|8.22.0.0; System.ClientModel|1.3.0.0; System.IdentityModel.Tokens.Jwt|8.22.0.0, but got @('Azure.Core|1.62.0.0', 'Fizzler|1.3.0.0', ... 6 more).
```

The text between `observed: ` and the following `, but got` holds exactly 16 `;`-separated entries: the 15 section 7 pairs (Azure.Core|1.62.0.0, Microsoft.Bcl.Memory|10.0.0.7, Microsoft.Bcl.Numerics|10.0.0.5, Microsoft.Extensions.Diagnostics.Abstractions|10.0.0.5, Microsoft.Identity.Client|4.89.0.0, Microsoft.Identity.Client.Extensions.Msal|4.89.0.0, Microsoft.IdentityModel.Abstractions|8.22.0.0, Microsoft.IdentityModel.JsonWebTokens|8.22.0.0, Microsoft.IdentityModel.Logging|8.22.0.0, Microsoft.IdentityModel.Protocols|8.22.0.0, Microsoft.IdentityModel.Protocols.OpenIdConnect|8.22.0.0, Microsoft.IdentityModel.Tokens|8.22.0.0, Microsoft.IdentityModel.Validators|8.22.0.0, System.IdentityModel.Tokens.Jwt|8.22.0.0, System.ClientModel|1.3.0.0) plus `Fizzler|1.3.0.0`. No other entry is present (the list is in Sort-Object order, which places `Microsoft.Identity.Client.Extensions.Msal` before `Microsoft.Identity.Client`; the set is the same). No KNOWN-DEBT-DRIFT.

Both failing messages begin `Expected ` and contain `, but got `, so each is the expected assertion failure and not an exception or StrictMode error inside the test body. Tests 1 to 12 passed (JUNIT-SUITE failures=2 corresponds to tests 13 and 14 only). No `.iter<N>` correction was needed.

Other suites equal their P0-T12 BASELINE-SUITE counts with `failures=0`: AnalyzerItemRepair 13, ConsistencyVerifier 14, DependabotConfig 17, PackageCompatibility 8, PackageGraph 32, ProjectConsistency 18, Repair-PackageManifestConsistency 31, RepositoryTreeConsistency 4.

COVERAGE-MEASUREMENT: deferred to the CI Pester job (_pester.yml); no local figure

Acceptance: JUNIT-SUITE `BindingRedirectVerification.Tests.ps1 tests=14 failures=2 skipped=0`; JUNIT-ROOT failures=2; exactly two JUNIT-NOTPASSED lines (tests 13 and 14 by name); test 13 and test 14 messages carry the required content; every other suite equals its baseline.

Output Summary: Red state observed as required: 151 tests, 2 failures (test 13 `but got 11` over the eleven stale configs; test 14 observed list of 16 entries including `Fizzler|1.3.0.0`). Tests 1 to 12 pass. Eight pre-existing suites unchanged from baseline. This is the AC5 fail-before evidence.

# Repo-Wide Test Count and Coverage Baseline (Remediation Cycle 1)

- Timestamp: 2026-07-08T00-17
- Command: `pwsh ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/remediation-cycle1-baseline.cobertura.xml`
  (run from the worktree root; forward-slash form used in the actual invocation to avoid a
  git-bash backslash-escaping defect — see Deviation note below)
- EXIT_CODE: 1 (non-zero; see Deviation/Findings below — the script's own gate treats any test
  failure as a hard failure and throws, which is expected behavior given 1 pre-existing failing
  test unrelated to this remediation's scope)
- Output Summary:
  - Total tests: 5032
  - Passed: 5031
  - Failed: 1
  - Total test time: 47.6660 seconds
  - Coverage output written to `coverage/remediation-cycle1-baseline.cobertura.xml` (28.8 MB
    Cobertura XML, 7 test assemblies discovered)
  - Repo-wide line coverage (Cobertura top-level `line-rate`, consistent with the method used in
    this feature's original `feature-audit.2026-07-07T23-46.md` §AC15, which reports the same
    baseline as "repo 81.08%"): **81.62%** (`lines-covered="119363"` / `lines-valid="146244"` =
    0.8161907497059708). This is within normal run-to-run variance of the 81.08% figure recorded
    in the prior audit cycle (same coverage source, minor variance from JIT/test-ordering and the
    1 pre-existing live-Outlook failure below).

## Deviations From Plan-Literal Baseline

1. **Path form**: the plan's literal command uses a backslash path
   (`coverage\remediation-cycle1-baseline.cobertura.xml`). In this git-bash execution
   environment, an unquoted backslash before a letter is consumed as a bash escape character
   (`\r` -> `r`), which on a first attempt produced a malformed output filename
   (`coverageremediation-cycle1-baseline.cobertura.xml` at the repo root). This malformed file was
   deleted and the command was re-run with the equivalent forward-slash path
   (`coverage/remediation-cycle1-baseline.cobertura.xml`), which PowerShell/.NET accept
   identically to the backslash form on Windows. No change to command semantics or output
   location intent.

2. **Baseline test count is 5031 passed / 1 failed, not 5032 passed / 0 failed as the plan
   anticipated.** The single failure is:
   - `TaskMaster.Test.AppGlobals.LiveOutlookHookupIntegrationTests.LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold`
   - Error: `System.Runtime.InteropServices.COMException (0x80010100): Retrieving the COM class
     factory for component with CLSID {0006F03A-0000-0000-C000-000000000046} failed ... (RPC_E_SYS_CALL_FAILED)`
     — CLSID `{0006F03A-...}` is `Outlook.Application`. This is a live-Outlook COM integration
     test that requires a running/registered Outlook COM server in the execution environment; the
     failure is an environment condition (no live Outlook COM class factory available in this
     worktree's test-run environment), not a defect introduced by this remediation. This test is
     entirely unrelated to `StoresWrapperTests.cs`, `StoresWrapperDisableTests.cs`, or
     `StoreDisableServiceTests.cs` (the only files this remediation touches).
   - This baseline run occurred before any Phase 1 edit was made (Phase 0 baseline capture, prior
     to R1/N1 changes), confirming the failure pre-exists this remediation and is not a regression
     it introduces.
   - This empirical baseline (5031 passed / 1 failed / 5032 total) — not the plan's anticipated
     5032/0 — is used as the reference baseline for the P2-T6 delta/threshold verification. The
     no-regression bar for this remediation is: total test count unchanged (5032), the same 1
     pre-existing environment-dependent failure unrelated to the touched files persists (or is
     resolved by environment/timing, which would be an improvement, not a regression), and no new
     failures appear in the touched files' test methods.

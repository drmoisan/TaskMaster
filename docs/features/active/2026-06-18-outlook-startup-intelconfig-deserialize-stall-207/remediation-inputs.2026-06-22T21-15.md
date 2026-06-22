# Remediation Inputs — Cycle 1 (2026-06-22T21-15)

## Trigger

Required CI check "Format, build, analyze, and test" failed on PR #210 (run 27984128719) after the
PR was opened. This transitions the orchestrator into the remediation loop.

## Failure (verbatim)

```
Failed LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold [83 ms]
Error Message:
 Expected captured to be <null> because the live hookup must not throw on the STA, but found
 System.Runtime.InteropServices.COMException (0x80040154): Retrieving the COM class factory for
 component with CLSID {0006F03A-0000-0000-C000-000000000046} failed ... 80040154 Class not
 registered (REGDB_E_CLASSNOTREG).
 at ...LiveOutlookHookupIntegrationTests...:line 71 / asserted line 132
```

Run summary: Total 4311, Passed 4310, Failed 1.

## Root cause

The CI MSTest step runs the entire solution's test suite and does NOT apply the
`/TestCaseFilter:"TestCategory!=LiveOutlook"` exclusion that the local gated run used. The opt-in
`LiveOutlook` integration harness (`TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs`)
therefore executed on a headless CI agent with no Outlook installed, and `new Application()` threw
`REGDB_E_CLASSNOTREG`. The harness asserts the hookup "must not throw," so it failed.

A developer-only, live-Outlook harness must SKIP when Outlook is unavailable, not fail. This is a
defect in the harness's environment handling.

## Scope of this cycle (test-only; no production change; no workflow change)

Guard the harness so that when the Outlook COM `Application` cannot be created because Outlook is not
registered/available (`COMException` with HRESULT `0x80040154` REGDB_E_CLASSNOTREG, and the related
class-not-available HRESULTs), the test calls `Assert.Inconclusive(...)` (skips) instead of failing.
This keeps CI green deterministically without modifying the CI workflow (avoiding the
`modified-workflow-needs-green-run` rule) and preserves the harness's real behavior when Outlook IS
present (run for real on a developer machine).

Out of scope: any production code change; any `.github/workflows/**` change; any `.runsettings`
global category filter (which would permanently exclude the harness from all runs); the IntelConfig
continuation stall (#211).

## Acceptance for this cycle

- The `LiveOutlook` harness, when run without Outlook available, reports Inconclusive (skipped), not
  Failed; when Outlook is available it runs as before.
- The full local toolchain passes (CSharpier -> analyzers -> nullable/TWAE -> MSTest gated), and the
  whole-suite behavior no longer fails on the harness.
- Change confined to `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs`.
- After push, the required CI check on PR #210 is green.
- Exit gate: code-review, feature-audit, and policy-audit reaudits show 0 blocking findings.

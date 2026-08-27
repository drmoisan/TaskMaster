---
name: bare-vstest-omits-liveoutlook-filter
description: A direct vstest.console call runs a live-Outlook test the wrapper script excludes, launching real Outlook and making results incomparable to any baseline
metadata:
  type: project
---

`scripts/vscode/Invoke-MSTestWithCoverage.ps1:76` appends
`'/TestCaseFilter:TestCategory!=LiveOutlook'` to every run it performs. Any plan task that calls
`vstest.console.exe` **directly** does not inherit that filter and therefore runs a strictly larger
population than the baseline the wrapper produced.

The excluded test is
`TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs:73`
`LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold`, carrying
`[TestCategory("LiveOutlook")]` at line 72 — the only test in the repo with that category. Its body
constructs a real `Outlook.Application` and polls a live store with a 120000 ms ceiling. The
construction-scoped skip does not fire on a machine where `Outlook.Application` is registered
(`HKLM\SOFTWARE\Classes\Outlook.Application\CLSID` resolves), so on a developer workstation the test
really does start or attach to Outlook.

Two distinct harms, and the second is the quiet one:

1. An external process in a test run, which the General Unit Test Policy prohibits outright.
2. **Population mismatch.** A gate phrased as "zero new failures against the baseline" compares a
   filtered baseline to an unfiltered run. A live failure — cold profile, no default store, a
   security prompt, or the 2000 ms single-tick threshold — reads as a NEW regression caused by the
   change under test.

**How to apply:** Any full-assembly `vstest` invocation written into a plan must carry
`"/TestCaseFilter:TestCategory!=LiveOutlook"` explicitly, alongside the `\.claude\` worktree
exclusion and `/InIsolation` noted in the local-vstest memory. Before accepting a "no new failures"
gate, confirm both sides of the comparison were produced with the same filter. Scoped
`/TestCaseFilter:FullyQualifiedName~...` runs are unaffected, since they cannot match the
LiveOutlook test anyway.

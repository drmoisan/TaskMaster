# P4-T3 — Plain-Builder Call-Site Repair

Timestamp: 2026-09-13T06-18
Task: [P4-T3]

Command: pwsh -NoProfile -Command '<Invoke-Pester limited to tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 with Run.PassThru, printing the passed, failed and skipped counts, ending with an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

```
Discovery found 28 tests in 134ms.
Tests Passed: 28, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
COUNTS passed=28 failed=0 skipped=0
```

PASSED: 28
FAILED: 0
SKIPPED: 0

## Hashtable count and why that number

DISTINCT_HASHTABLE_COUNT: 2.

One is not sufficient. The four plain-builder call sites do not share one argument set: the site in the
test named `includes /Settings: pointing at the off-root CLI TaskMaster.cli.runsettings` supplies a
two-element test-assembly array and the other three supply a one-element array. `$script:vsTestArgument`
holds the one-element set and `$script:vsTestPairArgument` is a per-site clone of it that overrides only
the `TestAssembly` key, so no site is bound to another site's assembly list.

The four converted sites, each now a single splatted invocation:

```
106:        $arguments = Get-VsTestArgumentList @script:vsTestPairArgument
112:        $arguments = Get-VsTestArgumentList @script:vsTestArgument
125:        $arguments = Get-VsTestArgumentList @script:vsTestArgument
143:        $arguments = Get-VsTestArgumentList @script:vsTestArgument
```

NO_SPLAT_PLUS_EXPLICIT_BINDING: true. A case-sensitive search of the whole file for a splat token
followed on the same line by a named parameter returned zero matches, so no invocation supplies a
parameter both inside the splatted hashtable and explicitly. Supplying one parameter by both routes is
a parameter-binding error in PowerShell rather than an override, which is why this is asserted rather
than assumed.

## Exact-array assertion

The one exact-array assertion in the plain-builder describe block, in the test named `preserves the test
assemblies and /InIsolation alongside /Settings:`, was updated from four elements to the six the builder
now returns, the two added elements being the results-directory switch and the trx logger switch, each
interpolated from the same hashtable the call site splats so the assertion and the invocation cannot
drift apart.

No new describe block was added to this file.

## File size

`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` measures 498 content lines after this
phase's format step, against the repository ceiling of 500 and the Phase 0 post-format baseline of 491.
Converting the four continuation call sites to splatting reclaimed 8 lines, which is what made the two
added arguments and the two added assertion elements fit: four continuation sites gaining two arguments
each would have added 8 lines and taken the file to 501.

Output Summary: 28 passed, 0 failed, 0 skipped. Two distinct hashtables, because one call site supplies
a two-element assembly array and three supply a one-element array. No invocation binds a parameter both
by splat and explicitly. The exact-array assertion now pins six elements. The file measures 498 lines,
which is at most 500.

# AC3B targeted consecutive-run streak (P5-T1)

Task: [P5-T1]
Timestamp: 2026-09-13T03-37
Command: `pwsh -Command '$fail = 0; $n = 0; for ($i = 1; $i -le 62; $i++) { & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~QfcItemController_SeamMarshallingTests" | Out-Null; $n = $i; if ($LASTEXITCODE -ne 0) { $fail = $fail + 1; break } } Write-Output ("RUNS=" + $n + " FAILURES=" + $fail)'` Run from the item worktree root via Set-Location inside one pwsh invocation with the Command Reference tool resolution prepended (inner quoting inverted to single quotes; semantics identical). The whole 62-run loop ran under one acquisition of the shared machine build lock for item 743 (acquired 03:35:26, released 03:37:45), so no sibling item's build or test run overlapped it. Outlook was closed; no induced load.
EXIT_CODE: 0
Output Summary: `RUNS=62 FAILURES=0`

## Achieved figures

- Achieved run count N = **62**
- Failure count = **0**
- Exact p-value `(20/21)^N` = `(20/21)^62` = **0.048558** (computed with `[math]::Pow(20.0/21.0, 62).ToString('F6')`)

REGIME: SERIAL (no /Settings: argument).

## Targeted reproduction scope (defined in P5-T1 and nowhere else)

The QuickFiler test assembly `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` filtered to exactly the one test class named in the filter, `QfcItemController_SeamMarshallingTests` (the class AC2 names; five tests per run), in the SERIAL regime (no `/Settings:` argument), with `/InIsolation`, on an otherwise-idle machine with Outlook closed, which is the load condition recorded in the P0-T11 declaration.

The 62-run count applies to this targeted scope only and explicitly does not apply to the full instrumented multi-assembly suite.

## Statistical statement

The 4.8 percent base rate is a point estimate from a single observed failing run; the exact 95 percent interval for 1 of 21 is approximately 0.0012 to 0.2382, so the 62-run figure is itself uncertain.

With N = 62 and zero failures, the probability of observing this streak if the per-run failure rate were still 1/21 is 0.048558, below the conventional 0.05 threshold; the statistical claim for this targeted scope is therefore established at that level, subject to the interval caveat above. Component AC3A (deterministic efficacy) is carried separately by `evidence/regression-testing/ac3a-deterministic-efficacy.2026-09-12T18-00.md` and does not depend on this figure.

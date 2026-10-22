# Phase 3 — Pass-After Run of the Two New Regression Tests (issue #440, plan task P3-T1)

Timestamp: 2026-08-29T06-32

Command (character-for-character the filter P1-T4 used, so the two runs are directly
comparable):

```
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled|FullyQualifiedName~LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot" "/Logger:trx;LogFileName=p3-t1.trx" "/ResultsDirectory:coverage\trx\p3-t1"
```

EXIT_CODE: 0

## Output Summary

```
  Passed LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled [34 ms]
  Passed LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot [< 1 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
```

Both tests that failed at P1-T4 now pass under the P2-T1 guard relaxation, with the
same filter and the same assertions. Paired with
`p1-t4-fail-before.2026-08-29T06-30.md` this is the fail-before / pass-after evidence
for AC-1 and AC-8.

The TRX was written to `coverage\trx\p3-t1\p3-t1.trx`, under the gitignored
`coverage/` tree.

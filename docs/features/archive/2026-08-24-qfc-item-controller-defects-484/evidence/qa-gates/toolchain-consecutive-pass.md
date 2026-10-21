# Toolchain consecutive-pass confirmation

Timestamp: 2026-08-26T14-14
Task: [P7-T12]

## Ordered timestamps of the final pass

| Order | Task | Stage | Timestamp | Result |
|---|---|---|---|---|
| 1 | `[P7-T1]` | Format (scope-locked `csharpier format`) | 2026-08-26T13-41 | EXIT_CODE 0, 0 of 9 files rewritten |
| 2 | `[P7-T2]` | Format verification (repo-wide `csharpier check .`) | 2026-08-26T13-42 | EXIT_CODE 0, 0 unformatted of 1520 |
| 3 | `[P7-T3]` | Lint (MSBuild analyzers, `/t:Rebuild`) | 2026-08-26T13-42 | EXIT_CODE 0, 0 errors |
| 4 | `[P7-T4]` | Type check (MSBuild `TreatWarningsAsErrors`, `/t:Rebuild`) | 2026-08-26T13-43 | EXIT_CODE 0, 0 errors |
| 5 | `[P7-T5]` | Test (`vstest.console.exe` with `/EnableCodeCoverage`) | 2026-08-26T13-56 | EXIT_CODE 0, 959 of 959 Passed |

The timestamps are strictly non-decreasing and the stages ran in the required order: format, lint,
type check, test.

## No owned file changed between the format pass and the test run

Command:

```
sha256sum <the nine owned files>
```

EXIT_CODE: 0

The SHA-256 of each of the nine owned files taken after `[P7-T5]` is byte-identical to the value
recorded after the `[P7-T1]` format pass in
`docs/features/active/qfc-item-controller-defects-484/evidence/qa-gates/csharpier-format.md`. The
comparison was made mechanically and reported no difference on any of the nine files.

| File | SHA-256 held across the whole pass |
|---|---|
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | `bf0b886e8ccc77ecee583418ba840db4a73bae896098a4865ca380c454f0aefc` |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | `378748eba4b24f5f739ae726aa967b947b768e4480477e5f9e16f7788ddcb8c1` |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | `b9ed07ec887d2f194819f716d79e72e4f9a50e4df5653e3e698dfc796ed4602d` |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | `299bc8ad90640cf0505161315113bb4451d8d736e44aeb4d2b91a1ac0a41d58a` |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | `a3c35259f1c5e5d2ed8d8a3e5ba923a964e2b164abe9d9ac7b6b32ec30644e4b` |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | `55cb918b6fb4d629d4d6d4bd3eb7320a0fa4f3c947895b374dd420e32d0aefe1` |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | `a65daf290761c09b5dab70f269cd3e632e633430083d217c2101267ff8715fc7` |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | `502633b48282d7211b6994f8fcc21b7c0a503d93d9fd89358fdfb8e186a9a178` |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | `6293904bd2dfacc7c2678481409d576ff651a400ae550cc3a628f89ec6958cdf` |

## Loop restarts before the final pass

**Zero.** No stage failed and no stage rewrote a file, so the loop never restarted at `[P7-T1]`.

One invocation-level note, recorded for completeness rather than as a restart: the first invocation of
the `[P7-T5]` command was terminated by the harness after a 10-minute wall-clock limit with the test
host stalled (CPU time flat over a 20-second sample). That invocation produced no test outcome and
modified no file. The stalled process chain belonging to this task alone was terminated, and the
identical command was run once more and completed in 12.3 seconds. Because no file changed between
the two invocations — as the SHA-256 table above independently confirms — the earlier stages remained
valid and a restart from `[P7-T1]` would have been a no-op.

Output Summary: The four stages ran in order at 13-41, 13-42, 13-42, 13-43, and 13-56, all passing.
All nine owned files held identical SHA-256 values from the format pass through the test run. Zero
loop restarts.

# [P3-T2] CSharpier check (read-only)

Timestamp: 2026-09-06T15-03

Command: `dotnet tool run csharpier check .`, with `DOTNET_ROOT` bound to the repository-local
`.dotnet-sdk` directory.

EXIT_CODE: 0

Verbatim printed line:

```
Checked 1587 files in 4198ms.
```

FINAL-CSHARPIER-CHECKED-FILES: 1587
BASELINE-CSHARPIER-CHECKED-FILES: 1583 (from [P0-T7])
DELTA: 4

The exit code is the gate here, because `check` is read-only and returns non-zero on drift. Exit 0
with the single success line and no drifting-path list means the whole tree agrees with the
manifest-pinned CSharpier 1.2.6, which is the version `.github/workflows/_format-check.yml` runs
after `dotnet tool restore`.

The delta of 4 is the expected observation stated by the task: this plan creates exactly four new
`.cs` files — `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`,
`QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs`,
`QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` and
`QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`. No file was added to or removed from the
formatter's scope by any other route.

This is step 1 of the uninterrupted toolchain pass; steps 2 through 4 are [P3-T3], [P3-T4] and
[P3-T5], and [P3-T6] records the closure.

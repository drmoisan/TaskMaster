# Issue #485 — Guard Tests Fail Against the Unguarded Extraction

Timestamp: 2026-08-26T09-08
Task: [P2-T4] [expect-fail]

ExpectedExitCode: 1

## Build step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0 (3 warnings, 0 errors). Not an analyzer or nullable gate (decision D2).

## Test step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TryResolveCidResource" "/Logger:trx;LogFileName=485-fail.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\regression-testing\485-fail
```

EXIT_CODE: **1**

```
Total tests: 8
     Passed: 4
     Failed: 4
Test Run Failed.
```

## Every distinct test result name in the group, with its outcome

Per decision D9 each `[DataRow]` of `TryResolveCidResource_RejectsUnusableUri_ReturnsFalseWithNullOutputs`
is reported as a distinct test result under its `DisplayName`.

| # | Distinct result name | Outcome | Failure reason |
|---|---|---|---|
| 1 | `malformed URI` (row of `TryResolveCidResource_RejectsUnusableUri_ReturnsFalseWithNullOutputs`) | **Failed** | `System.UriFormatException` from the unguarded `new Uri(...)` |
| 2 | `relative URI` (row of the same method) | **Failed** | `System.UriFormatException` from the unguarded `new Uri(...)` |
| 3 | `empty final segment` (row of the same method) | Passed | the pre-existing empty-segment test already covered this case |
| 4 | `TryResolveCidResource_WithNullMap_ReturnsFalse` | **Failed** | `System.NullReferenceException` from the unguarded map lookup |
| 5 | `TryResolveCidResource_WithMapMiss_ReturnsFalse` | Passed | the pre-existing `TryGetValue` miss branch already returned false |
| 6 | `TryResolveCidResource_WithNullAttachmentData_ReturnsFalse` | **Failed** | `Expected resolved to be False, but found True.` — a map hit was treated as implying a payload |
| 7 | `TryResolveCidResource_WithKnownExtension_ReturnsPayloadAndMimeType` | Passed | happy path, unaffected by the missing guards |
| 8 | `TryResolveCidResource_WithUnrecognisedExtension_ReturnsOctetStream` | Passed | happy path, unaffected by the missing guards |

The acceptance text requires at least the malformed-URI row and
`TryResolveCidResource_WithNullAttachmentData_ReturnsFalse` to be recorded with outcome `Failed`. Both
are, and the relative-URI row and the null-map test fail as well.

## Interpretation

The four failures are precisely the three #485 defects, exercised through the `[P2-T1]`
defect-preserving extraction:

- **Defect 1 (unguarded `new Uri(...)`).** Rows 1 and 2 throw `System.UriFormatException`. The relative
  URI throws at the same site rather than at `Uri.Segments`, which is why `UriKind.Absolute` is required
  in the `[P2-T5]` fix: `UriKind.RelativeOrAbsolute` would merely move the throw one line later.
- **Defect 2 (map hit does not imply a payload).** Row 6 returns `true` with a null payload, which is what
  produced the `ArgumentNullException` from `new MemoryStream(...)` in the production adapter.
- **Null map.** Row 4 throws `System.NullReferenceException` from the unguarded lookup.

The four passing rows are cases the pre-existing logic already handled; they are recorded so that the
`[P2-T6]` post-fix comparison is over the same eight result names.

TRX:
`docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/485-fail/485-fail.trx`

Output Summary: `EXIT_CODE: 1`, 8 total, 4 failed. The malformed-URI row, the relative-URI row,
`TryResolveCidResource_WithNullMap_ReturnsFalse`, and
`TryResolveCidResource_WithNullAttachmentData_ReturnsFalse` all fail against the unguarded code. This is
the fail-before evidence for issue #485.

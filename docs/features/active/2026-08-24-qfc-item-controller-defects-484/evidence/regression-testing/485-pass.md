# Issue #485 — All Guard Tests Pass After the Fix

Timestamp: 2026-08-26T09-14
Task: [P2-T6]

## Build step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0 (3 warnings, 0 errors). Not an analyzer or nullable gate (decision D2).

## Test step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TryResolveCidResource" "/Logger:trx;LogFileName=485-pass.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\regression-testing\485-pass
```

EXIT_CODE: **0**

## Counts

| Metric | Value |
|---|---|
| Total | 8 |
| Passed | 8 |
| **Failed** | **0** |

```
Test Run Successful.
Total tests: 8
     Passed: 8
```

## Distinct passing test result names (8)

| # | Distinct result name | Outcome |
|---|---|---|
| 1 | `malformed URI` (row of `TryResolveCidResource_RejectsUnusableUri_ReturnsFalseWithNullOutputs`) | Passed |
| 2 | `relative URI` (row of the same method) | Passed |
| 3 | `empty final segment` (row of the same method) | Passed |
| 4 | `TryResolveCidResource_WithNullMap_ReturnsFalse` | Passed |
| 5 | `TryResolveCidResource_WithMapMiss_ReturnsFalse` | Passed |
| 6 | `TryResolveCidResource_WithNullAttachmentData_ReturnsFalse` | Passed |
| 7 | `TryResolveCidResource_WithKnownExtension_ReturnsPayloadAndMimeType` | Passed |
| 8 | `TryResolveCidResource_WithUnrecognisedExtension_ReturnsOctetStream` | Passed |

The acceptance text requires at least eight distinct passing result names; there are exactly eight, and
all four rows that failed in `[P2-T4]` now pass.

## Guards delivered by `[P2-T5]`

- `Uri.TryCreate(requestedUri, UriKind.Absolute, out var uri)` with a debug log line on failure.
  `UriKind.Absolute` is used, **not** `UriKind.RelativeOrAbsolute`.
- Empty-final-segment early return.
- Null-map early return.
- Null-match early return (`|| match is null` on the `TryGetValue` result).
- Null `AttachmentData` early return with a debug log line.
- The lambda adapter now builds the map from a null-conditional `ItemHelper?.AttachmentsInfo` read, so a
  null `ItemHelper` cannot be dereferenced.

Both `out` values are null on every false return. `ResolveImageMimeType` remains `private static`. No new
`[ExcludeFromCodeCoverage]` attribute was added; the file's count is unchanged at 2.

TRX:
`docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/485-pass/485-pass.trx`

Output Summary: `EXIT_CODE: 0`, 8 total, 8 passed, 0 failed, over eight distinct result names.

# Baseline — FORMAT-VERIFY on the unmodified tree ([P0-T8])

Timestamp: 2026-08-10T22-45
Command: `./.dotnet-sdk/dotnet.exe tool run csharpier check .`
EXIT_CODE: 0

## Console output

```
Checked 1517 files in 7095ms.
```

## Output Summary

FORMAT-VERIFY (`dotnet tool run csharpier check .`), the read-only CI-parity form this feature
adopts, returns `EXIT_CODE: 0` against the unmodified tree. **`Checked 1517 files`** — this is the
baseline count [P5-T2] and [P6-T6] compare against. The tree is format-clean before any edit, so
correcting the documented format command requires no formatting churn, and any `*.cs` diff appearing
after [P5-T1] or [P6-T5] would be attributable to this feature.

The count matches the 1517 recorded in the pre-existing artifact
`baseline-csharpier-replacement-forms.2026-08-10T14-45.md`, confirming no file-population drift
since that measurement.

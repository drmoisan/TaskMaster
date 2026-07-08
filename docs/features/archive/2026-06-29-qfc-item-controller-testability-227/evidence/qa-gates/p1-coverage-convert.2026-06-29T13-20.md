# Phase 1 — Cobertura Conversion to Canonical Path (P1-T4)

Timestamp: 2026-06-29T13-20

Command: dotnet-coverage merge TestResults/3fdf5b12-d7b2-46d6-b1b1-e91fdc638167/DanMoisan_MEGALODON4_2026-06-29.12_35_42.coverage -f cobertura -o artifacts/csharp/coverage.xml

EXIT_CODE: 0

## Output Summary

```
dotnet-coverage v18.5.2.0 [win-x64 - .NET 10.0.9]
Including file ...\DanMoisan_MEGALODON4_2026-06-29.12_35_42.coverage.
Merged into file ...\artifacts/csharp/coverage.xml.
```

The conversion followed the documented #223 cycle-1 procedure (`dotnet-coverage merge <.coverage>
-f cobertura -o artifacts/csharp/coverage.xml`). The canonical artifact
`artifacts/csharp/coverage.xml` now exists (13,261,135 bytes). This is the single permitted
non-`<FEATURE>/evidence/<kind>/` output path per guardrail G5. Well-formedness and line-rate
readability are verified in P1-T5.

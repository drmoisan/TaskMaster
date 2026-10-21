# Baseline CSharpier check (P0-T8)

Timestamp: 2026-09-03T01-13

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

## Baseline unformatted set:

(empty — CSharpier reported no unformatted file)

## Full console output

```
Checked 1576 files in 5504ms.
```

Output Summary: The read-only formatter gate exits 0 at the merge base with zero files reported as
unformatted across 1576 checked files. The `Baseline unformatted set:` above is therefore empty,
and it is the comparison basis P6-T2 uses: any path P6-T2's final check reports that is not one of
the seven plan-owned formattable paths would be a new unformatted file rather than pre-existing
drift.

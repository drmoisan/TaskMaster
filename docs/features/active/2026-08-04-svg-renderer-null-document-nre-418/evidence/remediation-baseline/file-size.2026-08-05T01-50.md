# Pre-Change File Sizes — Remediation Cycle 1

- Task: `[P0-T5]`
- Issue: #418
- Branch / HEAD: `bug/svg-renderer-null-document-nre-418` @ `ea106111`
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T01-24 (UTC)

Command:

```
pwsh -NoProfile -Command "'SVGControl/SvgRenderer.cs','SVGControl/SvgAssemblyProbe.cs','SVGControl.Test/SvgRendererParseContractTests.cs','SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs','SVGControl.Test/SvgRendererNullToleranceTests.cs' | ForEach-Object { '{0} = {1}' -f $_, (Get-Content -LiteralPath $_ | Measure-Object -Line).Lines }"
```

EXIT_CODE: 0

Verbatim output of the mandated command:

```
SVGControl/SvgRenderer.cs = 462
SVGControl/SvgAssemblyProbe.cs = 65
SVGControl.Test/SvgRendererParseContractTests.cs = 292
SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs = 164
SVGControl.Test/SvgRendererNullToleranceTests.cs = 125
```

## Authoritative counts

`Measure-Object -Line` does not count blank lines and therefore undercounts. This is the known
undercount recorded in `policy-audit.2026-08-04T20-25.md` § 2 ("Verified with both `wc -l` and
`awk END{print NR}` to avoid the known PowerShell `Measure-Object -Line` undercount"). A cross-check
with `awk 'END{print NR}'` was run for every file and is the authoritative figure used by this cycle:

| File | `Measure-Object -Line` | `awk END{print NR}` (authoritative) | 500-line limit |
|---|---|---|---|
| `SVGControl/SvgRenderer.cs` | 462 | **497** | 3 lines of headroom |
| `SVGControl/SvgAssemblyProbe.cs` | 65 | **67** | 433 lines of headroom |
| `SVGControl.Test/SvgRendererParseContractTests.cs` | 292 | **332** | 168 lines of headroom |
| `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs` | 164 | **187** | 313 lines of headroom |
| `SVGControl.Test/SvgRendererNullToleranceTests.cs` | 125 | **143** | 357 lines of headroom |

Command used for the cross-check:

```
for f in SVGControl/SvgRenderer.cs SVGControl/SvgAssemblyProbe.cs SVGControl.Test/SvgRendererParseContractTests.cs SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs SVGControl.Test/SvgRendererNullToleranceTests.cs; do echo "$f = $(awk 'END{print NR}' $f)"; done
```

EXIT_CODE: 0

## Headroom statement for `SVGControl/SvgRenderer.cs`

`SVGControl/SvgRenderer.cs` is **497 lines** against the hard **500-line** limit in
`.claude/rules/general-code-change.md` § File Size Limit — **3 lines of headroom**. This is the
condition that fixes task order in Phase 1: R-3 (`[P1-T10]`) adds a `catch` clause to the resolver
region, which would breach the limit if R-6 (`[P1-T1]`–`[P1-T4]`) did not relieve the file first. The
plan's Design Decision 1 records this ordering explicitly.

`[P2-T8]` re-runs this command extended with `SVGControl/SvgAssemblyResolver.cs` and requires
`SVGControl/SvgRenderer.cs` at **at most 400 lines** with no file above 500.

## Output Summary

All five Scope Lock file line counts recorded. Authoritative (`awk`) counts: 497, 67, 332, 187, 143.
`SVGControl/SvgRenderer.cs` has 3 lines of headroom against the 500-line limit, confirming the
R-6-first ordering the plan mandates. No file currently exceeds 500 lines.

# Final QC Stage 1b — CSharpier Check

- Task: `[P2-T2]`
- Issue: #418
- Evidence series: `2026-08-05T05-00`
- Toolchain pass: **1**
- Timestamp: 2026-08-05T00-07

## Command

```
dotnet tool run csharpier check .
```

Run from the repository root.

```
EXIT_CODE: 0
```

Verbatim output:

```
Checked 1467 files in 4450ms.
```

## Files needing formatting: 0

```
Command: grep -c "Was not formatted" <output>
Output:  0
```

CSharpier emits one `Was not formatted` line per non-conforming file and emitted none, so every one of
the 1467 checked files conforms to the formatter.

## Comparison against the transcribed basis

Basis: `evidence/remediation-baseline/build-basis.2026-08-05T05-00.md` § 1, which transcribes
`evidence/qa-gates/csharpier-check.2026-08-05T01-50.md`.

| Figure | Basis (`2026-08-05T01-50`) | This run (`2026-08-05T05-00`) | Verdict |
|---|---|---|---|
| `EXIT_CODE` | 0 | **0** | matches |
| **Files needing formatting** | **0** | **0** | **matches** |
| Files checked | 1467 | 1467 | matches |

The operative figure — **zero files needing formatting** — matches the basis exactly, satisfying
`[P2-T2]`'s acceptance.

The file count also matches at 1467. That is the expected result and worth stating for a reaudit: this
cycle adds **no** `.cs` file, so the count that rose from 1466 to 1467 during cycle 1 (which added
`SVGControl/SvgAssemblyResolver.cs`) does not move again. The two build-configuration files this cycle
edits are a `.csproj`, which `.csharpierignore` excludes, and a `packages.config`, which it does not
exclude but which was already inside the checked set before this cycle and so adds nothing to the count.

## Output Summary

`EXIT_CODE: 0`, `Checked 1467 files in 4450ms`, and **zero files needing formatting**, matching the
figure transcribed in `evidence/remediation-baseline/build-basis.2026-08-05T05-00.md` exactly. Stage 1b
of toolchain pass 1 is clean; no file changed at `[P2-T1]` and none is non-conforming here, so the loop
proceeds to `[P2-T3]` without restart.

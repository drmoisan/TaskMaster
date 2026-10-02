---
name: write-tool-drops-utf8-bom-edit-prefix-restores-it
description: A whole-file Write of a BOM-bearing .cs file drops the UTF-8 BOM; one Edit prefixing line 1 with U+FEFF restores EF BB BF without touching any whitespace-insensitive or Get-Content gate, and CSharpier keeps it
metadata:
  type: project
---

Rewriting an existing BOM-bearing file with the Write tool (observed on #956 P2-T6, SortEmail.cs,
2026-10-01) leaves first bytes `23 6E 75` — the BOM is gone. New files written by Write never get one.

Restoring it inside the "source edited only by Write/Edit" rule: one Edit whose old_string is the
first line(s) (e.g. `#nullable enable\nusing System;\n...`) and whose new_string is the same text
prefixed with the literal U+FEFF character. Verified afterwards: first bytes `EF BB BF`, exactly one
U+FEFF in the file; a scoped `dotnet tool run csharpier format` left it in place.

**Why:** callers ask for BOM preservation "where feasible" without letting it move a gate; pwsh
.NET writes would break a plan's "source files are written only by Write/Edit" rule (PD-10 style).

**How to apply:** after any whole-file Write of a file whose merge-base copy had a BOM, read the
first 3 bytes with `[System.IO.File]::ReadAllBytes` (absolute path); if missing and preservation is
wanted, apply the Edit prefix, re-check bytes, and run the plan's census afterwards.
`Get-Content -Encoding UTF8` consumes the BOM, so FIRST-LINE / whitespace-stripped equality gates
are unaffected either way. Related: [[powershell-bom-required]], [[bom-grep-anchor-false-negative]].

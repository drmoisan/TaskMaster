---
name: policy-audit-required-structure
description: policy-audit validator hard requirements - Appendix A heading, full TS/PS coverage checklist lines, and a numeric Python comparison line
metadata:
  type: feedback
---

`validate_orchestration_artifacts` for `policy-audit` enforces several structural
items that are easy to drop when trimming the template:

- Both heading `## Appendix A: Test Inventory` AND `## Appendix B: Toolchain Commands Reference` must be present (verified again 2026-05-28 on issue #25 review — first write had only Appendix A naming and failed with both missing-heading errors).
- The PowerShell row's "Baseline Coverage" cell in the Coverage Metrics by Language table must contain a numeric value (e.g. `0.00% lines`) when PowerShell files are in scope. Prose like "LINE covered=0 missed=141" without an explicit percentage triggers `missing numeric baseline coverage for PowerShell`.
- The Coverage Evidence Checklist must keep all four lines verbatim even when those
  languages are out of scope: `TypeScript baseline coverage artifact:`,
  `TypeScript post-change coverage artifact:`, `PowerShell baseline coverage artifact:`,
  `PowerShell post-change coverage artifact:` (value `N/A - out of scope` is accepted).
- The Section 1.2.1 per-language comparison line for an in-scope language must contain
  a numeric baseline, a numeric post-change value, and an explicit `Disposition: PASS`
  (the validator looks for `Baseline:`/`Post-change:`/`Disposition:` tokens with numbers).
  Phrasing like "100% line / 100% branch" passes as long as numerals are present in
  the Baseline/Post-change segments.
- The validator's `_extract_policy_audit_comparison_lines` keeps reading `- ` bullets after
  the `### 1.2.1` heading until it hits the NEXT `### ` heading. Any later
  `- Python: ...` bullet (e.g. under `**Language-specific policies evaluated:**`)
  OVERWRITES the comparison-line entry and breaks validation. Fix: put a `### 1.2.2`
  heading immediately after the three comparison bullets so the scan terminates.

**Why:** Verified 2026-05-27 on issue #18 review — first write failed with all of the
above as missing-heading / missing-checklist-line / missing-numeric errors.

**How to apply:** Keep the template's Appendix A and the full coverage checklist;
for out-of-scope languages use `N/A - out of scope` rather than deleting the line.
See also [[feature-audit-checkoff-heading-case]].

**The checklist must be plain top-level `- ` bullets, not table cells (confirmed #791, 2026-09-06,
cost one rejection cycle).** Rendering the four `TypeScript|PowerShell baseline|post-change coverage
artifact:` items and `Per-language comparison summary:` as rows of a `| Item | State | Note |` table
is NOT accepted — the validator reported all five as missing even though every string was present in
the file. Emit them as the #781 shape, verbatim, ideally under a `### Coverage Evidence Checklist`
heading placed before `### 1.2.1`:

```
- C# baseline coverage artifact: `coverage/<N>-baseline.cobertura.xml`
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document
```

A detail table may be kept alongside, but then do not title it `### 1.2.2 Coverage Evidence
Checklist` — a second heading with that name risks the parser binding to the table instead of the
bullets. `### 1.2.2 Coverage Artifact State` works and still terminates the 1.2.1 bullet scan.

**The `**Coverage Metrics by Language:**` table is bound POSITIONALLY, not by header name
(confirmed #791, 2026-09-06, cost a second rejection cycle).** Every markdown row with exactly
SEVEN cells whose first cell is neither `Language` nor a dash rule is treated as a coverage row, and
the cells bind as:

`| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |`

Positions 5, 6 and 7 must each either start with `N/A` (case-insensitive) or contain an unanchored
`\d+(\.\d+)?%`. Consequences:

- Do NOT reorder, rename or substitute columns. A sensible-looking header
  `| Language | Changed Files | Coverage Artifact | Baseline | Post-Change | New Code | Verdict |`
  puts New-Code at position 5 and `Verdict` at position 6, so C#'s `FAIL` and PowerShell's `PASS`
  are read as coverage values and the audit fails with `missing numeric new/changed-code coverage`
  plus spurious per-language comparison-line errors.
- Keep the coverage artifact path and the per-language verdict OUT of that table. Put them in a
  separate table with a cell count that is not 7 (a 4-column
  `| Language | Coverage artifact | Verdict | Disposition |` works), or in prose. Do not use `- `
  bullets for it if it sits inside the 1.2.1 region, or it will overwrite the comparison lines.
- Before finalizing, scan the whole document for any OTHER 7-cell table row — a `code-review`-style
  findings table has exactly 7 and would be parsed as coverage rows if it appeared in a policy audit.
- The validator also treats `missing`, `unverified` and `tbd` inside the checklist or comparison
  bullets as placeholder markers. Keep those words out of both bullet sets.

**Inline-mention hazard (observed while drafting #791, 2026-09-06):** do not write the literal
strings `### 1.2.1 ...` or `### 1.2.2 ...` anywhere in body prose (for example in a Template
Provenance Deviation paragraph explaining which structure you followed). If the extractor does a
substring find rather than a line-start match it will begin scanning at the prose mention, hundreds
of lines above the real block, and pick up unrelated `- ` bullets. Refer to them as "the section
1.2.1 per-language coverage comparison block" instead, and verify afterwards by listing every `^\s*-`
line strictly between the two real `^### 1.2.` headings — the only bullets there should be the
per-language comparison bullets themselves.

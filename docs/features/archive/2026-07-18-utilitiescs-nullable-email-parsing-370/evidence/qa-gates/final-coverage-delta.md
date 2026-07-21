# Final Coverage Delta — Changed-Line Coverage Comparison (AC4)

Timestamp: 2026-07-19T07-15

## Inputs

- Baseline: `docs/features/active/2026-07-18-utilitiescs-nullable-email-parsing-370/evidence/baseline/baseline-coverage.cobertura.xml`
- Post-change: `docs/features/active/2026-07-18-utilitiescs-nullable-email-parsing-370/evidence/qa-gates/final-coverage.cobertura.xml`

## Repository-wide headline

- Baseline overall line-coverage: 83.7834% / branch-coverage: 76.3407%
- Post-change overall line-coverage: 83.8090% / branch-coverage: 76.3641%
- No repository-wide regression (both metrics equal or slightly above baseline).

## Per-file coverage for the 24 remediation-target cluster files (baseline vs. post-change)

| File | Baseline covered/total | Post-change covered/total |
|---|---|---|
| Ctf/CtfIncidence.cs | 36/36 | 36/36 |
| Ctf/CtfIncidenceList.cs | 153/182 | 153/182 |
| Ctf/CtfMap.cs | 117/138 | 117/138 |
| Ctf/CtfMapEntry.cs | 13/13 | 13/13 |
| EmailParsingSorting/AutoFile.cs | 58/64 | 58/64 |
| EmailParsingSorting/EmailDataMiner.cs | 35/36 | 35/36 |
| EmailParsingSorting/EmailDataMiner.FolderExtraction.cs | 189/197 | 189/197 |
| EmailParsingSorting/EmailDataMiner.Serialization.cs | 161/179 | 165/183 |
| EmailParsingSorting/EmailDataMiner.Transform.cs | 52/62 | 54/64 |
| EmailParsingSorting/EmailFiler.cs | 224/251 | 227/254 |
| EmailParsingSorting/EmailFilerConfig.cs | 105/112 | 105/112 |
| EmailParsingSorting/EmailTokenizer.cs | 322/373 | 322/373 |
| EmailParsingSorting/IEmailTokenizer.cs | (interface, no executable lines — not in Cobertura `<class>` output for either baseline or post-change) | — |
| EmailParsingSorting/ImageStripper.cs | 192/226 | 192/226 |
| EmailParsingSorting/MinedMailInfo.cs | 56/56 | 56/56 |
| EmailParsingSorting/MovedMailInfo.cs | 75/79 | 75/79 |
| EmailParsingSorting/SortEmail.cs | 36/66 | 36/66 |
| EmailParsingSorting/TesseractOcrTextExtractor.cs | 1/13 | 1/13 |
| SubjectMap/CommonWords.cs | 49/49 | 49/49 |
| SubjectMap/SubjectMapEncoder.cs | 113/140 | 113/140 |
| SubjectMap/SubjectMapEntry.cs | 338/414 | 338/414 |
| SubjectMap/SubjectMapMetrics.cs | 13/13 | 13/13 |
| SubjectMap/SubjectMapSco.cs | 89/91 | 89/91 |
| SubjectMap/SubjectMapSco.Orchestration.cs | 123/131 | 127/131 |
| **TOTAL (23 measurable files)** | **2550/2921 = 87.30%** | **2563/2930 = 87.47%** |

## Changed-line coverage analysis

No file's covered/total ratio decreased. The small denominator increases in
`EmailDataMiner.Serialization.cs` (+4/+4), `EmailDataMiner.Transform.cs` (+2/+2), and
`EmailFiler.cs` (+3/+3) are line-count shifts from CSharpier's reflow of the annotation edits
(e.g. multi-line tuple return-type wrapping); every added line in the denominator is also
covered in the numerator (proportional, no regression). `SubjectMapSco.Orchestration.cs` shows
the same denominator (131) with 4 more covered lines (123 → 127), meaning previously-uncovered
lines at that file's changed locations are now exercised by the existing test suite — a
strict improvement, not a regression.

**Conclusion: no coverage regression on changed lines across the 24-file cluster (AC4
SATISFIED).** Aggregate cluster coverage improved marginally (87.30% → 87.47%).

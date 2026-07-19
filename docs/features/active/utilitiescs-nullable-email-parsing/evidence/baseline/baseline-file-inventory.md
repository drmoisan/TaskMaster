# Baseline File Inventory — 25 `.cs` files in the cluster

Timestamp: 2026-07-19T00-05

Command: `find UtilitiesCS/EmailIntelligence/EmailParsingSorting UtilitiesCS/EmailIntelligence/SubjectMap UtilitiesCS/EmailIntelligence/Ctf -name "*.cs"` plus `wc -l` and `grep -c "#nullable enable"` per file.

| Path | Lines | `#nullable enable` present | Remediation target |
|---|---|---|---|
| UtilitiesCS/EmailIntelligence/Ctf/CtfIncidence.cs | 76 | No | Yes (Batch A) |
| UtilitiesCS/EmailIntelligence/Ctf/CtfIncidenceList.cs | 316 | No | Yes (Batch B) |
| UtilitiesCS/EmailIntelligence/Ctf/CtfMap.cs | 214 | No | Yes (Batch B) |
| UtilitiesCS/EmailIntelligence/Ctf/CtfMapEntry.cs | 36 | No | Yes (Batch A) |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/AutoFile.cs | 157 | No | Yes (Batch G) |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs | 143 | No | Yes (Batch F) |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.FolderExtraction.cs | 483 | No | Yes (Batch F) |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Serialization.cs | 404 | No | Yes (Batch F) |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Transform.cs | 410 | No | Yes (Batch F) |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs | 453 | No | Yes (Batch D) |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs | 238 | No | Yes (Batch D) |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailTokenizer.cs | 729 | No | Yes (Batch E) — exceeds 500-line limit, pre-existing, not split |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/IEmailTokenizer.cs | 17 | No | Yes (Batch A) |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs | 359 | No | Yes (Batch E) |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/MinedMailInfo.cs | 129 | No | Yes (Batch A) |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/MovedMailInfo.cs | 165 | No | Yes (Batch A) |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs | 1407 | No | Yes (Batch G) — exceeds 500-line limit, pre-existing, not split |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor.cs | 53 | No | Yes (Batch A) |
| UtilitiesCS/EmailIntelligence/SubjectMap/CommonWords.cs | 93 | No | Yes (Batch B) |
| UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapEncoder.cs | 198 | No | Yes (Batch C) |
| UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapEntry.cs | 657 | No | Yes (Batch C) — exceeds 500-line limit, pre-existing, not split |
| UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapMetrics.cs | 31 | No | Yes (Batch C) |
| UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapMetrics.Designer.cs | 109 | No | **Excluded** (Designer-generated) |
| UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.cs | 198 | No | Yes (Batch C) |
| UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs | 273 | No | Yes (Batch C) |

## Summary

- Total `.cs` files enumerated in the three target directories: 25.
- `SubjectMapMetrics.Designer.cs` confirmed excluded (Designer-generated code, no `#nullable`
  state to reconcile).
- Remaining remediation targets: 24 files, none of which currently carries `#nullable enable`.
- This matches the plan's and spec's stated file count (24 of 25).

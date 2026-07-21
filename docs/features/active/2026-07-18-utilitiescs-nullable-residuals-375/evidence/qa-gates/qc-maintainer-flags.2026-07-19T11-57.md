# QC Maintainer Decisions and Flags (P12-T9) — AC7

Timestamp: 2026-07-19T11-57

All six Maintainer Decisions and Flags are recorded in `spec.md` under the section
`## Maintainer Decisions and Flags` (spec.md line 219), and none is silently resolved by the code:

| # | Item | spec.md location | How this child treated it (not silently resolved) |
|---|---|---|---|
| 1 | Dead-duplicate exclusion (`PeopleScoDictionaryNewBackup.cs`) | spec.md line 224 | Left uncompiled and unmodified (P11-T2); flagged for exclude/delete decision |
| 2 | `Examples/MSDemoConv.cs` — annotate vs exclude vs delete | spec.md line 233 | Default: remediated annotation-only (P11-T1); exclude/delete flagged |
| 3 | `To Depricate/FileIO2.cs` + `StringManipulation.cs` deprecation-marked | spec.md line 239 | Remediated annotation-only (P10); deletion flagged, not performed |
| 4 | `MailResolution.cs` class `MailResolution_ToRemove` | spec.md line 245 | Remediated in place (P2-T5); deletion-candidate flagged, not deleted |
| 5 | Undeclared `ReusableTypeClasses` (#366) edge | spec.md line 249 | Flagged; the inherited CS8644 on PeopleScoDictionaryNew handled with a `#nullable disable` region on the class declaration (P6-T4), not by adding a manifest edge |
| 6 | Three pre-existing >500-line files | spec.md line 258 | Flagged, not split (P4-T2/P9-T1/P9-T2/P12-T6) |

EXIT_CODE: 0

Output Summary: All six items are present in `spec.md` and preserved as maintainer decisions; the code
changes did not silently resolve any of them. AC7 satisfied.

# AC11 — The Removed Null-Forgiving Suppression Is the One Inside EtlAsync

Timestamp: 2026-09-09T17-05

Command: git diff $b -- UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs, with $b re-derived from evidence/baseline/base-commit.md per D3

EXIT_CODE: 0

RemovedSuppressionLines: 1
AddedPlainReturnLines: 1

Output Summary: The anchored diff contains exactly one removed line whose text after the leading
minus is `            return (data!, columnDictionary);` and exactly one added line whose text after
the leading plus is `            return (data, columnDictionary);`. Both were matched as whole lines
including their twelve leading spaces, so the counts cannot be inflated by a substring match
elsewhere in the file.

## Why the criterion's literal wording is discharged by a count rather than a zero-hit search

AC11 as written asks for a search of OlTableExtensions.Etl.cs for the null-forgiving return `data!`
to return no hit. That form is not achievable and never was. The literal
`return (data!, columnDictionary);` appears twice in the pre-change file: at line 63, inside the
synchronous ETL method, and at line 131, inside EtlAsync. Only the second is in scope. ETL's
non-null tuple contract is a separate pre-existing latent condition, documented in that method at
lines 25 to 27, and changing it is outside this feature.

The occurrence count therefore moved from exactly 2 to exactly 1, and the surviving occurrence is
the ETL one at line 63 of the post-change file. The transition is discriminating: it fails if the
wrong suppression is removed, if neither is, or if both are. The anchored diff above additionally
pins which of the two moved, by matching the exact removed and added line text.

The declared return type is now
`Task<(object[,]? data, Dictionary<string, int> columnInfo)>`, so the null EtlAsync can return on the
deadline-expiry path is admitted by the type rather than forced through a suppression. The
DfDeedle guard remains the point at which that failure is named; it is now a null check against a
type that admits null.

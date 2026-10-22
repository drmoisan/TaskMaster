# P5-T1 SortEmail.cs Using Block (D10)

Timestamp: 2026-10-03T12-18
Command: Edit E-S-USINGS applied to UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs (lines 2 to 19 replaced by the nine-directive block); then CMD-CENSUS (PATHS-S with the TOKENS-S column of TOKENS-S and TOKENS-M)
EXIT_CODE: 0 (scoped to the CMD-CENSUS payload, its process exit code)
Output Summary: The eighteen-directive using block of S was replaced by the nine directives of E-S-USINGS (the anchor occurred once). Every TOKENS-S total equals the S FINAL column. S now has 268 lines.

```
TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 4
TOKEN usingDeedle; @ TOTAL = 0
TOKEN usingSDILReader; @ TOTAL = 0
TOKEN usingOutlook= @ TOTAL = 0
TOKEN usingUtilitiesCS; @ TOTAL = 0
TOKEN usingSystem.Diagnostics; @ TOTAL = 0
TOKEN usingSystem.Text.RegularExpressions; @ TOTAL = 0
TOKEN usingUtilitiesCS.EmailIntelligence; @ TOTAL = 0
TOKEN usingUtilitiesCS.ReusableTypeClasses @ TOTAL = 0
TOKEN usingSystem.Windows.Forms; @ TOTAL = 0
TOKEN usingUtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder; @ TOTAL = 1
TOKEN usingUtilitiesCS.OutlookExtensions; @ TOTAL = 1
TOKEN usingSystem; @ TOTAL = 1
TOKEN #nullableenable @ TOTAL = 1
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = 268
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = D08A06D2921F987BB6D0FCFDDA6CED5DAF88801C56A30E7D780234A8B71A2161
```

(Each token's single per-file line carries the same value as its TOTAL line.)

## Acceptance (P5-T1)

Every TOKENS-S total equals the S FINAL column (`usingDeedle;`, `usingSDILReader;`, `usingOutlook=`, `usingUtilitiesCS;`, `usingSystem.Diagnostics;`, `usingSystem.Text.RegularExpressions;`, `usingSystem.Windows.Forms;`, `usingUtilitiesCS.EmailIntelligence;` and `usingUtilitiesCS.ReusableTypeClasses` each 0; `usingSystem;`, `usingUtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;` and `usingUtilitiesCS.OutlookExtensions;` each 1; `[ExcludeFromCodeCoverage]` 4): met.

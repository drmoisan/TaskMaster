# P5-T2 SortEmail.MailItemSort.cs Using Block (D10)

Timestamp: 2026-10-03T12-18
Command: Edit E-M-USINGS applied to UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs (lines 2 to 19 replaced by the nine-directive block); then CMD-CENSUS (PATHS-M with the TOKENS-M column of TOKENS-S and TOKENS-M)
EXIT_CODE: 0 (scoped to the CMD-CENSUS payload, its process exit code)
Output Summary: The eighteen-directive using block of M was replaced by the nine directives of E-M-USINGS (the anchor occurred once). Every TOKENS-M total equals the M FINAL column. M now has 379 lines.

```
TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 5
TOKEN usingDeedle; @ TOTAL = 0
TOKEN usingSDILReader; @ TOTAL = 0
TOKEN usingOutlook= @ TOTAL = 0
TOKEN usingUtilitiesCS; @ TOTAL = 0
TOKEN usingSystem.Diagnostics; @ TOTAL = 0
TOKEN usingSystem.Text.RegularExpressions; @ TOTAL = 0
TOKEN usingUtilitiesCS.EmailIntelligence; @ TOTAL = 0
TOKEN usingUtilitiesCS.ReusableTypeClasses @ TOTAL = 0
TOKEN usingSystem.Windows.Forms; @ TOTAL = 1
TOKEN usingUtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder; @ TOTAL = 0
TOKEN usingUtilitiesCS.OutlookExtensions; @ TOTAL = 1
TOKEN usingSystem; @ TOTAL = 1
TOKEN #nullableenable @ TOTAL = 1
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 379
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 61FED1959AFE0C8D322ADDC88405E347E21CE44F038018E578D1FEBB7CB388AB
```

(Each token's single per-file line carries the same value as its TOTAL line.)

## Acceptance (P5-T2)

Every TOKENS-M total equals the M FINAL column (`usingSystem.Windows.Forms;` 1, `usingUtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;` 0, `usingUtilitiesCS.OutlookExtensions;` 1, the eight removed directives 0, `[ExcludeFromCodeCoverage]` 5): met.

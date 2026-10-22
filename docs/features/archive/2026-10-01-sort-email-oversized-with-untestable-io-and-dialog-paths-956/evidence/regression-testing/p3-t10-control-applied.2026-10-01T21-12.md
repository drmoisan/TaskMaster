# P3-T10 Negative control applied (clearReadOnly call removed)

Timestamp: 2026-10-01T21-12
Command: CMD-CONTROL-BACKUP (byte copy of UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs to the git-ignored coverage\control-956\SortEmail.TrySaveAttachment.fixed.bak); then one Edit replacing `clearReadOnly(directory);` (one occurrence) with `// NEGATIVE-CONTROL-956`; then CMD-CENSUS with PATHS-TRYSAVE and TOKENS-TRYSAVE (single-file census printed as TOTAL lines labelled by table ID A1 to A24, as in P3-T6)
EXIT_CODE: 0
Output Summary:
FIX-HASH-TRYSAVE: B1E3570AF37D4EBCB0118C39DE283F485D4AFF6401F04BFDDC2A50CBD798719E
BACKUP-HASH-TRYSAVE: B1E3570AF37D4EBCB0118C39DE283F485D4AFF6401F04BFDDC2A50CBD798719E
TOKEN A1 @ TOTAL = 1
TOKEN A2 @ TOTAL = 1
TOKEN A3 @ TOTAL = 1
TOKEN A4 @ TOTAL = 1
TOKEN A5 @ TOTAL = 1
TOKEN A6 @ TOTAL = 1
TOKEN A7 @ TOTAL = 1
TOKEN A8 @ TOTAL = 1
TOKEN A9 @ TOTAL = 1
TOKEN A10 @ TOTAL = 1
TOKEN A11 @ TOTAL = 1
TOKEN A12 @ TOTAL = 1
TOKEN A13 @ TOTAL = 0
TOKEN A14 @ TOTAL = 1
TOKEN A15 @ TOTAL = 1
TOKEN A16 @ TOTAL = 3
TOKEN A17 @ TOTAL = 2
TOKEN A18 @ TOTAL = 0
TOKEN A19 @ TOTAL = 1
TOKEN A20 @ TOTAL = 2
TOKEN A21 @ TOTAL = 2
TOKEN A22 @ TOTAL = 1
TOKEN A23 @ TOTAL = 3
TOKEN A24 @ TOTAL = 0
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 172
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 54C7BB243E6D73B11623420CBB21B437847C690BEA70583B9606D039570B2027
Acceptance: BACKUP-HASH-TRYSAVE equals FIX-HASH-TRYSAVE; every TOKENS-TRYSAVE total equals the CONTROL column (A13 0, A18 0, A19 1, all others as in SEAM); the census SHA256 differs from FIX-HASH-TRYSAVE (all hold).

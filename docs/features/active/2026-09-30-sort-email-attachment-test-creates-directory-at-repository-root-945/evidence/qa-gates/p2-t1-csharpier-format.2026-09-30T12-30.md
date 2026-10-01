# P2-T1 repository-wide CSharpier format

Timestamp: 2026-09-30T12-30
ITERATION: 1
Command: dotnet tool run csharpier format .
EXIT_CODE: 0

Output Summary:
Formatted 1627 files in 5732ms.
SRC (UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs) SHA256 before: 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B
SRC SHA256 after: 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B
TST (UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs) SHA256 before: 791E2B9E00C565C428939F0A3A26B8DE4A613AC492B5D16F80A2CD9627F2957E
TST SHA256 after: 791E2B9E00C565C428939F0A3A26B8DE4A613AC492B5D16F80A2CD9627F2957E
Anchored patch (git diff MERGE-BASE -- UtilitiesCS UtilitiesCS.Test) SHA256 before: 53776279F29CD68E2CA84510391B557FB21FD7B7DF8B92AAA4ECFD1A7FB5636E
Anchored patch SHA256 after: 53776279F29CD68E2CA84510391B557FB21FD7B7DF8B92AAA4ECFD1A7FB5636E
REWRITTEN: 0 (P1-T6 prediction of 0: met)
FORMAT_CHANGED_TREE: False (the two patch hashes are equal)
PORCELAIN-OUTSIDE-SET: NONE

Porcelain output (git status --porcelain --untracked-files=all), verbatim:
 M UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs
 M UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs
 M docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/issue.md
 M docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/plan.2026-09-30T07-20.md
?? (untracked evidence artifacts, all under docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/evidence/baseline/, evidence/other/ and evidence/regression-testing/; 21 paths)

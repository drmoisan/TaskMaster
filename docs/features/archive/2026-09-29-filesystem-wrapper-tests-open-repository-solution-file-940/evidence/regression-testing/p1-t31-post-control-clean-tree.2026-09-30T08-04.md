# Post-Control Clean Tree (P1-T31)

Timestamp: 2026-09-30T08-04
Task: P1-T31
Command: git diff --exit-code HEAD -- UtilitiesCS UtilitiesCS.Test; git diff --exit-code ANCHOR-SHA -- UtilitiesCS (ANCHOR-SHA substituted with 231e1c0b55105aeb626bf5a6e8d0266a567cacad, the P0-T3 `ANCHOR-SHA:` value); git status --porcelain -- UtilitiesCS UtilitiesCS.Test; git rev-parse origin/main; Get-FileHash -Algorithm SHA256 of the two Write Set files and the three production files
EXIT_CODE: 0 (scoped to `git diff --exit-code HEAD -- UtilitiesCS UtilitiesCS.Test`)
PRODUCTION-DIFF-EXIT: 0
ORIGIN-MAIN-NOW: 231e1c0b55105aeb626bf5a6e8d0266a567cacad (equal to ANCHOR-SHA; not gated)
Output Summary: after the eleven controls, UtilitiesCS and UtilitiesCS.Test equal HEAD, the UtilitiesCS working tree is byte-identical to the ANCHOR-SHA tree, the scoped porcelain is empty, and all five hashes equal their anchors. No temporary production edit and no control residue survives into Phase 2 (AC6 mechanical proof).

- HEAD-DIFF-EXIT: 0 (no output)
- PRODUCTION-DIFF-EXIT: 0 (no output)
- PORCELAIN: EMPTY

## Hashes beside anchors

| File | SHA-256 now | Anchor | Equal |
| --- | --- | --- | --- |
| UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs | C88A785C23D8DB2960E9FAA53DF9EF91F6F00359683485BEC7EDFC44F9A2F998 | FIX-HASH-PFS: C88A785C23D8DB2960E9FAA53DF9EF91F6F00359683485BEC7EDFC44F9A2F998 | yes |
| UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs | 6650B33204BABCAAB7CD6E97C8B4BA7012ABB1F1320DB6AF7764ECD8714B7910 | FIX-HASH-DIW: 6650B33204BABCAAB7CD6E97C8B4BA7012ABB1F1320DB6AF7764ECD8714B7910 | yes |
| UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs | FBAC7002BE4DF5979624F8AE649D52F1044BC40D54EBDA4618C6374EAB9CB242 | PRE-EDIT-HASH-PDA: FBAC7002BE4DF5979624F8AE649D52F1044BC40D54EBDA4618C6374EAB9CB242 | yes |
| UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs | 0337E5C1FF7A2E3FF5E5D67D383E68583838AFB5E5BA999A30502CF24333D948 | PRE-EDIT-HASH-PFA: 0337E5C1FF7A2E3FF5E5D67D383E68583838AFB5E5BA999A30502CF24333D948 | yes |
| UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs | F77616271ABB36C9133ADE496550703B391B077AB68C8F5DC652AB73FD177DC3 | PRE-EDIT-HASH-DIWP: F77616271ABB36C9133ADE496550703B391B077AB68C8F5DC652AB73FD177DC3 | yes |

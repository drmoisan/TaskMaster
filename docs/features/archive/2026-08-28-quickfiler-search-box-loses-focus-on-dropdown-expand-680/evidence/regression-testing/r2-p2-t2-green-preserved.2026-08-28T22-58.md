Timestamp: 2026-08-28T22-58
Command: mkdir evidence/regression-testing/r-p2-t3/; Copy-Item evidence/regression-testing/p2-t3/p2-t3.trx evidence/regression-testing/r-p2-t3/p2-t3.trx
EXIT_CODE: 0
Output Summary: Destination file exists. (Get-FileHash <source>).Hash equals (Get-FileHash <dest>).Hash
immediately after the copy (verified via SHA-256; both hashes matched exactly). The remediation's
green-run TRX is preserved at evidence/regression-testing/r-p2-t3/p2-t3.trx before the original path is
overwritten by the P2-T3 restore.

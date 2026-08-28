Timestamp: 2026-08-28T23-11
Command: git diff HEAD -- <file> | Select-String -Pattern '^-[^-]'; Select-String -SimpleMatch -Pattern
"Relocation Addendum" <file>; Select-String -SimpleMatch -Pattern
"evidence/regression-testing/r-p2-t3/p2-t3.trx" <file>
EXIT_CODE: 0
Output Summary: `git diff HEAD` for the addendum target file shows 0 matches for the removed-line
pattern `^-[^-]` (proves the edit is append-only). `Select-String -SimpleMatch -Pattern "Relocation
Addendum"` returns exactly 1 hit. `Select-String -SimpleMatch -Pattern
"evidence/regression-testing/r-p2-t3/p2-t3.trx"` returns 1 hit (>= 1 required). All three acceptance
conditions hold.

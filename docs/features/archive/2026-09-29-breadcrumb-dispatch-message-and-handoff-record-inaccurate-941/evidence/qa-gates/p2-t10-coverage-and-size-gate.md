# P2-T10 AC9 gate, coverage and size
Timestamp: 2026-10-01T07-32
Command: read p2-t7-coverage-delta.md and p2-t8-scope-and-size.md; git grep --no-index -c -F "AC9: NOT MET" over both files
EXIT_CODE: 0
Output Summary:
  p2-t7-coverage-delta.md: changed-line clauses held (lines 180-187 baseline HITS 1, final HITS 1). QF-RATE-DELTA: none (measurement 2 equals baseline; measurement 1 was one unit lower and is reported in that artifact).
  p2-t8-scope-and-size.md: footprint .cs files 285, 480 and 410 lines, all at or below 500; scope union and census held.
  git grep --no-index -c -F "AC9: NOT MET" over both files: no match (exit 1).
  AC9: met.

# P1-T9 Handoff record corrected (replacements E1 to E5)

Timestamp: 2026-10-01T07-22
Command: Edit applications of E1 to E5 to docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md; then git diff -U0 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f over that path with a hunk-header parse of the old-range token, git grep -n -F per token, and a [regex]::Matches CRLF/LF count
EXIT_CODE: 0
Output Summary:
- Hunk headers (old range token only; the text after the closing @@ is ignored): `-24,2` (old 24..25, inside 23-31), `-31` (old 31..31, inside 23-31), `-41,4` (old 41..44, inside 40-44), `-48,4` (old 48..51, inside 48-51). Hunk count 4; every hunk inside one of the ranges 23-31, 40-44, 48-51.
- Each required token occurs exactly 1 time: `Only the first is exercising`, `which never reads the owner thread id`, `inlining hazard described here applies to the`, `faults for every caller outside an executing dispatcher callback`, `delegate (line 301) that calls`, `so this site is not exposed to the inlining hazard`, `and supplied; the owner check compares`, `no idle-thread-reuse exposure`, `:276-277`.
- Each superseded token occurs 0 times: `owner-thread-id check rather than against`, `expected to throw a cross-thread marshalling`, `Its exposure is lower but not zero`, `: the owner check compares`.
- Line count 186 (180 plus the net 6 added by E1 to E5); CRLF count 186, LF count 186 (all equal).
- AC6 checked off in FEATURE/issue.md.

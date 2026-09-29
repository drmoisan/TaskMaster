# Outlook State ([P0-T8])

Timestamp: 2026-09-29T08-53
Command: pwsh -NoProfile -Command 'if (Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue) { "OUTLOOK_STATE=RUNNING" } else { "OUTLOOK_STATE=CLOSED" }'
EXIT_CODE: 0
Output Summary:
- OUTLOOK_STATE=CLOSED

## Re-runs (appended)

Each re-run used the same command; the minute shown is the minute of the Rebuild it preceded (taken from that Rebuild's log write time or the adjacent CMD-TS value), because the re-runs were executed inline immediately before each Rebuild rather than timestamped separately.

- 2026-09-29T08-54 (before the re-run of [P0-T10]): OUTLOOK_STATE=CLOSED
- 2026-09-29T09-10 (before [P1-T2]): OUTLOOK_STATE=CLOSED
- 2026-09-29T09-12 (before [P1-T5]): OUTLOOK_STATE=CLOSED
- 2026-09-29T09-14 (before [P1-T8]): OUTLOOK_STATE=CLOSED
- 2026-09-29T09-15 (before [P1-T11]): OUTLOOK_STATE=CLOSED
- 2026-09-29T09-19 (before [P2-T4]): OUTLOOK_STATE=CLOSED
- 2026-09-29T09-19 (before [P2-T5]): OUTLOOK_STATE=CLOSED

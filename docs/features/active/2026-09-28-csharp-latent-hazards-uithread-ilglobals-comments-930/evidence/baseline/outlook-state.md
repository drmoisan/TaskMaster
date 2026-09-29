# Outlook State ([P0-T8])

Timestamp: 2026-09-29T08-55
Command: pwsh -NoProfile -Command 'if (Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue) { "OUTLOOK_STATE=RUNNING" } else { "OUTLOOK_STATE=CLOSED" }'
EXIT_CODE: 0
Output Summary:
- OUTLOOK_STATE=CLOSED

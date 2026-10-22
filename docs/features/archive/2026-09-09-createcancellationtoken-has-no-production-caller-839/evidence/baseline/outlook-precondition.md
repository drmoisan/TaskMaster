# Outlook precondition before any solution build (issue #839)

Timestamp: 2026-09-13T02-30
Command: pwsh -NoProfile -Command 'if (Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue) { "OUTLOOK_STATE=RUNNING" } else { "OUTLOOK_STATE=CLOSED" }'
EXIT_CODE: 0

Output Summary:
- OUTLOOK_STATE=CLOSED
- Final state: OUTLOOK_STATE=CLOSED. Outlook was already closed at the first invocation, so no request to the user was needed and no process was terminated. No Stop-Process and no taskkill was used at any point.
- This gate is re-run before each msbuild task in the plan if Outlook may have been reopened in the interim.

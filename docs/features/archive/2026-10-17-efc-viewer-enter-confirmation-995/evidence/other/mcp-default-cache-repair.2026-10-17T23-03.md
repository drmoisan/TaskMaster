# Issue 995 normal MCP startup cache repair

Timestamp: 2026-10-17T23-03
Command: npm cache npx rm deda2f51b29ef8f6
EXIT_CODE: 0
Output Summary: Removed only the unused invalid 1.1.21 npx cache entry through npm's supported cache command. Verified resolved target remained inside the intended npx cache root and no node process used this entry. Running 1.1.20 package files were untouched.

The earlier new-version startup failed because this entry contained node_modules without package.json. npm cache npx ls identified it as empty/invalid. Standard npx startup subsequently reinstalled the published package successfully; see the corresponding normal-startup validation artifact. No manual package edits or validator changes were made.
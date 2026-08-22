Timestamp: 2026-08-22T13-13
Command: pwsh -NoProfile -Command 'New-Item -ItemType Directory -Force -Path "coverage\msbuild", "coverage\logs" | Out-Null'; git check-ignore -q coverage/msbuild; git check-ignore -q coverage/logs
EXIT_CODE: 0
Output Summary: Both directories created (or already present) successfully. Both `git check-ignore -q` invocations reported `EXIT_CODE: 0`, confirming `coverage/msbuild/` and `coverage/logs/` are excluded from version control. Raw MSBuild and vstest logs written under these directories cannot leave `git status --porcelain` dirty. Derived counts (error/CoreCompile/Skipping-target counts) are copied into committed evidence artifacts; raw logs are not committed.

mkdir EXIT_CODE: 0
coverage/msbuild check-ignore EXIT_CODE: 0
coverage/logs check-ignore EXIT_CODE: 0

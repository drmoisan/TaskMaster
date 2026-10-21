# Baseline: csharpier format check (read-only) — issue #839

Timestamp: 2026-09-13T02-47
Command: dotnet tool run csharpier check .
EXIT_CODE: 0

Output Summary:
- Final console line: Checked 1624 files in 5956ms.
- Count of lines containing "Was not formatted": 0.
- The read-only check subcommand was used at baseline, never the write-mode format subcommand, so no pre-existing formatting drift was silently repaired before the baseline was taken.
- The invocation was wrapped with a redirect and a Tee-Object to the gitignored log coverage/839-baseline-format.log purely so the console output could be transcribed into this artifact; the csharpier subcommand and its argument are unchanged from the plan's CMD-FORMAT-CHECK label. The exit code above is the checker's own.
- The check ran while this item held the shared machine build lock, which was released immediately after it returned.

# Final QA — Step 1 Verification, CSharpier Check (P7-T2, AC-29 second half)

Timestamp: 2026-08-27T20-57

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

Output Summary: `Checked 1542 files in 7036ms.`

**Files reported as needing formatting: 0.**

CSharpier `check` emits one `Error <path> - Was not formatted.` line plus an expected/actual diff for
every file that needs formatting, and exits non-zero when that count is greater than zero. The output
carries no `Error` line and no `Warning` line, and the exit code is 0, so the whole tree is formatted.

## Idempotence of the formatting step

This read-only run immediately follows P7-T1's `csharpier format .`, which rewrote 3 files. Its clean
result establishes that the formatter is now at a fixed point: a second `format` pass would rewrite
nothing. That is what allows the toolchain loop to advance to step 2 rather than restart — the
"restart if any step changes files" rule is about a step OTHER than the formatter changing files, and
the formatter itself is verified quiescent here.

It also confirms the line counts recorded from this point on are stable. No later step in the loop
rewrites source, so the P7-T8 post-format line-count audit measures a state the formatter will not move.

The invocation is `dotnet tool run csharpier`, the manifest-pinned 1.2.6 resolved at P0-T9. No globally
installed CSharpier was used at any point, as `CLAUDE.md` C#1.1 requires.

Acceptance: `EXIT_CODE: 0` and zero files needing formatting. PASS.

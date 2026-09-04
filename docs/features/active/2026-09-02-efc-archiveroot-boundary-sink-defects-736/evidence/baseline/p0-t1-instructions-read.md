# P0-T1 — Policy instructions read

Timestamp: 2026-09-03T23-30

Policy Order: the order below is the order given by the `policy-compliance-order` skill, and is the
order in which the six files were read.

1. `CLAUDE.md` — 447 lines
2. `.claude/rules/general-code-change.md` — 80 lines
3. `.claude/rules/general-unit-test.md` — 105 lines
4. `.claude/rules/quality-tiers.md` — 51 lines
5. `.claude/rules/csharp.md` — 96 lines
6. `.claude/rules/tonality.md` — 80 lines

Files listed above that do not exist on disk: none. Every one of the six files was located and read;
no entry carries the literal `ABSENT`.

Command: pwsh -NoProfile -Command '$f=@("CLAUDE.md",".claude\rules\general-code-change.md",".claude\rules\general-unit-test.md",".claude\rules\quality-tiers.md",".claude\rules\csharp.md",".claude\rules\tonality.md"); foreach($p in $f){ if(Test-Path $p){ "{0} = {1}" -f $p,(Get-Content -LiteralPath $p).Count } else { "{0} = ABSENT" -f $p } }'

EXIT_CODE: 0

Output Summary: all six policy files exist in this worktree and were read in the order above. Line
counts, in order: 447, 80, 105, 51, 96, 80. No file was absent.

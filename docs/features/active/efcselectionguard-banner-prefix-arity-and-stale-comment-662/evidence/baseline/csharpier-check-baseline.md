# CSharpier Read-Only Baseline (P0-T6)

Timestamp: 2026-09-01T15-45

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

Output Summary:

Final summary line, transcribed verbatim:

```
Checked 1566 files in 4937ms.
```

That is the complete output of the run. The exit code is 0 and the check named
no file as unformatted, so the list of unformatted files is empty.

Because the exit code is zero, the BLOCKED branch of this task's acceptance
does not arise: there is no file, in scope or out of scope, that the Phase 2
repository-wide format pass would rewrite on account of pre-existing drift.

This is the read-only `check` subcommand and rewrites nothing. The mutating
`format` pass does not run until Phase 2 (P2-T1), so this baseline describes the
tree as the work found it, per Decisions Record D8.

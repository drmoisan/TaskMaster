# QC loop step 2 — csharpier check (Issue #824, task P5-T2)

Timestamp: 2026-09-09T15-59

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; dotnet tool run csharpier check . 2>&1 | Tee-Object -FilePath coverage/csharpier-check.log; exit $LASTEXITCODE'`

EXIT_CODE: 0

Output Summary:

`coverage/csharpier-check.log` carries exactly **1** line. The last line, reproduced verbatim:

```
Checked 1622 files in 4670ms.
```

Because the log carries only one line, there is no preceding line to reproduce. **One line was
reproduced.** The plan requires the last line verbatim and, when the log carries more than one line,
the line preceding it as well; that second condition does not arise here. The number of summary
lines a clean `csharpier check` run prints was not observed while the plan was authored, and this
run establishes it as one.

The `check` subcommand exits non-zero when any file needs formatting, so `EXIT_CODE: 0` is a real
discriminator and not an artefact of the command always succeeding. Zero files need formatting,
which is what AC11 step 1 requires.

This result follows the second P5-T1 pass, in which `FORMAT_CHANGED_TREE=False`. The two
observations are consistent: the formatter changed nothing, and the read-only check confirms nothing
remains to change.

The log stays under `coverage/`, which is gitignored, and is not committed.

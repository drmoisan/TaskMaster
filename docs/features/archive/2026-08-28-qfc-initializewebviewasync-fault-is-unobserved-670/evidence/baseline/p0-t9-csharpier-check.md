# P0-T9 — CSharpier formatting baseline (read-only)

Timestamp: 2026-09-01T19-46
Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

Output Summary:

Final summary line, reproduced verbatim from the command's output:

    Checked 1566 files in 4553ms.

Files the command named as needing formatting: **none**. The command printed the summary line and nothing else, so the list is empty.

## Admission condition

The admission condition is that no file the command names lies under `QuickFiler/` or `QuickFiler.Test/`. The command named no file at all, so the condition holds vacuously in the strongest available sense: the tree carries no CSharpier drift anywhere, not merely none in those two directories. The plan is not blocked.

This baseline was captured **before** any write-mode formatter ran in this delivery run. That ordering is load-bearing rather than procedural: a `dotnet tool run csharpier format .` executed first would silently repair any pre-existing drift into the baseline, after which the P4-T1 gate — which requires the repo-wide format to rewrite nothing under `QuickFiler/` or `QuickFiler.Test/` — would either become a blanket waiver or become unsatisfiable. A clean baseline here is what makes the repo-wide format in P4-T1 a genuine no-op outside this plan's own files, and that in turn is what makes the zero-changed-lines gate on `ViewerSetup.cs` in P2-T4 satisfiable.

## Falsifiability

`csharpier check` is the read-only verification mode: it exits non-zero and names each offending file when a file's on-disk text differs from the formatter's output. The exit-0-with-no-named-files result recorded here is therefore distinguishable from a drifted tree, and the same command run against a drifted tree in this delivery run would report differently. That contrast is observed directly in P1-T5, where the newly authored production file is checked before and after its first `format` invocation.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.

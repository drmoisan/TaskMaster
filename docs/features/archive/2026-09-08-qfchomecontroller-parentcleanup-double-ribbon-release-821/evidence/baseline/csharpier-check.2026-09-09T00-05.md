# Phase 0 — Baseline formatting (read-only)

Timestamp: 2026-09-09T12-33
Task: [P0-T7]

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

Complete verbatim output:

```text
Checked 1622 files in 4446ms.
```

Output Summary: **0 files reported as needing formatting**, and therefore **no path list**. The run
checked 1622 files and printed no per-file diff or warning line; `csharpier check` prints one line
per non-conforming file and exits 1 when any file needs formatting, so a bare summary line with
exit 0 is the zero-drift result.

`check` is read-only, so its exit code is a valid and discriminating gate here rather than a
write-mode command whose exit code is identical on a clean and a repairing run.

Consequence for AC17 and AC20: the repository has **no pre-existing formatting drift**. The
repo-wide `dotnet tool run csharpier format .` that AC17 demands at `[P6-T1]` therefore cannot
rewrite an out-of-scope file to repair pre-existing drift, so the only `.cs` paths that run can leave
changed are files this plan itself edits. `[P6-T1]`'s acceptance condition — that every `.cs` path in
the post-format spans is one of the eight Write Set files — is consequently satisfiable rather than
pre-poisoned.

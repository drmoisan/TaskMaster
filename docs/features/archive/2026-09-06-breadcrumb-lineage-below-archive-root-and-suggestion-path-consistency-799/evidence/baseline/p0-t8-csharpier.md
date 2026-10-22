# [P0-T8] CSharpier formatting baseline

Timestamp: 2026-09-07T06-55

Command: dotnet tool run csharpier check .

EXIT_CODE: 0

## Verbatim printed line

```
Checked 1593 files in 6423ms.
```

BASELINE-CSHARPIER-CHECKED-FILES: 1593

Output Summary: The read-only CSharpier check exited 0 and printed the single success-case line
`Checked 1593 files in 6423ms.` with no drift entry, so the tree is formatting-clean at the base commit and there
is no disclosed pre-existing drifting-path set to carry forward. `check` returns non-zero on drift, so the exit
code is the gate for this task. The formatter was invoked through `dotnet tool run` against the manifest-pinned
version 1.2.6 confirmed by [P0-T5]. The checked-file count includes the `*.xml` and `packages.config` documents
CSharpier 1.2.6 processes in addition to `*.cs`; project files are excluded by `.csharpierignore`.

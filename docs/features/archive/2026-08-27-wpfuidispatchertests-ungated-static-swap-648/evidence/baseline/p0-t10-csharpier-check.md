# P0-T10 — CSharpier Baseline (Read-Only)

Timestamp: 2026-09-01T13-27

Command: `dotnet tool run csharpier check .` (run from the checkout root, with `PATH` and
`DOTNET_ROOT` pointed at the repository-local `.dotnet-sdk` directory)

EXIT_CODE: 0

Output Summary:

The command was executed immediately after P0-T5 and before P0-T6, per the ordering instruction in
the task text. At that moment `ls -d packages bin obj` reported `No such file or directory` for all
three, so the measured tree carried no restored `packages/` directory and no `bin/` or `obj/` build
output. `.dotnet-sdk/` was present, because P0-T4 installs it and this task runs after P0-T5; that
one directory is therefore present on both sides of the P2-T2 comparison and cannot introduce an
asymmetry in either direction.

The command produced exactly one output line, recorded verbatim:

```
Checked 1566 files in 4892ms.
```

No formatting step was run. This task executed `check`, which is read-only, and did not execute
`format`.

## Unfiltered path list

The command named no file paths. The complete unfiltered list of paths this run named is empty.

## SourceScopedDrift

SourceScopedDrift: none

Derivation: the segment rule removes from the unfiltered list every path having a path segment equal
to `packages`, `.dotnet-sdk`, `bin`, or `obj`, in either separator spelling. The unfiltered list is
empty, so the filtered list is empty and `SourceScopedDrift:` is recorded as the literal `none`. The
filter was inert on this side of the comparison, which is the outcome the task records as expected.

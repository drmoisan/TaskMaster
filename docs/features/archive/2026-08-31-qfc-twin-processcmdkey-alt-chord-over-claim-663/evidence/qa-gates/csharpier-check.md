# Phase 4 — Repository-wide formatting verification ([P4-T2])

Timestamp: 2026-09-01T23-01

Command: `dotnet tool run csharpier check .`

This is a read-only, repository-wide invocation, and it is the gate. It is equivalent to, but not
character-for-character identical with, the CI command: `.github/workflows/_format-check.yml` line 41 is
`run: dotnet csharpier check .`, which omits the `tool run` segment. Both resolve to the same
manifest-pinned 1.2.6 executable, because that job restores the tool manifest first on line 37.

EXIT_CODE: 0

Final summary line, verbatim:

```
Checked 1566 files in 4478ms.
```

## Acceptance readings

- Exit code is **0**, as required.
- **No reported unformatted path.** CSharpier listed no file as needing formatting.

BASELINE_CSHARPIER_EXIT recorded by `[P0-T8]` is 0, so the primary clause applies and the carry-forward
disposition does not. The three additional scoped read-only invocations that the carry-forward branch
would have required were therefore not run; this is recorded explicitly so their absence is not read as a
skipped step.

The file count, 1566, is identical to the `[P0-T8]` baseline count, which is consistent with this change
adding no file and removing none.

Output Summary: The read-only repository-wide CSharpier check exited 0 over the same 1566 files the
Phase 0 baseline checked, and reported no unformatted path. This is the same reading `[P0-T8]` recorded
as BASELINE_CSHARPIER_EXIT, so the format stage of the Phase 4 toolchain loop passes without a
carry-forward disposition.

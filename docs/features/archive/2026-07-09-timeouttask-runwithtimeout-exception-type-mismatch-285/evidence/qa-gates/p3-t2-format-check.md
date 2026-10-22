# P3-T2 — Format Verification (QC loop stage 1 verify)

Timestamp: 2026-09-01T08-23

Command: `dotnet tool run csharpier check .` (run from the repository root)

EXIT_CODE: 0

## Output Summary

Complete captured output, a single summary line:

```text
Checked 1565 files in 4487ms.
```

**Count of files reported as needing formatting: 0.**

CSharpier emitted no per-file diagnostic and no file path. In check mode a non-zero exit is
CSharpier's signal that at least one file needs formatting, so the exit code of 0 is itself a
discriminating observation here, and it agrees with the empty file list.

1565 files were examined, the same count as the P0-T6 baseline check and the P3-T1 format pass, so
the same file set was covered by all three invocations.

This is the read-only, CI-parity verification that `.github/workflows/ci.yml` runs after
`dotnet tool restore`. It was invoked through `dotnet tool run` so the manifest-pinned CSharpier
1.2.6 recorded by P0-T5 was used, not a globally installed version.

This artifact is the evidence cited by the AC8 check-off at P4-T8, whose criterion is that
`dotnet tool run csharpier check .` reports no unformatted files.

Acceptance: met. `EXIT_CODE: 0` and a reported unformatted-file count of 0.

# QA Gate 01 — CSharpier Format (P8-T1)

Timestamp: 2026-07-07T23-35

Command: dotnet tool run csharpier format . (then dotnet tool run csharpier check .)
(CSharpier 1.2.6; `format` writes, `check` verifies. The plan's `dotnet tool run csharpier .` is the
v0 default-format form; the v1 subcommands are the mechanical equivalent.)

EXIT_CODE: 0

Output Summary:
- First `format` run reformatted multi-line assertions/members across the touched files (wrapping),
  so the toolchain loop restarted at P8-T1 per the loop rule.
- Follow-up `csharpier check .` reported "Checked 1283 files" with 0 files needing formatting.
- Final state: formatting clean, idempotent, EXIT_CODE 0. No residual diff on any scope-lock file.

# Phase 0 — Policy Read Evidence (P0-T1)

Timestamp: 2026-07-10T01-17

Policy Order: CLAUDE.md → .claude/rules/general-code-change.md → .claude/rules/general-unit-test.md → .claude/rules/csharp.md → .claude/rules/quality-tiers.md

Files read (in required order):

1. `CLAUDE.md` — project standing instructions, C# toolchain order, MSTest/Moq/FluentAssertions requirement, coverage floors (80% project / 90% new), COM/VSTO/WinForms coverage exemption policy.
2. `.claude/rules/general-code-change.md` — cross-language design principles, 500-line file limit, fail-fast error handling, mandatory toolchain loop.
3. `.claude/rules/general-unit-test.md` — independence/isolation/determinism, coverage thresholds, no temp files, banned timing APIs, deterministic infrastructure.
4. `.claude/rules/csharp.md` — CSharpier formatting, MSBuild analyzer + nullable gates, vstest coverage, DI seam preference order (interface > delegate > adapter), banned APIs (DateTime.Now/UtcNow, Thread.Sleep, Task.Delay), analyzer stack.
5. `.claude/rules/quality-tiers.md` — T1–T4 module rigor tiers and uniform-vs-tier-dependent gate matrix.

Acceptance: artifact exists and enumerates all five files in order. PASS.

Tooling resolution for this worktree:
- `csharpier` = v1.3.0 on PATH (uses `check`/`format` subcommands).
- `dotnet` on PATH.
- `msbuild` NOT on PATH; using `C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe`.
- `vstest.console.exe` NOT on PATH; using `C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe`.

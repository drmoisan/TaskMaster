# Phase 0 — Instructions Read

Timestamp: 2026-06-13T01-05

Policy Order: CLAUDE.md → .claude/rules/general-code-change.md → .claude/rules/general-unit-test.md → .claude/rules/csharp.md

## Files Read (in order)

1. `CLAUDE.md` — standing project instructions (auto-loaded). Confirmed C#1 CSharpier
   scope statement: "csharpier is file-based and formats only `*.cs` without touching
   project files" and "Do not use `dotnet format` — it ... rewriting `.csproj` files."
2. `.claude/rules/general-code-change.md` — cross-language code change policy
   (auto-loaded in session context).
3. `.claude/rules/general-unit-test.md` — cross-language unit test policy
   (auto-loaded in session context).
4. `.claude/rules/csharp.md` — C#-specific toolchain and coding standards
   (read this session). Confirmed CSharpier formats C# source only; project files are
   not C# source.

## Scope Confirmation

This remediation edits ONLY `.csharpierignore`. No `.cs`, `.csproj`, `.props`,
`.targets`, or workflow file is modified. Per the plan's Toolchain Applicability
section, the analyzer, nullable, and test/coverage gates are N/A; the only required
empirical gate is `dotnet csharpier check .`.

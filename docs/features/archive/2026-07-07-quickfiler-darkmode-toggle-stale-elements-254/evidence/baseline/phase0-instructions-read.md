# Phase 0 — Policy Instructions Read (Issue #254)

Timestamp: 2026-07-07T13-00

Policy Order: policy-compliance-order sequence for C# work

Files read (in order):
1. `CLAUDE.md` (repository standing instructions — always loaded)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (C#-specific code standards, toolchain, DI seams, analyzer stack)

Supporting rules also in context: `.claude/rules/quality-tiers.md`, `.claude/rules/architecture-boundaries.md`, `.claude/rules/tonality.md`.

Key constraints acknowledged for this task:
- Minimal, targeted fix (single production file `Theme.Rendering.cs`); no opportunistic refactor.
- Narrow catch (`System.Runtime.InteropServices.COMException`), not broad `Exception`; `// why` comment required.
- MSTest + FluentAssertions; Moq available. Deterministic, seam-based test; no live Outlook/COM/WinForms; no temp files.
- Coverage: no regression on changed lines; new/changed code >= 90%.
- Toolchain order: csharpier -> analyzers msbuild -> nullable msbuild -> vstest with coverage; restart on any change/failure.
- Evidence only under `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/evidence/<kind>/`.

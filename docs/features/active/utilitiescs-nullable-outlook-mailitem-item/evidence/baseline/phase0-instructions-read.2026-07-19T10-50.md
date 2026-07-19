# Phase 0 — Instructions Read (P0-T1)

- Timestamp: 2026-07-19T10-50
- Task: [P0-T1]
- Feature: utilitiescs-nullable-outlook-mailitem-item (#371)
- Branch: bug/utilitiescs-nullable-outlook-mailitem-item-371 (branched from epic integration tip dffadd5a)

## Policy Order

Policy reading order per `policy-compliance-order`:

1. `CLAUDE.md` (standing instructions; loaded via session context)
2. `.claude/rules/general-code-change.md` (cross-language code change policy; loaded via session context)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy; loaded via session context)
4. `.claude/rules/csharp.md` (C#-specific toolchain and coding standards)

## Files Read (explicit list)

Policy files:
- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`

Requirements sources (per plan Requirements Sources block):
- `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/spec.md` (Definition of Done + AC1–AC6; AC source)
- `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/user-story.md` (Acceptance Criteria; AC source)
- `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/issue.md`
- `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/research/research.2026-07-18T22-15.md`
- `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/plan.2026-07-18T22-05.md`

## Key Constraints Confirmed

- Per-file `#nullable enable` pragma ONLY. No `<Nullable>` element in `UtilitiesCS.csproj` or the solution. No `/p:Nullable=enable` on any msbuild command.
- Nullable/type-check gate command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`.
- Annotation and null-safety only; no behavior change, no refactor, no API redesign, no new runtime guards to satisfy the compiler.
- net481 / C# 12: no `System.Diagnostics.CodeAnalysis` nullable post-condition attributes; no `init`/positional `record`/`record struct`.
- Three partial-class groups verified as one unit each: `MailItemHelper` (5), `ConvHelper` (2), `OlTableExtensions` (4).
- COM/VSTO coverage exemption for all in-scope files except `CidImageResolver.cs`.
- Flag-not-fix: `OutlookItem.cs` 503-line breach; `dynamic item` in `OlToDoTable.EnsureItemValues`.
- 30 in-scope files confirmed (MailItem 12, Item 9, Conversation 2, Attachment 2, Table 5).

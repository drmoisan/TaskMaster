# Phase 0 — Policy and Requirements Read Evidence (Issue #364)

- Timestamp: 2026-07-19T08-51
- Task: [P0-T1]

## Policy Order

Per `policy-compliance-order` and the plan's P0-T1 read order:

1. `CLAUDE.md` (standing instructions, always loaded)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (C#-specific toolchain and standards)

Then the requirements sources:

5. `docs/features/active/utilitiescs-nullable-helperclasses/spec.md` (Definition of Done — AC source)
6. `docs/features/active/utilitiescs-nullable-helperclasses/user-story.md` (Acceptance Criteria — AC source)
7. `docs/features/active/utilitiescs-nullable-helperclasses/issue.md`
8. `docs/features/active/utilitiescs-nullable-helperclasses/research/research-findings.2026-07-18T21-45.md`

## Files Read (explicit list)

- `CLAUDE.md` — read (loaded in session context; policy sections confirmed)
- `.claude/rules/general-code-change.md` — read (loaded in session context)
- `.claude/rules/general-unit-test.md` — read (loaded in session context)
- `.claude/rules/csharp.md` — read in full
- `docs/features/active/utilitiescs-nullable-helperclasses/spec.md` — read in full (8 Definition-of-Done checkboxes identified)
- `docs/features/active/utilitiescs-nullable-helperclasses/user-story.md` — read in full (6 Acceptance-Criteria checkboxes identified)
- `docs/features/active/utilitiescs-nullable-helperclasses/issue.md` — read in full
- `docs/features/active/utilitiescs-nullable-helperclasses/research/research-findings.2026-07-18T21-45.md` — read in full

## Key Compliance Notes Captured

- Work Mode: full-feature. AC sources are BOTH `spec.md` `## Definition of Done` AND `user-story.md` `## Acceptance Criteria`; each tracked independently.
- CRITICAL toolchain deviation for this child: the nullable/type-check stage uses the pragma-only build `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and MUST NOT add `/p:Nullable=enable`. This deviation is documented in spec.md Constraints & Risks item (1) and research section 7; it MUST NOT be resolved by editing `.claude/rules/*`.
- Scope: 43 `.cs` files under `UtilitiesCS/HelperClasses/` (recursive), per-file `#nullable enable` opt-in, annotation/null-safety only. No project- or solution-level `<Nullable>` element.
- Out of scope: `UtilitiesCS/Interfaces/IHelperClasses/` (FileSystem interfaces stay oblivious); no files outside `UtilitiesCS/HelperClasses/`.
- Maintainer flags to record (not fix): FileSystem adapter root-boundary `!` + latent root-throws behavior; `DvgForm.Designer.cs` left non-opted-in; `PrettyPrint.cs` (677 lines) 500-line pre-existing breach; `FilePathHelper.cs` (494 lines) near-limit.
- Toolchain tools resolve via pwsh: msbuild 18.8.2 (VS18, .NET Framework), csharpier 1.3.0 (global; v1 subcommand syntax `csharpier check .` / `csharpier format .`).

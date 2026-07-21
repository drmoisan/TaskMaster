# Phase 0 — Instructions Read (P0-T1)

- Timestamp: 2026-07-19T08-48
- Feature: utilitiescs-nullable-newtonsofthelpers (issue #367)
- Task: [P0-T1]

## Policy Order

Policy compliance reading order applied (per `policy-compliance-order` skill and `CLAUDE.md`):

1. `CLAUDE.md` (standing instructions, all sections)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (C#-specific toolchain and coding standards)

## Files Read (explicit list)

Policy files:
- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`

Requirements sources:
- `docs/features/active/2026-07-18-utilitiescs-nullable-newtonsofthelpers-367/spec.md` (Definition of Done — AC source)
- `docs/features/active/2026-07-18-utilitiescs-nullable-newtonsofthelpers-367/user-story.md` (Acceptance Criteria — AC source)
- `docs/features/active/2026-07-18-utilitiescs-nullable-newtonsofthelpers-367/issue.md`
- `docs/features/active/2026-07-18-utilitiescs-nullable-newtonsofthelpers-367/research/research-findings.2026-07-18T22-05.md`
- `docs/features/epics/utilitiescs-nullable-remediation/epic.md` (read for cluster dependency context; `NewtonsoftHelpers` has `depends_on: []`)

Plan under execution:
- `docs/features/active/2026-07-18-utilitiescs-nullable-newtonsofthelpers-367/plan.2026-07-18T22-04.md`

## Key Compliance Constraints Confirmed

- Per-file `#nullable enable` opt-in across `UtilitiesCS/NewtonsoftHelpers/` (recursive, incl. `MonoExtension/` and `SDIL Reader/`); each opted-in file must reach zero CS86xx under the pragma.
- NO project-level or solution-level `<Nullable>` element; `UtilitiesCS.csproj` keeps none.
- Annotation and null-safety only: `?`, null guards, justified `!`, null-flow corrections. No behavior change, no refactor, no API redesign, no feature work, no file splits.
- Type-check / nullable verification uses the pragma-only build and MUST NOT add `/p:Nullable=enable`:
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  This is a deliberate documented per-child deviation from `.claude/rules/csharp.md`; NOT resolved by editing `.claude/rules/*`.
- Toolchain order (restart on any failure/file change): csharpier → analyzer/codestyle build → pragma-only nullable build → vstest with coverage.

# Phase 0 — Policy Documents Read (P0-T1)

Timestamp: 2026-09-03T01-11
Task: [P0-T1]
Command: Read tool over each policy document in the item worktree; heading counts measured with a `^#{1,6} ` regex count over each file.
EXIT_CODE: 0

Policy Order: CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, .claude/rules/csharp.md

## Documents read, in the mandatory order

1. `CLAUDE.md` — read in full (448 lines). Heading count: 48.
2. `.claude/rules/general-code-change.md` — read in full (81 lines). Heading count: 11.
3. `.claude/rules/general-unit-test.md` — read in full (106 lines). Heading count: 11.
4. `.claude/rules/csharp.md` — read in full (97 lines). Heading count: 12.

## Modification status

No policy document was modified. All four were opened read-only. The hard constraint from
`policy-compliance-order` — do not modify anything under `.claude/rules/` — is observed for the
whole of this plan.

## Points carried into execution

- CLAUDE.md was changed on this branch by the base reconciliation that merged issue #564. Its
  toolchain citations now name `.github/workflows/_format-check.yml`, `_build-analyzers.yml` and
  `_build-nullable.yml` rather than a single `ci.yml`. The four toolchain commands themselves are
  unchanged, so every command this plan pins remains the approved command.
- Both msbuild gates in Phase 4 must use `/t:Rebuild`. A warm `/t:Build` exits 0 with `CoreCompile`
  skipped on every project, so the gate cannot fail.
- `/p:Nullable=enable` must not be added to the nullable gate. Nullable enforcement in this
  repository is per-file opt-in via `#nullable enable`.
- CSharpier is invoked only through `dotnet tool run` so the manifest-pinned version is used.
- The 500-line ceiling in `general-code-change.md` applies to production code, test code and
  reusable scripts. Markdown documentation is exempt; the CustomUI XML resource document is not
  measured against the ceiling by this plan (P4-T2 records that carve-out).
- Test policy: MSTest, Moq, FluentAssertions; no sleeps, no polling, no wall-clock reads, no
  temporary files, no message pump.

Output Summary: All four policy documents read in the mandatory order and none modified. Heading
counts: CLAUDE.md 48, general-code-change.md 11, general-unit-test.md 11, csharp.md 12.

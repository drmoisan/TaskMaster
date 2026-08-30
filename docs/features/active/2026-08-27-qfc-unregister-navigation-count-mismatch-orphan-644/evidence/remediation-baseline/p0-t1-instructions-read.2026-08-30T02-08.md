# [P0-T1] — Policy and cycle-document reads

- Timestamp: 2026-08-30T02-08
- Task: `[P0-T1]`
- Plan: `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/remediation-plan.2026-08-30T02-08.md`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Head at cycle entry: `85a1939f92f64ebada4e71d19cc095dc2e8e8a26`
- Command: no shell command was executed for this task. All entries below were read
  with the file-read tool against the worktree checkout of the branch named above.
- EXIT_CODE: 0

## Policy Order

The four rule files were read in the `policy-compliance-order` sequence, in this order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

## Complete list of documents read

Policy documents (in the order above):

1. `CLAUDE.md` — 448 lines. Standing instructions; embeds the General Code Change
   Policy, the General Unit Test Policy, the C# Code Change Policy, the C# Unit Test
   Policy, the Tone Policy, and the four-step C# toolchain order.
2. `.claude/rules/general-code-change.md` — 81 lines. Cross-language code change
   policy: design principles, toolchain loop with restart-from-step-1 rule, 500-line
   file limit, error handling, naming, dependencies, I/O boundaries.
3. `.claude/rules/general-unit-test.md` — 106 lines. Cross-language unit test policy:
   five core principles, coverage requirements, coverage exclusion policy, scenario
   completeness, Arrange-Act-Assert, external dependencies, test file location,
   determinism infrastructure.
4. `.claude/rules/csharp.md` — 97 lines. C#-specific toolchain (CSharpier via
   `dotnet tool run`, msbuild `/t:Rebuild` analyzer and nullable gates, vstest),
   coding standards, DI seams, analyzer stack, prohibited behaviors.

Cycle scope and finding documents:

5. `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/remediation-inputs.2026-08-30T02-08.md`
   — 98 lines, read in full. Cycle-2 scope source: item 1 (CR-6, line 179), item 2
   (CR-2, line 145), constraints, explicit out-of-scope list including AC-16, and the
   stated exit condition.
6. `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/code-review.2026-08-30T01-46.md`
   — the CR-6 section (lines 105-132) and the CR-2 section (lines 134-157) read in
   full. CR-6's suggested correction is adopted verbatim by `[P1-T1]`. CR-2's
   suggested correction is diverged from by `[P1-T2]` per the plan's stated rationale.

Contract and convention skills:

7. `.claude/skills/atomic-plan-contract/SKILL.md` — 242 lines. Canonical plan format,
   Phase 0 requirements, evidence-path clause, final QA loop, No-SKIPPED rule,
   wrap-tolerant assertion authoring, preflight validation protocol.
8. `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` — 177 lines.
   Non-overridable evidence path authority, ISO-8601 `yyyy-MM-ddTHH-mm` format,
   canonical evidence sub-paths, machine-checkable artifact schema, negative evidence
   claim requirements.

## Evidence location confirmation

Every artifact this plan produces is written under
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/`
in a subfolder named for the artifact kind (`remediation-baseline/`, `other/`,
`qa-gates/`), matching the canonical `<FEATURE>/evidence/<kind>/` scheme. No
delegation instruction in this cycle named a non-canonical evidence path, so there is
no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record to write.

## Output Summary

All eight documents read. The four rule files were read in the mandated
`policy-compliance-order` sequence. No conflicting instruction was encountered between
the policy documents and the approved plan: the plan's Phase 2 reproduces the four
C# toolchain commands character-for-character as `CLAUDE.md` and `.claude/rules/csharp.md`
state them, including `/t:Rebuild` and the absence of `/p:Nullable=enable`.

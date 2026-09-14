# Phase 0 — Policy Documents Read

Timestamp: 2026-09-13T04-58
Task: [P0-T1]
Issue: #872
Work Mode: minor-audit

Policy Order: CLAUDE.md (repository-root standing instructions), then the rules files under the
dot-claude rules directory in this order: general-code-change.md, general-unit-test.md,
quality-tiers.md, csharp.md, tonality.md, plan-acceptance-gates.md. This is the order the P0-T1 task
text states. It is consistent with the policy-compliance-order baseline, which places the standing
instructions file first, the cross-language code-change policy second, the cross-language unit-test
policy third, and the language- or domain-specific rules fourth.

## Files Read, In The Order Above

- CLAUDE.md — repository-root standing instructions. Embeds the General Code Change Policy, the
  General Unit Test Policy, the C# Code Change Policy, the C# Unit Test Policy, the Tone Policy and
  the four-stage C# toolchain order.
- .claude/rules/general-code-change.md — cross-language code change policy: design principles, module
  rigor tier pointer, the mandatory seven-stage toolchain loop, the 500-line file size limit, error
  handling and logging, naming, public API compatibility, dependencies and I/O boundaries.
- .claude/rules/general-unit-test.md — cross-language unit test policy: the five core principles,
  coverage requirements and the coverage exclusion policy, scenario completeness, Arrange-Act-Assert
  structure, external dependency prohibitions, test file location, test categories and determinism
  infrastructure.
- .claude/rules/quality-tiers.md — the T1 through T4 module rigor tier system, the source of truth in
  quality-tiers.yml at the repository root, and the uniform-versus-tier-dependent gate matrix.
- .claude/rules/csharp.md — C#-specific toolchain and coding standards: CSharpier formatting, the
  analyzer and nullable MSBuild commands with the /t:Rebuild requirement, the prohibition on
  /p:Nullable=enable, MSTest plus Moq plus FluentAssertions, deterministic test rules, DI seams, the
  five-package analyzer stack and the prohibited behaviours list.
- .claude/rules/tonality.md — required professional tone, the prohibitions on humor and hyperbole, the
  tight restriction on metaphor, evidence-first wording and the handling of difficult messages.
- .claude/rules/plan-acceptance-gates.md — acceptance-gate rules G1 through G9 applied to the shell
  commands an atomic plan states as acceptance conditions, their shipped severities, the write-mode
  register, the checkable-literal definition and the placeholder guard, and the deliberately uncovered
  sub-classes including the task-ordering class.

## Read Method

CLAUDE.md, .claude/rules/csharp.md and .claude/rules/plan-acceptance-gates.md were read directly from
this worktree. For .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md,
.claude/rules/quality-tiers.md and .claude/rules/tonality.md, the copies auto-loaded into the
execution context were confirmed byte-identical to this worktree's copies by four
`git diff --no-index --stat` comparisons, each of which produced no output. The content read is
therefore this worktree's content for all seven files.

## Governing Constraints Carried Forward Into Phase 0

- Phase 0 runs no formatter and edits no source. A baseline captured after a write-mode formatter has
  repaired pre-existing drift is not a baseline.
- The C# toolchain order is format, then analyzer rebuild, then nullable rebuild, then test. Any
  failure or file rewrite restarts the loop from the first stage. Phase 0 captures the baseline of
  these stages rather than running the loop.
- `/t:Rebuild` is mandatory for both MSBuild gates; a warm `/t:Build` can skip CoreCompile and exit 0
  without running analyzers.
- `/p:Nullable=enable` is never supplied.
- The acceptance-criteria source for this minor-audit item is the explicit `## Acceptance Criteria`
  section of issue.md in this feature folder and nothing else. No spec.md and no user-story.md exists
  in this folder, and their absence is correct by design.
- All evidence resolves under this feature folder's evidence directory in a canonical sub-folder. No
  artifacts path is used for evidence.

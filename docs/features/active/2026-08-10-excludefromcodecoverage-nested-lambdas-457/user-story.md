# 2026-08-10-excludefromcodecoverage-nested-lambdas-457 (User Story)

- **Issue:** #457
- **Epic:** `build-ci-coverage-gate-fidelity` (wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-10T14-30
- **Status:** Approved for planning
- **Version:** 1.0

## Note on why this file exists, and what it is not

The work mode recorded in `issue.md` is `full-bug`. Under
`.claude/skills/acceptance-criteria-tracking/SKILL.md`, `full-bug` resolves the authoritative
acceptance-criteria source to `spec.md` **only**, and a `user-story.md` is normally not produced for a bug.

This file exists solely because the epic preparation deliverables list for
`docs/features/epics/build-ci-coverage-gate-fidelity/` names `user-story.md` explicitly for each child
feature.

**This file carries no acceptance criteria.** It contains no checkbox items and must not be parsed as an AC
source. `spec.md` in this same folder is the sole acceptance-criteria source for this feature. Executors and
reviewers performing AC check-off must read `spec.md` and must not read this file for that purpose.

## Story

**As** a contributor or agent who adopts the repository's thin exempt production forwarder seam pattern —
splitting a host-bound class into `[ExcludeFromCodeCoverage]` production forwarders and testable pure logic,
exactly as the coverage policy asks —

**I want** the `[ExcludeFromCodeCoverage]` attribute on a forwarder to exclude the lambdas declared inside
that forwarder, not just the forwarder's own statements,

**so that** the coverage figure I am measured against reflects work I can actually do, and the file's
reported percentage rises when I write tests rather than stopping at an invisible ceiling.

## Current experience

The seam pattern is the repository's recommended response to code that cannot execute in a unit-test host.
A contributor applies it, writes tests against the extracted logic, and watches the file's line coverage
climb toward the repository floor. It then stops climbing, short of the floor, for no reason visible in the
report or in the source.

The cause is not visible at the point of use. The C# compiler hoists lambdas declared inside a member into a
separate compiler-generated closure type (`<>c`, `<>c__DisplayClass<N>_<M>`), and that synthesized type does
not inherit the member's `[ExcludeFromCodeCoverage]` attribute. The collector correctly suppresses the
attributed member — its `<method>` element is absent from the report entirely — while the lambda bodies
survive under the closure type with `hits="0"`. Because the exempt member cannot run in a test host, those
lambda lines can never be covered, and because they remain in the denominator, they permanently depress the
file's rate.

Nothing fails, nothing warns, and nothing in the Cobertura report labels the affected lines as belonging to
an exempt member. The contributor's only signal is that additional tests stop moving the number.

## Concrete cost

`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` currently measures **90.7% line coverage** and cannot
exceed approximately **91.5%** — `(258 - 22) / 258` — regardless of how many tests are written. The 22 lines
are the lambda bodies inside its `[ExcludeFromCodeCoverage]` members, verified in committed coverage
evidence as belonging to the closure types `<>c__DisplayClass41_0`, `46_0` and `46_1`, all declared inside
the attributed members `BeginProductionNavigation` and `BindProductionNavigation`.

Any gate, audit, or acceptance criterion that assumes the remaining 9.3% is closable by testing is working
from a false premise.

## Blast radius

The repository carries 263 `[ExcludeFromCodeCoverage]` occurrences across 110 `.cs` files, and
`TaskVisualization/FlagTasks.cs` is a second independently verified instance of the same defect.

Epic #136 requires every testable file to reach the repository line-coverage floor, and several of its
children plan to adopt exactly this seam pattern. Each of those children would inherit an unannounced
ceiling — and would then have to argue, file by file, for an exception to a floor that the measurement
itself made unreachable. Correcting the measurement removes that recurring argument rather than settling it
one file at a time.

## What "done" looks like from the beneficiary's perspective

- A contributor applying the seam pattern sees the file's reported coverage respond to the tests they write,
  with no residual gap attributable to lambdas inside exempt members.
- A reviewer reading a per-file coverage figure can treat the remainder as genuinely closable by testing.
- Where a gap is **not** closable — the named residuals recorded in `spec.md` — it is documented and carries a
  follow-up issue, so the contributor learns about it from the record rather than by discovering that the
  number will not move.

The verifiable conditions for all of the above are recorded as acceptance criteria in `spec.md`, not here.

## Out of scope for this story

- Deciding what the coverage thresholds should be once the figures are corrected. That is issue #494, epic
  wave 2.
- Re-baselining the unmerged branches of epic #136 whose committed coverage evidence was produced by the
  pre-correction pipeline.
- Changing any `[ExcludeFromCodeCoverage]` attribute in C# source.

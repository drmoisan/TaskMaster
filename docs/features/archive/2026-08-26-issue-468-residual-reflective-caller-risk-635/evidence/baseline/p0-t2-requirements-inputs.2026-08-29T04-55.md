# Requirements Inputs (P0-T2)

- **Issue:** #635
- **Plan task:** [P0-T2]

Timestamp: 2026-08-29T06-23

## Documents read in full

All paths are repository-relative to the root of this checkout.

1. `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md`
2. `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/issue.md`
3. `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/research/reflective-caller-closure.md`

## Resolved requirements metadata

AC_COUNT: 15
WORK_MODE: full-bug
AC_SOURCE: spec.md

The work-mode marker is the line `- Work Mode: full-bug` in the feature folder's `issue.md`. Under the
`acceptance-criteria-tracking` AC Source Resolution table, `full-bug` resolves the acceptance-criteria
source to `spec.md` only, and `user-story.md` is legitimately absent from this feature folder rather
than missing. The specification's `## Acceptance Criteria` section carries fifteen checkbox items,
AC-1 through AC-15, each written in the form `- [ ] **AC-n** — ...`.

## The thirteen identifiers, in the plan's preamble order

1. WireUpKeyboardHandler
2. AnyOpenDropDownsAsync
3. LoadGroups_02cAsync
4. LoadGroups_02bAsync
5. LoadGroup_03bAsync
6. LoadConversationsAndFoldersAsync
7. LoadItemGroup
8. LoadSequentialAsync
9. LoadGroupSequential
10. CacheTlpForMove
11. SwapTlp
12. CaptureTlpTemplate
13. _templateTlp

IDENTIFIER_COUNT: 13

This is the order in which the specification's Context table lists them. Items 1 through 12 are
methods; item 13 is a private field. The order is not the order in which the removal commit's diff
declares them; [P0-T4] records the commit-level derivation with the removed-line text for each.

## Requirements facts carried forward

- The item modifies no production or test source file. Its entire change set is Markdown under the
  feature folder.
- The specification's satisfiability constraint prohibits any repository-wide zero-hit acceptance
  condition. Acceptance is expressed as a total classification with the category "genuine name-based
  caller" empty.
- `full-bug` normally requires fail-before regression evidence. The specification records that a
  failing run is structurally impossible here and mandates a fail-before exception dossier under
  `evidence/regression-testing/` in its place; [P3-T2] produces it.
- The research document records that no command in its section 5 was executed, because no shell was
  available to that session. Every measurement in this item is therefore produced by the executor and
  is not treated as pre-verified.

Output Summary: The three requirements inputs were read in full. Work mode resolves to `full-bug`, the
acceptance-criteria source resolves to `spec.md` alone, and the specification carries fifteen
acceptance criteria numbered AC-1 through AC-15. The thirteen-identifier search set is recorded in the
plan's preamble order, twelve methods and one private field.

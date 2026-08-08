# conflicting-coverage-thresholds-across-policy-docs (Issue #494)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/conflicting-coverage-thresholds-across-policy-docs/ (Issue #494)

- Work Mode: full-bug

- Issue: #494
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/494
- Last Updated: 2026-08-08
## Problem / Why

Two authoritative, always-loaded policy documents state different coverage thresholds, and neither defers to the other.

| Source | Line coverage | Branch coverage | New/changed code |
|---|---|---|---|
| `CLAUDE.md` § UT2 (Coverage and Scenarios) | >= 80% repo-wide | not stated | >= 90% for new modules/classes/methods |
| `.claude/rules/general-unit-test.md` (Coverage Requirements) | >= 85% all tiers | >= 75% all tiers | not stated |
| `.claude/rules/quality-tiers.md` (gate matrix) | >= 85% | >= 75% | not stated |

`.claude/rules/general-unit-test.md` additionally states "Tier-specific lower coverage thresholds are not used in this repository," which reads as a deliberate, considered position — but it does not reconcile against CLAUDE.md's 80/90.

CLAUDE.md's own Policy Compliance Order places itself first and the rules files second, which would make 80/90 authoritative. But CLAUDE.md § UT2 also carries the COM/VSTO/WinForms "testable denominator" exemption, which the rules files do not mention, and `.claude/rules/general-unit-test.md` carries a strict "no production file may be excluded from coverage measurement" clause that appears to contradict that exemption. So the precedence rule does not cleanly resolve which numbers apply, and the two documents disagree on exclusion policy as well as on thresholds.

## Impact

CLAUDE.md instructs agents: "If you encounter **any** conflicting instructions, halt and notify the user." A conflict embedded in the policy documents themselves therefore puts every agent in an unresolvable position on any change that touches coverage — which is nearly every code change.

In practice agents have been working around it rather than halting, which is the worse outcome: the resolution is improvised per-run instead of decided once.

Observed occurrences:

- **Issue #424** established an in-repo precedent: treat no-regression against a captured baseline plus a 90% changed-line bar as blocking, and report raw repo-wide figures as non-blocking.
- **Issue #230 / PR #479** hit the same conflict during atomic planning and applied the #424 precedent by analogy (plan Decisions D5/D12).

A precedent carried between runs by agent memory and prior-plan archaeology is not a policy. It is invisible to reviewers, unenforced by tooling, and will drift.

## Discovery Context

Surfaced during atomic planning for issue #230 (PR #479, WinForms message-pump test seam), where the planner had to choose between the two threshold sets to author the coverage gate tasks.

## Proposed Behavior

Decide the authoritative thresholds once and make every document agree:

1. Choose the governing line, branch, and new/changed-code thresholds.
2. Decide whether the COM/VSTO/WinForms testable-denominator exemption in CLAUDE.md § UT2 survives, and reconcile it against the "no production file may be excluded from coverage measurement" clause in `.claude/rules/general-unit-test.md`. These two clauses cannot both stand as written.
3. Update `CLAUDE.md`, `.claude/rules/general-unit-test.md`, and `.claude/rules/quality-tiers.md` so the numbers and the exclusion policy match.
4. State explicitly which document is authoritative for coverage, so a future divergence is resolvable by rule rather than by precedent.
5. Encode the decision in tooling so the gate enforces the agreed numbers, rather than leaving enforcement to each agent's reading.

## Acceptance Criteria (early draft)

- [ ] A single set of coverage thresholds appears in all three policy documents, with no numeric disagreement.
- [ ] The exclusion/exemption policy is stated once and does not contradict itself across documents.
- [ ] The documents name which source is authoritative for coverage policy.
- [ ] Tooling enforces the agreed thresholds, and a deliberately-introduced coverage regression fails the gate.
- [ ] The #424 / #230 improvised precedent is either ratified as the written rule or explicitly superseded.

## Constraints & Risks

- Raising the effective floor may fail existing projects; measure current per-project coverage before choosing the number.
- The testable-denominator exemption exists because COM/VSTO/WinForms code genuinely cannot be unit-tested; removing it without a replacement mechanism would make the floor unreachable rather than more rigorous.
- CLAUDE.md § UT2 records that the exemption "must be ratified by the project maintainer" — this change needs the maintainer's decision, not an agent's.

## Test Conditions to Consider

- [ ] Introduce a coverage regression and confirm the gate fails at the agreed threshold.
- [ ] Confirm an exempted COM/VSTO/WinForms file behaves as the reconciled policy specifies.

## Next Step

- [ ] Promote to GitHub issue
- [ ] Create active feature folder from the template

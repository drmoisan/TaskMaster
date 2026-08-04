---
name: coverage-gate-clr-invoked-private-members
description: Never plan a >=90% coverage gate on a private member the CLR invokes (AssemblyResolve handlers, etc.) — split the gate into newly-added vs changed sets per the AC's own wording
metadata:
  type: feedback
---

Do not write a per-member `>= 90%` coverage task that includes a `private static` member invoked only by the CLR (e.g. an `AppDomain.CurrentDomain.AssemblyResolve` handler). Split the coverage-delta task into two tables keyed to the AC's own wording:

- **Newly added members** — `>= 90%` required, with a rerun clause if any is below.
- **Changed pre-existing members** — no-regression on changed lines only; `>= 90%` not required.

Then name the unreachable member explicitly, record its measured percentage, state which pure helpers carry its extracted logic, and report a `COVERAGE_MEMBER_UNREACHABLE: <FullyQualified.Member>` signal instead of a rerun.

**Why:** #418 preflight pass 3 blocked on a `[P2-T8]` task demanding `>= 90%` for `ResolveByNameAndKey`. Driving its `Assembly.LoadFrom` probe branch from a unit test requires staging a real same-public-key assembly in a probe directory — which UT4 prohibits with zero approved exceptions, and which another task in the same plan independently forbade. The task was an unbounded loop with no exit clause, and it was stricter than its AC source: the AC applied `>= 90%` only to *newly added* members, and this was a *changed* member.

**How to apply:** When drafting a coverage-delta task, read the AC's exact scope words ("newly added" vs "changed") and do not widen them. For every member on the list, ask whether a unit test can reach it without filesystem staging, a live host process, or a real assembly bind. If it cannot, it belongs in the changed/no-regression set with a named exception — the fix is extracting its decision logic into pure helpers that are covered directly, not a coverage carve-out and not an unreachable gate. Related: [[project_deadcode_removal_vs_coverage_exclusion]].

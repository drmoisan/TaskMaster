---
name: excludefromcodecoverage-invisible-to-coverage-gates
description: A class-level [ExcludeFromCodeCoverage] emits NO Cobertura class element, so a per-file coverage gate demanding a hits row for that file is unsatisfiable — and static preflight cannot detect it
metadata:
  type: project
---

`dotnet-coverage` honours a class-level `[ExcludeFromCodeCoverage]` by emitting **no
`class` element at all** for that file. The file is not "0% covered" — it is absent from
the document. Any plan gate that demands "at least one `hits=` row for each of these N
filenames", or that classifies a changed line as *covered* or *uncovered*, is unsatisfiable
for such a file, because both outcomes presume an entry that does not exist.

Confirmed on issue #731 (2026-09-03): `QuickFiler/Controllers/QfcCollectionController.cs:21`
and `QuickFiler/Controllers/QfcDatamodel.cs:25` both carry it. Execution blocked at the
Phase 0 per-line baseline task after ten prior rounds of review.

**Check this before writing any per-file coverage gate.** Grep the target files for
`ExcludeFromCodeCoverage` at planning time. It costs one search and it is the difference
between a plan that runs and a plan that halts after the baseline.

**Three categories, not two.** A changed-line no-regression gate needs:
1. covered at baseline, covered after — fine;
2. uncovered at baseline, still uncovered — not a regression, list separately;
3. **file not instrumented at all** — no judgment of any kind is available; list separately
   and exclude from both counts, citing the attribute's file and line.

Most plans author only the first two and silently mis-handle the third.

**Watch the vacuity it can introduce.** Excluding uninstrumented files shrinks the gate's
population. Re-check that a non-zero regression count is still *reachable* afterwards; if
every changed line has been excluded, the gate now passes unconditionally and says nothing.
State the surviving scope honestly in the plan rather than letting a reader assume the gate
covers every file it names.

**Why no amount of preflight finds this.** Static review compares the plan against the
source tree. The attribute's effect on the emitted document is a property of the coverage
*run*, not of either text. This is a structural limit of static preflight — useful evidence
against the instinct that more review rounds always help.

**METHOD-level exclusion is leakier than class-level, and the leak is a lambda.** The
"absent from the document" behaviour above is reliable for a *class*-level attribute. A
*method*-level `[ExcludeFromCodeCoverage]` removes the member's own lines but **does not
reach lambdas the compiler lifts out of it**, and which of two lowerings you get decides
whether the exclusion holds:

- a lambda capturing only **locals** is lifted into a compiler-generated **display type**,
  which the coverage config drops — exclusion holds, nothing surfaces;
- a lambda capturing **`this`** is lifted into an ordinary **instance member of the same
  class**, named `<OuterMemberName>b__NN_N` — the attribute does not apply to it, and it
  surfaces at `hits=0`.

Verified on issue #736 (2026-09-04). A wrapper carrying the attribute had its own lines
(86, 87, 88, 92, 93) correctly removed, while its three `Func`/`Action` arguments at lines
89, 90, 91 appeared as `<ResolveValidatedArchiveRootPath>b__74_0/_1/_2` at zero hits. The
file measured 18/21 = 85.71% against a 90% new-file floor and execution halted at task 53
of 77. A sibling member in the same change, whose lambda captured a local, excluded cleanly
— so the two behaviours occurred in one commit and a plan that reasoned from the clean one
mispredicted the other.

**How to apply:** when a plan puts a per-file coverage floor on a file whose exclusion is
method-level, check whether any excluded member contains a lambda that touches instance
state. If it does, budget those lines into the denominator up front, or convert the lambdas
to `[ExcludeFromCodeCoverage]` private methods passed as method groups. Anchor any
assertion on the `<MemberName>b__` prefix — the trailing ordinal is compiler-assigned and
will churn.

Related: [[coverage-seam-workaround-for-claude-worktrees]] for getting coverage to run at
all from an agent worktree.

Standing policy tension, unresolved and worth knowing before feature-review: CLAUDE.md's
COM/VSTO exemption explicitly sanctions `[ExcludeFromCodeCoverage]` in source, while
`.claude/rules/general-unit-test.md`'s Coverage Exclusion Policy says no production file may
be excluded and tells feature-review to treat a production-path exclusion as Blocking. Where
the attributes are pre-existing on main, they are not the current change's defect.

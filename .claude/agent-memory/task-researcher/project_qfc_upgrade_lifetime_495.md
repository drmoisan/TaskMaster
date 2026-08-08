---
name: qfc-upgrade-lifetime-495
description: "#495/epic #136 F12: Cobertura <class name> can be a SECONDARY type in the file (BreadcrumbUpgradeLease, not BreadcrumbCoordinatorUpgradeLifetime); ternary arms both report hits=1 so hits can't pick the untaken side; pick the discriminating call ordering for latch tests"
metadata:
  type: project
---

Three reusable findings from per-file coverage research on
`QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` (F12 of epic #136, issue #495),
verified against `.../424/evidence/qa-gates/coverage-final.cobertura.xml`.

**1. A Cobertura `<class name>` can name a SECONDARY type declared in the file.**
That file declares two types; the single merged `<class>` element is named
`QuickFiler.Viewers.BreadcrumbUpgradeLease` while its class-level `<lines>` block spans the whole
file. A harness keyed on class name would report the file's principal type as absent/not-measured.
It is also a second specimen of #478: the `<methods>` subtree held only the 4 secondary-type methods
while the class-level `<lines>` union was correct.

**Why:** confirms the epic's binding "key on `filename`, not `<class name>`" directive with a
concrete positive control, and shows #441/#478 recur per-file.

**How to apply:** when recomputing a per-file baseline, grep the coverage XML by `filename=` and
never assume the `<class name>` matches the file name. Cross-check that the class-level `<lines>`
first/last line numbers span the whole source file.

**2. `hits` cannot disambiguate a multi-line ternary's untaken arm.** A three-line
`cond ? A : B` initializer reported `condition-coverage="50% (1/2)"` on the condition line while
BOTH arm lines reported `hits="1"`. Fall back to an exhaustive call-site census (production + every
test call site) — that is stronger evidence than an ambiguous hit map, and say so explicitly rather
than claiming hits-derived certainty.

**3. For an idempotency/latch branch, choose the call ordering that DISCRIMINATES.** Two orderings
both reached the uncovered early-return, but only one (double-`Abandon`, where the token source is
already disposed on re-entry) fails if the latch is removed; the other (`Invalidate` then `Abandon`)
would pass either way and is therefore a coverage artefact, not a contract test. Trace the state
machine to find which ordering makes the guard observable before writing the sketch.

See also [[qfc-breadcrumb-dropdown-f13-455]], [[cobertura-perfile-attribution-contract]],
[[quickfiler-percoverage-epic-136]], [[qfc-perfile-coverage-viewerqueue-434]].

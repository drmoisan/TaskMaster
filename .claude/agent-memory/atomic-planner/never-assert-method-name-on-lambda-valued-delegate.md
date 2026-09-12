---
name: never-assert-method-name-on-lambda-valued-delegate
description: Before writing a ".Method.Name identity only" clause into a reset/restore test task, read each default's initializer — lambda-valued defaults have compiler-generated Method.Name and need reference (in)equality instead
metadata:
  type: feedback
---

When a plan task asserts that a `Reset*ForTesting()` call restored a set of delegate statics, the
standard safe idiom is `.Method.Name` identity (it proves restoration without invoking a dangerous
host-bound default). That idiom only works for defaults assigned a **named method group**. A default
assigned a **lambda** compiles to a `<...>b__N_M` display-class member whose name is a compiler
implementation detail. Assert `Should().NotBeSameAs(sentinel)` reference inequality for those.

**Why:** #437 preflight defect B5. The plan wrote "assert via `.Method.Name` identity only ... and the
same pattern for the remaining twelve" over all 16 `Production*` statics in
`EfcHomeControllerDependencyFactories.cs`, but three of them are lambda-valued. The clause was both
inapplicable to those three and arithmetically wrong (16 − 4 named in the task − 3 lambdas = nine,
not twelve). Two errors in one bullet, caught only at preflight.

**How to apply:** whenever a task text contains a delegate-set count and a `.Method.Name` clause:
1. Open the production file and classify every default as named-method vs lambda. Do not infer the
   count from the set size named elsewhere in the plan.
2. State the named-method subset count and the lambda subset count explicitly, and give the lambda
   subset its own assertion strategy.
3. Re-derive any "the remaining N" arithmetic from the classification, not from memory.

This generalizes the standing "never invoke the host-bound default, assert identity instead"
constraint. See also [[project_437_efc_home_controller_plan_seams]],
[[research-claims-as-acceptance-clauses]] (same failure family: an unverified literal written into
acceptance text).

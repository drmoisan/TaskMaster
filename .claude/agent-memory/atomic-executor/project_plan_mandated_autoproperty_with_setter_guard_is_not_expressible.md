---
name: plan-mandated-autoproperty-with-setter-guard-is-not-expressible
description: A plan task that says "auto-property ... with a setter guard that throws ArgumentNullException" is not expressible in C#; implement as backing field plus explicit property and say so
metadata:
  type: project
---

A C# seam task worded as "an `internal` **auto-property** named `X` ... initialized to `<expr>`,
with a setter guard that throws `ArgumentNullException` on null" cannot be implemented literally.
C# permits no accessor body on an auto-property, so the initializer form and the guard are mutually
exclusive. Implement it as a private backing field carrying the initializer plus an explicit
property whose `set` accessor holds the guard, and record the substitution.

**Why:** observed on issue 871 Phase 3 (seams S3 `ItemViewerFactory` and S6 `BackgroundTlpFactory`
in `QuickFiler/Controllers/QfcQueue.Tlp.cs`). The plan's Phase 3 preamble distinguishes only *lazy
`??=` getter* from *plain initializer form*, because the distinction it cared about was whether the
default can reference the instance. "Auto-property" was shorthand for "initializer at the
declaration", not a mandate on the declaration syntax. A separate later task (P4-T4 seam-contract
tests) asserted the guard for all six seams, so dropping the guard to keep the literal auto-property
would have made that task unsatisfiable.

**How to apply:** when a task names both an initializer and a setter guard, take the guard as
load-bearing (a later test usually asserts it) and the word "auto-property" as descriptive. Check
the task's acceptance text before choosing: if it reads "exactly one declaration of `X`", the
backing field must use a different casing (`_x`) so a case-sensitive search still returns one. If it
reads "**its** initializer contains `<literal>`", note in the artifact that the initializer sits on
the backing field of that declaration. Do not stop to request a plan revision mid-execution; this is
a mechanically necessary micro-action, not a new outcome.

Related: [[project_preflight_recurring_csharp_plan_defect_classes]],
[[project_preflight_csc_probe_for_mandated_csharp_shapes]].

---
name: async-state-machine-emits-no-method-element
description: dotnet-coverage merges an async method's state-machine lines into the parent class's class-level <lines> list and emits NO named <method> element, so a plan's per-method coverage aggregation returns an empty union
metadata:
  type: project
---

In this repo's Cobertura output, an `async` method produces **no `<method>` element at all** and **no separate state-machine `<class>` element**. Its lines are merged into the parent class's class-level `<lines>` list. A per-method aggregation written as "the union of `<method>` elements whose name is or contains `<MethodName>`" therefore returns an **empty set** — 0 covered of 0 valid, which is numeric but vacuous.

Two further traps in the same document:

- **`.//line` under a `<class>` double-counts.** The `<method>`-level `<line>` entries are a subset of the class-level `<lines>` list. Use `lines/line` (direct children) for a per-file figure. On `FileIO2.cs` the wrong idiom gave 189/223 and the right one 106/126.
- **A non-async method DOES get a `<method>` element.** After a fix converted a public overload from `async Task` to a plain `Task<bool>` forwarder, the method-element union went from 0 to 1 — so the same derivation silently changes shape across the change.

**Why:** the plan for #647 anticipated the *separate state-machine class* shape and wrote its per-method rule against it. The observed shape was merged-into-parent, so the stated rule was unsatisfiable and the AC's ">= 0.90 changed-method line rate" would have been unevaluable.

**How to apply:** when a plan defines a per-method coverage aggregation over `<method>` elements, measure the union *before* trusting it. If it is empty, substitute a span-based derivation — scan the source for the declaration, brace-match forward to the closing brace or terminating semicolon, and take the class-level `<line>` entries whose `number` falls in that span. Fix the substitute derivation in the baseline artifact and apply it identically at post-change, so both ends are one measurement. Record the substitution explicitly as a departure rather than reporting 0/0.

Related: [[project_coverage_delta_reproduce_baseline_counting_method]], [[project_koverage_cobertura_postprocessing_shape]], [[project_exempt_forward_extraction_leaves_call_site_uncovered]]

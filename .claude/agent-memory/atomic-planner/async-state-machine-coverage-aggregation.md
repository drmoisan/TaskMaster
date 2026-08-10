---
name: async-state-machine-coverage-aggregation
description: An async method's body lands in a separate Cobertura <class> element (<Method>d__N); a >=90% changed-class gate must aggregate by filename across compiler-generated nested types
metadata:
  type: project
---

When a plan gates changed-class line coverage at `>= 90%`, do not instruct the executor to read the named `<class>` element. An `async` method compiles into a nested state machine (`Type/<Method>d__N`) and lambdas into display classes (`<>c*`), each of which appears as its **own `<class>` element** in Cobertura output.

Reading the named element alone leaves only constructors and field initializers in the denominator. On `WpfDispatcherYield` (#508) that was ~6 coverable lines with 1 uncovered, i.e. ~83% — a gate failure for a pure **measurement** reason while the real aggregated figure passed.

Correct instruction: aggregate every `<class>` element whose `filename` attribute equals the changed source file path, then derive the line rate from the summed line/covered-line counts.

**Why:** #508 preflight pass 1 flagged the coverage gate as unreachable-as-measured. The same trap applies to any C# class in this repo whose changed member is `async` or uses lambdas — which is most of them.

**How to apply:** Write the aggregation rule into the coverage task text itself, plus into the plan's coverage design-decision section, and name the specific lines expected to remain uncovered up front so a shortfall is distinguishable from a measurement error. Related: [[named-coverage-exception-verify-member-body]], [[csharp-coverage-gate-jacoco-format]].

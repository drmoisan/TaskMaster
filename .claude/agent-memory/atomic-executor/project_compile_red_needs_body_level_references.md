---
name: compile-red-needs-body-level-references
description: A compile-time [expect-fail] task that must name N missing types will only report the ones bound in the same phase; a missing type in a method SIGNATURE suppresses all body-level diagnostics
metadata:
  type: project
---

When an `[expect-fail]` task's acceptance criterion requires the fail-before diagnostics to name several not-yet-existing types, put every reference in a **method body**, never in a signature.

Roslyn binds declarations first. If a private helper is declared `private static EngineReadinessGate CreateGateOver(...)` and `EngineReadinessGate` does not exist, the compiler emits one `CS0246` for that signature and then **does not bind any method body**, so a second missing type referenced only inside bodies never surfaces. Measured on #503 (2026-08-08): first run produced 1 diagnostic naming only `EngineReadinessGate`; after changing the helper's return type to a resolvable `Func<IAppItemEngines>` and constructing both types inline in the test bodies, the same command produced 4 diagnostics naming both `EngineReadinessGate` and `EngineGatedCommandRunner`.

**Why:** the plan's binary outcome was "diagnostics must include CS0246 naming X and Y". Recording a partial diagnostic set as satisfying it would be a false PASS, and re-running the build does not help — the shape of the test file is what determines which errors are reachable.

**How to apply:** before running the `[expect-fail]` build, check that no not-yet-existing type appears in any `class`/method/field declaration in the new test file. Keep helper signatures built only from types that already compile. Record the restructure in the fail-before artifact so the edit between the two runs is auditable.

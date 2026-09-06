---
name: fluentassertions-beempty-names-only-first-item
description: FluentAssertions BeEmpty renders "found at least one item {first}" and names ONE element, so a gate demanding two type names in the failure text is unsatisfiable
metadata:
  type: project
---

`collection.Should().BeEmpty(because)` in FluentAssertions 8.10.0 renders a non-empty failure as
`but found at least one item {<single item>}`. It names one representative element, not the whole
collection. An acceptance condition that demands two or more distinct identifiers appear in that
failure text can never pass, however many elements the collection actually holds.

Observed on issue #729 P3-T4: the `SVGControl.Test` no-live-Form guard failed as intended against
the pre-deletion assembly, but its acceptance required the pasted failure text to contain both
`SVGControl.Test.Form1` and `SVGControl.Test.Form2`. Whole-log counts were 1 and 0. Both types were
genuinely compiled in — both `.cs` files were on the `csc` command line and a metadata string scan
of the built DLL returned 2 occurrences of each name. Which single item gets named is deterministic
when the assertion sorts first: Block E's `OrderBy(name, StringComparer.Ordinal)` guarantees
`Form1` wins and `Form2` can never appear.

**Why:** the failure is invisible to plan review. The plan's premise (two Form types in the
assembly) was correct, the guard was correct, and the red-before state was real; only the renderer's
message shape defeated the gate. This is the class the `atomic-plan-contract` covers with "observe a
command's success-case output before asserting over that output" — and here it is the *failure*-case
output that had to be observed first.

**How to apply:** during preflight, treat any acceptance that counts N>1 distinct tokens inside one
assertion-library failure message as suspect. Prefer asserting the run-level `FailedCount`, or have
the test itself emit the full list (e.g. project the collection into the `because` argument or a
`Console.WriteLine`), rather than reading the library's summary line. Related:
[[project_preflight_recurring_csharp_plan_defect_classes]],
[[project_compile_red_needs_body_level_references]].

---
name: csharp-pure-move-extraction-pattern
description: Planning a members-to-new-file extraction to relieve the 500-line limit — preserve the static-ctor install trigger, and route the testable member to an existing covered class so the new class is not read as a new module owing >=90%
metadata:
  type: project
---

When a plan extracts members out of a near-500-line C# file into a new file, four things must be fixed in the plan text or the executor will invent them.

1. **A moved static constructor changes the install trigger.** `static Foo()` that subscribes an `AppDomain.AssemblyResolve` handler runs when `Foo` is first touched. Moving it to a new type moves the trigger and silently disables the handler. Keep the original static ctor and reduce its body to `NewType.Install();`, with the `Interlocked.Exchange` guard and the `+=` subscription moved verbatim into `Install()`.
2. **A `private` helper used by the moved code widens to `internal` mechanically.** Private members are not accessible across types, so the move forces the accessibility change. That can *deliver* a separate "make it testable" review item for free — say so, rather than planning a second edit.
3. **A new class holding a ratified-unreachable member creates a coverage trap.** `CLAUDE.md` requires new modules/classes to reach `>= 90%`. If the new file contains a host-bound member with a `COVERAGE_MEMBER_UNREACHABLE` exception, the class aggregate lands well below 90% and a reaudit can read it as a new module. Two mitigations, use both: state in the plan and in the coverage-delta artifact that the class is a **relocation, not a new module**, and re-record the ratified exception under its new fully-qualified name; and route any *testable* moved member to an **existing** already-100% class instead of the new one.
4. **Define "pure move" checkably.** Enumerate the only permitted deltas (csharpier indentation, plus the specific type qualifications the move forces) and require every string literal, comment, and control-flow construct to be carried verbatim. Then keep every behavior change in a *later* task.

**Why:** #418 remediation cycle 1. `SVGControl/SvgRenderer.cs` sat at 497/500 and the next fix added a `catch` block, so the extraction had to run *first* rather than last as the reviewer suggested. Recognising 1-3 up front avoided disabling the AC-8 resolver, avoided a second accessibility edit, and avoided manufacturing a ~74% "new" class.

**How to apply:** read the moved region and its callers before writing the task; check for a static ctor, for `private` helpers called from the region, and for any member carrying a named coverage exception. Related: [[named-coverage-exception-verify-member-body]], [[literal-call-clauses-block-file-size-tightening]], [[project_legacy_csproj_explicit_compile_include]].

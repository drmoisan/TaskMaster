---
name: reflective-property-read-escapes-member-expression-grep
description: A blast-radius grep for `Type.Member` cannot find a consumer that reads the same member reflectively by name string; adding a throwing guard to a previously-silent getter broke 8 tests a repo-wide grep had cleared
metadata:
  type: project
---

A blast-radius analysis that enumerates consumers with `git grep -n "UiThread.Dispatcher\b"` misses
any call site that reads the member **reflectively by name**, e.g.
`typeof(UiThread).GetProperty("Dispatcher", ...)` then `.GetValue(null)`. The qualified member
expression never appears in that file, so the grep clears it.

**Why:** On #584 the fix added an `InvalidOperationException` guard to a getter that had previously
returned `null` silently. `spec.md` asserted the repo-wide grep was "the complete set of production
reads". It was not: `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` snapshots the property
reflectively in `[TestInitialize]`/`[TestCleanup]`, so `PropertyInfo.GetValue` began throwing and all
8 tests in the class failed with the opaque MSTest message `One or more errors occurred.` The plan
had run the complementary census for reflective reads of the private **field**
(`git grep -n -F '"_dispatcher"'`) and got it right — it simply never ran the same census for the
**property** name.

**How to apply:** Before changing a getter, setter, or method from silent/lenient to throwing, run
BOTH censuses: the member-expression grep AND a literal search for the member name as a string
(`git grep -n -F '"Dispatcher"'`), filtering out `<see cref="..."/>` doc hits. Also note that a class
comment asserting "these tests do not depend on X being initialized" is a statement about the OLD
contract and becomes false the moment the contract tightens — treat such comments as call sites, not
as reassurance. See [[compile_red_needs_body_level_references]] for the related "grep shape does not
match usage shape" failure.

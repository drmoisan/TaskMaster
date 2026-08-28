---
name: test-file-name-vs-partial-class-name
description: A *Tests.cs file split for the 500-line limit often declares a partial of a DIFFERENT type, so a plan's FullyQualifiedName~<FileName> filter matches zero tests and the gate is vacuous
metadata:
  type: project
---

In this repo, a test file split to stay under the 500-line limit frequently keeps its own descriptive
file name while declaring `public sealed partial class <OriginalName>Tests`. A vstest
`/TestCaseFilter:FullyQualifiedName~<FileNameWithoutExtension>` selects by TYPE, so it matches nothing
and vstest prints `No test matches the given testcase filter` with **EXIT_CODE 0**.

Confirmed instance (2026-08-26, #498 `P7-T6`):
`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` declares
`partial class BreadcrumbStateModelTests` and carries 12 `[TestMethod]` members. The plan's filter
`FullyQualifiedName~BreadcrumbStateModelSequenceTests` returned zero tests.

**Why:** exit 0 on a zero-match filter means a `failed 0` gate PASSES while proving nothing. A plan
author reading only file names will write such a filter, and the same mismatch also mis-classifies a
class as "not written by this plan" in ownership/exclusion clauses (here it flipped a `P8-T5` carve-out
from weaker to stricter, which was harmless, but the reverse would silently weaken a gate).

**How to apply:** before running any class-scoped filter a plan supplies, `grep -rln "partial class
<Type>"` and read the actual `class` declaration in the named file. Resolve the filter to the TYPE, run
it, then verify each `[TestMethod]` name declared in the named FILE appears in the TRX with
`outcome="Passed"` — that is what makes the gate non-vacuous. Record the file-vs-type discrepancy in the
artifact. Related: [[project_vstest_testcasefilter_or_operator_and_env_setup]].

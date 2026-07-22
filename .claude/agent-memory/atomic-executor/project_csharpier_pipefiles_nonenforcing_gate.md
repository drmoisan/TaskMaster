---
name: csharpier-pipefiles-nonenforcing-gate
description: csharpier pipe-files + re-hash-unmodified-file is a fake format gate; use `csharpier check`/`format`; #400 P5 tests balloon past 500 lines when genuinely formatted
metadata:
  type: project
---

`csharpier pipe-files` (fed an absolute-path stdin list) writes formatted output to STDOUT and never modifies the file on disk. A gate that then re-hashes the (unmodified) file and reports "stable/PASS" is trivially satisfied and enforces nothing.

**Why:** In issue #400 (QuickFiler folder-selector drop-down), ~20 prior P5 CSharpier evidence artifacts used exactly this pattern, so the committed P5 production/test files were never actually CSharpier-clean. When P5-T154 ran genuine `csharpier format` (CSharpier 1.3.0, no `.csharpierrc`, default width 100), 8 of 10 files changed and two coverage test files blew past the hard 500-line limit: `BreadcrumbDropDownOpenCoordinatorTests.cs` 395->514 and `BreadcrumbPopupBoundaryCoverageTests.cs` 479->562. The prior batches sized those files to <=480 using their UNFORMATTED line counts. This created an unsatisfiable conflict (CSharpier-clean AND <=500) that only a plan revision (split into partial-class pairs + new includes) can resolve — P5-T154..T160 were left blocked.

**How to apply:** For any TaskMaster CSharpier gate, verify with `csharpier check <files>` (exit 1 == not formatted) or `csharpier format <files>` (writes in place), never `pipe-files`+re-hash. When sizing new/edited .cs files against the 500-line limit, measure the count AFTER `csharpier format`, not the hand-written count. Reformatting can both expand (arg lists broken across lines) and shrink (blank-line removal) files. See also [[vs18-build-toolchain-paths]].

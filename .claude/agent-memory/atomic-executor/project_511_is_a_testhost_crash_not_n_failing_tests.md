---
name: 511-is-a-testhost-crash-not-n-failing-tests
description: Issue #511 is a load-driven test-host crash with "Total tests: Unknown", not a fixed set of failing tests; plans that pin a failing-name count cannot be evaluated
metadata:
  type: project
---

Issue #511 on the 9-assembly single-process coverage run is an **intermittent test-host crash**, not a
deterministic set of failing tests. The authoritative in-repo record is
`docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/research/2026-08-10T14-20-cobertura-arithmetic-research.md:738-752`
and `.../441/spec.md:569-579`:

- symptom: `The active test run was aborted. Reason: Test host process crashed` / `Test Run Aborted.  Total tests: Unknown`
- character: environmental, load-driven, concentrated in the `QuickFiler.Test` `WinFormsPumpHost`
  message-pump family — explicitly "**not a test failure**"
- `Total tests: Unknown` means **no verdict can be read from the run**
- documented recovery: loop the 9 assemblies through `vstest.console.exe <dll> /InIsolation`, which on
  #505 produced **6435 passed, 1 skipped, 0 failed**; that loop yields nine separate `.coverage` files
  that need a `dotnet-coverage merge` before a repository-wide Cobertura figure exists

**Why:** a plan asserted "#511 leaves two `*ThroughThePumpHost*` MSTest cases failing", made a
per-failing-name #511 determination a gate, and declared "any third failing name" a halt. There are five
`*ThroughThePumpHost*` test methods, the two names are enumerated nowhere, and the real failure mode
produces zero named failures plus an unreadable total.
**How to apply:** never gate on a pinned count of #511 failures. Gate on the failing-name **pattern**,
require an artifact-existence check before reading the Cobertura file, and give the abort case an
explicit branch (re-run, or the `/InIsolation` + merge recovery). Related:
[[project_winformspumphost_tests_load_flaky]], [[project_timedout_mstest_leaves_detached_runner]].

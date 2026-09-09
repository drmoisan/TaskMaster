---
name: pester5-helper-function-must-live-in-beforeall
description: A helper function defined at Pester 5 test-file scope is not resolvable from an It block; define it inside BeforeAll or every test using it fails CommandNotFoundException.
metadata:
  type: project
---

In Pester 5, a `function` defined at the top level of a `*.Tests.ps1` file is **not** resolvable from
inside an `It` block. The test file is evaluated during the discovery pass into a session state the
run pass does not share, so the run pass raises `CommandNotFoundException: The term '<name>' is not
recognized`. Define the helper inside the file's `BeforeAll` block instead; `It` blocks run in a
child scope of the containing block, so a function defined there resolves.

**Why:** cost me one wasted red run on issue #815. The plan required a private differential helper in
the new test file; placed at file scope, the one test that called it failed with
`CommandNotFoundException` while the other six passed, which reads like a production defect and is
not one. Moving the function body unchanged into `BeforeAll` turned `PASSED=6 FAILED=1` into
`PASSED=7 FAILED=0`.

**How to apply:** when a Pester 5 test file needs a helper function, put it in `BeforeAll` from the
start. The same rule does not apply to `$script:`-scoped *variables*, which do survive from
`BeforeAll` into `It` — that is the established idiom in `tests/scripts/vscode/`. If a plan's task
text says the file "contains" the helper without saying where, `BeforeAll` is the only placement that
works, and moving it there is a mechanically necessary micro-action rather than a plan deviation.
Record the move in the pass-after artifact if it happens after a fail-before run was captured, since
it changes the delivered test file. See [[compile-red-needs-body-level-references]] for the analogous
C# case where placement decides whether a test can observe what it claims to.

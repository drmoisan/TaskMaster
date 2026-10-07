---
name: poshqc-junit-and-branch-ci-sources-for-powershell-gates
description: When raw Pester is banned, run_poshqc_test's artifacts/pester/pester-junit.xml supplies per-file counts; ci.yml does not run on feature-branch pushes, so branch coverage needs workflow_dispatch or the PR run
metadata:
  type: reference
---

Two facts that made the PoshQC-only PowerShell gate ruling executable on 929 (2026-09-30):

1. `mcp__drm-copilot__run_poshqc_test` returns only `{ok, summary}`, but it writes
   `artifacts/pester/pester-junit.xml` (gitignored): root `testsuites tests/failures`, one `testsuite`
   per test file (name = absolute path) with `tests` and `failures`, plus failure messages. That is a
   PoshQC-produced source for per-file pass/fail counts and fail-before messages. Its sibling
   `powershell-coverage.xml` reads 0 covered (it instruments `.claude/hooks`) and is NOT a coverage source.
2. `.github/workflows/ci.yml` triggers on push to main/development, pull_request and workflow_dispatch
   only, so pushing a feature branch starts no CI. CI-sourced Pester coverage before the PR needs
   `gh workflow run ci.yml --ref <branch>` (concurrency cancels older runs on the same ref), or wait for
   the PR run. In the Actions log, Pester colours each summary segment: strip `ESC[...m` before matching
   `Tests Passed: N, Failed: N`. `gh run download` refuses to overwrite an existing `--dir`.

Also: a gh-bearing pwsh command containing the substring `create` (e.g. `--json createdAt`) is blocked
by the pr-author hook's raw-containment check. Related: [[pr-author-hook-blocks-gh-in-this-repo]].

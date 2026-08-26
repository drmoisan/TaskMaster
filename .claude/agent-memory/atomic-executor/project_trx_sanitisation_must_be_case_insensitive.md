---
name: trx-sanitisation-must-be-case-insensitive
description: vstest writes the TRX `storage=` attribute in all-lower-case, so a case-SENSITIVE host-path substitution clears the header and leaves one leaked path per test; also the Deploy_ dir name itself carries the account and machine name
metadata:
  type: project
---

Sanitise every TRX **case-insensitively**, and sweep with a case-insensitive fixed-string search
afterwards rather than trusting the substitution.

**Why:** On 2026-08-26 a sweep of `qfc-collection-controller-defects-468` found the account name and
the absolute worktree path in **16 already-committed TRX files** — 946 occurrences in one full-suite
TRX. The earlier phase had substituted only the mixed-case spelling of the workspace root. That
cleared `<TestRun name=...>`, `runUser`, `computerName` and `runDeploymentRoot`, so a
machine-name search returned zero and the artifact recording the sanitisation truthfully reported
"0 hits" for the machine name. But vstest writes the `storage=` attribute of **every**
`<UnitTest>` element in all-lower-case, so one leaked absolute path survived per test. A stack
trace in a failing TRX leaks the source path of the throwing frame too, which is why a failing TRX
needs more substitutions than a passing one of the same size.

**How to apply:**
- Substitute in this order, all case-insensitive: workspace-root prefix -> `<repo-root>`,
  user-profile path -> `<user-profile>`, machine name -> `<host>`, account name -> `<user>`. A
  shorter token applied first eats the prefix of a longer one.
- Read and write the TRX through a raw byte layer (`open($fh, '<:raw', ...)`) so BOM and CRLF state
  are untouched. Verify the substitution count is a plausible multiple of the test count, not a
  whole-file rewrite: `git diff --stat` should show roughly `2 x substitutions` changed lines.
- vstest also creates an empty `Deploy_<account> <timestamp>_<pid>/In/<MACHINE>/` scaffolding
  directory inside every `/ResultsDirectory:`. Git does not track empty directories so it never
  reaches a commit, but delete it anyway with `rmdir` rather than relying on that.
- **An artifact that documents a sanitisation must not quote the raw tokens.** The same feature had
  a committed QA artifact whose substitution table's *From* column and `BEFORE:` examples
  reproduced every identifier the sanitisation had removed. Name the token **class**
  (workspace-root prefix, user-profile path, `computerName` attribute, `runUser`) and keep only the
  `AFTER:` lines.

See [[_shared_no_absolute_host_paths]].

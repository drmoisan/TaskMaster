---
name: trx-carries-host-tokens-in-two-casings
description: A committed .trx leaks the Windows account and machine name in four attributes and in TWO casings inside the same document, so a host-token sweep must be case-insensitive and must check artifact CONTENT, not just names
metadata:
  type: reference
---

`vstest.console.exe` writes host identity into every `.trx` it produces, and `.trx` is **not** matched
by any `.gitignore` pattern in TaskMaster, so those files enter the delivery commit unless a plan
sweeps them.

Measured on `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/regression-testing/r2-full-diagnostic.trx`
(2026-09-02):

- `TestRun/@name` — `<account>@<MACHINE>` , machine upper-cased
- `TestRun/@runUser` — `<Machine>\<account>` , machine **title-cased**
- `Deployment/@runDeploymentRoot` — `<account>_<MACHINE>_<timestamp>`
- every `UnitTestResult/@computerName` — `<MACHINE>` , upper-cased

**The two casings sit in the same document.** A case-sensitive sweep keyed on `$env:COMPUTERNAME`
(which returns the upper-cased form) rewrites `computerName` and `name` and leaves `runUser` intact.
Use `(?i)` on both patterns.

Sibling artifacts leak the account token too: MSBuild `.min.log` files carry the worktree's absolute
path, and a `Get-Command <global-tool>` result resolves under `$env:USERPROFILE`.

**How to apply:** any plan that commits `.trx` or `.min.log` evidence needs a sanitisation task placed
**before** the delivery commit — a sweep after committing cannot reach the commit already made. Gate it
on artifact **content**, not names: a name-only gate passes while the rule is violated. Derive both
tokens at run time per [[runtime-derived-account-token-pattern]] so the plan is self-exempt. Related:
[[../_shared_no_absolute_host_paths]], [[project-670-capture-time-sanitisation-seams]].

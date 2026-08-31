---
name: runtime-derived-account-token-pattern
description: How to gate host-identity leaks without spelling the account name in the plan; the run-time-derived pattern also exempts the plan from its own sweep and does not flag the GitHub org handle or the human display name
metadata:
  type: reference
---

To sweep a folder for a leaked Windows account name without writing that name into the plan (which would make the plan itself a hit under its own no-exclusion sweep), derive the token at run time from the profile folder name:

```
$t=[regex]::Escape((Split-Path -Leaf $env:USERPROFILE))
@(Get-ChildItem -Recurse -File -Path <folder> | Select-String -Pattern "(?i)$t").Count
```

Verified properties (measured on the issue #644 feature folder, 2026-08-30):

- **Self-exempt by construction.** The plan carries the expression, never the token, so the sweep needs no exclusion for the plan file. The companion shape pattern `[A-Za-z]:[\\/]Users[\\/]` is separately self-exempt because the character immediately before its `:` is the class-closing `]`, outside `[A-Za-z]`.
- **It does not over-match legitimate identity strings.** The repository's GitHub org handle and the human display name (first name + space + surname) both contain the surname but neither contains the contiguous profile-folder token, so neither is flagged. A broader surname-fragment probe returned 8 matching lines across 5 files where the correct pattern returns 3 — do not use a surname fragment as a proxy when measuring the real figure.
- **It is strictly broader than the shape pattern** for this leak class: it sees doubled-backslash paths and bare tokens that the shape pattern structurally cannot. Run both; require both to return `0`. See [[observation-scope-must-match-blast-radius]].
- **It does not cover the mail local-part**, which is shorter than the profile token. If a target line carries the mail address too, remove it in the same edit rather than relying on the account-token gate to catch it.

Related: [[../_shared_no_absolute_host_paths]].

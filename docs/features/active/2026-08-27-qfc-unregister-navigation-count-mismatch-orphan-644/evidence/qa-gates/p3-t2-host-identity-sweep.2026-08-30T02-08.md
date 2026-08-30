# [P3-T2] — Final host-identity sweep over the feature folder

- Timestamp: 2026-08-30T02-08
- Task: `[P3-T2]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Scope: `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644`,
  recursive, all files, **no path exclusion of any kind**.
- EXIT_CODE: 0 (every command below)

This sweep guards against a host path or account token having been echoed into an
evidence artifact by any Phase 2 command's captured output. In accordance with this
task's instruction, neither the raw absolute-path text nor the account token is
reproduced in this artifact.

## Sweep 1 — absolute user-profile path pattern

- Command: `@(Get-ChildItem -Recurse -File -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644 | Select-String -Pattern '[A-Za-z]:[\\/]Users[\\/]').Count`
- EXIT_CODE: 0
- Required: `0`
- Measured: **`0`**

### Shell-quoting correction applied while running this command

The first invocation of this command reached the regex engine as
`[A-Za-z]:[\/]Users[\/]` rather than the plan's `[A-Za-z]:[\\/]Users[\\/]`, because the
bash layer collapses a doubled backslash inside a double-quoted argument. In .NET
regex, `[\/]` is an escaped forward slash and matches `/` only, so the collapsed form
would have been blind to any Windows-style backslash path and the `0` it returned would
have been a partially vacuous result.

The command was therefore re-run with the backslash character constructed inside
PowerShell as `[char]92`, so the pattern that reached the regex engine was verified by
printing it: `[A-Za-z]:[\\/]Users[\\/]`, matching the plan's literal exactly.

Detector self-test on that verified pattern, confirming it is not vacuous:

| Probe | Expected | Measured |
|---|---|---|
| A synthetic drive-letter path using backslash separators | match | **True** |
| A synthetic drive-letter path using forward-slash separators | match | **True** |
| `no host path here` | no match | **False** |

Both probe strings are described rather than quoted. An earlier revision of this
artifact quoted the forward-slash probe literally, and `[P3-T3]`'s closing pre-staging
sweep — which is the pass that first covers this artifact, since this artifact did not
exist when `[P3-T2]` ran — returned `1` on it. The quoted probe was a synthetic
placeholder containing no real account or machine name, but the gate is mechanical, so
the literal was redacted in place under the repeat branch `[P3-T3]` authorizes, `[P3-T1]`
was re-run in full, and `[P3-T3]` was repeated from its start.

The detector matches both separator forms and rejects a non-matching string, so the
measured count of `0` over the feature folder is a real negative rather than an artefact
of a degraded pattern.

## Sweep 2 — account-token pattern

- Command: `$t=[regex]::Escape((Split-Path -Leaf $env:USERPROFILE)); @(Get-ChildItem -Recurse -File -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644 | Select-String -Pattern "(?i)$t").Count`
- EXIT_CODE: 0
- Required: `0`
- Measured: **`0`**

The account token is derived at runtime from the environment and is not written to this
artifact. Its length is 9 characters and it begins with an alphabetic character; those
two facts are recorded only to evidence that a non-empty token was actually built, so
the search was not run against an empty pattern that would match everywhere or nowhere
for an unrelated reason.

Detector self-test, confirming it is not vacuous:

| Probe | Expected | Measured |
|---|---|---|
| A synthetic string containing the account token between fixed affixes | match | **True** |
| The same affixes with the token removed | no match | **False** |

## Redaction points that this sweep confirms are effective

- `[P2-T3]` and `[P2-T4]` record msbuild output. msbuild echoes full project paths in
  its per-project output; every such path was replaced with a `<REPO_ROOT>` placeholder
  before being written.
- `[P2-T5]` records the vstest run. The default `vstest.console.exe` TRX filename embeds
  both the account name and the machine name; it is cited only as
  `<ACCOUNT>_<MACHINE>_2026-08-30_03_31_03_net481.trx`. The resolved
  `vstest.console.exe` path is recorded as `<VSTEST_CONSOLE>`, and the working directory
  as a generic repository-root placeholder.
- `[P2-T1]`, `[P2-T2]` and `[P3-T1]` record only repository-relative paths.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| Sweep 1 count | 0 | **0** | PASS |
| Sweep 2 count | 0 | **0** | PASS |

Both counts are `0`, so the redaction branch this task authorizes was not entered: no
artifact required in-place redaction and this task was not repeated.

## Output Summary

Both host-identity sweeps over the feature folder return `0` with `EXIT_CODE: 0`. Sweep
1 was re-run with the backslash constructed in PowerShell after the initial bash
invocation collapsed the doubled backslash, and the corrected pattern was printed and
self-tested against both separator forms before the result was accepted. Sweep 2's
detector was likewise self-tested with a positive and a negative probe. No absolute host
path, account name, or machine name is present in any file under the feature folder.

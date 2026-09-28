---
name: write-verbose-remedy-is-inert-without-a-verbose-call-site
description: A "visibility-only" discharge implemented as Write-Verbose proves nothing unless the deployed invocation passes -Verbose or raises $VerbosePreference - check the call site, not the function
metadata:
  type: feedback
---

When a remediation discharges a finding as "visibility-only" by adding a diagnostic record, trace
the record to the **deployed** invocation before accepting it.

**Why:** On #911 cycle 2 (2026-09-20) the R9c discharge added
`Write-Verbose ('Manifest discovery: enumerated directories {0}, returned files {1}' -f ...)` to a
non-recursive file lister, with a comment saying it "makes the shortfall observable in the run log".
`.github/workflows/dependabot-repair.yml` invokes the script with no `-Verbose` and sets no
`$VerbosePreference`, and a GitHub Actions `pwsh` step defaults to `SilentlyContinue`. The run log
contains nothing. The decision record asserted an outcome the production path cannot produce.

**How to apply:**
- `Write-Verbose` and `Write-Debug` are opt-in. Grep the call site for `-Verbose`, `$VerbosePreference`,
  or a `[CmdletBinding()]` caller that itself runs verbose. If none, the remedy is inert.
- `Write-Information ... -InformationAction Continue` and `Write-Warning` are not opt-in and are the
  correct choice for a record that must appear in a CI log. Point at a sibling in the same repo that
  already does it — on #911 `Sync-PackageReferences.ps1` used exactly that pattern for its summary.
- The same test applies to any observability discharge: a log line, a metric, a step-summary write.
  Ask "which deployed command emits this, and to where".

Sibling of [[feedback_verify-asserted-evidence-mechanism]] and the
"trace each AC's mechanism from the deployed invocation" rule in [[project_911-review-residuals]].

# Remediation Inputs — Issue #979

Timestamp: 2026-10-06T21-49
Review verdict: autonomous

## Required Change

Address `CR-979-1`: `BuildTriageClassifierAsync` must rebuild the Triage classifier when the ribbon command is selected and the Triage engine is disabled or otherwise absent from `Globals.Engines.InboxEngines`. It must not silently return in that state.

## Constraints

- Keep the existing injected `TriageClassifierRebuildAsync` test seam.
- Reuse the controller's existing lazy Triage initialization behavior rather than creating a second Triage lifecycle.
- Do not change the aggregate-coverage exception: it remains issue-979-only and applies only to the aggregate threshold.
- Add deterministic MSTest coverage for the absent/disabled-engine controller path without Outlook, filesystem, or external services.
- Run the C# toolchain in required order and retain the existing feature-method coverage threshold.

## Primary Evidence

- `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T21-49-audit/code-review.2026-10-06T21-49.md`
- `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T21-49-audit/feature-audit.2026-10-06T21-49.md`
- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- `docs/features/active/2026-10-06-build-triage-classifier-979/plan.2026-10-06T19-29.md`

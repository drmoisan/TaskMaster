---
name: epic-integration-base-invalidates-research-line-counts
description: Research measured against main is stale on an epic integration branch; TestSupport.cs was 489 not 365 lines, and every plan line citation into it was shifted by +3
metadata:
  type: project
---

When a child feature branches from an **epic integration branch** rather than from `main`, every line
count and line citation the research document took against `main` may already be stale, because
sibling epic children have landed into the integration branch in the meantime.

Measured on #493 (2026-08-27), base `epic/quickfiler-bug-family-integration`:

- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` was **489** lines, not the 365 both
  research §8 and `spec.md` recorded from `main` at `988e819b`. Siblings had appended shared arrange
  helpers carrying `Issue #480`, `Issue #483`, and `Issue #485` doc markers.
- Projected headroom was therefore 135 lines; **actual headroom was 11** against the 500-line ceiling.
- Every plan line citation into that file was shifted by a uniform **+3** (`238-249` was really
  `241-252`; `221-222` was `224-225`; `213-220` was `216-223`).
- `QuickFiler.Test.csproj`'s `<Compile Include>` anchor was at line **157**, not the 146 the spec and
  research cited.
- `Part2.cs`, `FocusAndThemeTests.cs`, and `UiThread.cs` matched their research figures exactly, so the
  staleness is per-file, not global. Do not assume a uniform offset across files.

**Why:** research runs before the epic's other children merge. The plan inherits the research figures
as prose, and a plan whose gates restate a projection instead of re-measuring would have reported
headroom that does not exist.

**How to apply:** in Phase 0, measure line counts and locate every cited member by a line-numbered
search on its **identifier**, never by trusting the cited line range. Record the divergence and the
per-file offset in the file-inventory baseline artifact so later tasks are read against a disclosed
baseline. Apply edits by matching exact source text (Edit tool / `.Replace`), not by line offset. A
plan that already says "treat this AC as a fresh measurement, not a restatement of the projection"
(#493's Decisions Record D2) survives this intact; one that pins an absolute count does not.

Related: [[verify-line-citations-with-numbered-output]], [[exact-count-gate-vs-remediation-loop]],
[[stale-base-deletes-silently-on-fan-in]].

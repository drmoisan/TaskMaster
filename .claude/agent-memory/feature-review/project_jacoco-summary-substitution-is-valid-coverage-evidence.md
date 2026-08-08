---
name: jacoco-summary-substitution-is-valid-coverage-evidence
description: Committed package-level JaCoCo summaries (not raw Cobertura) are the established TaskMaster coverage-evidence convention since commit d0955dc4; verify by re-summing counters, and corroborate per-class figures arithmetically because the raw report is gone.
metadata:
  type: project
---

Since commit `d0955dc4` ("docs(#503): replace raw cobertura coverage evidence with jacoco summaries",
2026-08-08), TaskMaster features commit **package-level JaCoCo summaries** as coverage evidence
(`<FEATURE>/evidence/baseline/coverage-baseline.jacoco.xml`,
`<FEATURE>/evidence/qa-gates/coverage-postchange.jacoco.xml`) instead of the ~10 MB raw Cobertura
reports the executor actually produces. Each raw report is ~187,000 lines; the pair would add ~20 MB
and ~378,000 lines to permanent history per bug fix. Seen again on #508.

The substitution is legitimate and is NOT an absent-artifact FAIL. Do not repeat #309's procedural
FAIL against it.

**Why:** The reviewer's mandated model is evidence verification from existing artifacts, and the
JaCoCo file carries the identical measured totals. The canonical gate path
`artifacts/csharp/coverage.xml` is also generated in JaCoCo form from the same source, because
`.claude/hooks/validate-feature-review-coverage.ps1` parses JaCoCo `<counter>` elements and cannot
read Cobertura at all (see [[project_csharp-coverage-artifact-is-cobertura]] for the older,
opposite failure mode).

**How to apply:**

1. Re-sum the counters yourself rather than trusting the prose. A few lines of Python summing
   `type="LINE"` / `type="BRANCH"` `missed`/`covered` across `<package>` elements reproduces the
   repo-wide figure exactly. On #508 this confirmed 95274/111021 = 85.8162% baseline ->
   95325/111059 = 85.8328% post-change.
2. Also re-sum `artifacts/csharp/coverage.xml` and check it matches the committed post-change file.
   A mismatch means the gate artifact is a different, unexplained measurement.
3. Per-class / per-changed-line figures are NOT re-derivable from a package-level summary. The
   evidence doc will still cite the deleted `*.cobertura.xml`. Corroborate arithmetically instead:
   the owning package's total-line delta should equal the changed class's line count, and the
   covered/missed split should be consistent with the claimed per-class rate. Record it as
   corroboration, not proof, and raise an advisory asking future substitutions to transcribe the
   per-changed-file counts inline.
4. Treat small repo-wide deltas skeptically. On #508 the reported +0.0166 pp line delta was smaller
   than measurement noise — the `QuickFiler` package, with **zero** changed lines, showed 6 lines
   flipping from missed to covered between the two reports. Rest the non-regression verdict on the
   absolute figures clearing the 85%/75% floors with margin, not on the sign of a sub-noise delta.

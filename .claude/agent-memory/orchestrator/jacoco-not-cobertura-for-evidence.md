---
name: jacoco-not-cobertura-for-evidence
description: Never commit raw Cobertura coverage reports as feature evidence — convert to package-level JaCoCo first; the maintainer deleted ~20MB of them on 2026-08-08
metadata:
  type: feedback
---

Commit coverage evidence as compact package-level **JaCoCo** summaries, never raw Cobertura reports.

**Why:** Commit `d0955dc4` ("docs(#503): replace raw cobertura coverage evidence with jacoco
summaries", 2026-08-08) deleted ~20 MB and ~374,000 lines of committed Cobertura from the #503
feature folder and replaced it with two 39-line JaCoCo files. The maintainer's stated reasoning is
that every feature would otherwise repeat this. A full-repo `coverage.cobertura.xml` for TaskMaster
is about 10 MB / 187,000 lines; two of them per feature is unacceptable permanent history.

**How to apply:** The atomic-executor will still produce raw Cobertura — that is fine, it is the
tool output. Before the docs commit is *pushed*, convert and swap. On #508 I caught it pre-push and
amended, so the 20 MB never entered history at all; that is strictly better than the #503 cleanup.

The conversion is a lossless projection: stream the Cobertura with `XmlReader`, count `<line>`
elements per `<package>` (`hits > 0` = covered), and parse the `(covered/total)` pair out of each
`condition-coverage` attribute for branches. Emit:

```xml
<report name="TaskMaster">
  <package name="UtilitiesCS">
    <counter type="LINE" missed="7530" covered="69250" />
    <counter type="BRANCH" missed="3129" covered="16129" />
  </package>
  ...
</report>
```

Verify the projection by checking the derived totals reproduce the Cobertura root
`lines-covered` / `lines-valid` attributes exactly. Write a
`evidence/qa-gates/coverage-artifact-substitution.<ts>.md` note recording the swap, the verified
totals, and the denominator scope (nine first-party packages; vendored assemblies excluded by
`coverage.config`).

Also generate `artifacts/csharp/coverage.xml` from the same JaCoCo projection — it is gitignored and
local-only, but `.claude/hooks/validate-feature-review-coverage.ps1` parses JaCoCo `<counter>`
elements and cannot read Cobertura. Confirm it re-sums above the floors (line >= 85, branch >= 75)
before running feature-review; the hook forces a FAIL verdict when repo-wide line is below 85.
This supersedes the older blanket "never generate coverage.xml" note — generate it, but only after
confirming the figure clears 85.

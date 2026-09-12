---
name: qfc-perfile-coverage-viewerqueue-434
description: Epic #136 / child F4 (#434) viewer-queue research — reusable coverage-baseline shortcut, the method-group optional-parameter trap, and the enum-file zero-denominator fact
metadata:
  type: project
---

Three non-obvious facts established while researching the VIEWER-QUEUE cluster of child F4 (#434) of
epic `quickfiler-per-file-coverage` (#136) on 2026-08-07.

**1. Per-file coverage baselines already exist on disk without running any coverage tool.**
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
and its sibling `evidence/baseline/coverage-baseline.cobertura.xml` are committed Cobertura reports
covering the whole QuickFiler assembly with per-line hit counts. Reading them gives exact
uncovered-line sets per file.

**Why:** epic #136 forbids running `msbuild`/`vstest` during research (slow, and F1's harness does
not exist yet), yet each per-file artifact must state the real coverage gap.
**How to apply:** for any epic-#136 child, grep those artifacts for
`filename="QuickFiler\<path>"` before claiming a gap is unmeasured. Verify the line numbers still
line up with the current file (they did for the whole helper-classes set) and label the figures
indicative, with F1's harness authoritative. Two caveats found: the `final` report omits the
compiler-generated `<>c` closure classes that the `baseline` report includes, and a file can emit
two `<class>` elements sharing one `filename`, so per-file aggregation must union by line number
taking max hits.

**2. A `static` method consumed as a method group cannot gain an optional parameter.**
`EfcViewerQueue.Dequeue()` is bound as `Func<EfcViewer> = EfcViewerQueue.Dequeue` at
`QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:40` and `:112`. C# method-group
conversion does not fill optional parameters, so adding `CancellationToken token = default` is a
compile break in that file.

**Why:** the epic's standard conflict-avoidance move is "new optional parameter with a production
default", which is normally safe; method-group call sites are the exception that breaks it.
**How to apply:** before proposing an optional-parameter seam on any member, grep for the member
name used *without* parentheses. If it is a method group, propose a new overload instead. See
[[qfc-item-controller-227-r2-denial]] for the related habit of checking sibling members before
concluding a seam is safe.

**3. An enum-only C# file has a zero coverage denominator, not 0% coverage.**
`QuickFiler/Helper Classes/QfEnums.cs` (a `static class` whose only member is a nested `enum`) emits
**no** Cobertura `<class>` element at all — the type name appears only inside other classes' method
`signature` attributes.

**Why:** enum members are literal fields with no IL body, and the containing static class declares
no method, so Roslyn emits no sequence point.
**How to apply:** classify such files `no-executable-code` in a coverage ledger, not
`ratified-exempt`; do not add `[ExcludeFromCodeCoverage]` and do not touch `coverage.config`. Warn
the harness owner that "absent from the report" must not render as "0% covered", or every
declaration-only file becomes an unfixable failure.

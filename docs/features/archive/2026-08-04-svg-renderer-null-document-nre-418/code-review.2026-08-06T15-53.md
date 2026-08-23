# Code Review — svg-renderer-null-document-nre (Issue #418)

- Artifact timestamp: `2026-08-06T15-53`
- Review cycle: reaudit 4 (maintainer-decision verification)
- Base: `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`
- Head: `bug/svg-renderer-null-document-nre-418` @ `215a6f7c8bbbc3157ecd4967bd44af632d786b8b`

## Executive Summary

**No code changed since the previous review cycle.** `git diff --name-only 69e675d0 HEAD` returns nine
paths, all Markdown. Every code finding from `code-review.2026-08-05T00-04.md` carries forward at the
same severity, and no new code finding arises. The four Low findings recorded there remain open and
remain optional.

The delivered fix is sound. The swallowing `catch (Exception) { return null; }` at the old
`GetSvgDocument` is replaced by a `TryGetSvgDocument` boundary that captures the exception, emits it on
two channels, and returns a `bool` the caller must inspect. Both byte-array constructors now degrade to
`Size.Empty` rather than dereferencing a null document, which is the behavior AC-3 specifies for a
control constructed by designer-generated code in eleven forms. The `AssemblyResolve` fallback's pure
decision logic was extracted into `SvgAssemblyProbe` and is exhaustively tested; what remains in
`SvgAssemblyResolver` is host-bound wiring.

Two **new findings this cycle are documentation-accuracy defects in the AC-11 evidence capture**, not
code defects. They are recorded here because the capture is a reviewed artifact and one of its claims
is verifiably false:

- **CR-Med-1** — the capture asserts the dual-channel diagnostic is "proven by unit tests in
  `SVGControl.Test`". No such test exists. Verified: zero occurrences of `Trace`, `log4net`,
  `Listener`, `Appender`, or `DescribeFailure` anywhere in `SVGControl.Test/*.cs`. The behavior is
  verified by code inspection and is executed-but-unasserted by the parse-failure tests.
- **CR-Med-2** — the capture omits two fields the runbook lists as mandatory, one of which (whether
  Visual Studio was restarted after the build) is the field the runbook added specifically to
  guarantee the designer loaded the freshly built assembly.

Neither blocks. Both warrant a short correction to the capture so the audit trail states the basis it
actually has.

A structural observation worth recording: the fix's most valuable design decision was extracting
`GetProbeDirectories` and `PublicKeyTokensEqual` into a pure static class. That is what makes 100% line
and 100% branch coverage reachable on the resolver's decision logic, and it is why the residual
uncovered region is confined to genuinely host-bound calls rather than spread across testable
branching. The G-9 waiver is defensible largely because that extraction was done first.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Medium | `evidence/regression-testing/designer-load-2026-08-06T19-47.md` | Line 39 | CR-Med-1. States "The dual-channel behavior is proven by unit tests in `SVGControl.Test`". No test asserts either the `Trace` or the `log4net` channel; the claim is false. | Reword to "verified by code inspection of the four paired `logger.Error` / `Trace.TraceError` sites, and executed though not asserted by the parse-failure tests". Optionally add a `TraceListener`-capturing test to convert inspection into assertion. | The clause is load-bearing: it is the fallback offered when disclaiming the unexercised designer-host observation. The conclusion survives, because an implementation-shape requirement is legitimately verifiable by inspection, but the stated basis must match the actual basis. | `grep -rn "Trace\|log4net\|Listener\|Appender" SVGControl.Test/*.cs` → no matches; `grep -rn "DescribeFailure" SVGControl.Test/*.cs` → no matches |
| Medium | `evidence/regression-testing/designer-load-2026-08-06T19-47.md` | Header block | CR-Med-2. Omits two fields the runbook lists under "The artifact must contain, at minimum": the Visual Studio product name/version and build configuration, and whether Visual Studio was restarted after the build. | Append an addendum recording both. If the restart cannot now be recalled, record it as unknown rather than assuming it. | Runbook step 2 exists to guarantee the designer loads the freshly built `SVGControl.dll`; without the record, a cached pre-fix assembly is not excluded. AC-11 still holds because the pre-fix failure in this same environment is documented in `issue.md`, but the inference spans two sessions instead of one recorded prerequisite. | Runbook lines 200-217 (mandatory field list); capture lines 1-9 (header block as written) |
| Low | `SVGControl/SvgAssemblyResolver.cs` | Diagnostic strings | CR-Low-1 (carried forward). Diagnostic prefixes still read `"SvgRenderer load ..."`, naming a type the code no longer lives in after the R-6 extraction. | Retarget the prefixes to `SvgAssemblyResolver`. | A diagnostic that names the wrong type sends a future reader to the wrong file — the same class of misdirection this issue was opened to fix. | Unchanged from `code-review.2026-08-05T00-04.md` |
| Low | `SVGControl/SvgAssemblyResolver.cs` | `DescribeFailure` / `typeof` usage | CR-Low-2 (carried forward). The resolver reaches back into `SvgRenderer` for `DescribeFailure` and `typeof`, so the R-6 separation is incomplete. | Move `DescribeFailure` to a shared internal helper, or duplicate the three-line formatter locally. | A one-directional dependency from the extracted type back into the type it was extracted from limits the value of the split. | Unchanged from `code-review.2026-08-05T00-04.md` |
| Low | `SVGControl/SvgRenderer.cs` | Lines 28-68 | CR-Low-3 (carried forward). The two byte-array constructors carry near-identical 17-line bodies differing only in the margin argument and the log prefix. | Delegate the three-argument overload to the four-argument one with `new Padding(0)`. | Duplication in a constructor pair is where divergence accumulates; the general policy directs factoring reusable logic. | Unchanged from `code-review.2026-08-05T00-04.md` |
| Low | `SVGControl/SvgAssemblyResolver.cs` | Lines 50-54 | CR-Low-4 (carried forward). The pre-guard region (`new AssemblyName(args.Name)`, `loaded.GetName()`) sits outside the containment `try`. | Leave as is, or extend the `try` to enclose it. | Disclosed and accepted in the remediation plan's Design Decision 11. Both calls raise only on a malformed assembly name supplied by the CLR itself. | Unchanged from `code-review.2026-08-05T00-04.md` |
| Info | `SVGControl/app.config`, `SVGControl.Test/app.config` | `Fizzler` `dependentAssembly` | CR-Info-1 (carried forward). Stale redirect to `1.3.0.0` while the on-disk package is `Fizzler.1.3.1` and both production references declare `Version=1.3.1.0`. | Promote `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`. | Inert today only because `Fizzler.dll` is absent from the test output. Correctly deferred rather than fixed in a `minor-audit`; the cycle-2 refusal to add the reference was the right call. | `ls -d packages/Fizzler*` → `Fizzler.1.3.1` |
| Info | `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` | Whole file | CR-Info-2 (new). The waived `SvgAssemblyResolver.cs` residual is named in no follow-up item, unlike G-1's residual which this file owns. | Add `SVGControl/SvgAssemblyResolver.cs` with a note that its shortfall is waived rather than accepted indefinitely. | Without an owner, the 66 uncovered lines have no path back to review if a host-level seam later makes them testable. | `grep -n "SvgAssemblyResolver\|ResolveByNameAndKey"` on that file → no matches |

## Positive Observations

- **The `Try` pattern is applied correctly.** `TryGetSvgDocument` returns `bool` with `out SvgDocument?`
  and `out Exception?`, and the true branch guarantees a non-null document — a contract the call sites
  rely on with a documented `!` rather than an unchecked assumption.
- **The tolerant contract was preserved deliberately.** `GetSvgDocument(byte[])` keeps its null-returning
  behavior with no `try`/`catch` of its own, so the six existing null-tolerant consumers are unaffected
  while new callers get an explicit failure surface. This is the right way to add a fail-fast API
  without a breaking change.
- **`DescribeFailure` handles the element-free case distinctly.** A null error yields "the payload
  contained no SVG elements." rather than a null-reference on `error.GetType()`. Small, but it is the
  exact defect class the issue is about.
- **The AC-5 amendment corrected a false premise in the criterion itself** rather than working around
  it, and disclosed the correction's measured scope precisely, including what was *not* measured. That
  is the standard this feature's documentation generally meets — which is why CR-Med-1 stands out.

## Verdict

**No blocking code findings.** The implementation is ready for a pull request. The two Medium findings
are corrections to an evidence artifact, and the four Low findings are optional polish carried forward
from the previous cycle.

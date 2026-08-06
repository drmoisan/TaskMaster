# Designer Load Verification — AC-11 (Issue #418)

Timestamp: 2026-08-06T19-47
Command: manual — Visual Studio WinForms designer, human-executed
EXIT_CODE: n/a (human procedure, not a command)
Procedure: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/runbooks/verify-winforms-designer-load.runbook.md`
Performed by: project maintainer (Dan Moisan)
Head at verification: `db8b59fb`
Reported to the orchestrator: 2026-08-06, in session

## Output Summary

**PASS.** `UtilitiesCS/Dialogs/MyBoxViewer.cs` opens in the Visual Studio WinForms designer and renders correctly. No `NullReferenceException`. The default SVG artwork is visible in the control, so `SvgDocument.Open` succeeded and the ExCSS assembly bind resolved inside `devenv.exe`.

## Observed outcome

| Observation | Result |
|---|---|
| Form loads in the designer | Yes — renders properly |
| `NullReferenceException` | **None** |
| Default SVG image visible in the control | **Yes** — the image appeared |
| Output window (Debug pane) diagnostic | **None present** |

## Why the empty Debug pane is the correct result

The maintainer initially asked where to find detail in the Output window and reported none. That is the expected outcome given the image rendered, not a missing observation.

`SvgRenderer` emits its dual-channel diagnostic — `log4net` plus `System.Diagnostics.Trace` — only on the failure path, when `SvgDocument.Open` cannot produce a document. Here the parse succeeded, so no failure occurred and there was nothing to report. An empty Debug pane is exactly what a successful load produces.

(For the record, the Output window's Trace content is reached via **View → Output**, then the **"Show output from:"** dropdown set to **Debug**. That dropdown has no default selection until a pane has been written to.)

## What this verifies

- **AC-11** — satisfied. The designer loads the form without a `NullReferenceException`, which is the criterion's stated requirement and the observation the bug report opened on.
- **AC-8** — corroborated. The original defect was that `SvgDocument.Open` threw `FileNotFoundException` for `ExCSS` inside `devenv.exe`, whose configuration carries no ExCSS binding redirect, and the pre-existing `AssemblyResolve` fallback returned `null` because `Assembly.Load` probed the Visual Studio directory rather than the directory holding `SVGControl.dll`. The bind now succeeds in that host.

## What this does NOT verify — recorded as a limitation, not an omission

**The designer-host observability of the diagnostic was not exercised.** AC-3 requires the failure diagnostic to reach both `log4net` and `Trace` so it is visible in the Visual Studio Output window. Because nothing failed during this session, that channel was never driven in `devenv.exe`.

**Correction, 2026-08-06.** An earlier revision of this artifact stated that the dual-channel behavior is "proven by unit tests in `SVGControl.Test`". **That was false and is retracted.** `SVGControl.Test` contains zero occurrences of `Trace`, `log4net`, `Listener`, `Appender`, or `DescribeFailure`; no test asserts either channel. The parse-failure tests *execute* those lines — which is why `DescribeFailure` measures 100% coverage — but execution is not assertion. The clause was load-bearing, because it was the fallback offered when disclaiming this very limitation, so the corrected basis is stated below.

The accurate basis is **static inspection of the implementation**, which is what AC-3's operative requirement actually calls for: it constrains the implementation ("must therefore also emit the failure through a channel the designer surfaces... Both channels must carry the exception type and message"), not an observation. Verified in source: four paired `logger.Error` / `Trace.TraceError` sites, with `DescribeFailure` composing `error.GetType().FullName + ": " + error.Message`. The degrade-without-throwing behavior *is* genuinely proven by the AC-1 regression tests, which assert no throw and a null `Document`.

The specific claim "an operator would see the diagnostic in the VS Output window" therefore remains verified by construction rather than by observation. Confirming it would require inducing a parse failure in the designer host, which is outside this issue's scope.

**Attribution of the successful bind is not established.** Three mechanisms could each account for it, and this capture cannot distinguish them:

1. the `SVGControl.Test`/`SVGControl` binding redirect being applied,
2. the AC-8 directory-probing `AssemblyResolve` fallback resolving `ExCSS.dll` from the directory holding `SVGControl.dll`, or
3. `ExCSS.dll` already being present in the designer's shadow-copy directory.

A pass/fail render cannot separate these. Distinguishing them would need a fusion log or a run with the resolver uninstalled.

**Open question U-2 remains open — but the runbook was fully executed.** Runbook step 10 asks whether `ExCSS.dll` is present in `%LOCALAPPDATA%\Microsoft\VisualStudio\<version>\ProjectAssemblies\` alongside `SVGControl.dll`.

**Correction, 2026-08-06.** An earlier revision recorded this as "not reported", which reads as an operator omission. It was not. Step 10 is explicitly conditional — "Optionally, and only if the designer error page reported a failure to load `ExCSS`" — and the runbook's own field list qualifies it "*if performed*". No error page appeared, so the precondition was false and the step was correctly skipped. The runbook was executed in full.

U-2 therefore stays open in the plan's Open Questions section for want of a triggering condition, not for want of an observation. It does not gate AC-11, whose criterion is the absence of a `NullReferenceException` on load.

**Two runbook fields are not recorded here:** the Visual Studio version and build configuration, and whether Visual Studio was restarted after the build. The second exists to guarantee the designer loaded the freshly built `SVGControl.dll` rather than a cached copy. AC-11 still holds without them, because the same environment demonstrably produced the `NullReferenceException` before the fix — that observation is the bug report itself — but the inference now spans two sessions rather than one recorded prerequisite. Recorded as a gap in this artifact rather than filled by assumption.

## Relationship to AC-7

The research artifact noted favourable sequencing: because the fix stops discarding the exception, a post-fix designer open would supply the observed exception identity for AC-7 *if the bind still failed*. The bind did not fail, so this capture supplies no exception identity. AC-7 was already satisfied on its own terms by `research/2026-08-04T15-05-svg-renderer-null-document-research.md`, which names the exception, the host, and the fallback's behavior; it does not depend on this capture.

## Human-interaction requirements discharged

- **H-1** — designer load verification. Response `exception`, runbook executed, evidence captured here.
- **H-2** — capturing the observed exception identity in the designer host. Response `exception`, same runbook. Discharged as *not applicable on this host*: no exception occurred, so there is no identity to capture. This is a discharge by absence of the condition, not by observation of it.

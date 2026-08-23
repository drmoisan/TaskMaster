# [P2-T11] AC-11 Human Runbook Handoff

Timestamp: 2026-08-04T20-05

Runbook: docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/runbooks/verify-winforms-designer-load.runbook.md

Owner: human operator

Cue: after AC-6 toolchain-clean-pass is recorded and before the feature is reported done

Expected evidence path: docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md

AC-11 state: unchecked pending human execution

## Why this is not automatable

AC-11 requires opening `UtilitiesCS/Dialogs/MyBoxViewer.cs` in the Visual Studio WinForms designer and
observing that the form loads without a `NullReferenceException`. That requires a live `devenv.exe` /
`DesignToolsServer.exe` designer host, which is an external process. `.claude/rules/general-unit-test.md`
prohibits unit tests from depending on external processes, and the plan's Test Plan records the
designer-host path as having no automatable integration equivalent. The executor therefore did not
attempt to automate it and left the criterion unchecked.

## Verification that AC-11 remains unchecked

`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md` still shows
`- [ ] **AC-11 — Designer load verified by the documented human step.**` It is the only unchecked
acceptance criterion in the file; AC-1 through AC-10 are all `- [x]`.

## What the human step is expected to confirm, and what already de-risks it

The automated evidence produced by this plan does not substitute for the runbook, but it narrows what
the runbook can still find:

- **The `NullReferenceException` failure mode is eliminated at the source level regardless of host.**
  Both byte-array `SvgRenderer` constructors now degrade rather than dereference a null document, proven
  by the four `SvgRendererParseContractTests` constructor tests that failed with
  `NullReferenceException` before the fix and pass after
  (`evidence/regression-testing/ac1-fail-before.2026-08-04T14-36.md` and
  `ac1-pass-after.2026-08-04T14-36.md`). This behavior is host-independent, which is the point of
  AC-3's degrade-and-log decision.
- **The ExCSS bind itself succeeds inside the vstest testhost**, a host that does apply the project
  binding redirects: `SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull` passes with its full
  `Document`-non-null assertion intact.
- **Open question U-2 remains the genuine unknown.** Whether `ExCSS.dll` is present in Visual Studio's
  `ProjectAssemblies` shadow-copy directory alongside `SVGControl.dll` determines whether the AC-8
  directory probe can succeed in the designer host. Step 10 of the runbook captures that observation.
  If the bind still fails there, the fix's second effect applies: the exception is no longer discarded,
  so the runbook capture supplies the observed exception identity rather than an opaque NRE.

## Next step for the owner

Execute the runbook, then write the capture to the expected evidence path above and change
`- [ ] **AC-11` to `- [x] **AC-11` in `issue.md`. Until that is done, this feature's acceptance-criteria
status is 10 of 11 delivered, with AC-11 intentionally outstanding.

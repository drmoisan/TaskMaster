# Remediation / Scope-Change Inputs — Issue #197 (2026-06-13T16-05)

- Canonical issue number: 197
- Trigger: Maintainer-directed scope change (not a review-blocking finding). The prior feature-review (2026-06-13T15-45) returned PASS with 0 blocking findings.
- Active folder: `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/`
- Branch: `refactor/com-vsto-coverage-exemption-197`

## Directive

Switch `TaskVisualization` from the **assembly-level** `coverage.config` exclude to **class-level** `[ExcludeFromCodeCoverage]` treatment, consistent with the other four assemblies (TaskMaster, ToDoModel, QuickFiler, Tags). The genuinely-testable seams in TaskVisualization must remain measured (in the coverage denominator), not exempted.

Reason: the blanket assembly exclude also removes the ~5–10% testable surface of TaskVisualization from measurement, masking real gaps and making the exempt/non-exempt boundary inconsistent with the rest of the change. The maintainer has directed class-level treatment for consistency, accepting the additional file edits.

## Required changes

1. **`coverage.config`**: remove the `<ModulePath>.*TaskVisualization.*</ModulePath>` entry added in Phase 1. (Leave the pre-existing third-party excludes — Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest — unchanged.)
2. **`TaskMaster.runsettings`**: remove the `TaskVisualization` `ModulePath` exclude added in Phase 1. (Leave pre-existing excludes unchanged.)
3. **TaskVisualization `.cs` files**: add `[ExcludeFromCodeCoverage]` to the COM/VSTO/WinForms-bound classes only. Use the same discipline applied to the other assemblies. Per the coverage roadmap §3.2 and the design memo §2.1:
   - **Exempt (COM/VSTO/WinForms-bound):** `TaskController`, `TaskViewer`, `AutoAssignContext`, `AutoAssignPeople`, `AutoCreateProject`, `FlagTasks`, `EditFilterViewer`, `ManageFilters`, and any other class whose members require `Microsoft.Office.Interop.Outlook.*` types or a live WinForms control with no injectable seam. Verify each by inspection.
   - **Preserve (do NOT exempt — keep measured):** `FlagChangeItem` (data container) and the testable paths of `FlagChangeTrainingQueue`. For partially-testable classes, do not apply a class-level attribute that would also exempt the testable half — leave the class unannotated (or annotate only the genuinely Outlook-bound methods at method level, mirroring the `IDList` method-level approach). `FlagChangeGroup` and `EditFilterController` must be assessed by inspection: exempt only if their members are genuinely Outlook/WinForms-bound with no testable pure-logic seam; otherwise leave measured.
4. **`spec.md`**: update the exempt-scope section (§ referencing TaskVisualization) to reflect class-level treatment with the preserved seams enumerated. Keep AC wording consistent.
5. **Re-measure coverage** and re-run the full C# toolchain (csharpier → analyzers → nullable → MSTest). Record the new production-only rate; the denominator will rise relative to the assembly-exclude variant because the preserved testable lines return to it.

## Constraints

- Attribute/config/doc changes only; no behavioral or API change.
- Apply the scope-change rule: if inspection shows a class listed as exempt above is actually a testable seam (or vice versa), record it and adjust rather than blindly annotating.
- Do not re-open the other four assemblies' annotations (TaskMaster/ToDoModel/QuickFiler/Tags) — they are unchanged by this directive.
- AC4 (measured rate vs the design estimate) remains a separate, still-open maintainer-acknowledgement item; this scope change is expected to lower the measured rate slightly and does not by itself resolve AC4.

## Acceptance for this cycle

- `coverage.config` and `TaskMaster.runsettings` no longer exclude TaskVisualization.
- TaskVisualization COM/WinForms classes carry `[ExcludeFromCodeCoverage]`; `FlagChangeItem` and `FlagChangeTrainingQueue` testable paths remain measured and appear in the post-change Cobertura denominator.
- C# toolchain green; test-result parity vs baseline maintained.
- spec.md and evidence updated; coverage delta recorded.

# B-3 verification (P4-T9, #1002 cycle 1)

Timestamp: 2026-10-22T13-33
Command: pwsh -NoProfile -File SCRATCH/issue-1002-r-perfile.ps1 -Files EfcFormController.Actions.cs,EfcFormController.Breadcrumb.cs -Classes
EXIT_CODE: 0
Output Summary:
- QuickFiler/Controllers/EfcFormController.Actions.cs: lines 26/28 (92.86%), branches 13/16 (81.25%). Baseline 26/111 (23.42%), 12/36 (33.33%).
- QuickFiler/Controllers/EfcFormController.Breadcrumb.cs: lines 93/101 (92.08%), branches 34/42 (80.95%). Baseline 78/121 (64.46%), 22/42 (52.38%).
- Both files meet at least 80% line and at least 75% branch.
- Class nodes remaining for these files in the post-processed Cobertura: one per file, `QuickFiler.Controllers.EfcFormController` (Actions.cs covered 26 valid 28; Breadcrumb.cs covered 93 valid 101). No residual compiler-generated state-machine or lambda-closure class nodes appear, so no shortfall exists and no exclusion beyond the five named members was added.
- Exemption form: member-level [ExcludeFromCodeCoverage] (parameterless attribute with adjacent comment, because the .NET Framework 4.8.1 attribute has no Justification property; see exclude-attribute-form artifact) on ActionOkAsync, ActionCancelAsync, CreateFolderAsync, RefreshSuggestionsAsync, ConfigureBreadcrumbControl.

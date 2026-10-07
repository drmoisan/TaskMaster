# Issue #973 update mirror

- Timestamp: 2026-10-03T10-31
- PostedAs: comment
- URL: https://github.com/drmoisan/TaskMaster/issues/973#issuecomment-5970118213
- Command: gh issue comment 973 --repo drmoisan/TaskMaster --body-file <session scratchpad>/issue-973-fold-comment.md
- EXIT_CODE: 0

## Posted text

Scope amendment (2026-10-03, maintainer direction): the following are folded into this issue rather than filed separately.

1. Remove six unused `using Microsoft.Graph.*` directives in UtilitiesCS:
   - `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` (line 7)
   - `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs` (line 10)
   - `UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs` (lines 11-12)
   - `UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs` (line 18)
   - `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs` (line 6)

   Research checked each directive against the types in the referenced Microsoft.Graph assembly and found no type or extension method that binds through any of them, including in Triage_OlLogic.cs. The plan proves each removal with a Grep count before and after, and with both msbuild Rebuild gates (zero CS0246, CS0103, CS0104, CS0234 or CS1061).
2. Reword the CLAUDE.md C#1 item 3 bullet "Do not add /p:Nullable=enable". It currently says there is no Directory.Build.props. The new wording says that no project and neither root build file (Directory.Build.props, Directory.Build.targets) sets `<Nullable>`. The conclusion is unchanged, and no `.claude/**` file is edited.
3. Related defect under the in-scope directive: CategoryClassifierGroup.cs is 539 lines, over the 500-line limit. The `IConditionalEngine Implementation` region moves unchanged into a new partial file, `UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.ConditionalEngine.cs`, with a Compile Include in UtilitiesCS.csproj.

Records in the feature folder `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/`:
- `issue.md`: section "Scope Amendment (2026-10-03, maintainer direction)".
- `spec.md`: Parts F, G and H, acceptance criteria AC19 to AC23, and the Scope Amendment Log. No existing criterion was weakened. AC16 now admits only the six named .cs files, limited to the permitted edits.
- `research/2026-10-03T00-41-graph-usings-and-claude-md-bullet-research.md`
- Plan `plan.2026-10-02T22-16.md` revision 1.6, cleared by executor preflight. The clearance record is under `evidence/other/`.

Branch: `bug/remaining-stale-binding-redirect-pairs-973`.

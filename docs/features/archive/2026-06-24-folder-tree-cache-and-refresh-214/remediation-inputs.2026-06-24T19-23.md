# Remediation Inputs - folder-tree-cache-and-refresh (Issue #214)

- Timestamp: 2026-06-24T19-23
- Source review artifacts:
  - `policy-audit.2026-06-24T19-23.md`
  - `code-review.2026-06-24T19-23.md`
  - `feature-audit.2026-06-24T19-23.md`
- Primary requirements source for remediation planning: this file
- Base context: `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`
- Original feature plan: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/plan.2026-06-24T15-42.md`

## Fix List

1. Fix cooperative traversal during live hierarchy enumeration.
   - Files: `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotBuilder.cs`, `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs`, related interfaces and tests.
   - Expected behavior: dispatcher-yield cadence and cancellation/deadline checks occur during live folder hierarchy traversal, before the entire Outlook hierarchy has been materialized.
   - Verification commands: targeted MSTest for snapshot builder/reader yield behavior; final C# QA loop.

2. Implement production Outlook notification subscriptions.
   - Files: `UtilitiesCS/OutlookObjects/Folder/OutlookFolderNotificationSink.cs`, related tests.
   - Expected behavior: the public `OutlookFolderNotificationSink(Outlook.NameSpace)` constructor creates and owns store-level and watched parent `Folders` event subscriptions, and `Dispose()` unsubscribes all production subscriptions.
   - Verification commands: targeted MSTest for production constructor subscription creation through testable adapters/factories; final C# QA loop.

3. Enforce cache request-scope correctness.
   - Files: `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshot.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderTreeRequest.cs`, related tests.
   - Expected behavior: a cached snapshot is returned only when it covers the requested store scope. A store-scoped snapshot must not satisfy all-store or different-store requests.
   - Verification commands: targeted MSTest for store A first/all stores second and store A first/store B second; final C# QA loop.

4. Preserve unaffected stores on store-scoped refresh.
   - Files: `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs`, snapshot merge or refresh policy helpers, related tests.
   - Expected behavior: a notification for store A either merges store A updates into an existing all-store snapshot or schedules an all-store refresh. Store B nodes must remain visible after store A refresh.
   - Verification commands: targeted MSTest for multi-store refresh preservation; final C# QA loop.

5. Retire direct EmailDataMiner `FolderTree` construction for issue #214 paths.
   - Files: `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.FolderExtraction.cs`, related tests and evidence.
   - Expected behavior: scrape paths use the shared cached hierarchy service or are removed if unreachable. No issue #214 EmailDataMiner partial contains direct `FolderTree` construction for in-scope full-enumeration behavior.
   - Verification commands: targeted EmailDataMiner tests; `rg -n "FolderTree\\s+\\w+\\s*=\\s*new\\(|new\\s+FolderTree|FolderTree\\.CreateAsync|Task\\.Run\\(\\(\\) => new FolderTree" UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner*.cs`.

6. Refresh evidence searches and whitespace check.
   - Files: feature evidence under `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/`.
   - Expected behavior: caller-migration evidence scans all relevant partials and catches target-typed direct construction. `git diff --check main..HEAD` either passes or any retained machine-generated diagnostics are documented as accepted generated-output exceptions in the final audit evidence.
   - Verification commands: updated caller-migration search; `git diff --check main..HEAD`; final C# QA loop.

## Do Not Do

- Do not modify `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs`.
- Do not reimplement startup-specific `JunkCertain` or `JunkPotential` path navigation in issue #214 remediation.
- Do not add `Application.DoEvents`.
- Do not use `Task.Run` as a mechanism for live Outlook folder hierarchy enumeration.
- Do not weaken acceptance criteria, repository policies, nullable settings, analyzer settings, or coverage thresholds.
- Do not add live Outlook COM requirements to unit tests.
- Do not broaden remediation beyond issue #214 folder-tree cache and refresh behavior.

## Required Final Verification

Run and record the full C# QA loop in order:

1. `dotnet tool run csharpier format .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. Coverage-enabled MSTest with `TestCategory!=LiveOutlook`

Also rerun:

- `git diff --name-status main..HEAD -- TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs`
- Updated banned API and caller-migration searches
- No-live-Outlook COM test search
- File-size check
- Coverage comparison

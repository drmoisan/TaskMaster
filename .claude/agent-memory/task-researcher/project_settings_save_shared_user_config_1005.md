---
name: settings-save-shared-user-config-1005
description: 26 production Settings Save() sites (15 test-reached) race on one testhost user.config; per-instance ISettingsStore seam + SettingsSaving cancel probe for fail-before
metadata:
  type: project
---

Issue #1005 (2026-10-22): every `Properties.Settings.Default.Save()` reached by a test races on the single host-derived user.config ("configuration file has been changed by another program"). Repo-wide there are 26 settings Save() sites (IDList 1, AppQuickFilerSettings 6, AppStagingFilenames 9, AppAutoFileObjects 6 via `_defaults.Save()`, AppOlObjects.JunkFolders 3, AppOlObjects DarkMode 1); 15 are test-reached. Test code also calls `defaults.Save()` in AppAutoFileObjectsCoverageExpansionTests.TestCleanup.

**Why:** `new IDList()` has `_maxIDLength == 0`, so the first GetNextToDoID always saves; TaskTree.Test MakeController hands out a fresh real IDList.

**How to apply:** recommend a public UtilitiesCS `ISettingsStore` (indexer + Save) with an `ApplicationSettingsBase` adapter, injected per instance. The adapter is coverable via a test subclass overriding the indexer and Save (both overridable). A deterministic fail-before: subscribe `SettingsSaving`, set `e.Cancel = true` (skips `base.Save`), attribute by thread id, unsubscribe in finally. Watch for grep misses: `_defaults.Save()` and `defaults.Save()` do not match `Settings.Default.Save()`. ToDoModel.Test and TaskTree.Test are C# 7.3 (no LangVersion).

Timestamp: 2026-06-24T16-25

Files read:
- docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/issue.md
- docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/spec.md
- docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/user-story.md
- artifacts/research/2026-06-24T15-44-folder-tree-cache-refresh-214-research.md

Issue confirmation:
- Canonical issue: #214
- Feature folder: docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/
- Work mode: full-feature

Non-goal confirmation:
- Issue #214 startup-scope exclusion treats startup-specific junk-folder work as related background only.
- Issue #214 excludes startup-specific junk-folder paths and must not modify TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs or the JunkCertain / JunkPotential startup construction sites unless separately coordinated.

Requirements summary:
- Implement a shared, lazy Outlook folder hierarchy cache service.
- Preserve Outlook STA access while using dispatcher-yield responsiveness.
- Use iterative traversal, cancellation/deadline checks, event-driven invalidation, deterministic disposal, and test seams that do not require live Outlook COM.
- Migrate the in-scope callers away from throwaway FolderTree construction for issue #214 paths.

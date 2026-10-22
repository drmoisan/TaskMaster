Timestamp: 2026-08-25T12-47
Command: Get-Content -Raw issue.md, spec.md, research.md, plan; git rev-parse HEAD
EXIT_CODE: 0
Output Summary: Issue #609 scope and source documents were read. The baseline revision is recorded below. The plan is limited to deterministic C# regression coverage and a router-only correction if a regression demonstrates one.

# Issue #609 scope record

BaselineRevision: b5c751519c6cf0eaeb2326d9e80b2439aeee7265

## Dual-representation invariants

- `IFolderHierarchyProvider.ResolveLeafKeyAsync` receives only the full hierarchy value `\\mailbox@example.com\Archive\Clients\North`.
- Direct row selection, ancestor activation, and immediate-child activation emit only the archive-relative filing target `Clients\North`.
- `EfcDataModel` and `EmailFilerConfig.DestinationOlStem` retain the archive-relative filing target.
- `EmailFilerConfig.ResolvePaths` remains the sole authority that prefixes the archive root once.

## Required regression behaviors

1. Direct row selection uses the full path only for provider lookup and returns the relative filing target.
2. Ancestor activation returns an archive-relative filing target.
3. Immediate-child activation returns an archive-relative filing target.
4. `EmailFilerConfig.ResolvePaths` builds one archive-root prefix and the existing save-path mapping for an `@` mailbox root.

## Planned file scope

- Permitted test files: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` and `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs`.
- Conditional production file: `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, and only if the router regression demonstrates an incorrect conversion.
- Prohibited changes: `EfcDataModel`, `EfcHomeController`, search generation, Outlook COM integration, persistence, filesystem APIs, and a full-stem normalizer.
- Prohibited substitutions: do not parse `@` as a mailbox delimiter and do not substitute `Store.FilePath` for the Outlook hierarchy `FolderPath`.

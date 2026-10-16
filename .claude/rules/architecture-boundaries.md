---
paths:
  - "**/*.ts"
  - "**/*.cs"
description: Architecture boundary enforcement rules for a host-neutral layered architecture.
---

# Architecture Boundaries

Architecture boundary enforcement is a uniform gate across all tiers (T1–T4). Violations block PRs.

## Enforcement Tools

- **TypeScript:** `dependency-cruiser`. Configuration file pattern: `.dependency-cruiser.cjs`.
- **.NET (when the backend exists):** `NetArchTest.Rules`. Test project naming pattern: `*.ArchitectureTests`.

## Host-Neutral Architecture Rules (enforceable assertions)

Production code in this repository must satisfy each of the following assertions. Each assertion is enforced by `dependency-cruiser` (TypeScript) or `NetArchTest.Rules` (.NET) where applicable; legacy import utilities, when added, must satisfy the same assertions.

1. New runtime code must not reference VSTO APIs (`Microsoft.Office.Tools.*`).
2. New runtime code must not reference Outlook desktop automation APIs (`Microsoft.Office.Interop.Outlook`).
3. New runtime code must not expose COM-visible interfaces (`[ComVisible(true)]` attribute is banned in production code).
4. New runtime code must not use Ribbon extensibility callbacks tied to the desktop object model.
5. New runtime code must not depend on local Outlook event streams.
6. New runtime code must not depend on Outlook user-defined fields as the primary state store.
7. Mailbox data must be accessed only through Office.js or Microsoft Graph.
8. Business behavior must be implemented in the backend or in host-neutral domain or application modules.
9. Client UI must be implemented as web UI.
10. Legacy integration, when required, must be limited to offline data import from files or exported data.

## Layer Boundary Assertions (TypeScript)

- `src/taskpane/` and `src/commands/` must not import from backend internals.
- Domain modules must not import from Office.js, Microsoft Graph SDK, or any infrastructure adapter.
- Adapters may import from domain; domain must not import from adapters.

## Layer Boundary Assertions (.NET, applies once the backend exists)

- `<Product>.Domain` must have zero references to Outlook PIA, VSTO, or Office.js types.
- `<Product>.Application` may depend on `<Product>.Domain` only.
- Adapter projects may depend on `<Product>.Domain` and `<Product>.Application`; domain may not depend on adapters.

## Enforcement Outcome

Violations of any rule above are PR-blocking findings. CI runs the architecture-boundary stage on every PR; a non-zero violation count fails the stage and prevents merge.

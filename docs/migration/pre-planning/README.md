# TaskMaster migration pre-planning

This folder contains the working migration plan for replacing the legacy TaskMaster VSTO add-in with a supported Outlook web add-in, a local-first companion PWA, and a modern backend.

## Architectural decision in one sentence

> The Outlook add-in is the contextual online integration; the installed companion PWA is the full local-first and offline TaskMaster client; the TaskMaster API and Microsoft Graph apply authoritative mailbox changes and synchronize every client.

The PWA can maintain and modify its own scoped mailbox projection while offline. It cannot directly rewrite Outlook's private native offline cache through supported Office.js or Graph APIs. After reconnection, queued operations are applied to Exchange and Outlook converges through its normal mailbox synchronization.

## Required execution order

1. Complete and release the Step 1 and Step 2 reusable capabilities in `drm-copilot`.
2. Push the released customizations into TaskMaster and TMW on separate adoption branches.
3. Reconcile repository-local rules and merge the adoption pull requests.
4. Run the TaskMaster Step 1 discovery prompt.
5. Complete required human characterization and product decisions.
6. Merge and pin the TaskMaster discovery baseline.
7. Run the TMW Step 1 parity prompt against that pinned TaskMaster baseline.
8. Merge the TMW parity matrix and Step 2 dependency catalog.
9. Run the TaskMaster Step 2 oracle prompt.
10. Run the TMW Step 2 platform-foundation prompt after its prerequisite gates pass.
11. Do not begin later migration waves until the Step 2 integrated vertical-slice gate passes.

## Document order

### Top-level decision

1. [TaskMaster Migration to a Modern Supported Architecture](TaskMaster%20Migration%20to%20a%20Modern%20Supported%20Architecture.md)

### Step 1 — Discovery and parity definition

1. [Overview](step%2001/01%20overview.md)
2. [Planned division of responsibilities](step%2001/02%20planned-division-of-responsibilities.md)
3. [`drm-copilot` discovery tooling prompt](step%2001/03%20drm-copilot-tooling-prompt.md)
4. [Remaining prompts and sequencing](step%2001/04%20remaining-prompts-and-sequencing.md)
5. [TaskMaster Step 1 prompt](step%2001/04a%20TaskMaster-prompt.md)
6. [TMW Step 1 prompt](step%2001/04b%20TMW-prompt.md)

### Step 2 — Platform foundation

1. [Overview, reusable tooling, and ownership](step%2002/01%20overview-and-tooling.md)
2. [Detailed repository work, architecture, and sequencing](step%2002/02%20repository-work-architecture-and-sequencing.md)
3. [`drm-copilot` platform-foundation tooling prompt](step%2002/03%20drm-copilot-platform-foundation-prompt.md)
4. [TaskMaster Step 2 oracle prompt](step%2002/04a%20TaskMaster-prompt.md)
5. [TMW Step 2 platform-foundation prompt](step%2002/04b%20TMW-prompt.md)

## Repository ownership summary

| Repository | Owns |
|---|---|
| `drm-copilot` | Generic agents, skills, schemas, validators, hooks, templates, analyzers, CLI/MCP/VS Code wrappers, publishing, and reusable completion gates |
| `TaskMaster` | Legacy behavior truth, runtime evidence, feature contracts, characterization fixtures, source baseline, and Step 2 oracle bundles |
| `TMW` | Target architecture, Outlook add-in, companion PWA, API, local store, sync, telemetry, feature flags, deployment, parity status, and target tests |

## Non-negotiable planning rules

- Do not run the authoritative TaskMaster or TMW prompts against unreleased `drm-copilot` tooling.
- Do not push customizations directly to `main`.
- Do not overwrite repository-local rules without reviewing the adoption diff.
- Do not use floating source branches for parity or oracle references; pin commit SHAs and checksums.
- Do not mark an Outlook Mobile shell, an API scaffold, or a queued command as feature parity.
- Do not claim that the Outlook add-in itself provides offline behavior in new Outlook.
- Do not weaken the PWA into a queue-only companion.
- Do not claim the PWA can directly mutate Outlook's private native cache.
- Do not begin broad feature migration until the Step 2 vertical slice proves online, offline, restart, reconnect, mobile, observability, feature-flag, and rollback behavior.

## Current Microsoft platform references

- [Develop Outlook add-ins for the new Outlook on Windows](https://learn.microsoft.com/en-us/office/dev/add-ins/outlook/one-outlook)
- [Add-ins for Outlook on mobile devices](https://learn.microsoft.com/en-us/office/dev/add-ins/outlook/outlook-mobile-addins)
- [Add support for add-in commands in Outlook on mobile devices](https://learn.microsoft.com/en-us/office/dev/add-ins/outlook/add-mobile-support)
- [Store data on the device in a PWA](https://learn.microsoft.com/en-us/microsoft-edge/progressive-web-apps/how-to/offline)
- [Get incremental message changes with Microsoft Graph delta](https://learn.microsoft.com/en-us/graph/delta-query-messages)

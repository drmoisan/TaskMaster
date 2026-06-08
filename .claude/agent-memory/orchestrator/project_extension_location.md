---
name: VS Code extension location in this repo
description: The publishable VS Code extension lives at extensions/drm-copilot/, not at the repo root.
type: project
---

The publishable VS Code extension lives at `extensions/drm-copilot/`, not at the repo root.

**Why:** This repo is a workspace orchestrator that contains multiple parallel toolchains (Python, PowerShell, TypeScript) plus a Claude/Copilot agent runtime. The VS Code extension is one component among several. Its source is at `extensions/drm-copilot/src/`, its build output is at `extensions/drm-copilot/out/`, its manifest is `extensions/drm-copilot/package.json`, and shipping `.vsix` artifacts go to `artifacts/vsix/` with timestamped filenames. Confirmed 2026-05-02 by inspecting the most recent shipping `.vsix` (`drm-copilot-20260501-114515.vsix`), whose `extension.vsixmanifest` description reads "Extension-side bundled workflow execution utilities and MCP bridge."

**How to apply:** All `vsce` commands (`ls`, `package`, `publish`) must run from `extensions/drm-copilot/`. The repo root `package.json` is not a publishable extension and should not declare `main`, `activationEvents`, or be passed to `vsce`. When the user asks about extension packaging, install paths, the manifest, or the `.vsix`, default to inspecting `extensions/drm-copilot/` first. Verify the location is still current before acting (the layout could change).

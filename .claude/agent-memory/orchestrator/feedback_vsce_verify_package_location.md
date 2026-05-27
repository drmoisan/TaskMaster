---
name: Verify which package.json is the VS Code extension before any vsce work
description: In multi-package repos, never assume the repo root is the publishable extension. Verify location before vsce package/publish/.vscodeignore work.
type: feedback
---

Before doing any `vsce` packaging, publishing, or `.vscodeignore` work, verify which directory contains the actual VS Code extension. Do not assume the repo root.

**Why:** On 2026-05-02, I spent multiple troubleshooting rounds (`.vscodeignore` rewrites, allow-list strategy, esbuild bundling) treating the repo root `package.json` as the publishable extension. The real extension lives at `extensions/drm-copilot/` with its own `package.json`, `src/`, `tsconfig.json`, `node_modules/`, and existing build pipeline. The user had been side-loading shipping `.vsix` builds for ~4 months from the correct location. The root `package.json` is a workspace orchestrator, not a publishable extension. My misdiagnosis caused the user to push back: "I have been side-loading this extension for a significant period of time and it functions properly." All the changes I made to the root (main, activationEvents, esbuild.config.cjs, bundle script, root .vscodeignore) were applied to the wrong package and produced no value for the actual extension.

**How to apply:** When the user mentions `vsce`, packaging, publishing, `.vscodeignore`, extension manifests, or "the extension":

1. First, locate every `package.json` in the workspace that declares `engines.vscode` or `contributes` or has `@types/vscode` as a dep. Use `Grep` for `"engines"` or `"contributes"` across all `package.json` files.
2. If multiple candidates exist, identify the one with `main`, `activationEvents`, and `contributes` populated — that is the real extension.
3. If shipping `.vsix` artifacts exist (e.g., under `artifacts/vsix/`), inspect the most recent one with `unzip -l` and read the included `extension/package.json` to see what is actually being shipped. The directory whose source matches the shipped `out/` files is the real extension.
4. Run `vsce ls` only from that directory.
5. State explicitly which directory you identified before making any edits.

This rule applies even when the user opens a root `package.json` in the IDE. The `ide_opened_file` hint indicates focus, not authority over which package is the extension.

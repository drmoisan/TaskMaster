---
name: csharpierignore-scope-packages-config
description: `.csharpierignore` excludes *.csproj/*.props/*.targets but NOT packages.config — csharpier does reflow packages.config, so never justify a single-line entry with "csharpier formats only *.cs"
metadata:
  type: project
---

`.csharpierignore` at repo root excludes `**/evidence/**`, coverage/trx artifacts, and `*.csproj`, `*.props`, `*.targets` — it does **not** exclude `packages.config`. CSharpier does reflow `packages.config`, and most `packages.config` files in this repo already show it: long entries are broken across four lines while short ones stay on one line.

**Why:** Authoring the #418 cycle-2 remediation plan, I justified a new single-line `<package id="ExCSS" ... />` entry with the clause "csharpier formats only `*.cs` and will not reflow this file". That is false and preflight blocked the plan for it. The real protection is **width**: single-line entries survive to at least 98 characters (`System.Diagnostics.DiagnosticSource` in `SVGControl.Test/packages.config`), and the new entry was 62. The false version was worse than a harmless slip — it would have let a later reader conclude `packages.config` is formatter-exempt, which it is not.

**How to apply:** When a plan task adds or edits a `packages.config` line and asserts an exact diff shape (for example "exactly one added line"), justify the expected form by character width against a measured in-file precedent, never by formatter exemption. Add a fallback clause: if the format stage reflows it anyway, the reflowed form is correct and the acceptance is re-evaluated post-format. Only `*.csproj`/`*.props`/`*.targets` may be called formatter-exempt. Related: [[csharpier-format-not-pipe-files-gate]] (formatting gates must mutate then assert exit 0), [[project_legacy_csproj_explicit_compile_include]] (the paired csproj/packages.config wiring these edits usually come in).

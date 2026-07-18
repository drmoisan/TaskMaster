---
name: svgcontrol-stale-binding-redirect-out-of-scope
description: SVGControl/app.config carries one real, uncorrected stale bindingRedirect (System.Runtime.CompilerServices.Unsafe 6.0.2.0 vs csproj 6.0.3.0) that any future "audit every app.config" AC should treat as a known, pre-existing, out-of-scope condition
metadata:
  type: project
---

Discovered during issue #354 (stale-app-config-binding-redirects) review, 2026-07-18: a standalone comparator cross-checking every project's `.csproj` `<Reference Version=...>` against its `app.config` `<bindingRedirect>` found exactly one real mismatch outside that issue's 57-item fix inventory — `SVGControl/app.config`'s `System.Runtime.CompilerServices.Unsafe` redirect caps at `newVersion="6.0.2.0"` while `SVGControl.csproj` references `Version=6.0.3.0` (package `System.Runtime.CompilerServices.Unsafe.6.1.2`).

**Why this matters:** `SVGControl`/`SVGControl.Test` are already established (per [[project_csharp-repowide-coverage-below-80]] and cross-session `csharp-analyzer-packages-config-quirks.md`/`project_repo_sdk_and_nullable_rebuild.md` memory in other agents' memory stores) as vendored/exempt from this repo's analyzer and nullable build gates. Issue #354's own fix script hardcodes `EXCLUDE_PROJECTS = {"SVGControl", "SVGControl.Test"}` and `issue.md`'s Suspected-Cause project inventory never names them either — consistent, but AC1's literal text ("every first-party project's app.config") does not itself state this carve-out, so a future strict re-audit of AC1 will surface this same gap again unless it is fixed or the AC wording is formally narrowed.

**How to apply:** If a future feature/bug touches `app.config` binding redirects again (broad audit, dependency bump remediation, etc.), check `SVGControl/app.config`'s `System.Runtime.CompilerServices.Unsafe` entry specifically — it was still stale as of 2026-07-18. Don't assume a clean audit script run against the non-vendored project set means the repo has zero remaining stale redirects.

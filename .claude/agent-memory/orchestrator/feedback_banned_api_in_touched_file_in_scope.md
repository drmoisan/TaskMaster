---
name: banned-api-in-touched-file-in-scope
description: When a fix modifies a production file, any banned API encountered in that file is in scope to remediate — do not defer
metadata:
  type: feedback
---

When a change touches a production file, any banned API encountered in that file must be remediated within the same change, not left for later. Banned APIs in this repo: `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay` (enforced by BannedApiAnalyzers / RS0030, currently `suggestion` severity so they do not break the build — but the maintainer still wants them fixed when the file is in play).

**Why:** On #207 the maintainer directed that the pre-existing `Task.Delay(100)` in `ProcessNewInboxItemsAsync` be fixed as part of the STA refactor, and stated the general rule: "Any banned API in a production file that is encountered in this fix should be in scope." This mirrors the maintainer's stance that quality-gate violations in files you are modifying are not out of scope. See [[whole-repo-CI-gate-not-out-of-scope]] and [[vsto-startup-sta-threading-directive]].

**How to apply:** When scoping/executing a change, scan each modified production file for banned APIs and remediate them with compliant equivalents (e.g., replace `Task.Delay`/`Thread.Sleep` on the STA with a non-blocking `DispatcherTimer`-based pumping delay; replace `DateTime.Now`/`UtcNow` with an injected `TimeProvider`). Keep the remediation within the touched file's scope; do not expand to untouched files unless separately directed.

---
name: webview2-host-476-family-shipped-issues-left-open
description: The webview2-host-initializer-defects-476 feature delivered #458 and #477 alongside #476 on main with all 37 ACs checked, but #458 remains OPEN — the third confirmed instance of the multi-issue-feature/orphaned-sibling pattern
metadata:
  type: project
---

`docs/features/active/webview2-host-initializer-defects-476/` delivered THREE issues in one
feature: #476 (its own), plus #458 and #477. Its `spec.md` line 3 states the relationship
outright — `**Issue:** #476 (also closes #458, #477)` — and its AC table carries 37 of 37
criteria checked, zero unchecked. Issue #458 is nonetheless still OPEN as of 2026-08-31.

The delivering commit is `b1dec0c2 fix(webview2-host-476): correct host lifecycle, marshalling
and seam contract`, on `main`. Its body opens the `WebView2BreadcrumbHost.cs` section with a
literal `#458:` bullet. Neither a `fix(458)` grep nor a WebView2BreadcrumbHost subject grep is
sufficient on its own — the bare-number grep is what finds it, exactly as
[[verify-delivery-before-preparing-an-admission]] predicts for a sibling-scoped subject.

The #458 fix on `main` is a per-control `ConditionalWeakTable<WebView2, WebView2BreadcrumbHost>`
owner registry plus a `_ownersGate` lock, with the constructor doing lookup-detach-replace and a
`_control.Disposed` handler evicting the entry. The dead constructor-side `-=` the issue describes
is gone, and the surviving comment explains why it could never have worked (delegate equality is
pairwise over target and method, so a delegate formed in a constructor is bound to the instance
under construction). ACs 946-950 cover exactly the five behaviours issue #458 asks for, all `[x]`.

**Why:** This is the THIRD confirmed family of the same pattern, after
[[qfc-collection-468-family-shipped-issues-left-open]] (#286/471/473/474) and
[[efc-464-family-shipped-issues-left-open]] (#461/463/465/466/467). Three independent families
makes the multi-issue feature folder that closes only its own issue a structural property of this
repository's workflow, not an accident. `/parallel-add 458` was rejected on 2026-08-31 without
delegating any preparation.

**How to apply:** Treat any OPEN issue in the WebView2/breadcrumb-host area as presumptively
delivered until the bare-number grep and the 476 spec's AC table say otherwise. #477 is the other
sibling this feature closed; check its issue state before ever admitting it. More generally, when
a candidate's subject area matches a known multi-issue feature, go to that feature's `spec.md` AC
table first — it maps each sibling's defects to numbered ACs and settles residual scope in one
command.

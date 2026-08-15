---
name: single-numeral-gates-must-name-the-role
description: "\"Exactly one coverage numeral\" gates must name the role (enforced repo-wide line floor) and enumerate the doc/policy-constant occurrences, or they contradict the task that writes the docs"
metadata:
  type: feedback
---

A gate worded "exactly one coverage numeral exists across these files" is self-contradicting whenever another
task in the same plan deliberately writes a second numeral. In #494 P3-T13 claimed a single permitted
documentation occurrence while P3-T7 required the hook's `.SYNOPSIS` to state **two** numerals (repo-wide 80 and
new-code 90), and the same hashtable held `NewCodeFloorPercent = 90.0` alongside the enforced
`LineFloorPercent`.

**Why:** the counted thing has to be a *role*, not a character sequence. Documentation prose, policy constants,
and the value actually compared against in executable code are three different populations.

**How to apply:** phrase the gate as "exactly one **enforced repository-wide line-floor** numeral is compared
against in executable code, and it is `<named symbol>`", then enumerate every other occurrence individually with
its classification (policy constant, documentation, deferred-to-follow-up). Same discipline as
[[zero-hit-grep-gates-need-carveouts]]. Related: an authority-equality assertion must anchor on a unique regex
capture (e.g. exactly one line matching ``Repository-wide line coverage must remain `>= (\d+)%` ``) — asserting
equality against "the numeral stated in § UT2" is trivially passable when that section holds 80, 90, `<= 1.0`,
`<= 0.5%` and `0%`.

Corollary for numeral *inventories*: a pattern built only from `>= 85%` / `85.0` forms cannot see prose sites
("below 80 percent", "repo-wide below 80, new-file below 90"), so a completeness AC bounded by that inventory
certifies against a search that structurally cannot fail. Include prose alternations and require each hit to be
classified `numeric-literal` or `prose`.

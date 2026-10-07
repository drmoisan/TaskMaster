---
name: project-947-passa-second-call-site-consolidation-seams
description: Issue 947 pass A - folding a maintainer-consolidated second call site into a half-written plan; nested catch arm end must use the header's own indentation; untracked new partial needs content-scan and ?? porcelain gates
metadata:
  type: project
---

Issue 947 (throwing logError sink, EngineToggleStateCoordinator) had a second call site (HandleToggleClickAsync catch arm) consolidated into scope by a maintainer comment on 2026-10-01, mid-planning. Pass A added AC6/AC7 to issue.md (appended, AC1-AC5 ordinals kept), a `## Scope Consolidation` section, windows E5 (remarks) and E6 (catch clause), and a fourth test in the ThrowingSink partial.

**Why:** a nested `catch (Exception)` inside an outer `catch (Exception ex)` breaks any arm-end rule keyed to a fixed indentation ("first line of twelve spaces and `}`") - it would run to the OUTER brace. The arm end must be the first following line equal to the header line's own leading spaces plus `}`. Also, adding a second guarded catch invalidates doc tokens like "one of the two catch clauses" written for a single-site fix - every count/token and E1/E3 doc text had to be re-derived (3 catch clauses total).

**How to apply:** when a scope addition lands on a partially-authored plan, sweep: AC count statements, identity table, must-not-touch paragraph (it had excluded the very method now in scope), handoff tasks that promoted the now-in-scope item, NAMES lists and BASELINE-TOTAL plus N, coverage arm measurement (generalise to every arm with owner method and an ARM-COUNT negative control). To delete a long stale note line, replace it in unique chunks via a sentinel token, then remove the sentinel. See [[project-940-r3-per-file-coverage-rule-and-post-merge-reanchor-seams]] for the untracked-file gate family.

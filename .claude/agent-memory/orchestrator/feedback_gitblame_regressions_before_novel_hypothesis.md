---
name: feedback-gitblame-regressions-before-novel-hypothesis
description: For a "was working, now broken" symptom, git-blame/bisect the responsible change before hypothesizing a novel runtime mechanism; and re-confirm the exact symptom before diagnosing
metadata:
  type: feedback
---

For a defect described as "this stopped working / this is now wrong," locate the regression in history FIRST — `git log`/`git blame` the specific values or lines, and read the responsible commit — before constructing a novel runtime-mechanism hypothesis.

**Why:** On issue #269 (QuickFiler Sender/Subject label miscoloring) two full delegated cycles were spent on an elaborate, plausible, code-cited hypothesis (a `NullReferenceException` probe-abort in `Theme.SetQfcTheme()` + WinForms ambient-inheritance) that shipped a reviewed-PASS fix which "did not work." The actual cause was a mechanical regression: commit `44bfdf204` (issue #236 "coverage seams" refactor) converted `QfcThemeHelper` theme definitions from named to positional `CreateTheme(...)` args and transposed foreground/background for the Light themes only. A `git blame` of the color lines pointed straight at it in one step, and the correct values were literally sitting in `44bfdf204~1`.

**Also:** the user's symptom description evolved across turns ("not changing light->dark" → "first two items blue" → "only in Light mode, dark is correct"). Do not lock a diagnosis to an early, imprecise symptom. Pin down the exact observable (which mode, which control, expected vs actual) before choosing a mechanism. When a delivered fix "did not work," treat the prior mechanism as refuted and restart from evidence, not from a variation of the same theory.

**How to apply:** when work targets code that recently changed (especially a large refactor commit) and the symptom is a wrongness rather than a crash, run `git blame`/`git log -L` on the exact miscolored/mis-valued lines and diff the responsible commit against its parent. Prefer restoring proven prior-correct values over inventing a new mechanism. Demonstrate red-before-green against a real build so the diagnosis is proven, not assumed. See [[feedback_verify_repro_before_bugfix_cycle]].

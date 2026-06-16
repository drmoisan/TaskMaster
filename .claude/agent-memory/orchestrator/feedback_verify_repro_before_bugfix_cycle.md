---
name: verify-repro-before-bugfix-cycle
description: Before opening a bugfix remediation cycle for a latent/worked-around defect, get a ground-truth reproducibility check on HEAD
metadata:
  type: feedback
---

Before committing a bugfix remediation cycle (atomic-planner -> atomic-executor red-before-green) to a defect that was "surfaced but worked around" in an earlier cycle, require a ground-truth reproducibility check on HEAD first — do not trust a scoping-research "root cause" alone.

**Why:** On #177 cycle 4, a scoping research artifact confidently described a reproducible `FilePathHelper` deserialize NRE and recommended a 3-line null-guard + RED test. The atomic-executor then could not make the test RED on HEAD (5 document orderings all passed), and a deeper code-trace researcher confirmed NOT REPRODUCIBLE: the path self-heals (`StemInitialized()`/`TryParseFileName` populates the field before the deref) AND the prior cycle's correct workaround (`DoNotSerializeContractResolver("Config")`) already removes the only trigger structurally. A null-guard would have been an unfalsifiable test that protects nothing, and red-before-green was impossible. The first research was simply wrong.

**How to apply:** When a user asks to "fix" a previously-worked-around or latent defect, before authoring remediation-inputs/AC and entering the cycle, run a focused ground-truth probe (task-researcher tracing the ACTUAL production path, or a throwaway repro) that either captures a real failing case with a stack trace or proves non-reachability. If NOT REPRODUCIBLE: stop, report to the user that the premise changed, and offer no-change vs explicit defensive-hardening (with an honest non-regression test, not a fake RED) rather than proceeding. A correct already-applied workaround can make the "defect" unreachable. Relates to [[migration-not-just-patch]].

---
name: msbuild-parallel-log-node-prefix-defeats-anchored-target-counts
description: Under msbuild /m the log prefixes some target lines with "N>" so a Select-String '^CoreCompile:' non-vacuity counter returns 0 on a build that did compile; count unanchored and pair with csc.exe lines plus the test-dll mtime
metadata:
  type: project
---

Under `msbuild ... /m`, the console log prefixes target-entry lines from secondary nodes
with the node id (`7>CoreCompile:`, `15>CoreCompile:`), while the primary node prints them
unprefixed. An anchored `Select-String -Pattern '^CoreCompile:'` therefore under-counts,
and on a run where every compile happened on a secondary node it prints `0`: a false zero
on the very counter that exists to prove the build was not vacuous.

**Why:** observed on issue #792 [P3-T7] (2026-09-17). The anchored count printed 0 while
the same log held 19 `CoreCompile:` lines, 2 `csc.exe` lines naming `QuickFiler.Test`, and
the test dll had been rewritten. Had the anchored count been the only counter, the run
would have looked like the vacuous "Build succeeded" from Phase 2 (the `$args` shadowing
incident) and would have triggered a false halt.

**How to apply:**
- Count `CoreCompile:` unanchored (or with `^\s*(\d+>)?CoreCompile:`).
- Keep three independent non-vacuity signals: target count, `csc\.exe` lines naming the
  project you changed, and the output assembly's LastWriteTime before/after.
- Treat a zero from any one of them as "check the pattern" before "the build was vacuous".

Related: [[project_pwsh_function_param_named_args_makes_msbuild_gate_vacuous]],
[[project_msbuild_log_token_search_matches_csc_command_line]],
[[project_incremental_build_vacuous_baseline]].

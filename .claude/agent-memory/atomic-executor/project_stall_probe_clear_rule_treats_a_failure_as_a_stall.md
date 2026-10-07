---
name: stall-probe-clear-rule-treats-a-failure-as-a-stall
description: A coverage-route stall probe whose CLEAR rule requires exit 0 and failed=0 selects COVERAGE-ROUTE DIRECT on a shell-icon test that FAILS fast (not hangs), which makes a "runner verbatim" AC unreachable by construction
metadata:
  type: project
---

On issue 950 (2026-10-02) the plan's P0-T15 stall probe ran the four UtilitiesCS.Test shell-icon classes (ShellUtilities_Tests, ShellUtilitiesStatic_Tests, SysImageListHelperTests, OSBrowser_Tests) and its rule was CLEAR only when EXIT_CODE 0, failed=0 and no Sequence file. The run did not stall: it finished in under a minute with 22/23 passed and one fast failure, `ShellUtilitiesStatic_Tests.GetFileIcon_WithUseFileType_...` ("Win32 handle that was passed to Icon is not valid or is the wrong type"). The rule still read REPRODUCES, so the route became DIRECT, and AC17 ("Invoke-MSTestWithCoverage ... in a single pass") ended NOT MET (ENVIRONMENTAL) even though every gate was green.

**Why:** the probe conflates "this workstation's shell-icon tests are unhealthy" with "they stall the runner". On this host they fail deterministically, which would also make the verbatim runner exit non-zero, so DIRECT was arguably still the right route, but the AC was unreachable from the moment the probe ran.

**How to apply:** at preflight, flag a stall-probe rule that cannot distinguish a hang from a fast failure when an AC requires the runner verbatim; at execution, follow the rule mechanically, record the exact failing test and its message, and surface the AC as a coordinator ruling rather than working around it. Related: [[project_local_shell_icon_tests_hang_shgetfileinfo]] (session memory: the same four classes stall on this machine at other times).

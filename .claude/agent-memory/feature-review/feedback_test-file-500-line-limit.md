---
name: test-file-500-line-limit
description: The repo 500-line file-size limit applies to test code; a regression-test addition can silently cross it, which is a FAIL-level policy finding
metadata:
  type: feedback
---

When reviewing C# bugfix/feature branches, always compare the head line count of every changed test file against its baseline line count, not just production files.

**Why:** The repo General Code Change Policy "File Size Limit" (500 lines) explicitly covers production code, test code, AND reusable scripts — the only exceptions are throwaway scripts, raw text fixtures, and Markdown. Test code is NOT excepted. A small regression-test addition can push a previously-compliant test fixture over 500 lines (observed on issue #183: `Triage_OlLogicTests.cs` 469 -> 553). This is a Section 2.3 / Section 4 FAIL-level finding even when all ACs pass and the toolchain is green, because the change is the proximate cause of the breach.

**How to apply:** For each changed test file, run `git show <merge-base>:<path> | awk 'END{print NR}'` (baseline) and `awk 'END{print NR}' <path>` (head). If head > 500 and the change crossed the limit, record a Major/FAIL finding in policy-audit (Module & File Structure) and code-review (Findings Table), drive overall policy verdict to PARTIALLY COMPLIANT, and emit remediation-inputs recommending a fixture split (partial class or separate file) with no test weakening. See [[powershell-measure-object-line-undercount]] for the correct line-counting command (use awk NR / wc -l, not Measure-Object -Line).

**Second confirmed instance (#177 cycle-1 remediation):** a coverage-raising remediation cycle (the very work meant to close a Minor finding) pushed the NEW test file `LcppnFolderPredictor_Tests.cs` 418 -> 554 lines. Two lessons: (1) when a NEW file is over cap it is a clean FAIL (no pre-existing-overage excuse, unlike a modified file); (2) check this on remediation cycles too — the cycle that fixes coverage is a prime suspect for crossing the cap, and AC source files like AC20 ("no new test file > 500 lines") will have a stale `[x]` that the reviewer must flag without editing the AC text.

**Crossing vs pre-existing-overage severity split (#324):** distinguish two cases when a changed file ends over 500. (a) The change CROSSES the cap (baseline <=500, head >500) = FAIL/blocking, per the instances above. (b) The file was ALREADY over 500 at baseline and the additive change only EXTENDED it (#324: FolderPredictor.cs 823->974, FolderScorer.cs 617->663) = Major NON-blocking, recorded as PARTIAL on the file-size row with a partial-class-extraction recommendation, not a blocking finding. Rationale for (b): the breach pre-dates the branch, the added members are cohesive instance methods over private state, and refactoring a pre-existing oversized class is out of an additive feature's scope and can endanger a byte-for-byte backward-compat guarantee. Still document it prominently; do not silently pass it.

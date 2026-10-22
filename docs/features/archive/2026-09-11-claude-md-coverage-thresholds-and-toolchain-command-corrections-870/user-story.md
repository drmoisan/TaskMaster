# User Story — CLAUDE.md coverage thresholds and toolchain command corrections (Issue #870)

This is a full-bug item; spec.md is the sole acceptance-criteria source, and this file contains no acceptance-criteria section or checkbox list.

## Actor

Any contributor or reviewing agent who reads the repository-root CLAUDE.md as the authoritative statement of the C# toolchain commands and coverage thresholds for this repository.

## Need

The contributor needs CLAUDE.md to name the coverage command actually run in this repository, to state the coverage thresholds the maintainer has settled, and to cite only analyzer-configuration files that exist. Today the file names a bare vstest invocation carrying the built-in coverage-collector switch that the real coverage script deliberately never uses, states an undifferentiated 80 percent coverage figure that omits the settled branch and PowerShell figures, and cites a nonexistent analyzer-configuration file twice alongside the real one.

## Value

A contributor who follows CLAUDE.md's stated toolchain command reaches a command that is not the one the repository's own coverage script runs, wasting time reconciling the discrepancy. A reviewing agent citing the undifferentiated coverage figure against the maintainer's settled decision produces false verdicts on later reviews. Correcting all three defects in this one file removes a recurring source of review friction and brings the repository's own instructions into agreement with the route, figures, and files that are actually in effect.

## Narrative

CLAUDE.md is corrected at three sites, all within the same file. The two toolchain step-4 entries are rewritten to name the PowerShell coverage script and its wrapping VS Code task, with a note that the built-in coverage-collector switch is intentionally withheld from the inner test invocation. The coverage-and-scenarios block is rewritten to state the four coverage figures the maintainer settled on 2026-09-11 under issue 563, while leaving the existing exemption classes untouched. The two analyzer-severity citations are corrected by removing the name of a configuration file that does not exist in this repository, leaving the file that does exist as the sole named source at each site. No source, build, or test file is touched by this change.

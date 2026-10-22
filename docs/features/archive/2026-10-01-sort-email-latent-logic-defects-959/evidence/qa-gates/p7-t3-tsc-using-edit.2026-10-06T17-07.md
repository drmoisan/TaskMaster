# P7-T3 CR-3 Unused Using Directive (Edit E-TSC-USING)

Timestamp: 2026-10-06T17-07
Command: Edit tool, E-TSC-USING on UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs (edit branch: P7-T1 recorded TSC-SYSTEM-IDENTIFIER-LINES: 0; OLD: TSC lines 1 to 2, found once; NEW: the second line only); then the Grep tool over the same file: `^using System;`, `^using System\.Collections\.Generic;`, `^`; then a Read of TSC lines 1 to 3
EXIT_CODE: 0 (a Grep-tool task; the row names the third search, `^`)
ITERATION: 1
Output Summary: edit branch applied; the unused `using System;` directive is gone, `using System.Collections.Generic;` is now the first line and the file has 342 lines.

- TSC-USING-SYSTEM-LINES: 0 (1 before the Edit)
- TSC-USING-GENERIC-LINES: 1
- TSC-LINES: 342
- TSC-HEAD:
  - 1: using System.Collections.Generic;
  - 2: using System.Threading.Tasks;
  - 3: using FluentAssertions;

## Acceptance (P7-T3, all three required under the edit branch)

1. TSC-USING-SYSTEM-LINES: 0: met.
2. TSC-USING-GENERIC-LINES: 1 with TSC-HEAD beginning `using System.Collections.Generic;`: met.
3. TSC-LINES: 342: met.

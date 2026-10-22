Timestamp: 2026-10-02T05-11
Command: Grep `scripts/hygiene` (count) in artifacts/pester/powershell-coverage.xml; Grep `<package ` (count) in the same file; Grep `<counter type="LINE"` (content) in the same file
EXIT_CODE: 0
Output Summary: `scripts/hygiene` match count = 0. `<package ` match count = 13 (control, above 0). Last matching `<counter type="LINE"` line (line 1278, informational and not a scripts/hygiene figure): missed="31" covered="0". Coverage of scripts/hygiene is measured by CI in _pester.yml (LINE at 80) and not by this document. No coverage-percentage acceptance condition is set for scripts/hygiene locally.

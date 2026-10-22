# Remediation cycle 1, P1-T3: Azure.Core deployed-version and TaskMaster presence probes (AC17 observation iv)

Timestamp: 2026-10-06T20-32
Command: pwsh -NoProfile -Command '$w = "<execution-worktree-root>"; foreach ($p in @("UtilitiesCS", "UtilitiesCS.Test", "TaskMaster.Test")) { "VER " + $p + " Azure.Core.dll=" + [System.Reflection.AssemblyName]::GetAssemblyName($w + "/" + $p + "/bin/Debug/Azure.Core.dll").Version.ToString() }; "VER UtilitiesCS Microsoft.Kiota.Authentication.Azure.dll=" + [System.Reflection.AssemblyName]::GetAssemblyName($w + "/UtilitiesCS/bin/Debug/Microsoft.Kiota.Authentication.Azure.dll").Version.ToString(); foreach ($n in @("Azure.Core.dll", "Microsoft.Kiota.Authentication.Azure.dll", "System.Linq.AsyncEnumerable.dll", "UtilitiesCS.dll", "TaskMaster.dll")) { "PRESENT TaskMaster " + $n + "=" + (Test-Path -LiteralPath ($w + "/TaskMaster/bin/Debug/" + $n)) }'
EXIT_CODE: 0

Printed lines (verbatim):
VER UtilitiesCS Azure.Core.dll=1.63.0.0
VER UtilitiesCS.Test Azure.Core.dll=1.63.0.0
VER TaskMaster.Test Azure.Core.dll=1.63.0.0
VER UtilitiesCS Microsoft.Kiota.Authentication.Azure.dll=2.1.2.0
PRESENT TaskMaster Azure.Core.dll=False
PRESENT TaskMaster Microsoft.Kiota.Authentication.Azure.dll=False
PRESENT TaskMaster System.Linq.AsyncEnumerable.dll=True
PRESENT TaskMaster UtilitiesCS.dll=True
PRESENT TaskMaster TaskMaster.dll=True

Output Summary:
- pwsh exit 0; nine lines printed.
- Deployed Azure.Core.dll is version 1.63.0.0 in UtilitiesCS, UtilitiesCS.Test and TaskMaster.Test bin\Debug, so the corrected redirect newVersion 1.63.0.0 targets a deployed file.
- Kiota version line recorded, not gated: 2.1.2.0.
- TaskMaster\bin\Debug: Azure.Core.dll False and Microsoft.Kiota.Authentication.Azure.dll False (consistent with P1-T1); System.Linq.AsyncEnumerable.dll, UtilitiesCS.dll and TaskMaster.dll True.
- No PROBE-MISMATCH.

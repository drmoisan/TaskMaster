# Pass-after popup host

Timestamp: 2026-07-21T16-37Z

Build Command: `msbuild QuickFiler.Test/QuickFiler.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU`

Build EXIT_CODE: 0

Filtered Test Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbPopupPlacementTests|FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~BreadcrumbDropDownLifecycleTests"`

Filtered Test EXIT_CODE: 0

- Total: 18
- Passed: 18
- Failed: 0
- Skipped: 0

Acceptance mapping:

- AC-3: eight pure geometry cases verify below-first, above fallback, greater-side selection, below tie, both-axis clamp, negative monitor coordinates, and zero working space.
- AC-4: ownership tests verify an auto-closing `ToolStripDropDown` containing one `ToolStripControlHost`, anchored to the viewer control without a global topmost form.
- AC-8: explicit commit and uncommitted close take separate paths; only uncommitted close invokes rollback.
- AC-13: opening focuses the pending option and all close/failure paths restore anchor focus; the host retains the current theme for surface replay.
- AC-14: the supplied existing environment reaches one lazy surface factory, the popup surface is reused across opens, reset permits a controlled new surface, and full disposal prevents later callbacks.
- AC-15: zero-space placement, invalid partial initialization, failed initialization tasks, repeat open/close, reset, and post-disposal close are deterministic and do not require a live display or sleep.

Compatibility Correction: `Platform=AnyCPU` is the root-authorized mechanical equivalent for the invalid direct-project `Platform='Any CPU'` token. No behavioral scope changed.

Output Summary: All 18 placement, ownership, focus, and lifecycle cases passed without opening a live popup or WebView runtime.

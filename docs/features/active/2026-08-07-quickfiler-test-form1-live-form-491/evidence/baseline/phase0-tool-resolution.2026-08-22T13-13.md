Timestamp: 2026-08-22T13-13
Command: pwsh -NoProfile -Command '$msbuild = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe; $vstest = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.VisualStudio.PackageGroup.TestTools.Core -find Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe; ...'
EXIT_CODE: 0
Output Summary: Both tool paths resolved and confirmed to exist on disk.

MSBuild.exe: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe (exists: True)
vstest.console.exe: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe (exists: True)

Every later MSBuild and vstest task in this plan re-resolves these paths with the same vswhere commands inside the same pwsh session that consumes them, or substitutes these literal absolute paths.

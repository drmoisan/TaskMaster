Timestamp: 2026-08-22T14-12

Command: & 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe; & 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -requires Microsoft.VisualStudio.PackageGroup.TestTools.Core -find Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe

EXIT_CODE: 0

Output Summary:
- Resolved MSBuild.exe: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` (exists: True)
- Resolved vstest.console.exe: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe` (exists: True)
- Both paths are non-empty and both files exist on disk. Every later MSBuild and vstest task in this plan re-resolves or substitutes these literal absolute paths in its own session.

# P3-T7 System.Linq.AsyncEnumerable assembly version (issue #973; AC8)

Timestamp: 2026-10-03T11-28
Command: pwsh -NoProfile -Command '$n = [System.Reflection.AssemblyName]::GetAssemblyName("<execution-worktree-root>\packages\System.Linq.AsyncEnumerable.10.0.12\lib\net462\System.Linq.AsyncEnumerable.dll"); "ASSEMBLY-NAME: " + $n.Name; "ASSEMBLY-VERSION: " + $n.Version.ToString(); "PUBLIC-KEY-TOKEN: " + (($n.GetPublicKeyToken() | ForEach-Object { $_.ToString("x2") }) -join "")'
EXIT_CODE: 0
Output Summary: the restored lib\net462 DLL reports assembly version 10.0.0.12 with public key token b03f5f7f11d50a3a; the printed version equals the expected 10.0.0.12 and is VERSION for every later task.

DLL path: <execution-worktree-root>\packages\System.Linq.AsyncEnumerable.10.0.12\lib\net462\System.Linq.AsyncEnumerable.dll

ASSEMBLY-NAME: System.Linq.AsyncEnumerable
ASSEMBLY-VERSION: 10.0.0.12
PUBLIC-KEY-TOKEN: b03f5f7f11d50a3a

VERSION: 10.0.0.12

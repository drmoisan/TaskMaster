# P9-T2 final remediation analyzer gate

`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Exit code: `0`. The solution build succeeded with zero analyzer errors. Five existing System.Reactive packages.config warnings remain.

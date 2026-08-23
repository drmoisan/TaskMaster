# P9-T3 final remediation nullable gate

`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`

Exit code: `0`. The solution build succeeded with zero compiler or nullable errors. Five existing System.Reactive packages.config warnings remain.

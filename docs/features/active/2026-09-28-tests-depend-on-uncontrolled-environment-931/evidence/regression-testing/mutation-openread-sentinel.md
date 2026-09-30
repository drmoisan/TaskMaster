# Negative Control M4: OpenRead Sentinel Replaced

Timestamp: 2026-09-29T09-29
Command: (1) CMD-CENSUS with PATH = UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs; (2) msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-UCS, TASKID p3-t7), resolved through vswhere, plus /nodeReuse:false; (3) vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~OpenRead_ShouldReturnReadableStreamForWrappedFile" "/ResultsDirectory:coverage\test-results\931\p3-t7" "/Logger:trx;LogFileName=p3-t7.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-VSTEST, resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:
- The EXIT_CODE row is scoped to the mutated run (the VSTEST_EXIT_CODE of step 3); the expected failure is the control's purpose.
- Predicted failure (D-6) observed: the test Failed at the same-instance assertion with a MESSAGE containing "to refer to".
- Acceptance: TOKEN .Returns(decoy) = 1, TOKEN Assembly.Location = 8, TOKEN BeSameAs(sentinel) = 1; MSBUILD_EXIT_CODE 0 and CSC_OUT_LINES 2; total 1, failed 1; MESSAGE contains "to refer to"; RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH - HOLD. MUTATION PREDICTION MISMATCH did not fire.

## Mutated run

- Mutated file: UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs
- Hunk: in OpenRead_ShouldReturnReadableStreamForWrappedFile, a second six-line using var decoy = new FileStream( declaration with the same four arguments inserted immediately after the sentinel declaration, and .Returns(sentinel) changed to .Returns(decoy) (applied with the Edit tool)
- TOKEN .Returns(decoy) = 1
- TOKEN Assembly.Location = 8
- TOKEN BeSameAs(sentinel) = 1
- Mutated-file census, other non-zero tokens: [TestMethod] 8, .Should() 53, FileMode. 14, FileMode.Open 14, FileAccess. 12, FileAccess.Read 12, FileShare.ReadWrite 10, FixturePath 4, using var sentinel = new FileStream( 1, wrapper.OpenRead() 2, stream.CanRead.Should().BeTrue() 1, stream.Length.Should().BeGreaterThan(0) 1; LINES = 363 (six more than 357); SHA256 = 4521642F719F7324B66F82FDC23C57CECAA1FE778168175AC6D1BA28034C4C54
- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- PROD_CSC_OUT_LINES: 0 (observation; no production file changed)
- ZERO_ERRORS_LINES: 1
- DLL_ADVANCED: True
- Runsettings file: TaskMaster.runsettings
- Isolation switch: /InIsolation
- Filter: "/TestCaseFilter:FullyQualifiedName~OpenRead_ShouldReturnReadableStreamForWrappedFile"
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COLLECTOR_LINES: 0

    COUNTERS total=1 executed=1 passed=0 failed=1
    RESULT_COUNT: 1
    RESULT OpenRead_ShouldReturnReadableStreamForWrappedFile = Failed
    MESSAGE OpenRead_ShouldReturnReadableStreamForWrappedFile :: Expected stream to refer to System.IO.FileStream
    {
        CanRead = True,
        CanSeek = True,
        CanTimeout = False,
        CanWrite = False,
        Handle = 4372,
        IsAsync = False,
        Length = 4150272L,
        Name = "<repo-root>\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll",
        Position = 0L,
        ReadTimeout = "[Member 'ReadTimeout' threw an exception: 'Timeouts are not supported on this stream.']",
        SafeFileHandle = Microsoft.Win32.SafeHandles.SafeFileHandle
        {
            IsClosed = False,
            IsInvalid = False
        },
        WriteTimeout = "[Member 'WriteTimeout' threw an exception: 'Timeouts are not supported on this stream.']"
    }, but found System.IO.FileStream
    {
        CanRead = True,
        CanSeek = True,
        CanTimeout = False,
        CanWrite = False,
        Handle = 4376,
        IsAsync = False,
        Length = 4150272L,
        Name = "<repo-root>\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll",
        Position = 0L,
        ReadTimeout = "[Member 'ReadTimeout' threw an exception: 'Timeouts are not supported on this stream.']",
        SafeFileHandle = Microsoft.Win32.SafeHandles.SafeFileHandle
        {
            IsClosed = False,
            IsInvalid = False
        },
        WriteTimeout = "[Member 'WriteTimeout' threw an exception: 'Timeouts are not supported on this stream.']"
    }.

## Revert and confirming run

- Recorded at: 2026-09-29T09-30
- Commands: git checkout -- UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs; git diff --exit-code HEAD -- UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs; git status --porcelain -- UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs; CMD-CENSUS on the file; CMD-BUILD-UCS (TASKID p3-t8); CMD-VSTEST with ASSEMBLY-UCS, FILTER-OPENREAD, NAMES-FIW, TASKID p3-t8
- REVERT-DIFF-EXIT: 0
- Porcelain output: EMPTY
- TOKEN .Returns(decoy) = 0
- TOKEN Assembly.Location = 7
- SHA256: 142BCB8E95D710890A71F325A65801ED8EB53A87A339B10018D889491ECEF596 (FIX-HASH-FIW: 142BCB8E95D710890A71F325A65801ED8EB53A87A339B10018D889491ECEF596; equal)
- LINES = 357
- MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 0; ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True
- CONFIRMING-RUN-EXIT: 0
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
- Acceptance: REVERT-DIFF-EXIT 0; porcelain empty; .Returns(decoy) 0, Assembly.Location 7 and SHA256 equal to FIX-HASH-FIW; CSC_OUT_LINES at least 1 with DLL_ADVANCED True; CONFIRMING-RUN-EXIT 0 with total 1, passed 1, RESULT Passed - HOLD.

    COUNTERS total=1 executed=1 passed=1 failed=0
    RESULT_COUNT: 1
    RESULT OpenRead_ShouldReturnReadableStreamForWrappedFile = Passed

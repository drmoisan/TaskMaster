Timestamp: 2026-10-18T01-24
Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /TestCaseFilter:Name=LegacySelectionChange_OutOfRootKnownChild_DoesNotMutateOrAcknowledge /Logger:console;verbosity=minimal
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: VSTest version 18.10.0 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
  Failed LegacySelectionChange_OutOfRootKnownChild_DoesNotMutateOrAcknowledge [321 ms]
  Error Message:
   Expected outputs to contain a single item, but found {"{"type":"render","rows":[{"rowIndex":0,"isSuggestion":true,"selected":true,"collapsed":false,"leafExpanded":true,"percentText":"73%","cells":[{"kind":"segment","text":"Apollo","segmentIndex":0,"truncationEligible":false},{"kind":"minus","text":"","segmentIndex":-1,"truncationEligible":false}],"subfolders":[{"displayName":"Alpha","folderPath":"\\\\mailbox\\Archive2\\Alpha","hasChildren":false,"identity":"72d4cc63fe344d01a956e980ca3875d1","expanded":false,"selected":true,"childrenLoaded":false,"children":[]}]}],"selectedSubfolderIndex":0,"selectedDescendantIdentity":"72d4cc63fe344d01a956e980ca3875d1"}", "{"type":"selectionChange","rowIndex":0,"subfolderIndex":0}"}.
  Stack Trace:
     at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message) in /_/Src/FluentAssertions/Execution/LateBoundTestFramework.cs:line 23
   at FluentAssertions.Execution.AssertionChain.FailWith(Func`1 getFailureReason) in /_/Src/FluentAssertions/Execution/AssertionChain.cs:line 278
   at FluentAssertions.Collections.GenericCollectionAssertions`3.ContainSingle(String because, Object[] becauseArgs) in /_/Src/FluentAssertions/Collections/GenericCollectionAssertions.cs:line 1350
   at UtilitiesCS.Test.OutlookObjects.Folder.BreadcrumbDescendantValidationIssue997Tests.<LegacySelectionChange_OutOfRootKnownChild_DoesNotMutateOrAcknowledge>d__3.MoveNext() in UtilitiesCS.Test\OutlookObjects\Folder\BreadcrumbDescendantValidationIssue997Tests.cs:line 119
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.Execution.TestMethodInfo.<ExecuteInternalAsync>d__64.MoveNext()
  Standard Output Messages:
      Warning:
      The component "Fluent Assertions" is governed by the rules defined in the Xceed License Agreement and
      the Xceed Fluent Assertions Community License. You may use Fluent Assertions free of charge for
      non-commercial use only. An active subscription is required to use Fluent Assertions for commercial use.
 
      Please contact Xceed Sales mailto:sales@xceed.com to acquire a subscription at a very low cost.
 
      A paid commercial license supports the development and continued increasing support of
      Fluent Assertions users under both commercial and community licenses. Help us
      keep Fluent Assertions at the forefront of unit testing.
 
      For more information, visit https://xceed.com/products/unit-testing/fluent-assertions/
 
 
 Debug Trace:
      Warning:
      The component "Fluent Assertions" is governed by the rules defined in the Xceed License Agreement and
      the Xceed Fluent Assertions Community License. You may use Fluent Assertions free of charge for
      non-commercial use only. An active subscription is required to use Fluent Assertions for commercial use.
 
      Please contact Xceed Sales mailto:sales@xceed.com to acquire a subscription at a very low cost.
 
      A paid commercial license supports the development and continued increasing support of
      Fluent Assertions users under both commercial and community licenses. Help us
      keep Fluent Assertions at the forefront of unit testing.
 
      For more information, visit https://xceed.com/products/unit-testing/fluent-assertions/



Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 476 ms - UtilitiesCS.Test.dll (net481)



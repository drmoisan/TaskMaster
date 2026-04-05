# UtilitiesCS Coverage Missing Classification

- Root Directory: `UtilitiesCS`
- Coverage Report: `coverage/coverage.cobertura.xml`
- Coverage Threshold: `80`
- Scan Date: `2026-03-22`

## Summary

Files present in the compiled project but absent from Cobertura coverage data were reviewed before planning. Per workflow, each missing file is either documented with a concrete non-planning reason or noted as an on-disk uncompiled exclusion.

### Compiled files missing from coverage data but excluded from planning with evidence-backed reasons

#### Interface / metadata / no executable body
- `UtilitiesCS\EmailIntelligence\EmailParsingSorting\IEmailTokenizer.cs` — interface definition only
- `UtilitiesCS\EmailIntelligence\Flags\IFlagTranslator.cs` — interface definition only
- `UtilitiesCS\Interfaces\Enums.cs` — enum definitions only; no executable bodies
- `UtilitiesCS\Interfaces\IEmailIntelligence\IAttachment.cs` — interface definition only
- `UtilitiesCS\Interfaces\IEmailIntelligence\IFolderWrapper.cs` — interface definition only
- `UtilitiesCS\Interfaces\IEmailIntelligence\IItemInfo.cs` — interface definition only
- `UtilitiesCS\Interfaces\IEmailIntelligence\IMovedMailInfo.cs` — interface definition only
- `UtilitiesCS\Interfaces\IGlobals\IAppAutoFileObjects.cs` — interface definition only
- `UtilitiesCS\Interfaces\IGlobals\IAppEvents.cs` — interface definition only
- `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` — interface definition only
- `UtilitiesCS\Interfaces\IGlobals\IApplicationGlobals.cs` — interface definition only
- `UtilitiesCS\Interfaces\IGlobals\IAppQuickFilerSettings.cs` — interface definition only
- `UtilitiesCS\Interfaces\IGlobals\IAppStagingFilenames.cs` — interface definition only
- `UtilitiesCS\Interfaces\IGlobals\IConditionalEngine.cs` — interface definition only
- `UtilitiesCS\Interfaces\IGlobals\IFileSystemFolderPaths.cs` — interface definition only
- `UtilitiesCS\Interfaces\IGlobals\IOlObjects.cs` — interface definition only
- `UtilitiesCS\Interfaces\IGlobals\IToDoObj.cs` — interface definition only
- `UtilitiesCS\Interfaces\IGlobals\IToDoObjects.cs` — interface definition only
- `UtilitiesCS\Interfaces\IHelperClasses\IDirectoryInfo.cs` — interface definition only
- `UtilitiesCS\Interfaces\IHelperClasses\IFileInfo.cs` — interface definition only
- `UtilitiesCS\Interfaces\IHelperClasses\IFileSystemInfo.cs` — interface definition only
- `UtilitiesCS\Interfaces\IOutlookObjects\IEmailDetailsWrapper.cs` — interface definition only
- `UtilitiesCS\Interfaces\IOutlookObjects\IOutlookItemFlaggable.cs` — interface definition only
- `UtilitiesCS\Interfaces\IOutlookObjects\IRecipientInfo.cs` — interface definition only
- `UtilitiesCS\Interfaces\IQuickFiler\IQfcTipsDetails.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\Concurrent\IConcurrentDictionary.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\Concurrent\Observable\Dictionary\IConcurrentObservableDictionary.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\Concurrent\Observable\Dictionary\IDictionaryObserver.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\IOutlookItem.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\IPercentageMatchable.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection2.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoDictionary.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\ISerializableDictionary.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\ISerializableList.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\ISmartSerializable.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\ISmartSerializableConfig.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\ISmartSerializableNonTyped.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\Observable\IObservableDictionary.cs` — interface definition only
- `UtilitiesCS\Interfaces\IReusableTypeClasses\SerializableNew\Concurrent\Observable\IScoDictionaryNew.cs` — interface definition only
- `UtilitiesCS\Interfaces\ITimerWrapper.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\IAutoAssign.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\IFlagChangeGroup.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\IFlagChangeItem.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\IFlagChangeTrainingQueue.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\IIDList.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\IPeopleScoDictionary.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\IPeopleScoDictionaryNew.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\IPrefix.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\IProjectData.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\IProjectEntry.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\IProjectInfoLegacy.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\ISubjectMapEncoder.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\ISubjectMapEntry.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\ISubjectMapSco.cs` — interface definition only
- `UtilitiesCS\Interfaces\IToDo\IToDoItem.cs` — interface definition only
- `UtilitiesCS\Interfaces\IWinForm\IContainerControl.cs` — interface definition only
- `UtilitiesCS\Interfaces\IWinForm\IControl.cs` — interface definition only
- `UtilitiesCS\Interfaces\IWinForm\IControlCollection.cs` — interface definition only
- `UtilitiesCS\Interfaces\IWinForm\IForm.cs` — interface definition only
- `UtilitiesCS\Interfaces\IWinForm\IScrollableControl.cs` — interface definition only
- `UtilitiesCS\Interfaces\IWinForm\IUserControl.cs` — interface definition only
- `UtilitiesCS\OutlookObjects\Store\IStoreWrapperViewer.cs` — interface definition only
- `UtilitiesCS\Properties\AssemblyInfo.cs` — assembly attributes only
- `UtilitiesCS\ReusableTypeClasses\Concurrent\Observable\Bag\ISimpleActionBagObserver.cs` — interface definition only
- `UtilitiesCS\ReusableTypeClasses\Locking\ILockingLinkedList.cs` — interface definition only
- `UtilitiesCS\ReusableTypeClasses\Locking\Observable\LinkedList\ILockingLinkedListObserver.cs` — interface definition only
- `UtilitiesCS\Threading\IProgressViewer.cs` — interface definition only

#### Commented-out or dead-code stubs with no executable lines
- `UtilitiesCS\EmailIntelligence\Bayesian\Obsolete\BayesianFilter.cs` — fully commented obsolete stub
- `UtilitiesCS\EmailIntelligence\Bayesian\Obsolete\CorpusExample.cs` — fully commented obsolete stub
- `UtilitiesCS\EmailIntelligence\Bayesian\Obsolete\CorpusVectorized_badidea.cs` — fully commented obsolete stub
- `UtilitiesCS\Extensions\ExtToChar.cs` — fully commented stub
- `UtilitiesCS\OutlookObjects\MailItem\CaptureEmailAddressesModule2.cs` — class shell with only commented implementation
- `UtilitiesCS\ReusableTypeClasses\Concurrent\Observable\Bag\ConcurrentObservableBag.cs` — fully commented stub
- `UtilitiesCS\ReusableTypeClasses\Observable\ObservableDictionary.cs` — fully commented stub
- `UtilitiesCS\Threading\AsyncIdleQueue1.cs` — fully commented stub
- `UtilitiesCS\To Depricate\FlattenArray.cs` — deprecated commented/dead stub
- `UtilitiesCS\To Depricate\StackObjectVB.cs` — deprecated commented/dead stub

#### Empty placeholder types with no executable branches
- `UtilitiesCS\EmailIntelligence\IntelligenceFilters.cs` — empty class declaration only
- `UtilitiesCS\OutlookObjects\Item\ItemComparer.cs` — empty comparer shell with no method body in file

### On-disk `.cs` files not compiled by `UtilitiesCS.csproj`
- `UtilitiesCS\EmailIntelligence\Bayesian\SpamBayes.cs` — present on disk but not included in `UtilitiesCS.csproj`
- `UtilitiesCS\EmailIntelligence\FolderConverter.cs` — present on disk but not included in `UtilitiesCS.csproj`
- `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\OSFolder.cs` — present on disk but not included in `UtilitiesCS.csproj`
- `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\OSFolder.Designer.cs` — generated file naming pattern and not included in `UtilitiesCS.csproj`
- `UtilitiesCS\EmailIntelligence\People\PeopleScoDictionaryNewBackup.cs` — backup file naming pattern and not included in `UtilitiesCS.csproj`
- `UtilitiesCS\Examples\MSDemoConv.cs` — example/sample file and not included in `UtilitiesCS.csproj`
- `UtilitiesCS\Interfaces\PrefixInterface.cs` — present on disk but not included in `UtilitiesCS.csproj`
- `UtilitiesCS\OutlookObjects\MailResolution.cs` — present on disk but not included in `UtilitiesCS.csproj`; compiled implementation lives elsewhere in project
- `UtilitiesCS\WindowsAPI\ExtraDeclarations.cs` — present on disk but not included in `UtilitiesCS.csproj`

## Planning outcome for missing coverage data

No file missing from Cobertura coverage data remains an implementation-planning target after classification. The active planning target set therefore consists of the compiled `UtilitiesCS` files with numeric line-rate below `80%`.

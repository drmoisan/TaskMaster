using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
using Outlook = Microsoft.Office.Interop.Outlook;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    public partial class FolderPredictorTests
    {
        private static Mock<Outlook.Application> CreateApplication(
            IDictionary<string, OutlookFolder> rootFolders
        )
        {
            var app = new Mock<Outlook.Application>();
            var nameSpace = new Mock<Outlook.NameSpace>();

            nameSpace.SetupGet(x => x.Folders).Returns(CreateFoldersCollection(rootFolders).Object);
            app.SetupGet(x => x.Session).Returns(nameSpace.Object);

            return app;
        }

        private static Mock<OutlookFolder> CreateFolder(
            string folderPath,
            IDictionary<string, OutlookFolder> childFolders = null,
            params OutlookFolder[] enumerableChildren
        )
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.Name).Returns(GetLeafName(folderPath));
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder
                .SetupGet(x => x.Folders)
                .Returns(
                    CreateFoldersCollection(
                        childFolders ?? new Dictionary<string, OutlookFolder>(),
                        enumerableChildren
                    ).Object
                );
            return folder;
        }

        private static Mock<OutlookFolders> CreateFoldersCollection(
            IDictionary<string, OutlookFolder> foldersByName,
            params OutlookFolder[] enumerableChildren
        )
        {
            var folders = new Mock<OutlookFolders>();
            var enumerableItems = enumerableChildren is { Length: > 0 }
                ? enumerableChildren
                : (foldersByName?.Values?.ToArray() ?? Array.Empty<OutlookFolder>());
            var collection = new ArrayList(enumerableItems);

            folders
                .Setup(x => x[It.IsAny<object>()])
                .Returns<object>(key =>
                {
                    if (
                        key is string name
                        && foldersByName.TryGetValue(name, out OutlookFolder folder)
                    )
                    {
                        return folder;
                    }

                    return null;
                });
            folders.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());

            return folders;
        }

        private static Mock<IApplicationGlobals> CreateGlobals(
            Mock<Outlook.Application> app,
            OutlookFolder rootFolder,
            IEnumerable<string> recents = null
        )
        {
            var recentsList = recents is null
                ? new SloLinkedList<string>()
                : new SloLinkedList<string>(recents);
            var autoFile = new Mock<IAppAutoFileObjects>();
            autoFile.SetupGet(x => x.RecentsList).Returns(recentsList);

            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.App).Returns(app.Object);
            olObjects.SetupGet(x => x.ArchiveRootPath).Returns(rootFolder.FolderPath);
            olObjects.SetupGet(x => x.Root).Returns(rootFolder);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.AF).Returns(autoFile.Object);
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static string GetLeafName(string folderPath)
        {
            return folderPath.Split('\\').Last(segment => !string.IsNullOrWhiteSpace(segment));
        }

        private sealed class TestableFolderPredictor : FolderPredictor
        {
            private readonly Queue<string> promptResponses;

            public TestableFolderPredictor(
                IApplicationGlobals globals,
                params string[] promptResponses
            )
                : base(globals)
            {
                this.promptResponses = new Queue<string>(promptResponses ?? Array.Empty<string>());
            }

            public List<string> Messages { get; } = new();

            public List<string> CreatedDirectories { get; } = new();

            public int EnterUiContextCalls { get; private set; }

            internal override string PromptForFolderName(
                string prompt,
                string title,
                string defaultValue = null
            )
            {
                return promptResponses.Count > 0 ? promptResponses.Dequeue() : null;
            }

            internal override void ShowPromptMessage(string message)
            {
                Messages.Add(message);
            }

            internal override Task EnterUiContextAsync()
            {
                EnterUiContextCalls++;
                return Task.CompletedTask;
            }

            internal override DirectoryInfo CreateDirectoryPath(string path)
            {
                CreatedDirectories.Add(path);
                return new DirectoryInfo(path);
            }
        }

        private sealed class ImmediateSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object state)
            {
                d(state);
            }
        }
    }
}

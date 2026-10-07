#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.Extensions.Lazy;

namespace UtilitiesCS.EmailIntelligence
{
    public partial class Triage
    {
        public async Task<bool> RebuildFromStagedMinedMailAsync(
            Func<string, Task<MinedMailInfo[]>>? loadAsync = null,
            CancellationToken cancellationToken = default
        )
        {
            if (!Globals.FS.SpecialFolders.TryGetValue("AppData", out var folderRoot))
            {
                return false;
            }

            var stagingPath = Path.Combine(folderRoot, "Bayesian");
            Func<string, Task<MinedMailInfo[]>> load =
                loadAsync ?? (path => EmailDataMiner.Load<MinedMailInfo[]>(path));
            var collection = await load(stagingPath);
            return await RebuildFromMinedMailAsync(
                collection,
                cancellationToken: cancellationToken
            );
        }

        public async Task<bool> RebuildFromMinedMailAsync(
            IEnumerable<MinedMailInfo>? collection,
            Func<BayesianClassifierGroup, Task>? persistAsync = null,
            Action<BayesianClassifierGroup>? replaceManager = null,
            CancellationToken cancellationToken = default
        )
        {
            if (collection is null)
            {
                return false;
            }

            var trainingMail = collection
                .Where(mail => ClassNames.Contains(mail.Triage ?? string.Empty))
                .ToArray();
            if (trainingMail.Length == 0)
            {
                return false;
            }

            var classifierGroup = await CreateTriageClassifiersAsync(ClassNames, cancellationToken);
            classifierGroup.TotalEmailCount = trainingMail.Length;
            classifierGroup.SharedTokenBase = new Corpus(
                trainingMail
                    .SelectMany(mail => mail.Tokens ?? Array.Empty<string>())
                    .GroupAndCount()
            );

            foreach (var className in ClassNames)
            {
                var classMail = trainingMail.Where(mail => mail.Triage == className).ToArray();
                await classifierGroup.RebuildClassifier(
                    className,
                    classMail
                        .SelectMany(mail => mail.Tokens ?? Array.Empty<string>())
                        .GroupAndCount(),
                    classMail.Length,
                    cancellationToken
                );
            }

            await (persistAsync ?? PersistClassifierGroupAsync)(classifierGroup);
            (replaceManager ?? ReplaceClassifierGroup)(classifierGroup);
            ClassifierGroup = classifierGroup;
            return true;
        }

        private async Task PersistClassifierGroupAsync(BayesianClassifierGroup classifierGroup)
        {
            if ((await Globals.AF.Manager.Configuration).TryGetValue(GroupName, out var loader))
            {
                classifierGroup.Config = loader.Config;
                classifierGroup.Serialize();
            }
        }

        private void ReplaceClassifierGroup(BayesianClassifierGroup classifierGroup)
        {
            Globals.AF.Manager[GroupName] = classifierGroup.ToAsyncLazy();
        }
    }
}

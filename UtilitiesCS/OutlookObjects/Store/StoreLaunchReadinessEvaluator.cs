#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Shared readiness evaluation for the store-settings dialogs: computes whether the
    /// store-wrapper model has finished loading and is safe to bind (issue #240), reused by
    /// both <see cref="StoreWrapperController"/> and <c>DisabledStoresController</c> so the
    /// readiness behavior is defined once.
    /// </summary>
    internal static class StoreLaunchReadinessEvaluator
    {
        /// <summary>
        /// Evaluates launch readiness from the supplied globals. Returns
        /// <see cref="StoreLaunchReadinessState.ModelUnavailable"/> when the model is null,
        /// <see cref="StoreLaunchReadinessState.StoresUnavailable"/> when the model's stores
        /// list is transiently null, otherwise <see cref="StoreLaunchReadinessState.Ready"/>
        /// with the model and the display names of every store it contains.
        /// </summary>
        internal static StoreLaunchReadiness Evaluate(IApplicationGlobals globals)
        {
            var model = globals?.Ol?.StoresWrapper;
            if (model is null)
            {
                return StoreLaunchReadiness.NotReady(StoreLaunchReadinessState.ModelUnavailable);
            }

            if (model.Stores is null)
            {
                return StoreLaunchReadiness.NotReady(StoreLaunchReadinessState.StoresUnavailable);
            }

            return StoreLaunchReadiness.Ready(
                model,
                model.Stores.Select(store => store.DisplayName).ToList()
            );
        }
    }
}

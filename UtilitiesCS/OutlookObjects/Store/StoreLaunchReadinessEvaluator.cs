#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Shared readiness evaluation for the store-settings dialogs: computes whether the
    /// store-wrapper model has finished loading and is safe to bind (issue #240), reused by
    /// both <see cref="StoreWrapperController"/> and <c>DisabledStoresController</c> so the
    /// readiness behavior is defined once. <c>ModelUnavailable</c> is also the terminal state
    /// for the remainder of an Outlook session once the store load has completed through its
    /// catch block: see <c>TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:66-72</c>, where a
    /// caught load failure leaves <c>StoresWrapper</c> unset and there is no later retry that
    /// would move the session out of this state.
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

        /// <summary>
        /// Builds the user-facing message for a not-ready <paramref name="state"/>, distinguishing
        /// a genuinely unavailable model from a store list that is still loading.
        /// </summary>
        /// <param name="state">The not-ready readiness state to build the message for.</param>
        /// <returns>The message text to show in the dialog for the given state.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="state"/> is <see cref="StoreLaunchReadinessState.Ready"/>,
        /// which has no unavailable message.
        /// </exception>
        internal static string BuildUnavailableMessage(StoreLaunchReadinessState state)
        {
            return state switch
            {
                StoreLaunchReadinessState.Ready => throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state,
                    "There is no unavailable message for a ready store model."
                ),
                StoreLaunchReadinessState.StoresUnavailable =>
                    "The store list has not finished loading. Please try again shortly.",
                _ =>
                    "Store settings are not available. Retry once startup has completed; if the message persists, the store settings failed to load and the application log records the cause.",
            };
        }

        /// <summary>
        /// Builds the user-facing title for a not-ready <paramref name="state"/>, distinguishing
        /// a genuinely unavailable model from a store list that is still loading.
        /// </summary>
        /// <param name="state">The not-ready readiness state to build the title for.</param>
        /// <returns>The title text to show in the dialog for the given state.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="state"/> is <see cref="StoreLaunchReadinessState.Ready"/>,
        /// which has no unavailable title.
        /// </exception>
        internal static string BuildUnavailableTitle(StoreLaunchReadinessState state)
        {
            return state switch
            {
                StoreLaunchReadinessState.Ready => throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state,
                    "There is no unavailable title for a ready store model."
                ),
                StoreLaunchReadinessState.StoresUnavailable => "Store Settings Loading",
                _ => "Store Settings Unavailable",
            };
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace TaskMaster.Test.Bootstrap
{
    /// <summary>
    /// Cross-domain proxy that performs every bind observation <em>inside</em> a freshly
    /// created child <see cref="AppDomain"/> and marshals primitive results back to the
    /// parent domain.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type deliberately references no assertion library. Loading one into the child
    /// domain would add assemblies to a domain whose emptiness is the point of the harness.
    /// Specifically, a <c>netstandard 2.0.0.0</c> facade arriving that way in the positive
    /// domain would be returned by ladder rung 1, letting the two-version bind assertion
    /// pass without rungs 2 or 3 ever executing. Error reporting therefore uses
    /// <see cref="InvalidOperationException"/>, a base-class-library type that marshals
    /// across the domain boundary, and outcomes are returned as plain strings.
    /// </para>
    /// <para>
    /// The installer call is kept in its own method, separate from every bind attempt, so
    /// the negative-control domain never JIT-resolves <c>UtilitiesCS</c>.
    /// </para>
    /// </remarks>
    public sealed class ChildDomainBindProbe : MarshalByRefObject
    {
        /// <summary>Outcome string returned when a load succeeded.</summary>
        public const string LoadedOutcome = "LOADED";

        /// <summary>Outcome string returned when a type initializer ran without throwing.</summary>
        public const string OkOutcome = "OK";

        private const string AssemblyResolveFieldName = "_AssemblyResolve";

        /// <summary>
        /// Counts assemblies loaded in this domain whose simple name matches
        /// <paramref name="simpleName"/>, compared case-insensitively.
        /// </summary>
        public int CountLoadedAssembliesNamed(string simpleName)
        {
            if (string.IsNullOrEmpty(simpleName))
            {
                throw new InvalidOperationException("simpleName must be supplied.");
            }

            return AppDomain
                .CurrentDomain.GetAssemblies()
                .Count(a =>
                    string.Equals(a.GetName().Name, simpleName, StringComparison.OrdinalIgnoreCase)
                );
        }

        /// <summary>
        /// Returns the number of handlers subscribed to this domain's assembly-resolution
        /// event.
        /// </summary>
        /// <remarks>
        /// <see cref="AppDomain"/> exposes no public accessor for the invocation list, so the
        /// private instance field is read by reflection. net481 is a frozen runtime on which
        /// the field is stable. If the field is absent this method throws rather than
        /// returning a sentinel: a silently skipped isolation check would make every
        /// positive assertion in the harness vacuous.
        /// </remarks>
        public int CountAssemblyResolveHandlers()
        {
            FieldInfo field = typeof(AppDomain).GetField(
                AssemblyResolveFieldName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            if (field == null)
            {
                throw new InvalidOperationException(
                    "The AppDomain field "
                        + AssemblyResolveFieldName
                        + " was not found, so the isolation check cannot be made."
                );
            }

            var handler = field.GetValue(AppDomain.CurrentDomain) as Delegate;
            return handler == null ? 0 : handler.GetInvocationList().Length;
        }

        /// <summary>
        /// Installs the production binding fallback in this domain, and does nothing else.
        /// </summary>
        /// <remarks>
        /// Keeping this call in its own method is what prevents <c>UtilitiesCS</c> from being
        /// JIT-resolved in the installer-free negative-control domain.
        /// </remarks>
        public void InstallProductionFallback()
        {
            UtilitiesCS.Bootstrap.AssemblyBindingFallback.Install();
        }

        /// <summary>
        /// Attempts <see cref="Assembly.Load(string)"/> of the supplied full display name.
        /// </summary>
        /// <returns>
        /// <see cref="LoadedOutcome"/> when the load returned an assembly, otherwise the
        /// simple name of the exception type that was raised.
        /// </returns>
        public string TryLoadDisplayName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                throw new InvalidOperationException("displayName must be supplied.");
            }

            try
            {
                Assembly loaded = Assembly.Load(displayName);
                return loaded == null ? "NULL" : LoadedOutcome;
            }
            catch (Exception ex)
            {
                // Boundary catch: the outcome of the bind is the measurement, so every
                // exception type is a legitimate result rather than a test failure here.
                return ex.GetType().Name;
            }
        }

        /// <summary>
        /// Loads Deedle from the supplied absolute path and forces the class constructor of
        /// <c>Deedle.Reflection</c>, which is the initializer the reported production failure
        /// occurs in.
        /// </summary>
        /// <returns>
        /// <see cref="OkOutcome"/> when the initializer ran without throwing, otherwise the
        /// simple name of the exception type that was raised.
        /// </returns>
        /// <exception cref="InvalidOperationException">The file is absent.</exception>
        public string DeedleTypeInitializerOutcome(string deedleDllPath)
        {
            if (string.IsNullOrEmpty(deedleDllPath) || !File.Exists(deedleDllPath))
            {
                throw new InvalidOperationException(
                    "Deedle.dll was not found at the supplied path: " + deedleDllPath
                );
            }

            try
            {
                Assembly deedle = Assembly.LoadFrom(deedleDllPath);
                Type reflection = deedle.GetType("Deedle.Reflection", throwOnError: true);
                RuntimeHelpers.RunClassConstructor(reflection.TypeHandle);
                return OkOutcome;
            }
            catch (Exception ex)
            {
                // Boundary catch: the outcome of the initializer is the measurement.
                return ex.GetType().Name;
            }
        }

        /// <summary>
        /// Parses the supplied configuration file and counts its <c>dependentAssembly</c>
        /// entries whose <c>assemblyIdentity</c> names <c>netstandard</c>.
        /// </summary>
        /// <exception cref="InvalidOperationException">The file is absent.</exception>
        public int ConfigurationFileNetstandardEntryCount(string configPath)
        {
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                throw new InvalidOperationException(
                    "The configuration file was not found at the supplied path: " + configPath
                );
            }

            XDocument document = XDocument.Load(configPath);
            return document
                .Descendants()
                .Where(e => e.Name.LocalName == "dependentAssembly")
                .Count(e =>
                    e.Elements()
                        .Any(child =>
                            child.Name.LocalName == "assemblyIdentity"
                            && string.Equals(
                                (string)child.Attribute("name"),
                                "netstandard",
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                );
        }
    }
}

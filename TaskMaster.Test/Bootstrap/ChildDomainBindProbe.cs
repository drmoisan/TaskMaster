using System;
using System.IO;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Outcome string returned when the record conversion was invoked and returned without
        /// throwing.
        /// </summary>
        public const string InvokedOutcome = "INVOKED-NO-EXCEPTION";

        /// <summary>
        /// Prefix of the outcome string returned when the conversion failed because a
        /// <c>netstandard</c> identity could not be bound.
        /// </summary>
        public const string BindFailurePrefix = "NETSTANDARD-BIND-FAILURE:";

        /// <summary>
        /// Prefix of the outcome string returned when the conversion failed for any other
        /// reason. Keeping the two classes apart is what stops an unrelated exception being
        /// read as evidence about the bind.
        /// </summary>
        public const string OtherFailurePrefix = "OTHER-FAILURE:";

        private const string AssemblyResolveFieldName = "_AssemblyResolve";

        /// <summary>
        /// Record-shaped input of the same kind production supplies to Deedle's frame builder
        /// at <c>UtilitiesCS/Extensions/DfDeedle.cs</c>.
        /// </summary>
        /// <remarks>
        /// This is not a copy of production's shape: production's record type is a private
        /// struct exposing public fields, whereas this one is a sealed class exposing
        /// auto-properties. Deedle's member accepts either, and a public class is used here
        /// because the cross-domain proxy must be able to close the generic method over the
        /// type. It carries no date-valued member, so the determinism sweep over this file
        /// stays clean.
        /// </remarks>
        public sealed class DeedleProbeRecord
        {
            /// <summary>An arbitrary text column.</summary>
            public string Label { get; set; }

            /// <summary>An arbitrary numeric column.</summary>
            public double Value { get; set; }
        }

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
        /// Returns the application base directory of this domain, read inside the domain.
        /// </summary>
        /// <remarks>
        /// It takes no argument, touches no file and loads no assembly, so it is safe to call in
        /// the installer-free negative-control domain as well as in the positive one. It exists
        /// so the harness can assert the directory the child domain is actually rooted at rather
        /// than assume the configured value took effect.
        /// </remarks>
        public string ApplicationBaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
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
        /// Loads Deedle from the supplied absolute path and invokes the single-generic-argument,
        /// single-parameter <c>FromRecords</c> member of <c>Deedle.Frame</c>, closed over
        /// <see cref="DeedleProbeRecord"/>. That member is production's own entry point, called
        /// from <c>UtilitiesCS/Extensions/DfDeedle.cs</c>.
        /// </summary>
        /// <returns>
        /// <see cref="InvokedOutcome"/> when the invocation returned without throwing;
        /// otherwise <see cref="BindFailurePrefix"/> or <see cref="OtherFailurePrefix"/>
        /// followed by the simple name of the unwrapped exception type.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The file is absent, or the member could not be resolved in the expected shape. Both
        /// are broken-probe conditions rather than measurements, so they are raised rather
        /// than classified.
        /// </exception>
        public string DeedleRecordConversionOutcome(string deedleDllPath)
        {
            if (string.IsNullOrEmpty(deedleDllPath) || !File.Exists(deedleDllPath))
            {
                throw new InvalidOperationException(
                    "Deedle.dll was not found at the supplied path: " + deedleDllPath
                );
            }

            var records = new[]
            {
                new DeedleProbeRecord { Label = "probe", Value = 1.0 },
            };

            try
            {
                Assembly deedle = Assembly.LoadFrom(deedleDllPath);
                Type frame = deedle.GetType("Deedle.Frame", throwOnError: true);
                MethodInfo definition = ResolveFromRecords(frame);
                MethodInfo closed = definition.MakeGenericMethod(typeof(DeedleProbeRecord));
                closed.Invoke(null, new object[] { records });
                return InvokedOutcome;
            }
            catch (InvalidOperationException)
            {
                // Fail loud, and deliberately first: a lookup miss means the probe is broken,
                // not that the bind failed. Letting it escape past the classifier is what
                // stops a broken probe being reported as a measurement.
                throw;
            }
            catch (Exception ex)
            {
                // Boundary catch: the outcome of the invocation is the measurement.
                return ClassifyConversionFailure(ex);
            }
        }

        /// <summary>
        /// Resolves the generic method definition the probe invokes, selecting it by shape and
        /// failing loudly unless exactly one member survives the filter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The declaring type carries more than one <c>FromRecords</c> overload, so a name-only
        /// lookup cannot be used: it would have to report an ambiguity rather than a member. The
        /// members are enumerated instead and filtered on generic arity and parameter count,
        /// which is the same rule the plan's metadata measurement applies, so the plan and the
        /// probe select the same member by the same rule.
        /// </para>
        /// <para>
        /// Resolving a <see cref="MethodInfo"/> does not run a class constructor, which is why
        /// this helper can sit inside the caller's try block without itself triggering the bind
        /// under test; the invocation is what triggers it. The non-public binding flag covers
        /// either accessibility the member may carry.
        /// </para>
        /// </remarks>
        private static MethodInfo ResolveFromRecords(Type frame)
        {
            const string MemberName = "FromRecords";

            MethodInfo[] candidates = frame
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m =>
                    m.Name == MemberName
                    && m.IsGenericMethodDefinition
                    && m.GetGenericArguments().Length == 1
                    && m.GetParameters().Length == 1
                )
                .ToArray();

            if (candidates.Length != 1)
            {
                throw new InvalidOperationException(
                    "The shape filter over "
                        + frame.FullName
                        + "."
                        + MemberName
                        + " left "
                        + candidates.Length
                        + " members, so the probe cannot name the overload it means."
                );
            }

            return candidates[0];
        }

        /// <summary>
        /// Classifies a conversion failure into one of the two structured outcome strings.
        /// </summary>
        /// <remarks>
        /// <see cref="TargetInvocationException"/> is unwrapped before the type name is
        /// reported, because <c>Invoke</c> wraps the real exception and without the unwrap every
        /// failure would read as that wrapper and carry no information. The whole inner
        /// exception chain of the original exception is walked rather than only its outermost
        /// layer, because the production shape nests the file-not-found failure two type
        /// initializers deep.
        /// </remarks>
        private static string ClassifyConversionFailure(Exception thrown)
        {
            Exception unwrapped = thrown;
            var invocation = thrown as TargetInvocationException;
            if (invocation != null && invocation.InnerException != null)
            {
                unwrapped = invocation.InnerException;
            }

            string reported = unwrapped.GetType().Name;

            for (Exception current = thrown; current != null; current = current.InnerException)
            {
                var missing = current as FileNotFoundException;
                if (missing == null)
                {
                    continue;
                }

                if (NamesNetstandard(missing.FileName) || NamesNetstandard(missing.Message))
                {
                    return BindFailurePrefix + reported;
                }
            }

            return OtherFailurePrefix + reported;
        }

        /// <summary>
        /// True when the supplied text names the <c>netstandard</c> identity, compared
        /// case-insensitively.
        /// </summary>
        /// <remarks>
        /// The caller inspects both the file name and the message, because the runtime does not
        /// guarantee the file name is populated on every binding failure, while the failing
        /// display name appears verbatim in the message text in either case.
        /// </remarks>
        private static bool NamesNetstandard(string text)
        {
            return text != null
                && text.IndexOf("netstandard", StringComparison.OrdinalIgnoreCase) >= 0;
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

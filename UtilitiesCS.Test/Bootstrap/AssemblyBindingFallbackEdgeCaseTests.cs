using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.Bootstrap;

namespace UtilitiesCS.Test.Bootstrap
{
    /// <summary>
    /// Edge-case unit tests for the host-neutral resolution ladder in
    /// <see cref="AssemblyBindingFallback"/> and for the handler that type attaches to
    /// <see cref="AppDomain.AssemblyResolve"/>. Every test below covers a decline path of
    /// that ladder or of that handler: a guard clause that returns before a rung runs, a
    /// rung that declines because its source reports nothing, or a rung-local catch that
    /// absorbs a failing source. Each one reaches its target through the ladder's injected
    /// delegates or through a guard clause rather than through a real bind, so no test in
    /// this class reads the assembly cache or touches the file system.
    /// </summary>
    [TestClass]
    public class AssemblyBindingFallbackEdgeCaseTests
    {
        // These constants and the helpers at the foot of this class deliberately mirror the
        // private members of the sibling class AssemblyBindingFallbackTests rather than
        // reusing them: those copies are private and that file must not be edited.
        private const string NetstandardToken = "cc7b13ffcd2ddd51";
        private const string MscorlibToken = "b77a5c561934e089";
        private const string FakeRuntimeDirectory = @"X:\fake-runtime\";
        private const string ProbeSimpleName = "NoSuchProbeAssembly";

        /// <summary>A real assembly object used as a resolution sentinel. Loading none.</summary>
        private static Assembly Sentinel => typeof(Uri).Assembly;

        /// <summary>A strongly named assembly already present in every test host.</summary>
        private static Assembly LoadedMscorlib => typeof(string).Assembly;

        /// <summary>
        /// The resolution seam declines a null identity at its first guard, before the
        /// production ladder is constructed and before any rung is reached.
        /// </summary>
        [TestMethod]
        public void Resolve_WhenRequestedIdentityIsNull_ReturnsNullBeforeAnyRung()
        {
            // Arrange: no ladder is built, because the guard returns before the factory runs.

            // Act
            Assembly resolved = AssemblyBindingFallback.Resolve(null);

            // Assert
            resolved.Should().BeNull();
        }

        /// <summary>
        /// The resolution seam declines an identity carrying no simple name at its second
        /// guard, again before any rung is reached.
        /// </summary>
        [TestMethod]
        public void Resolve_WhenRequestedSimpleNameIsAbsent_ReturnsNullBeforeAnyRung()
        {
            // Arrange: the parameterless constructor is the only way to obtain an identity
            // whose simple name is absent, because passing an empty string throws.
            var requested = new AssemblyName();

            // Act
            Assembly resolved = AssemblyBindingFallback.Resolve(requested);

            // Assert
            requested.Name.Should().BeNullOrEmpty();
            resolved.Should().BeNull();
        }

        /// <summary>
        /// The ladder carries its own null-identity guard, independent of the seam's, and
        /// declines there rather than walking its rungs.
        /// </summary>
        [TestMethod]
        public void LadderResolve_WhenRequestedIdentityIsNull_ReturnsNull()
        {
            // Arrange
            var ladder = CreateLadder();

            // Act
            Assembly resolved = ladder.Resolve(null);

            // Assert
            resolved.Should().BeNull();
        }

        /// <summary>
        /// Rung 3 declines without loading anything when the runtime-directory facade is not
        /// reported present, which is the early return that precedes its load call.
        /// </summary>
        [TestMethod]
        public void LadderResolve_WhenRuntimeFacadeFileIsAbsent_RungThreeDeclinesWithoutLoading()
        {
            // Arrange: rung 2 misses and no probed path is reported present.
            var loadFromPath = new Mock<Func<string, Assembly>>();
            var ladder = CreateLadder(
                loadByDisplayName: _ => throw new FileNotFoundException("no such assembly"),
                loadFromPath: loadFromPath.Object,
                fileExists: _ => false
            );

            // Act
            Assembly resolved = ladder.Resolve(NameOf("netstandard", "2.1.0.0", NetstandardToken));

            // Assert: the rung returned before loading, so no path was ever loaded.
            resolved.Should().BeNull();
            loadFromPath.Verify(f => f(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Rung 3 absorbs a failing load of the runtime-directory facade in its own catch and
        /// declines, rather than letting the failure escape the ladder.
        /// </summary>
        [TestMethod]
        public void LadderResolve_WhenRuntimeFacadeLoadThrows_RungThreeAbsorbsAndDeclines()
        {
            // Arrange: only a path under the fake runtime directory is reported present, which
            // keeps rung 4 out of this test so the covered catch is exactly rung 3's.
            var ladder = CreateLadder(
                loadByDisplayName: _ => throw new FileNotFoundException("no such assembly"),
                loadFromPath: _ => throw new BadImageFormatException("not a managed image"),
                fileExists: p => p.StartsWith(FakeRuntimeDirectory, StringComparison.Ordinal)
            );

            // Act
            Assembly resolved = null;
            Action act = () =>
                resolved = ladder.Resolve(NameOf("netstandard", "2.1.0.0", NetstandardToken));

            // Assert
            act.Should().NotThrow();
            resolved.Should().BeNull();
        }

        /// <summary>
        /// Rung 4 carries its own empty-simple-name guard and declines there, after the three
        /// preceding rungs have each declined.
        /// </summary>
        [TestMethod]
        public void LadderResolve_WhenRequestedSimpleNameIsAbsent_RungFourDeclinesOnTheNameGuard()
        {
            // Arrange
            var ladder = CreateLadder();

            // Act
            Assembly resolved = ladder.Resolve(new AssemblyName());

            // Assert: rungs 1 to 3 declined, and rung 4 returned at its own name guard.
            resolved.Should().BeNull();
        }

        /// <summary>
        /// Rung 4 loads the requested simple name plus its file extension by path when the
        /// probe directory is reported to hold it. This is the rung's success path reached on
        /// a non-throwing run.
        /// </summary>
        [TestMethod]
        public void LadderResolve_WhenProbeDirectoryHoldsTheAssembly_RungFourLoadsItByPath()
        {
            // Arrange: a non-netstandard identity, so rung 3 declines on its identity check.
            string observedPath = null;
            var ladder = CreateLadder(
                loadFromPath: p =>
                {
                    observedPath = p;
                    return Sentinel;
                },
                fileExists: _ => true
            );

            // Act
            Assembly resolved = ladder.Resolve(NameOf(ProbeSimpleName, "1.0.0.0", MscorlibToken));

            // Assert
            resolved.Should().BeSameAs(Sentinel);
            observedPath.Should().NotBeNull();
            observedPath.Should().EndWith(ProbeSimpleName + ".dll");
        }

        /// <summary>
        /// Rung 4 absorbs a failing load in its own catch and declines, rather than letting
        /// the failure escape the ladder.
        /// </summary>
        [TestMethod]
        public void LadderResolve_WhenProbeDirectoryLoadThrows_RungFourAbsorbsAndDeclines()
        {
            // Arrange: identical to the rung-4 success case except that the load throws.
            var ladder = CreateLadder(
                loadFromPath: _ => throw new BadImageFormatException("not a managed image"),
                fileExists: _ => true
            );

            // Act
            Assembly resolved = null;
            Action act = () =>
                resolved = ladder.Resolve(NameOf(ProbeSimpleName, "1.0.0.0", MscorlibToken));

            // Assert
            act.Should().NotThrow();
            resolved.Should().BeNull();
        }

        /// <summary>
        /// Rung 1 rejects an already-loaded assembly whose public key token is of a different
        /// length from the requested one, which is the length arm of the token comparison.
        /// </summary>
        [TestMethod]
        public void LadderResolve_WhenLoadedTokenLengthDiffers_RungOneRejectsTheLoadedAssembly()
        {
            // Arrange: a three-byte token, a length no real strong name carries, against the
            // eight-byte token of an assembly already loaded in the test host.
            var requested = new AssemblyName("mscorlib");
            requested.SetPublicKeyToken(new byte[] { 0x01, 0x02, 0x03 });
            var ladder = CreateLadder(getLoadedAssemblies: () => new[] { LoadedMscorlib });

            // Act
            Assembly resolved = ladder.Resolve(requested);

            // Assert
            LoadedMscorlib.GetName().GetPublicKeyToken().Should().HaveCount(8);
            requested.GetPublicKeyToken().Should().HaveCount(3);
            resolved.Should().BeNull();
        }

        /// <summary>
        /// The subscribed handler declines at its null-or-empty-name guard, both when the
        /// event arguments are absent and when they carry an empty name, so neither invocation
        /// reaches a bind, a rung or the production ladder factory.
        /// </summary>
        [TestMethod]
        public void OnAssemblyResolve_WhenEventArgsCarryNoName_ReturnsNullWithoutResolving()
        {
            // Arrange: bind the private handler exactly as the sibling class's detach helper
            // does, which is the only way to invoke it without raising a real resolution event.
            MethodInfo method = typeof(AssemblyBindingFallback).GetMethod(
                "OnAssemblyResolve",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Should().NotBeNull();
            var handler = (ResolveEventHandler)
                Delegate.CreateDelegate(typeof(ResolveEventHandler), method);

            // Act
            Assembly fromAbsentArgs = handler(null, null);
            Assembly fromEmptyName = handler(null, new ResolveEventArgs(string.Empty));

            // Assert
            fromAbsentArgs.Should().BeNull();
            fromEmptyName.Should().BeNull();
        }

        /// <summary>
        /// Builds a ladder whose five sources default to "miss", overriding only what a test
        /// needs.
        /// </summary>
        private static AssemblyBindingFallback.AssemblyBindingLadder CreateLadder(
            Func<Assembly[]> getLoadedAssemblies = null,
            Func<string, Assembly> loadByDisplayName = null,
            Func<string, Assembly> loadFromPath = null,
            Func<string, bool> fileExists = null,
            Func<string> getRuntimeDirectory = null
        )
        {
            return new AssemblyBindingFallback.AssemblyBindingLadder(
                getLoadedAssemblies ?? (() => new Assembly[0]),
                loadByDisplayName ?? (_ => null),
                loadFromPath ?? (_ => null),
                fileExists ?? (_ => false),
                getRuntimeDirectory ?? (() => FakeRuntimeDirectory)
            );
        }

        /// <summary>Builds a fully specified assembly identity.</summary>
        private static AssemblyName NameOf(string simpleName, string version, string token)
        {
            return new AssemblyName(
                simpleName + ", Version=" + version + ", Culture=neutral, PublicKeyToken=" + token
            );
        }
    }
}

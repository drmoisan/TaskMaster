using System;
using System.Reflection;
using System.Windows.Threading;
using FluentAssertions;
using UtilitiesCS;

namespace UtilitiesCS.Test
{
    /// <summary>
    /// Installs a replacement value into the private static <c>UiThread._dispatcher</c> backing
    /// field for the lifetime of a <c>using</c> statement, and restores the prior value on
    /// disposal.
    ///
    /// Reflection is still required because <c>InternalsVisibleTo</c> exposes internal members
    /// only; it does not expose private ones, and the backing field is private. Centralising the
    /// reflection here means the field name appears in exactly one place in this assembly rather
    /// than at each test that needs to control the dispatcher.
    /// </summary>
    /// <remarks>
    /// This type is deliberately <b>not</b> internally synchronized. It performs an unguarded
    /// read-then-write against a process-global static, so two tests installing concurrently would
    /// interleave and one would restore a value the other had already replaced. Serialization of
    /// writers is provided instead by <c>[DoNotParallelize]</c> on every test class that installs a
    /// value through this scope. A future caller must not assume this type is thread-safe: adding a
    /// new installing test class requires adding that attribute to the class as well.
    ///
    /// The scope is reachable only from <c>UtilitiesCS.Test</c>. <c>QuickFiler.Test</c> is a
    /// separate assembly and is not named in the <c>InternalsVisibleTo</c> grants on
    /// <c>UtilitiesCS</c>, so it uses its own fixture accessor rather than this type.
    /// </remarks>
#nullable enable annotations
    internal sealed class UiThreadDispatcherScope : IDisposable
    {
        /// <summary>
        /// The private static backing field of <c>UiThread.Dispatcher</c>, resolved once.
        /// </summary>
        /// <remarks>
        /// Resolution happens in the static initializer and asserts the field is non-null with a
        /// stated reason, mirroring the <c>ResolveDispatcherField</c> idiom in
        /// <c>QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs</c>. The
        /// assertion is what makes a rename of <c>_dispatcher</c> fail loudly: it raises
        /// <see cref="TypeInitializationException"/> on first use and fails every consuming test,
        /// rather than degrading to a silent no-op that installs nothing and still passes.
        /// </remarks>
        private static readonly FieldInfo DispatcherField = ResolveDispatcherField();

        private Dispatcher? _prior;
        private bool _disposed;

        private UiThreadDispatcherScope(Dispatcher? prior)
        {
            _prior = prior;
        }

        /// <summary>
        /// Reads the current value of the backing field directly, without going through the
        /// <c>UiThread.Dispatcher</c> property.
        /// </summary>
        /// <remarks>
        /// The property throws <see cref="InvalidOperationException"/> when the field is null, so a
        /// test that needs to observe the uninitialized state — for example to assert that a scope
        /// restored a null prior — cannot use the property to do it.
        /// </remarks>
        internal static Dispatcher? Current => (Dispatcher?)DispatcherField.GetValue(null);

        /// <summary>
        /// Captures the prior field value, writes <paramref name="replacement"/> in its place, and
        /// returns a scope that restores the captured value when disposed.
        /// </summary>
        /// <param name="replacement">
        /// The value to install. May be null, which is how a test reproduces the state in which
        /// <c>UiThread.Init()</c> has never run.
        /// </param>
        /// <returns>A scope whose disposal restores the captured prior value.</returns>
        internal static UiThreadDispatcherScope Install(Dispatcher? replacement)
        {
            var prior = (Dispatcher?)DispatcherField.GetValue(null);
            DispatcherField.SetValue(null, replacement);
            return new UiThreadDispatcherScope(prior);
        }

        /// <summary>
        /// Convenience for <c>Install(null)</c>: installs the uninitialized state in which reading
        /// <c>UiThread.Dispatcher</c> throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <returns>A scope whose disposal restores the captured prior value.</returns>
        internal static UiThreadDispatcherScope InstallNull()
        {
            return Install(null);
        }

        /// <summary>
        /// Restores the value captured at construction, including when that value was null.
        /// </summary>
        /// <remarks>
        /// The captured prior is written back unconditionally. It is never tested for null first:
        /// a null prior is a real state that must be restored, and skipping the write for it would
        /// leak an installed dispatcher into every later test on the same process-global static.
        /// A second call is a no-op, so the scope is safe inside a <c>using</c> statement that also
        /// disposes explicitly.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            DispatcherField.SetValue(null, _prior);
            _prior = null;
            _disposed = true;
        }

        private static FieldInfo ResolveDispatcherField()
        {
            FieldInfo field = typeof(UiThread).GetField(
                "_dispatcher",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            field.Should().NotBeNull(because: "UiThread._dispatcher backing field must exist");
            return field;
        }
    }

#nullable restore annotations
}

#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Svg;

namespace SVGControl
{
    internal class SvgRenderer : INotifyPropertyChanged
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private const string ParseFailed = "SvgRenderer could not parse the SVG payload: ";

        // Svg 3.4.7 was compiled against ExCSS 4.2.3.0 but the repo deploys ExCSS 4.3.1.0
        // (same publicKeyToken). Production resolves this via TaskMaster.exe.config binding
        // redirects, but vstest's testhost ignores the test DLL's .config in some modes, so
        // SvgDocument.Open throws FileNotFoundException for ExCSS 4.2.3. The exception is
        // swallowed by GetSvgDocument and surfaces downstream as an NRE in the SvgRenderer
        // ctor. Register an AssemblyResolve fallback that satisfies any version request
        // for an already-loaded assembly with a matching simple name + public key token.
        private static int _resolverInstalled;

        [ThreadStatic]
        private static HashSet<string>? _resolving;

        static SvgRenderer()
        {
            if (Interlocked.Exchange(ref _resolverInstalled, 1) == 0)
            {
                AppDomain.CurrentDomain.AssemblyResolve += ResolveByNameAndKey;
            }
        }

        private static System.Reflection.Assembly? ResolveByNameAndKey(
            object sender,
            ResolveEventArgs args
        )
        {
            var requested = new System.Reflection.AssemblyName(args.Name);
            byte[] requestedKey = requested.GetPublicKeyToken();
            foreach (var loaded in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var loadedName = loaded.GetName();
                if (
                    !string.Equals(
                        loadedName.Name,
                        requested.Name,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }
                byte[] loadedKey = loadedName.GetPublicKeyToken();
                if (PublicKeyTokensEqual(loadedKey, requestedKey))
                {
                    return loaded;
                }
            }

            // No loaded match — fall back to loading by simple name from the probing path.
            // This recovers cases where a versioned reference (e.g., ExCSS 4.2.3) is being
            // requested but only a newer same-key version is deployed alongside the test DLL.
            // Re-entrance guard prevents infinite recursion when Assembly.Load itself fails
            // and re-raises AssemblyResolve on this thread.
            _resolving ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!_resolving.Add(requested.Name))
            {
                return null;
            }
            try
            {
                // Strategy 2 — load by simple name from the probing path.
                try
                {
                    var name = new System.Reflection.AssemblyName(requested.Name);
                    var byName = System.Reflection.Assembly.Load(name);
                    byte[]? byNameKey = byName?.GetName().GetPublicKeyToken();
                    if (byName != null && PublicKeyTokensEqual(byNameKey, requestedKey))
                    {
                        return byName;
                    }
                }
                // Trace, not log4net: log4net inside an AssemblyResolve handler can itself trigger a
                // re-entrant assembly load, so this diagnostic must not depend on it being loadable.
                catch (Exception ex)
                {
                    Trace.TraceWarning(
                        $"SvgRenderer load '{requested.Name}': {DescribeFailure(ex)}"
                    );
                }

                // Strategy 3 — probe candidate directories for a same-key file on disk. Ordered after
                // strategies 1 and 2 so an already-loaded match always wins over a fresh LoadFrom.
                var self = typeof(SvgRenderer).Assembly;
                IReadOnlyList<string> probeDirectories = SvgAssemblyProbe.GetProbeDirectories(
                    self.Location,
                    self.CodeBase,
                    AppDomain.CurrentDomain.BaseDirectory
                );
                foreach (string directory in probeDirectories)
                {
                    string path = Path.Combine(directory, requested.Name + ".dll");
                    if (!File.Exists(path))
                    {
                        continue;
                    }
                    // Trace here for the same re-entrancy reason given above.
                    try
                    {
                        var loaded = System.Reflection.Assembly.LoadFrom(path);
                        byte[] loadedFileKey = loaded.GetName().GetPublicKeyToken();
                        if (PublicKeyTokensEqual(loadedFileKey, requestedKey))
                        {
                            return loaded;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning($"SvgRenderer load '{path}': {DescribeFailure(ex)}");
                    }
                }
            }
            finally
            {
                _resolving.Remove(requested.Name);
            }

            return null;
        }

        private static bool PublicKeyTokensEqual(byte[]? a, byte[]? b)
        {
            if (a == null || b == null)
            {
                return a == b || (a != null && a.Length == 0) || (b != null && b.Length == 0);
            }
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        public SvgRenderer(byte[] doc, Size size, AutoSize autoSize)
        {
            if (TryGetSvgDocument(doc, out SvgDocument? parsed, out Exception? error))
            {
                _doc = parsed;
                _original = parsed!.Draw().Size;
            }
            else
            {
                // Issue #418: degrade instead of raising. The swallowed null used to be dereferenced
                // one line later, surfacing as an opaque NRE inside the WinForms designer.
                string detail = "SvgRenderer(byte[], Size, AutoSize): " + DescribeFailure(error);
                logger.Error(detail, error);
                Trace.TraceError(detail);
                _original = Size.Empty;
            }
            _margin = new Padding(0);
            Size = CalcInnerSize(size, _margin);
            _autoSize = autoSize;
        }

        public SvgRenderer(byte[] doc, Size size, Padding margin, AutoSize autoSize)
        {
            if (TryGetSvgDocument(doc, out SvgDocument? parsed, out Exception? error))
            {
                _doc = parsed;
                _original = parsed!.Draw().Size;
            }
            else
            {
                // See the other byte[]-doc constructor for the degrade-and-log rationale.
                string detail =
                    "SvgRenderer(byte[], Size, Padding, AutoSize): " + DescribeFailure(error);
                logger.Error(detail, error);
                Trace.TraceError(detail);
                _original = Size.Empty;
            }
            _margin = margin;
            Size = CalcInnerSize(size, _margin);
            _autoSize = autoSize;
        }

        // Renders a failure for a log record. A null error is the element-free case, where the parser
        // reports failure by returning no document rather than by raising.
        private static string DescribeFailure(Exception? error)
        {
            return error == null
                ? "the payload contained no SVG elements."
                : error.GetType().FullName + ": " + error.Message;
        }

        public SvgRenderer(SvgDocument doc, Size size, AutoSize autoSize)
        {
            _doc = doc;
            _original = _doc.Draw().Size;
            _margin = new Padding(0);
            Size = CalcInnerSize(size, _margin);
            _autoSize = autoSize;
        }

        public SvgRenderer(SvgDocument doc, Size size, Padding margin, AutoSize autoSize)
        {
            _doc = doc;
            _original = _doc.Draw().Size;
            _margin = margin;
            Size = CalcInnerSize(size, _margin);
            _autoSize = autoSize;
        }

        public SvgRenderer(Size outer, Padding margin, AutoSize autoSize)
        {
            _outer = outer;
            Margin = margin;
            AutoSize = autoSize;
            Size = CalcInnerSize(outer, margin);
            //logger.Debug("SvgRenderer Initialized");
        }

        private Size _outer;
        private Size _original;
        private Padding _margin;
        private SvgDocument? _doc;
        private AutoSize _autoSize;
        private Size _size;

        [NotifyParentProperty(true)]
        internal Size Outer
        {
            get { return _outer; }
            set
            {
                _outer = value;
                Size = CalcInnerSize(Outer, _margin);
                NotifyPropertyChanged("Outer");
            }
        }

        [NotifyParentProperty(true)]
        public Size Size
        {
            get => _size;
            set => _size = value;
        }

        [NotifyParentProperty(true)]
        public Padding Margin
        {
            get { return _margin; }
            set
            {
                _margin = value;
                Size = CalcInnerSize(Outer, _margin);
                NotifyPropertyChanged("Margin");
            }
        }

        [NotifyParentProperty(true)]
        [DefaultValue(AutoSize.MaintainAspectRatio)]
        public AutoSize AutoSize
        {
            get => _autoSize;
            set => _autoSize = value;
        }

        [NotifyParentProperty(true)]
        public SvgDocument? Document
        {
            get => _doc;
            set
            {
                _doc = value;
                if (value != null)
                {
                    // _doc == value here (just assigned above); the null-forgiving operator
                    // reflects the guard on `value` that the compiler cannot see through the
                    // field assignment.
                    _original = _doc!.Draw().Size;
                }
                NotifyPropertyChanged();
            }
        }

        private Size CalcInnerSize(Size outer, Padding margin)
        {
            var innerWidth = outer.Width - margin.Left - margin.Right;
            var innerHeight = outer.Height - margin.Top - margin.Bottom;
            return new Size(innerWidth, innerHeight);
        }

        public Bitmap? Render()
        {
            if (_doc == null)
            {
                return null;
            }
            else if (
                (AutoSize == AutoSize.Disabled)
                || (Size == null)
                || (Size.Height == 0)
                || (Size.Width == 0)
            )
            {
                return _doc.Draw();
            }
            else if (AutoSize == AutoSize.AllowStretching)
            {
                _doc.Width = Size.Width;
                _doc.Height = Size.Height;
                return _doc.Draw();
            }
            else if (AutoSize == AutoSize.MaintainAspectRatio)
            {
                var targetAdjusted = AdjustSizeProportionately(_original, Size);
                _doc.Width = targetAdjusted.Width;
                _doc.Height = targetAdjusted.Height;
                //AddMargins(targetAdjusted.Width, targetAdjusted.Height);
                return _doc.Draw();
            }
            else
            {
                return null;
            }
        }

        private void AddMargins(int widthCurrent, int heightCurrent)
        {
            // _doc is expected to be set by the time this (currently unreferenced) helper runs;
            // preserves the pre-existing implicit non-null assumption in this method.
            var group = new SvgGroup();
            _doc!.Children.Add(group);
            group.Children.Add(
                new SvgRectangle
                {
                    X = -_margin.Left,
                    Y = -_margin.Top,
                    Width = widthCurrent + Margin.Left + Margin.Right,
                    Height = heightCurrent + Margin.Top + Margin.Bottom,
                    Stroke = new SvgColourServer(Color.Transparent),
                    Fill = new SvgColourServer(Color.Transparent),
                }
            );
        }

        private Size AdjustSizeProportionately(Size proportions, Size targetSize)
        {
            if (
                (targetSize.Height > 0)
                && (targetSize.Width > 0)
                && (
                    (proportions.Height != targetSize.Height)
                    || (proportions.Width != targetSize.Width)
                )
            )
            {
                int widthAspect = (int)(
                    targetSize.Height * proportions.Width / (double)proportions.Height
                );
                if (widthAspect < targetSize.Width)
                {
                    return new Size(widthAspect, targetSize.Height);
                }
                else
                {
                    int heightAspect = (int)(
                        targetSize.Width * proportions.Height / (double)proportions.Width
                    );
                    return new Size(targetSize.Width, heightAspect);
                }
            }
            return proportions;
        }

        // Can return null in principle. That path is driven in tests through the injected parse
        // delegate on TryGetSvgDocument; whether a well-formed-XML-but-no-SVG-element payload
        // reaches it here is unmeasured (open question U-3). An empty payload does not: it raises
        // XmlException. No handler here by design: TryGetSvgDocument is the boundary.
        internal static SvgDocument? OpenFromBytes(byte[] file)
        {
            using (var stream = new MemoryStream(file))
            {
                return SvgDocument.Open<SvgDocument>(stream);
            }
        }

        // Single parse-failure boundary for this type: every failure mode becomes a false result plus
        // an optionally captured exception, so no caller can silently lose a diagnostic. The parse
        // delegate is the seam that lets tests assert exact exception identity without global state.
        internal static bool TryGetSvgDocument(
            byte[] file,
            Func<byte[], SvgDocument?> parse,
            out SvgDocument? document,
            out Exception? error
        )
        {
            if (file == null || parse == null)
            {
                throw new ArgumentNullException(file == null ? nameof(file) : nameof(parse));
            }
            try
            {
                document = parse(file);
                error = null;
                if (document != null)
                {
                    return true;
                }
                // Element-free input makes the parser return null without raising: nothing to hand back.
                string empty = ParseFailed + DescribeFailure(null);
                logger.Error(empty);
                Trace.TraceError(empty);
                return false;
            }
            catch (Exception ex)
            {
                // Dual channel on purpose: log4net for production, Trace because no log4net appender
                // is known to be configured inside the designer host (devenv.exe).
                string detail = ParseFailed + DescribeFailure(ex);
                logger.Error(detail, ex);
                Trace.TraceError(detail);
                document = null;
                error = ex;
                return false;
            }
        }

        /// <summary>
        /// Production entry point for the try-style parse. Surfaces the captured exception so a caller
        /// can report the real cause instead of inferring it from a null result.
        /// </summary>
        public static bool TryGetSvgDocument(
            byte[] file,
            out SvgDocument? document,
            out Exception? error
        )
        {
            return TryGetSvgDocument(file, OpenFromBytes, out document, out error);
        }

        /// <summary>
        /// Fail-fast counterpart to GetSvgDocument. InnerException carries the original parser
        /// exception when one exists, and is null for the element-free case.
        /// </summary>
        public static SvgDocument GetSvgDocumentOrThrow(byte[] file)
        {
            if (TryGetSvgDocument(file, out SvgDocument? document, out Exception? error))
            {
                return document!; // non-null on the true branch by TryGetSvgDocument's contract
            }
            throw new InvalidOperationException(ParseFailed + DescribeFailure(error), error);
        }

        /// <summary>
        /// Tolerant parse kept for consumers that treat a missing document as a normal state. Use
        /// TryGetSvgDocument or GetSvgDocumentOrThrow when the cause matters.
        /// </summary>
        public static SvgDocument? GetSvgDocument(byte[] file)
        {
            TryGetSvgDocument(file, out SvgDocument? document, out _);
            return document;
        }

        #region EventHandlers

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}

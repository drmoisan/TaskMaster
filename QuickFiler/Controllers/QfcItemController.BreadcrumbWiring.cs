using QuickFiler.Viewers;

namespace QuickFiler.Controllers
{
    internal partial class QfcItemController
    {
        // #351: idempotently creates the host-neutral breadcrumb pipeline on the concrete viewer
        // so folder population/selection are correct even before WebView2 core init completes.
        // The 9101 provider is DI-resolved from the injected globals' folder-tree service seam —
        // no live Outlook query is issued inside breadcrumb code (G6). Skipped for mock viewers
        // (unit tests drive the coordinator directly through its own seams).
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal void EnsureBreadcrumbPipeline()
        {
            if (!(_itemViewer is ItemViewer viewer))
            {
                return;
            }

            if (viewer.BreadcrumbCoordinator == null)
            {
                var provider = new UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider(
                    _globals.Ol.FolderTreeService,
                    () => _globals.Ol.ArchiveRootPath
                );
                viewer.InitializeBreadcrumbPipeline(provider);
            }

            if (!ReferenceEquals(_breadcrumbViewer, viewer))
            {
                if (_breadcrumbViewer != null)
                {
                    _breadcrumbViewer.BreadcrumbUnhandledArrow -= OnBreadcrumbUnhandledArrow;
                }
                _breadcrumbViewer = viewer;
                _breadcrumbViewer.BreadcrumbUnhandledArrow -= OnBreadcrumbUnhandledArrow;
                _breadcrumbViewer.BreadcrumbUnhandledArrow += OnBreadcrumbUnhandledArrow;
            }
        }
    }
}

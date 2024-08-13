using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using UtilitiesCS.Extensions.Lazy;


namespace UtilitiesCS.OneDriveHelpers
{
    public class AngleSharpParsedEmailBody 
    {
        public AngleSharpParsedEmailBody(string html)
        {
            Parser = new HtmlParser();
            _document = new Lazy<IHtmlDocument>(() => Parser.ParseDocument(html));
        }

        private string _html;
        public string Html { get => _html; set => _html = value; }

        private HtmlParser _parser;
        public HtmlParser Parser { get => _parser; protected set => _parser = value; }

        private Lazy<IHtmlDocument> _document;
        public IHtmlDocument Document { get => _document.Value; protected set => _document = value.ToLazy(); }

        private IEnumerable<(string, string)> _links;
        public virtual IEnumerable<(string, string)> Links { get => _links; protected set => _links = value; }

        private IEnumerable<(string, string)> _filteredLinks;
        public IEnumerable<(string, string)> FilteredLinks { get => _filteredLinks; protected set => _filteredLinks = value; }

        public AngleSharpParsedEmailBody ExtractLinks()
        {
            Links = Document.QuerySelectorAll("a")
                            .OfType<IHtmlAnchorElement>()
                            .Select(anchor => (anchor.Text, anchor.Href))
                            .Where(tup => !string.IsNullOrEmpty(tup.Href));
            return this;
        }

        public AngleSharpParsedEmailBody FilterLinksByDomain(string domain)
        {
            Links ??= ExtractLinks().Links;
            if (Links is null) { return null; }
            FilteredLinks = Links.Where(tup => Uri.TryCreate(tup.Item2, UriKind.Absolute, out var uri) && uri.Host.Contains(domain));
            return this;
        }

        

        

    }
}


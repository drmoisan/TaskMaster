using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Tesseract;
using UtilitiesCS.Extensions;


namespace UtilitiesCS.EmailIntelligence
{
    public class EmailTokenizer
    {
        #region Constructors and Initializers

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EmailTokenizer() 
        {
            setup();
        }

        //public EmailTokenizer(IApplicationGlobals appGlobals) { _globals = appGlobals; }

        /// <summary>
        /// Get the tokenizer ready to use; this should be 
        /// called after all options have been set.
        /// </summary>
        public void setup()
        {
            // We put this here, rather than in __init__, so that this can be
            // done after we set options at runtime (since the tokenizer
            // instance is generally created when this module is imported)
            if (SpamBayesOptions.basic_header_tokenize)
            {
                throw new NotImplementedException("basic_header_tokenize");
                // Documentation from the Python version of SpamBayes:
                // If true, tokenizer.Tokenizer.tokenize_headers() will tokenize the
                // contents of each header field just like the text of the message
                // body, using the name of the header as a tag.Tokens look like
                // "header:word".The basic approach is simple and effective, but also
                // very sensitive to biases in the ham and spam collections.For
                // example, if the ham and spam were collected at different times,
                // several headers with date / time information will become the best
                // discriminators.  (Not just Date, but Received and X - From_
            }

            crack_images = new ImageStripper().analyze;
        }
        //private IApplicationGlobals _globals;

        #endregion Constructors and Initializers

        #region Constants

        private Regex fname_sep_re = new Regex(@"[\\\/\:]");
        private Regex urlsep_re = new Regex(@"[;?:@&=+,$.]");
        private Regex date_hms_re = new Regex(@"(?'hour'[0-9][0-9]):(?'minute'[0-9][0-9])(?::[0-9][0-9])? ");
        private Regex subject_word_re = new Regex(@"[\w\x80-\xff$.%]+");
        //private Regex punctuation_run_re = new Regex(@"\W+"); // original eliminated because it captures white spaces and needs to start with 2 consecutive to be a run
        private Regex punctuation_run_re = new Regex(@"\p{P}{2,}");
        private Regex numeric_entity_re = new Regex(@"&#(\d+);");
        private Regex virus_re = new Regex(@"""
    < /? \s* (?: script | iframe) \b
|   \b src= ['""]? cid:
|   \b (?: height | width) = ['""]? 0
""", RegexOptions.Compiled);
        //private Regex whitespace_split_re = new Regex(@"\s+"); // original. Doesn't eliminate punctuation from tokens
        private Regex whitespace_split_re = new Regex(@"\p{P}*\s+");

        private string[] date_formats = new string[]
        {
            "%a, %d %b %Y %H:%M:%S (%Z)",
            "%a, %d %b %Y %H:%M:%S %Z",
            "%d %b %Y %H:%M:%S (%Z)",
            "%d %b %Y %H:%M:%S %Z",
            "%a, %d %b %Y %H:%M (%Z)",
            "%a, %d %b %Y %H:%M %Z",
            "%d %b %Y %H:%M (%Z)",
            "%d %b %Y %H:%M %Z"
        };


        #endregion Constants

        #region Main Methods

        public async Task<string[]> tokenizeAsync(object obj, CancellationToken cancel)
        {
            return await Task.Run(() => tokenize(obj).ToArray(), cancel);
        }

        public IEnumerable<string> tokenize(object obj) 
        {
            if (obj is null) { throw new ArgumentNullException("obj"); }
            else if (obj is string[]) { return (string[])obj; }
            else if (obj is MailItemInfo) { return tokenize((MailItemInfo)obj);}
            else if (obj is MailItem) { return tokenize(new MailItemInfo((MailItem)obj)); }
            else
            {
                throw new ArgumentException($"obj type must be {typeof(string[])}, " +
                    $"{typeof(MailItem)}, or {typeof(MailItemInfo)}. {obj.GetType()} " +
                    $"is not supported");
            }
        }

        public IEnumerable<string> tokenize(MailItemInfo msg)
        {
            //var headers = msg.GetHeaders();
            foreach (var tok in this.tokenize_headers(msg))
                yield return tok;
            foreach (var tok in this.tokenize_body(msg))
                yield return tok;
        }

        internal IEnumerable<string> tokenize_headers(MailItemInfo msg)
        {
            // Special tagging of header lines and MIME metadata.

            // Content-{Type, Disposition} and their params, and charsets.
            // This is done for all MIME sections.
            foreach (var w in crack_content_xyz(msg))
                yield return w;

            // Subject:
            // Don't ignore case in Subject lines; e.g., 'free' versus 'FREE' is
            // especially significant in this context.  Experiment showed a small
            // but real benefit to keeping case intact in this specific context.

            var matches = subject_word_re.Matches(msg.Subject);
            foreach (var w in matches)
            {
                foreach (var t in tokenize_word(w.ToString()))
                    yield return "subject:" + t;
            }

            matches = punctuation_run_re.Matches(msg.Subject);
            foreach (var w in matches)
            {
                yield return "subject:"+ w;
            }

            // Dang -- I can't use Sender:.  If I do,
            //     'sender:email name:python-list-admin'
            // becomes the most powerful indicator in the whole database.
            //
            // From:         # this helps both rates
            // Reply-To:     # my error rates are too low now to tell about this
            //               # one (smalls wins & losses across runs, overall
            //               # not significant), so leaving it out
            // To:, Cc:      # These can help, if your ham and spam are sourced
            //               # from the same location. If not, they'll be horrible.

            List<(string field, RecipientInfo value)> addrlist = [];
            addrlist.Add(("from", msg.Sender));
            msg.ToRecipients.ForEach(x => addrlist.Add(("to", x)));
            msg.CcRecipients.ForEach(x => addrlist.Add(("cc", x)));

            foreach (var (field, value) in addrlist)
            {
                yield return $"{field}:name:{value?.Name?.ToLower() ?? "empty"}";
                
                var address = value?.Address?.ToLower() ?? "empty";
                foreach (var w in address.Split('@'))
                    yield return $"{field}:addr:{w}";
            }

            // Spammers sometimes send out mail alphabetically to fairly large
            // numbers of addresses.  This results in headers like:
            // To: <itinerart@videotron.ca>
            // Cc: <itinerant@skyful.com>, <itinerant@netillusions.net>,
            //       <itineraries@musi-cal.com>, <itinerario@rullet.leidenuniv.nl>,
            //       <itinerance@sorengo.com>
            //
            // This token attempts to exploit that property.  The above would
            // give a common prefix of "itinera" for 6 addresses, yielding a
            // gross score of 42.  We group scores into buckets by dividing by 10
            // to yield a final token value of "pfxlen:04".  The length test
            // eliminates the bad case where the message was sent to a single
            // individual.

            IEnumerable<string> all_addrs = null;
            
            if (SpamBayesOptions.summarize_email_prefixes) 
            {
                if (all_addrs is null) { all_addrs = addrlist.Select(x => x.value?.Address?.ToLower() ?? ""); }

                if (all_addrs.Count() > 1)
                {
                    var pfx = commonprefix(all_addrs);
                    if (pfx != "") 
                    { 
                        var score = pfx.Length * all_addrs.Count() / 10;
                        // After staring at pfxlen:* values generated from a large
                        // number of ham & spam I saw that any scores greater
                        // than 3 were always associated with spam.  Collapsing
                        // all such scores into a single token avoids a bunch of
                        // hapaxes like "pfxlen:28".

                        if (score > 3)
                        {
                            yield return "pfxlen:big";
                        }
                        else 
                        {
                            yield return $"pfxlen:{score / 10 * 10:00}";
                        }
                    }
                }
            }

            //# same idea as above, but works for addresses in the same domain
            //# like
            //# To: "skip" <bugs@mojam.com>, <chris@mojam.com>,
            //#       <concertmaster@mojam.com>, <concerts@mojam.com>,
            //#       <design@mojam.com>, <rob@mojam.com>, <skip@mojam.com>
            if (SpamBayesOptions.summarize_email_suffixes)
            {
                if (all_addrs is null) { all_addrs = addrlist.Select(x => x.value.Address.ToLower()); }

                if (all_addrs.Count() > 1)
                {
                    var sfx = commonsuffix(all_addrs);
                    if (sfx != "")
                    {
                        var score = sfx.Length * all_addrs.Count() / 10;
                        // Similar analysis as above regarding suffix length
                        // I suspect the best cutoff is probably dependent on
                        // how long the recipient domain is (e.g. "mojam.com" vs.
                        // "montanaro.dyndns.org")

                        if (score > 7)
                        {
                            yield return "sfxlen:big";
                        }
                        else
                        {
                            yield return $"sfxlen:{score / 10 * 10:00}";
                        }
                    }
                }
            }

            //# To:
            //# Cc:
            //# Count the number of addresses in each of the recipient headers.

            var tocount = msg.ToRecipients.Count();
            if (tocount > 0)
                yield return $"to:2**{Math.Round(Math.Log(tocount,2))}";

            var cccount = msg.ToRecipients.Count();
            if (cccount > 0)
                yield return $"to:2**{Math.Round(Math.Log(cccount, 2))}";

            
        }
                
        /// <summary>
        /// Generate a stream of tokens from an email Message.
        /// If options['Tokenizer', 'check_octets'] is True, the first few 
        /// undecoded characters of application/octet-stream parts of the
        /// message body become tokens.
        /// </summary>
        /// <param name="msg">MailItemInfo wrapper with email and metadata</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal IEnumerable<string> tokenize_body(MailItemInfo msg)
        {
            if (SpamBayesOptions.check_octets)
            {
                // Find, decode application/octet-stream parts of the body,
                // tokenizing the first few characters of each chunk.
                throw new NotImplementedException("check_octets");
                // Python code below
                // for part in octetparts(msg):
                //     try:
                //         text = part.get_payload(decode = True)
                //     except:
                //         yield "control: couldn't decode octet"
                //         text = part.get_payload(decode = False)

                //     if text is None:
                //         yield "control: octet payload is None"
                //         continue

                //     yield "octet:%s" % text[:options["Tokenizer",
                //                                      "octet_prefix_size"]]

            }

            var parts = imageparts(msg);
            if (SpamBayesOptions.image_size)
            {
                // Find image/* parts of the body, calculating the log(size) of
                // each image.
                //
                // Note: this version achieves same outcome by different means
                // using outlook attachment metadata 
                var total_len = 0;
                foreach (var part in parts)
                {
                    if (part is Attachment attachment)
                        total_len += attachment.Size;
                }
                if (total_len > 0)
                {
                    yield return $"image-size:2**{Math.Round(Math.Log(total_len, 2))}";
                }
            }

            if (SpamBayesOptions.crack_images)
            {
                var engine_name = "Tesseract";
                var (texts, tokens) = crack_images(engine_name, parts);
                foreach (var t in tokens)
                    yield return t;
                foreach (var t in this.tokenize_text(texts))
                    yield return t;
            }

            // Find, decode (base64, qp), and tokenize textual parts of the body.
            foreach (string part in textparts(msg))
            {
                // Decode, or take it as-is if decoding fails.
                string text = part;
                if (text is null)
                {
                    yield return "control: text payload is None";
                    continue;
                }

                //# Replace numeric character entities (like &#97; for the letter
                //# 'a').
                text = numeric_entity_re.Replace(text, NumericEntityReplacer);

                //# Normalize case.
                text = text.ToLower();

                if (SpamBayesOptions.replace_nonascii_chars)
                {
                    //# Translate accented characters to non-accented and Replace non-ascii characters .
                    text = text.StripAccents('?');
                }

                foreach (var t in find_html_virus_clues(msg.Item.HTMLBody))
                    yield return $"virus:{t}";

                foreach (var t in this.tokenize_text(text))
                    yield return t;



            }

        }

        #endregion Main Methods

        #region Helper Methods

        private IEnumerable<string> textparts(MailItemInfo msg)
        {
            yield return msg.Body;
        }

        /// <summary>
        /// Tokenize everything in the chunk of text we were handed.
        /// </summary>
        /// <param name="texts"></param>
        /// <returns></returns>
        private IEnumerable<string> tokenize_text(string texts)
        {

            var short_runs = new HashSet<int>();
            var short_count = 0;
            var words = whitespace_split_re.Split(texts);
            foreach (var w in words)
            {
                var n = w.Length;
                if (n < 3)
                {
                    //# count how many short words we see in a row - meant to
                    //# latch onto crap like this:
                    //# X j A m N j A d X h
                    //# M k E z R d I p D u I m A c
                    //# C o I d A t L j I v S j
                    short_count += 1;
                }
                else
                {
                    if (short_count > 0)
                    {
                        short_runs.Add(short_count);
                        short_count = 0;
                    }
                    //# Make sure this range matches in tokenize_word().
                    if (3 <= n && n <= SpamBayesOptions.skip_max_word_size)
                        yield return w;
                    else if (n >= 3)
                    {
                        foreach (var t in tokenize_word(w))
                            yield return t;
                    }
                }
            }

            if (short_runs.Count > 0 && SpamBayesOptions.x_short_runs)
                yield return $"short:{Math.Round(Math.Log(short_runs.Max(),2),0):N2}";
            
        }

        public string commonprefix(IEnumerable<string> strings) 
        { 
            var pfx = string.Join("", strings
                .Transpose()
                .TakeWhile(s => s.All(d => d == s.First()))
                .Select(s => s.First()));
            return pfx;
        }

        public string commonsuffix(IEnumerable<string> strings)
        {
            var commonLength = strings.Select(s => s.Length).Min();
            var sfx = string.Join("", strings
                .Select(x => x.Substring(x.Length - commonLength))
                .Transpose()
                .TakeWhile(s => s.All(d => d == s.Last()))
                .Select(s => s.Last()));
            return sfx;
        }

        // Port of the tokenizer from the Python version of SpamBayes
        internal IEnumerable<string> crack_filename(string fname)
        {
            yield return "fname:" + fname;
            var components = fname_sep_re.Split(fname);
            var morethan1 = components.Length > 1;
            foreach (var component in components)
            {
                if (morethan1)
                {
                    yield return "fname comp:" + component;
                }
                var pieces = urlsep_re.Split(component);
                if (pieces.Length > 1)
                {
                    foreach (var piece in pieces)
                    {
                        yield return "fname piece:" + piece;
                    }
                }
            }

        }

        internal bool has_highbit_char(string word)
        {
            var rx = new Regex(@"[\x80-\xff]");
            return rx.IsMatch(word);
        }

        internal List<object> imageparts(MailItemInfo msg)
        {
            var attachments = msg.Attachments;
            var parts = msg.Attachments.Where(x => x.IsImage).Select(x => x.Attachment).Cast<object>().ToList();
            return parts;
            // Original Python code below
            // # Return a list of all msg parts with type 'image/*'.
            // return [part for part in msg.walk() if part.get_content_type().startswith('image/')]
        }
        
        internal IEnumerable<string> tokenize_word(
            string word, 
            Func<string,int> _len = null, 
            int maxword = SpamBayesOptions.skip_max_word_size)
        {
            // Workaround for C# not allowing default delegate parameters.
            if (_len == null)
                _len = (x) => x.Length;

            var n = _len(word);
            // Make sure this range matches in tokenize().
            if (3 <= n && n <= maxword) 
                yield return word;

            else if (n >= 3)
            {
                // A long word.

                // Don't want to skip embedded email addresses.
                // An earlier scheme also split up the y in x@y on '.'.  Not splitting
                // improved the f-n rate; the f-p rate didn't care either way.
                if (n < 40 && word.Contains(".") && word.Count(x => x == '@') == 1)
                {
                    var parts = word.Split('@');
                    var p1 = parts[0];
                    var p2 = parts[1];
                    yield return "email name:" + p1;
                    yield return "email domain:" + p2;
                }
                else 
                {
                    // There's value in generating a token indicating roughly how
                    // many chars were skipped.  This has real benefit for the f-n
                    // rate, but is neutral for the f-p rate.  I don't know why!
                    // XXX Figure out why, and/or see if some other way of summarizing
                    // XXX this info has greater benefit.
                    if (SpamBayesOptions.generate_long_skips)
                    {
                        yield return string.Format("skip:{0} {1}", word[0], n / 10 * 10);
                    }
                    if (has_highbit_char(word))
                    {
                        var hicount = 0;
                        foreach (int i in word.Select(c => (int)c))
                        {
                            if (i >= 128)
                            {
                                hicount += 1;
                            }
                        }
                        yield return string.Format("8bit%%:{0}", Math.Round(hicount * 100.0 / word.Length));
                    }
                }
            }
        }

        internal Func<string, List<object>, (string texts, HashSet<string> tokens)> crack_images;

        internal static List<CharsetCodebase> charsetCodebases = 
            JsonExtensions.Deserialize<List<CharsetCodebase>>(
                Properties.Resources.charset_lookup);

        /// <summary>
        /// Original code used MIME headers to extract certain information.
        /// This version is designed to extract similar information through
        /// alternate means using the Outlook.<seealso cref="MailItem"/> object model.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal IEnumerable<string> crack_content_xyz(MailItemInfo itemInfo)
        {
            // content-type not clearly defined in Outlook object model
            // need to convert to System.Net.Mail.MailMessage to get this info
            // MimeKit seems like a promising library to explore later
            
            var codePage = itemInfo.Item.InternetCodepage;
            var charset = charsetCodebases.FirstOrDefault(x => x.Codepage == codePage).Charset;
            yield return $"charset:{charset}";
                        
            var attachments = itemInfo.Item.Attachments.Cast<Attachment>();
            foreach (var attachment in attachments)
            {
                var fname = attachment.FileName;
                if (fname.IsNullOrEmpty())
                    yield return "filename:<bogus>";
                else
                {
                    foreach (var x in crack_filename(fname))
                        yield return "filename:" + x;
                }
            }

            // Original Python code below
            //  yield 'content-type:' + msg.get_content_type()
            //
            //x = msg.get_param('type')
            //if x is not None:
            //    yield 'content-type/type:' + x.lower()
            //
            //try:
            //    for x in msg.get_charsets(None):
            //        if x is not None:
            //            yield 'charset:' + x.lower()
            //except UnicodeEncodeError:
            //    # Bad messages can cause an exception here.
            //    # See [ 1175439 ] UnicodeEncodeError raised for bogus Content-Type
            //    #                 header
            //    yield 'charset:invalid_unicode'
            //
            //x = msg.get('content-disposition')
            //if x is not None:
            //    yield 'content-disposition:' + x.lower()
            //
            //try:
            //    fname = msg.get_filename()
            //    if fname is not None:
            //        for x in crack_filename(fname):
            //            yield 'filename:' + x
            //except TypeError:
            //    # bug in email pkg?  see the thread beginning at
            //    # http://mail.python.org/pipermail/spambayes/2003-September/008006.html
            //    # and
            //    # http://mail.python.org/pipermail/spambayes-dev/2003-September/001177.html
            //    yield "filename:<bogus>"
            //
            //if 0:   # disabled; see comment before function
            //    x = msg.get('content-transfer-encoding')
            //    if x is not None:
            //        yield 'content-transfer-encoding:' + x.lower()
            
        }

        string NumericEntityReplacer(Match m)
        {
            try
            {
                return ((char)int.Parse(m.Groups[1].Value)).ToString();
            }
            catch
            {
                return "?";
            }
        }

        IEnumerable<string> find_html_virus_clues(string text)
        {
            var matches = virus_re.Matches(text);
            foreach (Match match in matches)
            {
                yield return match.Value;
            }
        }

        #endregion Helper Methods
    }

    public struct SpamBayesOptions
    {
        public const int skip_max_word_size = 12;
        public const bool generate_long_skips = true;
        public const bool basic_header_tokenize = false;
        public const bool check_octets = false;
        public const bool image_size = true;
        public const bool crack_images = true;
        public const int max_image_size = 1000000;
        public const bool summarize_email_prefixes = true;
        public const bool summarize_email_suffixes = true;
        public const bool replace_nonascii_chars = true;
        public const bool x_short_runs = true;
    }

    public class CharsetCodebase 
    {
        public CharsetCodebase() { }
        public string Name;
        public long Codepage;
        public string Charset;
    }
}

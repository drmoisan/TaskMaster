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
using System.Xml.Schema;
using Tesseract;
using UtilitiesCS.Extensions;


namespace UtilitiesCS.EmailIntelligence
{
    internal class EmailTokenizer
    {
        #region Constructors and Initializers

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EmailTokenizer(IApplicationGlobals appGlobals) { _globals = appGlobals; }

        /// <summary>
        /// Get the tokenizer ready to use; this should be 
        /// called after all options have been set.
        /// </summary>
        private void setup()
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

            crack_images = new ImageStripper(_globals).analyze;
        }
        private IApplicationGlobals _globals;

        #endregion Constructors and Initializers

        #region Constants

        private Regex fname_sep_re = new Regex(@"[\\\/\:]");
        private Regex urlsep_re = new Regex(@"[;?:@&=+,$.]");
        private Regex date_hms_re = new Regex(@"(?'hour'[0-9][0-9]):(?'minute'[0-9][0-9])(?::[0-9][0-9])? ");
        private Regex subject_word_re = new Regex(@"[\w\x80-\xff$.%]+");
        private Regex punctuation_run_re = new Regex(@"\W+");

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

        public IEnumerable<string> tokenize(MailItemInfo msg)
        {
            var headers = msg.GetHeaders();
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
            
            foreach (var w in subject_word_re.Matches(msg.Subject))
            {
                foreach (var t in tokenize_word(w.ToString()))
                    yield return "subject:" + t;
            }

            foreach (var w in punctuation_run_re.Matches(msg.Subject))
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
            yield return $"from:name:{msg.SenderName.ToLower()}";

            throw new NotImplementedException("tokenize_headers");
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
                    var len = part.Size;
                    total_len += len;
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
            
            throw new NotImplementedException("tokenize_body");

            // Find, decode (base64, qp), and tokenize textual parts of the body.
            foreach (var part in textparts(msg))
            {
                // Decode, or take it as-is if decoding fails.
                var text = "";// part.Text;
                if (text is null)
                {
                    yield return "control: text payload is None";
                    continue;
                }
                foreach (var t in this.tokenize_text(text))
                    yield return t;
            }

        }

        #endregion Main Methods

        #region Helper Methods

        private IEnumerable<string> textparts(MailItemInfo msg)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<string> tokenize_text(string texts)
        {
            throw new NotImplementedException();
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

        internal List<Attachment> imageparts(MailItemInfo msg)
        {
            var parts = msg.Attachments.Where(x => x.IsImage).Select(x => x.Attachment);
            return parts.ToList();
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

        internal Func<string, List<Attachment>, (string texts, HashSet<string> tokens)> crack_images;

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
    }

    public class CharsetCodebase 
    {
        public CharsetCodebase() { }
        public string Name;
        public long Codepage;
        public string Charset;
    }
}

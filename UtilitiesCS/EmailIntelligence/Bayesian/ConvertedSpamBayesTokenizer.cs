//using System;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using System.Net;
//using System.IO;
//using System.Linq;
//using System;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using System.Diagnostics;
//using System.Web.Caching;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
//using System.Web.UI.HtmlControls;
//using UtilitiesCS;


//namespace CodeTranslationAssistant
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            var email = new Email();
//            var emailMessage = new EmailMessage();
//            var emailHeader = new EmailHeader();
//            var emailUtils = new EmailUtils();
//            var emailErrors = new EmailErrors();
//            var regex = new Regex();
//            var math = new Math();
//            var os = new OS();
//            var binascii = new Binascii();
//            var urllibParse = new UrllibParse();
//            var urllibRequest = new UrllibRequest();
//            var spambayesClassifier = new SpambayesClassifier();
//            var spambayesOptions = new SpambayesOptions();
//            var spambayesMboxUtils = new SpambayesMboxUtils();
//            try
//            {
//                var spambayesDnscache = new SpambayesDnscache();
//                var cache = spambayesDnscache.Cache(spambayesOptions["Tokenizer", "lookup_ip_cache"]);
//                cache.printStatsAtEnd = false;
//            }
//            catch (IOException e)
//            {
//                var cache = new Cache();
//                cache.lookup = (args) => { return new List<string>(); };
//            }
//            catch (Exception e)
//            {
//                var cache = new Cache();
//                cache.lookup = (args) => { return new List<string>(); };
//            }
//            var encodingsAliases = new EncodingsAliases();
//            if (!encodingsAliases.aliases.ContainsKey("ansi_x3_4_1968"))
//            {
//                encodingsAliases.aliases["ansi_x3_4_1968"] = "ascii";
//            }
//            var textparts = new Func<EmailMessage, HashSet<EmailMessagePart>>(msg =>
//            {
//                return new HashSet<EmailMessagePart>(msg.walk().Where(part => part.get_content_maintype() == "text"));
//            });
//            var octetparts = new Func<EmailMessage, HashSet<EmailMessagePart>>(msg =>
//            {
//                return new HashSet<EmailMessagePart>(msg.walk().Where(part => part.get_content_type() == "application/octet-stream"));
//            });
//            var imageparts = new Func<EmailMessage, List<EmailMessagePart>>(msg =>
//            {
//                return msg.walk().Where(part => part.get_content_type().StartsWith("image/")).ToList();
//            });
//            var has_highbit_char = regex.Match(r"[\x80-\xff]").Success;
//            var html_re = regex.Match(r, RegexOptions.Multiline | RegexOptions.Singleline);
//            var received_host_re = regex.Match(r'from ([a-z0-9._-]+[a-z])[)\s]');
//            var received_ip_re = regex.Match(r'[[(]((\d{1,3}\.?){4})[])]');
//            var received_nntp_ip_re = regex.Match(r'((\d{1,3}\.?){4})');
//            var message_id_re = regex.Match(r'\s*<[^@]+@([^>]+)>\s*');
//            var subject_word_re = regex.Match(r"[\w\x80-\xff$.%]+");
//            var punctuation_run_re = regex.Match(r'\W+');
//            var fname_sep_re = regex.Match(r'[/\\:]');
//            var crack_filename = new Func<string, IEnumerable<string>>(fname =>
//            {
//                yield return "fname:" + fname;
//                var components = fname_sep_re.Split(fname);
//                var morethan1 = components.Length > 1;
//                foreach (var component in components)
//                {
//                    if (morethan1)
//                    {
//                        yield return "fname comp:" + component;
//                    }
//                    var pieces = urlsep_re.Split(component);
//                    if (pieces.Length > 1)
//                    {
//                        foreach (var piece in pieces)
//                        {
//                            yield return "fname piece:" + piece;
//                        }
//                    }
//                }
//            });
//            var tokenize_word = new Func<string, IEnumerable<string>>((word) =>
//            {
//                var n = word.Length;
//                if (3 <= n && n <= maxword)
//                {
//                    yield return word;
//                }
//                else if (n >= 3)
//                {
//                    if (n < 40 && word.Contains('.') && word.Count(c => c == '@') == 1)
//                    {
//                        var parts = word.Split('@');
//                        yield return "email name:" + parts[0];
//                        yield return "email addr:" + parts[1];
//                    }
//                    else
//                    {
//                        if (options["Tokenizer", "generate_long_skips"])
//                        {
//                            yield return "skip:%c %d" + (word[0], n / 10 * 10);
//                        }
//                        if (has_highbit_char)
//                        {
//                            var hicount = word.Count(c => (int)c >= 128);
//                            yield return "8bit%%:%d" + (hicount * 100.0 / word.Length);
//                        }
//                    }
//                }
//            });
//            var non_ascii_translate_tab = new string[256];
//            for (var i = 0; i < 256; i++)
//            {
//                non_ascii_translate_tab[i] = "?";
//            }
//            for (var i = 32; i < 127; i++)
//            {
//                non_ascii_translate_tab[i] = ((char)i).ToString();
//            }
//            foreach (var ch in " \t\r\n")
//            {
//                non_ascii_translate_tab[(int)ch] = ch.ToString();
//            }
//            var crack_content_xyz = new Func<EmailMessage, IEnumerable<string>>(msg =>
//            {
//                yield return "content-type:" + msg.get_content_type();
//                var x = msg.get_param("type");
//                if (x != null)
//                {
//                    yield return "content-type/type:" + x.ToLower();
//                }
//                try
//                {
//                    foreach (var x in msg.get_charsets(null))
//                    {
//                        if (x != null)
//                        {
//                            yield return "charset:" + x.ToLower();
//                        }
//                    }
//                }
//                catch (UnicodeEncodeError e)
//                {
//                    yield return "charset:invalid_unicode";
//                }
//                var x = msg.get("content-disposition");
//                if (x != null)
//                {
//                    yield return "content-disposition:" + x.ToLower();
//                }
//                try
//                {
//                    var fname = msg.get_filename();
//                    if (fname != null)
//                    {
//                        foreach (var x in crack_filename(fname))
//                        {
//                            yield return "filename:" + x;
//                        }
//                    }
//                }
//                catch (TypeError e)
//                {
//                    yield return "filename:<bogus>";
//                }
//                if (0)
//                {
//                    var x = msg.get("content-transfer-encoding");
//                    if (x != null)
//                    {
//                        yield return "content-transfer-encoding:" + x.ToLower();
//                    }
//                }
//            });
//            var try_to_repair_damaged_base64 = new Func<string, string>(text =>
//            {
//                var i = 0;
//                while (true)
//                {
//                    var m = base64_re.Match(text, i);
//                    if (!m.Success)
//                    {
//                        break;
//                    }
//                    i = m.Index + m.Length;
//                    if (m.Groups[1].Success)
//                    {
//                        break;
//                    }
//                }
//                var base64text = "";
//                if (i > 0)
//                {
//                    var base64 = text.Substring(0, i);
//                    try
//                    {
//                        base64text = Convert.FromBase64String(base64);
//                    }
//                    catch (Exception e)
//                    {
//                        // pass
//                    }
//                }
//                return base64text + text.Substring(i);
//            });
//            var breakdown_host = new Func<string, IEnumerable<string>>(host =>
//            {
//                var parts = host.Split('.');
//                for (var i = 1; i <= parts.Length; i++)
//                {
//                    yield return string.Join(".", parts.Skip(parts.Length - i));
//                }
//            });
//            var breakdown_ipaddr = new Func<string, IEnumerable<string>>(ipaddr =>
//            {
//                var parts = ipaddr.Split('.');
//                for (var i = 1; i <= 4; i++)
//                {
//                    yield return string.Join(".", parts.Take(i));
//                }
//            });
//            var log2 = new Func<double, double>(n =>
//            {
//                return math.Log(n) / math.Log(2);
//            });
//            var stripper = new Stripper();
//            var uuencode_begin_re = regex.Match(r, RegexOptions.Multiline);
//            var uuencode_end_re = regex.Match(r"^end\s*\n", RegexOptions.Multiline);
//            var crack_uuencode = new Func<string, Tuple<string, IEnumerable<string>>>(text =>
//            {
//                var i = 0;
//                while (true)
//                {
//                    var m = uuencode_begin_re.Match(text, i);
//                    if (!m.Success)
//                    {
//                        break;
//                    }
//                    var mode = m.Groups[1].Value;
//                    var fname = m.Groups[2].Value;
//                    var tokens = new List<string>();
//                    tokens.Add("uuencode mode:" + mode);
//                    tokens.AddRange(crack_filename(fname).Select(x => "uuencode:" + x));
//                    var result = Tuple.Create(tokens);
//                    i = m.Index + m.Length;
//                    var m = uuencode_end_re.Match(text, i);
//                    if (!m.Success)
//                    {
//                        result.Item1.Add(text.Substring(i));
//                        break;
//                    }
//                    i = m.Index + m.Length;
//                }
//                return result;
//            });
//            var url_fancy_re = regex.Match(r, RegexOptions.Multiline);
//            var url_re = regex.Match(r, RegexOptions.Multiline);
//            var urlsep_re = regex.Match(r"[;?:@&=+,$.]");
//            var urlStripper = new URLStripper();
//            var received_complaints_re = regex.Match(r'\([a-z]+(?:\s+[a-z]+)+\)');

//            Regex received_complaints_re = new Regex(@"\([a-z]+(?:\s+[a-z]+)+\)");

//            if (options["URLRetriever", "x-slurp_urls"])
//            {
//                crack_urls = new SlurpingURLStripper().Analyze;
//            }
//            else
//            {
//                crack_urls = new URLStripper().Analyze;
//            }

//            Regex html_style_start_re = new Regex(r, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

//            crack_html_style = new StyleStripper().Analyze;

//            crack_html_comment = new CommentStripper().Analyze;


//            crack_noframes = new NoframesStripper().Analyze;

//            Regex virus_re = new Regex(r, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
//            IEnumerable<string> FindHtmlVirusClues(string text)
//            {
//                foreach (Match bingo in virus_re.Matches(text))
//                {
//                    yield return bingo.Value;
//                }
//            }

//            Regex numeric_entity_re = new Regex(@"&#(\d+);");
//            string NumericEntityReplacer(Match m)
//            {
//                try
//                {
//                    return ((char)int.Parse(m.Groups[1].Value)).ToString();
//                }
//                catch
//                {
//                    return "?";
//                }
//            }

//            Regex breaking_entity_re = new Regex(r, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

//        }

//        class NoframesStripper : Stripper
//        {
//            public NoframesStripper() : base(new Regex(r"<\s*noframes\s*>").Match, new Regex(r"</noframes\s*>").Match)
//            {
//            }
//        }

//        class CommentStripper : Stripper
//        {
//            public CommentStripper() : base(new Regex(r"<!--|<\s*comment\s*[^>]*>").Match, new Regex(r"-->|</comment>").Match)
//            {
//            }
//        }

//        public class SlurpingURLStripper : URLStripper
//        {
//            public SlurpingURLStripper() : base()
//            {
//            }

//            public override string Analyze(string text)
//            {
//                classifier.slurp_wordstream = null;
//                return base.Analyze(text);
//            }

//            public override List<string> Tokenize(Match m)
//            {
//                List<string> tokens = base.Tokenize(m);
//                if (!options["URLRetriever", "x-slurp_urls"])
//                {
//                    return tokens;
//                }
//                string proto = m.Groups[1].Value;
//                string guts = m.Groups[2].Value;
//                if (proto != "http")
//                {
//                    return tokens;
//                }
//                while (guts.Length > 0 && ".:;?!/)".Contains(guts[guts.Length - 1]))
//                {
//                    guts = guts.Substring(0, guts.Length - 1);
//                }
//                classifier.slurp_wordstream = new Tuple<string, string>(proto, guts);
//                return tokens;
//            }
//        }

//        class StyleStripper : Stripper
//        {
//            public StyleStripper() : base(html_style_start_re.Match, new Regex(r"</style>").Match)
//            {
//            }
//        }

//        class URLStripper : Stripper
//        {
//            public URLStripper() : base()
//            {
//                Regex search;
//                if (options["Tokenizer", "x-fancy_url_recognition"])
//                {
//                    search = url_fancy_re.search;
//                }
//                else
//                {
//                    search = url_re.search;
//                }
//                base.__init__(search, new Regex("").search);
//            }

//            public List<string> tokenize(Match m)
//            {
//                string proto, guts;
//                proto = m.Groups[1].Value;
//                guts = m.Groups[2].Value;
//                Debug.Assert(guts != null);
//                if (proto == null)
//                {
//                    if (guts.ToLower().StartsWith("www"))
//                    {
//                        proto = "http";
//                    }
//                    else if (guts.ToLower().StartsWith("ftp"))
//                    {
//                        proto = "ftp";
//                    }
//                    else
//                    {
//                        proto = "unknown";
//                    }
//                }
//                List<string> tokens = new List<string>();
//                tokens.Add("proto:" + proto);
//                Action<string> pushclue = tokens.Add;
//                if (options["Tokenizer", "x-pick_apart_urls"])
//                {
//                    string url = proto + "://" + guts;
//                    List<string> escapes = new List<string>(Regex.Matches(guts, r'%..'));
//                    if (escapes.Count > 0)
//                    {
//                        pushclue("url:%%" + Math.Log2(escapes.Count));
//                    }
//                    foreach (string escape in escapes)
//                    {
//                        tokens.Add("url:" + escape);
//                    }
//                    url = Uri.UnescapeDataString(url);
//                    Uri uri = new Uri(url);
//                    string scheme = uri.Scheme;
//                    string netloc = uri.Host;
//                    string path = uri.AbsolutePath;
//                    string query = uri.Query;
//                    string frag = uri.Fragment;
//                    if (options["Tokenizer", "x-lookup_ip"])
//                    {
//                        List<string> ips = cache.lookup(netloc);
//                        if (ips.Count == 0)
//                        {
//                            pushclue("url-ip:lookup error");
//                        }
//                        else
//                        {
//                            foreach (string clue in gen_dotted_quad_clues("url-ip", ips))
//                            {
//                                pushclue(clue);
//                            }
//                        }
//                    }
//                    string user_pwd, host_port;
//                    Uri.TryCreate(netloc, UriKind.Absolute, out Uri result);
//                    user_pwd = result.UserInfo;
//                    host_port = result.Authority;
//                    if (user_pwd != null)
//                    {
//                        pushclue("url:has user");
//                    }
//                    string host, port;
//                    Uri.TryCreate(host_port, UriKind.Absolute, out Uri result2);
//                    host = result2.Host;
//                    port = result2.Port.ToString();
//                    if (port != null)
//                    {
//                        if ((scheme == "http" && port != "80") || (scheme == "https" && port != "443"))
//                        {
//                            pushclue("url:non-standard " + scheme + " port");
//                        }
//                    }
//                    if (Regex.IsMatch(host, @"(\d+\.?){4,4}$"))
//                    {
//                        pushclue("url:ip addr");
//                    }
//                    string[] pieces = guts.Split('/');
//                    foreach (string piece in pieces)
//                    {
//                        string[] chunks = urlsep_re.Split(piece);
//                        foreach (string chunk in chunks)
//                        {
//                            pushclue("url:" + chunk);
//                        }
//                    }
//                }
//                return tokens;
//            }
//        }

//        class Tokenizer
//        {
//            Regex date_hms_re = new Regex(r' (?P<hour>[0-9][0-9])'

//                                          r':(?P<minute>[0-9][0-9])'

//                                          r'(?::[0-9][0-9])? ');
//            string[] date_formats = new string[] {
//                    "%a, %d %b %Y %H:%M:%S (%Z)",
//                    "%a, %d %b %Y %H:%M:%S %Z",
//                    "%d %b %Y %H:%M:%S (%Z)",
//                    "%d %b %Y %H:%M:%S %Z",
//                    "%a, %d %b %Y %H:%M (%Z)",
//                    "%a, %d %b %Y %H:%M %Z",
//                    "%d %b %Y %H:%M (%Z)",
//                    "%d %b %Y %H:%M %Z"
//                };

//            public Tokenizer()
//            {
//                Setup();
//            }

//            public void Setup()
//            {
//                if (options["Tokenizer", "basic_header_tokenize"])
//                {
//                    basic_skip = new List<Regex>();
//                    foreach (string s in options["Tokenizer", "basic_header_skip"])
//                    {
//                        basic_skip.Add(new Regex(s));
//                    }
//                }
//            }

//            public string GetMessage(object obj)
//            {
//                return Get_Message(obj);
//            }

//            public IEnumerable<string> Tokenize(object obj)
//            {
//                string msg = GetMessage(obj);
//                foreach (string tok in TokenizeHeaders(msg))
//                {
//                    yield return tok;
//                }
//                foreach (string tok in TokenizeBody(msg))
//                {
//                    yield return tok;
//                }
//            }

//            public IEnumerable<string> TokenizeHeaders(string msg)
//            {
//                foreach (object x in msg.Walk())
//                {
//                    foreach (string w in CrackContentXyz(x))
//                    {
//                        yield return w;
//                    }
//                }
//                if (options["Tokenizer", "basic_header_tokenize"])
//                {
//                    foreach (KeyValuePair<string, string> kvp in msg.Items())
//                    {
//                        string k = kvp.Key.ToLower();
//                        bool skip = false;
//                        foreach (Regex rx in basic_skip)
//                        {
//                            if (rx.Match(k).Success)
//                            {
//                                skip = true;
//                                break;
//                            }
//                        }
//                        if (skip)
//                        {
//                            continue;
//                        }
//                        foreach (Match m in subject_word_re.Matches(kvp.Value))
//                        {
//                            foreach (string t in TokenizeWord(m.Value))
//                            {
//                                yield return $"{k}:{t}";
//                            }
//                        }
//                    }
//                    if (options["Tokenizer", "basic_header_tokenize_only"])
//                    {
//                        yield break;
//                    }
//                }
//                string x = msg.Get("subject", "");
//                try
//                {
//                    List<Tuple<string, string>> subjcharsetlist = email.header.decode_header(x);
//                    foreach (Tuple<string, string> tuple in subjcharsetlist)
//                    {
//                        string name = tuple.Item1;
//                        string charset = tuple.Item2;
//                        if (charset != null)
//                        {
//                            yield return $"subjectcharset:{charset}";
//                        }
//                        name = name.Replace('\r', ' ');
//                        foreach (Match m in subject_word_re.Matches(name))
//                        {
//                            foreach (string t in TokenizeWord(m.Value))
//                            {
//                                yield return $"subject:{t}";
//                            }
//                        }
//                        foreach (Match m in punctuation_run_re.Matches(name))
//                        {
//                            yield return $"subject:{m.Value}";
//                        }
//                    }
//                }
//                catch (Exception)
//                {
//                }
//                foreach (string field in options["Tokenizer", "address_headers"])
//                {
//                    List<string> addrlist = msg.Get_All(field, new List<string>());
//                    if (addrlist.Count == 0)
//                    {
//                        yield return $"{field}:none";
//                        continue;
//                    }
//                    int noname_count = 0;
//                    foreach (string addr in addrlist)
//                    {
//                        string name;
//                        string address;
//                        email.Utils.parseaddr(addr, out name, out address);
//                        if (name != "")
//                        {
//                            try
//                            {
//                                List<Tuple<string, string>> subjcharsetlist = email.header.decode_header(name);
//                                foreach (Tuple<string, string> tuple in subjcharsetlist)
//                                {
//                                    string n = tuple.Item1;
//                                    string c = tuple.Item2;
//                                    yield return $"{field}:name:{n.ToLower()}";
//                                    if (c != null)
//                                    {
//                                        yield return $"{field}:charset:{c}";
//                                    }
//                                }
//                            }
//                            catch (Exception)
//                            {
//                            }
//                        }
//                        else
//                        {
//                            noname_count += 1;
//                        }
//                        if (address != "")
//                        {
//                            foreach (string w in address.ToLower().Split('@'))
//                            {
//                                yield return $"{field}:addr:{w}";
//                            }
//                        }
//                        else
//                        {
//                            yield return $"{field}:addr:none";
//                        }
//                    }
//                    if (noname_count != 0)
//                    {
//                        yield return $"{field}:no real name:2**{Math.Round(Math.Log(noname_count, 2))}";
//                    }
//                }
//                if (options["Tokenizer", "summarize_email_prefixes"])
//                {
//                    List<string> all_addrs = new List<string>();
//                    List<string> addresses = msg.Get_All("to", new List<string>()).Concat(msg.Get_All("cc", new List<string>())).ToList();
//                    foreach (string addr in addresses)
//                    {
//                        all_addrs.Add(addr.ToLower());
//                    }
//                    if (all_addrs.Count > 1)
//                    {
//                        string pfx = all_addrs.Aggregate((x, y) => x.Substring(0, Math.Min(x.Length, y.Length)));
//                        if (pfx != "")
//                        {
//                            int score = (pfx.Length * all_addrs.Count) / 10;
//                            if (score > 3)
//                            {
//                                yield return "pfxlen:big";
//                            }
//                            else
//                            {
//                                yield return $"pfxlen:{score}";
//                            }
//                        }
//                    }
//                }
//                if (options["Tokenizer", "summarize_email_suffixes"])
//                {
//                    List<string> all_addrs = new List<string>();
//                    List<string> addresses = msg.Get_All("to", new List<string>()).Concat(msg.Get_All("cc", new List<string>())).ToList();
//                    foreach (string addr in addresses)
//                    {
//                        List<char> addr_chars = addr.ToList();
//                        addr_chars.Reverse();
//                        all_addrs.Add(new string(addr_chars.ToArray()).ToLower());
//                    }
//                    if (all_addrs.Count > 1)
//                    {
//                        string sfx = all_addrs.Aggregate((x, y) => x.Substring(0, Math.Min(x.Length, y.Length)));
//                        if (sfx != "")
//                        {
//                            int score = (sfx.Length * all_addrs.Count) / 10;
//                            if (score > 5)
//                            {
//                                yield return "sfxlen:big";
//                            }
//                            else
//                            {
//                                yield return $"sfxlen:{score}";
//                            }
//                        }
//                    }
//                }
//                foreach (string field in new string[] { "to", "cc" })
//                {
//                    int count = 0;
//                    foreach (string addrs in msg.Get_All(field, new List<string>()))
//                    {
//                        count += addrs.Split(',').Length;
//                    }
//                    if (count > 0)
//                    {
//                        yield return $"{field}:2**{Math.Round(Math.Log(count, 2))}";
//                    }
//                }
//                foreach (string field in new string[] { "x-mailer" })
//                {
//                    string prefix = $"{field}:";
//                    string x = msg.Get(field, "none").ToLower();
//                    yield return $"{prefix}{string.Join(" ", x.Split())}";
//                }
//                if (options["Tokenizer", "mine_received_headers"])
//                {
//                    foreach (string header in msg.Get_All("received", new List<string>()))
//                    {
//                        string header_lower = header.ToLower();
//                        foreach (Match m in received_complaints_re.Matches(header_lower))
//                        {
//                            yield return $"received:{m.Value}";
//                        }
//                        foreach (Tuple<Regex, Func<Match, IEnumerable<string>>> tuple in new Tuple<Regex, Func<Match, IEnumerable<string>>>[] {
//                                new Tuple<Regex, Func<Match, IEnumerable<string>>>(received_host_re, breakdown_host),
//                                new Tuple<Regex, Func<Match, IEnumerable<string>>>(received_ip_re, breakdown_ipaddr)
//                            })
//                        {
//                            Match m = tuple.Item1.Match(header_lower);
//                            if (m.Success)
//                            {
//                                foreach (string tok in tuple.Item2(m))
//                                {
//                                    yield return $"received:{tok}";
//                                }
//                            }
//                        }
//                    }
//                }
//                if (options["Tokenizer", "x-mine_nntp_headers"])
//                {
//                    foreach (string clue in MineNntp(msg))
//                    {
//                        yield return clue;
//                    }
//                }
//                string msgid = msg.Get("message-id", "");
//                Match m = message_id_re.Match(msgid);
//                if (m.Success)
//                {
//                    yield return $"message-id:@{m.Groups[1].Value}";
//                }
//                else
//                {
//                    yield return "message-id:invalid";
//                }
//                Dictionary<string, int> x2n = new Dictionary<string, int>();
//                if (options["Tokenizer", "count_all_header_lines"])
//                {
//                    foreach (string x in msg.Keys())
//                    {
//                        if (!x2n.ContainsKey(x))
//                        {
//                            x2n[x] = 0;
//                        }
//                        x2n[x] += 1;
//                    }
//                }
//                else
//                {
//                    List<string> safe_headers = options["Tokenizer", "safe_headers"];
//                    foreach (string x in msg.Keys())
//                    {
//                        if (safe_headers.Contains(x.ToLower()))
//                        {
//                            if (!x2n.ContainsKey(x))
//                            {
//                                x2n[x] = 0;
//                            }
//                            x2n[x] += 1;
//                        }
//                    }
//                }
//                foreach (KeyValuePair<string, int> kvp in x2n)
//                {
//                    yield return $"header:{kvp.Key}:{kvp.Value}";
//                }
//                if (options["Tokenizer", "record_header_absence"])
//                {
//                    foreach (string k in x2n.Keys)
//                    {
//                        if (!options["Tokenizer", "safe_headers"].Contains(k.ToLower()))
//                        {
//                            yield return $"noheader:{k}";
//                        }
//                    }
//                }
//            }

//            public IEnumerable<string> TokenizeText(string text, int maxword = options["Tokenizer", "skip_max_word_size"])
//            {
//                HashSet<int> short_runs = new HashSet<int>();
//                int short_count = 0;
//                foreach (string w in text.Split())
//                {
//                    int n = w.Length;
//                    if (n < 3)
//                    {
//                        short_count += 1;
//                    }
//                    else
//                    {
//                        if (short_count != 0)
//                        {
//                            short_runs.Add(short_count);
//                            short_count = 0;
//                        }
//                        if (3 <= n && n <= maxword)
//                        {
//                            yield return w;
//                        }
//                        else if (n >= 3)
//                        {
//                            foreach (string t in TokenizeWord(w))
//                            {
//                                yield return t;
//                            }
//                        }
//                    }
//                }
//                if (short_runs.Count != 0 && options["Tokenizer", "x-short_runs"])
//                {
//                    yield return $"short:{Math.Round(Math.Log(short_runs.Max(), 2))}";
//                }
//            }

//            public IEnumerable<string> TokenizeBody(string msg)
//            {
//                if (options["Tokenizer", "check_octets"])
//                {
//                    foreach (object part in octetparts(msg))
//                    {
//                        try
//                        {
//                            string text = part.Get_Payload(decode: true);
//                            if (text == null)
//                            {
//                                yield return "control: octet payload is None";
//                                continue;
//                            }
//                            yield return $"octet:{text.Substring(0, options["Tokenizer", "octet_prefix_size"])}";
//                        }
//                        catch (Exception)
//                        {
//                            yield return "control: couldn't decode octet";
//                        }
//                    }
//                }
//                List<object> parts = imageparts(msg);
//                if (options["Tokenizer", "image_size"])
//                {
//                    int total_len = 0;
//                    foreach (object part in parts)
//                    {
//                        try
//                        {
//                            string text = part.Get_Payload(decode: true);
//                            if (text == null)
//                            {
//                                yield return "control: image payload is None";
//                                continue;
//                            }
//                            total_len += text.Length;
//                        }
//                        catch (Exception)
//                        {
//                            yield return "control: couldn't decode image";
//                        }
//                    }
//                    if (total_len != 0)
//                    {
//                        yield return $"image-size:2**{Math.Round(Math.Log(total_len, 2))}";
//                    }
//                }
//                if (options["Tokenizer", "crack_images"])
//                {
//                    string engine_name = options["Tokenizer", "ocr_engine"];
//                    string text;
//                    List<string> tokens;
//                    crack_images(engine_name, parts, out text, out tokens);
//                    foreach (string t in tokens)
//                    {
//                        yield return t;
//                    }
//                    foreach (string t in TokenizeText(text))
//                    {
//                        yield return t;
//                    }
//                }
//                foreach (object part in textparts(msg))
//                {
//                    try
//                    {
//                        string text = part.Get_Payload(decode: true);
//                        if (text == null)
//                        {
//                            yield return "control: payload is None";
//                            continue;
//                        }
//                        text = numeric_entity_re.Replace(text, NumericEntityReplacer);
//                        text = text.ToLower();
//                        if (options["Tokenizer", "replace_nonascii_chars"])
//                        {
//                            text = text.Translate(non_ascii_translate_tab);
//                        }
//                        foreach (string t in FindHtmlVirusClues(text))
//                        {
//                            yield return $"virus:{t}";
//                        }
//                        foreach (string t in crack_uuencode(text))
//                        {
//                            yield return t;
//                        }
//                        foreach (string t in crack_urls(text))
//                        {
//                            yield return t;
//                        }
//                        foreach (string t in crack_html_style(text))
//                        {
//                            yield return t;
//                        }
//                        foreach (string t in crack_html_comment(text))
//                        {
//                            yield return t;
//                        }
//                        foreach (string t in crack_noframes(text))
//                        {
//                            yield return t;
//                        }
//                        text = breaking_entity_re.Replace(text, " ");
//                        text = html_re.Replace(text, "");
//                        foreach (string t in TokenizeText(text))
//                        {
//                            yield return t;
//                        }
//                    }
//                    catch (Exception)
//                    {
//                        yield return "control: couldn't decode";
//                    }
//                }
//            }

//            public IEnumerable<string> MineNntp(object msg)
//            {
//                List<string> nntp_headers = msg.Get_All("nntp-posting-host", new List<string>());
//                foreach (string address in nntp_headers)
//                {
//                    if (received_nntp_ip_re.Match(address).Success)
//                    {
//                        foreach (string clue in GenDottedQuadClues("nntp-host", new List<string> { address }))
//                        {
//                            yield return clue;
//                        }
//                        List<string> names = cache.lookup(address);
//                        if (names.Count != 0)
//                        {
//                            yield return "nntp-host-ip:has-reverse";
//                            yield return $"nntp-host-name:{names[0]}";
//                            yield return $"nntp-host-domain:{string.Join(".", names[0].Split('.').Skip(Math.Max(0, names[0].Split('.').Length - 2)))}";
//                        }
//                    }
//                    else
//                    {
//                        string name = address;
//                        yield return $"nntp-host-name:{name}";
//                        yield return $"nntp-host-domain:{string.Join(".", name.Split('.').Skip(Math.Max(0, name.Split('.').Length - 2)))}";
//                        List<string> addresses = cache.lookup(name);
//                        if (addresses.Count != 0)
//                        {
//                            foreach (string clue in GenDottedQuadClues("nntp-host-ip", addresses))
//                            {
//                                yield return clue;
//                            }
//                            if (cache.lookup(addresses[0], qType: "PTR") == name)
//                            {
//                                yield return "nntp-host-ip:has-reverse";
//                            }
//                        }
//                    }
//                }
//            }

//            public IEnumerable<string> GenDottedQuadClues(string pfx, List<string> ips)
//            {
//                foreach (string ip in ips)
//                {
//                    yield return $"{pfx}:{ip}/32";
//                    string[] dottedQuadList = ip.Split(".");
//                    yield return $"{pfx}:{dottedQuadList[0]}/8";
//                    yield return $"{pfx}:{dottedQuadList[0]}.{dottedQuadList[1]}/16";
//                    yield return $"{pfx}:{dottedQuadList[0]}.{dottedQuadList[1]}.{dottedQuadList[2]}/24";
//                }
//            }
//        }

//        class Email
//        {
//        }

//        class EmailMessage
//        {
//            public HashSet<EmailMessagePart> walk()
//            {
//                throw new NotImplementedException();
//            }

//            public string get_content_maintype()
//            {
//                throw new NotImplementedException();
//            }

//            public string get_content_type()
//            {
//                throw new NotImplementedException();
//            }

//            public string get_param(string type)
//            {
//                throw new NotImplementedException();
//            }

//            public IEnumerable<string> get_charsets(object p)
//            {
//                throw new NotImplementedException();
//            }

//            public string get(string p)
//            {
//                throw new NotImplementedException();
//            }

//            public string get_filename()
//            {
//                throw new NotImplementedException();
//            }
//        }

//        class EmailMessagePart
//        {
//            public string get_content_maintype()
//            {
//                throw new NotImplementedException();
//            }

//            public string get_content_type()
//            {
//                throw new NotImplementedException();
//            }
//        }

//        class EmailHeader
//        {
//        }

//        class EmailUtils
//        {
//        }

//        class EmailErrors
//        {
//        }

//        class Regex
//        {
//            public bool Match(string input, string pattern)
//            {
//                throw new NotImplementedException();
//            }
//        }

//        class Math
//        {
//            public double Log(double n)
//            {
//                throw new NotImplementedException();
//            }
//        }

//        class OS
//        {
//        }

//        class Binascii
//        {
//        }

//        class UrllibParse
//        {
//        }

//        class UrllibRequest
//        {
//        }

//        class SpambayesClassifier
//        {
//        }

//        class SpambayesOptions
//        {
//        }

//        class SpambayesMboxUtils
//        {
//        }

//        class SpambayesDnscache
//        {
//            public Cache Cache(string cachefile)
//            {
//                throw new NotImplementedException();
//            }
//        }

//        class Cache
//        {
//            public List<string> lookup(string[] args)
//            {
//                throw new NotImplementedException();
//            }
//        }

//        class EncodingsAliases
//        {
//            public Dictionary<string, string> aliases { get; set; }
//        }
//    }
//}

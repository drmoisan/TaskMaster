class SlurpingURLStripper extends URLStripper {
    constructor() {
        super();
        URLStripper.prototype.__init__.call(this);
    }
    analyze(text) {
        classifier.slurp_wordstream = null;
        return URLStripper.prototype.analyze.call(this, text);
    }
    tokenize(m) {
        let tokens = URLStripper.prototype.tokenize.call(this, m);
        if (!options["URLRetriever", "x-slurp_urls"]) {
            return tokens;
        }
        let proto, guts;
        [proto, guts] = m.groups();
        if (proto !== "http") {
            return tokens;
        }
        assert(guts);
        while (guts && guts[-1] in '.:;?!/)') {
            guts = guts.slice(0, -1);
        }
        classifier.slurp_wordstream = [proto, guts];
        return tokens;
    }
}
let crack_urls;
if (options["URLRetriever", "x-slurp_urls"]) {
    crack_urls = new SlurpingURLStripper().analyze;
} else {
    crack_urls = URLStripper.prototype.analyze;
}

let html_style_start_re = new RegExp(r, "VERBOSE");
class StyleStripper extends Stripper {
    constructor() {
        super(html_style_start_re.search, re.compile(r"</style>").search);
    }
}
let crack_html_style = new StyleStripper().analyze;

class CommentStripper extends Stripper {
    constructor() {
        super(re.compile(r"<!--|<\s*comment\s*[^>]*>").search, re.compile(r"-->|</comment>").search);
    }
}
let crack_html_comment = new CommentStripper().analyze;

class NoframesStripper extends Stripper {
    constructor() {
        super(re.compile(r"<\s*noframes\s*>").search, re.compile(r"</noframes\s*>").search);
    }
}
let crack_noframes = new NoframesStripper().analyze;
let virus_re = new RegExp(r, "VERBOSE");
function* find_html_virus_clues(text) {
    for (let bingo of virus_re.findall(text)) {
        yield bingo;
    }
}
let numeric_entity_re = new RegExp('&');
function numeric_entity_replacer(m) {
    try {
        return String.fromCharCode(parseInt(m.group(1)));
    } catch {
        return '?';
    }
}
let breaking_entity_re = new RegExp(r, "VERBOSE");
class Tokenizer {
    constructor() {
        this.setup();
    }
    setup() {
        if (options["Tokenizer", "basic_header_tokenize"]) {
            this.basic_skip = options["Tokenizer", "basic_header_skip"].map(s => new RegExp(s));
        }
    }
    get_message(obj) {
        return get_message(obj);
    }
    *tokenize(obj) {
        let msg = this.get_message(obj);
        for (let tok of this.tokenize_headers(msg)) {
            yield tok;
        }
        for (let tok of this.tokenize_body(msg)) {
            yield tok;
        }
    }
    *tokenize_headers(msg) {
        for (let x of msg.walk()) {
            for (let w of crack_content_xyz(x)) {
                yield w;
            }
        }
        if (options["Tokenizer", "basic_header_tokenize"]) {
            for (let [k, v] of Object.entries(msg.items())) {
                k = k.toLowerCase();
                for (let rx of this.basic_skip) {
                    if (rx.match(k)) {
                        break;
                    }
                }
                else {
                    for (let w of subject_word_re.findall(v)) {
                        for (let t of tokenize_word(w)) {
                            yield `${k}:${t}`;
                        }
                    }
                }
            }
            if (options["Tokenizer", "basic_header_tokenize_only"]) {
                return;
            }
        }
        let x = msg.get('subject', '');
        let subjcharsetlist;
        try {
            subjcharsetlist = email.header.decode_header(x);
        } catch (e) {
            subjcharsetlist = [[x, 'invalid']];
        }
        for (let [x, subjcharset] of subjcharsetlist) {
            if (subjcharset !== null) {
                yield `subjectcharset:${subjcharset}`;
            }
            x = x.replace('\r', ' ');
            for (let w of subject_word_re.findall(x)) {
                for (let t of tokenize_word(w)) {
                    yield `subject:${t}`;
                }
            }
            for (let w of punctuation_run_re.findall(x)) {
                yield `subject:${w}`;
            }
        }
        for (let field of options["Tokenizer", "address_headers"]) {
            let addrlist = msg.get_all(field, []);
            if (!addrlist) {
                yield `${field}:none`;
                continue;
            }
            let noname_count = 0;
            for (let [name, addr] of email.utils.getaddresses(addrlist)) {
                if (name) {
                    try {
                        subjcharsetlist = email.header.decode_header(name);
                    } catch (e) {
                        subjcharsetlist = [[name, 'invalid']];
                    }
                    for (let [name, charset] of subjcharsetlist) {
                        yield `${field}:name:${name.toLowerCase()}`;
                        if (charset !== null) {
                            yield `${field}:charset:${charset}`;
                        }
                    }
                } else {
                    noname_count += 1;
                }
                if (addr) {
                    for (let w of addr.toLowerCase().split('@')) {
                        yield `${field}:addr:${w}`;
                    }
                } else {
                    yield `${field}:addr:none`;
                }
            }
            if (noname_count) {
                yield `${field}:no real name:2**${Math.round(Math.log2(noname_count))}`;
            }
        }
        if (options["Tokenizer", "summarize_email_prefixes"]) {
            let all_addrs = [];
            let addresses = msg.get_all('to', []).concat(msg.get_all('cc', []));
            for (let [name, addr] of email.Utils.getaddresses(addresses)) {
                all_addrs.push(addr.toLowerCase());
            }
            if (all_addrs.length > 1) {
                let pfx = os.path.commonprefix(all_addrs);
                if (pfx) {
                    let score = (pfx.length * all_addrs.length) / 10;
                    if (score > 3) {
                        yield "pfxlen:big";
                    } else {
                        yield `pfxlen:${score}`;
                    }
                }
            }
        }
        if (options["Tokenizer", "summarize_email_suffixes"]) {
            let all_addrs = [];
            let addresses = msg.get_all('to', []).concat(msg.get_all('cc', []));
            for (let [name, addr] of email.Utils.getaddresses(addresses)) {
                addr = addr.split('').reverse().join('');
                all_addrs.push(addr.toLowerCase());
            }
            if (all_addrs.length > 1) {
                let sfx = os.path.commonprefix(all_addrs);
                if (sfx) {
                    let score = (sfx.length * all_addrs.length) / 10;
                    if (score > 5) {
                        yield "sfxlen:big";
                    } else {
                        yield `sfxlen:${score}`;
                    }
                }
            }
        }
        for (let field of ['to', 'cc']) {
            let count = 0;
            for (let addrs of msg.get_all(field, [])) {
                count += addrs.split(',').length;
            }
            if (count > 0) {
                yield `${field}:2**${Math.round(Math.log2(count))}`;
            }
        }
        for (let field of ['x-mailer']) {
            let prefix = `${field}:`;
            let x = msg.get(field, 'none').toLowerCase();
            yield `${prefix}${x.split(' ').join()}`;
        }
        if (options["Tokenizer", "mine_received_headers"]) {
            for (let header of msg.get_all("received", [])) {
                header = header.split(' ').join().toLowerCase();
                for (let clue of received_complaints_re.findall(header)) {
                    yield `received:${clue}`;
                }
                for (let [pat, breakdown] of [[received_host_re, breakdown_host], [received_ip_re, breakdown_ipaddr]]) {
                    let m = pat.search(header);
                    if (m) {
                        for (let tok of breakdown(m.group(1))) {
                            yield `received:${tok}`;
                        }
                    }
                }
            }
        }
        if (options["Tokenizer", "x-mine_nntp_headers"]) {
            for (let clue of mine_nntp(msg)) {
                yield clue;
            }
        }
        let msgid = msg.get("message-id", "");
        let m = message_id_re.match(msgid);
        if (m) {
            yield `message-id:@${m.group(1)}`;
        } else {
            yield 'message-id:invalid';
        }
        let x2n = {};
        if (options["Tokenizer", "count_all_header_lines"]) {
            for (let x of Object.keys(msg)) {
                x2n[x] = (x2n[x] || 0) + 1;
            }
        } else {
            let safe_headers = options["Tokenizer", "safe_headers"];
            for (let x of Object.keys(msg)) {
                if (safe_headers.includes(x.toLowerCase())) {
                    x2n[x] = (x2n[x] || 0) + 1;
                }
            }
        }
        for (let [x, n] of Object.entries(x2n)) {
            yield `header:${x}:${n}`;
        }
        if (options["Tokenizer", "record_header_absence"]) {
            for (let k of Object.keys(x2n)) {
                if (!options["Tokenizer", "safe_headers"].includes(k.toLowerCase())) {
                    yield `noheader:${k}`;
                }
            }
        }
    }
    *tokenize_text(text, maxword = options["Tokenizer", "skip_max_word_size"]) {
        let short_runs = new Set();
        let short_count = 0;
        for (let w of text.split(' ')) {
            let n = w.length;
            if (n < 3) {
                short_count += 1;
            } else {
                if (short_count) {
                    short_runs.add(short_count);
                    short_count = 0;
                }
                if (3 <= n <= maxword) {
                    yield w;
                } else if (n >= 3) {
                    for (let t of tokenize_word(w)) {
                        yield t;
                    }
                }
            }
        }
        if (short_runs && options["Tokenizer", "x-short_runs"]) {
            yield `short:${Math.log2(Math.max(...short_runs))}`;
        }
    }
    *tokenize_body(msg) {
        if (options["Tokenizer", "check_octets"]) {
            for (let part of octetparts(msg)) {
                try {
                    let text = part.get_payload(decode=True);
                } catch {
                    yield "control: couldn't decode octet";
                    let text = part.get_payload(decode=False);
                }
                if (text === null) {
                    yield "control: octet payload is None";
                    continue;
                }
                yield `octet:${text.slice(0, options["Tokenizer", "octet_prefix_size"])}`;
            }
        }
        let parts = imageparts(msg);
        if (options["Tokenizer", "image_size"]) {
            let total_len = 0;
            for (let part of parts) {
                try {
                    let text = part.get_payload(decode=True);
                } catch {
                    yield "control: couldn't decode image";
                    let text = part.get_payload(decode=False);
                }
                total_len += (text || "").length;
                if (text === null) {
                    yield "control: image payload is None";
                }
            }
            if (total_len) {
                yield `image-size:2**${Math.round(Math.log2(total_len))}`;
            }
        }
        if (options["Tokenizer", "crack_images"]) {
            let engine_name = options["Tokenizer", 'ocr_engine'];
            let [text, tokens] = crack_images(engine_name, parts);
            for (let t of tokens) {
                yield t;
            }
            for (let t of this.tokenize_text(text)) {
                yield t;
            }
        }
        for (let part of textparts(msg)) {
            try {
                let text = part.get_payload(decode=True);
            } catch {
                yield "control: couldn't decode";
                let text = part.get_payload(decode=False);
                if (text !== null) {
                    text = try_to_repair_damaged_base64(text);
                }
            }
            if (text === null) {
                yield 'control: payload is None';
                continue;
            }
            text = numeric_entity_re.sub(numeric_entity_replacer, text);
            text = text.toLowerCase();
            if (options["Tokenizer", "replace_nonascii_chars"]) {
                text = text.translate(non_ascii_translate_tab);
            }
            for (let t of find_html_virus_clues(text)) {
                yield `virus:${t}`;
            }
            for (let cracker of [crack_uuencode, crack_urls, crack_html_style, crack_html_comment, crack_noframes]) {
                let [text, tokens] = cracker(text);
                for (let t of tokens) {
                    yield t;
                }
            }
            text = breaking_entity_re.sub(' ', text);
            text = html_re.sub('', text);
            for (let t of this.tokenize_text(text)) {
                yield t;
            }
        }
    }
}
function* mine_nntp(msg) {
    let nntp_headers = msg.get_all("nntp-posting-host", []);
    for (let address of nntp_headers) {
        if (received_nntp_ip_re.match(address)) {
            for (let clue of gen_dotted_quad_clues("nntp-host", [address])) {
                yield clue;
            }
            let names = cache.lookup(address);
            if (names) {
                yield 'nntp-host-ip:has-reverse';
                yield `nntp-host-name:${names[0]}`;
                yield `nntp-host-domain:${names[0].split('.').slice(-2).join('.')}`;
            }
        } else {
            let name = address;
            yield `nntp-host-name:${name}`;
            yield `nntp-host-domain:${name.split('.').slice(-2).join('.')}`;
            let addresses = cache.lookup(name);
            if (addresses) {
                for (let clue of gen_dotted_quad_clues("nntp-host-ip", addresses)) {
                    yield clue;
                }
                if (cache.lookup(addresses[0], qType="PTR") === name) {
                    yield 'nntp-host-ip:has-reverse';
                }
            }
        }
    }
}
function* gen_dotted_quad_clues(pfx, ips) {
    for (let ip of ips) {
        yield `${pfx}:${ip}/32`;
        let dottedQuadList = ip.split(".");
        yield `${pfx}:${dottedQuadList[0]}/8`;
        yield `${pfx}:${dottedQuadList[0]}.${dottedQuadList[1]}/16`;
        yield `${pfx}:${dottedQuadList[0]}.${dottedQuadList[1]}.${dottedQuadList[2]}/24`;
    }
}



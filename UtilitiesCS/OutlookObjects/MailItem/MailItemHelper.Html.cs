using System;
using System.Text.RegularExpressions;

namespace UtilitiesCS
{
    public partial class MailItemHelper
    {
        #region HTML and Plain Text Methods

        internal static string CompressPlainText(string text, string emailPrefixToStrip)
        {
            return CompressPlainText(
                text ?? "",
                IItemInfo.PlainTextOptionsEnum.StripAll,
                emailPrefixToStrip ?? ""
            );
        }

        internal static string CompressPlainText(
            string text,
            IItemInfo.PlainTextOptionsEnum options,
            string emailPrefixToStrip
        )
        {
            if (
                options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripWarning)
                && emailPrefixToStrip != ""
            )
                text = text.Replace(emailPrefixToStrip, "");

            if (options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripLinks))
            {
                var replacementText = "";
                if (options.HasFlag(IItemInfo.PlainTextOptionsEnum.ShowStripped))
                    replacementText = "<link>";
                text = Regex.Replace(text, @"<https://[^>]+>", replacementText);
            }

            if (
                options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripReplyHeader)
                || options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripReplyBody)
            )
            {
                var replacementText = "";
                if (
                    options.HasFlag(
                        IItemInfo.PlainTextOptionsEnum.ShowStripped
                            | IItemInfo.PlainTextOptionsEnum.StripReplyHeader
                    ) && !options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripReplyBody)
                )
                    replacementText = "<EOM> Chain: $3";
                else if (!options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripReplyHeader))
                    replacementText += "$1";
                else if (!options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripReplyBody))
                    replacementText += "$3";

                text = Regex.Replace(
                    text,
                    @"(From:([^\n]*\n){1,4}Subject: {0,1}[rR][eE]:.*)(.|\n|\r)*\z",
                    replacementText
                );
            }

            if (options.HasFlag(IItemInfo.PlainTextOptionsEnum.StripFormatting))
                text = Regex.Replace(text, @"[\s]", " ");
            text = Regex.Replace(text, @"[ ]{2,}", " ");
            text = text.Trim();
            text += " <EOM>";
            return text;
        }

        internal string EmailHeader2
        {
            get =>
                @"
<p class=MsoNormal style='margin-left:225.0pt;text-indent:-225.0pt;tab-stops:
225.0pt;mso-layout-grid-align:none;text-autospace:none'><b><span
style='color:black'>From:<span style='mso-tab-count:1'> </span></span></b><span
style='color:black'>"
                + this.SenderName
                + @"<o:p></o:p></span></p>

<p class=MsoNormal style='margin-left:225.0pt;text-indent:-225.0pt;tab-stops:
225.0pt;mso-layout-grid-align:none;text-autospace:none'><b><span
style='color:black'>Sent:<span style='mso-tab-count:1'> </span></span></b><span
style='color:black'>"
                + this.SentOn
                + @"<o:p></o:p></span></p>

<p class=MsoNormal style='margin-left:225.0pt;text-indent:-225.0pt;tab-stops:
225.0pt;mso-layout-grid-align:none;text-autospace:none'><b><span
style='color:black'>To:<span style='mso-tab-count:1'> </span></span></b><span
style='color:black'>"
                + this.ToRecipientsName
                + @"<o:p></o:p></span></p>

<p class=MsoNormal style='margin-left:225.0pt;text-indent:-225.0pt;tab-stops:
225.0pt;mso-layout-grid-align:none;text-autospace:none'><b><span
style='color:black'>Subject:<span style='mso-tab-count:1'></span></span></b><span
style='color:black'>"
                + this.Subject
                + @"<o:p></o:p></span></p>

<p class=MsoNormal><o:p>&nbsp;</o:p></p>";
        }

#nullable enable
        private string? _emailHeader = null;
        internal string EmailHeader
        {
            get
            {
                if (_emailHeader is null)
                {
                    _emailHeader =
                        @"
    <div>
		<div style=""font-family:Calibri,serif;border-right:none;border-bottom:1pt solid rgb(225,225,225);border-left:none;border-top:none;padding:3pt 0in 0in"">
			<p class=""MsoNormal"">
				<b>From:</b>"
                        + this.SenderHtml
                        + @"<br>
				<b>Sent:</b>"
                        + this.SentOn
                        + @"<br>
				<b>To:</b>"
                        + this.ToRecipientsHtml
                        + @"<br>
				<b>Cc:</b>"
                        + this.CcRecipientsHtml
                        + @"<br>
				<b>Subject:</b>"
                        + this.Subject
                        + @"
			</p>
		</div>
	</div>
";
                }
                return _emailHeader;
            }
        }

#nullable disable

        internal string DarkModeHeader
        {
            get =>
                @"
<style>
body { filter: invert(100%) }
* { backdrop-filter: invert(20%) }
img {
    -webkit-filter: invert(100%) !important;
    -moz-filter: invert(100%) !important;
    -o-filter: invert(100%) !important;
    -ms-filter: invert(100%) !important;
}
</style>
";
        }

        public string ToggleDark()
        {
            if (_darkMode == Enums.ToggleState.On)
            {
                return ToggleDark(Enums.ToggleState.Off);
            }

            return ToggleDark(Enums.ToggleState.On);
        }

        public string ToggleDark(Enums.ToggleState desiredState)
        {
            if ((desiredState == Enums.ToggleState.On) && _darkMode == Enums.ToggleState.Off)
            {
                _darkMode = Enums.ToggleState.On;
                var regex = new Regex(@"(</head>)", RegexOptions.Multiline);
                Html = regex.Replace(Html, DarkModeHeader + "$1");
            }
            else if ((desiredState == Enums.ToggleState.Off) && _darkMode == Enums.ToggleState.On)
            {
                _darkMode = Enums.ToggleState.Off;
                var regex = new Regex(Regex.Escape(DarkModeHeader), RegexOptions.Multiline);
                Html = regex.Replace(Html, "");
            }
            return Html;
        }

        internal string GetHtml()
        {
            string body = _item.HTMLBody;
            var regex = new Regex(@"(<body[\S\s]*?>)", RegexOptions.Multiline);
            string revisedBody = regex.Replace(body, "$1" + EmailHeader);
            Sw?.LogDuration("GetHtml");
            return revisedBody;
        }

        internal string GetHtml(string htmlBody)
        {
            string body = _item.HTMLBody;
            var regex = new Regex(@"(<body[\S\s]*?>)", RegexOptions.Multiline);
            string revisedBody = regex.Replace(body, "$1" + EmailHeader);
            return revisedBody;
        }

        #endregion
    }
}

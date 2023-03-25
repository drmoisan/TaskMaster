using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
//using Microsoft.VisualBasic;
//using Microsoft.VisualBasic.CompilerServices;
using UtilitiesVB;

namespace QuickFiler
{

    public class cInfoMail
    {
        public string Subject;
        private DateTime _endDate;
        public DateTime StartDate;
        private long _durationSec;
        public string SentTo;
        public string SentCC;
        public string SentFrom;
        public string Body;
        public OlImportance Importance;
        public string Categories;
        public string strAction;
        public string strProcName;
        private Collection _col;
        private Dictionary<string, long> _dict;


        /// <summary>
        /// This is actually a reverse sort. 
        /// </summary>
        private void ReverseSortDictionary()
        {
            _dict = (from entry in _dict orderby entry.Value descending select entry).ToDictionary();
        }

        public void dict_new()
        {
            _dict = new Dictionary<string, long>();
        }
        public void dict_add(string strKey, long lngVal)
        {
            // _col.ADD lngVal, strKey
            _dict.Add(strKey, lngVal);
        }

        public int dict_ct
        {
            get
            {
                return _dict.Count;
            }
        }

        public string dict_strSum
        {
            get
            {
                string dict_strSumRet = default;
                int i;
                i = 0;
                if (_dict.Count == 0)
                {
                    dict_strSumRet = "";
                }
                else
                {
                    ReverseSortDictionary();
                    // Sort_Collections.sort _col, New Sort_CReverseComparator
                    dict_strSumRet = "Grouped Apps: ";
                    foreach (var key in _dict.Keys)
                    {
                        i = i + 1;
                        if (i < 3)
                        {
                            if (i > 1)
                                dict_strSumRet = dict_strSumRet + " | ";
                            dict_strSumRet = dict_strSumRet + key + " " + Strings.Format(_dict[key] / 60d, "#,##0.0") + " min";
                        }
                    }
                }

                return dict_strSumRet;
            }
        }

        public void dict_upORadd(string strKey, long lngVal)
        {
            if (_dict.ContainsKey(strKey))
            {
                _dict[strKey] = _dict[strKey] + lngVal;
            }
            else
            {
                _dict.Add(strKey, lngVal);
            }
        }
        internal object Init(
            string lcl_Subject = "",
            DateTime lcl_EndDate = default,
            DateTime lcl_StartDate = default,
            long lcl_DurationSec = 0L,
            string lcl_SentTo = "",
            string lcl_SentCC = "",
            string lcl_SentFrom = "",
            string lcl_Body = "",
            OlImportance lcl_Importance = OlImportance.olImportanceNormal,
            Categories lcl_Categories = null,
            string lcl_strAction = "")
        {
            object InitRet = default;

            Subject = lcl_Subject;
            EndDate = lcl_EndDate;
            StartDate = lcl_StartDate;
            DurationSec = lcl_DurationSec;
            SentTo = lcl_SentTo;
            SentCC = lcl_SentCC;
            SentFrom = lcl_SentFrom;
            Body = lcl_Body;
            Importance = lcl_Importance;
            Categories = lcl_Categories.ToString();
            strAction = lcl_strAction;

            if (Information.Err().Number == 0)
            {
                InitRet = 1;
            }
            else
            {
                InitRet = 0;
            }

            return InitRet;
        }

        internal bool Init_wMail(MailItem OlMail, DateTime OlEndTime = default, long lngDurationSec = 0L, string stringAction = "")
        {
            bool Init_wMailRet = default;
            Subject = OlMail.Subject;
            if (OlEndTime != default)
                EndDate = OlEndTime;
            if (Conversions.ToBoolean(lngDurationSec))
                DurationSec = lngDurationSec;
            SentTo = OlMail.To;
            SentCC = OlMail.CC;
            SentFrom = OlMail.Sender.ToString();
            Body = OlMail.Body;
            Importance = OlMail.Importance;
            Categories = OlMail.Categories;
            if (!string.IsNullOrEmpty(stringAction))
                strAction = stringAction;

            if (Information.Err().Number == 0)
            {
                Init_wMailRet = true;
            }
            else
            {
                Init_wMailRet = false;
                Debug.WriteLine(Information.Err().Description);
                Information.Err().Clear();
            }

            return Init_wMailRet;

        }

        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                _endDate = value;
                StartDate = DateAndTime.DateAdd("s", -_durationSec, _endDate);
            }
        }

        public long DurationSec
        {
            get
            {
                return _durationSec;
            }
            set
            {
                _durationSec = value;
                StartDate = DateAndTime.DateAdd("s", -_durationSec, _endDate);
            }
        }

        public new string ToString
        {
            get
            {
                string ToStringRet = default;
                string strTemp;
                double lngSeconds;
                double lngSeconds2;
                double lngMinutes;
                double lngMinutes2;

                lngSeconds = DateAndTime.DateDiff("s", StartDate, _endDate);
                lngMinutes = Math.Round(lngSeconds / 60d - 0.5d, 0);
                lngSeconds2 = lngSeconds - lngMinutes * 60d;
                lngMinutes2 = lngSeconds / 60d;

                if (strAction == "EventLog")
                {
                    strTemp = Strings.Format(StartDate, "General Date") + " TO " + Strings.Format(_endDate, "h:mm:ss AM/PM") + "| DUR: " + lngMinutes + " minutes " + lngSeconds2 + " seconds" + " |" + Strings.Format(lngMinutes2, "##0.0000") + " | " + "APP: " + Subject + " | " + "PROC: " + strProcName;
                }

                else if (strAction == "ToDo")
                {
                    strTemp = "|" + Strings.Format(_endDate, "General Date") + "| Duration: " + lngMinutes + " minutes " + lngSeconds2 + " seconds" + " |" + Strings.Format(lngMinutes2, "##0.0000") + " | Subject: " + Subject;
                }
                else
                {
                    strTemp = "|" + Strings.Format(_endDate, "General Date") + "| Duration: " + lngMinutes + " minutes " + lngSeconds2 + " seconds" + " |" + Strings.Format(lngMinutes2, "##0.0000") + "| Action: " + strAction + " | Subject: " + Subject + " | From: " + SentFrom + " | To: " + SentTo;
                }
                ToStringRet = strTemp;
                return ToStringRet;
            }

        }

    }
}
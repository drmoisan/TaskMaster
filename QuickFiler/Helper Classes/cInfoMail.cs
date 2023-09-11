using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using ToDoModel;
//using Microsoft.VisualBasic;
//using Microsoft.VisualBasic.CompilerServices;
using UtilitiesCS;


namespace QuickFiler
{

    public class cInfoMail
    {
        public string Subject;
        private DateTime _endDate;
        public DateTime StartDate;
        private int _durationSec;
        public string SentTo;
        public string SentCC;
        public string SentFrom;
        public string Body;
        public OlImportance Importance;
        public string Categories;
        public string strAction;
        public string strProcName;
        //private Collection _col;
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
                            dict_strSumRet = dict_strSumRet + key + " " + (_dict[key] / 60d).ToString("#,##0.0") + " min";
                            //dict_strSumRet = dict_strSumRet + key + " " + Strings.Format(_dict[key] / 60d, "#,##0.0") + " min";
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
            int lcl_DurationSec = 0,
            string lcl_SentTo = "",
            string lcl_SentCC = "",
            string lcl_SentFrom = "",
            string lcl_Body = "",
            OlImportance lcl_Importance = OlImportance.olImportanceNormal,
            Categories lcl_Categories = null,
            string lcl_strAction = "")
        {
            try
            {
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
                return 1;
            }
            catch (System.Exception)
            {
                return 0;
            }
        }

        internal bool Init_wMail(MailItem OlMail, DateTime OlEndTime = default, int lngDurationSec = 0, string stringAction = "")
        {
            try
            {
                Subject = OlMail.Subject;
                if (OlEndTime != default)
                    EndDate = OlEndTime;
                if (lngDurationSec != 0)
                    DurationSec = lngDurationSec;
                var recipients = EmailDetails.GetRecipients(OlMail);
                SentTo = recipients.recipientsTo;
                SentCC = recipients.recipientsCC;
                SentFrom = OlMail.Sender.ToString();
                Body = OlMail.Body;
                Importance = OlMail.Importance;
                Categories = OlMail.Categories;
                if (!string.IsNullOrEmpty(stringAction))
                    strAction = stringAction;
                return true;
            }
            catch (System.Exception e)
            {
                DialogResult result = MessageBox.Show(e.Message + " Should we break", "Error",MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    throw e;
                }
                return false;
            }
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
                StartDate = _endDate.Subtract(new TimeSpan(0,0, 0, _durationSec));
            }
        }

        public int DurationSec
        {
            get
            {
                return _durationSec;
            }
            set
            {
                _durationSec = value;
                StartDate = _endDate.Subtract(new TimeSpan(0, 0, 0, _durationSec));
            }
        }

        public new string ToString
        {
            get
            {
                string ToStringRet = default;
                string strTemp;
                double lngSeconds;
                int lngSeconds2;
                int lngMinutes;
                double lngMinutes2;

                TimeSpan duration = _endDate.Subtract(StartDate);
                lngSeconds = _endDate.Subtract(StartDate).TotalSeconds;
                lngMinutes = duration.Minutes;
                lngSeconds2 = duration.Seconds;
                lngMinutes2 = duration.TotalMinutes;

                if (strAction == "EventLog")
                {
                    strTemp = StartDate.ToString("General Date") + " TO " + _endDate.ToString("h:mm:ss AM/PM") + "| DUR: " + lngMinutes + " minutes " + lngSeconds2 + " seconds" + " |" + lngMinutes2.ToString("##0.0000") + " | " + "APP: " + Subject + " | " + "PROC: " + strProcName;
                }

                else if (strAction == "ToDo")
                {
                    strTemp = "|" + _endDate.ToString("General Date") + "| Duration: " + lngMinutes + " minutes " + lngSeconds2 + " seconds" + " |" + lngMinutes2.ToString("##0.0000") + " | Subject: " + Subject;
                }
                else
                {
                    strTemp = "|" + _endDate.ToString("General Date") + "| Duration: " + lngMinutes + " minutes " + lngSeconds2 + " seconds" + " |" + lngMinutes2.ToString("##0.0000") + "| Action: " + strAction + " | Subject: " + Subject + " | From: " + SentFrom + " | To: " + SentTo;
                }
                ToStringRet = strTemp;
                return ToStringRet;
            }

        }

    }
}
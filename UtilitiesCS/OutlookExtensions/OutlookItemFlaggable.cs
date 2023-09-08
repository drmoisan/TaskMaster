using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookExtensions
{
    public class OutlookItemFlaggable : OutlookItem
    {
        #region Constructor

        public OutlookItemFlaggable(object item) : base(item) 
        { 
            _olType = this.GetOlItemType();
        }

        private OlItemType _olType;
        private const string _olFlagStatus = "FlagStatus";
        private const string _olTaskDueDate = "TaskDueDate";
        private const string _olDueDate = "DueDate";
        private const string _olTaskStartDate = "TaskStartDate";
        private const string _olStartDate = "CreationTime";
        private const string _olComplete = "Complete";
        private const string _olTaskSubject = "TaskSubject";
        private const string _olSubject = "Subject";
        private const string _olTotalWork = "TotalWork";
        private const string PA_TOTAL_WORK = "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/81110003";

        #endregion

        #region Public Properties and Methods


        public bool Complete
        {
            get
            {
                try
                {
                    var complete = this.TryGetPropertyValue(_olComplete) ?? (OlFlagStatus)this.GetPropertyValue(_olFlagStatus) == OlFlagStatus.olFlagComplete;
                    return (bool)complete;
                }
                // if neither property exists, catch the exception and throw a custom one
                catch (System.Exception)
                {
                    throw new ArgumentException(GetTypeErrorMessage(nameof(Complete)));
                }
            }
            set
            {
                
                if (Complete != value)
                {
                    var success = this.TrySetPropertyValue(_olComplete, value);
                    if (!success) 
                    {
                        if (value) { success = this.TrySetPropertyValue(_olFlagStatus, OlFlagStatus.olFlagMarked); }
                        else { success = this.TrySetPropertyValue(_olFlagStatus, OlFlagStatus.olFlagComplete); }
                    }
                    if (!success) { throw new ArgumentException(GetTypeErrorMessage(nameof(Complete))); }
                }
            }
        }
        
        public DateTime DueDate
        {
            get
            {
                var dueDate = this.TryGetPropertyValue(_olTaskDueDate) ?? this.TryGetPropertyValue(_olDueDate);
                if (dueDate is null)
                {
                    throw new ArgumentException(GetTypeErrorMessage(nameof(DueDate))); 
                }
                return (DateTime)dueDate;
            }
            set
            {
                DateTime current = DueDate;
                if (current != value)
                {
                    var success = this.TrySetPropertyValue(_olTaskDueDate, value);
                    if (!success) { success = this.TrySetPropertyValue(_olDueDate, value); }
                    if (!success) { throw new ArgumentException(GetTypeErrorMessage(nameof(DueDate))); }
                }
            }
        }

        public bool FlagAsTask
        {
            get
            {
                var mailFlag = this.TryGetPropertyValue(_olFlagStatus);
                if (mailFlag != null)
                {
                    return (OlFlagStatus)mailFlag == OlFlagStatus.olFlagMarked ||
                           (OlFlagStatus)mailFlag == OlFlagStatus.olFlagComplete;
                }
                else if (this.InnerObject is TaskItem) { return true; }
                else { throw new ArgumentException(GetTypeErrorMessage(nameof(FlagAsTask))); }
            }
            set
            {
                bool current = FlagAsTask;
                if (current != value)
                {
                    //If it was false and now is true, set the flag status to olFlagMarked. Irrelevant for TaskItems
                    if (value) { this.TrySetPropertyValue(_olFlagStatus, OlFlagStatus.olFlagMarked); }
                    else
                    {
                        // If it was true and now is false, set the flag status to olNoFlag
                        if (!this.TrySetPropertyValue(_olFlagStatus, OlFlagStatus.olNoFlag)) 
                        {
                            // TaskItems cannot be set to false
                            throw new ArgumentOutOfRangeException($"{nameof(TaskItem)} items cannot be set to False");
                        }
                    }
                }
            }
        }
        
        public bool PropertyExists(string propertyName)
        {
            return (GetPropertyValue(propertyName) != null);
        }
        
        public DateTime TaskStartDate
        {
            get
            {
                var startDate = this.TryGetPropertyValue(_olTaskStartDate) ?? this.TryGetPropertyValue(_olStartDate);
                if (startDate is null)
                {
                    throw new ArgumentException(GetTypeErrorMessage(nameof(TaskStartDate)));
                }
                return (DateTime)startDate;
            }
            set
            {
                DateTime current = TaskStartDate;
                if (current != value)
                {
                    var success = this.TrySetPropertyValue(_olTaskStartDate, value);
                    if (!success) { success = this.TrySetPropertyValue(_olStartDate, value); }
                    if (!success) { throw new ArgumentException(GetTypeErrorMessage(nameof(TaskStartDate))); }
                }
            }
        }

        public string TaskSubject
        {
            get
            {
                var taskSubject = this.TryGetPropertyValue(_olTaskSubject, _olTaskSubject) ?? throw new ArgumentException(GetTypeErrorMessage(nameof(TaskSubject)));
                return (string)taskSubject;
            }
            set
            {
                if (TaskSubject != value)
                {
                    var success = this.TrySetPropertyValue(_olTaskSubject, _olSubject, value);
                    if (!success) { throw new ArgumentException(GetTypeErrorMessage(nameof(TaskSubject))); }
                }
            }
        }

        public int TotalWork 
        {
            get 
            {
                var work = this.TryGetPropertyValue(_olTotalWork) ?? this.PropertyAccessor.TryGetProperty(PA_TOTAL_WORK);
                try { return (int)work; }
                catch (System.Exception) { return 0; }
            }
            set
            {
                if (TotalWork != value)
                {
                    var success = this.TrySetPropertyValue(_olTotalWork, value);
                    if (!success) { this.PropertyAccessor.SetProperty(PA_TOTAL_WORK, value); }
                }
            }
        }

        private string GetTypeErrorMessage(string propertyName)
        {
            return $"{nameof(OutlookItemFlaggable)}.{nameof(InnerObject)} is of type {_olType} which is not supported for property {propertyName}.";
        }


        #endregion

        
    }
}

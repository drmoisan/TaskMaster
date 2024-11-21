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

        public OutlookItemFlaggable(IOutlookItem item) : base()
        {
            if (item is not null)
            {
                base._item = item.InnerObject;
                base._type = item.InnerObject?.GetType();
                base._args = item.Args;
                _olType = this.GetOlItemType();
            }
        }

        private OlItemType _olType;
        private const string _olFlagStatus = "FlagStatus";
        private const string _olTaskDueDate = "TaskDueDate";
        private const string _olDueDate = "DueDate";
        private const string _olTaskStartDate = "TaskStartDate";
        private const string _olStartDate = "StartDate";
        private const string _olCreationTime = "CreationTime";
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
                    bool complete;
                    if (_olType == OlItemType.olTaskItem) { complete = (bool)(OutlookItemExtensions.TryGetPropertyValue(this, _olComplete) ?? false); }
                    else { complete = (OlFlagStatus)this.TryGetPropertyValue(_olFlagStatus) == OlFlagStatus.olFlagComplete; }
                    return complete;
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
                    bool success;
                    if (_olType == OlItemType.olTaskItem) { success = OutlookItemExtensions.TrySetPropertyValue(this, _olComplete, value); }
                    else { success = OutlookItemExtensions.TrySetPropertyValue(this, _olFlagStatus, value ? OlFlagStatus.olFlagComplete : OlFlagStatus.olFlagMarked); }
                    if (!success) { throw new ArgumentException(GetTypeErrorMessage(nameof(Complete))); }
                }
            }
        }
        
        public DateTime DueDate
        {
            get
            {
                object dueDate;
                if (_olType == OlItemType.olTaskItem) { dueDate = OutlookItemExtensions.TryGetPropertyValue(this, _olDueDate); }
                else
                {
                    dueDate = this.TryGetPropertyValue(_olTaskDueDate, _olDueDate);
                }
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
                    bool success;
                    if (_olType == OlItemType.olTaskItem) { success = OutlookItemExtensions.TrySetPropertyValue(this, _olDueDate, value); }
                    else { success = OutlookItemExtensions.TrySetPropertyValue(this, _olTaskDueDate, value); }
                    if (!success) { success = OutlookItemExtensions.TrySetPropertyValue(this, _olDueDate, value); }
                    if (!success) { throw new ArgumentException(GetTypeErrorMessage(nameof(DueDate))); }
                }
            }
        }

        public bool FlagAsTask
        {
            get
            {
                if (_olType == OlItemType.olTaskItem) { return true; }
                var mailFlag = OutlookItemExtensions.TryGetPropertyValue(this, _olFlagStatus);
                if (mailFlag != null)
                {
                    return (OlFlagStatus)mailFlag == OlFlagStatus.olFlagMarked ||
                           (OlFlagStatus)mailFlag == OlFlagStatus.olFlagComplete;
                }
                else { throw new ArgumentException(GetTypeErrorMessage(nameof(FlagAsTask))); }
            }
            set
            {
                bool current = FlagAsTask;
                if (current != value)
                {
                    //If it was false and now is true, set the flag status to olFlagMarked. Irrelevant for TaskItems
                    if (value) 
                    {
                        OutlookItemExtensions.TryCallMethod(this, "MarkAsTask", new object[] { OlMarkInterval.olMarkNoDate });
                        //this.TrySetPropertyValue(_olFlagStatus, OlFlagStatus.olFlagMarked); 
                    }
                    else
                    {
                        // If it was true and now is false, set the flag status to olNoFlag
                        //if (!this.TrySetPropertyValue(_olFlagStatus, OlFlagStatus.olNoFlag))
                        if (OutlookItemExtensions.TryCallMethod(this, "ClearTaskFlag") is null)
                            {
                            // TaskItems cannot be set to false
                            throw new ArgumentOutOfRangeException($"{nameof(Outlook.TaskItem)} items cannot be set to False");
                        }
                    }
                }
            }
        }
                
        public DateTime TaskStartDate
        {
            get
            {                
                object startDate = null;
                if (_olType == OlItemType.olTaskItem) { startDate = this.TryGetPropertyValue(_olStartDate, _olCreationTime); }
                else { startDate = this.TryGetPropertyValue(_olTaskStartDate, _olCreationTime); }
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
                    var success = OutlookItemExtensions.TrySetPropertyValue(this, _olTaskStartDate, value);
                    if (!success) { success = OutlookItemExtensions.TrySetPropertyValue(this, _olStartDate, value); }
                    if (!success) { throw new ArgumentException(GetTypeErrorMessage(nameof(TaskStartDate))); }
                }
            }
        }

        public string TaskSubject
        {
            get
            {
                if (_olType == OlItemType.olTaskItem) { return Subject; }
                else
                {
                    var taskSubject = this.TryGetPropertyValue(_olTaskSubject, _olSubject) ?? throw new ArgumentException(GetTypeErrorMessage(nameof(TaskSubject)));
                    return (string)taskSubject;
                }
            }
            set
            {
                if (TaskSubject != value)
                {
                    if (_olType == OlItemType.olTaskItem) { Subject = value; }
                    else 
                    { 
                        var success = this.TrySetPropertyValue(_olTaskSubject, _olSubject, value);
                        if (!success) { throw new ArgumentException(GetTypeErrorMessage(nameof(TaskSubject))); }
                    }
                }
            }
        }

        public int TotalWork 
        {
            get 
            {
                object work;
                if (_olType == OlItemType.olTaskItem) { work = OutlookItemExtensions.TryGetPropertyValue(this, _olTotalWork); }
                else { work = this.PropertyAccessor.TryGetProperty(PA_TOTAL_WORK); }
                try { return (int)work; }
                catch (System.Exception) { return 0; }
            }
            set
            {
                if (TotalWork != value)
                {
                    bool success;
                    try
                    {
                        if (_olType == OlItemType.olTaskItem) { success = OutlookItemExtensions.TrySetPropertyValue(this, _olTotalWork, value); }
                        else { success = this.PropertyAccessor.TrySetProperty(PA_TOTAL_WORK, value); }
                        if (!success) { this.PropertyAccessor.SetProperty(PA_TOTAL_WORK, value); }
                    }
                    catch (System.Exception)
                    {
                        Debug.WriteLine($"Error setting TotalWork to value {value}");
                    }
                    
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

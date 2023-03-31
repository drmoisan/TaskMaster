using Microsoft.Office.Interop.Outlook;
using System;
using System.Diagnostics.Eventing.Reader;
//using Microsoft.VisualBasic;
//using Microsoft.VisualBasic.CompilerServices;

namespace QuickFiler
{
    
    internal class AcceleratorParser
    {
        private IAcceleratorCallbacks _parent;

        internal AcceleratorParser(IAcceleratorCallbacks Parent)
        {
            _parent = Parent;
        }

        internal void ParseAndExecute(string strToParse, int _intActiveSelection)
        {
            int intNewSelection;
            bool blExpanded = false;

            if (AnythingToParse(strToParse))
            {
                int idxLastNum = GetFinalNumericIndex(strToParse);
                if (SelectionDetected(idxLastNum))
                {
                    intNewSelection = GetFinalNumeric(strToParse, idxLastNum);
                    if (_parent.IsSelectionBelowMax(intNewSelection))
                    {
                        if (IsChange(intNewSelection, _intActiveSelection))
                        {
                            if (IsAnythingActive(_intActiveSelection))
                                blExpanded = _parent.ToggleOffActiveItem(blExpanded);
                            if (IsActivatingNode(intNewSelection))
                            {
                                _intActiveSelection = _parent.ActivateByIndex(intNewSelection, blExpanded);
                            }
                            else { _intActiveSelection = intNewSelection; }
                        }

                        if (AdditionalInstructions(idxLastNum, strToParse)) 
                        {
                            if (IsAnythingActive(_intActiveSelection))
                            {
                                string strCommand = ExtractInstruction(idxLastNum, strToParse);
                                _parent.ResetAcceleratorSilently();
                                IQfcItemController QF = _parent.TryGetQfc(_intActiveSelection - 1);
                                switch (strCommand ?? "")
                                {
                                    case "O":
                                        {
                                            _parent.ToggleKeyboardDialog();
                                            QF.ApplyReadEmailFormat();
                                            _parent.OpenQFMail(QF.Mail);
                                            break;
                                        }
                                    case "C":
                                        {
                                            _parent.ToggleKeyboardDialog();
                                            QF.ToggleConversationCheckbox();
                                            break;
                                        }
                                    case "T":
                                        {
                                            _parent.ToggleKeyboardDialog();
                                            QF.FlagAsTask();
                                            break;
                                        }
                                    case "F":
                                        {
                                            _parent.ToggleKeyboardDialog();
                                            QF.JumpToSearchTextbox();
                                            break;
                                        }
                                    case "D":
                                        {
                                            _parent.ToggleKeyboardDialog();
                                            QF.JumpToFolderDropDown();
                                            break;
                                        }
                                    case "X":
                                        {
                                            _parent.ToggleKeyboardDialog();
                                            QF.MarkItemForDeletion();
                                            break;
                                        }
                                    case "R":
                                        {
                                            _parent.ToggleKeyboardDialog();
                                            _parent.RemoveSpecificControlGroup(_intActiveSelection - 1);
                                            break;
                                        }
                                    case "A":
                                        {
                                            QF.ToggleSaveAttachments();
                                            break;
                                        }
                                    case "W":
                                        {
                                            QF.ToggleDeleteFlow();
                                            break;
                                        }
                                    case "M":
                                        {
                                            QF.ToggleSaveCopyOfMail();
                                            break;
                                        }
                                    case "E":
                                        {
                                            if (QF.BlExpanded)
                                            {
                                                _parent.MoveDownPix(_intActiveSelection + 1, (int)Math.Round(QF.Height * -0.5d));
                                                QF.ExpandCtrls1();
                                            }
                                            else
                                            {
                                                _parent.MoveDownPix(_intActiveSelection + 1, QF.Height);
                                                QF.ExpandCtrls1();
                                            }

                                            break;
                                        }

                                    default:
                                        {
                                            break;
                                        }
                                }
                            }
                            else { _parent.ResetAcceleratorSilently(); }
                        }
                    }
                    else { _parent.ResetAcceleratorSilently();}
                }

                else { blExpanded = _parent.ToggleOffActiveItem(blExpanded);}
            }
            else { blExpanded = _parent.ToggleOffActiveItem(blExpanded); }
        }

        private string ExtractInstruction(int idxLastNum, string strToParse)
        {
            return strToParse.Substring(idxLastNum + 1, 1).ToUpper();
        }

        private bool AdditionalInstructions(int idxLastNum, string strToParse)
        {
            if (strToParse.Length - 1 > idxLastNum)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsActivatingNode(int NewSelection)
        {
            return (NewSelection != 0) ? true : false; 
        }
        
        private bool IsAnythingActive(int ActiveSelection)
        {
            if (ActiveSelection != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AnythingToParse(string strToParse)
        {
            if (!string.IsNullOrEmpty(strToParse))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool SelectionDetected(int idxLastNum)
        {
            if (idxLastNum > -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsChange(int intNewSelection, int ActiveSelection)
        {
            if (intNewSelection != ActiveSelection)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int GetFinalNumeric(string strToParse, int idxLastNum)
        {
            if (idxLastNum > -1)
            {
                // Get last digit 
                // TODO: Add support for multiple digit numbers 
                return int.Parse(strToParse.Substring(idxLastNum, 1));
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
    /// Gets the index of the last number in a string. Returns 0 if none is found
    /// </summary>
    /// <param name="strToParse"></param>
    /// <returns></returns>
        private int GetFinalNumericIndex(string strToParse)
        {
            int i;
            int intLastNum = 0;
            int intLastIndex = -1;
            int intLen = strToParse.Length;

            var loopTo = intLen - 1;
            for (i = 0; i <= loopTo; i++)
            {
                if (int.TryParse(strToParse.Substring(i, 1), out intLastNum))
                {
                    intLastIndex = i;
                }
                else
                {
                    break;
                }
            }
            return intLastIndex;
        }

    }
}
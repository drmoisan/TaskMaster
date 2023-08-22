using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ToDoModel.Test
{
    public class AssertAll
    {
        public static string Peek(params Action[] assertions)
        {
            var errorMessages = new List<string>();

            foreach (var action in assertions)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    errorMessages.Add(ex.Message);
                }
            }

            if (errorMessages.Count == 0)
            {
                return "";
            }

            return string.Join(Environment.NewLine, errorMessages);

        }
        
        public static void Check(params Action[] assertions)
        {
            var errorMessages = new List<string>();

            foreach (var action in assertions)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    errorMessages.Add(ex.Message);
                }
            }

            if (errorMessages.Count == 0)
            {
                return;
            }

            string errorMessageString = string.Join(Environment.NewLine, errorMessages);

            //throw new UnitTestAssertException($"The following conditions failed: {Environment.NewLine}{errorMessageString}");
            throw new AssertFailedException($"The following conditions failed: {Environment.NewLine}{errorMessageString}");
        }
    }
    
}

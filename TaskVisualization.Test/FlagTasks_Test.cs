using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Office.Interop.Outlook;
using System;
using TaskVisualization;
using UtilitiesCS;
using Moq;
using System.Collections.Generic;
using System.Collections;

namespace TaskVisualization.Test
{
    [TestClass]
    public class FlagTasks_Test
    {
        

        [TestMethod]
        public void GetFlagsToSet_TestMultiple()
        {
            
            MoqOlToDo mockGlobals = new MoqOlToDo();
                                    
            FlagTasks testFlagger = new FlagTasks(mockGlobals.MockGlobals());
            testFlagger.Run();
            //testFlagger.GetFlagsToSet(2);
        }
    }
}

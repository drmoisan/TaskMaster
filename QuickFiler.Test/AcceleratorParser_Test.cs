using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
using QuickFiler;
using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;
using System.Windows.Forms;

namespace QuickFiler.Test
{
    [TestClass]
    public class AcceleratorParser_Test
    {
        internal static Mock<IAcceleratorCallbacks> mock;
        internal static AcceleratorParser parser;

        private static void VerifyFunctionCalls(
            int IsSelectionBelowMax,
            int ActivateByIndex,
            int MoveDownPix,
            int ResetAcceleratorSilently,
            int ToggleAcceleratorDialogue,
            int ToggleOffActiveItem,
            int OpenQFMail,
            int TryGetQfc) 
        {
            mock.Verify(x => x.IsSelectionBelowMax(It.IsAny<int>()), Times.Exactly(IsSelectionBelowMax));
            mock.Verify(x => x.ActivateByIndex(It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(ActivateByIndex)); 
            mock.Verify(x => x.MoveDownPix(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(MoveDownPix));
            mock.Verify(x => x.ResetAcceleratorSilently(), Times.Exactly(ResetAcceleratorSilently)); 
            mock.Verify(x => x.toggleAcceleratorDialogue(), Times.Exactly(ToggleAcceleratorDialogue));
            mock.Verify(x => x.ToggleOffActiveItem(It.IsAny<bool>()), Times.Exactly(ToggleOffActiveItem));
            mock.Verify(x => x.OpenQFMail(It.IsAny<MailItem>()), Times.Exactly(OpenQFMail));
            mock.Verify(x => x.TryGetQfc(It.IsAny<int>()), Times.Exactly(TryGetQfc));
        }

        [TestInitialize()]
        public void Initialize() 
        {

            //// Create Mock of Panel for QF.frm.Height call
            //Mock<Panel> frm = new Mock<Panel>();
            //frm.Setup(x => x.Height).Returns(90);


            Panel frm = new Panel();
            frm.Height = 90;

            // Create Mock of QF for TryGetQfc function of AcceleratorMock
            Mock<QfcController> qfc = new Mock<QfcController>();
            qfc.Setup(x => x.KeyboardHandler(It.IsAny<string>()));
            qfc.Setup(x => x.ExpandCtrls1());
            qfc.Setup(x => x.Frm).Returns(frm);

            // Create Instance of callbacks mock
            mock = new Mock<IAcceleratorCallbacks>();
            mock.Setup(x => x.IsSelectionBelowMax(It.IsAny<int>())).Returns((int a) => { return (a < 6); });
            mock.Setup(x => x.ActivateByIndex(It.IsAny<int>(), It.IsAny<bool>())).Returns((int a, bool b) => { return a; });
            mock.Setup(x => x.MoveDownPix(It.IsAny<int>(), It.IsAny<int>()));
            mock.Setup(x => x.ResetAcceleratorSilently());
            mock.Setup(x => x.toggleAcceleratorDialogue());
            mock.Setup(x => x.ToggleOffActiveItem(It.IsAny<bool>())).Returns(false);
            mock.Setup(x => x.OpenQFMail(It.IsAny<MailItem>()));
            mock.Setup(x => x.TryGetQfc(It.IsAny<int>())).Returns(qfc.Object);
            
            // Create Instance of Actual parser using mock callbacks
            parser = new AcceleratorParser(mock.Object);
        }

        [TestMethod]
        public void ParseAndExecute_Test_000000000()
        {
            // 15	"", 0	[ N1 ]							_parent.ToggleOffActiveItem
            parser.ParseAndExecute(strToParse: "", _intActiveSelection: 0);
            VerifyFunctionCalls(IsSelectionBelowMax: 0,
                                ActivateByIndex: 0,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 0,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 1,
                                OpenQFMail: 0,
                                TryGetQfc: 0);
        }

        [TestMethod]
        public void ParseAndExecute_Test_100000000()
        {
            // 14	"e", 0	[ Y1 N2 ]						_parent.ToggleOffActiveItem	
            parser.ParseAndExecute(strToParse: "e", _intActiveSelection: 0);
            VerifyFunctionCalls(IsSelectionBelowMax: 0,
                                ActivateByIndex: 0,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 0,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 1,
                                OpenQFMail: 0,
                                TryGetQfc: 0);
        }

        [TestMethod]
        public void ParseAndExecute_Test_110000000()
        {
            //13	"8", 0	[ Y1 Y2 N3 ]					_parent.IsSelectionBelowMax	=>	_parent.ResetAcceleratorSilently'
            parser.ParseAndExecute(strToParse: "8", _intActiveSelection: 0);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 0,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 1,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 0,
                                OpenQFMail: 0,
                                TryGetQfc: 0);
        }

        [TestMethod]
        public void ParseAndExecute_Test_111000000()
        {
            //12	"3", 3	[ Y1 Y2 Y3 N4 N5 ]				_parent.IsSelectionBelowMax	
            parser.ParseAndExecute(strToParse: "3", _intActiveSelection: 3);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 0,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 0,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 0,
                                OpenQFMail: 0,
                                TryGetQfc: 0);
        }

        [TestMethod]
        public void ParseAndExecute_Test_111010000()
        {
            //11	"0E",0	[ Y1 Y2 Y3 N4 Y5 N6 ]			_parent.IsSelectionBelowMax	=>	_parent.ResetAcceleratorSilently
            parser.ParseAndExecute(strToParse: "0E", _intActiveSelection: 0);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 0,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 1,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 0,
                                OpenQFMail: 0,
                                TryGetQfc: 0);
        }

        [TestMethod]
        public void ParseAndExecute_Test_111011300()
        {
            /* Logic tree codepath 
             * 10	"2E",2	
             * [ Y1 Y2 Y3 N4 Y5 Y6 *3 ]		
             * _parent.IsSelectionBelowMax	=>	
             * _parent.ResetAcceleratorSilently => 
             * _parent.TryGetQfc =>	
             * _parent.MoveDownPix */

            AcceleratorParser parser = new AcceleratorParser(mock.Object);
            parser.ParseAndExecute(strToParse: "2E", _intActiveSelection: 2);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 0,
                                MoveDownPix: 1,
                                ResetAcceleratorSilently: 1,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 0,
                                OpenQFMail: 0,
                                TryGetQfc: 1);
        }

        [TestMethod]
        public void ParseAndExecute_Test_111011200()
        {
            /* Logic tree codepath
             * 09	"2W",2	
             * [ Y1 Y2 Y3 N4 Y5 Y6 *2 ]		
             * _parent.IsSelectionBelowMax	=>	
             * _parent.ResetAcceleratorSilently => 
             * _parent.TryGetQfc */

            AcceleratorParser parser = new AcceleratorParser(mock.Object);
            parser.ParseAndExecute(strToParse: "2W", _intActiveSelection: 2);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 0,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 1,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 0,
                                OpenQFMail: 0,
                                TryGetQfc: 1);
        }

        [TestMethod]
        public void ParseAndExecute_Test_111011100()
        {
            /* Logic tree codepath
             * 08	"2C",2	[ Y1 Y2 Y3 N4 Y5 Y6 *1 ]		
             * _parent.IsSelectionBelowMax	=>	
             * _parent.ResetAcceleratorSilently =>
             * _parent.TryGetQfc =>	
             * _parent.toggleAcceleratorDialogue */

            AcceleratorParser parser = new AcceleratorParser(mock.Object);
            parser.ParseAndExecute(strToParse: "2C", _intActiveSelection: 2);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 0,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 1,
                                ToggleAcceleratorDialogue: 1,
                                ToggleOffActiveItem: 0,
                                OpenQFMail: 0,
                                TryGetQfc: 1);
        }
        
        [TestMethod]
        public void ParseAndExecute_Test_111101111()
        {
            /* Logic tree codepath
             * 07-1 "2C",0 	
             * [ Y1 Y2 Y3 Y4 N5 Y6 Y7 Y8 *1 ]
             * 
             * Conditions:
             ***** (new selection < 6) AND 
             ***** (new selection != old selection) AND 
             ***** (old selection == 0) AND 
             ***** (extra letters) And 
             ***** (Letters in O,C,T,F,D,X,R)
             * 
             * Parent functions called in codepath
             ***** _parent.IsSelectionBelowMax	=>	
             ***** _parent.ActivateByIndex	=>	
             ***** _parent.ResetAcceleratorSilently => 
             ***** _parent.TryGetQfc => 
             ***** _parent.toggleAcceleratorDialogue
             */

            AcceleratorParser parser = new AcceleratorParser(mock.Object);
            parser.ParseAndExecute(strToParse: "2C", _intActiveSelection: 0);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 1,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 1,
                                ToggleAcceleratorDialogue: 1,
                                ToggleOffActiveItem: 0,
                                OpenQFMail: 0,
                                TryGetQfc: 1);
        }

        [TestMethod]
        public void ParseAndExecute_Test_111101112()
        {
            /* Logic tree codepath
             * 07-2 "2W",0 	
             * [ Y1 Y2 Y3 Y4 N5 Y6 Y7 Y8 *2 ]		
             * 
             * Conditions:
             ***** (new selection < 6) AND 
             ***** (new selection != old selection) AND 
             ***** (old selection == 0) AND 
             ***** (extra letters) And 
             ***** (Letters in A,W,M)
             * 
             * Parent functions called in codepath
             ***** _parent.IsSelectionBelowMax	=>	
             ***** _parent.ActivateByIndex	=>	
             ***** _parent.ResetAcceleratorSilently => 
             ***** _parent.TryGetQfc
             */


            AcceleratorParser parser = new AcceleratorParser(mock.Object);
            parser.ParseAndExecute(strToParse: "2W", _intActiveSelection: 0);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 1,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 1,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 0,
                                OpenQFMail: 0,
                                TryGetQfc: 1);
        }

        [TestMethod]
        public void ParseAndExecute_Test_111101113()
        {
            /* Logic tree codepath
             * 07-3 "2E",0 	
             * [ Y1 Y2 Y3 Y4 N5 Y6 Y7 Y8 *3 ]		
             * _parent.IsSelectionBelowMax	=>	
             * _parent.ActivateByIndex	=>	
             * _parent.ResetAcceleratorSilently => 
             * _parent.TryGetQfc => 
             * _parent.MoveDownPix
             */


            AcceleratorParser parser = new AcceleratorParser(mock.Object);
            parser.ParseAndExecute(strToParse: "2E", _intActiveSelection: 0);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 1,
                                MoveDownPix: 1,
                                ResetAcceleratorSilently: 1,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 0,
                                OpenQFMail: 0,
                                TryGetQfc: 1);
        }

        [TestMethod]
        public void ParseAndExecute_Test_111101114()
        {
            /* Logic tree codepath
             * 07-2 "2Z",0 	
             * [ Y1 Y2 Y3 Y4 N5 Y6 Y7 Y8 *2 ]		
             * 
             * Conditions:
             ***** (new selection < 6) AND 
             ***** (new selection != old selection) AND 
             ***** (old selection == 0) AND 
             ***** (extra letters) And 
             ***** (Letters not in O,C,T,F,D,X,R,A,W,M,E)
             * 
             * Parent functions called in codepath
             ***** _parent.IsSelectionBelowMax	=>	
             ***** _parent.ActivateByIndex	=>	
             ***** _parent.ResetAcceleratorSilently => 
             ***** _parent.TryGetQfc
             */


            AcceleratorParser parser = new AcceleratorParser(mock.Object);
            parser.ParseAndExecute(strToParse: "2Z", _intActiveSelection: 0);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 1,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 1,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 0,
                                OpenQFMail: 0,
                                TryGetQfc: 1);
        }

        [TestMethod]
        public void ParseAndExecute_Test_111111000()
        {
            /* Logic tree codepath
             * 04	"3",2	
             * [ Y1 Y2 Y3 Y4 Y5 Y6 N7 ]				
             * 
             * Conditions:
             * (new selection < 6) AND 
             * (new selection != old selection) AND 
             * (old selection != 0) AND (new selection != 0) AND
             * (no letters at end)
             * 
             * Parent functions called in codepath:
             * _parent.IsSelectionBelowMax	=>	
             * _parent.ToggleOffActiveItem	=>	
             * _parent.ActivateByIndex
             */

            AcceleratorParser parser = new AcceleratorParser(mock.Object);
            parser.ParseAndExecute(strToParse: "3", _intActiveSelection: 2);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 1,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 0,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 1,
                                OpenQFMail: 0,
                                TryGetQfc: 0);
        }

        [TestMethod]
        public void ParseAndExecute_Test_111111110()
        {
            /* Logic tree codepath
             * 01	"3T",2	
             * [ Y1 Y2 Y3 Y4 Y5 Y6 Y7 *1 ]				
             * 
             * Conditions:
             * (new selection < 6) AND 
             * (new selection != old selection) AND 
             * (old selection != 0) AND (new selection != 0) AND
             * (O,C,T,F,D,X,R at end)
             * 
             * Parent functions called in codepath:
             * _parent.IsSelectionBelowMax	=>	
             * _parent.ToggleOffActiveItem	=>	
             * _parent.ActivateByIndex =>
             * _parent.ResetAcceleratorSilently => 
             * _parent.TryGetQfc	=>	
             * _parent.toggleAcceleratorDialogue
             */

            AcceleratorParser parser = new AcceleratorParser(mock.Object);
            parser.ParseAndExecute(strToParse: "3T", _intActiveSelection: 2);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 1,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 1,
                                ToggleAcceleratorDialogue: 1,
                                ToggleOffActiveItem: 1,
                                OpenQFMail: 0,
                                TryGetQfc: 1);
        }

        [TestMethod]
        public void ParseAndExecute_Test_111111120()
        {
            /* Logic tree codepath
             * 02	"3W",2	
             * [ Y1 Y2 Y3 Y4 Y5 Y6 Y7 *2 ]				
             * 
             * Conditions:
             * (new selection < 6) AND 
             * (new selection != old selection) AND 
             * (old selection != 0) AND (new selection != 0) AND
             * (A,W,M at end)
             * 
             * Parent functions called in codepath:
             * _parent.IsSelectionBelowMax	=>	
             * _parent.ToggleOffActiveItem	=>	
             * _parent.ActivateByIndex  =>	
             * _parent.ResetAcceleratorSilently => 
             * _parent.TryGetQfc
             */

            AcceleratorParser parser = new AcceleratorParser(mock.Object);
            parser.ParseAndExecute(strToParse: "3W", _intActiveSelection: 2);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 1,
                                MoveDownPix: 0,
                                ResetAcceleratorSilently: 1,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 1,
                                OpenQFMail: 0,
                                TryGetQfc: 1);
        }

        [TestMethod]
        public void ParseAndExecute_Test_111111130()
        {
            /* Logic tree codepath
             * 03	"3e",2	
             * [ Y1 Y2 Y3 Y4 Y5 Y6 Y7 *3 ]				
             * 
             * Conditions:
             * (new selection < 6) AND 
             * (new selection != old selection) AND 
             * (old selection != 0) AND (new selection != 0) AND
             * (E at end)
             * 
             * Parent functions called in codepath:
             * _parent.IsSelectionBelowMax	=>	
             * _parent.ToggleOffActiveItem	=>	
             * _parent.ActivateByIndex	=>  
             * 
             * _parent.ResetAcceleratorSilently => 
             * _parent.TryGetQfc	=>	
             * _parent.MoveDownPix
             */

            AcceleratorParser parser = new AcceleratorParser(mock.Object);
            parser.ParseAndExecute(strToParse: "3e", _intActiveSelection: 2);
            VerifyFunctionCalls(IsSelectionBelowMax: 1,
                                ActivateByIndex: 1,
                                MoveDownPix: 1,
                                ResetAcceleratorSilently: 1,
                                ToggleAcceleratorDialogue: 0,
                                ToggleOffActiveItem: 1,
                                OpenQFMail: 0,
                                TryGetQfc: 1);
        }
    }
}

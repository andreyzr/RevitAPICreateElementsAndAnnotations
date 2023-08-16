using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPITrainingLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPICreatePointObject
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public Pipe Pipe { get; }
        public List<FamilySymbol> FamilyTypes { get; }=new List<FamilySymbol>();
        public DelegateCommand SaveCommand { get;}


        public FamilySymbol SelectedFamilyType { get; set; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            Pipe = SelectionUtils.GetObject<Pipe>(commandData, "Выберете трубу");
            FamilyTypes =FamilySymbolUtils.GetFamilySymbols(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var locationCurve = Pipe.Location as LocationCurve;
            var pipeCurve = locationCurve.Curve;

            var oLavel = (Level)doc.GetElement(Pipe.LevelId);

            FamilyInstanceUtils.CreateFamilyInstance(_commandData, SelectedFamilyType, pipeCurve.GetEndPoint(0),oLavel);
            FamilyInstanceUtils.CreateFamilyInstance(_commandData, SelectedFamilyType, pipeCurve.GetEndPoint(1),oLavel);

            RaiseCloseRequest(); //закрытие окна

        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);//закрытие окна
        }
    }
}

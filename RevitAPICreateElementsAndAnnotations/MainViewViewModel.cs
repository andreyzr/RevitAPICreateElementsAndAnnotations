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

namespace RevitAPICreateElementsAndAnnotations
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<WallType> WallTypes { get; }=new List<WallType>();
        public List<Level> Levels { get;  } = new List<Level>();
        public DelegateCommand SaveCommand { get;  }
        public double WallHeight { get; set; }
        public List<XYZ> Points { get; } = new List<XYZ>();
        public WallType SelectedWallType { get; set; }
        public Level SelectedLevel { get; set; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            WallTypes = RevitAPITrainingLibrary.WallTools.GetWallTypes(commandData);
            Levels =LevelsUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            WallHeight = 100;
            Points = SelectionUtils.GetPoints(_commandData, "Выберете точки", ObjectSnapTypes.Endpoints);
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count < 2 ||
                SelectedWallType == null ||
                SelectedLevel == null)
                return;
            var curves=new List<Curve>();
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0)
                    continue;

                var prevPoint = Points[i-1];
                var currentPoin = Points[i];

                Curve curve=Line.CreateBound(prevPoint, currentPoin);
                curves.Add(curve);
            }

            using (var ts=new Transaction(doc,"Create wall"))
            {
                ts.Start();

                foreach (var curve in curves)
                {
                    Wall.Create(doc, curve,SelectedWallType.Id,SelectedLevel.Id,
                        UnitUtils.ConvertToInternalUnits(WallHeight,UnitTypeId.Millimeters),
                        0,false,false);
                }

                ts.Commit();
                
            }

            RaiseCloseRequest();

                    
        }
        public event EventHandler CloseRequest;

        private void RaiseCloseRequest ()
        {
            CloseRequest?.Invoke (this, EventArgs.Empty);//закрытие окна
        }
    }
}

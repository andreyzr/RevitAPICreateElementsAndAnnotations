using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
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

namespace RevitAPICreateDuct
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<DuctType> DuctTypes { get; } = new List<DuctType>();
        public List<MechanicalSystemType> DuctSystemType { get; }=new List<MechanicalSystemType>();
        public List<Level> Levels { get; } = new List<Level>();
        public DelegateCommand SaveCommand { get; }
        public double DuctHeight { get; set; }
        public List<XYZ> Points { get; } = new List<XYZ>();
        public DuctType SelectedDuctType { get; set; }
        public MechanicalSystemType SelectedSystemType { get; set; }
        public Level SelectedLevel { get; set; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DuctTypes = RevitAPITrainingLibrary.DuctTools.GetDuctTypes(commandData);
            DuctSystemType = GetSystemType(commandData);
            Levels = LevelsUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            DuctHeight = 100;
            Points = SelectionUtils.GetPoints(_commandData, "Выберете точки", ObjectSnapTypes.Endpoints);
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            
            if (Points.Count < 2 ||
                SelectedDuctType == null ||
                SelectedLevel == null)
                return;
            var points1 = new List<XYZ>();
            var points2 = new List<XYZ>();
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0)
                    continue;

                var point1 = Points[i - 1];
                var point2 = Points[i];

                points1.Add(point1);
                points2.Add(point2);
            }

            using (var ts = new Transaction(doc, "Create duct"))
            {
                ts.Start();

                for (int i = 0; i < points1.Count; i++)
                {
                    Duct duct=Duct.Create(doc, SelectedSystemType.Id, SelectedDuctType.Id, SelectedLevel.Id, points1[i], points2[i]);
                    Parameter ductHeight = duct.LookupParameter("Отметка посередине");
                    ductHeight.Set(UnitUtils.ConvertToInternalUnits(DuctHeight, UnitTypeId.Millimeters));
                }

                ts.Commit();
            }


            RaiseCloseRequest();


        }
        public event EventHandler CloseRequest;

        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);//закрытие окна
        }

        public static List<MechanicalSystemType> GetSystemType(ExternalCommandData commandData)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            List<MechanicalSystemType> ductTypes = new FilteredElementCollector(doc)
                                                        .OfClass(typeof(MechanicalSystemType))
                                                        .Cast<MechanicalSystemType>()
                                                        .ToList();

            return ductTypes;
        }
    }
}

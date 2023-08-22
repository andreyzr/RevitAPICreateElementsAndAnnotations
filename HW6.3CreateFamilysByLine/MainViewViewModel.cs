using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
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

namespace HW6._3CreateFamilysByLine
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<FamilyInstance> Furniture { get; } = new List<FamilyInstance>();
        public List<FamilySymbol> FurnitureSymbol { get; }
        public List<Level> Levels { get; } = new List<Level>();
        public DelegateCommand SaveCommand { get; }
        public double Number { get; set; }
        public List<XYZ> Points { get; } = new List<XYZ>();
        public FamilyInstance SelectedFurniture { get; set; }
        public FamilySymbol SelectedFurnitureSymbol { get; set; }
        public Level SelectedLevel { get; set; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            Furniture = GetFurniture(commandData);
            FurnitureSymbol = GetFamilySymbolsFurniture(commandData);
            Levels = GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            Points = GetPoints(_commandData, "Выберете точки", ObjectSnapTypes.Endpoints);
            Number = 2;
        }

        private void OnSaveCommand()
        {

            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count < 2 ||
                SelectedLevel == null || SelectedFurnitureSymbol == null)
                return;
            var curves = new List<Curve>();
            List<XYZ> points = new List<XYZ>();
            for (int i = 1; i < Points.Count; i+=2)
            {
                if (i == 0)
                    continue;

                var prevPoint = Points[i - 1]; 
                var currentPoin = Points[i];

                Curve curve1 = Line.CreateBound(prevPoint, currentPoin); 

                curves.Add(curve1);
            }

            using (var ts = new Transaction(doc, "Create duct"))
            {
                ts.Start();
                foreach (var curve in curves)
                {
                    XYZ A= curve.GetEndPoint(0);
                    XYZ B= curve.GetEndPoint(1);
                    XYZ c = B - A;
                    XYZ part = c * (1/ (Number+1));

                    for (int i = 0; i < Number; i++)
                    {
                        A += part;
                        points.Add(A);
                    }
                }
                
                foreach (var point in points)
                {
                    FamilyInstance furniture = doc.Create.NewFamilyInstance(point,
                    SelectedFurnitureSymbol, SelectedLevel, StructuralType.NonStructural);
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




        public static List<FamilyInstance> GetFurniture(ExternalCommandData commandData)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            List<FamilyInstance> doors = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Furniture)
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .ToList();

            return doors;
        }
        public static List<Level> GetLevels(ExternalCommandData commandData)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            List<Level> levels = new FilteredElementCollector(doc)
                                                .OfClass(typeof(Level))
                                                .Cast<Level>()
                                                .ToList();
            return levels;
        }
        public static List<XYZ> GetPoints(ExternalCommandData commandData,
            string promptMessage, ObjectSnapTypes objectSnapTypes)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            List<XYZ> points = new List<XYZ>();

            while (true)
            {
                XYZ pickedPoint = null;
                try
                {
                    pickedPoint = uidoc.Selection.PickPoint(objectSnapTypes, promptMessage);
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                {
                    break;
                }
                points.Add(pickedPoint);
            }

            return points;
        }
        public static List<FamilySymbol> GetFamilySymbolsFurniture(ExternalCommandData commandData)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;

            var familySymbols = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Furniture)
                .Cast<FamilySymbol>()
                .ToList();

            return familySymbols;
        }

    }
}

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

namespace HW6._2CreateFamilyInstance
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<FamilyInstance> Furniture { get; } = new List<FamilyInstance>();
        public List<FamilySymbol> FurnitureSymbol { get; }
        public List<Level> Levels { get; } = new List<Level>();
        public DelegateCommand SaveCommand { get; }
        public double DuctHeight { get; set; }
        public List<XYZ> Points { get; } = new List<XYZ>();
        public FamilyInstance SelectedFurniture { get; set; }
        public FamilySymbol SelectedFurnitureSymbol { get; set; }
        public Level SelectedLevel { get; set; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            Furniture= GetFurniture(commandData);
            FurnitureSymbol = GetFamilySymbolsFurniture(commandData);
            Levels = GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            Points = GetPoints(_commandData, "Выберете точки", ObjectSnapTypes.Endpoints);
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count ==0 ||
                SelectedLevel == null|| SelectedFurnitureSymbol==null)
                return;

            using (var ts = new Transaction(doc, "Create duct"))
            {
                ts.Start();


                foreach (var point in Points) 
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

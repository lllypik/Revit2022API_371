using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Prism.Commands;
using RevitAPILibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit2022API_371
{
    class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        public List<FamilySymbol> TitleBlocks { get; } = new List<FamilySymbol>();
        public FamilySymbol SelectedTitleBlock { get; set; }
        public int ParameterNumberSheets { get; set; }
        public string ParameterTextDesignBy { get; set; }
        public List<View> Views { get; } = new List<View>();
        public View SelectedView { get; set; }
        public List<Viewport> Viewports { get; set; }

        public DelegateCommand CreateSheetCommand { get; }
        public List<ViewSheet> Sheets { get; set; } = new List<ViewSheet>();


        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            TitleBlocks = FamilySymbolData.PickAllTitleBlockType(_commandData);
            Views = ViewportData.PickAllView(_commandData);
            CreateSheetCommand = new DelegateCommand(OnCreateSheetCommand);
        }

        private void OnCreateSheetCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            //create sheet
            if (SelectedTitleBlock != null || ParameterNumberSheets > 0)

            {
                using (var t = new Transaction(doc, "Create Sheets"))
                {
                    t.Start();

                    for (int i = 0; i < ParameterNumberSheets; i++)
                    {
                        ViewSheet Sheet = ViewSheet.Create(doc, SelectedTitleBlock.Id);
                        Sheets.Add(Sheet);
                    }                 
                    
                    t.Commit();
                }
            }

            //create viewport
            if (SelectedView != null)
            {
                using (var t = new Transaction(doc, "Create viewport"))
                {
                    t.Start();

                    XYZ poinInsertView = new XYZ(0, 0, 0);

                    //for (int i = 0; i < Sheets.Count; i++)
                    //{
                    //    Viewport viewport = Viewport.Create(doc, Sheets[i].Id, SelectedView.Id, poinInsertView);
                    //}

                    Viewport viewport = Viewport.Create(doc, Sheets[0].Id, SelectedView.Id, poinInsertView);

                    t.Commit();
                }
            }

            //entry DesignBy
            if (ParameterTextDesignBy != null || ParameterTextDesignBy != "")
            {
                using (var t = new Transaction(doc, "Parameter entry"))
                {
                    t.Start();

                    foreach (var sheet in Sheets)
                    {
                        Parameter parameterDesignBy = sheet.get_Parameter(BuiltInParameter.SHEET_DESIGNED_BY);
                        parameterDesignBy.Set(ParameterTextDesignBy);
                    }

                    t.Commit();
                }
            }

            RaiseCloseReqest();

        }

        public event EventHandler CloseReqest;
        private void RaiseCloseReqest()
        {
            CloseReqest?.Invoke(this, EventArgs.Empty);
        }

    }
}

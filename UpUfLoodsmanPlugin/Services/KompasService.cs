using Kompas6API5;
using Kompas6Constants;
using KompasAPI7;
using Pdf2d_LIBRARY;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using UpUfLoodsmanPlugin.Entities;
using Thread = System.Threading.Thread;

namespace UpUfLoodsmanPlugin.Services
{
    public class KompasService
    {

        KompasObject kompasObjectAPI5;
        IApplication KompasObjectAPI7;
        IKompasDocument kompasDocAPI7;
        ksDocument2D kompasDocAPI5;
        ksDocument2D temporaryDoc2DApi5;
        List<ModelVersion> modelVersions = new List<ModelVersion>();
        int temporaryGroup = 0;

        public KompasObject ConnectToKompasApp()
        {

            try
            {
                kompasObjectAPI5 = (KompasObject)Marshal.GetActiveObject("KOMPAS.Application.5");

                return kompasObjectAPI5;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public KompasObject StartKompas()
        {

            Type t = Type.GetTypeFromProgID("KOMPAS.Application.5");
            kompasObjectAPI5 = (KompasObject)Activator.CreateInstance(t);

            return kompasObjectAPI5;
        }

        public async Task<bool> ConnectOrStartKompas()
        {

            KompasObject kompasObjectAPI5 = ConnectToKompasApp();

            if (kompasObjectAPI5 == null)
            {
                kompasObjectAPI5 = StartKompas();
            }

            if (kompasObjectAPI5 != null)
            {
                kompasObjectAPI5.Visible = true;
                return true;
            }
            return false;
        }

        public async Task<bool> OpenDocument(string Path)
        {
            KompasObjectAPI7 = kompasObjectAPI5.ksGetApplication7();

            KompasObjectAPI7.HideMessage = ksHideMessageEnum.ksHideMessageYes;
            bool docVisible = true;
            kompasDocAPI7 = KompasObjectAPI7.Documents.Open(Path, docVisible, false);
            if (kompasDocAPI7 != null)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> OpenOrSelectDocument(string Path)
        {
            KompasObjectAPI7 = kompasObjectAPI5.ksGetApplication7();
            string docName = Path.Split('\\').Last();

            if (DocumentAreOpen(docName))
            {
                return true;
            }

            KompasObjectAPI7.HideMessage = ksHideMessageEnum.ksHideMessageYes;
            bool docVisible = true;
            kompasDocAPI7 = KompasObjectAPI7.Documents.Open(Path, docVisible, false);
            if (kompasDocAPI7 != null)
            {
                return true;
            }
            return false;
        }

        public bool DocumentAreOpen(string name)
        {
            KompasObjectAPI7 = kompasObjectAPI5.ksGetApplication7();
            IDocuments docs = KompasObjectAPI7.Documents;
            bool isOpened = false;

            if (docs != null)
            {
                int count = docs.Count;
                for (int i = 0; i < count; i++)
                {
                    IKompasDocument doc = docs[i];
                    if (doc != null && doc.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        doc.Active = true;
                        kompasDocAPI7 = doc;
                        isOpened = true;
                        break;
                    }
                }
            }
            return isOpened;

        }
        public async Task<bool> DeleteTrashFromDocument()
        {
            bool result = false;
            IKompasDocument2D kompasDocument2DAPI7 = (IKompasDocument2D)kompasDocAPI7;
            kompasDocAPI5 = kompasObjectAPI5.ActiveDocument2D();

            List<int> trash = GetAllTrash();
            List<int> deletedObject = new List<int>();
            foreach (int i in trash)
            {
                if (kompasDocAPI5.ksDeleteObj(i) == 1)
                {
                    deletedObject.Add(i);
                }
            }
            if (deletedObject.Count == trash.Count)
            {
                result = true;
            }
            return result;

        }

        public List<int> GetAllTrash()
        {
            List<int> trashReferences = new List<int>();
            IKompasDocument2D kompasDocument2DAPI7 = (IKompasDocument2D)kompasDocAPI7;
            kompasDocAPI5 = kompasObjectAPI5.ActiveDocument2D();

            IViewsAndLayersManager viewsAndLayersManager = kompasDocument2DAPI7.ViewsAndLayersManager;
            IViews views = viewsAndLayersManager.Views;

            for (int i = 0; i < views.Count; i++)
            {

                IView view = views.get_View(i);

                // Вид также является контейнером символов (ISymbols2DContainer)
                ISymbols2DContainer symContainer = (ISymbols2DContainer)view;
                ILineDimensions lineDims = symContainer.LineDimensions;


                //Ищем все размеры (не факт что будет работать с нелинейными)
                if (lineDims != null)
                {

                    for (int p = lineDims.Count; p >= 0; p--)
                    {

                        ILineDimension lineDim = lineDims.LineDimension[p];
                        if (lineDim != null)
                        {
                            int lineDimRef = lineDim.Reference;
                            trashReferences.Add(lineDimRef);
                        }

                    }

                }

                //Ищем все линии выноски
                ILeaders leaders = symContainer.Leaders;
                for (int j = leaders.Count - 1; j >= 0; j--)
                {
                    BaseLeader leader = leaders.Leader[j];

                    int leaderRef = leader.Reference;
                    trashReferences.Add(leaderRef);
                }
            }
            return trashReferences;
        }

        public int GetVievWithPictures()
        {
            List<int> result = new List<int>();
            IKompasDocument2D kompasDocument2DAPI7 = (IKompasDocument2D)kompasDocAPI7;
            kompasDocAPI5 = kompasObjectAPI5.ActiveDocument2D();

            IViewsAndLayersManager viewsAndLayersManager = kompasDocument2DAPI7.ViewsAndLayersManager;
            IViews views = viewsAndLayersManager.Views;
            for (int i = 0; i < views.Count; i++)
            {
                IView view = views.get_View(i);
                IDrawingContainer drawingContainer = (IDrawingContainer)view;
                IDrawingTexts drawingTexts = drawingContainer.DrawingTexts;

                if (drawingTexts != null)
                {
                    int count = drawingTexts.Count;
                    for (int t = 0; t < count; t++)
                    {

                        IDrawingText drawingText = drawingTexts.get_DrawingText(t);


                        IText text = drawingText as IText;
                        if (text != null)
                        {

                            string content = text.Str;

                            if (content.ToLower().Contains("изображение"))
                            {
                                if (result.FirstOrDefault(_ => _ == i) == 0)
                                {
                                    result.Add(i);

                                    
                                }
                                modelVersions.Add(new ModelVersion { Name = content, X = drawingText.X });
                            }

                        }
                    }
                }
            }
            if (result.Count == 1)
            {
                return result[0];
            }
            return 0;
        }

        public async Task<bool> CreatetemporaryGroup()
        {
            int viewWithPictures = GetVievWithPictures();

            if (viewWithPictures == 0)
            {
                return false;
            }
            IKompasDocument2D kompasDocument2DAPI7 = (IKompasDocument2D)kompasDocAPI7;
            kompasDocAPI5 = kompasObjectAPI5.ActiveDocument2D();

            IViewsAndLayersManager viewsAndLayersManager = kompasDocument2DAPI7.ViewsAndLayersManager;
            IViews views = viewsAndLayersManager.Views;

            IKompasDocument2D1 kompasDocument2D1 = (IKompasDocument2D1)kompasDocument2DAPI7;

            IDrawingGroups drGroups = kompasDocument2D1.DrawingGroups;

            ISelectionManager selectionManager = kompasDocument2D1.SelectionManager;
            for (int i = viewWithPictures; i < views.Count; i++)
            {
                IView view = views.get_View(i);
                IDrawingContainer drawingContainer = (IDrawingContainer)view;
                IMacroObjects macroObjects = drawingContainer.MacroObjects;
                if (macroObjects != null)
                {

                    List<string> macros = new List<string>();
                    temporaryGroup = kompasDocAPI5.ksNewGroup(1);

                    for (int j = 0; j < macroObjects.Count; j++)
                    {
                        IMacroObject macroObj = macroObjects.MacroObject[j];
                        string macroName = macroObj.Name;

                        int macroObjectId = macroObj.Reference;
                        kompasDocAPI5.ksLightObj(macroObjectId, 0);

                        //doc2D.ksOpenView(0);
                        //string viewName = view.Name;
                        //view.Current = true;

                        ksRectParam ksRectParam = (ksRectParam)kompasObjectAPI5.GetParamStruct((short)StructType2DEnum.ko_RectParam);
                        kompasDocAPI5.ksGetObjGabaritRect(macroObjectId, ksRectParam);

                        ksMathPointParam ksMathPointParam = (ksMathPointParam)kompasObjectAPI5.GetParamStruct((short)StructType2DEnum.ko_MathPointParam);

                        ksMathPointParam = ksRectParam.GetpBot();
                        double botX = ksMathPointParam.x;
                        double botY = ksMathPointParam.y;


                        ksMathPointParam = ksRectParam.GetpTop();

                        double topX = ksMathPointParam.x;
                        double topY = ksMathPointParam.y;

                        double widnt = topX - botX;
                        double height = topY - botY;


                        macros.Add($"{macroName} - {macroObjectId} width {widnt} height {height}");



                        selectionManager.Select(macroObj);

                        int addmacroId = kompasDocAPI5.ksAddObjGroup(temporaryGroup, macroObj.Reference);


                    }


                }
                IColourings colourings = drawingContainer.Colourings;
                if (colourings != null)
                {
                    for (int k = 0; k < colourings.Count; k++)
                    {
                        IColouring colouring = colourings.Colouring[k];
                        selectionManager.Select(colouring);
                        int testAdd2 = kompasDocAPI5.ksAddObjGroup(temporaryGroup, colouring.Reference);


                    }
                }

                ksCopyObjectParam copyObjectParam = (ksCopyObjectParam)kompasObjectAPI5.GetParamStruct((short)StructType2DEnum.ko_CopyObjectParam);

                if (kompasDocAPI5.ksExistGroupObj(temporaryGroup) != 0)
                {
                    int endedGroup = kompasDocAPI5.ksEndGroup();
                    int copyGroupToBuffer = kompasDocAPI5.ksWriteGroupToClip(temporaryGroup, true);
                    if (copyGroupToBuffer == 1)
                    {
                        double shiftX = 0;
                        double shiftY = 0;
                        // найденные изображения скопированы в буфер обмена, теперь надо создать новый документ и вставить их туда 

                        IKompasDocument2D newDoc2DAPI7 = CreateDoc2D();

                        ksDocument2D newDoc2DAPI5 = kompasObjectAPI5.ActiveDocument2D();

                        int readGroupFromBuffer = newDoc2DAPI5.ksReadGroupFromClip();

                        newDoc2DAPI5.ksStoreTmpGroup(readGroupFromBuffer);

                        viewsAndLayersManager = newDoc2DAPI7.ViewsAndLayersManager;
                        views = viewsAndLayersManager.Views;

                        for (int v = 0; v < views.Count; v++)
                        {
                            view = views.get_View(v);

                            // Вид также является контейнером символов (ISymbols2DContainer)
                            ISymbols2DContainer symContainer = (ISymbols2DContainer)view;
                            ILineDimensions lineDims = symContainer.LineDimensions;

                            drawingContainer = (IDrawingContainer)view;
                            IDrawingTexts drawingTexts = drawingContainer.DrawingTexts;

                            macroObjects = drawingContainer.MacroObjects;

                            kompasDocument2D1 = (IKompasDocument2D1)newDoc2DAPI7;

                            selectionManager = kompasDocument2D1.SelectionManager;

                            IDrawingGroups drawinfGroups = kompasDocument2D1.DrawingGroups;

                            IDrawingGroup drawingGroup = (IDrawingGroup)drawinfGroups.Add(true, "выделенные объекты");


                            List<string> newDocMacros = new List<string>();
                            newDoc2DAPI5.ksMoveObj(0, shiftX, shiftY);
                            if (macroObjects != null)
                            {

                                for (int j = 0; j < macroObjects.Count; j++)
                                {
                                    IMacroObject macroObj = macroObjects.MacroObject[j];
                                    string macroName = macroObj.Name;

                                    int macroObjectId = macroObj.Reference;
                                    newDoc2DAPI5.ksLightObj(macroObjectId, 0);

                                    //doc2D.ksOpenView(0);
                                    //string viewName = view.Name;
                                    //view.Current = true;

                                    ksRectParam ksRectParam = (ksRectParam)kompasObjectAPI5.GetParamStruct((short)StructType2DEnum.ko_RectParam);
                                    newDoc2DAPI5.ksGetObjGabaritRect(macroObjectId, ksRectParam);

                                    ksMathPointParam ksMathPointParam = (ksMathPointParam)kompasObjectAPI5.GetParamStruct((short)StructType2DEnum.ko_MathPointParam);

                                    ksMathPointParam = ksRectParam.GetpBot();
                                    double botX = ksMathPointParam.x;
                                    double botY = ksMathPointParam.y;


                                    ksMathPointParam = ksRectParam.GetpTop();

                                    double topX = ksMathPointParam.x;
                                    double topY = ksMathPointParam.y;

                                    double widnt = topX - botX;
                                    double height = topY - botY;

                                    if (shiftX == 0)
                                    {
                                        shiftX = -botX - 20;

                                        //foreach(ModelVersion model in modelVersions)
                                        //{
                                        //    model.X += shiftX;
                                        //}
                                    }
                                    if (shiftY == 0)
                                    {
                                        shiftY = -botY;
                                    }
                                    newDocMacros.Add($"{macroName} - {macroObjectId} width {widnt} height {height}");


                                    kompasDocument2D1 = (IKompasDocument2D1)newDoc2DAPI7;


                                    selectionManager = kompasDocument2D1.SelectionManager;

                                    selectionManager.Select(macroObj);



                                }
                                IColourings newDocColourings = drawingContainer.Colourings;
                                if (newDocColourings != null)
                                {
                                    for (int k = 0; k < newDocColourings.Count; k++)
                                    {
                                        IColouring colouring = newDocColourings.Colouring[k];

                                        selectionManager.Select(colouring);

                                    }
                                }

                            }

                            newDoc2DAPI5.ksMoveObj(0, shiftX, shiftY);
                            foreach(ModelVersion mv in modelVersions)
                            {
                                mv.X += shiftX;
                            }
                            selectionManager.UnselectAll();

                        }


                        return true;
                    }
                }

            }
            return false;
        }

        public async Task<List<PrintGroup>> SplitAndCreate()
        {
            List<PrintGroup> grouppsToPrint = new List<PrintGroup>();

            IKompasDocument kompasDoc = KompasObjectAPI7.ActiveDocument;
            IKompasDocument2D kompasDocument2D = (IKompasDocument2D)kompasDoc;
            IKompasDocument2D1 kompasDocument2D1 = (IKompasDocument2D1)kompasDocument2D;
            ISelectionManager selectionManager = kompasDocument2D1.SelectionManager;
            IViewsAndLayersManager viewsAndLayersManager = kompasDocument2D.ViewsAndLayersManager;
            IViews views = viewsAndLayersManager.Views;

            for (int i = 0; i < views.Count; i++)
            {
                IView view = views.get_View(i);
                IDrawingContainer drawingContainer = (IDrawingContainer)view;
                IDrawingTexts drawingTexts = drawingContainer.DrawingTexts;
                IMacroObjects macroObjects = drawingContainer.MacroObjects;
                ksDocument2D doc2DApi5 = kompasObjectAPI5.ActiveDocument2D();


                if (macroObjects != null)
                {

                    for (int j = 0; j < macroObjects.Count; j++)
                    {
                        IMacroObject macroObj = macroObjects.MacroObject[j];
                        string macroName = macroObj.Name;

                        int macroObjectId = macroObj.Reference;
                        doc2DApi5.ksLightObj(macroObjectId, 0);

                        //doc2D.ksOpenView(0);
                        //string viewName = view.Name;
                        //view.Current = true;

                        ksRectParam ksRectParam = (ksRectParam)kompasObjectAPI5.GetParamStruct((short)StructType2DEnum.ko_RectParam);
                        doc2DApi5.ksGetObjGabaritRect(macroObjectId, ksRectParam);

                        ksMathPointParam ksMathPointParam = (ksMathPointParam)kompasObjectAPI5.GetParamStruct((short)StructType2DEnum.ko_MathPointParam);

                        ksMathPointParam = ksRectParam.GetpBot();
                        double botX = ksMathPointParam.x;
                        double botY = ksMathPointParam.y;


                        ksMathPointParam = ksRectParam.GetpTop();

                        double topX = ksMathPointParam.x;
                        double topY = ksMathPointParam.y;

                        double widnt = topX - botX;
                        double height = topY - botY;

                        //Вот это очерь тонкое место, непонятно будут ли у них макроэлементы тазываться там или как-то иначе
                        if (macroName == "Макро:1")
                        {

                            ModelVersion modelVersion = modelVersions.FirstOrDefault(_ => _.X > botX && _.X < topX);

                            string outputPath = $@"C:\Users\SPankratov\source\repos\UpUfLoodsmanPlugin\UpUfLoodsmanPlugin\TestFiles\{modelVersion.Name}.pdf";

                            grouppsToPrint.Add(new PrintGroup($"{modelVersion.Name}", OperationStatus.Waiting, botX, botY, topX, topY, macroObjectId, outputPath));

                            int pointTest1 = doc2DApi5.ksPoint(botX + 20, topY - 20, 10);
                            int pointTest2 = doc2DApi5.ksPoint(topX, botY, 10);

                        }


                    }


                }
                temporaryDoc2DApi5 = doc2DApi5;

            }

            return grouppsToPrint;
        }

        public async Task<bool> CreatePdf(PrintGroup pGroup, int number)
        {
            IKompasDocument kompasDoc = KompasObjectAPI7.ActiveDocument;
            IKompasDocument2D kompasDocument2D = (IKompasDocument2D)kompasDoc;
            IKompasDocument2D1 kompasDocument2D1 = (IKompasDocument2D1)kompasDocument2D;
            ISelectionManager selectionManager = kompasDocument2D1.SelectionManager;
            IViewsAndLayersManager viewsAndLayersManager = kompasDocument2D.ViewsAndLayersManager;
            IViews views = viewsAndLayersManager.Views;
            IView view = views.get_View(0);
            IDrawingContainer drawingContainer = (IDrawingContainer)view;
            IDrawingTexts drawingTexts = drawingContainer.DrawingTexts;
            IMacroObjects macroObjects = drawingContainer.MacroObjects;
            ksDocument2D doc2DApi5 = kompasObjectAPI5.ActiveDocument2D();

            doc2DApi5.ksDeleteObj(pGroup.Reference);
            doc2DApi5.ksRebuildDocument();

            double minX = pGroup.Xmin;
            double minY = pGroup.Ymin;
            double maxX = pGroup.Xmax;
            double maxY = pGroup.Ymax;
            int newG = doc2DApi5.ksSelectGroup(0, 1, minX+ 0.1, minY, maxX, maxY);
            

            int ival2 = doc2DApi5.ksNewGroup(1);

            //selectionManager.UnselectAll();

            var selectedArray = (object[])selectionManager.SelectedObjects;

            int testCount = selectedArray.Length;

            if (selectedArray != null)
            {
                foreach (var obj in selectedArray)
                {
                    // Cast to target interface, e.g., IKompasAPIObject
                    var apiObj = obj as IKompasAPIObject;

                    int testRef = apiObj.Reference;
                    int testAdd1 = doc2DApi5.ksAddObjGroup(ival2, testRef);

                }
            }

            int newEndGroup3 = doc2DApi5.ksEndGroup();
            int testCopy1 = doc2DApi5.ksWriteGroupToClip(newEndGroup3, true);
            int testCopy2 = doc2DApi5.ksWriteGroupToClip(ival2, true);


            ksDocumentParam ksDocumentParam = (ksDocumentParam)kompasObjectAPI5.GetParamStruct((short)StructType2DEnum.ko_DocumentParam);
            ksDocumentParam.Init();
            ksDocumentParam.fileName = $"Временный чертёж {number}";

            ksDocument2D newDoc = (ksDocument2D)kompasObjectAPI5.Document2D();
            if (newDoc != null)
            {
                newDoc.ksCreateDocument(ksDocumentParam);

            }
            int testRead3 = newDoc.ksReadGroupFromClip();
            newDoc.ksMoveObj(testRead3, -pGroup.Xmin - 20, 0);
            int newRef = newDoc.ksStoreTmpGroup(testRead3);

            //string fileName = $@"C:\Users\SPankratov\Desktop\Модели\УФ\Итоговые PDF\Временный чертёж {number}.pdf";


            Converter converter = KompasObjectAPI7.Converter[@"C:\Users\SPankratov\source\repos\UpUfLoodsmanPlugin\Libraries\Pdf2d.dll"];
            object rawParams = converter.ConverterParameters(0);
            IPdf2dParam pdf2DParam = rawParams as IPdf2dParam;

            pdf2DParam.ColorType = 3;

            //string newFileName = $@"C:\Users\SPankratov\source\repos\UpUfLoodsmanPlugin\UpUfLoodsmanPlugin\TestFiles\Тестовый файл {number}.pdf";
            string newFileName = pGroup.OutputPath;

            if (File.Exists(newFileName))
            {
                File.Delete(newFileName);
            }
            int createPdfCheck = converter.Convert("", newFileName, 0, false);

            //bool checkSave = newDoc.ksSaveDocument(fileName);
            newDoc.ksCloseDocument();

            if (createPdfCheck == 1) { return true; }
            return false;

        }


        public IKompasDocument2D CreateDoc2D()
        {
            IDocuments docs = KompasObjectAPI7.Documents;
            IKompasDocument2D newDoc2DApi7 = (IKompasDocument2D)docs.AddWithDefaultSettings(DocumentTypeEnum.ksDocumentDrawing);

            IDrawingDocument drawDoc = (IDrawingDocument)newDoc2DApi7;

            if (drawDoc != null)
            {

                ILayoutSheets sheets = drawDoc.LayoutSheets;
                if (sheets != null && sheets.Count > 0)
                {
                    // Берем первый лист (индекс 0)
                    ILayoutSheet sheet = sheets[0];
                    sheet.LayoutStyleNumber = 0;
                    sheet.Update();

                }
            }
            return newDoc2DApi7;
        }

        public void CloseTemporaryDocument()
        {
            temporaryDoc2DApi5.ksCloseDocument();
        }
    }
}

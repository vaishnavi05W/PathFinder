namespace PathFinder.gui
{
    using PathFinder.util;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using vdIFC;
    using VectorDraw.Geometry;
    using VectorDraw.Professional.Constants;
    using VectorDraw.Professional.vdFigures;
    using VectorDraw.Professional.vdObjects;
    using VectorDraw.Professional.vdPrimaries;
    using static System.Net.Mime.MediaTypeNames;

    public partial class HiddenExport : Form
    {
        private Info info;
        private VectorDraw.Professional.Control.VectorDrawBaseControl vdbase;
        public HiddenExport(VectorDraw.Professional.Control.VectorDrawBaseControl vdc, Info info)
        {
            InitializeComponent();
            vdbase = vdc;
            this.info = info;
          
            
        }
        vdIFCBuildingStorey vdIFCBuildingStorey;
        public void importIFC(vdIFCBuildingStorey storey, vdDocument doc)
        {
            vdIFCBuildingStorey = storey;
            IFCToCAD.getIFCToCAD(storey, info.floor, doc);
            this.vectorDrawBaseControl1.ActiveDocument.Redraw(true);
            this.vectorDrawBaseControl1.ActiveDocument.ZoomExtents();
           // this.setData();
        }
        private void HiddenExport_Load(object sender, EventArgs e)
        {
            if (vdbase != null)
            {
                this.vectorDrawBaseControl1= vdbase;

                this.vectorDrawBaseControl1.ActiveDocument.Redraw(true);
                this.vectorDrawBaseControl1.ActiveDocument.ZoomExtents();
                Export();
            }
            //this.Close();
        }
        private void Export()
        {
            SaveFileDialog sd = new SaveFileDialog();

            sd.Filter = "AutoCAD|*.dwg";
            List<vdLayer> newLayers = new List<vdLayer>();
            List<vdPolyline> newPolylines = new List<vdPolyline>();
            List<vdCircle> newCircles = new List<vdCircle>();
            var newDoc = vdbase.ActiveDocument;
            if (sd.ShowDialog(this) == DialogResult.OK)
            {
                short count = 10;
                foreach (SequenceGroup sequenceGroup in this.info.sequenceGroups)
                {
                    count++;
                    vdLayer layer = newDoc.Layers.Add(sequenceGroup.definedSequence.name);
                    newLayers.Add(layer);


                    vdPolyline poly = new vdPolyline(newDoc, sequenceGroup.getShortestPath());
                    newPolylines.Add(poly);

                    layer.PenColor.ColorIndex = count;

                    layer.Name = sequenceGroup.ToString();
                    //poly.visibility = vdFigure.VisibilityEnum.Invisible;
                    newDoc.ActiveLayOut.Entities.AddItem(poly);
                    poly.Layer = layer;
                    newDoc.SetActiveLayer(layer); ////PTK Added Layer
                    poly.SetUnRegisterDocument(newDoc);
                    poly.setDocumentDefaults();

                    List<vdPolyline> polylines2 = writeTriangle(poly);
                    foreach (vdPolyline polyline in polylines2)
                    {
                        newDoc.ActiveLayOut.Entities.AddItem(polyline);
                        polyline.SetUnRegisterDocument(newDoc);
                        polyline.setDocumentDefaults();
                        polyline.PenColor = new vdColor(Color.Yellow);
                        newDoc.Update();
                        newDoc.Redraw(true);
                        newDoc.ActiveLayer.Update();
                    }


                    List<vdPolyline> polylines = sequenceGroup.getShortestPaths(newDoc);

                    if (polylines.Count == 0) continue;

                    vdPolyline first = polylines.First();
                    vdCircle vdCircle = new vdCircle((newDoc));
                    newCircles.Add(vdCircle);
                    vdCircle.Layer = layer;
                    newDoc.Model.Entities.AddItem(vdCircle);
                    vdCircle.SetUnRegisterDocument(newDoc);
                    vdCircle.setDocumentDefaults();
                    vdCircle.Center = first.getStartPoint();
                    poly.PenColor.SystemColor = Color.Green; //Using system color 
                    vdCircle.Radius = 800;
                    vdCircle.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);

                    vdPolyline last = polylines.Last();
                    vdCircle vdCircle2 = new vdCircle((newDoc));
                    newCircles.Add(vdCircle2);
                    vdCircle.Layer = layer;
                    newDoc.Model.Entities.AddItem(vdCircle2);
                    vdCircle2.SetUnRegisterDocument(newDoc);
                    vdCircle2.setDocumentDefaults();
                    vdCircle2.Center = last.getEndPoint();
                    poly.PenColor.SystemColor = Color.Green;  //Using system color 
                    vdCircle2.Radius = 800;
                    vdCircle2.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);

                    foreach (vdPolyline poly2 in polylines)
                    {
                        if (poly2 != null)
                        {

                            vdCircle circle3 = new vdCircle(newDoc);
                            newCircles.Add(circle3);
                            circle3.Center = poly2.getStartPoint();
                            circle3.Radius = 400;
                            circle3.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);
                            circle3.PenColor.Red = 255; circle3.PenColor.Green = 0; circle3.PenColor.Blue = 0;
                            newDoc.Model.Entities.AddItem(circle3);
                            vdCircle circle4 = new vdCircle(newDoc);
                            newCircles.Add(circle4);
                            circle4.Center = poly.getEndPoint();
                            circle4.Radius = 400;
                            circle4.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);
                            circle4.PenColor.Red = 255; circle4.PenColor.Green = 0; circle4.PenColor.Blue = 0;
                            newDoc.Model.Entities.AddItem(circle4);
                        }
                    }

                }

                setText(newDoc); //PTK Added Room
                setColor(newDoc); //PTK added Color
                                  // clearTriagle();
                newDoc.Update();
                newDoc.Redraw(true);

                newDoc.SaveAs(sd.FileName);

                foreach (vdCircle circle in newCircles)
                {
                    newDoc.ActiveLayOut.Entities.RemoveItem(circle);
                }
                foreach (vdPolyline poly in newPolylines)
                {
                    newDoc.ActiveLayOut.Entities.RemoveItem(poly);
                }
                foreach (vdLayer layer in newLayers)
                {
                    newDoc.Layers.RemoveItem(layer);
                }
                newDoc.Update();
                newDoc.Redraw(true);
            }
        }
        public void setColor(vdDocument doc)
        {
            foreach (SequenceGroup sequenceGroup in this.info.sequenceGroups)
            {
                Random r = new Random();
                foreach (vdFigure ent in this.vectorDrawBaseControl1.ActiveDocument.ActiveLayOut.Entities.ArrayItems)
                {
                    if (ent.Layer.Name == sequenceGroup.definedSequence.name)
                    {
                        if (ent is vdPolyline pl)
                        {
                            if (pl.HatchProperties == null && pl.Flag == VdConstPlineFlag.PlFlagOPEN)
                            {


                                pl.PenColor.SystemColor = Color.FromArgb(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));
                                this.vectorDrawBaseControl1.ActiveDocument.ActiveLayer.Update();
                                this.vectorDrawBaseControl1.ActiveDocument.ActiveLayer.Update();
                                this.vectorDrawBaseControl1.ActiveDocument.Update();
                                this.vectorDrawBaseControl1.ActiveDocument.Redraw(true);
                            }
                        }
                    }
                }
            }
        }
        public void setText(vdDocument doc)
        {
            foreach (var room in info.floor.roomList)
            {

                this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.RemoveItem(room.text);
                text.Height = 200;
                text.TextString = room.name;
                text.Update();
                double cen = text.BoundingBox.Width / 2.0;
                text.SetUnRegisterDocument(doc);
                text.setDocumentDefaults();
                text.InsertionPoint = new VectorDraw.Geometry.gPoint(room.centerPoint.x - cen, room.centerPoint.y + 100, room.centerPoint.z);
                try
                {
                    this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(room.text);
                }
                catch { }
                this.vectorDrawBaseControl1.ActiveDocument.ActiveLayer.Update();
                this.vectorDrawBaseControl1.ActiveDocument.ActiveLayer.Update();
                this.vectorDrawBaseControl1.ActiveDocument.Update();
                this.vectorDrawBaseControl1.ActiveDocument.Redraw(true);
            }
        }
        public VectorDraw.Professional.vdFigures.vdText text = new VectorDraw.Professional.vdFigures.vdText();
        public List<vdPolyline> writeTriangle(vdPolyline polyline)
        {


            int count = (int)(polyline.Length() / 2000);

            List<vdPolyline> polylines = new List<vdPolyline>();
            gPoints gps = polyline.Divide(count);
            if (gps != null)
            {


                foreach (gPoint gp in gps)
                {


                    int index = polyline.SegmentIndexFromPoint(gp, 0.1);
                    vdCurve curve = polyline.GetSegmentAtPoint(gp);
                    polyline.IsClockwise();
                    gPoint firstPoint = curve.getStartPoint();
                    gPoint lastPoint = curve.getEndPoint();



                    Vector v = new Vector(firstPoint, lastPoint);
                    double angle = v.Angle2DDirection() - Math.PI / 2.0;


                    vdPolyline poly = new vdPolyline(polyline.Document);// preapred PTK

                    poly.VertexList.Add(new VectorDraw.Geometry.gPoint(-100, -100));
                    poly.VertexList.Add(new VectorDraw.Geometry.gPoint(100, -100));
                    poly.VertexList.Add(new VectorDraw.Geometry.gPoint(0, 100));
                    poly.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);
                    poly.PenColor = new vdColor(Color.Yellow);
                    poly.LineWeight = VdConstLineWeight.LW_25;
                    poly.Flag = VdConstPlineFlag.PlFlagCLOSE;
                    Matrix m = new Matrix();
                    m.RotateZMatrix(angle);
                    m.TranslateMatrix(gp);
                    poly.Transformby(m);
                    //poly.Draw(render);
                    polylines.Add(poly);
                    //if (lstCacheTriangle == null)
                    //    lstCacheTriangle = new List<vdPolyline>();
                    //lstCacheTriangle.Add(poly);
                }

            }

            return polylines;
        }
    }
}

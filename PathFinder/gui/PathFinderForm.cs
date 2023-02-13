using System;
using System;
using System.Collections;
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
using VectorDraw.Professional.vdCollections;
using VectorDraw.Professional.vdFigures;
using VectorDraw.Professional.vdObjects;
using VectorDraw.Professional.vdPrimaries;
/*using ClipperLib;*/
using VectorDraw.Generics;
using VectorDraw.Professional.Constants;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using PathFinder.util;
using System.Runtime.CompilerServices;
using PathFinder.analysis;
using PathFinder.gui;
using static RenderFormats.PrimitiveRender3d;
using System.Threading;
using VectorDraw.SolidModel;
using VectorDraw.Render;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Text.RegularExpressions;
using VectorDraw.Professional.Control;

namespace PathFinder
{
    public partial class PathFinderForm : Form
    {
        public Info info;
        vdPolyline routeLine = null;
        Thread analysisThread = null;
        private vdRender render;
        private bool IsErase = false;
        private Stopwatch watch = new Stopwatch();
        public PathFinderForm()
        {
            InitializeComponent();
            this.Visible = true;
            info = new Info();

            roomGroupControl1.groupChangedReceiveMsg += RoomGroupControl1_groupChangedReceiveMsg;
            roomGroupControl1.roomSelectionChangedEventMsg += RoomGroupControl1_roomSelectionChangedEventMsg;
            roomGroupControl1.roomHighlightChangedEventMsg += RoomGroupControl1_roomHighlightChangedEventMsg;
            sequenceSettingControlcs1.sequenceGroupChangedReceiveMsg += sequenceSettingControlcs1_sequenceGroupChangedReceiveMsg;
            sequenceSettingControlcs1.sequenceNameChangedReceiveMsg += SequenceSettingControlcs1_sequenceNameChangedReceiveMsg;
            sequenceSettingControlcs1.sequenceSelectionChangedEventHanlder += SequenceSettingControlcs1_sequenceSelectionChangedEventHanlder;
            sequenceSettingControlcs1.sequenceHighlightChangedReceiveMsg += SequenceSettingControlcs1_sequenceHighlightChangedReceiveMsg;
            mainRouteControl1.mainRouteSelectionChangedEventMsg += MainRouteControl1_mainRouteSelectionChangedEventMsg;
            mainRouteControl1.mainRouteHighlightChangedEventMsg += MainRouteControl1_mainRouteHighlightChangedEventMsg;
            algorithComboBox.SelectedIndex = 0;
            toolStripComboBox1.SelectedIndex = 0;
        }

        private void SequenceSettingControlcs1_sequenceHighlightChangedReceiveMsg(object sender, List<Room> rooms)
        {
            vectorDrawBaseControl1.Redraw();
        }

        private void SequenceSettingControlcs1_sequenceSelectionChangedEventHanlder(object sender, EventArgs e)
        {
            vectorDrawBaseControl1.Redraw();
        }

        private void MainRouteControl1_mainRouteHighlightChangedEventMsg(object sender, List<Room> e)
        {
            vectorDrawBaseControl1.Redraw();
        }

        private void MainRouteControl1_mainRouteSelectionChangedEventMsg(object sender, List<Room> e)
        {
            vectorDrawBaseControl1.Redraw();
        }

        private void RoomGroupControl1_roomHighlightChangedEventMsg(object sender, List<Room> e)
        {
            vectorDrawBaseControl1.Redraw();
        }

        private void SequenceSettingControlcs1_sequenceNameChangedReceiveMsg(object sender, EventArgs e)
        {
            analysisRouteControl1.setSequence();
        }

        private void RoomGroupControl1_roomSelectionChangedEventMsg(object sender, List<Room> e)
        {
            vectorDrawBaseControl1.Redraw();
        }



        private void RoomGroupControl1_groupChangedReceiveMsg(object sender, EventArgs e)
        {
            sequenceSettingControlcs1.setRoomGroups(this.info);
            this.sequenceSettingControlcs1.setSequenceGroups(info.sequenceGroups);
        }
        public VectorDraw.Professional.vdFigures.vdText text = new VectorDraw.Professional.vdFigures.vdText();
        private void sequenceSettingControlcs1_sequenceGroupChangedReceiveMsg(object sender, EventArgs e)
        {
            info.setsequenceGroup();
            analysisRouteControl1.setSequence(info, this.vectorDrawBaseControl1.ActiveDocument);


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
                            if (pl.HatchProperties == null && pl.Flag == VdConstPlineFlag.PlFlagOPEN )
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

        public void setData()
        {
     
            List<RoomTable> Rlist1 = new List<RoomTable>();

            this.roomGroupControl1.setRoomList(info.floor.roomList);
            this.sequenceSettingControlcs1.setRoomList(info.floor.roomList);
            this.mainRouteControl1.setRoomList(info.floor.roomList);



            IFCToCAD.setRoomRelation(info.floor, info.roomRelations, this.vectorDrawBaseControl1.ActiveDocument);

            IFCToCAD.setRelationObstacleWithRoom(info.floor);





            List<Connector> connectors = new List<Connector>();
            foreach (Connector c in info.floor.connectorList)
            {
                if (c.roomList.Count == 2 )
                {
                    c.shape.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeDoubleHatch);
                    c.shape.PenColor = new vdColor(Color.Yellow);
                }

                /*
                    if (c.roomList.Count > 2)
                {
                    c.shape.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeDoubleHatch);
                    c.shape.PenColor = new vdColor(Color.Blue);
                }
                if (c.roomList.Count == 0)
                {
                    c.shape.PenColor = new vdColor(Color.Green);
                    c.shape.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeHatchCross);

                }
                if (c.roomList.Count == 1)
                {
                    c.shape.PenColor = new vdColor(Color.Magenta);
                    c.shape.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeHatchDiagCross);

                }
                */
            }

            foreach (Connector c in connectors)
            {
                info.floor.connectorList.Remove(c);
            }



            ReadWriteUtil.readRoomGroupList(info.floor, info.roomGroups);
            ReadWriteUtil.readSequenceList(info.floor, info.roomGroups, info.sequenceGroups);
            ReadWriteUtil.readMainRouteList(info.floor, info.mainRoutes);

            this.roomGroupControl1.setRoomGroups(info);
            this.sequenceSettingControlcs1.setRoomGroups(info);
            this.sequenceSettingControlcs1.setSequenceGroups(info.sequenceGroups);
            this.analysisRouteControl1.setSequence(info, this.vectorDrawBaseControl1.ActiveDocument);
            this.mainRouteControl1.setMainRoutes(info);


            var rtList = new List<RouteTable>();
            int count = 0;
            foreach (RoomRelation rr in info.roomRelations)
            {
                string n1 = rr.sRoom.name;
                string n2 = rr.eRoom.name;
                if (rr.roomLists.Count > 0)
                {

                    foreach (ArrayList list in rr.roomLists)
                    {
                        string rooms = "";
                        foreach (Room r in list) rooms += r.name + ",";
                        rooms = rooms.Substring(0, rooms.Length - 1);
                        rtList.Add(new RouteTable()
                        {
                            Index = count++,
                            SRoom = n1,
                            ERoom = n2,
                            Route = rooms
                        }); ;
                    }
                }
            }

            info.setsequenceGroup();

            dataGridView1.DataSource = rtList;


            this.tabPage2.Text = info.floor.name;
            watch.Stop();
        } 
        public void SelectAllRoute()
        {
            IsErase = false;
            analysisRouteControl1.sequenceGroupDataGridView.ClearSelection();
            this.vectorDrawBaseControl1.ActiveDocument.Document.Update();
            this.vectorDrawBaseControl1.ActiveDocument.Document.Redraw(true);
            
        }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //analysisRouteControl1.sequenceGroupDataGridView.ClearSelection();
            SelectAllRoute();
            //return;

            SaveFileDialog sd = new SaveFileDialog();

            sd.Filter = "AutoCAD|*.dwg";
            List<vdLayer> newLayers = new List<vdLayer>();
            List<vdPolyline> newPolylines = new List<vdPolyline>();
            List<vdCircle> newCircles = new List<vdCircle>();
            var newDoc = this.vectorDrawBaseControl1.ActiveDocument;
            if (sd.ShowDialog(this) == DialogResult.OK)
            {
                short count = 10;
                foreach (SequenceGroup sequenceGroup in this.info.sequenceGroups)
                {
                    count++;
                    vdLayer layer = this.vectorDrawBaseControl1.ActiveDocument.Layers.Add(sequenceGroup.definedSequence.name);
                    //layer.VisibleOnForms = false;
                    
                    newLayers.Add(layer); 

                    vdPolyline poly = new vdPolyline(this.vectorDrawBaseControl1.ActiveDocument, sequenceGroup.getShortestPath());
                    newPolylines.Add(poly);

                    layer.PenColor.ColorIndex = count;

                    layer.Name = sequenceGroup.ToString(); 
                    
                    this.vectorDrawBaseControl1.ActiveDocument.ActiveLayOut.Entities.AddItem(poly);
                    poly.Layer = layer;
                    vectorDrawBaseControl1.ActiveDocument.SetActiveLayer(layer); ////PTK Added Layer
                    poly.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                    poly.setDocumentDefaults();

                    List<vdPolyline> polylines2 = writeTriangle(poly);
                    foreach (vdPolyline polyline in polylines2)
                    {
                        this.vectorDrawBaseControl1.ActiveDocument.ActiveLayOut.Entities.AddItem(polyline);
                        polyline.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                        polyline.setDocumentDefaults();
                        polyline.PenColor = new vdColor(Color.Yellow);
                        this.vectorDrawBaseControl1.ActiveDocument.Update();
                        this.vectorDrawBaseControl1.ActiveDocument.Redraw(true);
                        this.vectorDrawBaseControl1.ActiveDocument.ActiveLayer.Update();
                    }


                    List<vdPolyline> polylines = sequenceGroup.getShortestPaths(this.vectorDrawBaseControl1.ActiveDocument);

                    if (polylines.Count == 0) continue;

                    vdPolyline first = polylines.First();
                    vdCircle vdCircle = new vdCircle((this.vectorDrawBaseControl1.ActiveDocument));
                    newCircles.Add(vdCircle);
                    vdCircle.Layer = layer;
                    this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(vdCircle);
                    vdCircle.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                    vdCircle.setDocumentDefaults();
                    vdCircle.Center = first.getStartPoint();
                    poly.PenColor.SystemColor = Color.Green; //Using system color 
                    vdCircle.Radius = 800;
                    vdCircle.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);

                    vdPolyline last = polylines.Last();
                    vdCircle vdCircle2 = new vdCircle((this.vectorDrawBaseControl1.ActiveDocument));
                    newCircles.Add(vdCircle2);
                    vdCircle.Layer = layer;
                    this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(vdCircle2);
                    vdCircle2.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                    vdCircle2.setDocumentDefaults();
                    vdCircle2.Center = last.getEndPoint();
                    poly.PenColor.SystemColor = Color.Green;  //Using system color 
                    vdCircle2.Radius = 800;
                    vdCircle2.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);

                    foreach (vdPolyline poly2 in polylines)
                    {
                        if (poly2 != null)
                        {

                            vdCircle circle3 = new vdCircle(this.vectorDrawBaseControl1.ActiveDocument);
                            newCircles.Add(circle3);
                            circle3.Center = poly2.getStartPoint();
                            circle3.Radius = 400;
                            circle3.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);
                            circle3.PenColor.Red = 255; circle3.PenColor.Green = 0; circle3.PenColor.Blue = 0;
                            this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(circle3);
                            vdCircle circle4 = new vdCircle(this.vectorDrawBaseControl1.ActiveDocument);
                            newCircles.Add(circle4);
                            circle4.Center = poly.getEndPoint();
                            circle4.Radius = 400;
                            circle4.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);
                            circle4.PenColor.Red = 255; circle4.PenColor.Green = 0; circle4.PenColor.Blue = 0;
                            this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(circle4);
                        }
                    }

                }

                setText(getDoc()); //PTK Added Room
                setColor(getDoc()); //PTK added Color

                this.vectorDrawBaseControl1.ActiveDocument.SaveAs(sd.FileName);
               

                clearTriagle();
                foreach (vdCircle circle in newCircles)
                {
                    this.vectorDrawBaseControl1.ActiveDocument.ActiveLayOut.Entities.RemoveItem(circle);
                }
                foreach (vdPolyline poly in newPolylines)
                {
                    this.vectorDrawBaseControl1.ActiveDocument.ActiveLayOut.Entities.RemoveItem(poly);
                }
                foreach (vdLayer layer in newLayers)
                {
                    this.vectorDrawBaseControl1.ActiveDocument.Layers.RemoveItem(layer);
                }

                this.vectorDrawBaseControl1.ActiveDocument.Update();
                this.vectorDrawBaseControl1.ActiveDocument.Redraw(true);
            }
            //HiddenExport export= new HiddenExport(this.vectorDrawBaseControl1 , this.info);
            //export.ShowDialog();
            //export.Hide();

        }
         

        vdIFCBuildingStorey vdIFCBuildingStorey; 
        public void importIFC(vdIFCBuildingStorey storey, vdDocument doc)
        {
            vdIFCBuildingStorey = storey;
            IFCToCAD.getIFCToCAD(storey, info.floor, doc);
            this.vectorDrawBaseControl1.ActiveDocument.Redraw(true);
            this.vectorDrawBaseControl1.ActiveDocument.ZoomExtents();
            this.setData();
        }

        private void vectorDrawBaseControl1_DrawAfter(object sender, VectorDraw.Render.vdRender render)
        {

            Graphics gr = this.vectorDrawBaseControl1.ActiveDocument.GlobalRenderProperties.GraphicsContext.MemoryGraphics;
            Rectangle rc = new Rectangle(new Point(0, 0), this.vectorDrawBaseControl1.ActiveDocument.GlobalRenderProperties.GraphicsContext.MemoryBitmap.Size);
            rc.Inflate(1, 1);
            Font font = new Font("Verdana", 20);
            ;


            try
            {
                drawText(info.floor, font, Color.White, render);
                drawRoute(render);
                selectRooms(render);

            }
            catch (Exception ex) {

            }
        }

        public void drawText(Floor floor, Font font, Color color, VectorDraw.Render.vdRender render)
        {
            
                foreach (Room room in floor.roomList)
                {
                    room.text.PenColor.SystemColor = color;
                    room.text.Update();
                    room.text.Draw(render);
                }
            
        }
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
                    if (lstCacheTriangle == null)
                        lstCacheTriangle = new List<vdPolyline>();
                    lstCacheTriangle.Add(poly);
                }

            }
            
            return polylines;
        }
        public void drawRoute(VectorDraw.Render.vdRender render)
        {
            var eras = this.ActiveControl.Name;
            var doc = this.vectorDrawBaseControl1.ActiveDocument;
            DataGridViewSelectedRowCollection drselect = analysisRouteControl1.sequenceGroupDataGridView.SelectedRows;
            if (drselect.Count == 0 && !IsErase)
            {
                
                foreach (var sequenceGroup in info.sequenceGroups)
                {
                    info.routes = sequenceGroup.getShortestPaths(doc);
                    info.route = new vdPolyline(doc, sequenceGroup.getShortestPath());
                    DrawRoutePath(render);
                }
            }
            else
            { 
                DrawRoutePath(render);
            }


        }
        private void DrawRoutePath(VectorDraw.Render.vdRender render)
        {
            if (info.route != null)
                info.route.Draw(render);

            List<vdPolyline> polylines = writeTriangle(info.route);

            foreach (vdPolyline polyline in polylines)
                polyline.Draw(render);


            if (info.routes != null && info.routes.Count > 0)
            {

                vdPolyline first = info.routes.First();
                if (first != null)
                {
                    vdCircle circle11 = new vdCircle(this.vectorDrawBaseControl1.ActiveDocument);
                    circle11.Center = first.getStartPoint();
                    circle11.Radius = 800;
                    circle11.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);
                    circle11.PenColor.Red = 0; circle11.PenColor.Green = 0; circle11.PenColor.Blue = 255;
                    circle11.Draw(render);
                }
                vdPolyline last = info.routes.Last();
                if (last != null)
                {
                    vdCircle circle22 = new vdCircle(this.vectorDrawBaseControl1.ActiveDocument);
                    circle22.Center = last.getEndPoint();
                    circle22.Radius = 800;
                    circle22.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);
                    circle22.PenColor.Red = 0; circle22.PenColor.Green = 0; circle22.PenColor.Blue = 255;
                    circle22.Draw(render);

                }
                foreach (vdPolyline poly in info.routes)
                {
                    if (poly != null)
                    {
                        poly.Draw(render);
                        vdCircle circle1 = new vdCircle(this.vectorDrawBaseControl1.ActiveDocument);
                        circle1.Center = poly.getStartPoint();
                        circle1.Radius = 500;
                        circle1.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);
                        circle1.PenColor.Red = 255; circle1.PenColor.Green = 0; circle1.PenColor.Blue = 0;

                        circle1.Draw(render);


                        vdCircle circle2 = new vdCircle(this.vectorDrawBaseControl1.ActiveDocument);
                        circle2.Center = poly.getEndPoint();
                        circle2.Radius = 500;
                        circle2.HatchProperties = new VectorDraw.Professional.vdObjects.vdHatchProperties(VectorDraw.Professional.Constants.VdConstFill.VdFillModeSolid);
                        circle2.PenColor.Red = 255; circle2.PenColor.Green = 0; circle2.PenColor.Blue = 0;

                        circle2.Draw(render);


                        vdPolyline divide = new vdPolyline(this.vectorDrawBaseControl1.ActiveDocument);




                    }
                }
            }
        }
        public void selectRooms(VectorDraw.Render.vdRender render)
        {
            
                if (info.selectRooms != null)
                {
                    foreach (Room room in info.selectRooms)
                    {
                        if ( room != null && room.roomBoundary != null)
                        {

                            vdPolyline polyline = (vdPolyline)room.roomBoundary.Clone(this.vectorDrawBaseControl1.ActiveDocument);
                            polyline.LineWeight = VdConstLineWeight.LW_50;
                            polyline.PenColor.Red = 0; polyline.PenColor.Green = 255; polyline.PenColor.Blue = 0;

                            polyline.Draw(render);

                        }
                    }
                }
            
        }

        public vdDocument getDoc() {

            return this.vectorDrawBaseControl1.ActiveDocument;

        }



        private void vectorDrawBaseControl1_GripSelectionModified(object sender, vdLayout layout, vdSelection gripSelection)
        {
            info.selectRooms.Clear();
            string text = "";
            for (int i = 0; i < gripSelection.Count; i++) {
                if (gripSelection[i] is vdPolyline)
                {
                    vdPolyline poly = (vdPolyline)gripSelection[i];
                    Object ob = poly.XProperties["Room"];
                    if (ob != null)
                    {
                        Room room = (Room)ob;
                        text += room.name + ",";
                        info.selectRooms.Add(room);
                    }
                }
            }
            if (text.Length > 0) text = text.Substring(0, text.Length - 1);
            this.seletionLabel.Text = text;
            this.vectorDrawBaseControl1.Redraw();
        }


        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rw;
            List<vdPolyline> clipopers = new List<vdPolyline>();
            List<vdPolyline> obstacle = new List<vdPolyline>();
            List<Connector> connectorList = new List<Connector>();
            double[,] doorCenterPoints = new double[2, 4];

            rw = this.dataGridView1.SelectedCells[0].RowIndex;

            String rowval = this.dataGridView1.Rows[rw].Cells[3].Value.ToString();
            String[] roomNames = rowval.Split(',');




            List<Room> roomList = new List<Room>();
            string rooms = "";
            for (int iter = 0; iter < roomNames.Length; iter++)
            {
                String spstr1 = roomNames[iter];
                Room room1 = FindUtil.findRoom(info.floor, spstr1);
                roomList.Add(room1);
                rooms += spstr1 + Protocol.Delimiter_Rooms;
            }
            rooms = rooms.Substring(0, rooms.Length - 1);
            gPoints ps = null;
            bool isRoomCenter = false;
            if (this.toolStripComboBox1.SelectedIndex == 0) isRoomCenter = true;


            if (this.algorithComboBox.SelectedIndex == 0) ps = AnalysisShortDistance.getShortDistanceLikeHuman(roomList, this.info, this.vectorDrawBaseControl1.ActiveDocument, isRoomCenter);
            else ps = AnalysisShortDistance.getShortDistanceLikeMachine(roomList, this.info, this.vectorDrawBaseControl1.ActiveDocument);

            rooms += ":" + (Math.Round(ps.Length() / 10)) / 100.0 + "M";
            this.setInformation(rooms);

            info.route = new vdPolyline(this.vectorDrawBaseControl1.ActiveDocument, ps);
            info.route.PenColor = new vdColor(Color.Red);
            info.route.LineWeight = VdConstLineWeight.LW_50;

            info.routes = new List<vdPolyline>();
            this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.Update();
            this.vectorDrawBaseControl1.ActiveDocument.Update();
            this.vectorDrawBaseControl1.ActiveDocument.Redraw(true);
        }

        public void setInformation(string text) {
            seletionLabel.Text = text;
        }

        private void vectorDrawBaseControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) {
                info.route = new vdPolyline();
                info.routes = new List<vdPolyline>();
                info.selectRooms.Clear();
                this.vectorDrawBaseControl1.ActiveDocument.Update();
                this.vectorDrawBaseControl1.ActiveDocument.Redraw(true);
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            info.route = new vdPolyline();
            info.routes = new List<vdPolyline>();
            clearTriagle();
            lstCacheTriangle = new List<vdPolyline>();
            IsErase = true;
            this.vectorDrawBaseControl1.ActiveDocument.Update();
            this.vectorDrawBaseControl1.ActiveDocument.Redraw(true);

        }
        public List<vdPolyline> lstCacheTriangle ;
        private void clearTriagle()
        {
            if (lstCacheTriangle != null)
            foreach (vdPolyline vcache in lstCacheTriangle)
                this.vectorDrawBaseControl1.ActiveDocument.ActiveLayOut.Entities.RemoveItem(vcache);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }


        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void cADFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sd = new SaveFileDialog();
            sd.Filter = "*";
            {
                if (sd.ShowDialog(this) == DialogResult.OK)
                {
                    this.vectorDrawBaseControl1.ActiveDocument.SaveAs(sd.FileName);
                    foreach (SequenceGroup sequenceGroup in this.info.sequenceGroups)
                    {

                        vdLayer layer = this.vectorDrawBaseControl1.ActiveDocument.Layers.Add(sequenceGroup.definedSequence.name);
                        vdPolyline poly = new vdPolyline(this.vectorDrawBaseControl1.ActiveDocument, sequenceGroup.getShortestPath());
                        poly.Layer = layer;
                        this.vectorDrawBaseControl1.ActiveDocument.ActiveLayOut.Entities.AddItem(poly);
                        poly.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                        poly.setDocumentDefaults();
                        poly.PenColor.SystemColor = Color.Red; // Using system color

                    }

                    
                }
            }
        }
        public class RouteTable
        {
            public int Index { get; set; }
            public string SRoom { get; set; }
            public string ERoom { get; set; }
            public string Route { get; set; }
        }

        public class RoomTable
        {
            public Room Room { get; set; }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.watch.Reset();
            watch.Start();
        

            this.info.setGrid();
            //this.disForm.Visible = true;

            /*
            for (int i = 0; i < this.info.mathMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < this.info.mathMatrix.GetLength(1); j++)
                {

                    gPoint gPoint = this.info.getMatrixLocation(i, j);
                    vdCircle vdCircle = new vdCircle((this.vectorDrawBaseControl1.ActiveDocument));
                    vdCircle.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                    vdCircle.setDocumentDefaults();
                    vdCircle.Center = gPoint;
                    vdCircle.Radius = 50;

                    this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(vdCircle);
                    if (this.info.mathMatrix[i, j] == 1)
                    {
                        vdCircle.PenColor.SystemColor = Color.Blue;
                    }
                    else vdCircle.PenColor.SystemColor = Color.Red;
                }
            }
            this.vectorDrawBaseControl1.Redraw();
  

            for (int i = 0; i < this.info.gridMtrix.GetLength(0); i++)
            {
                for (int j = 0; j < this.info.gridMtrix.GetLength(1); j++)
                {

                    gPoint gPoint = this.info.getLocation(i, j);
                    vdCircle vdCircle = new vdCircle((this.vectorDrawBaseControl1.ActiveDocument));
                    vdCircle.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                    vdCircle.setDocumentDefaults();
                    vdCircle.Center = gPoint;
                    vdCircle.Radius = 50;

                    this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(vdCircle);
                    if (this.info.gridMtrix[i, j] == 1)
                    {
                        vdCircle.PenColor.SystemColor = Color.Blue;
                    }
                    else vdCircle.PenColor.SystemColor = Color.Red;
                }
            }
            this.vectorDrawBaseControl1.Redraw();

            return;
              */

            int count = 1;
            //Console.WriteLine("this.info.floor.roomList.Count  "+ (this.info.floor.roomList.Count* this.info.floor.roomList.Count/2));

            for (int i = 0; i < this.info.floor.roomList.Count-1; i++)
            {
                for (int j = i+1; j < this.info.floor.roomList.Count; j++)
                {
                    Room r1 = this.info.floor.roomList[i];
                    Room r2 = this.info.floor.roomList[j];
                    int xx1 = -1;
                    int yy1 = -1;
                    info.getBoolMatrixIndex(r1.centerPoint.x, r1.centerPoint.y, ref xx1, ref yy1);

                    int xx2 = -1;
                    int yy2 = -1;
                    info.getBoolMatrixIndex(r2.centerPoint.x, r2.centerPoint.y, ref xx2, ref yy2);
                    Point sp = new Point(xx1, yy1);
                    Point ep = new Point(xx2, yy2);

                    gPoints gps = this.info.gridDistanceAlgorithm.setInit(this.info.boolMtrix, sp, ep);
                    gPoints gps2 = new gPoints();
                    
                    foreach (gPoint gp in gps)
                    {
                        double x = info.minGpBoundary.x + gp.x * info.gridWidth * info.unionCount / 2.0;
                        double y = info.minGpBoundary.y + gp.y * info.gridHeight * info.unionCount / 2.0;
                        gPoint point = new gPoint(x, y); ;
                        gps2.Add(x, y, 0);

                    }
                   

                    long totalTime = this.watch.ElapsedMilliseconds;
                    string s = String.Format("Time elapsed = {0:n0} ms", this.watch.ElapsedMilliseconds);

                 // if (gps.Length() == 0) Console.WriteLine((count++) +"\t" +r1.name + "\t" + r2.name );
                  //else Console.WriteLine((count++) + " :  " + s + " " + i + "  " + j + "  " + r1.name + "  " + r2.name + "  " + gps.Length());
                  /*
                    vdPolyline path = new vdPolyline(this.vectorDrawBaseControl1.ActiveDocument, gps2);
                    path.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                    path.setDocumentDefaults();
                    this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(path);
                    path.PenColor.SystemColor = Color.Yellow;
                  */
                }
            }

            this.vectorDrawBaseControl1.ActiveDocument.Update();
            this.vectorDrawBaseControl1.Update();
            this.vectorDrawBaseControl1.ActiveDocument.Redraw(true);

         
            string s2 = String.Format("Last Time elapsed = {0:n0} ms", this.watch.ElapsedMilliseconds);
           // Console.WriteLine("last "+  s2);

            watch.Stop();

        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
   
            Room r1 = this.info.floor.roomList[9];
            Room r2 = this.info.floor.roomList[10];
 

            int row = -1;
            int col = -1;
            info.getBoolMatrixIndex(r1.centerPoint.x, r1.centerPoint.y, ref row, ref col);

            int row2 = -1;
            int col2 = -1;
            info.getBoolMatrixIndex(r2.centerPoint.x, r2.centerPoint.y, ref row2, ref col2);

            Point sp = new Point(row, col);
            Point ep = new Point(row2, col2);

           //// gPoints gps =  disForm.getPath(this.info.boolMtrix, sp, ep);
         ////   vdPolyline path = new vdPolyline(this.vectorDrawBaseControl1.ActiveDocument, gps); ;
            ////this.info.route = path;
            this.vectorDrawBaseControl1.Redraw();

           //// Console.WriteLine("path length "+path.Length());


            /*
           foreach (Room r in this.info.floor.roomList)
           {
               foreach (VPoint v in r.vPoints)
               {
                   gPoint gPoint = this.info.getLocation(v.row, v.col);
                   vdCircle vdCircle = new vdCircle((this.vectorDrawBaseControl1.ActiveDocument));
                   vdCircle.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                   vdCircle.setDocumentDefaults();
                   vdCircle.Center = gPoint;
                   vdCircle.Radius = 50;
                   vdCircle.PenColor.SystemColor = Color.Blue;
                   this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(vdCircle);
               }
           }
            */
            
            /*
           foreach (Connector c in this.info.floor.connectorList)
           {
               foreach (VPoint v in c.vPoints)
               {
                   gPoint gPoint = this.info.getLocation(v.row, v.col);
                   vdCircle vdCircle = new vdCircle((this.vectorDrawBaseControl1.ActiveDocument));
                   vdCircle.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                   vdCircle.setDocumentDefaults();
                   vdCircle.Center = gPoint;
                   vdCircle.Radius = 50;
                   vdCircle.PenColor.SystemColor = Color.Red;
                   this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(vdCircle);
               }
           }
            */

/*
           for (int i = 0; i < this.info.gridMtrix.GetLength(0); i++)
           {
               for (int j = 0; j < this.info.gridMtrix.GetLength(1); j++)
               {
                   if (this.info.gridMtrix[i, j] == 0)
                   {
                       gPoint gPoint = this.info.getLocation(i, j);
                       vdCircle vdCircle = new vdCircle((this.vectorDrawBaseControl1.ActiveDocument));
                       vdCircle.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                       vdCircle.setDocumentDefaults();
                       vdCircle.Center = gPoint;
                       vdCircle.Radius = 50;
                       this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(vdCircle);
                       vdCircle.PenColor.SystemColor = Color.Yellow;
                   }

               }
           }

            */

             for (int i = 0; i < this.info.mathMatrix.GetLength(0); i++)
           {
               for (int j = 0; j < this.info.mathMatrix.GetLength(1); j++)
               {

                  gPoint gPoint = this.info.getLocation(i, j);
                   vdCircle vdCircle = new vdCircle((this.vectorDrawBaseControl1.ActiveDocument));
                   vdCircle.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                   vdCircle.setDocumentDefaults();
                    vdCircle.Center = gPoint;
                   vdCircle.Radius = 50;

                   this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(vdCircle);
                   if (this.info.mathMatrix[i, j] == 1)
                   {
                       vdCircle.PenColor.SystemColor = Color.Blue;
                   }
                   else vdCircle.PenColor.SystemColor = Color.Red;
               }
           }
             /*
            foreach (Room room in this.info.floor.roomList)
            {
                double xx = room.centerPoint.x;
                double yy = room.centerPoint.y;

                int row3 = -1;
                int col3 = -1;

                this.info.getMatrixIndex(xx, yy, ref row, ref col);
                Console.WriteLine(xx+"  "+yy+"  "+row+"  "+col);    


                vdCircle circle = new vdCircle((this.vectorDrawBaseControl1.ActiveDocument));
                circle.SetUnRegisterDocument(this.vectorDrawBaseControl1.ActiveDocument);
                circle.setDocumentDefaults();
                circle.Center = new gPoint(xx, yy);
                circle.Radius = 550;

                this.vectorDrawBaseControl1.ActiveDocument.Model.Entities.AddItem(circle);
                circle.PenColor.SystemColor = Color.Yellow;
            }
           */


            this.vectorDrawBaseControl1.Redraw();
        }

        private void splitContainer3_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void vectorDrawBaseControl1_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void vectorDrawBaseControl1_Move(object sender, EventArgs e)
        {

        }

        private void vectorDrawBaseControl1_MouseMove(object sender, MouseEventArgs e)
        {
            //DataGridViewSelectedRowCollection drselect = analysisRouteControl1.sequenceGroupDataGridView.SelectedRows;
            //if (drselect.Count == 0)
            //{
            //    SelectFlg = true;
            //    this.vectorDrawBaseControl1.ActiveDocument.Document.Update();
            //    this.vectorDrawBaseControl1.ActiveDocument.Document.Redraw(true);
            //}

        }

        private void PathFinderForm_Load(object sender, EventArgs e)
        {
            this.vectorDrawBaseControl1.MouseWheel += VectorDrawBaseControl1_MouseWheel;
        }

        private void VectorDrawBaseControl1_MouseWheel(object sender, MouseEventArgs e)
        {
           // DataGridViewSelectedRowCollection drselect = analysisRouteControl1.sequenceGroupDataGridView.SelectedRows;
           //if ( drselect.Count == 0)
           // {
           //     SelectFlg = true;
           //     this.vectorDrawBaseControl1.ActiveDocument.Document.Update();
           //     this.vectorDrawBaseControl1.ActiveDocument.Document.Redraw(true);
           // }

        }
    }

}





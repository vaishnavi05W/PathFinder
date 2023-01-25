namespace PathFinder
{
    using PathFinder.analysis;
    using PathFinder.util;
    using System;
using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
using System.Threading.Tasks;
    using VectorDraw.Geometry;
    using VectorDraw.Professional.vdFigures;
    using PathFinder.gui;
    public class Info
{
        public  DisForm gridDistanceAlgorithm = new DisForm();
        public int unionCount = 3;
        public int gridWidth =  47;
        public int gridHeight = 47;
        public gPoint minGpBoundary = new gPoint(double.MaxValue, double.MaxValue);
        public gPoint maxGpBoundary = new gPoint(double.MinValue, double.MinValue);
        public double[,] gridMtrix = null;
        public double[,] mathMatrix = null;


        public bool[,] boolMtrix = null;
        public static FileInfo fileInfo;
        public  List<RoomGroup> roomGroups = new List<RoomGroup>();
        public List<SequenceGroup> sequenceGroups = new List<SequenceGroup>();
        public List<RoomRelation> roomRelations = new List<RoomRelation>();
        public Floor floor = new Floor();
        public List<MainRoute> mainRoutes = new List<MainRoute>();
        public vdPolyline route = new vdPolyline();
        public List<vdPolyline> routes = new List<vdPolyline>();
        public List<Room> selectRooms = new List<Room>();
        

        public void setsequenceGroup() {
           /*
            
            foreach (SequenceGroup sequenceGroup in sequenceGroups) {

                sequenceGroup.setCombinedSequence(roomRelations,this);
            }
           */

        }

        public void setGrid(List<Room> rooms)
        {


            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (Room room in rooms)
            {

                double miY = room.roomBoundary.BoundingBox.Min.y;
                double miX = room.roomBoundary.BoundingBox.Min.x;

                double maY = room.roomBoundary.BoundingBox.Max.y;
                double maX = room.roomBoundary.BoundingBox.Max.x;

                if (minX > miX) minX = miX;
                if (minY > miY) minY = miY;

                if (maxX < maX) maxX = maX;
                if (maxY < maY) maxY = maY;
            }

            this.minGpBoundary = new gPoint(minX, minY);
            this.maxGpBoundary = new gPoint(maxX, maxY);

            double width = maxX - minX;
            double height = maxY - minY;


            int gridXCount = (int)(width / this.gridWidth);
            int gridYCount = (int)(height / this.gridHeight);


            int remainderX = gridXCount % this.unionCount;
            int remainderY = gridYCount % this.unionCount;

            gridXCount = gridXCount + (this.unionCount - remainderX);
            gridYCount = gridYCount + (this.unionCount - remainderY);

            mathMatrix = new double[gridXCount, gridYCount];



            for (int i = 0; i < mathMatrix.GetLength(0); i++)  // 0  calxxx
            {
                for (int j = 0; j < mathMatrix.GetLength(1); j++)  //rowyyy
                {
                    gPoint gp = new gPoint(minX + i * this.gridWidth, minY + j * this.gridHeight);
                    bool isIn = isConatinRoomAndConntor(gp, i, j, false, rooms);
                    if (isIn) mathMatrix[i, j] = 1;
                    else mathMatrix[i, j] = 0;

                }
            }

            int subXCount = gridXCount / unionCount;
            int subYCount = gridYCount / unionCount;

            gridMtrix = new double[subXCount, subYCount];
            boolMtrix = new bool[subXCount, subYCount];

            int unionCount2 = unionCount * unionCount;
            for (int i = 0; i < gridMtrix.GetLength(0); i++)
            {
                for (int j = 0; j < gridMtrix.GetLength(1); j++)
                {
                    double v = 0;
                    for (int m = 0; m < unionCount; m++)
                    {
                        for (int n = 0; n < unionCount; n++)
                        {
                            v += mathMatrix[unionCount * i + m, unionCount * j + n];
                        }
                    }
                    if (v == unionCount2)
                    {
                        boolMtrix[i, j] = true;
                        gridMtrix[i, j] = 1;
                    }
                    else
                    {
                        boolMtrix[i, j] = false;
                        gridMtrix[i, j] = 0;
                    }
                }
            }

            //
            foreach (Obstacle obstacle in floor.obstacles) obstacle.vPoints.Clear();
            foreach (Connector connector in floor.connectorList) connector.vPoints.Clear();
            foreach (Room room in floor.roomList) room.vPoints.Clear();

            for (int i = 0; i < this.gridMtrix.GetLength(0); i++)
            {
                for (int j = 0; j < this.gridMtrix.GetLength(1); j++)
                {
                    gPoint gPoint = getLocation(i, j);
                    isConatinRoomAndConntor(gPoint, i, j, true, rooms);
                }
            }
        }


        public void setGrid()
        {


            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (Room room in floor.roomList)
            {

                double miY = room.roomBoundary.BoundingBox.Min.y;
                double miX = room.roomBoundary.BoundingBox.Min.x;

                double maY = room.roomBoundary.BoundingBox.Max.y;
                double maX = room.roomBoundary.BoundingBox.Max.x;

                if (minX > miX) minX = miX;
                if (minY > miY) minY = miY;

                if (maxX < maX) maxX = maX;
                if (maxY < maY) maxY = maY;
            }

            this.minGpBoundary = new gPoint(minX, minY);
            this.maxGpBoundary = new gPoint(maxX, maxY);

            double width = maxX - minX;
            double height = maxY - minY;


            int gridXCount = (int)(width / this.gridWidth);
            int gridYCount = (int)(height / this.gridHeight);


            int remainderX = gridXCount % this.unionCount;
            int remainderY = gridYCount % this.unionCount;

            gridXCount = gridXCount + (this.unionCount - remainderX);
            gridYCount = gridYCount + (this.unionCount - remainderY);

            mathMatrix = new double[gridXCount, gridYCount];



            for (int i = 0; i < mathMatrix.GetLength(0); i++)  // 0  calxxx
            {
                for (int j = 0; j < mathMatrix.GetLength(1); j++)  //rowyyy
                {
                    gPoint gp = new gPoint(minX + i * this.gridWidth , minY + j * this.gridHeight );
                    bool isIn = isConatinRoomAndConntor(gp, i, j, false, this.floor.roomList);
                    if (isIn) mathMatrix[i, j] = 1;
                    else mathMatrix[i, j] = 0;

                }
            }

            int subXCount = gridXCount / unionCount;
            int subYCount = gridYCount / unionCount;

            gridMtrix = new double[subXCount  , subYCount];
            boolMtrix = new bool[subXCount, subYCount];

            int unionCount2 = unionCount * unionCount;
            for (int i = 0; i < gridMtrix.GetLength(0); i++)
            {
                for (int j = 0; j < gridMtrix.GetLength(1); j++)
                {
                    double v = 0;
                    for (int m = 0; m < unionCount; m++)
                    {
                        for (int n = 0; n < unionCount; n++)
                        {
                            v += mathMatrix[unionCount * i + m, unionCount * j + n];
                        }
                    }
                    if (v == unionCount2)
                    {
                        boolMtrix[i, j] = true;
                        gridMtrix[i, j] = 1;
                    }
                    else
                    {
                        boolMtrix[i, j] = false;
                        gridMtrix[i, j] = 0;
                    }
                }
            }

            //
            foreach (Obstacle obstacle in floor.obstacles) obstacle.vPoints.Clear();
            foreach (Connector connector in floor.connectorList) connector.vPoints.Clear();
            foreach (Room room in floor.roomList) room.vPoints.Clear();

            for (int i = 0; i < this.gridMtrix.GetLength(0); i++)
            {
                for (int j = 0; j < this.gridMtrix.GetLength(1); j++)
                {
                    gPoint gPoint = getLocation(i, j);
                    isConatinRoomAndConntor(gPoint, i, j, true, this.floor.roomList);
                }
            }
        }

        public void getMatrixIndex(double x, double y, ref int xIndex, ref int yIndex)
        {
            xIndex = (int)((x - this.minGpBoundary.x) / (gridWidth ));
            yIndex = (int)((y - this.minGpBoundary.y) / (gridHeight ));
        }

        public void getBoolMatrixIndex(double x, double y, ref int xIndex, ref int yIndex)
        {
            xIndex = (int)((x - this.minGpBoundary.x) / (gridWidth * unionCount));
            yIndex = (int)((y - this.minGpBoundary.y) / (gridHeight * unionCount));
        }


        public gPoint getLocation(int xx, int yy)
        {
            double x = this.minGpBoundary.x + xx * gridWidth * unionCount;
            double y = this.minGpBoundary.y + yy* gridHeight * unionCount;

            gPoint gPoint = new gPoint(x, y); 
            return gPoint;

        }
        public gPoint getMatrixLocation(int xx, int yy)
        {
            double x = this.minGpBoundary.x + xx * gridWidth ;
            double y = this.minGpBoundary.y + yy * gridHeight ;

            gPoint gPoint = new gPoint(x, y);
            return gPoint;

        }

        public bool isConatinRoomAndConntor(gPoint gp, int x, int y, bool isAdd, List<Room> rooms)
        {
            foreach (Obstacle obstacle in floor.obstacles)
            {
                bool isIn = CadUtil.contains(obstacle.shape.VertexList, gp);
                if (isIn)
                {
                    if (isAdd)
                    {
                        VPoint v = new VPoint(x, y);
                        obstacle.vPoints.Add(v);
                    }
                    return false;
                }

            }

            foreach (Connector connector in floor.connectorList)
            {
                 bool isInBoundary = false;
                if (isAdd) isInBoundary = CadUtil.contains(connector.outsideShape.VertexList, gp);
                else  isInBoundary = CadUtil.contains(connector.shape.VertexList, gp);
                if (isInBoundary)
                {
                    if (isAdd)
                    {
                        VPoint v = new VPoint(x, y);
                        connector.vPoints.Add(v);
                    }
                    return true;
                }
            }

            foreach (Room room in rooms)
            {
                if (room.inShapeList.Count == 0)
                {
                    bool isInBoundary = CadUtil.contains(room.roomBoundary.VertexList, gp);
                    if (isInBoundary)
                    {
                        if (isAdd)
                        {
                            VPoint v = new VPoint(x, y);
                            room.vPoints.Add(v);
                        }
                        return true;
                    }
                }
            }

            foreach (Room room in rooms)
            {
                if (room.shapeList.Count > 0)
                {
                    bool isInBoundary = CadUtil.contains(room.roomBoundary.VertexList, gp);
                    if (isInBoundary)
                    {
                        foreach (vdPolyline poly in room.inShapeList)
                        {
                            bool isInObstacle = CadUtil.contains(poly.VertexList, gp);
                            if (isInObstacle) return false;
                        }
                        if (isInBoundary)
                        {
                            if (isAdd)
                            {
                                VPoint v = new VPoint(x, y);
                                room.vPoints.Add(v);
                            }
                            return true;
                        }
                    }
                }
            }

            return false;
        }


    }


}

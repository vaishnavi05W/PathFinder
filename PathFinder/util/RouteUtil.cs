namespace PathFinder.util
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using VectorDraw.Professional.vdObjects;

    internal class RouteUtil
    {
        public static List<Room> getRouteAllRoomList(Info info, DataGridView dataGridView) {

            List<Room> roomList = new List<Room>();
            for (int i = 0; i < info.sequenceGroups.Count; i++)
            {
                bool isSelected = (bool)dataGridView.Rows[i].Cells[1].Value;
                if (!isSelected) continue;
                SequenceGroup sequenceGroup = (SequenceGroup)info.sequenceGroups[i];
                foreach (RoomAndGroupObject ro in sequenceGroup.definedSequence.roomList)
                    if (ro is Room)
                    {
                        if (!roomList.Contains(ro))  roomList.Add((Room)ro);

                    }
                    else if (ro is RoomGroup)
                    {
                        RoomGroup rg = (RoomGroup)ro;
                        RouteUtil.getRoomsInRoomGroup(rg, info, roomList);
                    }
            }

            List<Room> rList = new List<Room>();
            for (int i = 0; i < roomList.Count - 1; i++)
            {
                Room r1 = roomList[i];
                if (!roomList.Contains(r1)) roomList.Add(r1);
                for (int j = i + 1; j < roomList.Count; j++)
                {
                    Room r2 = roomList[j];
                    if (!rList.Contains(r2)) rList.Add(r2);

                    foreach (RoomRelation rr in info.roomRelations)
                    {
                        if (rr.sRoom == r1 && rr.eRoom == r2)
                        {
                            foreach (ArrayList list in rr.roomLists)
                            {
                                foreach (Room r in list)
                                {
                                    if (!rList.Contains(r)) rList.Add(r);
                                }
                            }
                            break;
                        }
                    }
                }
            }

            return rList;
        }
      
        public static void getRoomsInRoomGroup(RoomGroup rg, Info info, List<Room> roomList)
        {
            foreach (Room r in rg.roomList) {
               if(!roomList.Contains(r)) roomList.Add(r);
            }
        }
        
    }
}

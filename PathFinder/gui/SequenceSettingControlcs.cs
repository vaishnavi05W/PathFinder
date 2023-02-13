namespace PathFinder.gui
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using PathFinder.util;
    using static System.Net.Mime.MediaTypeNames;

    public delegate void SequenceGroupChangedEventHandler(object sender, EventArgs e);
    public delegate void SequenceNameChangedEventHandler(object sender, EventArgs e);
    public delegate void SequenceSelectionChangedEventHanlder(object sender, EventArgs e);
    public delegate void SequenceHighlightChangedEventHandler(object sender, List<Room> rooms);

    public partial class SequenceSettingControlcs : UserControl
    {

        Info info;
        //        SequenceSettingForm ssf = new SequenceSettingForm();
        public event SequenceGroupChangedEventHandler sequenceGroupChangedReceiveMsg;
        public event SequenceNameChangedEventHandler sequenceNameChangedReceiveMsg;
        public event SequenceSelectionChangedEventHanlder sequenceSelectionChangedEventHanlder;
        public event SequenceHighlightChangedEventHandler sequenceHighlightChangedReceiveMsg;
        public SequenceSettingControlcs()
        {
            InitializeComponent();
        }

        public void setRoomList(List<Room> roomList)
        {

            foreach (Room r in roomList)
            {
                roomListDataGridView.Rows.Add(new object[] { r });
            }



            // roomListDataGridView.Columns["Room"].SortMode = DataGridViewColumnSortMode.Automatic;
        }

        public void setRoomGroups(Info info)
        {
            this.info = info;
            addRoomGroup(info);

        }
        public void addRoomGroup(Info info)
        {
            this.info = info;
            roomGroupDataGridView1.Rows.Clear();

            foreach (RoomGroup rg in info.roomGroups)
            {
                object[] obs = new object[] { rg.name, rg.getOrder(), rg.getRooms() };
                roomGroupDataGridView1.Rows.Add(obs);
            }
        }
        private void deleteSequnce(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection rows = this.sequenceSettingdataGridView.SelectedRows;
            foreach (DataGridViewRow row in rows)
            {
                this.info.sequenceGroups.Remove((SequenceGroup)row.Cells[0].Value);
                this.sequenceSettingdataGridView.Rows.Remove(row);
            }
            sequenceGroupChangedReceiveMsg(this, e);
        }

        private void AddRoomsToSequence_OnClick(object sender, EventArgs e)
        {

            if (roomListDataGridView.SelectedRows.Count > 0)
            {
                DataGridViewSelectedRowCollection str = this.roomListDataGridView.SelectedRows;
                foreach (DataGridViewRow s in str)
                {
                    Room nm = (Room)s.Cells[0].Value;
                    bool roomListContain = this.listBox1.Items.Contains(nm);
                    if (roomListContain)
                    {
                        DialogResult dialogResult = MessageBox.Show("중복된 룸이 선택됐습니다. 중복된 값을 추가하시겠습니까?", "중복된 값 메시지",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                        if (dialogResult == DialogResult.OK)
                        {
                            MessageBox.Show("확인", "중복된 값 확인 메시지",
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                        }

                        else if (dialogResult == DialogResult.Cancel)
                        {
                            MessageBox.Show("확인", "중복된 값 취소 메시지",
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                            break;

                        }


                    }
                    this.listBox1.Items.Add(nm);
                }
            }

            //room group
            if (this.roomGroupDataGridView1.SelectedRows.Count > 0)
            {


                if (roomGroupDataGridView1.SelectedRows.Count > 0)
                {
                    int rowIndex = roomGroupDataGridView1.SelectedRows[0].Index;

                    string name = (string)roomGroupDataGridView1.SelectedRows[0].Cells[0].Value;
                    RoomGroup rg = info.roomGroups[rowIndex];

                    rg.name = name;
                    this.listBox1.Items.Add(rg);

                }
            }
        }






        private void MoveRoomUpInSeq(object sender, EventArgs e)
        {

            int selectedIndex = this.listBox1.SelectedIndex;
            if (selectedIndex > 0)
            {
                this.listBox1.Items.Insert(selectedIndex - 1, this.listBox1.Items[selectedIndex]);
                this.listBox1.Items.RemoveAt(selectedIndex + 1);
                this.listBox1.SelectedIndex = selectedIndex - 1;
            }
        }

        private void MoveRoomDownInSeq(object sender, EventArgs e)
        {
            int selectedIndex = this.listBox1.SelectedIndex;
            if (selectedIndex < this.listBox1.Items.Count - 1 & selectedIndex != -1)
            {
                this.listBox1.Items.Insert(selectedIndex + 2, this.listBox1.Items[selectedIndex]);
                this.listBox1.Items.RemoveAt(selectedIndex);
                this.listBox1.SelectedIndex = selectedIndex + 1;

            }
        }

        private void RemoveRoomsFromSequence_OnClick(object sender, EventArgs e)
        {
            //if (this.listBox1.SelectedItems.Count > 0) this.listBox1.Items.Remove(this.listBox1.SelectedItems[0]);
            for (int x = this.listBox1.SelectedItems.Count - 1; x >= 0; x--)
            {
                RoomAndGroupObject ob = (RoomAndGroupObject)this.listBox1.SelectedItems[x];
                listBox1.Items.Remove(ob);
            }

        }






        private void roomGroupDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            roomListDataGridView.ClearSelection();
        }

        private void roomListDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            roomGroupDataGridView1.ClearSelection();
        }

        public void setSequenceGroups(List<SequenceGroup> sequenceGroups)
        {
            this.sequenceSettingdataGridView.Rows.Clear();
            this.info.sequenceGroups = sequenceGroups;
            foreach (SequenceGroup sg in this.info.sequenceGroups)
            {
                this.sequenceSettingdataGridView.Rows.Add(new object[] { sg.definedSequence.name, sg.definedSequence.frequency, sg.definedSequence.getNames() });
                string text = "";
                foreach (RoomAndGroupObject rgo in sg.definedSequence.roomList)
                {
                    text += rgo.name + Protocol.Delimiter_Rooms;
                }
            }
        }


        private void SequenceSettingList_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            int iCellNum = this.sequenceSettingdataGridView.CurrentCell.ColumnIndex;
            int iRetCellNum = this.sequenceSettingdataGridView.Columns["Frequency"].Index;

            if (iCellNum == iRetCellNum)
            {
                e.Control.KeyPress += new KeyPressEventHandler(IsNumericCheck);
            }
        }

        private void IsNumericCheck(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar)
                && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void roomListDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            info.selectRooms.Clear();
            for (int i = 0; i < this.roomListDataGridView.SelectedRows.Count; i++)
            {

                Room r = (Room)roomListDataGridView.SelectedRows[i].Cells[0].Value;

                info.selectRooms.Add(r);


            }
            sequenceHighlightChangedReceiveMsg(sender, info.selectRooms);
        }

        private void sequenceSettingdataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = e.RowIndex;
            if (index < 0) return;
            SequenceGroup sg = (SequenceGroup)this.sequenceSettingdataGridView.Rows[index].Cells[0].Value;


        }

        private void roomListDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void sequenceSettingdataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void sequenceSettingdataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;


            string name = (string)this.sequenceSettingdataGridView.Rows[e.RowIndex].Cells[0].Value;
            SequenceGroup sg = info.sequenceGroups[e.RowIndex];
            sg.definedSequence.name = name;
            try
            {
                int fr = int.Parse(this.sequenceSettingdataGridView.Rows[e.RowIndex].Cells[1].Value.ToString());

                sg.definedSequence.frequency = fr;

            }
            catch (FormatException ex)
            {

                MessageBox.Show("");

            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in roomListDataGridView.Rows)
            {
                int n = roomGroupDataGridView1.Rows.Add();
                foreach (DataGridViewColumn col in roomGroupDataGridView1.Columns)
                {
                    roomGroupDataGridView1.Rows[n].Cells[col.Index].Value = roomGroupDataGridView1.Rows[row.Index].Cells[col.Index].Value.ToString();
                }
            }
        }

        private void toolStrip3_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }







        private void toolStripButton7_Click(object sender, EventArgs e)
        {

            sequenceNameChangedReceiveMsg(this, e);

            sequenceSettingdataGridView.EndEdit();
            ReadWriteUtil.saveSequence(this.info.sequenceGroups);
            sequenceGroupChangedReceiveMsg(this, e);
        }





        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection rows = this.sequenceSettingdataGridView.SelectedRows;
            foreach (DataGridViewRow row in rows)
            {
                this.info.sequenceGroups.RemoveAll(d => d.definedSequence.getNames() == row.Cells[2].Value.ToString());
                this.sequenceSettingdataGridView.Rows.Remove(row);
            }
            sequenceGroupChangedReceiveMsg(this, e);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (this.listBox1.Items.Count == 0) return;
            SequenceGroup sg = new SequenceGroup();
            sg.definedSequence = new DefinedSequence();
            sg.definedSequence.name = "DefinedSequence" + this.info.sequenceGroups.Count;
            foreach (var item in this.listBox1.Items) sg.definedSequence.roomList.Add((RoomAndGroupObject)item);
            foreach (var ds in info.sequenceGroups) if (ds.definedSequence.getNames() == sg.definedSequence.getNames()) return;
            this.sequenceSettingdataGridView.Rows.Add(new object[] { sg, sg.definedSequence.frequency, sg.definedSequence.getNames() });
            info.sequenceGroups.Add(sg);
            sequenceGroupChangedReceiveMsg(this, e);
        }

        private void sequenceSettingdataGridView_CellValueChanged_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;
            Console.WriteLine(e.RowIndex + " " + e.ColumnIndex);

            string name = (string)this.sequenceSettingdataGridView.Rows[e.RowIndex].Cells[0].Value;
            SequenceGroup sg = info.sequenceGroups[e.RowIndex];
            sg.definedSequence.name = name;
            try
            {
                int fr = int.Parse(this.sequenceSettingdataGridView.Rows[e.RowIndex].Cells[1].Value.ToString());

                sg.definedSequence.frequency = fr;

            }
            catch (FormatException ex)
            {

                MessageBox.Show("");

            }
        }
        private List<SequenceGroup> SGTemp;
        private void sequenceSettingdataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
        }

      

        private void sequenceSettingdataGridView_CellStateChanged(object sender, DataGridViewCellStateChangedEventArgs e)
        {

        }

        private void sequenceSettingdataGridView_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.StateChanged != DataGridViewElementStates.Selected) return;
            if (sequenceSettingdataGridView.SelectedRows != null && sequenceSettingdataGridView.SelectedRows.Count == 1)
            {
                tsb_edit.Visible = true;
            }
            else
                tsb_edit.Visible = false;
        }

        private void SequenceSettingControlcs_Load(object sender, EventArgs e)
        {
            tsb_edit.Visible = false;
            sequenceSettingdataGridView.ClearSelection();
        }
        private bool isEditting()
        {
            foreach (DataGridViewRow dgr in sequenceSettingdataGridView.Rows)
            {
                if (dgr.DefaultCellStyle.BackColor == Color.YellowGreen)
                {
                    return true;
                }

            }
            return false;
        }
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (sequenceSettingdataGridView.SelectedRows != null && sequenceSettingdataGridView.SelectedRows.Count == 1)
            {
                if (isEditting())
                {
                    if (this.listBox1.Items.Count == 0)
                    {
                        MessageBox.Show("선택된 룸이 없습니다.");
                        return;
                    }
                    foreach (DataGridViewRow dgr in sequenceSettingdataGridView.Rows)
                    {

                        if (dgr.DefaultCellStyle.BackColor == Color.YellowGreen)
                        {

                            //List<Room> rooms = new List<Room>();
                            //foreach (Room r in this.listBox1.Items) rooms.Add(r);
                            //var activeRG = dgr.Cells[0].Value.ToString();

                            if (this.listBox1.Items.Count == 0) return;
                            SequenceGroup sg = new SequenceGroup();
                            sg.definedSequence = new DefinedSequence();
                            sg.definedSequence.name = dgr.Cells[0].Value.ToString() ;
                            sg.definedSequence.frequency = Convert.ToInt32((dgr.Cells[1].Value.ToString()));
                            foreach (var item in this.listBox1.Items) 
                                sg.definedSequence.roomList.Add((RoomAndGroupObject)item);
                            foreach (var ds in info.sequenceGroups)
                            {
                                if (ds.definedSequence.name != sg.definedSequence.name)
                                {
                                    if (ds.definedSequence.getNames() == sg.definedSequence.getNames())
                                    {
                                        MessageBox.Show("이 시퀀스 목록은 이미 포함되어 있습니다.");
                                        return;
                                    }
                                } 
                            } 
                           
                            foreach (var ds in info.sequenceGroups)
                            {
                                if (ds.definedSequence.name == sg.definedSequence.name)
                                {
                                    ds.definedSequence.frequency = sg.definedSequence.frequency;  
                                    ds.definedSequence.roomList = sg.definedSequence.roomList;
                                }
                            }
                            dgr.Cells[2].Value = sg.definedSequence.getNames();
                            sequenceGroupChangedReceiveMsg(this, e); 
                            this.listBox1.Items.Clear();
                          
                        }
                        dgr.DefaultCellStyle.BackColor = Color.White;
                        dgr.DefaultCellStyle.SelectionBackColor = sequenceSettingdataGridView.RowsDefaultCellStyle.SelectionBackColor;
                    }

                    OtherActionState(true);
                    tsb_edit.Image = Properties.Resources.EditStart;
                }
                else
                {
                    listBox1.Items.Clear();
                    var lst = sequenceSettingdataGridView.SelectedRows[0].Cells[2].Value.ToString().GetSplitList();
                    foreach (var data in GetObject(lst))
                    {
                        listBox1.Items.Add(data);
                    }
                    listBox1.Update();



                    foreach (DataGridViewRow dgr in sequenceSettingdataGridView.Rows)
                    {
                        if (dgr == this.sequenceSettingdataGridView.CurrentRow)
                        {
                            this.sequenceSettingdataGridView.SelectedRows[0].DefaultCellStyle.BackColor = this.sequenceSettingdataGridView.SelectedRows[0].DefaultCellStyle.SelectionBackColor = Color.YellowGreen;
                        
                        }
                        else
                        {
                            dgr.DefaultCellStyle.BackColor = Color.White;
                            dgr.DefaultCellStyle.SelectionBackColor = sequenceSettingdataGridView.RowsDefaultCellStyle.SelectionBackColor;
                        }
                    }
                    OtherActionState(false);
                    tsb_edit.Image = Properties.Resources.Editting;
                }
            }
        }
        private List<object> GetObject(List<string> lst)
        {
            List<object> res = new List<object>();
            if (lst != null)
            {

                if (roomListDataGridView.Rows.Count > 0)
                {
                    foreach (var l in lst)
                    {
                        bool isContainRoom = false;
                        foreach (DataGridViewRow dgr in roomListDataGridView.Rows)
                        {
                            if (dgr.Cells[0].Value != null && l == dgr.Cells[0].Value.ToString())
                            {
                                res.Add(dgr.Cells[0].Value);
                                isContainRoom= true;
                                break;
                            }
                        }
                        if (!isContainRoom)
                        {
                            foreach (DataGridViewRow dgr in roomGroupDataGridView1.Rows)
                            {
                                if (dgr.Cells[0].Value != null && l == dgr.Cells[0].Value.ToString())
                                {  
                                    RoomGroup rg = info.roomGroups[dgr.Index]; 
                                    rg.name = l;
                                    res.Add(rg); 
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        private void OtherActionState(bool enable)
        {
            toolStripButton5.Enabled = toolStripButton6.Enabled = toolStripButton7.Enabled = enable;
            foreach (DataGridViewRow dgr in sequenceSettingdataGridView.Rows)
                (dgr.Cells[0] as DataGridViewTextBoxCell).ReadOnly = (dgr.Cells[1] as DataGridViewTextBoxCell).ReadOnly = !enable;
        }
    }
}

using DLCounting;
using Ini;
using MetroFramework.Forms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace VisionCounting
{
    public partial class FrmProduct : MetroForm
    {

        #region  NOCLOSE_BUTTON
        private const int CP_NOCLOSE_BUTTON = 0x200;
        private const int WS_SYSMENU = 0x80000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style &= ~WS_SYSMENU;
                return cp;
            }
        }
        #endregion


        public string connectionString = "";
        public string TableProduct = "product";



        public string BarcodeIDSelect = "";
        public string Data = "";
        public string BarCodeData = "";


        OpenFileDialog openimage = new OpenFileDialog();
        string imagename = "";

        string aPath = Application.StartupPath + "\\config.ini";
        IniFile ini;

        public Frmmain ParentForm { get; set; }

        public FrmProduct()
        {
            InitializeComponent();

             ini = new IniFile(aPath);
        }

        private void FrmProduct_Load(object sender, EventArgs e)
        {
            connectionString = ini.IniReadValue("Database", "connectionstring", "Data Source=(local)\\SQLEXPRESS;Initial Catalog=visioncounter;Integrated Security=True");
            bool ret = InitialMSSQL();

            if (ret == true)
            {
                UpdateProductTable();
            }
        }


        public void InitialProductTable()
        {

            ProductTable.Columns.Clear();

            ProductTable.ColumnCount = 7;

        
            ProductTable.Columns[0].HeaderText = "ลำดับ";
            ProductTable.Columns[1].HeaderText = "รหัสสินค้า";
            ProductTable.Columns[2].HeaderText = "ชื่อสินค้า(ไทย)";
            ProductTable.Columns[3].HeaderText = "รหัสบาร์โค้ด";
            ProductTable.Columns[4].HeaderText = "ขนาดล่อง";
            ProductTable.Columns[5].HeaderText = "สีชิ้นงาน";
            ProductTable.Columns[6].HeaderText = "จำนวนสินค้า";


            ProductTable.Columns[0].Width = 80;
            ProductTable.Columns[1].Width = 100;
            ProductTable.Columns[2].Width = 150;
            ProductTable.Columns[3].Width = 150;
            ProductTable.Columns[4].Width = 100;
            ProductTable.Columns[5].Width = 100;
            ProductTable.Columns[6].Width = 100;


            ProductTable.AllowUserToResizeColumns = false;

            foreach (DataGridViewColumn col in ProductTable.Columns)
            {
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.HeaderCell.Style.Font = new Font("Angsana New", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
                col.Resizable = DataGridViewTriState.False;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            this.ProductTable.DefaultCellStyle.Font = new Font("Angsana New", 14F, GraphicsUnit.Pixel);

            this.ProductTable.DefaultCellStyle.ForeColor = Color.Blue;
            this.ProductTable.DefaultCellStyle.BackColor = Color.Beige;
            this.ProductTable.DefaultCellStyle.SelectionForeColor = Color.Yellow;
            this.ProductTable.DefaultCellStyle.SelectionBackColor = Color.BlueViolet;
            this.ProductTable.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;


            ProductTable.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ProductTable.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        }

        public void UpdateProductTable()
        {

            string WhereCondition = "";
            string FieldSql = "id,product_name_id,product_display_name,product_barcode,product_size,product_color,count_taget";

            String sql = "";

            sql = "Select " + FieldSql + " From " + TableProduct + " " + WhereCondition;// +" order by MC asc";// +order;//desc";//asc

            InitialProductTable();


            ProductList.Items.Clear();
          

            int count = 0;

            SqlConnection sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();

            SqlDataAdapter da = new SqlDataAdapter(sql, sqlConn);
            DataSet ds = new DataSet();

            DataTable dt = new DataTable();
            da.Fill(ds, TableProduct);


            dt = ds.Tables[TableProduct];
            if (ds.Tables[TableProduct].Rows.Count <= 0)
            {

            }
            else
            {

                // lbRowCount.Text = ds.Tables[TableINVENTITEMBARCODE].Rows.Count.ToString();

                for (int i = 0; i < ds.Tables[TableProduct].Rows.Count; i++)
                {

                    //  id,product_id,product_name,product_barcode,position_size,position_partition,count_taget
                    string id = dt.Rows[i]["id"].ToString();
                    string product_id = dt.Rows[i]["product_name_id"].ToString();
                    string product_name = dt.Rows[i]["product_display_name"].ToString();
                    string product_barcode = dt.Rows[i]["product_barcode"].ToString();
                    string position_size = dt.Rows[i]["product_size"].ToString();
                    string product_color = dt.Rows[i]["product_color"].ToString();
                    string count_taget = dt.Rows[i]["count_taget"].ToString();


                    string[] row = new string[] { "", "", "", "", "", "", "" };


                    ProductList.Items.Add(product_id);

                    if(product_color=="1")
                    {
                        product_color = "Blue";
                    }
                    else if (product_color == "2")
                    {
                        product_color = "White";
                    }
                    else if (product_color == "3")
                    {
                        product_color = "Yellow";
                    }
                    else if (product_color == "4")
                    {
                        product_color = "Gray";
                    }
                    row = new string[] { id, product_id, product_name, product_barcode, position_size, product_color, count_taget };

                    ProductTable.Rows.Add(row);

                    count++;
                }



            }


            sqlConn.Close();
            sqlConn = null;




        }
        public bool InitialMSSQL()
        {
            bool Status = false;


            string sql;

            // sql = ("SELECT * FROM " + TableName + " WHERE (tray_id LIKE \'%" + (txtTrayIDLocation.Text + "%\')"));
            sql = ("SELECT id FROM " + TableProduct + "");// WHERE (tray_id LIKE \'%" + (txtTrayIDLocation.Text + "%\')"));

            SqlConnection sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();

            try
            {

                SqlDataAdapter da = new SqlDataAdapter(sql, sqlConn);
                DataSet ds = new DataSet();

                DataTable dt = new DataTable();
                da.Fill(ds, TableProduct);


                dt = ds.Tables[TableProduct];
                if (ds.Tables[TableProduct].Rows.Count >= 0)
                {
                    Status = true;
                }

                sqlConn.Close();
                sqlConn = null;
            }
            catch
            {
                Status = false;
                sqlConn.Close();
                sqlConn = null;

            }

            return Status;
        }


        private void CkTopMost_CheckedChanged(object sender, EventArgs e)
        {
            if (ckTopMost.Checked == true)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
        }

        private void BtDone_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void BtMoveUp_Click(object sender, EventArgs e)
        {
            ParentForm.frmDigitalIO.MoveTG("-" + txtTragetPos.Text.Trim(), ParentForm.frmDigitalIO.txtTragetSpeed.Text, ParentForm.frmDigitalIO.txtTragetAcc.Text, true, 1);
        }

        private void BtMoveDown_Click(object sender, EventArgs e)
        {
            ParentForm.frmDigitalIO.MoveTG( txtTragetPos.Text.Trim(), ParentForm.frmDigitalIO.txtTragetSpeed.Text, ParentForm.frmDigitalIO.txtTragetAcc.Text, true, 1);
        }

        private void BtTeachGatePosition_Click(object sender, EventArgs e)
        {
            if (ParentForm == null)
            {
                return;
            }

            if (SelectPos1.Checked)
            {
                txtProductPos.Text = ParentForm.frmDigitalIO.txtCurrentPosition.Text;
            }
            else if (SelectPos2.Checked)
            {
                txtProductPosClose.Text = ParentForm.frmDigitalIO.txtCurrentPosition.Text;
            }
           
        }

        private void BtMoveGatePos_Click(object sender, EventArgs e)
        {

            if (SelectPos1.Checked)
            {
                
                ParentForm.frmDigitalIO.MoveTG(txtProductPos.Text.Trim(), ParentForm.frmDigitalIO.txtTragetSpeed.Text, ParentForm.frmDigitalIO.txtTragetAcc.Text, false, 1);
            }
            else if (SelectPos2.Checked)
            {
                
                ParentForm.frmDigitalIO.MoveTG(txtProductPosClose.Text.Trim(), ParentForm.frmDigitalIO.txtTragetSpeed.Text, ParentForm.frmDigitalIO.txtTragetAcc.Text, false, 1);
            }
          
        }


        public bool FindProductIDExist(string ProductID)
        {
            bool ret = false;

            string WhereCondition = "";
            string FieldSql = "*";// "id,product_id";

            String sql = "";
            WhereCondition = "WHERE (product_name_id = '" + ProductID + "')";

            sql = "Select " + FieldSql + " From " + TableProduct + " " + WhereCondition;// " order by SALESID asc";// +order;//desc";//asc



            int count = 0;

            SqlConnection sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();

            SqlDataAdapter da = new SqlDataAdapter(sql, sqlConn);
            DataSet ds = new DataSet();

            DataTable dt = new DataTable();
            da.Fill(ds, TableProduct);


            dt = ds.Tables[TableProduct];
            if (ds.Tables[TableProduct].Rows.Count <= 0)
            {

            }
            else
            {



                if (ds.Tables[TableProduct].Rows.Count > 0)
                {
                    ret = true;
                }




            }


            sqlConn.Close();
            sqlConn = null;


            return ret;

        }

        private void btNewProduct_Click(object sender, EventArgs e)
        {
            /****** Script for SelectTopNRows command from SSMS  *****
            SELECT TOP 1000[id]
      ,[product_id]
      ,[product_name_id]
      ,[product_display_name]
      ,[product_barcode]
      ,[product_size]
      ,[product_color]
      ,[position_partition]
      ,[count_taget]
      ,[product_alarm_enable]
      ,[product_alarm_value]
      ,[picproduct]
      ,[enable_dl]
      ,[dl_name_type]
      ,[dl_confidence_accept]
      ,[dl_count_accept]
      ,[object_least_distance]
      ,[object_min_area]
      ,[object_max_area]
      ,[object_area_case1]
      ,[object_area_case2]
      ,[object_area_case3]
      ,[object_area_case4]
      ,[object_area_case5]
      ,[object_area_case6]
      ,[object_area_case7]
      ,[object_area_case8]
      ,[object_area_case9]
      ,[object_area_case10]
      ,[datetime_update]
        FROM[visioncounter].[dbo].[product]
        */
            string[] data = new string[30];


            data[0] = ProductList.Text;
            data[1] = txtProductFullName.Text;
            data[2] = txtBarcodeID.Text;
        
            if (rdBoxSizeM.Checked == true)
            {
                data[3] = "1";
            }
            else
            {
                data[3] = "0";
            }

            if (radioColorTypeBlue.Checked == true)
            {
                data[4] = "1";
            }
            else if (radioColorTypeWhite.Checked == true)
            {
                data[4] = "2";
            }
            else if(radioColorTypeYellow.Checked == true)
            {
                data[4] = "3";
            }
            else if(radioColorTypeGray.Checked == true)
            {
                data[4] = "4";
            }


            data[5] = txtProductPos.Text.Trim();

            data[6] = numTarget.Value.ToString();

            if (ckReduceSpeed.Checked == true)
            {
                data[7] = "1";
            }
            else
            {
                data[7] = "0";
            }

            data[8] = numReduceSpeed.Value.ToString();

            if (ckDLProcess.Checked == true)
            {
                data[9] = "1";
            }
            else
            {
                data[9] = "0";
            }
            
            data[10] = cbObjectDL.Text.Trim();
            data[11] = numMinDLConfidance.Value.ToString();
            data[12] = numDLCountAccept.Value.ToString();


           



            data[13] = numLeastDistance.Value.ToString();
            data[14] = numRectAreaAceptMin.Value.ToString();
            data[15] = numRectAreaAceptMax.Value.ToString();
            data[16] = numObCase1.Value.ToString();
            data[17] = numObCase2.Value.ToString();
            data[18] = numObCase3.Value.ToString();
            data[19] = numObCase4.Value.ToString();
            data[20] = numObCase5.Value.ToString();
            data[21] = numObCase6.Value.ToString();
            data[22] = numObCase7.Value.ToString();
            data[23] = numObCase8.Value.ToString();
            data[24] = numObCase9.Value.ToString();
            data[25] = numObCase10.Value.ToString();


            data[26] = txtProductPosClose.Text.Trim(); //Update 10/10/2019

            if (FindProductIDExist(ProductList.Text))
            {
                MessageBox.Show("มีข้อมูลรหัสผู้ผลิตเดิมอยู่แล้ว!!", "ผลการตรวจชอบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ProductList.Text = "";
                return;
            }

            if ((ProductList.Text != "") && (txtProductFullName.Text != "") && (txtBarcodeID.Text != ""))
            {
                if (MessageBox.Show("กรุณายืนยันที่จะเพิ่มข้อมูล ?", "ยืนยันการเพื่มข้อมูล", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    AddProduct(data);

                }
            }
            else
            {
                MessageBox.Show("กรุณากรอกรายละเอียดที่สำคัญให้ครบ!!", "ผลการตรวจชอบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }

        public bool AddProduct(string[] data)
        {
            bool ret = false;
            string Sql;

   /*
      //,[product_id]
      ,[product_name_id]
      ,[product_display_name]
      ,[product_barcode]
      ,[product_size]
      ,[product_color]
      ,[position_partition]
      ,[count_taget]
      ,[product_alarm_enable]
      ,[product_alarm_value]
      ,[picproduct]
      ,[enable_dl]
      ,[dl_name_type]
      ,[dl_confidence_accept]
      ,[dl_count_accept]
      ,[object_least_distance]
      ,[object_min_area]
      ,[object_max_area]
      ,[object_area_case1]
      ,[object_area_case2]
      ,[object_area_case3]
      ,[object_area_case4]
      ,[object_area_case5]
      ,[object_area_case6]
      ,[object_area_case7]
      ,[object_area_case8]
      ,[object_area_case9]
      ,[object_area_case10]
      ,[datetime_update]
      */
            if (imagename != "")
            {
                Sql = "INSERT INTO " + TableProduct + "(product_name_id,product_display_name,product_barcode,product_size,product_color,position_partition,position_partition_end,count_taget,product_alarm_enable,product_alarm_value";
                Sql = Sql + ",picproduct,enable_dl,dl_name_type,dl_confidence_accept,dl_count_accept";
                Sql = Sql + ",object_least_distance,object_min_area,object_max_area,object_area_case1,object_area_case2,object_area_case3,object_area_case4,object_area_case5";
                Sql = Sql + ",object_area_case6,object_area_case7,object_area_case8,object_area_case9,object_area_case10,datetime_update)";

                Sql = Sql + "VALUES (@product_name_id,@product_display_name,@product_barcode,@product_size,@product_color,@position_partition,@position_partition_end,@count_taget,@product_alarm_enable,@product_alarm_value";
                Sql = Sql + ",@picproduct,@enable_dl,@dl_name_type,@dl_confidence_accept,@dl_count_accept";
                Sql = Sql + ",@object_least_distance,@object_min_area,@object_max_area,@object_area_case1,@object_area_case2,@object_area_case3,@object_area_case4,@object_area_case5";
                Sql = Sql + ",@object_area_case6,@object_area_case7,@object_area_case8,@object_area_case9,@object_area_case10,@datetime_update)";
            }
            else
            {
                Sql = "INSERT INTO " + TableProduct + "(product_name_id,product_display_name,product_barcode,product_size,product_color,position_partition,position_partition_end,count_taget,product_alarm_enable,product_alarm_value";
                Sql = Sql + ",enable_dl,dl_name_type,dl_confidence_accept,dl_count_accept";
                Sql = Sql + ",object_least_distance,object_min_area,object_max_area,object_area_case1,object_area_case2,object_area_case3,object_area_case4,object_area_case5";
                Sql = Sql + ",object_area_case6,object_area_case7,object_area_case8,object_area_case9,object_area_case10,datetime_update)";

                Sql = Sql + "VALUES (@product_name_id,@product_display_name,@product_barcode,@product_size,@product_color,@position_partition,@position_partition_end,@count_taget,@product_alarm_enable,@product_alarm_value";
                Sql = Sql + ",@enable_dl,@dl_name_type,@dl_confidence_accept,@dl_count_accept";
                Sql = Sql + ",@object_least_distance,@object_min_area,@object_max_area,@object_area_case1,@object_area_case2,@object_area_case3,@object_area_case4,@object_area_case5";
                Sql = Sql + ",@object_area_case6,@object_area_case7,@object_area_case8,@object_area_case9,@object_area_case10,@datetime_update)";
            }

            SqlConnection sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();


            SqlCommand cmd = new SqlCommand();
            cmd.Connection = sqlConn;
            cmd.CommandText = Sql;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == "")
                {
                   
                }
                if (data[i] == null)
                {
                    data[i] = "";
                }
            }


            string sdate = DateTime.Now.ToString();
            DateTime time = DateTime.Parse(sdate);


            cmd.Parameters.AddWithValue("@product_name_id", data[0]);
            cmd.Parameters.AddWithValue("@product_display_name", data[1]);
            cmd.Parameters.AddWithValue("@product_barcode", data[2]);
            cmd.Parameters.AddWithValue("@product_size", data[3]);
            cmd.Parameters.AddWithValue("@product_color", data[4]);
            cmd.Parameters.AddWithValue("@position_partition", data[5]);
            cmd.Parameters.AddWithValue("@count_taget", data[6]);
            cmd.Parameters.AddWithValue("@product_alarm_enable", data[7]);
            cmd.Parameters.AddWithValue("@product_alarm_value", data[8]);



            if (imagename != "")
            {

                if (File.Exists(imagename))
                {


                    FileStream fs;


                    fs = new FileStream(@imagename, FileMode.Open, FileAccess.Read);


                    //a byte array to read the image

                    byte[] picbyte = new byte[fs.Length];

                    fs.Read(picbyte, 0, System.Convert.ToInt32(fs.Length));

                    fs.Close();

                    cmd.Parameters.AddWithValue("@picproduct", (object)picbyte);


                    imagename = "";
                }


            }

          



            cmd.Parameters.AddWithValue("@enable_dl", data[9]);
            cmd.Parameters.AddWithValue("@dl_name_type", data[10]);
            cmd.Parameters.AddWithValue("@dl_confidence_accept", data[11]);
            cmd.Parameters.AddWithValue("@dl_count_accept", data[12]);

            cmd.Parameters.AddWithValue("@object_least_distance", data[13]);
            cmd.Parameters.AddWithValue("@object_min_area", data[14]);
            cmd.Parameters.AddWithValue("@object_max_area", data[15]);

            cmd.Parameters.AddWithValue("@object_area_case1", data[16]);
            cmd.Parameters.AddWithValue("@object_area_case2", data[17]);
            cmd.Parameters.AddWithValue("@object_area_case3", data[18]);
            cmd.Parameters.AddWithValue("@object_area_case4", data[19]);
            cmd.Parameters.AddWithValue("@object_area_case5", data[20]);
            cmd.Parameters.AddWithValue("@object_area_case6", data[21]);
            cmd.Parameters.AddWithValue("@object_area_case7", data[22]);
            cmd.Parameters.AddWithValue("@object_area_case8", data[23]);
            cmd.Parameters.AddWithValue("@object_area_case9", data[24]);
            cmd.Parameters.AddWithValue("@object_area_case10", data[25]);

          

            cmd.Parameters.AddWithValue("@datetime_update", time);

            cmd.Parameters.AddWithValue("@position_partition_end", data[26]);
            

            int state = cmd.ExecuteNonQuery();
            if (state == 1)//OK
            {

                UpdateProductTable();

                ret = true;
            }

            sqlConn.Close();
            sqlConn = null;


            return ret;
        }
        private void btEditProduct_Click(object sender, EventArgs e)
        {
            string[] data = new string[30];


            data[0] = ProductList.Text;
            data[1] = txtProductFullName.Text;
            data[2] = txtBarcodeID.Text;

            if (rdBoxSizeM.Checked == true)
            {
                data[3] = "1";
            }
            else
            {
                data[3] = "0";
            }

            if (radioColorTypeBlue.Checked == true)
            {
                data[4] = "1";
            }
            else if (radioColorTypeWhite.Checked == true)
            {
                data[4] = "2";
            }
            else if (radioColorTypeYellow.Checked == true)
            {
                data[4] = "3";
            }
            else if (radioColorTypeGray.Checked == true)
            {
                data[4] = "4";
            }


            data[5] = txtProductPos.Text.Trim();

            data[6] = numTarget.Value.ToString();

            if (ckReduceSpeed.Checked == true)
            {
                data[7] = "1";
            }
            else
            {
                data[7] = "0";
            }

            data[8] = numReduceSpeed.Value.ToString();

            if (ckDLProcess.Checked == true)
            {
                data[9] = "1";
            }
            else
            {
                data[9] = "0";
            }

            data[10] = cbObjectDL.Text.Trim();
            data[11] = numMinDLConfidance.Value.ToString();
            data[12] = numDLCountAccept.Value.ToString();






            data[13] = numLeastDistance.Value.ToString();
            data[14] = numRectAreaAceptMin.Value.ToString();
            data[15] = numRectAreaAceptMax.Value.ToString();
            data[16] = numObCase1.Value.ToString();
            data[17] = numObCase2.Value.ToString();
            data[18] = numObCase3.Value.ToString();
            data[19] = numObCase4.Value.ToString();
            data[20] = numObCase5.Value.ToString();
            data[21] = numObCase6.Value.ToString();
            data[22] = numObCase7.Value.ToString();
            data[23] = numObCase8.Value.ToString();
            data[24] = numObCase9.Value.ToString();
            data[25] = numObCase10.Value.ToString();

            data[26] = txtProductPosClose.Text.Trim(); //Update 10/10/2019

            data[27] = numofDilate.Value.ToString();
            data[28] = numofErode.Value.ToString();

            if (ProductList.Text == "")
            {
                MessageBox.Show("กรุณาเลือกข้อมูลที่ต้องการแก้ไข!!", "ผลการตรวจชอบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (lbID.Text == "")
            {
                MessageBox.Show("กรุณาเลือกข้อมูลที่ต้องการแก้ไข!!", "ผลการตรวจชอบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("กรุณายืนยันที่จะเพิ่มข้อมูล ?", "ยืนยันการเพื่มข้อมูล", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                UpdateProductBYID(lbID.Text, data, imagename);
            }
        }


        public bool UpdateProductBYID(string id, string[] data, string checkImageUpdate)
        {
            bool ret = false;


            string str = "UPDATE " + TableProduct + " SET product_name_id=@product_name_id";

           
    
              str += ",product_display_name=@product_display_name";
              str += ",product_barcode=@product_barcode";
              str += ",product_size=@product_size";
              str += ",product_color=@product_color";
              str += ",position_partition=@position_partition";
              str += ",position_partition_end=@position_partition_end";
            
              str += ",count_taget=@count_taget";
              str += ",product_alarm_enable=@product_alarm_enable";
              str += ",product_alarm_value=@product_alarm_value";

              if (checkImageUpdate != "") //ตรวจสอบข้อมูลว่ามีการเลือกรูปที่จะแก้ไขหรือเปล่า
              {
                 str += ",picproduct=@picproduct";
              }

              str += ",enable_dl=@enable_dl";
              str += ",dl_name_type=@dl_name_type";
              str += ",dl_confidence_accept=@dl_confidence_accept";
              str += ",dl_count_accept=@dl_count_accept";
              str += ",object_least_distance=@object_least_distance";
              str += ",object_min_area=@object_min_area";
              str += ",object_max_area=@object_max_area";
              str += ",object_area_case1=@object_area_case1";
              str += ",object_area_case2=@object_area_case2";
              str += ",object_area_case3=@object_area_case3";
              str += ",object_area_case4=@object_area_case4";
              str += ",object_area_case5=@object_area_case5";
              str += ",object_area_case6=@object_area_case6";
              str += ",object_area_case7=@object_area_case7";
              str += ",object_area_case8=@object_area_case8";
              str += ",object_area_case9=@object_area_case9";
              str += ",object_area_case10=@object_area_case10";
              str += ",Dilate=@Dilate";
              str += ",Erode=@Erode";
              str += ",datetime_update=@datetime_update";

 
            str += " WHERE id=" + id + " ";

            SqlConnection connection = new SqlConnection(this.connectionString);
            connection.Open();

            SqlCommand command = new SqlCommand
            {
                Connection = connection,
                CommandText = str
            };


            string sdate = DateTime.Now.ToString();
            DateTime time = DateTime.Parse(sdate);




            command.Parameters.AddWithValue("@product_name_id", data[0]);
            command.Parameters.AddWithValue("@product_display_name", data[1]);
            command.Parameters.AddWithValue("@product_barcode", data[2]);
            command.Parameters.AddWithValue("@product_size", data[3]);
            command.Parameters.AddWithValue("@product_color", data[4]);
            command.Parameters.AddWithValue("@position_partition", data[5]);
            command.Parameters.AddWithValue("@count_taget", data[6]);
            command.Parameters.AddWithValue("@product_alarm_enable", data[7]);
            command.Parameters.AddWithValue("@product_alarm_value", data[8]);



            if (checkImageUpdate != "") //ตรวจสอบข้อมูลว่ามีการเลือกรูปที่จะแก้ไขหรือเปล่า
            {
                if (imagename != "")
                {

                    if (File.Exists(imagename))
                    {


                        FileStream fs;


                        fs = new FileStream(@imagename, FileMode.Open, FileAccess.Read);


                        //a byte array to read the image

                        byte[] picbyte = new byte[fs.Length];

                        fs.Read(picbyte, 0, System.Convert.ToInt32(fs.Length));

                        fs.Close();

                        command.Parameters.AddWithValue("@picproduct", (object)picbyte);


                        imagename = "";
                    }


                }
            }





            command.Parameters.AddWithValue("@enable_dl", data[9]);
            command.Parameters.AddWithValue("@dl_name_type", data[10]);
            command.Parameters.AddWithValue("@dl_confidence_accept", data[11]);
            command.Parameters.AddWithValue("@dl_count_accept", data[12]);

            command.Parameters.AddWithValue("@object_least_distance", data[13]);
            command.Parameters.AddWithValue("@object_min_area", data[14]);
            command.Parameters.AddWithValue("@object_max_area", data[15]);

            command.Parameters.AddWithValue("@object_area_case1", data[16]);
            command.Parameters.AddWithValue("@object_area_case2", data[17]);
            command.Parameters.AddWithValue("@object_area_case3", data[18]);
            command.Parameters.AddWithValue("@object_area_case4", data[19]);
            command.Parameters.AddWithValue("@object_area_case5", data[20]);
            command.Parameters.AddWithValue("@object_area_case6", data[21]);
            command.Parameters.AddWithValue("@object_area_case7", data[22]);
            command.Parameters.AddWithValue("@object_area_case8", data[23]);
            command.Parameters.AddWithValue("@object_area_case9", data[24]);
            command.Parameters.AddWithValue("@object_area_case10", data[25]);

           


            command.Parameters.AddWithValue("@datetime_update", time);



            command.Parameters.AddWithValue("@position_partition_end", data[26]);

            command.Parameters.AddWithValue("@Dilate", data[27]);
            command.Parameters.AddWithValue("@Erode", data[28]);



            if (command.ExecuteNonQuery() == 1)
            {
                ret = true;
                UpdateProductTable();

            }


            connection.Close();
            connection = null;

            return ret;
        }

        private void btDeleteProduct_Click(object sender, EventArgs e)
        {
            if (lbID.Text == "")
            {
                MessageBox.Show("กรุณาเลือกข้อมูลที่ต้องการลบ!!", "ผลการตรวจชอบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("กรุณายืนยันที่จะลบข้อมูล?", "ยืนยันการลบข้อมูล", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {

                DeleteProductByID(lbID.Text);
                lbID.Text = "";
                imagename = "";
            }
        }


        public bool DeleteProductByID(string id)
        {
            bool ret = false;

            string WhereCondition = "";
            string FieldSql = "";

            String sql = "";
            WhereCondition = " WHERE  (id = '" + id + "')";

            sql = "Delete " + FieldSql + " From " + TableProduct + " " + WhereCondition;

            SqlConnection sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = sqlConn;
                cmd.CommandText = sql;
                int sqlret = cmd.ExecuteNonQuery();




                if (sqlret == 1)
                {
                    UpdateProductTable();
                    //UpdateSupplierTable();
                    ret = true;
                }


                sqlConn.Close();
                sqlConn = null;


            }
            catch
            {
                sqlConn.Close();
                sqlConn = null;
            }




            return ret;

        }

        private void btLoadData_Click(object sender, EventArgs e)
        {

            if (txtBarcodeSelect.Text != "")
            {
                ClearProductDisplay();


                if (ParentForm != null)
                {
                    ParentForm.ClearUIDisplay();

                }

                FindProductBYBarcode(txtBarcodeSelect.Text);
            }
        }

        private void btbrowseimage_Click(object sender, EventArgs e)
        {
            if (picShow.Image != null)
                picShow.Image.Dispose();


            openimage = new OpenFileDialog();

            openimage.InitialDirectory = @"C:\";
            // image filters  
            openimage.Filter = "Image Files(*.jpg; *.jpeg; *.bmp; *.png)|*.jpg; *.jpeg; *.bmp ; *.png";
            if (openimage.ShowDialog() == DialogResult.OK)
            {
                // display image in picture box  
                picShow.Image = new Bitmap(openimage.FileName);
                picShow.SizeMode = PictureBoxSizeMode.StretchImage;

                imagename = openimage.FileName;
                picShow.Image = Image.FromFile(imagename);

                if (picShow.Image != null)
                {


                }

            }
            else
            {
                imagename = "";
            }


            openimage = null;
        }

        private void ProductList_SelectedIndexChanged(object sender, EventArgs e)
        {

            txtBarcodeID.Text = "";
            txtBarcodeSelect.Text = "";
            FindProductBYProductID(ProductList.Text.Trim());
        }


        public void FindProductBYProductID(string ProductID)
        {

            string WhereCondition = "";
            string FieldSql = "*";


            String sql = "";
            WhereCondition = " WHERE product_name_id = '" + ProductID.Trim() + "' ";

            sql = "Select " + FieldSql + " From " + TableProduct + " " + WhereCondition;// " order by SALESID asc";// +order;//desc";//asc



            int count = 0;

            SqlConnection sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();

            SqlDataAdapter da = new SqlDataAdapter(sql, sqlConn);
            DataSet ds = new DataSet();

            DataTable dt = new DataTable();
            da.Fill(ds, TableProduct);


            dt = ds.Tables[TableProduct];
            if (ds.Tables[TableProduct].Rows.Count <= 0)
            {
                ClearProductDisplay();

                if (ParentForm != null)
                {
                    ParentForm.ClearUIDisplay();
                }

            }
            else
            {


                for (int i = 0; i < ds.Tables[TableProduct].Rows.Count; i++)
                {

                    string idedit = dt.Rows[i]["id"].ToString();

                    lbID.Text = idedit;

                  
                    string product_barcode = dt.Rows[i]["product_barcode"].ToString();
                    txtBarcodeID.Text = product_barcode;
                    txtBarcodeSelect.Text = product_barcode;
                    count++;

                }


            }


            sqlConn.Close();
            sqlConn = null;

        }

        public void ClearProductDisplay()
        {
                        picShow.Image = null;

                        txtProductName.Text = "";
                        txtProductFullName.Text = "";
                        txtBarcodeID.Text = "";

                        rdBoxSizeM.Checked = true;
                        radioColorTypeBlue.Checked = true;

                        numTarget.Value = 200;
                        ckReduceSpeed.Checked = false;
                        numReduceSpeed.Value = 80;
                        ckDLProcess.Checked = false;

                        cbObjectDL.Text = "";
                        numMinDLConfidance.Value = 0;
                        numDLCountAccept.Value = 0;

                        ParentForm.numLeastDistance.Value = 0;
                        ParentForm.numRectAreaAceptMin.Value = 0;
                        ParentForm.numRectAreaAceptMax.Value = 0;

                        ParentForm.numObCase1.Value = 0;
                        ParentForm.numObCase2.Value = 0;
                        ParentForm.numObCase3.Value = 0;
                        ParentForm.numObCase4.Value = 0;
                        ParentForm.numObCase5.Value = 0;

                        ParentForm.numObCase6.Value = 0;
                        ParentForm.numObCase7.Value = 0;
                        ParentForm.numObCase8.Value = 0;
                        ParentForm.numObCase9.Value = 0;
                        ParentForm.numObCase10.Value = 0;
        }
        public void FindProductBYBarcode(string product_barcode)
        {





            string WhereCondition = "";
            string FieldSql = "*";

            String sql = "";
            WhereCondition = " WHERE product_barcode = '" + product_barcode.Trim() + "' ";

            sql = "Select " + FieldSql + " From " + TableProduct + " " + WhereCondition;

            int count = 0;

            SqlConnection sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();

            SqlDataAdapter da = new SqlDataAdapter(sql, sqlConn);
            DataSet ds = new DataSet();

            DataTable dt = new DataTable();
            da.Fill(ds, TableProduct);


            dt = ds.Tables[TableProduct];
            if (ds.Tables[TableProduct].Rows.Count <= 0)
            {

                ClearProductDisplay();

                if (ParentForm != null)
                {
                    ParentForm.ClearUIDisplay();
                }
               
            }
            else
            {


                for (int i = 0; i < ds.Tables[TableProduct].Rows.Count; i++)
                {

                    string idedit = dt.Rows[i]["id"].ToString();

                    lbID.Text = idedit;


                    string product_name_id = dt.Rows[i]["product_name_id"].ToString();
                    string product_display_name = dt.Rows[i]["product_display_name"].ToString();
                    string product_barcode_ret = dt.Rows[i]["product_barcode"].ToString();
                    string product_size = dt.Rows[i]["product_size"].ToString();
                    string product_color = dt.Rows[i]["product_color"].ToString();

                    string position_partition = dt.Rows[i]["position_partition"].ToString();
                    string position_partition_end = dt.Rows[i]["position_partition_end"].ToString();
                    
                    string count_taget = dt.Rows[i]["count_taget"].ToString();
                    string product_alarm_enable = dt.Rows[i]["product_alarm_enable"].ToString();
                    string product_alarm_value = dt.Rows[i]["product_alarm_value"].ToString();
                  
                    string enable_dl = dt.Rows[i]["enable_dl"].ToString();
                    string dl_name_type = dt.Rows[i]["dl_name_type"].ToString();
                    string dl_confidence_accept = dt.Rows[i]["dl_confidence_accept"].ToString();
                    string dl_count_accept = dt.Rows[i]["dl_count_accept"].ToString();
                    string object_least_distance = dt.Rows[i]["object_least_distance"].ToString();
                    string object_min_area = dt.Rows[i]["object_min_area"].ToString();
                    string object_max_area = dt.Rows[i]["object_max_area"].ToString();
                    string object_area_case1 = dt.Rows[i]["object_area_case1"].ToString();
                    string object_area_case2 = dt.Rows[i]["object_area_case2"].ToString();
                    string object_area_case3 = dt.Rows[i]["object_area_case3"].ToString();
                    string object_area_case4 = dt.Rows[i]["object_area_case4"].ToString();
                    string object_area_case5 = dt.Rows[i]["object_area_case5"].ToString();
                    string object_area_case6 = dt.Rows[i]["object_area_case6"].ToString();
                    string object_area_case7 = dt.Rows[i]["object_area_case7"].ToString();
                    string object_area_case8 = dt.Rows[i]["object_area_case8"].ToString();
                    string object_area_case9 = dt.Rows[i]["object_area_case9"].ToString();
                    string object_area_case10 = dt.Rows[i]["object_area_case10"].ToString();

                    string Dilate = dt.Rows[i]["Dilate"].ToString();
                    string Erode = dt.Rows[i]["Erode"].ToString();

                    if(Dilate=="")
                    {
                        Dilate = "3";
                    }

                    if (Erode == "")
                    {
                        Erode = "3";
                    }

                    if (object_least_distance == "")
                    {
                        object_least_distance = "0";
                    }


                    if (object_min_area == "")
                    {
                        object_min_area = "0";
                    }

                    if (object_max_area == "")
                    {
                        object_max_area = "0";
                    }

                    if (object_area_case1 == "")
                    {
                        object_area_case1 = "0";
                    }
                    if (object_area_case2 == "")
                    {
                        object_area_case2 = "0";
                    }
                    if (object_area_case3 == "")
                    {
                        object_area_case3 = "0";
                    }
                    if (object_area_case4 == "")
                    {
                        object_area_case4 = "0";
                    }
                    if (object_area_case5 == "")
                    {
                        object_area_case5 = "0";
                    }

                    if (object_area_case6 == "")
                    {
                        object_area_case6 = "0";
                    }
                    if (object_area_case7 == "")
                    {
                        object_area_case7 = "0";
                    }
                    if (object_area_case8 == "")
                    {
                        object_area_case8 = "0";
                    }
                    if (object_area_case9 == "")
                    {
                        object_area_case9 = "0";
                    }
                    if (object_area_case10 == "")
                    {
                        object_area_case10 = "0";
                    }


                    if (dl_confidence_accept == "")
                    {
                        dl_confidence_accept = "0";
                    }

                    if (dl_count_accept == "")
                    {
                        dl_count_accept = "0";
                    }
                    





                    try
                    {



                        //Get image data from gridview column.
                        byte[] imageData = (byte[])dt.Rows[i]["picproduct"];

                        MemoryStream stream = new MemoryStream();


                        stream.Write(imageData, 0, imageData.Length);
                       
                        picShow.SizeMode = PictureBoxSizeMode.StretchImage;
                        picShow.Image = Image.FromStream(stream);

                        if (ParentForm != null) //Update Main Display
                        {
                            ParentForm.picShow.SizeMode = PictureBoxSizeMode.StretchImage;
                            ParentForm.picShow.Image = Image.FromStream(stream);
                        }

                    }
                    catch (Exception ex)
                    {
                        if (ParentForm != null)
                        {

                            ParentForm.picShow.Image = null;
                           
                        }


                        picShow.Image = null;
                    }

               

                    txtProductName.Text = product_name_id.Trim();
                    txtProductFullName.Text = product_display_name.Trim();
                    txtBarcodeID.Text = product_barcode_ret.Trim();


                    if (product_size.Trim() == "1")
                    {
                        rdBoxSizeM.Checked = true;
                        rdBoxSizeL.Checked = false;
                    }
                    else
                    {
                        rdBoxSizeM.Checked = false;
                        rdBoxSizeL.Checked = true;
                    }


                    if (product_color.Trim() == "1")
                    {
                        radioColorTypeBlue.Checked = true;
                        radioColorTypeWhite.Checked = false;
                        radioColorTypeYellow.Checked = false;
                        radioColorTypeGray.Checked = false;

                    }
                    else if (product_color.Trim() == "2")
                    {
                        radioColorTypeBlue.Checked = false;
                        radioColorTypeWhite.Checked = true;
                        radioColorTypeYellow.Checked = false;
                        radioColorTypeGray.Checked = false;
                    }
                    else if (product_color.Trim() == "3")
                    {
                        radioColorTypeBlue.Checked = false;
                        radioColorTypeWhite.Checked = false;
                        radioColorTypeYellow.Checked = true;
                        radioColorTypeGray.Checked = false;
                    }
                    else if (product_color.Trim() == "4")
                    {
                        radioColorTypeBlue.Checked = false;
                        radioColorTypeWhite.Checked = false;
                        radioColorTypeYellow.Checked = false;
                        radioColorTypeGray.Checked = true;
                    }

                    numTarget.Value = decimal.Parse(count_taget.Trim());
                    
                    if (product_alarm_enable.Trim() == "1")
                    {
                        ckReduceSpeed.Checked = true;
                    }
                    else
                    {
                        ckReduceSpeed.Checked = false;
                    }


                    numReduceSpeed.Value = decimal.Parse(product_alarm_value.Trim());

                    if (enable_dl.Trim() == "1")
                    {
                        ckDLProcess.Checked = true;
                    }
                    else
                    {
                        ckDLProcess.Checked = false;
                    }

                  

                    cbObjectDL.Text = dl_name_type.Trim();

                    numMinDLConfidance.Value = decimal.Parse(dl_confidence_accept.Trim());
                    numDLCountAccept.Value = decimal.Parse(dl_count_accept.Trim());

                    numLeastDistance.Value = decimal.Parse(object_least_distance.Trim());
                    numRectAreaAceptMin.Value = decimal.Parse(object_min_area.Trim());
                    numRectAreaAceptMax.Value = decimal.Parse(object_max_area.Trim());

                    numObCase1.Value = decimal.Parse(object_area_case1.Trim());
                    numObCase2.Value = decimal.Parse(object_area_case2.Trim());
                    numObCase3.Value = decimal.Parse(object_area_case3.Trim());
                    numObCase4.Value = decimal.Parse(object_area_case4.Trim());
                    numObCase5.Value = decimal.Parse(object_area_case5.Trim());

                    numObCase6.Value = decimal.Parse(object_area_case6.Trim());
                    numObCase7.Value = decimal.Parse(object_area_case7.Trim());
                    numObCase8.Value = decimal.Parse(object_area_case8.Trim());
                    numObCase9.Value = decimal.Parse(object_area_case9.Trim());
                    numObCase10.Value = decimal.Parse(object_area_case10.Trim());


                    numofDilate.Value = decimal.Parse(Dilate.Trim());
                    numofErode.Value = decimal.Parse(Erode.Trim());


                    txtProductPos.Text = position_partition.Trim();
                    txtProductPosClose.Text = position_partition_end.Trim();

                    if (ParentForm != null) //Update Main Display
                    {
                        

                        ParentForm.txtProductName.Text = txtProductName.Text;
                        ParentForm.txtProductFullName.Text = txtProductFullName.Text;
                        ParentForm.txtBarcodeID.Text = txtBarcodeID.Text;
                        ParentForm.txtTargetCount.Text = count_taget.Trim();

                        ParentForm.txtProductPos.Text = txtProductPos.Text;
                        ParentForm.txtProductPosEnd.Text = txtProductPosClose.Text;

                        ParentForm.ckReduceSpeed.Checked = ckReduceSpeed.Checked;
                        ParentForm.numReduceSpeed.Value = numReduceSpeed.Value;
                        ParentForm.ckDLProcess.Checked = ckDLProcess.Checked;

                        ParentForm.cbObjectList.Text = cbObjectDL.Text;
                        ParentForm.txtConfidence.Text = numMinDLConfidance.Value.ToString();

                        ParentForm.numDLCountAccept.Value = numDLCountAccept.Value;

                        ParentForm.DL_Type_Count_Accept = (int)numDLCountAccept.Value;

                        ParentForm.DLConfidence = (double)numMinDLConfidance.Value;
                        ParentForm.ObjectDLSelect = cbObjectDL.Text; 

                        ParentForm.numLeastDistance.Value = numLeastDistance.Value;
                        ParentForm.numRectAreaAceptMin.Value = numRectAreaAceptMin.Value;
                        ParentForm.numRectAreaAceptMax.Value = numRectAreaAceptMax.Value;

                        ParentForm.numObCase1.Value = numObCase1.Value;
                        ParentForm.numObCase2.Value = numObCase2.Value;
                        ParentForm.numObCase3.Value = numObCase3.Value;
                        ParentForm.numObCase4.Value = numObCase4.Value;
                        ParentForm.numObCase5.Value = numObCase5.Value;

                        ParentForm.numObCase6.Value = numObCase6.Value;
                        ParentForm.numObCase7.Value = numObCase7.Value;
                        ParentForm.numObCase8.Value = numObCase8.Value;
                        ParentForm.numObCase9.Value = numObCase9.Value;
                        ParentForm.numObCase10.Value = numObCase10.Value;

                        ParentForm.numofDilate.Value = numofDilate.Value;
                        ParentForm.numofErode.Value = numofErode.Value;

                        ParentForm.rdColorBlueSelect.Checked = radioColorTypeBlue.Checked;
                        ParentForm.rdColorWhiteSelect.Checked = radioColorTypeWhite.Checked;
                        ParentForm.rdColorYellowSelect.Checked = radioColorTypeYellow.Checked;
                        ParentForm.rdColorGraySelect.Checked = radioColorTypeGray.Checked;

                        ParentForm.ColorYellowSelect =radioColorTypeYellow.Checked;
                        ParentForm.ColorGraySelect = radioColorTypeGray.Checked;
                        ParentForm.ColorWhiteSelect = radioColorTypeWhite.Checked;
                        ParentForm.ColorBlueSelect = radioColorTypeBlue.Checked;

                        ParentForm.AreaAceptmin = (Int64)numRectAreaAceptMin.Value;
                        ParentForm.AreaAceptmax = (Int64)numRectAreaAceptMax.Value;

                        ParentForm.LeastDistance = (Int64)numLeastDistance.Value;

                        ParentForm.AreaAceptCase1 = (Int64)numObCase1.Value;   
                        ParentForm.AreaAceptCase2 = (Int64)numObCase2.Value;          
                        ParentForm.AreaAceptCase3 = (Int64)numObCase3.Value;
                        ParentForm.AreaAceptCase4 = (Int64)numObCase4.Value;                 
                        ParentForm.AreaAceptCase5 = (Int64)numObCase5.Value;

                        ParentForm.AreaAceptCase6 = (Int64)numObCase6.Value;
                        ParentForm.AreaAceptCase7 = (Int64)numObCase7.Value;
                        ParentForm.AreaAceptCase8 = (Int64)numObCase8.Value;
                        ParentForm.AreaAceptCase9 = (Int64)numObCase9.Value;
                        ParentForm.AreaAceptCase10 = (Int64)numObCase10.Value;


                    }


                    count++;
                }


            }


            sqlConn.Close();
            sqlConn = null;

        }
        private void ProductTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (ProductTable.SelectedRows.Count > 0)
            {
                int rowsel = ProductTable.CurrentRow.Index;

                if (ProductTable.Rows[rowsel].Cells[0].Value.ToString() != "")
                {
                    // = ProductTable.Rows[rowsel].Cells[0].Value.ToString();
                    BarcodeIDSelect = ProductTable.Rows[rowsel].Cells[3].Value.ToString();
                    txtBarcodeSelect.Text = BarcodeIDSelect;
                    txtBarcodeID.Text = BarcodeIDSelect;
                }

            }


        }

        private void button18_Click(object sender, EventArgs e)
        {

            ClearProductDisplay();


            if (ParentForm != null)
            {
                ParentForm.ClearUIDisplay();

            }

            FindProductBYBarcode(txtBarcodeSelect.Text);
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void ProductTable_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void BtAutoCal_Click(object sender, EventArgs e)
        {
            try
            {
                double ObCase1 = Convert.ToDouble(numerObjectArea.Value);
                double percentageCase1 = (ObCase1 * Convert.ToDouble(numpercentageob.Value)) / 100;

                numObCase1.Value = Convert.ToInt64(percentageCase1);
                numObCase2.Value = Convert.ToInt64(ObCase1 * 1 + percentageCase1);
                numObCase3.Value = Convert.ToInt64(ObCase1 * 2 + percentageCase1);
                numObCase4.Value = Convert.ToInt64(ObCase1 * 3 + percentageCase1);
                numObCase5.Value = Convert.ToInt64(ObCase1 * 4 + percentageCase1);
                numObCase6.Value = Convert.ToInt64(ObCase1 * 5 + percentageCase1);
                numObCase7.Value = Convert.ToInt64(ObCase1 * 6 + percentageCase1);
                numObCase8.Value = Convert.ToInt64(ObCase1 * 7 + percentageCase1);
                numObCase9.Value = Convert.ToInt64(ObCase1 * 8 + percentageCase1);
                numObCase10.Value = Convert.ToInt64(ObCase1 * 9 + percentageCase1);
            }
            catch
            {


            }

        }

        private void BtRecheckColor_Click(object sender, EventArgs e)
        {
            UpdateProductTable();
        }
    }
}

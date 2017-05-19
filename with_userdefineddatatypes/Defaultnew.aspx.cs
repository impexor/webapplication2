using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Data.Common;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using Excel = Microsoft.Office.Interop.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using MySql.Data.MySqlClient;
using Renci.SshNet;
namespace WebApplication2
{
    public partial class _Defaultnew : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //testmtd();
            wsFFHP.ffhpservice ws = new wsFFHP.ffhpservice();
            string sqlConn = System.Configuration.ConfigurationManager.AppSettings["SqlConnectionString"].ToString();

            string queryString = @"exec sp_calculate_purchaseweight";

           DataTable dtWeight = new DataTable();
            DataTable dtTesttotalWeight = new DataTable();
            DataTable dtVendor = new DataTable();

            SqlDataAdapter daSQL = new SqlDataAdapter(queryString, sqlConn);
            //  SqlDataAdapter daVendor = new SqlDataAdapter(vendordetails, sqlConn);
            
            dtTesttotalWeight = ws.GetCalculateWeightNew();
            copyTestPackFinaltoDB(dtTesttotalWeight);
            daSQL.Fill(dtWeight);
           

            GridView1.DataSource = dtWeight;
            GridView1.DataBind();
            
            FreeProductWeightCalc(); // to update the free products weight to the ffhp_purchasetemplate table
            weightCalculate(dtWeight, dtVendor);
            
            

        }

        public void FreeProductWeightCalc()
        {
            double dblweight = 0.0;
            int intOrdercount = 0;
            int intPdtID = 0;
            double dblOrderWeight = 0.0;
            //connect to the sql db to get the number of oders
            string sqlConn = System.Configuration.ConfigurationManager.AppSettings["SqlConnectionString"].ToString();
            using (SqlConnection conn = new SqlConnection(sqlConn))
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "select count(distinct ordernumber) from [dbo].[ffhporders]";
                intOrdercount = (int)cmd.ExecuteScalar();
                conn.Close();
             }

            //connect to the mysql db to get the free product
            string MysqlConn = System.Configuration.ConfigurationManager.AppSettings["MysqlConnection"].ToString();
            MySqlConnection connection = new MySqlConnection(MysqlConn);
            string queryString = @"SELECT * FROM 1_ffhp_free ";
            DataTable dtFree = new DataTable();
            MySqlDataAdapter daMySQL = new MySqlDataAdapter(queryString, MysqlConn);
            daMySQL.Fill(dtFree);
           // intPdtID = Convert.ToInt16(dtFree.Rows[0]["ParentProductID"]);

            intPdtID = Convert.ToInt16(dtFree.Rows[0]["product_id"]);
            if(dtFree.Rows[0]["units"].ToString() == "kg")
            {
                dblweight = Convert.ToDouble(dtFree.Rows[0]["weight"]);
            }
            else if (dtFree.Rows[0]["units"].ToString() == "PC")
            {
                dblweight = Convert.ToInt16(dtFree.Rows[0]["weight"]);
                 
            }

            double dblFreeWeight = dblweight * intOrdercount;
            //insert the weight if the product is not there update the weight if the product is there already
            using (SqlConnection conn = new SqlConnection(sqlConn))
            {

                conn.Open();
                SqlCommand cmdFreeProduct = new SqlCommand();
                cmdFreeProduct.Connection = conn;

                string sqlQuery = "select * from ffhp_PurchaseTemplate where Product_Id= " + intPdtID + " ";
                DataTable dtTemplate = new DataTable();
                SqlDataAdapter daTemplate = new SqlDataAdapter(sqlQuery, sqlConn);
                daTemplate.Fill(dtTemplate);
                string strFreePdtQuery = "";
                

                if (dtTemplate.Rows.Count > 0)
                {
                    dblOrderWeight = Convert.ToDouble(dtTemplate.Rows[0]["TotalWeight"]);
                    strFreePdtQuery = "update ffhp_PurchaseTemplate set TotalWeight =" + (dblOrderWeight + dblFreeWeight) + " ,Extra_Wt =" + (dblOrderWeight + dblFreeWeight) + " ,PurchaseWeight =" + (dblOrderWeight + dblFreeWeight) + " where  Product_Id = " + intPdtID + " ";
                    cmdFreeProduct = new SqlCommand(strFreePdtQuery, conn);
                }
                else if (dtTemplate.Rows.Count == 0)
                {
                    //to get the vendor details if the product does not exist in the order list
                    string strVendorQuery = "select productid, D.vendorid, vendorname  from vendor_products_default D left outer join vendordetails  V on d.vendorid=V.vendorid where productid='" + intPdtID + "' ";
                    DataTable dtVendor = new DataTable();
                    SqlDataAdapter daVendor = new SqlDataAdapter(strVendorQuery, sqlConn);
                    daVendor.Fill(dtVendor);

                    strFreePdtQuery = "INSERT INTO ffhp_PurchaseTemplate (Product_Id , Name, TotalWeight , Extra_Wt , PurchaseWeight, Units , vendorid ,vendorname ,created_at ,updated_at) VALUES (@Product_Id ,@Name ,@TotalWeight ,@Extra_Wt ,@PurchaseWeight ,@Units ,@vendorid,@vendorname,@created_at,@updated_at)";

                    using (cmdFreeProduct = new SqlCommand(strFreePdtQuery,conn))
                    {

                        cmdFreeProduct.Parameters.Add("@Product_Id", SqlDbType.Int).Value = intPdtID;
                        cmdFreeProduct.Parameters.Add("@Name", SqlDbType.VarChar, 50).Value = dtFree.Rows[0]["name"];
                        cmdFreeProduct.Parameters.Add("@TotalWeight", SqlDbType.Decimal).Value = dblFreeWeight;
                        cmdFreeProduct.Parameters.Add("@Extra_Wt", SqlDbType.Decimal, 30).Value = dblFreeWeight;
                        cmdFreeProduct.Parameters.Add("@PurchaseWeight", SqlDbType.Decimal, 30).Value = dblFreeWeight;
                        cmdFreeProduct.Parameters.Add("@Units", SqlDbType.VarChar, 10).Value = dtFree.Rows[0]["units"];
                        cmdFreeProduct.Parameters.Add("@vendorid", SqlDbType.Int).Value = Convert.ToInt16(dtVendor.Rows[0]["vendorid"]);
                        cmdFreeProduct.Parameters.Add("@vendorname", SqlDbType.VarChar, 50).Value = dtVendor.Rows[0]["vendorname"].ToString();
                        cmdFreeProduct.Parameters.Add("@created_at", SqlDbType.DateTime).Value = DateTime.Now;
                        cmdFreeProduct.Parameters.Add("@updated_at", SqlDbType.DateTime).Value = DateTime.Now;
                    }
                  
                    
                }
               // cmdFreeProduct.Connection = conn;
                cmdFreeProduct.ExecuteNonQuery();
                conn.Close();
            }
           // dtFree.Columns["product_id"]
             //   dtFree.Columns["name"] 
           
        }
        public void copyTestPackFinaltoDB(DataTable dt)
        {
            string sqlConn = System.Configuration.ConfigurationManager.AppSettings["SqlConnectionString"].ToString();
            using (SqlConnection con = new SqlConnection(sqlConn))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Insert_testtotalweight"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@tblWindowsServiceData", dt);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }
        public void weightCalculate(DataTable dtWeight, DataTable dtVendor)
        {

            copyDatatabletoDB(dtWeight);
            // DataTable dtExcelPdf = dtWeight.DefaultView.ToTable(false, "Product_Id", "name", "PurchaseWeight", "units", "Vendorname");
            string sqlConn = System.Configuration.ConfigurationManager.AppSettings["SqlConnectionString"].ToString();


            string queryString = @"exec sp_ExportPQExcel";

            DataSet dsGrid = new DataSet();

            SqlDataAdapter daSQL = new SqlDataAdapter(queryString, sqlConn);

            daSQL.Fill(dsGrid);
            DataSet dsFont = GetDSetwithFont(); //To get the product name font from mysql DB

            //join dsGrid and dsFont to get the desired result to bind to grid
       
            DataTable dtGrid = new DataTable();
            
            dtGrid.Columns.Add("product_id", typeof(int));
            dtGrid.Columns.Add("Name",typeof(string));
            dtGrid.Columns.Add("OrderWeight",typeof(string));
            dtGrid.Columns.Add("PurchaseWeight", typeof(string));
            dtGrid.Columns.Add("Cutdescription", typeof(string));
            dtGrid.Columns.Add("VendorName", typeof(string));
            foreach (DataRow drGrid in dsGrid.Tables[0].Rows)
            {

                dtGrid.ImportRow(drGrid);
   
            }
            
            DataTable dtFont = new DataTable();
            dtFont.Columns.Add("product_id", typeof(int));
            dtFont.Columns.Add("product_name", typeof(string));
            dtFont.Columns.Add("sku", typeof(string));
            foreach (DataRow drFont in dsFont.Tables[0].Rows)
            {

                dtFont.ImportRow(drFont);

            }

            DataTable dtGridFont = new DataTable();
            dtGridFont.Columns.Add("ID", typeof(int));
            dtGridFont.Columns.Add("name", typeof(string));
            dtGridFont.Columns.Add("OrderWeight", typeof(string));
            dtGridFont.Columns.Add("PurchaseWeight", typeof(string));
            dtGridFont.Columns.Add("Cutdescription", typeof(string));
            dtGridFont.Columns.Add("VendorName", typeof(string));

            var result = from dataRows1 in dtGrid.AsEnumerable()
                         join dataRows2 in dtFont.AsEnumerable()
                         on dataRows1.Field<int>("product_id") equals dataRows2.Field<int>("product_id")

                         select dtGridFont.LoadDataRow(new object[]
             {
                dataRows1.Field<int>("product_id"),
                dataRows2.Field<string>("product_name"),
                dataRows1.Field<string>("OrderWeight"),
                dataRows1.Field<string>("PurchaseWeight"),
                dataRows1.Field<string>("Cutdescription"),
                dataRows1.Field<string>("VendorName")
               
               
              }, false);
            result.CopyToDataTable();

            gvWeight.DataSource = dtGridFont;

            // gvWeight.DataSource = dsGrid;
            gvWeight.DataBind();
            //  GridView1.DataSource = dsGrid;
            //GridView1.DataBind();
            // ExportDatatabletoExcel(dtExcelPdf); gives cls error
            // ExportDatatabletoExcel();not wrking
            //ExportToPdf(dsGrid);
            renderExcel();

            //    sendsms(dt, "8754543655");



        }
        public void copyDatatabletoDB(DataTable dt)
        {
            string sqlConn = System.Configuration.ConfigurationManager.AppSettings["SqlConnectionString"].ToString();
            using (SqlConnection con = new SqlConnection(sqlConn))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Insert_purchaseTemplate"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@tblPurchaseTemplate", dt);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }
        protected void Butbtnsendexcel_Click(object sender, EventArgs e)
        {

        }
        public void ExportDatatabletoExcel(DataTable Tbl)
        {

            string ExcelFilePath = System.Configuration.ConfigurationManager.AppSettings["FilePath"].ToString();
            string filename = "TotalWeight" + DateTime.Now.ToString("dd-MM-yyyy");
            //  string ExcelFilePath="E:\\FFHP\\MailFiles";
            gvWeight.Visible = true;

            if (Tbl == null || Tbl.Columns.Count == 0)
                throw new Exception("ExportToExcel: Null or empty input table!\n");

            // load excel, and create a new workbook
            Excel.Application excelApp = new Excel.Application();
            excelApp.Workbooks.Add();

            // single worksheet
            //Excel._Worksheet workSheet = excelApp.ActiveSheet;
            Excel._Worksheet workSheet = (Excel.Worksheet)excelApp.ActiveSheet;

            // column headings
            for (int i = 0; i < Tbl.Columns.Count; i++)
            {
                workSheet.Cells[1, (i + 1)] = Tbl.Columns[i].ColumnName;
            }

            // rows
            for (int i = 0; i < Tbl.Rows.Count; i++)
            {
                // to do: format datetime values before printing
                for (int j = 0; j < Tbl.Columns.Count; j++)
                {
                    workSheet.Cells[(i + 2), (j + 1)] = Tbl.Rows[i][j];
                }
            }

            // check filepath
            if (ExcelFilePath != null && ExcelFilePath != "")
            {
                try
                {
                    //  workSheet.SaveAs(ExcelFilePath);
                    workSheet.SaveAs(Server.MapPath(ExcelFilePath + filename + ".xlsx"));
                    excelApp.Quit();
                    // MessageBox.Show("Excel file saved!");
                }
                catch (Exception ex)
                {
                    throw new Exception("ExportToExcel: Excel file could not be saved! Check filepath.\n"
                        + ex.Message);
                }
            }
            else    // no filepath is given
            {
                excelApp.Visible = true;
            }


        }

        public void renderExcel()
        {
            #region
            /* try
            {


                string filepath = System.Configuration.ConfigurationManager.AppSettings["FilePath"].ToString();

                gvWeight.Visible = true;
                string filename = "Totalweight" + DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss");
                //Response.ContentType = "application/ms-excel";
                //Response.AddHeader("content-disposition", "attachment;filename=CustomerInfo.xls");
                //Response.Cache.SetCacheability(HttpCacheability.NoCache);
                StringWriter sw = new StringWriter();
                HtmlTextWriter hw = new HtmlTextWriter(sw);
                System.Web.UI.HtmlControls.HtmlForm f = new System.Web.UI.HtmlControls.HtmlForm();
                //Panel Tom = new Panel();
                //Tom.ID = base.UniqueID;
                //Tom.Controls.Add(myControl);
                //Page.FindControl("WebForm1").Controls.Add(Tom);

                gvWeight.AllowPaging = false;
                f.Controls.Add(gvWeight);
                //GVOrderDetails2.DataBind();
                gvWeight.RenderControl(hw);
                //GVOrderDetails2.HeaderRow.Style.Add("width", "15%");
                //GVOrderDetails2.HeaderRow.Style.Add("font-size", "10px");
                //GVOrderDetails2.Style.Add("text-decoration", "none");
                //GVOrderDetails2.Style.Add("font-family", "Arial, Helvetica, sans-serif;");
                //GVOrderDetails2.Style.Add("font-size", "8px");

                // Open an existing Excel 2007 file.

                //IWorkbook workbook = excelEngine.Excel.Workbooks.Open(Server.MapPath(filepath + "Book1.xlsx"), ExcelOpenType.Automatic);



                // Select the version to be saved.

                //workbook.Version = ExcelVersion.Excel2007;



                // Save it as "Excel 2007" format.

                //workbook.SaveAs("Sample.xlsx");
                StreamWriter writer = File.AppendText(Server.MapPath(filepath + filename + ".xlsx"));
                //Response.WriteFile(Server.MapPath("MailFiles/CustomerInformation/" + Session.SessionID + ".xls"));
                writer.WriteLine(sw.ToString());
                writer.Close();
                gvWeight.Visible = false;

                
                string mailto = System.Configuration.ConfigurationManager.AppSettings["Mail_To"].ToString();
                string mailcredential = System.Configuration.ConfigurationManager.AppSettings["Mail_Credential"].ToString();
                string mailpassword = System.Configuration.ConfigurationManager.AppSettings["Mail_Password"].ToString();

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(mailcredential);
                mail.To.Add(mailto);
                mail.Subject = "PFA - CustomerInfo(XLS)";
                mail.Body = "PFA - CustomerInfo(XLS)";

                System.Net.Mail.Attachment attachment;
                //attachment = new System.Net.Mail.Attachment(Server.MapPath("MailFiles/CustomerInformation/" + Session.SessionID +s+ ".xls"));
                attachment = new System.Net.Mail.Attachment(Server.MapPath(filepath + filename + ".xlsx"));
                mail.Attachments.Add(attachment);

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(mailcredential, mailpassword);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                lblerror.Text = "Mail sent successfully.";
                //MessageBox.Show("mail Send");

            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
               // lblerror.Text = errormsg;//ex.ToString();
            }*/
            #endregion
            try
            {
                Response.ClearContent();
                Response.Buffer = true;
                string filename = "TotalWeight_" + DateTime.Now.ToString("dd-MM-yyyy");
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", filename + ".xls"));
                Response.ContentType = "application/ms-excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gvWeight.AllowPaging = false;
                gvWeight.Visible = true;
                //Change the Header Row back to white color
                gvWeight.HeaderRow.Style.Add("background-color", "#FFFFFF");
                gvWeight.HeaderRow.Style.Add("fore-color", "#000000");
                //Applying stlye to gridview header cells
                for (int i = 0; i < gvWeight.HeaderRow.Cells.Count; i++)
                {
                    gvWeight.HeaderRow.Cells[i].Style.Add("background-color", "#FFFFFF");
                    gvWeight.HeaderRow.Cells[i].Style.Add("color", "#000000");
                }
                int j = 1;
                //This loop is used to apply stlye to cells based on particular row
                foreach (GridViewRow gvrow in gvWeight.Rows)
                {
                    //gvrow.BackColor = Color.WHITE.ToString;
                    //if (j <= GVOrderDetails2.Rows.Count)
                    //{
                    //if (j % 2 != 0)
                    //{
                    for (int k = 0; k < gvrow.Cells.Count; k++)
                    {
                        gvrow.Cells[k].Style.Add("background-color", "#FFFFFF");
                        gvrow.Cells[k].Style.Add("color", "#000000");//ItemStyle-ForeColor
                    }
                    //}
                    //}
                    //j++;
                }
                for (int k = 0; k < gvWeight.Columns.Count; k++)
                {
                    gvWeight.FooterRow.Cells[k].Style.Add("background-color", "#FFFFFF");
                    gvWeight.FooterRow.Cells[k].Style.Add("color", "#000000");
                }
                System.Web.UI.HtmlControls.HtmlForm f = new System.Web.UI.HtmlControls.HtmlForm();
                //GVOrderDetails2.EnableTheming = false;
                f.Controls.Add(gvWeight);
                //GVOrderDetails2.DataBind();

                gvWeight.RenderControl(htw);
                Response.Write(sw.ToString());
               // Response.End();
                Response.Flush();
                Response.SuppressContent = true;
                //GVOrderDetails2.EnableTheming = true;
            }
            catch (Exception ex)
            {
            }
        }
        public DataSet GetDSetwithFont()
        {
            DataSet DSFont;
            string conn = System.Configuration.ConfigurationManager.AppSettings["MysqlConnection"].ToString();
            string queryString = @"SELECT catalog_product_entity_varchar.entity_id AS product_id, catalog_product_entity_varchar.`value` AS product_name, catalog_product_entity.sku
                            FROM catalog_product_entity_varchar INNER JOIN catalog_product_entity ON catalog_product_entity_varchar.entity_id = catalog_product_entity.entity_id
                            WHERE catalog_product_entity_varchar.entity_type_id = (SELECT entity_type_id FROM eav_entity_type
                            WHERE entity_type_code = 'catalog_product' ) AND attribute_id = (SELECT attribute_id FROM eav_attribute WHERE attribute_code = 'name'
                            AND entity_type_id = (SELECT entity_type_id FROM eav_entity_type WHERE entity_type_code = 'catalog_product' ) )
                            ORDER BY catalog_product_entity_varchar.entity_id";
           
                MySqlConnection con = new MySqlConnection(conn);
                con.Open();

                MySqlDataAdapter adapterFont = new MySqlDataAdapter(queryString, conn);

                 DSFont = new DataSet();
                adapterFont.Fill(DSFont, "Font");
            
            return DSFont;
                    
        }
        public void ExportDatatabletoExcel()
        {

            string sqlConn = System.Configuration.ConfigurationManager.AppSettings["SqlConnectionString"].ToString();



            string queryString = @"exec sp_ExportPQExcel";

            DataTable Tbl = new DataTable();



            SqlDataAdapter daSQL = new SqlDataAdapter(queryString, sqlConn);

            daSQL.Fill(Tbl);



            string ExcelFilePath = System.Configuration.ConfigurationManager.AppSettings["FilePath"].ToString();

            string filename = "TotalWeight" + DateTime.Now.ToString("dd-MM-yyyy");

            //  string ExcelFilePath="E:\\FFHP\\MailFiles";

            gvWeight.Visible = true;



            if (Tbl == null || Tbl.Columns.Count == 0)

                throw new Exception("ExportToExcel: Null or empty input table!\n");



            // load excel, and create a new workbook

            Excel.Application excelApp = new Excel.Application();

            excelApp.Workbooks.Add();



            // single worksheet

            //Excel._Worksheet workSheet = excelApp.ActiveSheet;
            Excel._Worksheet workSheet = (Excel.Worksheet)excelApp.ActiveSheet;
            Excel.Range xlRange = workSheet.UsedRange; //mahi





            // column headings

            for (int i = 0; i < Tbl.Columns.Count; i++)
            {

                workSheet.Cells[1, (i + 1)] = Tbl.Columns[i].ColumnName;

            }



            // rows

            for (int i = 0; i < Tbl.Rows.Count; i++)
            {

                // to do: format datetime values before printing

                for (int j = 0; j < Tbl.Columns.Count; j++)
                {

                    workSheet.Cells[(i + 2), (j + 1)] = Tbl.Rows[i][j];

                }

            }



            //mahi

            int rowCount = xlRange.Rows.Count;//mahi

            int colCount = xlRange.Columns.Count;//mahi

            for (int i = 1; i <= rowCount; i++)
            {

                for (int j = 1; j <= colCount; j++)
                {

                    // me.Show(xlRange.Cells[i, j].Value2.ToString());

                }

            }//mahi



            // check filepath

            if (ExcelFilePath != null && ExcelFilePath != "")
            {

                try
                {

                    //  workSheet.SaveAs(ExcelFilePath);

                    workSheet.SaveAs(Server.MapPath(ExcelFilePath + filename + ".xlsx"));

                    excelApp.Quit();

                    // MessageBox.Show("Excel file saved!");

                }

                catch (Exception ex)
                {

                    throw new Exception("ExportToExcel: Excel file could not be saved! Check filepath.\n"

                        + ex.Message);

                }

            }

            else    // no filepath is given
            {

                excelApp.Visible = true;

            }





        }

        public void ExportToPdf(DataSet ds)
        {
            Document document = new Document();
            DataTable dt = ds.Tables[0];
            string pdfFilePath = System.Configuration.ConfigurationManager.AppSettings["FilePath"].ToString();
            string filename = "TotalWeightpdf_" + DateTime.Now.ToString("dd-MM-yyyy");
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(Server.MapPath(pdfFilePath + filename + ".pdf"), FileMode.Create));
            document.Open();
            iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 7);

            PdfPTable table = new PdfPTable(dt.Columns.Count);
            PdfPRow row = null;
            float[] widths = new float[] { 4f, 4f, 4f, 4f, 4f,4f };

            table.SetWidths(widths);

            table.WidthPercentage = 100;

            PdfPCell cell = new PdfPCell(new Phrase("Products"));

            cell.Colspan = dt.Columns.Count;

            foreach (DataColumn c in dt.Columns)
            {

                table.AddCell(new Phrase(c.ColumnName, font5));
            }

            foreach (DataRow r in dt.Rows)
            {
                if (dt.Rows.Count > 0)
                {
                    table.AddCell(new Phrase(r[0].ToString(), font5));
                    table.AddCell(new Phrase(r[1].ToString(), font5));
                    table.AddCell(new Phrase(r[2].ToString(), font5));
                    table.AddCell(new Phrase(r[3].ToString(), font5));
                    table.AddCell(new Phrase(r[4].ToString(), font5));
                    table.AddCell(new Phrase(r[5].ToString(), font5));
                }
            } document.Add(table);
            document.Close();
            /*
            string sqlConn = System.Configuration.ConfigurationManager.AppSettings["SqlConnectionString"].ToString();


            string queryString = @"exec sp_ExportPQExcel";

            DataTable Tbl = new DataTable();

            SqlDataAdapter daSQL = new SqlDataAdapter(queryString, sqlConn);

            daSQL.Fill(Tbl);
            Document document = new Document();
            string pdfFilePath = System.Configuration.ConfigurationManager.AppSettings["FilePath"].ToString();
            string filename = "TotalWeightpdf" + DateTime.Now.ToString("dd-MM-yyyy");
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(Server.MapPath(pdfFilePath + filename + ".pdf"), FileMode.Create));
            document.Open();
            iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 7);

           // PdfPTable table = new PdfPTable(dt.Columns.Count);
            PdfPTable table = new PdfPTable(Tbl.Columns.Count);
            PdfPRow row = null;
         //   float[] widths = new float[] { 4f, 4f, 4f, 4f, 4f };
            float[] widths = new float[] { 4f, 4f, 4f, 4f };

            table.SetWidths(widths);

            table.WidthPercentage = 100;

            PdfPCell cell = new PdfPCell(new Phrase("Products"));

           // cell.Colspan = dt.Columns.Count;
            cell.Colspan = Tbl.Columns.Count;

            //foreach (DataColumn c in dt.Columns)
            foreach (DataColumn c in Tbl.Columns)
            {

                table.AddCell(new Phrase(c.ColumnName, font5));
            }

           // foreach (DataRow r in dt.Rows)
            foreach (DataRow r in Tbl.Rows)
            {
               // if (dt.Rows.Count > 0)
                if (Tbl.Rows.Count > 0)
                {
                    table.AddCell(new Phrase(r[0].ToString(), font5));
                    table.AddCell(new Phrase(r[1].ToString(), font5));
                    table.AddCell(new Phrase(r[2].ToString(), font5));
                    table.AddCell(new Phrase(r[3].ToString(), font5));

                    table.AddCell(new Phrase(r[4].ToString(), font5));
                }
            } document.Add(table);
            document.Close();*/
        }
        public string sendsms(DataTable dt, string mobilenumber)
        {
            WebClient client = new WebClient();
            string message = "";

            string _username = System.Configuration.ConfigurationManager.AppSettings["username"].ToString();
            string _password = System.Configuration.ConfigurationManager.AppSettings["password"].ToString();
            string _senderid = System.Configuration.ConfigurationManager.AppSettings["senderid"].ToString();
            DataTable dtSms = new DataTable();


            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                    message = string.Concat(message) + dt.Rows[i][j].ToString();

            }

            string baseurl = System.Configuration.ConfigurationManager.AppSettings["smslink"].ToString();
            string apiurl = baseurl + "username=" + _username + "&password=" + _password + "&sendername=" + _senderid + "&mobileno=" + mobilenumber + "&message=" + message;//Authentication Fail:UserName or Password is incorrect.

            Stream data = client.OpenRead(apiurl);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd().Trim();
            data.Close();
            reader.Close();
            // "Your message is successfully sent";
            return s;
        }
    }
}

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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using Excel = Microsoft.Office.Interop.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;

namespace WebApplication2
{
    public partial class default_dup : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            wsFFHP.ffhpservice ws = new wsFFHP.ffhpservice();
            string sqlConn = System.Configuration.ConfigurationManager.AppSettings["SqlConnectionString"].ToString();


            string queryString = @"SELECT distinct ew.Name ,mw.parent_product_id,sum(PurchaseWeight) as TotalPurchaseWeight,ew.Units,mw.Units as orderedunit,pcw.OnePCWeight

            FROM 
             (
			SELECT distinct p.parent_product_id,pr.Units,p.Product_Id,
			
			 CASE WHEN p.Units='KG' 
		THEN 
			(CAST(pr.TotalWeight as DECIMAL(9,2))*p.Extra_Wt + pr.TotalWeight)-ISNULL((balancescannedweight-balancetrayweight),0)
		WHEN p.Units='PC'
		THEN  (CAST(pr.TotalWeight as DECIMAL(9,2)) + p.Extra_Wt)-ISNULL(balancepiececount,0)
		ELSE 0
		END as PurchaseWeight

					from [dbo].[testtotalweight] pr 
											   LEFT OUTER JOIN Products_ExtraWeights p 
													ON pr.product_Id=p.Product_Id 
												LEFT OUTER JOIN (select productid,balancescannedweight,balancetrayweight,balancepiececount,stockdate from stockproducts_history where stockdate='2016-06-01 00:00:00.000' ) as sh ON pr.Product_Id=sh.productid 
                                   				) as 
												mw LEFT OUTER JOIN Products_ExtraWeights ew
													ON mw.Parent_product_Id =ew.Product_id
												LEFT OUTER JOIN ffhp_PCWeight pcw
													ON mw.Product_Id = pcw.Product_id
												
													 group by ew.Name ,mw.parent_product_id,ew.Units,mw.Units,pcw.OnePCWeight";

            string vendordetails = @"SELECT vp.productid,ISNULL(vd.vendorid,0) as vendorid,ISNULL(vd.vendorname,'-') as vendorname from 
                                   
                                     vendor_products vp 
                                   
                                    LEFT OUTER JOIN vendordetails vd ON vp.vendorid=vd.vendorid";

            DataTable dtWeight = new DataTable();
           
            DataTable dtVendor = new DataTable();
           
            SqlDataAdapter daSQL = new SqlDataAdapter(queryString, sqlConn);
            SqlDataAdapter daVendor = new SqlDataAdapter(vendordetails, sqlConn);

            daSQL.Fill(dtWeight);
            daVendor.Fill(dtVendor);

            // dtWeight = ws.GetCalculateWeightNew();
            //to test start
          //  string teststring = @"select * from testtotalweight";
          //  SqlDataAdapter daTest = new SqlDataAdapter(teststring, sqlConn);
          //  daTest.Fill(dtWeight);

            GridView1.DataSource = dtWeight;
            GridView1.DataBind();
            //  ExportDatatabletoExcel(dtWeight);
            weightCalculate(dtWeight, dtVendor);
            // sendsms("from asp.net", "8754543655");

        }
        public void weightCalculate(DataTable dtWeight, DataTable dtVendor)
        {

            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("Product_Id", typeof(int));
            dtResult.Columns.Add("Name", typeof(string));
            dtResult.Columns.Add("TotalWeight", typeof(double));
            dtResult.Columns.Add("Extra_Wt", typeof(decimal));
            dtResult.Columns.Add("Units", typeof(string));
            dtResult.Columns.Add("PurchaseWeight", typeof(double));

            dtResult.Columns.Add("vendorid", typeof(int));
            dtResult.Columns.Add("vendorname", typeof(string));
            dtResult.Columns.Add("created_at", typeof(DateTime));
            dtResult.Columns.Add("updated_at", typeof(DateTime));

            var items = (from p in dtWeight.AsEnumerable()
                         join t in dtVendor.AsEnumerable()
                            on Convert.ToInt32(p.Field<string>("Product_Id")) equals Convert.ToInt32(t.Field<string>("productid"))

                         //  join q in dtClosingStack.AsEnumerable()
                         //     on Convert.ToInt32(p.Field<string>("Product_Id")) equals Convert.ToInt32(q.Field<int>("productid")) into dtAll
                         //       from r in dtAll.DefaultIfEmpty()
                         orderby t.Field<int>("vendorid") ascending
                         select dtResult.LoadDataRow(new object[]
                                    {
                                     Convert.ToInt32(p.Field<string>("Product_Id")),
                                    p.Field<string>("Name"),
                                    Convert.ToDouble(p.Field<string>("TotalPurchaseWeight")),
                                    Convert.ToDouble(p.Field<double>("TotalPurchaseWeight")),
                                    p.Field<string>("orderedunit"),
                                  // (p.Field<string>("Units")=="KG") ? Convert.ToDouble(p.Field<string>("TotalWeight")) + (Convert.ToDouble(p.Field<string>("TotalWeight")) * Convert.ToDouble(t.Field<Double>("Extra_Wt"))) - Convert.ToDouble(r.Field<decimal>("closingstockwt")) :Convert.ToDouble(p.Field<string>("TotalWeight")) + (Convert.ToDouble(p.Field<string>("TotalWeight")) * Convert.ToDouble(t.Field<Double>("Extra_Wt"))) - Convert.ToDouble(r.Field<decimal>("closingstockpc")),
                                  // (t.Field<string>("unit")=="KG") ? Convert.ToDouble(p.Field<string>("TotalWeight")) + (Convert.ToDouble(p.Field<string>("TotalWeight")) * Convert.ToDouble(t.Field<Double>("Extra_Wt")))- Convert.ToDouble(t.Field<decimal>("closingstock")) : Convert.ToDouble(p.Field<string>("TotalWeight"))+Convert.ToDouble(t.Field<Double>("Extra_Wt")) -Convert.ToDouble(t.Field<decimal>("closingstock"))  ,
                                    Convert.ToDouble(p.Field<double>("TotalPurchaseWeight")),
                                     t.Field<int>("vendorid") ,
                                    t.Field<string>("vendorname") ,
                                   // Convert.ToDouble(p.Field<string>("TotalWeight")) + (Convert.ToDouble(p.Field<string>("TotalWeight")) * Convert.ToDouble(t.Field<Double>("Extra_Wt"))),
                                    System.DateTime.Now,
                                    System.DateTime.Now,
                                    }, false));

            /* var items = (from p in dtWeight.AsEnumerable()
                          join t in dtExtraWt.AsEnumerable()
                             on Convert.ToInt32(p.Field<string>("Product_Id")) equals Convert.ToInt32(t.Field<string>("productid"))
                          join q in dtClosingStack.AsEnumerable()
                              on Convert.ToInt32(p.Field<string>("Product_Id")) equals Convert.ToInt32(q.Field<int>("productid"))
                                orderby t.Field<int>("vendorid") ascending 
                                  
                          select dtResult.LoadDataRow(new object[]
                            {
                             Convert.ToInt32(p.Field<string>("Product_Id")),
                            p.Field<string>("Name"),
                            Convert.ToDouble(p.Field<string>("TotalWeight")),
                            Convert.ToDouble(t.Field<double>("Extra_Wt")),
                            t.Field<string>("unit"),
                            (p.Field<string>("Units")=="Kg") ? Convert.ToDouble(p.Field<string>("TotalWeight")) + (Convert.ToDouble(p.Field<string>("TotalWeight")) * Convert.ToDouble(t.Field<Double>("Extra_Wt"))) - Convert.ToDouble(q.Field<decimal>("closingstockwt")) :Convert.ToDouble(p.Field<string>("TotalWeight")) + (Convert.ToDouble(p.Field<string>("TotalWeight")) * Convert.ToDouble(t.Field<Double>("Extra_Wt"))) - Convert.ToDouble(q.Field<decimal>("closingstockpc")),
                                    
                                    
                             t.Field<int>("vendorid") ,
                            t.Field<string>("vendorname") ,
                           // Convert.ToDouble(p.Field<string>("TotalWeight")) + (Convert.ToDouble(p.Field<string>("TotalWeight")) * Convert.ToDouble(t.Field<Double>("Extra_Wt"))),
                            System.DateTime.Now,
                            System.DateTime.Now,
                            }, false));*/
            DataTable dt = items.CopyToDataTable();
            copyDatatabletoDB(dt);
            gvWeight.DataSource = dt;
            gvWeight.DataBind();
            ExportDatatabletoExcel(dt);

            ExportToPdf(dt);
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
            string filename = "Purchaseweight" + DateTime.Now.ToString("dd-MM-yyyy");
            //  string ExcelFilePath="E:\\FFHP\\MailFiles";
            gvWeight.Visible = true;

            if (Tbl == null || Tbl.Columns.Count == 0)
                throw new Exception("ExportToExcel: Null or empty input table!\n");

            // load excel, and create a new workbook
            Excel.Application excelApp = new Excel.Application();
            excelApp.Workbooks.Add();

            // single worksheet
            Excel._Worksheet workSheet = excelApp.ActiveSheet;

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
        public void ExportGridtoExcel()
        {
            try
            {


                string filepath = System.Configuration.ConfigurationManager.AppSettings["FilePath"].ToString();

                gvWeight.Visible = true;
                string filename = "TotalWeight" + DateTime.Now.ToString("dd-MM-yyyy");
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
                lblerror.Text = "mail not sent";//ex.ToString();
            }
        }

        public void ExportToPdf(DataTable dt)
        {
            Document document = new Document();
            string pdfFilePath = System.Configuration.ConfigurationManager.AppSettings["FilePath"].ToString();
            string filename = "TotalWeightpdf" + DateTime.Now.ToString("dd-MM-yyyy");
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(Server.MapPath(pdfFilePath + filename + ".pdf"), FileMode.Create));
            document.Open();
            iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 5);

            PdfPTable table = new PdfPTable(dt.Columns.Count);
            PdfPRow row = null;
            float[] widths = new float[] { 4f, 4f, 4f, 4f, 4f, 4f, 4f, 4f, 4f, 4f };

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
                }
            } document.Add(table);
            document.Close();
        }
        public string sendsms(DataTable dt, string mobilenumber)
        {
            WebClient client = new WebClient();
            string message = "";
            //string baseurl = "http://bulksms.mysmsmantra.com:8080/WebSMS/SMSAPI.jsp?username=username&password=password&sendername=sender id&mobileno=919999999999&message=Hello";//Authentication Fail:UserName or Password is incorrect.
            //string baseurl = "http://bulksms.mysmsmantra.com:8080/WebSMS/balance.jsp?username=ffhp&password=169639334";
            //string baseurl = "http://bulksms.mysmsmantra.com:8080/WebSMS/SMSAPI.jsp?username=demouser&password=763475132&sendername=dm&mobileno=918680939328&message=Hello Binarch Test";//DND//Your message is successfully sent to:919999999999
            //string baseurl = "http://bulksms.mysmsmantra.com:8080/WebSMS/sentreport.jsp?username=demouser&password=763475132&fromdate=01-12-2012&todate=30-12-2012";

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
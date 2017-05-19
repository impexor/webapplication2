using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using MySql.Data.MySqlClient;
using Excel = Microsoft.Office.Interop.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.html.simpleparser;
using System.Globalization;

namespace WebApplication2
{
    public partial class HearAbtUs_Report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnsubmit_OnClick(object sender, EventArgs e)
        {
            DateTime dtf = DateTime.ParseExact(TbxFromDate.Text, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            DateTime dtt = DateTime.ParseExact(TbxToDate.Text, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            string MysqlConn = System.Configuration.ConfigurationManager.AppSettings["MysqlConnection"].ToString();
            string queryString = @"SELECT c.email, c.group_id, c.created_at, CONCAT( IFNULL( fn.value, '' ) ,  ' ', IFNULL( ln.value, '' ) ) name, ph.value AS telephone, hda.value
FROM customer_entity AS c
JOIN customer_entity_varchar fn ON c.entity_id = fn.entity_id
AND fn.attribute_id =5
JOIN customer_entity_varchar ln ON c.entity_id = ln.entity_id
AND ln.attribute_id =7
LEFT OUTER JOIN customer_address_entity_varchar ph ON c.entity_id = ph.entity_id
AND ph.attribute_id =31
LEFT OUTER JOIN customer_entity_int AS hd ON hd.entity_id = c.entity_id
AND hd.attribute_id =145
LEFT OUTER JOIN eav_attribute_option_value AS hda ON hd.value = hda.option_id
WHERE created_at between '" + dtf.ToString("yyyy-MM-dd") + "' and '" + dtt.AddDays(1).ToString("yyyy-MM-dd") + "'";
            DataTable dtReport = new DataTable();


            MySqlDataAdapter daMySQL = new MySqlDataAdapter(queryString, MysqlConn);


            daMySQL.Fill(dtReport);

            gvReport.DataSource = dtReport;
            gvReport.DataBind();
            Session.Add("dt", dtReport);
           
        }

        public void ExportDatatabletoExcel(DataTable Tbl)
        {
            try
            {
                Response.ClearContent();
                Response.Buffer = true;
                string filename = "HowYou_HearAboutUs_" + DateTime.Now.ToString("dd-MM-yyyy");
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", filename));
                Response.ContentType = "application/ms-excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gvReport.AllowPaging = false;

                //Change the Header Row back to white color
                gvReport.HeaderRow.Style.Add("background-color", "#FFFFFF");
                gvReport.HeaderRow.Style.Add("fore-color", "#000000");
                //Applying stlye to gridview header cells
                for (int i = 0; i < gvReport.HeaderRow.Cells.Count; i++)
                {
                    gvReport.HeaderRow.Cells[i].Style.Add("background-color", "#FFFFFF");
                    gvReport.HeaderRow.Cells[i].Style.Add("color", "#000000");
                }
                int j = 1;
                //This loop is used to apply stlye to cells based on particular row
                foreach (GridViewRow gvrow in gvReport.Rows)
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
                for (int k = 0; k < gvReport.Columns.Count; k++)
                {
                    gvReport.FooterRow.Cells[k].Style.Add("background-color", "#FFFFFF");
                    gvReport.FooterRow.Cells[k].Style.Add("color", "#000000");
                }
                System.Web.UI.HtmlControls.HtmlForm f = new System.Web.UI.HtmlControls.HtmlForm();
                //GVOrderDetails2.EnableTheming = false;
                f.Controls.Add(gvReport);
                //GVOrderDetails2.DataBind();

                gvReport.RenderControl(htw);
                Response.Write(sw.ToString());
                Response.End();
                //GVOrderDetails2.EnableTheming = true;
            }
            catch (Exception ex)
            {
            }

            #region direct_export
           /* string ExcelFilePath = System.Configuration.ConfigurationManager.AppSettings["FilePath"].ToString();
            string filename = "HowYouHearAboutUs" + DateTime.Now.ToString("dd-MM-yyyy");
            //  string ExcelFilePath="E:\\FFHP\\MailFiles";
           // gvReport.Visible = true;

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
            }*/

            #endregion
        }
        public void ExportToPdf(DataTable dt)
        {
            Document document = new Document();
            string pdfFilePath = System.Configuration.ConfigurationManager.AppSettings["FilePath"].ToString();
            string filename = "HowYouHearAboutUs" + DateTime.Now.ToString("dd-MM-yyyy");
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(Server.MapPath(pdfFilePath + filename + ".pdf"), FileMode.Create));
            document.Open();
            iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 5);

            PdfPTable table = new PdfPTable(dt.Columns.Count);
            PdfPRow row = null;
            float[] widths = new float[] { 4f, 4f, 4f, 4f, 4f, 4f};

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
        }
        public void RenderPDF(DataTable ExDataTable)
        {
            Document pdfDoc = new Document(PageSize.A4, 10, 10, 10, 10);

            try
            {
                PdfWriter.GetInstance(pdfDoc, System.Web.HttpContext.Current.Response.OutputStream);
                pdfDoc.Open();

                //Set Font Properties for PDF File
                Font fnt = FontFactory.GetFont("Times New Roman", 12);
                DataTable dt = ExDataTable;

                if (dt != null)
                {

                    PdfPTable PdfTable = new PdfPTable(dt.Columns.Count);
                    PdfPCell PdfPCell = null;

                    //Here we create PDF file tables

                    for (int rows = 0; rows < dt.Rows.Count; rows++)
                    {
                        if (rows == 0)
                        {
                            for (int column = 0; column < dt.Columns.Count; column++)
                            {
                                PdfPCell = new PdfPCell(new Phrase(new Chunk(dt.Columns[column].ColumnName.ToString(), fnt)));
                                PdfTable.AddCell(PdfPCell);
                            }
                        }
                        for (int column = 0; column < dt.Columns.Count; column++)
                        {
                            PdfPCell = new PdfPCell(new Phrase(new Chunk(dt.Rows[rows][column].ToString(), fnt)));
                            PdfTable.AddCell(PdfPCell);
                        }
                    }

                    // Finally Add pdf table to the document 
                    pdfDoc.Add(PdfTable);
                }

                pdfDoc.Close();

                Response.ContentType = "application/pdf";

                //Set default file Name as current datetime
                Response.AddHeader("content-disposition", "attachment; filename=" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");

                System.Web.HttpContext.Current.Response.Write(pdfDoc);

                Response.Flush();
                Response.End();

            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            DataTable dtReport = (DataTable)Session["dt"];
            ExportDatatabletoExcel(dtReport);
        }

        protected void btnExportPdf_Click(object sender, EventArgs e)
        {
            DataTable dtReport = (DataTable)Session["dt"];
            RenderPDF(dtReport);
            //ExportToPdf(dtReport);
        }
    }
}
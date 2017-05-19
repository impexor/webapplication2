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
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace WebApplication2
{
    public partial class Freegift : System.Web.UI.Page
    {
        string MysqlConn = System.Configuration.ConfigurationManager.AppSettings["MysqlConnection"].ToString();

        MySqlCommand cmd = new MySqlCommand();
        string sqlConn = System.Configuration.ConfigurationManager.AppSettings["SqlConnectionString"].ToString();

        protected void Page_Load(object sender, EventArgs e)
        {

            MySqlConnection cn = new MySqlConnection(MysqlConn);

            cmd.Connection = cn;

            string queryString = @"SELECT * from 1_giftclaim where total_order_amount=0 and Claim_status='Not claimed' ";
            DataTable dtGift = new DataTable();
            DataTable dtGiftUpdated = new DataTable();
            MySqlDataAdapter daMySQL = new MySqlDataAdapter(queryString, MysqlConn);

            daMySQL.Fill(dtGift);

            int intMintotal = 0, intMaxtotal = 0;
            int intDatespan = 0;

            updateTotalClaimAmount(dtGift);



            //string strQuery = @"SELECT  customer_email, max(created_at), min(created_at), sum(grand_total) ,datediff(max(created_at), min(created_at)) FROM `sales_flat_order` where increment_id in(" + commaseperatedorders + " ) group by customer_email";
            // daMySQL = new MySqlDataAdapter(strQuery, MysqlConn);
            // DataTable dtCalTable = new DataTable();
            // daMySQL.Fill(dtCalTable);

            // //sql table.

            // string strSQLQuery = @"select * from Freegift_Form_mapping";

            // DataTable dtMaster = new DataTable();


            // SqlDataAdapter daSQL = new SqlDataAdapter(strSQLQuery, sqlConn);
            // daSQL.Fill(dtMaster);

            // DataTable dtEligible = new DataTable();
            // dtEligible.Columns.Add("formid", typeof(int));
            // dtEligible.Columns.Add("customer_email", typeof(string));
            // dtEligible.Columns.Add("Total", typeof(int));
            // dtEligible.Columns.Add("orderlist", typeof(string));
            // dtEligible.Columns.Add("Eligible", typeof(string));


            // var result = from dataRows1 in dtCalTable.AsEnumerable()
            //              join dataRows2 in dtMaster.AsEnumerable()
            //              on dataRows1.Field<int>("formid") equals dataRows2.Field<int>("formid")


            //              select dtEligible.LoadDataRow(new object[]
            //  {
            //     dataRows2.Field<int>("formid"),
            //     dataRows1.Field<string>("customer_email"),
            //     dataRows1.Field<int>("Total"),
            //     dataRows1.Field<string>("orderlist"),


            //   }, false);
            // result.CopyToDataTable();

            // //to bind to grid
            // daMySQL = new MySqlDataAdapter(queryString, MysqlConn);

            //  daMySQL.Fill(dtGiftUpdated);
            //  gvGift.DataSource = dtGiftUpdated;
            //  gvGift.DataBind();
        }
        public void updateTotalClaimAmount(DataTable dtGift)
        {
            MySqlConnection cn = new MySqlConnection(MysqlConn);
            cmd.Connection = cn;
            int intOrderCount = 0;
            int[] ordernumbers;
            string email = "";
            string commaseperatedorders = "";
            string updatequery = "";
            string strStatus = "";
            if (dtGift.Rows.Count > 0) //update the order total value in the mysql table.
            {
                cn.Open();
                for (int i = 0; i < dtGift.Rows.Count; i++)
                {
                    commaseperatedorders = dtGift.Rows[i]["Claimed_orderIds"].ToString();

                    ordernumbers = Array.ConvertAll<string, int>(commaseperatedorders.Split(','), Convert.ToInt32);
                    intOrderCount = validateOrderIds(commaseperatedorders);
                    if (intOrderCount > 0)
                    {
                        strStatus = "Not Eligible";
                        email = dtGift.Rows[i]["Customer_email"].ToString();
                        updatequery = @"update 1_giftclaim set Claim_status =strStatus, total_order_amount =(SELECT sum(grand_total) FROM  sales_flat_order o where
                                           o.increment_id in(" + commaseperatedorders + ") and Customer_email='" + email + "' )  ";
                        cmd = new MySqlCommand(updatequery, cn);
                        //cmd.CommandText = "update 1_giftclaim set total_order_amount =(SELECT sum(grand_total) FROM  sales_flat_order o where  o.increment_id in(ordernumbers) ) where Customer_email=email";
                        int numRowsUpdated = cmd.ExecuteNonQuery();
                    }
                    else
                    {

                    }
                }

            }
        }
        public int validateOrderIds(string commaseperatedorders)
        {
            SqlConnection conn = new SqlConnection(sqlConn);
            conn.Open();
            int rowsAffected;


            using (SqlConnection con = new SqlConnection(sqlConn))
            {
                using (SqlCommand cmd = new SqlCommand("select count(*) from Freegift_Orders where OrderId in (" + commaseperatedorders + ")", con))
                {
                    cmd.CommandType = CommandType.Text;

                    con.Open();
                     rowsAffected = cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return rowsAffected;

        }
    }
}
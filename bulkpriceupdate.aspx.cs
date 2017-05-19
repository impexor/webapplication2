using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication2
{
    public partial class bulkpriceupdate : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
           // Response.Redirect ("http://staging2.ffhp.in/update_prices.php");
            Response.Write("<script>window.open('http://staging2.ffhp.in/update_prices.php','_blank');</script>");
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
           // Server.Transfer("http://staging2.ffhp.in/update_prices.php");
        }
    }
}
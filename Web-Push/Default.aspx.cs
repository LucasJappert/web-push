using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

namespace Web_Push
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //string _asd = Datos.Datos.GuardarLog_NotificacionesPush("Page_Load", "", "Lucas", Global._CadenaConexionAutomatica);

                Response.Clear();
                Response.ContentEncoding = Encoding.UTF8;
                Response.ContentType = "application/json";
                Response.StatusCode = 200;
                Response.Write("OK");
                Response.End();
            }
            catch { }
        }
    }
}
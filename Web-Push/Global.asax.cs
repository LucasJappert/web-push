using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Web_Push
{
    using MEGA.Web;
    using Web_Push.Modelos;

    public class Global : System.Web.HttpApplication
    {
        //public static string _CadenaConexionAutomatica = "MegatoneWebDesarrollo";
        public static string _CadenaConexionAutomatica = "Default";

        /// <summary>
        /// NO GENERAR NUEVAS CLAVES PÚBLICAS!!!!
        /// </summary>
        public static string _MailTo = "mailto:lucasnicolasjappert@gmail.com";
        public static string _ClavePublica = "BAs9NMqSXKx8gzTzm6yf1m_Ibim8y5iR6hMkZqKOS-ibar7ZEQ7n9AUB-W9bPN2rf3_MzEsV_CYXVhfE-ESGSts";
        public static string _ClavePrivada = "48_-GG285qXnpX3StAVn1XMcx005DSRy-ruqM55ANqg";

        /// <summary>
        /// La key es el SessionID. Cada valor es del tipo ClasesUtiles.NotificacionPushUsuario.
        /// </summary>
        public static Hashtable _HashInfoNotificacionesPushUsuarios = new Hashtable();
        /// <summary>
        /// Contiene todas las Keys de SessionIDs que se encuentran en el _HashInfoNotificacionesPushUsuarios.
        /// </summary>
        public static List<string> _SessionIDsUsuariosPush = new List<string>();
        public static int _MinutosExpiracionCookiePush = 60 * 24 * 7;///7 dias

        public static bool _ThreadVariablesGlobalesCorriendo = false;

        public static void Main()
        {
            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-AR");
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-AR");

            ////VapidDetails vapidKeys = VapidHelper.GenerateVapidKeys();
            ////_ClavePublica = vapidKeys.PublicKey;
            ////_ClavePrivada = vapidKeys.PrivateKey;

            //Thread _ThreadVariablesGlobales = new Thread(Auxiliares.LoopActualizarVariablesGlobales);
            //_ThreadVariablesGlobales.Start();


            //Datos.Datos.GuardarLog_NotificacionesPush("Application_Start", "", "Lucas", _CadenaConexionAutomatica);
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            try
            {
                
                //HerramientasWEB.GuardarEnTXT(new string[] { DateTime.Now.ToString(), Auxiliares._NombreMiHost}, "ReporteWebPush", "asd", Global._CadenaConexionAutomatica);

                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-AR");
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-AR");

                //VapidDetails vapidKeys = VapidHelper.GenerateVapidKeys();
                //_ClavePublica = vapidKeys.PublicKey;
                //_ClavePrivada = vapidKeys.PrivateKey;

                Datos.Datos.GuardarLog_NotificacionesPush("Application_Start", "", "Lucas",  _CadenaConexionAutomatica);

                Thread _ThreadVariablesGlobales = new Thread(Auxiliares.LoopActualizarVariablesGlobales);
                _ThreadVariablesGlobales.Start();

                
            }
            catch (Exception ex) { }
        }

        void Application_End(object sender, EventArgs e)
        {
            try
            {
                _ThreadVariablesGlobalesCorriendo = false;
                Datos.Datos.GuardarLog_NotificacionesPush("Application_End", "", "Lucas", _CadenaConexionAutomatica);
            }
            catch (Exception ex) {}
        }

        void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex is ThreadAbortException)
                return;
            Exception ex1 = Server.GetLastError().GetBaseException();
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex1, true);

            Datos.Datos.GuardarLog_NotificacionesPush("Application_Error", "Excepción: " + ex.Message + " *** Error en: " + trace.ToString(), "Lucas", _CadenaConexionAutomatica);
            
        }
    }
}
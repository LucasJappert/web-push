using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Runtime.Serialization.Json;

namespace Web_Push.Modelos
{
    public class ClasesVarias
    {
        public class InfoNotificacionPushUsuario
        {
            public int Id { get; set; } = 0;
            public string SessionID { get; set; } = "";
            public string JsonSuscripcion { get; set; } = "";
            public DateTime FechaExpira { get; set; } = DateTime.Now;

            public JSonSuscripcion ObtenerJSonSuscripcion()
            {
                JSonSuscripcion _Retorno = new JSonSuscripcion();
                try
                {

                    DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(JSonSuscripcion));
                    MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(JsonSuscripcion));
                    JSonSuscripcion _JSonSuscripcion = (JSonSuscripcion)js.ReadObject(ms);
                    _Retorno = _JSonSuscripcion;
                    _JSonSuscripcion = null;
                    ms = null;
                    js = null;
                }
                catch { }
                return _Retorno;
            }

            public InfoNotificacionPushUsuario ObtenerInstancia()
            {
                return (InfoNotificacionPushUsuario)this.MemberwiseClone();
            }
        }
        
        public class JSonSuscripcion
        {
            public string endpoint { get; set; } = "";
            public string expirationTime { get; set; } = "";
            public KeyWebPush keys { get; set; }

        }
        
        public class KeyWebPush
        {
            public string p256dh { get; set; } = "";
            public string auth { get; set; } = "";
        }

        public class InfoNotificacionPush
        {
            public int IdNotificacion { get; set; } = 0;
            public string UrlImagenIcono { get; set; } = "";
            public string Titulo { get; set; } = "";
            public string Descripcion { get; set; } = "";
            public string UrlImagenBody { get; set; } = "";
            public string Link { get; set; } = "";
            public DateTime FechaEnvio { get; set; } = DateTime.Now;

            public InfoNotificacionPush ObtenerInstancia()
            {
                return (InfoNotificacionPush)this.MemberwiseClone();
            }
        }
    }
}
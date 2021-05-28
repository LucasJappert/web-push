using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;


namespace Web_Push.Modelos
{
    using System.Data;
    using System.Reflection;
    using Web_Push.Datos;
    using WebPush;

    public class Auxiliares
    {
        private static int _SegundosBucle = 6; //5 minutos
        public static string _NombreMiHost = System.Net.Dns.GetHostName().ToLower();
        private static string _NombreHostPrimario = "";
        private static DateTime _FechaUltimaActualizacion = DateTime.Now;

        public static void LoopActualizarVariablesGlobales()
        {
            try
            {
                Global._ThreadVariablesGlobalesCorriendo = true;
                while (Global._ThreadVariablesGlobalesCorriendo == true)
                {
                    ActualizarVariablesGlobales();
                    Thread.Sleep(1000 * _SegundosBucle);//Milisegundos * segundos
                }
            }
            catch (IOException ex)
            {
                Datos.GuardarLog_NotificacionesPush(MethodBase.GetCurrentMethod().Name, ex.Message, "Lucas", Global._CadenaConexionAutomatica);
            }
        }

        private static void ActualizarVariablesGlobales()
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-AR");
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-AR");

                //Obtengos servidores bloqueados
                string _ServidoresBloqueados = Datos.ObtenerServidoresBloqueados();

                if (_ServidoresBloqueados.ToLower().Contains(_NombreMiHost.ToLower()) == false)
                {
                    //Seteo variables PRINCIPALES
                    Datos.SetearVariablesDelGlobal(ref _NombreHostPrimario, ref _FechaUltimaActualizacion);

                    //NO ARRANCABAAAAAAAAAAAAAAAAAAAAAAA - Con Ctrl + F5 agarra viaje..
                    if (_NombreHostPrimario.ToLower() == _NombreMiHost.ToLower())
                    {
                        TrabajarNotificaciones();
                    }
                    else
                    {
                        if (_FechaUltimaActualizacion.AddSeconds(_SegundosBucle * 4) < DateTime.Now)
                        {
                            Datos.UpdatearServidorPrimario(_NombreMiHost);
                            Datos.UpdatearServidorSecundario(_NombreHostPrimario);
                            TrabajarNotificaciones();
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Datos.GuardarLog_NotificacionesPush(MethodBase.GetCurrentMethod().Name, ex.Message, "Lucas", Global._CadenaConexionAutomatica);
            }
        }



        #region Auixiliares

        /// <summary>
        /// Arma el HashInfoNotificacionesPushUsuarios y envía las notificaciones.
        /// </summary>
        private static void TrabajarNotificaciones()
        {
            try
            {
                //Envía las notificaciones.
                ProcesoEnviarNotificaciones();

                //Updateo FechaUltimaActualizacion
                Datos.UpdatearKALServidorPrimario(_NombreMiHost);

            }
            catch (Exception ex)
            {
                Datos.GuardarLog_NotificacionesPush("TrabajarNotificaciones1", ex.Message, "Lucas", Global._CadenaConexionAutomatica);
            }
        }

        public static void ArmarHashInfoNotificacionesPushUsuarios(int _IdNoti)
        {
            try
            {
                DataTable _DT = Datos.ObtenerInfoNotificacionesPushUsuarios(_IdNoti);

                ClasesVarias.InfoNotificacionPushUsuario _NotificacionPushUsuario = new ClasesVarias.InfoNotificacionPushUsuario();
                if (_DT.Rows.Count > 0)
                {
                    Global._HashInfoNotificacionesPushUsuarios.Clear();
                    Global._SessionIDsUsuariosPush.Clear();
                    foreach (DataRow _Row in _DT.Rows)
                    {
                        _NotificacionPushUsuario = new ClasesVarias.InfoNotificacionPushUsuario();
                        _NotificacionPushUsuario.Id = Convert.ToInt32(_Row["Id"]);
                        _NotificacionPushUsuario.SessionID = _Row["SessionID"].ToString();
                        _NotificacionPushUsuario.JsonSuscripcion = _Row["JsonSuscripcion"].ToString();
                        _NotificacionPushUsuario.FechaExpira = Convert.ToDateTime(_Row["FechaExpira"]);
                        AgregarInfoNotificacionPushUsuario(_NotificacionPushUsuario.ObtenerInstancia());
                    }
                }
                _DT = null;
            }
            catch { }
        }

        private static void ProcesoEnviarNotificaciones()
        {
            try
            {
                List<ClasesVarias.InfoNotificacionPush> _ListaNotificacionPush = ObtenerListaNotificacionPush();

                ClasesVarias.InfoNotificacionPushUsuario _INPU = null; string _Data = "";

                //System.Diagnostics.Process.Start("C:\\Users\\ljappert\\Desktop\\ProxyDesabilitar.reg");

                if (_ListaNotificacionPush.Count > 0)
                {

                    //Ejecutar vbs
                    //System.Diagnostics.Process.Start("C:\\Users\\ljappert\\Desktop\\ProxyDesabilitar.reg");


                     foreach (ClasesVarias.InfoNotificacionPush _INP in _ListaNotificacionPush)
                    {
                        //Arma el Hash que contiene info todas las SessionIDs de usuarios PUSHs. (hasta el momento no necesita de ninguna otra variable preseteada)
                        ArmarHashInfoNotificacionesPushUsuarios(_INP.IdNotificacion);
                        
                        List<string> _ListaUsuariosReceptores = Global._SessionIDsUsuariosPush;
                        foreach (string _Session in _ListaUsuariosReceptores)
                        {
                           //Datos.GuardarLog_NotificacionesPush("Prueba", "1", _NombreMiHost, Global._CadenaConexionAutomatica);
                            _INPU = ObtenerInfoNotificacionPushUsuario(_Session);
                            if (_INPU.SessionID != "" && _INPU.JsonSuscripcion != "")
                            {
                                _Data = "";
                                _Data += _INP.UrlImagenIcono + "*";
                                _Data += _INP.Titulo + "*";
                                _Data += _INP.Descripcion + "*";
                                _Data += _INP.Link + "*";
                                _Data += _INP.UrlImagenBody + "*";
                                _Data += _INP.IdNotificacion + "*";
                                _Data += _INPU.Id + "*";
                                //Datos.GuardarLog_NotificacionesPush("Prueba", "2", _NombreMiHost, Global._CadenaConexionAutomatica);
                                EnviarNotificacion(_INPU.ObtenerJSonSuscripcion(), _Data, _INPU);
                                //Datos.GuardarLog_NotificacionesPush("Prueba", "3", _NombreMiHost, Global._CadenaConexionAutomatica);
                                Datos.LogearAccionNotificacion(_INPU.SessionID, _INP.IdNotificacion, "Push", "");
                            }
                        }

                        if(_INPU != null)
                        {
                            Datos.LogearAccionNotificacion(_INPU.SessionID, _INP.IdNotificacion, "Proceso finalizado: " + _INP.FechaEnvio, "");
                            Datos.MarcarNotificacionEnviada(_INP.IdNotificacion);
                        }
                    }
                }

            }
            catch (IOException ex)
            {
                Datos.GuardarLog_NotificacionesPush(MethodBase.GetCurrentMethod().Name, ex.Message, "Lucas", Global._CadenaConexionAutomatica);
            }
        }

        /// <summary>
        /// Intenta agregar en memoria el objeto _NotificacionPushUsuario.
        /// También agrega el SessionID en la lista Global._SessionIDsUsuariosPush
        /// </summary>
        public static void AgregarInfoNotificacionPushUsuario(ClasesVarias.InfoNotificacionPushUsuario _NotificacionPushUsuario)
        {
            try
            {
                if (Global._HashInfoNotificacionesPushUsuarios.ContainsKey(_NotificacionPushUsuario.SessionID) == false)
                {
                    Global._HashInfoNotificacionesPushUsuarios.Add(_NotificacionPushUsuario.SessionID, _NotificacionPushUsuario);
                }
                if (Global._SessionIDsUsuariosPush.Contains(_NotificacionPushUsuario.SessionID) == false)
                {
                    Global._SessionIDsUsuariosPush.Add(_NotificacionPushUsuario.SessionID);
                }
            }
            catch (IOException ex)
            {
                Datos.GuardarLog_NotificacionesPush(MethodBase.GetCurrentMethod().Name, ex.Message, "Lucas", Global._CadenaConexionAutomatica);
            }
        }

        /// <summary>
        /// Obtiene una instancia nueva de un objeto del tipo ClasesVarias.InfoNotificacionPushUsuario.
        /// </summary>
        public static ClasesVarias.InfoNotificacionPushUsuario ObtenerInfoNotificacionPushUsuario(string _SessionID)
        {
            ClasesVarias.InfoNotificacionPushUsuario _Retorno = new ClasesVarias.InfoNotificacionPushUsuario();
            try
            {
                if (Global._HashInfoNotificacionesPushUsuarios.ContainsKey(_SessionID))
                {
                    _Retorno = ((ClasesVarias.InfoNotificacionPushUsuario)Global._HashInfoNotificacionesPushUsuarios[_SessionID]).ObtenerInstancia();
                }
            }
            catch (IOException ex)
            {
                Datos.GuardarLog_NotificacionesPush(MethodBase.GetCurrentMethod().Name, ex.Message, "Lucas", Global._CadenaConexionAutomatica);
            }
            return _Retorno;
        }

        #region AUXILIARES MASTER

        public static async void EnviarNotificacion(ClasesVarias.JSonSuscripcion _JsonSuscripcion, string _Mensaje, ClasesVarias.InfoNotificacionPushUsuario _INPU)
        {
            int _IdUser = 0;
            try
            {
                _IdUser = _INPU.Id;
                PushSubscription _PushSubscription = new PushSubscription(_JsonSuscripcion.endpoint, _JsonSuscripcion.keys.p256dh, _JsonSuscripcion.keys.auth);
                VapidDetails _VapidDetails = new VapidDetails(Global._MailTo, Global._ClavePublica, Global._ClavePrivada);
                //EnviarNotificacion(_PushSubscription, _Mensaje, _VapidDetails);
                var webPushClient = new WebPushClient();
                await webPushClient.SendNotificationAsync(_PushSubscription, _Mensaje, _VapidDetails);

            }
            catch (Exception ex)
            {
                Datos.GuardarLog_NotificacionesPush(MethodBase.GetCurrentMethod().Name, _IdUser + ": " + ex.Message, "Lucas", Global._CadenaConexionAutomatica);
            }
        }

        public static void EnviarNotificacion(PushSubscription _PushSubscription, string _Mensaje, VapidDetails _VapidDetails)
        {
            try
            {
                var webPushClient = new WebPushClient();
                try
                {
                    webPushClient.SendNotification(_PushSubscription, _Mensaje, _VapidDetails);
                }
                catch (WebPushException exception)
                {
                    Console.WriteLine("Http STATUS code" + exception.StatusCode);
                    Datos.GuardarLog_NotificacionesPush(MethodBase.GetCurrentMethod().Name, exception.Message, "Lucas", Global._CadenaConexionAutomatica);
                }
            }
            catch (Exception ex)
            {
                Datos.GuardarLog_NotificacionesPush(MethodBase.GetCurrentMethod().Name, ex.Message, "Lucas", Global._CadenaConexionAutomatica);
            }
        }


        /// <summary>
        /// Retorna una Lista de todas las NotificacionesPush por enviar.
        /// </summary>
        public static List<ClasesVarias.InfoNotificacionPush> ObtenerListaNotificacionPush()
        {
            List<ClasesVarias.InfoNotificacionPush> _Retorno = new List<ClasesVarias.InfoNotificacionPush>();
            try
            {
                DataTable _DT = Datos.ObtenerDataNotificacionPush();
                ClasesVarias.InfoNotificacionPush _INP = null;
                foreach (DataRow _Row in _DT.Rows)
                {
                    _INP = new ClasesVarias.InfoNotificacionPush();
                    _INP.IdNotificacion = Convert.ToInt32(_Row["Id"]);
                    _INP.UrlImagenIcono = _Row["Icono"].ToString();
                    _INP.Titulo = _Row["Titulo"].ToString();
                    _INP.Descripcion = _Row["Descripcion"].ToString();
                    _INP.UrlImagenBody = _Row["ImagenBody"].ToString();
                    _INP.Link = _Row["Link"].ToString();
                    _INP.FechaEnvio = Convert.ToDateTime(_Row["FechaEnvio"]);
                    _Retorno.Add(_INP.ObtenerInstancia());
                }
                _INP = null;
            }
            catch { }
            return _Retorno;
        }
        #endregion
        
        #endregion

    }
}
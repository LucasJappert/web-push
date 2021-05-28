
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Web_Push.Datos
{
    using MEGA.Datos;

    public class Datos
    {

        public static string GuardarLog_NotificacionesPush(string asunto, string descripcion, string usuario, string BaseQueApunta)
        {
            string _Retorno = "1";
            ConsultaSQL query = new ConsultaSQL();
            query.Conexion = BaseQueApunta;
            try
            {
                string hostName = System.Net.Dns.GetHostName().ToLower();

                query.Consulta += " INSERT INTO Logs.NotificacionesPush (Asunto, Descripcion, UsuarioAlta, FechaAlta)";
                query.Consulta += " VALUES(@asunto, @descripcion, @usuario, getdate())";
                query.AgregarParametro("@asunto", SqlDbType.VarChar, asunto);
                query.AgregarParametro("@descripcion", SqlDbType.VarChar, descripcion);
                query.AgregarParametro("@usuario", SqlDbType.VarChar, usuario);
                query.Ejecutar();
            }
            catch(Exception ex)
            {
                _Retorno = ex.Message;
            }
            query.Cerrar();
            query = null;
            return _Retorno;
        }

        /// <summary>
        /// Obtiene una cadena con los nombres de los servidores bloqueados.
        /// </summary>
        public static string ObtenerServidoresBloqueados()
        {
            string _Retorno = "";
            try
            {

                ConsultaSQL _Query = new ConsultaSQL();
                _Query.Conexion = Global._CadenaConexionAutomatica;
                _Query.Consulta += " SELECT TOP 1 Valor";
                _Query.Consulta += " FROM Config.NotificacionesPush";
                _Query.Consulta += " WHERE Clave = 'ServidoresBloqueados'";
                DataTable _DT = new DataTable();
                try { _DT = _Query.ObtenerTabla(); } catch { }
                _Query.Cerrar();

                if (_DT.Rows.Count > 0)
                {
                    _Retorno = _DT.Rows[0]["Valor"].ToString();
                }
                _DT = null;
            }
            catch { }
            return _Retorno;
        }

        /// <summary>
        /// Devuelve el nombre del Servidor activo en mayúscula.
        /// </summary>
        public static void SetearVariablesDelGlobal(ref string _NombreHostPrimario, ref DateTime _FechaUltimaActualizacion)
        {
            try
            {
                ConsultaSQL _Query = new ConsultaSQL();
                _Query.Conexion = Global._CadenaConexionAutomatica;
                _Query.Consulta += " SELECT";
                _Query.Consulta += " (SELECT TOP 1 Valor FROM Config.NotificacionesPush WHERE Clave = 'ServidorPrimario') as ServidorPrimario, ";
                _Query.Consulta += " (SELECT TOP 1 FechaModi FROM Config.NotificacionesPush WHERE Clave = 'KALServidorPrimario') as FechaUltimaActualizacion ";
                DataTable _DT = new DataTable();
                try { _DT = _Query.ObtenerTabla(); } catch { }
                _Query.Cerrar();
                
                if (_DT.Rows.Count > 0)
                {
                    _NombreHostPrimario = _DT.Rows[0]["ServidorPrimario"].ToString().ToUpper();
                    _FechaUltimaActualizacion = Convert.ToDateTime(_DT.Rows[0]["FechaUltimaActualizacion"]);
                }
                _DT = null;
            }
            catch { }
        }

        /// <summary>
        /// Retorna un DT con info de la tabla InfoNotificacionesPushUsuarios
        /// </summary>
        public static DataTable ObtenerInfoNotificacionesPushUsuarios(int _IdNoti)
        {
            DataTable _Retorno = new DataTable();
            try
            {
                ConsultaSQL query = new ConsultaSQL();
                query.Conexion = Global._CadenaConexionAutomatica;
                query.Consulta += " SELECT INPU.Id, INPU.SessionID, INPU.JsonSuscripcion, INPU.FechaExpira";
                query.Consulta += " FROM InfoNotificacionesNotificacionesUsuarios INNU";
                query.Consulta += " JOIN InfoNotificacionesPush I ON I.id = INNU.IdInfoNotificacionPush";
                query.Consulta += " JOIN InfoNotificacionesPushUsuarios INPU ON INNU.IdInfoNotificacionesPushUsuarios=INPU.Id";
                query.Consulta += " WHERE I.Id=@IdNoti";
                query.AgregarParametro("@IdNoti", SqlDbType.Int, _IdNoti);
                try {_Retorno = query.ObtenerTabla();} catch { }

                //Si no hay relación de Noti con Usuarios específicos, recupero todos los usuarios
                if (_Retorno.Rows.Count == 0)
                {
                    query = new ConsultaSQL();
                    query.Conexion = Global._CadenaConexionAutomatica;
                    query.Consulta += " SELECT Id, SessionID, JsonSuscripcion, FechaExpira ";
                    query.Consulta += " FROM InfoNotificacionesPushUsuarios WHERE Id > 103936";
                    //query.Consulta += " WHERE FechaExpira + 7 > GETDATE()";
                    try { _Retorno = query.ObtenerTabla(); } catch { }
                }

                query.Cerrar();
            }
            catch { }
            return _Retorno;
        }

        /// <summary>
        /// Retorna una Lista de todas las NotificacionesPush por enviar.
        /// </summary>
        public static DataTable ObtenerDataNotificacionPush()
        {
            DataTable _Retorno = new DataTable();
            try
            {
                ConsultaSQL query = new ConsultaSQL();
                query.Conexion = Global._CadenaConexionAutomatica;
                query.Consulta += " SELECT Id, Icono, Titulo, Descripcion, Link, FechaEnvio, ImagenBody ";
                query.Consulta += " FROM InfoNotificacionesPush";
                query.Consulta += " WHERE Enviado=0 AND FechaEnvio < getdate() AND Borrado=0";

                try { _Retorno = query.ObtenerTabla(); } catch { }
                query.Cerrar();
            }
            catch { }
            return _Retorno;
        }

        public static List<string> ObtenerListaUsuariosReceptores(int _IdNoti)
        {
            List<string> _Retorno = new List<string>();
            try
            {
                ConsultaSQL query = new ConsultaSQL();
                query.Conexion = Global._CadenaConexionAutomatica;
                query.Consulta += " SELECT INP.Id, COALESCE(INPU.SessionID, '') as SessionIdUsuario ";
                query.Consulta += " FROM InfoNotificacionesPush INP";
                query.Consulta += " LEFT JOIN InfoNotificacionesNotificacionesUsuarios INNU ON INP.Id=INNU.IdInfoNotificacionPush";
                query.Consulta += " LEFT JOIN InfoNotificacionesPushUsuarios INPU ON INNU.IdInfoNotificacionesPushUsuarios=INPU.Id";
                query.Consulta += " WHERE INP.Id=@IdNoti AND INPU.Id > 103936";
                query.AgregarParametro("@IdNoti", SqlDbType.Int, _IdNoti);

                DataTable _DT = new DataTable();
                try { _DT = query.ObtenerTabla(); } catch { }
                query.Cerrar();

                if(_DT.Rows.Count > 0)
                {
                    foreach(DataRow _Row in _DT.Rows)
                    {
                        if(_Row["SessionIdUsuario"].ToString() == "")
                        {
                            if(_DT.Rows.Count == 1)
                            {
                                //Caso envío a todos los usuarios
                                _Retorno = Global._SessionIDsUsuariosPush;
                                break;
                            }
                        }
                        else
                        {
                            _Retorno.Add(_Row["SessionIdUsuario"].ToString());
                        }
                    }
                }
                _DT = null;
            }
            catch { }
            return _Retorno;
        }

        /// <summary>
        /// Setea a una Notificacion como enviada y también la fecha en que se envió.
        /// </summary>
        public static void MarcarNotificacionEnviada(int _IdNotificacion)
        {
            try
            {
                ConsultaSQL query = new ConsultaSQL();
                query.Conexion = Global._CadenaConexionAutomatica;
                query.Consulta += " UPDATE InfoNotificacionesPush ";
                query.Consulta += " 	SET Enviado=1, FechaEnviado=getdate()";
                query.Consulta += " WHERE Id=@IdNotificacion";
                query.AgregarParametro("@IdNotificacion", SqlDbType.Int, _IdNotificacion);
                try { query.Ejecutar(); } catch { }
                query.Cerrar();
            }
            catch { }
        }


        /// <summary>
        /// Logea una acción de una Notificación.
        /// </summary>
        public static void LogearAccionNotificacion(string _SessionID, int _IdNoti, string _Accion, string _Valor)
        {
            try
            {
                ConsultaSQL query = new ConsultaSQL();
                query.Conexion = Global._CadenaConexionAutomatica;
                query.Consulta += " INSERT INTO Logs.AccionesNotificacionesPush (Accion, Valor, SessionID, IdNoti)";
                query.Consulta += " VALUES (@Accion, @Valor, @SessionID, @IdNoti)";
                query.AgregarParametro("@Accion", SqlDbType.VarChar, _Accion);
                query.AgregarParametro("@SessionID", SqlDbType.VarChar, _SessionID);
                query.AgregarParametro("@Valor", SqlDbType.VarChar, _Valor);
                query.AgregarParametro("@IdNoti", SqlDbType.Int, _IdNoti);

                try
                {
                    query.Ejecutar();
                }
                catch { }
                query = null;
            }
            catch { }
        }

        #region ---------------------------------------------------------- Updates a Config.NotificacionesPush
        public static void UpdatearServidorPrimario(string _Valor)
        {
            try
            {
                ConsultaSQL _Query = new ConsultaSQL();
                _Query.Conexion = Global._CadenaConexionAutomatica;
                _Query.Consulta += " UPDATE Config.NotificacionesPush";
                _Query.Consulta += " SET Valor=@Valor, FechaModi=getdate()";
                _Query.Consulta += " WHERE Clave = 'ServidorPrimario'";
                _Query.AgregarParametro("@Valor", SqlDbType.VarChar, _Valor);
                try { _Query.Ejecutar(); } catch { }
                _Query.Cerrar();
            }
            catch { }
        }
        public static void UpdatearServidorSecundario(string _Valor)
        {
            try
            {
                ConsultaSQL _Query = new ConsultaSQL();
                _Query.Conexion = Global._CadenaConexionAutomatica;
                _Query.Consulta += " UPDATE Config.NotificacionesPush";
                _Query.Consulta += " SET Valor=@Valor, FechaModi=getdate()";
                _Query.Consulta += " WHERE Clave = 'ServidorSecundario'";
                _Query.AgregarParametro("@Valor", SqlDbType.VarChar, _Valor);
                try { _Query.Ejecutar(); } catch { }
                _Query.Cerrar();
            }
            catch { }
        }
        /// <summary>
        /// Update la fecha en que terminó de realizar las notificaciones
        /// </summary>
        public static void UpdatearKALServidorPrimario(string _NombreServidor)
        {
            try
            {
                ConsultaSQL _Query = new ConsultaSQL();
                _Query.Conexion = Global._CadenaConexionAutomatica;
                _Query.Consulta += " UPDATE Config.NotificacionesPush";
                _Query.Consulta += " SET Valor=@Valor, FechaModi=getdate()";
                _Query.Consulta += " WHERE Clave = 'KALServidorPrimario'";
                _Query.AgregarParametro("@Valor", SqlDbType.VarChar, _NombreServidor);
                try { _Query.Ejecutar(); } catch { }
                _Query.Cerrar();
            }
            catch { }
        }
        #endregion

    }
}
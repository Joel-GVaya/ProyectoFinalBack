Imports System.Net
Imports System.Web.Http
Imports System.Threading.Tasks
Imports Ptoyecto.Application
Imports Proyecto.Utils

<RoutePrefix("api/estilos")>
Public Class EstilosController
    Inherits ApiController

    <HttpGet>
    <Route("")>
    Public Async Function ObtenerEstilos() As Task(Of IHttpActionResult)
        Logger.LogInfo("Peticion [GET] recibida para obtener los estilos")
        Await Task.Yield()
        Try
            Dim servicio As New EstilosService()
            Dim estilos = Await servicio.CargarEstilos()
            Logger.LogInfo("Estilos cargados correctamente")
            Return Ok(estilos)
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

End Class

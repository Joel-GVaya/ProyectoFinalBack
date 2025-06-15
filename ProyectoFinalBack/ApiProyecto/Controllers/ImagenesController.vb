' En el archivo: EndpointsApi/Controllers/UsuariosController.vb

Imports System.Net                ' Para HttpStatusCode
Imports System.Web.Http           ' Para ApiController, HttpPost, HttpPut, HttpDelete, HttpGet, RoutePrefix, Route, FromBody
Imports System.Threading.Tasks    ' Para Task(Of T)
Imports Ptoyecto.Application
Imports Proyecto.Utils
' Ya no necesitamos estos Imports si no usamos los tipos de request/response
' Imports Proyecto.Application ' Para CrearUsuarioRequest, LoginRequest, EditarUsuarioRequest, UsuarioResponse, LoginResponse

' Atributos para configurar el controlador
<RoutePrefix("api/imagenes")>
Public Class ImagenesController
    Inherits ApiController

    ' GET: api/imagenes/publicaciones
    <HttpGet>
    <Route("publicaciones")>
    Public Async Function ObtenerPublicaciones() As Task(Of IHttpActionResult)
        Logger.LogInfo("[GET] recibido /api/imagenes/publicaciones")
        Try
            Dim servicio As New ImagenService()
            Logger.LogInfo("Obteniendo imagenes publicadas")
            Dim imagenesPublicadas = Await servicio.ObtenerPublicacionesAsync()

            If imagenesPublicadas Is Nothing OrElse imagenesPublicadas.Count = 0 Then
                Logger.LogInfo("No hay imagenes publicadas disponibles")
                Return Content(HttpStatusCode.NotFound, New With {
                .message = "No hay imágenes publicadas disponibles."
            })
            End If

            Return Ok(imagenesPublicadas)
        Catch ex As Exception
            Return InternalServerError(New Exception("Error al obtener imágenes publicadas: " & ex.Message))
        End Try
    End Function



    ' GET: api/imagenes/usuario/{id}
    <HttpGet>
    <Route("usuario/{id}")>
    Public Async Function ObtenerUsuario(id As String) As Task(Of IHttpActionResult)
        Logger.LogInfo("[GET] recibido /api/imagenes/usuario/{id}")
        Await Task.Yield()
        Try
            Dim servicio As New ImagenService()
            Logger.LogInfo("Obteniendo imagen del usuario")
            Dim imagen = Await servicio.ObtenerImagenPorIdYTokenAsync(id, Request)

            If imagen Is Nothing Then
                Logger.LogError("Imagen no encontrada o el usuario no tiene permisos para acceder a esta imagen")
                Return Content(HttpStatusCode.NotFound, New With {
                .message = "Imagen no encontrada o el usuario no tiene permisos para acceder a esta imagen."
            })
            End If

            Return Ok(imagen)
        Catch ex As UnauthorizedAccessException
            Return Content(HttpStatusCode.Unauthorized, New With {
            .message = "Token inválido o usuario no autorizado."
        })
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function


    <HttpGet>
    <Route("usuario")>
    Public Async Function ObtenerImagenesDeUsuario() As Task(Of IHttpActionResult)
        Logger.LogInfo("[GET] recibido /api/imagenes/usuario")
        Try
            Dim servicio As New ImagenService()
            Logger.LogInfo("Obteniendo todas las imagenes del usuario")
            Dim imagenes = Await servicio.ObtenerImagenesDeUsuarioAsync(Request)

            If imagenes Is Nothing OrElse imagenes.Count = 0 Then
                Logger.LogError("No se encontraron imagenes para este usuario")
                Return Content(HttpStatusCode.NotFound, New With {
                .message = "No se encontraron imágenes para este usuario."
            })
            End If

            Return Ok(imagenes)
        Catch ex As UnauthorizedAccessException
            Logger.LogError("Token invvalido o usuario no autorizado")
            Return Content(HttpStatusCode.Unauthorized, New With {
            .message = "Token inválido o usuario no autorizado."
        })
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

    <HttpDelete>
    <Route("usuario/{id}")>
    Public Async Function EliminarImagen(id As String) As Task(Of IHttpActionResult)
        Logger.LogInfo("[DELETE] recibido /api/imagenes/usuario/{id}")
        Await Task.Yield()

        Try
            Dim servicio As New ImagenService()
            Dim token = Request.Headers.Authorization.Parameter
            Logger.LogInfo("Eliminando imagen del usuario")
            Dim resultado = Await servicio.EliminarImagenDeUsuario(id, token)

            If resultado = "Eliminada" Then
                Logger.LogInfo("Imagen eliminado correctamente")
                Return Ok("Imagen eliminada correctamente.")
            Else
                Return Content(HttpStatusCode.Forbidden, resultado)
            End If

        Catch ex As Exception
            Logger.LogError($"Error al eliminar la imagen {ex}")
            Return InternalServerError(ex)
        End Try
    End Function


    <HttpGet>
    <Route("publicar/{id}")>
    Public Async Function PublicarImagen(id As String) As Task(Of IHttpActionResult)
        Logger.LogInfo("[GET] /api/imagenes/publicar/{id}")
        Try
            Dim service As New ImagenService()
            Logger.LogInfo("Alternando estado de publicacion de la imagen")
            Dim resultado = Await service.AlternarPublicacion(id, Request)

            If resultado.Exito Then
                Dim respuesta = New Dictionary(Of String, Object) From {
                    {"success", True},
                    {"publicada", If(resultado.Publicada, 1, 0)}
                }
                Return Ok(respuesta)

            Else
                Logger.LogError("No se pudo actualizar el estado de la publicacion")
                Return Content(HttpStatusCode.Forbidden, "No se pudo actualizar el estado de publicación. Verifica si eres el propietario de la imagen.")
            End If

        Catch ex As UnauthorizedAccessException
            Logger.LogError($"No tienes permisos para modificar la publicacion de esta imagen: {ex}")
            Return Content(HttpStatusCode.Unauthorized, New With {.message = ex.Message})
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function





End Class

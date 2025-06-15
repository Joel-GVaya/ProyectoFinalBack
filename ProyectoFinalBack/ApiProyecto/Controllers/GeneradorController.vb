Imports System.Net
Imports System.Net.Http
Imports System.Web.Http
Imports System.Web.Http.Description
Imports Proyecto.Modelos
Imports Proyecto.Utils
Imports Ptoyecto.Application.Proyecto.Servicios

<RoutePrefix("api/generador")>
Public Class GeneradorController
    Inherits ApiController

    Private ReadOnly _generacionService As GeneracionService

    Public Sub New()
        _generacionService = New GeneracionService()
    End Sub

    <HttpPost>
    <Route("subir/imagen")>
    <ResponseType(GetType(Object))>
    Public Async Function SubirImagen() As Threading.Tasks.Task(Of IHttpActionResult)
        Logger.LogInfo("POST recibido: /api/Generador1/subir/imagen")

        Try
            Dim httpRequest = HttpContext.Current.Request

            ' Validar token en header Authorization Bearer
            Dim authHeader = Request.Headers.Authorization
            If authHeader Is Nothing OrElse authHeader.Scheme.ToLower() <> "bearer" Then
                Logger.LogWarning("Falta el encabezado Authorization Bearer.")
                Return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Token de autorización no proporcionado."))
            End If

            Dim token As String = authHeader.Parameter
            If String.IsNullOrEmpty(token) Then
                Logger.LogWarning("El token Bearer está vacío.")
                Return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Token inválido."))
            End If

            ' Validar que haya archivo en la petición
            If httpRequest.Files.Count = 0 Then
                Logger.LogWarning("No se ha enviado ningún archivo de imagen.")
                Return BadRequest("Se requiere un archivo de imagen.")
            End If

            Dim archivo As HttpPostedFile = httpRequest.Files(0)

            ' Validar que sea jpg o png
            Dim allowedExtensions = New String() {".jpg", ".jpeg", ".png"}
            Dim fileExtension = IO.Path.GetExtension(archivo.FileName).ToLower()

            If Not allowedExtensions.Contains(fileExtension) Then
                Logger.LogWarning("Archivo no permitido. Solo jpg/jpeg/png.")
                Return BadRequest("Formato de imagen no permitido. Solo jpg/jpeg/png.")
            End If

            ' Leer imagen a bytes
            Dim imagenBytes As Byte()
            Using ms As New IO.MemoryStream()
                archivo.InputStream.CopyTo(ms)
                imagenBytes = ms.ToArray()
            End Using

            ' Convertir a base64 para el servicio
            Dim imagenBase64 As String = Convert.ToBase64String(imagenBytes)

            ' Obtener EstiloId desde formulario (puedes cambiar a header si quieres)
            Dim estiloIdStr As String = httpRequest.Form("EstiloId")
            Dim estiloId As Integer
            If Not Integer.TryParse(estiloIdStr, estiloId) OrElse estiloId <= 0 Then
                Logger.LogWarning("EstiloId inválido o no proporcionado.")
                Return BadRequest("Se requiere un EstiloId válido.")
            End If

            ' Llamar al servicio
            Logger.LogInfo("Llamando al servicion de generacion de imagen")
            Dim resultado As ResultadoGeneracion = Await _generacionService.GenerarImagen(imagenBase64, estiloId, token)

            If Not resultado.Exito Then
                Logger.LogWarning("No se pudo generar la imagen.")
                Return Ok(New With {
                    .exito = resultado.Exito,
                    .mensaje = resultado.Mensaje
                })

            End If

            Logger.LogInfo($"Imagen generada correctamente con ID: {resultado.IdImagen}")
            Return Ok(New With {.IdImagenGenerada = resultado.IdImagen})


        Catch ex As Exception
            Logger.LogError($"Error en SubirImagen: {ex}")
            Return InternalServerError(New Exception("Error interno al procesar la imagen."))
        End Try
    End Function


    <HttpPost>
    <Route("subir/texto")>
    <ResponseType(GetType(Object))>
    Public Async Function SubirTexto() As Threading.Tasks.Task(Of IHttpActionResult)
        Logger.LogInfo("POST recibido: /api/Generador1/subir/texto")

        Try
            Dim httpRequest = HttpContext.Current.Request

            ' Validar token en header Authorization Bearer
            Dim authHeader = Request.Headers.Authorization
            If authHeader Is Nothing OrElse authHeader.Scheme.ToLower() <> "bearer" Then
                Logger.LogWarning("Falta el encabezado Authorization Bearer.")
                Return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Token de autorización no proporcionado."))
            End If

            Dim token As String = authHeader.Parameter
            If String.IsNullOrEmpty(token) Then
                Logger.LogWarning("El token Bearer está vacío.")
                Return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Token inválido."))
            End If

            ' Obtener el prompt desde el formulario
            Dim prompt As String = httpRequest.Form("prompt")
            If String.IsNullOrWhiteSpace(prompt) Then
                Logger.LogWarning("Prompt no proporcionado o vacío.")
                Return BadRequest("Se requiere un prompt de texto válido.")
            End If

            ' Obtener EstiloId desde el formulario
            Dim estiloIdStr As String = httpRequest.Form("EstiloId")
            Dim estiloId As Integer
            If Not Integer.TryParse(estiloIdStr, estiloId) OrElse estiloId <= 0 Then
                Logger.LogWarning("EstiloId inválido o no proporcionado.")
                Return BadRequest("Se requiere un EstiloId válido.")
            End If

            ' Llamar al servicio correspondiente
            Dim resultado As ResultadoGeneracion = Await _generacionService.GenerarImagenDesdeTexto(prompt, estiloId, token)

            If Not resultado.Exito Then
                Return Content(HttpStatusCode.BadRequest, New With {.Mensaje = resultado.Mensaje})
            End If
            Logger.LogInfo($"Imagen generada correctamente desde texto con ID: {resultado.IdImagen}")
            Return Ok(New With {
                .IdImagenGenerada = resultado.IdImagen,
                .Mensaje = resultado.Mensaje
            })




        Catch ex As Exception
            Logger.LogError($"Error en SubirTexto: {ex}")
            Return InternalServerError(New Exception("Error interno al procesar el prompt."))
        End Try
    End Function



End Class

' Modelo de entrada (cuerpo JSON)
Public Class SubirImagenRequest
    Public Property ImagenBase64 As String
    Public Property EstiloId As Integer
End Class

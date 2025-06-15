Imports Proyecto.Data
Imports System.Security.Claims
Imports Proyecto.Modelos.Proyecto.Modelos
Imports System.Net.Http
Imports Proyecto.Modelos
Imports Proyecto.Utils

Public Class ImagenService

    Private ReadOnly _repo As New ImagenRepository()

    Public Async Function ObtenerImagenPorIdYTokenAsync(idImagen As String, request As HttpRequestMessage) As Task(Of Imagen)
        Logger.LogInfo($"[ObtenerImagenPorIdYTokenAsync] Iniciando para imagen: {idImagen}")
        Dim authHeader = request.Headers.Authorization
        If authHeader Is Nothing OrElse authHeader.Scheme <> "Bearer" Then
            Logger.LogError("[ObtenerImagenPorIdYTokenAsync] Token ausente o esquema inválido.")
            Throw New UnauthorizedAccessException("No se proporcionó un token válido.")
        End If

        Dim token = authHeader.Parameter
        Dim claims = TokenHelper.ValidateTokenAndGetClaims(token)
        If claims Is Nothing Then
            Logger.LogError("[ObtenerImagenPorIdYTokenAsync] Token inválido.")
            Throw New UnauthorizedAccessException("Token inválido.")
        End If

        Dim userIdClaim = claims.FirstOrDefault(Function(c) c.Type = ClaimTypes.NameIdentifier)
        If userIdClaim Is Nothing Then
            Logger.LogError("[ObtenerImagenPorIdYTokenAsync] Claim de usuario no encontrado.")
            Throw New UnauthorizedAccessException("Token no contiene el id del usuario.")
        End If

        Dim userId = Convert.ToInt32(userIdClaim.Value)
        Logger.LogInfo($"[ObtenerImagenPorIdYTokenAsync] Buscando imagen para usuario: {userId}")
        Return Await _repo.ObtenerPorIdYUsuarioAsync(idImagen, userId)
    End Function

    Public Async Function EliminarImagenDeUsuario(idImagen As String, token As String) As Task(Of String)
        Logger.LogInfo($"[EliminarImagenDeUsuario] Iniciando eliminación de imagen: {idImagen}")
        Dim claims = TokenHelper.ValidateTokenAndGetClaims(token)
        If claims Is Nothing Then
            Logger.LogError("[EliminarImagenDeUsuario] Token inválido.")
            Return "Token inválido o expirado."
        End If

        Dim userIdClaim = claims.FirstOrDefault(Function(c) c.Type = ClaimTypes.NameIdentifier)
        If userIdClaim Is Nothing Then
            Logger.LogError("[EliminarImagenDeUsuario] Claim de usuario no encontrado.")
            Return "No se pudo obtener el ID del usuario desde el token."
        End If

        Dim idUsuario = Convert.ToInt32(userIdClaim.Value)
        Logger.LogInfo($"[EliminarImagenDeUsuario] Verificando pertenencia de imagen al usuario {idUsuario}")

        Dim imagen = Await _repo.ObtenerPorIdYUsuarioAsync(idImagen, idUsuario)
        If imagen Is Nothing Then
            Logger.LogError("[EliminarImagenDeUsuario] Imagen no encontrada o no pertenece al usuario.")
            Return "Imagen no encontrada o no pertenece al usuario."
        End If

        Dim exito = Await _repo.EliminarPorIdAsync(idImagen)
        Logger.LogInfo($"[EliminarImagenDeUsuario] Resultado de eliminación: {exito}")
        If exito Then
            Return "Eliminada"
        Else
            Return "Error al eliminar la imagen en base de datos."
        End If
    End Function

    Public Async Function AlternarPublicacion(idImagen As String, request As HttpRequestMessage) As Task(Of ResultadoPublicacion)
        Logger.LogInfo($"[AlternarPublicacion] Alternando publicación para imagen: {idImagen}")
        Dim authHeader = request.Headers.Authorization
        If authHeader Is Nothing OrElse authHeader.Scheme <> "Bearer" Then
            Logger.LogError("[AlternarPublicacion] Token ausente o inválido.")
            Throw New UnauthorizedAccessException("No se proporcionó un token válido.")
        End If

        Dim token = authHeader.Parameter
        Dim claims = TokenHelper.ValidateTokenAndGetClaims(token)
        If claims Is Nothing Then
            Logger.LogError("[AlternarPublicacion] Token inválido.")
            Throw New UnauthorizedAccessException("Token inválido.")
        End If

        Dim userIdClaim = claims.FirstOrDefault(Function(c) c.Type = ClaimTypes.NameIdentifier)
        If userIdClaim Is Nothing Then
            Logger.LogError("[AlternarPublicacion] Claim de usuario no encontrado.")
            Throw New UnauthorizedAccessException("El token no contiene el ID del usuario.")
        End If

        Dim idUsuario = Convert.ToInt32(userIdClaim.Value)
        Logger.LogInfo($"[AlternarPublicacion] Usuario autenticado: {idUsuario}")

        Dim imagen = Await _repo.ObtenerPorIdYUsuarioAsync(idImagen, idUsuario)
        If imagen Is Nothing Then
            Logger.LogError("[AlternarPublicacion] Imagen no encontrada o no pertenece al usuario.")
            Return New ResultadoPublicacion With {.Exito = False}
        End If

        imagen.Publicada = Not imagen.Publicada
        Logger.LogInfo($"[AlternarPublicacion] Nuevo estado de publicación: {imagen.Publicada}")

        Dim actualizado = Await _repo.ActualizarPublicacionAsync(imagen.Id, imagen.Publicada)
        Logger.LogInfo($"[AlternarPublicacion] Estado actualizado en BD: {actualizado}")

        Return New ResultadoPublicacion With {
            .Exito = actualizado,
            .Publicada = imagen.Publicada
        }
    End Function

    Public Async Function ObtenerImagenesDeUsuarioAsync(request As HttpRequestMessage) As Task(Of List(Of Imagen))
        Logger.LogInfo("[ObtenerImagenesDeUsuarioAsync] Iniciando obtención de imágenes del usuario")
        Dim authHeader = request.Headers.Authorization
        If authHeader Is Nothing OrElse authHeader.Scheme <> "Bearer" Then
            Logger.LogError("[ObtenerImagenesDeUsuarioAsync] Token ausente o inválido.")
            Throw New UnauthorizedAccessException("No se proporcionó un token válido.")
        End If

        Dim token = authHeader.Parameter
        Dim claims = TokenHelper.ValidateTokenAndGetClaims(token)
        If claims Is Nothing Then
            Logger.LogError("[ObtenerImagenesDeUsuarioAsync] Token inválido.")
            Throw New UnauthorizedAccessException("Token inválido.")
        End If

        Dim userIdClaim = claims.FirstOrDefault(Function(c) c.Type = ClaimTypes.NameIdentifier)
        If userIdClaim Is Nothing Then
            Logger.LogError("[ObtenerImagenesDeUsuarioAsync] Claim de usuario no encontrado.")
            Throw New UnauthorizedAccessException("Token no contiene el id del usuario.")
        End If

        Dim userId = Convert.ToInt32(userIdClaim.Value)
        Logger.LogInfo($"[ObtenerImagenesDeUsuarioAsync] Usuario autenticado: {userId}")
        Return Await _repo.ObtenerTodasPorUsuarioAsync(userId)
    End Function

    Public Async Function ObtenerPublicacionesAsync() As Task(Of List(Of Imagen))
        Logger.LogInfo("[ObtenerPublicacionesAsync] Obteniendo publicaciones públicas")
        Dim imagenes = Await _repo.ObtenerImagenesPublicadasAsync()

        Dim resultado = imagenes.Select(Function(img) New Imagen With {
            .Id = img.Id,
            .Fecha = img.Fecha,
            .ImagenBase64 = img.ImagenBase64,
            .Estilo = img.Estilo,
            .Publicada = img.Publicada,
            .IdUsuario = img.IdUsuario
        }).ToList()

        Logger.LogInfo($"[ObtenerPublicacionesAsync] Total publicaciones encontradas: {resultado.Count}")
        Return resultado
    End Function

End Class

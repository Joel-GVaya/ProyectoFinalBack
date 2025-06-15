' En el archivo: EndpointsApi/Controllers/UsuariosController.vb

Imports System.Net                ' Para HttpStatusCode
Imports System.Web.Http           ' Para ApiController, HttpPost, HttpPut, HttpDelete, HttpGet, RoutePrefix, Route, FromBody
Imports System.Threading.Tasks    ' Para Task(Of T)
Imports Proyecto.Modelos
Imports Ptoyecto.Application
Imports System.Security.Claims
Imports Proyecto.Utils
Imports System.Web.Http.Description
Imports System.Net.Http
' Ya no necesitamos estos Imports si no usamos los tipos de request/response
' Imports Proyecto.Application ' Para CrearUsuarioRequest, LoginRequest, EditarUsuarioRequest, UsuarioResponse, LoginResponse

' Atributos para configurar el controlador

<RoutePrefix("api/usuarios")>
Public Class UsuariosController
    Inherits ApiController
    Private ReadOnly _usuarioService As New UsuarioService()


    ' Ya no necesitamos el servicio si no hay lógica de negocio
    ' Private ReadOnly _userService As IUserService 

    ' Ya no necesitamos el constructor para inyección de dependencias
    ' Public Sub New(userService As IUserService)
    '     _userService = userService
    ' End Sub

    ' POST: api/usuarios/registrar
    ' Endpoint para crear un nuevo usuario
    <HttpPost>
    <Route("registrar")>
    Public Async Function Registrar(<FromBody> usuario As Usuario) As Task(Of IHttpActionResult)
        Logger.LogInfo("[POST] recibido /api/usuarios/registrar")
        If usuario Is Nothing Then
            Logger.LogError("El cuerpo de la solicitud esta vacio o mal formado")
            Return BadRequest("El cuerpo de la solicitud está vacío o mal formado.")
        End If

        ' Await al llamar al método async
        Logger.LogInfo("Registrando al usuario en la BBDD")
        Dim exito As Boolean = Await _usuarioService.RegistrarUsuario(usuario)

        If exito Then
            Logger.LogInfo("Usuario registrado correctamente")
            Return Ok(New With {
            .mensaje = "Usuario registrado correctamente."
        })
        Else
            Logger.LogError("Error al registrart al usuario. Se han de verificar los datos")
            Return Content(HttpStatusCode.BadRequest, New With {
            .mensaje = "Error al registrar el usuario. Verifica los datos."
        })
        End If
    End Function



    ' POST: api/usuarios/login
    ' Endpoint para iniciar sesión y obtener un token JWT
    <HttpPost>
    <Route("login")>
    Public Async Function Login(<FromBody> request As LoginRequest) As Task(Of IHttpActionResult)
        Logger.LogInfo("[POST] recibido /api/usuarios/login")
        If request Is Nothing OrElse String.IsNullOrWhiteSpace(request.Correo) OrElse String.IsNullOrWhiteSpace(request.Password) Then
            Logger.LogError("Correo y contraseña requeridos")
            Return BadRequest("Correo y contraseña son obligatorios.")
        End If

        Dim usuarioService As New UsuarioService()
        Dim loginResult = Await usuarioService.LoginAsync(request.Correo, request.Password)

        If loginResult Is Nothing Then
            Return Unauthorized()
        End If
        Logger.LogInfo("Inicio de session exitoso")
        Return Ok(New With {
        .Mensaje = "Inicio de sesión exitoso",
        .Token = loginResult.Token,
        .Usuario = New With {
            loginResult.Usuario.Id,
            loginResult.Usuario.Nombre,
            loginResult.Usuario.Apellidos,
            loginResult.Usuario.Correo,
            loginResult.Usuario.Edad,
            loginResult.Usuario.Telefono,
            loginResult.Usuario.NivelAcceso,
            loginResult.Usuario.Imagen
        }
    })
    End Function




    ' PUT: api/usuarios/
    ' Endpoint para editar los datos de un usuario
    <HttpPut>
    <Route("")>
    Public Async Function Editar(<FromBody> request As Usuario) As Task(Of IHttpActionResult)
        Logger.LogInfo("[PUT] recibido /api/usuarios")
        Dim authHeader = Me.Request.Headers.Authorization
        If authHeader Is Nothing OrElse authHeader.Scheme <> "Bearer" Then
            Logger.LogError("Usuario no autenticado")
            Return Unauthorized()
        End If

        Dim token = authHeader.Parameter
        Dim claims = TokenHelper.ValidateTokenAndGetClaims(token)
        If claims Is Nothing Then
            Logger.LogError("Usuario no autenticado")
            Return Unauthorized()
        End If

        Dim usuarioIdStr = claims.FirstOrDefault(Function(c) c.Type = ClaimTypes.NameIdentifier)?.Value
        If String.IsNullOrEmpty(usuarioIdStr) Then
            Logger.LogError("Usuario no autorizado")
            Return Unauthorized()
        End If

        Dim usuarioId As Integer = Integer.Parse(usuarioIdStr)

        Dim resultado = Await _usuarioService.ModificarUsuarioAsync(usuarioId, request)
        If Not resultado Then
            Logger.LogError("No se pudo actualizar el usuario")
            Return InternalServerError(New Exception("No se pudo actualizar el usuario"))
        End If
        Logger.LogInfo("Usuario actualizado correctamente")
        Return Ok(New With {.Mensaje = "Usuario actualizado correctamente"})
    End Function

    <HttpPost>
    <Route("verificarContrasena")>
    Public Async Function VerificarContrasena(<FromBody> req As VerificarContrasenaRequest) As Threading.Tasks.Task(Of IHttpActionResult)
        Try
            If req Is Nothing OrElse String.IsNullOrWhiteSpace(req.Passwd) Then
                Return BadRequest("Se requiere la contraseña.")
            End If

            ' Obtener token desde los headers
            Dim authHeader = Request.Headers.Authorization
            If authHeader Is Nothing OrElse authHeader.Scheme.ToLower() <> "bearer" Then
                Return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Token no proporcionado."))
            End If

            Dim token As String = authHeader.Parameter
            If String.IsNullOrEmpty(token) Then
                Return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Token inválido."))
            End If

            ' Llamar al servicio pasándole el token y la contraseña
            Dim servicio As New UsuarioService()
            Dim resultado = Await servicio.VerificarContraseña(token, req.Passwd)

            If resultado.Exito Then
                Return Ok(resultado)
            Else
                Return Content(HttpStatusCode.Unauthorized, resultado)
            End If

        Catch ex As Exception
            Logger.LogError("Error al verificar la contraseña: " & ex.Message)
            Return InternalServerError(New Exception("Error al verificar la contraseña."))
        End Try
    End Function




End Class
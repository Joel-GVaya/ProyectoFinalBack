' Application\Services\UsuarioService.vb
Imports Proyecto.Utils
Imports Proyecto.Modelos
Imports Proyecto.Data

Public Class UsuarioService

    Private ReadOnly _repo As New UsuarioRepository()

    Public Async Function RegistrarUsuario(usuario As Usuario) As Task(Of Boolean)
        Dim validador As New ValidateUser()
        Logger.LogInfo("Validando los datos del usuario")
        If Not validador.Validar(usuario) Then
            ' Mostrar errores de validación
            For Each E In validador.UsuarioErrores
                Logger.LogError("Error: " + E)
            Next
            Return False
        End If

        ' Encriptar contraseña
        usuario.Password = PasswordHelper.HashPassword(usuario.Password)

        ' Llamar al repositorio de forma asíncrona
        Dim resultado As Boolean = Await _repo.AgregarAsync(usuario)

        Return resultado
    End Function

    Public Async Function LoginAsync(correo As String, password As String) As Task(Of LoginResult)
        Logger.LogInfo("Obteniendo usuario de la BBDD")
        Dim usuario = Await _repo.ObtenerPorCorreoAsync(correo)

        If usuario Is Nothing Then
            Logger.LogError("Usuario no encontrado")
            Return Nothing ' Usuario no encontrado
        End If

        Dim esValido As Boolean = PasswordHelper.VerifyPassword(password, usuario.Password)

        If Not esValido Then
            Logger.LogError("Contraseña del usuario incorrecta")
            Return Nothing ' Contraseña incorrecta
        End If

        ' Generar el token para este usuario
        Logger.LogInfo("Generando token para el usuario")
        Dim token = TokenHelper.GenerateToken(usuario, 360) ' Token válido por 6 horas

        Return New LoginResult With {
            .Usuario = usuario,
            .Token = token
        }
    End Function

    Public Async Function ModificarUsuarioAsync(usuarioId As Integer, datosActualizados As Usuario) As Task(Of Boolean)
        ' Obtener el usuario actual
        Dim usuarioExistente = Await _repo.ObtenerPorIdAsync(usuarioId)
        If usuarioExistente Is Nothing Then
            Logger.LogError("El usuario no existe")
            Return False ' Usuario no existe
        End If

        ' Actualizar campos que quieras permitir modificar
        usuarioExistente.Nombre = datosActualizados.Nombre
        usuarioExistente.Apellidos = datosActualizados.Apellidos
        usuarioExistente.Correo = datosActualizados.Correo
        usuarioExistente.Edad = datosActualizados.Edad
        usuarioExistente.Telefono = datosActualizados.Telefono

        ' Si la contraseña viene diferente y no está vacía, hashearla
        If Not String.IsNullOrWhiteSpace(datosActualizados.Password) Then
            Logger.LogInfo("Haseando contraseña")
            usuarioExistente.Password = PasswordHelper.HashPassword(datosActualizados.Password)
        End If

        usuarioExistente.NivelAcceso = datosActualizados.NivelAcceso
        usuarioExistente.Imagen = datosActualizados.Imagen

        ' Guardar cambios en la base de datos (crea el método en el repo)
        Dim resultado = Await _repo.ActualizarAsync(usuarioExistente)

        Return resultado
    End Function

    Public Async Function VerificarContraseña(token As String, contra As String) As Task(Of VerificarContraseñaResponse)
        Try
            Logger.LogInfo("Extrayendo ID del usuario desde el token")

            ' Extraer usuarioId desde el token JWT
            Dim usuarioId As Integer = TokenHelper.ExtraerUsuarioId(token)

            Logger.LogInfo("Verificando la contraseña del usuario con ID: " & usuarioId)

            ' Obtener el usuario por ID
            Dim usuario = Await _repo.ObtenerPorIdAsync(usuarioId)

            If usuario Is Nothing Then
                Logger.LogError("Usuario no encontrado.")
                Return New VerificarContraseñaResponse With {
                    .Exito = False,
                    .Mensaje = "Usuario no encontrado."
                }
            End If

            ' Verificar la contraseña usando el helper
            Dim esValida = PasswordHelper.VerifyPassword(contra, usuario.Password)

            If esValida Then
                Logger.LogInfo("Contraseña válida para el usuario.")
                Return New VerificarContraseñaResponse With {
                    .Exito = True,
                    .Mensaje = "Contraseña válida."
                }
            Else
                Logger.LogWarning("Contraseña incorrecta.")
                Return New VerificarContraseñaResponse With {
                    .Exito = False,
                    .Mensaje = "Contraseña incorrecta."
                }
            End If

        Catch ex As Exception
            Logger.LogError("Error verificando la contraseña: " & ex.Message)
            Return New VerificarContraseñaResponse With {
                .Exito = False,
                .Mensaje = "Error interno al verificar la contraseña."
            }
        End Try
    End Function







End Class


Imports Proyecto.Modelos
Imports System.Text.RegularExpressions


Public Class ValidateUser

    ' Lista de errores accesible después de validar
    Public Property UsuarioErrores As List(Of String)

    Public Function Validar(usuario As Usuario) As Boolean
        UsuarioErrores = New List(Of String)()

        ' Validar nombre
        If String.IsNullOrWhiteSpace(usuario.Nombre) Then
            UsuarioErrores.Add("El nombre es obligatorio.")
        End If

        ' Validar apellidos
        If String.IsNullOrWhiteSpace(usuario.Apellidos) Then
            UsuarioErrores.Add("Los apellidos son obligatorios.")
        End If

        ' Validar correo
        If String.IsNullOrWhiteSpace(usuario.Correo) Then
            UsuarioErrores.Add("El correo es obligatorio.")
        ElseIf Not Regex.IsMatch(usuario.Correo, "^[^@\s]+@[^@\s]+\.[^@\s]+$") Then
            UsuarioErrores.Add("El correo no tiene un formato válido.")
        End If

        ' Validar edad (mínimo 18)
        If usuario.Edad < 18 Then
            UsuarioErrores.Add("Debes ser mayor de 18 años.")
        End If

        ' Validar teléfono (mínimo 9 dígitos por ejemplo)
        If String.IsNullOrWhiteSpace(usuario.Telefono) OrElse usuario.Telefono.Length < 9 Then
            UsuarioErrores.Add("El teléfono es obligatorio y debe tener al menos 9 dígitos.")
        End If

        ' Validar password
        If String.IsNullOrWhiteSpace(usuario.Password) OrElse usuario.Password.Length < 6 Then
            UsuarioErrores.Add("La contraseña es obligatoria y debe tener al menos 6 caracteres.")
        End If

        ' Nivel de acceso por defecto
        If String.IsNullOrWhiteSpace(usuario.NivelAcceso) Then
            usuario.NivelAcceso = "4"
        End If

        ' Imagen es opcional

        Return UsuarioErrores.Count = 0
    End Function

End Class

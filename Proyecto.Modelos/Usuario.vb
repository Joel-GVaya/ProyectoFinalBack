' En el archivo: Proyecto.Models/Usuario.vb

Public Class Usuario

    Public Property Id As String
    Public Property Nombre As String
    Public Property Apellidos As String
    Public Property Correo As String
    Public Property Edad As Integer
    Public Property Telefono As String
    Public Property Password As String ' Considera almacenar contraseñas hasheadas (por ejemplo, con BCrypt o PBKDF2) por seguridad.
    Public Property NivelAcceso As Integer ' Clave foránea al nivel de acceso
    Public Property Imagen As String ' Para almacenar la imagen en Base64 o una ruta.

End Class
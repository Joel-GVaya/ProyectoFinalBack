Imports System.Security.Cryptography
Imports System.Text

Public Class PasswordHelper

    ' Hashea la contraseña con PBKDF2 y devuelve un string con: salt:hash
    Public Shared Function HashPassword(password As String) As String
        Dim saltBytes(15) As Byte
        Using rng = New RNGCryptoServiceProvider()
            rng.GetBytes(saltBytes)
        End Using

        Dim pbkdf2 = New Rfc2898DeriveBytes(password, saltBytes, 10000)
        Dim hashBytes = pbkdf2.GetBytes(20)

        Dim saltBase64 = Convert.ToBase64String(saltBytes)
        Dim hashBase64 = Convert.ToBase64String(hashBytes)

        Return $"{saltBase64}:{hashBase64}"
    End Function

    ' Verifica si la contraseña coincide con el hash guardado
    Public Shared Function VerifyPassword(password As String, storedHash As String) As Boolean
        Dim parts = storedHash.Split(":"c)
        If parts.Length <> 2 Then Return False

        Dim saltBytes = Convert.FromBase64String(parts(0))
        Dim storedHashBytes = Convert.FromBase64String(parts(1))

        Dim pbkdf2 = New Rfc2898DeriveBytes(password, saltBytes, 10000)
        Dim hashBytes = pbkdf2.GetBytes(20)

        Return hashBytes.SequenceEqual(storedHashBytes)
    End Function

End Class

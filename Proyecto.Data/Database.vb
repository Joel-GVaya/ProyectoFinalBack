Imports MySql.Data.MySqlClient

Public Class Database

    Private connectionString As String =
        "Server=localhost;Database=ProyectoImagenes;Uid=joelGarcia;Pwd=JoelGarcia1234;"

    Public Function GetConnection() As MySqlConnection
        Return New MySqlConnection(connectionString)
    End Function

End Class

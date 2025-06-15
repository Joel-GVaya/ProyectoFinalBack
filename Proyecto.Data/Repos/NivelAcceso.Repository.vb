Imports MySql.Data.MySqlClient
Imports Proyecto.Modelos.Proyecto.Modelos

Public Class NivelAccesoRepository

    Private ReadOnly db As New Database()
    Public Async Function ObtenerNivelPorId(id As Integer) As Task(Of NivelAcceso)
        ' SELECT * FROM niveles_acceso WHERE id = @id
        ' Devuelve el objeto NivelAcceso
        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync()

                Dim query As String = "SELECT * FROM niveles_acceso WHERE id = @id"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@id", id)

                    Using reader = Await cmd.ExecuteReaderAsync()
                        If Await reader.ReadAsync() Then
                            Dim nivelAcceso As New NivelAcceso() With {
                                .id = Convert.ToInt32(reader("id")),
                                .cantidad = Convert.ToInt32(reader("cantidad"))
                            }
                            Return nivelAcceso
                        End If

                        ' Aquí devuelve Nothing si no leyó ningún registro
                        Return Nothing
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ' Puedes loguear ex.Message aquí
            Return Nothing
        End Try
    End Function
End Class

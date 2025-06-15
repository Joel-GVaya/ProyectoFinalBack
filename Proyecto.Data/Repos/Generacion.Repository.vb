Imports MySql.Data.MySqlClient
Imports Proyecto.Ut
Imports Proyecto.Modelos.Proyecto.Modelos

Public Class GeneracionRepository
    Private ReadOnly db As New Database()

    Public Async Function Gunal(imagenBase64 As String, id As Integer) As Task(Of Integer)
        Try
            Using conn = db.GetConnection()
                Console.WriteLine("Antes de OpenAsync")
                Await conn.OpenAsync().ConfigureAwait(False)
                Console.WriteLine("Después de OpenAsync")

                ' Insertar imagen
                Dim queryInsert As String = "INSERT INTO imagenesSubidas (imagenBase64, id_usuario) VALUES (@imagenBase64, @id_usuari);"
                Using cmdInsert As New MySqlCommand(queryInsert, conn)
                    cmdInsert.Parameters.AddWithValue("@imagenBase64", imagenBase64)
                    cmdInsert.Parameters.AddWithValue("@id_usuari", id)
                    cmdInsert.CommandTimeout = 30
                    Console.WriteLine("Antes de ExecuteNonQueryAsync")
                    Await cmdInsert.ExecuteNonQueryAsync().ConfigureAwait(False)
                    Console.WriteLine("Después de ExecuteNonQueryAsync")
                End Using

                ' Obtener último ID insertado
                Dim queryLastId As String = "SELECT LAST_INSERT_ID();"
                Using cmdLastId As New MySqlCommand(queryLastId, conn)
                    cmdLastId.CommandTimeout = 30
                    Console.WriteLine("Antes de ExecuteScalarAsync")
                    Dim result = Await cmdLastId.ExecuteScalarAsync().ConfigureAwait(False)
                    Console.WriteLine("Después de ExecuteScalarAsync")

                    If result IsNot Nothing Then
                        Return Convert.ToInt32(result)
                    End If
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine($"Error en Gunal: {ex.Message}")
            Throw
        End Try

        Return -1
    End Function


    Public Async Function GunalPrompt(prompt As String, id As Integer) As Task(Of Integer)
        Try
            Using conn = db.GetConnection()
                Console.WriteLine("Antes de OpenAsync")
                Await conn.OpenAsync().ConfigureAwait(False)
                Console.WriteLine("Después de OpenAsync")

                ' Insertar imagen
                Dim queryInsert As String = "INSERT INTO PromptsUsuariosSubidos (prompt, id_usuario) VALUES (@prompt, @id_usuari);"
                Using cmdInsert As New MySqlCommand(queryInsert, conn)
                    cmdInsert.Parameters.AddWithValue("@prompt", prompt)
                    cmdInsert.Parameters.AddWithValue("@id_usuari", id)
                    cmdInsert.CommandTimeout = 30
                    Console.WriteLine("Antes de ExecuteNonQueryAsync")
                    Await cmdInsert.ExecuteNonQueryAsync().ConfigureAwait(False)
                    Console.WriteLine("Después de ExecuteNonQueryAsync")
                End Using

                ' Obtener último ID insertado
                Dim queryLastId As String = "SELECT LAST_INSERT_ID();"
                Using cmdLastId As New MySqlCommand(queryLastId, conn)
                    cmdLastId.CommandTimeout = 30
                    Console.WriteLine("Antes de ExecuteScalarAsync")
                    Dim result = Await cmdLastId.ExecuteScalarAsync().ConfigureAwait(False)
                    Console.WriteLine("Después de ExecuteScalarAsync")

                    If result IsNot Nothing Then
                        Return Convert.ToInt32(result)
                    End If
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine($"Error en Gunal: {ex.Message}")
            Throw
        End Try

        Return -1
    End Function

    Public Async Function ActualizarImagenGeneradaId(idOriginal As Integer, idGenerada As String) As Task(Of Boolean)
        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync().ConfigureAwait(False)

                Dim query As String = "UPDATE imagenesSubidas SET ImagenGeneradaId = @idGenerada WHERE Id = @idOriginal"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@idGenerada", idGenerada)
                    cmd.Parameters.AddWithValue("@idOriginal", idOriginal)
                    cmd.CommandTimeout = 30

                    Dim filas As Integer = Await cmd.ExecuteNonQueryAsync().ConfigureAwait(False)
                    Return filas > 0
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine($"Error en ActualizarImagenGeneradaId: {ex.Message}")
        End Try
        Return False
    End Function

    Public Async Function ActualizarImagenGeneradaIdPrompt(idOriginal As Integer, idGenerada As String) As Task(Of Boolean)
        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync().ConfigureAwait(False)

                Dim query As String = "UPDATE PromptsUsuariosSubidos SET imagen_generada_id = @idGenerada WHERE id = @idOriginal"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@idGenerada", idGenerada)
                    cmd.Parameters.AddWithValue("@idOriginal", idOriginal)
                    cmd.CommandTimeout = 30

                    Dim filas As Integer = Await cmd.ExecuteNonQueryAsync().ConfigureAwait(False)
                    Return filas > 0
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine($"Error en ActualizarImagenGeneradaId: {ex.Message}")
        End Try
        Return False
    End Function

    Public Async Function InsertarImagenGenerada(imagen As Imagen) As Task(Of Boolean)
        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync().ConfigureAwait(False)

                Dim query As String = "INSERT INTO imagenesGeneradas (id, id_usuario, publicada, fecha, estilo, imagen_base64) " &
                        "VALUES (@Id, @IdUsuario, @Publicada, @Fecha, @Estilo, @ImagenBase64)"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@Id", imagen.Id)
                    cmd.Parameters.AddWithValue("@IdUsuario", imagen.IdUsuario)
                    cmd.Parameters.AddWithValue("@Publicada", imagen.Publicada)
                    cmd.Parameters.AddWithValue("@Fecha", imagen.Fecha)
                    cmd.Parameters.AddWithValue("@Estilo", imagen.Estilo)
                    cmd.Parameters.AddWithValue("@ImagenBase64", imagen.ImagenBase64)
                    cmd.CommandTimeout = 30

                    Dim filas As Integer = Await cmd.ExecuteNonQueryAsync().ConfigureAwait(False)
                    Return filas > 0
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine($"Error en InsertarImagenGenerada: {ex.Message}")
        End Try
        Return False
    End Function





End Class

Imports MySql.Data.MySqlClient
Imports Proyecto.Modelos
Imports Proyecto.Modelos.Proyecto.Modelos

Public Class ImagenRepository

    Private db As New Database()

    Public Async Function ObtenerPorIdYUsuarioAsync(idImagen As String, idUsuario As Integer) As Task(Of Imagen)
        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync()

                Dim query As String = "SELECT * FROM imagenesGeneradas WHERE id = @id AND id_usuario = @idUsuario"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@id", idImagen)
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario)

                    Using reader = Await cmd.ExecuteReaderAsync()
                        If Await reader.ReadAsync() Then
                            Dim imagen As New Imagen() With {
                                .Id = reader("id").ToString(),
                                .IdUsuario = Convert.ToInt32(reader("id_usuario")),
                                .Publicada = Convert.ToBoolean(reader("publicada")),
                                .Fecha = Convert.ToDateTime(reader("fecha")),
                                .Estilo = Convert.ToInt32(reader("estilo")),
                                .ImagenBase64 = reader("imagen_base64").ToString()
                            }
                            Return imagen
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ' Log si es necesario
        End Try

        Return Nothing
    End Function

    Public Async Function EliminarPorIdAsync(id As String) As Task(Of Boolean)
        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync()

                Dim query = "DELETE FROM imagenesGeneradas WHERE id = @id"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@id", id)
                    Dim filas = Await cmd.ExecuteNonQueryAsync()
                    Return filas > 0
                End Using
            End Using

        Catch ex As Exception
            ' Log opcional
            Return False
        End Try
    End Function


    Public Async Function ActualizarPublicacionAsync(id As String, publicada As Boolean) As Task(Of Boolean)
        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync()

                Dim query As String = "UPDATE imagenesGeneradas SET publicada = @publicada WHERE id = @id"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@id", id)
                    cmd.Parameters.AddWithValue("@publicada", publicada)

                    Dim filas = Await cmd.ExecuteNonQueryAsync()
                    Return filas > 0
                End Using
            End Using
        Catch ex As Exception
            ' Log opcional
            Return False
        End Try
    End Function

    Public Async Function ObtenerTodasPorUsuarioAsync(idUsuario As Integer) As Task(Of List(Of Imagen))
        Dim imagenes As New List(Of Imagen)()

        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync()

                Dim query As String = "SELECT * FROM imagenesGeneradas WHERE id_usuario = @idUsuario ORDER BY fecha DESC"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario)

                    Using reader = Await cmd.ExecuteReaderAsync()
                        While Await reader.ReadAsync()
                            imagenes.Add(New Imagen() With {
                            .Id = reader("id").ToString(),
                            .IdUsuario = Convert.ToInt32(reader("id_usuario")),
                            .Publicada = Convert.ToBoolean(reader("publicada")),
                            .Fecha = Convert.ToDateTime(reader("fecha")),
                            .Estilo = Convert.ToInt32(reader("estilo")),
                            .ImagenBase64 = reader("imagen_base64").ToString()
                        })
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ' Log opcional
        End Try

        Return imagenes
    End Function

    Public Async Function ObtenerImagenesPublicadasAsync() As Task(Of List(Of Imagen))
        Dim imagenes As New List(Of Imagen)()

        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync()

                Dim query As String = "SELECT * FROM imagenesGeneradas WHERE publicada = 1 ORDER BY fecha DESC"

                Using cmd As New MySqlCommand(query, conn)

                    Using reader = Await cmd.ExecuteReaderAsync()
                        While Await reader.ReadAsync()
                            imagenes.Add(New Imagen() With {
                            .Id = reader("id").ToString(),
                            .IdUsuario = Convert.ToInt32(reader("id_usuario")),
                            .Publicada = Convert.ToBoolean(reader("publicada")),
                            .Fecha = Convert.ToDateTime(reader("fecha")),
                            .Estilo = Convert.ToInt32(reader("estilo")),
                            .ImagenBase64 = reader("imagen_base64").ToString()
                        })
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ' Log opcional
        End Try

        Return imagenes
    End Function



    Public Async Function ObtenerCantidadImagenesGeneradasHoy(idUsuario As Integer) As Task(Of Integer)
        Dim cantidad As Integer = 0
        Dim fechaHoy = DateTime.UtcNow.Date

        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync()

                Dim query As String = "SELECT COUNT(*) FROM imagenesGeneradas WHERE id_usuario = @idUsuario AND DATE(fecha) = @fechaHoy"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario)
                    cmd.Parameters.AddWithValue("@fechaHoy", fechaHoy)

                    Dim result = Await cmd.ExecuteScalarAsync()
                    If result IsNot Nothing AndAlso Not Convert.IsDBNull(result) Then
                        cantidad = Convert.ToInt32(result)
                    End If
                End Using
            End Using
        Catch ex As Exception
            ' Puedes loguear el error si lo deseas
            cantidad = -1 ' Opcional: podrías devolver -1 en caso de error como indicador
        End Try

        Return cantidad
    End Function





End Class

Imports MySql.Data.MySqlClient
Imports Proyecto.Modelos
Imports Proyecto.Utils ' O como se llame tu namespace con el modelo Usuario

Public Class UsuarioRepository

    Private db As New Database()

    Public Async Function AgregarAsync(usuario As Usuario) As Task(Of Boolean)
        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync()

                Dim query As String = "INSERT INTO usuarios (nombre, apellidos, correo, edad, telefono, password, nivelAcceso, imagen)
                                       VALUES (@nombre, @apellidos, @correo, @edad, @telefono, @password, @nivel, @imagen)"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@nombre", usuario.Nombre)
                    cmd.Parameters.AddWithValue("@apellidos", usuario.Apellidos)
                    cmd.Parameters.AddWithValue("@correo", usuario.Correo)
                    cmd.Parameters.AddWithValue("@edad", usuario.Edad)
                    cmd.Parameters.AddWithValue("@telefono", usuario.Telefono)
                    cmd.Parameters.AddWithValue("@password", usuario.Password)
                    cmd.Parameters.AddWithValue("@nivel", 4)
                    cmd.Parameters.AddWithValue("@imagen", If(String.IsNullOrWhiteSpace(usuario.Imagen), DBNull.Value, usuario.Imagen))

                    Await cmd.ExecuteNonQueryAsync()
                End Using
            End Using

            Return True

        Catch ex As Exception
            Logger.LogError("Error al agregar usuario a la BBDD")
            Return False
        End Try
    End Function

    Public Async Function ObtenerPorCorreoAsync(correo As String) As Task(Of Usuario)
        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync()

                Dim query As String = "SELECT * FROM usuarios WHERE correo = @correo"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@correo", correo)

                    Using reader = Await cmd.ExecuteReaderAsync()
                        If Await reader.ReadAsync() Then
                            Dim usuario As New Usuario() With {
                            .Id = Convert.ToInt32(reader("id")),
                            .Nombre = reader("nombre").ToString(),
                            .Apellidos = reader("apellidos").ToString(),
                            .Correo = reader("correo").ToString(),
                            .Edad = Convert.ToInt32(reader("edad")),
                            .Telefono = reader("telefono").ToString(),
                            .Password = reader("password").ToString(),
                            .NivelAcceso = Convert.ToInt32(reader("nivelAcceso")),
                            .Imagen = If(reader("imagen") Is DBNull.Value, Nothing, reader("imagen").ToString())
                        }
                            Return usuario
                        End If

                        Return Nothing
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Return Nothing
        End Try
    End Function


    Public Async Function ActualizarAsync(usuario As Usuario) As Task(Of Boolean)
        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync()

                Dim query As String = "UPDATE usuarios SET 
                nombre = @nombre,
                apellidos = @apellidos,
                correo = @correo,
                edad = @edad,
                telefono = @telefono,
                password = @password,
                nivelAcceso = @nivelAcceso,
                imagen = @imagen
                WHERE id = @id"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@nombre", usuario.Nombre)
                    cmd.Parameters.AddWithValue("@apellidos", usuario.Apellidos)
                    cmd.Parameters.AddWithValue("@correo", usuario.Correo)
                    cmd.Parameters.AddWithValue("@edad", usuario.Edad)
                    cmd.Parameters.AddWithValue("@telefono", usuario.Telefono)
                    cmd.Parameters.AddWithValue("@password", usuario.Password)
                    cmd.Parameters.AddWithValue("@nivelAcceso", usuario.NivelAcceso)
                    cmd.Parameters.AddWithValue("@imagen", If(String.IsNullOrWhiteSpace(usuario.Imagen), DBNull.Value, usuario.Imagen))
                    cmd.Parameters.AddWithValue("@id", usuario.Id)

                    Dim rowsAffected = Await cmd.ExecuteNonQueryAsync()
                    Return rowsAffected > 0
                End Using
            End Using
        Catch ex As Exception
            ' Aquí puedes loguear el error si quieres: Console.WriteLine(ex.Message)
            Return False
        End Try
    End Function

    Public Async Function ObtenerPorIdAsync(id As Integer) As Task(Of Usuario)
        Try
            Using conn = db.GetConnection()
                Await conn.OpenAsync()

                Dim query As String = "SELECT * FROM usuarios WHERE id = @id"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@id", id)

                    Using reader = Await cmd.ExecuteReaderAsync()
                        If Await reader.ReadAsync() Then
                            Dim usuario As New Usuario() With {
                                .Id = Convert.ToInt32(reader("id")),
                                .Nombre = reader("nombre").ToString(),
                                .Apellidos = reader("apellidos").ToString(),
                                .Correo = reader("correo").ToString(),
                                .Edad = Convert.ToInt32(reader("edad")),
                                .Telefono = reader("telefono").ToString(),
                                .Password = reader("password").ToString(),
                                .NivelAcceso = Convert.ToInt32(reader("nivelAcceso")),
                                .Imagen = If(reader("imagen") Is DBNull.Value, Nothing, reader("imagen").ToString())
                            }
                            Return usuario
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

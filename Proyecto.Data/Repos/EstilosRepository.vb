Imports Proyecto.Modelos
Imports MySql.Data.MySqlClient

Public Class EstilosRepository

    Private db As New Database()

    Public Async Function ObtenerTodosLosEstilos() As Task(Of List(Of Estilo))
        Dim estilos As New List(Of Estilo)

        Using conn = db.GetConnection()
            Await conn.OpenAsync()

            Dim query As String = "SELECT id, nombre, promptImagen, promptTexto, imagen FROM estilosDisponibles"

            Using cmd As New MySqlCommand(query, conn)
                Using reader = Await cmd.ExecuteReaderAsync()
                    While Await reader.ReadAsync()
                        Dim estilo As New Estilo With {
                            .id = Convert.ToInt32(reader("id")),
                            .nombre = reader("nombre").ToString(),
                            .promptImagen = reader("promptImagen").ToString(),
                            .promptTexto = reader("promptTexto").ToString(),
                            .imagen = reader("imagen").ToString()
                        }
                        estilos.Add(estilo)
                    End While
                End Using
            End Using
        End Using

        Return estilos
    End Function

    Public Async Function ObtenerPorIdAsync(id As Integer) As Task(Of Estilo)
        Try
            Using conn = db.GetConnection()
                Console.WriteLine("Abriendo conexión...")
                Await conn.OpenAsync().ConfigureAwait(False)
                Console.WriteLine("Conexión abierta.")

                Dim query = "SELECT id, nombre, promptImagen, promptTexto, imagen FROM estilosDisponibles WHERE id = @id"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@id", id)
                    cmd.CommandTimeout = 30

                    Console.WriteLine("Ejecutando lector...")
                    Using reader = Await cmd.ExecuteReaderAsync().ConfigureAwait(False)
                        Console.WriteLine("Lector ejecutado.")
                        If Await reader.ReadAsync().ConfigureAwait(False) Then
                            Return New Estilo With {
                                .id = Convert.ToInt32(reader("id")),
                                .nombre = reader("nombre").ToString(),
                                .promptImagen = reader("promptImagen").ToString(),
                                .promptTexto = reader("promptTexto").ToString(),
                                .imagen = reader("imagen").ToString()
                            }
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine($"Error en ObtenerPorIdAsync: {ex.Message}")
            Throw
        End Try

        Return Nothing
    End Function


End Class

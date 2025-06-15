
Imports System.IO

Imports System.Text
Imports Proyecto.Modelos

Imports Proyecto.Data
Imports Proyecto.Modelos.Proyecto.Modelos
Imports System.Security.Claims
Imports Proyecto.Utils ' Donde tengas el GeneracionRepository

Namespace Proyecto.Servicios

    Public Class GeneracionService

        Private ReadOnly _generacionRepository As New GeneracionRepository()
        Private ReadOnly _estiloRepository As New EstilosRepository()

        Private ReadOnly _rutaScriptExe As String = "C:\Users\garci\Documents\ProyectoFinalBUENO\ProyectoFinalBack\GeneradorLineArt\Script\dist\scrypt1.exe"
        Private ReadOnly _rutaScriptTextoExe As String = "C:\Users\garci\Documents\ProyectoFinalBUENO\ProyectoFinalBack\GeneradorLineArt\Script\dist\scryptImage.exe"

        Public Async Function GenerarImagen(imagenBase64 As String, estiloId As Integer, token As String) As Task(Of ResultadoGeneracion)
            Dim rutaTemporal As String = String.Empty

            Try
                Dim idUsuario As Integer
                Try
                    idUsuario = ObtenerIdUsuarioDesdeToken(token)
                Catch ex As UnauthorizedAccessException
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Token inválido o no contiene el ID del usuario"
            }
                End Try

                Dim puedeGenerar = Await PuedeGenerarImagen(idUsuario)
                If Not puedeGenerar Then
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Límite de generación de imágenes diario alcanzado"
            }
                End If

                Dim idOriginal As Integer = Await _generacionRepository.Gunal(imagenBase64, idUsuario)
                If idOriginal <= 0 Then
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "No se pudo guardar la imagen original"
            }
                End If

                Dim estilo As Estilo = _estiloRepository.ObtenerPorIdAsync(estiloId).GetAwaiter().GetResult()
                If estilo Is Nothing Then
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Estilo no encontrado"
            }
                End If

                rutaTemporal = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() & ".jpg")
                If Not GuardarBase64ComoArchivo(imagenBase64, rutaTemporal) Then
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Error al guardar imagen temporal"
            }
                End If

                Dim imagenFinalBase64 As String = Await EjecutarScriptAsync(rutaTemporal, estilo.promptImagen)
                If String.IsNullOrEmpty(imagenFinalBase64) Then
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Fallo al generar la imagen"
            }
                End If

                Dim idSeguro As String = Guid.NewGuid().ToString("N")

                Dim imagenGenerada As New Imagen With {
            .Id = idSeguro,
            .IdUsuario = idUsuario,
            .Publicada = False,
            .Fecha = DateTime.UtcNow,
            .Estilo = estiloId,
            .ImagenBase64 = imagenFinalBase64
        }

                If Not _generacionRepository.InsertarImagenGenerada(imagenGenerada).GetAwaiter().GetResult() Then
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Error al guardar imagen generada"
            }
                End If

                If Not _generacionRepository.ActualizarImagenGeneradaId(idOriginal, idSeguro).GetAwaiter().GetResult() Then
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Error al actualizar imagen original con ID generado"
            }
                End If

                Return New ResultadoGeneracion With {
            .Exito = True,
            .IdImagen = idSeguro,
            .Mensaje = "Imagen generada correctamente"
        }

            Catch ex As Exception
                Logger.LogError($"Error en GenerarImagen: {ex.Message}")
                Return New ResultadoGeneracion With {
            .Exito = False,
            .Mensaje = "Error inesperado: " & ex.Message
        }
            Finally
                If Not String.IsNullOrEmpty(rutaTemporal) AndAlso File.Exists(rutaTemporal) Then
                    Try
                        File.Delete(rutaTemporal)
                    Catch
                    End Try
                End If
            End Try
        End Function



        Public Async Function GenerarImagenDesdeTexto(prompt As String, estiloId As Integer, token As String) As Task(Of ResultadoGeneracion)
            Try
                ' 1. Obtener IdUsuario desde token
                Dim idUsuario As Integer
                Try
                    idUsuario = ObtenerIdUsuarioDesdeToken(token)
                Catch ex As UnauthorizedAccessException
                    Logger.LogError($"Error al obtener el id del usuario del token: {ex}")
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Token inválido o sin id válido"
            }
                End Try

                ' 2. Comprobar si puede generar
                Dim puedeGenerar As Boolean
                Try
                    puedeGenerar = Await PuedeGenerarImagen(idUsuario)
                    If Not puedeGenerar Then
                        Return New ResultadoGeneracion With {
                    .Exito = False,
                    .Mensaje = "Límite de generación de imágenes diario alcanzado"
                }
                    End If
                Catch ex As Exception
                    Logger.LogError($"Error al comprobar si puede generar imagen: {ex.Message}")
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Error interno al verificar capacidad de generación"
            }
                End Try

                ' 3. Guardar prompt original
                Dim idOriginal As Integer = Await _generacionRepository.GunalPrompt(prompt, idUsuario)
                If idOriginal <= 0 Then
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Error al guardar prompt original"
            }
                End If

                ' 4. Obtener estilo
                Dim estilo As Estilo = _estiloRepository.ObtenerPorIdAsync(estiloId).GetAwaiter().GetResult()
                If estilo Is Nothing Then
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Estilo no encontrado"
            }
                End If

                ' 5. Ejecutar el script de generación
                Dim imagenFinalBase64 As String = Await EjecutarScriptTextoAsync(prompt, estilo.promptTexto)
                If String.IsNullOrEmpty(imagenFinalBase64) Then
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Error al generar la imagen desde texto"
            }
                End If

                ' 6. Crear imagen generada
                Dim idSeguro As String = Guid.NewGuid().ToString("N")
                Dim imagenGenerada As New Imagen With {
            .Id = idSeguro,
            .IdUsuario = idUsuario,
            .Publicada = False,
            .Fecha = DateTime.UtcNow,
            .Estilo = estiloId,
            .ImagenBase64 = imagenFinalBase64
        }

                ' 7. Guardar imagen generada
                Dim guardo = _generacionRepository.InsertarImagenGenerada(imagenGenerada).GetAwaiter().GetResult()
                If Not guardo Then
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Error al guardar imagen generada"
            }
                End If

                ' 8. Actualizar prompt original con la imagen generada
                Dim actualizo = _generacionRepository.ActualizarImagenGeneradaIdPrompt(idOriginal, idSeguro).GetAwaiter().GetResult()
                If Not actualizo Then
                    Return New ResultadoGeneracion With {
                .Exito = False,
                .Mensaje = "Error al enlazar la imagen generada con el prompt"
            }
                End If

                Return New ResultadoGeneracion With {
            .Exito = True,
            .IdImagen = idSeguro,
            .Mensaje = "Imagen generada exitosamente"
        }

            Catch ex As Exception
                Logger.LogError($"Error en GenerarImagenDesdeTexto: {ex.Message}")
                Return New ResultadoGeneracion With {
            .Exito = False,
            .Mensaje = "Error interno al generar la imagen desde texto"
        }
            End Try
        End Function



        Private Function GuardarBase64ComoArchivo(base64String As String, rutaArchivo As String) As Boolean
            Try
                Dim bytes As Byte() = Convert.FromBase64String(base64String)
                File.WriteAllBytes(rutaArchivo, bytes)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Private Async Function EjecutarScriptAsync(rutaArchivo As String, prompt As String) As Task(Of String)
            Try
                Dim startInfo As New ProcessStartInfo() With {
                    .FileName = _rutaScriptExe,
                    .Arguments = $"""{rutaArchivo}"" ""{prompt}""",
                    .RedirectStandardOutput = True,
                    .RedirectStandardError = True,
                    .UseShellExecute = False,
                    .CreateNoWindow = True
                }

                Dim output As String = String.Empty

                Using process As New Process()
                    process.StartInfo = startInfo

                    Dim outputBuilder As New StringBuilder()
                    Dim errorBuilder As New StringBuilder()

                    AddHandler process.OutputDataReceived, Sub(sender, args)
                                                               If args.Data IsNot Nothing Then
                                                                   outputBuilder.AppendLine(args.Data)
                                                               End If
                                                           End Sub

                    AddHandler process.ErrorDataReceived, Sub(sender, args)
                                                              If args.Data IsNot Nothing Then
                                                                  errorBuilder.AppendLine(args.Data)
                                                              End If
                                                          End Sub

                    process.Start()
                    process.BeginOutputReadLine()
                    process.BeginErrorReadLine()

                    ' Esperar que termine de forma asíncrona
                    Await Task.Run(Sub() process.WaitForExit())

                    If process.ExitCode <> 0 Then
                        Logger.LogError("Error ejecutando script: " & errorBuilder.ToString())
                        Return String.Empty
                    End If

                    Return outputBuilder.ToString().Trim()
                End Using
            Catch ex As Exception
                Logger.LogError("Excepción en EjecutarScriptAsync: " & ex.Message)
                Return String.Empty
            End Try
        End Function

        Private Async Function EjecutarScriptTextoAsync(texto_usuario As String, prompt As String) As Task(Of String)
            Try
                Dim safePrompt = prompt.Replace("""", "\""")
                Dim safeTextoUsuario = texto_usuario.Replace("""", "\""")
                Dim startInfo As New ProcessStartInfo() With {
                    .FileName = _rutaScriptTextoExe,
                    .Arguments = $"""{safePrompt}"" ""{safeTextoUsuario}""",
                    .RedirectStandardOutput = True,
                    .RedirectStandardError = True,
                    .UseShellExecute = False,
                    .CreateNoWindow = True
                }

                Dim outputBuilder As New StringBuilder()
                Dim errorBuilder As New StringBuilder()

                Using process As New Process()
                    process.StartInfo = startInfo

                    AddHandler process.OutputDataReceived, Sub(sender, args)
                                                               If args.Data IsNot Nothing Then
                                                                   outputBuilder.AppendLine(args.Data)
                                                               End If
                                                           End Sub

                    AddHandler process.ErrorDataReceived, Sub(sender, args)
                                                              If args.Data IsNot Nothing Then
                                                                  errorBuilder.AppendLine(args.Data)
                                                              End If
                                                          End Sub

                    process.Start()
                    process.BeginOutputReadLine()
                    process.BeginErrorReadLine()

                    Await Task.Run(Sub() process.WaitForExit())

                    If process.ExitCode <> 0 Then
                        Dim err = errorBuilder.ToString()
                        Logger.LogError("Error ejecutando script: " & err)
                        Return String.Empty
                    End If

                    Return outputBuilder.ToString().Trim()
                End Using
            Catch ex As Exception
                Logger.LogError("Excepción en EjecutarScriptAsync: " & ex.Message)
                Return String.Empty
            End Try
        End Function

        Public Function ObtenerIdUsuarioDesdeToken(token As String) As Integer
            Dim claims = TokenHelper.ValidateTokenAndGetClaims(token)
            If claims Is Nothing Then
                Throw New UnauthorizedAccessException("Token inválido.")
            End If

            Dim userIdClaim = claims.FirstOrDefault(Function(c) c.Type = ClaimTypes.NameIdentifier)
            If userIdClaim Is Nothing Then
                Logger.LogError("Token no contiene el id del usuario")
                Throw New UnauthorizedAccessException("Token no contiene el id del usuario.")
            End If

            Dim userId As Integer
            If Integer.TryParse(userIdClaim.Value, userId) Then
                Return userId
            Else
                Logger.LogError("Id de usuario invalido en token")
                Throw New UnauthorizedAccessException("Id de usuario inválido en token.")
            End If
        End Function

        Public Async Function PuedeGenerarImagen(idUsuario As Integer) As Task(Of Boolean)
            Try
                Dim usuarioRepo As New UsuarioRepository()
                Dim imagenRepo As New ImagenRepository()
                Dim nivelRepo As New NivelAccesoRepository()

                ' 1. Obtener el usuario
                Dim usuario = Await usuarioRepo.ObtenerPorIdAsync(idUsuario)
                If usuario Is Nothing Then
                    Logger.LogWarning($"Usuario no encontrado: {idUsuario}")
                    Return False
                End If

                ' 2. Obtener el nivel de acceso
                Dim nivel = Await nivelRepo.ObtenerNivelPorId(usuario.NivelAcceso)
                If nivel Is Nothing Then
                    Logger.LogWarning($"Nivel de acceso no encontrado para usuario {idUsuario}")
                    Return False
                End If

                ' 3. Contar imágenes generadas hoy
                Dim cantidadHoy = Await imagenRepo.ObtenerCantidadImagenesGeneradasHoy(idUsuario)

                ' 4. Comparar
                If cantidadHoy >= nivel.cantidad Then
                    Logger.LogInfo($"Usuario {idUsuario} ha alcanzado el límite diario de imágenes ({nivel.cantidad})")
                    Return False
                End If

                Return True

            Catch ex As Exception
                Logger.LogError($"Error en PuedeGenerarImagen: {ex.Message}")
                Return False
            End Try
        End Function

    End Class
End Namespace

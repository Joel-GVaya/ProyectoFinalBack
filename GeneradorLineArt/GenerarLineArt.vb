Imports System.Configuration
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Text
Imports System.Diagnostics
Imports System.Security.Cryptography
Imports System.Runtime.InteropServices
Imports System.Linq

' *** Modulo Unico que contiene AMBAS funcionalidades ***
Module ProcesadorImagenes

    ' --- Constantes para tipos de operacion ---
    Public Const TIPO_OPERACION_LINEART_DESDE_IMAGEN As Integer = 1
    Public Const TIPO_OPERACION_IMAGEN_DESDE_PROMPT As Integer = 2

    ' --- Rutas y Configuracion (Consolidadas) ---
    Private ReadOnly ConfiguredBaseImagenesPath As String = ConfigurationManager.AppSettings("RutaBaseImagenes")
    Private ReadOnly BaseImagenesPath As String = Path.GetFullPath(ConfiguredBaseImagenesPath)
    Private ReadOnly BaseUploadPath As String = Path.Combine(BaseImagenesPath, "Subidas") ' Necesaria para LineArt desde Imagen
    Private ReadOnly BaseLineArtPath As String = Path.Combine(BaseImagenesPath, "LineArt") ' Salida para ambos scripts
    Private ReadOnly BaseMarcaAguaPath As String = Path.Combine(BaseImagenesPath, "MarcaAgua") ' Salida final con marca
    Private ReadOnly LogoPath As String = Path.Combine(BaseImagenesPath, "logoBlack.png")
    ' --- Rutas a AMBOS scripts ---
    Private ReadOnly LineArtScriptPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Script", "scrypt.exe") ' Para LineArt desde imagen
    Private ReadOnly ImageScriptPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Script", "scryptImage.exe") ' Para Imagen desde prompt
    Private ReadOnly ApiKey As String = If(ConfigurationManager.AppSettings("ApiKey"), "TU_API_KEY_POR_DEFECTO_SI_FALTA_EN_CONFIG")
    Private ReadOnly TimeoutScriptMs As Integer = If(Integer.TryParse(ConfigurationManager.AppSettings("TimeoutPythonScriptMs"), TimeoutScriptMs), TimeoutScriptMs, 600000) ' Default 10 minutos

    ' ==========================================================================
    ' --- PUNTO DE ENTRADA UNIFICADO (Para ser llamado por API o Main) ---
    ' ==========================================================================

    ''' <summary>
    ''' Procesa una entrada (nombre de archivo o prompt) segun el tipo de operacion especificado.
    ''' </summary>
    ''' <param name="tipoOperacion">1 para LineArt desde Imagen, 2 para Imagen desde Prompt.</param>
    ''' <param name="entrada">Nombre del archivo (si tipo=1) o Prompt (si tipo=2).</param>
    ''' <returns>Nombre del archivo final con marca de agua si tiene exito, String.Empty en caso contrario.</returns>
    Public Function ProcesarEntrada(tipoOperacion As Integer, entrada As String) As String
        Logger.LogInfo($"ProcesarEntrada llamado con Tipo: {tipoOperacion}, Entrada: '{entrada}'")

        Select Case tipoOperacion
            Case TIPO_OPERACION_LINEART_DESDE_IMAGEN
                Logger.LogInfo("Llamando al script srypt.exe")
                Return EjecutarFlujoLineArtDesdeArchivo(entrada) ' Llama a la funcion especifica
            Case TIPO_OPERACION_IMAGEN_DESDE_PROMPT
                Logger.LogInfo("Llamando al script sryptImage.exe")
                Return EjecutarFlujoImagenDesdePrompt(entrada) ' Llama a la funcion especifica
            Case Else
                Logger.LogError($"Error: Tipo de operacion no valido: ", New Exception(tipoOperacion))
                Return String.Empty
        End Select
    End Function

    ' ==========================================================================
    ' --- SUB MAIN (Punto de entrada para ejecucion desde Consola/EXE) ---
    ' ==========================================================================

    Sub Main(args As String())
        Logger.LogInfo("--- Iniciando Procesador de Imagenes ---") ' Mensaje de inicio

        ' --- Validacion de Argumentos de Consola ---
        If args Is Nothing OrElse args.Length < 2 Then
            Logger.LogError("Error: Se requieren 2 argumentos:", New Exception(""))
            Logger.LogError("  1. Tipo de operacion (1: LineArt desde Imagen, 2: Imagen desde Prompt)", New Exception)
            Logger.LogError("  2. Entrada (Nombre de archivo o Prompt entre comillas)", New Exception)
            Logger.LogError("Ejemplo LineArt: ProcesadorImagenes.exe 1 mi_imagen.jpg", New Exception)
            Logger.LogError("Ejemplo Prompt: ProcesadorImagenes.exe 2 ""Un gato con botas espaciales""", New Exception)
            Logger.LogError("Error de argumentos: No se proporcionaron suficientes argumentos.", New ArgumentException("Faltan argumentos"))
            Environment.ExitCode = 1 ' Codigo de error de argumentos
            Return
        End If

        Dim tipoOpArg As String = args(1)
        Dim entradaArg As String = args(0) ' El prompt o nombre de archivo
        Dim tipoOperacion As Integer
        Logger.LogInfo($"Tipo de operacion: {tipoOpArg}, Prompt o nombre del archivo:{entradaArg}")

        ' Validar y convertir el tipo de operacion
        If Not Integer.TryParse(tipoOpArg, tipoOperacion) OrElse (tipoOperacion <> TIPO_OPERACION_LINEART_DESDE_IMAGEN AndAlso tipoOperacion <> TIPO_OPERACION_IMAGEN_DESDE_PROMPT) Then
            Console.Error.WriteLine($"Error: El primer argumento '{tipoOpArg}' debe ser {TIPO_OPERACION_LINEART_DESDE_IMAGEN} o {TIPO_OPERACION_IMAGEN_DESDE_PROMPT}.")
            Logger.LogError($"Error de argumentos: Tipo de operacion invalido: {tipoOpArg}", New ArgumentException("Tipo de operación inválido"))
            Environment.ExitCode = 1
            Return
        End If

        If String.IsNullOrWhiteSpace(entradaArg) Then
            Console.Error.WriteLine("Error: El segundo argumento (entrada) no puede estar vacío.")
            Logger.LogError("Error de argumentos: La entrada esta vacia.", New ArgumentException("Entrada vacía"))
            Environment.ExitCode = 1
            Return
        End If

        Console.WriteLine($"Tipo Operacion: {tipoOperacion}")
        Console.WriteLine($"Entrada: '{entradaArg}'")
        Console.WriteLine("Procesando...")

        ' --- Llamar al Punto de Entrada Unificado ---
        Dim nombreArchivoResultado As String = String.Empty
        Try
            nombreArchivoResultado = ProcesarEntrada(tipoOperacion, entradaArg)
        Catch ex As Exception
            ' Captura excepciones generales que puedan surgir de ProcesarEntrada
            Console.Error.WriteLine($"Error inesperado durante el procesamiento: {ex.Message}")
            Logger.LogError($"Excepcion no controlada en Main llamando a ProcesarEntrada: {ex.ToString()}", ex)
            Environment.ExitCode = 2 ' Codigo de error de procesamiento
            Return
        End Try

        ' --- Informar Resultado ---
        If Not String.IsNullOrEmpty(nombreArchivoResultado) Then
            Console.WriteLine($"--- Proceso completado con éxito ---")
            Console.WriteLine($"Archivo final generado: {nombreArchivoResultado}")
            Logger.LogInfo($"Main completado con exito. Resultado: {nombreArchivoResultado}")
            Environment.ExitCode = 0 ' Exito
        Else
            Console.Error.WriteLine("--- El proceso falló ---")
            Console.Error.WriteLine("Revise los logs para más detalles.")
            Logger.LogError("Main finalizado con fallo (ProcesarEntrada devolvio vacio).", New Exception("ProcesarEntrada no ha devuelto nada"))
            Environment.ExitCode = 3 ' Codigo de error especifico de fallo en logica interna
        End If

    End Sub


    ' ==========================================================================
    ' --- LOGICA ESPECIFICA PARA IMAGEN DESDE PROMPT (Refactorizada) ---
    ' ==========================================================================

    ''' <summary>
    ''' Orquesta el flujo completo para generar una imagen desde un prompt.
    ''' </summary>
    Private Function EjecutarFlujoImagenDesdePrompt(prompt As String) As String
        ' --- Validaciones Iniciales Especificas ---
        If String.IsNullOrWhiteSpace(prompt) Then
            Logger.LogError("Error VB (ImagenDesdePrompt): El prompt no puede estar vacio.", New ArgumentException("El prompt esta vacio."))
            Return String.Empty
        End If
        If Not ValidarConfiguracionBasica(ImageScriptPath) Then Return String.Empty ' Valida API, Logo, Script

        ' --- Asegurar directorios de salida ---
        If Not PrepararDirectoriosSalida() Then Return String.Empty

        ' --- Preparar Ejecucion del Script de Imagen ---
        Dim nombreBaseSeguro As String = GenerarNombreArchivoSeguro(prompt) ' <-- OBTENER SOLO EL NOMBRE BASE
        Dim nombreArchivoEsperado As String = nombreBaseSeguro & ".png"
        Dim rutaImagenGenerada As String = Path.Combine(BaseLineArtPath, nombreArchivoEsperado) ' Ruta donde scryptImage debe guardar

        ' Parametros: "prompt" "RUTA_Guardar_Imagen" "nombre_guardarImagen"
        Dim arguments As String = String.Format("""{0}"" ""{1}"" ""{2}""", prompt, BaseLineArtPath, nombreBaseSeguro)

        Logger.LogInfo(String.Format("VB (ImagenDesdePrompt): Ejecutando script: ""{0}"" {1}", ImageScriptPath, arguments))

        Dim resultadoEjecucion = EjecutarScriptExterno(ImageScriptPath, arguments, prompt)

        ' --- Verificar Resultado de la Generacion ---
        If resultadoEjecucion Is Nothing OrElse resultadoEjecucion.Value.ExitCode <> 0 OrElse resultadoEjecucion.Value.TimedOut Then
            ' El error ya se logueo en EjecutarScriptExterno o es Nothing
            Return String.Empty
        End If

        ' *** Verificacion CRUCIAL: ¿Existe el archivo generado? ***
        If Not File.Exists(rutaImagenGenerada) Then
            Logger.LogError(String.Format("Error VB (ImagenDesdePrompt): Script termino OK pero no se encontro archivo en: {0}", rutaImagenGenerada), New FileNotFoundException("Archivo generado por script no encontrado.", rutaImagenGenerada))
            Return String.Empty
        End If
        Logger.LogInfo(String.Format("VB (ImagenDesdePrompt): Script finalizado. Archivo generado: {0}", rutaImagenGenerada))

        ' --- Añadir Marca de Agua ---
        Logger.LogInfo(String.Format("VB (ImagenDesdePrompt): Anadiendo marca de agua a {0}...", nombreArchivoEsperado))
        If AnadirMarcaDeAgua(nombreArchivoEsperado, BaseLineArtPath, BaseMarcaAguaPath, LogoPath) Then
            Return nombreArchivoEsperado ' Exito: devuelve el nombre del archivo
        Else
            Logger.LogError("Error VB (ImagenDesdePrompt): Fallo la adicion de la marca de agua.", New Exception("Fallo marca de agua"))
            Return String.Empty
        End If
    End Function

    ' ==========================================================================
    ' --- LOGICA ESPECIFICA PARA LINEART DESDE IMAGEN (Refactorizada) ---
    ' ==========================================================================

    ''' <summary>
    ''' Orquesta el flujo completo para generar Line Art desde un archivo de imagen.
    ''' </summary>
    Private Function EjecutarFlujoLineArtDesdeArchivo(inputFileNameOnly As String) As String
        ' 1. Validar el nombre del archivo de entrada
        If Not ValidarInputFileName(inputFileNameOnly) Then Return String.Empty

        ' 2. Construir la ruta completa al archivo de entrada y verificar que exista
        Dim fullInputImagePath As String = Path.Combine(BaseUploadPath, inputFileNameOnly)
        If Not File.Exists(fullInputImagePath) Then
            Logger.LogError($"Error VB (LineArtDesdeArchivo): No se encuentra archivo de entrada: {fullInputImagePath}", New FileNotFoundException("Archivo subido no encontrado.", fullInputImagePath))
            Return String.Empty
        End If

        ' 3. Validar configuracion basica (API, logo, existencia del script)
        If Not ValidarConfiguracionBasica(LineArtScriptPath) Then Return String.Empty

        ' 4. Asegurar que los directorios de salida existan
        If Not PrepararDirectoriosSalida() Then Return String.Empty

        ' 5. Ejecutar el script y verificar el archivo. Esta funcion ahora devuelve la RUTA COMPLETA.
        Dim rutaCompletaLineArtGenerado As String = EjecutarYVerificarScriptLineArt(fullInputImagePath)

        If String.IsNullOrEmpty(rutaCompletaLineArtGenerado) Then
            ' El error ya fue logueado dentro de EjecutarYVerificarScriptLineArt
            Return String.Empty
        End If

        ' 6. Añadir marca de agua y devolver el nombre del archivo (no la ruta completa)
        Return AnadirMarcaDeAguaYDevolverResultado(rutaCompletaLineArtGenerado)
    End Function

    Private Function EjecutarYVerificarScriptLineArt(fullInputImagePath As String) As String
        ' Argumentos para el script Python:
        ' 1. Ruta completa del archivo de imagen de entrada.
        ' 2. Ruta de la carpeta donde el script Python debe guardar el archivo LineArt.
        Dim arguments As String = String.Format("""{0}"" ""{1}""", fullInputImagePath, BaseLineArtPath)
        Logger.LogInfo($"VB (LineArtDesdeArchivo): Ejecutando script: ""{LineArtScriptPath}"" {arguments}")

        Dim resultadoEjecucion = EjecutarScriptExterno(LineArtScriptPath, arguments, Path.GetFileName(fullInputImagePath))

        ' 1. Verificar problemas con la ejecucion del proceso en si (timeout, no se pudo iniciar)
        If resultadoEjecucion Is Nothing OrElse resultadoEjecucion.Value.TimedOut Then
            ' Error ya logueado en EjecutarScriptExterno o fue timeout
            Return String.Empty
        End If

        ' 2. Verificar el codigo de salida del script
        If resultadoEjecucion.Value.ExitCode <> 0 Then
            Logger.LogError($"Error VB (LineArtDesdeArchivo): Script Line Art '{Path.GetFileName(LineArtScriptPath)}' termino con codigo de error {resultadoEjecucion.Value.ExitCode}. " &
                        $"Entrada: '{Path.GetFileName(fullInputImagePath)}'. Salida Script: '{resultadoEjecucion.Value.Output}'. Error Script: '{resultadoEjecucion.Value.ErrorOutput}'", New Exception($"Script Line Art falló con código {resultadoEjecucion.Value.ExitCode}"))
            Return String.Empty
        End If

        ' 3. Extraer el NOMBRE del archivo de la salida del script Python
        Dim nombreArchivoLineArtDevueltoPorScript As String = ExtraerNombreArchivoDesdeSalida(resultadoEjecucion.Value.Output)

        If String.IsNullOrWhiteSpace(nombreArchivoLineArtDevueltoPorScript) Then
            Logger.LogError($"Error VB (LineArtDesdeArchivo): Script Line Art ('{Path.GetFileName(LineArtScriptPath)}') para '{Path.GetFileName(fullInputImagePath)}' devolvio un nombre de archivo vacio o invalido. " &
                        $"Salida bruta del script: '{resultadoEjecucion.Value.Output}'. Salida error script: '{resultadoEjecucion.Value.ErrorOutput}'",
                        New Exception("Nombre de archivo devuelto por script Line Art es vacío o inválido."))
            Return String.Empty
        End If

        ' 4. Construir la RUTA COMPLETA donde el script DEBERIA haber guardado el archivo
        Dim rutaCompletaLineArtGenerado As String = Path.Combine(BaseLineArtPath, nombreArchivoLineArtDevueltoPorScript)

        ' 5. Verificar si el archivo existe en la ruta construida (con reintentos)
        Dim fileFound As Boolean = False
        Dim intentosMax As Integer = 3 ' Intentar 3 veces (puedes ajustar esto)
        Dim esperaMs As Integer = 250  ' Esperar 250ms entre intentos (puedes ajustar esto)

        For i As Integer = 1 To intentosMax
            If File.Exists(rutaCompletaLineArtGenerado) Then
                fileFound = True
                Exit For
            End If
            If i < intentosMax Then
                Logger.LogInfo($"VB (LineArtDesdeArchivo): Archivo '{rutaCompletaLineArtGenerado}' (devuelto como '{nombreArchivoLineArtDevueltoPorScript}' por script) no encontrado (intento {i}/{intentosMax}). Reintentando en {esperaMs}ms...")
                System.Threading.Thread.Sleep(esperaMs) ' Pausa
            End If
        Next

        If Not fileFound Then
            Logger.LogError($"Error VB (LineArtDesdeArchivo): Script Line Art devolvio nombre '{nombreArchivoLineArtDevueltoPorScript}', " &
                        $"pero el archivo NO SE ENCUENTRA en la ruta esperada '{rutaCompletaLineArtGenerado}' despues de {intentosMax} intentos. " &
                        $"Salida original del script: '{resultadoEjecucion.Value.Output}'. Error Script: '{resultadoEjecucion.Value.ErrorOutput}'",
                        New FileNotFoundException("Archivo Line Art no encontrado en la ubicación esperada después de la ejecución del script.", rutaCompletaLineArtGenerado))
            Return String.Empty
        End If

        Logger.LogInfo($"VB (LineArtDesdeArchivo): Script Line Art finalizado correctamente. Archivo generado verificado en: {rutaCompletaLineArtGenerado}")
        Return rutaCompletaLineArtGenerado ' Devolver la RUTA COMPLETA del archivo generado
    End Function

    Private Function AnadirMarcaDeAguaYDevolverResultado(rutaCompletaLineArtGenerado As String) As String
        ' rutaCompletaLineArtGenerado es la RUTA COMPLETA del archivo line art.
        ' Por ejemplo: "C:\datos\svn\...\Imagenes\LineArt\0cd489fa454b4a6998b49cb19455af69.png"

        Dim nombreArchivoLineArt As String = Path.GetFileName(rutaCompletaLineArtGenerado)
        ' nombreArchivoLineArt sera "0cd489fa454b4a6998b49cb19455af69.png"

        Logger.LogInfo(String.Format("VB (LineArtDesdeArchivo): Anadiendo marca de agua a {0} (ubicado en {1})...", nombreArchivoLineArt, Path.GetDirectoryName(rutaCompletaLineArtGenerado)))

        ' AnadirMarcaDeAgua(inputFileNameOnly As String, inputFolderPath As String, ...)
        ' inputFileNameOnly: nombreArchivoLineArt (correcto)
        ' inputFolderPath: BaseLineArtPath (porque ahi es donde esta el archivo, segun rutaCompletaLineArtGenerado)
        If AnadirMarcaDeAgua(nombreArchivoLineArt, BaseLineArtPath, BaseMarcaAguaPath, LogoPath) Then
            Return nombreArchivoLineArt ' Exito: devuelve solo el nombre del archivo
        Else
            Logger.LogError($"Error VB (LineArtDesdeArchivo): Fallo la adicion de la marca de agua a {nombreArchivoLineArt}.", New Exception("Fallo marca de agua Line Art"))
            Return String.Empty
        End If
    End Function

    Private Function ValidarInputFileName(inputFileName As String) As Boolean
        If String.IsNullOrWhiteSpace(inputFileName) Then
            Logger.LogError("Error VB (LineArtDesdeArchivo): Nombre de archivo vacio.", New ArgumentException("inputFileName está vacío."))
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Ejecuta un script externo, captura su salida y maneja errores comunes.
    ''' </summary>
    ''' <returns>Nullable Tupla con el resultado, o Nothing si hubo excepcion al iniciar.</returns>
    Private Function EjecutarScriptExterno(scriptPath As String, arguments As String, inputIdentifierForLog As String) As Nullable(Of (ExitCode As Integer, Output As String, ErrorOutput As String, TimedOut As Boolean))
        Dim startInfo As ProcessStartInfo = ConfigurarProceso(scriptPath, arguments)
        Return EjecutarYCapturarSalida(startInfo, inputIdentifierForLog)
    End Function

    Private Function ConfigurarProceso(scriptPath As String, arguments As String) As ProcessStartInfo
        Dim startInfo As New ProcessStartInfo()
        startInfo.FileName = scriptPath
        startInfo.Arguments = arguments
        startInfo.UseShellExecute = False
        startInfo.RedirectStandardOutput = True
        startInfo.RedirectStandardError = True
        startInfo.CreateNoWindow = True
        startInfo.StandardOutputEncoding = Encoding.UTF8
        startInfo.StandardErrorEncoding = Encoding.UTF8
        startInfo.WorkingDirectory = Path.GetDirectoryName(scriptPath)
        Return startInfo
    End Function

    Private Function EjecutarYCapturarSalida(startInfo As ProcessStartInfo, inputIdentifierForLog As String) As Nullable(Of (ExitCode As Integer, Output As String, ErrorOutput As String, TimedOut As Boolean))
        Try
            Using process As New Process()
                process.StartInfo = startInfo
                Return EjecutarProcesoYCapturarSalidaInterno(process, TimeoutScriptMs, inputIdentifierForLog)
            End Using
        Catch ex As Exception
            Logger.LogError(String.Format("Error VB: Excepcion al ejecutar/gestionar script '{0}' para '{1}': {2}", Path.GetFileName(startInfo.FileName), inputIdentifierForLog, ex.Message), ex)
            Return Nothing
        End Try
    End Function

    ' ==========================================================================
    ' --- FUNCIONES AUXILIARES COMUNES (Consolidadas) ---
    ' ==========================================================================

    ''' <summary>
    ''' Valida configuracion comun: API Key, Logo, y existencia del Script especifico.
    ''' </summary>
    Private Function ValidarConfiguracionBasica(scriptPath As String) As Boolean
        If String.IsNullOrWhiteSpace(ApiKey) OrElse ApiKey = "TU_API_KEY_POR_DEFECTO_SI_FALTA_EN_CONFIG" Then
            Logger.LogError("Error VB: La clave API no esta configurada correctamente.", New ConfigurationErrorsException("Clave API no configurada."))
            Return False
        End If
        If Not File.Exists(LogoPath) Then
            Logger.LogError($"Error VB: No se encuentra el archivo de logo: {LogoPath}", New FileNotFoundException("Logo no encontrado.", LogoPath))
            Return False
        End If
        If Not File.Exists(scriptPath) Then
            Logger.LogError($"Error VB: No se encuentra el script ejecutable: {scriptPath}", New FileNotFoundException("Script no encontrado.", scriptPath))
            Return False
        End If
        Return True ' Todo OK
    End Function

    ''' <summary>
    ''' Asegura que los directorios BaseLineArtPath y BaseMarcaAguaPath existan.
    ''' </summary>
    Private Function PrepararDirectoriosSalida() As Boolean
        Try
            If Not Directory.Exists(BaseLineArtPath) Then Directory.CreateDirectory(BaseLineArtPath)
            If Not Directory.Exists(BaseMarcaAguaPath) Then Directory.CreateDirectory(BaseMarcaAguaPath)
            Return True
        Catch ex As Exception
            Logger.LogError($"Error VB: Creando carpetas de salida ({BaseLineArtPath}, {BaseMarcaAguaPath}): {ex.Message}", New IOException("Error al crear carpetas de salida.", ex))
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Logica interna para ejecutar un proceso ya configurado y capturar salida.
    ''' </summary>
    Private Function EjecutarProcesoYCapturarSalidaInterno(ByVal process As Process, ByVal timeoutMilliseconds As Integer, ByVal inputIdentifierForLog As String) As (ExitCode As Integer, Output As String, ErrorOutput As String, TimedOut As Boolean)
        Dim processOutputBuilder As New StringBuilder()
        Dim processErrorOutputBuilder As New StringBuilder()
        Dim exitCode As Integer = -1
        Dim timedOut As Boolean = False
        Dim processNameForLog As String = Path.GetFileName(process.StartInfo.FileName)

        Try
            Using outputWaitHandle As New System.Threading.AutoResetEvent(False)
                Using errorWaitHandle As New System.Threading.AutoResetEvent(False)

                    Dim outputHandler As DataReceivedEventHandler = Sub(sender, e)
                                                                        If e.Data IsNot Nothing Then
                                                                            processOutputBuilder.AppendLine(e.Data)
                                                                        Else
                                                                            ' Señal de que el stream de salida ha terminado
                                                                            Try
                                                                                outputWaitHandle.Set()
                                                                            Catch ex As ObjectDisposedException
                                                                                ' Puede ocurrir si el proceso termina abruptamente
                                                                                Logger.LogError($"VB: outputWaitHandle.Set() fallo para {processNameForLog} debido a ObjectDisposedException.", New Exception)
                                                                            End Try
                                                                        End If
                                                                    End Sub
                    AddHandler process.OutputDataReceived, outputHandler

                    Dim errorHandler As DataReceivedEventHandler = Sub(sender, e)
                                                                       If e.Data IsNot Nothing Then
                                                                           processErrorOutputBuilder.AppendLine(e.Data)
                                                                       Else
                                                                           ' Señal de que el stream de error ha terminado
                                                                           Try
                                                                               errorWaitHandle.Set()
                                                                           Catch ex As ObjectDisposedException
                                                                               Logger.LogError($"VB: errorWaitHandle.Set() fallo para {processNameForLog} debido a ObjectDisposedException.", New Exception)
                                                                           End Try
                                                                       End If
                                                                   End Sub
                    AddHandler process.ErrorDataReceived, errorHandler

                    process.Start()
                    process.BeginOutputReadLine()
                    process.BeginErrorReadLine()

                    ' Esperar a que el proceso termine Y a que ambos streams de datos hayan sido completamente leidos.
                    If process.WaitForExit(timeoutMilliseconds) AndAlso
                   outputWaitHandle.WaitOne(Math.Max(100, timeoutMilliseconds \ 4)) AndAlso ' Dar un tiempo razonable para que los streams cierren
                   errorWaitHandle.WaitOne(Math.Max(100, timeoutMilliseconds \ 4)) Then
                        exitCode = process.ExitCode
                        ' Log de exito basico, los detalles de salida se loguearan despues
                    Else
                        timedOut = True ' Se considera timeout si el proceso o los streams no terminan a tiempo
                        Logger.LogError($"Error VB: Script '{processNameForLog}' para '{inputIdentifierForLog}' excedio timeout ({timeoutMilliseconds}ms) o sus streams de E/S no cerraron a tiempo.", New TimeoutException($"Timeout o error de stream para script {processNameForLog}"))
                        Try
                            If Not process.HasExited Then
                                process.Kill()
                                Logger.LogInfo($"VB: Proceso '{processNameForLog}' terminado forzosamente debido a timeout/error de stream.")
                            End If
                        Catch exKill As Exception
                            Logger.LogError($"Error VB: No se pudo terminar proceso '{processNameForLog}' tras timeout/error de stream: {exKill.Message}", exKill)
                        End Try
                        ' Si el proceso termino (HasExited es true) pero los streams no (WaitOne dio false),
                        ' intentamos obtener el ExitCode de todas formas si esta disponible.
                        If process.HasExited AndAlso exitCode = -1 Then exitCode = process.ExitCode
                    End If

                    RemoveHandler process.OutputDataReceived, outputHandler
                    RemoveHandler process.ErrorDataReceived, errorHandler
                End Using
            End Using

            Dim finalOutput As String = processOutputBuilder.ToString().Trim()
            Dim finalErrorOutput As String = processErrorOutputBuilder.ToString().Trim()

            If Not timedOut Then ' Solo loguear detalles si no fue un timeout (el timeout ya tiene su propio log de error)
                Logger.LogInfo($"VB: Script '{processNameForLog}' para '{inputIdentifierForLog}' termino. Codigo Salida: {exitCode}")
                If Not String.IsNullOrWhiteSpace(finalErrorOutput) Then
                    If exitCode = 0 Then
                        Logger.LogError($"VB: Script '{processNameForLog}' para '{inputIdentifierForLog}' termino con ExitCode 0 pero tuvo salida en Stderr: {finalErrorOutput}", New Exception)
                    Else
                        Logger.LogError($"VB: Script '{processNameForLog}' para '{inputIdentifierForLog}' termino con error (Codigo: {exitCode}). Salida error: {finalErrorOutput}", New Exception($"Error del script {processNameForLog}"))
                    End If
                End If
                If Not String.IsNullOrWhiteSpace(finalOutput) Then
                    Logger.LogInfo($"VB: Salida estandar script '{processNameForLog}' para '{inputIdentifierForLog}': {finalOutput}")
                ElseIf exitCode = 0 Then
                    Logger.LogInfo($"VB: Salida estandar script '{processNameForLog}' para '{inputIdentifierForLog}' estuvo vacia (termino OK).")
                End If
            Else
                ' Si hubo timeout, aun podemos loguear lo que se haya capturado
                If Not String.IsNullOrWhiteSpace(finalOutput) Then Logger.LogInfo($"VB (Timeout): Salida estandar parcial de '{processNameForLog}': {finalOutput}")
                If Not String.IsNullOrWhiteSpace(finalErrorOutput) Then Logger.LogInfo($"VB (Timeout): Salida de error parcial de '{processNameForLog}': {finalErrorOutput}")
            End If


        Catch startEx As Exception
            Logger.LogError($"Error VB: No se pudo iniciar o gestionar proceso '{processNameForLog}' para '{inputIdentifierForLog}': {startEx.Message}", startEx)
            Return (-1, String.Empty, startEx.Message, False) ' Error al iniciar el proceso
        End Try

        Return (exitCode, processOutputBuilder.ToString().Trim(), processErrorOutputBuilder.ToString().Trim(), timedOut)
    End Function

    Private Function GenerarNombreArchivoSeguro(prompt As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(prompt & DateTime.Now.ToString("yyyyMMddHHmmssfff"))
            Dim hashBytes As Byte() = sha256.ComputeHash(bytes)
            Dim sb As New StringBuilder()
            For i As Integer = 0 To hashBytes.Length - 1
                sb.Append(hashBytes(i).ToString("x2"))
            Next
            Return "img_" & sb.ToString().Substring(0, 32)
        End Using
    End Function

    Private Function ExtraerRutaDesdeSalida(ByVal rawOutput As String) As String
        If String.IsNullOrWhiteSpace(rawOutput) Then Return String.Empty
        Dim lines = rawOutput.Split(New Char() {ControlChars.Lf, ControlChars.Cr}, StringSplitOptions.RemoveEmptyEntries)
        Dim lastLine As String = lines.LastOrDefault()
        Return If(lastLine IsNot Nothing, lastLine.Trim(), String.Empty)
    End Function

    ' --- FUNCIONES PARA ANADIR MARCA DE AGUA (Sin cambios) ---
    Public Function AnadirMarcaDeAgua(ByVal inputFileNameOnly As String, ByVal inputFolderPath As String, ByVal outputMarcaAguaFolderPath As String, ByVal logoFullPath As String) As Boolean
        Dim fullImagePath As String = Path.Combine(inputFolderPath, inputFileNameOnly)
        Dim outputFileName As String = Path.Combine(outputMarcaAguaFolderPath, inputFileNameOnly)

        If Not ValidarArchivosEntrada(fullImagePath, logoFullPath) Then Return False
        Try
            Using imagenBase As Bitmap = CargarBitmap(fullImagePath)
                If imagenBase Is Nothing Then Return False
                Using logo As Bitmap = CargarBitmap(logoFullPath)
                    If logo Is Nothing Then Return False
                    If Not ValidarDimensionesImagen(imagenBase, logo, inputFileNameOnly) Then Return False
                    AplicarMarcaDeAguaEnImagen(imagenBase, logo)
                    ' If Not PrepararDirectorioSalida(outputMarcaAguaFolderPath) Then Return False ' Ya deberia existir, pero validamos
                    If Not GuardarImagenConMarca(imagenBase, outputFileName) Then Return False
                End Using
            End Using
            Logger.LogInfo($"VB (MarcaAgua): Exito para {inputFileNameOnly}. Resultado: {outputFileName}")
            Return True
        Catch ex As Exception
            Logger.LogError($"Error VB (MarcaAgua): Error procesando {inputFileNameOnly}: {ex.Message}", ex)
            Return False
        End Try
    End Function

    Private Function ValidarArchivosEntrada(ByVal baseImagePath As String, ByVal logoPath As String) As Boolean
        If Not File.Exists(baseImagePath) Then
            Logger.LogError($"Error VB (MarcaAgua): No se encuentra imagen base: {baseImagePath}", New FileNotFoundException("No se encuentra imagen base.", baseImagePath))
            Return False
        End If
        If Not File.Exists(logoPath) Then
            Logger.LogError($"Error VB (MarcaAgua): No se encuentra logo: {logoPath}", New FileNotFoundException("No se encuentra logo.", logoPath))
            Return False
        End If
        Return True
    End Function

    Private Function CargarBitmap(ByVal imagePath As String) As Bitmap
        Try
            Using fs As New FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Return New Bitmap(fs)
            End Using
        Catch ex As Exception
            Logger.LogError($"Error VB (MarcaAgua): No se pudo cargar imagen {imagePath}: {ex.Message}", ex)
            Return Nothing
        End Try
    End Function

    Private Function ValidarDimensionesImagen(ByVal imagenBase As Bitmap, ByVal logo As Bitmap, ByVal fileName As String) As Boolean
        If imagenBase.Width <= 0 OrElse imagenBase.Height <= 0 OrElse logo.Width <= 0 OrElse logo.Height <= 0 Then
            Logger.LogError($"Error VB (MarcaAgua) [{fileName}]: Tamanos invalidos. Base:{imagenBase.Width}x{imagenBase.Height}, Logo:{logo.Width}x{logo.Height}", New ArgumentException("Tamanos inválidos."))
            Return False
        End If
        If logo.Width > imagenBase.Width OrElse logo.Height > imagenBase.Height Then
            Logger.LogError($"Error VB (MarcaAgua) [{fileName}]: Logo ({logo.Width}x{logo.Height}) > Imagen ({imagenBase.Width}x{imagenBase.Height}).", New ArgumentException("Logo más grande que imagen."))
            Return False
        End If
        Return True
    End Function

    Private Sub AplicarMarcaDeAguaEnImagen(ByRef imagenBase As Bitmap, ByVal logo As Bitmap)
        Using g As Graphics = Graphics.FromImage(imagenBase)
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality

            ' Escalar el logo (por ejemplo, 120%)
            Dim escala As Single = 1.2F
            Dim logoAncho As Integer = CInt(logo.Width * escala)
            Dim logoAlto As Integer = CInt(logo.Height * escala)

            ' Redimensionar el logo
            Dim logoEscalado As New Bitmap(logo, New Size(logoAncho, logoAlto))

            ' Posicion abajo a la derecha con margen de 10px
            Dim margen As Integer = 10
            Dim x As Integer = imagenBase.Width - logoAncho - margen
            Dim y As Integer = imagenBase.Height - logoAlto - margen

            g.DrawImage(logoEscalado, New Rectangle(x, y, logoAncho, logoAlto))
        End Using
    End Sub


    ' PrepararDirectorioSalida se llama ahora desde la logica principal, no desde AnadirMarcaDeAgua
    ' Private Function PrepararDirectorioSalida(ByVal outputFolderPath As String) As Boolean ...

    Private Function GuardarImagenConMarca(ByVal imagenConMarca As Bitmap, ByVal outputFileName As String) As Boolean
        Try
            Logger.LogInfo($"VB (MarcaAgua): Guardando imagen con marca en: {outputFileName}")
            imagenConMarca.Save(outputFileName, ImageFormat.Png)
            Return True
        Catch exSave As Exception
            Logger.LogError($"Error VB (MarcaAgua): No se pudo guardar {outputFileName}: {exSave.Message}", New IOException($"No se pudo guardar {outputFileName}", exSave))
            Return False
        End Try
    End Function

    Private Function ExtraerNombreArchivoDesdeSalida(ByVal rawOutput As String) As String
        If String.IsNullOrWhiteSpace(rawOutput) Then
            Logger.LogError("VB (ExtraerNombreArchivoDesdeSalida): La salida del script esta vacia o solo contiene espacios en blanco.", New Exception)
            Return String.Empty
        End If

        ' Divide por saltos de linea (LF o CR) y elimina entradas vacias
        Dim lines = rawOutput.Split(New Char() {ControlChars.Lf, ControlChars.Cr}, StringSplitOptions.RemoveEmptyEntries)

        If Not lines.Any() Then ' Verifica si hay alguna linea despues de dividir
            Logger.LogError($"VB (ExtraerNombreArchivoDesdeSalida): La salida del script no contiene lineas validas despues de procesar. Salida original: '{rawOutput}'", New Exception)
            Return String.Empty
        End If

        ' Toma la ultima linea no vacia y la trimea
        Dim nombreArchivo As String = lines.LastOrDefault()?.Trim() ' El '?' es por si LastOrDefault devuelve Nothing

        If String.IsNullOrWhiteSpace(nombreArchivo) Then
            Logger.LogError($"VB (ExtraerNombreArchivoDesdeSalida): La ultima linea de salida del script esta vacia despues del Trim. Salida original: '{rawOutput}'", New Exception)
            Return String.Empty
        End If

        ' Validar que no sea una ruta completa y que no contenga caracteres de ruta invalidos
        If nombreArchivo.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 Then
            Logger.LogError($"Error VB (ExtraerNombreArchivoDesdeSalida): El nombre de archivo '{nombreArchivo}' devuelto por el script contiene caracteres invalidos.", New ArgumentException("Nombre de archivo inválido devuelto por script."))
            Return String.Empty
        End If

        ' Si por alguna razon el script aun devuelve una ruta completa, intentamos obtener solo el nombre.
        If nombreArchivo.Contains(Path.DirectorySeparatorChar) OrElse nombreArchivo.Contains(Path.AltDirectorySeparatorChar) Then
            Logger.LogError($"VB (ExtraerNombreArchivoDesdeSalida): La salida '{nombreArchivo}' parece una ruta completa cuando se esperaba solo un nombre de archivo. Se intentara extraer solo el nombre.", New Exception)
            nombreArchivo = Path.GetFileName(nombreArchivo) ' Extrae el nombre del archivo de la ruta
            If String.IsNullOrWhiteSpace(nombreArchivo) Then ' Si Path.GetFileName devuelve vacio (ej. si termina en '\')
                Logger.LogError($"Error VB (ExtraerNombreArchivoDesdeSalida): No se pudo extraer un nombre de archivo valido de la ruta aparente '{lines.LastOrDefault()?.Trim()}'.", New ArgumentException("No se pudo extraer nombre de ruta aparente."))
                Return String.Empty
            End If
        End If

        Logger.LogInfo($"VB (ExtraerNombreArchivoDesdeSalida): Nombre de archivo extraido de la salida del script: '{nombreArchivo}'")
        Return nombreArchivo
    End Function

End Module
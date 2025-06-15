Imports System.Configuration
Imports System.IO

Public Class Logger
    Private Shared ReadOnly logPath As String = Path.Combine(ConfigurationManager.AppSettings("RutaBaseLogs"), $"logs_{DateTime.Now:yyyy-MM-dd}.txt")



    ''' <summary>
    ''' Registra un mensaje de información en el archivo de logs.
    ''' </summary>
    ''' <param name="mensaje">Mensaje informativo</param>
    Public Shared Sub LogInfo(ByVal mensaje As String)
        Dim logMessage As String = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | INFO | {mensaje}{Environment.NewLine}"
        WriteLog(logMessage)
    End Sub

    ''' <summary>
    ''' Registra un error en el archivo de logs.
    ''' </summary>
    ''' <param name="mensaje">Mensaje personalizado del error</param>
    ''' <param name="ex">Excepción capturada</param>
    Public Shared Sub LogError(ByVal mensaje As String, ByVal ex As Exception)
        Dim logMessage As String = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | ERROR | {mensaje}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}"
        WriteLog(logMessage)
    End Sub

    ''' <summary>
    ''' Escribe el mensaje en el archivo de logs
    ''' </summary>
    ''' <param name="logMessage">Mensaje a escribir en el log</param>
    Private Shared Sub WriteLog(ByVal logMessage As String)
        Try
            ' Escribir en el archivo de logs
            File.AppendAllText(logPath, logMessage)
        Catch fileEx As Exception
#If DEBUG Then
            Console.WriteLine($"Error escribiendo en el log: {fileEx.Message}")
#End If
        End Try
    End Sub
End Class

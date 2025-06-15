Imports System.Configuration
Imports System.IO

Public Class Logger
    Private Shared ReadOnly logPath As String = Path.Combine("C:\Users\garci\Documents\ProyectoFinalBUENO\Data\Logs", $"logs_{DateTime.Now:yyyy-MM-dd}.txt")


    ''' <summary>
    ''' Registra un mensaje de información en el archivo de logs.
    ''' </summary>
    ''' <param name="mensaje">Mensaje informativo</param>
    Public Shared Sub LogInfo(ByVal mensaje As String)
        WriteLog("INFO", mensaje)
    End Sub

    ''' <summary>
    ''' Registra un error en el archivo de logs.
    ''' </summary>
    ''' <param name="mensaje">Mensaje personalizado del error</param>
    Public Shared Sub LogError(ByVal mensaje As String)
        WriteLog("ERROR", $"{mensaje}")
    End Sub

    ''' <summary>
    ''' Registra un mensaje de advertencia en el archivo de logs.
    ''' </summary>
    ''' <param name="mensaje">Mensaje de advertencia</param>
    Public Shared Sub LogWarning(ByVal mensaje As String)
        WriteLog("WARNING", mensaje)
    End Sub

    ''' <summary>
    ''' Registra un mensaje de depuración en el archivo de logs.
    ''' </summary>
    ''' <param name="mensaje">Mensaje de depuración</param>
    Public Shared Sub LogDebug(ByVal mensaje As String)
#If DEBUG Then
        WriteLog("DEBUG", mensaje)
#End If
    End Sub

    ''' <summary>
    ''' Escribe el mensaje en el archivo de logs.
    ''' </summary>
    ''' <param name="nivel">Nivel del log (INFO, ERROR, DEBUG, WARNING)</param>
    ''' <param name="mensaje">Mensaje a escribir en el log</param>
    Private Shared Sub WriteLog(ByVal nivel As String, ByVal mensaje As String)
        Try
            Dim logMessage As String = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {nivel} | {mensaje}{Environment.NewLine}"
            File.AppendAllText(logPath, logMessage)
        Catch fileEx As Exception
#If DEBUG Then
            Console.WriteLine($"Error escribiendo en el log: {fileEx.Message}")
#End If
        End Try
    End Sub
End Class

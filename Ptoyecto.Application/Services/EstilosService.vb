Imports Proyecto.Data
Imports Proyecto.Modelos
Imports Proyecto.Utils

Public Class EstilosService

    Private ReadOnly _repo As New EstilosRepository()

    Public Async Function CargarEstilos() As Task(Of List(Of EstiloDTO))
        Dim estilos = Await _repo.ObtenerTodosLosEstilos()

        ' Mapear a DTO excluyendo el campo Prompt
        Dim estilosDTO = estilos.Select(Function(e) New EstiloDTO With {
            .Id = e.Id,
            .Nombre = e.Nombre,
            .Imagen = e.Imagen
        }).ToList()
        Logger.LogInfo("Cargando estilos para el usuario")
        Return estilosDTO
    End Function

End Class

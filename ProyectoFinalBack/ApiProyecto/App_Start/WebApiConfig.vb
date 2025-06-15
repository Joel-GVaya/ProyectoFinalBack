Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web.Http
Imports System.Web.Http.Cors

Public Module WebApiConfig

    Public Sub Register(config As HttpConfiguration)
        ' Habilitar CORS para todos los orígenes, métodos y encabezados
        Dim cors = New EnableCorsAttribute("*", "*", "*")
        config.EnableCors(cors)

        ' Rutas y demás configuración...
        config.MapHttpAttributeRoutes()

        config.Routes.MapHttpRoute(
                name:="DefaultApi",
                routeTemplate:="api/{controller}/{id}",
                defaults:=New With {.id = RouteParameter.Optional}
            )
    End Sub
End Module

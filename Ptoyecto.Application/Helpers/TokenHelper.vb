Imports System.IdentityModel.Tokens.Jwt
Imports Microsoft.IdentityModel.Tokens
Imports System.Security.Claims
Imports System.Text
Imports Proyecto.Modelos
Imports Proyecto.Utils

Public Class TokenHelper

    Private Shared SecretKey As String = "EstaEsUnaClaveSuperSecretaParaFirmarElToken123!"

    Public Shared Function GenerateToken(usuario As Usuario, expireMinutes As Integer) As String
        Dim tokenHandler = New JwtSecurityTokenHandler()
        Dim key = Encoding.ASCII.GetBytes(SecretKey)

        Dim claims = New List(Of Claim) From {
            New Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            New Claim(ClaimTypes.Email, usuario.Correo),
            New Claim(ClaimTypes.Role, usuario.NivelAcceso.ToString())
        }

        Dim tokenDescriptor = New SecurityTokenDescriptor With {
            .Subject = New ClaimsIdentity(claims),
            .Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
            .SigningCredentials = New SigningCredentials(New SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        }

        Dim token = tokenHandler.CreateToken(tokenDescriptor)

        Return tokenHandler.WriteToken(token)
    End Function

    Public Shared Function ValidateTokenAndGetClaims(token As String) As IEnumerable(Of Claim)
        Try
            Dim tokenHandler = New JwtSecurityTokenHandler()
            Dim key = Encoding.ASCII.GetBytes(SecretKey)

            Dim validationParameters = New TokenValidationParameters With {
            .ValidateIssuerSigningKey = True,
            .IssuerSigningKey = New SymmetricSecurityKey(key),
            .ValidateIssuer = False,
            .ValidateAudience = False,
            .ClockSkew = TimeSpan.Zero ' Opcional: evitar tolerancia de tiempo
        }

            Dim principal = tokenHandler.ValidateToken(token, validationParameters, Nothing)

            Return principal.Claims

        Catch ex As Exception
            ' Token inválido o expirado
            Return Nothing
        End Try
    End Function

    Public Shared Function ExtraerUsuarioId(token As String) As Integer
        Try
            Dim claims = ValidateTokenAndGetClaims(token)

            If claims Is Nothing Then
                Throw New Exception("Token inválido o expirado.")
            End If

            ' Buscar el claim que contiene el ID del usuario
            Dim idClaim = claims.FirstOrDefault(Function(c) c.Type = ClaimTypes.NameIdentifier)

            If idClaim Is Nothing OrElse Not Integer.TryParse(idClaim.Value, Nothing) Then
                Throw New Exception("El token no contiene un ID de usuario válido.")
            End If

            Return Integer.Parse(idClaim.Value)

        Catch ex As Exception
            Logger.LogError("Error extrayendo usuarioId desde token: " & ex.Message)
            Throw New Exception("No se pudo extraer el usuarioId del token.")
        End Try
    End Function



End Class

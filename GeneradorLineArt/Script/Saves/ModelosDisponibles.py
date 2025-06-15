import requests
import base64

url = "https://ir-api.myqa.cc/v1/openai/images/generations"
payload = {
    "prompt": "Convierte esta descripción en una imagen: Un lineart de alta calidad para colorear. Usa líneas definidas y ligeramente gruesas para facilitar el coloreado. "
    "Conserva los detalles importantes de la descripción. El fondo debe ser simple o blanco. "
    "Asegúrate de que el dibujo sea completamente en blanco y negro, sin sombras ni colores adicionales. "
    "Mantén un estilo adecuado para niños y para colorear fácilmente. "
    "El resultado debe ser nítido, claro y fácil de colorear. La descripción específica es:Serpiente de agua tomando el sol en un lago",
    "model": "google/gemini-2.0-flash-exp:free"
}
headers = {
    "Authorization": "Bearer 9c353174cfbd55006b7689f82e0222fabe419888de63dc4d9fe0e8271bd58cc2",
    "Content-Type": "application/json"
}

# Realizar la solicitud
response = requests.post(url, json=payload, headers=headers)

# Obtener la respuesta JSON
data = response.json()

# Extraer la cadena base64 de la respuesta
base64_str = data['data'][0]['b64_json']

# Decodificar y guardar como archivo PNG
with open("imagen_generada.png", "wb") as f:
    f.write(base64.b64decode(base64_str))

print("Imagen guardada como 'imagen_generada.png'")

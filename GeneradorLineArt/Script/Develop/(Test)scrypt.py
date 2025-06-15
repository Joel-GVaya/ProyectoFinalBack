# Este codigo no funciona

import requests
import base64

# Use this function to convert an image file from the filesystem to base64
def image_file_to_base64(image_path):
    with open(image_path, 'rb') as f:
        image_data = f.read()
    return base64.b64encode(image_data).decode('utf-8')

# Define the API key and URL
api_key = "SG_0a63cb6ff024b147"
url = "https://api.segmind.com/v1/flux-img2img"

# Define the path to your image
image_path = "C:/datos/svn/201144_mabisy/Utilidades/CreadorDeCuentos/Imagenes/Subidas/8fab0d8a810b4863bb81148d0a84354d.jpg"

# Request payload with the new prompt
data = {
  "prompt": "Convierte esta imagen en un lineart de alta calidad para colorear. "
            "Usa líneas definidas y ligeramente gruesas para facilitar el coloreado. "
            "Conserva los detalles importantes de la imagen original, además mantén el fondo pero de una forma simplificada. "
            "Asegúrate de que el dibujo sea completamente en blanco y negro, sin sombras ni colores adicionales. "
            "Mantén un equilibrio entre fidelidad a la imagen y simplicidad, haciéndolo adecuado para niños. "
            "El resultado debe ser nítido, claro y fácil de colorear.",
  "image": image_file_to_base64(image_path),  # Convertir la imagen desde el archivo
  "steps": 20,
  "seed": 46588,
  "denoise": 0.75,
  "scheduler": "simple",
  "sampler_name": "euler",
  "base64": False
}

# Set the headers for the API request
headers = {'x-api-key': api_key}

# Send the request to the API
response = requests.post(url, json=data, headers=headers)

# Save the response in a .txt file (if the response is a string, or JSON, or some textual data)
with open("response.txt", "w", encoding="utf-8") as file:
    file.write(response.text)  # Save the text of the response into the file

# If the response contains binary data, you could save it as an image (optional):
# with open("generated_image.jpg", "wb") as image_file:
#     image_file.write(response.content)

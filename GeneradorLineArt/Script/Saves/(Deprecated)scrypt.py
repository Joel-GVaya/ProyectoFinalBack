import sys
print("Python executable:", sys.executable)

import base64
import os  # Importa el módulo os
from google import genai
from google.genai import types
from PIL import Image
from io import BytesIO

def process_image(image_path, api_key, output_folder, file_name): # Recibe el nombre de la imagen, falta hacer que la guarde con este nombre + su extension.
    """
    Procesa la imagen y guarda la imagen resultante en un archivo en la carpeta de salida.
    Devuelve la ruta del archivo de salida.
    """
    client = genai.Client(api_key=api_key)

    # Cargar la imagen y codificarla en base64
    with open(image_path, "rb") as image_file:
        encoded_image = base64.b64encode(image_file.read()).decode("utf-8")

    # Combinar texto e imagen en el contenido
    contents = [
        types.ContentDict({"text": "Convierte esta imagen en un lineart de alta calidad para colorear. Usa líneas definidas y ligeramente gruesas para facilitar el coloreado. Conserva los detalles importantes de la imagen original, ademas has mantener el fondo pero de unaConvierte esta imagen en un lineart de alta calidad para colorear. Usa líneas definidas y ligeramente gruesas para facilitar el coloreado. Conserva los detalles importantes de la imagen original, ademas has mantener el fondo pero de una forma simplificada. Asegúrate de que el dibujo sea completamente en blanco y negro, sin sombras ni colores adicionales. Mantén un equilibrio entre fidelidad a la imagen y simplicidad, haciéndolo adecuado para niños. El resultado debe ser nítido, claro y fácil de colorear.  "}),
        types.ContentDict({"inline_data": {"data": encoded_image, "mime_type": "image/jpeg"}})
    ]

    response = client.models.generate_content(
        model="gemini-2.0-flash-exp-image-generation",
        contents=contents,
        config=types.GenerateContentConfig(
            response_modalities=['Text', 'Image']
        )
    )

    for part in response.candidates[0].content.parts:
        if part.inline_data is not None:
            image = Image.open(BytesIO((part.inline_data.data)))
            # Genera un nombre de archivo basado en el parámetro file_name
            output_filename = os.path.join(output_folder, f"{file_name}.png")
            image.save(output_filename, format="PNG")  # Guarda la imagen en un archivo
            return output_filename

    return None # Devuelve None si no se encuentra la imagen

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("Uso: python script.py <ruta_imagen> <api_key> <carpeta_salida>")
        sys.exit(1)

    image_path = sys.argv[1]
    api_key = sys.argv[2]
    output_folder = sys.argv[3] # Recibe la carpeta de salida como argumento
    output_file = process_image(image_path, api_key, output_folder, os.path.splitext(os.path.basename(image_path))[0])

    if output_file:
        print(output_file)  # Imprime la ruta del archivo de salida a stdout
    else:
        print("Error: No se pudo procesar la imagen.")
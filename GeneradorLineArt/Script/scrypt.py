import sys
import base64
import os
import requests

API_KEY = "9c353174cfbd55006b7689f82e0222fabe419888de63dc4d9fe0e8271bd58cc2"

def process_image(image_path, output_folder, file_name):
    """
    Procesa la imagen enviándola a la API externa y guarda el resultado como archivo PNG.
    """
    url = "https://ir-api.myqa.cc/v1/openai/images/edits"
    headers = {
        "Authorization": f"Bearer {API_KEY}"
    }

    with open(image_path, "rb") as img_file:
        files = {
            "image[]": (os.path.basename(image_path), img_file, "image/jpeg")
        }

        payload = {
            "prompt": "Convierte esta imagen en un lineart de alta calidad para colorear. Usa líneas definidas y ligeramente gruesas para facilitar el coloreado. Conserva los detalles importantes de la imagen original, ademas has mantener el fondo pero de una forma simplificada. Asegúrate de que el dibujo sea completamente en blanco y negro, sin sombras ni colores adicionales. Mantén un equilibrio entre fidelidad a la imagen y simplicidad, haciéndolo adecuado para niños. El resultado debe ser nítido, claro y fácil de colorear.",
            "model": "google/gemini-2.0-flash-exp:free",
            "quality": "auto",
            "response_format": "b64_json"
        }

        response = requests.post(url, files=files, data=payload, headers=headers)

        if response.status_code == 200:
            response_data = response.json()
            print("Respuesta JSON completa:")
            print(response.json())

            b64_image = response_data["data"][0]["b64_json"]
            output_filename = os.path.join(output_folder, f"{file_name}.png")

            with open(output_filename, "wb") as output_file:
                output_file.write(base64.b64decode(b64_image))

            return os.path.basename(output_filename)  # Devuelve solo el nombre del archivo
        else:
            print(f"Error en la solicitud: {response.status_code}")
            print(response.text)
            return None

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Uso: python script.py <ruta_imagen> <carpeta_salida>")
        sys.exit(1)

    image_path = sys.argv[1]
    output_folder = sys.argv[2]

    if not os.path.exists(image_path):
        print(f"Error: El archivo '{image_path}' no existe.")
        sys.exit(1)

    # Crear carpeta de salida si no existe
    os.makedirs(output_folder, exist_ok=True)

    output_file = process_image(
        image_path,
        output_folder,
        os.path.splitext(os.path.basename(image_path))[0]
    )

    if output_file:
        print(output_file)  # Imprime solo el nombre del archivo
    else:
        print("Error: No se pudo procesar la imagen.")
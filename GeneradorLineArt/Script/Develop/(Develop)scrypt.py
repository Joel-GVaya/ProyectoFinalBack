# Este scrypt no funciona, no admite proporcionar una imagen en la URL

import os
import sys
import base64
import requests

# --- API Key fija ---
API_KEY = "9c353174cfbd55006b7689f82e0222fabe419888de63dc4d9fe0e8271bd58cc2"
URL = "https://ir-api.myqa.cc/v1/openai/images/transform"  # <-- Asegúrate que esta URL es válida

# --- Prompt fijo ---
BASE_IMAGE_PROMPT = """Convierte esta imagen en un lineart de alta calidad para colorear.
Usa líneas definidas y ligeramente gruesas para facilitar el coloreado.
Conserva los detalles importantes de la imagen original, además mantén el fondo pero de una forma simplificada.
Asegúrate de que el dibujo sea completamente en blanco y negro, sin sombras ni colores adicionales.
Mantén un equilibrio entre fidelidad a la imagen y simplicidad, haciéndolo adecuado para niños.
El resultado debe ser nítido, claro y fácil de colorear."""

def generate_image_from_input_image(image_path, output_folder, output_filename_base):
    try:
        print(f"[DEBUG] Leyendo imagen desde: {image_path}")
        with open(image_path, "rb") as img_file:
            encoded_image = base64.b64encode(img_file.read()).decode("utf-8")
        print(f"[DEBUG] Imagen codificada correctamente.")

        payload = {
            "prompt": BASE_IMAGE_PROMPT,
            "image": encoded_image,
            "model": "google/gemini-2.0-flash-exp:free"
        }

        headers = {
            "Authorization": f"Bearer {API_KEY}",
            "Content-Type": "application/json"
        }

        print(f"[DEBUG] Enviando petición a la API...")
        response = requests.post(URL, json=payload, headers=headers)
        print(f"[DEBUG] Código de estado recibido: {response.status_code}")
        response.raise_for_status()

        data = response.json()
        print(f"[DEBUG] Respuesta JSON recibida: {data}")

        if 'data' not in data or not data['data']:
            print("[ERROR] La respuesta no contiene datos de imagen.")
            return None

        base64_result = data['data'][0].get('b64_json')
        if not base64_result:
            print("[ERROR] No se encontró 'b64_json' en la respuesta.")
            return None

        os.makedirs(output_folder, exist_ok=True)
        output_filepath = os.path.join(output_folder, f"{output_filename_base}.png")

        with open(output_filepath, "wb") as f:
            f.write(base64.b64decode(base64_result))

        print(f"[DEBUG] Imagen guardada en: {output_filepath}")
        return f"{output_filename_base}.png"

    except Exception as e:
        print(f"[ERROR] Excepción ocurrida: {e}")
        return None

# --- Entrada principal ---
if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("Error: Uso: python script.py <ruta_imagen> <carpeta_salida> <nombre_archivo_base>")
        sys.exit(1)

    image_path = sys.argv[1]
    output_folder = sys.argv[2]
    output_filename_base = sys.argv[3]

    print(f"[DEBUG] Parámetros recibidos:\n - Imagen: {image_path}\n - Carpeta: {output_folder}\n - Nombre base: {output_filename_base}")

    result = generate_image_from_input_image(image_path, output_folder, output_filename_base)

    if result:
        print(result)
        sys.exit(0)
    else:
        print("ERROR")
        sys.exit(1)

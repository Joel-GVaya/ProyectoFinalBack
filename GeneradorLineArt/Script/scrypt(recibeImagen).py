import requests
from io import BytesIO
import sys

API_KEY = "9c353174cfbd55006b7689f82e0222fabe419888de63dc4d9fe0e8271bd58cc2"

def transformar_imagen_base64(image_bytes, prompt_text):
    """
    Transforma la imagen usando la API externa y devuelve la imagen resultante en base64 (string).
    :param image_bytes: bytes de la imagen original
    :param prompt_text: texto del prompt para la transformación
    :return: string base64 de la imagen transformada, o None si hay error
    """
    url = "https://ir-api.myqa.cc/v1/openai/images/edits"
    headers = {
        "Authorization": f"Bearer {API_KEY}"
    }

    files = {
        "image[]": ("input.jpg", BytesIO(image_bytes), "image/jpeg")
    }

    payload = {
        "prompt": prompt_text,
        "model": "google/gemini-2.0-flash-exp:free",
        "quality": "auto",
        "response_format": "b64_json"
    }

    response = requests.post(url, files=files, data=payload, headers=headers)

    if response.status_code == 200:
        try:
            response_data = response.json()
            if "data" in response_data and isinstance(response_data["data"], list):
                return response_data["data"][0].get("b64_json")
            else:
                print("ERROR: La respuesta no contiene el campo 'data' o no es una lista.", file=sys.stderr)
                print(response.text, file=sys.stderr)
        except Exception as e:
            print(f"ERROR al parsear JSON: {e}", file=sys.stderr)
            print(response.text, file=sys.stderr)
    else:
        print(f"ERROR HTTP: {response.status_code}", file=sys.stderr)
        print(response.text, file=sys.stderr)

    return None

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Uso: script1.exe <ruta_imagen> <prompt>", file=sys.stderr)
        sys.exit(1)

    ruta_imagen = sys.argv[1]
    prompt = sys.argv[2]

    try:
        with open(ruta_imagen, "rb") as f:
            imagen_bytes = f.read()

        resultado_base64 = transformar_imagen_base64(imagen_bytes, prompt)

        if resultado_base64:
            print(resultado_base64)
        else:
            print("ERROR_GENERANDO_IMAGEN", file=sys.stderr)
            sys.exit(1)

    except Exception as e:
        print(f"ERROR: {str(e)}", file=sys.stderr)
        sys.exit(1)

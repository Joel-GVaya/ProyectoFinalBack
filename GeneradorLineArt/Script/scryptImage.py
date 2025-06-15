import requests
import sys
import base64

# --- API Key y URL ---
API_KEY = "9c353174cfbd55006b7689f82e0222fabe419888de63dc4d9fe0e8271bd58cc2"
URL = "https://ir-api.myqa.cc/v1/openai/images/generations"

def generate_image_base64(base_prompt, user_text):
    full_prompt = base_prompt + user_text

    payload = {
        "prompt": full_prompt,
        "model": "google/gemini-2.0-flash-exp:free",
        "response_format": "b64_json"
    }

    headers = {
        "Authorization": f"Bearer {API_KEY}",
        "Content-Type": "application/json"
    }

    try:
        response = requests.post(URL, json=payload, headers=headers)
        response.raise_for_status()
        data = response.json()

        if "data" in data and isinstance(data["data"], list):
            return data["data"][0].get("b64_json")
        else:
            print("ERROR: Formato inesperado en la respuesta.", file=sys.stderr)
            return None

    except Exception as e:
        print(f"ERROR_GENERANDO_IMAGEN: {e}", file=sys.stderr)
        return None

# --- Entrada principal ---
if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Uso: python script.py \"<base_prompt>\" \"<user_text>\"", file=sys.stderr)
        sys.exit(1)

    base_prompt = sys.argv[1]
    user_text = sys.argv[2]

    result = generate_image_base64(base_prompt, user_text)

    if result:
        print(result)
        sys.exit(0)
    else:
        print("ERROR_GENERANDO_IMAGEN", file=sys.stderr)
        sys.exit(1)

import sys
# Descomentar si quieres ver qué ejecutable de Python se usa
# print("Python executable:", sys.executable)

import base64 # Lo mantenemos por si acaso inline_data necesitara decodificación, aunque probablemente no
import os
# Usamos la estructura de importación del script original funcional
from google import genai
from google.genai import types
from PIL import Image
from io import BytesIO

# --- NUEVO: Prompt base para text-to-image ---
# Puedes ajustar este texto base según necesites
BASE_TEXT_PROMPT = """Convierte esta descripción en una imagen: Un lineart de alta calidad para colorear.
Usa líneas definidas y ligeramente gruesas para facilitar el coloreado.
Conserva los detalles importantes de la descripción. El fondo debe ser simple o blanco.
Asegúrate de que el dibujo sea completamente en blanco y negro, sin sombras ni colores adicionales.
Mantén un estilo adecuado para niños y para colorear fácilmente.
El resultado debe ser nítido, claro y fácil de colorear. La descripción específica es: """
# --- FIN NUEVO ---

# --- Función renombrada y modificada ---
def generate_image_from_text(user_text, api_key, output_folder, output_filename_base):
    """
    Genera una imagen basada en texto usando el modelo experimental y la guarda si tiene éxito.
    Devuelve la ruta del archivo de salida o None si falla.
    """
    print(f"--- Iniciando generación para texto: '{user_text}' ---")

    # Usamos la inicialización del script original funcional
    try:
        client = genai.Client(api_key=api_key)
        print("Cliente genai.Client inicializado correctamente.")
    except Exception as e:
        print(f"Error al inicializar genai.Client con la clave: {e}")
        return None

    # Usamos el nombre del modelo experimental del script original
    model_name = 'gemini-2.0-flash-exp-image-generation'
    print(f"Usando el modelo: '{model_name}'")

    # Combinamos el prompt base con el texto del usuario
    full_prompt = BASE_TEXT_PROMPT + user_text
    print(f"Prompt completo enviado a la API:\n{full_prompt}\n--------------------")

    # --- Contenido MODIFICADO: Solo enviamos texto ---
    contents = [
        types.ContentDict({"text": full_prompt})
        # Quitamos la parte de 'inline_data' de la imagen original
    ]
    # --- FIN Contenido MODIFICADO ---

    # Usamos la configuración del script original
    config = types.GenerateContentConfig(
        response_modalities=['Text', 'Image'] # Indica que esperamos imagen (y/o texto)
    )

    try:
        print(f"Llamando a {model_name} (client.models.generate_content)...")
        # Usamos la llamada del script original
        response = client.models.generate_content(
            model=model_name,
            contents=contents,
            config=config
        )
        print("Respuesta recibida de la API.")

        # --- Procesamiento de Respuesta (Igual al script original, buscando inline_data) ---
        print("\n--- Procesando Respuesta ---")
        if response.candidates:
            candidate = response.candidates[0]
            if candidate.content and candidate.content.parts:
                for i, part in enumerate(candidate.content.parts):
                    print(f"Analizando Parte {i+1}...")
                    # Priorizamos inline_data como en el script original
                    if part.inline_data is not None and part.inline_data.mime_type.startswith("image/"):
                        print(f"Parte contiene INLINE_DATA de imagen (formato: {part.inline_data.mime_type}).")
                        image_data = part.inline_data.data
                        try:
                            image = Image.open(BytesIO(image_data))
                            # Asegurarse de que la carpeta de salida exista
                            os.makedirs(output_folder, exist_ok=True)
                            # Construye la ruta completa usando el nombre base recibido
                            output_filepath = os.path.join(output_folder, f"{output_filename_base}.png")
                            print(f"Guardando imagen generada en: {output_filepath}")
                            image.save(output_filepath, format="PNG")
                            return output_filepath # Éxito

                        except Exception as img_ex:
                            print(f"Error al procesar/guardar datos de imagen inline_data: {img_ex}")
                            # Continuar por si otra parte es la correcta

                    elif hasattr(part, 'text') and part.text:
                        print(f"Parte contiene TEXTO: '{part.text[:150]}...'")
                    else:
                        print("Esta parte no contiene inline_data de imagen reconocible o texto.")

            # Si el bucle termina sin devolver una ruta
            print("Error: No se encontró 'inline_data' de imagen en ninguna parte de la respuesta.")
            if hasattr(candidate, 'finish_reason'):
                print(f"Razón de finalización del candidato: {candidate.finish_reason}")
            return None
        else:
            print("Error: La respuesta no contiene candidatos válidos.")
            if hasattr(response, 'prompt_feedback'):
                print(f"Feedback del prompt: {response.prompt_feedback}")
            return None

    except Exception as e:
        print(f"\nError durante la llamada a la API (client.models.generate_content) o procesamiento de imagen: {e}")
        # import traceback
        # traceback.print_exc()
        return None

# --- Bloque Principal MODIFICADO para aceptar texto ---
if __name__ == "__main__":
    # Argumentos esperados: script_name, user_text, api_key, output_folder, output_filename_base
    if len(sys.argv) != 5:
        print("\nError: Número incorrecto de argumentos.")
        print("Uso: python <nombre_script>.py \"<texto_del_prompt>\" <api_key> <carpeta_salida> <nombre_archivo_salida_base>")
        print("Ejemplo: python script_texto_a_imagen.py \"Un gato con sombrero\" tu_api_key ./imagenes_generadas gato_con_sombrero")
        sys.exit(1)

    # Asignar argumentos a variables
    user_provided_text = sys.argv[1]
    api_key = sys.argv[2]
    output_folder = sys.argv[3]
    output_filename_base = sys.argv[4] # Nombre base sin extensión

    print(f"\nArgumentos recibidos:")
    print(f"  Texto: {user_provided_text}")
    print(f"  API Key: {'*' * (len(api_key) - 4) + api_key[-4:]}")
    print(f"  Carpeta Salida: {output_folder}")
    print(f"  Nombre Archivo Salida (base): {output_filename_base}\n")

    # Llamar a la función modificada
    output_file_path = generate_image_from_text(user_provided_text, api_key, output_folder, output_filename_base)

    # Verificar resultado y salir
    if output_file_path:
        print(output_file_path)  # Solo se imprime el nombre del archivo si tuvo éxito
        sys.exit(0)
    else:
        print("ERROR: No se pudo generar o guardar la imagen.")  # Mensaje simple en caso de fallo
        sys.exit(1)


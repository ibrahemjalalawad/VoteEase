import face_recognition
import sys
import io
from PIL import Image

def load_image_from_file(file_path):
    with open(file_path, "rb") as file:
        image_data = file.read()
    return face_recognition.load_image_file(io.BytesIO(image_data))

def verify_faces(stored_image_path, captured_image_file):
    try:
        # Write debug logs to stderr
        print(f"Stored Image Path: {stored_image_path}", file=sys.stderr)
        print(f"Captured Image File: {captured_image_file}", file=sys.stderr)

        # Load the stored image
        stored_image = face_recognition.load_image_file(stored_image_path)
        print("Stored image loaded successfully.", file=sys.stderr)

        # Load the captured image
        captured_image = load_image_from_file(captured_image_file)
        print("Captured image loaded successfully.", file=sys.stderr)

        # Get the face encodings
        stored_encoding = face_recognition.face_encodings(stored_image)[0]
        captured_encoding = face_recognition.face_encodings(captured_image)[0]
        print("Face encodings generated.", file=sys.stderr)

        # Compare faces
        results = face_recognition.compare_faces([stored_encoding], captured_encoding)
        confidence = face_recognition.face_distance([stored_encoding], captured_encoding)[0]
        print(f"Results: {results}, Confidence: {confidence}", file=sys.stderr)

        # Output only the final result to stdout
        print(f"{results[0]},{confidence}", end="")
    except IndexError:
        # No face detected
        print("false,0.0", end="")
    except Exception as e:
        # Log unexpected errors to stderr
        print(f"Error: {e}", file=sys.stderr)
        sys.exit(1)

# Run the script
if __name__ == "__main__":
    stored_image_path = sys.argv[1]
    captured_image_file = sys.argv[2]
    verify_faces(stored_image_path, captured_image_file)

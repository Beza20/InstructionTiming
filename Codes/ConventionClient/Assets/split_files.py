import json
import os

def split_json_file(file_path, max_size_mb):
    # Load the JSON file
    with open(file_path, 'r') as f:
        data = json.load(f)

    # Convert the JSON data to a string
    json_str = json.dumps(data, indent=4)

    # Calculate the maximum size in bytes
    max_size_bytes = max_size_mb * 1024 * 1024

    # Split the JSON string into chunks
    chunks = [json_str[i:i + max_size_bytes] for i in range(0, len(json_str), max_size_bytes)]

    # Save each chunk as a separate file
    base_name = os.path.splitext(file_path)[0]
    for i, chunk in enumerate(chunks):
        chunk_file_path = f"{base_name}_part{i + 1}.json"
        with open(chunk_file_path, 'w') as f:
            f.write(chunk)
        print(f"Saved {chunk_file_path}")

# Example usage
file_path = 'P9.json'  # Replace with your JSON file path
max_size_mb = 1  # Maximum size of each chunk in MB
split_json_file(file_path, max_size_mb)
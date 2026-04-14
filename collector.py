import os

EXCLUDED_DIRS = {
    'node_modules', 'bin', 'obj', '.git', '.vs', '.vscode', 
    'dist', 'publish', 'migrations', 'wwwroot', 'migrations'
}
ALLOWED_EXTENSIONS = {
    '.cs', '.js', '.jsx', '.ts', '.tsx', '.css', 
    '.json', '.html', '.csproj', '.sln', '.txt'
}

def collect_project_code(root_path, output_file):
    with open(output_file, 'w', encoding='utf-8') as f_out:
        for root, dirs, files in os.walk(root_path):
            # Gereksiz klasörleri ele
            dirs[:] = [d for d in dirs if d not in EXCLUDED_DIRS]
            
            for file in files:
                ext = os.path.splitext(file)[1].lower()
                if ext in ALLOWED_EXTENSIONS:
                    file_path = os.path.join(root, file)
                    relative_path = os.path.relpath(file_path, root_path)
                    
                    try:
                        with open(file_path, 'r', encoding='utf-8') as f_in:
                            content = f_in.read()
                            
                        f_out.write(f"\n{'='*50}\n")
                        f_out.write(f"DOSYA YOLU: {relative_path}\n")
                        f_out.write(f"{'='*50}\n\n")
                        f_out.write(content)
                        f_out.write("\n")
                    except Exception as e:
                        f_out.write(f"\n[HATA] {relative_path} okunamadı: {str(e)}\n")

if __name__ == "__main__":
    project_root = "."
    output_name = "lifequest_project_dump.txt"
    print(f"Tarama başlıyor: {os.path.abspath(project_root)}")
    collect_project_code(project_root, output_name)
    print(f"Tamamlandı! Dosya oluşturuldu: {output_name}")
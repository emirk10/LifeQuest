import React, { useState, useRef } from 'react';

const PhotoUpload = ({ onPhotoSelect, hint }) => {
  const [preview, setPreview] = useState(null);
  const [fileName, setFileName] = useState('');
  const fileInputRef = useRef(null);

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (!file) return;

    // Dosya tipi kontrolu
    if (!file.type.startsWith('image/')) {
      alert('Lutfen bir gorsel dosyasi secin.');
      return;
    }

    // Max 5MB
    if (file.size > 5 * 1024 * 1024) {
      alert('Dosya boyutu 5MB\'dan kucuk olmali.');
      return;
    }

    setFileName(file.name);

    const reader = new FileReader();
    reader.onloadend = () => {
      const base64 = reader.result.split(',')[1]; // data:image/...;base64, kisminı at
      setPreview(reader.result);
      onPhotoSelect(base64);
    };
    reader.readAsDataURL(file);
  };

  const handleRemove = () => {
    setPreview(null);
    setFileName('');
    onPhotoSelect(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  return (
    <div className="bg-slate-900/60 p-5 rounded-2xl border border-slate-800 mt-3">
      {hint && (
        <p className="text-amber-400/80 text-xs mb-3 flex items-center gap-1.5">
          <span>💡</span> {hint}
        </p>
      )}

      {!preview ? (
        <label className="flex flex-col items-center justify-center w-full h-32 border-2 border-dashed border-slate-700 rounded-xl cursor-pointer hover:border-indigo-500 transition-colors">
          <div className="flex flex-col items-center">
            <span className="text-2xl mb-1">📸</span>
            <span className="text-slate-400 text-xs font-bold">Fotograf Yukle</span>
            <span className="text-slate-600 text-[10px] mt-1">JPG, PNG (max 5MB)</span>
          </div>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/*"
            className="hidden"
            onChange={handleFileChange}
          />
        </label>
      ) : (
        <div className="relative">
          <img
            src={preview}
            alt="Onizleme"
            className="w-full h-40 object-cover rounded-xl border border-slate-700"
          />
          <button
            onClick={handleRemove}
            className="absolute top-2 right-2 w-7 h-7 bg-red-500/90 hover:bg-red-400 rounded-full flex items-center justify-center text-white text-xs font-bold transition-colors"
          >
            X
          </button>
          <p className="text-slate-500 text-[10px] mt-2 truncate">{fileName}</p>
        </div>
      )}
    </div>
  );
};

export default PhotoUpload;

const errorMap = {
  
  "AUTH_INVALID_CREDENTIALS": "E-posta veya şifre hatalı.",
  "AUTH_USER_NOT_FOUND": "Kullanıcı bulunamadı.",
  "AUTH_EMAIL_ALREADY_EXISTS": "Bu e-posta adresi zaten kullanımda.",
  "AUTH_WEAK_PASSWORD": "Şifre çok zayıf. En az 8 karakter, harf ve rakam içermelidir.",
  "AUTH_LOCKED": "Çok fazla deneme yaptınız. Hesabınız geçici olarak kilitlendi.",

  
  "TASK_LIMIT_REACHED": "Günlük görev limitine ulaştın!",
  "TASK_ALREADY_ASSIGNED": "Bu görev zaten sana atanmış.",
  "TASK_NOT_FOUND": "Görev bulunamadı.",
  "TASK_DEPENDENCY_NOT_MET": "Bu görevi yapmak için gerekli seviyede değilsin.",

  
  "ERR_NETWORK": "Sunucuya bağlanılamadı. Lütfen internetinizi kontrol edin.",
  "UNKNOWN_ERROR": "Beklenmedik bir hata oluştu. Lütfen tekrar deneyin.",
  "VALIDATION_ERROR": "Lütfen girdiğiniz bilgileri kontrol edin."
};

export const getFriendlyErrorMessage = (errorCode) => {
  return errorMap[errorCode] || errorMap["UNKNOWN_ERROR"];
};

export default errorMap;

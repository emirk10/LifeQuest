export const getUserIdFromToken = () => {
  const userDataStr = sessionStorage.getItem("user_data");
  if (!userDataStr) return null;
  try {
    const userData = JSON.parse(userDataStr);
    return userData.userId || null;
  } catch {
    return null;
  }
};

export const getUserNameFromToken = () => {
  const userDataStr = sessionStorage.getItem("user_data");
  if (!userDataStr) return null;
  try {
    const userData = JSON.parse(userDataStr);
    return userData.userName || "Bilinmeyen Kullanıcı";
  } catch {
    return null;
  }
};

export const getUserRoleFromToken = () => {
  const userDataStr = sessionStorage.getItem("user_data");
  if (!userDataStr) return null;
  try {
    const userData = JSON.parse(userDataStr);
    return userData.role || null;
  } catch {
    return null;
  }
};

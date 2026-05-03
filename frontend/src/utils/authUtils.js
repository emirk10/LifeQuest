export const getUserIdFromToken = () => {
  const token = sessionStorage.getItem("token");
  if (!token) return null;

  try {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      window
        .atob(base64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
    const decoded = JSON.parse(jsonPayload);

    return decoded.nameid || decoded.sub || decoded.unique_name || null;
  } catch (error) {
    return null;
  }
};

export const getUserNameFromToken = () => {
  const token = sessionStorage.getItem("token");
  if (!token) return null;

  try {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      window
        .atob(base64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
    const decoded = JSON.parse(jsonPayload);

    return (
      decoded.unique_name ||
      decoded.name ||
      decoded.email ||
      "Bilinmeyen Kullanıcı"
    );
  } catch (error) {
    return null;
  }
};

export const getUserRoleFromToken = () => {
  const token = sessionStorage.getItem("token");
  if (!token) return null;

  try {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      window
        .atob(base64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
    const decoded = JSON.parse(jsonPayload);

    return (
      decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
      decoded.role ||
      null
    );
  } catch (error) {
    return null;
  }
};

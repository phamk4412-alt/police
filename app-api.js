(function () {
  const apiBase = (window.POLICE_API_BASE || window.location.origin || "").replace(/\/$/, "");

  function apiUrl(path) {
    if (/^https?:\/\//i.test(path)) {
      return path;
    }

    return `${apiBase}${path}`;
  }

  async function apiFetch(path, options) {
    return await fetch(apiUrl(path), {
      credentials: "include",
      ...(options || {})
    });
  }

  window.POLICE_API = {
    base: apiBase,
    url: apiUrl,
    fetch: apiFetch
  };
})();

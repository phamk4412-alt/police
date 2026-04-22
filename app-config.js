(function () {
  const storageKey = "POLICE_PUBLIC_API_BASE";
  const defaultApiBaseUrl = "https://police-otit.onrender.com";

  function normalizeBaseUrl(value) {
    return (value || "").trim().replace(/\/$/, "");
  }

  function getRuntimeConfiguredApiBase() {
    try {
      const params = new URLSearchParams(window.location.search);
      const queryBase = normalizeBaseUrl(params.get("apiBase"));
      if (queryBase) {
        window.localStorage.setItem(storageKey, queryBase);
        return queryBase;
      }

      return normalizeBaseUrl(window.localStorage.getItem(storageKey));
    } catch {
      return "";
    }
  }

  const apiBaseUrl = getRuntimeConfiguredApiBase() || defaultApiBaseUrl;

  function apiUrl(path) {
    if (!path) {
      return apiBaseUrl;
    }

    if (/^https?:\/\//i.test(path)) {
      return path;
    }

    return `${apiBaseUrl}${path.startsWith("/") ? path : `/${path}`}`;
  }

  function apiFetch(path, options) {
    return fetch(apiUrl(path), {
      credentials: "include",
      ...(options || {})
    });
  }

  window.APP_CONFIG = {
    apiBaseUrl,
    apiUrl,
    apiFetch,
    credentials: "include"
  };

  window.POLICE_API_BASE = apiBaseUrl;
  window.POLICE_REALTIME_POLL_INTERVAL_MS = 4000;
  window.apiUrl = apiUrl;
  window.apiFetch = apiFetch;
})();

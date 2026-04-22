(function () {
  const PRODUCTION_API_BASE_URL = "https://police-otit.onrender.com";
  const LOCAL_API_BASE_URL = "http://localhost:5055";
  const DEFAULT_TIMEOUT_MS = 15000;
  const AUTH_TOKEN_STORAGE_KEY = "POLICE_AUTH_TOKEN";

  function normalizeBaseUrl(value) {
    return (value || "").trim().replace(/\/$/, "");
  }

  function resolveApiBaseUrl() {
    const params = new URLSearchParams(window.location.search);
    const queryBase = normalizeBaseUrl(params.get("apiBase"));

    if (queryBase) {
      return queryBase;
    }

    if (window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1") {
      return LOCAL_API_BASE_URL;
    }

    return PRODUCTION_API_BASE_URL;
  }

  const apiBaseUrl = normalizeBaseUrl(resolveApiBaseUrl());

  function getAuthToken() {
    try {
      return window.sessionStorage.getItem(AUTH_TOKEN_STORAGE_KEY) || "";
    } catch {
      return "";
    }
  }

  function setAuthToken(token) {
    try {
      if (token) {
        window.sessionStorage.setItem(AUTH_TOKEN_STORAGE_KEY, token);
      } else {
        window.sessionStorage.removeItem(AUTH_TOKEN_STORAGE_KEY);
      }
    } catch {
    }
  }

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
    const { timeoutMs = DEFAULT_TIMEOUT_MS, signal, ...requestOptions } = options || {};
    const controller = new AbortController();
    const timeoutId = window.setTimeout(() => controller.abort(), timeoutMs);
    const headers = new Headers(requestOptions.headers || {});
    const authToken = getAuthToken();

    if (signal) {
      signal.addEventListener("abort", () => controller.abort(), { once: true });
    }

    if (authToken && !headers.has("Authorization")) {
      headers.set("Authorization", `Bearer ${authToken}`);
    }

    return fetch(apiUrl(path), {
      ...requestOptions,
      headers,
      credentials: "include",
      signal: controller.signal
    }).finally(() => {
      window.clearTimeout(timeoutId);
    });
  }

  window.APP_CONFIG = {
    apiBaseUrl,
    apiUrl,
    apiFetch,
    getAuthToken,
    setAuthToken,
    clearAuthToken: () => setAuthToken(""),
    credentials: "include"
  };

  window.POLICE_API_BASE = apiBaseUrl;
  window.POLICE_REALTIME_POLL_INTERVAL_MS = 4000;
  window.apiUrl = apiUrl;
  window.apiFetch = apiFetch;
  window.getPoliceAuthToken = getAuthToken;
  window.setPoliceAuthToken = setAuthToken;
  window.clearPoliceAuthToken = () => setAuthToken("");
})();

(function () {
  const PRODUCTION_API_BASE_URL = "https://police-otit.onrender.com";
  const LOCAL_API_BASE_URL = "http://localhost:5055";
  const DEFAULT_TIMEOUT_MS = 15000;

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

    if (signal) {
      signal.addEventListener("abort", () => controller.abort(), { once: true });
    }

    return fetch(apiUrl(path), {
      ...requestOptions,
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
    credentials: "include"
  };

  window.POLICE_API_BASE = apiBaseUrl;
  window.POLICE_REALTIME_POLL_INTERVAL_MS = 4000;
  window.apiUrl = apiUrl;
  window.apiFetch = apiFetch;
})();

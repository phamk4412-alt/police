(function () {
  const isLocalHost = ["localhost", "127.0.0.1"].includes(window.location.hostname);
  const storageKey = "POLICE_PUBLIC_API_BASE";

  function normalizeBaseUrl(value) {
    return (value || "").trim().replace(/\/$/, "");
  }

  function isLoopbackUrl(value) {
    try {
      const url = new URL(value);
      return ["localhost", "127.0.0.1"].includes(url.hostname);
    } catch {
      return false;
    }
  }

  function getRuntimeConfiguredApiBase() {
    try {
      const params = new URLSearchParams(window.location.search);
      const queryBase = normalizeBaseUrl(params.get("apiBase"));
      if (queryBase) {
        window.localStorage.setItem(storageKey, queryBase);
        return queryBase;
      }

      const storedBase = normalizeBaseUrl(window.localStorage.getItem(storageKey));
      if (!isLocalHost && isLoopbackUrl(storedBase)) {
        window.localStorage.removeItem(storageKey);
        return "";
      }

      return storedBase;
    } catch {
      return "";
    }
  }

  // Keep localhost for local development only.
  // On deployed domains, default back to the current origin unless overridden.
  const configuredPublicApiBase = isLocalHost ? "http://127.0.0.1:5055" : "";
  const runtimeConfiguredApiBase = getRuntimeConfiguredApiBase();

  window.POLICE_API_BASE =
    runtimeConfiguredApiBase ||
    configuredPublicApiBase ||
    (isLocalHost ? "http://localhost:5055" : window.location.origin);

  window.POLICE_REALTIME_POLL_INTERVAL_MS = 4000;
})();

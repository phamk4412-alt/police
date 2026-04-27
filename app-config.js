(function () {
  const isLocalHost = ["localhost", "127.0.0.1"].includes(window.location.hostname);
  const storageKey = "POLICE_PUBLIC_API_BASE";

  function normalizeBaseUrl(value) {
    return (value || "").trim().replace(/\/$/, "");
  }

  function migrateKnownBadBaseUrl(value) {
    return value.replace("https://police-lahn.onrender.com", "https://police-1ahn.onrender.com");
  }

  function getRuntimeConfiguredApiBase() {
    try {
      const params = new URLSearchParams(window.location.search);
      const queryBase = migrateKnownBadBaseUrl(normalizeBaseUrl(params.get("apiBase")));
      if (queryBase) {
        window.localStorage.setItem(storageKey, queryBase);
        return queryBase;
      }

      const storedBase = migrateKnownBadBaseUrl(normalizeBaseUrl(window.localStorage.getItem(storageKey)));
      if (storedBase) {
        window.localStorage.setItem(storageKey, storedBase);
      }

      return storedBase;
    } catch {
      return "";
    }
  }

  // Set window.POLICE_PUBLIC_API_BASE before this file, or open the site once
  // with ?apiBase=https://your-backend.example.com to store a different backend.
  const configuredPublicApiBase = normalizeBaseUrl(
    window.POLICE_PUBLIC_API_BASE || "https://police-1ahn.onrender.com"
  );
  const runtimeConfiguredApiBase = getRuntimeConfiguredApiBase();

  window.POLICE_API_BASE =
    runtimeConfiguredApiBase ||
    configuredPublicApiBase ||
    (isLocalHost ? "http://localhost:5055" : window.location.origin);

  window.POLICE_REALTIME_POLL_INTERVAL_MS = 4000;
})();

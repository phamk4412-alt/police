(function () {
  const isLocalHost = ["localhost", "127.0.0.1"].includes(window.location.hostname);

  // When frontend and backend run on different public domains, set the backend URL here once.
  // Example: "https://police-api.your-domain.com"
  const configuredPublicApiBase = "";

  window.POLICE_API_BASE =
    configuredPublicApiBase ||
    (isLocalHost ? "http://localhost:5055" : window.location.origin);

  window.POLICE_REALTIME_POLL_INTERVAL_MS = 4000;
})();

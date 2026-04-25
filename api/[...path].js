const crypto = require("crypto");

const SESSION_COOKIE = "PoliceSmartHub.Auth";
const SESSION_SECRET = process.env.AUTH_SECRET || "police-smart-hub-vercel-secret";

const APP_ROLES = {
  Admin: "Admin",
  User: "User",
  Police: "Police",
  Support: "Support"
};

const INCIDENT_STATUSES = {
  MOI_TIEP_NHAN: "Moi tiep nhan",
  DA_TIEP_NHAN: "Da tiep nhan",
  DANG_XAC_MINH: "Dang xac minh",
  DA_DIEU_PHOI: "Da dieu phoi",
  DA_XU_LY: "Da xu ly"
};

const AUDIT_ACTIONS = {
  LOGIN_SUCCESS: "auth.login.success",
  LOGIN_FAILED: "auth.login.failed",
  LOGOUT: "auth.logout",
  ANALYZE_INCIDENT: "incident.analyze",
  CREATE_INCIDENT: "incident.create",
  UPDATE_INCIDENT_STATUS: "incident.status.update",
  UPDATE_INCIDENT_DENIED: "incident.status.denied",
  EXPORT_INCIDENTS: "incident.export"
};

const AUDIT_ENTITIES = {
  AUTH: "auth",
  INCIDENT: "incident"
};

const DEMO_USERS = {
  admin: { username: "admin", password: "admin123", displayName: "Quan tri vien", role: APP_ROLES.Admin },
  user: { username: "user", password: "user123", displayName: "Nguoi dung", role: APP_ROLES.User },
  police: { username: "police", password: "police123", displayName: "Canh sat", role: APP_ROLES.Police },
  support: { username: "support", password: "support123", displayName: "Nhan vien ho tro", role: APP_ROLES.Support }
};

function getStore() {
  if (!globalThis.__POLICE_VERCEL_STORE__) {
    globalThis.__POLICE_VERCEL_STORE__ = {
      incidents: createSeedIncidents(),
      auditLogs: createSeedAuditLogs()
    };
  }

  return globalThis.__POLICE_VERCEL_STORE__;
}

function createSeedIncidents() {
  const now = Date.now();
  return [
    makeIncident({
      id: crypto.randomUUID(),
      title: "Cuop giat tai san",
      detail: "Nguoi dan bao mat tui xach tai khu vuc trung tam.",
      category: "Mat cap tai san",
      level: "high",
      urgencyScore: 90,
      classificationReason: "phat hien tu khoa: cuop, giat",
      latitude: 10.7769,
      longitude: 106.7009,
      district: "Quan 1",
      timeLabel: "08:15",
      status: INCIDENT_STATUSES.DANG_XAC_MINH,
      source: "user",
      reporterName: "Nguoi dung",
      lastUpdatedBy: "Canh sat",
      internalNote: "Uu tien doi gan nhat tiep can hien truong.",
      createdAt: new Date(now - 45 * 60 * 1000).toISOString(),
      updatedAt: new Date(now - 20 * 60 * 1000).toISOString()
    }),
    makeIncident({
      id: crypto.randomUUID(),
      title: "Va cham giao thong",
      detail: "Hai xe may va cham, can ho tro dieu tiet.",
      category: "Tai nan / cap cuu",
      level: "medium",
      urgencyScore: 68,
      classificationReason: "phat hien tu khoa: va cham",
      latitude: 10.7626,
      longitude: 106.6602,
      district: "Quan 3",
      timeLabel: "08:42",
      status: INCIDENT_STATUSES.DA_TIEP_NHAN,
      source: "user",
      reporterName: "Nguoi dung",
      lastUpdatedBy: "Canh sat",
      internalNote: "Da dieu doi dieu tiet giao thong.",
      createdAt: new Date(now - 30 * 60 * 1000).toISOString(),
      updatedAt: new Date(now - 12 * 60 * 1000).toISOString()
    }),
    makeIncident({
      id: crypto.randomUUID(),
      title: "Ho tro xac minh",
      detail: "Yeu cau xac minh doi tuong la mat quanh truong hoc.",
      category: "Nghi van can xac minh",
      level: "low",
      urgencyScore: 42,
      classificationReason: "mo ta chua co tu khoa ro rang, can xac minh them",
      latitude: 10.8039,
      longitude: 106.7298,
      district: "Thu Duc",
      timeLabel: "09:05",
      status: INCIDENT_STATUSES.MOI_TIEP_NHAN,
      source: "user",
      reporterName: "Nguoi dung",
      lastUpdatedBy: "Nguoi dung",
      internalNote: "Cho bo sung mo ta va hinh anh.",
      createdAt: new Date(now - 10 * 60 * 1000).toISOString(),
      updatedAt: new Date(now - 10 * 60 * 1000).toISOString()
    })
  ];
}

function createSeedAuditLogs() {
  const now = Date.now();
  return [
    makeAuditLog({
      id: crypto.randomUUID(),
      action: AUDIT_ACTIONS.LOGIN_SUCCESS,
      entityType: AUDIT_ENTITIES.AUTH,
      entityId: "admin",
      actorUsername: "admin",
      actorDisplayName: "Quan tri vien",
      actorRole: APP_ROLES.Admin,
      summary: "Dang nhap thanh cong.",
      detail: "Quan tri vien dang nhap vao he thong.",
      ipAddress: "",
      createdAt: new Date(now - 90 * 60 * 1000).toISOString()
    }),
    makeAuditLog({
      id: crypto.randomUUID(),
      action: AUDIT_ACTIONS.CREATE_INCIDENT,
      entityType: AUDIT_ENTITIES.INCIDENT,
      entityId: "seed",
      actorUsername: "user",
      actorDisplayName: "Nguoi dung",
      actorRole: APP_ROLES.User,
      summary: "Tao bao cao moi.",
      detail: "Nguoi dung tao bao cao seeding de demo he thong.",
      ipAddress: "",
      createdAt: new Date(now - 35 * 60 * 1000).toISOString()
    })
  ];
}

function makeIncident(data) {
  return {
    Id: data.id,
    Title: data.title,
    Detail: data.detail,
    Category: data.category,
    Level: data.level,
    UrgencyScore: data.urgencyScore,
    ClassificationReason: data.classificationReason,
    Latitude: data.latitude,
    Longitude: data.longitude,
    District: data.district,
    TimeLabel: data.timeLabel,
    Status: data.status,
    Source: data.source,
    ReporterName: data.reporterName,
    LastUpdatedBy: data.lastUpdatedBy,
    InternalNote: data.internalNote,
    CreatedAt: data.createdAt,
    UpdatedAt: data.updatedAt
  };
}

function makeAuditLog(data) {
  return {
    Id: data.id,
    Action: data.action,
    EntityType: data.entityType,
    EntityId: data.entityId,
    ActorUsername: data.actorUsername,
    ActorDisplayName: data.actorDisplayName,
    ActorRole: data.actorRole,
    Summary: data.summary,
    Detail: data.detail,
    IpAddress: data.ipAddress,
    CreatedAt: data.createdAt
  };
}

function getLandingPathForRole(role) {
  switch (role) {
    case APP_ROLES.Admin:
      return "/admin/admin.html";
    case APP_ROLES.User:
      return "/user/user.html";
    case APP_ROLES.Police:
      return "/police/police.html";
    case APP_ROLES.Support:
      return "/support/support.html";
    default:
      return "/";
  }
}

function parseCookies(req) {
  const raw = req.headers.cookie || "";
  return raw.split(";").reduce((acc, part) => {
    const index = part.indexOf("=");
    if (index < 0) {
      return acc;
    }

    const key = part.slice(0, index).trim();
    const value = part.slice(index + 1).trim();
    if (key) {
      acc[key] = decodeURIComponent(value);
    }
    return acc;
  }, {});
}

function toBase64Url(value) {
  return Buffer.from(value).toString("base64url");
}

function fromBase64Url(value) {
  return Buffer.from(value, "base64url").toString("utf8");
}

function signPayload(payload) {
  return crypto.createHmac("sha256", SESSION_SECRET).update(payload).digest("base64url");
}

function createSessionCookie(user) {
  const payload = toBase64Url(JSON.stringify({
    username: user.username,
    displayName: user.displayName,
    role: user.role,
    exp: Date.now() + 8 * 60 * 60 * 1000
  }));
  const signature = signPayload(payload);
  return `${payload}.${signature}`;
}

function readSession(req) {
  const cookies = parseCookies(req);
  const raw = cookies[SESSION_COOKIE];
  if (!raw) {
    return null;
  }

  const [payload, signature] = raw.split(".");
  if (!payload || !signature) {
    return null;
  }

  if (signPayload(payload) !== signature) {
    return null;
  }

  try {
    const session = JSON.parse(fromBase64Url(payload));
    if (!session?.exp || session.exp < Date.now()) {
      return null;
    }
    return session;
  } catch {
    return null;
  }
}

function setSessionCookie(res, user) {
  const cookieValue = createSessionCookie(user);
  const parts = [
    `${SESSION_COOKIE}=${encodeURIComponent(cookieValue)}`,
    "Path=/",
    "HttpOnly",
    "SameSite=Lax",
    "Max-Age=28800"
  ];

  if (process.env.NODE_ENV === "production") {
    parts.push("Secure");
  }

  res.setHeader("Set-Cookie", parts.join("; "));
}

function clearSessionCookie(res) {
  const parts = [
    `${SESSION_COOKIE}=`,
    "Path=/",
    "HttpOnly",
    "SameSite=Lax",
    "Max-Age=0"
  ];

  if (process.env.NODE_ENV === "production") {
    parts.push("Secure");
  }

  res.setHeader("Set-Cookie", parts.join("; "));
}

async function readBody(req) {
  if (req.body && typeof req.body === "object") {
    return req.body;
  }

  const chunks = [];
  for await (const chunk of req) {
    chunks.push(Buffer.isBuffer(chunk) ? chunk : Buffer.from(chunk));
  }

  const raw = Buffer.concat(chunks).toString("utf8").trim();
  if (!raw) {
    return {};
  }

  try {
    return JSON.parse(raw);
  } catch {
    return {};
  }
}

function sendJson(res, statusCode, data) {
  res.statusCode = statusCode;
  res.setHeader("Content-Type", "application/json; charset=utf-8");
  res.end(JSON.stringify(data));
}

function sendCsv(res, filename, content) {
  res.statusCode = 200;
  res.setHeader("Content-Type", "text/csv; charset=utf-8");
  res.setHeader("Content-Disposition", `attachment; filename=\"${filename}\"`);
  res.end(content);
}

function getActor(req) {
  const session = readSession(req);
  if (!session) {
    return { username: "demo-user", displayName: "Nguoi dung demo", role: "Anonymous" };
  }

  return {
    username: session.username,
    displayName: session.displayName,
    role: session.role
  };
}

function requireAuth(req, res, allowedRoles) {
  const session = readSession(req);
  if (!session) {
    sendJson(res, 401, { message: "Unauthorized" });
    return null;
  }

  if (allowedRoles && !allowedRoles.includes(session.role)) {
    sendJson(res, 403, { message: "Forbidden" });
    return null;
  }

  return session;
}

function normalizeLevel(level) {
  switch ((level || "").trim().toLowerCase()) {
    case "high":
    case "khancap":
    case "cao":
      return "high";
    case "medium":
    case "trungbinh":
      return "medium";
    case "low":
    case "thap":
      return "low";
    default:
      return "high";
  }
}

function normalizeStatus(status) {
  switch ((status || "").trim().toLowerCase()) {
    case "moi tiep nhan":
      return INCIDENT_STATUSES.MOI_TIEP_NHAN;
    case "da tiep nhan":
      return INCIDENT_STATUSES.DA_TIEP_NHAN;
    case "dang xac minh":
      return INCIDENT_STATUSES.DANG_XAC_MINH;
    case "da dieu phoi":
      return INCIDENT_STATUSES.DA_DIEU_PHOI;
    case "da xu ly":
      return INCIDENT_STATUSES.DA_XU_LY;
    default:
      return status && status.trim() ? status.trim() : INCIDENT_STATUSES.MOI_TIEP_NHAN;
  }
}

function canUpdateIncidentStatus(role, status) {
  if (role === APP_ROLES.Admin) {
    return true;
  }

  if (role === APP_ROLES.Police) {
    return [
      INCIDENT_STATUSES.DA_TIEP_NHAN,
      INCIDENT_STATUSES.DANG_XAC_MINH,
      INCIDENT_STATUSES.DA_DIEU_PHOI,
      INCIDENT_STATUSES.DA_XU_LY
    ].includes(status);
  }

  return false;
}

function removeDiacritics(input) {
  return (input || "")
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D");
}

function analyzeIncident(title, detail, requestedLevel) {
  const combined = `${title || ""} ${detail || ""}`.trim();
  const normalized = removeDiacritics(combined).toLowerCase();

  const profiles = [
    { category: "Nguy co bao luc", baseScore: 96, keywords: ["giet nguoi", "sat hai", "dam chet", "thu tieu", "co vu khi", "dao", "sung", "hanh hung", "bi tan cong", "chem", "cuop"] },
    { category: "Su co hien truong", baseScore: 88, keywords: ["tai nan", "va cham", "chay", "no", "bi thuong", "mau nhieu", "ngat"] },
    { category: "Mat cap tai san", baseScore: 70, keywords: ["mat cap", "trom", "giat", "xe may bi lay", "dot nhap"] },
    { category: "Nghi ngo lua dao", baseScore: 64, keywords: ["lua dao", "otp", "gia mao", "chuyen khoan", "tai khoan ngan hang"] },
    { category: "Mat trat tu cong cong", baseScore: 52, keywords: ["gay roi", "danh nhau", "tap trung dong nguoi", "on ao", "tu tap"] },
    { category: "Tinh huong can xac minh", baseScore: 40, keywords: ["dang nghi", "kha nghi", "la mat", "theo doi"] }
  ];

  let bestProfile = null;
  for (const profile of profiles) {
    const matches = profile.keywords.filter((keyword) => normalized.includes(keyword));
    if (!matches.length) {
      continue;
    }

    if (!bestProfile || matches.length > bestProfile.matches.length || (matches.length === bestProfile.matches.length && profile.baseScore > bestProfile.profile.baseScore)) {
      bestProfile = { profile, matches };
    }
  }

  let score = bestProfile ? bestProfile.profile.baseScore : 38;
  const reasons = [];

  if (bestProfile) {
    reasons.push(`phat hien tu khoa: ${bestProfile.matches.slice(0, 3).join(", ")}`);
  } else {
    reasons.push("mo ta chua co tu khoa ro rang, can xac minh them");
  }

  const urgencyBoosters = {
    "ngay bay gio": 8,
    dang: 6,
    "vua xay ra": 8,
    "tre em": 10,
    "nguoi gia": 10,
    "co nguoi bi thuong": 14,
    "bat tinh": 16,
    "chay lon": 16,
    "de doa": 10
  };

  for (const [keyword, boost] of Object.entries(urgencyBoosters)) {
    if (normalized.includes(keyword)) {
      score += boost;
      reasons.push(`co dau hieu tang muc khan: ${keyword}`);
    }
  }

  const requestedNormalized = normalizeLevel(requestedLevel);
  score = Math.max(score, requestedNormalized === "high" ? 82 : requestedNormalized === "medium" ? 58 : 35);
  score = Math.max(15, Math.min(99, score));

  const level = score >= 85 ? "high" : score >= 55 ? "medium" : "low";
  const shouldCallEmergency = score >= 88;
  const category = bestProfile ? bestProfile.profile.category : "Tinh huong can xac minh";
  const recommendation = shouldCallEmergency
    ? "Uu tien ket noi 113 ngay, dong thoi bo sung vi tri va dau hieu nhan dang."
    : level === "medium"
      ? "Can xac minh them thong tin va theo doi phan hoi tu trung tam."
      : "Luu vao hang doi, uu tien bo sung chi tiet de phan loai chinh xac hon.";

  return {
    Category: category,
    Level: level,
    UrgencyScore: score,
    Reason: reasons.join("; "),
    ShouldCallEmergency: shouldCallEmergency,
    Recommendation: recommendation
  };
}

function resolveDistrict(latitude, longitude) {
  if (latitude >= 10.76 && latitude <= 10.79 && longitude >= 106.69 && longitude <= 106.71) {
    return "Quan 1";
  }
  if (latitude >= 10.77 && longitude >= 106.72) {
    return "Thu Duc";
  }
  if (latitude >= 10.79 && longitude <= 106.69) {
    return "Binh Thanh";
  }
  if (latitude < 10.76 && longitude <= 106.69) {
    return "Quan 3";
  }
  if (latitude < 10.74) {
    return "Quan 7";
  }
  return "TP.HCM";
}

function tryParseLocation(raw) {
  const parts = String(raw || "").split(",").map((part) => part.trim()).filter(Boolean);
  if (parts.length !== 2) {
    return null;
  }

  const latitude = Number(parts[0]);
  const longitude = Number(parts[1]);
  if (!Number.isFinite(latitude) || !Number.isFinite(longitude)) {
    return null;
  }

  if (!(latitude >= 10.3 && latitude <= 11.1 && longitude >= 106.4 && longitude <= 107.1)) {
    return null;
  }

  return { latitude, longitude };
}

function sortIncidents(items, sort) {
  const copy = items.slice();
  switch ((sort || "").trim().toLowerCase()) {
    case "created_asc":
      copy.sort((a, b) => new Date(a.CreatedAt) - new Date(b.CreatedAt));
      break;
    case "updated_desc":
      copy.sort((a, b) => new Date(b.UpdatedAt) - new Date(a.UpdatedAt));
      break;
    case "updated_asc":
      copy.sort((a, b) => new Date(a.UpdatedAt) - new Date(b.UpdatedAt));
      break;
    case "urgency_desc":
      copy.sort((a, b) => (b.UrgencyScore - a.UrgencyScore) || (new Date(b.CreatedAt) - new Date(a.CreatedAt)));
      break;
    case "urgency_asc":
      copy.sort((a, b) => (a.UrgencyScore - b.UrgencyScore) || (new Date(b.CreatedAt) - new Date(a.CreatedAt)));
      break;
    default:
      copy.sort((a, b) => new Date(b.CreatedAt) - new Date(a.CreatedAt));
      break;
  }
  return copy;
}

function filterIncidents(items, query) {
  let filtered = items.slice();
  const search = (query.search || "").trim().toLowerCase();
  const status = query.status ? normalizeStatus(query.status) : "";
  const level = query.level ? normalizeLevel(query.level) : "";
  const source = (query.source || "").trim().toLowerCase();
  const district = (query.district || "").trim().toLowerCase();
  const from = query.from ? new Date(query.from) : null;
  const to = query.to ? new Date(query.to) : null;

  if (search) {
    filtered = filtered.filter((item) =>
      item.Title.toLowerCase().includes(search) ||
      item.Detail.toLowerCase().includes(search) ||
      item.Category.toLowerCase().includes(search) ||
      item.District.toLowerCase().includes(search)
    );
  }

  if (status) {
    filtered = filtered.filter((item) => item.Status === status);
  }

  if (level) {
    filtered = filtered.filter((item) => item.Level === level);
  }

  if (source) {
    filtered = filtered.filter((item) => item.Source.toLowerCase() === source);
  }

  if (district) {
    filtered = filtered.filter((item) => item.District.toLowerCase().includes(district));
  }

  if (from && !Number.isNaN(from.getTime())) {
    filtered = filtered.filter((item) => new Date(item.CreatedAt) >= from);
  }

  if (to && !Number.isNaN(to.getTime())) {
    filtered = filtered.filter((item) => new Date(item.CreatedAt) <= to);
  }

  return sortIncidents(filtered, query.sort);
}

function buildIncidentCsv(incidents) {
  const lines = [
    "Id,Title,Category,Level,UrgencyScore,District,Status,Source,ReporterName,LastUpdatedBy,CreatedAt,UpdatedAt"
  ];

  for (const incident of incidents) {
    lines.push([
      incident.Id,
      incident.Title,
      incident.Category,
      incident.Level,
      incident.UrgencyScore,
      incident.District,
      incident.Status,
      incident.Source,
      incident.ReporterName,
      incident.LastUpdatedBy,
      incident.CreatedAt,
      incident.UpdatedAt
    ].map(csvEscape).join(","));
  }

  return lines.join("\n");
}

function csvEscape(value) {
  const safe = value == null ? "" : String(value);
  return `"${safe.replace(/"/g, "\"\"")}"`;
}

function writeAuditLog(req, entry) {
  const store = getStore();
  const actor = getActor(req);
  store.auditLogs.unshift(makeAuditLog({
    id: crypto.randomUUID(),
    action: entry.action,
    entityType: entry.entityType,
    entityId: entry.entityId,
    actorUsername: entry.actorUsername || actor.username,
    actorDisplayName: entry.actorDisplayName || actor.displayName,
    actorRole: entry.actorRole || actor.role,
    summary: entry.summary,
    detail: entry.detail,
    ipAddress: getClientIp(req),
    createdAt: new Date().toISOString()
  }));

  if (store.auditLogs.length > 250) {
    store.auditLogs.length = 250;
  }
}

function getClientIp(req) {
  const forwarded = req.headers["x-forwarded-for"];
  if (typeof forwarded === "string" && forwarded.trim()) {
    return forwarded.split(",")[0].trim();
  }
  return "";
}

async function handleLogin(req, res) {
  if (req.method !== "POST") {
    return sendJson(res, 405, { message: "Method Not Allowed" });
  }

  const body = await readBody(req);
  const username = String(body.username || body.Username || "").trim().toLowerCase();
  const password = String(body.password || body.Password || "");
  const candidate = DEMO_USERS[username];

  if (!candidate || candidate.password !== password) {
    writeAuditLog(req, {
      action: AUDIT_ACTIONS.LOGIN_FAILED,
      entityType: AUDIT_ENTITIES.AUTH,
      entityId: username || "unknown",
      summary: "Dang nhap that bai.",
      detail: `Tai khoan ${username || "trong"} dang nhap that bai.`,
      actorUsername: username || "anonymous",
      actorDisplayName: "Dang nhap that bai",
      actorRole: "Unknown"
    });
    return sendJson(res, 401, { message: "Unauthorized" });
  }

  setSessionCookie(res, candidate);
  writeAuditLog(req, {
    action: AUDIT_ACTIONS.LOGIN_SUCCESS,
    entityType: AUDIT_ENTITIES.AUTH,
    entityId: candidate.username,
    summary: "Dang nhap thanh cong.",
    detail: `${candidate.displayName} dang nhap vao he thong voi vai tro ${candidate.role}.`,
    actorUsername: candidate.username,
    actorDisplayName: candidate.displayName,
    actorRole: candidate.role
  });

  return sendJson(res, 200, {
    Username: candidate.username,
    DisplayName: candidate.displayName,
    Role: candidate.role,
    RedirectPath: getLandingPathForRole(candidate.role),
    username: candidate.username,
    displayName: candidate.displayName,
    role: candidate.role,
    redirectPath: getLandingPathForRole(candidate.role)
  });
}

function handleLogout(req, res) {
  if (req.method !== "POST") {
    return sendJson(res, 405, { message: "Method Not Allowed" });
  }

  const session = readSession(req);
  if (session) {
    writeAuditLog(req, {
      action: AUDIT_ACTIONS.LOGOUT,
      entityType: AUDIT_ENTITIES.AUTH,
      entityId: session.username,
      summary: "Dang xuat.",
      detail: `${session.displayName} dang xuat khoi he thong.`,
      actorUsername: session.username,
      actorDisplayName: session.displayName,
      actorRole: session.role
    });
  }

  clearSessionCookie(res);
  return sendJson(res, 200, { message: "Da dang xuat." });
}

function handleMe(req, res) {
  if (req.method !== "GET") {
    return sendJson(res, 405, { message: "Method Not Allowed" });
  }

  const session = readSession(req);
  if (!session) {
    return sendJson(res, 401, { message: "Unauthorized" });
  }

  return sendJson(res, 200, {
    Username: session.username,
    DisplayName: session.displayName,
    Role: session.role,
    RedirectPath: getLandingPathForRole(session.role),
    username: session.username,
    displayName: session.displayName,
    role: session.role,
    redirectPath: getLandingPathForRole(session.role)
  });
}

async function handleAnalyze(req, res) {
  if (req.method !== "POST") {
    return sendJson(res, 405, { message: "Method Not Allowed" });
  }

  const body = await readBody(req);
  const assessment = analyzeIncident(body.Title || body.title, body.Detail || body.detail, body.Level || body.level);

  writeAuditLog(req, {
    action: AUDIT_ACTIONS.ANALYZE_INCIDENT,
    entityType: AUDIT_ENTITIES.INCIDENT,
    entityId: "preview",
    summary: "Phan tich muc do khan cap.",
    detail: `He thong phan tich yeu cau preview va danh gia ${assessment.Category} - ${assessment.Level}.`
  });

  return sendJson(res, 200, assessment);
}

function handleHealth(req, res) {
  if (req.method !== "GET") {
    return sendJson(res, 405, { message: "Method Not Allowed" });
  }

  return sendJson(res, 200, {
    status: "ok",
    databaseProvider: "memory",
    signalRHub: "/hubs/incidents",
    timestamp: new Date().toISOString()
  });
}

function handleListIncidents(req, res) {
  if (req.method !== "GET") {
    return sendJson(res, 405, { message: "Method Not Allowed" });
  }

  const session = requireAuth(req, res, [APP_ROLES.Admin, APP_ROLES.Police, APP_ROLES.Support]);
  if (!session) {
    return;
  }

  const store = getStore();
  return sendJson(res, 200, filterIncidents(store.incidents, req.query || {}));
}

async function handleCreateIncident(req, res) {
  if (req.method !== "POST") {
    return sendJson(res, 405, { message: "Method Not Allowed" });
  }

  const session = requireAuth(req, res, [APP_ROLES.User]);
  if (!session) {
    return;
  }

  const body = await readBody(req);
  const title = String(body.Title || body.title || "").trim();
  const location = String(body.Location || body.location || "").trim();
  const detail = String(body.Detail || body.detail || "").trim();
  const requestedLevel = body.Level || body.level;

  if (!title || !location) {
    return sendJson(res, 400, { message: "Can co loai vu viec va toa do." });
  }

  const coords = tryParseLocation(location);
  if (!coords) {
    return sendJson(res, 400, { message: "Toa do khong hop le. Dung dinh dang '10.7769, 106.7009'." });
  }

  const assessment = analyzeIncident(title, detail, requestedLevel);
  const now = new Date();
  const incident = makeIncident({
    id: crypto.randomUUID(),
    title,
    detail: detail || "Nguoi dung vua gui bao cao moi.",
    category: assessment.Category,
    level: assessment.Level,
    urgencyScore: assessment.UrgencyScore,
    classificationReason: assessment.Reason,
    latitude: coords.latitude,
    longitude: coords.longitude,
    district: resolveDistrict(coords.latitude, coords.longitude),
    timeLabel: now.toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit", hour12: false }),
    status: assessment.UrgencyScore >= 85 ? INCIDENT_STATUSES.DANG_XAC_MINH : INCIDENT_STATUSES.MOI_TIEP_NHAN,
    source: "user",
    reporterName: session.displayName,
    lastUpdatedBy: session.displayName,
    internalNote: assessment.Recommendation,
    createdAt: now.toISOString(),
    updatedAt: now.toISOString()
  });

  const store = getStore();
  store.incidents.unshift(incident);

  writeAuditLog(req, {
    action: AUDIT_ACTIONS.CREATE_INCIDENT,
    entityType: AUDIT_ENTITIES.INCIDENT,
    entityId: incident.Id,
    summary: "Tao bao cao moi.",
    detail: `${session.displayName} tao bao cao ${incident.Title} voi muc ${incident.Level} (${incident.Category}).`,
    actorUsername: session.username,
    actorDisplayName: session.displayName,
    actorRole: session.role
  });

  return sendJson(res, 200, {
    message: assessment.ShouldCallEmergency
      ? "Da gui bao cao thanh cong. He thong danh gia day la tinh huong khan cap cao."
      : "Da gui bao cao thanh cong.",
    analysis: assessment,
    incident
  });
}

function handleGetIncident(req, res, id) {
  if (req.method !== "GET") {
    return sendJson(res, 405, { message: "Method Not Allowed" });
  }

  const session = requireAuth(req, res, [APP_ROLES.Admin, APP_ROLES.Police, APP_ROLES.Support, APP_ROLES.User]);
  if (!session) {
    return;
  }

  const store = getStore();
  const incident = store.incidents.find((item) => item.Id === id);
  if (!incident) {
    return sendJson(res, 404, { message: "Khong tim thay vu viec." });
  }

  return sendJson(res, 200, incident);
}

async function handleUpdateIncidentStatus(req, res, id) {
  if (req.method !== "PATCH") {
    return sendJson(res, 405, { message: "Method Not Allowed" });
  }

  const session = requireAuth(req, res, [APP_ROLES.Admin, APP_ROLES.Police]);
  if (!session) {
    return;
  }

  const body = await readBody(req);
  const normalizedStatus = normalizeStatus(body.Status || body.status);

  if (!canUpdateIncidentStatus(session.role, normalizedStatus)) {
    writeAuditLog(req, {
      action: AUDIT_ACTIONS.UPDATE_INCIDENT_DENIED,
      entityType: AUDIT_ENTITIES.INCIDENT,
      entityId: id,
      summary: "Bi tu choi cap nhat trang thai.",
      detail: `${session.displayName} khong du quyen cap nhat trang thai sang ${normalizedStatus}.`,
      actorUsername: session.username,
      actorDisplayName: session.displayName,
      actorRole: session.role
    });

    return sendJson(res, 403, { message: "Vai tro hien tai khong du quyen cap nhat trang thai nay." });
  }

  const store = getStore();
  const incident = store.incidents.find((item) => item.Id === id);
  if (!incident) {
    return sendJson(res, 404, { message: "Khong tim thay vu viec." });
  }

  incident.Status = normalizedStatus;
  incident.LastUpdatedBy = session.displayName;
  incident.UpdatedAt = new Date().toISOString();

  const note = String(body.InternalNote || body.internalNote || "").trim();
  if (note) {
    incident.InternalNote = note;
  }

  writeAuditLog(req, {
    action: AUDIT_ACTIONS.UPDATE_INCIDENT_STATUS,
    entityType: AUDIT_ENTITIES.INCIDENT,
    entityId: incident.Id,
    summary: "Cap nhat trang thai vu viec.",
    detail: `${session.displayName} cap nhat vu viec ${incident.Title} sang ${incident.Status}.`,
    actorUsername: session.username,
    actorDisplayName: session.displayName,
    actorRole: session.role
  });

  return sendJson(res, 200, {
    message: "Da cap nhat trang thai.",
    incident
  });
}

function handleExportIncidents(req, res) {
  if (req.method !== "GET") {
    return sendJson(res, 405, { message: "Method Not Allowed" });
  }

  const session = requireAuth(req, res, [APP_ROLES.Admin]);
  if (!session) {
    return;
  }

  const store = getStore();
  const incidents = filterIncidents(store.incidents, req.query || {});
  writeAuditLog(req, {
    action: AUDIT_ACTIONS.EXPORT_INCIDENTS,
    entityType: AUDIT_ENTITIES.INCIDENT,
    entityId: `count:${incidents.length}`,
    summary: "Xuat bao cao vu viec.",
    detail: `${session.displayName} xuat ${incidents.length} dong du lieu bao cao.`,
    actorUsername: session.username,
    actorDisplayName: session.displayName,
    actorRole: session.role
  });

  return sendCsv(res, `incident-export-${new Date().toISOString().replace(/[:.]/g, "-")}.csv`, buildIncidentCsv(incidents));
}

function handleAuditLogs(req, res) {
  if (req.method !== "GET") {
    return sendJson(res, 405, { message: "Method Not Allowed" });
  }

  const session = requireAuth(req, res, [APP_ROLES.Admin]);
  if (!session) {
    return;
  }

  const store = getStore();
  let logs = store.auditLogs.slice();
  const action = String(req.query.action || "").trim().toLowerCase();
  const actorRole = String(req.query.actorRole || "").trim().toLowerCase();
  const entityType = String(req.query.entityType || "").trim().toLowerCase();
  const limit = Math.max(1, Math.min(200, Number(req.query.limit || 50) || 50));

  if (action) {
    logs = logs.filter((item) => item.Action.toLowerCase() === action);
  }
  if (actorRole) {
    logs = logs.filter((item) => item.ActorRole.toLowerCase() === actorRole);
  }
  if (entityType) {
    logs = logs.filter((item) => item.EntityType.toLowerCase() === entityType);
  }

  logs.sort((a, b) => new Date(b.CreatedAt) - new Date(a.CreatedAt));
  return sendJson(res, 200, logs.slice(0, limit));
}

module.exports = async (req, res) => {
  const queryPathParts = Array.isArray(req.query?.path)
    ? req.query.path
    : typeof req.query?.path === "string"
      ? [req.query.path]
      : [];
  const requestUrl = new URL(req.url || "/", `https://${req.headers.host || "localhost"}`);
  const pathnameParts = requestUrl.pathname
    .split("/")
    .filter(Boolean)
    .slice(1);
  const pathParts = queryPathParts.length > 0 ? queryPathParts : pathnameParts;

  res.setHeader("Cache-Control", "no-store");

  if (pathParts.length === 0) {
    return sendJson(res, 404, { message: "Not found" });
  }

  if (pathParts[0] === "auth" && pathParts[1] === "login") {
    return handleLogin(req, res);
  }

  if (pathParts[0] === "auth" && pathParts[1] === "logout") {
    return handleLogout(req, res);
  }

  if (pathParts[0] === "auth" && pathParts[1] === "me") {
    return handleMe(req, res);
  }

  if (pathParts[0] === "health") {
    return handleHealth(req, res);
  }

  if (pathParts[0] === "audit-logs") {
    return handleAuditLogs(req, res);
  }

  if (pathParts[0] === "incidents" && pathParts.length === 1) {
    if (req.method === "GET") {
      return handleListIncidents(req, res);
    }
    if (req.method === "POST") {
      return handleCreateIncident(req, res);
    }
    return sendJson(res, 405, { message: "Method Not Allowed" });
  }

  if (pathParts[0] === "incidents" && pathParts[1] === "analyze") {
    return handleAnalyze(req, res);
  }

  if (pathParts[0] === "incidents" && pathParts[1] === "export") {
    return handleExportIncidents(req, res);
  }

  if (pathParts[0] === "incidents" && pathParts[2] === "status") {
    return handleUpdateIncidentStatus(req, res, pathParts[1]);
  }

  if (pathParts[0] === "incidents" && pathParts[1]) {
    return handleGetIncident(req, res, pathParts[1]);
  }

  return sendJson(res, 404, { message: "Not found" });
};

const els = {
  baseUrl: document.getElementById("baseUrl"),
  saveBtn: document.getElementById("saveBtn"),
  form: document.getElementById("form"),
  studentId: document.getElementById("studentId"),
  studentName: document.getElementById("studentName"),
  taskCount: document.getElementById("taskCount"),
  averageGrade: document.getElementById("averageGrade"),
  output: document.getElementById("output"),
  statusPill: document.getElementById("statusPill"),
  pdfBtn: document.getElementById("pdfBtn"),
  docxBtn: document.getElementById("docxBtn"),
  resetBtn: document.getElementById("resetBtn"),
};

function setStatus(type, text) {
  els.statusPill.classList.remove("ok", "work", "err");
  if (type) els.statusPill.classList.add(type);
  els.statusPill.textContent = text;
}

function normalizeBaseUrl(url) {
  return (url || "").trim().replace(/\/+$/, "");
}

function getBaseUrl() {
  const saved = localStorage.getItem("report_base_url") || "";
  const current = normalizeBaseUrl(els.baseUrl.value || saved);
  return current;
}

function saveBaseUrl() {
  const u = normalizeBaseUrl(els.baseUrl.value);
  localStorage.setItem("report_base_url", u);
  setStatus("ok", "сохранено");
}

function getPayload() {
  return {
    studentId: Number(els.studentId.value),
    studentName: String(els.studentName.value || ""),
    taskCount: Number(els.taskCount.value),
    averageGrade: Number(els.averageGrade.value),
  };
}

function pretty(obj) {
  return JSON.stringify(obj, null, 2);
}

async function postJson(url, body) {
  const res = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  // Если сервер вернул JSON с ошибкой — покажем его
  const contentType = res.headers.get("content-type") || "";
  if (!res.ok) {
    if (contentType.includes("application/json")) {
      const err = await res.json().catch(() => ({}));
      throw new Error(`${res.status} ${res.statusText}: ${pretty(err)}`);
    }
    const text = await res.text().catch(() => "");
    throw new Error(`${res.status} ${res.statusText}: ${text}`);
  }

  return res;
}

async function downloadFile(url, payload, fileNameFallback) {
  const res = await postJson(url, payload);

  const blob = await res.blob();
  const cd = res.headers.get("content-disposition") || "";
  let filename = fileNameFallback;

  const match = /filename\*?=(?:UTF-8''|")?([^;"\n]+)/i.exec(cd);
  if (match && match[1]) {
    filename = decodeURIComponent(match[1].replace(/"/g, "").trim());
  }

  const link = document.createElement("a");
  link.href = URL.createObjectURL(blob);
  link.download = filename || fileNameFallback;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(link.href);
}

function init() {
  const saved = localStorage.getItem("report_base_url") || "https://localhost:7101";
  els.baseUrl.value = saved;
  setStatus("", "готово");
}

els.saveBtn.addEventListener("click", saveBaseUrl);

els.resetBtn.addEventListener("click", () => {
  els.studentId.value = 1;
  els.studentName.value = "Test Student";
  els.taskCount.value = 5;
  els.averageGrade.value = 4.2;
  els.output.textContent = "Пока пусто. Нажми «Сформировать отчёт».";
  setStatus("", "готово");
});

els.form.addEventListener("submit", async (e) => {
  e.preventDefault();
  const base = getBaseUrl();
  if (!base) {
    setStatus("err", "нет URL");
    els.output.textContent = "Укажи Backend URL, например https://localhost:7101";
    return;
  }

  const payload = getPayload();
  setStatus("work", "работаю...");
  els.output.textContent = "Отправляю запрос...";

  try {
    const res = await postJson(`${base}/api/report`, payload);
    const data = await res.json();
    els.output.textContent = pretty(data);
    setStatus("ok", "успех");
  } catch (err) {
    setStatus("err", "ошибка");
    els.output.textContent = String(err?.message || err);
  }
});

els.pdfBtn.addEventListener("click", async () => {
  const base = getBaseUrl();
  const payload = getPayload();
  if (!base) return setStatus("err", "нет URL");

  setStatus("work", "pdf...");
  try {
    await downloadFile(`${base}/api/report/export/pdf`, payload, `report_${payload.studentId}.pdf`);
    setStatus("ok", "pdf готов");
  } catch (err) {
    setStatus("err", "ошибка");
    els.output.textContent = String(err?.message || err);
  }
});

els.docxBtn.addEventListener("click", async () => {
  const base = getBaseUrl();
  const payload = getPayload();
  if (!base) return setStatus("err", "нет URL");

  setStatus("work", "docx...");
  try {
    await downloadFile(`${base}/api/report/export/docx`, payload, `report_${payload.studentId}.docx`);
    setStatus("ok", "docx готов");
  } catch (err) {
    setStatus("err", "ошибка");
    els.output.textContent = String(err?.message || err);
  }
});

init();

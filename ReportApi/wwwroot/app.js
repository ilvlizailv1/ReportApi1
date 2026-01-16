function byId(id) { return document.getElementById(id); }

const els = {
    studentId: byId("studentId"),
    studentName: byId("studentName"),
    taskCount: byId("taskCount"),
    averageGrade: byId("averageGrade"),
    email: byId("email"),

    jsonBtn: byId("jsonBtn"),
    pdfBtn: byId("pdfBtn"),
    docxBtn: byId("docxBtn"),
    sendCsvBtn: byId("sendCsvBtn"),
    resetBtn: byId("resetBtn"),

    output: byId("output"),
    statusPill: byId("statusPill"),
};

function setStatus(type, text) {
    if (!els.statusPill) return;
    els.statusPill.classList.remove("ok", "work", "err");
    if (type) els.statusPill.classList.add(type);
    els.statusPill.textContent = text;
}

function payload() {
    return {
        studentId: Number(els.studentId.value || 0),
        studentName: String(els.studentName.value || ""),
        taskCount: Number(els.taskCount.value || 0),
        averageGrade: Number(String(els.averageGrade.value || "0").replace(",", ".")),
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

    const text = await res.text().catch(() => "");
    if (!res.ok) throw new Error(`${res.status} ${res.statusText}: ${text}`);

    const ct = res.headers.get("content-type") || "";
    return ct.includes("application/json") ? JSON.parse(text || "{}") : text;
}

async function downloadFile(url, body, fallbackName) {
    const res = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
    });

    if (!res.ok) {
        const t = await res.text().catch(() => "");
        throw new Error(`${res.status} ${res.statusText}: ${t}`);
    }

    const blob = await res.blob();
    const cd = res.headers.get("content-disposition") || "";
    let filename = fallbackName;

    const match = /filename\*?=(?:UTF-8''|")?([^;"\n]+)/i.exec(cd);
    if (match?.[1]) filename = decodeURIComponent(match[1].replace(/"/g, "").trim());

    const a = document.createElement("a");
    a.href = URL.createObjectURL(blob);
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(a.href);
}

function out(text) {
    els.output.textContent = text;
}

// JSON
els.jsonBtn.addEventListener("click", async () => {
    setStatus("work", "работаю...");
    out("Отправляю запрос...");
    try {
        const data = await postJson(`/api/report`, payload());
        out(pretty(data));
        setStatus("ok", "успех");
    } catch (e) {
        setStatus("err", "ошибка");
        out(String(e?.message || e));
    }
});

// PDF
els.pdfBtn.addEventListener("click", async () => {
    setStatus("work", "pdf...");
    try {
        const p = payload();
        await downloadFile(`/api/report/export/pdf`, p, `report_${p.studentId}.pdf`);
        setStatus("ok", "pdf готов");
    } catch (e) {
        setStatus("err", "ошибка");
        out(String(e?.message || e));
    }
});

// DOCX
els.docxBtn.addEventListener("click", async () => {
    setStatus("work", "docx...");
    try {
        const p = payload();
        await downloadFile(`/api/report/export/docx`, p, `report_${p.studentId}.docx`);
        setStatus("ok", "docx готов");
    } catch (e) {
        setStatus("err", "ошибка");
        out(String(e?.message || e));
    }
});

// Отправка CSV на email
els.sendCsvBtn.addEventListener("click", async () => {
    const email = (els.email.value || "").trim();
    if (!email) {
        setStatus("err", "нет email");
        out("Введи email для отправки CSV.");
        return;
    }

    setStatus("work", "отправляю...");
    out("Формирую CSV и отправляю через OtpravkaApi...");

    try {
        const data = await postJson(`/api/integration/send-csv-to-email?email=${encodeURIComponent(email)}`, payload());
        out(pretty(data));
        setStatus("ok", "отправлено");
    } catch (e) {
        setStatus("err", "ошибка");
        out(String(e?.message || e));
    }
});

// Сброс
els.resetBtn.addEventListener("click", () => {
    els.studentId.value = 1;
    els.studentName.value = "Test Student";
    els.taskCount.value = 5;
    els.averageGrade.value = "4,2";
    els.email.value = "";
    out("Пока пусто. Нажми кнопку действия.");
    setStatus("", "готово");
});

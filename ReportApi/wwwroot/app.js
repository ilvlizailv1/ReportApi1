const els = {
    form: document.getElementById("form"),
    studentId: document.getElementById("studentId"),
    studentName: document.getElementById("studentName"),
    taskCount: document.getElementById("taskCount"),
    averageGrade: document.getElementById("averageGrade"),
    email: document.getElementById("email"),
    output: document.getElementById("output"),
    statusPill: document.getElementById("statusPill"),
    pdfBtn: document.getElementById("pdfBtn"),
    docxBtn: document.getElementById("docxBtn"),
    sendEmailBtn: document.getElementById("sendEmailBtn"),
    resetBtn: document.getElementById("resetBtn"),
};

function setStatus(type, text) {
    els.statusPill.className = "pill";
    if (type) els.statusPill.classList.add(type);
    els.statusPill.textContent = text;
}

function payload() {
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

    const ct = res.headers.get("content-type") || "";
    if (!res.ok) {
        const txt = ct.includes("application/json")
            ? pretty(await res.json().catch(() => ({})))
            : await res.text().catch(() => "");
        throw new Error(`${res.status} ${res.statusText}: ${txt}`);
    }

    return res;
}

async function downloadFile(url, body, fallbackName) {
    const res = await postJson(url, body);
    const blob = await res.blob();

    const cd = res.headers.get("content-disposition") || "";
    let filename = fallbackName;
    const match = /filename\*?=(?:UTF-8''|")?([^;"\n]+)/i.exec(cd);
    if (match && match[1]) filename = decodeURIComponent(match[1].replace(/"/g, "").trim());

    const a = document.createElement("a");
    a.href = URL.createObjectURL(blob);
    a.download = filename || fallbackName;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(a.href);
}

// JSON
els.form.addEventListener("submit", async (e) => {
    e.preventDefault();
    setStatus("work", "работаю...");
    els.output.textContent = "Отправляю запрос...";

    try {
        const res = await postJson(`/api/report`, payload());
        const data = await res.json();
        els.output.textContent = pretty(data);
        setStatus("ok", "успех");
    } catch (err) {
        setStatus("err", "ошибка");
        els.output.textContent = String(err?.message || err);
    }
});

// PDF
els.pdfBtn.addEventListener("click", async () => {
    setStatus("work", "pdf...");
    try {
        const p = payload();
        await downloadFile(`/api/report/export/pdf`, p, `report_${p.studentId}.pdf`);
        setStatus("ok", "pdf готов");
    } catch (err) {
        setStatus("err", "ошибка");
        els.output.textContent = String(err?.message || err);
    }
});

// DOCX
els.docxBtn.addEventListener("click", async () => {
    setStatus("work", "docx...");
    try {
        const p = payload();
        await downloadFile(`/api/report/export/docx`, p, `report_${p.studentId}.docx`);
        setStatus("ok", "docx готов");
    } catch (err) {
        setStatus("err", "ошибка");
        els.output.textContent = String(err?.message || err);
    }
});

// SEND PDF EMAIL
els.sendEmailBtn.addEventListener("click", async () => {
    const mail = (els.email.value || "").trim();
    if (!mail) {
        setStatus("err", "нет email");
        els.output.textContent = "Заполни поле Email для отправки PDF.";
        return;
    }

    setStatus("work", "отправляю...");
    els.output.textContent = "Генерирую PDF и отправляю на почту...";

    try {
        const res = await postJson(`/api/integration/send-pdf-email?email=${encodeURIComponent(mail)}`, payload());
        const txt = await res.text();
        els.output.textContent = txt || "Письмо отправлено.";
        setStatus("ok", "готово");
    } catch (err) {
        setStatus("err", "ошибка");
        els.output.textContent = String(err?.message || err);
    }
});

els.resetBtn.addEventListener("click", () => {
    els.studentId.value = 1;
    els.studentName.value = "Test Student";
    els.taskCount.value = 5;
    els.averageGrade.value = 4.2;
    els.email.value = "";
    els.output.textContent = "Пока пусто. Нажми «Сформировать отчёт».";
    setStatus("", "готово");
});

setStatus("", "готово");

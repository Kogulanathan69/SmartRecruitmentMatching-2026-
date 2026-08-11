(() => {
  const $ = selector => document.querySelector(selector);
  const MAX_FILE_BYTES = 5 * 1024 * 1024;

  const state = {
    resumes: [],
    templates: [],
    selectedId: null,
    loadVersion: 0,
    detailVersion: 0,
    dialogResumeId: null,
    dialogBusy: false,
    uploadBusy: false,
    actionBusy: false,
    profileMissing: false,
    completeness: new Map(),
    preview: null
  };

  function clean(value) {
    return String(value ?? "").trim();
  }

  function setupShell() {
    if (!window.NexHireSession.requireRole("JobSeeker")) return false;

    const name = window.NexHireSession.fullName() || "Job seeker";
    document.querySelectorAll("[data-user-name]").forEach(node => {
      node.textContent = name;
    });
    document.querySelectorAll("[data-user-role]").forEach(node => {
      node.textContent = "Job seeker";
    });
    document.querySelectorAll("[data-user-initials]").forEach(node => {
      node.textContent = window.NexHireSession.initials(name);
    });

    $("[data-logout]")?.addEventListener("click", window.NexHireSession.logout);
    const menu = $("[data-menu]");
    const scrim = $("[data-scrim]");
    menu?.addEventListener("click", () => document.body.classList.toggle("menu-open"));
    scrim?.addEventListener("click", () => document.body.classList.remove("menu-open"));
    window.addEventListener("keydown", event => {
      if (event.key === "Escape" && !$("#resumeDialog")?.open && !$("#uploadDialog")?.open) {
        document.body.classList.remove("menu-open");
      }
    });
    return true;
  }

  function text(tag, value, className = "") {
    const node = document.createElement(tag);
    if (className) node.className = className;
    node.textContent = value ?? "";
    return node;
  }

  function setBadge(label, kind = "") {
    const badge = $("#resumesState");
    if (!badge) return;
    badge.textContent = label;
    badge.className = `badge${kind ? ` ${kind}` : ""}`;
  }

  function setMessage(message = "", kind = "") {
    const box = $("#resumesMessage");
    if (!box) return;
    box.textContent = message;
    box.className = `status-message${kind ? ` ${kind}` : ""}`;
  }

  function setDialogMessage(message = "", kind = "") {
    const box = $("#resumeDialogMessage");
    if (!box) return;
    box.textContent = message;
    box.className = `status-message${kind ? ` ${kind}` : ""}`;
  }

  function setUploadMessage(message = "", kind = "") {
    const box = $("#uploadDialogMessage");
    if (!box) return;
    box.textContent = message;
    box.className = `status-message${kind ? ` ${kind}` : ""}`;
  }

  function isUploaded(resume) {
    return clean(resume?.fileUrl).toLowerCase().endsWith("/file");
  }

  function formatDate(value) {
    if (!value) return "Not specified";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "Not specified";
    return new Intl.DateTimeFormat(undefined, { dateStyle: "medium" }).format(date);
  }

  function currentResume() {
    return state.resumes.find(resume => String(resume.id) === String(state.selectedId)) || null;
  }

  function filteredResumes() {
    const query = clean($("#resumeSearch")?.value).toLocaleLowerCase();
    const type = $("#resumeTypeFilter")?.value || "";
    return state.resumes.filter(resume => {
      const matchesName = !query || clean(resume.resumeName).toLocaleLowerCase().includes(query);
      const resumeType = isUploaded(resume) ? "uploaded" : "builder";
      return matchesName && (!type || type === resumeType);
    });
  }

  function clearPreview() {
    if (state.preview?.url) URL.revokeObjectURL(state.preview.url);
    state.preview = null;
  }

  function renderSummary() {
    $("#resumeTotal").textContent = String(state.resumes.length);
    const primary = state.resumes.find(resume => resume.isPrimary);
    $("#resumePrimary").textContent = primary?.resumeName || "None";
    const bestScore = state.resumes.reduce(
      (best, resume) => Math.max(best, Number(resume.completenessScore || 0)),
      0
    );
    $("#resumeBestScore").textContent = `${bestScore}%`;
  }

  function emptyList(title, copy, link = null) {
    const wrapper = document.createElement("div");
    wrapper.className = "resume-empty";
    wrapper.append(text("strong", title), text("p", copy));
    if (link) {
      const anchor = document.createElement("a");
      anchor.className = "btn btn-primary btn-small";
      anchor.href = link.href;
      anchor.textContent = link.label;
      wrapper.appendChild(anchor);
    }
    return wrapper;
  }

  function renderResumeRow(resume) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = `resume-row${String(resume.id) === String(state.selectedId) ? " active" : ""}`;
    button.dataset.resumeId = resume.id;
    button.setAttribute("aria-pressed", String(String(resume.id) === String(state.selectedId)));

    const top = document.createElement("div");
    top.className = "resume-row-top";
    top.appendChild(text("strong", resume.resumeName || "Untitled CV"));
    if (resume.isPrimary) top.appendChild(text("span", "Primary", "resume-primary-pill"));

    const metadata = document.createElement("div");
    metadata.className = "resume-row-meta";
    metadata.append(
      text("span", isUploaded(resume) ? "Uploaded CV" : "Builder CV"),
      text("span", `${Number(resume.completenessScore || 0)}% ready`),
      text("span", resume.templateName || (isUploaded(resume) ? "Original file" : "Classic default"))
    );

    const foot = document.createElement("div");
    foot.className = "resume-row-foot";
    foot.append(
      text("span", resume.qualityRating || "Needs Improvement"),
      text("span", formatDate(resume.updatedAt || resume.createdAt))
    );

    button.append(top, metadata, foot);
    button.addEventListener("click", () => selectResume(resume.id));
    return button;
  }

  function renderList() {
    renderSummary();
    const list = $("#resumeList");
    if (!list) return;
    list.replaceChildren();

    const visible = filteredResumes();
    $("#resumeCount").textContent = `${visible.length} CV${visible.length === 1 ? "" : "s"}`;

    if (state.profileMissing) {
      list.appendChild(emptyList(
        "Create your profile first",
        "The CV Builder uses your verified profile evidence.",
        { href: "/app/jobseeker/profile.html", label: "Create profile" }
      ));
      return;
    }

    if (!state.resumes.length) {
      list.appendChild(emptyList(
        "No CVs yet",
        "Build an ATS-friendly CV from your profile or upload an existing document."
      ));
      return;
    }

    if (!visible.length) {
      list.appendChild(emptyList(
        "No matching CVs",
        "Change the search text or type filter to see another document."
      ));
      return;
    }

    visible.forEach(resume => list.appendChild(renderResumeRow(resume)));
  }

  function addFact(container, label, value) {
    const fact = document.createElement("div");
    fact.className = "resume-fact";
    fact.append(text("span", label), text("strong", value));
    container.appendChild(fact);
  }

  function actionButton(label, handler, className = "btn btn-secondary btn-small") {
    const button = document.createElement("button");
    button.type = "button";
    button.className = className;
    button.textContent = label;
    button.disabled = state.actionBusy;
    button.addEventListener("click", handler);
    return button;
  }

  function renderPreview(container, resume) {
    if (!state.preview || String(state.preview.resumeId) !== String(resume.id)) return;

    const preview = document.createElement("section");
    preview.className = "resume-preview";
    const head = document.createElement("header");
    head.append(text("strong", "Generated preview"));
    const open = document.createElement("a");
    open.href = state.preview.url;
    open.target = "_blank";
    open.rel = "noopener";
    open.className = "text-link";
    open.textContent = "Open full preview";
    head.appendChild(open);

    const frame = document.createElement("iframe");
    frame.title = `${resume.resumeName} generated CV preview`;
    frame.src = state.preview.url;
    preview.append(head, frame);
    container.appendChild(preview);
  }

  function renderDetail(resume = currentResume()) {
    const detail = $("#resumeDetail");
    if (!detail) return;
    detail.replaceChildren();

    if (!resume) {
      detail.appendChild(emptyList(
        state.profileMissing ? "Profile setup required" : "Select a CV",
        state.profileMissing
          ? "Create your professional profile before using the CV workspace."
          : "Choose a document to review readiness, preview and download actions."
      ));
      return;
    }

    const head = document.createElement("header");
    head.className = "resume-detail-head";
    const type = text("span", isUploaded(resume) ? "Uploaded document" : "NexHire CV Builder", "detail-company");
    const title = text("h2", resume.resumeName || "Untitled CV");
    const labels = document.createElement("div");
    labels.className = "resume-detail-labels";
    if (resume.isPrimary) labels.appendChild(text("span", "Primary CV", "status-chip"));
    labels.appendChild(text("span", isUploaded(resume) ? "Secure file" : (resume.isGenerated ? "Generated" : "Draft"), "status-chip"));
    labels.appendChild(text("span", resume.templateName || (isUploaded(resume) ? "Original format" : "Classic default"), "status-chip"));

    const actions = document.createElement("div");
    actions.className = "resume-detail-actions";
    actions.appendChild(actionButton("Edit details", () => openResumeDialog(resume)));
    if (!resume.isPrimary) {
      actions.appendChild(actionButton("Make primary", () => makePrimary(resume)));
    }
    if (isUploaded(resume)) {
      actions.appendChild(actionButton("Download file", () => downloadResume(resume), "btn btn-primary btn-small"));
    } else {
      actions.appendChild(actionButton(
        resume.isGenerated ? "Refresh preview" : "Generate preview",
        () => generatePreview(resume),
        "btn btn-primary btn-small"
      ));
      if (resume.isGenerated) {
        actions.appendChild(actionButton("Download HTML", () => downloadResume(resume)));
      }
    }
    actions.appendChild(actionButton("Delete", () => deleteResume(resume), "btn btn-danger btn-small"));
    head.append(type, title, labels, actions);

    const body = document.createElement("div");
    body.className = "resume-detail-body";
    const score = document.createElement("section");
    score.className = "resume-score-block";
    const meter = document.createElement("div");
    meter.className = "resume-score";
    meter.setAttribute("role", "progressbar");
    meter.setAttribute("aria-valuemin", "0");
    meter.setAttribute("aria-valuemax", "100");
    meter.setAttribute("aria-valuenow", String(Number(resume.completenessScore || 0)));
    meter.append(text("strong", `${Number(resume.completenessScore || 0)}%`), text("span", "CV readiness"));
    const scoreCopy = document.createElement("div");
    scoreCopy.append(
      text("h3", resume.qualityRating || "Needs Improvement"),
      text("p", isUploaded(resume)
        ? "Readiness reflects the NexHire profile evidence available alongside this uploaded document."
        : "Readiness reflects the evidence that will be included when this builder CV is generated.")
    );
    score.append(meter, scoreCopy);

    const facts = document.createElement("div");
    facts.className = "resume-facts";
    addFact(facts, "Document type", isUploaded(resume) ? "Uploaded CV" : "Builder CV");
    addFact(facts, "Template", resume.templateName || (isUploaded(resume) ? "Original format" : "Classic default"));
    addFact(facts, "Created", formatDate(resume.createdAt));
    addFact(facts, "Last updated", formatDate(resume.updatedAt || resume.createdAt));

    const completeness = state.completeness.get(String(resume.id));
    const guidance = document.createElement("section");
    guidance.className = "resume-guidance";
    guidance.appendChild(text("h3", "Readiness guidance"));
    if (!completeness) {
      guidance.appendChild(text("p", "Checking profile evidence…", "muted"));
    } else {
      const missing = Array.isArray(completeness.missingSections) ? completeness.missingSections : [];
      const recommendations = Array.isArray(completeness.recommendations) ? completeness.recommendations : [];
      if (!missing.length) {
        guidance.appendChild(text("p", "All scored sections are represented. Review the content before downloading.", "resume-ready-copy"));
      } else {
        const list = document.createElement("ul");
        missing.forEach((section, index) => {
          const item = document.createElement("li");
          item.append(text("strong", section), text("span", recommendations[index] || "Add this evidence to strengthen the CV."));
          list.appendChild(item);
        });
        guidance.appendChild(list);
      }
    }

    body.append(score, facts, guidance);
    renderPreview(body, resume);
    detail.append(head, body);
  }

  async function loadCompleteness(resumeId) {
    const requestVersion = ++state.detailVersion;
    try {
      const result = await window.NexHireApi.request(
        `/api/resumes/completeness?resumeId=${encodeURIComponent(resumeId)}`
      );
      if (requestVersion !== state.detailVersion || String(state.selectedId) !== String(resumeId)) return;
      state.completeness.set(String(resumeId), result);
      renderDetail();
    } catch (error) {
      if (requestVersion !== state.detailVersion || String(state.selectedId) !== String(resumeId)) return;
      state.completeness.set(String(resumeId), {
        missingSections: ["Readiness unavailable"],
        recommendations: [error.message || "The readiness details could not be loaded."]
      });
      renderDetail();
    }
  }

  function selectResume(resumeId) {
    if (String(state.selectedId) !== String(resumeId)) clearPreview();
    state.selectedId = resumeId;
    renderList();
    renderDetail();
    loadCompleteness(resumeId);
  }

  function keepValidSelection() {
    const visible = filteredResumes();
    if (!visible.length) {
      state.selectedId = null;
      clearPreview();
      renderList();
      renderDetail(null);
      return;
    }

    if (!visible.some(resume => String(resume.id) === String(state.selectedId))) {
      clearPreview();
      state.selectedId = visible[0].id;
    }
    renderList();
    renderDetail();
    loadCompleteness(state.selectedId);
  }

  function sortResumes(resumes) {
    return [...resumes].sort((left, right) => {
      if (Boolean(left.isPrimary) !== Boolean(right.isPrimary)) return left.isPrimary ? -1 : 1;
      const leftDate = new Date(left.updatedAt || left.createdAt || 0).getTime();
      const rightDate = new Date(right.updatedAt || right.createdAt || 0).getTime();
      return rightDate - leftDate;
    });
  }

  async function loadWorkspace(preferredId = null, successMessage = "") {
    const requestVersion = ++state.loadVersion;
    setBadge("Loading");
    if (!successMessage) setMessage();

    const [resumesResult, templatesResult] = await Promise.allSettled([
      window.NexHireApi.request("/api/resumes"),
      window.NexHireApi.request("/api/resumes/templates")
    ]);
    if (requestVersion !== state.loadVersion) return;

    if (templatesResult.status === "fulfilled") {
      state.templates = Array.isArray(templatesResult.value) ? templatesResult.value : [];
    } else {
      state.templates = [];
    }

    if (resumesResult.status === "rejected") {
      const error = resumesResult.reason;
      if (error?.status === 404) {
        state.profileMissing = true;
        state.resumes = [];
        state.selectedId = null;
        clearPreview();
        $("#openCreateResume").disabled = true;
        $("#openUploadResume").disabled = true;
        renderList();
        renderDetail(null);
        setBadge("Setup required");
        setMessage("Create your professional profile before building or uploading CVs.");
        return;
      }
      setBadge("Could not load", "bad");
      setMessage(error?.message || "Your CV workspace could not be loaded.", "error");
      return;
    }

    state.profileMissing = false;
    $("#openCreateResume").disabled = false;
    $("#openUploadResume").disabled = false;
    state.resumes = sortResumes(Array.isArray(resumesResult.value) ? resumesResult.value : []);
    const preferred = state.resumes.find(resume => String(resume.id) === String(preferredId));
    const retained = state.resumes.find(resume => String(resume.id) === String(state.selectedId));
    state.selectedId = preferred?.id || retained?.id || state.resumes[0]?.id || null;

    renderList();
    renderDetail();
    if (state.selectedId) loadCompleteness(state.selectedId);
    setBadge("Live CVs", "ok");
    if (successMessage) setMessage(successMessage, "success");
    if (templatesResult.status === "rejected" && !successMessage) {
      setMessage("CVs loaded, but template choices are temporarily unavailable.", "error");
    }
  }

  function populateTemplates(selectedId = "") {
    const select = $("#templateId");
    select.replaceChildren();
    const fallback = document.createElement("option");
    fallback.value = "";
    fallback.textContent = "Classic default";
    select.appendChild(fallback);
    state.templates.forEach(template => {
      const option = document.createElement("option");
      option.value = template.id;
      option.textContent = `${template.name}${template.isAtsFriendly ? " · ATS friendly" : ""}`;
      select.appendChild(option);
    });
    select.value = selectedId || "";
  }

  function setBuilderFieldsVisible(visible) {
    ["templateField", "objectiveField", "languagesField", "linkedInField", "gitHubField", "portfolioField"]
      .forEach(id => {
        const field = document.getElementById(id);
        if (field) field.hidden = !visible;
      });
  }

  function updateObjectiveCount() {
    const value = $("#careerObjective")?.value || "";
    $("#objectiveCount").textContent = `${value.length} / 1200 characters`;
  }

  function openResumeDialog(resume = null) {
    if (state.profileMissing) return;
    const uploaded = isUploaded(resume);
    state.dialogResumeId = resume?.id || null;
    state.dialogBusy = false;
    setDialogMessage();

    $("#resumeDialogEyebrow").textContent = uploaded
      ? "Uploaded CV details"
      : resume ? "Update CV Builder" : "CV Builder";
    $("#resumeDialogTitle").textContent = resume
      ? `Edit ${resume.resumeName || "CV"}`
      : "Build a CV";
    $("#saveResume").textContent = resume ? "Save changes" : "Create builder CV";
    $("#resumeName").value = resume?.resumeName || "";
    populateTemplates(resume?.templateId || "");
    $("#careerObjective").value = resume?.careerObjective || "";
    $("#languages").value = Array.isArray(resume?.languages) ? resume.languages.join(", ") : "";
    $("#linkedInUrl").value = resume?.linkedInUrl || "";
    $("#gitHubUrl").value = resume?.gitHubUrl || "";
    $("#portfolioUrl").value = resume?.portfolioUrl || "";
    $("#resumeIsPrimary").checked = Boolean(resume?.isPrimary);
    setBuilderFieldsVisible(!uploaded);
    updateObjectiveCount();

    $("#resumeDialog").showModal();
    $("#resumeName").focus();
  }

  function closeResumeDialog() {
    if (state.dialogBusy) return;
    $("#resumeDialog")?.close();
    state.dialogResumeId = null;
    setDialogMessage();
  }

  function setDialogBusy(busy) {
    state.dialogBusy = busy;
    $("#saveResume").disabled = busy;
    $("#cancelResume").disabled = busy;
    $("#closeResumeDialog").disabled = busy;
  }

  function parseLanguages(value) {
    const seen = new Set();
    return String(value || "")
      .split(",")
      .map(clean)
      .filter(language => {
        const key = language.toLocaleLowerCase();
        if (!language || seen.has(key)) return false;
        seen.add(key);
        return true;
      });
  }

  function recordPayload(resume) {
    return {
      resumeName: clean(resume.resumeName),
      templateId: resume.templateId || null,
      careerObjective: clean(resume.careerObjective) || null,
      languages: Array.isArray(resume.languages) ? resume.languages : [],
      linkedInUrl: clean(resume.linkedInUrl) || null,
      gitHubUrl: clean(resume.gitHubUrl) || null,
      portfolioUrl: clean(resume.portfolioUrl) || null,
      isPrimary: Boolean(resume.isPrimary)
    };
  }

  function formPayload(existing) {
    if (isUploaded(existing)) {
      return {
        ...recordPayload(existing),
        resumeName: clean($("#resumeName").value),
        isPrimary: $("#resumeIsPrimary").checked
      };
    }
    return {
      resumeName: clean($("#resumeName").value),
      templateId: $("#templateId").value || null,
      careerObjective: clean($("#careerObjective").value) || null,
      languages: parseLanguages($("#languages").value),
      linkedInUrl: clean($("#linkedInUrl").value) || null,
      gitHubUrl: clean($("#gitHubUrl").value) || null,
      portfolioUrl: clean($("#portfolioUrl").value) || null,
      isPrimary: $("#resumeIsPrimary").checked
    };
  }

  function validateLanguages(payload) {
    const input = $("#languages");
    input.setCustomValidity("");
    if (payload.languages.length > 10) {
      input.setCustomValidity("Add no more than 10 unique languages.");
      input.reportValidity();
      return false;
    }
    if (payload.languages.some(language => language.length > 80)) {
      input.setCustomValidity("Each language name must be 80 characters or fewer.");
      input.reportValidity();
      return false;
    }
    if (payload.languages.join(",").length > 500) {
      input.setCustomValidity("Languages cannot exceed 500 characters in total.");
      input.reportValidity();
      return false;
    }
    return true;
  }

  async function saveResume(event) {
    event.preventDefault();
    const form = $("#resumeForm");
    if (!form.reportValidity()) return;
    const existing = state.resumes.find(resume => String(resume.id) === String(state.dialogResumeId));
    const payload = formPayload(existing);
    if (!isUploaded(existing) && !validateLanguages(payload)) return;

    setDialogBusy(true);
    setDialogMessage();
    $("#saveResume").textContent = existing ? "Saving…" : "Creating…";
    try {
      const result = await window.NexHireApi.request(
        `/api/resumes${existing ? `/${existing.id}` : ""}`,
        { method: existing ? "PUT" : "POST", body: payload }
      );
      setDialogBusy(false);
      $("#resumeDialog").close();
      state.dialogResumeId = null;
      clearPreview();
      await loadWorkspace(result.id, existing ? "CV details updated." : "Builder CV created.");
    } catch (error) {
      setDialogBusy(false);
      $("#saveResume").textContent = existing ? "Save changes" : "Create builder CV";
      setDialogMessage(error.message || "The CV could not be saved.", "error");
    }
  }

  function openUploadDialog() {
    if (state.profileMissing) return;
    $("#uploadForm").reset();
    state.uploadBusy = false;
    $("#saveUpload").textContent = "Upload CV";
    setUploadMessage();
    $("#uploadDialog").showModal();
    $("#resumeFile").focus();
  }

  function closeUploadDialog() {
    if (state.uploadBusy) return;
    $("#uploadDialog")?.close();
    setUploadMessage();
  }

  function setUploadBusy(busy) {
    state.uploadBusy = busy;
    $("#saveUpload").disabled = busy;
    $("#cancelUpload").disabled = busy;
    $("#closeUploadDialog").disabled = busy;
  }

  function validateFile(file) {
    const input = $("#resumeFile");
    input.setCustomValidity("");
    if (!file) {
      input.setCustomValidity("Select a CV file.");
    } else if (file.size > MAX_FILE_BYTES) {
      input.setCustomValidity("CV file cannot exceed 5 MB.");
    } else if (!/\.(pdf|doc|docx)$/i.test(file.name)) {
      input.setCustomValidity("Only PDF, DOC and DOCX CV files are allowed.");
    }
    if (input.validationMessage) {
      input.reportValidity();
      return false;
    }
    return true;
  }

  async function saveUpload(event) {
    event.preventDefault();
    const form = $("#uploadForm");
    if (!form.reportValidity()) return;
    const file = $("#resumeFile").files?.[0];
    if (!validateFile(file)) return;

    const data = new FormData();
    data.append("file", file, file.name);
    data.append("resumeName", clean($("#uploadResumeName").value));
    data.append("isPrimary", String($("#uploadIsPrimary").checked));

    setUploadBusy(true);
    setUploadMessage();
    $("#saveUpload").textContent = "Uploading…";
    try {
      const result = await window.NexHireApi.request("/api/resumes/upload", {
        method: "POST",
        formData: data
      });
      setUploadBusy(false);
      $("#uploadDialog").close();
      clearPreview();
      await loadWorkspace(result.id, "CV uploaded securely.");
    } catch (error) {
      setUploadBusy(false);
      $("#saveUpload").textContent = "Upload CV";
      setUploadMessage(error.message || "The CV file could not be uploaded.", "error");
    }
  }

  async function runAction(work) {
    if (state.actionBusy) return;
    state.actionBusy = true;
    renderDetail();
    setMessage();
    try {
      await work();
    } finally {
      state.actionBusy = false;
      renderDetail();
    }
  }

  async function makePrimary(resume) {
    await runAction(async () => {
      try {
        const result = await window.NexHireApi.request(`/api/resumes/${resume.id}`, {
          method: "PUT",
          body: { ...recordPayload(resume), isPrimary: true }
        });
        clearPreview();
        await loadWorkspace(result.id, `“${result.resumeName}” is now your primary CV.`);
      } catch (error) {
        setMessage(error.message || "The primary CV could not be changed.", "error");
      }
    });
  }

  async function generatePreview(resume) {
    await runAction(async () => {
      setBadge("Generating");
      try {
        const html = await window.NexHireApi.request(`/api/resumes/${resume.id}/generate`, {
          method: "POST",
          responseType: "text"
        });
        clearPreview();
        state.preview = {
          resumeId: resume.id,
          url: URL.createObjectURL(new Blob([html], { type: "text/html" }))
        };
        await loadWorkspace(resume.id, "CV generated. Review the preview before downloading.");
        setBadge("Live CVs", "ok");
      } catch (error) {
        setBadge("Live CVs", "ok");
        setMessage(error.message || "The CV preview could not be generated.", "error");
      }
    });
  }

  function safeDownloadName(value) {
    return clean(value).replace(/[\\/:*?"<>|]+/g, "_") || "resume";
  }

  async function downloadResume(resume) {
    await runAction(async () => {
      try {
        const uploaded = isUploaded(resume);
        const blob = await window.NexHireApi.request(
          `/api/resumes/${resume.id}/${uploaded ? "file" : "download"}`,
          { responseType: "blob" }
        );
        const extensions = {
          "application/pdf": ".pdf",
          "application/msword": ".doc",
          "application/vnd.openxmlformats-officedocument.wordprocessingml.document": ".docx",
          "text/html": ".html"
        };
        const extension = extensions[blob.type] || (uploaded ? "" : ".html");
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = `${safeDownloadName(resume.resumeName)}${extension}`;
        document.body.appendChild(anchor);
        anchor.click();
        anchor.remove();
        setTimeout(() => URL.revokeObjectURL(url), 0);
        setMessage(`“${resume.resumeName}” download started.`, "success");
      } catch (error) {
        setMessage(error.message || "The CV could not be downloaded.", "error");
      }
    });
  }

  async function deleteResume(resume) {
    if (!window.confirm(`Delete “${resume.resumeName}”? This cannot be undone.`)) return;
    await runAction(async () => {
      try {
        await window.NexHireApi.request(`/api/resumes/${resume.id}`, { method: "DELETE" });
        const wasSelected = String(state.selectedId) === String(resume.id);
        if (wasSelected) clearPreview();
        state.completeness.delete(String(resume.id));
        await loadWorkspace(null, `“${resume.resumeName}” deleted.`);
      } catch (error) {
        setMessage(error.message || "The CV could not be deleted.", "error");
      }
    });
  }

  function bindEvents() {
    $("#openCreateResume")?.addEventListener("click", () => openResumeDialog());
    $("#openUploadResume")?.addEventListener("click", openUploadDialog);
    $("#resumeSearch")?.addEventListener("input", keepValidSelection);
    $("#resumeTypeFilter")?.addEventListener("change", keepValidSelection);
    $("#resumeForm")?.addEventListener("submit", saveResume);
    $("#careerObjective")?.addEventListener("input", updateObjectiveCount);
    $("#cancelResume")?.addEventListener("click", closeResumeDialog);
    $("#closeResumeDialog")?.addEventListener("click", closeResumeDialog);
    $("#resumeDialog")?.addEventListener("cancel", event => {
      if (state.dialogBusy) event.preventDefault();
    });
    $("#resumeDialog")?.addEventListener("close", () => {
      if (!state.dialogBusy) state.dialogResumeId = null;
    });
    $("#uploadForm")?.addEventListener("submit", saveUpload);
    $("#cancelUpload")?.addEventListener("click", closeUploadDialog);
    $("#closeUploadDialog")?.addEventListener("click", closeUploadDialog);
    $("#uploadDialog")?.addEventListener("cancel", event => {
      if (state.uploadBusy) event.preventDefault();
    });
    window.addEventListener("beforeunload", clearPreview);
  }

  async function init() {
    if (!setupShell()) return;
    bindEvents();
    await loadWorkspace();
  }

  document.addEventListener("DOMContentLoaded", init);
})();

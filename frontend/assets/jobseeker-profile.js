(() => {
  const $ = selector => document.querySelector(selector);

  const state = {
    profile: null,
    profileRequestVersion: 0,
    dialogKind: null,
    dialogRecordId: null,
    dialogBusy: false
  };

  const educationLevels = Object.freeze([
    { value: "", label: "Select qualification level", disabled: true },
    { value: 1, label: "School / secondary" },
    { value: 2, label: "Diploma / HND" },
    { value: 3, label: "Bachelor's degree" },
    { value: 4, label: "Postgraduate diploma" },
    { value: 5, label: "Master's degree" },
    { value: 6, label: "Doctorate / PhD" }
  ]);

  const sections = {
    skills: {
      singular: "skill",
      property: "skills",
      endpoint: "skills",
      list: "#skillsList",
      fields: [
        { name: "skillName", label: "Skill name", required: true, maxLength: 100, placeholder: "e.g. C#" },
        { name: "proficiencyLevel", label: "Proficiency level", type: "number", required: true, min: 1, max: 5, step: 1, value: 3 },
        { name: "yearsOfExperience", label: "Years using this skill", type: "number", required: true, min: 0, max: 60, step: 1, value: 0 }
      ],
      payload: form => ({
        skillName: clean(form.skillName.value),
        proficiencyLevel: Number(form.proficiencyLevel.value),
        yearsOfExperience: Number(form.yearsOfExperience.value)
      }),
      title: item => item.skillName || "Unnamed skill",
      subtitle: item => `Proficiency ${item.proficiencyLevel || 0} of 5`,
      meta: item => [`${item.yearsOfExperience || 0} year${Number(item.yearsOfExperience) === 1 ? "" : "s"} experience`]
    },
    education: {
      singular: "education",
      property: "educations",
      endpoint: "education",
      list: "#educationList",
      fields: [
        { name: "institution", label: "Institution", required: true, maxLength: 200, placeholder: "Institution name", wide: true },
        { name: "degree", label: "Degree or qualification", required: true, maxLength: 150, placeholder: "e.g. HND in Software Engineering" },
        { name: "educationLevel", label: "Qualification level", type: "select", required: true, options: educationLevels },
        { name: "fieldOfStudy", label: "Field of study", maxLength: 150, placeholder: "Optional" },
        { name: "startDate", label: "Start date", type: "date", required: true },
        { name: "endDate", label: "End date", type: "date" },
        { name: "gradeOrGpa", label: "Grade or GPA", maxLength: 50, placeholder: "Optional", wide: true }
      ],
      payload: form => ({
        institution: clean(form.institution.value),
        degree: clean(form.degree.value),
        educationLevel: Number(form.educationLevel.value),
        fieldOfStudy: optional(form.fieldOfStudy.value),
        startDate: form.startDate.value,
        endDate: form.endDate.value || null,
        gradeOrGpa: optional(form.gradeOrGpa.value)
      }),
      title: item => item.degree || "Qualification",
      subtitle: item => item.institution || "Institution not specified",
      meta: item => [educationLevelLabel(item.educationLevel), dateRange(item.startDate, item.endDate), item.fieldOfStudy, item.gradeOrGpa].filter(Boolean)
    },
    experience: {
      singular: "experience",
      property: "experiences",
      endpoint: "experience",
      list: "#experienceList",
      fields: [
        { name: "companyName", label: "Company", required: true, maxLength: 200, placeholder: "Company name" },
        { name: "jobTitle", label: "Job title", required: true, maxLength: 150, placeholder: "Role title" },
        { name: "startDate", label: "Start date", type: "date", required: true },
        { name: "endDate", label: "End date", type: "date" },
        { name: "isCurrent", label: "I currently work here", type: "checkbox", wide: true },
        { name: "description", label: "Responsibilities and outcomes", type: "textarea", maxLength: 2000, rows: 5, placeholder: "Describe the work and outcomes.", wide: true }
      ],
      payload: form => ({
        companyName: clean(form.companyName.value),
        jobTitle: clean(form.jobTitle.value),
        startDate: form.startDate.value,
        endDate: form.isCurrent.checked ? null : (form.endDate.value || null),
        isCurrent: form.isCurrent.checked,
        description: optional(form.description.value)
      }),
      title: item => item.jobTitle || "Role",
      subtitle: item => item.companyName || "Company not specified",
      meta: item => [dateRange(item.startDate, item.endDate, item.isCurrent)].filter(Boolean)
    },
    projects: {
      singular: "project",
      property: "projects",
      endpoint: "projects",
      list: "#projectsList",
      fields: [
        { name: "title", label: "Project title", required: true, maxLength: 200, placeholder: "Project name", wide: true },
        { name: "description", label: "Description", type: "textarea", maxLength: 2000, rows: 4, placeholder: "What problem did it solve?", wide: true },
        { name: "techStack", label: "Technology stack", maxLength: 500, placeholder: "e.g. ASP.NET Core, SQL Server", wide: true },
        { name: "projectUrl", label: "Project URL", type: "url", maxLength: 500, placeholder: "https://...", wide: true },
        { name: "startDate", label: "Start date", type: "date" },
        { name: "endDate", label: "End date", type: "date" }
      ],
      payload: form => ({
        title: clean(form.title.value),
        description: optional(form.description.value),
        techStack: optional(form.techStack.value),
        projectUrl: optional(form.projectUrl.value),
        startDate: form.startDate.value || null,
        endDate: form.endDate.value || null
      }),
      title: item => item.title || "Project",
      subtitle: item => item.description || item.techStack || "Project evidence",
      meta: item => [item.techStack, dateRange(item.startDate, item.endDate)].filter(Boolean)
    },
    certifications: {
      singular: "certification",
      property: "certifications",
      endpoint: "certifications",
      list: "#certificationsList",
      fields: [
        { name: "name", label: "Certification name", required: true, maxLength: 200, placeholder: "Certification", wide: true },
        { name: "issuingOrganization", label: "Issuing organization", maxLength: 200, placeholder: "Optional", wide: true },
        { name: "issueDate", label: "Issue date", type: "date" },
        { name: "expiryDate", label: "Expiry date", type: "date" },
        { name: "credentialUrl", label: "Credential URL", type: "url", maxLength: 500, placeholder: "https://...", wide: true }
      ],
      payload: form => ({
        name: clean(form.name.value),
        issuingOrganization: optional(form.issuingOrganization.value),
        issueDate: form.issueDate.value || null,
        expiryDate: form.expiryDate.value || null,
        credentialUrl: optional(form.credentialUrl.value)
      }),
      title: item => item.name || "Certification",
      subtitle: item => item.issuingOrganization || "Issuing organization not specified",
      meta: item => [item.issueDate ? `Issued ${formatDate(item.issueDate)}` : null, item.expiryDate ? `Expires ${formatDate(item.expiryDate)}` : null].filter(Boolean)
    }
  };

  function clean(value) {
    return String(value ?? "").trim();
  }

  function optional(value) {
    const result = clean(value);
    return result || null;
  }

  function educationLevelLabel(value) {
    return educationLevels.find(level => Number(level.value) === Number(value))?.label || "";
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
      if (event.key === "Escape" && !$("#recordDialog")?.open) {
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
    const badge = $("#profileState");
    if (!badge) return;
    badge.textContent = label;
    badge.className = `badge${kind ? ` ${kind}` : ""}`;
  }

  function setMessage(message = "", kind = "") {
    const box = $("#profileMessage");
    if (!box) return;
    box.textContent = message;
    box.className = `status-message${kind ? ` ${kind}` : ""}`;
  }

  function setRecordMessage(message = "", kind = "") {
    const box = $("#recordMessage");
    if (!box) return;
    box.textContent = message;
    box.className = `status-message${kind ? ` ${kind}` : ""}`;
  }

  function formatDate(value) {
    if (!value) return "";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "";
    return new Intl.DateTimeFormat(undefined, {
      month: "short",
      year: "numeric"
    }).format(date);
  }

  function inputDate(value) {
    if (!value) return "";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "";
    return date.toISOString().slice(0, 10);
  }

  function dateRange(start, end, current = false) {
    const first = formatDate(start);
    const last = current ? "Present" : formatDate(end);
    if (first && last) return `${first} – ${last}`;
    if (first) return `From ${first}`;
    if (last) return last;
    return "";
  }

  function emptyBlock(title, copy) {
    const wrapper = document.createElement("div");
    wrapper.className = "evidence-empty";
    wrapper.append(text("strong", title), text("p", copy));
    return wrapper;
  }

  function readiness() {
    const profile = state.profile;
    if (!profile) return 0;

    let score = 10;
    if (clean(profile.headline)) score += 10;
    if (clean(profile.summary).length >= 50) score += 10;
    if (clean(profile.city) && clean(profile.country)) score += 10;
    if ((profile.skills || []).length >= 3) score += 20;
    if ((profile.educations || []).length) score += 15;
    if (Number(profile.yearsOfExperience || 0) === 0 || (profile.experiences || []).length) score += 15;
    if ((profile.projects || []).length) score += 10;
    return Math.min(score, 100);
  }

  function renderReadiness() {
    const score = readiness();
    $("#profileReadinessValue").textContent = `${score}%`;
    $("#profileProgressFill").style.width = `${score}%`;
    const progress = $(".profile-progress");
    progress?.setAttribute("aria-valuenow", String(score));

    const label = score === 100
      ? "Your core matching evidence is complete. Keep it current."
      : score >= 70
        ? "Strong foundation. Add the remaining evidence for a complete profile."
        : score >= 40
          ? "Good start. Skills, education and project evidence will strengthen it."
          : "Complete your core details to improve matching evidence.";
    $("#profileReadinessLabel").textContent = label;
  }

  function setFormValue(id, value) {
    const input = document.getElementById(id);
    if (!input) return;
    if (input.type === "checkbox") {
      input.checked = Boolean(value);
    } else {
      input.value = value ?? "";
    }
  }

  function renderProfileForm() {
    const profile = state.profile;
    setFormValue("headline", profile?.headline);
    setFormValue("summary", profile?.summary);
    setFormValue("dateOfBirth", inputDate(profile?.dateOfBirth));
    setFormValue("gender", profile?.gender);
    setFormValue("address", profile?.address);
    setFormValue("city", profile?.city);
    setFormValue("country", profile?.country);
    setFormValue("yearsOfExperience", profile?.yearsOfExperience ?? 0);
    setFormValue("expectedSalaryMin", profile?.expectedSalaryMin);
    setFormValue("expectedSalaryMax", profile?.expectedSalaryMax);
    setFormValue("isProfilePublic", profile ? profile.isProfilePublic : true);
    setFormValue("isOpenToWork", profile ? profile.isOpenToWork : true);

    $("#saveProfile").textContent = profile ? "Save profile changes" : "Create profile";
    $("#profileModeBadge").textContent = profile ? "Profile active" : "Not created";
    $("#profileModeBadge").className = `badge${profile ? " ok" : ""}`;
    $("#profileModeCopy").textContent = profile
      ? "Keep these details accurate; matching and CV tools use this record."
      : "Create the profile required for match previews and applications.";

    const savedAt = profile?.updatedAt || profile?.createdAt;
    $("#profileSavedAt").textContent = savedAt
      ? `Last saved ${new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeStyle: "short" }).format(new Date(savedAt))}`
      : "";
  }

  function renderRecord(kind, item) {
    const config = sections[kind];
    const card = document.createElement("article");
    card.className = "evidence-record";
    card.dataset.recordId = item.id;

    const copy = document.createElement("div");
    copy.className = "evidence-record-copy";
    copy.append(
      text("h3", config.title(item)),
      text("p", config.subtitle(item))
    );

    const metadata = config.meta(item);
    if (metadata.length) {
      const meta = document.createElement("div");
      meta.className = "evidence-meta";
      metadata.forEach(value => meta.appendChild(text("span", value)));
      copy.appendChild(meta);
    }

    const actions = document.createElement("div");
    actions.className = "record-actions";

    const edit = document.createElement("button");
    edit.type = "button";
    edit.className = "record-action";
    edit.textContent = "Edit";
    edit.setAttribute("aria-label", `Edit ${config.title(item)}`);
    edit.addEventListener("click", () => openRecordDialog(kind, item));

    const remove = document.createElement("button");
    remove.type = "button";
    remove.className = "record-action danger";
    remove.textContent = "Delete";
    remove.setAttribute("aria-label", `Delete ${config.title(item)}`);
    remove.addEventListener("click", () => deleteRecord(kind, item, remove));

    actions.append(edit, remove);
    card.append(copy, actions);
    return card;
  }

  function renderSection(kind) {
    const config = sections[kind];
    const list = $(config.list);
    if (!list) return;
    list.replaceChildren();

    if (!state.profile) {
      list.appendChild(emptyBlock("Create your core profile first", `Save the professional summary before adding ${kind}.`));
      return;
    }

    const items = Array.isArray(state.profile[config.property])
      ? state.profile[config.property]
      : [];

    if (!items.length) {
      list.appendChild(emptyBlock(`No ${kind} added`, `Use “Add ${config.singular}” to create your first record.`));
      return;
    }

    items.forEach(item => list.appendChild(renderRecord(kind, item)));
  }

  function renderAll() {
    renderProfileForm();
    renderReadiness();
    Object.keys(sections).forEach(renderSection);
    document.querySelectorAll("[data-add-kind]").forEach(button => {
      button.disabled = !state.profile;
      button.title = state.profile ? "" : "Create your core profile first";
    });
  }

  async function loadProfile() {
    const requestVersion = ++state.profileRequestVersion;
    setBadge("Loading");

    try {
      const profile = await window.NexHireApi.request("/api/jobseekers/me");
      if (requestVersion !== state.profileRequestVersion) return;
      state.profile = profile;
      renderAll();
      setBadge("Live profile", "ok");
    } catch (error) {
      if (requestVersion !== state.profileRequestVersion) return;
      if (error.status === 404) {
        state.profile = null;
        renderAll();
        setBadge("Setup required");
        setMessage("Create your core profile first. Skills and other evidence will unlock after it is saved.");
        return;
      }
      setBadge("Could not load", "bad");
      setMessage(error.message || "Your profile could not be loaded.", "error");
    }
  }

  function profilePayload() {
    const dateOfBirth = $("#dateOfBirth").value || null;
    const expectedSalaryMin = $("#expectedSalaryMin").value === ""
      ? null
      : Number($("#expectedSalaryMin").value);
    const expectedSalaryMax = $("#expectedSalaryMax").value === ""
      ? null
      : Number($("#expectedSalaryMax").value);

    return {
      headline: clean($("#headline").value),
      summary: clean($("#summary").value),
      dateOfBirth,
      gender: clean($("#gender").value),
      address: clean($("#address").value),
      city: clean($("#city").value),
      country: clean($("#country").value),
      yearsOfExperience: Number($("#yearsOfExperience").value),
      expectedSalaryMin,
      expectedSalaryMax,
      isProfilePublic: $("#isProfilePublic").checked,
      isOpenToWork: $("#isOpenToWork").checked,
      clearDateOfBirth: Boolean(state.profile?.dateOfBirth) && dateOfBirth == null,
      clearExpectedSalaryMin: state.profile?.expectedSalaryMin != null && expectedSalaryMin == null,
      clearExpectedSalaryMax: state.profile?.expectedSalaryMax != null && expectedSalaryMax == null
    };
  }

  function validateProfilePayload(payload) {
    const dob = $("#dateOfBirth");
    dob.setCustomValidity("");
    if (payload.dateOfBirth && new Date(`${payload.dateOfBirth}T00:00:00Z`) > new Date()) {
      dob.setCustomValidity("Date of birth cannot be in the future.");
      dob.reportValidity();
      return false;
    }

    const maxSalary = $("#expectedSalaryMax");
    maxSalary.setCustomValidity("");
    if (
      payload.expectedSalaryMin != null &&
      payload.expectedSalaryMax != null &&
      payload.expectedSalaryMin > payload.expectedSalaryMax
    ) {
      maxSalary.setCustomValidity("Maximum expected salary must be greater than or equal to the minimum.");
      maxSalary.reportValidity();
      return false;
    }
    return true;
  }

  async function saveProfile(event) {
    event.preventDefault();
    const form = $("#profileForm");
    if (!form.reportValidity()) return;

    const payload = profilePayload();
    if (!validateProfilePayload(payload)) return;

    const button = $("#saveProfile");
    const creating = !state.profile;
    button.disabled = true;
    button.textContent = creating ? "Creating…" : "Saving…";
    setMessage();

    try {
      state.profile = await window.NexHireApi.request("/api/jobseekers/me", {
        method: creating ? "POST" : "PUT",
        body: payload
      });
      renderAll();
      setBadge("Live profile", "ok");
      setMessage(creating ? "Professional profile created." : "Professional profile updated.", "success");
    } catch (error) {
      setMessage(error.message || "The profile could not be saved.", "error");
    } finally {
      button.disabled = false;
      button.textContent = state.profile ? "Save profile changes" : "Create profile";
    }
  }

  function buildField(field, item) {
    const wrapper = document.createElement("label");
    wrapper.className = `record-field${field.wide ? " field-wide" : ""}${field.type === "checkbox" ? " record-check" : ""}`;

    let input;
    if (field.type === "textarea") {
      input = document.createElement("textarea");
      input.rows = field.rows || 4;
    } else if (field.type === "select") {
      input = document.createElement("select");
      (field.options || []).forEach(option => {
        const choice = document.createElement("option");
        choice.value = String(option.value);
        choice.textContent = option.label;
        choice.disabled = Boolean(option.disabled);
        input.appendChild(choice);
      });
    } else {
      input = document.createElement("input");
      input.type = field.type || "text";
    }

    input.name = field.name;
    input.id = `record-${field.name}`;
    if (field.required) input.required = true;
    if (field.maxLength) input.maxLength = field.maxLength;
    if (field.min != null) input.min = String(field.min);
    if (field.max != null) input.max = String(field.max);
    if (field.step != null) input.step = String(field.step);
    if (field.placeholder) input.placeholder = field.placeholder;

    const raw = item?.[field.name];
    if (field.type === "checkbox") {
      input.checked = Boolean(raw ?? field.value ?? false);
      wrapper.append(input, text("span", field.label));
    } else {
      const value = field.type === "date" ? inputDate(raw) : (raw ?? field.value ?? "");
      input.value = value;
      wrapper.append(text("span", field.label), input);
    }
    return wrapper;
  }

  function syncExperienceEndDate() {
    const current = $("#record-isCurrent");
    const end = $("#record-endDate");
    if (!current || !end) return;
    end.disabled = current.checked;
    if (current.checked) end.value = "";
  }

  function openRecordDialog(kind, item = null) {
    if (!state.profile) {
      setMessage("Create your core profile before adding professional evidence.", "error");
      $("#profileCoreTitle")?.scrollIntoView({ behavior: "smooth", block: "start" });
      return;
    }

    const config = sections[kind];
    if (!config) return;

    state.dialogKind = kind;
    state.dialogRecordId = item?.id || null;
    state.dialogBusy = false;
    setRecordMessage();

    $("#recordDialogEyebrow").textContent = item ? "Update professional evidence" : "Add professional evidence";
    $("#recordDialogTitle").textContent = `${item ? "Edit" : "Add"} ${config.singular}`;
    $("#saveRecord").textContent = item ? "Save changes" : `Add ${config.singular}`;

    const fields = $("#recordFields");
    fields.replaceChildren();
    config.fields.forEach(field => fields.appendChild(buildField(field, item)));

    if (kind === "experience") {
      $("#record-isCurrent")?.addEventListener("change", syncExperienceEndDate);
      syncExperienceEndDate();
    }

    const dialog = $("#recordDialog");
    dialog.showModal();
    fields.querySelector("input:not([type=checkbox]), select, textarea")?.focus();
  }

  function closeRecordDialog() {
    if (state.dialogBusy) return;
    $("#recordDialog")?.close();
    state.dialogKind = null;
    state.dialogRecordId = null;
    setRecordMessage();
  }

  function validateRecordDates(kind, form) {
    const pairs = {
      education: ["startDate", "endDate"],
      experience: ["startDate", "endDate"],
      projects: ["startDate", "endDate"],
      certifications: ["issueDate", "expiryDate"]
    };
    const pair = pairs[kind];
    if (!pair) return true;

    const [startName, endName] = pair;
    const start = form.elements[startName];
    const end = form.elements[endName];
    end?.setCustomValidity("");

    if (start?.value && end?.value && end.value < start.value) {
      end.setCustomValidity("End date cannot be earlier than the start date.");
      end.reportValidity();
      return false;
    }
    return true;
  }

  function setDialogBusy(busy) {
    state.dialogBusy = busy;
    $("#saveRecord").disabled = busy;
    $("#cancelRecord").disabled = busy;
    $("#closeRecordDialog").disabled = busy;
  }

  async function saveRecord(event) {
    event.preventDefault();
    const form = $("#recordForm");
    const kind = state.dialogKind;
    const config = sections[kind];
    if (!config || !form.reportValidity() || !validateRecordDates(kind, form)) return;

    const editingId = state.dialogRecordId;
    const payload = config.payload(form.elements);
    setDialogBusy(true);
    setRecordMessage();
    $("#saveRecord").textContent = editingId ? "Saving…" : "Adding…";

    try {
      const result = await window.NexHireApi.request(
        `/api/jobseekers/me/${config.endpoint}${editingId ? `/${editingId}` : ""}`,
        { method: editingId ? "PUT" : "POST", body: payload }
      );

      const items = Array.isArray(state.profile[config.property])
        ? state.profile[config.property]
        : (state.profile[config.property] = []);
      const index = items.findIndex(item => String(item.id) === String(editingId));
      if (index >= 0) items[index] = result;
      else items.push(result);

      setDialogBusy(false);
      $("#recordDialog").close();
      state.dialogKind = null;
      state.dialogRecordId = null;
      renderAll();
      setMessage(`${config.singular[0].toUpperCase()}${config.singular.slice(1)} ${editingId ? "updated" : "added"}.`, "success");
    } catch (error) {
      setDialogBusy(false);
      $("#saveRecord").textContent = editingId ? "Save changes" : `Add ${config.singular}`;
      setRecordMessage(error.message || `The ${config.singular} could not be saved.`, "error");
    }
  }

  async function deleteRecord(kind, item, button) {
    const config = sections[kind];
    if (!config || !window.confirm(`Delete “${config.title(item)}”? This cannot be undone.`)) return;

    button.disabled = true;
    setMessage();
    try {
      await window.NexHireApi.request(`/api/jobseekers/me/${config.endpoint}/${item.id}`, {
        method: "DELETE"
      });
      state.profile[config.property] = (state.profile[config.property] || [])
        .filter(record => String(record.id) !== String(item.id));
      renderAll();
      setMessage(`${config.singular[0].toUpperCase()}${config.singular.slice(1)} deleted.`, "success");
    } catch (error) {
      button.disabled = false;
      setMessage(error.message || `The ${config.singular} could not be deleted.`, "error");
    }
  }

  function bindEvents() {
    $("#profileForm")?.addEventListener("submit", saveProfile);
    document.querySelectorAll("[data-add-kind]").forEach(button => {
      button.addEventListener("click", () => openRecordDialog(button.dataset.addKind));
    });
    $("#recordForm")?.addEventListener("submit", saveRecord);
    $("#cancelRecord")?.addEventListener("click", closeRecordDialog);
    $("#closeRecordDialog")?.addEventListener("click", closeRecordDialog);
    $("#recordDialog")?.addEventListener("cancel", event => {
      if (state.dialogBusy) event.preventDefault();
    });
    $("#recordDialog")?.addEventListener("close", () => {
      if (state.dialogBusy) return;
      state.dialogKind = null;
      state.dialogRecordId = null;
      setRecordMessage();
    });
  }

  async function init() {
    if (!setupShell()) return;
    bindEvents();
    await loadProfile();
  }

  document.addEventListener("DOMContentLoaded", init);
})();

(() => {
  const page = document.body.dataset.page || "";
  const $ = selector => document.querySelector(selector);

  const state = {
    profile: null,
    jobs: [],
    applications: [],
    selectedJobId: null,
    selectedApplicationId: null,
    jobsRequestVersion: 0,
    jobSelectionVersion: 0,
    applicationSelectionVersion: 0
  };

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

    menu?.addEventListener("click", () => {
      document.body.classList.toggle("menu-open");
    });

    scrim?.addEventListener("click", () => {
      document.body.classList.remove("menu-open");
    });

    window.addEventListener("keydown", event => {
      if (event.key === "Escape") document.body.classList.remove("menu-open");
    });

    return true;
  }

  function text(tag, value, className = "") {
    const node = document.createElement(tag);
    if (className) node.className = className;
    node.textContent = value ?? "";
    return node;
  }

  function formatDate(value, fallback = "Not specified") {
    if (!value) return fallback;
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return fallback;

    return new Intl.DateTimeFormat(undefined, {
      day: "2-digit",
      month: "short",
      year: "numeric"
    }).format(date);
  }

  function formatMoney(min, max, currency) {
    if (min == null && max == null) return "Salary not disclosed";

    const code = currency || "LKR";
    const format = value => {
      try {
        return new Intl.NumberFormat(undefined, {
          style: "currency",
          currency: code,
          maximumFractionDigits: 0
        }).format(value);
      } catch {
        return `${code} ${Number(value).toLocaleString()}`;
      }
    };

    if (min != null && max != null) return `${format(min)} – ${format(max)}`;
    if (min != null) return `From ${format(min)}`;
    return `Up to ${format(max)}`;
  }

  function locationLabel(job) {
    if (job.isRemote) return "Remote";

    const place = [job.locationCity, job.locationCountry]
      .filter(Boolean)
      .join(", ");

    if (job.isHybrid && place) return `${place} · Hybrid`;
    if (job.isHybrid) return "Hybrid";
    return place || "Location flexible";
  }

  function setMessage(selector, message = "", type = "") {
    const box = $(selector);
    if (!box) return;

    box.textContent = message;
    box.className = `status-message${type ? ` ${type}` : ""}`;
  }

  function setBadge(selector, label, kind = "") {
    const badge = $(selector);
    if (!badge) return;

    badge.textContent = label;
    badge.className = `badge${kind ? ` ${kind}` : ""}`;
  }

  function statusChip(status) {
    const chip = text("span", status || "Unknown", "status-chip");
    chip.dataset.status = String(status || "").toLowerCase();
    return chip;
  }

  function metaPill(value) {
    return text("span", value, "job-meta-pill");
  }

  function emptyBlock(title, copy) {
    const wrapper = document.createElement("div");
    wrapper.className = "list-empty";

    const inner = document.createElement("div");
    inner.append(
      text("strong", title),
      text("p", copy)
    );

    wrapper.appendChild(inner);
    return wrapper;
  }

  async function loadProfile() {
    try {
      state.profile = await window.NexHireApi.request("/api/jobseekers/me");
    } catch (error) {
      if (error.status === 404) {
        state.profile = null;
        return;
      }

      throw error;
    }
  }

  async function loadMyApplications() {
    const data = await window.NexHireApi.request("/api/applications/mine");
    state.applications = Array.isArray(data) ? data : [];
    return state.applications;
  }

  function appliedJobIds() {
    return new Set(
      state.applications
        .map(item => item.jobId)
        .filter(Boolean)
        .map(String)
    );
  }

  function recordAppliedJob(job, application) {
    if (appliedJobIds().has(String(job.id))) return;

    const response = application && typeof application === "object"
      ? application
      : {};

    state.applications.unshift({
      ...response,
      applicationId: response.applicationId || response.id || `pending-${job.id}`,
      jobId: response.jobId || job.id,
      jobTitle: response.jobTitle || job.title,
      companyName: response.companyName || job.companyName,
      status: response.status || "Submitted",
      appliedAtUtc: response.appliedAtUtc || new Date().toISOString()
    });
  }

  function buildSearchUrl() {
    const params = new URLSearchParams();

    const query = $("#query")?.value.trim();
    const location = $("#location")?.value.trim();
    const skill = $("#skill")?.value.trim();
    const experience = $("#experienceYears")?.value.trim();
    const remote = $("#isRemote")?.checked;

    if (query) params.set("query", query);
    if (location) params.set("location", location);
    if (skill) params.set("skill", skill);
    if (experience) params.set("experienceYears", experience);
    if (remote) params.set("isRemote", "true");

    params.set("page", "1");
    params.set("pageSize", "50");

    return `/api/jobs?${params.toString()}`;
  }

  async function loadJobs({ selectFirst = true } = {}) {
    const requestVersion = ++state.jobsRequestVersion;
    ++state.jobSelectionVersion;
    setBadge("#jobsState", "Searching");

    try {
      const data = await window.NexHireApi.request(buildSearchUrl());
      if (requestVersion !== state.jobsRequestVersion) return;

      state.jobs = Array.isArray(data) ? data : [];

      renderJobList();

      if (selectFirst && state.jobs.length) {
        await selectJob(state.jobs[0].id);
      } else if (!state.jobs.length) {
        state.selectedJobId = null;
        renderJobDetailPlaceholder(
          "No open roles match these filters.",
          "Try a broader keyword, remove a filter, or search another location."
        );
      }

      if (requestVersion !== state.jobsRequestVersion) return;
      setBadge("#jobsState", "Live results", "ok");
    } catch (error) {
      if (requestVersion !== state.jobsRequestVersion) return;
      setBadge("#jobsState", "Could not load", "bad");
      setMessage("#jobsMessage", error.message, "error");
    }
  }

  function renderJobList() {
    const list = $("#jobList");
    const count = $("#resultCount");
    if (!list || !count) return;

    count.textContent = `${state.jobs.length} open ${state.jobs.length === 1 ? "role" : "roles"}`;
    list.replaceChildren();

    if (!state.jobs.length) {
      list.appendChild(
        emptyBlock(
          "No matching vacancies",
          "Change one or two filters and try again."
        )
      );
      return;
    }

    state.jobs.forEach(job => {
      const button = document.createElement("button");
      button.type = "button";
      button.className = "job-result";
      button.dataset.jobId = job.id;

      const isSelected = String(job.id) === String(state.selectedJobId);
      button.setAttribute("aria-pressed", isSelected ? "true" : "false");

      if (isSelected) {
        button.classList.add("active");
      }

      const top = document.createElement("div");
      top.className = "job-result-top";

      const titleBlock = document.createElement("div");
      titleBlock.append(
        text("h3", job.title || "Untitled role"),
        text("div", job.companyName || "Company", "job-result-company")
      );

      top.appendChild(titleBlock);

      const meta = document.createElement("div");
      meta.className = "job-result-meta";
      meta.append(
        metaPill(locationLabel(job)),
        metaPill(job.employmentType || "Employment type not specified")
      );

      if (job.experienceMinYears != null) {
        meta.appendChild(metaPill(`${job.experienceMinYears}+ yrs`));
      }

      const foot = document.createElement("div");
      foot.className = "job-result-foot";
      foot.append(
        text("span", formatMoney(job.salaryMin, job.salaryMax, job.currency)),
        text("span", job.closingDate ? `Closes ${formatDate(job.closingDate)}` : "Open until filled")
      );

      button.append(top, meta, foot);

      button.addEventListener("click", () => selectJob(job.id));

      list.appendChild(button);
    });
  }

  function renderJobDetailPlaceholder(title, copy) {
    const detail = $("#jobDetail");
    if (!detail) return;

    detail.replaceChildren();

    const placeholder = document.createElement("div");
    placeholder.className = "job-detail-placeholder";
    placeholder.append(
      text("span", "NH", "placeholder-mark"),
      text("strong", title),
      text("p", copy)
    );

    detail.appendChild(placeholder);
  }

  async function selectJob(jobId) {
    const selectedId = String(jobId);
    const selectionVersion = ++state.jobSelectionVersion;
    state.selectedJobId = selectedId;
    setMessage("#jobsMessage");
    renderJobList();

    renderJobDetailPlaceholder(
      "Reviewing this opportunity…",
      "Loading the vacancy and your match evidence."
    );

    try {
      const [jobResult, matchResult] = await Promise.allSettled([
        window.NexHireApi.request(`/api/jobs/${jobId}`),
        window.NexHireApi.request(`/api/jobseekers/me/matches/${jobId}`)
      ]);

      if (
        selectionVersion !== state.jobSelectionVersion ||
        selectedId !== String(state.selectedJobId)
      ) return;

      if (jobResult.status !== "fulfilled") {
        throw jobResult.reason;
      }

      const match = matchResult.status === "fulfilled"
        ? matchResult.value
        : null;

      renderJobDetail(jobResult.value, match);

      if (
        matchResult.status === "rejected" &&
        matchResult.reason?.status !== 404
      ) {
        setMessage(
          "#jobsMessage",
          `Match preview could not be loaded: ${matchResult.reason?.message || "Unknown error"}`,
          "error"
        );
      }
    } catch (error) {
      if (
        selectionVersion !== state.jobSelectionVersion ||
        selectedId !== String(state.selectedJobId)
      ) return;

      renderJobDetailPlaceholder(
        "This role could not be opened.",
        error.message || "Refresh the page and try again."
      );
    }
  }

  function renderSkills(items, required) {
    const cloud = document.createElement("div");
    cloud.className = "skill-cloud";

    const safeItems = Array.isArray(items) ? items : [];

    if (!safeItems.length) {
      cloud.appendChild(text("span", "None specified", "muted"));
      return cloud;
    }

    safeItems.forEach(item => {
      const name = item.skillName || item.name || "Skill";
      const level = item.minimumProficiencyLevel;
      const label = level ? `${name} · level ${level}` : name;
      cloud.appendChild(
        text("span", label, `skill-tag${required ? " required" : ""}`)
      );
    });

    return cloud;
  }

  function renderMatch(match) {
    const wrapper = document.createDocumentFragment();

    const strip = document.createElement("section");
    strip.className = "match-strip";

    const score = document.createElement("div");
    score.className = "match-score";
    score.append(
      text("strong", `${Math.round(Number(match.totalScore || 0))}%`),
      text("span", "Match")
    );

    const copy = document.createElement("div");
    copy.className = "match-copy";
    copy.append(
      text("h3", match.recommendation || "Match preview"),
      text("p", match.summary || "Your profile has been compared with this vacancy."),
      text(
        "p",
        "This eligibility result is matching guidance based on mandatory requirements; it does not block you from applying.",
        "match-guidance"
      )
    );

    const labels = document.createElement("div");
    labels.className = "match-labels";
    labels.append(
      text(
        "span",
        match.isEligible ? "Eligible" : "Requirements gap",
        `badge ${match.isEligible ? "ok" : "bad"}`
      )
    );
    copy.appendChild(labels);

    strip.append(score, copy);
    wrapper.appendChild(strip);

    const details = Array.isArray(match.scoreDetails)
      ? match.scoreDetails
      : [];

    if (details.length) {
      const section = document.createElement("section");
      section.className = "match-details";
      section.appendChild(text("h3", "Why this score"));

      details.forEach(item => {
        const row = document.createElement("div");
        row.className = "score-detail";

        const raw = Math.max(0, Math.min(100, Number(item.rawScore || 0)));
        const track = document.createElement("div");
        track.className = "score-track";

        const fill = document.createElement("div");
        fill.className = "score-fill";
        fill.style.width = `${raw}%`;
        track.appendChild(fill);

        row.append(
          text("div", item.category || "Factor", "score-name"),
          track,
          text("div", `${raw.toFixed(0)}%`, "score-number")
        );

        if (item.explanation) {
          row.appendChild(
            text("div", item.explanation, "score-explanation")
          );
        }

        section.appendChild(row);
      });

      wrapper.appendChild(section);
    }

    return wrapper;
  }

  function detailSection(title, content) {
    const section = document.createElement("section");
    section.className = "detail-section";
    section.appendChild(text("h3", title));

    if (content instanceof Node) {
      section.appendChild(content);
    } else {
      section.appendChild(text("p", content || "Not specified"));
    }

    return section;
  }

  function ensureViewApplicationsLink(actions) {
    if (!actions || actions.querySelector("[data-view-applications]")) return;

    const link = document.createElement("a");
    link.href = "/app/jobseeker/applications.html";
    link.className = "text-link";
    link.dataset.viewApplications = "true";
    link.textContent = "View applications";
    actions.appendChild(link);
  }

  function renderJobDetail(job, match) {
    const detail = $("#jobDetail");
    if (!detail) return;

    detail.replaceChildren();

    const head = document.createElement("header");
    head.className = "detail-head";

    head.append(
      text("div", job.companyName || "Company", "detail-company"),
      text("h2", job.title || "Untitled role")
    );

    const meta = document.createElement("div");
    meta.className = "detail-meta";
    meta.append(
      metaPill(locationLabel(job)),
      metaPill(job.employmentType || "Employment type not specified"),
      metaPill(`${job.vacancyCount || 1} ${job.vacancyCount === 1 ? "opening" : "openings"}`),
      metaPill(formatMoney(job.salaryMin, job.salaryMax, job.currency))
    );
    head.appendChild(meta);

    const actions = document.createElement("div");
    actions.className = "detail-actions";

    const apply = document.createElement("button");
    apply.type = "button";
    apply.className = "btn btn-primary";

    const alreadyApplied = appliedJobIds().has(String(job.id));

    if (alreadyApplied) {
      apply.textContent = "Application submitted";
      apply.disabled = true;
    } else if (!state.profile || !state.profile.id) {
      apply.textContent = "Profile required before applying";
      apply.disabled = true;
    } else {
      apply.textContent = "Apply for this role";
      apply.addEventListener("click", () => applyToJob(job, apply));
    }

    actions.appendChild(apply);

    if (!state.profile || !state.profile.id) {
      actions.appendChild(
        text(
          "span",
          "Create your professional profile before applying or viewing a match.",
          "apply-note"
        )
      );
      const profileLink = document.createElement("a");
      profileLink.href = "/app/jobseeker/profile.html";
      profileLink.className = "text-link";
      profileLink.textContent = "Create profile";
      actions.appendChild(profileLink);
    } else if (alreadyApplied) {
      ensureViewApplicationsLink(actions);
    }

    head.appendChild(actions);
    detail.appendChild(head);

    if (match) {
      detail.appendChild(renderMatch(match));
    } else {
      const noMatch = document.createElement("section");
      noMatch.className = "match-strip";
      noMatch.append(
        text("span", "—", "match-score"),
        (() => {
          const copy = document.createElement("div");
          copy.className = "match-copy";
          copy.append(
            text("h3", "Match preview unavailable"),
            text(
              "p",
              state.profile
                ? "A match could not be calculated for this vacancy right now."
                : "Create your JobSeeker profile to unlock transparent match scoring."
            )
          );
          return copy;
        })()
      );
      detail.appendChild(noMatch);
    }

    const body = document.createElement("div");
    body.className = "detail-body";

    body.append(
      detailSection("About the role", job.description),
      detailSection("Responsibilities", job.responsibilities),
      detailSection("Required skills", renderSkills(job.requiredSkills, true)),
      detailSection("Preferred skills", renderSkills(job.preferredSkills, false)),
      detailSection(
        "Experience",
        `${job.experienceMinYears ?? 0}–${job.experienceMaxYears ?? job.experienceMinYears ?? 0} years`
      ),
      detailSection(
        "Education",
        job.educationRequirement || "No specific education requirement stated."
      ),
      detailSection(
        "Certifications",
        job.requiredCertifications || "No specific certification requirement stated."
      ),
      detailSection(
        "Closing date",
        job.closingDate ? formatDate(job.closingDate) : "Open until filled"
      )
    );

    detail.appendChild(body);
  }

  async function applyToJob(job, button) {
    if (!state.profile || !state.profile.id) return;

    const applyingJobId = String(job.id);
    const actions = button.parentElement;
    const original = button.textContent;
    button.disabled = true;
    button.textContent = "Submitting…";
    setMessage("#jobsMessage");

    try {
      const application = await window.NexHireApi.request("/api/applications", {
        method: "POST",
        body: { jobId: job.id }
      });

      recordAppliedJob(job, application);
      loadMyApplications().catch(() => {});

      if (String(state.selectedJobId) !== applyingJobId) return;

      button.textContent = "Application submitted";
      ensureViewApplicationsLink(actions);
      setMessage(
        "#jobsMessage",
        `Application sent to ${job.companyName || "the employer"}.`,
        "success"
      );
    } catch (error) {
      if (error.status === 409) {
        recordAppliedJob(job);
        loadMyApplications().catch(() => {});

        if (String(state.selectedJobId) !== applyingJobId) return;

        button.textContent = "Application already exists";
        ensureViewApplicationsLink(actions);
        setMessage(
          "#jobsMessage",
          error.message || "You already applied for this role.",
          "success"
        );
        return;
      }

      if (String(state.selectedJobId) !== applyingJobId) return;

      button.disabled = false;
      button.textContent = original;
      setMessage("#jobsMessage", error.message, "error");
    }
  }

  function populateStatusFilter() {
    const select = $("#statusFilter");
    if (!select) return;

    const current = select.value;
    const statuses = [...new Set(
      state.applications
        .map(item => item.status)
        .filter(Boolean)
    )].sort((a, b) => a.localeCompare(b));

    select.replaceChildren();

    const all = document.createElement("option");
    all.value = "";
    all.textContent = "All statuses";
    select.appendChild(all);

    statuses.forEach(status => {
      const option = document.createElement("option");
      option.value = status;
      option.textContent = status;
      select.appendChild(option);
    });

    select.value = statuses.includes(current) ? current : "";
  }

  function filteredApplications() {
    const filter = $("#statusFilter")?.value || "";
    return state.applications.filter(item => !filter || item.status === filter);
  }

  function renderApplicationList() {
    const list = $("#applicationList");
    const count = $("#applicationCount");
    if (!list || !count) return [];

    const filter = $("#statusFilter")?.value || "";
    const items = filteredApplications();

    count.textContent = `${items.length} ${items.length === 1 ? "application" : "applications"}`;
    list.replaceChildren();

    if (!items.length) {
      list.appendChild(
        emptyBlock(
          filter ? "No applications with this status" : "No applications yet",
          filter
            ? "Choose another status to review your applications."
            : "Find an open role and submit your first application."
        )
      );
      return items;
    }

    items.forEach(item => {
      const button = document.createElement("button");
      button.type = "button";
      button.className = "application-row";
      button.dataset.applicationId = item.applicationId;

      const isSelected = String(item.applicationId) === String(state.selectedApplicationId);
      button.setAttribute("aria-pressed", isSelected ? "true" : "false");

      if (isSelected) {
        button.classList.add("active");
      }

      button.append(
        text("h3", item.jobTitle || "Untitled role"),
        text("div", item.companyName || "Company", "application-row-company")
      );

      const foot = document.createElement("div");
      foot.className = "application-row-foot";
      foot.append(
        statusChip(item.status),
        text("span", formatDate(item.appliedAtUtc), "application-row-date")
      );

      button.appendChild(foot);
      button.addEventListener("click", () => selectApplication(item.applicationId));

      list.appendChild(button);
    });

    return items;
  }

  async function selectApplication(applicationId) {
    const selectedId = String(applicationId);
    const selectionVersion = ++state.applicationSelectionVersion;
    state.selectedApplicationId = selectedId;
    renderApplicationList();

    const detail = $("#applicationDetail");
    if (!detail) return;

    detail.replaceChildren(
      (() => {
        const block = document.createElement("div");
        block.className = "job-detail-placeholder";
        block.append(
          text("span", "NH", "placeholder-mark"),
          text("strong", "Opening application…"),
          text("p", "Loading the latest status from NexHire.")
        );
        return block;
      })()
    );

    try {
      const item = await window.NexHireApi.request(
        `/api/applications/${applicationId}`
      );

      if (
        selectionVersion !== state.applicationSelectionVersion ||
        selectedId !== String(state.selectedApplicationId)
      ) return;

      renderApplicationDetail(item);
    } catch (error) {
      if (
        selectionVersion !== state.applicationSelectionVersion ||
        selectedId !== String(state.selectedApplicationId)
      ) return;

      detail.replaceChildren(
        emptyBlock(
          "Application could not be opened",
          error.message || "Refresh and try again."
        )
      );
    }
  }

  function renderApplicationDetail(item) {
    const detail = $("#applicationDetail");
    if (!detail) return;

    detail.replaceChildren();

    const head = document.createElement("header");
    head.className = "application-detail-head";
    head.append(
      text("div", item.companyName || "Company", "detail-company"),
      text("h2", item.jobTitle || "Untitled role")
    );

    const meta = document.createElement("div");
    meta.className = "detail-meta";
    meta.append(statusChip(item.status));
    head.appendChild(meta);

    const body = document.createElement("div");
    body.className = "application-detail-body";

    const facts = document.createElement("div");
    facts.className = "application-facts";

    [
      ["Applied", formatDate(item.appliedAtUtc)],
      ["Last updated", formatDate(item.updatedAtUtc, "No updates yet")],
      ["Application ref", String(item.applicationId || "").toUpperCase()],
      ["Company ref", String(item.companyId || "").toUpperCase()]
    ].forEach(([label, value]) => {
      const card = document.createElement("div");
      card.className = "application-fact";
      card.append(
        text("span", label),
        text("strong", value)
      );
      facts.appendChild(card);
    });

    const statusBlock = document.createElement("div");
    statusBlock.className = "application-status-block";
    statusBlock.append(
      text("strong", `Current status: ${item.status || "Unknown"}`),
      text(
        "p",
        "This status comes directly from your NexHire application record. New employer decisions will appear here when the application is updated."
      )
    );

    body.append(facts, statusBlock);
    detail.append(head, body);
  }

  async function initJobs() {
    if (!setupShell()) return;

    setBadge("#jobsState", "Loading");

    try {
      await Promise.all([
        loadProfile(),
        loadMyApplications()
      ]);

      $("#jobSearchForm")?.addEventListener("submit", event => {
        event.preventDefault();
        setMessage("#jobsMessage");
        loadJobs();
      });

      $("#clearFilters")?.addEventListener("click", () => {
        ["query", "location", "skill", "experienceYears"].forEach(id => {
          const input = document.getElementById(id);
          if (input) input.value = "";
        });

        const remote = $("#isRemote");
        if (remote) remote.checked = false;

        setMessage("#jobsMessage");
        loadJobs();
      });

      await loadJobs();
    } catch (error) {
      setBadge("#jobsState", "Could not load", "bad");
      setMessage("#jobsMessage", error.message, "error");
    }
  }

  async function initApplications() {
    if (!setupShell()) return;

    setBadge("#applicationsState", "Loading");

    try {
      await loadMyApplications();

      state.applications.sort((a, b) => {
        return new Date(b.appliedAtUtc || 0) - new Date(a.appliedAtUtc || 0);
      });

      populateStatusFilter();
      renderApplicationList();

      $("#statusFilter")?.addEventListener("change", async () => {
        ++state.applicationSelectionVersion;
        state.selectedApplicationId = null;
        const items = renderApplicationList();
        const detail = $("#applicationDetail");

        if (items.length) {
          await selectApplication(items[0].applicationId);
        } else if (detail) {
          detail.replaceChildren(
            emptyBlock(
              "No application selected",
              "Choose another status to review an application."
            )
          );
        }
      });

      if (state.applications.length) {
        await selectApplication(state.applications[0].applicationId);
      }

      setBadge("#applicationsState", "Live data", "ok");
    } catch (error) {
      setBadge("#applicationsState", "Could not load", "bad");
      setMessage("#applicationsMessage", error.message, "error");
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    if (page === "jobs") initJobs();
    if (page === "applications") initApplications();
  });
})();

const API = "/api/Admin/users",
  $ = (x) => document.getElementById(x),
  token = localStorage.accessToken;
let all = [];
async function api(path = "", o = {}) {
  const r = await fetch(API + path, {
    ...o,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
      ...o.headers,
    },
  });
  if (r.status === 204) return;
  const d = await r.json().catch(() => ({}));
  if (!r.ok) throw Error(d.message || `Request failed (${r.status})`);
  return d;
}
function render() {
  const q = $("search").value.toLowerCase(),
    f = $("filter").value;
  const m = all.filter(
    (x) =>
      (!f || x.role === f) &&
      `${x.fullName} ${x.email}`.toLowerCase().includes(q),
  );
  $("rows").innerHTML = m
    .map(
      (x) =>
        `<tr><td><b>${x.fullName}</b><br><small>${x.email}</small></td><td><span class="badge">${x.role}</span></td><td>${x.status}</td><td>${new Date(x.createdAtUtc).toLocaleDateString()}</td><td class="actions"><button onclick="editMember('${x.id}')">Edit</button><button onclick="setStatus('${x.id}','${x.status === "Active" ? "Suspended" : "Active"}')">${x.status === "Active" ? "Suspend" : "Activate"}</button><button onclick="disableMember('${x.id}')">Disable</button></td></tr>`,
    )
    .join("");
  $("total").textContent = all.length;
  $("active").textContent = all.filter((x) => x.status === "Active").length;
  $("admins").textContent = all.filter((x) => x.role === "Admin").length;
}
async function load() {
  try {
    all = await api();
    render();
  } catch (e) {
    $("message").textContent =
      e.message +
      (e.message.includes("401") || e.message.includes("403")
        ? " — Admin login required."
        : "");
  }
}
$("search").oninput = render;
$("filter").onchange = render;
$("openCreate").onclick = () => {
  form.reset();
  id.value = "";
  email.disabled = false;
  passwordFields.hidden = false;
  title.textContent = "Create member";
  dialog.showModal();
};
$("cancel").onclick = () => dialog.close();
form.onsubmit = async (e) => {
  e.preventDefault();
  const key = id.value,
    body = {
      firstName: firstName.value,
      lastName: lastName.value,
      phoneNumber: phone.value || null,
      role: role.value,
      ...(!key
        ? {
            email: email.value,
            password: password.value,
            confirmPassword: confirm.value,
          }
        : {}),
    };
  try {
    await api(key ? `/${key}` : "", {
      method: key ? "PUT" : "POST",
      body: JSON.stringify(body),
    });
    dialog.close();
    await load();
  } catch (e) {
    $("message").textContent = e.message;
  }
};
window.editMember = (key) => {
  const x = all.find((v) => v.id === key);
  id.value = key;
  firstName.value = x.firstName;
  lastName.value = x.lastName;
  email.value = x.email;
  email.disabled = true;
  phone.value = x.phoneNumber || "";
  role.value = x.role;
  passwordFields.hidden = true;
  title.textContent = "Edit member";
  dialog.showModal();
};
window.setStatus = async (key, status) => {
  try {
    await api(`/${key}/status`, {
      method: "PATCH",
      body: JSON.stringify({ status }),
    });
    await load();
  } catch (e) {
    $("message").textContent = e.message;
  }
};
window.disableMember = async (key) => {
  if (confirm("Disable this member and revoke all sessions?"))
    try {
      await api(`/${key}`, { method: "DELETE" });
      await load();
    } catch (e) {
      $("message").textContent = e.message;
    }
};
if (!token) $("message").textContent = "Admin JWT missing. Sign in first.";
load();

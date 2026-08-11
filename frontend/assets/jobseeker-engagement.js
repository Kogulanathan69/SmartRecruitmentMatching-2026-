(()=>{
  "use strict";
  const $=(s)=>document.querySelector(s);
  const page=document.body.dataset.page||"";
  const api=window.NexHireApi;
  const session=window.NexHireSession;

  const normalize=(value)=>Array.isArray(value)?value:(value==null?[]:[value]);
  const fmtDate=(value)=>{
    if(!value)return "—";
    const d=new Date(value);
    return Number.isNaN(d.getTime())?"—":new Intl.DateTimeFormat(undefined,{day:"2-digit",month:"short",year:"numeric",hour:"2-digit",minute:"2-digit"}).format(d);
  };
  const setText=(selector,value)=>{const el=$(selector);if(el)el.textContent=String(value??"");};
  const setState=(label,kind="")=>{const el=$("#engagementState");if(!el)return;el.textContent=label;el.className=`badge ${kind}`.trim();};
  const message=(text,kind="")=>{const el=$("#engagementMessage");if(!el)return;el.textContent=text||"";el.className=text?`status-message ${kind}`.trim():"status-message";};

  function setupShell(){
    if(!session?.requireRole("JobSeeker"))return false;
    const name=session.fullName()||"Job seeker";
    document.querySelectorAll("[data-user-name]").forEach(el=>el.textContent=name);
    document.querySelectorAll("[data-user-role]").forEach(el=>el.textContent="Job seeker");
    document.querySelectorAll("[data-user-initials]").forEach(el=>el.textContent=session.initials(name));
    $("[data-logout]")?.addEventListener("click",session.logout);
    $("[data-menu]")?.addEventListener("click",()=>document.body.classList.toggle("menu-open"));
    $("[data-scrim]")?.addEventListener("click",()=>document.body.classList.remove("menu-open"));
    return true;
  }

  function emptyState(title,text){
    const box=document.createElement("div");box.className="engagement-empty";
    const strong=document.createElement("strong");strong.textContent=title;
    const span=document.createElement("span");span.textContent=text;
    box.append(strong,span);return box;
  }

  function statusChip(status){
    const chip=document.createElement("span");chip.className="status-chip";chip.dataset.status=String(status||"").toLowerCase();chip.textContent=status||"Unknown";return chip;
  }

  async function decideContact(id,decision,button){
    button.disabled=true;
    message(`${decision}ing contact request…`);
    try{
      await api.request(`/api/v1/candidate/contact-requests/${encodeURIComponent(id)}/decision`,{method:"POST",body:{decision}});
      message(decision==="Accept"?"Contact request accepted.":"Contact request declined.","success");
      await loadContacts(false);
    }catch(error){message(error.message,"error");button.disabled=false;}
  }

  function renderContact(request){
    const card=document.createElement("article");card.className="request-card";
    const head=document.createElement("div");head.className="request-card-head";
    const copy=document.createElement("div");
    const company=document.createElement("span");company.className="request-company";company.textContent=request.companyName||"Employer";
    const title=document.createElement("h3");title.textContent=request.jobTitle||"Job opportunity";
    const meta=document.createElement("p");meta.textContent=`Requested ${fmtDate(request.createdAtUtc)}`;
    copy.append(company,title,meta);head.append(copy,statusChip(request.status));
    const body=document.createElement("p");body.className="request-message";body.textContent=request.message||"The employer would like permission to contact you.";
    card.append(head,body);
    if(String(request.status||"").toLowerCase()==="pending"){
      const actions=document.createElement("div");actions.className="request-actions";
      const accept=document.createElement("button");accept.type="button";accept.className="engagement-primary";accept.textContent="Accept";
      const decline=document.createElement("button");decline.type="button";decline.className="engagement-danger";decline.textContent="Decline";
      accept.addEventListener("click",()=>decideContact(request.id,"Accept",accept));
      decline.addEventListener("click",()=>decideContact(request.id,"Decline",decline));
      actions.append(accept,decline);card.append(actions);
    }else if(request.decisionAtUtc){
      const decision=document.createElement("div");decision.className="request-decision";decision.textContent=`Decision recorded ${fmtDate(request.decisionAtUtc)}`;card.append(decision);
    }
    return card;
  }

  async function loadContacts(showLoading=true){
    const list=$("#contactList");if(!list)return;
    if(showLoading){setState("Loading");list.replaceChildren(emptyState("Loading requests","Please wait while NexHire checks your contact permissions."));}
    try{
      const rows=normalize(await api.request("/api/v1/candidate/contact-requests"));
      const counts={pending:0,accepted:0,declined:0};
      rows.forEach(row=>{const key=String(row.status||"").toLowerCase();if(key in counts)counts[key]+=1;});
      setText("#contactPending",counts.pending);setText("#contactAccepted",counts.accepted);setText("#contactDeclined",counts.declined);setText("#contactTotal",rows.length);
      list.replaceChildren();
      if(!rows.length)list.append(emptyState("No contact requests yet","Shortlisted employers can request permission to contact you."));
      else rows.forEach(row=>list.append(renderContact(row)));
      setState("Live data","ok");
    }catch(error){setState("Could not load","bad");list.replaceChildren(emptyState("Could not load contact requests",error.message));message(error.message,"error");}
  }

  function notificationIcon(type){
    const icon=document.createElement("span");icon.className="notification-icon";
    const t=String(type||"").toLowerCase();icon.textContent=t.includes("contact")?"↔":t.includes("application")?"✓":"•";return icon;
  }

  async function markNotificationRead(notification,button){
    button.disabled=true;
    try{await api.request(`/api/notifications/${encodeURIComponent(notification.id)}/read`,{method:"PATCH"});await loadNotifications(false);}catch(error){message(error.message,"error");button.disabled=false;}
  }

  function renderNotification(item){
    const row=document.createElement("article");row.className=`notification-item${item.isRead?"":" unread"}`;
    const icon=notificationIcon(item.type);const copy=document.createElement("div");copy.className="notification-copy";
    const top=document.createElement("div");top.className="notification-top";const title=document.createElement("h3");title.textContent=item.title||"Notification";const time=document.createElement("time");time.textContent=fmtDate(item.createdAtUtc);top.append(title,time);
    const text=document.createElement("p");text.textContent=item.message||"";const type=document.createElement("span");type.className="notification-type";type.textContent=item.type||"Update";copy.append(top,text,type);row.append(icon,copy);
    if(!item.isRead){const button=document.createElement("button");button.type="button";button.className="notification-read";button.textContent="Mark read";button.addEventListener("click",()=>markNotificationRead(item,button));row.append(button);}
    return row;
  }

  async function loadNotifications(showLoading=true){
    const list=$("#notificationList");if(!list)return;
    if(showLoading){setState("Loading");list.replaceChildren(emptyState("Loading notifications","Please wait while NexHire checks your latest activity."));}
    try{
      const [rowsRaw,countResult]=await Promise.all([api.request("/api/notifications"),api.request("/api/notifications/unread-count")]);
      const rows=normalize(rowsRaw);const unread=Number(countResult?.unreadCount||0);setText("#notificationUnread",unread);
      const all=$("#markAllRead");if(all)all.disabled=unread===0;
      list.replaceChildren();
      if(!rows.length)list.append(emptyState("No notifications yet","Application and contact-request updates will appear here."));
      else rows.forEach(row=>list.append(renderNotification(row)));
      setState(unread?`${unread} unread`:"All caught up","ok");
    }catch(error){setState("Could not load","bad");list.replaceChildren(emptyState("Could not load notifications",error.message));message(error.message,"error");}
  }

  async function markAllRead(button){
    button.disabled=true;message("Marking notifications as read…");
    try{const result=await api.request("/api/notifications/read-all",{method:"PATCH"});message(`${Number(result?.markedRead||0)} notification(s) marked as read.`,"success");await loadNotifications(false);}catch(error){message(error.message,"error");button.disabled=false;}
  }

  document.addEventListener("DOMContentLoaded",()=>{
    if(!setupShell())return;
    if(page==="contacts"){
      $("#refreshContacts")?.addEventListener("click",()=>loadContacts(true));loadContacts(true);
    }
    if(page==="notifications"){
      $("#refreshNotifications")?.addEventListener("click",()=>loadNotifications(true));$("#markAllRead")?.addEventListener("click",event=>markAllRead(event.currentTarget));loadNotifications(true);
    }
  });
})();


const ROLE_NAV={
 "Job Seeker":[["dashboard.html","OV","Overview"],["jobs.html","JB","Find jobs"],["applications.html","AP","Applications"],["resumes.html","CV","CV Studio"],["profile.html","SP","Skill passport"],["matching.html","MT","Match insights"],["../shared/notifications.html","NT","Notifications"],["privacy.html","PR","Privacy center"]],
 "Employer":[["dashboard.html","OV","Overview"],["company.html","CO","Company profile"],["jobs.html","JB","Vacancies"],["applications.html","CA","Candidate ranking"],["matching.html","MT","Matching"],["reports.html","RP","Reports"],["../shared/notifications.html","NT","Notifications"]],
 "Administrator":[["dashboard.html","OV","Overview"],["companies.html","VR","Verification"],["users.html","US","User accounts"],["../shared/api-console.html","JB","Vacancy control"],["matching-rules.html","MR","Matching rules"],["audit.html","AU","Audit log"],["reports.html","RP","Reports"],["privacy.html","PR","Privacy"]]
};
function initials(s){return (s||'NH').split(/\s+/).slice(0,2).map(x=>x[0]||'').join('').toUpperCase()}
function side(role,active=''){return `<div class="proto-brand"><span class="proto-brand-mark">N</span><span>NexHire<small>TEAM ALPHA</small></span></div><div class="proto-role">Signed in as<strong>${role}</strong></div><div class="proto-label">Workspace</div><nav class="proto-nav">${(ROLE_NAV[role]||[]).map(i=>`<a class="${active===i[2]?'active':''}" href="${i[0]}"><span class="proto-icon">${i[1]}</span>${i[2]}</a>`).join('')}<a href="../shared/api-console.html"><span class="proto-icon">99</span>All API endpoints</a><a href="#" onclick="logout()"><span class="proto-icon">↪</span>Sign out</a></nav>`}
function shell(role,title,active,body){return `<div class="proto-shell"><aside class="proto-side">${side(role,active)}</aside><section class="proto-main"><header class="proto-topbar"><div class="proto-title">${title}</div><div class="proto-spacer"></div><input class="proto-search" placeholder="Search this workspace"><button class="proto-btn light" onclick="location.href='../shared/notifications.html'">Updates</button><div class="proto-avatar">${role==='Job Seeker'?'JS':role==='Employer'?'EM':'AD'}</div></header><main class="proto-content">${body}</main></section></div><div id="toast"></div>`}
function setPage(role,title,active,body){document.body.innerHTML=shell(role,title,active,body)}
function esc(s){return String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]))}
function arr(d){return Array.isArray(d)?d:(d?.items||d?.data||d?.results||[])}
async function safe(path, fallback=null){try{return await request(path)}catch(e){return fallback}}
function money(v){return v==null?'—':new Intl.NumberFormat().format(v)}

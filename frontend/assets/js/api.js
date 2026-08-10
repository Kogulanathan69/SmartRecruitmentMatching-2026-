
const API_BASE = localStorage.getItem("nexhire_api") || "https://localhost:7119";
const getToken=()=>localStorage.getItem("nexhire_token")||"";
function toast(m){const t=document.getElementById("toast");if(!t)return alert(m);t.textContent=m;t.style.display="block";setTimeout(()=>t.style.display="none",3500)}
async function request(path,opt={}){
 const headers={...(opt.headers||{})}; const body=opt.body;
 if(!(body instanceof FormData) && body!==undefined) headers["Content-Type"]="application/json";
 if(getToken()) headers.Authorization="Bearer "+getToken();
 const r=await fetch(API_BASE+path,{...opt,headers});
 const ct=r.headers.get("content-type")||""; let data;
 if(ct.includes("json")) data=await r.json(); else if(ct.includes("application/pdf")||ct.includes("octet-stream")) data=await r.blob(); else data=await r.text();
 if(!r.ok) throw new Error(data?.message||data?.detail||data||`HTTP ${r.status}`);
 return data;
}
function saveAuth(d){if(d.accessToken)localStorage.setItem("nexhire_token",d.accessToken);if(d.refreshToken)localStorage.setItem("nexhire_refresh",d.refreshToken);if(d.role)localStorage.setItem("nexhire_role",d.role)}
function logout(){localStorage.removeItem("nexhire_token");localStorage.removeItem("nexhire_refresh");location.href="../auth/login.html"}
function pretty(x){return typeof x==="string"?x:JSON.stringify(x,null,2)}


function renderModule(controllers){
 const list=ENDPOINTS.filter(e=>controllers.includes(e.controller)),root=document.getElementById("moduleApis");
 root.innerHTML=list.map((e,i)=>`<div class="api-card"><div class="api-head"><span class="method ${e.method}">${e.method}</span><b>${e.route}</b></div><div class="api-body"><input id="mp${i}" value="${e.route}"><textarea id="mj${i}">{}</textarea><button class="btn sm" onclick='moduleRun(${i},${JSON.stringify(controllers)})'>Execute</button><pre id="mr${i}">Ready</pre></div></div>`).join("")||'<div class="panel">No implemented endpoint in backend.</div>';
}
async function moduleRun(i,controllers){const e=ENDPOINTS.filter(x=>controllers.includes(x.controller))[i];let opt={method:e.method},raw=document.getElementById("mj"+i).value.trim();if(!["GET","DELETE"].includes(e.method)&&raw)opt.body=raw;try{let d=await request(document.getElementById("mp"+i).value,opt);document.getElementById("mr"+i).textContent=d instanceof Blob?`Binary response (${d.size} bytes)`:pretty(d)}catch(x){document.getElementById("mr"+i).textContent=x.message}}

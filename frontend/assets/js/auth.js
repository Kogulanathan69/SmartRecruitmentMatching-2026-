
function showToast(message, type="info"){
  const t=document.getElementById("toast");
  if(!t){ alert(message); return; }
  t.textContent=message;
  t.style.display="block";
  t.style.background=type==="success"?"#166534":type==="error"?"#991b1b":"#0f172a";
  clearTimeout(window.__toastTimer);
  window.__toastTimer=setTimeout(()=>t.style.display="none",2800);
}
const sleep=ms=>new Promise(r=>setTimeout(r,ms));

async function registerUser(e){
  e.preventDefault();
  const form=e.target;
  const body=Object.fromEntries(new FormData(form));
  const btn=form.querySelector('button[type="submit"]');
  if(btn){btn.disabled=true;btn.textContent="Registering...";}
  try{
    await request("/api/Auth/register",{method:"POST",body:JSON.stringify(body)});
    localStorage.setItem("verify_email",body.email);
    showToast("Registration successful. OTP sent to your email.","success");
    await sleep(1200);
    location.href="verify-otp.html";
  }catch(err){
    const msg=String(err.message||"");
    if(msg.toLowerCase().includes("already registered")){
      localStorage.setItem("verify_email",body.email);
      showToast("Email already registered. Continue to OTP verification or login.","error");
      await sleep(1000);
      location.href="verify-otp.html";
      return;
    }
    showToast(msg||"Registration failed","error");
  }finally{
    if(btn){btn.disabled=false;btn.textContent="Register";}
  }
}

async function verifyOtp(e){
  e.preventDefault();
  const form=e.target;
  const body=Object.fromEntries(new FormData(form));
  const btn=form.querySelector('button[type="submit"]');
  if(btn){btn.disabled=true;btn.textContent="Verifying...";}
  try{
    await request("/api/Auth/verify-otp",{method:"POST",body:JSON.stringify(body)});
    showToast("Email verified successfully. Please login.","success");
    await sleep(1200);
    location.href="login.html";
  }catch(err){
    showToast(err.message||"OTP verification failed","error");
  }finally{
    if(btn){btn.disabled=false;btn.textContent="Verify";}
  }
}

async function resend(){
  const email=(document.querySelector('[name="email"]')?.value||localStorage.getItem("verify_email")||"").trim();
  if(!email){showToast("Enter your email first.","error");return;}
  try{
    await request("/api/Auth/resend-otp",{method:"POST",body:JSON.stringify({email})});
    localStorage.setItem("verify_email",email);
    showToast("A new OTP has been sent to your email.","success");
  }catch(err){
    showToast(err.message||"Could not resend OTP","error");
  }
}

async function loginUser(e){
  e.preventDefault();
  const form=e.target;
  const body=Object.fromEntries(new FormData(form));
  const btn=form.querySelector('button[type="submit"]');
  if(btn){btn.disabled=true;btn.textContent="Logging in...";}
  try{
    const data=await request("/api/Auth/login",{method:"POST",body:JSON.stringify(body)});
    saveAuth(data);
    localStorage.setItem("current_email",body.email);
    showToast("Login successful.","success");
    await sleep(900);
    const role=String(data.role||localStorage.getItem("nexhire_role")||"").toLowerCase();
    if(role.includes("admin")) location.href="../admin/dashboard.html";
    else if(role.includes("employer")) location.href="../employer/dashboard.html";
    else location.href="../jobseeker/dashboard.html";
  }catch(err){
    const msg=String(err.message||"");
    if(msg.toLowerCase().includes("verify")){
      localStorage.setItem("verify_email",body.email);
      showToast("Please verify your email first.","error");
      await sleep(1000);
      location.href="verify-otp.html";
      return;
    }
    showToast(msg||"Login failed","error");
  }finally{
    if(btn){btn.disabled=false;btn.textContent="Login";}
  }
}

async function forgot(e){
  e.preventDefault();
  const body=Object.fromEntries(new FormData(e.target));
  try{
    const data=await request("/api/Auth/forgot-password",{method:"POST",body:JSON.stringify(body)});
    const r=document.getElementById("result"); if(r) r.textContent=pretty(data);
    showToast("Password reset request completed.","success");
  }catch(err){showToast(err.message||"Request failed","error");}
}

async function resetPw(e){
  e.preventDefault();
  const body=Object.fromEntries(new FormData(e.target));
  try{
    await request("/api/Auth/reset-password",{method:"POST",body:JSON.stringify(body)});
    showToast("Password reset successful. Please login.","success");
    await sleep(1000);
    location.href="login.html";
  }catch(err){showToast(err.message||"Reset failed","error");}
}
